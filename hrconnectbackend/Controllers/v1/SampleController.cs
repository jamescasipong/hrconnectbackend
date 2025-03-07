// // Models - EmailVerification.Core/Models
// namespace EmailVerification.Core.Models
// {
//     public class User
//     {
//         public int Id { get; set; }
//         public string Email { get; set; }
//         public string PasswordHash { get; set; }
//         public bool IsEmailVerified { get; set; }
//         public DateTime CreatedAt { get; set; }
//         public DateTime? LastLoginAt { get; set; }
//     }

//     public class VerificationCode
//     {
//         public int Id { get; set; }
//         public string Email { get; set; }
//         public string Code { get; set; }
//         public VerificationPurpose Purpose { get; set; }
//         public DateTime CreatedAt { get; set; }
//         public DateTime ExpiresAt { get; set; }
//         public bool IsUsed { get; set; }
//     }

//     public enum VerificationPurpose
//     {
//         EmailVerification,
//         PasswordReset,
//         TwoFactorAuth
//     }

//     public class RefreshToken
//     {
//         public int Id { get; set; }
//         public int UserId { get; set; }
//         public string Token { get; set; }
//         public DateTime ExpiresAt { get; set; }
//         public bool IsRevoked { get; set; }
//         public string CreatedByIp { get; set; }
//         public DateTime CreatedAt { get; set; }
//     }
// }

// // DTOs - EmailVerification.Core/DTOs
// namespace EmailVerification.Core.DTOs
// {
//     // Authentication DTOs
//     public class RegisterRequest
//     {
//         public string Email { get; set; }
//         public string Password { get; set; }
//         public string ConfirmPassword { get; set; }
//     }

//     public class LoginRequest
//     {
//         public string Email { get; set; }
//         public string Password { get; set; }
//     }

//     public class VerifyEmailRequest
//     {
//         public string Email { get; set; }
//         public string Code { get; set; }
//     }

//     public class ForgotPasswordRequest
//     {
//         public string Email { get; set; }
//     }

//     public class ResetPasswordRequest
//     {
//         public string Email { get; set; }
//         public string Code { get; set; }
//         public string Password { get; set; }
//         public string ConfirmPassword { get; set; }
//     }

//     public class RefreshTokenRequest
//     {
//         public string Token { get; set; }
//     }

//     // Response DTOs
//     public class AuthResponse
//     {
//         public bool Success { get; set; }
//         public string Message { get; set; }
//         public string Token { get; set; }
//         public string RefreshToken { get; set; }
//         public bool IsEmailVerified { get; set; }
//         public UserDto User { get; set; }
//     }

//     public class UserDto
//     {
//         public int Id { get; set; }
//         public string Email { get; set; }
//         public bool IsEmailVerified { get; set; }
//         public DateTime CreatedAt { get; set; }
//     }

//     public class VerificationResponse
//     {
//         public bool Success { get; set; }
//         public string Message { get; set; }
//     }
// }

// // Interfaces - EmailVerification.Core/Interfaces
// namespace EmailVerification.Core.Interfaces
// {
//     // User Repository Interface
//     public interface IUserRepository
//     {
//         Task<User> GetByEmailAsync(string email);
//         Task<User> GetByIdAsync(int id);
//         Task CreateUserAsync(User user);
//         Task UpdateUserAsync(User user);
//     }

//     // Verification Code Repository Interface
//     public interface IVerificationCodeRepository
//     {
//         Task<VerificationCode> SaveCodeAsync(string email, string code, VerificationPurpose purpose, DateTime expiresAt);
//         Task<VerificationCode> GetCodeAsync(string email, VerificationPurpose purpose);
//         Task MarkCodeAsUsedAsync(int codeId);
//     }

//     // Refresh Token Repository Interface
//     public interface IRefreshTokenRepository
//     {
//         Task<RefreshToken> CreateTokenAsync(RefreshToken token);
//         Task<RefreshToken> GetByTokenAsync(string token);
//         Task RevokeTokenAsync(string token);
//         Task RevokeAllUserTokensAsync(int userId);
//     }

//     // Email Service Interface
//     public interface IEmailService
//     {
//         Task SendVerificationEmailAsync(string email, string code);
//         Task SendPasswordResetEmailAsync(string email, string code);
//         Task SendWelcomeEmailAsync(string email);
//     }

