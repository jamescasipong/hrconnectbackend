namespace hrconnectbackend.Models
{
    public class LeaveApproval
    {
        public enum Decision
        {
            Approved = 0,
            Rejected = 1,
        }
        public int LeaveApprovalId { get; set; }
        public int ApproverId { get; set; }
        public DateOnly ApprovedDate { get; set; }
        public Decision decision { get; set; }
        public Employee Approver { get; set; }
        public LeaveApplication LeaveApplication { get; set; }

    }
}
