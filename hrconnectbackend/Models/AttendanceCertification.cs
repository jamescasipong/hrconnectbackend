
using hrconnectbackend.Models.EmployeeModels;

namespace hrconnectbackend.Models
{
    public class AttendanceCertification
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public int SupervisorId { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public int TenantId { get; set; }
        public TimeSpan ClockIn { get; set; }
        public TimeSpan ClockOut { get; set; }
        public string Reason { get; set; } = string.Empty;
        public DateTime DateCreated { get; set; }
        public int OrganizationId { get; set; } // Foreign key to Organization
        public Organization? Organization { get; set; } // Navigation property to Organization
        public Employee? Employee { get; set; }
    }
}
