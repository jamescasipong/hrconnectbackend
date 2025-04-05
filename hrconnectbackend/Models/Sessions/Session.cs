using System.ComponentModel.DataAnnotations;

namespace hrconnectbackend.Models.Sessions;

public class Session
{
    [Key]
    [Required]
    public string SessionId { get; set; }
    [Required]
    public DateTime ExpiresAt { get; set; }
}