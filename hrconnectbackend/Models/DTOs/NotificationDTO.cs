namespace hrconnectbackend.Models.DTOs
{
    public class CreateNotificationDto
    {
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }

    public class ReadNotificationsDto
    {
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

    }

    public class CreateNotificationHubDto
    {
        public int EmployeeId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }


    public class ReadUserNotificationDto 
    {
        public int EmployeeId { get; set; }
        public int NotificationId { get; set; }
        public bool IsRead { get; set; }
        public string Status { get; set; } = string.Empty;
        public ReadNotificationsDto? Notification { get; set; }
    }

    public class CreateUserNotificationDto
    {
        public int EmployeeId { get; set; }
        public int NotificationId { get; set; }
        public bool IsRead { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
