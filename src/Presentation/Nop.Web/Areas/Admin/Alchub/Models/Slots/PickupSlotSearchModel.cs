using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Web.Areas.Admin.Models.Slots
{
    /// <summary>
    /// Represents a zone search model
    /// </summary>
    public partial record PickupSlotSearchModel : BaseSearchModel
    {
        #region Properties
        public PickupSlotSearchModel()
        {
            VendorList = new List<SelectListItem>();
            ActiveList = new List<SelectListItem>();
        }

        [NopResourceDisplayName("Admin.ContentManagement.Zone.List.SelectedZoneId")]
        public string ZoneName { get; set; }

        [NopResourceDisplayName("Admin.ContentManagement.Zone.List.SelectedVendorId")]
        public int SelectedVendorId { get; set; }
        public IList<SelectListItem> VendorList { get; set; }

        [NopResourceDisplayName("Admin.ContentManagement.Zone.List.SelectedActiveId")]
        public int SelectedActiveId { get; set; }
        public IList<SelectListItem> ActiveList { get; set; }

        public bool IsLoggedInAsVendor { get; set; }

        public bool IsCount { get; set; }

        #endregion
    }
}