using System.ComponentModel.DataAnnotations;

namespace hrconnectbackend.Models
{
    public class Supervisor
    {
        [Key]
        public int Id { get; set; }
        public int? EmployeeId { get; set; }
        public Employee Employee { get; set; }
        public Department Department { get; set; }
        public List<LeaveApproval> LeaveApprovals { get; set; }
        public List<OTApproval> OTApprovals { get; set; }
        public List<Employee> Subordinates { get; set; }
    }
}
