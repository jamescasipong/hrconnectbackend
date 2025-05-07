using hrconnectbackend.Models.Enums;
using hrconnectbackend.Models.Requests;
using System.ComponentModel.DataAnnotations.Schema;

namespace hrconnectbackend.Models.DTOs
{
    public class CreateEmployeeDto
    {
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? Password { get; set; }
    }

    public class ReadEmployeeDto
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string PositionId { get; set; } = string.Empty;
        public bool IsAdmin { get; set; } = false;
        public string Status { get; set; } = RequestStatus.Pending.ToString();
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int? EmployeeDepartmentId { get; set; } = null;
        public ReadAboutEmployeeDto? AboutEmployee { get; set; }
    }

    public class UpdateEmployeeDto
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Status { get; set; } = StatusType.Offline.ToString();
        public int? SupervisorId { get; set; } // Foreign Key
        public int? DepartmentId { get; set; }

    }
}
