using System;
using Nop.Core;

namespace Nop.Plugin.Payments.StripeConnectRedirect.Domain
{
    /// <summary>
    /// Represents a Stripe Vendor Connect
    /// </summary>
    public partial class StripeVendorConnect : BaseEntity
    {
        /// <summary>
        /// Gets or sets the vendor id
        /// </summary>
        public int VendorId { get; set; }

        /// <summary>
        /// Gets or sets the Account 
        /// </summary>
        public string Account { get; set; }

        /// <summary>
        /// Gets or sets the Admin Delivery Commission Percentage
        /// </summary>
        public decimal AdminDeliveryCommissionPercentage { get; set; }

        /// <summary>
        /// Gets or sets the Admin Pickup Commission Percentage
        /// </summary>
        public decimal AdminPickupCommissionPercentage { get; set; }

        /// <summary>
        /// Gets or sets the Is Verified 
        /// </summary>
        public bool IsVerified { get; set; }

        /// <summary>
        /// Gets or sets the CreatedOnUtc 
        /// </summary>
        public DateTime CreatedOnUtc { get; set; }

    }
}