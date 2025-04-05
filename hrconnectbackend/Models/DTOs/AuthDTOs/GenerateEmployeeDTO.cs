using System.ComponentModel.DataAnnotations;

namespace hrconnectbackend.Models.DTOs.AuthDTOs
{
    public class GenerateEmployeeDto
    {
        [EmailAddress]
        public string Email { get; set; } = String.Empty;
    }
}
