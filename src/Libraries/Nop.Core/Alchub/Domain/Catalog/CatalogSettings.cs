namespace Nop.Core.Domain.Catalog
{
    /// <summary>
    /// Extended Catalog settings
    /// </summary>
    public partial class CatalogSettings
    {
        /// <summary>
        /// Gets or sets a value indicating wether to show fastest slot in product listing (product box) or not. Disable for performance enahncement.
        /// </summary>
        public bool ShowFastestSlotOnCatalogPage { get; set; }

        #region Filter

        /// <summary>
        /// Gets or set the value of filter price range option 1
        /// </summary>
        public int FilterPriceRangeOption1 { get; set; }

        /// <summary>
        /// Gets or set the value of filter price range option 2
        /// </summary>
        public int FilterPriceRangeOption2 { get; set; }

        /// <summary>
        /// Gets or set the value of filter price range option 3
        /// </summary>
        public int FilterPriceRangeOption3 { get; set; }

        /// <summary>
        /// Get or set a value indicating whether the vendor filtering is enabled on catalog pages
        /// </summary>
        public bool EnableVendorFiltering { get; set; }

        /// <summary>
        /// Get or set a value indicating whether the category filtering is enabled on catalog pages
        /// </summary>
        public bool EnableCategoryFiltering { get; set; }

        #endregion

        /// <summary>
        /// Gets or sets a number of Manufacturers on home page
        /// </summary>
        public int NumberOfManufacturersOnHomepage { get; set; }

        #region Elastic search

        /// <summary>
        /// Get or set a value indicating whether the catalog data will be showned using elastic search or using olad Linq method.
        /// </summary>
        public bool EnableElasticSearch { get; set; }

        #endregion
    }
}
