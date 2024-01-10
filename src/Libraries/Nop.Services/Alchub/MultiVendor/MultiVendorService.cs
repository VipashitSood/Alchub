using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core.Domain.MultiVendor;
using Nop.Core.Domain.Customers;
using Nop.Data;

namespace Nop.Services.MultiVendors
{
    /// <summary>
    /// Multi vendor service
    /// </summary>
    public partial class MultiVendorService : IMultiVendorService
    {
        #region Fields
        private readonly IRepository<ManagerVendorMapping> _managerVendorMappingRepository;
        #endregion

        #region Ctor

        public MultiVendorService(IRepository<ManagerVendorMapping> managerVendorMappingRepository)
        {
            _managerVendorMappingRepository = managerVendorMappingRepository;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets a vendor identifiers by multi vendors identifier
        /// </summary>
        /// <param name="multiVendorId">Multi vendors identifiers</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the vendors identifiers
        /// </returns>
        public virtual Task<List<int>> GetVendorIdsByMultiVendorAsync(int multiVendorId)
        {
            if (multiVendorId == 0)
                return Task.FromResult(new List<int>()); 

            return Task.FromResult((from mvm in _managerVendorMappingRepository.Table
                                    where mvm.MultiVendorId == multiVendorId
                                    select mvm.VendorId).ToList());
        }


        /// <summary>
        /// Gets a multi vendor by multi vendor identifier
        /// </summary>
        /// <param name="vendorId">Vendor identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the vendor
        /// </returns>
        public virtual Task<List<ManagerVendorMapping>> GetManagerVendorMappingAsync(int vendorId=0, int multiVendorId=0)
        {
            var query = _managerVendorMappingRepository.Table;

            if (vendorId > 0)
                query = query.Where(x => x.VendorId == vendorId);
            if (multiVendorId > 0)
                query = query.Where(x=>x.MultiVendorId == multiVendorId);

            return Task.FromResult(query.ToList());
        }

        /// <summary>
        /// Delete a manager vendor mapping
        /// </summary>
        /// <param name="mapping">mapping</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task DeleteManagerVendorMappingAsync(ManagerVendorMapping mapping)
        {
            await _managerVendorMappingRepository.DeleteAsync(mapping);
        }

        /// <summary>
        /// Remove a manager vendor mapping
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <param name="vendorId">Vendor identifier</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task RemoveManagerVendorMappingAsync(Customer customer, int vendorId = 0)
        {
            if (customer is null)
                throw new ArgumentNullException(nameof(customer));

            if (vendorId is 0)
                throw new ArgumentOutOfRangeException(nameof(vendorId));

            var mapping = await _managerVendorMappingRepository.Table
                .SingleOrDefaultAsync(ccrm => ccrm.MultiVendorId == customer.Id && ccrm.VendorId == vendorId);

            if (mapping != null)
                await _managerVendorMappingRepository.DeleteAsync(mapping);
        }

        /// <summary>
        /// Inserts a manager vendor mapping
        /// </summary>
        /// <param name="mapping">mapping</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task InsertManagerVendorMappingAsync(ManagerVendorMapping mapping)
        {
            await _managerVendorMappingRepository.InsertAsync(mapping);
        }

        /// <summary>
        /// Updates the manager vendor mapping
        /// </summary>
        /// <param name="mapping">mapping</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task UpdateManagerVendorMappingAsync(ManagerVendorMapping mapping)
        {
            await _managerVendorMappingRepository.UpdateAsync(mapping);
        }
        #endregion
    }
}