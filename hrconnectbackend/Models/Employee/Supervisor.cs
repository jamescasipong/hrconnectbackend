using System.ComponentModel.DataAnnotations;

namespace hrconnectbackend.Models
{
    public class Supervisor
    {
        [Key]
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public Employee Employee { get; set; }
        public Department? Department { get; set; }
        public List<Employee>? Subordinates { get; set; }
        public List<AttendanceCertification>? AttendanceCertifications { get; set; }
    }
}
