using System.Security.Claims;
using AutoMapper;
using hrconnectbackend.Helper;
using hrconnectbackend.Helper.CustomExceptions;
using hrconnectbackend.Interface.Services;
using hrconnectbackend.Interface.Services.Clients;
using hrconnectbackend.Models;
using hrconnectbackend.Models.DTOs;
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
        IEmployeeServices employeeServices,
        IShiftServices shiftServices,
        IAttendanceCertificationServices attendanceCertificationServices,
        ISupervisorServices supervisorServices,
        ILogger<Attendance> logger)
        : Controller
    {
        private readonly IShiftServices _shiftServices = shiftServices;
        private readonly IEmployeeServices _employeeServices = employeeServices;

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

                var mappedAttendances = mapper.Map<List<ReadAttendanceDTO>>(attendances);

                if (!attendances.Any()) Ok(new ApiResponse<List<ReadAttendanceDTO>>(true, $"Attendances not found.", mappedAttendances));

                return Ok(new ApiResponse<List<ReadAttendanceDTO>>(true, $"Attendances retrived successfully!", mappedAttendances));
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

                if (attendance == null) return NotFound(new ApiResponse(false, $"Attendance with an ID: {id} not found."));

                return Ok(new ApiResponse<ReadAttendanceDTO>(false, $"Attendance with id: {id} retrieved successfully.", mapper.Map<ReadAttendanceDTO>(attendance)));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error deleting attendance with an ID: {id}", id);
                return StatusCode(500, new ApiResponse(false, "Internal server error"));
            }
        }
        [Authorize]
        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateAttendance(int id, UpdateAttendanceDTO updateAttendanceDTO)
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

                attendance.ClockIn = TimeSpan.Parse(updateAttendanceDTO.ClockIn);
                attendance.ClockOut = TimeSpan.Parse(updateAttendanceDTO.ClockOut);
                attendance.DateToday = DateTime.Parse(updateAttendanceDTO.DateToday);

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
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var userRoles = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();

                if (currentUserId == null)
                {
                    return Unauthorized(new ApiResponse(false, "User not authenticated."));
                }

                bool isAdmin = userRoles.Contains("Admin");
                // // Convert user ID to int
                // if (!int.TryParse(currentUserId, out int employeeId))
                // {
                //     return Unauthorized(new ApiResponse(false, "Invalid user identifier."));
                // }
                
                

                var attendances = await attendanceServices.GetAttendanceByEmployeeId(employeeId, pageIndex, pageSize);

                var mappedAttendance = mapper.Map<List<ReadAttendanceDTO>>(attendances);

                if (!attendances.Any()) return Ok(new ApiResponse<List<ReadAttendanceDTO>>(true, $"Attendances by employee {employeeId} not found.", mappedAttendance));

                return Ok(new ApiResponse<List<ReadAttendanceDTO>>(true, $"Attendances by employee {employeeId} retrieved successfully!", mappedAttendance));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving attendance for employee with an ID: {id}", employeeId);
                return StatusCode(500, new ApiResponse(false, "Internal server error"));
            }
        }

        [Authorize] // Ensures the user is authenticated
        [HttpPost("clock-in")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ClockIn()
        {
            try
            {
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                // var userRoles = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();

                if (currentUserId == null)
                {
                    return Unauthorized(new ApiResponse(false, "User not authenticated."));
                }

                // Convert user ID to int
                if (!int.TryParse(currentUserId, out int employeeId))
                {
                    return Unauthorized(new ApiResponse(false, "Invalid user identifier."));
                }

                var hasClockedOut = await attendanceServices.HasClockedOut(employeeId);

                if (hasClockedOut){
                    return Ok(new ApiResponse(false, $"You have already clocked out"));
                }

                await attendanceServices.ClockIn(employeeId);
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
                return NotFound(new ApiResponse(false, ex.Message));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing clock-in for employee");
                return StatusCode(500, new ApiResponse(false, "Internal server error"));
            }
        }

        [Authorize]
        [HttpGet("clocked-in")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> HasClockedIn()
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (currentUserId == null)
            {
                return Unauthorized(new ApiResponse(false, "User not authenticated."));
            }

            if (!int.TryParse(currentUserId, out int employeeId))
            {
                return Unauthorized(new ApiResponse(false, "Invalid user identifier."));
            }

            try
            {
                var clockedIn = await attendanceServices.HasClockedIn(employeeId);

                return Ok(new ApiResponse<bool>(true, $"{(clockedIn ? "Has Clocked In" : "Not yet clocked In")}", clockedIn));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error checking's employee attendance {EmployeeId}", employeeId);
                return StatusCode(500, new ApiResponse(false, "Internal server error"));
            }
        }
        [Authorize]
        [HttpPut("clock-out")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ClockOut()
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (currentUserId == null)
            {
                return Unauthorized(new ApiResponse(false, "User not authenticated."));
            }

            if (!int.TryParse(currentUserId, out int employeeId))
            {
                return Unauthorized(new ApiResponse(false, "Invalid user identifier."));
            }

            try
            {
                var clockedIn = await attendanceServices.HasClockedIn(employeeId);

                if (!clockedIn) return BadRequest(new ApiResponse(false, $"You must clock in first."));

                await attendanceServices.ClockOut(employeeId);

                return Ok(new ApiResponse(true, $"Clock-out recorded for employee {employeeId} successfully"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponse(false, ex.Message));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing clock-out for employee {EmployeeId}", employeeId);
                return StatusCode(500, new ApiResponse(false, "Internal server error"));
            }
        }
        [Authorize]
        [HttpGet("clocked-out")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> HasClockedOut()
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (currentUserId == null)
            {
                return Unauthorized(new ApiResponse(false, "User not authenticated."));
            }

            if (!int.TryParse(currentUserId, out var employeeId)){
                return Unauthorized(new ApiResponse(false, "User not authenticated."));
            }

            try
            {
                bool clockedOut = await attendanceServices.HasClockedOut(employeeId);

                return Ok(new ApiResponse<bool>(true, $"Has already clocked in.", clockedOut));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error checking's employee attendance {EmployeeId}", employeeId);
                return StatusCode(500, new ApiResponse(false, "Internal server error"));
            }
        }

        [Authorize]
        [HttpGet("daily/{employeeId:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetDailyAttendance(int employeeId)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userRoles = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();

            if (currentUserId == null)
            {
            logger.LogError("User not authenticated.");
            Console.WriteLine("User not authenticated.");
            return Unauthorized(new ApiResponse(false, "User not authenticated."));
            }

            if (!int.TryParse(currentUserId, out int currentUserIdInt))
            {
                Console.WriteLine("User not authenticated.");
            logger.LogError("Invalid user identifier.");
            return Unauthorized(new ApiResponse(false, "Invalid user identifier."));
            }

            bool isAdmin = userRoles.Contains("Admin");
            if (!isAdmin && currentUserIdInt != employeeId)
            {
                return Forbid(); // 403 Forbidden
            }
            
            try
            {
            var attendanceToday = await attendanceServices.GetDailyAttendanceByEmployeeId(employeeId);

            return Ok(attendanceToday);
            }
            catch (KeyNotFoundException ex)
            {
            return NotFound(new ApiResponse(false, ex.Message));
            }
            catch (ArgumentException ex)
            {
            return BadRequest(new ApiResponse(false, ex.Message));
            }
            catch (Exception ex)
            {
            logger.LogError(ex, "Error retrieving daily attendance for employee ID: {id}", employeeId);
            return StatusCode(500, new { error = "An internal error occurred" });
            }
        }

        [Authorize]
        [HttpGet("my-attendance-today")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetDailyAttendance()
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (currentUserId == null)
            {
            logger.LogError("User not authenticated.");
            Console.WriteLine("User not authenticated.");
            return Unauthorized(new ApiResponse(false, "User not authenticated."));
            }

            if (!int.TryParse(currentUserId, out int employeeId))
            {
                Console.WriteLine("User not authenticated.");
            logger.LogError("Invalid user identifier.");
            return Unauthorized(new ApiResponse(false, "Invalid user identifier."));
            }
            
            try
            {
                var attendanceToday = await attendanceServices.GetDailyAttendanceByEmployeeId(employeeId);
                logger.Log(LogLevel.Information, "Attendance retrieved successfully");

                var attendanceMapped = mapper.Map<ReadAttendanceDTO>(attendanceToday);

                return Ok(new ApiResponse<ReadAttendanceDTO>(true, "Attendance retrieved successfully", attendanceMapped));
            }
            catch (KeyNotFoundException ex)
            {
                logger.Log(LogLevel.Information, ex.Message);
                return NotFound(new ApiResponse(false, ex.Message));
            }
            catch (ArgumentException ex)
            {
            return BadRequest(new ApiResponse(false, ex.Message));
            }
            catch (Exception ex)
            {
            logger.LogError(ex, "Error retrieving daily attendance for employee ID: {id}", employeeId);
            return StatusCode(500, new { error = "An internal error occurred" });
            }
        }

        [Authorize]
        [HttpGet("range/{employeeId:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAttendanceInRange(int employeeId, [FromQuery] string start, [FromQuery] string end, int? pageIndex = 1, int? pageSize = 5)
        {
            try
            {
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var userRoles = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();

                if (currentUserId == null)
                {
                    return Unauthorized(new ApiResponse(false, "User not authenticated."));
                }

                if (!int.TryParse(currentUserId, out int currentUserIdInt))
                {
                    return Unauthorized(new ApiResponse(false, "Invalid user identifier."));
                }

                bool isAdmin = userRoles.Contains("Admin");

                if (!isAdmin && currentUserIdInt != employeeId)
                {
                    return Forbid(); // 403 Forbidden
                }

                var attendanceRecords = await attendanceServices.GetRangeAttendanceByEmployeeId(employeeId, DateTime.Parse(start), DateTime.Parse(end), pageIndex, pageSize);
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
                logger.LogError(ex, $"Error retrieving in the range between {start} and {end} for employee ID: {employeeId}");
                return StatusCode(500, new { error = "An internal error occurred" });
            }
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
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var userRoles = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();

                if (currentUserId == null)
                {
                    return Unauthorized(new ApiResponse(false, "User not authenticated."));
                }

                if (!int.TryParse(currentUserId, out int currentUserIdInt))
                {
                    return Unauthorized(new ApiResponse(false, "Invalid user identifier."));
                }

                bool isAdmin = userRoles.Contains("Admin");

                if (!isAdmin && currentUserIdInt != id)
                {
                    return Forbid(); // 403 Forbidden
                }

                var attendanceRecords = await attendanceServices.GetMonthlyAttendanceByEmployeeId(id, pageIndex, pageSize);
                return Ok(new ApiResponse<List<Attendance>>(true, "Monthly attendance retrieved successfully!", attendanceRecords));
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
            try
            {
                var employeeAttendanceStats = await attendanceServices.EmployeeAttendanceStatsByDeptSpecificOrToday(departmentId, null);

                return Ok(new ApiResponse<dynamic>(true, "Employees Attendance Stats Retrieved", employeeAttendanceStats));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving attendance statistics of employees of today from department ID: {departmentId}", departmentId);
                return StatusCode(500, new ApiResponse(false, "Internal server error"));
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("department/{departmentId:int}/attendance-stats")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> EmployeeAttendanceStatsByDeptSpecificDate(int departmentId, [FromQuery] string specificDate)
        {
            try
            {
                var employeeAttendanceStats = await attendanceServices.EmployeeAttendanceStatsByDeptSpecificOrToday(departmentId, DateTime.Parse(specificDate));

                return Ok(new ApiResponse<dynamic>(true, "Employees Attendance Stats Retrieved", employeeAttendanceStats));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error retrieving attendance statistics of employees of {specificDate} from department ID: {departmentId}");
                return StatusCode(500, new ApiResponse(false, "Internal server error"));
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("attendance-stats/today")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> EmployeeAttendanceStatsToday()
        {
            try
            {
                var employeeAttendanceStats = await attendanceServices.EmployeeAttendanceStatsSpecificOrToday(null);

                return Ok(new ApiResponse<dynamic>(true, "Employees Attendance Stats Retrieved", employeeAttendanceStats));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving attendance statistics of employees of today");
                return StatusCode(500, new ApiResponse(false, "Internal server error"));
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("attendance-stats")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> EmployeeAttendanceStatsSpecificOrToday(string specificDate)
        {
            var employeeAttendanceStats = await attendanceServices.EmployeeAttendanceStatsSpecificOrToday(DateTime.Parse(specificDate));

            return Ok(new ApiResponse<dynamic>(true, "Employees Attendance Stats Retrieved", employeeAttendanceStats));
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("certification")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateCOA(CreateAttendanceCertificationDTO attendanceDTO)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(currentUserId, out int employeeId))
            {
                return Unauthorized(new ApiResponse(false, "Invalid user identifier."));
            }

            if (attendanceDTO.EmployeeId != employeeId)
            {
                return Unauthorized(new ApiResponse(false, "You are not authorized to create a certification for another employee."));
            }

            var supervisor = await supervisorServices.GetSupervisor(attendanceDTO.SupervisorId);

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

            await attendanceCertificationServices.AddAsync(newAttendance);

            return Ok(new ApiResponse(true, $"Attendance certifications created sucessfully!"));
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("certification")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAttendanceCertification()
        {
            var certifications = await attendanceCertificationServices.GetAllAsync();

            return Ok(new ApiResponse<List<AttendanceCertification>>(true, $"Attendance certifications retrieved sucessfully!", certifications));
        }

        [Authorize]
        [HttpGet("certification/{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAttendanceCertification(int id)
        {
            var certification = await attendanceCertificationServices.GetByIdAsync(id);

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userRoles = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();

            if (currentUserId == null)
            {
                return Unauthorized(new ApiResponse(false, "User not authenticated."));
            }

            if (!int.TryParse(currentUserId, out int currentUserIdInt))
            {
                return Unauthorized(new ApiResponse(false, "Invalid user identifier."));
            }

            bool isAdmin = userRoles.Contains("Admin");

            if (!isAdmin && currentUserIdInt != certification.EmployeeId)
            {
                return Forbid(); // 403 Forbidden
            }

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
                logger.LogError(ex, "Error retrieving attendance with ID {Id}", id);
                return StatusCode(500, new ApiResponse(false, "Internal server error"));
            }
        }

        [HttpPut("certification/{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateAttendanceCertification(int id, [FromBody] UpdateAttendanceCertificationDTO attendanceDTO)
        {
            var certification = await attendanceCertificationServices.GetByIdAsync(id);

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
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
                return BadRequest(new ApiResponse(false, ModelState.IsValid.ToJson().ToString()));
            }
            try
            {
                var existingAttendance = await attendanceCertificationServices.GetByIdAsync(id);
                if (existingAttendance == null)
                {
                    return NotFound(new ApiResponse(false, $"Attendance certification with ID {id} not found"));
                }

                existingAttendance.ClockIn = TimeOnly.Parse(attendanceDTO.ClockIn).ToTimeSpan();
                existingAttendance.ClockOut = TimeOnly.Parse(attendanceDTO.ClockOut).ToTimeSpan();
                existingAttendance.Date = DateTime.Parse(attendanceDTO.Date);
                existingAttendance.Reason = attendanceDTO.Reason;

                await attendanceCertificationServices.UpdateAsync(existingAttendance);

                return Ok(new ApiResponse(false, $"Attendance certification with ID: {id} updated successfully"));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error updating attendance with ID {Id}", id);
                return StatusCode(500, new ApiResponse(false, "Internal server error"));
            }
        }

        [HttpDelete("certification/{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteCertification(int id)
        {
            var certification = await attendanceCertificationServices.GetByIdAsync(id);

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userRoles = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();

            if (currentUserId == null)
            {
                return Unauthorized(new ApiResponse(false, "User not authenticated."));
            }

            if (!int.TryParse(currentUserId, out int currentUserIdInt))
            {
                return Unauthorized(new ApiResponse(false, "Invalid user identifier."));
            }

            bool isAdmin = userRoles.Contains("Admin");

            if (!isAdmin && currentUserIdInt != certification.EmployeeId)
            {
                return Forbid(); // 403 Forbidden
            }

            try
            {
                if (certification == null)
                {
                    return NotFound("No certification found");
                }

                await attendanceCertificationServices.DeleteAsync(certification);

                return Ok(new ApiResponse
                (
                    true, "Attendance certification deleted succcessfully."
                ));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving attendance with ID {Id}", id);
                return StatusCode(500, new ApiResponse(false, "Internal server error"));
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("certification/approve/{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ApproveCOA(int id)
        {

            try
            {
                await attendanceCertificationServices.ApproveCertification(id);

                return Ok(new ApiResponse(true, "Attendance certification approved successfully."));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponse(false, ex.Message));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error approving a certification with ID {id}", id);
                return StatusCode(500, "An internal server error occurred.");
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("certification/reject/{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RejectCOA(int id)
        {

            try
            {
                await attendanceCertificationServices.RejectCertification(id);

                return Ok(new ApiResponse(true, "Attendance certification deleted successfully."));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponse(false, ex.Message));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error rejecting attendance certification.");
                return StatusCode(500, "An internal server error occurred.");
            }
        }
        
    }
}
