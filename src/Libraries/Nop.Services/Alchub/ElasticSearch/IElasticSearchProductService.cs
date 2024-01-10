using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Core.Domain.Catalog;

namespace Nop.Services.Alchub.ElasticSearch
{
    public interface IElasticSearchProductService
    {
        Task<IList<ElasticSearchProductServiceResult>> GetAllProductsToSyncElasticCacheAsync(int page, long noOfProducts);
        /// <summary>
        /// Get All product to Sync elastic cache
        /// </summary>
        /// <returns></returns>
        Task<Master_products_result> SyncAllProducts(int page, long noOfProducts);
        Task<IList<ElasticSearchProductServiceResult>> GetAllVendorProductsToSyncElasticCacheAsync(string upcCode, int page, long noOfProducts);
    }
}
