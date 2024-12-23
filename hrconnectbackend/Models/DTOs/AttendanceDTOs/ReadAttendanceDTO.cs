using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace hrconnectbackend.Models
{
    public class ReadAttendanceDTO
    {
        public int AttendanceId { get; set; }
        public int EmployeeId { get; set; }
        public DateOnly DateToday { get; set; }  // Store as string
        public string ClockIn { get; set; }      // Store as string
        public string ClockOut { get; set; }     // Store as string
        public int hoursWorked { get; set; }
    }

}
