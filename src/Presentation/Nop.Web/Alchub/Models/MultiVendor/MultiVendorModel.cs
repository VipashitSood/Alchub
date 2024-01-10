using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Web.Areas.Admin.Models.Vendors
{
    /// <summary>
    /// Represents a vendor model
    /// </summary>
    public partial record MultiVendorModel : BaseNopEntityModel
    { 
        #region Ctor

        public MultiVendorModel()
        {
            SelectedVendorIds = new List<int>();
            AvailableVendors = new List<SelectListItem>();
        }

        #endregion

        #region Properties

        [NopResourceDisplayName("Admin.MultiVendors.Fields.Name")]
        public string Name { get; set; }

        [NopResourceDisplayName("Admin.MultiVendors.Fields.Vendors")]
        public IList<int> SelectedVendorIds { get; set; }

        public IList<SelectListItem> AvailableVendors { get; set; }

        #endregion
    }
}