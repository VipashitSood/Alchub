using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core.Alchub.Domain.Vendors;

namespace Nop.Services.Alchub.Vendors
{
    public partial interface IVendorTimingService
    {
        /// <summary>
        /// Insert a vendor timing
        /// </summary>
        /// <param name="vendorTiming"></param>
        /// <returns></returns>
        Task InsertVendorTimingAsync(VendorTiming vendorTiming);

        /// <summary>
        /// Update a vendor timing
        /// </summary>
        /// <param name="vendorTiming"></param>
        /// <returns></returns>
        Task UpdateVendorTimingAsync(VendorTiming vendorTiming);

        /// <summary>
        /// Get vendor timing by identifier
        /// </summary>
        /// <param name="vendorTimingId"></param>
        /// <returns></returns>
        Task<VendorTiming> GetVendorTimingByIdAsync(int vendorTimingId);

        /// <summary>
        /// Get a vendor timing by vendor identifier & day of week identifier
        /// </summary>
        /// <param name="vendorId"></param>
        /// <param name="dayId"></param>
        /// <returns></returns>
        Task<VendorTiming> GetVendorTimingByVendorIdAsync(int vendorId, int dayId);

        /// <summary>
        /// Get a vendor weekly timings by vendor identifier
        /// </summary>
        /// <param name="vendorId"></param>
        /// <returns></returns>
        Task<IList<VendorTiming>> GetVendorWeeklyTimingsByVendorIdAsync(int vendorId);
    }
}
