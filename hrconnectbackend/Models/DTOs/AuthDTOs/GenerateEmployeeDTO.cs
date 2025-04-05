using System.ComponentModel.DataAnnotations;

namespace hrconnectbackend.Models.DTOs.AuthDTOs
{
    public class GenerateEmployeeDTO
    {
        [EmailAddress]
        public string Email { get; set; }
    }
}
