using System.ComponentModel.DataAnnotations;

namespace hrconnectbackend.Models;

public class RefreshToken
{
    [Key]
    [Required]
    public string RefreshTokenId { get; set; }
    [Required]
    public int UserId { get; set; }
    [Required]
    public string CookieName { get; set; }
    [Required]
    public DateTime CreateAt { get; set; } = DateTime.Now;
    [Required]
    public DateTime Expires { get; set; }
    [Required]
    public bool IsActive { get; set; } = false;
    public UserAccount UserAccount { get; set; }
}