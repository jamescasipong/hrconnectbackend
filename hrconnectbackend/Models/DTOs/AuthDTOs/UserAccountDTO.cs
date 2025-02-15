namespace hrconnectbackend.Models.DTOs
{
    public class UserAccountDTO
    {
        public int UserId { get; set; }
        public int? VerificationCode { get; set; }
        public bool IsAuthenticated { get; set; } = false;
        public bool EmailVerified { get; set; } = false;
        public bool SMSVerified { get; set; } = false;
        public string UserName { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
    }
}
