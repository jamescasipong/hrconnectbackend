using System.ComponentModel.DataAnnotations;

namespace hrconnectbackend.Models.DTOs
{
    public class ShiftDTO
    {
        [Required]
        public int EmployeeShiftId { get; set; }
        [Required]
        public string DaysOfWorked { get; set; } = string.Empty;
        [Required]
        public string TimeIn { get; set; } = string.Empty;
        [Required]
        public string TimeOut { get; set; } = string.Empty;
        
    }
}
