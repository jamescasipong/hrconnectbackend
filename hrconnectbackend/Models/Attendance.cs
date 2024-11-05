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
        public TimeOnly ClockIn { get; set; }
        public TimeOnly ClockOut { get; set; }
        public Employee Employee { get; set; }
    }
}
