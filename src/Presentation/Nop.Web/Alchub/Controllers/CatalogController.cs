using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Vendors;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Services.Vendors;
using Nop.Web.Factories;
using Nop.Web.Framework.Mvc.Filters;
using Nop.Web.Models.Catalog;

namespace Nop.Web.Controllers
{
    public partial class CatalogController
    {
        #region Fields

        private readonly CatalogSettings _catalogSettings;
        private readonly IAclService _aclService;
        private readonly ICatalogModelFactory _catalogModelFactory;
        private readonly ICategoryService _categoryService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ILocalizationService _localizationService;
        private readonly IManufacturerService _manufacturerService;
        private readonly IPermissionService _permissionService;
        private readonly IProductModelFactory _productModelFactory;
        private readonly IProductService _productService;
        private readonly IProductTagService _productTagService;
        private readonly IStoreContext _storeContext;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IVendorService _vendorService;
        private readonly IWebHelper _webHelper;
        private readonly IWorkContext _workContext;
        private readonly MediaSettings _mediaSettings;
        private readonly VendorSettings _vendorSettings;

        #endregion

        #region Ctor

        public CatalogController(CatalogSettings catalogSettings,
            IAclService aclService,
            ICatalogModelFactory catalogModelFactory,
            ICategoryService categoryService,
            ICustomerActivityService customerActivityService,
            IGenericAttributeService genericAttributeService,
            ILocalizationService localizationService,
            IManufacturerService manufacturerService,
            IPermissionService permissionService,
            IProductModelFactory productModelFactory,
            IProductService productService,
            IProductTagService productTagService,
            IStoreContext storeContext,
            IStoreMappingService storeMappingService,
            IVendorService vendorService,
            IWebHelper webHelper,
            IWorkContext workContext,
            MediaSettings mediaSettings,
            VendorSettings vendorSettings)
        {
            _catalogSettings = catalogSettings;
            _aclService = aclService;
            _catalogModelFactory = catalogModelFactory;
            _categoryService = categoryService;
            _customerActivityService = customerActivityService;
            _genericAttributeService = genericAttributeService;
            _localizationService = localizationService;
            _manufacturerService = manufacturerService;
            _permissionService = permissionService;
            _productModelFactory = productModelFactory;
            _productService = productService;
            _productTagService = productTagService;
            _storeContext = storeContext;
            _storeMappingService = storeMappingService;
            _vendorService = vendorService;
            _webHelper = webHelper;
            _workContext = workContext;
            _mediaSettings = mediaSettings;
            _vendorSettings = vendorSettings;
        }

        #endregion

        public virtual async Task<IActionResult> OldCustomSearch(SearchModel model, CatalogProductsCommand command)
        {
            var store = await _storeContext.GetCurrentStoreAsync();

            //'Continue shopping' URL
            await _genericAttributeService.SaveAttributeAsync(await _workContext.GetCurrentCustomerAsync(),
                NopCustomerDefaults.LastContinueShoppingPageAttribute,
                _webHelper.GetThisPageUrl(true),
                store.Id);

            if (model == null)
                model = new SearchModel();

            model = await _catalogModelFactory.PrepareSearchModelAsync(model, command);

            return View(model);
        }

        [CheckLanguageSeoCode(true)]
        public virtual async Task<IActionResult> OldSearchTermAutoComplete(string term)
        {
            if (string.IsNullOrWhiteSpace(term))
                return Content("");

            term = term.Trim();

            if (string.IsNullOrWhiteSpace(term) || term.Length < _catalogSettings.ProductSearchTermMinimumLength)
                return Content("");

            //customer
            var customer = await _workContext.GetCurrentCustomerAsync();

            //products
            var productNumber = _catalogSettings.ProductSearchAutoCompleteNumberOfProducts > 0 ?
                _catalogSettings.ProductSearchAutoCompleteNumberOfProducts : 10;
            var store = await _storeContext.GetCurrentStoreAsync();

            //get products result
            var products = await _productService.SearchProductsAsync(0,
                storeId: store.Id,
                keywords: term,
                //languageId: (await _workContext.GetWorkingLanguageAsync()).Id,
                visibleIndividuallyOnly: true,
                pageSize: productNumber,
                //master products only filter along with geoVendorIds
                isMaster: true,
                geoVendorIds: (await _workContext.GetCurrentCustomerGeoRadiusVendorIdsAsync()),
                //exclude other search exept product name
                searchDescriptions: false,
                searchManufacturerPartNumber: false,
                searchSku: false,
                searchProductTags: false,
                languageId: 0);

            var showLinkToResultSearch = _catalogSettings.ShowLinkToAllResultInSearchAutoComplete && (products.TotalCount > productNumber);

            var models = (await _productModelFactory.PrepareProductSearchOverviewModelsAsync(products, _catalogSettings.ShowProductImagesInSearchAutoComplete, _mediaSettings.AutoCompleteSearchThumbPictureSize)).ToList();
            var result = (from p in models
                          select new
                          {
                              label = p.Name,
                              producturl = Url.RouteUrl("Product", new { SeName = p.SeName }),
                              productpictureurl = p.DefaultPictureModel.ImageUrl,
                              showlinktoresultsearch = showLinkToResultSearch
                          })
                .ToList();
            return Json(result);
        }

