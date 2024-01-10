using System;
using System.Threading.Tasks;
using Nop.Core.Alchub.Domain.Vendors;
using Nop.Core.Domain.Vendors;
using Nop.Web.Areas.Admin.Models.Vendors;

namespace Nop.Web.Areas.Admin.Factories
{
    public partial interface IVendorModelFactory
    {
        /// <summary>
        /// Prepare a vendor timing model
        /// </summary>
        /// <param name="model"></param>
        /// <param name="vendor"></param>
        /// <param name="vendorTiming"></param>
        /// <param name="dayId"></param>
        /// <param name="excludeProperties"></param>
        /// <returns></returns>
        Task<VendorTimingModel> PrepareVendorTimingModelAsync(VendorTimingModel model,
            Vendor vendor, VendorTiming vendorTiming, int dayId, bool excludeProperties = false);

        /// <summary>
        /// Prepare vendor timing list model
        /// </summary>
        /// <param name="searchModel"></param>
        /// <param name="vendor"></param>
        /// <returns></returns>
        Task<VendorTimingListModel> PrepareVendorTimingListModel(VendorTimingSearchModel searchModel, Vendor vendor);
    }
}
