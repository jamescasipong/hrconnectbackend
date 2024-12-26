using AutoMapper;
using hrconnectbackend.IRepositories;
using hrconnectbackend.Models;
using hrconnectbackend.Models.DTOs;
using hrconnectbackend.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace hrconnectbackend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : Controller
    {
        private readonly AuthRepositories _authRepositories;
        private readonly IEmployeeRepositories _employeeRepository;
        private readonly IMapper _mapper;

        public AuthController(AuthRepositories authRepositories, IMapper mapper, IEmployeeRepositories employeeRepository)
        {
            _authRepositories = authRepositories;
            _mapper = mapper;
            _employeeRepository = employeeRepository;
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO loginDTO)
        {
            try
            {
                if (loginDTO == null) return BadRequest(new { message = "Invalid login data" });

                var allEmployees = await _employeeRepository.GetAllEmployeesAsync();

                var employee = allEmployees.FirstOrDefault(e => e.Email == loginDTO.Email);

                if (employee == null)
                {
                    return NotFound(new { message = "Employee not found" });
                };

                if (!BCrypt.Net.BCrypt.Verify(loginDTO.Password, employee.Password))
                {
                    return BadRequest(new { message = "Invalid password; we only accept gmail, yahoo. Nigger." });
                };

                var employeeDTO = _mapper.Map<ReadEmployeeDTO>(employee);

                return Ok
                (
                    new
                    {
                        message = "Login successful",
                        employee = employeeDTO
                    }
                );
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error logging in");
            }
        }


        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] CreateEmployeeDTO employeeDTO)
        {
            try
            {
                var employee = await _employeeRepository.GetEmployeeByEmailAsync(employeeDTO.Email);

                if (employee != null)
                {
                    return BadRequest(new
                    {
                        message = "An Email Exist",
                        StatusCode = StatusCodes.Status409Conflict,
                    });
                }


                var newEmployee = new Employee
                {
                    Email = employeeDTO.Email,
                    Password = BCrypt.Net.BCrypt.HashPassword(employeeDTO.Password),
                    IsAdmin = employeeDTO.IsAdmin,
                    EmployeeInfo = new EmployeeInfo
                    {
                        FirstName = "N/A",
                        LastName = "N/A",
                        Address = "N/A",
                        BirthDate = null,
                        Age = null,
                    }
                };


                await _employeeRepository.AddEmployeeAsync(newEmployee);


                return Ok(new { message = "Leave application created successfully" });
            }
            catch (Exception ex)
            {
                return Ok(new
                {
                    message = "Invalid BodyRequest",
                    StatusCode = StatusCodes.Status500InternalServerError,
                    Exception = ex.Message
                });
            }
        }


        // GET: /auth/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAuth(int id)
        {
            var auth = await _authRepositories.GetByIdAsync(id);

            if (auth == null)
                return NotFound(new { message = "Not Found" });

            if (!ModelState.IsValid) return BadRequest(ModelState);

            var authDTO = _mapper.Map<AuthDTO>(auth);
            return Ok(authDTO);
        }

        // GET: /auth
        [HttpGet]
        public async Task<IActionResult> GetListAuth()
        {
            var auths = await _authRepositories.GetAllAsync();

            if (auths.Count == 0) return NotFound();

            if (!ModelState.IsValid) return BadRequest(ModelState);

            var authsDTO = _mapper.Map<List<AuthDTO>>(auths);
            return Ok(authsDTO);
        }

        // POST: /auth (Create)
        [HttpPost]
        public async Task<IActionResult> CreateAuth([FromBody] AuthDTO authDTO)
        {
            if (authDTO == null)
                return BadRequest(new { message = "Invalid data" });

            // Map the DTO to the entity
            var auth = _mapper.Map<Auth>(authDTO);

            // Save the entity in the repository
            var createdAuth = await _authRepositories.AddAsync(auth);

            if (createdAuth == null)
                return StatusCode(500, new { message = "Failed to create the record" });

            // Map the created entity to DTO for response
            var createdAuthDTO = _mapper.Map<AuthDTO>(createdAuth);

            // Return the created object with 201 status code
            return CreatedAtAction(nameof(GetAuth), new { id = createdAuthDTO.AuthEmpId }, createdAuthDTO);
        }

        // DELETE: /auth/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAuth(int id)
        {
            var auth = await _authRepositories.GetByIdAsync(id);

            if (auth == null)
                return NotFound(new { message = "Auth not found" });

            await _authRepositories.DeleteAsync(auth);

            return Ok(new
            {
                message = "Auth deleted"
            });
        }


    }
}
