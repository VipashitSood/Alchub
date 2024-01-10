using System;
using System.ComponentModel.DataAnnotations;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Web.Areas.Admin.Models.Vendors
{
    public partial record VendorTimingModel : BaseNopEntityModel
    {
        public int VendorId { get; set; }

        public int DayId { get; set; }

        [NopResourceDisplayName("Alchub.Admin.Vendors.VendorTiming.Fields.Day")]
        public string Day { get; set; }

        [NopResourceDisplayName("Alchub.Admin.Vendors.VendorTiming.Fields.OpenTimeUtc")]
        public TimeSpan? OpenTimeUtc { get; set; }

        [NopResourceDisplayName("Alchub.Admin.Vendors.VendorTiming.Fields.CloseTimeUtc")]
        public TimeSpan? CloseTimeUtc { get; set; }

        [NopResourceDisplayName("Alchub.Admin.Vendors.VendorTiming.Fields.DayOff")]
        public bool DayOff { get; set; }

        [NopResourceDisplayName("Alchub.Admin.Vendors.VendorTiming.Fields.OpenTimeUtc")]
        [UIHint("TimeNullable")]
        public string OpenTimeStr { get; set; }

        [NopResourceDisplayName("Alchub.Admin.Vendors.VendorTiming.Fields.CloseTimeUtc")]
        [UIHint("TimeNullable")]
        public string CloseTimeStr { get; set; }
    }
}
