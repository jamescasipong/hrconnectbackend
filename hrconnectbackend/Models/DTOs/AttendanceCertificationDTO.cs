using System.ComponentModel.DataAnnotations;

namespace hrconnectbackend.Models.DTOs
{
    public class CreateAttendanceCertificationDTO
    {
        [Required(ErrorMessage = "Employee Id is required")]
        public int EmployeeId { get; set; }
        public int SupervisorId { get; set; }
        [Required(ErrorMessage = "Date is required")]
        public string Date { get; set; }
        public string ClockIn { get; set; }
        public string ClockOut { get; set; }
        public string? Reason { get; set; }
    }


    public class UpdateAttendanceCertificationDTO
    {
        public string Status { get; set; }
        public string Date { get; set; }
        public int? SupervisorId { get; set; }
        public string ClockIn { get; set; }
        public string ClockOut { get; set; }
        public string Reason { get; set; }
    }
}
