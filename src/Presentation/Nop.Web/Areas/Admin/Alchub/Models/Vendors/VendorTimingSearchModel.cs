using Nop.Web.Framework.Models;

namespace Nop.Web.Areas.Admin.Models.Vendors
{
    public partial record VendorTimingSearchModel : BaseSearchModel
    {
        public int VendorId { get; set; }
    }
}
