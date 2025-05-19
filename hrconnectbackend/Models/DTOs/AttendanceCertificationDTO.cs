using System.ComponentModel.DataAnnotations;

namespace hrconnectbackend.Models.DTOs
{
    public class CreateAttendanceCertificationDto
    {
        [Required(ErrorMessage = "Employee Id is required")]
        public int SupervisorId { get; set; }
        [Required(ErrorMessage = "Date is required")]
        public string Date { get; set; } = string.Empty;
        public string ClockIn { get; set; } = string.Empty;
        public string ClockOut { get; set; } = string.Empty;
        public string? Reason { get; set; }
    }


    public class UpdateAttendanceCertificationDto
    {
        public string Status { get; set; } = string.Empty;
        public string Date { get; set; } = string.Empty;
        public int? SupervisorId { get; set; }
        public string ClockIn { get; set; } = string.Empty;
        public string ClockOut { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
    }
}
