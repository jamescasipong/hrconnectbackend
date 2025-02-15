namespace hrconnectbackend.Models.DTOs
{
    public class CreateNotificationDTO
    {
        public string Title { get; set; }
        public string Message { get; set; }
    }

    public class ReadNotificationsDTO
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public DateTime Date { get; set; }
        public bool IsRead { get; set; }

    }

    public class CreateNotificationHubDTO
    {
        public int EmployeeId { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
    }
}
