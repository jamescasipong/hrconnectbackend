using hrconnectbackend.Models.Enums;

namespace hrconnectbackend.Models
{
    public class OTApproval
    {

        public int OTApprovalId { get; set; }
        public int SupervisorId { get; set; }
        public int OTApplicationId { get; set; }
        public DateOnly ApprovedDate { get; set; }
        public string Decision { get; set; } = RequestStatus.Pending.ToString();
        public Supervisor Supervisor { get; set; }
        public OTApplication OTApplication { get; set; }
    }
}
