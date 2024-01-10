using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core.Alchub.Domain.Google;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Web.Areas.Admin.Models.Settings
{
    /// <summary>
    /// Represents a vendor settings model
    /// </summary>
    public partial record VendorSettingsModel
    {
        #region Properties

        [NopResourceDisplayName("Alchub.Admin.Configuration.Settings.Vendor.DistanceRadiusValue")]
        public decimal DistanceRadiusValue { get; set; }
        public bool DistanceRadiusValue_OverrideForStore { get; set; }

        [NopResourceDisplayName("Alchub.Admin.Configuration.Settings.Vendor.DistanceUnit")]
        public int DistanceUnit { get; set; }
        public bool DistanceUnit_OverrideForStore { get; set; }
        public SelectList DistanceUnitValues { get; set; }

        [NopResourceDisplayName("Alchub.Admin.Configuration.Settings.Vendor.ShowStoreAddressInFavoriteSection")]
        public bool ShowStoreAddressInFavoriteSection { get; set; }
        public bool ShowStoreAddressInFavoriteSection_OverrideForStore { get; set; }

        #endregion
    }
}