using AutoMapper;
using hrconnectbackend.Data;
using hrconnectbackend.Interface.Services;
using hrconnectbackend.Models;
using hrconnectbackend.Models.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace hrconnectbackend.Controllers;

[ApiController]
[Route("[controller]")]
public class SupervisorController : Controller
{
    private readonly ISupervisorServices _supervisorServices;
    private readonly ILeaveApplicationServices _leaveApplicationServices;
    private readonly INotificationServices _notificationServices;
    private readonly IEmployeeServices _employeeServices;
    private readonly IMapper _mapper;

    public SupervisorController(ISupervisorServices supervisorServices, ILeaveApplicationServices leaveApplicationServices, INotificationServices notificationServices, IEmployeeServices employeeServices, IMapper mapper)
    {
        _supervisorServices = supervisorServices;
        _leaveApplicationServices = leaveApplicationServices;
        _notificationServices = notificationServices;
        _employeeServices = employeeServices;
        _mapper = mapper;
    }

    [HttpPost("create-supervisor/{id}")]
    public async Task<IActionResult> CreateSupervisor(int id)
    {
        var employee = await _employeeServices.GetByIdAsync(id);

        if (employee == null) return NotFound();

        var supervisor = await _supervisorServices.GetByIdAsync(id);

        if (supervisor != null) return BadRequest("It already exist!");

        var newSupervisor = new Supervisor
        {
            EmployeeId = id
        };

        await _supervisorServices.AddAsync(newSupervisor);

        return Ok(newSupervisor);
    }

    [HttpGet("get/all")]
    public async Task<IActionResult> GetAllSupervisors()
    {
        var supervisors = await _supervisorServices.GetAllAsync();

        if (supervisors.Count == 0)
        {
            return NotFound(new { message = "No supervisors found" });
        }

        return Ok(supervisors);
    }

    [HttpGet("get/{id}")]
    public async Task<IActionResult> GetSupervisor(int id)
    {
        var supervisor = await _supervisorServices.GetByIdAsync(id);

        if (supervisor == null)
        {
            return NotFound(new { message = "Supervisor not found" });
        }

        return Ok(supervisor);
    }

    [HttpPut("leaveapproval/approve/{id}")]
    public async Task<IActionResult> ApproveLeave(int id)
    {
        var leave = await _leaveApplicationServices.GetByIdAsync(id);

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

        return Ok(new { message = "Leave approved" });
    }

    [HttpPut("leaveapproval/reject/{id}")]
    public async Task<IActionResult> RejectLeave(int id)
    {
        var leave = await _leaveApplicationServices.GetByIdAsync(id);

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

        return Ok(new { message = "Leave rejected" });
    }

    [HttpPost("send-notification/{id}")]
    public async Task<IActionResult> SendNotification(int employeeId, [FromBody] CreateNotificationDTO notificationDTO)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (notificationDTO == null)
        {
            return BadRequest(new { message = "Invalid data" });
        }

        if (_employeeServices.GetByIdAsync(employeeId) == null)
        {
            return NotFound(new { message = "Employee not found" });
        }

        var mappedNotification = _mapper.Map<Notifications>(notificationDTO);

        await _notificationServices.AddAsync(mappedNotification);

        return Ok(new { message = "Notification sent" });
    }
}
