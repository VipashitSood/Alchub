using System.ComponentModel.DataAnnotations;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Payments.StripeConnectRedirect.Models
{
    public record ConfigurationModel : BaseNopModel
    {
        public ConfigurationModel()
        {
            VendorSearchModel = new VendorSearchModel();
        }

        public int ActiveStoreScopeConfiguration { get; set; }

        /// <summary>
        /// Gets or sets the Additional Fee
        /// </summary>
        [NopResourceDisplayName("Plugin.Payments.StripeConnectRedirect.Configuration.Fields.AdditionalFee")]
        public decimal AdditionalFee { get; set; }

        public bool AdditionalFee_OverrideForStore { get; set; }

        /// <summary>
        /// Gets or sets the Additional Fee Percentage
        /// </summary>
        [NopResourceDisplayName("Plugin.Payments.StripeConnectRedirect.Configuration.Fields.AdditionalFeePercentage")]
        public bool AdditionalFeePercentage { get; set; }

        public bool AdditionalFeePercentage_OverrideForStore { get; set; }

        /// <summary>
        /// Gets or sets the Client Id
        /// </summary>
        [NopResourceDisplayName("Plugin.Payments.StripeConnectRedirect.Configuration.Fields.ClientId")]
        public string ClientId { get; set; }

        public bool ClientId_OverrideForStore { get; set; }

        /// <summary>
        /// Gets or sets the Publishable Api Key
        /// </summary>
        [NopResourceDisplayName("Plugin.Payments.StripeConnectRedirect.Configuration.Fields.PublishableApiKey")]
        public string PublishableApiKey { get; set; }

        public bool PublishableApiKey_OverrideForStore { get; set; }

        /// <summary>
        /// Gets or sets the Secret Api Key
        /// </summary>
		[NopResourceDisplayName("Plugin.Payments.StripeConnectRedirect.Configuration.Fields.SecretApiKey")]
        public string SecretApiKey { get; set; }

        public bool SecretApiKey_OverrideForStore { get; set; }

        /// <summary>
        /// Gets or sets the Payment Description
        /// </summary>
        [NopResourceDisplayName("Plugin.Payments.StripeConnectRedirect.Configuration.Fields.PaymentDescription")]
        [StringLength(22)]
        public string PaymentDescription { get; set; }

        public bool PaymentDescription_OverrideForStore { get; set; }

        /// <summary>
        /// Gets or sets the Webhook Url
        /// </summary>
        [NopResourceDisplayName("Plugin.Payments.StripeConnectRedirect.Configuration.Fields.WebhookUrl")]
        public string WebhookUrl { get; set; }

        /// <summary>
        /// Gets or sets the Webhook Id
        /// </summary>
        [NopResourceDisplayName("Plugin.Payments.StripeConnectRedirect.Configuration.Fields.WebhookId")]
        public string WebhookId { get; set; }

        /// <summary>
        /// Gets or sets the Signing Secret Key
        /// </summary>
        [NopResourceDisplayName("Plugin.Payments.StripeConnectRedirect.Configuration.Fields.SigningSecretKey")]
        public string SigningSecretKey { get; set; }

        /// <summary>
        /// Gets or sets the Vendor Search Model
        /// </summary>
        public VendorSearchModel VendorSearchModel { get; set; }
    }
}