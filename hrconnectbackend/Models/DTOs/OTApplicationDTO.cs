using hrconnectbackend.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace hrconnectbackend.Models.DTOs
{
    public class CreateOTApplicationDTO
    {
        [Required(ErrorMessage = "EmployeeId is required")]
        public int EmployeeId { get; set; }
        [Required(ErrorMessage = "StartDate is required")]
        public DateTime Date { get; set; }
        public int? SupervisorId { get; set; }
        [Required(ErrorMessage = "EndTime is required")]
        public TimeOnly EndTime { get; set; }
        [Required(ErrorMessage = "Reason is required")]
        public string Reasons { get; set; }
    }

    public class ReadOTApplicationDTO
    {
        public int EmployeeId { get; set; }
        public DateTime StartDate { get; set; }
        public int? SupervisorId { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public string Reasons { get; set; }
        public string Status { get; set; } = RequestStatus.Pending.ToString();
    }

    public class UpdateOTApplicationDTO
    {
        [Required(ErrorMessage = "StartDate is required")]
        public DateTime Date { get; set; }
        [Required(ErrorMessage = "StartTime is required")]
        public TimeOnly StartTime { get; set; }
        [Required(ErrorMessage = "EndTime is required")]
        public TimeOnly EndTime { get; set; }
        [Required(ErrorMessage = "Reasons is required")]
        public string Reasons { get; set; }
        [Required(ErrorMessage = "Status is required")]
        public string Status { get; set; } = RequestStatus.Pending.ToString();
    }
}
