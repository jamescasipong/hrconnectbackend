using hrconnectbackend.Models.Enums;

namespace hrconnectbackend.Models.DTOs
{
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
