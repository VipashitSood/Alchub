using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Web.Areas.Admin.Models.Markup
{
    /// <summary>
    /// Represents a zone model
    /// </summary>
    public partial record CategoryMarkupModel : BaseNopEntityModel
    {
        #region Properties

        public CategoryMarkupModel()
        {
            AvailableCategories = new List<SelectListItem>();
        }

        //categories
        [NopResourceDisplayName("Admin.CategoryMarkup.Fields.Categories")]
        public int SelectedCategoryId { get; set; }
        public IList<SelectListItem> AvailableCategories { get; set; }

        [NopResourceDisplayName("Admin.CategoryMarkup.Fields.Markup")]
        public decimal Markup { get; set; }

        [NopResourceDisplayName("Admin.CategoryMarkup.Fields.CategoryName")]
        public string CategoryName { get; set; }

        #endregion
    }
}