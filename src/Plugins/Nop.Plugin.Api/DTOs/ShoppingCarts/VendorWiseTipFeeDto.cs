namespace Nop.Plugin.Api.DTOs.ShoppingCarts
{
    /// <summary>
    /// Represents Vendor wise Tip Fee entity
    /// </summary>
    public partial class VendorWiseTipFeeDto
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
        /// Gets or sets Tip Fee
        /// </summary>
        public decimal TipFeeValue { get; set; }

        /// <summary>
        /// Gets or sets Tip Fee
        /// </summary>
        public string TipFee { get; set; }
    }
}