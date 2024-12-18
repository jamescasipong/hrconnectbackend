using Microsoft.AspNetCore.Mvc;
using hrconnectbackend.Models;
using hrconnectbackend.IRepositories;
using AutoMapper;
using hrconnectbackend.Models.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace hrconnectbackend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class EmployeeController : ControllerBase
    {
        private readonly IEmployeeRepositories _employeeRepository;
        private readonly IMapper _mapper;

        public EmployeeController(IEmployeeRepositories employeeRepository, IMapper mapper)
        {
            _employeeRepository = employeeRepository;
            _mapper = mapper;
        }


        [HttpGet("supervisor/{id}")]
        public async Task<IActionResult> GetSupervisor(int id)
        {
            var supervisor = await _employeeRepository.GetSupervisor(id);

            if (supervisor == null) return NotFound(new { response = "This user doesn't have a superisor" });

            var supervisorDTO = _mapper.Map<EmployeeDTO>(supervisor);

            return Ok(supervisorDTO);
        }

        [HttpGet("getSupervisee/{id}")]
        public async Task<IActionResult> GetSupervisee(int id)
        {
            var supervisee = await _employeeRepository.GetSupervisee(id);

            if (supervisee == null || supervisee.Count() == 0) return NotFound(new { response = "This employee has no supervisee" });

            var superviseeDTO = _mapper.Map<List<EmployeeDTO>>(supervisee);

            return Ok(superviseeDTO);

        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var employees = await _employeeRepository.GetAllEmployeesAsync();

            var employeesDTO = _mapper.Map<ICollection<EmployeeDTO>>(employees);

            if (employees.Count == 0)
            {
                return NotFound();
            }

            return Ok(employeesDTO);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetEmployee(int id)
        {
            var employee = await _employeeRepository.GetEmployeeByIdAsync(id);

            if (employee == null)
            {
                return NotFound(new { message = "Employee not found" });
            }

            var employeeDTO = _mapper.Map<EmployeeDTO>(employee);
            return Ok(employeeDTO);
        }

        [HttpPost]
        public async Task<IActionResult> CreateEmployee([FromBody] EmployeeDTO employee)
        {
            try
            {
                if (employee == null)
                    return BadRequest(new { message = "Invalid employee data" });

                var matchingEmployee = _employeeRepository.GetAllEmployeesAsync().Result.FirstOrDefault(e => e.Email == employee.Email);


                bool emailExist = matchingEmployee != null && matchingEmployee.Email == employee.Email;


                if (emailExist)
                {
                    return BadRequest(new { message = $"Email Exist" });
                }

                var employeeEntity = _mapper.Map<Employee>(employee);

                employeeEntity.Password = BCrypt.Net.BCrypt.HashPassword(employee.Password);

                Console.WriteLine(employeeEntity.ToString());

                await _employeeRepository.AddEmployeeAsync(employeeEntity);


                return CreatedAtAction(nameof(GetEmployee), new { id = employee.Id }, employee);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex);
            }
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO loginDTO)
        {
            try
            {
                if (loginDTO == null) return BadRequest(new { message = "Invalid login data" });

                var employee = _employeeRepository.GetAllEmployeesAsync().Result.FirstOrDefault(e => e.Email == loginDTO.Email);

                if (employee == null)
                {
                    return NotFound(new { message = "Employee not found" });
                };

                if (!BCrypt.Net.BCrypt.Verify(loginDTO.Password, employee.Password))
                {
                    return BadRequest(new { message = "Invalid password" });
                };

                var employeeDTO = _mapper.Map<EmployeeDTO>(employee);

                return Ok
                (
                    new {
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

        // Add Employee (Optional method for handling bulk operations or specific add functionality)
        [HttpPost("add")]
        public async Task<IActionResult> AddEmployee([FromBody] Employee employee)
        {
            try
            {
                if (employee == null)
                    return BadRequest(new { message = "Invalid employee data" });

                await _employeeRepository.AddEmployeeAsync(employee);

                var employeeDTO = _mapper.Map<UpdateEmployeeDTO>(employee);

                return CreatedAtAction(nameof(GetEmployee), new { id = employee.Id }, employeeDTO);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error adding new employee record");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEmployee(int id)
        {
            try
            {
                var employee = await _employeeRepository.GetEmployeeByIdAsync(id);

                if (employee == null)
                    return NotFound(new { message = "Employee not found" });

                await _employeeRepository.DeleteEmployeeAsync(id);
                
                return Ok(new { message = "Employee deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error deleting employee record");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEmployee(int id, [FromBody] Employee employee)
        {
            try
            {
                if (employee == null)
                {
                    return BadRequest(new { message = "Invalid employee data" });
                }

                // Get the existing employee by ID
                var existingEmployee = await _employeeRepository.GetEmployeeByIdAsync(id);

                if (existingEmployee == null)
                {
                    return NotFound(new { message = "Employee not found" });
                }

                // Update specific properties if provided in the request
                if (!string.IsNullOrEmpty(employee.Name))
                {
                    existingEmployee.Name = employee.Name;
                }

                if (!string.IsNullOrEmpty(employee.Email))
                {
                    existingEmployee.Email = employee.Email;
                }

                if (employee.IsAdmin != existingEmployee.IsAdmin)
                {
                    existingEmployee.IsAdmin = employee.IsAdmin;
                }

                if (employee.SupervisorId.HasValue)
                {
                    existingEmployee.SupervisorId = employee.SupervisorId;
                }

                if (employee.DepartmentId.HasValue)
                {
                    existingEmployee.DepartmentId = employee.DepartmentId;
                }

                if (employee.status != existingEmployee.status)
                {
                    existingEmployee.status = employee.status;
                }

                existingEmployee.UpdatedAt = DateOnly.FromDateTime(DateTime.Now);

                var employeeDTO = _mapper.Map<UpdateEmployeeDTO>(existingEmployee);
                // Save changes to the repository
                await _employeeRepository.UpdateEmployeeAsync(employeeDTO);

                // Return the updated employee as DTO

                return Ok(employeeDTO); // Return the updated employee in the response
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error updating employee record");
            }
        }

        
    }
}
