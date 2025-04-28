using System.Security.Claims;
using AutoMapper;
using hrconnectbackend.Helper;
using hrconnectbackend.Interface.Services;
using hrconnectbackend.Interface.Services.Clients;
using hrconnectbackend.Models;
using hrconnectbackend.Models.DTOs;
using hrconnectbackend.Models.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace hrconnectbackend.Controllers.v1.Clients;

[Authorize]
[ApiController]
[Route("api/v{version:apiVersion}/supervisor")]
[ApiVersion("1.0")]
public class SupervisorController(
    ISupervisorServices supervisorServices,
    ILeaveApplicationServices leaveApplicationServices,
    INotificationServices notificationServices,
    IMapper mapper,
    IServiceProvider serviceProvider)
    : Controller
{
    private readonly ILeaveApplicationServices _leaveApplicationServices = leaveApplicationServices;
    private readonly INotificationServices _notificationServices = notificationServices;
    private readonly IServiceProvider _serviceProvider = serviceProvider;
    
    [Authorize(Roles ="Admin,HR")]
    [HttpGet]
    public async Task<IActionResult> GetAllSupervisors()
    {
        try
        {
            var supervisors = await supervisorServices.GetAllSupervisors();

            var mappedSupervisors = mapper.Map<List<ReadSupervisorDto>>(supervisors);

            if (!supervisors.Any())
            {
                return Ok(new ApiResponse<List<ReadSupervisorDto>?>(false, $"Supervisors not found.", mappedSupervisors));
            }

            return Ok(new ApiResponse<List<ReadSupervisorDto>?>(true, $"Supervisors retreved successfully!", mappedSupervisors));
        }
        catch (Exception)
        {
            return StatusCode(500, new ApiResponse(false, $"Internal Server Error"));
        }
    }
    [Authorize]
    [HttpGet("{supervisorId:int}")]
    public async Task<IActionResult> GetSupervisor(int supervisorId)
    {
        try
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var supervisor = await supervisorServices.GetSupervisor(supervisorId);

            if (supervisor == null)
            {
                return NotFound(new ApiResponse(false, $"Supervisor with id: {supervisorId} not found."));
            }

            var mapped = mapper.Map<ReadSupervisorDto>(supervisor);

            return Ok(new ApiResponse<ReadSupervisorDto?>(true, $"Supervisor with id: {supervisorId} retrieved successfully!", mapped));
        }
        catch (Exception)
        {
            return StatusCode(500, new ApiResponse(false, $"Internal Server Error"));
        }
    }
    [Authorize(Roles ="Admin,HR")]
    [HttpDelete("{supervisorId:int}")]
    public async Task<IActionResult> DeleteSupervisor(int supervisorId)
    {
        try
        {
            var supervisor = await supervisorServices.GetSupervisor(supervisorId);

            if (supervisor == null) return NotFound(new ApiResponse(false, $"Supervisor with id: {supervisorId} not found."));

            return Ok(new ApiResponse<ReadSupervisorDto>(false, $"Supervisor with id: {supervisorId} retrieved successfully."));
        }
        catch (Exception)
        {
            return StatusCode(500, new ApiResponse(false, $"Internal Server Error"));
        }
    }
    [Authorize]
    [HttpGet("{supervisorId:int}/employee")]
    public async Task<IActionResult> RetrieveEmployeesBySupervisor(int supervisorId)
    {
        try
        {
            var employee = await supervisorServices.GetAllEmployeesByASupervisor(supervisorId);

            var mapped = mapper.Map<List<ReadEmployeeDto>>(employee);

            return Ok(new ApiResponse<List<ReadEmployeeDto>?>(false, $"Employees under a supervisor with id: {supervisorId} retrieved successfully.", mapped));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ApiResponse(false, ex.Message));
        }
        catch (Exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new ApiResponse(false, "Internal Server Error"));
        }
    }
    [Authorize]
    [HttpGet("employee/{employeeId:int}")]
    public async Task<IActionResult> RetrieveEmployeeSupervisor(int employeeId)
    {
        try
        {
            var employeeSupervisor = await supervisorServices.GetEmployeeSupervisor(employeeId);

            return Ok(new ApiResponse<ReadSupervisorDto?>(false, $"Employee with id: {employeeId} retrieve its supervisor successfully.", mapper.Map<ReadSupervisorDto>(employeeSupervisor)));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ApiResponse(false, ex.Message));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ApiResponse(false, ex.Message));
        }
        catch (Exception)
        {
            return StatusCode(500, new ApiResponse(false, $"Internal Server Error"));
        }
    }

    
}
