using hrconnectbackend.Models.Enums;

namespace hrconnectbackend.Models.DTOs;

public class LeavesApprovalDTO
{
    public int LeaveApprovalId { get; set; }
    public int ApproverId { get; set; }
    public DateOnly ApprovedDate { get; set; }
    public string Decision { get; set; } = RequestStatus.Pending.ToString();
    public ICollection<LeaveApplication> LeaveApplications { get; set; }
}