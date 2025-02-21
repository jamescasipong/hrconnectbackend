namespace hrconnectbackend.Models.DTOs
{
    public class CreateAttendanceDTO
    {
        public string DateToday { get; set; }  // Store as string
        public string ClockIn { get; set; }      // Store as string
        public string ClockOut { get; set; }     // Store as string
    }

    public class ReadAttendanceDTO
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

    public class UpdateAttendanceDTO
    {
        public string DateToday { get; set; }  // Store as string
        public string ClockIn { get; set; }      // Store as string
        public string ClockOut { get; set; }     // Store as string
    }
}
