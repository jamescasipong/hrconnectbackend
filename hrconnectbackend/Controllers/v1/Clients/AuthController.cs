using hrconnectbackend.Attributes.Authorization.Requirements;
using hrconnectbackend.Config.Settings;
using hrconnectbackend.Interface.Services.Clients;
using hrconnectbackend.Models;
using hrconnectbackend.Models.RequestModel;
using hrconnectbackend.Models.Response;
using hrconnectbackend.Services.Clients;
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
                return BadRequest(ModelState);
            }

            if (auth == null)
            {
                logger.LogWarning("Invalid login or password for email: {Email}", signinBody.Email);
                return StatusCode(401, new ApiResponse(false, "Invalid login or password"));
            }

            try
            {
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
                
                return Ok(new ApiResponse(true, "Successfully logged in"));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error during signin process for email: {Email}", signinBody.Email);
                return StatusCode(500, new ApiResponse(false, ex.Message));
            }
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

            return Ok(new ApiResponse<UserAccount>(true, "User account found", userAccount));
        }

        [HttpPost("signup")]
        public async Task<IActionResult> CreateUserAccount([FromBody] CreateUser userAccount)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse(false, $"Your body request is invalid."));
            }

            try
            {
                var newUser = new UserAccount
                {
                    UserName = userAccount.UserName,
                    Password = userAccount.Password,
                    Email = userAccount.Email,
                    OrganizationId = null,
                    Role = "Admin",
                };
                var createdUser = await authService.SignUpAdmin(newUser);
                return Ok(new ApiResponse<UserAccount>(true, $"User account created successfully", createdUser));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while creating the user account.");
                return StatusCode(500, new ApiResponse(false, $"An error occurred while creating the user account: {ex.Message}"));
            }
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

            if (userAccount == null) return BadRequest(new
            {
                Message = "Invalid request",
            });

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

            if (userAccount == null)
                return BadRequest(new
                {
                    Message = "Invalid Request",
                });
            
            return Ok(userAccount);
        }
        
        [HttpPut("change-password")]
        public async Task<IActionResult> ChangePassword(ChangePasswordRequest user)
        {
            var userAccount = await authService.ChangePassword(user.Email, user.Password);
            
            if (!userAccount) return BadRequest();
            
            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> GetUsersAccountByOrg(int orgId)
        {
            var users = await authService.GetUsers(orgId);
            // var org = HttpContext.Items["orgId"];
            return Ok(users);
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> GenerateAccess()
        {
            logger.LogInformation("Refreshing access token.");

            var refreshToken = HttpContext.Request.Cookies["backend_rt"];
            
            if (refreshToken == null)
            {
                logger.LogWarning("No refresh token found in request.");
                return Unauthorized();
            }

            var generateAccessToken = await authService.GenerateAccessToken(refreshToken);

            if (string.IsNullOrEmpty(generateAccessToken))
            {
                logger.LogWarning("Failed to generate access token using the provided refresh token.");
                return Unauthorized();
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
                try
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
                    
                    return Ok("Successfully logged out");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error during logout process.");
                    return NotFound();
                }
            }
            else
            {
                logger.LogWarning("No refresh token found for logout.");
                return NotFound();
            }
        }
    }
}

public record ChangePasswordRequest(string Email, string Password);

