using hrconnectbackend.Models.EmployeeModels;
using hrconnectbackend.Models.Sessions;

namespace hrconnectbackend.Models
{
    public class UserAccount
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public bool EmailVerified { get; set; } = false;
        public bool SmsVerified { get; set; } = false;
        public int OrganizationId { get; set; } // Foreign Key, nullable
        public bool ChangePassword { get; set; } = false;
        public UserRole Role { get; set; }

        public string GetRoleAsString()
        {
            return Role.ToString(); // Returns "Admin", "User", or "Manager"
        }
        public List<RefreshToken>? RefreshTokens { get; set; }
        

        // Make the Organization navigation property nullable
        public Employee? Employee { get; set; }
        public UserRole UserRole { get; set; }
        public UserSettings? UserSettings { get; set; }
        public Organization? Organization { get; set; } // Optional relationship
        public UserPermission? UserPermission { get; set; }
    }
}

public enum UserRole
{
    Admin,
    Employee,
}