using System.ComponentModel.DataAnnotations;
using hrconnectbackend.Exceptions;
using hrconnectbackend.Models.EmployeeModels;
using hrconnectbackend.Models.RequestModel;
using hrconnectbackend.Models.Sessions;
using SharpCompress.Common;

namespace hrconnectbackend.Models
{
    public class UserAccount
    {
        [Key]
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool EmailVerified { get; set; } = false;
        public bool SmsVerified { get; set; } = false;
        public int? OrganizationId { get; set; } // Foreign Key, nullable
        public bool ChangePassword { get; set; } = false;
        public string Role { get; set; } = string.Empty; // Nullable role property
        public bool Locked { get; set; } = false;
        public int NumberOfAttempts { get; set; } = 0;
    
        public List<RefreshToken>? RefreshTokens { get; set; }
        public Employee? Employee { get; set; }
        public UserSettings? UserSettings { get; set; }
        public Organization? Organization { get; set; } // Optional relationship
        public UserPermission? UserPermission { get; set; }
    }
}
