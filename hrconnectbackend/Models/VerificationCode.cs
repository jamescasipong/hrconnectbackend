namespace hrconnectbackend.Models;

public class VerificationCode
{
    public int Id { get; set; }           // Unique identifier
    public int UserId { get; set; }       // The User ID to whom the verification code belongs
    public string Code { get; set; } = string.Empty;      // The actual verification code
    public DateTime ExpiryDate { get; set; } // Expiry date of the code
    public DateTime CreatedAt { get; set; }  // When the code was created
    public bool IsUsed { get; set; }      // If the code has been used
}
