using Nop.Web.Framework.Mvc.ModelBinding;
using System.Collections.Generic;

namespace Nop.Plugin.Api.Areas.Admin.Models
{
    public class ConfigurationModel
    {
        public int ActiveStoreScopeConfiguration { get; set; }

        [NopResourceDisplayName("Plugins.Api.Admin.EnableApi")]
        public bool EnableApi { get; set; }

        public bool EnableApi_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Api.Admin.TokenExpiryInDays")]
        public int TokenExpiryInDays { get; set; }

        public bool TokenExpiryInDays_OverrideForStore { get; set; }

        /// <summary>
        /// Get or Set the payment method system names which will be included in API getPaymentMethods list. Keep empty to load all just like web.
        /// </summary>
        [NopResourceDisplayName("Plugins.Api.Admin.Configuration.Restricted_PaymentMethodsSystemNames")]
        public string Restricted_PaymentMethodsSystemNames { get; set; }
        public bool Restricted_PaymentMethodsSystemNames_OverrideForStore { get; set; }

        /// <summary>
        /// Get or Set the value which indicates OTP is required to complete the customer registration process (From mobile).
        /// Note: Registration process from Web will work as default. It has no relevence with this setting. 
        /// </summary>
        [NopResourceDisplayName("Plugins.Api.Admin.Configuration.OtpRequiredOnRegistration")]
        public bool OtpRequiredOnRegistration { get; set; }
        public bool OtpRequiredOnRegistration_OverrideForStore { get; set; }
    }
}
