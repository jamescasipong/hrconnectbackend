using hrconnectbackend.Models.Enums;

namespace hrconnectbackend.Models.DTOs
{
    public class CreateEmployeeDTO
    {
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string? Password { get; set; }
        public bool IsAdmin { get; set; } = false;
        public string Status { get; set; } = StatusType.Offline.ToString();
    }

    public class ReadEmployeeDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public bool IsAdmin { get; set; } = false;
        public string Status { get; set; } = RequestStatus.Pending.ToString();
        public DateOnly CreatedAt { get; set; } = new DateOnly(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
        public DateOnly UpdatedAt { get; set; } = new DateOnly(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
        public int? SupervisorId { get; set; } = null; // Foreign Key
        public int? DepartmentId { get; set; } = null;
    }

    public class UpdateEmployeeDTO
    {

        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Status { get; set; } = StatusType.Offline.ToString();
        public int? SupervisorId { get; set; } // Foreign Key
        public int? DepartmentId { get; set; }

    }
}
