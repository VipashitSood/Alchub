namespace Nop.Plugin.Api.Models.ProductsParameters
{
    public class GroupedProductVariantBrifInfoModel
    {
        /// <summary>
        /// Grouped product identifiers
        /// </summary>
        public int GroupedProductId { get; set; }

        /// <summary>
        /// Variant name. (Specification attribute option name.)
        /// </summary>
        public string VariantName { get; set; }

        /// <summary>
        /// Display order
        /// </summary>
        public int DisplayOrder { get; set; }

        /// <summary>
        /// Is active (current product details page grouped product ?)
        /// </summary>
        public bool IsActive { get; set; }
    }
}
