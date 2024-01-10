using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Vendors;
using Nop.Core.Infrastructure;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Discounts;
using Nop.Services.ExportImport;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Shipping;
using Nop.Services.Vendors;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Web.Areas.Admin.Controllers
{
    public partial class ProductController : BaseAdminController
    {
        #region Fields

        private readonly IAclService _aclService;
        private readonly IBackInStockSubscriptionService _backInStockSubscriptionService;
        private readonly ICategoryService _categoryService;
        private readonly ICopyProductService _copyProductService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly ICustomerService _customerService;
        private readonly IDiscountService _discountService;
        private readonly IDownloadService _downloadService;
        private readonly IExportManager _exportManager;
        private readonly IImportManager _importManager;
        private readonly ILanguageService _languageService;
        private readonly ILocalizationService _localizationService;
        private readonly ILocalizedEntityService _localizedEntityService;
        private readonly IManufacturerService _manufacturerService;
        private readonly INopFileProvider _fileProvider;
        private readonly INotificationService _notificationService;
        private readonly IPdfService _pdfService;
        private readonly IPermissionService _permissionService;
        private readonly IPictureService _pictureService;
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly IProductAttributeService _productAttributeService;
        private readonly IProductAttributeFormatter _productAttributeFormatter;
        private readonly IProductModelFactory _productModelFactory;
        private readonly IProductService _productService;
        private readonly IProductTagService _productTagService;
        private readonly ISettingService _settingService;
        private readonly IShippingService _shippingService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly ISpecificationAttributeService _specificationAttributeService;
        private readonly IStoreContext _storeContext;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IWorkContext _workContext;
        private readonly VendorSettings _vendorSettings;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IVendorService _vendorService;

        #endregion

        #region Ctor

        public ProductController(IAclService aclService,
            IBackInStockSubscriptionService backInStockSubscriptionService,
            ICategoryService categoryService,
            ICopyProductService copyProductService,
            ICustomerActivityService customerActivityService,
            ICustomerService customerService,
            IDiscountService discountService,
            IDownloadService downloadService,
            IExportManager exportManager,
            IImportManager importManager,
            ILanguageService languageService,
            ILocalizationService localizationService,
            ILocalizedEntityService localizedEntityService,
            IManufacturerService manufacturerService,
            INopFileProvider fileProvider,
            INotificationService notificationService,
            IPdfService pdfService,
            IPermissionService permissionService,
            IPictureService pictureService,
            IProductAttributeParser productAttributeParser,
            IProductAttributeService productAttributeService,
            IProductAttributeFormatter productAttributeFormatter,
            IProductModelFactory productModelFactory,
            IProductService productService,
            IProductTagService productTagService,
            ISettingService settingService,
            IShippingService shippingService,
            IShoppingCartService shoppingCartService,
            ISpecificationAttributeService specificationAttributeService,
            IStoreContext storeContext,
            IUrlRecordService urlRecordService,
            IGenericAttributeService genericAttributeService,
            IWorkContext workContext,
            VendorSettings vendorSettings,
            IWebHostEnvironment webHostEnvironment,
            IVendorService vendorService)
        {
            _aclService = aclService;
            _backInStockSubscriptionService = backInStockSubscriptionService;
            _categoryService = categoryService;
            _copyProductService = copyProductService;
            _customerActivityService = customerActivityService;
            _customerService = customerService;
            _discountService = discountService;
            _downloadService = downloadService;
            _exportManager = exportManager;
            _importManager = importManager;
            _languageService = languageService;
            _localizationService = localizationService;
            _localizedEntityService = localizedEntityService;
            _manufacturerService = manufacturerService;
            _fileProvider = fileProvider;
            _notificationService = notificationService;
            _pdfService = pdfService;
            _permissionService = permissionService;
            _pictureService = pictureService;
            _productAttributeParser = productAttributeParser;
            _productAttributeService = productAttributeService;
            _productAttributeFormatter = productAttributeFormatter;
            _productModelFactory = productModelFactory;
            _productService = productService;
            _productTagService = productTagService;
            _settingService = settingService;
            _shippingService = shippingService;
            _shoppingCartService = shoppingCartService;
            _specificationAttributeService = specificationAttributeService;
            _storeContext = storeContext;
            _urlRecordService = urlRecordService;
            _genericAttributeService = genericAttributeService;
            _workContext = workContext;
            _vendorSettings = vendorSettings;
            _webHostEnvironment = webHostEnvironment;
            _vendorService = vendorService;
        }

        #endregion

        #region Product list / create / edit / delete

        public virtual async Task<IActionResult> Create(bool showtour = false)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts) ||
                !await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessVendorProductActions))
                return AccessDeniedView();

            //validate maximum number of products per vendor
            var currentVendor = await _workContext.GetCurrentVendorAsync();
            if (_vendorSettings.MaximumProductNumber > 0 && currentVendor != null
                && await _productService.GetNumberOfProductsByVendorIdAsync(currentVendor.Id) >= _vendorSettings.MaximumProductNumber)
            {
                _notificationService.ErrorNotification(string.Format(await _localizationService.GetResourceAsync("Admin.Catalog.Products.ExceededMaximumNumber"),
                    _vendorSettings.MaximumProductNumber));
                return RedirectToAction("List");
            }

            //prepare model
            var model = await _productModelFactory.PrepareProductModelAsync(new ProductModel(), null);

            //show configuration tour
            if (showtour)
            {
                var customer = await _workContext.GetCurrentCustomerAsync();
                var hideCard = await _genericAttributeService.GetAttributeAsync<bool>(customer, NopCustomerDefaults.HideConfigurationStepsAttribute);
                var closeCard = await _genericAttributeService.GetAttributeAsync<bool>(customer, NopCustomerDefaults.CloseConfigurationStepsAttribute);

                if (!hideCard && !closeCard)
                    ViewBag.ShowTour = true;
            }

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> Create(ProductModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts) ||
                !await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessVendorProductActions))
                return AccessDeniedView();

            //validate maximum number of products per vendor
            var currentVendor = await _workContext.GetCurrentVendorAsync();
            if (_vendorSettings.MaximumProductNumber > 0 && currentVendor != null
                && await _productService.GetNumberOfProductsByVendorIdAsync(currentVendor.Id) >= _vendorSettings.MaximumProductNumber)
            {
                _notificationService.ErrorNotification(string.Format(await _localizationService.GetResourceAsync("Admin.Catalog.Products.ExceededMaximumNumber"),
                    _vendorSettings.MaximumProductNumber));
                return RedirectToAction("List");
            }

            if (ModelState.IsValid)
            {
                //a vendor should have access only to his products
                if (currentVendor != null)
                    model.VendorId = currentVendor.Id;

                //vendors cannot edit "Show on home page" property
                if (currentVendor != null && model.ShowOnHomepage)
                    model.ShowOnHomepage = false;

                //product
                var product = model.ToEntity<Product>();
                product.CreatedOnUtc = DateTime.UtcNow;
                product.UpdatedOnUtc = DateTime.UtcNow;
                await _productService.InsertProductAsync(product);

                //search engine name
                model.SeName = await _urlRecordService.ValidateSeNameAsync(product, model.SeName, product.Name, true);
                await _urlRecordService.SaveSlugAsync(product, model.SeName, 0);

                //locales
                await UpdateLocalesAsync(product, model);

                //categories
                await SaveCategoryMappingsAsync(product, model);

                //manufacturers
                await SaveManufacturerMappingsAsync(product, model);

                //ACL (customer roles)
                await SaveProductAclAsync(product, model);

                //stores
                await _productService.UpdateProductStoreMappingsAsync(product, model.SelectedStoreIds);

                //discounts
                await SaveDiscountMappingsAsync(product, model);

                //tags
                await _productTagService.UpdateProductTagsAsync(product, ParseProductTags(model.ProductTags));

                //warehouses
                await SaveProductWarehouseInventoryAsync(product, model);

                //quantity change history
                await _productService.AddStockQuantityHistoryEntryAsync(product, product.StockQuantity, product.StockQuantity, product.WarehouseId,
                    await _localizationService.GetResourceAsync("Admin.StockQuantityHistory.Messages.Edit"));

                //activity log
                await _customerActivityService.InsertActivityAsync("AddNewProduct",
                    string.Format(await _localizationService.GetResourceAsync("ActivityLog.AddNewProduct"), product.Name), product);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Catalog.Products.Added"));

                if (!continueEditing)
                    return RedirectToAction("List");

                return RedirectToAction("Edit", new { id = product.Id });
            }

            //prepare model
            model = await _productModelFactory.PrepareProductModelAsync(model, null, true);

            //if we got this far, something failed, redisplay form
            return View(model);
        }

        public virtual async Task<IActionResult> Edit(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            //try to get a product with the specified id
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null || product.Deleted)
                return RedirectToAction("List");

            //a vendor should have access only to his products
            var currentVendor = await _workContext.GetCurrentVendorAsync();
            if (currentVendor != null && product.VendorId != currentVendor.Id)
                return RedirectToAction("List");

            //++Alchub

            //a master product is not accesible in sub product section
            if (product.IsMaster)
            {
                _notificationService.WarningNotification("A Master product can not be accessible in the sub product section");
                return RedirectToAction("List");
            }

            //--Alchub

            //prepare model
            var model = await _productModelFactory.PrepareProductModelAsync(null, product);

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> Edit(ProductModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            //try to get a product with the specified id
            var product = await _productService.GetProductByIdAsync(model.Id);
            if (product == null || product.Deleted)
                return RedirectToAction("List");

            //a vendor should have access only to his products
            var currentVendor = await _workContext.GetCurrentVendorAsync();
            if (currentVendor != null && product.VendorId != currentVendor.Id)
                return RedirectToAction("List");

            //a master product is not accesible in sub product section
            if (product.IsMaster)
            {
                _notificationService.WarningNotification("A Master product can not be accessible in the sub product section");
                return RedirectToAction("List");
            }

            //--Alchub

            //check if the product quantity has been changed while we were editing the product
            //and if it has been changed then we show error notification
            //and redirect on the editing page without data saving
            if (product.StockQuantity != model.LastStockQuantity)
            {
                _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Catalog.Products.Fields.StockQuantity.ChangedWarning"));
                return RedirectToAction("Edit", new { id = product.Id });
            }

            if (ModelState.IsValid)
            {
                //a vendor should have access only to his products
                if (currentVendor != null)
                    model.VendorId = currentVendor.Id;

                //we do not validate maximum number of products per vendor when editing existing products (only during creation of new products)
                //vendors cannot edit "Show on home page" property
                if (currentVendor != null && model.ShowOnHomepage != product.ShowOnHomepage)
                    model.ShowOnHomepage = product.ShowOnHomepage;

                //some previously used values
                var prevTotalStockQuantity = await _productService.GetTotalStockQuantityAsync(product);
                var prevDownloadId = product.DownloadId;
                var prevSampleDownloadId = product.SampleDownloadId;
                var previousStockQuantity = product.StockQuantity;
                var previousWarehouseId = product.WarehouseId;
                var previousProductType = product.ProductType;

                //product
                product = model.ToEntity(product);

                product.UpdatedOnUtc = DateTime.UtcNow;
                await _productService.UpdateProductAsync(product);

                //remove associated products
                if (previousProductType == ProductType.GroupedProduct && product.ProductType == ProductType.SimpleProduct)
                {
                    var store = await _storeContext.GetCurrentStoreAsync();
                    var storeId = store?.Id ?? 0;
                    var vendorId = currentVendor?.Id ?? 0;

                    var associatedProducts = await _productService.GetAssociatedProductsAsync(product.Id, storeId, vendorId);
                    foreach (var associatedProduct in associatedProducts)
                    {
                        associatedProduct.ParentGroupedProductId = 0;
                        await _productService.UpdateProductAsync(associatedProduct);
                    }
                }

                //search engine name
                model.SeName = await _urlRecordService.ValidateSeNameAsync(product, model.SeName, product.Name, true);
                await _urlRecordService.SaveSlugAsync(product, model.SeName, 0);

                //locales
                await UpdateLocalesAsync(product, model);

                //tags
                await _productTagService.UpdateProductTagsAsync(product, ParseProductTags(model.ProductTags));

                //warehouses
                await SaveProductWarehouseInventoryAsync(product, model);

                //categories
                await SaveCategoryMappingsAsync(product, model);

                //manufacturers
                await SaveManufacturerMappingsAsync(product, model);

                //ACL (customer roles)
                await SaveProductAclAsync(product, model);

                //stores
                await _productService.UpdateProductStoreMappingsAsync(product, model.SelectedStoreIds);

                //discounts
                await SaveDiscountMappingsAsync(product, model);

                //picture seo names
                await UpdatePictureSeoNamesAsync(product);

                //back in stock notifications
                if (product.ManageInventoryMethod == ManageInventoryMethod.ManageStock &&
                    product.BackorderMode == BackorderMode.NoBackorders &&
                    product.AllowBackInStockSubscriptions &&
                    await _productService.GetTotalStockQuantityAsync(product) > 0 &&
                    prevTotalStockQuantity <= 0 &&
                    product.Published &&
                    !product.Deleted)
                {
                    await _backInStockSubscriptionService.SendNotificationsToSubscribersAsync(product);
                }

                //delete an old "download" file (if deleted or updated)
                if (prevDownloadId > 0 && prevDownloadId != product.DownloadId)
                {
                    var prevDownload = await _downloadService.GetDownloadByIdAsync(prevDownloadId);
                    if (prevDownload != null)
                        await _downloadService.DeleteDownloadAsync(prevDownload);
                }

                //delete an old "sample download" file (if deleted or updated)
                if (prevSampleDownloadId > 0 && prevSampleDownloadId != product.SampleDownloadId)
                {
                    var prevSampleDownload = await _downloadService.GetDownloadByIdAsync(prevSampleDownloadId);
                    if (prevSampleDownload != null)
                        await _downloadService.DeleteDownloadAsync(prevSampleDownload);
                }

                //quantity change history
                if (previousWarehouseId != product.WarehouseId)
                {
                    //warehouse is changed 
                    //compose a message
                    var oldWarehouseMessage = string.Empty;
                    if (previousWarehouseId > 0)
                    {
                        var oldWarehouse = await _shippingService.GetWarehouseByIdAsync(previousWarehouseId);
                        if (oldWarehouse != null)
                            oldWarehouseMessage = string.Format(await _localizationService.GetResourceAsync("Admin.StockQuantityHistory.Messages.EditWarehouse.Old"), oldWarehouse.Name);
                    }

                    var newWarehouseMessage = string.Empty;
                    if (product.WarehouseId > 0)
                    {
                        var newWarehouse = await _shippingService.GetWarehouseByIdAsync(product.WarehouseId);
                        if (newWarehouse != null)
                            newWarehouseMessage = string.Format(await _localizationService.GetResourceAsync("Admin.StockQuantityHistory.Messages.EditWarehouse.New"), newWarehouse.Name);
                    }

                    var message = string.Format(await _localizationService.GetResourceAsync("Admin.StockQuantityHistory.Messages.EditWarehouse"), oldWarehouseMessage, newWarehouseMessage);

                    //record history
                    await _productService.AddStockQuantityHistoryEntryAsync(product, -previousStockQuantity, 0, previousWarehouseId, message);
                    await _productService.AddStockQuantityHistoryEntryAsync(product, product.StockQuantity, product.StockQuantity, product.WarehouseId, message);
                }
                else
                {
                    await _productService.AddStockQuantityHistoryEntryAsync(product, product.StockQuantity - previousStockQuantity, product.StockQuantity,
                        product.WarehouseId, await _localizationService.GetResourceAsync("Admin.StockQuantityHistory.Messages.Edit"));
                }

                //activity log
                await _customerActivityService.InsertActivityAsync("EditProduct",
                    string.Format(await _localizationService.GetResourceAsync("ActivityLog.EditProduct"), product.Name), product);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Catalog.Products.Updated"));

                if (!continueEditing)
                    return RedirectToAction("List");

                return RedirectToAction("Edit", new { id = product.Id });
            }

            //prepare model
            model = await _productModelFactory.PrepareProductModelAsync(model, product, true);

            //if we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> Delete(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts) ||
                !await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessVendorProductActions))
                return AccessDeniedView();

            //try to get a product with the specified id
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
                return RedirectToAction("List");

            //a vendor should have access only to his products
            var currentVendor = await _workContext.GetCurrentVendorAsync();
            if (currentVendor != null && product.VendorId != currentVendor.Id)
                return RedirectToAction("List");

            await _productService.DeleteProductAsync(product);

            //activity log
            await _customerActivityService.InsertActivityAsync("DeleteProduct",
                string.Format(await _localizationService.GetResourceAsync("ActivityLog.DeleteProduct"), product.Name), product);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Catalog.Products.Deleted"));

            return RedirectToAction("List");
        }

        [HttpPost]
        public virtual async Task<IActionResult> DeleteSelected(ICollection<int> selectedIds)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts) ||
                !await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessVendorProductActions))
                return AccessDeniedView();

            if (selectedIds == null || selectedIds.Count == 0)
                return NoContent();

            var currentVendor = await _workContext.GetCurrentVendorAsync();
            await _productService.DeleteProductsAsync((await _productService.GetProductsByIdsAsync(selectedIds.ToArray()))
                .Where(p => currentVendor == null || p.VendorId == currentVendor.Id).ToList());

            return Json(new { Result = true });
        }

        [HttpPost]
        public virtual async Task<IActionResult> UpdateProduct(ProductModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            //negative or 0 value should not be update
            if (model.StockQuantity < 0)
                return ErrorJson(await _localizationService.GetResourceAsync("Alchub.Admin.UpdateProduct.StockQuantity.Error"));
            if (model.Price <= 0)
                return ErrorJson(await _localizationService.GetResourceAsync("Alchub.Admin.UpdateProduct.Price.Error"));

            //try to get a product with the specified id
            var product = await _productService.GetProductByIdAsync(model.Id);
            if (product == null || product.Deleted)
                return new NullJsonResult();

            //a vendor should have access only to his products
            var currentVendor = await _workContext.GetCurrentVendorAsync();
            if (currentVendor != null && product.VendorId != currentVendor.Id)
                return new NullJsonResult();

            //some previously used values
            var previousStockQuantity = product.StockQuantity;

            //product
            product.Price = model.Price; //have to add markUp calculation
            product.UpdatedOnUtc = DateTime.UtcNow;
            product.StockQuantity = model.StockQuantity;
            product.Published = model.Published;

            await _productService.UpdateProductAsync(product);

            //search engine name
            model.SeName = await _urlRecordService.ValidateSeNameAsync(product, model.SeName, product.Name, true);
            await _urlRecordService.SaveSlugAsync(product, model.SeName, 0);

            //stockQuantity changed message
            await _productService.AddStockQuantityHistoryEntryAsync(product, product.StockQuantity - previousStockQuantity, product.StockQuantity,
               product.WarehouseId, await _localizationService.GetResourceAsync("Admin.StockQuantityHistory.Messages.Edit"));

            //activity log
            await _customerActivityService.InsertActivityAsync("EditProduct",
                string.Format(await _localizationService.GetResourceAsync("ActivityLog.EditProduct"), product.Name), product);

            return new NullJsonResult();
        }

        [HttpPost]
        public virtual async Task<IActionResult> CopyProduct(ProductModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts) ||
                !await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessVendorProductActions))
                return AccessDeniedView();

            var copyModel = model.CopyProductModel;
            try
            {
                var originalProduct = await _productService.GetProductByIdAsync(copyModel.Id);

                //a vendor should have access only to his products
                var currentVendor = await _workContext.GetCurrentVendorAsync();
                if (currentVendor != null && originalProduct.VendorId != currentVendor.Id)
                    return RedirectToAction("List");

                var vendorId = currentVendor != null ? currentVendor.Id : originalProduct.VendorId;
                var newProduct = await _copyProductService.CopyProductAsync(originalProduct, copyModel.Name, copyModel.Published, copyModel.CopyImages, vendorId: vendorId);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Catalog.Products.Copied"));

                return RedirectToAction("Edit", new { id = newProduct.Id });
            }
            catch (Exception exc)
            {
                _notificationService.ErrorNotification(exc.Message);
                return RedirectToAction("Edit", new { id = copyModel.Id });
            }
        }

        #endregion

        #region Export / Import

        [HttpPost, ActionName("DownloadCatalogPDF")]
        [FormValueRequired("download-catalog-pdf")]
        public virtual async Task<IActionResult> DownloadCatalogAsPdf(ProductSearchModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts) ||
                !await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessVendorProductActions))
                return AccessDeniedView();

            //a vendor should have access only to his products
            var currentVendor = await _workContext.GetCurrentVendorAsync();
            if (currentVendor != null)
            {
                model.SearchVendorId = currentVendor.Id;
            }

            var categoryIds = new List<int> { model.SearchCategoryId };
            //include subcategories
            if (model.SearchIncludeSubCategories && model.SearchCategoryId > 0)
                categoryIds.AddRange(await _categoryService.GetChildCategoryIdsAsync(parentCategoryId: model.SearchCategoryId, showHidden: true));

            //0 - all (according to "ShowHidden" parameter)
            //1 - published only
            //2 - unpublished only
            bool? overridePublished = null;
            if (model.SearchPublishedId == 1)
                overridePublished = true;
            else if (model.SearchPublishedId == 2)
                overridePublished = false;

            var products = await _productService.SearchProductsAsync(0,
                categoryIds: categoryIds,
                manufacturerIds: new List<int> { model.SearchManufacturerId },
                storeId: model.SearchStoreId,
                vendorId: model.SearchVendorId,
                warehouseId: model.SearchWarehouseId,
                productType: model.SearchProductTypeId > 0 ? (ProductType?)model.SearchProductTypeId : null,
                keywords: model.SearchProductName,
                showHidden: true,
                overridePublished: overridePublished);

            try
            {
                byte[] bytes;
                await using (var stream = new MemoryStream())
                {
                    await _pdfService.PrintProductsToPdfAsync(stream, products);
                    bytes = stream.ToArray();
                }

                return File(bytes, MimeTypes.ApplicationPdf, "pdfcatalog.pdf");
            }
            catch (Exception exc)
            {
                await _notificationService.ErrorNotificationAsync(exc);
                return RedirectToAction("List");
            }
        }

        [HttpPost, ActionName("ExportToXml")]
        [FormValueRequired("exportxml-all")]
        public virtual async Task<IActionResult> ExportXmlAll(ProductSearchModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts) ||
                !await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessVendorProductActions))
                return AccessDeniedView();

            //a vendor should have access only to his products
            var currentVendor = await _workContext.GetCurrentVendorAsync();
            if (currentVendor != null)
            {
                model.SearchVendorId = currentVendor.Id;
            }

            var categoryIds = new List<int> { model.SearchCategoryId };
            //include subcategories
            if (model.SearchIncludeSubCategories && model.SearchCategoryId > 0)
                categoryIds.AddRange(await _categoryService.GetChildCategoryIdsAsync(parentCategoryId: model.SearchCategoryId, showHidden: true));

            //0 - all (according to "ShowHidden" parameter)
            //1 - published only
            //2 - unpublished only
            bool? overridePublished = null;
            if (model.SearchPublishedId == 1)
                overridePublished = true;
            else if (model.SearchPublishedId == 2)
                overridePublished = false;

            var products = await _productService.SearchProductsAsync(0,
                categoryIds: categoryIds,
                manufacturerIds: new List<int> { model.SearchManufacturerId },
                storeId: model.SearchStoreId,
                vendorId: model.SearchVendorId,
                warehouseId: model.SearchWarehouseId,
                productType: model.SearchProductTypeId > 0 ? (ProductType?)model.SearchProductTypeId : null,
                keywords: model.SearchProductName,
                showHidden: true,
                overridePublished: overridePublished);

            try
            {
                var xml = await _exportManager.ExportProductsToXmlAsync(products);

                return File(Encoding.UTF8.GetBytes(xml), MimeTypes.ApplicationXml, "products.xml");
            }
            catch (Exception exc)
            {
                await _notificationService.ErrorNotificationAsync(exc);
                return RedirectToAction("List");
            }
        }

        [HttpPost]
        public virtual async Task<IActionResult> ExportXmlSelected(string selectedIds)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts) ||
                !await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessVendorProductActions))
                return AccessDeniedView();

            var products = new List<Product>();
            if (selectedIds != null)
            {
                var ids = selectedIds
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => Convert.ToInt32(x))
                    .ToArray();
                products.AddRange(await _productService.GetProductsByIdsAsync(ids));
            }
            //a vendor should have access only to his products
            var currentVendor = await _workContext.GetCurrentVendorAsync();
            if (currentVendor != null)
            {
                products = products.Where(p => p.VendorId == currentVendor.Id).ToList();
            }

            try
            {
                var xml = await _exportManager.ExportProductsToXmlAsync(products);
                return File(Encoding.UTF8.GetBytes(xml), MimeTypes.ApplicationXml, "products.xml");
            }
            catch (Exception exc)
            {
                await _notificationService.ErrorNotificationAsync(exc);
                return RedirectToAction("List");
            }
        }

        [HttpPost, ActionName("ExportToExcel")]
        [FormValueRequired("exportexcel-all")]
        public virtual async Task<IActionResult> ExportExcelAll(ProductSearchModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts) ||
                !await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessVendorProductActions))
                return AccessDeniedView();

            //a vendor should have access only to his products
            var currentVendor = await _workContext.GetCurrentVendorAsync();
            if (currentVendor != null)
            {
                model.SearchVendorId = currentVendor.Id;
            }

            var categoryIds = new List<int> { model.SearchCategoryId };
            //include subcategories
            if (model.SearchIncludeSubCategories && model.SearchCategoryId > 0)
                categoryIds.AddRange(await _categoryService.GetChildCategoryIdsAsync(parentCategoryId: model.SearchCategoryId, showHidden: true));

            //0 - all (according to "ShowHidden" parameter)
            //1 - published only
            //2 - unpublished only
            bool? overridePublished = null;
            if (model.SearchPublishedId == 1)
                overridePublished = true;
            else if (model.SearchPublishedId == 2)
                overridePublished = false;

            var products = await _productService.SearchProductsAsync(0,
                categoryIds: categoryIds,
                manufacturerIds: new List<int> { model.SearchManufacturerId },
                storeId: model.SearchStoreId,
                vendorId: model.SearchVendorId,
                warehouseId: model.SearchWarehouseId,
                productType: model.SearchProductTypeId > 0 ? (ProductType?)model.SearchProductTypeId : null,
                keywords: model.SearchProductName,
                showHidden: true,
                overridePublished: overridePublished);

            try
            {
                var bytes = await _exportManager.ExportProductsToXlsxAsync(products);

                return File(bytes, MimeTypes.TextXlsx, "products.xlsx");
            }
            catch (Exception exc)
            {
                await _notificationService.ErrorNotificationAsync(exc);

                return RedirectToAction("List");
            }
        }

        [HttpPost]
        public virtual async Task<IActionResult> ExportExcelSelected(string selectedIds)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts) ||
                !await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessVendorProductActions))
                return AccessDeniedView();

            var products = new List<Product>();
            if (selectedIds != null)
            {
                var ids = selectedIds
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => Convert.ToInt32(x))
                    .ToArray();
                products.AddRange(await _productService.GetProductsByIdsAsync(ids));
            }
            //a vendor should have access only to his products
            var currentVendor = await _workContext.GetCurrentVendorAsync();
            if (currentVendor != null)
            {
                products = products.Where(p => p.VendorId == currentVendor.Id).ToList();
            }

            try
            {
                var bytes = await _exportManager.ExportProductsToXlsxAsync(products);

                return File(bytes, MimeTypes.TextXlsx, "products.xlsx");
            }
            catch (Exception exc)
            {
                await _notificationService.ErrorNotificationAsync(exc);
                return RedirectToAction("List");
            }
        }

        [HttpPost]
        public virtual async Task<IActionResult> ImportExcel(IFormFile importexcelfile)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts) ||
                !await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessVendorProductActions))
                return AccessDeniedView();

            if (await _workContext.GetCurrentVendorAsync() != null && !_vendorSettings.AllowVendorsToImportProducts)
                //a vendor can not import products
                return AccessDeniedView();

            try
            {
                if (importexcelfile != null && importexcelfile.Length > 0)
                {
                    await _importManager.ImportProductsFromXlsxAsync(importexcelfile.OpenReadStream());
                }
                else
                {
                    _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Common.UploadFile"));

                    return RedirectToAction("List");
                }

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Catalog.Products.Imported"));

                return RedirectToAction("List");
            }
            catch (Exception exc)
            {
                await _notificationService.ErrorNotificationAsync(exc);

                return RedirectToAction("List");
            }
        }

        #endregion

        #region Vendor export/import

        [HttpPost]
        public virtual async Task<IActionResult> ImportExcelAsVendor(IFormFile importexcelfile)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts) ||
                !await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessVendorProductActions))
                return AccessDeniedView();

            try
            {
                if (importexcelfile != null && importexcelfile.Length > 0)
                {
                    await using var fileStream = importexcelfile.OpenReadStream();
                    await _importManager.VendorImportProductsFromXlsxAsync(fileStream);
                }
                else
                {
                    _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Common.UploadFile"));

                    return RedirectToAction("List");
                }

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Catalog.Products.Imported"));

                return RedirectToAction("List");
            }
            catch (Exception exc)
            {
                await _notificationService.ErrorNotificationAsync(exc);

                return RedirectToAction("List");
            }
        }

        [HttpPost, ActionName("DownloadTopBestSellingProductExcel")]
        [FormValueRequired("download-topsellingproducts-excel")]
        public virtual async Task<IActionResult> DownloadTopBestSellingProductAsExcel()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts) ||
                !await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessVendorProductActions))
                return AccessDeniedView();

            try
            {
                var filename = await _settingService.GetSettingByKeyAsync<string>("TopBestSelling.Products.Excel", defaultValue: null);
                if (filename == null)
                {
                    _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Common.FileNotFound"));
                    return RedirectToAction("List");
                }

                var targetPath = _fileProvider.Combine(_webHostEnvironment.WebRootPath, "TopSellingProductsXlsFile");
                var filePath = _fileProvider.Combine(targetPath, filename);

                var memory = new MemoryStream();
                using (var stream = new FileStream(filePath, FileMode.Open))
                {
                    await stream.CopyToAsync(memory);
                }

                memory.Position = 0;
                return File(memory, MimeTypes.TextXlsx, filename);
            }
            catch (Exception exc)
            {
                _notificationService.ErrorNotification(exc.Message.ToString());
                return RedirectToAction("List");
            }
        }

        #endregion

        #region Alchub

        [HttpPost]
        public virtual async Task<IActionResult> SaveOverridesSelected(string selectedIds,
            bool isMasterSelected,
            int selectedCategoryId = 0,
            string searchProductName = null,
            bool? overridePrice = null,
            bool? overrideStock = null,
            bool? overrideNegativeStock = null,
            bool? selectedPublished = null)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var currentVendor = await _workContext.GetCurrentVendorAsync();
            if (currentVendor == null)
                return AccessDeniedView();

            var products = new List<Product>();
            if (isMasterSelected == true)
            {
                var categoryIds = new List<int>() { selectedCategoryId };

                //get all products of current vendor
                products = (await _productService.SearchProductsAsync(showHidden: true, categoryIds: categoryIds,
                    vendorId: currentVendor.Id, keywords: searchProductName, isMaster: false))?.ToList();
            }
            else
            {
                if (selectedIds != null)
                {
                    var ids = selectedIds
                        .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(x => Convert.ToInt32(x))
                        .ToArray();
                    products.AddRange(await _productService.GetProductsByIdsAsync(ids));
                }

                //a vendor should have access only to his products
                products = products.Where(p => p.VendorId == currentVendor.Id).ToList();
            }

            foreach (var product in products)
            {
                try
                {
                    if (overrideStock.HasValue)
                        product.OverrideStock = overrideStock.Value;

                    if (overridePrice.HasValue)
                        product.OverridePrice = overridePrice.Value;

                    if (overrideNegativeStock.HasValue)
                        product.OverrideNegativeStock = overrideNegativeStock.Value;

                    if (selectedPublished.HasValue)
                        product.Published = selectedPublished.Value;

                    await _productService.UpdateProductAsync(product);
                }
                catch (Exception exc)
                {
                    await _notificationService.ErrorNotificationAsync(exc);
                    return RedirectToAction("List");
                }
            }

            //notification according action
            if (overridePrice.HasValue && overridePrice.Value)
                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Alchub.Admin.Catalog.Products.overridePrice.Changes.Saved"));
            else if (overrideStock.HasValue && overrideStock.Value)
                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Alchub.Admin.Catalog.Products.overrideStock.Changes.Saved"));
            else if (overrideNegativeStock.HasValue && overrideNegativeStock.Value)
                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Alchub.Admin.Catalog.Products.overrideNegativeStock.Changes.Saved"));
            else if (selectedPublished.HasValue && selectedPublished.Value)
                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Alchub.Admin.Catalog.Products.Published.Changes.Saved"));

            return RedirectToAction("List");
        }

        #region Vednor product temp

        public class VendorProducCategoryMap
        {
            public VendorProducCategoryMap()
            {
                VendorProducts = new List<VendorProduct>();
            }

            public int VendorId { get; set; }

            public List<VendorProduct> VendorProducts { get; set; }

            #region Nested

            public class VendorProduct
            {
                public VendorProduct()
                {
                    CategoryIds = new List<int>();
                }
                public int ProductId { get; set; }
                public IList<int> CategoryIds { get; set; }
            }

            #endregion
        }

        [HttpPost]
        public virtual async Task<IActionResult> FixVendorProductsCategoriesMap()
        {
            try
            {
                int vendorId = 0;
                var currentVendor = await _workContext.GetCurrentVendorAsync();
                if (currentVendor != null)
                    vendorId = currentVendor.Id;

                var vendorIds = new List<int>();
                if (vendorId > 0)
                    vendorIds.Add(vendorId);
                else
                {
                    //all vednors
                    var allVendors = await _vendorService.GetAllVendorsAsync();
                    vendorIds.AddRange(allVendors?.Select(v => v.Id));
                }

                int processedProductsCount = 0;
                var vendorProductCategoryMappings = new List<VendorProducCategoryMap>();

                foreach (var vId in vendorIds)
                {
                    //get vendor products
                    var vendorProducts = await _productService.SearchProductsAsync(showHidden: true,
                        vendorId: vId,
                        productType: ProductType.SimpleProduct,
                        isMaster: false);

                    var vendorProductsMap = new List<VendorProducCategoryMap.VendorProduct>();

                    foreach (var vendorProduct in vendorProducts)
                    {
                        var upcArray = new List<string> { vendorProduct.UPCCode }.ToArray();
                        //get its master product
                        var masterProduct = (await _productService.GetMasterProductsByUPCCodeAsync(upcArray))?.FirstOrDefault();
                        if (masterProduct == null)
                            continue;

                        //get master categories
                        var masterCategories = await _categoryService.GetProductCategoriesByProductIdAsync(masterProduct.Id, true);
                        //vendor product categories 
                        var vendorProductCategories = await _categoryService.GetProductCategoriesByProductIdAsync(vendorProduct.Id, true);

                        //delete categories which are maped in vendor product but not mapped with master product
                        foreach (var vendorProductCategory in vendorProductCategories)
                        {
                            var materCategoryIds = masterCategories?.Select(x => x.CategoryId) ?? new List<int>();
                            if (!materCategoryIds.Contains(vendorProductCategory.CategoryId))
                                await _categoryService.DeleteProductCategoryAsync(vendorProductCategory);
                        }

                        var mappedCategoryIds = new List<int>();

                        //insert product categories if not exist
                        foreach (var masterCategory in masterCategories)
                        {
                            var matchRecord = vendorProductCategories?.FirstOrDefault(x => x.CategoryId == masterCategory.CategoryId);
                            if (matchRecord == null)
                            {
                                //insert
                                var newVendorProductCategory = new ProductCategory
                                {
                                    ProductId = vendorProduct.Id,
                                    CategoryId = masterCategory.CategoryId,
                                    IsFeaturedProduct = masterCategory.IsFeaturedProduct,
                                    DisplayOrder = masterCategory.DisplayOrder
                                };

                                await _categoryService.InsertProductCategoryAsync(newVendorProductCategory);

                                //add mapping record
                                mappedCategoryIds.Add(masterCategory.CategoryId);
                                processedProductsCount++;
                            }
                        }

                        //add map record if any
                        if (mappedCategoryIds.Any())
                        {
                            vendorProductsMap.Add(new VendorProducCategoryMap.VendorProduct
                            {
                                ProductId = vendorProduct.Id,
                                CategoryIds = mappedCategoryIds
                            });
                        }
                    }

                    //add vendor category record if any 
                    if (vendorProductsMap.Any())
                    {
                        vendorProductCategoryMappings.Add(new VendorProducCategoryMap
                        {
                            VendorId = vId,
                            VendorProducts = vendorProductsMap
                        });
                    }
                }

                string dataJson = JsonSerializer.Serialize<IList<VendorProducCategoryMap>>(vendorProductCategoryMappings);
                return Json(new
                {
                    success = true,
                    message = $"Total {processedProductsCount} vendor products mapping updated!",
                    mapiingData = dataJson
                });

            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex.ToString(),
                });
            }
        }

        #endregion

        #endregion
    }
}