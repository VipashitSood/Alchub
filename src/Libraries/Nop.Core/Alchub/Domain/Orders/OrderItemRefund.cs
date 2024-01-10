using System;

namespace Nop.Core.Domain.Orders
{
    /// <summary>
    /// Represents a Order Cancel
    /// </summary>
    public partial class OrderItemRefund : BaseEntity
    {

        /// <summary>
        /// Gets or sets the orderItemId
        /// </summary>
        public int OrderId { get; set; }

        /// <summary>
        /// Gets or sets the orderItemId
        /// </summary>
        public int OrderItemId { get; set; }

        /// <summary>
        /// Gets or sets the vendor id
        /// </summary>
        public int VendorId { get; set; }

        /// <summary>
        /// Gets or sets the Admin Commission
        /// </summary>
        public decimal AdminCommission { get; set; }

        /// <summary>
        /// Gets or sets the PriceIncltax
        /// </summary>
        public decimal PriceIncltax { get; set; }

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
        /// Gets or sets the Delivery Subtotal
        /// </summary>
        public decimal TaxFee { get; set; }

        /// <summary>
        /// Gets or sets the Total Amount 
        /// </summary>
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// Gets or sets the Is Refund 
        /// </summary>
        public bool IsRefunded { get; set; }

        public DateTime CreatedOnUtc { get; set; }
    }
}