//     // Authentication Service Interface
//     public interface IAuthService
//     {
//         Task<AuthResponse> RegisterAsync(RegisterRequest request);
//         Task<AuthResponse> LoginAsync(LoginRequest request, string ipAddress);
//         Task<VerificationResponse> VerifyEmailAsync(VerifyEmailRequest request);
//         Task<VerificationResponse> ForgotPasswordAsync(ForgotPasswordRequest request);
//         Task<VerificationResponse> ResetPasswordAsync(ResetPasswordRequest request);
//         Task<AuthResponse> RefreshTokenAsync(string token, string ipAddress);
//         Task<VerificationResponse> RevokeTokenAsync(string token, string ipAddress);
//     }

//     // Token Service Interface
//     public interface ITokenService
//     {
//         string GenerateJwtToken(User user);
//         string GenerateRefreshToken();
//         int? ValidateJwtToken(string token);
//     }
// }

// // Auth Service Implementation - EmailVerification.Core/Services
// namespace EmailVerification.Core.Services
// {
//     public class AuthService : IAuthService
//     {
//         private readonly IUserRepository _userRepository;
//         private readonly IVerificationCodeRepository _verificationRepository;
//         private readonly IRefreshTokenRepository _refreshTokenRepository;
//         private readonly IEmailService _emailService;
//         private readonly ITokenService _tokenService;
//         private readonly ILogger<AuthService> _logger;
//         private readonly IOptions<AuthOptions> _authOptions;

//         public AuthService(
//             IUserRepository userRepository,
//             IVerificationCodeRepository verificationRepository,
//             IRefreshTokenRepository refreshTokenRepository,
//             IEmailService emailService,
//             ITokenService tokenService,
//             ILogger<AuthService> logger,
//             IOptions<AuthOptions> authOptions)
//         {
//             _userRepository = userRepository;
//             _verificationRepository = verificationRepository;
//             _refreshTokenRepository = refreshTokenRepository;
//             _emailService = emailService;
//             _tokenService = tokenService;
//             _logger = logger;
//             _authOptions = authOptions;
//         }

//         public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
//         {
//             try
//             {
//                 // Validate request
//                 if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
//                 {
//                     return new AuthResponse { Success = false, Message = "Email and password are required" };
//                 }

//                 if (request.Password != request.ConfirmPassword)
//                 {
//                     return new AuthResponse { Success = false, Message = "Passwords do not match" };
//                 }

//                 // Check if user exists
//                 var existingUser = await _userRepository.GetByEmailAsync(request.Email);
//                 if (existingUser != null)
//                 {
//                     return new AuthResponse { Success = false, Message = "User with this email already exists" };
//                 }

//                 // Create user
//                 var user = new User
//                 {
//                     Email = request.Email,
//                     PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
//                     IsEmailVerified = false,
//                     CreatedAt = DateTime.UtcNow
//                 };

//                 await _userRepository.CreateUserAsync(user);

//                 // Generate verification code
//                 var expiry = DateTime.UtcNow.AddMinutes(_authOptions.Value.VerificationCodeExpirationMinutes);
//                 var verificationCode = await _verificationRepository.SaveCodeAsync(
//                     request.Email,
//                     GenerateRandomCode(),
//                     VerificationPurpose.EmailVerification,
//                     expiry);

//                 // Send verification email
//                 await _emailService.SendVerificationEmailAsync(request.Email, verificationCode.Code);

//                 return new AuthResponse
//                 {
//                     Success = true,
//                     Message = "Registration successful. Please verify your email.",
//                     IsEmailVerified = false,
//                     User = new UserDto
//                     {
//                         Id = user.Id,
//                         Email = user.Email,
//                         IsEmailVerified = user.IsEmailVerified,
//                         CreatedAt = user.CreatedAt
//                     }
//                 };
//             }
//             catch (Exception ex)
//             {
//                 _logger.LogError(ex, "Error in RegisterAsync for {Email}", request.Email);
//                 return new AuthResponse { Success = false, Message = "Registration failed. Please try again." };
//             }
//         }

