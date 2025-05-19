using System.Security.Claims;
using AutoMapper;
using hrconnectbackend.Constants;
using hrconnectbackend.Extensions;
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
[Route("api/v{version:apiVersion}/supervisors")]
[ApiVersion("1.0")]
public class SupervisorController(
    ISupervisorServices supervisorServices,
    IMapper mapper) : ControllerBase
{
    [Authorize(Roles = "Admin,HR")]
    [HttpGet]
    public async Task<IActionResult> GetAllSupervisors([FromQuery] PaginationParams paginationParams)
    {
        var organizationId = User.RetrieveSpecificUser("organizationId")!;

        var supervisors = await supervisorServices.GetAllSupervisors(int.Parse(organizationId), paginationParams);
        var mappedSupervisors = mapper.Map<List<ReadSupervisorDto>>(supervisors.Data);

        var pagedResponse = new PagedResponse<IEnumerable<ReadSupervisorDto>>(
            mappedSupervisors,
            supervisors.Pagination);

        return Ok(pagedResponse);
    }

    [Authorize]
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetSupervisorById(int id)
    {
        var supervisor = await supervisorServices.GetSupervisor(id);
        return Ok(new SuccessResponse<ReadSupervisorDto>(
            mapper.Map<ReadSupervisorDto>(supervisor),
            $"Supervisor with ID {id} retrieved successfully"));
    }

    [Authorize(Roles = "Admin,HR")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteSupervisor(int id)
    {
        var supervisor = await supervisorServices.GetSupervisor(id);
        await supervisorServices.DeleteSupervisor(id); // Assuming you'll add this method

        return Ok(new SuccessResponse<ReadSupervisorDto>(
            mapper.Map<ReadSupervisorDto>(supervisor),
            $"Supervisor with ID {id} deleted successfully"));
    }

    [Authorize]
    [HttpGet("{id:int}/employees")]
    public async Task<IActionResult> GetSupervisees(int supervisorId, [FromQuery] PaginationParams paginationParams)
    {
        var employees = await supervisorServices.GetAllEmployeesByASupervisor(supervisorId, paginationParams);

        return Ok(new SuccessResponse<List<ReadEmployeeDto>>(
            mapper.Map<List<ReadEmployeeDto>>(employees),
            $"Employees supervised by ID {supervisorId} retrieved successfully"));
    }

    [Authorize]
    [HttpGet("employees/{employeeId:int}/supervisor")]
    public async Task<IActionResult> GetEmployeeSupervisor(int employeeId)
    {
        var supervisor = await supervisorServices.GetEmployeeSupervisor(employeeId);
        return Ok(new SuccessResponse<ReadSupervisorDto>(
            mapper.Map<ReadSupervisorDto>(supervisor),
            $"Supervisor for employee ID {employeeId} retrieved successfully"));
    }
}