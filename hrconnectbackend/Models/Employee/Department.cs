namespace hrconnectbackend.Models
{
    public class Department
    {
        public int DepartmentId { get; set; }
        public int ManagerId { get; set; }
        public string DeptName { get; set; }
        public Employee Employee { get; set; } // Foreign Key referencing to User's Id
    }
}
