using System.ComponentModel.DataAnnotations;
using hrconnectbackend.Models.Enums;

namespace hrconnectbackend.Models
{
    public class LeaveApplication
    {

        [Key]
        public int LeaveApplicationId { get; set; }
        public int EmployeeId { get; set; }
        public string Type { get; set; } = LeaveType.Sick.ToString();
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public DateOnly AppliedDate { get; set; } = new DateOnly();
        public string Reason { get; set; }
        public string Status { get; set; } = RequestStatus.Pending.ToString();
        public Employee? Employee { get; set; }
        public LeaveApproval? LeaveApproval { get; set; }

    }

}
