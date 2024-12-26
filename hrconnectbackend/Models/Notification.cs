namespace hrconnectbackend.Models;

public class Notification
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string Title { get; set; }
    public string Message { get; set; }
    public string Type { get; set; }
    public bool IsRead { get; set; }
    public DateOnly DateCreated { get; set; }
}