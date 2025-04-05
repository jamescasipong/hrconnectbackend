using AutoMapper;
using hrconnectbackend.Interface.Services;
using hrconnectbackend.Interface.Services.Clients;
using Microsoft.AspNetCore.Mvc;

namespace hrconnectbackend.Controllers.v1.Admin;


[ApiController]
[Route("api/v{version:apiVersion}/admin")]
[ApiVersion("1.0")]
public class AdminController(
    ILogger<AdminController> logger,
    IMapper mapper,
    IEmployeeServices employeeServices,
    IAboutEmployeeServices employeeInfoServices,
    IUserAccountServices authServices,
    IAttendanceServices attendanceServices,
    IDepartmentServices departmentServices)
    : Controller
{
    private readonly IMapper _mapper = mapper;
    private readonly ILogger<AdminController> _logger = logger;
    private readonly IEmployeeServices _employeeServices = employeeServices;
    private readonly IAboutEmployeeServices _employeeInfoServices = employeeInfoServices;
    private readonly IUserAccountServices _authServices = authServices;
    private readonly IAttendanceServices _attendanceServices = attendanceServices;
    private readonly IDepartmentServices _departmentServices = departmentServices;

    // Constructor: Initialize the required services
}
