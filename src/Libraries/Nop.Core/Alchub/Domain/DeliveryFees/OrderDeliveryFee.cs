namespace Nop.Core.Domain.DeliveryFees
{
    /// <summary>
    /// Represents Order Delivery Fee entity
    /// </summary>
    public partial class OrderDeliveryFee : BaseEntity
    {
        /// <summary>
        /// Gets or sets Order Id
        /// </summary>
        public int OrderId { get; set; }

        /// <summary>
        /// Gets or sets Vendor Id
        /// </summary>
        public int VendorId { get; set; }

        /// <summary>
        /// Gets or sets the Delivery Fee
        /// </summary>
        public decimal DeliveryFee { get; set; }
    }
}