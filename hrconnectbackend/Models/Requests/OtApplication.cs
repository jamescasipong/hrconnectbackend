using hrconnectbackend.Models.EmployeeModels;
using hrconnectbackend.Models.Enums;

namespace hrconnectbackend.Models.Requests
{
    public class OtApplication
    {
        public int OtApplicationId { get; set; }
        public int EmployeeId { get; set; }
        public int SupervisorId { get; set; }
        public DateTime Date { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public string Reasons { get; set; } = string.Empty;
        public string Status { get; set; } = RequestStatus.Pending.ToString();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public Employee? Employee { get; set; }
        public DateTime? UpdatedAt { get; set; } = null;
        
    }
}
