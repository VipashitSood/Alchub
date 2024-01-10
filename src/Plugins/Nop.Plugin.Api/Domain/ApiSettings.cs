using Nop.Core.Configuration;
using System.Collections.Generic;

namespace Nop.Plugin.Api.Domain
{
    public class ApiSettings : ISettings
    {
        public ApiSettings()
        {
            Restricted_PaymentMethodsSystemNames = new List<string>();
        }

        public bool EnableApi { get; set; } = true;

        public int TokenExpiryInDays { get; set; } = 0;

        /// <summary>
        /// Get or Set the payment method system names which will be included in API getPaymentMethods list. Keep empty to load all just like web.
        /// </summary>
        public List<string> Restricted_PaymentMethodsSystemNames { get; set; }

        /// <summary>
        /// Get or Set the value which indicates OTP is required to complete the customer registration process (From mobile).
        /// Note: Registration process from Web will work as default. It has no relevence with this setting. 
        /// </summary>
        public bool OtpRequiredOnRegistration { get; set; }
    }
}
