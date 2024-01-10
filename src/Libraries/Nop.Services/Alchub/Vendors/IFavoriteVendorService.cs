using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Core.Alchub.Domain.Vendors;

namespace Nop.Services.Alchub.Vendors
{
    /// <summary>
    /// Represents a IFavoriteVendorService
    /// </summary>
    public partial interface IFavoriteVendorService
    {
        /// <summary>
        /// Insert a favorite vendor
        /// </summary>
        /// <param name="favoriteVendor"></param>
        /// <returns></returns>
        Task InsertFavoriteVendorAsync(FavoriteVendor favoriteVendor);

        /// <summary>
        /// Delete a favorite vendor
        /// </summary>
        /// <param name="favoriteVendor"></param>
        /// <returns></returns>
        Task DeleteFavoriteVendorAsync(FavoriteVendor favoriteVendor);

        /// <summary>
        /// Get favorite vendor by identifier
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<FavoriteVendor> GetFavoriteVendorByIdAsync(int id);

        /// <summary>
        /// Get favorite vendor by vendor identifier & customer identifier 
        /// </summary>
        /// <param name="vendorId"></param>
        /// <param name="customerId"></param>
        /// <returns></returns>
        Task<FavoriteVendor> GetFavoriteVendorByVendorIdAsync(int vendorId, int customerId);

        /// <summary>
        /// Is favorite vendor
        /// </summary>
        /// <param name="vendorId"></param>
        /// <param name="customerId"></param>
        /// <returns></returns>
        Task<bool> IsFavoriteVendorAsync(int vendorId, int customerId);

        /// <summary>
        /// Get favorite vendor by customer identifier
        /// </summary>
        /// <param name="customerId"></param>
        /// <returns></returns>
        Task<IList<FavoriteVendor>> GetFavoriteVendorByCustomerIdAsync(int customerId);
    }
}
