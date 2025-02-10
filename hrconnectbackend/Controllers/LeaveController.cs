using AutoMapper;
using hrconnectbackend.Helper;
using hrconnectbackend.Interface.Services;
using hrconnectbackend.Models;
using hrconnectbackend.Models.DTOs;
using hrconnectbackend.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace hrconnectbackend.Controllers;

[ApiController]
[Route("api/leave")]
public class LeaveController : ControllerBase
{
    private readonly ILeaveApplicationServices _leaveServices;
    private readonly ILogger<LeaveController> _logger;
    private readonly IMapper _mapper;
    private readonly ILeaveBalanceServices _leaveBalanceServices;

    public LeaveController(ILeaveApplicationServices leaveServices, ILogger<LeaveController> logger, ILeaveBalanceServices leaveBalanceServices, IMapper mapper)
    {
        _leaveServices = leaveServices;
        _logger = logger;
        _mapper = mapper;
        _leaveBalanceServices = leaveBalanceServices;
    }


    [HttpPost("applications")]
    public async Task<IActionResult> CreateLeave([FromBody] CreateLeaveApplicationDTO leaveRequest)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var newLeaveApplication = new LeaveApplication
            {
                EmployeeId = leaveRequest.EmployeeId,
                SupervisorId = leaveRequest.SupervisorId ?? null,
                Type = leaveRequest.Type,
                StartDate = DateOnly.Parse(leaveRequest.StartDate),
                Reason = leaveRequest.Reason,
                AppliedDate = DateOnly.FromDateTime(DateTime.Now)
            };

            var createdLeave = await _leaveServices.RequestLeave(newLeaveApplication);

