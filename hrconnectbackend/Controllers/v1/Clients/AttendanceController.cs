using System.Security.Claims;
using AutoMapper;
using hrconnectbackend.Constants;
using hrconnectbackend.Exceptions;
using hrconnectbackend.Extensions;
using hrconnectbackend.Interface.Services;
using hrconnectbackend.Interface.Services.Clients;
using hrconnectbackend.Models;
using hrconnectbackend.Models.DTOs;
using hrconnectbackend.Models.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;

namespace hrconnectbackend.Controllers.v1.Clients
{
    [ApiController]
    [Route("api/v{version:apiVersion}/attendance")]
    [ApiVersion("1.0")]
    public class AttendanceController(
        IAttendanceServices attendanceServices,
        IMapper mapper,
        IAttendanceCertificationServices attendanceCertificationServices,
        ILogger<Attendance> logger)
        : Controller
    {

        //[HttpPost]
        //public async Task<IActionResult> CreateAttendance(CreateAttendanceDTO attendanceDTO)
        //{

        //}
        [Authorize]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAttendances()
        {
            try
            {
                var attendances = await attendanceServices.GetAllAsync();

                var mappedAttendances = mapper.Map<List<ReadAttendanceDto>>(attendances);

                if (!attendances.Any()) Ok(new ApiResponse<List<ReadAttendanceDto>?>(true, $"Attendances not found.", mappedAttendances));

                return Ok(new ApiResponse<List<ReadAttendanceDto>?>(true, $"Attendances retrived successfully!", mappedAttendances));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving attendance");
                return StatusCode(500, new ApiResponse(false, "Internal server error"));
            }
        }
        [Authorize]
        [HttpGet("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAttendance(int id)
        {
            try
            {
                var attendance = await attendanceServices.GetByIdAsync(id);

                if (attendance == null)
                {
                    return NotFound(new ApiResponse<object>(false, $"Attendance with an ID: {id} not found."));
                }

                var attendanceDto = mapper.Map<ReadAttendanceDto>(attendance);
                return Ok(new ApiResponse<ReadAttendanceDto?>(true, $"Attendance with ID: {id} retrieved successfully.", attendanceDto));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving attendance with an ID: {id}", id);
                return StatusCode(500, new ApiResponse<object>(false, "Internal server error"));
            }
        }

        [Authorize]
        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateAttendance(int id, UpdateAttendanceDto updateAttendanceDto)
        {
            try
            {
                var attendance = await attendanceServices.GetByIdAsync(id);

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (attendance == null)
                {
                    return NotFound(new ApiResponse(false, $"Attendance with an ID: {id} not found."));
                }

                attendance.ClockIn = TimeSpan.Parse(updateAttendanceDto.ClockIn);
                attendance.ClockOut = TimeSpan.Parse(updateAttendanceDto.ClockOut);
                attendance.DateToday = DateTime.Parse(updateAttendanceDto.DateToday);

                await attendanceServices.UpdateAsync(attendance);

                return Ok(new ApiResponse(true, $"Attendance with id: {id} updated successfully!"));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error deleting attendance with an ID: {id}", id);
                return StatusCode(500, new ApiResponse(false, "Internal server error"));
            }
        }


        [Authorize]
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteAttendance(int id)
        {
            try
            {
                var attendance = await attendanceServices.GetByIdAsync(id);

                if (attendance == null)
                {
                    return NotFound(new ApiResponse(false, $"Attendance with an ID: {id} not found."));
                }

                await attendanceServices.DeleteAsync(attendance);

                return Ok(new ApiResponse(false, $"Attendance with an ID: {id} deleted successfully!"));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error deleting attendance with an ID: {id}", id);
                return StatusCode(500, new ApiResponse(false, "Internal server error"));
            }
        }
        [Authorize]
        [HttpGet("employee/{employeeId:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAttendanceByEmployee(int employeeId, int? pageIndex = 1, int? pageSize = 5)
        {
            try
            {
                var currentUserId = User.FindFirstValue("EmployeeId");

                if (currentUserId == null)
                {
                    return Unauthorized(new ApiResponse(false, "User not authenticated."));
                }

                var attendances = await attendanceServices.GetAttendanceByEmployeeId(employeeId, pageIndex, pageSize);

                var mappedAttendance = mapper.Map<List<ReadAttendanceDto>>(attendances);

                if (!attendances.Any()) return Ok(new ApiResponse<List<ReadAttendanceDto>?>(true, $"Attendances by employee {employeeId} not found.", mappedAttendance));

                return Ok(new ApiResponse<List<ReadAttendanceDto>?>(true, $"Attendances by employee {employeeId} retrieved successfully!", mappedAttendance));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving attendance for employee with an ID: {id}", employeeId);
                return StatusCode(500, new ApiResponse(false, "Internal server error"));
            }
        }

        //[Authorize] // Ensures the user is authenticated
        [HttpPost("clock-in")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ClockIn()
        {
            string currentEmployeeId = User.GetEmployeeSession();

            if (!int.TryParse(currentEmployeeId, out int employeeId))
            {
                return Unauthorized(new ErrorResponse(ErrorCodes.Unauthorized, "Employee session not found."));
            }

            var hasClockedOut = await attendanceServices.HasClockedOut(employeeId);

            if (hasClockedOut)
            {
                return Ok(new ApiResponse(false, $"You have already clocked out"));
            }

            await attendanceServices.ClockIn(employeeId);
            return Ok(new ApiResponse(true, $"Clock-in recorded for employee {employeeId} successfully"));
        }

        [Authorize]
        [HttpGet("clocked-in")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> HasClockedIn()
        {
            var currentUserId = User.FindFirstValue("EmployeeId");

            if (currentUserId == null)
            {
                return Unauthorized(new ApiResponse(false, "User not authenticated."));
            }

            if (!int.TryParse(currentUserId, out int employeeId))
            {
                return Unauthorized(new ApiResponse(false, "Invalid user identifier."));
            }


            var clockedIn = await attendanceServices.HasClockedIn(employeeId);

            return Ok(new ApiResponse<bool>(true, $"{(clockedIn ? "Has Clocked In" : "Not yet clocked In")}", clockedIn));

        }
        [Authorize]
        [HttpPut("clock-out")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ClockOut()
        {
            var currentUserId = User.FindFirstValue("EmployeeId");

            if (currentUserId == null)
            {
                return Unauthorized(new ApiResponse(false, "User not authenticated."));
            }

            if (!int.TryParse(currentUserId, out int employeeId))
            {
                return Unauthorized(new ApiResponse(false, "Invalid user identifier."));
            }

            await attendanceServices.ClockOut(employeeId);

            return Ok(new ApiResponse(true, $"Clock-out recorded for employee {employeeId} successfully"));
        }
        [Authorize]
        [HttpGet("clocked-out")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> HasClockedOut()
        {
            var currentUserId = User.FindFirstValue("EmployeeId");

            if (currentUserId == null)
            {
                return Unauthorized(new ApiResponse(false, "User not authenticated."));
            }

            if (!int.TryParse(currentUserId, out var employeeId))
            {
                return Unauthorized(new ApiResponse(false, "User not authenticated."));
            }

            bool clockedOut = await attendanceServices.HasClockedOut(employeeId);

            return Ok(new ApiResponse<bool>(true, clockedOut ? "Has Clocked Out" : "Not yet clocked out", clockedOut));
        }

        [Authorize(Roles = "Admin,HR")]
        [HttpGet("daily/{employeeId:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetDailyAttendance(int employeeId)
        {
            var currentUserId = User.FindFirstValue("EmployeeId");
            var userRoles = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();

            if (currentUserId == null)
            {
                logger.LogError("User not authenticated.");
                Console.WriteLine("User not authenticated.");
                return StatusCode(401, new ErrorResponse(ErrorCodes.Unauthorized, "User not authenticated. Please login."));
            }

            if (!int.TryParse(currentUserId, out int currentUserIdInt))
            {
                Console.WriteLine("User not authenticated.");
                logger.LogError("Invalid user identifier.");
                return StatusCode(401, new ErrorResponse(ErrorCodes.Unauthorized, "Invalid user identifier."));
            }

            var attendanceToday = await attendanceServices.GetDailyAttendanceByEmployeeId(employeeId);

            if (attendanceToday == null)
            {
                return NotFound(new ErrorResponse(ErrorCodes.AttendanceNotFound, $"Attendance for employee {employeeId} not found."));
            }

            var attendanceMapped = mapper.Map<ReadAttendanceDto>(attendanceToday);

            return Ok(new SuccessResponse<ReadAttendanceDto?>(attendanceMapped, $"Attendance for employee {employeeId} retrieved successfully!"));
        }

        [Authorize]
        [HttpGet("my-attendance-today")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetDailyAttendance()
        {
            var currentUserId = User.FindFirstValue("EmployeeId");

            if (currentUserId == null)
            {
                logger.LogError("User not authenticated.");
                Console.WriteLine("User not authenticated.");
                return StatusCode(401, new ErrorResponse(ErrorCodes.Unauthorized, "User not authenticated. Please login."));
            }

            if (!int.TryParse(currentUserId, out int employeeId))
            {
                Console.WriteLine("User not authenticated.");
                logger.LogError("Invalid user identifier.");
                return StatusCode(401, new ErrorResponse(ErrorCodes.Unauthorized, "Invalid user identifier."));
            }

            var attendanceToday = await attendanceServices.GetDailyAttendanceByEmployeeId(employeeId);
            logger.Log(LogLevel.Information, "Attendance retrieved successfully");

            if (attendanceToday == null)
            {
                return StatusCode(404, new ErrorResponse(ErrorCodes.AttendanceNotFound, $"Attendance for employee {employeeId} not found."));
            }

            var attendanceMapped = mapper.Map<ReadAttendanceDto>(attendanceToday);

            return Ok(new ApiResponse<ReadAttendanceDto?>(true, "Attendance retrieved successfully", attendanceMapped));

        }

        [Authorize(Roles = "Admin,HR")]
        [HttpGet("range/{employeeId:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAttendanceInRange(int employeeId, [FromQuery] string start, [FromQuery] string end, int? pageIndex = 1, int? pageSize = 5)
        {

            var currentUserId = User.FindFirstValue("EmployeeId");
            var userRoles = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();

            if (currentUserId == null)
            {
                return StatusCode(401, new ErrorResponse(ErrorCodes.Unauthorized, "User not authenticated. Please login."));
            }

            if (!int.TryParse(currentUserId, out int currentUserIdInt))
            {
                return StatusCode(401, new ErrorResponse(ErrorCodes.Unauthorized, "Invalid user identifier."));
            }

            var attendanceRecords = await attendanceServices.GetRangeAttendanceByEmployeeId(employeeId, DateTime.Parse(start), DateTime.Parse(end), pageIndex, pageSize);

            return Ok(new ApiResponse<List<Attendance>?>(true, $"Attendance in the range between {start} and {end} retrieved successfully!", attendanceRecords));
        }
        [Authorize]
        [HttpGet("monthly/{employeeId:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetMonthlyAttendance(int id, int? pageIndex = 1, int? pageSize = 5)
        {
            try
            {
                var currentUserId = User.FindFirstValue("EmployeeId");
                var userRoles = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();

                if (currentUserId == null)
                {
                    return StatusCode(401, new ErrorResponse(ErrorCodes.Unauthorized, "User not authenticated. Please login."));
                }

                if (!int.TryParse(currentUserId, out int currentUserIdInt))
                {
                    return StatusCode(401, new ErrorResponse(ErrorCodes.Unauthorized, "Invalid user identifier."));
                }

                bool isAdmin = userRoles.Contains("Admin");

                if (!isAdmin && currentUserIdInt != id)
                {
                    return Forbid(); // 403 Forbidden
                }

                var attendanceRecords = await attendanceServices.GetMonthlyAttendanceByEmployeeId(id, pageIndex, pageSize);
                return Ok(new ApiResponse<List<Attendance>?>(true, "Monthly attendance retrieved successfully!", attendanceRecords));
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
                logger.LogError(ex, "Error retrieving monthly attendance with ID {Id}", id);
                return StatusCode(500, new ApiResponse(false, "Internal server error"));
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("department/{departmentId:int}/attendance-stats/today")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> EmployeeAttendanceStatsByDeptToday(int departmentId)
        {

            var employeeAttendanceStats = await attendanceServices.EmployeeAttendanceStatsByDeptSpecificOrToday(departmentId, null);

            return Ok(new ApiResponse<dynamic?>(true, "Employees Attendance Stats Retrieved", employeeAttendanceStats));
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("department/{departmentId:int}/attendance-stats")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> EmployeeAttendanceStatsByDeptSpecificDate(int departmentId, [FromQuery] string specificDate)
        {

            var employeeAttendanceStats = await attendanceServices.EmployeeAttendanceStatsByDeptSpecificOrToday(departmentId, DateTime.Parse(specificDate));

            return Ok(new ApiResponse<dynamic?>(true, "Employees Attendance Stats Retrieved", employeeAttendanceStats));

        }

        [Authorize(Roles = "Admin")]
        [HttpGet("attendance-stats/today")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> EmployeeAttendanceStatsToday()
        {

            var employeeAttendanceStats = await attendanceServices.EmployeeAttendanceStatsSpecificOrToday(null);

            return Ok(new ApiResponse<dynamic?>(true, "Employees Attendance Stats Retrieved", employeeAttendanceStats));

        }

        [Authorize(Roles = "Admin")]
        [HttpGet("attendance-stats")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> EmployeeAttendanceStatsSpecificOrToday(string specificDate)
        {
            var employeeAttendanceStats = await attendanceServices.EmployeeAttendanceStatsSpecificOrToday(DateTime.Parse(specificDate));

            return Ok(new ApiResponse<dynamic?>(true, "Employees Attendance Stats Retrieved", employeeAttendanceStats));
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("certification")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateCoa(CreateAttendanceCertificationDto attendanceDto)
        {
            var currentUserId = User.FindFirstValue("EmployeeId");

            if (!int.TryParse(currentUserId, out int employeeId))
            {
                return StatusCode(401, new ErrorResponse(ErrorCodes.Unauthorized, "User not authenticated. Please login."));
            }

            if (attendanceDto.EmployeeId != employeeId)
            {
                return StatusCode(403, new ErrorResponse(ErrorCodes.Forbidden, "You are not authorized to create a certification for this employee."));
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse(false, "Employee Id or Date is required."));
            }

            var newAttendance = new AttendanceCertification
            {
                EmployeeId = attendanceDto.EmployeeId,
                SupervisorId = attendanceDto.SupervisorId,
                Date = DateTime.Parse(attendanceDto.Date),
                Status = "Pending",
                ClockIn = TimeSpan.Parse(attendanceDto.ClockIn),
                ClockOut = TimeSpan.Parse(attendanceDto.ClockOut),
                Reason = attendanceDto.Reason ?? "No Log",
                DateCreated = DateTime.Now,
            };

            await attendanceCertificationServices.AddAsync(newAttendance);

            return Ok(new SuccessResponse($"Attendance certification created successfully!"));
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("certification")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAttendanceCertification()
        {
            var certifications = await attendanceCertificationServices.GetAllAsync();

            if (certifications == null || !certifications.Any())
            {
                return NotFound(new ApiResponse<List<AttendanceCertification?>>(false, "No attendance certifications found."));
            }

            return Ok(new SuccessResponse<List<AttendanceCertification?>>(certifications, "Attendance certifications retrieved successfully!"));
        }

        [Authorize]
        [HttpGet("certification/{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAttendanceCertification(int id)
        {
            var certification = await attendanceCertificationServices.GetByIdAsync(id);

            var currentUserId = User.FindFirstValue("EmployeeId");
            var userRoles = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();

            if (currentUserId == null)
            {
                return Unauthorized(new ApiResponse(false, "User not authenticated."));
            }

            if (!int.TryParse(currentUserId, out int currentUserIdInt))
            {
                return Unauthorized(new ApiResponse(false, "Invalid user identifier."));
            }


            if (!ModelState.IsValid)
            {
                return StatusCode(400, new ErrorResponse(ErrorCodes.InvalidRequestModel, "Invalid request. Please check the model."));
            }

            if (certification == null)
            {
                return StatusCode(404, new ErrorResponse(ErrorCodes.AttendanceNotFound, $"Attendance certification with ID {id} not found."));
            }

            return Ok(new SuccessResponse<AttendanceCertification?>(certification, $"Attendance certification with ID {id} retrieved successfully!"));

        }

        [HttpPut("certification/{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateAttendanceCertification(int id, [FromBody] UpdateAttendanceCertificationDto attendanceDto)
        {
            var existingAttendance = await attendanceCertificationServices.GetByIdAsync(id);

            var currentUserId = User.FindFirstValue("EmployeeId");

            if (currentUserId == null)
            {
                return StatusCode(401, new ErrorResponse(ErrorCodes.Unauthorized, "User not authenticated."));
            }

            if (!ModelState.IsValid)
            {
                return StatusCode(400, new ErrorResponse(ErrorCodes.InvalidRequestModel, "Invalid request. Please check the model."));
            }

            if (existingAttendance == null)
            {
                return StatusCode(404, new ErrorResponse(ErrorCodes.AttendanceNotFound, $"Attendance certification with ID {id} not found."));
            }

            existingAttendance.ClockIn = TimeOnly.Parse(attendanceDto.ClockIn).ToTimeSpan();
            existingAttendance.ClockOut = TimeOnly.Parse(attendanceDto.ClockOut).ToTimeSpan();
            existingAttendance.Date = DateTime.Parse(attendanceDto.Date);
            existingAttendance.Reason = attendanceDto.Reason;

            await attendanceCertificationServices.UpdateAsync(existingAttendance);

            return Ok(new ApiResponse(false, $"Attendance certification with ID: {id} updated successfully"));

        }
        [Authorize(Roles = "Admin,HR")]
        [HttpDelete("certification/{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteCertification(int id)
        {
            var certification = await attendanceCertificationServices.GetByIdAsync(id);

            var currentUserId = User.FindFirstValue("EmployeeId");

            if (currentUserId == null)
            {
                return StatusCode(401, new ErrorResponse(ErrorCodes.Unauthorized, "User not authenticated. Please login."));
            }

            if (!int.TryParse(currentUserId, out int currentUserIdInt))
            {
                return StatusCode(401, new ErrorResponse(ErrorCodes.Unauthorized, "Invalid user identifier."));
            }

            if (certification == null)
            {
                return StatusCode(404, new ErrorResponse(ErrorCodes.AttendanceNotFound, $"Attendance certification with ID {id} not found."));
            }

            await attendanceCertificationServices.DeleteAsync(certification);

            return Ok(new SuccessResponse($"Attendance certification with ID: {id} deleted successfully!"));

        }

        [Authorize(Roles = "Admin")]
        [HttpPost("certification/approve/{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ApproveCoa(int id)
        {

            await attendanceCertificationServices.ApproveCertification(id);

            return Ok(new ApiResponse(true, "Attendance certification approved successfully."));

        }

        [Authorize(Roles = "Admin")]
        [HttpPost("certification/reject/{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RejectCoa(int id)
        {

            await attendanceCertificationServices.RejectCertification(id);

            return Ok(new ApiResponse(true, "Attendance certification deleted successfully."));

        }

    }
}
