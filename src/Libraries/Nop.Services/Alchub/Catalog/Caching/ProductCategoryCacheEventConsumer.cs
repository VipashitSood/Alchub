using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Core.Domain.Catalog;
using Nop.Services.Caching;
using Nop.Services.Catalog;

namespace Nop.Services.Catalog.Caching
{
    /// <summary>
    /// Represents a product category cache event consumer
    /// </summary>
    public partial class ProductCategoryCacheEventConsumer : CacheEventConsumer<ProductCategory>
    {
        /// <summary>
        /// Clear cache data
        /// </summary>
        /// <param name="entity">Entity</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        protected override async Task ClearCacheAsync(ProductCategory entity)
        {
            await RemoveByPrefixAsync(NopCatalogDefaults.ProductCategoriesByProductPrefix, entity.ProductId);
            await RemoveByPrefixAsync(NopCatalogDefaults.CategoryProductsNumberPrefix);
            await RemoveByPrefixAsync(NopCatalogDefaults.ProductPricePrefix, entity.ProductId);
            await RemoveByPrefixAsync(NopCatalogDefaults.CategoryFeaturedProductsIdsPrefix, entity.CategoryId);
            await RemoveAsync(NopCatalogDefaults.SpecificationAttributeOptionsByCategoryCacheKey, entity.CategoryId.ToString());
            await RemoveAsync(NopCatalogDefaults.ManufacturersByCategoryCacheKey, entity.CategoryId.ToString());

            //++ alchub
            await RemoveByPrefixAsync(NopCatalogDefaults.ApiAllFilterDataPrefix, entity.CategoryId);
            //custom product count
            await RemoveByPrefixAsync(NopCatalogDefaults.CategoryProductsNumberByVendorPrefix);
            //-- alchub
        }
    }
}
