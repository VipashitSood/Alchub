using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core.Alchub.Domain.Vendors;
using Nop.Data;

namespace Nop.Services.Alchub.Vendors
{
    public partial class VendorTimingService : IVendorTimingService
    {
        #region Fields
        private readonly IRepository<VendorTiming> _vendorTimingRepository;
        #endregion

        #region Ctor
        public VendorTimingService(IRepository<VendorTiming> vendorTimingRepository)
        {
            _vendorTimingRepository = vendorTimingRepository;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Insert a vendor timing
        /// </summary>
        /// <param name="vendorTiming"></param>
        /// <returns></returns>
        public virtual async Task InsertVendorTimingAsync(VendorTiming vendorTiming)
        {
            if (vendorTiming == null)
                throw new ArgumentNullException(nameof(vendorTiming));

            await _vendorTimingRepository.InsertAsync(vendorTiming);
        }

        /// <summary>
        /// Update a vendor timing
        /// </summary>
        /// <param name="vendorTiming"></param>
        /// <returns></returns>
        public virtual async Task UpdateVendorTimingAsync(VendorTiming vendorTiming)
        {
            if (vendorTiming == null)
                throw new ArgumentNullException(nameof(vendorTiming));

            await _vendorTimingRepository.UpdateAsync(vendorTiming);
        }

        /// <summary>
        /// Get vendor timing by identifier
        /// </summary>
        /// <param name="vendorTimingId"></param>
        /// <returns></returns>
        public virtual async Task<VendorTiming> GetVendorTimingByIdAsync(int vendorTimingId)
        {
            return await _vendorTimingRepository.GetByIdAsync(vendorTimingId);
        }

        /// <summary>
        /// Get a vendor timing by vendor identifier & day of week identifier
        /// </summary>
        /// <param name="vendorId"></param>
        /// <param name="dayId"></param>
        /// <returns></returns>
        public virtual Task<VendorTiming> GetVendorTimingByVendorIdAsync(int vendorId, int dayId)
        {
            var query = _vendorTimingRepository.Table.
                Where(x => x.VendorId == vendorId && x.DayId == dayId).
                FirstOrDefault();

            return Task.FromResult(query);
        }

        /// <summary>
        /// Get a vendor weekly timings by vendor identifier
        /// </summary>
        /// <param name="vendorId"></param>
        /// <returns></returns>
        public virtual async Task<IList<VendorTiming>> GetVendorWeeklyTimingsByVendorIdAsync(int vendorId)
        {
            return await _vendorTimingRepository.Table.Where(x => x.VendorId == vendorId).OrderBy(x => x.DayId).ToListAsync();
        }

        #endregion
    }
}
