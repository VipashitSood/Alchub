using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Web.Areas.Admin.Alchub.Models.Catalog
{
    /// <summary>
    /// Represents a product friendly upc search model
    /// </summary>
    public partial record ProductFriendlyUpcSearchModel : BaseSearchModel
    {
        #region Ctor

        public ProductFriendlyUpcSearchModel()
        {
            AvailableCategories = new List<SelectListItem>();
        }

        #endregion

        #region Properties

        [NopResourceDisplayName("Alchub.Admin.Catalog.ProductFriendlyUpc.List.SearchProductName")]
        public string SearchProductName { get; set; }

        [NopResourceDisplayName("Alchub.Admin.Catalog.ProductFriendlyUpc.List.SearchFriendlyUpcCode")]
        public string SearchFriendlyUpcCode { get; set; }

        [NopResourceDisplayName("Alchub.Admin.Catalog.ProductFriendlyUpc.List.SearchCategory")]
        public int SearchCategoryId { get; set; }

        public IList<SelectListItem> AvailableCategories { get; set; }

        #endregion
    }
}
