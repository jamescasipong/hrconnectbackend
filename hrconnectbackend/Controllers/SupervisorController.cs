using hrconnectbackend.Data;
using hrconnectbackend.IRepositories;
using Microsoft.AspNetCore.Mvc;

namespace hrconnectbackend.Controllers;


[ApiController]
[Route("[controller]")]
public class SupervisorController : Controller
{

    private readonly ISupervisorRepositories _supervisorRepository;
    private readonly ILeaveApprovalRepositories _leaveApprovalRepository;
    private readonly ILeaveApplicationRepositories _leaveApplicationRepository;
    public SupervisorController(ISupervisorRepositories supervisorRepository, ILeaveApprovalRepositories leaveApprovalRepository, ILeaveApplicationRepositories leaveApplicationRepository)
    {
        _supervisorRepository = supervisorRepository;
        _leaveApprovalRepository = leaveApprovalRepository;
        _leaveApplicationRepository = leaveApplicationRepository;
    }


    [HttpGet("get/all")]
    public async Task<IActionResult> GetAllSupervisors()
    {
        var supervisors = await _supervisorRepository.GetAllAsync();

        if (supervisors.Count == 0)
        {
            return NotFound(new { message = "No supervisors found" });
        }

        return Ok(supervisors);
    }

    [HttpGet("get/{id}")]
    public async Task<IActionResult> GetSupervisor(int id)
    {
        var supervisor = await _supervisorRepository.GetByIdAsync(id);

        if (supervisor == null)
        {
            return NotFound(new { message = "Supervisor not found" });
        }

        return Ok(supervisor);
    }


    [HttpPut("leaveapproval/approve/{id}")]
    public async Task<IActionResult> ApproveLeave(int id)
    {
        var leave = await _leaveApplicationRepository.GetByIdAsync(id);

        if (leave == null)
        {
            return NotFound(new { message = "Leave not found" });
        }

        if (leave.Status == "Approved")
        {
            return BadRequest(new { message = "Leave already approved" });
        }

        if (leave.Status == "Rejected")
        {
            return BadRequest(new { message = "Leave already rejected" });
        }

        await _leaveApprovalRepository.ReponseLeave(id, "Approved");

        return Ok(new { message = "Leave approved" });
    }

    [HttpPut("leaveapproval/reject/{id}")]
    public async Task<IActionResult> RejectLeave(int id)
    {
        var leave = await _leaveApplicationRepository.GetByIdAsync(id);

        if (leave == null)
        {
            return NotFound(new { message = "Leave not found" });
        }

        if (leave.Status == "Approved")
        {
            return BadRequest(new { message = "Leave already approved" });
        }

        if (leave.Status == "Rejected")
        {
            return BadRequest(new { message = "Leave already rejected" });
        }

        await _leaveApprovalRepository.ReponseLeave(id, "Rejected");

        return Ok(new { message = "Leave rejected" });
    }

}