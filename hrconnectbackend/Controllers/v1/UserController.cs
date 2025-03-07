
using System.Security.Claims;
using AutoMapper;
using hrconnectbackend.CustomAttributeAnnotation;
using hrconnectbackend.Helper;
using hrconnectbackend.Interface.Services;
using hrconnectbackend.Models.DTOs;
using hrconnectbackend.Models.DTOs.GenericDTOs;
using hrconnectbackend.Services.ExternalServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;


namespace hrconnectbackend.Controllers.v1
{
    [ApiController]
    [Route("api/v{version:apiVersion}/user")]
    [ApiVersion("1.0")]
    public class UserController
    (IUserAccountServices _userAccountServices, ILogger<UserController> _logger, IMapper _mapper, IEmployeeServices _employeeServices, IConfiguration _configuration, 
    IUserSettingsServices _userSettingsServices, INotificationServices _notificationServices) : Controller
    {

        [Authorize]
        [HttpGet("account/profile")]
        public async Task<IActionResult> GetProfile(){
            
            var User = HttpContext.User;

            var NameIdentifier = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userName = User.FindFirstValue(ClaimTypes.Name);
            var role = User.FindFirstValue(ClaimTypes.Role);
            
            if (NameIdentifier == null)
            {
                return NotFound(new ApiResponse(false, "User not found."));
            }

            var employee = await _employeeServices.GetByIdAsync(int.Parse(NameIdentifier));

            if (employee == null)
            {
                return NotFound(new ApiResponse(false, $"Employee with ID: {NameIdentifier} not found."));
            }

            return Ok(new ApiResponse<dynamic>(true, "Profile retrqieved successfully!", new { Id = NameIdentifier, userName, role, isAdmin = employee.IsAdmin }));
        }

        [HttpPost("account/login")]
        [EnableRateLimiting("fixed")]
        public async Task<IActionResult> Login([FromBody] LoginDTO loginDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse(false, $"Your body request is invalid."));
            }

