using AutoMapper;
using hrconnectbackend.Helper;
using hrconnectbackend.IRepositories;
using hrconnectbackend.Models;
using hrconnectbackend.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace hrconnectbackend.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize(Roles = "Admin")] // Ensures only users with the "Admin" role can access this controller
public class AdminController : Controller
{
    private readonly IMapper _mapper;
    private readonly ILogger<AdminController> _logger;
    private readonly IEmployeeRepositories _employeeRepository;
    private readonly IEmployeeInfoRepositories _employeeInfoRepository;
    private readonly IAuthRepositories _authRepository;
    private readonly IAttendanceRepositories _attendanceRepository;
    private readonly IDepartmentRepositories _departmentRepository;
    private readonly ISupervisorRepositories _supervisorRepository;

    // Constructor: Initialize the required repositories and services
    public AdminController(ILogger<AdminController> logger, IMapper mapper, IEmployeeRepositories employeeRepository,
        IEmployeeInfoRepositories employeeInfoRepository, IAuthRepositories authRepository, IAttendanceRepositories attendanceRepository,
        IDepartmentRepositories departmentRepository, ISupervisorRepositories supervisorRepository)
    {
        _logger = logger;
        _mapper = mapper;
        _employeeRepository = employeeRepository;
        _employeeInfoRepository = employeeInfoRepository;
        _authRepository = authRepository;
        _attendanceRepository = attendanceRepository;
        _departmentRepository = departmentRepository;
        _supervisorRepository = supervisorRepository;
    }

    // Fetch all employees
    [HttpGet("employee-get-all")]
    public async Task<IActionResult> GetAllEmployees()
    {
        try
        {
            var employees = await _employeeRepository.GetAllEmployeesAsync();
            if (employees.Count == 0)
            {
                return NotFound(new { message = "No employees found" });
            }
            return Ok(employees);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching employees");
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request");
        }
    }

    // Update an employee's details
    [HttpPut("update-employee/{id}")]
    public async Task<IActionResult> UpdateEmployee(int id, [FromBody] UpdateEmployeeDTO employee)
    {
        try
        {
            if (employee == null)
                return BadRequest(new { message = "Invalid employee data" });

            var existingEmployee = await _employeeRepository.GetEmployeeByIdAsync(id);
            if (existingEmployee == null)
                return NotFound(new { message = "Employee not found" });

            // Update properties if provided in the request
            existingEmployee.Name = employee.Name ?? existingEmployee.Name;
            existingEmployee.Email = employee.Email ?? existingEmployee.Email;
            existingEmployee.SupervisorId = employee.SupervisorId ?? existingEmployee.SupervisorId;
            existingEmployee.DepartmentId = employee.DepartmentId ?? existingEmployee.DepartmentId;
            existingEmployee.Status = employee.Status != existingEmployee.Status ? employee.Status : existingEmployee.Status;
            existingEmployee.UpdatedAt = DateOnly.FromDateTime(DateTime.Now);

            var updatedEmployee = _mapper.Map<Employee>(existingEmployee);
            await _employeeRepository.UpdateEmployeeAsync(updatedEmployee);

            return Ok(updatedEmployee);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating employee");
            return StatusCode(StatusCodes.Status500InternalServerError, "Error updating employee record");
        }
    }

    // Create a new employee
    [HttpPost("create-employee")]
    public async Task<IActionResult> CreateEmployee([FromBody] CreateEmployeeDTO employee)
    {
        try
        {
            if (employee == null)
                return BadRequest(new { message = "Invalid employee data" });

            var existingEmployee = (await _employeeRepository.GetAllEmployeesAsync())
                                   .FirstOrDefault(e => e.Email == employee.Email);

            if (!EmailServices.IsValidEmail(employee.Email))
                return BadRequest(new { message = "Invalid email format" });

            if (existingEmployee != null)
                return BadRequest(new { message = "Email already exists" });

            if (employee.Status != "offline" && employee.Status != "online")
                return BadRequest(new { message = "Invalid status" });

            var employeeEntity = _mapper.Map<Employee>(employee);
            employeeEntity.Password = BCrypt.Net.BCrypt.HashPassword(employee.Password);
            employeeEntity.CreatedAt = DateOnly.FromDateTime(DateTime.Now);
            employeeEntity.UpdatedAt = DateOnly.FromDateTime(DateTime.Now);

            await _employeeRepository.AddEmployeeAsync(employeeEntity);

            // Create associated records
            await _authRepository.AddAsync(new Auth
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
                Age = 0
            });

            await _attendanceRepository.AddAsync(new Attendance
            {
                EmployeeId = employeeEntity.Id,
                ClockIn = TimeOnly.FromDateTime(DateTime.Now),
                ClockOut = TimeOnly.FromDateTime(DateTime.Now),
                DateToday = DateOnly.FromDateTime(DateTime.Now)
            });

            await _employeeInfoRepository.AddEducationBackgroundAsync(new EducationBackground
            {
                UserId = employeeEntity.Id,
                InstitutionName = "No institution name",
                Degree = "No degree",
                FieldOfStudy = "No field of study",
                StartDate = DateTime.Now,
                EndDate = DateTime.Now,
                GPA = 0.0
            });

            return Ok(new { message = "Employee created successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating employee");
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request");
        }
    }

    // Delete an employee
    [HttpDelete("delete-employee/{id}")]
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
            _logger.LogError(ex, "Error deleting employee");
            return StatusCode(StatusCodes.Status500InternalServerError, "Error deleting employee record");
        }
    }

    // Assign a department to an employee
    [HttpPut("assign-department/{employeeId}/{departmentId}")]
    public async Task<IActionResult> AssignDepartment(int employeeId, int departmentId)
    {
        try
        {
            var department = await _departmentRepository.GetByIdAsync(departmentId);
            if (department == null)
                return BadRequest(new { message = "Department not found" });

            var employee = await _employeeRepository.GetEmployeeByIdAsync(employeeId);
            if (employee == null)
                return NotFound(new { message = "Employee not found" });

            employee.DepartmentId = departmentId;
            await _employeeRepository.UpdateEmployeeAsync(employee);

            return Ok(new { message = "Department assigned successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning department");
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request");
        }
    }

    // Create a new department
    [HttpPost("create-department")]
    public async Task<IActionResult> CreateDepartment([FromBody] CreateDepartmentDTO department)
    {
        try
        {
            if (department == null)
                return BadRequest(new { message = "Invalid department data" });

            var departmentEntity = _mapper.Map<Department>(department);
            await _departmentRepository.AddAsync(departmentEntity);

            return Ok(new { message = "Department created successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating department");
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request");
        }
    }

    // Update an existing department
    [HttpPut("update-department/{id}")]
    public async Task<IActionResult> UpdateDepartment(int id, [FromBody] CreateDepartmentDTO department)
    {
        try
        {
            if (department == null)
                return BadRequest(new { message = "Invalid department data" });

            var existingDepartment = await _departmentRepository.GetByIdAsync(id);
            if (existingDepartment == null)
                return NotFound(new { message = "Department not found" });

            existingDepartment.DeptName = department.DeptName ?? existingDepartment.DeptName;
            await _departmentRepository.UpdateDepartmentByAsync(existingDepartment);

            return Ok(new { message = "Department updated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating department");
            return StatusCode(StatusCodes.Status500InternalServerError, "Error updating department record");
        }
    }

    // Delete a department
    [Authorize(Roles = "Admin")]
    [HttpDelete("delete-department/{id}")]
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
            _logger.LogError(ex, "Error deleting department");
            return StatusCode(StatusCodes.Status500InternalServerError, "Error deleting department record");
        }
    }
}
