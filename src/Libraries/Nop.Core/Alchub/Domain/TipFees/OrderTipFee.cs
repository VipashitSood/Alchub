namespace Nop.Core.Domain.TipFees
{
    /// <summary>
    /// Represents Order Tip Fee entity
    /// </summary>
    public partial class OrderTipFee : BaseEntity
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
        /// Gets or sets the Tip Fee
        /// </summary>
        public decimal TipFee { get; set; }
    }
}