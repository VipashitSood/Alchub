using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Media;
using Nop.Services.Caching;
using Nop.Services.Catalog;
using System.Threading.Tasks;

namespace Nop.Services.Media.Caching
{
    /// <summary>
    /// Represents a picture cache event consumer
    /// </summary>
    public partial class ProductPictureCacheEventConsumer : CacheEventConsumer<ProductPicture>
    {
        /// <summary>
        /// Clear cache data
        /// </summary>
        /// <param name="entity">Entity</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        protected override async Task ClearCacheAsync(ProductPicture entity)
        {
            await RemoveByPrefixAsync(NopCatalogDefaults.UpccodeProductPicturePrefix, entity.PictureId);
        }
    }
}
