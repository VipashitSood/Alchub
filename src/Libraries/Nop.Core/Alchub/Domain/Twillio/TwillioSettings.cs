using Nop.Core.Configuration;

namespace Nop.Core.Alchub.Domain.Twillio
{
    public partial class TwillioSettings : ISettings
    {
        // <summary>
        /// Gets or sets the enabaled
        /// </summary>
        public bool Enabled { get; set; }

        // <summary>
        /// Gets or sets the account sid
        /// </summary>
        public string AccountSid { get; set; }

        /// <summary>
        /// Gets or sets the auth token 
        /// </summary>
        public string AuthToken { get; set; }

        /// <summary>
        /// Gets or sets the from number
        /// </summary>
        public string FromNumber { get; set; }

        /// <summary>
        /// Gets or sets the from default country code (+1, +91, etc)
        /// </summary>
        public string DefaultCountryCode { get; set; }

        /// <summary>
        /// Gets or sets the order place sms body template
        /// </summary>
        public string OrderPlacedBody { get; set; }

        /// <summary>
        /// Gets or sets the order items dispatched body template
        /// </summary>
        public string OrderItemsDispatchedBody { get; set; }

        /// <summary>
        /// Gets or sets the order items pick up completed sms body template
        /// </summary>
        public string OrderItemsPickedUpBody { get; set; }

        /// <summary>
        /// Gets or sets the order items delivered sms body template
        /// </summary>
        public string OrderItemsDeliveredBody { get; set; }


        /// <summary>
        /// Gets or sets the order items pick up completed sms body template
        /// </summary>
        public string OrderItemsCancelBody { get; set; }

        /// <summary>
        /// Gets or sets the order items delivered sms body template
        /// </summary>
        public string OrderItemsDelivereyDeniedBody { get; set; }


        public string OrderItemPickupBody { get; set; }

        /// <summary>
        /// Gets or sets the order place vendor sms body template
        /// </summary>
        public string OrderPlacedVendorBody { get; set; }
    }
}
