namespace hrconnectbackend.Models.DTOs
{
    public class CreateNotificationDTO
    {
        public string Title { get; set; }
        public string Message { get; set; }
    }

    public class ReadNotificationsDTO
    {
        public string Title { get; set; }
        public string Message { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

    }

    public class CreateNotificationHubDTO
    {
        public int EmployeeId { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
    }


    public class ReadUserNotificationDTO 
    {
        public int EmployeeId { get; set; }
        public int NotificationId { get; set; }
        public bool IsRead { get; set; }
        public string Status { get; set; }
        public ReadNotificationsDTO Notification { get; set; }
    }

    public class CreateUserNotificationDTO
    {
        public int EmployeeId { get; set; }
        public int NotificationId { get; set; }
        public bool IsRead { get; set; }
        public string Status { get; set; }
    }
}
