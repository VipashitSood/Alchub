using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;

namespace Nop.Web.Models.Catalog
{
    /// <summary>
    /// Represents a manufacturer filter model
    /// </summary>
    public partial record CategoryFilterModel : BaseNopModel
    {
        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether filtering is enabled
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets the filtrable categories
        /// </summary>
        public IList<CategoryModel> Categories { get; set; }

        /// <summary>
        /// Gets or sets the filtrable sub categories
        /// </summary>
        public IList<CategoryModel> SubCategories { get; set; }

        /// <summary>
        /// Gets or sets the filtrable sub categories
        /// </summary>
        public IList<CategoryModel> ChildCategories { get; set; }

        #endregion

        #region Ctor

        public CategoryFilterModel()
        {
            Categories = new List<CategoryModel>();
            SubCategories = new List<CategoryModel>();
            ChildCategories = new List<CategoryModel>();
        }

        #endregion
    }
}
