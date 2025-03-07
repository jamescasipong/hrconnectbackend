using System.Net;
using Amazon.Auth.AccessControlPolicy;
using AutoMapper;
using hrconnectbackend.Helper;
using hrconnectbackend.Interface.Services;
using hrconnectbackend.Models;
using hrconnectbackend.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace hrconnectbackend.Controllers.v1;


[ApiController]
[Route("api/v{version:apiVersion}/admin")]
[ApiVersion("1.0")]
public class AdminController : Controller
{
    private readonly IMapper _mapper;
    private readonly ILogger<AdminController> _logger;
    private readonly IEmployeeServices _employeeServices;
    private readonly IAboutEmployeeServices _employeeInfoServices;
    private readonly IUserAccountServices _authServices;
    private readonly IAttendanceServices _attendanceServices;
    private readonly IDepartmentServices _departmentServices;
    private readonly ISupervisorServices _supervisorServices;

    // Constructor: Initialize the required services
    public AdminController(ILogger<AdminController> logger, IMapper mapper, IEmployeeServices employeeServices,
        IAboutEmployeeServices employeeInfoServices, IUserAccountServices authServices, IAttendanceServices attendanceServices,
        IDepartmentServices departmentServices, ISupervisorServices supervisorServices)
    {
        _logger = logger;
        _mapper = mapper;
        _employeeServices = employeeServices;
        _employeeInfoServices = employeeInfoServices;
        _authServices = authServices;
        _attendanceServices = attendanceServices;
        _departmentServices = departmentServices;
        _supervisorServices = supervisorServices;
    }

    


}
