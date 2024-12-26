using Microsoft.AspNetCore.Mvc;
using hrconnectbackend.Models;
using hrconnectbackend.IRepositories;
using AutoMapper;
using hrconnectbackend.Models.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;
using hrconnectbackend;
using hrconnectbackend.Helper;
using hrconnectbackend.Repositories;
using hrconnectbackend.Models.DTOs.EmployeeDTOs;

namespace hrconnectbackend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class EmployeeController : ControllerBase
    {
        private readonly IEmployeeRepositories _employeeRepository;
        private readonly IEmployeeInfoRepositories _employeeInfoRepository;
        private readonly AuthRepositories _authRepository;
        private readonly SupervisorRepositories _supervisorRepository;
        private readonly IShiftRepositories _shiftRepository;
        private readonly IDepartmentRepositories _departmentRepository;
        private readonly ILeaveApplicationRepositories _leaveApplicationRepository;
        private readonly ILeaveApprovalRepositories _leaveApprovalRepository;
        private readonly IAttendanceRepositories _attendanceRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<EmployeeController> _logger;

        public EmployeeController
        (IEmployeeRepositories employeeRepository,
        IMapper mapper,
        IEmployeeInfoRepositories employeeInfoRepository,
        IDepartmentRepositories departmentRepository,
        ILogger<EmployeeController> logger,
        AuthRepositories authRepository,
        SupervisorRepositories supervisorRepository,
        ILeaveApplicationRepositories leaveApplicationRepository,
        ILeaveApprovalRepositories leaveApprovalRepository,
        IAttendanceRepositories attendanceRepository,
        IShiftRepositories shiftRepository)
        {
            _attendanceRepository = attendanceRepository;
            _employeeInfoRepository = employeeInfoRepository;
            _supervisorRepository = supervisorRepository;
            _authRepository = authRepository;
            _employeeRepository = employeeRepository;
            _mapper = mapper;
            _departmentRepository = departmentRepository;
            _logger = logger;
            _leaveApplicationRepository = leaveApplicationRepository;
            _leaveApprovalRepository = leaveApprovalRepository;
            _shiftRepository = shiftRepository;
        }

        [HttpPost("create-leave/{id}")]
        public async Task<IActionResult> CreateLeave(int id, [FromBody] CreateLeaveApplicationDTO leaveDTO)
        {
            try
            {
                if (leaveDTO == null)
                {
                    return BadRequest(new { message = "Invalid leave data" });
                }

                var employee = await _employeeRepository.GetEmployeeByIdAsync(id);

                if (employee == null)
                {
                    return NotFound(new { message = "Employee not found" });
                }

                if (employee.SupervisorId == null)
                {
                    return BadRequest(new { message = "Employee does not have a supervisor" });
                }


                var leaveEntity = new LeaveApplication
                {
                    EmployeeId = id,
                    Type = leaveDTO.Type,
                    Status = "Pending",
                    StartDate = DateOnly.Parse(leaveDTO.StartDate),
                    EndDate = DateOnly.Parse(leaveDTO.EndDate),
                    AppliedDate = DateOnly.FromDateTime(DateTime.Now),
                    Reason = leaveDTO.Reason,
                };

                await _leaveApplicationRepository.AddAsync(leaveEntity);


                var leaveApproval = new LeaveApproval
                {
                    LeaveApplicationId = leaveEntity.LeaveApplicationId,
                    SupervisorId = employee.SupervisorId.Value,
                    Decision = "Pending",
                    ApprovedDate = null,
                };


                await _leaveApprovalRepository.AddAsync(leaveApproval);


                return Ok(new { message = "Leave application created successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }


        [HttpGet("get-supervisor/{id}")]
        public async Task<IActionResult> GetSupervisor(int id)
        {
            var supervisor = await _employeeRepository.GetSupervisor(id);

            if (supervisor == null) return NotFound(new { response = "This user doesn't have a superisor" });

            var supervisorDTO = _mapper.Map<ReadEmployeeDTO>(supervisor);

            return Ok(supervisorDTO);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetEmployee(int id)
        {
            var employee = await _employeeRepository.GetEmployeeByIdAsync(id);

            if (employee == null)
            {
                return NotFound(new { message = "Employee not found" });
            }

            var educationBackground = await _employeeInfoRepository.GetEmployeeEducationBackgroundAsync(id);

            var employeeInfo = await _employeeInfoRepository.GetEmployeeInfoByIdAsync(id);

            var employeeInfoDTO = new EmployeeInfoDTO
            {
                EmployeeInfoId = employeeInfo.EmployeeInfoId,
                FirstName = employeeInfo.FirstName,
                LastName = employeeInfo.LastName,
                Address = employeeInfo.Address,
                BirthDate = employeeInfo.BirthDate ?? default(DateOnly),
                Age = employeeInfo?.Age ?? 0,
                EducationalBackground = educationBackground
            };


            var employeeDTO = _mapper.Map<ReadEmployeeDTO>(employee);

            employeeDTO.employeeInfo = employeeInfoDTO;



            return Ok(employeeDTO);
        }


        [HttpPost("create/clockin/{id}")]
        public async Task<IActionResult> clockIn(int id)
        {
            var hasShift = await _shiftRepository.HasShiftToday(id);

            if (!hasShift) return BadRequest("Employee has no shift today!");

            var result = await _attendanceRepository.ClockIn(id);

            return Ok(result);
        }

        [HttpPost("create/clockout/{id}")]
        public async Task<IActionResult> clockOut(int id)
        {
            var result = await _attendanceRepository.ClockOut(id);

            return Ok(result);
        }


        [HttpGet("department/{id}")]
        public async Task<IActionResult> GetDepartment(int id)
        {
            var department = await _departmentRepository.GetByIdAsync(id);

            if (department == null)
            {
                return NotFound(new { message = "Department not found" });
            }

            return Ok(department);
        }


        [HttpGet("subordinates/{id}")]
        public async Task<IActionResult> GetSubordinates(int id)
        {
            var subordinates = await _supervisorRepository.GetSuperVisorSubordinates(id);

            if (subordinates == null || subordinates.Count() == 0) return NotFound(new { response = "This supervisor has no subordinates" });

            var subordinatesDTO = _mapper.Map<List<ReadEmployeeDTO>>(subordinates);

            return Ok(subordinatesDTO);

        }

    }
}
