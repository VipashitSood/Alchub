namespace Nop.Core.Domain.DeliveryFees
{
    /// <summary>
    /// Represents Vendor wise Delivery Fee entity
    /// </summary>
    public partial class VendorWiseDeliveryFee : BaseEntity
    {
        /// <summary>
        /// Gets or sets Vendor Id
        /// </summary>
        public int VendorId { get; set; }

        /// <summary>
        /// Gets or sets Vendor Name
        /// </summary>
        public string VendorName { get; set; }

        /// <summary>
        /// Gets or sets Delivery Fee
        /// </summary>
        public decimal DeliveryFeeValue { get; set; }

        /// <summary>
        /// Gets or sets Delivery Fee
        /// </summary>
        public string DeliveryFee { get; set; }
    }
}