using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Nop.Core;
using Nop.Core.Alchub.Domain;
using Nop.Core.Caching;
using Nop.Core.Domain;
using Nop.Core.Domain.Blogs;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Forums;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.News;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Security;
using Nop.Core.Domain.Vendors;
using Nop.Core.Infrastructure;
using Nop.Services.Blogs;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Forums;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.News;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Themes;
using Nop.Services.Topics;
using Nop.Web.Framework.Themes;
using Nop.Web.Framework.UI;
using Nop.Web.Infrastructure.Cache;
using Nop.Web.Models.Common;

namespace Nop.Web.Factories
{
    /// <summary>
    /// Represents the extended common models factory
    /// </summary>
    public partial class CommonModelFactory
    {
        #region Fields

        private readonly BlogSettings _blogSettings;
        private readonly CaptchaSettings _captchaSettings;
        private readonly CatalogSettings _catalogSettings;
        private readonly CommonSettings _commonSettings;
        private readonly CustomerSettings _customerSettings;
        private readonly DisplayDefaultFooterItemSettings _displayDefaultFooterItemSettings;
        private readonly ForumSettings _forumSettings;
        private readonly IActionContextAccessor _actionContextAccessor;
        private readonly IBlogService _blogService;
        private readonly ICategoryService _categoryService;
        private readonly ICurrencyService _currencyService;
        private readonly ICustomerService _customerService;
        private readonly IForumService _forumService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILanguageService _languageService;
        private readonly ILocalizationService _localizationService;
        private readonly IManufacturerService _manufacturerService;
        private readonly INewsService _newsService;
        private readonly INopFileProvider _fileProvider;
        private readonly INopHtmlHelper _nopHtmlHelper;
        private readonly IPermissionService _permissionService;
        private readonly IPictureService _pictureService;
        private readonly IProductService _productService;
        private readonly IProductTagService _productTagService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly ISitemapGenerator _sitemapGenerator;
        private readonly IStaticCacheManager _staticCacheManager;
        private readonly IStoreContext _storeContext;
        private readonly IThemeContext _themeContext;
        private readonly IThemeProvider _themeProvider;
        private readonly ITopicService _topicService;
        private readonly IUrlHelperFactory _urlHelperFactory;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IWebHelper _webHelper;
        private readonly IWorkContext _workContext;
        private readonly LocalizationSettings _localizationSettings;
        private readonly MediaSettings _mediaSettings;
        private readonly NewsSettings _newsSettings;
        private readonly SitemapSettings _sitemapSettings;
        private readonly SitemapXmlSettings _sitemapXmlSettings;
        private readonly StoreInformationSettings _storeInformationSettings;
        private readonly VendorSettings _vendorSettings;
        private readonly AlchubSettings _alchubSettings;

        #endregion

        #region Ctor

