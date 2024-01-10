using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Web.Areas.Admin.Models.Markup
{
    /// <summary>
    /// Represents a zone search model
    /// </summary>
    public partial record CategoryMarkupSearchModel : BaseSearchModel
    {
        #region Properties
        public CategoryMarkupSearchModel()
        {
            CategoryList = new List<SelectListItem>();
        }

        [NopResourceDisplayName("Admin.CategoryMarkup.List.CategoryId")]
        public int CategoryId { get; set; }
        public IList<SelectListItem> CategoryList { get; set; }
        public bool IsLoggedInAsVendor { get; set; }
        public int VendorId { get; set; }

        #endregion
    }
}