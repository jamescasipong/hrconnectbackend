using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace hrconnectbackend.Models
{
    public class Attendance
    {
        [Key]
        public int AttendanceId { get; set; }
        public int EmployeeId { get; set; }
        public DateOnly DateToday { get; set; }
        public TimeSpan ClockIn { get; set; }
        public TimeSpan? ClockOut { get; set; }
        public Employee? Employee { get; set; }
    }
}
