using hrconnectbackend.Models.EmployeeModels;
using System.ComponentModel.DataAnnotations;

namespace hrconnectbackend.Models
{
    public class Leaves
    {
        [Key]
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public string leaveType {  get; set; }  = string.Empty;
        public int UsedLeaves { get; set; }
        public int TotalLeaves { get; set; }
        public int RemaningLeaves { get; set; }
        public int OrganizationId { get; set; } // Foreign key to Organization
        public Employee? Employee { get; set; }
        public virtual Organization? Organization { get; set; } // Navigation property to Organization

    }
}
