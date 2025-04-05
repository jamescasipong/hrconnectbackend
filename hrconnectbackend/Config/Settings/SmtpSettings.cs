namespace hrconnectbackend.Config
{
    /// <summary>
    /// Represents the settings required for configuring SMTP (Simple Mail Transfer Protocol) email sending.
    /// This class contains properties for SMTP server authentication, server address, and port configuration.
    /// </summary>
    public class SmtpSettings
    {
        /// <summary>
        /// Gets or sets the username for authenticating to the SMTP server.
        /// This is typically the email address or the username assigned by the email service provider.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Gets or sets the password for authenticating to the SMTP server.
        /// This password is used to verify the identity of the user when connecting to the SMTP server for sending emails.
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Gets or sets the SMTP server address (e.g., smtp.gmail.com).
        /// This is the server that will handle the sending of emails.
        /// </summary>
        public string Server { get; set; }

        /// <summary>
        /// Gets or sets the SMTP port number to use for the connection to the server.
        /// Common SMTP ports are 25, 587, or 465, depending on the encryption (TLS/SSL) and the server's configuration.
        /// </summary>
        public int SmtpPort { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SmtpSettings"/> class with required parameters.
        /// </summary>
        public SmtpSettings(string username, string password, string server, int smtpPort)
        {
            Username = username ?? throw new ArgumentNullException(nameof(username));
            Password = password ?? throw new ArgumentNullException(nameof(password));
            Server = server ?? throw new ArgumentNullException(nameof(server));
            SmtpPort = smtpPort;
        }
    }
}