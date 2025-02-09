using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AutoMapper;
using hrconnectbackend.Interface.Services;
using hrconnectbackend.Models;
using hrconnectbackend.Models.DTOs;
using hrconnectbackend.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace hrconnectbackend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserAccountController : Controller
    {
        private readonly UserAccountServices _userAccountServices;
        private readonly IEmployeeServices _employeeServices;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;

        public UserAccountController(UserAccountServices userAccountServices, IMapper mapper, IEmployeeServices employeeServices, IConfiguration configuration)
        {
            _userAccountServices = userAccountServices;
            _mapper = mapper;
            _employeeServices = employeeServices;
            _configuration = configuration;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO loginDTO)
        {
            if (loginDTO == null) return BadRequest(new { message = "Invalid login data" });

            var employee = await _employeeServices.GetEmployeeByEmail(loginDTO.Email);

            var employeePassword = await _userAccountServices.RetrievePassword(loginDTO.Email);

            var userName = await _userAccountServices.RetrieveUsername(loginDTO.Email);

            if (employee == null)
            {
                return NotFound(new { message = "Employee not found" });
            }

            if (!BCrypt.Net.BCrypt.Verify(loginDTO.Password, employeePassword))
            {
                return BadRequest(new { message = "Invalid password" });
            }

            var employeeDTO = _mapper.Map<ReadEmployeeDTO>(employee);
            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]);
            var tokenHandler = new JwtSecurityTokenHandler();

            string department = _employeeServices.GetEmployeeByEmail(loginDTO.Email).Result.Email;

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, userName),
                new Claim(ClaimTypes.Role, employee.IsAdmin ? "Admin" : "User"),
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var jwtToken = tokenHandler.WriteToken(token);

            return Ok(jwtToken);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAuth(int id)
        {
            var auth = await _userAccountServices.GetByIdAsync(id);

            if (auth == null)
                return NotFound(new { message = "Not Found" });

            if (!ModelState.IsValid) return BadRequest(ModelState);

            var authDTO = _mapper.Map<UserAccountDTO>(auth);
            return Ok(authDTO);
        }
    }
}
