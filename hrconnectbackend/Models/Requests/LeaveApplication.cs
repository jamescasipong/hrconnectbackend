using System.ComponentModel.DataAnnotations;

namespace hrconnectbackend.Models
{
    public class LeaveApplication
    {
        public enum LeaveType
        {
            Vacation = 0,
            Sick = 1,
            Leave = 2
        }
        public enum Status
        {
            Pending = 0,
            Approved = 1,
            Rejected = 2,
        }
        [Key]
        public int LeaveApplicationId { get; set; }

        public int EmployeeId { get; set; }
        public LeaveType leavelType {  get; set; } = LeaveType.Vacation;
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public DateOnly AppliedDate { get; set; } = new DateOnly();
        public string Reason { get; set; }
        public Status status { get; set; } = Status.Pending;
        public Employee Employee { get; set; }
        public LeaveApproval LeaveApproval { get; set; }
       

    }
}