//         public async Task<AuthResponse> LoginAsync(LoginRequest request, string ipAddress)
//         {
//             try
//             {
//                 // Validate request
//                 if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
//                 {
//                     return new AuthResponse { Success = false, Message = "Email and password are required" };
//                 }

//                 // Get user
//                 var user = await _userRepository.GetByEmailAsync(request.Email);
//                 if (user == null)
//                 {
//                     return new AuthResponse { Success = false, Message = "Invalid email or password" };
//                 }

//                 // Verify password
//                 if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
//                 {
//                     return new AuthResponse { Success = false, Message = "Invalid email or password" };
//                 }

//                 // Update last login
//                 user.LastLoginAt = DateTime.UtcNow;
//                 await _userRepository.UpdateUserAsync(user);

//                 // Generate JWT token
//                 var token = _tokenService.GenerateJwtToken(user);
                
//                 // Generate refresh token
//                 var refreshToken = new RefreshToken
//                 {
//                     UserId = user.Id,
//                     Token = _tokenService.GenerateRefreshToken(),
//                     ExpiresAt = DateTime.UtcNow.AddDays(_authOptions.Value.RefreshTokenExpirationDays),
//                     CreatedByIp = ipAddress,
//                     CreatedAt = DateTime.UtcNow
//                 };

//                 await _refreshTokenRepository.CreateTokenAsync(refreshToken);

//                 return new AuthResponse
//                 {
//                     Success = true,
//                     Message = "Login successful",
//                     Token = token,
//                     RefreshToken = refreshToken.Token,
//                     IsEmailVerified = user.IsEmailVerified,
//                     User = new UserDto
//                     {
//                         Id = user.Id,
//                         Email = user.Email,
//                         IsEmailVerified = user.IsEmailVerified,
//                         CreatedAt = user.CreatedAt
//                     }
//                 };
//             }
//             catch (Exception ex)
//             {
//                 _logger.LogError(ex, "Error in LoginAsync for {Email}", request.Email);
//                 return new AuthResponse { Success = false, Message = "Login failed. Please try again." };
//             }
//         }

//         public async Task<VerificationResponse> VerifyEmailAsync(VerifyEmailRequest request)
//         {
//             try
//             {
//                 // Validate request
//                 if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Code))
//                 {
//                     return new VerificationResponse { Success = false, Message = "Email and code are required" };
//                 }

//                 // Get user
//                 var user = await _userRepository.GetByEmailAsync(request.Email);
//                 if (user == null)
//                 {
//                     return new VerificationResponse { Success = false, Message = "User not found" };
//                 }

//                 // Get verification code
//                 var verificationCode = await _verificationRepository.GetCodeAsync(
//                     request.Email, 
//                     VerificationPurpose.EmailVerification);

//                 if (verificationCode == null)
//                 {
//                     return new VerificationResponse { Success = false, Message = "Verification code not found" };
//                 }

//                 // Validate code
//                 if (verificationCode.IsUsed || DateTime.UtcNow > verificationCode.ExpiresAt)
//                 {
//                     return new VerificationResponse { Success = false, Message = "Verification code expired" };
//                 }

//                 if (verificationCode.Code != request.Code)
//                 {
//                     return new VerificationResponse { Success = false, Message = "Invalid verification code" };
//                 }

//                 // Mark code as used
//                 await _verificationRepository.MarkCodeAsUsedAsync(verificationCode.Id);

//                 // Update user
//                 user.IsEmailVerified = true;
//                 await _userRepository.UpdateUserAsync(user);

//                 // Send welcome email
//                 await _emailService.SendWelcomeEmailAsync(user.Email);

//                 return new VerificationResponse { Success = true, Message = "Email verified successfully" };
//             }
//             catch (Exception ex)
//             {
//                 _logger.LogError(ex, "Error in VerifyEmailAsync for {Email}", request.Email);
//                 return new VerificationResponse { Success = false, Message = "Email verification failed" };
//             }
//         }

//         public async Task<VerificationResponse> ForgotPasswordAsync(ForgotPasswordRequest request)
//         {
//             try
//             {
//                 // Validate request
//                 if (string.IsNullOrEmpty(request.Email))
//                 {
//                     return new VerificationResponse { Success = false, Message = "Email is required" };
//                 }

