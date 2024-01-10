using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Vendors;

namespace Nop.Services.Vendors
{
    /// <summary>
    /// Vendor service interface
    /// </summary>
    public partial interface IVendorService
    {
        #region Geo

        /// <summary>
        /// Gets the vendor idetifiers who are avialble within geo radius of current customers's searched location 
        /// </summary>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task<IList<int>> GetAvailableGeoRadiusVendorIdsAsync(Customer customer, bool applyToggleFilter = true);

        /// <summary>
        /// Gets the vendor idetifiers who are avialble within geo radius of passed coordinates location 
        /// </summary>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task<IList<int>> GetAvailableGeoRadiusVendorIdsAsync(string latlongCoordinates, IList<Vendor> allVendors = null);

        /// <summary>
        /// Gets the vendors who are avialble within geo fence of current customers's searched location 
        /// </summary>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task<IList<Vendor>> GetAvailableGeoFenceVendorsAsync(Customer customer, bool includePickupByDefault = true, Product product = null, bool applyToggleFilter = false);

        #endregion

        /// <summary>
        /// Get vendors by identifiers
        /// </summary>
        /// <param name="vendorIds">Vendor identifiers</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the vendors
        /// </returns>
        Task<IList<Vendor>> GetVendorsByIdsAsync(int[] vendorIds);

        /// <summary>
        /// Gets all cached vendors
        /// </summary>
        /// <param name="storeId">Store identifiers</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <param name="checkSlotCreated">A value indicating whether to check if vendor has created slot or not, pass false if you do not want to check slot created</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the vendors
        /// </returns>
        Task<IList<Vendor>> GetAllCachedVendorsAsync(int storeId = 0, bool showHidden = false, bool checkSlotCreated = false);

        #region Favorite Toggle filter

        /// <summary>
        /// Apply favorite vendor toggle filter
        /// </summary>
        /// <param name="vendorIds">Vendor identifiers</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the vendors
        /// </returns>
        Task<IList<Vendor>> ApplyFavoriteToggleFilterAsync(IList<Vendor> vendors, Customer customer);

        #endregion
    }
}