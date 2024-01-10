using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Plugin.Widgets.JCarousel.Domain;
using Nop.Plugin.Widgets.JCarousel.Models.Configuration;
using Nop.Plugin.Widgets.JCarousel.Services;
using Nop.Services.Catalog;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Web.Factories;
using Nop.Web.Infrastructure.Cache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Services.Stores;
using Nop.Services.Seo;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Stores;
using Nop.Services.Configuration;

namespace Nop.Plugin.Widgets.JCarousel.Factories
{
    public partial class PublicJCarouselModelFactory : IPublicJCarouselModelFactory
    {
        #region Fields
        private readonly IJCarouselService _jCarouselService;
        private readonly IProductService _productService;
        private readonly IProductModelFactory _productModelFactory;
        private readonly IStoreContext _storeContext;
        private readonly IStaticCacheManager _staticCacheManager;
        private readonly IAclService _aclService;
        private readonly CatalogSettings _catalogSettings;
        private readonly IOrderReportService _orderReportService;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IRecentlyViewedProductsService _recentlyViewedProductsService;
        private readonly IWorkContext _workContext;
        private readonly IUrlRecordService _urlRecordService;
        private readonly ICategoryService _categoryService;
        private readonly ISettingService _settingService;
        #endregion

        #region Ctor

        public PublicJCarouselModelFactory(
            IJCarouselService jCarouselService,
            IProductService productService,
            IProductModelFactory productModelFactory,
            IStaticCacheManager staticCacheManager,
            IStoreContext storeContext,
            IAclService aclService,
            CatalogSettings catalogSettings,
            IOrderReportService orderReportService,
            IStoreMappingService storeMappingService,
            IRecentlyViewedProductsService recentlyViewedProductsService,
            IWorkContext workContext,
            IUrlRecordService urlRecordService,
            ICategoryService categoryService,
            ISettingService settingService)
        {
            _jCarouselService = jCarouselService;
            _productService = productService;
            _productModelFactory = productModelFactory;
            _staticCacheManager = staticCacheManager;
            _storeContext = storeContext;
            _aclService = aclService;
            _catalogSettings = catalogSettings;
            _orderReportService = orderReportService;
            _storeMappingService = storeMappingService;
            _recentlyViewedProductsService = recentlyViewedProductsService;
            _workContext = workContext;
            _urlRecordService = urlRecordService;
            _categoryService = categoryService;
            _settingService = settingService;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Prepare list of Jcarousel Products mapping
        /// </summary>
        /// <param name="jcarousels">List of Jcarousels</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        private async Task<IList<Product>> PrepareJCarouselProductsModelAsync(JCarouselLog jcarousel, Customer customer)
        {
            if (jcarousel == null)
                throw new ArgumentNullException(nameof(jcarousel));

            var store = await _storeContext.GetCurrentStoreAsync();

            var settings = await _settingService.LoadSettingAsync<JCarouselPluginSettings>(store.Id);
            
            var products = await _jCarouselService.GetProductByJcarouselIdAsync(jcarousel.Id, geoVendorIds: await _workContext.GetCurrentCustomerGeoRadiusVendorIdsAsync(customer), takeNumberOfProducts: settings.LazyLoadNumberOfProductInCarousel);

            //return await _productService.GetProductsByIdsAsync(productIds.ToArray());
            return products;
        }

        #endregion

        #region Methods
        /// <summary>
        /// Prepare Jcarousel public view model
        /// </summary>
        /// <param name="jcarousels">List of Jcarousels</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task<IList<JCarouselModel>> PrepareJcarouselOverviewModelsAsync(IEnumerable<JCarouselLog> jcarousels, Customer customer)
        {
            if (jcarousels == null)
                throw new ArgumentNullException(nameof(jcarousels));

            //load all carausel with products (cached) - according available vendorids
            //var geoRadiusVendorIds = (await _workContext.GetCurrentCustomerGeoRadiusVendorIdsAsync())?.ToArray();
            //var cacheKey = _staticCacheManager.PrepareKeyForDefaultCache(Nop.Plugin.Widgets.JCarousel.Infrastructure.Cache.ModelCacheEventConsumer.ALL_CAROUSELS_MODEL_KEY, geoRadiusVendorIds);
            //var carouselList = await _staticCacheManager.GetAsync(cacheKey, async () =>
            //{
            var models = new List<JCarouselModel>();
            var store = await _storeContext.GetCurrentStoreAsync();
            foreach (var jcarousel in jcarousels)
            {
                var jcarouselModel = jcarousel.ToModel<JCarouselModel>();
                jcarouselModel.SelectedCategoryId = jcarousel.CategoryId;
                var category = await _categoryService.GetCategoryByIdAsync(jcarouselModel.SelectedCategoryId);
                if (category != null)
                {
                    jcarouselModel.SeName = await _urlRecordService.GetSeNameAsync(category);
                    jcarouselModel.ParentCategoryId = category.ParentCategoryId;
                    jcarouselModel.ParentCategoryName = (await _categoryService.GetCategoryByIdAsync(category.ParentCategoryId))?.Name ?? "";
                }

                var carsouelProducts = new List<Product>();
                if (jcarousel.DataSourceType == DataSourceType.BestSellersProductsByQuantity)
                {
                    var bsReport = await _staticCacheManager.GetAsync(_staticCacheManager.PrepareKeyForDefaultCache(NopModelCacheDefaults.HomepageBestsellersIdsKey,
                     store),
                     async () => await (await _orderReportService.BestSellersReportAsync(
                     storeId: store.Id,
                     orderBy: OrderByEnum.OrderByQuantity,
                     isMasterOnly: true)).ToListAsync());

                    carsouelProducts = await (await _productService.GetProductsByIdsAsync(bsReport.Select(x => x.ProductId).ToArray()))
                    //ACL and store mapping
                    .WhereAwait(async p => await _aclService.AuthorizeAsync(p) && await _storeMappingService.AuthorizeAsync(p))
                    //availability dates
                    .Where(p => _productService.ProductIsAvailable(p)).ToListAsync();
                }
                else
                    carsouelProducts = (await PrepareJCarouselProductsModelAsync(jcarousel, customer)).ToList();

                //Max items in a jcarousel selected from admin side
                //carsouelProducts = carsouelProducts.ToList();
                jcarouselModel.JcarouselProductsModel.Products.AddRange((await _productModelFactory.PrepareProductOverviewModelsAsync(carsouelProducts)).ToList());
                models.Add(jcarouselModel);
            }

            return models;
            //});

            //return carouselList;
        }

        /// <summary>
        /// Prepare data of the current carousel
        /// </summary>
        /// <param name="jcarousel"></param>
        /// <param name="customer"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public virtual async Task<JCarouselModel> PrepareJcarouselSliderDataModelAsync(JCarouselLog jcarousel, Customer customer)
        {
            if (jcarousel == null)
                throw new ArgumentNullException(nameof(jcarousel));

            var store = await _storeContext.GetCurrentStoreAsync();

            var jcarouselModel = jcarousel.ToModel<JCarouselModel>();

            var settings = await _settingService.LoadSettingAsync<JCarouselPluginSettings>(store.Id);

            jcarouselModel.SelectedCategoryId = jcarousel.CategoryId;
            var category = await _categoryService.GetCategoryByIdAsync(jcarouselModel.SelectedCategoryId);
            if (category != null)
            {
                jcarouselModel.SeName = await _urlRecordService.GetSeNameAsync(category);
                jcarouselModel.ParentCategoryId = category.ParentCategoryId;
                jcarouselModel.ParentCategoryName = (await _categoryService.GetCategoryByIdAsync(category.ParentCategoryId))?.Name ?? "";
            }

            var carsouelProducts = new List<Product>();
            if (jcarousel.DataSourceType == DataSourceType.BestSellersProductsByQuantity)
            {
                var bsReport = await _staticCacheManager.GetAsync(_staticCacheManager.PrepareKeyForDefaultCache(NopModelCacheDefaults.HomepageBestsellersIdsKey,
                 store),
                 async () => await (await _orderReportService.BestSellersReportAsync(
                 storeId: store.Id,
                 orderBy: OrderByEnum.OrderByQuantity,
                 isMasterOnly: true)).ToListAsync());

                carsouelProducts = await (await _productService.GetProductsByIdsAsync(bsReport.Select(x => x.ProductId).Take(settings.LazyLoadNumberOfProductInCarousel).ToArray()))
                //ACL and store mapping
                .WhereAwait(async p => await _aclService.AuthorizeAsync(p) && await _storeMappingService.AuthorizeAsync(p))
                //availability dates
                .Where(p => _productService.ProductIsAvailable(p)).ToListAsync();
            }
            else
                carsouelProducts = (await PrepareJCarouselProductsModelAsync(jcarousel, customer)).ToList();

            jcarouselModel.JcarouselProductsModel.Products.AddRange((await _productModelFactory.PrepareProductOverviewModelsAsync(carsouelProducts)).ToList());
            return jcarouselModel;
        }
        #endregion
    }
}
