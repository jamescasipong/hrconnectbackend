using System.ComponentModel.DataAnnotations;

namespace hrconnectbackend.Models.Sessions
{
    public class ResetPasswordSession
    {
        [Key]
        public string Token { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        public DateTime ExpiresAt { get; set; }
    }
}
