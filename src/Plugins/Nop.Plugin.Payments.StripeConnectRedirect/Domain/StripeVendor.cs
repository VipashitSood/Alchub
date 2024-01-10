using System;
using Nop.Core;

namespace Nop.Plugin.Payments.StripeConnectRedirect.Domain
{
    /// <summary>
    /// Represents a Stripe Vendor
    /// </summary>
    public partial class StripeVendor : BaseEntity
    {
        /// <summary>
        /// Gets or sets the Name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the Name
        /// </summary>
        public string Email { get; set; }

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
    }
}