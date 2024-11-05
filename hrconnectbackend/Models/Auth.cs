namespace hrconnectbackend.Models
{
    public class Auth
    {
        public int AuthEmpId { get; set; }
        public int VerificationCode { get; set; }
        public bool IsAuthenticated { get; set; } = false;
        public bool EmailConfirmed { get; set; } = false;
        public bool PhoneConfirmed { get; set; } = false;
        public Employee Employee { get; set; } // Foreign Key referencing to User's Id
    }
}
