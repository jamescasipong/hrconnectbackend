namespace hrconnectbackend.Interface.Services.ExternalServices;

public interface IEmailServices
{
    Task SendEmailAsync(string toEmail, string subject, string body);
    Task SendConfirmationEmailAsync(string toEmail, string token);
    Task SendResetPasswordEmailAsync(string toEmail, string token);
    Task SendPasswordChangedEmailAsync(string toEmail);
    Task SendPasswordResetSuccessEmailAsync(string toEmail);
    Task SendAuthenticationCodeAsync(string toEmail, string code, DateTime expiryTime);
}