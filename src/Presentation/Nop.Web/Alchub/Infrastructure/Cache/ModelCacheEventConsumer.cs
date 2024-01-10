using System.Threading.Tasks;
using Nop.Core.Domain.Catalog;
using Nop.Core.Events;

namespace Nop.Web.Infrastructure.Cache
{
    /// <summary>
    /// Model cache event consumer (used for caching of presentation layer models)
    /// </summary>
    public partial class ModelCacheEventConsumer
    {
        #region Methods

        #region  Manufacturers

        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task HandleEventAsync(EntityInsertedEvent<Manufacturer> eventMessage)
        {
            await _staticCacheManager.RemoveByPrefixAsync(NopModelCacheDefaults.ManufacturerNavigationPrefixCacheKey);
            await _staticCacheManager.RemoveByPrefixAsync(NopModelCacheDefaults.SitemapPrefixCacheKey);

            //++Alchub
            await _staticCacheManager.RemoveByPrefixAsync(NopModelCacheDefaults.HomeManufacturersPrefixCacheKey);
        }

        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task HandleEventAsync(EntityUpdatedEvent<Manufacturer> eventMessage)
        {
            await _staticCacheManager.RemoveByPrefixAsync(NopModelCacheDefaults.ManufacturerNavigationPrefixCacheKey);
            await _staticCacheManager.RemoveByPrefixAsync(NopModelCacheDefaults.SitemapPrefixCacheKey);
            await _staticCacheManager.RemoveByPrefixAsync(string.Format(NopModelCacheDefaults.ManufacturerPicturePrefixCacheKeyById, eventMessage.Entity.Id));

            //++Alchub
            await _staticCacheManager.RemoveByPrefixAsync(NopModelCacheDefaults.HomeManufacturersPrefixCacheKey);
        }

        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task HandleEventAsync(EntityDeletedEvent<Manufacturer> eventMessage)
        {
            await _staticCacheManager.RemoveByPrefixAsync(NopModelCacheDefaults.ManufacturerNavigationPrefixCacheKey);
            await _staticCacheManager.RemoveByPrefixAsync(NopModelCacheDefaults.SitemapPrefixCacheKey);

            //++Alchub
            await _staticCacheManager.RemoveByPrefixAsync(NopModelCacheDefaults.HomeManufacturersPrefixCacheKey);
        }

        #endregion

        #endregion
    }
}