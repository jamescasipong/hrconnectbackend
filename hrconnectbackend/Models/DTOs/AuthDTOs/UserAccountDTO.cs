namespace hrconnectbackend.Models.DTOs
{
    public class UserAccountDTO
    {
        public int AuthEmpId { get; set; }
        public int VerificationCode { get; set; }
        public bool IsAuthenticated { get; set; } = false;
        public bool EmailConfirmed { get; set; } = false;
        public bool PhoneConfirmed { get; set; } = false;
    }
}
