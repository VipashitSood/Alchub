using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Vendors;
using Nop.Core.Infrastructure;
using Nop.Services.Alchub.Catalog;
using Nop.Services.Alchub.General;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Discounts;
using Nop.Services.ExportImport;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Markup;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Shipping;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Framework.Mvc.ModelBinding;
using static Nop.Web.Areas.Admin.Models.Catalog.ProductSearchModel;

namespace Nop.Web.Areas.Admin.Controllers
{
    public partial class AddVendorProductController : BaseAdminController
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
        private readonly IAlchubGeneralService _alchubGeneralService;
        private readonly ICategoryMarkupService _categoryMarkupService;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly IProductFriendlyUpcService _productFriendlyUpcService;

        #endregion

        #region Ctor

        public AddVendorProductController(IAclService aclService,
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
            IAlchubGeneralService alchubGeneralService,
            ICategoryMarkupService categoryMarkupService,
            IWorkflowMessageService workflowMessageService,
            IProductFriendlyUpcService productFriendlyUpcService)
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
            _alchubGeneralService = alchubGeneralService;
            _categoryMarkupService = categoryMarkupService;
            _workflowMessageService = workflowMessageService;
            _productFriendlyUpcService = productFriendlyUpcService;
        }

        #endregion

        #region Utilites 


        private async Task ApplyMarkupOnProductPrice(Product product, int vendorId)
        {
            ////get all products of current vendor
            //var products = await _productService.SearchProductsAsync(vendorId: vendorId,
            //    overridePublished: true, storeId: storeId, visibleIndividuallyOnly: true);

            var categoryIds = (await _categoryService.GetProductCategoriesByProductIdAsync(product.Id))?.
                                Select(pc => pc.CategoryId)?.ToList();
            if (!categoryIds.Any())
                return;

            //get category markup by categoryIds and vendor id
            var categoryMarkup = await _categoryMarkupService.GetCategoryMarkupAsync(categoryIds, vendorId);
            if (categoryMarkup == null)
                return;

            var calPrice = Math.Round(product.Price * categoryMarkup.Markup, 2, MidpointRounding.AwayFromZero) / 100;
            product.Price += calPrice;
            //second decimal place should always be 9.
            var price = string.Format("{0:0.0}", product.Price - (product.Price % 0.1M));
            if (Convert.ToDecimal(price) > 0)
                product.Price = Convert.ToDecimal(price + "9");
            await _productService.UpdateProductAsync(product);
        }

        private async Task SendEmailForInvalidProduct(Vendor vendor, string upcode, string name, string category)
        {
            var invalidUPCCodes = new List<AddProductVendor>();
            var languageId = (await _workContext.GetWorkingLanguageAsync()).Id;

            var product = await _alchubGeneralService.GetMasterProductByUpcCodeAsync(upcode);
            if (product == null)
            {
                AddProductVendor addProductVendor = new AddProductVendor();
                addProductVendor.Name = name;
                addProductVendor.UPCCode = upcode;
                addProductVendor.Category = category;
                invalidUPCCodes.Add(addProductVendor);
            }
            //send invalid product email
            if (invalidUPCCodes.Any())
            {
                var sb = new StringBuilder();
                var body = await UPCCodeHtmlTable(sb, invalidUPCCodes, languageId);

                //send email to super admin
                await _workflowMessageService.SendInvalidProductMessage(languageId, vendor, body);
            }
        }

        private Task<string> UPCCodeHtmlTable(StringBuilder sb, List<AddProductVendor> addProductVendors = null, int languageId = 0)
        {
            if (addProductVendors.Any())
            {
                int sNo = 1;
                sb.AppendLine("<table border=\"1\" style=\"border-collapse: collapse;\">");
                sb.AppendLine($"<tr>");
                sb.AppendLine($"<th>Name<br /></th>");
                sb.AppendLine($"<th>UPCCode/SKU <br /></th>");
                sb.AppendLine($"<th>Category<br /></th>");
                sb.AppendLine("</tr>");

                foreach (var item in addProductVendors)
                {
                    sb.AppendLine($"<tr style=\"text-align: center;\">");
                    sb.AppendLine("<td style=\"padding: 0.6em 0.4em;text-align: left;\">" + item.Name + "</td>");
                    sb.AppendLine("<td style=\"padding: 0.6em 0.4em;text-align: left;\">" + item.UPCCode + "</td>");
                    sb.AppendLine("<td style=\"padding: 0.6em 0.4em;text-align: left;\">" + item.Category + "</td>");
                    sb.AppendLine("</tr>");
                }
                sb.AppendLine("</table>");
            }
            return Task.FromResult(sb?.ToString());
        }
        #endregion

        #region Methods

        public virtual IActionResult Index()
        {
            return RedirectToAction("List");
        }

        public virtual async Task<IActionResult> List()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            //prepare model
            var model = await _productModelFactory.PrepareProductSearchModelAsync(new ProductSearchModel());

            return View(model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> ProductList(ProductSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            //prepare model
            var model = await _productModelFactory.PrepareVendorForMasterProductListModelAsync(searchModel);

            return Json(model);
        }


        [HttpPost]
        public virtual async Task<IActionResult> AddProduct(ProductSearchModel.AddProductVendor model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessVendorProductActions))
                return ErrorJson("You do not have permission to perform the selected operation");

            if (!ModelState.IsValid)
                return ErrorJson(ModelState.SerializeErrors());

            //prepare model
            var storeId = (await _storeContext.GetCurrentStoreAsync()).Id;
            var currentVendor = await _workContext.GetCurrentVendorAsync();
            if (currentVendor == null)
            {
                _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Alchub.Error.Message.Admin.Cannot.Add.Product"));
                return Json(new { Result = true });
            }

            //get all master products by upcCode
            Product product = null;
            string[] upccode = new string[] { model.UPCCode };

            //upccode fetch 
            if (!string.IsNullOrEmpty(model.UPCCode))
            {
                //upc code is sku, fetch by sku 
                product = (await _productService.GetMasterProductsByUPCCodeAsync(upccode, 0))?.FirstOrDefault();
                if (product == null)
                {
                    //sku is upc code, fetch by upc code
                    product = (await _productService.GetMasterProductsBySKUCodeAsync(upccode, 0)).FirstOrDefault();
                }

                //check with friendly upc - 03/07/2023
                if (product == null)
                    product = await _productFriendlyUpcService.GetMasterProductByFriendlyUPCCodeAsync(currentVendor.Id, model.UPCCode);
            }
            if (product == null || string.IsNullOrEmpty(model.UPCCode))
            {
                //send email to super admin for invalid products
                await SendEmailForInvalidProduct(currentVendor, model.UPCCode, model.Name, model.Category);
                _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Alchub.Error.Message.Add.Vendor.Product"));
                return Json(new { Result = false });
            }

            product.StockQuantity = model.Stock;
            product.Price = model.Price;

            var existingProduct = await _productService.GetVendorProduct(vendorId: currentVendor.Id, storeId: storeId, upcCode: product.UPCCode);
            if (existingProduct == null || existingProduct?.Id == 0)
                existingProduct = (await _productService.GetProductsBySkuAsync(upccode, currentVendor.Id))?.
             Where(x => x.UPCCode != null)?.FirstOrDefault();

            if (existingProduct != null)
            {
                _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Alchub.Error.Already.Exist.Vendor.Product"));
                return Json(new { Result = false });
            }
            else
            {
                var exProduct = product;
                var newProduct = await _copyProductService.CopyProductAsync(product, product.Name, true, true, vendorId: currentVendor.Id);

                //update some values for new product which was not copied
                newProduct.VendorId = currentVendor.Id;
                newProduct.Sku = exProduct.Sku;
                //default config flag
                newProduct.IsMaster = false;
                newProduct.OverridePrice = true;
                newProduct.OverrideStock = true;
                newProduct.Price = model.Price;
                newProduct.StockQuantity = model.Stock;
                //associated product handle
                newProduct.ParentGroupedProductId = 0;
                await _productService.UpdateProductAsync(newProduct);

                //apply markup only on current product price --- 20/06/23 
                await ApplyMarkupOnProductPrice(newProduct, currentVendor.Id);
            }

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Alchub.Successfully.Message.Add.Vendor.Product"));
            return Json(new { Result = true });
        }
        #endregion
    }
}