using Nop.Web.Framework.Models;

namespace Nop.Plugin.Api.Models.ProductsParameters
{
    /// <summary>
    /// Represents a products price range filter model
    /// </summary>
    public partial record PriceRangeFilterModel : BaseNopModel
    {
        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether filtering is enabled
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets the available price range
        /// </summary>
        public PriceRangeModel AvailablePriceRange { get; set; }

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        public PriceRangeFilterModel()
        {
            AvailablePriceRange = new PriceRangeModel();
        }

        #endregion
    }
}
