using System.ComponentModel.DataAnnotations;

namespace hrconnectbackend.Models;

public class Notifications
{
    [Key]
    public int Id { get; set; }
    [Required]
    public int EmployeeId { get; set; }

    [Required]
    public string Title { get; set; }
    [Required]
    public string Message { get; set; }
    [Required]
    public DateTime Date { get; set; }

    public Employee Employee { get; set; }

    public List<UserNotification> UserNotification { get; set; }
}