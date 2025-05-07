using System.ComponentModel.DataAnnotations;

namespace hrconnectbackend.Models;

public class Notifications
{
    [Key]
    public int Id { get; set; }
    [Required]
    public string Title { get; set; }  = string.Empty;
    [Required]
    public string Message { get; set; }  = string.Empty;
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? UpdatedAt { get; set; } = null;
    public int OrganizationId { get; set; }

    public Organization? Organization { get; set; }
    public List<UserNotification>? UserNotification { get; set; }
}