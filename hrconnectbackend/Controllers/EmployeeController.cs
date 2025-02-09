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

namespace hrconnectbackend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class EmployeeController : ControllerBase
    {
        private readonly IEmployeeServices _employeeService;
        private readonly IAboutEmployeeServices _employeeInfoService;
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
            IAboutEmployeeServices employeeInfoService,
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
            _employeeInfoService = employeeInfoService;
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
        public async Task<IActionResult> CreateEmployee([FromBody] CreateEmployeeDTO employee)
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

                return Ok(new
                {
                    messsage = "Successfully Created!",
                    status = 200
                });  // Return a success message
            }
            catch (ArgumentNullException ex)
            {
                _logger.LogError(ex, "Error creating employee due to null data.");
                return BadRequest(ex.Message);  // Return BadRequest for specific exceptions
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Error creating employee: Invalid argument.");
                return BadRequest(ex.Message);  // Return BadRequest for invalid arguments
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Error creating employee: Invalid operation.");
                return Conflict(ex.Message);  // Return Conflict for specific scenarios like existing employee
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while creating employee.");
                return StatusCode(500, "Internal server error");  // Handle unexpected errors
            }
        }

        [HttpPut("update-username/{id}")]
        public async Task<IActionResult> ChangeUserName(int id, string name)
        {
            var user = await _userAccountServices.GetByIdAsync(id);

            try
            {
                if (user == null) throw new KeyNotFoundException("User not found.");

                user.UserName = name;

                await _userAccountServices.UpdateAsync(user);
                return Ok(new
                {
                    message = "Success!"
                });

            }
            catch(KeyNotFoundException ex)
            {
                return NotFound(new
                {
                    ex.Message,
                });
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }

}



