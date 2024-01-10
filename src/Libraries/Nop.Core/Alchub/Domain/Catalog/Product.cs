namespace Nop.Core.Domain.Catalog
{
    /// <summary>
    /// Represents a product
    /// </summary>
    public partial class Product
    {
        /// <summary>
        /// IsMaster
        /// </summary>
        public bool IsMaster { get; set; }

        /// <summary>
        /// Code
        /// </summary>
        public string UPCCode { get; set; }

        /// <summary>
        /// Gets or sets a size.
        /// This value is used in associated products (used with "grouped" products)
        /// </summary>
        public string Size { get; set; }

        /// <summary>
        /// Gets or sets a Containers.
        /// This value is used in associated products (used with "grouped" products)
        /// </summary>
        public string Container { get; set; }

        /// <summary>
        /// OverridePrice
        /// </summary>
        public bool OverridePrice { get; set; }

        /// <summary>
        /// OverrideStock
        /// </summary>
        public bool OverrideStock { get; set; }

        /// <summary>
        /// OverrideNegativeStock
        /// </summary>
        public bool OverrideNegativeStock { get; set; }
        /// <summary>
        /// ImageUrl
        /// </summary>
        public string ImageUrl { get; set; }
        /// <summary>
        /// IsAlcohol
        /// </summary>
        public bool IsAlcohol { get; set; }
    }
}