            try
            {
                if (loginDTO == null) return BadRequest(new ApiResponse(success: false, message: "Body request is required."));

                var employee = await _employeeServices.GetEmployeeByEmail(loginDTO.Email);

                var employeePassword = await _userAccountServices.RetrievePassword(loginDTO.Email);

                var userName = await _userAccountServices.RetrieveUsername(loginDTO.Email);

                if (employee == null)
                {
                    _logger.LogWarning($"An employee with email: {loginDTO.Email} not found.");
                    return NotFound(new ApiResponse(false, $"An employee with email: {loginDTO.Email} not found."));
                }

                if (!BCrypt.Net.BCrypt.Verify(loginDTO.Password, employeePassword))
                {
                    _logger.LogWarning($"Invalid password; try again");
                    return Unauthorized(new ApiResponse(false, $"Invalid password; try again."));
                }

                var claims = new[]
                {
                    new Claim(ClaimTypes.Name, userName),
                    new Claim(ClaimTypes.Role, employee.IsAdmin ? "Admin" : "User"),
                    new Claim("Role", employee.IsAdmin ? "Admin" : "User"),
                    new Claim(ClaimTypes.NameIdentifier, employee.Id.ToString())
                };

                if (employee.Department != null)
                {
                    claims.Append(new Claim("Department", employee.Department.DeptName));
                }

                var key = _configuration.GetValue<string>("JWT:Key")!;
                var audience = _configuration.GetValue<string>("JWT:Audience")!;
                var issuer = _configuration.GetValue<string>("JWT:Issuer")!;
                var jwtService = new JwtService(key, audience, issuer);

                var jwtToken = jwtService.GenerateToken(claims);

                string sessionId = Guid.NewGuid().ToString();

                HttpContext.Session.SetString("sessionId", sessionId);
                
                Response.Cookies.Append("token", jwtToken, new CookieOptions
                {
                    HttpOnly = true,  // Secure from JavaScript (prevent XSS)
                    SameSite = SameSiteMode.None, // Prevent CSRF attacks
                    Secure = true,
                    Path="/",
                    Expires = DateTime.UtcNow.AddHours(1), // Cookie expires in 1 hour
                });

                _logger.LogWarning("$A user has authenticated successfully!");
                employee.Status = "Online";
                await _employeeServices.UpdateAsync(employee);
                return Ok(new ApiResponse(success: true, message: $"A user has authenticated successfully!"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponse(false, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex.Message);
                return StatusCode(500, new ApiResponse(false, $"Internal Server Error: {ex.Message}"));
            }
        }

        [HttpPost("send-email")]
        public async Task<IActionResult> SendEmail(string email, string subject, string body){
            try {
                var sourceEmail = _configuration.GetValue<string>("EmailSettings:Username")!;
                var password = _configuration.GetValue<string>("EmailSettings:Password")!;


                var emailService = new EmailServices("smtp.gmail.com", 587, sourceEmail, password);

                var envEmail = _configuration["EmailPassword"];
                

                await emailService.SendEmailAsync(email, subject, body);

                return Ok(new ApiResponse<dynamic>(true, "Email sent successfully!", new {
                    envEmail
                }));
            }
            catch(Exception ex) {
                return StatusCode(500, new ApiResponse(false, ex.Message));
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
        [HttpGet("profile-session")]
        public async Task<IActionResult> GetMyProfile(){
            var userSession = HttpContext.Session.GetString("sessionId");
            var user = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            if (userSession == null)
            {
                return NotFound(new ApiResponse(false, "User not found."));
            }

            var employee = await _employeeServices.GetByIdAsync(int.Parse(user));

            if (employee == null)
            {
                return NotFound(new ApiResponse(false, $"Employee with ID: {userSession} not found."));
            }

            return Ok(new ApiResponse<dynamic>(true, "Profile retrqieved successfully!", new { Id = userSession, userName = employee.FirstName, role = employee.IsAdmin ? "Admin" : "User", isAdmin = employee.IsAdmin }));
        }

        [Authorize]
        [HttpPost("account/logout")]
        public async Task<IActionResult> Logout(){
            var user = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (user == null)
            {
                return NotFound(new ApiResponse(false, "User not found."));
            }

            var employee = await _employeeServices.GetByIdAsync(int.Parse(user));

            if (employee == null)
            {
                return NotFound(new ApiResponse(false, "Employee not found."));
            }

            if (employee.Status == "Offline")
            {
                return BadRequest(new ApiResponse(false, "User is already logged out."));
            }

            employee.Status = "Offline";

            await _employeeServices.UpdateAsync(employee);

            Response.Cookies.Delete("token");

            return Ok(new ApiResponse(true, "User logged out successfully."));
        }


        [Authorize]
        [HttpGet("account/{id}")]
        public async Task<IActionResult> GetUserAccount(int id)
        {
            var auth = await _userAccountServices.GetByIdAsync(id);

            if (auth == null)
                return NotFound(new ApiResponse(false, $"User Account with ID: {id} not found."));

            if (!ModelState.IsValid) return BadRequest(new ApiResponse(success: false, message: ModelState.IsValid.ToString()));

            var authDTO = _mapper.Map<UserAccountDTO>(auth);

            return Ok(new ApiResponse<UserAccountDTO>(success: true, message: $"User Account with ID: {id} has been retrieved successfully!", authDTO));
        }

        
        [Authorize]
        [Authorize(Roles = "Admin")]
        [HttpGet("account")]
        public async Task<IActionResult> RetrieveUserAccount(int? pageIndex, int? pageSize)
        {
            var userAccounts = await _userAccountServices.GetAllAsync();

            return Ok(userAccounts);
        }

        [Authorize(Roles = "HR Department")]
        [HttpDelete("account/{userId:int}")]
        public async Task<IActionResult> DeleteAccount(int userId)
        {
            try
            {
                var userAccount = await _userAccountServices.GetByIdAsync(userId);

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
            var userAccount = await _userAccountServices.GetByIdAsync(employeeId);

            if (userAccount == null)
            {
                return NotFound(new ApiResponse(true, $"User account with id: {employeeId} not found."));
            }

            await _userAccountServices.UpdateEmail(employeeId, email);

            return Ok(new ApiResponse(false, $"User account with id: {employeeId} successfully updated its email"));
        }

        [Authorize]
        [HttpPost("settings/{employeeId}")]
        public async Task<IActionResult> AddUserSetting(int employeeId)
        {
            try
            {
                await _userSettingsServices.CreateDefaultSettings(employeeId);
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
                await _userSettingsServices.ResetSettings(employeeId);

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
                _logger.LogWarning("Patch document is null for employee ID: {EmployeeId}.", employeeId);
                return BadRequest(new ApiResponse(false, "Invalid patch document."));
            }

            var userSettings = await _userSettingsServices.GetByIdAsync(employeeId);
            if (userSettings == null)
            {
                _logger.LogWarning("User settings for employee ID: {EmployeeId} not found.", employeeId);
                return NotFound(new ApiResponse(false, $"User settings for employee ID: {employeeId} not found."));
            }

            // Map the entity model to the DTO model if necessary
            var userSettingsDto = _mapper.Map<UserSettingsPatchDTO>(userSettings);

            // Apply the patch document to the DTO model
            patchDoc.ApplyTo(userSettingsDto, (error) =>
            {
                // Log and add the error message to ModelState
                ModelState.AddModelError(error.Operation.path, error.Operation.value?.ToString() ?? "Invalid operation");
            });

            // Check if there were any validation errors after applying the patch
            if (!ModelState.IsValid)
            {
                _logger.LogError("Invalid model state for employee ID: {EmployeeId}.", employeeId);
                return BadRequest(new ApiResponse(false, "Invalid model state."));
            }

            try
            {
                // Map the DTO back to the original entity model and save the updates
                var updatedUserSettings = _mapper.Map(userSettingsDto, userSettings);

                await _userSettingsServices.UpdateAsync(updatedUserSettings);
                _logger.LogInformation("User settings updated successfully for employee ID: {EmployeeId}.", employeeId);
                return Ok(new ApiResponse(true, "User settings updated successfully."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating user settings for employee ID: {EmployeeId}.", employeeId);
                return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse(false, "An error occurred while updating the settings."));
            }
        }

        [Authorize]
        [HttpGet("notifications")]
        public async Task<IActionResult> RetrieveNotification(int userId, int? pageIndex, int? pageSize)
        {
            try
            {
                var employee = await _employeeServices.GetByIdAsync(userId);

                if (employee == null)
                {
                    return NotFound(new ApiResponse(false, "Employee not found."));
                }

                var userNotifications = await _notificationServices.GetNotificationsByEmployeeId(userId, pageIndex, pageIndex);

                var mappedUserNotification = _mapper.Map<List<ReadUserNotificationDTO>>(userNotifications);

                return Ok(new ApiResponse<List<ReadUserNotificationDTO>>(true, "Notifications retrieved successfully!", mappedUserNotification));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
    }
}