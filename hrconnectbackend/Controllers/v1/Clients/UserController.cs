using System.Security.Claims;
using AutoMapper;
using hrconnectbackend.Claims;
using hrconnectbackend.Helper;
using hrconnectbackend.Interface.Services;
using hrconnectbackend.Interface.Services.Clients;
using hrconnectbackend.Interface.Services.ExternalServices;
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

        [HttpPost("account/login")]
        [EnableRateLimiting("fixed")]
        public async Task<IActionResult> Login([FromBody] Signin? signinBody)
        {
            var userAgent = HttpContext.Request.Headers.UserAgent;
            var ipAddress = HttpContext.Connection.RemoteIpAddress;
            
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse(false, $"Your body request is invalid."));
            }

            try
            {
                logger.LogInformation($"Attempting to log in...");
                if (signinBody == null) return BadRequest(new ApiResponse(success: false, message: "Body request is required."));

                var employee = await employeeServices.GetEmployeeByEmail(signinBody.Email);

                var employeePassword = await userAccountServices.RetrievePassword(signinBody.Email);
                
                var userName = await userAccountServices.RetrieveUsername(signinBody.Email);

                if (employee == null)
                {
                    logger.LogWarning($"An employee with email: {signinBody.Email} not found.");
                    return Unauthorized(new ApiResponse(false, $"An employee with email: {signinBody.Email} not found."));
                }

                if (!BCrypt.Net.BCrypt.Verify(signinBody.Password, employeePassword))
                {
                    logger.LogWarning($"Invalid password; try again");
                    return Unauthorized(new ApiResponse(false, $"Invalid password; try again."));
                }

                // var twoFactorCode = new Random().Next(100000, 999999).ToString();

                bool twoFactorCodeEnabled = true;

                if (twoFactorCodeEnabled)
                {
                    logger.LogWarning($"User Agent: {userAgent}");
                    logger.LogWarning($"IPAddress: {ipAddress}");
                    DateTime expiry = DateTime.Now.AddMinutes(5);
                    string twoFactorCode = await userAccountServices.GenerateOTP(employee.Id, expiry);
                    await emailServices.SendAuthenticationCodeAsync(signinBody.Email, twoFactorCode, expiry);

                    try
                    {
                        logger.LogInformation($"Two-factor authentication code sent successfully!");
                        return Ok(new ApiResponse<dynamic>(false, "two-factor authentication code sent successfully!", new
                        {
                            twoFactor = true
                        }));
                    }
                    catch(Exception ex)
                    {
                        return StatusCode(500, new ApiResponse(false, ex.Message));
                    }

                }

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, userName),
                    // new Claim(ClaimTypes.Role, employee.IsAdmin ? "Admin" : "User"),
                    // new Claim("Role", employee.IsAdmin ? "Admin" : "User"),
                    new Claim(ClaimTypes.NameIdentifier, employee.Id.ToString())
                };

                if (employee.EmployeeDepartment != null)
                {
                    var empDepartment = await departmentServices.GetDepartmentByEmployee(employee.Id);
                    
                    claims.Add(new Claim("Department", empDepartment.DeptName));
                }

                var key = configuration.GetValue<string>("JWT:Key")!;
                var audience = configuration.GetValue<string>("JWT:Audience")!;
                var issuer = configuration.GetValue<string>("JWT:Issuer")!;
                var jwtService = new JwtService(key, audience, issuer);


                var jwtToken = jwtService.GenerateToken(claims, DateTime.UtcNow.AddMinutes(15));

                string sessionId = Guid.NewGuid().ToString();

                // HttpContext.Session.SetString("sessionId", sessionId);
                
                Response.Cookies.Append("token", jwtToken, new CookieOptions
                {
                    HttpOnly = true,  // Secure from JavaScript (prevent XSS)
                    SameSite = SameSiteMode.None, // Prevent CSRF attacks
                    Secure = true,
                    Path="/",
                    Expires = DateTime.UtcNow.AddMinutes(30), // Cookie expires in 1 hour
                });

                logger.LogWarning("$A user has authenticated successfully!");
                employee.Status = "Online";
                await employeeServices.UpdateAsync(employee);
                return Ok(new ApiResponse(success: true, message: $"A user has authenticated successfully!"));
            }
            catch (KeyNotFoundException ex)
            {
                return Unauthorized(new ApiResponse(false, ex.Message));
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex.Message);
                return StatusCode(500, new ApiResponse(false, $"Internal Server Error: {ex.Message}"));
            }
        }

        
        
        [HttpPost("account/login/verify")]
        public async Task<IActionResult> LoginVerification([FromBody] SigninVerification signinVerification)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse(false, $"Your body request is invalid."));
            }

            try
            {
                if (signinVerification == null) return BadRequest(new ApiResponse(success: false, message: "Body request is required."));

                var employee = await employeeServices.GetEmployeeByEmail(signinVerification.Email);

                if (employee == null)
                {
                    logger.LogWarning($"An employee with email: {signinVerification.Email} not found.");
                    return Unauthorized(new ApiResponse(false, $"An employee with email: {signinVerification.Email} not found."));
                }

                var userAccount = await userAccountServices.GetUserAccountByEmail(signinVerification.Email);

                if (userAccount == null)
                {
                    logger.LogWarning($"User account with email: {signinVerification.Email} not found.");
                    return Unauthorized(new ApiResponse(false, $"User account with email: {signinVerification.Email} not found."));
                }

                // if (userAccount.VerificationCode != signinVerification.Code)
                // {
                //     _logger.LogWarning($"Invalid verification code; try again.");
                //     return Unauthorized(new ApiResponse(false, $"Invalid verification code; try again."));
                // }
                //
                // if (userAccount.VerificationCodeExpiry < DateTime.Now)
                // {
                //     _logger.LogWarning($"Verification code has expired; try again.");
                //     return Unauthorized(new ApiResponse(false, $"Verification code has expired; try again."));
                // }

                var userName = await userAccountServices.RetrieveUsername(signinVerification.Email);
                var activePlan = await subscriptionServices.GetUserSubscription(userAccount.UserId);
                
                logger.LogInformation(activePlan.Id.ToString());
                
                List<Claim> claims = new List<Claim>
                {
                    // new Claim(ClaimTypes.Role, employee.IsAdmin ? "Admin" : "User"),
                    // new Claim("Role", employee.IsAdmin ? "Admin" : "User"),
                    new Claim(ClaimTypes.NameIdentifier, employee.Id.ToString())
                };

                if (employee.EmployeeDepartment != null)
                {
                    var empDepartment = await departmentServices.GetDepartmentByEmployee(employee.Id);

                    claims.Add(new Claim("Department", empDepartment.DeptName));
                }
                string expiresAt = activePlan.EndDate.ToString("o");
                string subscription = activePlan.Id.ToString();
                string subscriptionPlan = activePlan.SubscriptionPlan!.Name;
                
                if (activePlan != null)
                {
                    logger.LogInformation("Working");   
                    
                    claims.Add(new Claim(SubscriptionTypes.SubscriptionId, subscription));
                    claims.Add(new Claim(SubscriptionTypes.Subscription, subscriptionPlan));
                    claims.Add(new Claim(SubscriptionTypes.Expiration, expiresAt));
                }

                var key = configuration.GetValue<string>("JWT:Key")!;
                var audience = configuration.GetValue<string>("JWT:Audience")!;
                var issuer = configuration.GetValue<string>("JWT:Issuer")!;
                var jwtService = new JwtService(key, audience, issuer);

                var jwtToken = jwtService.GenerateToken(claims, DateTime.UtcNow.AddMinutes(15));

                string sessionId = Guid.NewGuid().ToString();

                
                // Response.Headers.Append("Set-Cookie", "token=" + jwtToken + "; HttpOnly; Secure; SameSite=None; Path=/; Expires=" + DateTime.UtcNow.AddMinutes(15).ToString());
                Response.Cookies.Append("token", jwtToken, new CookieOptions
                {
                    HttpOnly = true,  // Secure from JavaScript (prevent XSS)
                    SameSite = SameSiteMode.None, // Prevent CSRF attacks
                    Secure = true,
                    Path="/",
                    Expires = DateTime.UtcNow.AddMinutes(30) // Cookie expires in 1 hour
                });

                logger.LogWarning("$A user has authenticated successfully!");
                // userAccount.VerificationCode = null;
                // userAccount.VerificationCodeExpiry = null;
                employee.Status = "Online";

                await employeeServices.UpdateAsync(employee);
                await userAccountServices.UpdateAsync(userAccount);

                return Ok(new ApiResponse<string>(success: true, message: $"A user has authenticated successfully!", data: jwtToken));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponse(false, ex.Message));
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex.Message);
                return StatusCode(500, new ApiResponse(false, $"Internal Server Error: {ex.Message}"));
            }
        }

        // [HttpPost("send-email")]
        // public async Task<IActionResult> SendEmail(string email, string subject, string body)
        // {
        //     try
        //     {
        //         var sourceEmail = _configuration.GetValue<string>("EmailSettings:Username")!;
        //         var password = _configuration.GetValue<string>("EmailSettings:Password")!;

        //         // Ensure sensitive data is not logged or exposed
        //         if (string.IsNullOrEmpty(sourceEmail) || string.IsNullOrEmpty(password))
        //         {
        //             _logger.LogError("Email configuration is missing.");
        //             return StatusCode(500, new ApiResponse(false, "Email configuration is missing."));
        //         }

        //         var emailService = new EmailServices("smtp.gmail.com", 587, sourceEmail, password);

        //         // await emailService.SendEmailAsync(email, subject, body);

        //         return Ok(new ApiResponse<dynamic>(true, "Email sent successfully!"));
        //     }
        //     catch (Exception ex)
        //     {
        //         _logger.LogError(ex, "An error occurred while sending email.");
        //         return StatusCode(500, new ApiResponse(false, "An error occurred while sending email."));
        //     }
        // }

        [HttpPost("account/send-reset")]
        public async Task<IActionResult> VerifyReset([FromBody] InputEmail verifyResetDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse(false, $"Your body request is invalid."));
            }


            var sourceEmail = Environment.GetEnvironmentVariable("EmailUsername")!;
            var sourcePassword = Environment.GetEnvironmentVariable("EmailPassword")!;


            try
            {
                var user = await userAccountServices.GetUserAccountByEmail(verifyResetDTO.Email);
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
                    Email = verifyResetDTO.Email,
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

        
        // private async Task<string> CreateSessionForUser(ApplicationUser user, string deviceId, string deviceName, string deviceIp, string userAgent)
        // {
        //     var sessionId = Guid.NewGuid().ToString(); // Create a new session ID

        //     var activeSession = new ActiveSession
        //     {
        //         SessionId = sessionId,
        //         UserId = user.Id,
        //         DeviceId = deviceId,
        //         DeviceName = deviceName,
        //         DeviceIp = deviceIp,
        //         UserAgent = userAgent,
        //         CreatedAt = DateTime.UtcNow,
        //         LastAccessed = DateTime.UtcNow
        //     };

        //     // Save the session in the database
        //     _dbContext.ActiveSessions.Add(activeSession);
        //     await _dbContext.SaveChangesAsync();

        //     return sessionId;
        // }


        // [HttpGet("profile-session")]
        // public async Task<IActionResult> GetMyProfile(){
        //     var userSession = HttpContext.Session.GetString("sessionId");
        //     var user = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        //     if (userSession == null)
        //     {
        //         return NotFound(new ApiResponse(false, "User not found."));
        //     }

        //     var employee = await _employeeServices.GetByIdAsync(int.Parse(user));

        //     if (employee == null)
        //     {
        //         return NotFound(new ApiResponse(false, $"Employee with ID: {userSession} not found."));
        //     }

        //     return Ok(new ApiResponse<dynamic>(true, "Profile retrqieved successfully!", new { Id = userSession, userName = employee.FirstName, role = employee.IsAdmin ? "Admin" : "User", isAdmin = employee.IsAdmin }));
        // }
        
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

            var authDTO = mapper.Map<UserAccountDto>(auth);

            return Ok(new ApiResponse<UserAccountDto>(success: true, message: $"User Account with ID: {id} has been retrieved successfully!", authDTO));
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

                return Ok(new ApiResponse<List<ReadUserNotificationDto>>(true, "Notifications retrieved successfully!", mappedUserNotification));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
    }
    
    public record NewPasswords(string Password);

}