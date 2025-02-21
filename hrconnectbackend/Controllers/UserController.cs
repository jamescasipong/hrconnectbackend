using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AutoMapper;
using hrconnectbackend.Helper;
using hrconnectbackend.Interface.Services;
using hrconnectbackend.Models;
using hrconnectbackend.Models.DTOs;
using hrconnectbackend.Models.DTOs.GenericDTOs;
using hrconnectbackend.Repositories;
using hrconnectbackend.Services;
using hrconnectbackend.SignalR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.IdentityModel.Tokens;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Pqc.Crypto.Lms;

namespace hrconnectbackend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : Controller
    {
        private readonly IUserAccountServices _userAccountServices;
        private readonly IUserSettingsServices _userSettingsServices;
        private readonly INotificationServices _notificationServices;
        private readonly IEmployeeServices _employeeServices;
        private readonly IMapper _mapper;
        private readonly ILogger<UserController> _logger;
        private readonly IConfiguration _configuration;
        public UserController(IUserAccountServices userAccountServices, ILogger<UserController> logger, IMapper mapper, IEmployeeServices employeeServices, IConfiguration configuration, IUserSettingsServices userSettingsServices, INotificationServices notificationServices)
        {
            _userAccountServices = userAccountServices;
            _userSettingsServices = userSettingsServices;
            _mapper = mapper;
            _employeeServices = employeeServices;
            _configuration = configuration;
            _logger = logger;
            _notificationServices = notificationServices;
        }

        [HttpPost("account/login")]
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
                    return Unauthorized(new ApiResponse(false, $"Invalid password; try again.s"));
                }

                //if (!await _userAccountServices.IsVerified(loginDTO.Email))
                //{
                //    return Ok(new ApiResponse(true, $"User Account not verified."));
                //}

                var employeeDTO = _mapper.Map<ReadEmployeeDTO>(employee);
                byte[] key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]);
                JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();


                var claims = new[]
                {
                    new Claim(ClaimTypes.Name, userName),
                    new Claim(ClaimTypes.Role, employee.IsAdmin ? "Admin" : "User"),
                };

                if (employee.Department != null)
                {
                    claims.Append(new Claim("Department", employee.Department.DeptName));
                }

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(claims),
                    Expires = DateTime.UtcNow.AddHours(1),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                };
                var token = tokenHandler.CreateToken(tokenDescriptor);
                var jwtToken = tokenHandler.WriteToken(token);  

                //Response.Cookies.Append("token", jwtToken, new CookieOptions
                //{
                //    HttpOnly = true,  // Secure from JavaScript (prevent XSS)
                //    SameSite = SameSiteMode.Lax, // Prevent CSRF attacks
                //    Secure = false,
                //    Domain = "localhost",
                //    Expires = DateTime.UtcNow.AddHours(1), // Cookie expires in 1 hour
                //    Path = "/"
                //});


                _logger.LogWarning("$A user has authenticated successfully!");
                return Ok(new ApiResponse<string>(success: true, message: $"A user has authenticated successfully!", data: jwtToken));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponse(false, ex.Message));
            }
            catch (Exception)
            {
                return StatusCode(500, new ApiResponse(false, $"Internal Server Error"));
            }
        }

        //[HttpPost("logout")]
        //public IActionResult Logout()
        //{
        //    // Remove the JWT cookie by setting it with an expired date
        //    Response.Cookies.Append("token", "", new CookieOptions
        //    {
        //        HttpOnly = true,
        //        SameSite = SameSiteMode.Lax,
        //        Expires = DateTime.UtcNow.AddDays(-1),// Expire immediately
        //        Path = "/"
        //    });

        //    return Ok(new { message = "Logged out successfully" });
        //}


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
        public async Task<IActionResult> RetrieveNotification(int userId)
        {
            try
            {
                var notifications = await _notificationServices.GetAllAsync();

                var notification = notifications.Where(n => n.EmployeeId == userId);

                if (notification == null)
                {
                    return NotFound(new ApiResponse(false, "No notifications found."));
                }

                var notificationDTO = notification.Select(n => new CreateNotificationHubDTO
                {
                    EmployeeId = n.EmployeeId,
                    Title = n.Title,
                    Message = n.Message
                }).ToList();

                return Ok(new ApiResponse<List<CreateNotificationHubDTO>>(true, "Notifications retrieved successfully!", notificationDTO));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [Authorize]
        [HttpDelete("notifications")]
        public async Task<IActionResult> DeleteNotification(int userId, int? pageIndex, int? pageSize)
        {
            try
            {
                var notification = await _notificationServices.GetNotificationsByEmployeeId(userId, pageIndex, pageSize);

                if (notification == null)
                {
                    return NotFound(new ApiResponse(false, "No notifications found."));
                }

                foreach (var n in notification)
                {
                    if (n != null)
                    {
                        await _notificationServices.DeleteAsync(n); // Await each deletion sequentially
                    }
                }

                return Ok(new ApiResponse(true, "Notification deleted successfully!"));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        
    }
}