//                 // Get user
//                 var user = await _userRepository.GetByEmailAsync(request.Email);
//                 if (user == null)
//                 {
//                     // For security reasons, don't reveal that the user doesn't exist
//                     return new VerificationResponse { Success = true, Message = "If your email is registered, you will receive a password reset link" };
//                 }

//                 // Generate verification code
//                 var expiry = DateTime.UtcNow.AddMinutes(_authOptions.Value.VerificationCodeExpirationMinutes);
//                 var verificationCode = await _verificationRepository.SaveCodeAsync(
//                     request.Email,
//                     GenerateRandomCode(),
//                     VerificationPurpose.PasswordReset,
//                     expiry);

//                 // Send password reset email
//                 await _emailService.SendPasswordResetEmailAsync(request.Email, verificationCode.Code);

//                 return new VerificationResponse { Success = true, Message = "If your email is registered, you will receive a password reset link" };
//             }
//             catch (Exception ex)
//             {
//                 _logger.LogError(ex, "Error in ForgotPasswordAsync for {Email}", request.Email);
//                 return new VerificationResponse { Success = false, Message = "Password reset request failed" };
//             }
//         }

//         public async Task<VerificationResponse> ResetPasswordAsync(ResetPasswordRequest request)
//         {
//             try
//             {
//                 // Validate request
//                 if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Code) || 
//                     string.IsNullOrEmpty(request.Password))
//                 {
//                     return new VerificationResponse { Success = false, Message = "Email, code and password are required" };
//                 }

//                 if (request.Password != request.ConfirmPassword)
//                 {
//                     return new VerificationResponse { Success = false, Message = "Passwords do not match" };
//                 }

//                 // Get user
//                 var user = await _userRepository.GetByEmailAsync(request.Email);
//                 if (user == null)
//                 {
//                     return new VerificationResponse { Success = false, Message = "User not found" };
//                 }

//                 // Get verification code
//                 var verificationCode = await _verificationRepository.GetCodeAsync(
//                     request.Email,
//                     VerificationPurpose.PasswordReset);

//                 if (verificationCode == null)
//                 {
//                     return new VerificationResponse { Success = false, Message = "Reset code not found" };
//                 }

//                 // Validate code
//                 if (verificationCode.IsUsed || DateTime.UtcNow > verificationCode.ExpiresAt)
//                 {
//                     return new VerificationResponse { Success = false, Message = "Reset code expired" };
//                 }

//                 if (verificationCode.Code != request.Code)
//                 {
//                     return new VerificationResponse { Success = false, Message = "Invalid reset code" };
//                 }

//                 // Mark code as used
//                 await _verificationRepository.MarkCodeAsUsedAsync(verificationCode.Id);

//                 // Update user password
//                 user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
//                 await _userRepository.UpdateUserAsync(user);

//                 // Revoke all refresh tokens
//                 await _refreshTokenRepository.RevokeAllUserTokensAsync(user.Id);

//                 return new VerificationResponse { Success = true, Message = "Password reset successfully" };
//             }
//             catch (Exception ex)
//             {
//                 _logger.LogError(ex, "Error in ResetPasswordAsync for {Email}", request.Email);
//                 return new VerificationResponse { Success = false, Message = "Password reset failed" };
//             }
//         }

//         public async Task<AuthResponse> RefreshTokenAsync(string token, string ipAddress)
//         {
//             try
//             {
//                 // Validate token
//                 var refreshToken = await _refreshTokenRepository.GetByTokenAsync(token);
//                 if (refreshToken == null)
//                 {
//                     return new AuthResponse { Success = false, Message = "Invalid token" };
//                 }

//                 // Check if token is expired or revoked
//                 if (refreshToken.ExpiresAt < DateTime.UtcNow || refreshToken.IsRevoked)
//                 {
//                     return new AuthResponse { Success = false, Message = "Token expired" };
//                 }

//                 // Get user
//                 var user = await _userRepository.GetByIdAsync(refreshToken.UserId);
//                 if (user == null)
//                 {
//                     return new AuthResponse { Success = false, Message = "User not found" };
//                 }

//                 // Generate new JWT token
//                 var jwtToken = _tokenService.GenerateJwtToken(user);

