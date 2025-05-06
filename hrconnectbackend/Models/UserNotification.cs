using hrconnectbackend.Models.EmployeeModels;

namespace hrconnectbackend.Models
{
    public class UserNotification
    {
        public int EmployeeId { get; set; }
        public int NotificationId { get; set; }
        public Guid ReferenceId { get; set; } = Guid.NewGuid();
        public bool IsRead { get; set; }
        public string Status { get; set; } = string.Empty;
        public Employee? Employee { get; set; }
        public Notifications? Notification { get; set; }
    }
}
