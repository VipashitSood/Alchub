using Nop.Core.Configuration;

namespace Nop.Plugin.Payments.StripeConnectRedirect
{
    /// <summary>
    /// Represents settings of the Stripe Connect Redirect payment plugin
    /// </summary>
    public class StripeConnectRedirectPaymentSettings : ISettings
    {
        /// <summary>
        /// Gets or sets a Client Id
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        /// Gets or sets a Publishable Api Key
        /// </summary>
        public string PublishableApiKey { get; set; }

        /// <summary>
        /// Gets or sets a Secret Api Key
        /// </summary>
        public string SecretApiKey { get; set; }

        /// <summary>
        /// Gets or sets a Payment Description
        /// </summary>
        public string PaymentDescription { get; set; }

        /// <summary>
        /// Gets or sets a Webhook Id
        /// </summary>
        public string WebhookId { get; set; }

        /// <summary> 
        /// Gets or sets a Signing Secret Key
        /// </summary>
        public string SigningSecretKey { get; set; }

        /// <summary>
        /// Gets or sets a additional fee
        /// </summary>
        public decimal AdditionalFee { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to "additional fee" is specified as percentage. true - percentage, false - fixed value.
        /// </summary>
        public bool AdditionalFeePercentage { get; set; }
    }
}