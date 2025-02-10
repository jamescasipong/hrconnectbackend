using hrconnectbackend.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace hrconnectbackend.Models.DTOs
{
    public class CreateLeaveApplicationDTO
    {
        [Required(ErrorMessage = "Employee ID is required")]
        public int EmployeeId { get; set; }
        public int? SupervisorId { get; set; }
        [Required(ErrorMessage = "Leave type is required")]
        public string Type { get; set; }
        [Required(ErrorMessage = "Start date is required")]
        public string StartDate { get; set; }
        [Required(ErrorMessage = "Reason is required")]
        public string Reason { get; set; }
    }

    public class ReadLeaveApplicationDTO
    {
        public int EmployeeId { get; set; }
        public int? SupervisorId { get; set; }
        public string Type { get; set; } = LeaveType.Sick.ToString();
        public DateOnly StartDate { get; set; }
        public DateOnly AppliedDate { get; set; } = new DateOnly();
        public string Reason { get; set; }
        public string Status { get; set; } = RequestStatus.Pending.ToString();

    }

    public class UpdateLeaveApplicationDTO
    {
        public string Type { get; set; }
        public string StartDate { get; set; }
        public string Reason { get; set; }
        public string Status { get; set; }

    }
}
