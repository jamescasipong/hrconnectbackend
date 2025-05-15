using System.Security.Claims;
using AutoMapper;
using hrconnectbackend.Constants;
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

    [Authorize(Roles = "Admin,HR")]
    [HttpGet]
    public async Task<IActionResult> GetAllSupervisors()
    {

        var supervisors = await supervisorServices.GetAllSupervisors();

        var mappedSupervisors = mapper.Map<List<ReadSupervisorDto>>(supervisors);

        if (!supervisors.Any())
        {
            return StatusCode(404, new ErrorResponse(ErrorCodes.SubscriptionNotFound, $"No supervisors found."));
        }

        return Ok(new SuccessResponse<List<ReadSupervisorDto>>(mappedSupervisors, $"All supervisors retrieved successfully."));

    }
    [Authorize]
    [HttpGet("{supervisorId:int}")]
    public async Task<IActionResult> GetSupervisor(int supervisorId)
    {

        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var supervisor = await supervisorServices.GetSupervisor(supervisorId);

        var mapped = mapper.Map<ReadSupervisorDto>(supervisor);

        return Ok(new SuccessResponse<ReadSupervisorDto>(mapped, $"Supervisor with id: {supervisorId} retrieved successfully."));

    }
    [Authorize(Roles = "Admin,HR")]
    [HttpDelete("{supervisorId:int}")]
    public async Task<IActionResult> DeleteSupervisor(int supervisorId)
    {

        var supervisor = await supervisorServices.GetSupervisor(supervisorId);

        return Ok(new SuccessResponse<ReadSupervisorDto>(mapper.Map<ReadSupervisorDto>(supervisor), $"Supervisor with id: {supervisorId} deleted successfully."));

    }
    [Authorize]
    [HttpGet("{supervisorId:int}/employee")]
    public async Task<IActionResult> RetrieveEmployeesBySupervisor(int supervisorId)
    {

        var employee = await supervisorServices.GetAllEmployeesByASupervisor(supervisorId);

        var mapped = mapper.Map<List<ReadEmployeeDto>>(employee);

        return Ok(new SuccessResponse<List<ReadEmployeeDto>>(mapped, $"Employees under supervisor with id: {supervisorId} retrieved successfully."));
    }
    [Authorize]
    [HttpGet("employee/{employeeId:int}")]
    public async Task<IActionResult> RetrieveEmployeeSupervisor(int employeeId)
    {

        var employeeSupervisor = await supervisorServices.GetEmployeeSupervisor(employeeId);

        return Ok(new SuccessResponse<ReadSupervisorDto>(mapper.Map<ReadSupervisorDto>(employeeSupervisor), $"Supervisor of employee with id: {employeeId} retrieved successfully."));


    }


}
