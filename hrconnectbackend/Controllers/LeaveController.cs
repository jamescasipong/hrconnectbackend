using hrconnectbackend.Interface.Services;
using hrconnectbackend.Models;
using hrconnectbackend.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace hrconnectbackend.Controllers;

[ApiController]
[Route("api/leave")]
public class LeaveController : ControllerBase
{
    private readonly ILeaveApplicationServices _leaveServices;

    public LeaveController(ILeaveApplicationServices leaveServices)
    {
        _leaveServices = leaveServices;
    }


    [HttpPost]
    public async Task<IActionResult> CreateLeave([FromBody] LeaveApplication leaveRequest)
    {
        if (leaveRequest == null)
            return BadRequest("Invalid leave request.");

        try
        {
            var createdLeave = await _leaveServices.RequestLeave(leaveRequest);
            return Ok(createdLeave);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetAllLeaves()
    {
        try
        {
            var leaves = await _leaveServices.GetAllAsync();
            return Ok(leaves);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetLeaveById(int id)
    {
        try
        {
            var leave = await _leaveServices.GetByIdAsync(id);
            if (leave == null)
                return NotFound(new { error = "Leave application not found." });

            return Ok(leave);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }


    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateLeaveDates(int id, [FromBody] LeaveApplication leaveRequest)
    {
        if (leaveRequest == null)
            return BadRequest("Invalid request.");

        try
        {
            await _leaveServices.UpdateAsync(leaveRequest);
            return Ok(new { message = "Leave dates updated successfully." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteLeave(int leaveId)
    {
        try
        {
            var leave = await _leaveServices.GetByIdAsync(leaveId);

            if (leave == null)
            {
                return NotFound();
            }

            await _leaveServices.DeleteAsync(leave);

            return Ok(new
            {
                message = "Success!",
                status = 200
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    

    [HttpPut("approve/{id}")]
    public async Task<IActionResult> ApproveLeave(int id)
    {
        try
        {
            await _leaveServices.ApproveLeave(id);
            return Ok(new { message = "Leave approved successfully." });
        }
        catch (Exception ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    [HttpPut("reject/{id}")]
    public async Task<IActionResult> RejectLeave(int id)
    {
        try
        {
            await _leaveServices.RejectLeave(id);
            return Ok(new { message = "Leave rejected successfully." });
        }
        catch (Exception ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }


}
