using System.ComponentModel.DataAnnotations;

namespace hrconnectbackend.Models.DTOs
{
    public class CreateDepartmentDto
    {
        [Required(ErrorMessage = "Department name is required")]
        public string Description { get; set; } = string.Empty;
        public string DeptName { get; set; } = string.Empty;
    }

    public class ReadDepartmentDto
    {
        public int DepartmentId { get; set; }
        public string Description { get; set; } = string.Empty;
        public string DeptName { get; set; } = string.Empty;
        public List<ReadEmployeeDto>? Employees { get; set; }
    }
}
