using hrconnectbackend.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace hrconnectbackend.Models.DTOs
{
    public class CreateLeaveApplicationDto
    {
        [Required(ErrorMessage = "Employee ID is required")]
        public int EmployeeId { get; set; }
        public int? SupervisorId { get; set; }
        [Required(ErrorMessage = "Leave type is required")]
        public string Type { get; set; } = string.Empty;
        [Required(ErrorMessage = "Start date is required")]
        public string StartDate { get; set; } = string.Empty;
        [Required(ErrorMessage = "Reason is required")]
        public string Reason { get; set; } = string.Empty;
    }

    public class ReadLeaveApplicationDto
    {
        public int EmployeeId { get; set; }
        public int? SupervisorId { get; set; }
        public string Type { get; set; } = LeaveType.Sick.ToString();
        public DateOnly StartDate { get; set; }
        public DateOnly AppliedDate { get; set; } = new DateOnly();
        public string Reason { get; set; } = string.Empty;
        public string Status { get; set; } = RequestStatus.Pending.ToString();
    }

    public class UpdateLeaveApplicationDto
    {
        public string Type { get; set; } = string.Empty;
        public string StartDate { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;

    }
}
