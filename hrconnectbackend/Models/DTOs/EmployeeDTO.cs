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
    }

    public class ReadEmployeeDTO
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Position { get; set; }
        public bool IsAdmin { get; set; } = false;
        public string Status { get; set; } = RequestStatus.Pending.ToString();
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int? SupervisorId { get; set; } = null; // Foreign Key
        public int? DepartmentId { get; set; } = null;
        public ReadAboutEmployeeDTO AboutEmployee { get; set; }
        public ReadDepartmentDTO Department { get; set; }
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
