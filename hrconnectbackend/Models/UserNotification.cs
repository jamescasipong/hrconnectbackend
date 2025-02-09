namespace hrconnectbackend.Models
{
    public class UserNotification
    {
        public int EmployeeId { get; set; }
        public int NotificationId { get; set; }
        public bool IsRead { get; set; }
        public Employee Employee { get; set; }
        public Notifications Notification { get; set; }
    }
}
