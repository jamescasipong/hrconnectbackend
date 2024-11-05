namespace hrconnectbackend.Models.DTOs
{
    public class EmployeeDTO
    {
        public enum Status
        {
            offline = 0,
            online = 1,
        }
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public bool IsAdmin { get; set; }
        public Status status { get; set; } = Status.offline;
        public DateOnly CreatedAt { get; set; } = new DateOnly();
        public DateOnly UpdatedAt { get; set; } = new DateOnly();
        public int? SupervisorId { get; set; } // Foreign Key
        public int? DepartmentId { get; set; }
        public Employee? Supervisor { get; set; }
        public Department? Department { get; set; }
    }
}
