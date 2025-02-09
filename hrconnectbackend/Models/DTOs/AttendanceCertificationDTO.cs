namespace hrconnectbackend.Models.DTOs
{
    public class CreateAttendanceCertificationDTO
    {
        public int EmployeeId { get; set; }
        public int SupervisorId { get; set; }
        public string Date { get; set; }
        public string ClockIn { get; set; }
        public string ClockOut { get; set; }
        public string? Reason { get; set; }
    }
}
