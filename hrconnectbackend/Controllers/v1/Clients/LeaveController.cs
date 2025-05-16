using AutoMapper;
using hrconnectbackend.Constants;
using hrconnectbackend.Exceptions;
using hrconnectbackend.Interface.Services;
using hrconnectbackend.Models;
using hrconnectbackend.Models.DTOs;
using hrconnectbackend.Models.Response;
using hrconnectbackend.Services.ExternalServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace hrconnectbackend.Controllers.v1.Clients;

[Authorize]
[ApiController]
[Route("api/v{version:apiVersion}/leave")]
[ApiVersion("1.0")]
public class LeaveController(
    ILeaveApplicationServices leaveServices,
    ILogger<LeaveController> logger,
    ILeaveBalanceServices leaveBalanceServices,
    IMapper mapper)
    : ControllerBase
{
    [Authorize]
    [HttpPost("applications")]
    public async Task<IActionResult> CreateLeave([FromBody] CreateLeaveApplicationDto leaveRequest)
    {
        if (!ModelState.IsValid)
        {
            return StatusCode(400, new ErrorResponse(ErrorCodes.InvalidRequestModel, "Invalid leave application data."));
        }

        var newLeaveApplication = new LeaveApplication
        {
            EmployeeId = leaveRequest.EmployeeId,
            SupervisorId = leaveRequest.SupervisorId ?? null,
            Type = leaveRequest.Type,
            StartDate = DateOnly.Parse(leaveRequest.StartDate),
            Reason = leaveRequest.Reason,
            AppliedDate = DateOnly.FromDateTime(DateTime.Now)
        };

        await leaveServices.RequestLeave(newLeaveApplication);

        return Ok(new SuccessResponse("Leave application created successfully!"));

    }

    [Authorize(Roles = "Admin,HR")]
    [HttpGet("applications")]
    public async Task<IActionResult> GetAllLeaves([FromQuery] int? pageIndex, [FromQuery] int? pageSize)
    {

        List<LeaveApplication> leaves;

        // Check if pagination parameters are provided
        if (pageIndex == null || pageSize == null)
        {
            // If no pagination parameters, fetch all leaves
            leaves = await leaveServices.GetAllAsync();
        }
        else
        {
            // Validate pageIndex and pageSize
            if (pageIndex <= 0)
            {
                return StatusCode(400, new ErrorResponse(ErrorCodes.InvalidPageNumber, "Page index must be greater than 0"));
            }

            if (pageSize <= 0)
            {
                return StatusCode(400, new ErrorResponse(ErrorCodes.InvalidPageSize, "Page size must be greater than 0"));
            }

            // Fetch all leaves and apply pagination
            leaves = await leaveServices.GetAllAsync();
            leaves = leaves.Skip((pageIndex.Value - 1) * pageSize.Value)
                .Take(pageSize.Value)
                .ToList();
        }

        // If no leaves are found, return an empty list
        if (!leaves.Any())
        {
            return Ok(new SuccessResponse<List<LeaveApplication>?>(new List<LeaveApplication>(), "No leave applications found."));
        }

        // Map the leave data to DTOs
        var leaveDto = mapper.Map<List<ReadLeaveApplicationDto>>(leaves);

        // Return the leave data in the response
        return Ok(new SuccessResponse<List<ReadLeaveApplicationDto>?>(leaveDto, "Leave applications retrieved successfully!"));

    }


    [Authorize(Roles = "Admin,HR")]
    [HttpGet("applications/{id:int}")]
    public async Task<IActionResult> GetLeaveById(int id)
    {
        var leave = await leaveServices.GetByIdAsync(id);

        if (leave == null)
            return NotFound(new ErrorResponse(ErrorCodes.LeaveNotFound, $"Leave application with ID: {id} not found."));

        var leaveDto = mapper.Map<ReadLeaveApplicationDto>(leave);

        return Ok(new SuccessResponse<ReadLeaveApplicationDto?>(leaveDto, $"Leave application with ID: {id} retrieved successfully!"));

    }

    [Authorize(Roles = "Admin,HR")]
    [HttpPut("applications/{id:int}")]
    public async Task<IActionResult> UpdateLeaveDates(int id, [FromBody] UpdateLeaveApplicationDto leaveRequest)
    {

        if (leaveRequest == null) return StatusCode(400, new ErrorResponse(ErrorCodes.InvalidRequestModel, "Invalid leave application data."));


        var leaveApplication = await leaveServices.GetByIdAsync(id);

        if (leaveApplication == null)
        {
            return StatusCode(404, new ErrorResponse(ErrorCodes.LeaveNotFound, $"Leave application with ID: {id} not found."));
        }

        leaveApplication.Type = leaveRequest.Type;
        leaveApplication.Status = leaveRequest.Status;
        leaveApplication.Reason = leaveRequest.Reason;
        leaveApplication.StartDate = leaveApplication.StartDate;

        await leaveServices.UpdateAsync(leaveApplication);

        return Ok(new SuccessResponse($"Leave application with ID: {id} updated successfully!"));
    }

    [Authorize(Roles = "Admin,HR")]
    [HttpDelete("applications/{id:int}")]
    public async Task<IActionResult> DeleteLeave(int id)
    {
        var leave = await leaveServices.GetByIdAsync(id);

        if (leave == null)
        {
            return StatusCode(404, new ErrorResponse(ErrorCodes.LeaveNotFound, $"Leave application with ID: {id} not found."));
        }

        await leaveServices.DeleteAsync(leave);

        return Ok(new SuccessResponse($"Leave application with ID: {id} deleted successfully!"));

    }

    [Authorize(Roles = "Admin,HR")]
    [HttpGet("applications/employee/{employeeId:int}")]
    public async Task<IActionResult> GetLeaveApplicationByEmp(int employeeId, [FromQuery] int? pageIndex, [FromQuery] int? pageSize)
    {

        List<LeaveApplication> employeeLeaveApplication;

        if (pageIndex == null || pageSize == null)
        {
            // Fetch all leave applications for the employee if pagination is not provided
            employeeLeaveApplication = await leaveServices.GetLeaveByEmployee(employeeId);
        }
        else
        {
            // Validate pageIndex and pageSize
            if (pageIndex <= 0)
            {
                return BadRequest(new ErrorResponse(ErrorCodes.InvalidPageNumber, "Page index must be greater than 0"));
            }

            if (pageSize <= 0)
            {
                return BadRequest(new ErrorResponse(ErrorCodes.InvalidPageSize, "Page index must be greater than 0"));
            }

            // Fetch leave applications with pagination
            employeeLeaveApplication = await leaveServices.GetLeaveByEmployee(employeeId);
            employeeLeaveApplication = employeeLeaveApplication
                                        .Skip((pageIndex.Value - 1) * pageSize.Value)
                                        .Take(pageSize.Value)
                                        .ToList();
        }

        // Map the data to the DTO
        var employeeLeaveApplicationDto = mapper.Map<List<ReadLeaveApplicationDto>>(employeeLeaveApplication);

        return Ok(new SuccessResponse<List<ReadLeaveApplicationDto>?>(employeeLeaveApplicationDto, $"Leave applications for employee with ID: {employeeId} retrieved successfully!"));

    }


    [Authorize(Roles = "HR,Admin")]
    [HttpPut("applications/approve/{id:int}")]
    public async Task<IActionResult> ApproveLeave(int id)
    {
        await leaveServices.ApproveLeave(id);

        return Ok(new SuccessResponse($"Leave application with ID: {id} approved successfully!"));

    }
    [Authorize(Roles = "HR,Admin")]
    [HttpPut("applications/reject/{id:int}")]
    public async Task<IActionResult> RejectLeave(int id)
    {
        await leaveServices.RejectLeave(id);

        return Ok(new SuccessResponse($"Leave application with ID: {id} rejected successfully!"));
    }

    [Authorize("Admin,HR")]
    [HttpGet("balances")]
    public async Task<IActionResult> GetAllLeaveBalances([FromQuery] int? pageIndex, [FromQuery] int? pageSize)
    {
        // Fetch all leave balances
        var leaves = await leaveBalanceServices.GetAllAsync();

        // If pagination is requested, apply pagination logic
        if (pageIndex.HasValue && pageSize.HasValue && pageIndex.Value > 0 && pageSize.Value > 0)
        {
            leaves = leaves.Skip((pageIndex.Value - 1) * pageSize.Value)
                .Take(pageSize.Value)
                .ToList();
        }

        // If no leave balances are found
        if (!leaves.Any())
        {
            return Ok(new SuccessResponse<List<LeaveBalance>?>(new List<LeaveBalance>(), "No leave balances found."));
        }

        // Return the leave balances
        return Ok(new SuccessResponse<List<LeaveBalance>?>(leaves, "Leave balances retrieved successfully!"));

    }


    [Authorize(Roles = "Admin,HR")]
    [HttpGet("balances/{employeeId}")]
    public async Task<IActionResult> GetLeaveBalanceByEmployeeId(int employeeId)
    {
        var balances = await leaveBalanceServices.GetLeaveBalanceByEmployeeId(employeeId);

        if (!balances.Any()) return Ok(new SuccessResponse<List<LeaveBalance>?>(new List<LeaveBalance>(), $"No leave balances found for employee with ID: {employeeId}."));

        return Ok(new SuccessResponse<List<LeaveBalance>?>(balances, $"Leave balances for employee with ID: {employeeId} retrieved successfully!"));

    }

    [Authorize]
    [HttpPost("balances")]
    public async Task<ActionResult<LeaveBalance>> AddOrUpdateLeaveBalance([FromBody] LeaveBalance leaveBalance)
    {
        await leaveBalanceServices.AddOrUpdateLeaveBalance(leaveBalance);

        return Ok(new SuccessResponse($"Leave balance for employee with ID: {leaveBalance.EmployeeId} added/updated successfully!"));
    }
}
