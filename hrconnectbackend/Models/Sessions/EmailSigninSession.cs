using System.ComponentModel.DataAnnotations;

namespace hrconnectbackend.Models.Sessions
{
    public class EmailSigninSession: Session
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string UserAgent { get; set; } = string.Empty;

        [Required]
        public string IpAddress { get; set; } = string.Empty;

        [Required]
        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;

    }
}