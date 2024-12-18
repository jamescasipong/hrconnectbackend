using hrconnectbackend.IRepositories;
using hrconnectbackend.Models;
using Microsoft.AspNetCore.Mvc;

namespace hrconnectbackend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AttendanceController : Controller
    {
        private readonly IAttendanceRepositories _attendanceRepositories;
        public AttendanceController(IAttendanceRepositories attendanceRepositories) {
            _attendanceRepositories = attendanceRepositories;
        }

        [HttpGet]
        public async Task<IActionResult> getAttendances()
        {
            List<Attendance> attendances = await _attendanceRepositories.GetAttendances();

            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (attendances.Count == 0) return NotFound("Not Found!");

            return Ok(attendances);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> getAttendance(int id)
        {
            Attendance attendance = await _attendanceRepositories.getAttendance(id);

            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (attendance == null) return Ok(null);

            return Ok(attendance);
        }
    }
}
