using DnsClient;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.VisualBasic;
using System;
using System.Diagnostics.Metrics;
using System.Drawing;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using hrconnectbackend.Config;
using hrconnectbackend.Interface.Services.ExternalServices;
using Microsoft.CodeAnalysis.Options;
using Microsoft.Extensions.Options;
using static Org.BouncyCastle.Crypto.Engines.SM2Engine;
using static System.Net.Mime.MediaTypeNames;

namespace hrconnectbackend.Services.ExternalServices
{
    public class EmailService(IOptions<SmtpSettings> smtpSettings, ILogger<EmailService> logger)
        : IEmailServices
    {
        private readonly SmtpSettings _smtpSettings = smtpSettings.Value;

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            logger.LogInformation($"Sending email to: {toEmail}");
            
            try
            {
                var smtpClient = new SmtpClient(_smtpSettings.Server)
                {
                    Port = _smtpSettings.SmtpPort,
                    Credentials = new NetworkCredential(_smtpSettings.Username, _smtpSettings.Password),
                    EnableSsl = true,
                    UseDefaultCredentials = false,
                };


                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_smtpSettings.Username),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true,
                };

                mailMessage.To.Add(toEmail);

                await smtpClient.SendMailAsync(mailMessage);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error sending email to: {toEmail}");
                
