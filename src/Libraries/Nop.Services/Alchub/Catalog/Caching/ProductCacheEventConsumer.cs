using System.Linq;
using System.Threading.Tasks;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Orders;
using Nop.Services.Caching;
using Nop.Services.Discounts;

namespace Nop.Services.Catalog.Caching
{
    /// <summary>
    /// Represents a product cache event consumer
    /// </summary>
    public partial class ProductCacheEventConsumer
    {
        private readonly IProductService _productService;
        public ProductCacheEventConsumer(IProductService productService)
        {
            _productService = productService;
        }

        /// <summary>
        /// Clear cache data
        /// </summary>
        /// <param name="entity">Entity</param>
        /// <param name="entityEventType">Entity event type</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        protected override async Task ClearCacheAsync(Product entity, EntityEventType entityEventType)
        {
            await RemoveByPrefixAsync(NopCatalogDefaults.ProductManufacturersByProductPrefix, entity);
            await RemoveAsync(NopCatalogDefaults.ProductsHomepageCacheKey);
            await RemoveByPrefixAsync(NopCatalogDefaults.ProductPricePrefix, entity);
            await RemoveByPrefixAsync(NopEntityCacheDefaults<ShoppingCartItem>.AllPrefix);
            await RemoveByPrefixAsync(NopCatalogDefaults.FeaturedProductIdsPrefix);

            if (entityEventType == EntityEventType.Delete)
            {
                await RemoveByPrefixAsync(NopCatalogDefaults.FilterableSpecificationAttributeOptionsPrefix);
                await RemoveByPrefixAsync(NopCatalogDefaults.ManufacturersByCategoryPrefix);
            }

            await RemoveAsync(NopDiscountDefaults.AppliedDiscountsCacheKey, nameof(Product), entity);

            //Alchub custom
            await RemoveByPrefixAsync(NopCatalogDefaults.SameUpcProductsPrefix, entity.UPCCode);

            //picture cache clear on basic of UPCCODE
            var pictures = await _productService.GetProductPicturesByProductIdAsync(entity.Id);
            if (pictures != null && pictures.Any())
            {
                foreach (var picture in pictures)
                {
                    await RemoveByPrefixAsync(NopCatalogDefaults.UpccodeProductPicturePrefix, picture.Id);
                }                
            }

            await base.ClearCacheAsync(entity, entityEventType);
        }
    }
}
