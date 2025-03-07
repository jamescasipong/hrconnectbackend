namespace hrconnectbackend.Models
{
    public class Department
    {
        public int DepartmentId { get; set; }
        public int ManagerId { get; set; }
        public string DeptName { get; set; }
        public List<Employee>? Employees { get; set; }
        public Supervisor? Supervisor { get; set; } // Foreign Key referencing to User's Id
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
