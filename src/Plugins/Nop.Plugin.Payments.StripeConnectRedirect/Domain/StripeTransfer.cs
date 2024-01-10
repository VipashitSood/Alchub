using System;
using Nop.Core;

namespace Nop.Plugin.Payments.StripeConnectRedirect.Domain
{
    /// <summary>
    /// Represents a Stripe Transfer
    /// </summary>
    public partial class StripeTransfer : BaseEntity
    {
        /// <summary>
        /// Gets or sets the order id
        /// </summary>
        public int OrderId { get; set; }

        /// <summary>
        /// Gets or sets the vendor id
        /// </summary>
        public int VendorId { get; set; }

        /// <summary>
        /// Gets or sets the Vendor Account 
        /// </summary>
        public string VendorAccount { get; set; }

        /// <summary>
        /// Gets or sets the Delivery Subtotal Incl Tax
        /// </summary>
        public decimal DeliverySubtotalInclTax { get; set; }

        /// <summary>
        /// Gets or sets the Delivery Subtotal Excl Tax
        /// </summary>
        public decimal DeliverySubtotalExclTax { get; set; }

        /// <summary>
        /// Gets or sets the Pickup Subtotal Incl Tax
        /// </summary>
        public decimal PickupSubtotalInclTax { get; set; }

        /// <summary>
        /// Gets or sets the Pickup Subtotal Excl Tax
        /// </summary>
        public decimal PickupSubtotalExclTax { get; set; }

        /// <summary>
        /// Gets or sets the Admin Delivery Commission Percentage
        /// </summary>
        public decimal AdminDeliveryCommissionPercentage { get; set; }

        /// <summary>
        /// Gets or sets the Admin Pickup Commission Percentage
        /// </summary>
        public decimal AdminPickupCommissionPercentage { get; set; }

        /// <summary>
        /// Gets or sets the Admin Delivery Commission
        /// </summary>
        public decimal AdminDeliveryCommission { get; set; }

        /// <summary>
        /// Gets or sets the Admin Pickup Commission
        /// </summary>
        public decimal AdminPickupCommission { get; set; }

        /// <summary>
        /// Gets or sets the Service Fee
        /// </summary>
        public decimal ServiceFee { get; set; }

        /// <summary>
        /// Gets or sets the Delivery Fee
        /// </summary>
        public decimal DeliveryFee { get; set; }

        /// <summary>
        /// Gets or sets the Slot Fee
        /// </summary>
        public decimal SlotFee { get; set; }

        /// <summary>
        /// Gets or sets the Tip Fee
        /// </summary>
        public decimal TipFee { get; set; }

        /// <summary>
        /// Gets or sets the Total Amount 
        /// </summary>
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// Gets or sets the Transfer Id
        /// </summary>
        public string TransferId { get; set; }

        /// <summary>
        /// Gets or sets the Is Transferred
        /// </summary>
        public bool IsTransferred { get; set; }

        /// <summary>
        /// Gets or sets the CreatedOnUtc 
        /// </summary>
        public DateTime CreatedOnUtc { get; set; }
    }
}