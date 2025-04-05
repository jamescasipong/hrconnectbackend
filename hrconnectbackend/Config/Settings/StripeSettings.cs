namespace hrconnectbackend.Config
{
    /// <summary>
    /// Represents the settings required for configuring Stripe payment gateway integration.
    /// This class contains properties for Stripe secret key, publishable key, and webhook secret used in payment processing and webhook validation.
    /// </summary>
    public class StripeSettings
    {
        /// <summary>
        /// Gets or sets the Stripe secret key used for authenticating API requests.
        /// This key is used to make secure server-side requests to the Stripe API, such as creating charges or processing payments.
        /// Keep this key secure and do not expose it to the client side.
        /// </summary>
        public string SecretKey { get; set; }

        /// <summary>
        /// Gets or sets the Stripe publishable key, which is used on the client side.
        /// This key is safe to be exposed in your frontend code and is used to create tokens for handling payments on the client side.
        /// </summary>
        public string PublishableKey { get; set; }

        /// <summary>
        /// Gets or sets the secret key used to validate Stripe webhook events.
        /// This key ensures that incoming webhook requests are from Stripe and have not been tampered with.
        /// Use this to verify the authenticity of webhook events sent by Stripe to your application.
        /// </summary>
        public string WebhookSecret { get; set; }
    }
}