            return Ok(new ApiResponse(true, $"Leave application created successfully!"));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ApiResponse(false, ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating an employee.");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("applications")]
    public async Task<IActionResult> GetAllLeaves([FromQuery] int? pageIndex, [FromQuery] int? pageSize)
    {
        var leaves = new List<LeaveApplication>();

        try
        {

            if (pageIndex == null & pageIndex == null)
            {
                leaves = await _leaveServices.GetAllAsync();
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


                leaves = await _leaveServices.GetAllAsync();
                leaves = leaves.Skip((pageIndex.Value - 1) * pageSize.Value)
                                         .Take(pageSize.Value)
                                         .ToList();
            }

            var leaveDTO = _mapper.Map<List<ReadLeaveApplicationDTO>>(leaves);

            if (!leaves.Any()) return Ok(new ApiResponse<List<LeaveApplication>>(true, $"Leave application not found.", leaves));

            return Ok(new ApiResponse<List<ReadLeaveApplicationDTO>>(true, $"Leave application created successfully!", leaveDTO));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving leave applications.");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("applications/{id:int}")]
    public async Task<IActionResult> GetLeaveById(int id)
    {
        try
        {
            var leave = await _leaveServices.GetByIdAsync(id);

            if (leave == null)
                return NotFound(new ApiResponse(false, $"Leave application with ID: {id} not found."));

            var leaveDTO = _mapper.Map<ReadLeaveApplicationDTO>(leave);

            return Ok(new ApiResponse<ReadLeaveApplicationDTO>(true, $"Leave application with ID: {id} retrieved successfully!", leaveDTO));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving a leave application with ID: {id}", id);
            return StatusCode(500, "Internal server error");
        }
    }


    [HttpPut("applications/{id:int}")]
    public async Task<IActionResult> UpdateLeaveDates(int id, [FromBody] UpdateLeaveApplicationDTO leaveRequest)
    {
        if (leaveRequest == null)
            return BadRequest("Body request not found.");

        try
        {
            var leaveApplication = await _leaveServices.GetByIdAsync(id);

            if (leaveApplication == null)
            {
                return NotFound(new ApiResponse(false, $"Leave application with ID: {id} not found."));
            }

            leaveApplication.Type = leaveRequest.Type;
            leaveApplication.Status = leaveRequest.Status;
            leaveApplication.Reason = leaveRequest.Reason;
            leaveApplication.StartDate = leaveApplication.StartDate;

            await _leaveServices.UpdateAsync(leaveApplication);

            return Ok(new ApiResponse(true, $"Leave application with ID: {id} updated successfully!"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating a leave application with ID: {id}", id);
            return StatusCode(500, "Internal server error");
        }
    }    

    [HttpDelete("applications/{id:int}")]
    public async Task<IActionResult> DeleteLeave(int id)
    {
        try
        {
            var leave = await _leaveServices.GetByIdAsync(id);

            if (leave == null)
            {
                return NotFound(new ApiResponse(false, $"Leave application with ID: {id} not found."));
            }

            await _leaveServices.DeleteAsync(leave);

            return Ok(new ApiResponse(true, $"Leave application with ID: {id} deleted successfully!"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting a leave application with ID: {id}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("applications/employee/{employeeId:int}")]
    public async Task<IActionResult> GetLeaveApplicationByEmp(int employeeId, [FromQuery] int? pageIndex, [FromQuery] int? pageSize)
    {
        var employeeLeaveApplication = new List<LeaveApplication>();

        try
        {
            if (pageIndex == null && pageSize == null)
            {
                employeeLeaveApplication = await _leaveServices.GetLeaveByEmployee(employeeId);
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


                employeeLeaveApplication = await _leaveServices.GetLeaveByEmployee(employeeId);
                employeeLeaveApplication = employeeLeaveApplication.Skip((pageIndex.Value - 1) * pageSize.Value).Take(pageSize.Value).ToList();
            }

            var employeeLeaveApplicationDTO = _mapper.Map<List<ReadLeaveApplicationDTO>>(employeeLeaveApplication);

            return Ok(new ApiResponse<List<ReadLeaveApplicationDTO>>(true, $"Leave application by employee with ID: {employeeId} retrieved successfully!", employeeLeaveApplicationDTO));
        }
        catch (KeyNotFoundException ex)
        {
            return Ok(new ApiResponse(false, ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting a leave application with ID: {id}", employeeId);
            return StatusCode(500, "Internal server error");
        }
    }
    

    [HttpPut("applications/approve/{id:int}")]
    public async Task<IActionResult> ApproveLeave(int id)
    {
        try
        {
            await _leaveServices.ApproveLeave(id);

            return Ok(new ApiResponse(true, $"Leave application with ID: {id} approved successfully!"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ApiResponse(false, ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error approving a leave application with ID: {id}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPut("applications/reject/{id:int}")]
    public async Task<IActionResult> RejectLeave(int id)
    {
        try
        {
            await _leaveServices.RejectLeave(id);

            return Ok(new ApiResponse(false, $"Leave application with ID: {id} rejected successfully!"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ApiResponse(false, ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error approving a leave application with ID: {id}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    // Get all leave balances
    [HttpGet("balances")]
    public async Task<IActionResult> GetAllLeaveBalances([FromQuery] int? pageIndex, [FromQuery] int? pageSize)
    {
        try
        {
            var leaves = new List<LeaveBalance>();

            if (pageIndex == null && pageSize == null)
            {
                leaves = await _leaveBalanceServices.GetAllAsync();
            }
            else
            {
                leaves = await _leaveBalanceServices.GetAllAsync();
                leaves = leaves.Skip((pageIndex.Value - 1)  * pageSize.Value).Take(pageSize.Value).ToList();
            }

            if (!leaves.Any())
            {
                return Ok(new ApiResponse<List<LeaveBalance>>(true, $"Leave balances not found.", leaves));
            }

            return Ok(new ApiResponse<List<LeaveBalance>>(success: true, message: $"Leave balances retrieved successfully!", data: leaves));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving leave balances");
            return StatusCode(500, "Internal server error");
        }
    }

    // Get leave balance by employee ID
    [HttpGet("balances/{employeeId}")]
    public async Task<ActionResult<LeaveBalance>> GetLeaveBalanceByEmployeeId(int employeeId)
    {
        try
        {
            var balances = await _leaveBalanceServices.GetLeaveBalanceByEmployeeId(employeeId);

            if (!balances.Any()) return Ok(new ApiResponse<List<LeaveBalance>>(true, $"Leave balances by employee with ID: {employeeId} retrieved successfully!", balances));

            return Ok(new ApiResponse<List<LeaveBalance>>(true, $"Leave balances by employee with ID: {employeeId} retrieved successfully!", balances));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ApiResponse(false, ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving leave balances for employee with ID: {employeeId}", employeeId);
            return StatusCode(500, "Internal server error");
        }
    }

    // Add or update leave balance
    [HttpPost("balances")]
    public async Task<ActionResult<LeaveBalance>> AddOrUpdateLeaveBalance([FromBody] LeaveBalance leaveBalance)
    {
        await _leaveBalanceServices.AddOrUpdateLeaveBalance(leaveBalance);
        return Ok();
    }
}
