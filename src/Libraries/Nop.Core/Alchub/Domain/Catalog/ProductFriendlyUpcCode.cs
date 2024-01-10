namespace Nop.Core.Alchub.Domain.Catalog
{
    /// <summary>
    /// Represents a product friendly upc code
    /// </summary>
    public partial class ProductFriendlyUpcCode : BaseEntity
    {
        /// <summary>
        /// Gets or sets the master product type identifier
        /// </summary>
        public int MasterProductId { get; set; }

        /// <summary>
        /// Gets or sets the vendor type identifier
        /// </summary>
        public int VendorId { get; set; }

        /// <summary>
        /// Gets or sets the friendly upc code
        /// </summary>
        public string FriendlyUpcCode { get; set; }
    }
}
