using System.ComponentModel.DataAnnotations;

namespace hrconnectbackend.Models.DTOs
{
    public class CreateDepartmentDTO
    {
        public int? ManagerId { get; set; }
        [Required(ErrorMessage = "Department name is required")]
        public string Description { get; set; }
        public string DeptName { get; set; }
    }

    public class ReadDepartmentDTO
    {
        public int DepartmentId { get; set; }
        public int ManagerId { get; set; }
        public string Description { get; set; }
        public string DeptName { get; set; }
        public List<ReadEmployeeDTO> Employees { get; set; }
    }
}
