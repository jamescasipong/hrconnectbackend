using System.ComponentModel.DataAnnotations;
using hrconnectbackend.Models.EmployeeModels;
using hrconnectbackend.Models.RequestModel;
using hrconnectbackend.Models.Sessions;

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
        public int OrganizationId { get; set; } // Foreign Key, nullable
        public bool ChangePassword { get; set; } = false;
        public UserRole? Role { get; set; }

        // public static UserAccount CreateUserAccount(CreateUser user)
        // {
        //     return new UserAccount
        //     {
        //         UserName = user.UserName,
        //         Email = user.Email,
        //         Password = user.Password,
        //         EmailVerified = false,
        //         SmsVerified = false,
        //         OrganizationId = user.OrganizationId,
        //         Role = user.role
        //     };
        // }

        public string GetRoleAsString()
        {
            return Role.ToString() ?? "N/A";
        }
        public List<RefreshToken>? RefreshTokens { get; set; }
        

        // Make the Organization navigation property nullable
        public Employee? Employee { get; set; }
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