//                 // Generate new refresh token
//                 var newRefreshToken = new RefreshToken
//                 {
//                     UserId = user.Id,
//                     Token = _tokenService.GenerateRefreshToken(),
//                     ExpiresAt = DateTime.UtcNow.AddDays(_authOptions.Value.RefreshTokenExpirationDays),
//                     CreatedByIp = ipAddress,
//                     CreatedAt = DateTime.UtcNow
//                 };

//                 // Save new refresh token
//                 await _refreshTokenRepository.CreateTokenAsync(newRefreshToken);

//                 // Revoke old refresh token
//                 await _refreshTokenRepository.RevokeTokenAsync(token);

//                 return new AuthResponse
//                 {
//                     Success = true,
//                     Message = "Token refreshed",
//                     Token = jwtToken,
//                     RefreshToken = newRefreshToken.Token,
//                     IsEmailVerified = user.IsEmailVerified,
//                     User = new UserDto
//                     {
//                         Id = user.Id,
//                         Email = user.Email,
//                         IsEmailVerified = user.IsEmailVerified,
//                         CreatedAt = user.CreatedAt
//                     }
//                 };
//             }
//             catch (Exception ex)
//             {
//                 _logger.LogError(ex, "Error in RefreshTokenAsync");
//                 return new AuthResponse { Success = false, Message = "Token refresh failed" };
//             }
//         }

//         public async Task<VerificationResponse> RevokeTokenAsync(string token, string ipAddress)
//         {
//             try
//             {
//                 // Validate token
//                 var refreshToken = await _refreshTokenRepository.GetByTokenAsync(token);
//                 if (refreshToken == null)
//                 {
//                     return new VerificationResponse { Success = false, Message = "Invalid token" };
//                 }

//                 // Revoke token
//                 await _refreshTokenRepository.RevokeTokenAsync(token);

//                 return new VerificationResponse { Success = true, Message = "Token revoked" };
//             }
//             catch (Exception ex)
//             {
//                 _logger.LogError(ex, "Error in RevokeTokenAsync");
//                 return new VerificationResponse { Success = false, Message = "Token revocation failed" };
//             }
//         }

//         private string GenerateRandomCode()
//         {
//             using var rng = RandomNumberGenerator.Create();
//             var bytes = new byte[4];
//             rng.GetBytes(bytes);
//             uint random = BitConverter.ToUInt32(bytes, 0);
//             return (random % 1000000).ToString("D6");
//         }
//     }
// }

// // Token Service Implementation - EmailVerification.Infrastructure/Services
// namespace EmailVerification.Infrastructure.Services
// {
//     public class TokenService : ITokenService
//     {
//         private readonly IOptions<AuthOptions> _authOptions;

//         public TokenService(IOptions<AuthOptions> authOptions)
//         {
//             _authOptions = authOptions;
//         }

//         public string GenerateJwtToken(User user)
//         {
//             var tokenHandler = new JwtSecurityTokenHandler();
//             var key = Encoding.ASCII.GetBytes(_authOptions.Value.JwtSecret);
            
//             var claims = new List<Claim>
//             {
//                 new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
//                 new Claim(ClaimTypes.Email, user.Email),
//                 new Claim("IsEmailVerified", user.IsEmailVerified.ToString())
//             };

//             var tokenDescriptor = new SecurityTokenDescriptor
//             {
//                 Subject = new ClaimsIdentity(claims),
//                 Expires = DateTime.UtcNow.AddMinutes(_authOptions.Value.JwtExpirationMinutes),
//                 SigningCredentials = new SigningCredentials(
//                     new SymmetricSecurityKey(key), 
//                     SecurityAlgorithms.HmacSha256Signature)
//             };

//             var token = tokenHandler.CreateToken(tokenDescriptor);
//             return tokenHandler.WriteToken(token);
//         }

//         public string GenerateRefreshToken()
//         {
//             using var rng = RandomNumberGenerator.Create();
//             var randomBytes = new byte[64];
//             rng.GetBytes(randomBytes);
//             return Convert.ToBase64String(randomBytes);
//         }

//         public int? ValidateJwtToken(string token)
//         {
//             if (string.IsNullOrEmpty(token))
//                 return null;

//             var tokenHandler = new JwtSecurityTokenHandler();
//             var key = Encoding.ASCII.GetBytes(_authOptions.Value.JwtSecret);

