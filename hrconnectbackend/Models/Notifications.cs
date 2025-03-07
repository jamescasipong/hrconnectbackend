using System.ComponentModel.DataAnnotations;

namespace hrconnectbackend.Models;

public class Notifications
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string Title { get; set; }
    [Required]
    public string Message { get; set; }
    public List<UserNotification> UserNotification { get; set; }
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? UpdatedAt { get; set; } = null;
}