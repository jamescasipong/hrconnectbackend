using System.Runtime.Serialization;
using hrconnectbackend.Models.Enums;

namespace hrconnectbackend.Models.DTOs
{
    public class CreateEmployeeDTO
    {


        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Status { get; set; } = StatusType.Offline.ToString();


    }
}