        #region Elastic

        public virtual async Task<IActionResult> CustomSearch(SearchModel model, CatalogProductsCommand command)
        {
            var store = await _storeContext.GetCurrentStoreAsync();

            //'Continue shopping' URL
            await _genericAttributeService.SaveAttributeAsync(await _workContext.GetCurrentCustomerAsync(),
                NopCustomerDefaults.LastContinueShoppingPageAttribute,
                _webHelper.GetThisPageUrl(true),
                store.Id);

            if (model == null)
                model = new SearchModel();

            //get result based on setting, true -> Elastic, false: Old Linq
            if (_catalogSettings.EnableElasticSearch)
                model = await _catalogModelFactory.PrepareElasticSearchModelAsync(model, command);
            else
                model = await _catalogModelFactory.PrepareSearchModelAsync(model, command);

            return View(model);
        }

        #region Searching

        [CheckLanguageSeoCode(true)]
        public virtual async Task<IActionResult> SearchTermAutoComplete(string term)
        {
            if (string.IsNullOrWhiteSpace(term))
                return Content("");

            term = term.Trim();

            if (string.IsNullOrWhiteSpace(term) || term.Length < _catalogSettings.ProductSearchTermMinimumLength)
                return Content("");

            //customer
            var customer = await _workContext.GetCurrentCustomerAsync();
            var productNumber = _catalogSettings.ProductSearchAutoCompleteNumberOfProducts > 0 ?
                _catalogSettings.ProductSearchAutoCompleteNumberOfProducts : 10;

            //get result based on setting, true -> Elastic, false: Old Linq
            if (_catalogSettings.EnableElasticSearch)
            {
                var models = await _catalogModelFactory.PrepareProductSearchOverviewModelsElasticAsync(
                term,
                pageSize: productNumber,
                visibleIndividuallyOnly: true,
                isMaster: true,
                languageId: 0,
                geoVendorIds: await _workContext.GetCurrentCustomerGeoRadiusVendorIdsAsync());

                var result = (from p in models
                              select new
                              {
                                  label = p.Name,
                                  producturl = Url.RouteUrl("Product", new { SeName = p.SeName }),
                                  productpictureurl = p.DefaultPictureModel.ImageUrl,
                                  //showlinktoresultsearch = showLinkToResultSearch
                              })
                    .ToList();
                return Json(result);
            }
            else
            {
                var store = await _storeContext.GetCurrentStoreAsync();

                //get products result
                var products = await _productService.SearchProductsAsync(0,
                    storeId: store.Id,
                    keywords: term,
                    //languageId: (await _workContext.GetWorkingLanguageAsync()).Id,
                    visibleIndividuallyOnly: true,
                    pageSize: productNumber,
                    //master products only filter along with geoVendorIds
                    isMaster: true,
                    geoVendorIds: (await _workContext.GetCurrentCustomerGeoRadiusVendorIdsAsync()),
                    //exclude other search exept product name
                    searchDescriptions: false,
                    searchManufacturerPartNumber: false,
                    searchSku: false,
                    searchProductTags: false,
                    languageId: 0);

                var showLinkToResultSearch = _catalogSettings.ShowLinkToAllResultInSearchAutoComplete && (products.TotalCount > productNumber);

                var models = (await _productModelFactory.PrepareProductSearchOverviewModelsAsync(products, _catalogSettings.ShowProductImagesInSearchAutoComplete, _mediaSettings.AutoCompleteSearchThumbPictureSize)).ToList();
                var result = (from p in models
                              select new
                              {
                                  label = p.Name,
                                  producturl = Url.RouteUrl("Product", new { SeName = p.SeName }),
                                  productpictureurl = p.DefaultPictureModel.ImageUrl,
                                  showlinktoresultsearch = showLinkToResultSearch
                              })
                    .ToList();
                return Json(result);
            }
        }

        #endregion

        #endregion
    }
}