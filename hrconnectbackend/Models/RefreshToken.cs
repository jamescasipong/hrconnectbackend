using System.ComponentModel.DataAnnotations;

namespace hrconnectbackend.Models;

public class RefreshToken
{
    [Key]
    [Required]
    public required string RefreshTokenId { get; set; }
    [Required]
    public int UserId { get; set; }
    [Required]
    public string CookieName { get; set; } = string.Empty;
    [Required]
    public DateTime CreateAt { get; set; } = DateTime.UtcNow;
    [Required]
    public DateTime Expires { get; set; } = DateTime.UtcNow;
    [Required]
    public bool IsActive { get; set; } = false;
    public UserAccount? UserAccount { get; set; }
    
}