                throw new Exception(ex.Message);
                // Handle exception
            }
        }

        public async Task SendConfirmationEmailAsync(string toEmail, string token)
        {
            var subject = "Confirm your email address";
            var body = $@"
                <h1>Confirm your email address</h1>
                <p>Click the link below to confirm your email address:</p>
                <p><a href='http://localhost:3000/confirm-email?token={token}'>Confirm email address</a></p>";

            await SendEmailAsync(toEmail, subject, body);
        }
        

        public async Task SendResetPasswordEmailAsync(string toEmail, string token)
        {
            var companyName = "HR Management System";
            var companyLogo = "https://hrconnect.vercel.app/hrlogo.png"; // Replace with your actual logo URL
            var resetLink = $"http://localhost:3000/reset-password?token={token}&email={Uri.EscapeDataString(toEmail)}";
            var expiryHours = 24; // Set this to match your actual token expiry time
    
            var subject = $"Reset Your {companyName} Password";
            var body = $@"
        <!DOCTYPE html>
        <html lang='en'>
        <head>
            <meta charset='UTF-8'>
            <meta name='viewport' content='width=device-width, initial-scale=1.0'>
            <title>Reset Your Password</title>
        </head>
        <body style='margin: 0; padding: 0; font-family: Arial, sans-serif; background-color: #f7f7f7; color: #333333;'>
            <table role='presentation' width='100%' cellspacing='0' cellpadding='0' border='0'>
                <tr>
                    <td style='padding: 20px 0; text-align: center; background-color: #f7f7f7;'>
                        <table role='presentation' width='600' cellspacing='0' cellpadding='0' border='0' align='center' style='margin: auto; background-color: #ffffff; border-radius: 8px; box-shadow: 0 2px 8px rgba(0, 0, 0, 0.05);'>
                            <!-- Header -->
                            <tr>
                                <td style='padding: 30px 40px; text-align: center; background-color: #4f46e5; border-radius: 8px 8px 0 0;'>
                                    <img src='{companyLogo}' alt='{companyName}' width='150' style='max-width: 150px; height: auto;'>
                                </td>
                            </tr>
                    
                            <!-- Content -->
                            <tr>
                                <td style='padding: 40px 40px 20px 40px;'>
                                    <h1 style='margin: 0 0 20px 0; font-size: 24px; line-height: 32px; font-weight: 700; color: #111827;'>Reset Your Password</h1>
                                    <p style='margin: 0 0 24px 0; font-size: 16px; line-height: 24px; color: #4b5563;'>We received a request to reset your password for your {companyName} account. Click the button below to create a new password:</p>
                            
                                    <!-- CTA Button -->
                                    <table role='presentation' width='100%' cellspacing='0' cellpadding='0' border='0'>
                                        <tr>
                                            <td style='padding: 10px 0 30px 0; text-align: center;'>
                                                <a href='{resetLink}' target='_blank' style='display: inline-block; padding: 14px 28px; background-color: #4f46e5; color: #ffffff; text-decoration: none; font-weight: 600; font-size: 16px; border-radius: 6px; transition: background-color 0.2s ease-in-out;'>Reset Password</a>
                                            </td>
                                        </tr>
                                    </table>
                            
                                    <p style='margin: 0 0 16px 0; font-size: 16px; line-height: 24px; color: #4b5563;'>If the button doesn't work, copy and paste this link into your browser:</p>
                                    <p style='margin: 0 0 24px 0; font-size: 14px; line-height: 22px; color: #6b7280; word-break: break-all;'><a href='{resetLink}' style='color: #4f46e5; text-decoration: underline;'>{resetLink}</a></p>
                            
                                    <p style='margin: 0 0 24px 0; font-size: 16px; line-height: 24px; color: #4b5563;'>This password reset link will expire in {expiryHours} hours.</p>
                            
                                    <p style='margin: 0 0 8px 0; font-size: 16px; line-height: 24px; color: #4b5563;'>If you didn't request a password reset, you can safely ignore this email.</p>
                                </td>
                            </tr>
                    
                            <!-- Security Notice -->
                            <tr>
                                <td style='padding: 0 40px 30px 40px;'>
                                    <table role='presentation' width='100%' cellspacing='0' cellpadding='0' border='0' style='border-top: 1px solid #e5e7eb; padding-top: 20px;'>
                                        <tr>
                                            <td>
                                                <p style='margin: 0 0 16px 0; font-size: 14px; line-height: 22px; color: #6b7280;'><strong>Security Tip:</strong> {companyName} will never ask you for your password or personal information via email.</p>
                                            </td>
                                        </tr>
                                    </table>
                                </td>
                            </tr>
                    
                            <!-- Footer -->
                            <tr>
                                <td style='padding: 20px 40px; text-align: center; background-color: #f9fafb; border-radius: 0 0 8px 8px; border-top: 1px solid #e5e7eb;'>
                                    <p style='margin: 0 0 10px 0; font-size: 14px; line-height: 22px; color: #6b7280;'>Need help? Contact our support team at <a href='mailto:support@your-domain.com' style='color: #4f46e5; text-decoration: underline;'>support@your-domain.com</a></p>
                                    <p style='margin: 0; font-size: 12px; line-height: 20px; color: #9ca3af;'>&copy; {DateTime.Now.Year} {companyName}. All rights reserved.</p>
                                </td>
                            </tr>
                        </table>
                    </td>
                </tr>
            </table>
        </body>
        </html>";

            await SendEmailAsync(toEmail, subject, body);
        }



        public async Task SendPasswordChangedEmailAsync(string toEmail)
        {
            var subject = "Your password has been changed";
            var body = $@"
                <h1>Your password has been changed</h1>
                <p>If you didn't change your password, please contact us immediately.</p>";

            await SendEmailAsync(toEmail, subject, body);
        }

        public async Task SendPasswordResetSuccessEmailAsync(string toEmail)
        {
            var subject = "Your password has been reset";
            var body = $@"
                <h1>Your password has been reset</h1>
                <p>Your password has been successfully reset. If you didn't request this, please contact us immediately.</p>";

            await SendEmailAsync(toEmail, subject, body);
        }

        public async Task SendAuthenticationCodeAsync(string toEmail, string code, DateTime expiryTime)
        {
            int minutes = (int)Math.Ceiling(expiryTime.Subtract(DateTime.Now).TotalMinutes);

            var subject = "Your HRConnect Authentication Code";
            var styledBody = $@"
    <!DOCTYPE html>
    <html lang=""en"">
    <head>
        <meta charset=""UTF-8"">
        <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
        <title>Authentication Code</title>
    </head>
    <body style=""margin: 0; padding: 0; font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; background-color: #f7f9fc;"">
        <table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"" style=""max-width: 600px; margin: 0 auto; background-color: #ffffff; border-radius: 12px; overflow: hidden; box-shadow: 0 4px 12px rgba(0,0,0,0.05); margin-top: 40px; margin-bottom: 40px;"">
            <!-- Header -->
            <tr>
                <td style=""padding: 30px 0; text-align: center; background-color: #4f46e5; background-image: linear-gradient(135deg, #4f46e5 0%, #7c3aed 100%);"">
                    <img src=""https://hrconnect.vercel.app/hrlogo.png"" alt=""HRConnect"" width=""180"" style=""display: block; margin: 0 auto;"">
                    <h1 style=""color: #ffffff; font-size: 24px; font-weight: 600; margin: 20px 0 0 0;"">Authentication Code</h1>
                </td>
            </tr>
            
            <!-- Content -->
            <tr>
                <td style=""padding: 40px 30px;"">
                    <p style=""font-size: 16px; line-height: 1.6; color: #4b5563; margin-top: 0;"">Hello,</p>
                    <p style=""font-size: 16px; line-height: 1.6; color: #4b5563;"">We received a request to access your HRConnect account. Please use the verification code below to complete your sign-in:</p>
                    
                    <!-- Code Box -->
                    <div style=""background-color: #f3f4f6; border-radius: 8px; padding: 20px; margin: 30px 0; text-align: center;"">
                        <div style=""letter-spacing: 8px; font-size: 32px; font-weight: 700; color: #1f2937; font-family: monospace;"">{code}</div>
                    </div>
                    
                    <p style=""font-size: 16px; line-height: 1.6; color: #4b5563;"">This code will expire in <strong>{minutes} minutes</strong>.</p>
                    <p style=""font-size: 16px; line-height: 1.6; color: #4b5563;"">If you didn't request this code, please ignore this email or contact support if you have concerns.</p>
                </td>
            </tr>
            
            <!-- Security Notice -->
            <tr>
                <td style=""padding: 0 30px 30px 30px;"">
                    <div style=""padding: 20px; background-color: #fffbeb; border-left: 4px solid #f59e0b; border-radius: 4px;"">
                        <p style=""font-size: 14px; line-height: 1.5; color: #92400e; margin: 0;"">
                            <strong>Security Tip:</strong> HRConnect will never ask for your password or personal information via email.
                        </p>
                    </div>
                </td>
            </tr>
            
            <!-- Footer -->
            <tr>
                <td style=""background-color: #f9fafb; padding: 30px; text-align: center; border-top: 1px solid #e5e7eb;"">
                    <p style=""font-size: 14px; color: #6b7280; margin: 0 0 10px 0;"">� 2025 HRConnect. All rights reserved.</p>
                    <p style=""font-size: 14px; color: #6b7280; margin: 0;"">
                        <a href=""https://hrconnect.vercel.app/terms"" style=""color: #6b7280; text-decoration: underline;"">Privacy Policy</a> � 
                        <a href=""https://hrconnect.vercel.app/terms"" style=""color: #6b7280; text-decoration: underline;"">Terms of Service</a> � 
                        <a href=""https://hrconnect.vercel.app/contact-us"" style=""color: #6b7280; text-decoration: underline;"">Contact Support</a>
                    </p>
                </td>
            </tr>
        </table>
    </body>
    </html>";

            await SendEmailAsync(toEmail, subject, styledBody);
        }



    }
}