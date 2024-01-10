using System;
using Nop.Core;

namespace Nop.Plugin.Payments.StripeConnectRedirect.Domain
{
    /// <summary>
    /// Represents a Stripe Order
    /// </summary>
    public partial class StripeOrder : BaseEntity
    {
        /// <summary>
        /// Gets or sets the Order id
        /// </summary>
        public int OrderId { get; set; }

        /// <summary>
        /// Gets or sets the Session id
        /// </summary>
        public string SessionId { get; set; }

        /// <summary>
        /// Gets or sets the Status
        /// </summary>
        public string SessionStatus { get; set; }

        /// <summary>
        /// Gets or sets the Payment Intent id
        /// </summary>
        public string PaymentIntentId { get; set; }

        /// <summary>
        /// Gets or sets the Status
        /// </summary>
        public string PaymentStatus { get; set; }

        /// <summary>
        /// Gets or sets the Order Account 
        /// </summary>
        public decimal OrderAmount { get; set; }

        /// <summary>
        /// Gets or sets the CreatedOnUtc 
        /// </summary>
        public DateTime CreatedOnUtc { get; set; }
    }
}