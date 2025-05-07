using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using hrconnectbackend.Models.EmployeeModels;

namespace hrconnectbackend.Models
{
    public class LeaveBalance
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey("Employee")]
        public int EmployeeId { get; set; }

        [Required]
        public string LeaveType { get; set; } = string.Empty; // e.g., Sick Leave, Vacation Leave

        [Required]
        public int TotalLeaves { get; set; } // Total allocated leaves

        [Required]
        public int UsedLeaves { get; set; } = 0; // Leaves used so far

        public int RemainingLeaves => TotalLeaves - UsedLeaves; // Computed field
        public int OrganizationId { get; set; } // Foreign key to Organization

        public virtual Organization? Organization { get; set; } // Navigation property to Organization
        public virtual Employee? Employee { get; set; }
    }
}
