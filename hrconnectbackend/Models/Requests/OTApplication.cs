using hrconnectbackend.Models;
using hrconnectbackend.Models.Enums;

namespace hrconnectbackend.Models
{
    public class OTApplication
    {

        public int OTApplicationId { get; set; }
        public int EmployeeId { get; set; }
        public DateTime Date { get; set; }
        public int? SupervisorId { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public string Reasons { get; set; }
        public string Status { get; set; } = RequestStatus.Pending.ToString();
        public Employee Employee { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; } = null;
    }
}
