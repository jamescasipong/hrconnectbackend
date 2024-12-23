using hrconnectbackend.Models;
using hrconnectbackend.Models.Enums;

namespace hrconnectbackend.Models
{
    public class OTApplication
    {

        public int OTApplicationId { get; set; }
        public int EmployeeId { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly AppliedDate { get; set; } = new DateOnly();
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public string Reasons { get; set; }
        public string Status { get; set; } = RequestStatus.Pending.ToString();
        public Employee Employee { get; set; }
        public OTApproval OTApproval { get; set; }
    }
}
