using AutoMapper;
using hrconnectbackend.Data;
using hrconnectbackend.Helper;
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

    [HttpGet]
    public async Task<IActionResult> GetAllSupervisors()
    {
        try
        {
            var supervisors = await _supervisorServices.GetAllAsync();

            var mappedSupervisors = _mapper.Map<List<ReadSupervisorDTO>>(supervisors);

            if (!supervisors.Any())
            {
                return Ok(new ApiResponse<List<ReadSupervisorDTO>>(false, $"Supervisors not found.", mappedSupervisors));
            }

            return Ok(new ApiResponse<List<ReadSupervisorDTO>>(true, $"Supervisors retreved successfully!", mappedSupervisors));
        }
        catch (Exception)
        {
            return StatusCode(500, new ApiResponse(false, $"Internal Server Error"));
        }
    }

    [HttpGet("{supervisorId:int}")]
    public async Task<IActionResult> GetSupervisor(int supervisorId)
    {
        try
        {
            var supervisor = await _supervisorServices.GetByIdAsync(supervisorId);

            if (supervisor == null)
            {
                return NotFound(new ApiResponse(false, $"Supervisor with id: {supervisorId} not found."));
            }

            var mapped = _mapper.Map<ReadSupervisorDTO>(supervisor);

            return Ok(new ApiResponse<ReadSupervisorDTO>(true, $"Supervisor with id: {supervisorId} retrieved successfully!", mapped));
        }
        catch (Exception)
        {
            return StatusCode(500, new ApiResponse(false, $"Internal Server Error"));
        }
    } 


}
