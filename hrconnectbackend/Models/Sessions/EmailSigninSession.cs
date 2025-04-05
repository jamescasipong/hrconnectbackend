using System;
using System.ComponentModel.DataAnnotations;
using hrconnectbackend.Models.Sessions;

namespace hrconnectbackend.Models
{
    public class EmailSigninSession: Session
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string UserAgent { get; set; }

        [Required]
        public string IpAddress { get; set; }

        [Required]
        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;

    }
}