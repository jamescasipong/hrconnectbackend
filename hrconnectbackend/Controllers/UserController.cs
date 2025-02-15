using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AutoMapper;
using hrconnectbackend.Helper;
using hrconnectbackend.Interface.Services;
using hrconnectbackend.Models;
using hrconnectbackend.Models.DTOs;
using hrconnectbackend.Repositories;
using hrconnectbackend.Services;
using hrconnectbackend.SignalR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.IdentityModel.Tokens;

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
        private readonly IConfiguration _configuration;
        private readonly IHubContext<NotificationHub> _hubContext;
        public UserController(IUserAccountServices userAccountServices, IMapper mapper, IEmployeeServices employeeServices, IConfiguration configuration, IUserSettingsServices userSettingsServices, IHubContext<NotificationHub> hubContext, INotificationServices notificationServices)
        {
            _userAccountServices = userAccountServices;
            _mapper = mapper;
            _employeeServices = employeeServices;
            _configuration = configuration;
            _hubContext = hubContext;
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
                if (loginDTO == null) return BadRequest(new ApiResponse(success: false, message: "Body request is requured."));

                var employee = await _employeeServices.GetEmployeeByEmail(loginDTO.Email);

                var employeePassword = await _userAccountServices.RetrievePassword(loginDTO.Email);

                var userName = await _userAccountServices.RetrieveUsername(loginDTO.Email);

                if (employee == null)
                {
                    return NotFound(new ApiResponse(false, $"An employee with email: {loginDTO.Email} not found."));
                }

                if (!BCrypt.Net.BCrypt.Verify(loginDTO.Password, employeePassword))
                {
                    return BadRequest(new ApiResponse(false, $"Invalid password; try again.s"));
                }

                if (!await _userAccountServices.IsVerified(loginDTO.Email))
                {
                    return Ok(new ApiResponse(true, $"User Account not verified."));
                }

                var employeeDTO = _mapper.Map<ReadEmployeeDTO>(employee);
                var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]);
                var tokenHandler = new JwtSecurityTokenHandler();


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

                return Ok(new ApiResponse<string>(success: false, message: $"A user has authenticated successfully!", jwtToken));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponse(false, ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal Server Error");
            }
        }

        [HttpGet("account/{id}")]
        public async Task<IActionResult> GetUserAccount(int id)
        {
            var auth = await _userAccountServices.GetByIdAsync(id);

            if (auth == null)
                return NotFound(new ApiResponse(false, $"User Account with ID: {id} not found."));

            if (!ModelState.IsValid) return BadRequest(new ApiResponse(success: false, message: ModelState.IsValid.ToString()));

            var authDTO = _mapper.Map<UserAccountDTO>(auth);

            return Ok(new ApiResponse(success: false, message: $"User Account with ID: {id} has been retrieved successfully!"));
        }

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

        [HttpDelete("notifications")]
        public async Task<IActionResult> DeleteNotification(int userId)
        {
            try
            {
                var notification = await _notificationServices.GetNotificationsByEmployeeId(userId);

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