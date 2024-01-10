using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Discounts;
using Nop.Data;
using Nop.Services.Customers;
using Nop.Services.Discounts;
using Nop.Services.Security;
using Nop.Services.Stores;

namespace Nop.Services.Catalog
{
    /// <summary>
    /// Manufacturer service
    /// </summary>
    public partial class ManufacturerService : IManufacturerService
    {
        #region Methods

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
        public virtual async Task<IPagedList<ProductManufacturer>> GetProductManufacturersByManufacturerIdAsync(int manufacturerId,
            int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false, bool? isMaster = null)
        {
            if (manufacturerId == 0)
                return new PagedList<ProductManufacturer>(new List<ProductManufacturer>(), pageIndex, pageSize);

            var query = from pm in _productManufacturerRepository.Table
                        join p in _productRepository.Table on pm.ProductId equals p.Id
                        where pm.ManufacturerId == manufacturerId && !p.Deleted &&
                        //master filter
                        (!isMaster.HasValue || p.IsMaster == isMaster.Value)
                        orderby pm.DisplayOrder, pm.Id
                        select pm;

            if (!showHidden)
            {
                var manufacturersQuery = _manufacturerRepository.Table.Where(m => m.Published);

                //apply store mapping constraints
                var store = await _storeContext.GetCurrentStoreAsync();
                manufacturersQuery = await _storeMappingService.ApplyStoreMapping(manufacturersQuery, store.Id);

                //apply ACL constraints
                var customer = await _workContext.GetCurrentCustomerAsync();
                manufacturersQuery = await _aclService.ApplyAcl(manufacturersQuery, customer);

                query = query.Where(pm => manufacturersQuery.Any(m => m.Id == pm.ManufacturerId));
            }

            return await query.ToPagedListAsync(pageIndex, pageSize);
        }

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
        public virtual async Task<IPagedList<Manufacturer>> SearchManufacturersAsync(int storeId = 0,
            bool showHidden = false,
            DateTime? createdOrUpdatedFromUtc = null,
            int pageIndex = 0,
            int pageSize = int.MaxValue)
        {
            return await _manufacturerRepository.GetAllPagedAsync(async query =>
            {
                if (!showHidden)
                    query = query.Where(m => m.Published);

                //apply store mapping constraints
                query = await _storeMappingService.ApplyStoreMapping(query, storeId);

                //exclude deleted
                //query = query.Where(m => !m.Deleted);

                //from date time
                if (createdOrUpdatedFromUtc.HasValue)
                    query = query.Where(m => m.CreatedOnUtc > createdOrUpdatedFromUtc || m.UpdatedOnUtc > createdOrUpdatedFromUtc);

                return query.OrderBy(m => m.DisplayOrder).ThenBy(m => m.Id);
            }, pageIndex, pageSize);
        }

        /// <summary>
        /// Deletes a manufacturer
        /// </summary>
        /// <param name="manufacturer">Manufacturer</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task DeleteManufacturerAsync(Manufacturer manufacturer)
        {
            //++Alchub

            //set updateOnUtc to include product in sync cataLOG system.
            manufacturer.UpdatedOnUtc = DateTime.UtcNow;

            //--Alchub

            await _manufacturerRepository.DeleteAsync(manufacturer);
        }

        /// <summary>
        /// Delete manufacturers
        /// </summary>
        /// <param name="manufacturers">Manufacturers</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task DeleteManufacturersAsync(IList<Manufacturer> manufacturers)
        {
            foreach(var manufacturer in manufacturers)
            {
                //++Alchub

                //set updateOnUtc to include product in sync cataLOG system.
                manufacturer.UpdatedOnUtc = DateTime.UtcNow;

                //--Alchub
            }

            await _manufacturerRepository.DeleteAsync(manufacturers);
        }

        #endregion
    }
}