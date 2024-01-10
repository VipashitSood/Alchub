using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Web.Areas.Admin.Models.Settings
{
    /// <summary>
    /// Represents a extended catalog settings model
    /// </summary>
    public partial record CatalogSettingsModel
    {
        #region Properties

        [NopResourceDisplayName("Alchub.Admin.Configuration.Settings.Catalog.ShowFastestSlotOnCatalogPage")]
        public bool ShowFastestSlotOnCatalogPage { get; set; }
        public bool ShowFastestSlotOnCatalogPage_OverrideForStore { get; set; }

        #region Filters

        [NopResourceDisplayName("Alchub.Admin.Configuration.Settings.Catalog.Filter.PriceRange.Option1")]
        public int FilterPriceRangeOption1 { get; set; }
        public bool FilterPriceRangeOption1_OverrideForStore { get; set; }

        [NopResourceDisplayName("Alchub.Admin.Configuration.Settings.Catalog.Filter.PriceRange.Option2")]
        public int FilterPriceRangeOption2 { get; set; }
        public bool FilterPriceRangeOption2_OverrideForStore { get; set; }

        [NopResourceDisplayName("Alchub.Admin.Configuration.Settings.Catalog.Filter.PriceRange.Option3")]
        public int FilterPriceRangeOption3 { get; set; }
        public bool FilterPriceRangeOption3_OverrideForStore { get; set; }

        #endregion

        [NopResourceDisplayName("Alchub.Admin.Configuration.Settings.Catalog.NumberOfManufacturersOnHomepage")]
        public int NumberOfManufacturersOnHomepage { get; set; }
        public bool NumberOfManufacturersOnHomepage_OverrideForStore { get; set; }

        #endregion
    }
}