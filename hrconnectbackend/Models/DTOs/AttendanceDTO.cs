namespace hrconnectbackend.Models.DTOs
{
    public class CreateAttendanceDto
    {
        public string DateToday { get; set; } = string.Empty;  // Store as string
        public string ClockIn { get; set; } = string.Empty;     // Store as string
        public string ClockOut { get; set; } = string.Empty; // Store as string
    }

    public class ReadAttendanceDto
    {
        public int AttendanceId { get; set; }
        public int EmployeeId { get; set; }
        public DateTime DateToday { get; set; }  // Store as string
        public TimeSpan ClockIn { get; set; }      // Store as string
        public TimeSpan ClockOut { get; set; }     // Store as string
        public decimal WorkingHours { get; set; }
        public TimeSpan? LateClockIn { get; set; }
        public TimeSpan? EarlyLeave { get; set; }
    }

    public class UpdateAttendanceDto
    {
        public string DateToday { get; set; } = string.Empty;  // Store as string
        public string ClockIn { get; set; } = string.Empty;     // Store as string
        public string ClockOut { get; set; } = string.Empty; // Store as string
    }
}
