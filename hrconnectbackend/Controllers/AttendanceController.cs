using AutoMapper;
using hrconnectbackend.Helper;
using hrconnectbackend.Helper.CustomExceptions;
using hrconnectbackend.Interface.Services;
using hrconnectbackend.Models;
using hrconnectbackend.Models.DTOs;
using hrconnectbackend.Models.DTOs.AttendanceDTOs;
using hrconnectbackend.Repositories;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;

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
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAttendances()
        {
            try
            {
                var attendances = await _attendanceServices.GetAllAsync();

                var mappedAttendances = _mapper.Map<List<ReadAttendanceDTO>>(attendances);

                if (!attendances.Any()) Ok(new ApiResponse<List<ReadAttendanceDTO>>(true, $"Attendances not found.", mappedAttendances));

                return Ok(new ApiResponse<List<ReadAttendanceDTO>>(true, $"Attendances retrived successfully!", mappedAttendances));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving attendance");
                return StatusCode(500, new ApiResponse(false, "Internal server error"));
            }
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAttendance(int id)
        {
            try
            {
                var attendance = await _attendanceServices.GetByIdAsync(id);

                if (attendance == null) return NotFound(new ApiResponse(false, $"Attendance with an ID: {id} not found."));

                return Ok(attendance);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting attendance with an ID: {id}", id);
                return StatusCode(500, new ApiResponse(false, "Internal server error"));
            }
        }

        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateAttendance(int id, UpdateAttendanceDTO updateAttendanceDTO)
        {
            try
            {
                var attendance = await _attendanceServices.GetByIdAsync(id);

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (attendance == null)
                {
                    return NotFound(new ApiResponse(false, $"Attendance with an ID: {id} not found."));
                }

                attendance.ClockIn = TimeSpan.Parse(updateAttendanceDTO.ClockIn);
                attendance.ClockOut = TimeSpan.Parse(updateAttendanceDTO.ClockOut);
                attendance.DateToday = DateTime.Parse(updateAttendanceDTO.DateToday);

                await _attendanceServices.UpdateAsync(attendance);

                return Ok(new ApiResponse(true, $"Attendance with id: {id} updated successfully!"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponse(false, $"Attendance with an ID: {id} not found."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting attendance with an ID: {id}", id);
                return StatusCode(500, new ApiResponse(false, "Internal server error"));
            }
        }

        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteAttendance(int id)
        {
            try
            {
                var attendance = await _attendanceServices.GetByIdAsync(id);

                if (attendance == null)
                {
                    return NotFound(new ApiResponse(false, $"Attendance with an ID: {id} not found."));
                }

                await _attendanceServices.DeleteAsync(attendance);

                return Ok(new ApiResponse(false, $"Attendance with an ID: {id} deleted successfully!"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting attendance with an ID: {id}", id);
                return StatusCode(500, new ApiResponse(false, "Internal server error"));
            }
        }

        [HttpGet("employee/{employeeId:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAttendanceByEmployee(int employeeId)
        {
            try
            {
                var attendances = await _attendanceServices.GetAttendanceByEmployeeId(employeeId);

                var mappedAttendance = _mapper.Map<List<ReadAttendanceDTO>>(attendances);

                if (!attendances.Any()) return Ok(new ApiResponse<List<ReadAttendanceDTO>>(true, $"Attendances by employee {employeeId} not found.", mappedAttendance));

                return Ok(new ApiResponse<List<ReadAttendanceDTO>>(true, $"Attendances by employee {employeeId} retrieved successfully!", mappedAttendance));
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error retrieving attendance for employee with an ID: {id}", employeeId);
                return StatusCode(500, new ApiResponse(false, "Internal server error"));
            }
        }

        [HttpPost("clock-in/{employeeId:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> ClockIn(int employeeId)
        {
            try
            {
                await _attendanceServices.ClockIn(employeeId);

                return Ok(new ApiResponse(true, $"Clock-in recorded for employee {employeeId} successfully"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ApiResponse(false, ex.Message));
            }
            catch (ConflictException ex)
            {
                return Conflict(new ApiResponse(false, ex.Message));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponse(false, $"Clock-out record for employee {employeeId} not found."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing clock-in for employee {EmployeeId}", employeeId);
                return StatusCode(500, new ApiResponse(false, "Internal server error"));
            }
        }

        [HttpGet("clocked-in/{employeeId:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> HasClockedIn(int employeeId)
        {
            try
            {
                var clockedOut =await _attendanceServices.HasClockedOut(employeeId);

                return Ok(new ApiResponse<bool>(true, $"Has already clocked in.", clockedOut));
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error checking's employee attendance {EmployeeId}", employeeId);
                return StatusCode(500, new ApiResponse(false, "Internal server error"));
            }
        }

        [HttpPost("clock-out/{employeeId:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ClockOut(int employeeId)
        {
            try
            {
                await _attendanceServices.ClockOut(employeeId);

                return Ok(new ApiResponse(true, $"Clock-out recorded for employee {employeeId} successfully"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponse(false,ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing clock-out for employee {EmployeeId}", employeeId);
                return StatusCode(500, new ApiResponse(false, "Internal server error"));
            }
        }

        [HttpGet("clocked-out/{employeeId:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> HasClockedOut(int employeeId)
        {
            try
            {
                bool clockedOut = await _attendanceServices.HasClockedOut(employeeId);

                return Ok(new ApiResponse<bool>(true, $"Has already clocked in.", clockedOut));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking's employee attendance {EmployeeId}", employeeId);
                return StatusCode(500, new ApiResponse(false, "Internal server error"));
            }
        }


        [HttpGet("daily/{employeeId:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetDailyAttendance(int employeeId)
        {
            try
            {
                var attendanceToday = await _attendanceServices.GetDailyAttendanceByEmployeeId(employeeId);

                return Ok(attendanceToday);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponse(false, $"Daily attendance for employee {employeeId} not found."));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResponse(false, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving daily attendance for employee ID: {id}", employeeId);
                return StatusCode(500, new { error = "An internal error occurred" });
            }
        }


        [HttpGet("range/{employeeId:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAttendanceInRange(int employeeId, [FromQuery] string start, [FromQuery] string end)
        {
            try
            {
                var attendanceRecords = await _attendanceServices.GetRangeAttendanceByEmployeeId(employeeId, DateTime.Parse(start), DateTime.Parse(end));
                return Ok(new ApiResponse<List<Attendance>>(true, $"Attendance in the range between {start} and {end} retrieved successfully!", attendanceRecords));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResponse(false, ex.Message));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponse(false, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving in the range between {start} and {end} for employee ID: {employeeId}");
                return StatusCode(500, new { error = "An internal error occurred" });
            }
        }

        [HttpGet("monthly/{employeeId:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetMonthlyAttendance(int id)
        {
            try
            {
                var attendanceRecords = await _attendanceServices.GetMonthlyAttendanceByEmployeeId(id);
                return Ok(new ApiResponse<List<Attendance>>(true, "Monthly attendance retrieved successfully!", attendanceRecords));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResponse(false, ex.Message));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponse(false, $"Monthly attendance with ID {id} not found"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving monthly attendance with ID {Id}", id);
                return StatusCode(500, new ApiResponse(false, "Internal server error"));
            }
        }

        [HttpGet("department/{departmentId:int}/attendance-stats/today")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> EmployeeAttendanceStatsByDeptToday(int departmentId)
        {
            try
            {

                var employeeAttendanceStats = await _attendanceServices.EmployeeAttendanceStatsByDeptSpecificOrToday(departmentId, null);

                return Ok(new ApiResponse<dynamic>(true, "Employees Attendance Stats Retrieved", employeeAttendanceStats));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving attendance statistics of employees of today from department ID: {departmentId}", departmentId);
                return StatusCode(500, new ApiResponse(false, "Internal server error"));
            }
        }

        [HttpGet("department/{departmentId:int}/attendance-stats")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> EmployeeAttendanceStatsByDeptSpecificDate(int departmentId, [FromQuery] string specificDate)
        {
            try
            {

                var employeeAttendanceStats = await _attendanceServices.EmployeeAttendanceStatsByDeptSpecificOrToday(departmentId, DateTime.Parse(specificDate));

                return Ok(new ApiResponse<dynamic>(true, "Employees Attendance Stats Retrieved", employeeAttendanceStats));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving attendance statistics of employees of {specificDate} from department ID: {departmentId}");
                return StatusCode(500, new ApiResponse(false, "Internal server error"));
            }
        }

        [HttpGet("attendance-stats/today")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> EmployeeAttendanceStatsToday()
        {
            try
            {
                var employeeAttendanceStats = await _attendanceServices.EmployeeAttendanceStatsSpecificOrToday(null);

                return Ok(new ApiResponse<dynamic>(true, "Employees Attendance Stats Retrieved", employeeAttendanceStats));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving attendance statistics of employees of today");
                return StatusCode(500, new ApiResponse(false, "Internal server error"));
            }
        }

        [HttpGet("attendance-stats")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> EmployeeAttendanceStatsSpecificOrToday(string specificDate)
        {
            var employeeAttendanceStats = await _attendanceServices.EmployeeAttendanceStatsSpecificOrToday(DateTime.Parse(specificDate));

            return Ok(new ApiResponse<dynamic>(true, "Employees Attendance Stats Retrieved", employeeAttendanceStats));
        }

        [HttpPost("certification")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateCOA(CreateAttendanceCertificationDTO attendanceDTO)
        {

            var supervisor = await _supervisorServices.GetByIdAsync(attendanceDTO.SupervisorId);


            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse(false, "Employee Id or Date is required."));
            }

            var newAttendance = new AttendanceCertification
            {
                EmployeeId = attendanceDTO.EmployeeId,
                SupervisorId = attendanceDTO.SupervisorId,
                Date = DateTime.Parse(attendanceDTO.Date),
                Status = "Pending",
                ClockIn = TimeSpan.Parse(attendanceDTO.ClockIn),
                ClockOut = TimeSpan.Parse(attendanceDTO.ClockOut),
                Reason = attendanceDTO.Reason ?? "No Log",
                DateCreated = DateTime.Now,
            };

            await _attendanceCertificationServices.AddAsync(newAttendance);

            return Ok(new ApiResponse(true, $"Attendance certifications created sucessfully!"));
        }

        [HttpGet("certification")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAttendanceCertification()
        {
            var certifications = await _attendanceCertificationServices.GetAllAsync();

            return Ok(new ApiResponse<List<AttendanceCertification>>(true, $"Attendance certifications retrieved sucessfully!", certifications));
        }

        [HttpGet("certification/{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAttendanceCertification(int id)
        {
            var certification = await _attendanceCertificationServices.GetByIdAsync(id);

            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse(false, ModelState.ToString()));
                }

                if (certification == null)
                {
                    return NotFound(new ApiResponse(false, $"Attendance certification with ID {id} not found"));
                }

                return Ok(new ApiResponse<AttendanceCertification>(true, $"Attendance certification retrieved sucessfully!", certification));
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving attendance with ID {Id}", id);
                return StatusCode(500, new ApiResponse(false, "Internal server error"));
            }
        }

        [HttpPut("certification/{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateAttendanceCertification(int id, [FromBody] UpdateAttendanceCertificationDTO attendanceDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse(false, ModelState.IsValid.ToJson().ToString()));
            }
            try
            {
                var existingAttendance = await _attendanceCertificationServices.GetByIdAsync(id);
                if (existingAttendance == null)
                {
                    return NotFound(new ApiResponse(false, $"Attendance certification with ID {id} not found"));
                }

                existingAttendance.ClockIn = TimeOnly.Parse(attendanceDTO.ClockIn).ToTimeSpan();
                existingAttendance.ClockOut = TimeOnly.Parse(attendanceDTO.ClockOut).ToTimeSpan();
                existingAttendance.Date = DateTime.Parse(attendanceDTO.Date);
                existingAttendance.Reason = attendanceDTO.Reason;

                await _attendanceCertificationServices.UpdateAsync(existingAttendance);

                return Ok(new ApiResponse(false, $"Attendance certification with ID: {id} updated successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating attendance with ID {Id}", id);
                return StatusCode(500, new ApiResponse(false, "Internal server error"));
            }
        }

        [HttpDelete("certification/{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteCertification(int id)
        {
            var certification = await _attendanceCertificationServices.GetByIdAsync(id);

            try
            {
                if (certification == null)
                {
                    return NotFound("No certification found");
                }

                await _attendanceCertificationServices.DeleteAsync(certification);

                return Ok(new ApiResponse
                (
                    true, "Attendance certification deleted succcessfully."
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving attendance with ID {Id}", id);
                return StatusCode(500, new ApiResponse(false, "Internal server error"));
            }
        }


        [HttpPost("certification/approve/{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ApproveCOA(int id)
        {

            try
            {
                await _attendanceCertificationServices.ApproveCertification(id);

                return Ok(new ApiResponse(true, "Attendance certification approved successfully."));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponse(false, ex.Message));
            }
            catch (Exception ex) 
            {
                _logger.LogError(ex, "Error approving a certification with ID {id}", id);
                return StatusCode(500, "An internal server error occurred.");
            }
        }


        [HttpPost("certification/reject/{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RejectCOA(int id)
        {

            try
            {
                await _attendanceCertificationServices.RejectCertification(id);

                return Ok(new ApiResponse(true, "Attendance certification deleted successfully."));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponse(false, ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting attendance certification.");
                return StatusCode(500, "An internal server error occurred.");
            }
        }
    }
}
