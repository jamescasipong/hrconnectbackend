namespace hrconnectbackend.Models
{
    public class OTApproval
    {
        public enum Decision
        {
            Approved = 0,
            Rejected = 1,
        }
        public int OTApprovalId { get; set; }
        public int ApproverId { get; set; }
        public DateOnly ApprovedDate { get; set; }
        public Decision decision { get; set; }
        public Employee Approver { get; set; }
        public OTApplication OTApplication { get; set; }
    }
}
