using AutoMapper;
using hrconnectbackend.Interface.Services;
using hrconnectbackend.Models;
using hrconnectbackend.Models.DTOs;
using hrconnectbackend.Models.DTOs.AttendanceDTOs;
using hrconnectbackend.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace hrconnectbackend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AttendanceController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IAttendanceServices _attendanceServices;
        private readonly IAttendanceCertificationServices _attendanceCertificationServices;
        private readonly ISupervisorServices _supervisorServices;
        private readonly IShiftServices _shiftServices;
        private readonly IEmployeeServices _employeeServices;
        private readonly ILogger<Attendance> _logger;

        public AttendanceController(IAttendanceServices attendanceServices, IMapper mapper, IEmployeeServices employeeServices, IShiftServices shiftServices, IAttendanceCertificationServices attendanceCertificationServices, ISupervisorServices supervisorServices, ILogger<Attendance> logger)
        {
            _attendanceServices = attendanceServices;
            _mapper = mapper;
            _employeeServices = employeeServices;
            _shiftServices = shiftServices;
            _logger = logger;
            _attendanceCertificationServices = attendanceCertificationServices;
            _supervisorServices = supervisorServices;
        }

        //[HttpPost]
        //public async Task<IActionResult> CreateAttendance(CreateAttendanceDTO attendanceDTO)
        //{

        //}

        [HttpGet]
        public async Task<IActionResult> GetAttendances()
        {
            try
            {
                var attendances = await _attendanceServices.GetAllAsync();

                return Ok(attendances);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAttendance(int id)
        {
            try
            {
                var attendance = await _attendanceServices.GetByIdAsync(id);

                if (attendance == null) throw new KeyNotFoundException("No attendance found");

                return Ok(attendance);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest();
            }
        }

        [HttpPut]
        public async Task<IActionResult> UpdateAttendance(UpdateAttendanceDTO updateAttendanceDTO)
        {
            try
            {
                var attendance = await _attendanceServices.GetByIdAsync(updateAttendanceDTO.attendanceId);

                if (attendance == null)
                {
                    throw new KeyNotFoundException("No attendance found");
                }

                attendance.ClockIn = TimeSpan.Parse(updateAttendanceDTO.ClockIn);
                attendance.ClockOut = TimeSpan.Parse(updateAttendanceDTO.ClockOut);
                attendance.DateToday = DateOnly.Parse(updateAttendanceDTO.DateToday);

                await _attendanceServices.UpdateAsync(attendance);

                return Ok(new
                {
                    message = "Success!",
                    status = 200
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAttendance(int id)
        {
            try
            {
                var attendance = await _attendanceServices.GetByIdAsync(id);

                if (attendance == null)
                {
                    throw new KeyNotFoundException("No attendance found");
                }

                await _attendanceServices.DeleteAsync(attendance);

                return Ok(new
                {
                    message = "Success!",
                    status = 200
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest();
            }
        }

        [HttpGet("employee/{id}")]
        public async Task<IActionResult> GetAttendanceByEmployee(int id)
        {
            var attendances = await _attendanceServices.GetAttendanceByEmployeeId(id);

            return Ok(attendances);
        }

        [HttpPost("clock-in/{id}")]
        public async Task<IActionResult> ClockIn(int id)
        {
            try
            {
                await _attendanceServices.ClockIn(id);

                return Ok(new
                {
                    message = "Success!",
                    status = 200,
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest();
            }
        }

        [HttpPost("clock-out/{id}")]
        public async Task<IActionResult> ClockOut(int id)
        {
            try
            {
                await _attendanceServices.ClockOut(id);

                return Ok(new
                {
                    message = "Success!",
                    status = 200,
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest();
            }
        }


        [HttpGet("daily/{id}")]
        public async Task<IActionResult> GetDailyAttendance(int id)
        {
            try
            {
                var attendanceToday = await _attendanceServices.GetDailyAttendanceByEmployeeId(id);
                return Ok(attendanceToday);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new
                {
                    error = ex.Message,
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new
                {
                    error = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving attendance for employee ID: {id}", id);
                return StatusCode(500, new { error = "An internal error occurred" });
            }
        }


        [HttpGet("range-date/{id}")]
        public async Task<IActionResult> GetAttendanceInRange(int id, [FromQuery] DateTime start, [FromQuery] DateTime end)
        {
            try
            {
                var attendanceRecords = await _attendanceServices.GetRangeAttendanceByEmployeeId(id, start, end);
                return Ok(attendanceRecords);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid request parameters for employee ID: {id}", id);
                return BadRequest(new { error = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "No attendance found for employee ID: {id} in the given range", id);
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving attendance for employee ID: {id}", id);
                return StatusCode(500, new { error = "An internal error occurred" });
            }
        }

        [HttpGet("monthly/{id}")]
        public async Task<IActionResult> GetMonthlyAttendance(int id)
        {
            try
            {
                var attendanceRecords = await _attendanceServices.GetMonthlyAttendanceByEmployeeId(id);
                return Ok(attendanceRecords);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid employee ID: {id}", id);
                return BadRequest(new { error = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "No attendance found for employee ID: {id}", id);
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving attendance for employee ID: {id}", id);
                return StatusCode(500, new { error = "An internal error occurred" });
            }
        }

        [HttpPost("certification")]
        public async Task<IActionResult> CreateCOA(CreateAttendanceCertificationDTO attendanceDTO)
        {

            var supervisor = await _supervisorServices.GetByIdAsync(attendanceDTO.SupervisorId);

            if (supervisor == null)
            {
                return BadRequest("Supervisor not found");
            }

            var newAttendance = new AttendanceCertification
            {
                EmployeeId = attendanceDTO.EmployeeId,
                SupervisorId = attendanceDTO.SupervisorId,
                Date = DateOnly.Parse(attendanceDTO.Date),
                Status = "Pending",
                ClockIn = TimeSpan.Parse(attendanceDTO.ClockIn),
                ClockOut = TimeSpan.Parse(attendanceDTO.ClockOut),
                Reason = attendanceDTO.Reason ?? "No Log",
                DateCreated = DateTime.Now,
            };

            await _attendanceCertificationServices.AddAsync(newAttendance);

            return Ok(new
            {
                message = "Success!",
                status = 200
            });
        }

        [HttpPost("certification/approve/{id}")]
        public async Task<IActionResult> ApproveCOA(int id)
        {

            var coa = await _attendanceCertificationServices.GetByIdAsync(id);

            if (coa == null) return NotFound();

            coa.Status = "Approved";

            await _attendanceCertificationServices.UpdateAsync(coa);

            var attendances = await _attendanceServices.GetAllAsync();

            var attendance = attendances.Where(a => a.EmployeeId == coa.EmployeeId && a.DateToday == coa.Date).FirstOrDefault();

            if (attendance == null)
            {
                var newAttendance = new Attendance
                {
                    EmployeeId = coa.EmployeeId,
                    ClockIn = coa.ClockIn,
                    ClockOut = coa.ClockOut,
                    DateToday = coa.Date,
                };

                await _attendanceServices.AddAsync(newAttendance);
            }
            else
            {
                attendance.ClockIn = coa.ClockIn;
                attendance.ClockOut = coa.ClockOut;

                await _attendanceServices.UpdateAsync(attendance);
            }

            return Ok(new
            {
                message = "Success!",
                status = 200
            });
        }


        [HttpPost("certification/reject/{id}")]
        public async Task<IActionResult> RejectCOA(int id)
        {

            var coa = await _attendanceCertificationServices.GetByIdAsync(id);

            if (coa == null) return NotFound();

            coa.Status = "Reject";

            await _attendanceCertificationServices.UpdateAsync(coa);

            var attendances = await _attendanceServices.GetAllAsync();

            var attendance = attendances.Where(a => a.EmployeeId == coa.EmployeeId && a.DateToday == coa.Date).FirstOrDefault();

            if (attendance == null)
            {
                var newAttendance = new Attendance
                {
                    EmployeeId = coa.EmployeeId,
                    ClockIn = coa.ClockIn,
                    ClockOut = coa.ClockOut,
                    DateToday = coa.Date,
                };

                await _attendanceServices.AddAsync(newAttendance);
            }
            else
            {
                attendance.ClockIn = coa.ClockIn;
                attendance.ClockOut = coa.ClockOut;

                await _attendanceServices.UpdateAsync(attendance);
            }

            return Ok(new
            {
                message = "Success!",
                status = 200
            });
        }
    }
}
