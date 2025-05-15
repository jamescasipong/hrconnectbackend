using System.Security.Claims;
using AutoMapper;
using hrconnectbackend.Claims;
using hrconnectbackend.Constants;
using hrconnectbackend.Exceptions;
using hrconnectbackend.Interface.Services;
using hrconnectbackend.Interface.Services.Clients;
using hrconnectbackend.Interface.Services.ExternalServices;
using hrconnectbackend.Models;
using hrconnectbackend.Models.DTOs;
using hrconnectbackend.Models.DTOs.AuthDTOs;
using hrconnectbackend.Models.DTOs.GenericDTOs;
using hrconnectbackend.Models.RequestModel;
using hrconnectbackend.Models.Response;
using hrconnectbackend.Models.Sessions;
using hrconnectbackend.Services.ExternalServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace hrconnectbackend.Controllers.v1.Clients
{
    [ApiController]
    [Route("api/v{version:apiVersion}/user")]
    [ApiVersion("1.0")]
    public class UserController
    (IUserAccountServices userAccountServices, ILogger<UserController> logger, IMapper mapper, IEmployeeServices employeeServices, IConfiguration configuration,
    IUserSettingsServices userSettingsServices, INotificationServices notificationServices, ISubscriptionServices subscriptionServices, IEmailServices emailServices, IDepartmentServices departmentServices) : Controller
    {

        [Authorize]
        [HttpGet("account/profile")]
        public async Task<IActionResult> GetProfile()
        {
            var nameIdentifier = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userName = User.FindFirstValue(ClaimTypes.Name);
            var role = User.FindFirstValue(ClaimTypes.Role);

            if (nameIdentifier == null)
            {
                return StatusCode(401, new ErrorResponse(ErrorCodes.Unauthorized, "User not authenticated. Please login."));
            }

            var employee = await employeeServices.GetByIdAsync(int.Parse(nameIdentifier));

            if (employee == null)
            {
                return StatusCode(404, new ErrorResponse(ErrorCodes.EmployeeNotFound, "Employee not found."));
            }

            return Ok(new SuccessResponse<object>(new
            {
                employeeId = employee.Id,
                email = employee.Email,
                organizationId = employee.OrganizationId,
                role = role,
                userName = userName
            }, $"Profile retrieved successfully."));
        }

        [HttpPost("account/send-reset")]
        public async Task<IActionResult> VerifyReset([FromBody] InputEmail verifyResetDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ErrorResponse(ErrorCodes.InvalidRequestModel, "Invalid model state."));
            }


            var user = await userAccountServices.GetUserAccountByEmail(verifyResetDto.Email);
            if (user == null)
            {
                return NotFound(new ErrorResponse(ErrorCodes.UserNotFound, $"User with email: {verifyResetDto.Email} not found."));
            }

            var key = configuration.GetValue<string>("JWT:Key")!;
            var audience = configuration.GetValue<string>("JWT:Audience")!;
            var issuer = configuration.GetValue<string>("JWT:Issuer")!;
            var jwtService = new JwtService(key, audience, issuer);

            var token = jwtService.GenerateToken();

            var resetSession = new ResetPasswordSession
            {
                Email = verifyResetDto.Email,
                Token = token,
                ExpiresAt = DateTime.Now.AddHours(24)
            };

            await userAccountServices.CreatePasswordSession(resetSession);

            await emailServices.SendResetPasswordEmailAsync(resetSession.Email, token);


            return Ok(new SuccessResponse($"Reset password email sent successfully to {verifyResetDto.Email}."));
        }

        [HttpGet("account/reset-password")]
        public async Task<IActionResult> ResetSessionExist(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return StatusCode(400, new ErrorResponse(ErrorCodes.InvalidRequestModel, "Invalid token"));
            }

            var resetSession = await userAccountServices.GetResetPasswordSession(token);


            if (resetSession == null)
            {
                logger.LogError("Reset session not found.");

                return Unauthorized(new ErrorResponse(ErrorCodes.UserNotFound, $"Reset session not found."));
            }

            if (resetSession.ExpiresAt < DateTime.Now)
            {
                logger.LogError("Reset session expired.");
                return Unauthorized(new ErrorResponse(ErrorCodes.TokenExpired, $"Reset session expired."));
            }

            return Ok(new SuccessResponse<ResetPasswordSession>(resetSession, $"Reset session found."));

        }


        [HttpPost("account/reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] NewPasswords newPassword, string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return BadRequest(new ErrorResponse(ErrorCodes.InvalidRequestModel, "Invalid token"));
            }

            logger.LogInformation("Retrieving token");

            var resetSession = await userAccountServices.GetResetPasswordSession(token);

            logger.LogInformation("Token retrieved: {resetSession}", resetSession);

            if (resetSession == null)
            {
                return Unauthorized(new ErrorResponse(ErrorCodes.UserNotFound, $"Reset session not found"));
            }

            if (resetSession.ExpiresAt < DateTime.Now)
            {
                return Unauthorized(new ErrorResponse(ErrorCodes.TokenExpired, $"Reset session expired"));
            }

            var user = await userAccountServices.GetUserAccountByEmail(resetSession.Email);

            if (user == null)
            {
                return NotFound(new ErrorResponse(ErrorCodes.UserNotFound, $"User not found"));
            }

            var password = BCrypt.Net.BCrypt.HashPassword(newPassword.Password);

            user.Password = password;

            await userAccountServices.UpdateAsync(user);

            await userAccountServices.DeleteResetPassword(resetSession.Token);

            return Ok(new SuccessResponse($"Password reset successfully!"));

        }

        [HttpPost("send-code")]
        public async Task<IActionResult> SendVerificationCode(string email, string code, string? expiryDate)
        {

            var thirtyMinutes = expiryDate != null ? DateTime.Parse(expiryDate) : DateTime.Now.AddMinutes(5);

            await emailServices.SendAuthenticationCodeAsync(email, code, thirtyMinutes);

            return Ok(new SuccessResponse($"Verification code sent successfully to {email}."));

        }


        [HttpPost("account/logout")]
        public async Task<IActionResult> Logout()
        {
            Response.Cookies.Append("token", "", new CookieOptions
            {
                HttpOnly = true,  // Secure from JavaScript (prevent XSS)
                SameSite = SameSiteMode.None, // Prevent CSRF attacks
                Secure = true,
                Path = "/",
                Expires = DateTime.UtcNow.AddMinutes(-1), // Cookie expires in 1 hour
            });

            var user = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (user == null)
            {
                return NotFound(new ErrorResponse(ErrorCodes.UserNotFound, "User not found."));
            }

            var employee = await employeeServices.GetByIdAsync(int.Parse(user));

            if (employee == null)
            {
                return NotFound(new ErrorResponse(ErrorCodes.EmployeeNotFound, $"Employe with ID: {user} not found."));
            }

            await employeeServices.UpdateAsync(employee);

            return Ok(new SuccessResponse($"Logout successful!"));
        }


        [Authorize]
        [HttpGet("account/{id}")]
        public async Task<IActionResult> GetUserAccount(int id)
        {
            var auth = await userAccountServices.GetByIdAsync(id);

            if (auth == null) return NotFound(new ErrorResponse(ErrorCodes.UserNotFound, $"User Account with ID: {id} does not exist!"));

            if (!ModelState.IsValid) return BadRequest(new ErrorResponse(ErrorCodes.InvalidRequestModel, "Invalid model state."));

            var authDto = mapper.Map<UserAccountDto>(auth);

            return Ok(new SuccessResponse<UserAccountDto>(authDto, $"User Account with ID: {id} retrieved successfully!"));
        }


        [Authorize]
        [Authorize(Roles = "Admin")]
        [HttpGet("account")]
        public async Task<IActionResult> RetrieveUserAccount(int? pageIndex, int? pageSize)
        {
            var userAccounts = await userAccountServices.GetAllAsync();

            return Ok(new SuccessResponse<List<UserAccount>>(userAccounts, $"User Accounts retrieved successfully!"));
        }

        [Authorize(Roles = "HR, Department")]
        [HttpDelete("account/{userId:int}")]
        public async Task<IActionResult> DeleteAccount(int userId)
        {

            var userAccount = await userAccountServices.GetByIdAsync(userId);

            if (userAccount == null)
            {
                return NotFound(new ErrorResponse(ErrorCodes.UserNotFound, $"User Account with id: {userId} not found."));
            }

            return Ok(new SuccessResponse($"User Account with id: {userId} deleted successfully!"));

        }

        [Authorize]
        [HttpPut("account/change-email/{employeeId:int}")]
        public async Task<IActionResult> UpdateEmail(int employeeId, string email)
        {
            var userAccount = await userAccountServices.GetByIdAsync(employeeId);

            if (userAccount == null)
            {
                throw new NotFoundException(ErrorCodes.UserNotFound, $"User Account with ID: {employeeId} not found.");
            }

            var employee = await employeeServices.GetByIdAsync(employeeId);

            if (employee == null)
            {
                throw new NotFoundException(ErrorCodes.EmployeeNotFound, $"Employee with ID: {employeeId} not found.");
            }

            employee.Email = email;

            await userAccountServices.UpdateEmail(employeeId, email);
            await employeeServices.UpdateAsync(employee);

            return Ok(new SuccessResponse($"Email updated successfully!"));
        }

        [Authorize]
        [HttpPost("settings/{employeeId}")]
        public async Task<IActionResult> AddUserSetting(int employeeId)
        {

            await userSettingsServices.CreateDefaultSettings(employeeId);

            return Ok(new SuccessResponse($"User settings created successfully!"));

        }

        [Authorize]
        [HttpPut("settings/{employeeId}")]
        public async Task<IActionResult> ResetSettings(int employeeId)
        {

            await userSettingsServices.ResetSettings(employeeId);

            return Ok(new SuccessResponse($"User settings reset successfully!"));

        }

        [Authorize]
        [HttpPatch("settings/{employeeId}")]
        public async Task<IActionResult> PartialUpdate(int employeeId, [FromBody] JsonPatchDocument<UserSettingsPatchDTO> patchDoc)
        {
            if (patchDoc == null)
            {
                logger.LogWarning("Patch document is null for employee ID: {EmployeeId}.", employeeId);
                throw new BadRequestException(ErrorCodes.InvalidRequestModel, "Invalid patch document.");
            }

            var userSettings = await userSettingsServices.GetByIdAsync(employeeId);
            if (userSettings == null)
            {
                logger.LogWarning("User settings for employee ID: {EmployeeId} not found.", employeeId);
                throw new NotFoundException(ErrorCodes.UserSettingsNotFound, $"User settings for employee ID: {employeeId} not found.");
            }

            // Map the entity model to the DTO model if necessary
            var userSettingsDto = mapper.Map<UserSettingsPatchDTO>(userSettings);

            // Apply the patch document to the DTO model
            patchDoc.ApplyTo(userSettingsDto, (error) =>
            {
                // Log and add the error message to ModelState
                ModelState.AddModelError(error.Operation.path, error.Operation.value?.ToString() ?? "Invalid operation");
            });

            // Check if there were any validation errors after applying the patch
            if (!ModelState.IsValid)
            {
                logger.LogError("Invalid model state for employee ID: {EmployeeId}.", employeeId);
                throw new BadRequestException(ErrorCodes.InvalidRequestModel, "Invalid model state.");
            }

            var updatedUserSettings = mapper.Map(userSettingsDto, userSettings);

            await userSettingsServices.UpdateAsync(updatedUserSettings);
            logger.LogInformation("User settings updated successfully for employee ID: {EmployeeId}.", employeeId);
            return Ok(new SuccessResponse($"User settings updated successfully!"));

        }

        [Authorize]
        [HttpGet("notifications")]
        public async Task<IActionResult> RetrieveNotification(int userId, int? pageIndex, int? pageSize)
        {

            var employee = await employeeServices.GetByIdAsync(userId);

            if (employee == null)
            {
                throw new NotFoundException(ErrorCodes.EmployeeNotFound, $"Employee with ID: {userId} not found.");
            }

            var userNotifications = await notificationServices.GetNotificationsByEmployeeId(userId, pageIndex, pageIndex);

            var mappedUserNotification = mapper.Map<List<ReadUserNotificationDto>>(userNotifications);

            return Ok(new SuccessResponse<List<ReadUserNotificationDto>>(mappedUserNotification, $"Notifications retrieved successfully!"));

        }
    }

    public record NewPasswords(string Password);

}