namespace hrconnectbackend.Models.DTOs
{
    public class CreateDepartmentDTO
    {
        public int ManagerId { get; set; }
        public string DeptName { get; set; }
    }

    public class ReadDepartmentDTO
    {
        public int DepartmentId { get; set; }
        public int ManagerId { get; set; }
        public string DeptName { get; set; }
        public ReadDepartmentDTO Supervisor { get; set; }
        public List<ReadEmployeeDTO> Employees { get; set; }
    }
}
