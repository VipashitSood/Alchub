using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;

namespace Nop.Plugin.Api.Models.ProductsParameters
{
    /// <summary>
    /// Represents a manufacturer filter model
    /// </summary>
    public partial record ManufacturerFilterModel
    {
        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether filtering is enabled
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets the filtrable manufacturers
        /// </summary>
        public IList<SelectListItem> Manufacturers { get; set; }

        #endregion

        #region Ctor

        public ManufacturerFilterModel()
        {
            Manufacturers = new List<SelectListItem>();
        }

        #endregion
    }
}
