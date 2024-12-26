

using hrconnectbackend.Models.Enums;

namespace hrconnectbackend.Models
{
    public class LeaveApproval
    {
        public int LeaveApprovalId { get; set; }
        public int LeaveApplicationId { get; set; }
        public int SupervisorId { get; set; }
        public DateOnly? ApprovedDate { get; set; }
        public string Decision { get; set; } = RequestStatus.Pending.ToString();
        public Supervisor? Supervisor { get; set; }
        public LeaveApplication? LeaveApplication { get; set; }

    }
}
