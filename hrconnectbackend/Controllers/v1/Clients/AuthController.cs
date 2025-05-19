using hrconnectbackend.Attributes.Authorization.Requirements;
using hrconnectbackend.Config.Settings;
using hrconnectbackend.Constants;
using hrconnectbackend.Exceptions;
using hrconnectbackend.Interface.Services.Clients;
using hrconnectbackend.Models;
using hrconnectbackend.Models.RequestModel;
using hrconnectbackend.Models.Response;
using hrconnectbackend.Services.Clients;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace hrconnectbackend.Controllers.v1.Clients
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(
        IAuthService authService,
        ILogger<AuthController> logger,
        IOptions<JwtSettings> jwtSettings,
        IUserAccountServices userAccountServices)
        : ControllerBase
    {
        private readonly JwtSettings _jwtSettings = jwtSettings.Value;

        // Constructor with dependency injection

        [HttpPost("signin")]
        public async Task<IActionResult> Signin([FromBody] Signin signinBody, [FromQuery] bool rememberMe = false)
        {
            logger.LogInformation("Signin attempt for email: {Email}", signinBody.Email);

            var auth = await authService.Signin(signinBody.Email, signinBody.Password, rememberMe);

            if (!ModelState.IsValid)
            {
                logger.LogWarning("Invalid model state for signin attempt.");
                throw new BadRequestException(ErrorCodes.InvalidRequestModel, "Your body request is invalid.");
            }

            if (auth == null)
            {
                logger.LogWarning("Invalid login or password for email: {Email}", signinBody.Email);
                throw new UnauthorizedException(ErrorCodes.InvalidCredentials, "Invalid login or password.");
            }

            logger.LogInformation("Signin successful for email: {Email}", signinBody.Email);

            // Append access token to response cookies
            Response.Cookies.Append("at_session", auth.AccessToken, new CookieOptions
            {
                HttpOnly = true,  // Secure from JavaScript (prevent XSS)
                SameSite = SameSiteMode.None, // Prevent CSRF attacks
                Secure = true,
                Path = "/",
                Expires = DateTime.Now.AddMinutes(_jwtSettings.AccessExpiration)
            });

            // Append refresh token to response cookies
            Response.Cookies.Append("backend_rt", auth.RefreshToken, new CookieOptions
            {
                HttpOnly = true,  // Secure from JavaScript (prevent XSS)
                SameSite = SameSiteMode.None, // Prevent CSRF attacks
                Secure = true,
                Path = "/",
                Expires = rememberMe ? DateTime.Now.AddMinutes(_jwtSettings.RefreshExpirationRemember) : DateTime.Now.AddMinutes(_jwtSettings.RefreshExpiration)
            });

            Response.Headers["Content-Security-Policy"] = "default-src 'self'; script-src 'self' 'unsafe-inline';";

            logger.LogInformation("Tokens generated and sent to client for email: {Email}", signinBody.Email);

            return Ok(new SuccessResponse("Signin Successful"));

        }

        [HttpGet("session")]
        public async Task<IActionResult> AuthSession()
        {
            var user = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (user == null)
            {
                logger.LogWarning("User not found in claims.");
                return Unauthorized();
            }

            var userAccount = await userAccountServices.GetByIdAsync(int.Parse(user));

            return Ok(new SuccessResponse<UserAccount>(userAccount, "User account found"));
        }

        [HttpPost("signup")]
        [ProducesResponseType(typeof(SuccessResponse<UserAccount>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateUserAccount([FromBody] CreateUser userAccount)
        {
            if (!ModelState.IsValid)
            {
                throw new BadRequestException(ErrorCodes.InvalidRequestModel, "Your body request is invalid.");
            }

            var newUser = new UserAccount
            {
                UserName = userAccount.UserName,
                Password = userAccount.Password,
                Email = userAccount.Email,
                OrganizationId = null,
                Role = "Admin",
            };

            var createdUser = await authService.SignUpAdmin(newUser);

            if (createdUser == null)
            {
                throw new NotFoundException(ErrorCodes.UserNotFound, "User account not found.");
            }

            return Ok(new SuccessResponse<UserAccount>(createdUser, "User account created successfully"));

        }

        [HttpPost("signin/email")]
        public Task<IActionResult> Signin(string email)
        {
            return Task.FromResult<IActionResult>(Ok());
        }

        [HttpPost("signin/email/verify")]
        public Task<IActionResult> SigninVerify([FromQuery] string token, [FromQuery] string email)
        {
            return Task.FromResult<IActionResult>(Ok());
        }

        // [UserRole("Admin,Operator")]
        [HttpPost("operator/signup")]
        public async Task<IActionResult> SignupOperator(CreateUserOperator user)
        {
            var userAccount = await authService.SignUpOperator(user);

            return Ok(userAccount);
        }

        //[UserRole("Admin,Operator")]
        //[HttpPost("admin/signup")]
        //public async Task<IActionResult> SignupUser(CreateUser user)
        //{
        //    var userAccount = await authService.SignUpAdmin(user);

        //    if (userAccount == null) return BadRequest(new
        //    {
        //        Message = "Invalid request",
        //    });

        //    return Ok(userAccount);
        //}

        [UserRole("Admin,Operator")]
        [HttpPost("employee/signup")]
        public async Task<IActionResult> SignupEmployee(CreateUser user)
        {
            var userAccount = await authService.SignUpEmployee(user);

            return Ok(new SuccessResponse("User account created successfully"));
        }

        [HttpPut("change-password")]
        public async Task<IActionResult> ChangePassword(ChangePasswordRequest user)
        {
            await authService.ChangePassword(user.Email, user.Password);

            return Ok(new SuccessResponse("Password changed successfully"));
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetUsersAccountByOrg(int orgId)
        {
            var users = await authService.GetUsers(orgId);

            return Ok(new SuccessResponse<IEnumerable<UserAccount>>(users, "User accounts found"));
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> GenerateAccess()
        {
            logger.LogInformation("Refreshing access token.");

            var refreshToken = HttpContext.Request.Cookies["backend_rt"];

            if (refreshToken == null)
            {
                throw new UnauthorizedException(ErrorCodes.Unauthorized, "Refresh token not found.");
            }

            var generateAccessToken = await authService.GenerateAccessToken(refreshToken);

            if (string.IsNullOrEmpty(generateAccessToken))
            {
                throw new UnauthorizedException(ErrorCodes.Unauthorized, "Invalid refresh token.");
            }

            var exp = DateTime.UtcNow.ToLocalTime().AddMinutes(_jwtSettings.AccessExpiration);

            HttpContext.Response.Cookies.Append("at_session", generateAccessToken, new CookieOptions
            {
                HttpOnly = true,  // Secure from JavaScript (prevent XSS)
                SameSite = SameSiteMode.None, // Prevent CSRF attacks
                Secure = true,
                Path = "/",
                Expires = exp
            });

            logger.LogInformation("Access token successfully generated and sent to client.");

            return Ok(new NewTokenResponse(generateAccessToken));
        }

        [HttpPost("sign-out")]
        public async Task<IActionResult> Logout()
        {
            logger.LogInformation("Logging out user and invalidating tokens.");


            if (Request.Cookies.TryGetValue("backend_rt", out var value))
            {

                await authService.UpdateRefreshToken(async (context) =>
                {
                    var refresh = context.RefreshTokens.FirstOrDefault(a => a.RefreshTokenId == value);
                    if (refresh != null)
                    {
                        context.RefreshTokens.Remove(refresh);
                    }

                    await context.SaveChangesAsync(); // ✅ Await here
                });

                Response.Cookies.Delete("at_session");
                Response.Cookies.Delete("backend_rt");

                logger.LogInformation("Tokens invalidated successfully for refresh token: {RefreshToken}", value);

                return Ok(new SuccessResponse("Successfully logged out"));
            }
            else
            {
                logger.LogWarning("Refresh token not found in cookies.");
                throw new UnauthorizedException(ErrorCodes.Unauthorized, "Refresh token not found.");
            }
        }
    }
}

public record ChangePasswordRequest(string Email, string Password);

