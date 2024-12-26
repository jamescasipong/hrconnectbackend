using AutoMapper;
using hrconnectbackend.IRepositories;
using hrconnectbackend.Models;
using hrconnectbackend.Models.DTOs.AttendanceDTOs;
using hrconnectbackend.Models.DTOs.Shifts;
using Microsoft.AspNetCore.Mvc;

namespace hrconnectbackend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AttendanceController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IAttendanceRepositories _attendanceRepositories;
        private readonly IShiftRepositories _shiftRepositories;
        private readonly IEmployeeRepositories _employeeRepositories;
        public AttendanceController(IAttendanceRepositories attendanceRepositories, IMapper mapper, IEmployeeRepositories employeeRepositories, IShiftRepositories shiftRepositories)
        {
            _attendanceRepositories = attendanceRepositories;
            _mapper = mapper;
            _employeeRepositories = employeeRepositories;
            _shiftRepositories = shiftRepositories;
        }


        [HttpGet("get/all")]
        public async Task<IActionResult> getAttendances()
        {
            ICollection<Attendance> attendances = await _attendanceRepositories.GetAllAsync();

            if (!ModelState.IsValid) return BadRequest(ModelState);


            if (attendances.Count == 0) return NotFound("Not Found!");

            var dto = _mapper.Map<ICollection<ReadAttendanceDTO>>(attendances);

            foreach (var item in dto)
            {
                item.Day = item.DateToday.DayOfWeek.ToString();
            }

            return Ok(dto);
        }

        [HttpGet("get/{id}")]
        public async Task<IActionResult> getAttendance(int id)
        {
            Attendance attendance = await _attendanceRepositories.GetByIdAsync(id);

            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (attendance == null) return Ok(null);

            var dto = _mapper.Map<ReadAttendanceDTO>(attendance);

            return Ok(dto);
        }


        [HttpPut("update/{id}")]
        public async Task<IActionResult> updateAttendance(int id, [FromBody] UpdateAttendanceDTO attendance)
        {
            var employee = await _attendanceRepositories.GetAllAsync();

            var employeeToUpdate = employee.Where(a => a.AttendanceId == id).FirstOrDefault();

            if (employee == null) return NotFound("Not Found!");

            if (attendance.DateToday != null) employeeToUpdate.DateToday = DateOnly.Parse(attendance.DateToday);  // Store as string
            if (!string.IsNullOrEmpty(attendance.ClockIn)) employeeToUpdate.ClockIn = TimeOnly.Parse(attendance.ClockIn);    // Convert string to TimeOnly
            if (!string.IsNullOrEmpty(attendance.ClockOut)) employeeToUpdate.ClockOut = TimeOnly.Parse(attendance.ClockOut);  // Convert string to TimeOnly

            await _attendanceRepositories.UpdateAsync(employeeToUpdate);

            return Ok(attendance);
        }


        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> deleteAttendance(int id)
        {


            var attendance = await _attendanceRepositories.GetAllAsync();

            var attendanceToDelete = attendance.Where(a => a.AttendanceId == id).FirstOrDefault();

            if (attendance == null) return NotFound("Not Found!");

            await _attendanceRepositories.DeleteAsync(attendanceToDelete);

            return Ok("Deleted!");
        }


        [HttpPost("create/{id}")]
        public async Task<IActionResult> createAttendance(int id, [FromBody] CreateAttendanceDTO attendanceDTO)
        {

            var attendance = new Attendance
            {
                EmployeeId = id,
                DateToday = DateOnly.Parse(attendanceDTO.DateToday),
                ClockIn = TimeOnly.Parse(attendanceDTO.ClockIn),
                ClockOut = TimeOnly.Parse(attendanceDTO.ClockOut)
            };

            await _attendanceRepositories.AddAsync(attendance);

            return Ok(attendance);
        }


        [HttpPost("createMutiple/{id}")]
        public async Task<IActionResult> CreateAttendance(int id, [FromBody] List<CreateAttendanceDTO> attendanceDTO)
        {
            for (int i = 0; i < attendanceDTO.Count; i++)
            {

                var attendance = new Attendance
                {
                    EmployeeId = id,
                    DateToday = DateOnly.Parse(attendanceDTO[i].DateToday),
                    ClockIn = TimeOnly.Parse(attendanceDTO[i].ClockIn),
                    ClockOut = TimeOnly.Parse(attendanceDTO[i].ClockOut)
                };



                await _attendanceRepositories.AddAsync(attendance);
            }


            return Ok(attendanceDTO);
        }


        [HttpPost("create/clockin/{id}")]
        public async Task<IActionResult> clockIn(int id)
        {
            var hasShift = await _shiftRepositories.HasShiftToday(id);

            if (!hasShift) return BadRequest("Employee has no shift today!");

            var result = await _attendanceRepositories.ClockIn(id);

            return Ok(result);
        }

        [HttpPost("create/clockout/{id}")]
        public async Task<IActionResult> clockOut(int id)
        {
            var result = await _attendanceRepositories.ClockOut(id);

            return Ok(result);
        }


        [HttpPost("create/fileattendance/{id}")]
        public async Task<IActionResult> fileAttendance(int id, [FromBody] DateOnly date)
        {
            await _attendanceRepositories.FileAttendance(id, date);

            return Ok("Attendance filed!");
        }

        [HttpPost("create/approve/{id}")]

        public async Task<IActionResult> approveAttendance(int id)
        {
            await _attendanceRepositories.ApproveAttendance(id);

            return Ok("Attendance approved!");
        }


        [HttpPost("create/reject/{id}")]

        public async Task<IActionResult> rejectAttendance(int id)
        {
            await _attendanceRepositories.RejectAttendance(id);

            return Ok("Attendance rejected!");
        }

        [HttpGet("get/daily/{id}")]

        public async Task<IActionResult> getDailyAttendanceByEmployeeId(int id)
        {
            var attendance = await _attendanceRepositories.GetDailyAttendanceByEmployeeId(id);

            if (attendance == null) return NotFound("Not Found!");

            return Ok(attendance);
        }

        [HttpGet("get/monthly/{id}")]

        public async Task<IActionResult> getMonthlyAttendanceByEmployeeId(int id)
        {
            var attendance = await _attendanceRepositories.GetMonthlyAttendanceByEmployeeId(id);

            if (attendance.Count == 0) return NotFound("Not Found!");

            return Ok(attendance);
        }

        [HttpPost("post/getRangeDate/{id}")]
        public async Task<IActionResult> GetRangeAttendanceByEmployeeId(int id, [FromBody] DateRangeDTO dateRange)
        {
            var attendance = await _attendanceRepositories.GetRangeAttendanceByEmployeeId(id, DateOnly.Parse(dateRange.Start), DateOnly.Parse(dateRange.End));

            var attendanceList = attendance.ToList();

            if (attendance.Count == 0) return NotFound("Not Found!");

            var skippedDays = 0;

            var dto = _mapper.Map<ICollection<ReadAttendanceDTO>>(attendance);

            var dtoList = dto.ToList();

            for (var i = 0; i < dtoList.Count - 1; i++)
            {
                var clockInTime = attendanceList[i].ClockIn;
                var clockOutTime = attendanceList[i].ClockOut;
                TimeSpan hoursWorked = clockInTime - clockOutTime;

                int gapDays = dtoList[i + 1].DateToday.Day - dtoList[i].DateToday.Day;

                dtoList[i].hoursWorked = hoursWorked.Hours;


                if (gapDays > 1)
                {
                    skippedDays += gapDays - 1;
                    Console.WriteLine("Skipped Days: " + skippedDays);
                }

            }

            return Ok(new
            {
                dtoList,
                skippedDays
            });
        }

        [HttpPost("post/addShiftToEmployee/{id}")]
        public async Task<IActionResult> AddShiftToEmployee(int id, [FromBody] CreateShiftDTO shiftDTO)
        {
            var addedShift = await _shiftRepositories.AddShiftToEmployee(id, shiftDTO);


            return Ok(addedShift);
        }

        [HttpGet("get/employeeShifts/{id}")]
        public async Task<IActionResult> EmpHasShifts(int id)
        {
            var shifts = await _shiftRepositories.GetEmployeeShiftsById(id);

            if (shifts.Count == 0) return NotFound("No shifts found!");

            var getDayToday = DateTime.Now.DayOfWeek.ToString();

            foreach (var shift in shifts)
            {
                if (shift.DaysOfWorked == getDayToday)
                {
                    return Ok(new { message = "Employee has shift today" });
                }
            }

            return Ok(new { message = "Employee has no shift today" });
        }

        [HttpPost("get/employeeisLate/{id}/{date}")]
        public async Task<IActionResult> EmpIsLate(int id)
        {
            var shifts = await _shiftRepositories.GetEmployeeShiftsById(id);

            var empAttendance = await _attendanceRepositories.GetDailyAttendanceByEmployeeId(id);

            if (empAttendance == null) return NotFound("No attendance found!");

            if (shifts.Count == 0) return NotFound("No shifts found!");

            var getDayToday = DateTime.Now.DayOfWeek.ToString();

            foreach (var shift in shifts)
            {
                if (shift.DaysOfWorked == getDayToday)
                {
                    if (empAttendance.ClockIn > shift.TimeIn)
                    {
                        return Ok(new { message = "Employee is late" });
                    }
                }
            }

            return Ok(new { message = "Employee is not late" });

        }

    }
}
