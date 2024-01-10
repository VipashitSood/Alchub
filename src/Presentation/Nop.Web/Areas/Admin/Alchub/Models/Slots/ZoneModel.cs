using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using System;
using System.Collections.Generic;

namespace Nop.Web.Areas.Admin.Models.Slots
{
    /// <summary>
    /// Represents a zone model
    /// </summary>
    public partial record ZoneModel : BaseNopEntityModel
    {
        public ZoneModel()
        {
            VendorList = new List<SelectListItem>();
        }
        #region Properties

        [NopResourceDisplayName("Admin.ContentManagement.Zone.List.SelectedVendorId")]
        public int SelectedVendorId { get; set; }
        public IList<SelectListItem> VendorList { get; set; }

        [NopResourceDisplayName("Admin.Zones.Fields.Name")]
        public string Name { get; set; }

        [NopResourceDisplayName("Admin.Zones.Fields.CreatedOn")]
        public DateTime CreatedOn { get; set; }

        [NopResourceDisplayName("Admin.Zones.Fields.IsActive")]
        public bool IsActive { get; set; }

        public bool IsLoggedInAsVendor { get; set; }

        [NopResourceDisplayName("Admin.Zones.Fields.VendorName")]
        public string VendorName { get; set; }

        public int CreatedBy { get; set; }

        #endregion
    }
}