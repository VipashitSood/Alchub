using Nop.Core.Alchub.Domain.Google;

namespace Nop.Core.Domain.Vendors
{
    /// <summary>
    /// Alchub Vendor settings
    /// </summary>
    public partial class VendorSettings
    {
        /// <summary>
        /// Gets or sets a distance radius value to show catalog pruducts based on searched location
        /// </summary>
        public decimal DistanceRadiusValue { get; set; }

        /// <summary>
        /// Get or set a distance unit type
        /// </summary>
        public DistanceUnit DistanceUnit { get; set; }

        /// <summary>
        /// Gets or sets a value which indicates weather to shw vendor pickup address in customer's favorite section.
        /// </summary>
        public bool ShowStoreAddressInFavoriteSection { get; set; }
    }
}
