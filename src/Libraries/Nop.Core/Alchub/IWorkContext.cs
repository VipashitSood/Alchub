using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core.Domain.Customers;

namespace Nop.Core
{
    /// <summary>
    /// Represents extended work context
    /// </summary>
    public partial interface IWorkContext
    {
        /// <summary>
        /// Gets the vendor idetifiers who are avialble within geo radius of current customers's searched location 
        /// </summary>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task<IList<int>> GetCurrentCustomerGeoRadiusVendorIdsAsync(Customer customer = null, bool applyToggleFilter = true);

        /// <summary>
        /// Gets the vendor idetifiers who are avialble within geo fence of current customers's searched location 
        /// </summary>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task<IList<int>> GetAllGeoFenceVendorIdsAsync(Customer customer = null, bool applyToggleFilter = true);

        /// <summary>
        /// Gets the current multi vendor (logged-in manager)
        /// </summary>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task<Customer> GetCurrentMultiVendorAsync();
    }
}
