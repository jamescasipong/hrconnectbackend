using System.Text.Json;
using AutoMapper;
using hrconnectbackend.Constants;
using hrconnectbackend.Exceptions;
using hrconnectbackend.Extensions;
using hrconnectbackend.Interface.Services.Clients;
using hrconnectbackend.Models;
using hrconnectbackend.Models.DTOs;
using hrconnectbackend.Models.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(SuccessResponse<List<ReadAttendanceDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAttendances([FromQuery] PaginationParams paginationParams)
        {
            var orgId = User.RetrieveSpecificUser("organizationId");

            var attendances = await attendanceServices.GetAllAttendanceByOrganization(int.Parse(orgId), paginationParams);

            var mappedAttendances = mapper.Map<List<ReadAttendanceDto>>(attendances.Data);

            Response.Headers.Append("X-Pagination",
            JsonSerializer.Serialize(attendances.Pagination, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            }));

            var pagedResponse = new PagedResponse<List<ReadAttendanceDto>>(mappedAttendances, attendances.Pagination, $"Attendance records retrieved successfully!");

            return Ok(pagedResponse);

        }
        [Authorize]
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(SuccessResponse<ReadAttendanceDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAttendance(int id)
        {
            var attendance = await attendanceServices.GetByIdAsync(id);

            var attendanceDto = mapper.Map<ReadAttendanceDto>(attendance);
            return Ok(new SuccessResponse<ReadAttendanceDto>(attendanceDto, $"Attendance with ID: {id} retrieved successfully!"));

        }

        [Authorize]
        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(SuccessResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateAttendance(int id, UpdateAttendanceDto updateAttendanceDto)
        {

            var attendance = await attendanceServices.GetByIdAsync(id);

            if (!ModelState.IsValid)
            {
                throw new BadRequestException(ErrorCodes.InvalidRequestModel, "Invalid request. Please check the model.");
            }

            if (attendance == null)
            {
                throw new NotFoundException(ErrorCodes.AttendanceNotFound, $"Attendance with ID: {id} not found.");
            }

            attendance.ClockIn = TimeSpan.Parse(updateAttendanceDto.ClockIn);
            attendance.ClockOut = TimeSpan.Parse(updateAttendanceDto.ClockOut);
            attendance.DateToday = DateTime.Parse(updateAttendanceDto.DateToday);

            await attendanceServices.UpdateAsync(attendance);

            return Ok(new SuccessResponse($"Attendance with ID: {id} updated successfully!"));

        }


        [Authorize]
        [HttpDelete("{id:int}")]
        [ProducesResponseType(typeof(SuccessResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteAttendance(int id)
        {

            var attendance = await attendanceServices.GetByIdAsync(id);

            if (attendance == null)
            {
                throw new NotFoundException(ErrorCodes.AttendanceNotFound, $"Attendance with ID: {id} not found.");
            }

            await attendanceServices.DeleteAsync(attendance);

            return Ok(new SuccessResponse($"Attendance with ID: {id} deleted successfully!"));
        }
        [Authorize]
        [HttpGet("employee/{employeeId:int}")]
        [ProducesResponseType(typeof(SuccessResponse<List<ReadAttendanceDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAttendanceByEmployee(int employeeId, [FromQuery] PaginationParams paginationParams)
        {
            var attendances = await attendanceServices.GetAttendanceByEmployeeId(employeeId, paginationParams);

            var mappedAttendance = mapper.Map<List<ReadAttendanceDto>>(attendances.Data);

            var PagedResponse = new PagedResponse<List<ReadAttendanceDto>>(mappedAttendance, attendances.Pagination, $"Attendance records for employee with ID: {employeeId} retrieved successfully!");

            return Ok(PagedResponse);

        }

        //[Authorize] // Ensures the user is authenticated
        [HttpPost("clock-in")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(SuccessResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> ClockIn()
        {
            string currentEmployeeId = User.GetEmployeeSession();

            if (!int.TryParse(currentEmployeeId, out int employeeId))
            {
                throw new UnauthorizedException(ErrorCodes.Unauthorized, "User not authenticated. Please login.");
            }

            var hasClockedOut = await attendanceServices.HasClockedOut(employeeId);

            if (hasClockedOut)
            {
                throw new ConflictException(ErrorCodes.AlreadyClockedOut, "You have already clocked out for today.");
            }

            await attendanceServices.ClockIn(employeeId);
            return Ok(new SuccessResponse($"Clock-in recorded for employee {employeeId} successfully"));
        }

        [Authorize]
        [HttpGet("clocked-in")]
        [ProducesResponseType(typeof(SuccessResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> HasClockedIn()
        {
            var currentUserId = User.RetrieveSpecificUser("EmployeeId");

            if (!int.TryParse(currentUserId, out int employeeId))
            {
                throw new UnauthorizedException(ErrorCodes.Unauthorized, "User not authenticated. Please login.");
            }

            var clockedIn = await attendanceServices.HasClockedIn(employeeId);

            return Ok(new SuccessResponse<bool>(clockedIn, clockedIn ? "Has Clocked In" : "Not yet clocked in"));

        }
        [Authorize]
        [HttpPut("clock-out")]
        [ProducesResponseType(typeof(SuccessResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ClockOut()
        {
            var currentUserId = User.RetrieveSpecificUser("EmployeeId");

            if (!int.TryParse(currentUserId, out int employeeId))
            {
                throw new UnauthorizedException(ErrorCodes.Unauthorized, "User not authenticated. Please login.");
            }

            await attendanceServices.ClockOut(employeeId);

            return Ok(new SuccessResponse($"Clock-out recorded for employee {employeeId} successfully"));
        }
        [Authorize]
        [HttpGet("clocked-out")]
        [ProducesResponseType(typeof(SuccessResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> HasClockedOut()
        {
            var currentUserId = User.RetrieveSpecificUser("EmployeeId");

            if (!int.TryParse(currentUserId, out var employeeId))
            {
                throw new UnauthorizedException(ErrorCodes.Unauthorized, "User not authenticated. Please login.");
            }

            bool clockedOut = await attendanceServices.HasClockedOut(employeeId);

            return Ok(new SuccessResponse<bool>(clockedOut, clockedOut ? "Has Clocked Out" : "Not yet clocked out"));
        }

        [Authorize(Roles = "Admin,HR")]
        [HttpGet("daily/{employeeId:int}")]
        [ProducesResponseType(typeof(SuccessResponse<ReadAttendanceDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetDailyAttendance(int employeeId)
        {
            var attendanceToday = await attendanceServices.GetDailyAttendanceByEmployeeId(employeeId);

            var attendanceMapped = mapper.Map<ReadAttendanceDto>(attendanceToday);

            return Ok(new SuccessResponse<ReadAttendanceDto>(attendanceMapped, $"Attendance for employee {employeeId} retrieved successfully!"));
        }

        [Authorize]
        [HttpGet("my-attendance-today")]
        [ProducesResponseType(typeof(SuccessResponse<ReadAttendanceDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetDailyAttendance()
        {
            var currentUserId = User.RetrieveSpecificUser("EmployeeId");

            var attendanceToday = await attendanceServices.GetDailyAttendanceByEmployeeId(int.Parse(currentUserId));
            logger.Log(LogLevel.Information, "Attendance retrieved successfully");

            if (attendanceToday == null)
            {
                throw new NotFoundException(ErrorCodes.AttendanceNotFound, $"Attendance for employee {currentUserId} not found.");
            }

            var attendanceMapped = mapper.Map<ReadAttendanceDto>(attendanceToday);

            return Ok(new SuccessResponse<ReadAttendanceDto>(attendanceMapped, "Attendance retrieved successfully"));

        }

        [Authorize(Roles = "Admin,HR")]
        [HttpGet("range/{employeeId:int}")]
        [ProducesResponseType(typeof(PagedResponse<List<ReadAttendanceDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAttendanceInRange(int employeeId, [FromQuery] string start, [FromQuery] string end, [FromQuery] PaginationParams paginationParams)
        {
            var attendanceRecords = await attendanceServices.GetRangeAttendanceByEmployeeId(employeeId, new DateIntervalParam(DateTime.Parse(start), DateTime.Parse(end)), paginationParams);

            var mappedAttendance = mapper.Map<List<ReadAttendanceDto>>(attendanceRecords.Data);

            var PagedResponse = new PagedResponse<List<ReadAttendanceDto>>(mappedAttendance, attendanceRecords.Pagination, $"Attendance records for employee {employeeId} retrieved successfully!");

            return Ok(PagedResponse);
        }
        [Authorize]
        [HttpGet("monthly/{employeeId:int}")]
        [ProducesResponseType(typeof(SuccessResponse<List<ReadAttendanceDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetMonthlyAttendance(int employeeId, [FromQuery] PaginationParams paginationParams)
        {
            var attendanceRecords = await attendanceServices.GetMonthlyAttendanceByEmployeeId(employeeId, paginationParams);

            var mappedAttendance = mapper.Map<List<ReadAttendanceDto>>(attendanceRecords.Data);

            var PagedResponse = new PagedResponse<List<ReadAttendanceDto>>(mappedAttendance, attendanceRecords.Pagination, "Monthly attendance records retrieved successfully!");

            return Ok(PagedResponse);

        }

        [Authorize(Roles = "Admin")]
        [HttpGet("department/{departmentId:int}/attendance-stats/today")]
        [ProducesResponseType(typeof(SuccessResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> EmployeeAttendanceStatsByDeptToday(int departmentId)
        {

            var employeeAttendanceStats = await attendanceServices.EmployeeAttendanceStatsByDeptSpecificOrToday(departmentId, null);

            return Ok(new SuccessResponse<dynamic?>(employeeAttendanceStats, "Employees Attendance Stats Retrieved"));
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("department/{departmentId:int}/attendance-stats")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> EmployeeAttendanceStatsByDeptSpecificDate(int departmentId, [FromQuery] string specificDate)
        {

            var employeeAttendanceStats = await attendanceServices.EmployeeAttendanceStatsByDeptSpecificOrToday(departmentId, DateTime.Parse(specificDate));

            return Ok(new SuccessResponse<dynamic?>(employeeAttendanceStats, "Employees Attendance Stats Retrieved"));

        }

        [Authorize(Roles = "Admin")]
        [HttpGet("attendance-stats/today")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> EmployeeAttendanceStatsToday()
        {
            var employeeAttendanceStats = await attendanceServices.EmployeeAttendanceStatsSpecificOrToday(null);

            return Ok(new SuccessResponse<dynamic?>(employeeAttendanceStats, "Employees Attendance Stats Retrieved"));

        }

        [Authorize(Roles = "Admin")]
        [HttpGet("attendance-stats")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> EmployeeAttendanceStatsSpecificOrToday(string specificDate)
        {
            var employeeAttendanceStats = await attendanceServices.EmployeeAttendanceStatsSpecificOrToday(DateTime.Parse(specificDate));

            return Ok(new SuccessResponse<dynamic?>(employeeAttendanceStats, "Employees Attendance Stats Retrieved"));
        }
        [Authorize]
        [HttpPost("certification")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateCoa(CreateAttendanceCertificationDto attendanceDto)
        {
            var currentUserId = User.RetrieveSpecificUser("EmployeeId");

            if (!int.TryParse(currentUserId, out int employeeId))
            {
                throw new UnauthorizedException(ErrorCodes.Unauthorized, "User not authenticated. Please login.");
            }

            var newAttendance = new AttendanceCertification
            {
                EmployeeId = employeeId,
                SupervisorId = attendanceDto.SupervisorId,
                Date = DateTime.Parse(attendanceDto.Date),
                Status = "Pending",
                ClockIn = TimeSpan.Parse(attendanceDto.ClockIn),
                ClockOut = TimeSpan.Parse(attendanceDto.ClockOut),
                Reason = attendanceDto.Reason ?? "No Log",
                DateCreated = DateTime.Now,
            };

            await attendanceCertificationServices.CreateCertification(newAttendance);

            return Ok(new SuccessResponse($"Attendance certification created successfully!"));
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("certification")]
        [ProducesResponseType(typeof(SuccessResponse<List<AttendanceCertification>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAttendanceCertification([FromQuery] PaginationParams paginationParams)
        {
            var orgId = User.RetrieveSpecificUser("organizationId");

            var certifications = await attendanceCertificationServices.GetAllCertifications(int.Parse(orgId), paginationParams);

            return Ok(certifications);
        }

        [Authorize]
        [HttpGet("certification/employee/{employeeId:int}")]
        [ProducesResponseType(typeof(SuccessResponse<List<AttendanceCertification>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAttendanceCertificationByEmployee(int employeeId, [FromQuery] PaginationParams paginationParams)
        {
            var certifications = await attendanceCertificationServices.GetCertificationsByEmployeeId(employeeId, paginationParams);

            return Ok(certifications);
        }

        [Authorize]
        [HttpGet("certification/{id:int}")]
        [ProducesResponseType(typeof(SuccessResponse<AttendanceCertification>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAttendanceCertification(int id)
        {
            var certification = await attendanceCertificationServices.GetCertificationById(id);

            return Ok(new SuccessResponse<AttendanceCertification>(certification, $"Attendance certification with ID {id} retrieved successfully!"));

        }

        [HttpPut("certification/{id:int}")]
        [ProducesResponseType(typeof(SuccessResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateAttendanceCertification(int id, [FromBody] UpdateAttendanceCertificationDto attendanceDto)
        {
            var existingAttendance = await attendanceCertificationServices.GetCertificationById(id);

            existingAttendance.ClockIn = TimeOnly.Parse(attendanceDto.ClockIn).ToTimeSpan();
            existingAttendance.ClockOut = TimeOnly.Parse(attendanceDto.ClockOut).ToTimeSpan();
            existingAttendance.Date = DateTime.Parse(attendanceDto.Date);
            existingAttendance.Reason = attendanceDto.Reason;

            await attendanceCertificationServices.UpdateCertification(existingAttendance);

            return Ok(new SuccessResponse($"Attendance certification with ID {id} updated successfully!"));

        }
        [Authorize(Roles = "Admin,HR")]
        [HttpDelete("certification/{id:int}")]
        [ProducesResponseType(typeof(SuccessResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteCertification(int id)
        {
            await attendanceCertificationServices.DeleteCertification(id);

            return Ok(new SuccessResponse($"Attendance certification with ID: {id} deleted successfully!"));

        }

        [Authorize(Roles = "Admin")]
        [HttpPost("certification/approve/{id:int}")]
        [ProducesResponseType(typeof(SuccessResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ApproveCoa(int id)
        {
            await attendanceCertificationServices.ApproveCertification(id);

            return Ok(new SuccessResponse($"Attendance certification approved successfully!"));
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("certification/reject/{id:int}")]
        [ProducesResponseType(typeof(SuccessResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RejectCoa(int id)
        {
            await attendanceCertificationServices.RejectCertification(id);

            return Ok(new SuccessResponse($"Attendance certification rejected successfully!"));
        }
    }
}
