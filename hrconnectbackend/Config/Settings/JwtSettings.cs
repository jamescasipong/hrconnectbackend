namespace hrconnectbackend.Config
{
    /// <summary>
    /// Represents the settings required for configuring JWT authentication.
    /// This class contains properties related to JWT token generation and validation,
    /// such as the signing key, issuer, audience, and expiration times for both access and refresh tokens.
    /// </summary>
    public class JwtSettings
    {
        /// <summary>
        /// Gets or sets the secret key used to sign and validate JWT tokens.
        /// This key should be kept secret and secure, as it ensures the integrity of the token.
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Gets or sets the issuer of the JWT token (e.g., the identity provider).
        /// The issuer represents the entity that issued the token and is typically used for validation.
        /// </summary>
        public string Issuer { get; set; }

        /// <summary>
        /// Gets or sets the intended audience for the JWT token (e.g., the API that will consume the token).
        /// The audience is used to validate that the token was issued for the intended recipient.
        /// </summary>
        public string Audience { get; set; }

        /// <summary>
        /// Gets or sets the expiration time (in minutes) for the access token.
        /// This defines how long the access token is valid before the user needs to obtain a new one.
        /// </summary>
        public int AccessExpiration { get; set; }

        /// <summary>
        /// Gets or sets the expiration time (in minutes) for the refresh token.
        /// The refresh token is used to obtain a new access token when the current access token expires.
        /// </summary>
        public int RefreshExpiration { get; set; }

        /// <summary>
        /// Gets or sets the expiration time (in minutes) for the refresh token when the "remember me" option is enabled.
        /// This allows the refresh token to have a longer validity, keeping the user logged in for an extended period.
        /// </summary>
        public int RefreshExpirationRemember { get; set; }
    }
}
