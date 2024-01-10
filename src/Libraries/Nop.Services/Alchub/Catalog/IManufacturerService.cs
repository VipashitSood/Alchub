using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Discounts;

namespace Nop.Services.Catalog
{
    /// <summary>
    /// Manufacturer service
    /// </summary>
    public partial interface IManufacturerService
    {
        /// <summary>
        /// Gets product manufacturer collection
        /// </summary>
        /// <param name="manufacturerId">Manufacturer identifier</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the product manufacturer collection
        /// </returns>
        Task<IPagedList<ProductManufacturer>> GetProductManufacturersByManufacturerIdAsync(int manufacturerId,
            int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false, bool? isMaster = null);

        /// <summary>
        /// Gets all manufacturers
        /// </summary>
        /// <param name="storeId">Store identifier; 0 if you want to get all records</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <param name="createdOrUpdatedFromUtc">Created or Updated From date time utc</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the manufacturers
        /// </returns>
        Task<IPagedList<Manufacturer>> SearchManufacturersAsync(int storeId = 0,
            bool showHidden = false,
            DateTime? createdOrUpdatedFromUtc = null,
            int pageIndex = 0,
            int pageSize = int.MaxValue);
    }
}