        public CommonModelFactory(BlogSettings blogSettings,
            CaptchaSettings captchaSettings,
            CatalogSettings catalogSettings,
            CommonSettings commonSettings,
            CustomerSettings customerSettings,
            DisplayDefaultFooterItemSettings displayDefaultFooterItemSettings,
            ForumSettings forumSettings,
            IActionContextAccessor actionContextAccessor,
            IBlogService blogService,
            ICategoryService categoryService,
            ICurrencyService currencyService,
            ICustomerService customerService,
            IForumService forumService,
            IGenericAttributeService genericAttributeService,
            IHttpContextAccessor httpContextAccessor,
            ILanguageService languageService,
            ILocalizationService localizationService,
            IManufacturerService manufacturerService,
            INewsService newsService,
            INopFileProvider fileProvider,
            INopHtmlHelper nopHtmlHelper,
            IPermissionService permissionService,
            IPictureService pictureService,
            IProductService productService,
            IProductTagService productTagService,
            IShoppingCartService shoppingCartService,
            ISitemapGenerator sitemapGenerator,
            IStaticCacheManager staticCacheManager,
            IStoreContext storeContext,
            IThemeContext themeContext,
            IThemeProvider themeProvider,
            ITopicService topicService,
            IUrlHelperFactory urlHelperFactory,
            IUrlRecordService urlRecordService,
            IWebHelper webHelper,
            IWorkContext workContext,
            LocalizationSettings localizationSettings,
            MediaSettings mediaSettings,
            NewsSettings newsSettings,
            SitemapSettings sitemapSettings,
            SitemapXmlSettings sitemapXmlSettings,
            StoreInformationSettings storeInformationSettings,
            VendorSettings vendorSettings,
            AlchubSettings alchubSettings)
        {
            _blogSettings = blogSettings;
            _captchaSettings = captchaSettings;
            _catalogSettings = catalogSettings;
            _commonSettings = commonSettings;
            _customerSettings = customerSettings;
            _displayDefaultFooterItemSettings = displayDefaultFooterItemSettings;
            _forumSettings = forumSettings;
            _actionContextAccessor = actionContextAccessor;
            _blogService = blogService;
            _categoryService = categoryService;
            _currencyService = currencyService;
            _customerService = customerService;
            _forumService = forumService;
            _genericAttributeService = genericAttributeService;
            _httpContextAccessor = httpContextAccessor;
            _languageService = languageService;
            _localizationService = localizationService;
            _manufacturerService = manufacturerService;
            _newsService = newsService;
            _fileProvider = fileProvider;
            _nopHtmlHelper = nopHtmlHelper;
            _permissionService = permissionService;
            _pictureService = pictureService;
            _productService = productService;
            _productTagService = productTagService;
            _shoppingCartService = shoppingCartService;
            _sitemapGenerator = sitemapGenerator;
            _staticCacheManager = staticCacheManager;
            _storeContext = storeContext;
            _themeContext = themeContext;
            _themeProvider = themeProvider;
            _topicService = topicService;
            _urlHelperFactory = urlHelperFactory;
            _urlRecordService = urlRecordService;
            _webHelper = webHelper;
            _workContext = workContext;
            _mediaSettings = mediaSettings;
            _localizationSettings = localizationSettings;
            _newsSettings = newsSettings;
            _sitemapSettings = sitemapSettings;
            _sitemapXmlSettings = sitemapXmlSettings;
            _storeInformationSettings = storeInformationSettings;
            _vendorSettings = vendorSettings;
            _alchubSettings = alchubSettings;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Prepare the footer model
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the footer model
        /// </returns>
        public virtual async Task<FooterModel> PrepareFooterModelAsync()
        {
            //footer topics
            var store = await _storeContext.GetCurrentStoreAsync();
            var topicModels = await (await _topicService.GetAllTopicsAsync(store.Id))
                    .Where(t => t.IncludeInFooterColumn1 || t.IncludeInFooterColumn2 || t.IncludeInFooterColumn3)
                    .SelectAwait(async t => new FooterModel.FooterTopicModel
                    {
                        Id = t.Id,
                        Name = await _localizationService.GetLocalizedAsync(t, x => x.Title),
                        SeName = await _urlRecordService.GetSeNameAsync(t),
                        IncludeInFooterColumn1 = t.IncludeInFooterColumn1,
                        IncludeInFooterColumn2 = t.IncludeInFooterColumn2,
                        IncludeInFooterColumn3 = t.IncludeInFooterColumn3
                    }).ToListAsync();

            //model
            var model = new FooterModel
            {
                StoreName = await _localizationService.GetLocalizedAsync(store, x => x.Name),
                WishlistEnabled = await _permissionService.AuthorizeAsync(StandardPermissionProvider.EnableWishlist),
                ShoppingCartEnabled = await _permissionService.AuthorizeAsync(StandardPermissionProvider.EnableShoppingCart),
                SitemapEnabled = _sitemapSettings.SitemapEnabled,
                SearchEnabled = _catalogSettings.ProductSearchEnabled,
                WorkingLanguageId = (await _workContext.GetWorkingLanguageAsync()).Id,
                BlogEnabled = _blogSettings.Enabled,
                CompareProductsEnabled = _catalogSettings.CompareProductsEnabled,
                ForumEnabled = _forumSettings.ForumsEnabled,
                NewsEnabled = _newsSettings.Enabled,
                RecentlyViewedProductsEnabled = _catalogSettings.RecentlyViewedProductsEnabled,
                NewProductsEnabled = _catalogSettings.NewProductsEnabled,
                DisplayTaxShippingInfoFooter = _catalogSettings.DisplayTaxShippingInfoFooter,
                HidePoweredByNopCommerce = _storeInformationSettings.HidePoweredByNopCommerce,
                IsHomePage = _webHelper.GetStoreLocation().Equals(_webHelper.GetThisPageUrl(false), StringComparison.InvariantCultureIgnoreCase),
                AllowCustomersToApplyForVendorAccount = _vendorSettings.AllowCustomersToApplyForVendorAccount,
                AllowCustomersToCheckGiftCardBalance = _customerSettings.AllowCustomersToCheckGiftCardBalance && _captchaSettings.Enabled,
                Topics = topicModels,
                DisplaySitemapFooterItem = _displayDefaultFooterItemSettings.DisplaySitemapFooterItem,
                DisplayContactUsFooterItem = _displayDefaultFooterItemSettings.DisplayContactUsFooterItem,
                DisplayProductSearchFooterItem = _displayDefaultFooterItemSettings.DisplayProductSearchFooterItem,
                DisplayNewsFooterItem = _displayDefaultFooterItemSettings.DisplayNewsFooterItem,
                DisplayBlogFooterItem = _displayDefaultFooterItemSettings.DisplayBlogFooterItem,
                DisplayForumsFooterItem = _displayDefaultFooterItemSettings.DisplayForumsFooterItem,
                DisplayRecentlyViewedProductsFooterItem = _displayDefaultFooterItemSettings.DisplayRecentlyViewedProductsFooterItem,
                DisplayCompareProductsFooterItem = _displayDefaultFooterItemSettings.DisplayCompareProductsFooterItem,
                DisplayNewProductsFooterItem = _displayDefaultFooterItemSettings.DisplayNewProductsFooterItem,
                DisplayCustomerInfoFooterItem = _displayDefaultFooterItemSettings.DisplayCustomerInfoFooterItem,
                DisplayCustomerOrdersFooterItem = _displayDefaultFooterItemSettings.DisplayCustomerOrdersFooterItem,
                DisplayCustomerAddressesFooterItem = _displayDefaultFooterItemSettings.DisplayCustomerAddressesFooterItem,
                DisplayShoppingCartFooterItem = _displayDefaultFooterItemSettings.DisplayShoppingCartFooterItem,
                DisplayWishlistFooterItem = _displayDefaultFooterItemSettings.DisplayWishlistFooterItem,
                DisplayApplyVendorAccountFooterItem = _displayDefaultFooterItemSettings.DisplayApplyVendorAccountFooterItem,
                //++Alchub++
                AndroidAppLink = _alchubSettings.AndroidAppLink,
                IOSAppLink = _alchubSettings.IOSAppLink
                //--Alchub--
            };

            return model;
        }

        #endregion
    }
}