using System.Net;
using Microsoft.AspNetCore.Mvc;
using hrconnectbackend.Models;
using AutoMapper;
using hrconnectbackend.Models.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;
using hrconnectbackend;
using hrconnectbackend.Helper;
using hrconnectbackend.Repositories;
using hrconnectbackend.Interface.Services;
using Microsoft.AspNetCore.Authorization;

namespace hrconnectbackend.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class EmployeeController : ControllerBase
    {
        private readonly IEmployeeServices _employeeService;
        private readonly IAboutEmployeeServices _aboutEmployeeServices;
        private readonly UserAccountServices _userAccountServices;
        private readonly SupervisorServices _supervisorService;
        private readonly IShiftServices _shiftService;
        private readonly IDepartmentServices _departmentService;
        private readonly ILeaveApplicationServices _leaveApplicationService;
        private readonly IAttendanceServices _attendanceService;
        private readonly INotificationServices _notificationService;
        private readonly IMapper _mapper;
        private readonly ILogger<EmployeeController> _logger;

        public EmployeeController(
            IEmployeeServices employeeService,
            IMapper mapper,
            IAboutEmployeeServices aboutEmployeeServices,
            IDepartmentServices departmentService,
            ILogger<EmployeeController> logger,
            UserAccountServices userAccountServices,
            SupervisorServices supervisorService,
            ILeaveApplicationServices leaveApplicationService,
            IAttendanceServices attendanceService,
            IShiftServices shiftService,
            INotificationServices notificationService)
        {
            _attendanceService = attendanceService;
            _aboutEmployeeServices = aboutEmployeeServices;
            _supervisorService = supervisorService;
            _userAccountServices = userAccountServices;
            _employeeService = employeeService;
            _mapper = mapper;
            _departmentService = departmentService;
            _logger = logger;
            _leaveApplicationService = leaveApplicationService;
            _shiftService = shiftService;
            _notificationService = notificationService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateEmployee([FromBody] CreateEmployeeDTO employee, bool? createAccount)
        {
            if (employee == null)
            {
                _logger.LogWarning("Received null data for employee creation.");
                return BadRequest("Employee data cannot be null.");
            }

            try
            {
                // Call the CreateEmployeeAsync method
                await _employeeService.CreateEmployee(employee);

                return Ok(new ApiResponse(true, $"Employee created successfully!"));  // Return a success message
            }
            catch (ArgumentNullException ex)
            {
                _logger.LogError(ex, "Error creating employee due to null data.");
                return BadRequest(new ApiResponse(false, ex.Message));  // Return BadRequest for specific exceptions
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Error creating employee: Invalid argument.");
                return BadRequest(new ApiResponse(false, ex.Message)); // Return BadRequest for invalid arguments
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Error creating employee: Invalid operation.");
                return Conflict(new ApiResponse(false, ex.Message));  // Return Conflict for specific scenarios like existing employee
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while creating employee.");
                return StatusCode(500, "Internal server error");  // Handle unexpected errors
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetEmployees([FromQuery] int? pageIndex, [FromQuery] int? pageSize)
        {
            try
            {
                var employees = new List<Employee>();

                // Check if pagination parameters are provided
                if (pageIndex == null || pageSize == null)
                {
                    employees = await _employeeService.GetAllAsync();
                }
                else
                {
                    if (pageIndex <= 0)
                    {
                        return BadRequest(new ApiResponse(false, "Page index must be greater than 0"));
                    }

                    if (pageSize <= 0)
                    {
                        return BadRequest(new ApiResponse(false, "Page size must be greater than 0"));
                    }

                    // Apply pagination only if pageIndex and pageSize are not null
                    employees = await _employeeService.GetAllAsync();
                    employees = employees.Skip((pageIndex.Value - 1) * pageSize.Value)
                                         .Take(pageSize.Value)
                                         .ToList();
                }

                if (!employees.Any())
                {
                    return NotFound(new ApiResponse(false, $"No employees exist"));
                }

                var employeesDTO = _mapper.Map<List<ReadEmployeeDTO>>(employees);

                return Ok(new ApiResponse<List<ReadEmployeeDTO>>(true, $"Employees retrieved successfully!", employeesDTO));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving employees");
                return StatusCode(500, new ApiResponse(false, "Internal server error"));
            }
        }


        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetEmployee(int id)
        {
            try
            {
                var employee = await _employeeService.GetByIdAsync(id);

                if (employee == null)
                {
                    return NotFound(new ApiResponse(false, $"Employee with an ID: {id} not found."));
                }

                var employeeDTO = _mapper.Map<ReadEmployeeDTO>(employee);

                return Ok(new ApiResponse<ReadEmployeeDTO>(true, $"Employee with an ID: {id} retrieved successfully!", employeeDTO));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving an employee with ID: {id}", id);
                return StatusCode(500, new ApiResponse(false, "Internal server error"));
            }
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateEmployee(int id, UpdateEmployeeDTO employeeDTO)
        {
            var employee = await _employeeService.GetByIdAsync(id);

            try
            {
                if (employee == null)
                {
                    return NotFound(new ApiResponse(false, $"Employee with an ID: {id} not found."));
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                employee.FirstName = employee.FirstName;
                employee.LastName = employee.LastName;
                employee.Email = employee.Email;
                employee.Status = employee.Status;

                await _employeeService.UpdateAsync(employee);

                return Ok(new ApiResponse(true, $"Employee with an ID: {id} updated successfully"));
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error updating an employee with ID {id}", id);
                return StatusCode(500, new ApiResponse(false, "Internal server error"));
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteEmployee(int id)
        {
            var employee = await _employeeService.GetByIdAsync(id);

            try
            {
                if (employee == null)
                {
                    return NotFound(new ApiResponse(false, $"Employee with ID: {id} not found."));
                }

                

                await _employeeService.DeleteAsync(employee);

                return Ok(new ApiResponse(true, $"Employee with ID: {id} deleted successfully!"));
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error deleting an employee with ID {id}", id);
                return StatusCode(500, new ApiResponse(false, "Internal server error"));
            }
        }

        [HttpGet("department/{departmentId}")]
        public async Task<IActionResult> RetrieveEmployeeByDepartment(int deptId, int? pageIndex, int? pageSize)
        {
            try
            {
                var employeesByDept = await _employeeService.GetEmployeeByDepartment(deptId, pageIndex, pageSize);

                var employeesMapped = _mapper.Map<List<ReadEmployeeDTO>>(employeesByDept);
                
                return Ok(new ApiResponse<List<ReadEmployeeDTO>>(false, $"Employees under a department {deptId} retrieved successfully.", employeesMapped));
            }
            catch (KeyNotFoundException ex)
            {
                return Ok(new ApiResponse(false, ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse(false, $"Internal Server Error"));
            }
        }

        [HttpPut("update-username/{accountId:int}")]
        public async Task<IActionResult> ChangeUserName(int accountId, string name)
        {
            var user = await _userAccountServices.GetByIdAsync(accountId);

            try
            {
                if (user == null) return NotFound(new ApiResponse(false, $"Employee account with account ID: {accountId} not found."));

                user.UserName = name;

                await _userAccountServices.UpdateAsync(user);
                return Ok(new ApiResponse(true, $"Employee's account username with account ID: {accountId} changed to {name} successfully!"));

            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error updating an employee's account username with account ID {id}", accountId);
                return StatusCode(500, new ApiResponse(false, "Internal server error"));
            }
        }
    }

}



