using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Core.Alchub.Domain.Vendors;
using Nop.Core.Domain.Vendors;
using Nop.Data;

namespace Nop.Services.Alchub.Vendors
{
    /// <summary>
    /// Represents a FavoriteVendorService
    /// </summary>
    public partial class FavoriteVendorService : IFavoriteVendorService
    {
        #region Fields

        private readonly IRepository<FavoriteVendor> _favoriteVendorRepository;
        private readonly IRepository<Vendor> _vendorRepository;

        #endregion

        #region Ctor

        public FavoriteVendorService(IRepository<FavoriteVendor> favoriteVendorRepository,
            IRepository<Vendor> vendorRepository)
        {
            _favoriteVendorRepository = favoriteVendorRepository;
            _vendorRepository = vendorRepository;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Insert a favorite vendor
        /// </summary>
        /// <param name="favoriteVendor"></param>
        /// <returns></returns>
        public virtual async Task InsertFavoriteVendorAsync(FavoriteVendor favoriteVendor)
        {
            if (favoriteVendor == null)
                throw new ArgumentNullException(nameof(favoriteVendor));

            await _favoriteVendorRepository.InsertAsync(favoriteVendor);
        }

        /// <summary>
        /// Delete a favorite vendor
        /// </summary>
        /// <param name="favoriteVendor"></param>
        /// <returns></returns>
        public virtual async Task DeleteFavoriteVendorAsync(FavoriteVendor favoriteVendor)
        {
            if (favoriteVendor == null)
                throw new ArgumentNullException(nameof(favoriteVendor));

            await _favoriteVendorRepository.DeleteAsync(favoriteVendor);
        }

        /// <summary>
        /// Get favorite vendor by identifier
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual async Task<FavoriteVendor> GetFavoriteVendorByIdAsync(int id)
        {
            return await _favoriteVendorRepository.GetByIdAsync(id);
        }

        /// <summary>
        /// Get favorite vendor by vendor identifier & customer identifier 
        /// </summary>
        /// <param name="vendorId"></param>
        /// <param name="customerId"></param>
        /// <returns></returns>
        public virtual Task<FavoriteVendor> GetFavoriteVendorByVendorIdAsync(int vendorId, int customerId)
        {
            var query = _favoriteVendorRepository.Table.
                Where(x => x.VendorId == vendorId && x.CustomerId == customerId).
                FirstOrDefault();

            return Task.FromResult(query);
        }

        /// <summary>
        /// Is favorite vendor
        /// </summary>
        /// <param name="vendorId"></param>
        /// <param name="customerId"></param>
        /// <returns></returns>
        public virtual Task<bool> IsFavoriteVendorAsync(int vendorId, int customerId)
        {
            var query = _favoriteVendorRepository.Table.
                Where(x => x.VendorId == vendorId && x.CustomerId == customerId).
                Any();

            return Task.FromResult(query);
        }

        /// <summary>
        /// Get favorite vendor by customer identifier
        /// </summary>
        /// <param name="customerId"></param>
        /// <returns></returns>
        public virtual async Task<IList<FavoriteVendor>> GetFavoriteVendorByCustomerIdAsync(int customerId)
        {
            return await (from v in _vendorRepository.Table
                          join fv in _favoriteVendorRepository.Table on v.Id equals fv.VendorId
                          where fv.CustomerId == customerId && !v.Deleted && v.Active
                          select fv).ToListAsync();
        }

        #endregion
    }
}
