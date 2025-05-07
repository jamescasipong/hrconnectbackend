using System.Security.Claims;
using AutoMapper;
using hrconnectbackend.Claims;
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
        public async Task<IActionResult> GetProfile(){
            var nameIdentifier = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userName = User.FindFirstValue(ClaimTypes.Name);
            var role = User.FindFirstValue(ClaimTypes.Role);
            
            if (nameIdentifier == null)
            {
                return Unauthorized(new ApiResponse(false, "User not found."));
            }

            var employee = await employeeServices.GetByIdAsync(int.Parse(nameIdentifier));

            if (employee == null)
            {
                return Unauthorized(new ApiResponse(false, $"Employee with ID: {nameIdentifier} not found."));
            }

            return Ok(new ApiResponse<dynamic>(true, "Profile retrqieved successfully!", new { Id = nameIdentifier, userName, role }));
        }

        [HttpPost("account/send-reset")]
        public async Task<IActionResult> VerifyReset([FromBody] InputEmail verifyResetDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse(false, $"Your body request is invalid."));
            }


            var sourceEmail = Environment.GetEnvironmentVariable("EmailUsername")!;
            var sourcePassword = Environment.GetEnvironmentVariable("EmailPassword")!;


            try
            {
                var user = await userAccountServices.GetUserAccountByEmail(verifyResetDto.Email);
                if (user == null)
                {
                    return NotFound(new ApiResponse(false, $"This user does not exist"));
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


                return Ok(new ApiResponse(true, $"Email sent successfully"));
            }
            catch (Exception ex)
            {
                var message = ex.Message;
                logger.LogError("log error: {message}", message);
                return StatusCode(500, new ApiResponse(false, $"An error occured while verifying the code"));
            }
        }

        [HttpGet("account/reset-password")]
        public async Task<IActionResult> ResetSessionExist(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return BadRequest(new ApiResponse(success: false, message: "Invalid"));
            }

            try
            {
                var resetSession = await userAccountServices.GetResetPasswordSession(token);


                if (resetSession == null)
                {
                    logger.LogError("Reset session not found.");
                    return Unauthorized(new ApiResponse(false, $"Reset session not found"));
                }

                if (resetSession.ExpiresAt < DateTime.Now)
                {
                    logger.LogError("Reset session expired.");
                    return Unauthorized(new ApiResponse(false, $"Reset session expired."));
                }

                logger.LogInformation("Reset session found.");
                return Ok(new ApiResponse(true, $"Reset session found."));
            }
            catch(Exception ex)
            {
                var message = ex.Message;
                logger.LogError("log error: {message}", message);
                return StatusCode(500, new ApiResponse(false, $"An error occured while resetting the password"));
            }
        }


        [HttpPost("account/reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] NewPasswords newPassword, string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return BadRequest(new ApiResponse(success: false, message: "Invalid"));
            }

            try
            {
                logger.LogInformation("Retrieving token");

                var resetSession = await userAccountServices.GetResetPasswordSession(token);

                logger.LogInformation("Token retrieved: {resetSession}", resetSession);

                if (resetSession == null)
                {
                    return Unauthorized(new ApiResponse(false, $"Reset session not found"));
                }

                if (resetSession.ExpiresAt < DateTime.Now)
                {
                    return Unauthorized(new ApiResponse(false, $"Reset session expired"));
                }

                var user = await userAccountServices.GetUserAccountByEmail(resetSession.Email);

                if (user == null)
                {
                    return NotFound(new ApiResponse(false, $"User not found"));
                }

                var password = BCrypt.Net.BCrypt.HashPassword(newPassword.Password);

                user.Password = password;

                await userAccountServices.UpdateAsync(user);

                await userAccountServices.DeleteResetPassword(resetSession.Token);

                return Ok(new ApiResponse(true, $"Successfully reset the password"));
            }
            catch (Exception ex)
            {
                var message = ex.Message;
                logger.LogError("log error: {message}", message);
                return StatusCode(500, new ApiResponse(false, $"An error occured while resetting the password"));
            }
        }

        [HttpPost("send-code")]
        public async Task<IActionResult> SendVerificationCode(string email, string code, string? expiryDate)
        {
            try
            {
                var thirtyMinutes = expiryDate != null ? DateTime.Parse(expiryDate) : DateTime.Now.AddMinutes(5);
                
                await emailServices.SendAuthenticationCodeAsync(email, code, thirtyMinutes);

                return Ok(new ApiResponse<dynamic>(true, "Email sent successfully!"));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while sending email.");
                return StatusCode(500, new ApiResponse(false, "An error occurred while sending email."));
            }
        }
        
        
        [HttpPost("account/logout")]
        public async Task<IActionResult> Logout(){
            Response.Cookies.Append("token", "", new CookieOptions
            {
                HttpOnly = true,  // Secure from JavaScript (prevent XSS)
                SameSite = SameSiteMode.None, // Prevent CSRF attacks
                Secure = true,
                Path="/",
                Expires = DateTime.UtcNow.AddMinutes(-1), // Cookie expires in 1 hour
            });

            var user = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (user == null)
            {
                return NotFound(new ApiResponse(false, "User not found."));
            }

            var employee = await employeeServices.GetByIdAsync(int.Parse(user));

            if (employee == null)
            {
                return NotFound(new ApiResponse(false, "Employee not found."));
            }

            await employeeServices.UpdateAsync(employee);

            // Response.Cookies.Delete("token");

            return Ok(new ApiResponse(true, "User logged out successfully."));
        }


        [Authorize]
        [HttpGet("account/{id}")]
        public async Task<IActionResult> GetUserAccount(int id)
        {
            var auth = await userAccountServices.GetByIdAsync(id);

            if (auth == null)
                return NotFound(new ApiResponse(false, $"User Account with ID: {id} not found."));

            if (!ModelState.IsValid) return BadRequest(new ApiResponse(success: false, message: ModelState.IsValid.ToString()));

            var authDto = mapper.Map<UserAccountDto>(auth);

            return Ok(new ApiResponse<UserAccountDto?>(success: true, message: $"User Account with ID: {id} has been retrieved successfully!", authDto));
        }

        
        [Authorize]
        [Authorize(Roles = "Admin")]
        [HttpGet("account")]
        public async Task<IActionResult> RetrieveUserAccount(int? pageIndex, int? pageSize)
        {
            var userAccounts = await userAccountServices.GetAllAsync();

            return Ok(userAccounts);
        }

        [Authorize(Roles = "HR, Department")]
        [HttpDelete("account/{userId:int}")]
        public async Task<IActionResult> DeleteAccount(int userId)
        {
            try
            {
                var userAccount = await userAccountServices.GetByIdAsync(userId);

                if (userAccount == null)
                {
                    return NotFound(new ApiResponse(false, $"User Account with id: {userId} does not exist."));
                }

                return Ok(new ApiResponse(true, $"User Account with id: {userId} deleted succcessfully."));
            }
            catch (Exception)
            {
                return StatusCode(500, new ApiResponse(false, $"Internal Server Error"));
            }
        }

        [Authorize]
        [HttpPut("account/change-email/{employeeId:int}")]
        public async Task<IActionResult> UpdateEmail(int employeeId, string email)
        {
            var userAccount = await userAccountServices.GetByIdAsync(employeeId);

            if (userAccount == null)
            {
                return NotFound(new ApiResponse(false, $"User account with id: {employeeId} not found."));
            }

            var employee = await employeeServices.GetByIdAsync(employeeId);

            if (employee == null){
                return NotFound(new ApiResponse(false, $"Employee with id: {employeeId} not found"));
            }
            
            employee.Email = email;

            await userAccountServices.UpdateEmail(employeeId, email);
            await employeeServices.UpdateAsync(employee);

            return Ok(new ApiResponse(true, $"User account with id: {employeeId} successfully updated its email"));
        }

        [Authorize]
        [HttpPost("settings/{employeeId}")]
        public async Task<IActionResult> AddUserSetting(int employeeId)
        {
            try
            {
                await userSettingsServices.CreateDefaultSettings(employeeId);
                return Ok(new ApiResponse(true, "Success!"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [Authorize]
        [HttpPut("settings/{employeeId}")]
        public async Task<IActionResult> ResetSettings(int employeeId)
        {
            try
            {
                await userSettingsServices.ResetSettings(employeeId);

                return Ok(new ApiResponse(true, "Success!"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [Authorize]
        [HttpPatch("settings/{employeeId}")]
        public async Task<IActionResult> PartialUpdate(int employeeId, [FromBody] JsonPatchDocument<UserSettingsPatchDTO> patchDoc)
        {
            if (patchDoc == null)
            {
                logger.LogWarning("Patch document is null for employee ID: {EmployeeId}.", employeeId);
                return BadRequest(new ApiResponse(false, "Invalid patch document."));
            }

            var userSettings = await userSettingsServices.GetByIdAsync(employeeId);
            if (userSettings == null)
            {
                logger.LogWarning("User settings for employee ID: {EmployeeId} not found.", employeeId);
                return NotFound(new ApiResponse(false, $"User settings for employee ID: {employeeId} not found."));
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
                return BadRequest(new ApiResponse(false, "Invalid model state."));
            }

            try
            {
                // Map the DTO back to the original entity model and save the updates
                var updatedUserSettings = mapper.Map(userSettingsDto, userSettings);

                await userSettingsServices.UpdateAsync(updatedUserSettings);
                logger.LogInformation("User settings updated successfully for employee ID: {EmployeeId}.", employeeId);
                return Ok(new ApiResponse(true, "User settings updated successfully."));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while updating user settings for employee ID: {EmployeeId}.", employeeId);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse(false, "An error occurred while updating the settings."));
            }
        }

        [Authorize]
        [HttpGet("notifications")]
        public async Task<IActionResult> RetrieveNotification(int userId, int? pageIndex, int? pageSize)
        {
            try
            {
                var employee = await employeeServices.GetByIdAsync(userId);

                if (employee == null)
                {
                    return NotFound(new ApiResponse(false, "Employee not found."));
                }

                var userNotifications = await notificationServices.GetNotificationsByEmployeeId(userId, pageIndex, pageIndex);

                var mappedUserNotification = mapper.Map<List<ReadUserNotificationDto>>(userNotifications);

                return Ok(new ApiResponse<List<ReadUserNotificationDto>?>(true, "Notifications retrieved successfully!", mappedUserNotification));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
    }
    
    public record NewPasswords(string Password);

}