//             try
//             {
//                 tokenHandler.ValidateToken(token, new TokenValidationParameters
//                 {
//                     ValidateIssuerSigningKey = true,
//                     IssuerSigningKey = new SymmetricSecurityKey(key),
//                     ValidateIssuer = false,
//                     ValidateAudience = false,
//                     ClockSkew = TimeSpan.Zero
//                 }, out SecurityToken validatedToken);

//                 var jwtToken = (JwtSecurityToken)validatedToken;
//                 var userId = int.Parse(jwtToken.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value);

//                 return userId;
//             }
//             catch
//             {
//                 return null;
//             }
//         }
//     }
// }

// // Email Service Implementation - EmailVerification.Infrastructure/Services
// namespace EmailVerification.Infrastructure.Services
// {
//     public class EmailService : IEmailService
//     {
//         private readonly IOptions<EmailOptions> _emailOptions;
//         private readonly ILogger<EmailService> _logger;
        
//         public EmailService(IOptions<EmailOptions> emailOptions, ILogger<EmailService> logger)
//         {
//             _emailOptions = emailOptions;
//             _logger = logger;
//         }
        
//         public async Task SendVerificationEmailAsync(string email, string code)
//         {
//             var subject = "Verify Your Email Address";
//             var body = GenerateEmailTemplate(
//                 "Email Verification",
//                 $"Please use the following code to verify your email address:",
//                 code,
//                 "This code will expire in 10 minutes.",
//                 "If you didn't request this code, please ignore this email."
//             );
            
//             await SendEmailAsync(email, subject, body);
//         }
        
//         public async Task SendPasswordResetEmailAsync(string email, string code)
//         {
//             var subject = "Reset Your Password";
//             var body = GenerateEmailTemplate(
//                 "Password Reset",
//                 $"You requested to reset your password. Please use the following code:",
//                 code,
//                 "This code will expire in 10 minutes.",
//                 "If you didn't request a password reset, please ignore this email or contact support."
//             );
            
//             await SendEmailAsync(email, subject, body);
//         }
        
//         public async Task SendWelcomeEmailAsync(string email)
//         {
//             var subject = "Welcome to Our Service";
//             var body = @$"
//             <!DOCTYPE html>
//             <html>
//             <head>
//                 <style>
//                     body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
//                     .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
//                     .header {{ background-color: #4285f4; color: white; padding: 10px; text-align: center; }}
//                     .content {{ padding: 20px; }}
//                     .footer {{ font-size: 12px; text-align: center; margin-top: 20px; color: #666; }}
//                 </style>
//             </head>
//             <body>
//                 <div class='container'>
//                     <div class='header'>
//                         <h1>Welcome!</h1>
//                     </div>
//                     <div class='content'>
//                         <p>Thank you for verifying your email address!</p>
//                         <p>Your account is now fully active, and you can access all our services.</p>
//                         <p>If you have any questions, please don't hesitate to contact our support team.</p>
//                     </div>
//                     <div class='footer'>
//                         <p>This is an automated message, please do not reply.</p>
//                     </div>
//                 </div>
//             </body>
//             </html>";
            
//             await SendEmailAsync(email, subject, body);
//         }
        
//         private async Task SendEmailAsync(string email, string subject, string body)
//         {
//             try
//             {
//                 var options = _emailOptions.Value;
                
//                 var client = new SmtpClient(options.SmtpServer)
//                 {
//                     Port = options.SmtpPort,
//                     Credentials = new NetworkCredential(options.Username, options.Password),
//                     EnableSsl = true,
//                 };
                
//                 var mailMessage = new MailMessage
//                 {
//                     From = new MailAddress(options.SenderEmail, options.SenderName),
//                     Subject = subject,
//                     Body = body,
//                     IsBodyHtml = true,
//                 };
                
//                 mailMessage.To.Add(email);
                
//                 await client.SendMailAsync(mailMessage);
//                 _logger.LogInformation("Email sent to {Email} with subject {Subject}", email, subject);
//             }
//             catch (Exception ex)
//             {
//                 _logger.LogError(ex, "Failed to send email to {Email} with subject {Subject}", email, subject);
//                 throw new VerificationException("Failed to send email", ex);
//             }
//         }
        
