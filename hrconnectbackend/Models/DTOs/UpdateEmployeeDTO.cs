namespace hrconnectbackend.Models.DTOs
{
    public class UpdateEmployeeDTO
    {
        public enum Status
        {
            offline = 0,
            online = 1,
        }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public bool IsAdmin { get; set; } = false;
        public Status status { get; set; } = Status.offline;
        public DateOnly CreatedAt { get; set; } = new DateOnly(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);

        public DateOnly UpdatedAt { get; set; } = new DateOnly(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
        public int? SupervisorId { get; set; } // Foreign Key
        public int? DepartmentId { get; set; }

    }
}
