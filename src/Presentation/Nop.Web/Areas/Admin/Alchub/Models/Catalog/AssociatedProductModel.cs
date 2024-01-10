using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Web.Areas.Admin.Models.Catalog
{
    /// <summary>
    /// Represents an extended associated product model
    /// </summary>
    public partial record AssociatedProductModel
    {
        #region Ctor

        public AssociatedProductModel()
        {
            AvailableSize = new List<SelectListItem>();
            AvailableContainer = new List<SelectListItem>();
        }

        #endregion

        #region Properties

        [NopResourceDisplayName("Alchub.Admin.Catalog.Products.AssociatedProducts.Fields.Size")]
        public string Size { get; set; }

        [NopResourceDisplayName("Alchub.Admin.Catalog.Products.AssociatedProducts.Fields.Container")]
        public string Container { get; set; }

        public int BaseProductId { get; set; }

        public IList<SelectListItem> AvailableSize { get; set; }
        public IList<SelectListItem> AvailableContainer { get; set; }

        #endregion
    }
}