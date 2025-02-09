namespace hrconnectbackend.Models
{
    public class AttendanceCertification
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public int? SupervisorId { get; set; }
        public string Status { get; set; }
        public DateOnly Date { get; set; }
        public TimeSpan ClockIn { get; set; }
        public TimeSpan ClockOut { get; set; }
        public string Reason { get; set; }
        public DateTime DateCreated { get; set; }
        public Employee? Employee { get; set; }
        public Supervisor? Supervisor { get; set; }
    }
}
