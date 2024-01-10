using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Services.Catalog;
using Nop.Web.Framework.Models;

namespace Nop.Web.Models.Catalog
{
    /// <summary>
    /// Represents a products price range filter model
    /// </summary>
    public partial record PriceRangeFilterModel : BaseNopModel
    {
        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        //public PriceRangeFilterModel()
        //{
        //    SelectedPriceRange = new PriceRangeModel();
        //    AvailablePriceRange = new PriceRangeModel();
        //    Items = new List<PriceRangeFilterItem>();
        //}

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether filtering is enabled
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets the selected price range
        /// </summary>
        public PriceRangeModel SelectedPriceRange { get; set; }

        /// <summary>
        /// Gets or sets the available price range
        /// </summary>
        public PriceRangeModel AvailablePriceRange { get; set; }

        #endregion
    }
}
