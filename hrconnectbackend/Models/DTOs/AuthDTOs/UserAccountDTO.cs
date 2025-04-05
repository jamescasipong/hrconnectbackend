namespace hrconnectbackend.Models.DTOs.AuthDTOs
{
    public class UserAccountDto
    {
        public int UserId { get; set; }
        public int? VerificationCode { get; set; }
        public bool IsAuthenticated { get; set; } = false;
        public bool EmailVerified { get; set; } = false;
        public bool SMSVerified { get; set; } = false;
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Phone { get; set; }
    }
}