//         private string GenerateEmailTemplate(string title, string message, string code, string expiry, string warning)
//         {
//             return @$"
//             <!DOCTYPE html>
//             <html>
//             <head>
//                 <style>
//                     body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
//                     .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
//                     .header {{ background-color: #4285f4; color: white; padding: 10px; text-align: center; }}
//                     .content {{ padding: 20px; }}
//                     .code {{ font-size: 32px; font-weight: bold; text-align: center; margin: 20px 0; letter-spacing: 5px; }}
//                     .footer {{ font-size: 12px; text-align: center; margin-top: 20px; color: #666; }}
//                 </style>
//             </head>
//             <body>
//                 <div class='container'>
//                     <div class='header'>
//                         <h1>{title}</h1>
//                     </div>
//                     <div class='content'>
//                         <p>{message}</p>
//                         <div class='code'>{code}</div>
//                         <p>{expiry}</p>
//                         <p>{warning}</p>
//                     </div>
//                     <div class='footer'>
//                         <p>This is an automated message, please do not reply.</p>
//                     </div>
//                 </div>
//             </body>
//             </html>";
//         }
//     }
// }

// // Controllers - EmailVerification.API/Controllers
// namespace EmailVerification.API.Controllers
// {
//     [ApiController]
//     [Route("api/[controller]")]
//     public class AuthController : ControllerBase
//     {
//         private readonly IAuthService _authService;
//         private readonly ILogger<AuthController> _logger;

//         public AuthController(
//             IAuthService authService,
//             ILogger<AuthController> logger)
//         {
//             _authService = authService;
//             _logger = logger;
//         }

//         [HttpPost("register")]
//         public async Task<IActionResult> Register([FromBody] RegisterRequest request)
//         {
//             var response = await _authService.RegisterAsync(request);
//             return response.Success ? Ok(response) : BadRequest(response);
//         }

//         [HttpPost("login")]
//         public async Task<IActionResult> Login([FromBody] LoginRequest request)
//         {
//             var ipAddress = GetIpAddress();
//             var response = await _authService.LoginAsync(request, ipAddress);
            
//             if (!response.Success)
//                 return BadRequest(response);
                
//             SetRefreshTokenCookie(response.RefreshToken);
//             return Ok(response);
//         }

//         [HttpPost("verify-email")]
//         public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailRequest request)
//         {
//             var response = await _authService.VerifyEmailAsync(request);
//             return response.Success ? Ok(response) : BadRequest(response);
//         }

//         [HttpPost("forgot-password")]
//         public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
//         {
//             var response = await _authService.ForgotPasswordAsync(request);
//             return Ok(response); // Always return OK for security
//         }

//         [HttpPost("reset-password")]
//         public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
//         {
//             var response = await _authService.ResetPasswordAsync(request);
//             return response.Success ? Ok(response) : BadRequest(response);
//         }

//         [HttpPost("refresh-token")]
//         public async Task<IActionResult> RefreshToken()
//         {
//             var refreshToken = Request.Cookies["refreshToken"];
//             var ipAddress = GetIpAddress();
            
//             if (string.IsNullOrEmpty(refreshToken))
//                 return BadRequest(new { Success = false, Message = "Refresh token is required" });
                
//             var response = await _authService.RefreshTokenAsync(refreshToken, ipAddress);
            
//             if (!response.Success)
//                 return BadRequest(response);
                
//             SetRefreshTokenCookie(response.RefreshToken);
//             return Ok(response);
//         }

//         [Authorize]
//         [HttpPost("revoke-token")]
//         public async Task<IActionResult> RevokeToken([FromBody] RefreshTokenRequest request)
//         {
//             var token = request.Token ?? Request.Cookies["refreshToken"];
//             var ipAddress = GetIpAddress();
            
//             if (string.IsNullOrEmpty(token))
//                 return BadRequest(new { Success = false, Message = "Token is required" });
                
//             var response = await _authService.RevokeTokenAsync(token, ipAddress);
//             return response.Success ? Ok(response) : BadRequest(response);
//         }

//         [Authorize]
//         [HttpGet("me")]
//         public IActionResult GetCurrentUser()
//         {
//             var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

//             return Ok(new { UserId = userId });
//         }