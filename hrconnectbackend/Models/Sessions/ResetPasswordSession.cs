using System.ComponentModel.DataAnnotations;

namespace hrconnectbackend.Models.Sessions
{
    public class ResetPasswordSession
    {
        [Key]
        public string Token { get; set; } = string.Empty;
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
    }
}
