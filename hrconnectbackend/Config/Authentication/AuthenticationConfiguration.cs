using System.IdentityModel.Tokens.Jwt;
using System.Text;
using hrconnectbackend.Config;
using hrconnectbackend.Data;
using hrconnectbackend.Helper.Crypto;
using hrconnectbackend.Interface.Services;
using hrconnectbackend.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace hrconnectbackend.Config.Authentication.Configuration
{
    /// <summary>
    /// Configures JWT-based authentication settings, including token validation and refresh logic.
    /// This class is responsible for handling the validation of access tokens and refresh tokens,
    /// including decryption and renewal mechanisms when necessary.
    /// </summary>
    public class AuthenticationConfiguration
    {
        private readonly IConfiguration _configuration;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<AuthenticationConfiguration> _logger;
        private readonly JwtSettings _jwtSettings;

        /// <summary>
        /// Constructor that initializes the authentication configuration with necessary dependencies:
        /// - IConfiguration: To retrieve settings from the configuration file.
        /// - IServiceProvider: To resolve other services like database context and authentication services.
        /// - ILogger: To log relevant information, warnings, and errors during token processing.
        /// - JwtSettings: To access JWT-specific settings like token expiration.
        /// </summary>
        /// <param name="configuration">The configuration for application settings.</param>
        /// <param name="serviceProvider">Service provider for resolving services like DataContext.</param>
        /// <param name="logger">Logger to log messages for debugging and monitoring.</param>
        /// <param name="jwtSettings">JWT-specific settings, such as token expiration times.</param>
        public AuthenticationConfiguration(IConfiguration configuration, IServiceProvider serviceProvider, ILogger<AuthenticationConfiguration> logger, IOptions<JwtSettings> jwtSettings)
        {
            _jwtSettings = jwtSettings.Value;
            _configuration = configuration;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        /// <summary>
        /// Configures the JWT Bearer options used in the authentication process. 
        /// Specifically, it sets the token validation parameters, such as:
        /// - Issuer signing key
        /// - Token expiration validation
        /// - Lifetime validation
        /// 
        /// Also configures event handlers for processing JWT token messages from requests.
        /// </summary>
        /// <param name="options">JWT Bearer options to configure.</param>
        public void JwtOptions(JwtBearerOptions options)
        {
            // Configure the token validation parameters
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,  // Ensure the issuer's signing key is validated
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(
                    _configuration.GetValue<string>("JWT:Key")!  // Retrieve the signing key from the configuration
                )),
                ValidateIssuer = false,  // Ignore issuer validation (can be enabled if needed)
                ValidateAudience = false,  // Ignore audience validation (can be enabled if needed)
                ValidateLifetime = false,  // Token expiration will be handled manually in the event handler
            };

            // Define event handlers for processing JWT tokens
            options.Events = new JwtBearerEvents
            {
                // Event triggered when the message is received (token validation)
                OnMessageReceived = async (context) =>
                {
                    var httpContext = context.HttpContext;

                    _logger.LogInformation("Processing token validation.");

                    // Check if an access token is available in the cookies
                    if (httpContext.Request.Cookies.TryGetValue("at_session", out var accessToken))
                    {
                        try
                        {
                            _logger.LogInformation("Access token found in cookies. Attempting decryption.");

                            // Decrypt the access token using AES encryption
                            var aes = new AES256Encrpytion(_configuration.GetValue<string>("EncryptionSettings:Key")!);
                            var decryptedToken = aes.Decrypt(accessToken);

                            var jwtHandler = new JwtSecurityTokenHandler();
                            var securityToken = jwtHandler.ReadToken(decryptedToken) as JwtSecurityToken;

                            // Check if the token has expired
                            if (securityToken.ValidTo < DateTime.UtcNow)
                            {
                                _logger.LogWarning("Token is expired.");
                                context.Fail("Token is Expired");
                                return;
                            }

                            _logger.LogInformation("Access token is valid.");
                            context.Token = decryptedToken;  // Assign the decrypted token for further processing
                            return;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error decrypting access token.");
                            context.Fail("Error processing token.");
                            return;
                        }
                    }
                    else
                    {
                        _logger.LogInformation("Access token not found, checking for refresh token.");

                        // Check if a refresh token is available in the cookies
                        if (httpContext.Request.Cookies.TryGetValue("backend_rt", out var rt))
                        {
                            try
                            {
                                _logger.LogInformation("Refresh token found in cookies. Attempting to validate.");

                                // Use the service provider to resolve services like the DataContext and AuthService
                                using var service = _serviceProvider.CreateScope();
                                var dbContext = service.ServiceProvider.GetRequiredService<DataContext>();
                                var authService = service.ServiceProvider.GetRequiredService<IAuthService>();

                                // Look up the refresh token in the database
                                var refreshToken = await dbContext.RefreshTokens.FirstOrDefaultAsync(a => a.RefreshTokenId == rt);

                                // Check if the refresh token exists and is not expired
                                if (refreshToken == null)
                                {
                                    _logger.LogWarning("Refresh token not found.");
                                    context.Fail("Refresh token not found");
                                    return;
                                }

                                if (refreshToken.Expires < DateTime.UtcNow)
                                {
                                    _logger.LogWarning("Refresh token is expired.");
                                    context.Fail("Refresh token is expired");
                                    return;
                                }

                                _logger.LogInformation("Refresh token is valid. Generating new access token.");

                                // Generate a new access token using the valid refresh token
                                var generatedAccessToken = await authService.GenerateAccessToken(rt);

                                // Encrypt the new access token and store it in the cookies
                                var aes = new AES256Encrpytion(_configuration.GetValue<string>("EncryptionSettings:Key")!);
                                var encrypted = aes.Encrypt(generatedAccessToken);

                                DateTime exp = DateTime.UtcNow.ToLocalTime().AddMinutes(_jwtSettings.AccessExpiration);
                                context.HttpContext.Response.Cookies.Append("at_session", encrypted, new CookieOptions
                                {
                                    HttpOnly = true,  // Ensure the cookie is accessible only via HTTP requests
                                    SameSite = SameSiteMode.None,  // Allow the cookie to be sent with cross-site requests
                                    Secure = true,  // Ensure the cookie is sent over HTTPS only
                                    Path = "/",  // Define the path scope for the cookie
                                    Expires = exp,  // Set the expiration for the new access token cookie
                                });

                                _logger.LogInformation("New access token generated and stored in cookie.");
                            }
                            catch (Exception ex)
                            {
                                // Log any errors encountered while processing the refresh token
                                _logger.LogError(ex, "Error processing refresh token.");
                                context.Fail("Error processing refresh token.");
                                return;
                            }
                        }
                        else
                        {
                            _logger.LogWarning("No valid token found in cookies.");
                            context.Fail("Token is invalid");
                        }
                    }

                    await Task.CompletedTask;
                }
            };
        }
    }
}
