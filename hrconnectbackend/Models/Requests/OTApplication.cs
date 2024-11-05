namespace hrconnectbackend.Models
{
    public class OTApplication
    {
        public enum Status
        {
            Pending = 0,
            Approved = 1,
            Rejected = 2
        }
        public int OTApplicationId { get; set; }
        public int EmployeeId { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly AppliedDate { get; set; } = new DateOnly();
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public string Reasons { get; set; }
        public Status status { get; set; } = Status.Pending;
        public Employee Employee { get; set; }
        public OTApproval OTApproval { get; set; }
    }
}
