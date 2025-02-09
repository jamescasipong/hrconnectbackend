namespace hrconnectbackend.Models
{
    public class UserAccount
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public int? VerificationCode { get; set; }
        public bool EmailVerified { get; set; } = false;
        public bool SMSVerified { get; set; } = false;
        public Employee? Employee { get; set; } // Foreign Key referencing to User's Id
    }
}
