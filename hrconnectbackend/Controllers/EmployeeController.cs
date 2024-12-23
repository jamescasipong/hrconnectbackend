using Microsoft.AspNetCore.Mvc;
using hrconnectbackend.Models;
using hrconnectbackend.IRepositories;
using AutoMapper;
using hrconnectbackend.Models.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;
using hrconnectbackend;
using hrconnectbackend.Helper;
using hrconnectbackend.Repositories;
using hrconnectbackend.Models.DTOs.EmployeeDTOs;

namespace hrconnectbackend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class EmployeeController : ControllerBase
    {
        private readonly IEmployeeRepositories _employeeRepository;
        private readonly IEmployeeInfoRepositories _employeeInfoRepository;
        private readonly AuthRepositories _authRepositories;
        private readonly SupervisorRepositories _supervisorRepositories;
        private readonly IDepartmentRepositories _departmentRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<EmployeeController> _logger;

        public EmployeeController(IEmployeeRepositories employeeRepository, IMapper mapper, IEmployeeInfoRepositories employeeInfoRepositories, IDepartmentRepositories departmentRepository, ILogger<EmployeeController> logger, AuthRepositories authRepositories, SupervisorRepositories supervisorRepositories)
        {
            _employeeInfoRepository = employeeInfoRepositories;
            _supervisorRepositories = supervisorRepositories;
            _authRepositories = authRepositories;
            _employeeRepository = employeeRepository;
            _mapper = mapper;
            _departmentRepository = departmentRepository;
            _logger = logger;
        }



        [HttpGet("supervisor/{id}")]
        public async Task<IActionResult> GetSupervisor(int id)
        {
            var supervisor = await _employeeRepository.GetSupervisor(id);

            if (supervisor == null) return NotFound(new { response = "This user doesn't have a superisor" });

            var supervisorDTO = _mapper.Map<ReadEmployeeDTO>(supervisor);

            return Ok(supervisorDTO);
        }

        [HttpGet("getSupervisee/{id}")]
        public async Task<IActionResult> GetSupervisee(int id)
        {
            var supervisee = await _employeeRepository.GetSupervisee(id);

            if (supervisee == null || supervisee.Count() == 0) return NotFound(new { response = "This employee has no supervisee" });

            var superviseeDTO = _mapper.Map<List<ReadEmployeeDTO>>(supervisee);

            return Ok(superviseeDTO);

        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var employees = await _employeeRepository.GetAllEmployeesAsync();

            var employeesDTO = _mapper.Map<ICollection<ReadEmployeeDTO>>(employees);


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


            var employeeDTO = _mapper.Map<ReadEmployeeDTO>(employee);


            return Ok(employeeDTO);
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateEmployee([FromBody] CreateEmployeeDTO employee)
        {
            try
            {
                if (employee == null)
                    return BadRequest(new { message = "Invalid employee data" });

                var matchingEmployee = (await _employeeRepository.GetAllEmployeesAsync())
                                       .FirstOrDefault(e => e.Email == employee.Email);

                if (EmailServices.IsValidEmail(employee.Email))
                {
                    if (matchingEmployee != null)
                    {
                        return BadRequest(new { message = "Email already exists" });
                    }

                    if (employee.Status != "offline" && employee.Status != "online")
                    {
                        return BadRequest(new { message = "Invalid status" });
                    }


                    var employeeEntity = _mapper.Map<Employee>(employee);
                    employeeEntity.Password = BCrypt.Net.BCrypt.HashPassword(employee.Password);
                    employeeEntity.CreatedAt = DateOnly.FromDateTime(DateTime.Now);
                    employeeEntity.UpdatedAt = DateOnly.FromDateTime(DateTime.Now);
                    employeeEntity.SupervisorId = null;
                    employeeEntity.DepartmentId = null;

                    await _employeeRepository.AddEmployeeAsync(employeeEntity);

                    await _authRepositories.AddAsync(new Auth
                    {
                        AuthEmpId = employeeEntity.Id,
                        VerificationCode = 0,
                        IsAuthenticated = false,
                        PhoneConfirmed = false,
                        EmailConfirmed = false
                    });

                    await _employeeInfoRepository.AddEmployeeInfoAsync(new EmployeeInfo
                    {
                        EmployeeInfoId = employeeEntity.Id,
                        FirstName = "No first name",
                        LastName = "No last name",
                        Address = "No address",
                        BirthDate = DateOnly.FromDateTime(DateTime.Now),
                        EducationalBackground = "No educational background",
                        Age = 0,
                    });


                    return Ok(new
                    {
                        message = "Employee created successfully",
                    });
                }
                else
                {
                    return BadRequest(new { message = "Invalid email format" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating employee");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request");
            }
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

        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateEmployee(int id, [FromBody] UpdateEmployeeDTO employee)
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

                if (employee.SupervisorId.HasValue)
                {
                    existingEmployee.SupervisorId = employee.SupervisorId;
                }

                if (employee.DepartmentId.HasValue)
                {
                    existingEmployee.DepartmentId = employee.DepartmentId;
                }

                if (employee.Status != existingEmployee.Status)
                {
                    existingEmployee.Status = employee.Status;
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


        [HttpGet("department")]
        public async Task<IActionResult> GetAllDeparments()
        {
            var departments = await _departmentRepository.GetAllAsync();

            if (departments.Count == 0)
            {
                return NotFound(new { message = "No departments found" });
            }

            return Ok(departments);
        }

        [HttpGet("department/{id}")]
        public async Task<IActionResult> GetDepartment(int id)
        {
            var department = await _departmentRepository.GetByIdAsync(id);

            if (department == null)
            {
                return NotFound(new { message = "Department not found" });
            }

            return Ok(department);
        }

        [HttpPost("department/create")]
        public async Task<IActionResult> CreateDepartment([FromBody] CreateDepartmentDTO department)
        {
            try
            {
                if (department == null)
                {
                    return BadRequest(new { message = "Invalid department data" });
                }

                var departmentEntity = _mapper.Map<Department>(department);

                await _departmentRepository.AddAsync(departmentEntity);

                return CreatedAtAction(nameof(GetDepartment), new { id = departmentEntity.DepartmentId }, department);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpPut("department/update/{id}")]
        public async Task<IActionResult> UpdateDepartment(int id, [FromBody] CreateDepartmentDTO department)
        {
            try
            {
                if (department == null)
                {
                    return BadRequest(new { message = "Invalid department data" });
                }

                // Get the existing department by ID
                var existingDepartment = await _departmentRepository.GetByIdAsync(id);

                if (existingDepartment == null)
                {
                    return NotFound(new { message = "Department not found" });
                }

                // Update specific properties if provided in the request
                if (!string.IsNullOrEmpty(department.DeptName))
                {
                    existingDepartment.DeptName = department.DeptName;
                }




                // Save changes to the repository
                await _departmentRepository.UpdateDepartmentByAsync(existingDepartment);

                // Return the updated department as DTO

                return Ok(_departmentRepository); // Return the updated department in the response
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error updating department record");
            }
        }

        [HttpDelete("department/delete/{id}")]
        public async Task<IActionResult> DeleteDepartment(int id)
        {
            try
            {
                var department = await _departmentRepository.GetByIdAsync(id);

                if (department == null)
                    return NotFound(new { message = "Department not found" });

                await _departmentRepository.DeleteAsync(department);

                return Ok(new { message = "Department deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error deleting department record");
            }
        }


        [HttpPost("createSupervisor/{id}")]
        public async Task<IActionResult> CreateSupervisor(int id)
        {
            try
            {
                var employee = await _employeeRepository.GetEmployeeByIdAsync(id);


                var dtoSupervisor = new Supervisor
                {
                    EmployeeId = id,
                };

                var supervisorEntity = _mapper.Map<Supervisor>(dtoSupervisor);

                await _supervisorRepositories.CreateSupervisor(supervisorEntity);

                return Ok(new { message = "Supervisor created successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpGet("getSupervisee")]
        public async Task<IActionResult> GetSupervisee()
        {
            var supervisee = await _supervisorRepositories.GetAllSupervisors();

            if (supervisee == null || supervisee.Count() == 0) return NotFound(new { response = "This employee has no supervisee" });

            var superviseeDTO = _mapper.Map<List<ReadEmployeeDTO>>(supervisee);

            return Ok(superviseeDTO);

        }

    }
}
