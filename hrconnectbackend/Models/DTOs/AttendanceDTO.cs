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
        public DateOnly DateToday { get; set; }  // Store as string
        public string Day { get; set; }
        public string ClockIn { get; set; }      // Store as string
        public string ClockOut { get; set; }     // Store as string
        public int hoursWorked { get; set; }
    }

    public class UpdateAttendanceDTO
    {
        public string DateToday { get; set; }  // Store as string
        public string ClockIn { get; set; }      // Store as string
        public string ClockOut { get; set; }     // Store as string
    }
}
