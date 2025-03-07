using hrconnectbackend.Models;
using System.ComponentModel.DataAnnotations;

public class UserSettings
{
    [Required]
    public int EmployeeId { get; set; }

    [StringLength(10)]
    public string Language { get; set; } = "en";  // User's preferred language

    [StringLength(10)]
    public string Theme { get; set; } = "Light";

    public bool NotificationsEnabled { get; set; } = true;  // Whether notifications are enabled or not

    [StringLength(50)]
    public string Timezone { get; set; } = "UTC";  // User's timezone

    [StringLength(20)]
    public string DateFormat { get; set; } = "yyyy-MM-dd";  // User's date format preference

    [StringLength(10)]
    public string TimeFormat { get; set; } = "24h";  // User's time format preference

    [StringLength(20)]
    public string PrivacyLevel { get; set; } = "medium";  // User's privacy level (low, medium, high)

    // New 2FA Fields
    public bool IsTwoFactorEnabled { get; set; } = false;  // Whether 2FA is enabled for the user
    [StringLength(20)]
    public string TwoFactorMethod { get; set; }  = "email";  // Method of 2FA (email, sms, app)
    public string? TwoFactorSecret { get; set; } = null;  // Secret key for 2FA
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;  // Timestamp of when the settings were created
    public DateTime? UpdatedAt { get; set; } = null;  // Timestamp of when the settings were last updated

    public virtual Employee? Employee { get; set; }
}
