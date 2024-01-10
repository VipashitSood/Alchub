using System.Linq;
using System.Threading.Tasks;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Events;
using Nop.Plugin.Widgets.JCarousel.Domain;
using Nop.Plugin.Widgets.JCarousel.Services;
using Nop.Services.Configuration;
using Nop.Services.Events;

namespace Nop.Plugin.Widgets.JCarousel.Infrastructure.Cache
{
    /// <summary>
    /// Model cache event consumer (used for caching of presentation layer models)
    /// </summary>
    public partial class ModelCacheEventConsumer :
        //jcarousel record
        IConsumer<EntityInsertedEvent<JCarouselLog>>,
        IConsumer<EntityUpdatedEvent<JCarouselLog>>,
        IConsumer<EntityDeletedEvent<JCarouselLog>>,
        //jcarousel product
        IConsumer<EntityInsertedEvent<ProductJCarouselMapping>>,
        IConsumer<EntityDeletedEvent<ProductJCarouselMapping>>,
        //category
        IConsumer<EntityUpdatedEvent<Category>>,
        IConsumer<EntityDeletedEvent<Category>>,
        //product
        IConsumer<EntityUpdatedEvent<Product>>,
        IConsumer<EntityDeletedEvent<Product>>
    {
        #region Constants

        /// <summary>
        /// Key for caching all jcarousels
        /// {0} : available vendor ids
        /// </summary>
        public static CacheKey ALL_CAROUSELS_MODEL_KEY = new("Nop.plugins.widgets.jcarousel.all-{0}", CAROUSEL_PATTERN_KEY);
        public static CacheKey CAROUSEL_ALL_KEY = new("Nop.plugins.widgets.jcarousel.jcarousel.records.all", CAROUSEL_PATTERN_KEY);

        public const string CAROUSEL_PATTERN_KEY = "Nop.plugins.widgets.jcarousel.";

        /// <summary>
        /// Gets a key for caching
        /// </summary>
        /// <remarks>
        /// {0} : carsousel ID
        /// {1} : vendor ids
        /// {2} : number of products
        /// </remarks>
        public static CacheKey ProductsByCarauselCacheKey => new("Nop.plugins.widgets.jcarousel.product.bycarousel.{0}-{1}-{2}", ProductsByCarauselPrefix);

        /// <summary>
        /// Gets a key pattern to clear cache
        /// </summary>
        public static string ProductsByCarauselPrefix => "Nop.plugins.widgets.jcarousel.product.bycarousel.{0}";

        #endregion

        #region Fields

        private readonly ISettingService _settingService;
        private readonly IStaticCacheManager _staticCacheManager;
        private readonly IJCarouselService _jCarouselService;

        #endregion

        #region Ctor

        public ModelCacheEventConsumer(ISettingService settingService,
            IStaticCacheManager staticCacheManager,
            IJCarouselService jCarouselService)
        {
            _settingService = settingService;
            _staticCacheManager = staticCacheManager;
            _jCarouselService = jCarouselService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Handle carousel inserted event
        /// </summary>
        /// <param name="eventMessage">Event message</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task HandleEventAsync(EntityInsertedEvent<JCarouselLog> eventMessage)
        {
            //clear cache
            await _staticCacheManager.RemoveByPrefixAsync(CAROUSEL_PATTERN_KEY);
        }

        /// <summary>
        /// Handle carousel updated event
        /// </summary>
        /// <param name="eventMessage">Event message</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task HandleEventAsync(EntityUpdatedEvent<JCarouselLog> eventMessage)
        {
            //clear cache
            await _staticCacheManager.RemoveByPrefixAsync(CAROUSEL_PATTERN_KEY);
        }

        /// <summary>
        /// Handle carousel deleted event
        /// </summary>
        /// <param name="eventMessage">Event message</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task HandleEventAsync(EntityDeletedEvent<JCarouselLog> eventMessage)
        {
            //clear cache
            await _staticCacheManager.RemoveByPrefixAsync(CAROUSEL_PATTERN_KEY);
        }

        /// <summary>
        /// Handle carousel product insert event
        /// </summary>
        /// <param name="eventMessage">Event message</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task HandleEventAsync(EntityInsertedEvent<ProductJCarouselMapping> eventMessage)
        {
            //clear cache
            await _staticCacheManager.RemoveByPrefixAsync(CAROUSEL_PATTERN_KEY);
            if (eventMessage != null)
                await _staticCacheManager.RemoveByPrefixAsync(ProductsByCarauselPrefix, eventMessage.Entity.JCarouselId);
        }

        /// <summary>
        /// Handle carousel product deleted event
        /// </summary>
        /// <param name="eventMessage">Event message</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task HandleEventAsync(EntityDeletedEvent<ProductJCarouselMapping> eventMessage)
        {
            //clear cache
            await _staticCacheManager.RemoveByPrefixAsync(CAROUSEL_PATTERN_KEY);
            if (eventMessage != null)
                await _staticCacheManager.RemoveByPrefixAsync(ProductsByCarauselPrefix, eventMessage.Entity.JCarouselId);
        }

        /// <summary>
        /// Handle category updated event
        /// </summary>
        /// <param name="eventMessage">Event message</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task HandleEventAsync(EntityUpdatedEvent<Category> eventMessage)
        {
            //clear cache
            await _staticCacheManager.RemoveByPrefixAsync(CAROUSEL_PATTERN_KEY);
        }

        /// <summary>
        /// Handle category deleted event
        /// </summary>
        /// <param name="eventMessage">Event message</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task HandleEventAsync(EntityDeletedEvent<Category> eventMessage)
        {
            var category = eventMessage?.Entity;
            if (category == null)
                return;

            //delete an appropriate record when category is deleted
            var recordsToDelete = (await _jCarouselService.GetAllJCarouselsAsync())?.Where(c => c.CategoryId == category.Id)?.ToList();
            foreach (var jcarouselRecord in recordsToDelete)
            {
                //jcarousel
                await _jCarouselService.DeleteJCarouselAsync(jcarouselRecord);
                //product reference
                await _jCarouselService.DeleteProductReferenceAsync(jcarouselRecord.Id);
            }
        }

        /// <summary>
        /// Handle product updated event
        /// </summary>
        /// <param name="eventMessage">Event message</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task HandleEventAsync(EntityUpdatedEvent<Product> eventMessage)
        {
            var product = eventMessage?.Entity;
            if (product == null)
                return;

            //clear cache
            await _staticCacheManager.RemoveByPrefixAsync(CAROUSEL_PATTERN_KEY);
            if (eventMessage != null)
                await _staticCacheManager.RemoveByPrefixAsync(ProductsByCarauselPrefix, eventMessage.Entity.Id);

            var jcarousels = (await _jCarouselService.GetAllJCarouselsAsync())?.ToList();
            foreach (var jcarouselRecord in jcarousels)
            {
                //check for product
                var productsRef = (await _jCarouselService.GetProductJCarouselsByJCarouselIdAsync(jcarouselRecord.Id))?.Where(x => x.ProductId == product.Id)?.ToList();
                foreach (var productMap in productsRef)
                {
                    //clear cache
                    await _staticCacheManager.RemoveByPrefixAsync(ProductsByCarauselPrefix, productMap.JCarouselId);
                }
            }
        }

        /// <summary>
        /// Handle product deleted event
        /// </summary>
        /// <param name="eventMessage">Event message</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task HandleEventAsync(EntityDeletedEvent<Product> eventMessage)
        {
            var product = eventMessage?.Entity;
            if (product == null)
                return;

            //delete an appropriate record when category is deleted
            var jcarousels = (await _jCarouselService.GetAllJCarouselsAsync())?.ToList();
            foreach (var jcarouselRecord in jcarousels)
            {
                //check for product
                var productsRef = (await _jCarouselService.GetProductJCarouselsByJCarouselIdAsync(jcarouselRecord.Id))?.Where(x => x.ProductId == product.Id)?.ToList();
                foreach (var productMap in productsRef)
                {
                    //clear cache
                    await _staticCacheManager.RemoveByPrefixAsync(ProductsByCarauselPrefix, productMap.JCarouselId);

                    //product reference
                    await _jCarouselService.DeleteProductJCarouselAsync(productMap);
                }
            }
        }

        #endregion
    }
}
