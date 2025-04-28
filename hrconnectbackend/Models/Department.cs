using hrconnectbackend.Models;

namespace hrconnectbackend.Models
{
    public class Department
    {
        public int DepartmentId { get; set; }
        public Guid DepartmentGuid { get; set; } = Guid.NewGuid(); // Generate a new GUID
        public int TenantId { get; set; }
        public required string DeptName { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; } = null;
    }
}
