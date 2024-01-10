using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core.Domain.MultiVendor;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Vendors;

namespace Nop.Services.MultiVendors
{
    public interface IMultiVendorService
    {
        /// <summary>
        /// Gets a vendor identifiers by multi vendors identifier
        /// </summary>
        /// <param name="multiVendorId">Multi vendors identifiers</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the vendors identifiers
        /// </returns>
        Task<List<int>> GetVendorIdsByMultiVendorAsync(int multiVendorId);

        /// <summary>
        /// Gets a multi vendor by multi vendor identifier
        /// </summary>
        /// <param name="vendorId">Vendor identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the vendor
        /// </returns>
        Task<List<ManagerVendorMapping>> GetManagerVendorMappingAsync(int vendorId = 0, int multiVendorId = 0);

        /// <summary>
        /// Delete a manager vendor mapping
        /// </summary>
        /// <param name="mapping">mapping</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task DeleteManagerVendorMappingAsync(ManagerVendorMapping mapping);

        /// <summary>
        /// Remove a manager vendor mapping
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <param name="vendorId">Vendor identifier</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task RemoveManagerVendorMappingAsync(Customer customer, int vendorId = 0);

        /// <summary>
        /// Inserts a manager vendor mapping
        /// </summary>
        /// <param name="mapping">mapping</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task InsertManagerVendorMappingAsync(ManagerVendorMapping mapping);

        /// <summary>
        /// Updates the manager vendor mapping
        /// </summary>
        /// <param name="mapping">mapping</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task UpdateManagerVendorMappingAsync(ManagerVendorMapping mapping);
    }
}
