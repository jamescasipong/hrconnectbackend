using hrconnectbackend.Models;

namespace hrconnectbackend.Models
{
    public class Department
    {
        public int DepartmentId { get; set; }
        public int TenantId { get; set; }
        public string DeptName { get; set; }
        public string? Description { get; set; } = "N/A";
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; } = null;
        
    }
}
