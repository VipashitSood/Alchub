using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using ClosedXML.Excel;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core;
using Nop.Core.Alchub.Domain;
using Nop.Core.Alchub.Domain.Vendors;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Tax;
using Nop.Core.Domain.Vendors;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Services.Alchub.Catalog;
using Nop.Services.Alchub.ExportImport;
using Nop.Services.Alchub.General;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.ExportImport.Help;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Markup;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Seo;
using Nop.Services.Shipping;
using Nop.Services.Shipping.Date;
using Nop.Services.Stores;
using Nop.Services.Tax;
using Nop.Services.Vendors;

namespace Nop.Services.ExportImport
{
    public partial class ImportManager : IImportManager
    {
        #region fields

        private readonly CatalogSettings _catalogSettings;
        private readonly ICategoryService _categoryService;
        private readonly ICountryService _countryService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly INopDataProvider _dataProvider;
        private readonly IDateRangeService _dateRangeService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILocalizationService _localizationService;
        private readonly ILogger _logger;
        private readonly IManufacturerService _manufacturerService;
        private readonly IMeasureService _measureService;
        private readonly INewsLetterSubscriptionService _newsLetterSubscriptionService;
        private readonly INopFileProvider _fileProvider;
        private readonly IPictureService _pictureService;
        private readonly IProductAttributeService _productAttributeService;
        private readonly IProductService _productService;
        private readonly IProductTagService _productTagService;
        private readonly IProductTemplateService _productTemplateService;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IShippingService _shippingService;
        private readonly ISpecificationAttributeService _specificationAttributeService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly IStoreContext _storeContext;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IStoreService _storeService;
        private readonly ITaxCategoryService _taxCategoryService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IVendorService _vendorService;
        private readonly IWorkContext _workContext;
        private readonly MediaSettings _mediaSettings;
        private readonly VendorSettings _vendorSettings;
        private readonly ICopyProductService _copyProductService;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly IAlchubGeneralService _alchubGeneralService;
        private readonly ICategoryMarkupService _categoryMarkupService;
        private readonly ISettingService _settingService;
        private readonly AlchubSettings _alchubSettings;
        private readonly IProductFriendlyUpcService _productFriendlyUpcService;

        #endregion

        #region ctor

        public ImportManager(CatalogSettings catalogSettings,
            ICategoryService categoryService,
            ICountryService countryService,
            ICustomerActivityService customerActivityService,
            INopDataProvider dataProvider,
            IDateRangeService dateRangeService,
            IHttpClientFactory httpClientFactory,
            ILocalizationService localizationService,
            ILogger logger,
            IManufacturerService manufacturerService,
            IMeasureService measureService,
            INewsLetterSubscriptionService newsLetterSubscriptionService,
            INopFileProvider fileProvider,
            IPictureService pictureService,
            IProductAttributeService productAttributeService,
            IProductService productService,
            IProductTagService productTagService,
            IProductTemplateService productTemplateService,
            IServiceScopeFactory serviceScopeFactory,
            IShippingService shippingService,
            ISpecificationAttributeService specificationAttributeService,
            IStateProvinceService stateProvinceService,
            IStoreContext storeContext,
            IStoreMappingService storeMappingService,
            IStoreService storeService,
            ITaxCategoryService taxCategoryService,
            IUrlRecordService urlRecordService,
            IVendorService vendorService,
            IWorkContext workContext,
            MediaSettings mediaSettings,
            VendorSettings vendorSettings,
            ICopyProductService copyProductService,
            IWorkflowMessageService workflowMessageService,
            IAlchubGeneralService alchubGeneralService,
            ICategoryMarkupService categoryMarkupService,
            ISettingService settingService,
            AlchubSettings alchubSettings,
            IProductFriendlyUpcService productFriendlyUpcService)
        {
            _catalogSettings = catalogSettings;
            _categoryService = categoryService;
            _countryService = countryService;
            _customerActivityService = customerActivityService;
            _dataProvider = dataProvider;
            _dateRangeService = dateRangeService;
            _httpClientFactory = httpClientFactory;
            _fileProvider = fileProvider;
            _localizationService = localizationService;
            _logger = logger;
            _manufacturerService = manufacturerService;
            _measureService = measureService;
            _newsLetterSubscriptionService = newsLetterSubscriptionService;
            _pictureService = pictureService;
            _productAttributeService = productAttributeService;
            _productService = productService;
            _productTagService = productTagService;
            _productTemplateService = productTemplateService;
            _serviceScopeFactory = serviceScopeFactory;
            _shippingService = shippingService;
            _specificationAttributeService = specificationAttributeService;
            _stateProvinceService = stateProvinceService;
            _storeContext = storeContext;
            _storeMappingService = storeMappingService;
            _storeService = storeService;
            _taxCategoryService = taxCategoryService;
            _urlRecordService = urlRecordService;
            _vendorService = vendorService;
            _workContext = workContext;
            _mediaSettings = mediaSettings;
            _vendorSettings = vendorSettings;
            _copyProductService = copyProductService;
            _workflowMessageService = workflowMessageService;
            _alchubGeneralService = alchubGeneralService;
            _categoryMarkupService = categoryMarkupService;
            _settingService = settingService;
            _alchubSettings = alchubSettings;
            _productFriendlyUpcService = productFriendlyUpcService;
        }

        #endregion

        #region Const

        //private const string ATT_COLOR = "Color";
        //private const string ATT_TYPE = "Type";
        //private const string ATT_COUNTRY_OF_ORIGIN = "Country of Origin";
        //private const string ATT_STATE_OF_ORIGIN = "State of Origin";
        //private const string ATT_SUBTYPE = "Sub Type";
        //private const string ATT_REGION = "Region";
        //private const string ATT_ABV = "ABV";
        //private const string ATT_FLAVOR = "Flavor";
        //private const string ATT_VINTAGE = "Vintage";
        //private const string ATT_ALCOHOL_PROOF = "Alcohol Proof";
        //private const string ATT_SPECIALTY = "Specialty";
        //private const string ATT_RATINGS = "Ratings";
        //private const string ATT_FOOD_PAIRING = "Food Pairing";
        //private const string ATT_BODY = "Body";
        //private const string ATT_TASTING_NOTES = "Tasting Notes";
        //private const string ATT_CONTAINER = "Container";
        //private const string ATT_BASE_UNIT_CLOSURE = "Base Unit Closure";
        //private const string ATT_APPELLATION = "Appellation";
        //private const string ATT_BRAND_DESCRIPTION = "Brand Description";
        //private const string ATT_SIZE = "Size";

        //syn excel vendor products (modisoft)
        private const string SCAN_CODE = "Scan Code";
        private const string CURRENT_QTY = "Current Qty";
        private const string RETAIL = "Retail";
        private const string PRODUCT_NAME = "Description";

        #endregion

        #region Utilities
        private async Task<ImportProductMetadata> PrepareVendorImportedProductDataAsync(IXLWorksheet worksheet)
        {
            //the columns
            var properties = GetPropertiesByExcelCells<Product>(worksheet);
            var manager = new PropertyManager<Product>(properties, _catalogSettings);
            var endRow = 2;
            var allSku = new List<string>();
            var skuProperty = manager.GetProperty("SKU");
            var skuCellNum = skuProperty?.PropertyOrderPosition ?? -1;
            var productsInFile = new List<int>();

            //find end of data
            while (true)
            {
                var allColumnsAreEmpty = manager.GetProperties
                    .Select(property => worksheet.Row(endRow).Cell(property.PropertyOrderPosition))
                    .All(cell => string.IsNullOrEmpty(cell?.Value?.ToString()));

                if (allColumnsAreEmpty)
                    break;

                if (skuCellNum > 0)
                {
                    var skuCode = worksheet.Row(endRow).Cell(skuCellNum).Value?.ToString() ?? string.Empty;
                    if (!string.IsNullOrEmpty(skuCode))
                    {
                        skuCode = skuCode.Trim();
                        allSku.Add(skuCode);
                    }
                }

                //counting the number of products
                productsInFile.Add(endRow);

                endRow++;
            }

            return new ImportProductMetadata
            {
                EndRow = endRow,
                Manager = manager,
                Properties = properties,
                ProductsInFile = productsInFile,
                SkuCellNum = skuCellNum,
                AllSku = allSku
            };
        }

        private Task<string> UPCCodeHtmlTable(StringBuilder sb, List<ImportFailedProduct> invalidProducts, int languageId)
        {
            if (invalidProducts != null && invalidProducts.Any())
            {
                var sNo = 1;
                sb.AppendLine("<table border=\"1\" style=\"border-collapse: collapse;\">");
                sb.AppendLine($"<tr>");
                sb.AppendLine($"<th>S. NO.</th>");
                sb.AppendLine($"<th>Name</th>");
                sb.AppendLine($"<th>UPC</th>");
                sb.AppendLine($"<th>Price</th>");
                sb.AppendLine($"<th>Stock</th>");
                sb.AppendLine($"<th>Size</th>");
                sb.AppendLine("</tr>");

                var table = invalidProducts;
                for (var i = 0; i <= table.Count - 1; i++)
                {
                    var invalidProduct = table[i];

                    sb.AppendLine($"<tr style=\"text-align: center;\">");
                    sb.AppendLine("<td style=\"padding: 0.6em 0.4em;text-align: left;\">" + sNo + "</td>");
                    sb.AppendLine("<td style=\"padding: 0.6em 0.4em;text-align: left;\">" + invalidProduct.Name + "</td>");
                    sb.AppendLine("<td style=\"padding: 0.6em 0.4em;text-align: left;\">" + invalidProduct.Sku + "</td>");
                    sb.AppendLine("<td style=\"padding: 0.6em 0.4em;text-align: left;\">" + invalidProduct.Price + "</td>");
                    sb.AppendLine("<td style=\"padding: 0.6em 0.4em;text-align: left;\">" + invalidProduct.Stock + "</td>");
                    sb.AppendLine("<td style=\"padding: 0.6em 0.4em;text-align: left;\">" + invalidProduct.Size + "</td>");
                    sb.AppendLine("</tr>");
                    sNo++;
                }
                sb.AppendLine("</table>");
            }

            return Task.FromResult(sb?.ToString());
        }

        private Task<string> ImportsFailedHtmlTable(StringBuilder sb, List<ImportFailedProduct> invalidProducts, int languageId)
        {
            if (invalidProducts != null && invalidProducts.Any())
            {
                var sNo = 1;
                sb.AppendLine("<table border=\"1\" style=\"border-collapse: collapse;\">");
                sb.AppendLine($"<tr>");
                sb.AppendLine($"<th>S. NO.</th>");
                sb.AppendLine($"<th>Name</th>");
                sb.AppendLine($"<th>Sku</th>");
                sb.AppendLine($"<th>Upc code</th>");
                sb.AppendLine($"<th>Reason of failure</th>");
                sb.AppendLine("</tr>");

                var table = invalidProducts;
                for (var i = 0; i <= table.Count - 1; i++)
                {
                    var invalidProduct = table[i];

                    sb.AppendLine($"<tr style=\"text-align: center;\">");
                    sb.AppendLine("<td style=\"padding: 0.6em 0.4em;text-align: left;\">" + sNo + "</td>");
                    sb.AppendLine("<td style=\"padding: 0.6em 0.4em;text-align: left;\">" + invalidProduct.Name + "</td>");
                    sb.AppendLine("<td style=\"padding: 0.6em 0.4em;text-align: left;\">" + invalidProduct.Sku + "</td>");
                    sb.AppendLine("<td style=\"padding: 0.6em 0.4em;text-align: left;\">" + invalidProduct.Upc + "</td>");
                    sb.AppendLine("<td style=\"padding: 0.6em 0.4em;text-align: left;\">" + invalidProduct.Exception + "</td>");
                    sb.AppendLine("</tr>");
                    sNo++;
                }
                sb.AppendLine("</table>");
            }

            return Task.FromResult(sb?.ToString());
        }

        /// <summary>
        /// Apply category markup on imported products
        /// </summary>
        /// <param name="vendorId"></param>
        /// <param name="storeId"></param>
        /// <param name="products"></param>
        /// <returns></returns>
        private async Task ApplyMarkupOnProductPrice(int vendorId, int storeId, IList<Product> products)
        {
            ////get all products of current vendor
            //var products = await _productService.SearchProductsAsync(vendorId: vendorId,
            //    overridePublished: true, storeId: storeId, visibleIndividuallyOnly: true);

            //apply markUp on impoert processed products only. - 08-02-23
            foreach (var item in products)
            {
                var productCategories = await _categoryService.GetProductCategoriesByProductIdAsync(item.Id);
                var categoryIds = new List<int>();
                foreach (var productCategory in productCategories)
                {
                    categoryIds.Add(productCategory.CategoryId);
                }

                if (!categoryIds.Any())
                    continue;
                //get category markup by categoryIds and vendor id
                var categoryMarkup = await _categoryMarkupService.GetCategoryMarkupAsync(categoryIds, vendorId);
                if (categoryMarkup == null)
                    continue;

                var calPrice = Math.Round(item.Price * categoryMarkup.Markup, 2, MidpointRounding.AwayFromZero) / 100;
                item.Price += calPrice;
                //second decimal place should always be 9.
                var price = string.Format("{0:0.0}", item.Price - (item.Price % 0.1M));
                if (Convert.ToDecimal(price) > 0)
                    item.Price = Convert.ToDecimal(price + "9");
                await _productService.UpdateProductAsync(item);
            }
        }

        /// <summary>
        /// Sends email for vendor products failed to import
        /// </summary>
        /// <param name="invalidProducts"></param>
        /// <param name="vendor"></param>
        /// <returns></returns>
        private async Task SendEmailForInvalidProduct(List<ImportFailedProduct> invalidProducts, Vendor vendor)
        {
            if (invalidProducts == null || vendor == null)
                return;

            var languageId = (await _workContext.GetWorkingLanguageAsync()).Id;
            //send invalid product email
            if (invalidProducts.Any())
            {
                var sb = new StringBuilder();
                var body = await UPCCodeHtmlTable(sb, invalidProducts, languageId);

                //send email to super admin
                await _workflowMessageService.SendInvalidProductMessage(languageId, vendor, body);
                //send email to vendor
                await _workflowMessageService.SendInvalidProductMessageForVendor(languageId, vendor, body);
            }
        }

        /// <summary>
        /// Sends email for vendor products has duplicate sku
        /// </summary>
        /// <param name="invalidProducts"></param>
        /// <param name="vendor"></param>
        /// <returns></returns>
        private async Task SendEmailForDuplicateSkuProduct(List<ImportFailedProduct> invalidProducts, Vendor vendor)
        {
            if (invalidProducts == null || vendor == null)
                return;

            var languageId = (await _workContext.GetWorkingLanguageAsync()).Id;

            if (invalidProducts.Any())
            {
                var sb = new StringBuilder();
                var body = await UPCCodeHtmlTable(sb, invalidProducts, languageId);

                //send email to vendor
                await _workflowMessageService.SendDuplicateProductSkuMessageForVendor(languageId, vendor, body);
            }
        }

        /// <summary>
        /// Sends email for master products failed to import
        /// </summary>
        /// <param name="invalidProducts"></param>
        /// <returns></returns>
        private async Task SendEmailForInvalidProduct(List<ImportFailedProduct> invalidProducts)
        {
            if (!invalidProducts.Any())
                return;

            var languageId = (await _workContext.GetWorkingLanguageAsync()).Id;
            var sb = new StringBuilder();
            var body = await ImportsFailedHtmlTable(sb, invalidProducts, languageId);

            //send email to super admin
            await _workflowMessageService.SendInvalidProductMessage(languageId, body);
        }

        /// <returns>A task that represents the asynchronous operation</returns>
        private async Task SaveGroupProductPictureMappingsAsync(Product groupedProduct, Product associatedProduct)
        {
            if (groupedProduct == null)
                return;

            if (associatedProduct == null)
                return;

            //get existing associated product pictures
            var existingPictures = await _pictureService.GetPicturesByProductIdAsync(associatedProduct.Id);
            if (existingPictures != null && existingPictures.Any())
            {
                try
                {
                    var existingAssociatedProductPicture = existingPictures.FirstOrDefault();
                    var existingAssociatedProductBinary = await _pictureService.LoadPictureBinaryAsync(existingAssociatedProductPicture);
                    var pictureAlreadyExists = false;

                    //get existing group product picture
                    var existingGroupPictures = await _pictureService.GetPicturesByProductIdAsync(groupedProduct.Id);
                    foreach (var picture in existingGroupPictures)
                    {
                        var existingBinary = await _pictureService.LoadPictureBinaryAsync(picture);
                        //picture binary after validation (like in database)
                        var validatedPictureBinary = await _pictureService.ValidatePictureAsync(existingAssociatedProductBinary, existingAssociatedProductPicture.MimeType);
                        if (!existingBinary.SequenceEqual(validatedPictureBinary) &&
                            !existingBinary.SequenceEqual(existingAssociatedProductBinary))
                        {
                            //delete group product image.
                            await _pictureService.DeletePictureAsync(picture);
                            continue;
                        }
                        //the same picture content
                        pictureAlreadyExists = true;
                    }

                    if (pictureAlreadyExists)
                        return;

                    var newPicture = await _pictureService.InsertPictureAsync(existingAssociatedProductBinary, existingAssociatedProductPicture.MimeType, await _pictureService.GetPictureSeNameAsync(groupedProduct.Name));
                    await _productService.InsertProductPictureAsync(new ProductPicture
                    {
                        //EF has some weird issue if we set "Picture = newPicture" instead of "PictureId = newPicture.Id"
                        //pictures are duplicated
                        //maybe because entity size is too large
                        PictureId = newPicture.Id,
                        DisplayOrder = 1,
                        ProductId = groupedProduct.Id
                    });

                    await _productService.UpdateProductAsync(groupedProduct);
                }
                catch (Exception ex)
                {
                    //await LogPictureInsertErrorAsync(picturePath, ex);
                }
            }
        }

        /// <summary>
        /// Copy category mapping
        /// </summary>
        /// <param name="groupedProduct">Product</param>
        /// <param name="associatedProduct">New product</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        protected virtual async Task SaveGroupProductCategoryMappingsAsync(Product groupedProduct, Product associatedProduct)
        {
            var groupedProductCategories = await _categoryService.GetProductCategoriesByProductIdAsync(groupedProduct.Id, showHidden: true);
            var associatedProductCategories = await _categoryService.GetProductCategoriesByProductIdAsync(associatedProduct.Id, showHidden: true);

            //delete categories
            foreach (var existingGroupedProductCategory in groupedProductCategories)
                if (!associatedProductCategories.Select(x => x.CategoryId).Contains(existingGroupedProductCategory.CategoryId))
                    await _categoryService.DeleteProductCategoryAsync(existingGroupedProductCategory);

            //add categories
            foreach (var categoryId in associatedProductCategories.Select(x => x.CategoryId))
            {
                if (_categoryService.FindProductCategory(groupedProductCategories, groupedProduct.Id, categoryId) == null)
                {
                    //find next display order
                    var displayOrder = associatedProductCategories.FirstOrDefault(x => x.CategoryId == categoryId)?.DisplayOrder ?? 0;
                    await _categoryService.InsertProductCategoryAsync(new ProductCategory
                    {
                        ProductId = groupedProduct.Id,
                        CategoryId = categoryId,
                        DisplayOrder = displayOrder
                    });
                }
            }
        }

        /// <summary>
        /// Copy manufacturer mapping
        /// </summary>
        /// <param name="groupedProduct">Product</param>
        /// <param name="associatedProduct">New product</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        protected virtual async Task SaveGroupProductManufacturerMappingsAsync(Product groupedProduct, Product associatedProduct)
        {
            var groupedProductManufacturers = await _manufacturerService.GetProductManufacturersByProductIdAsync(groupedProduct.Id, true);
            var associatedProductManufacturers = await _manufacturerService.GetProductManufacturersByProductIdAsync(associatedProduct.Id, true);

            //delete manufacturers
            foreach (var groupedProductManufacturer in groupedProductManufacturers)
                if (!associatedProductManufacturers.Select(x => x.ManufacturerId).Contains(groupedProductManufacturer.ManufacturerId))
                    await _manufacturerService.DeleteProductManufacturerAsync(groupedProductManufacturer);

            //add manufacturers
            foreach (var manufacturerId in associatedProductManufacturers.Select(x => x.ManufacturerId))
            {
                if (_manufacturerService.FindProductManufacturer(groupedProductManufacturers, groupedProduct.Id, manufacturerId) == null)
                {
                    //find next display order
                    var displayOrder = associatedProductManufacturers.FirstOrDefault(x => x.ManufacturerId == manufacturerId)?.DisplayOrder ?? 0;
                    await _manufacturerService.InsertProductManufacturerAsync(new ProductManufacturer
                    {
                        ProductId = groupedProduct.Id,
                        ManufacturerId = manufacturerId,
                        DisplayOrder = displayOrder
                    });
                }
            }
        }

        /// <summary>
        /// Copy associated product data to group product.
        /// </summary>
        /// <param name="groupProduct"></param>
        /// <param name="associatedProductId"></param>
        /// <returns></returns>
        protected virtual async Task CopyAssociatedProductDataToGroupProduct(Product groupedProduct, int associatedProductId)
        {
            if (groupedProduct == null)
                return;

            //get associatedProduct 
            var associatedProduct = await _productService.GetProductByIdAsync(associatedProductId);
            if (associatedProduct == null)
                return;

            //set associated product picture to group product
            await SaveGroupProductPictureMappingsAsync(groupedProduct, associatedProduct);

            //product <-> categories mappings
            await SaveGroupProductCategoryMappingsAsync(groupedProduct, associatedProduct);
            //product <-> manufacturers mappings
            await SaveGroupProductManufacturerMappingsAsync(groupedProduct, associatedProduct);
        }

        #endregion

        #region Methods

        public virtual async Task VendorImportProductsFromXlsxAsync(Stream stream)
        {
            using var workbook = new XLWorkbook(stream);
            // get the first worksheet in the workbook
            var worksheet = workbook.Worksheets.FirstOrDefault();
            if (worksheet == null)
                throw new NopException("No worksheet found");

            var downloadedFiles = new List<string>();
            var storeId = (await _storeContext.GetCurrentStoreAsync()).Id;
            var metadata = await PrepareVendorImportedProductDataAsync(worksheet);

            //check for duplicate upc code/sku
            var skuWith11Digits = metadata?.AllSku?.Where(x => x?.Length == 11)?.ToList();
            var skuWithout11Digits = metadata?.AllSku?.Where(x => x?.Length != 12)?.ToList();

            //to optimize performace, lets load data in chunks to prevent SQL Timeouts.
            var masterProductsByUpc = new List<Product>();
            int chunkSize = 3000;
            var allSkuChunks = metadata?.AllSku?.Chunk(chunkSize)?.ToList() ?? new List<string[]>();
            foreach (var skuChunk in allSkuChunks)
            {
                //get upc produts using sku.
                masterProductsByUpc.AddRange((await _productService.GetMasterProductsByUPCCodeAsync(skuChunk, 0))?.ToList());
            }
            //getSkufromUpc = (await _productService.GetMasterProductsByUPCCodeAsync(metadata.AllSku.ToArray(), 0))?.Select(mp => mp.Sku)?.ToList();

            var getSkufromUpc = masterProductsByUpc?.Select(x => x.Sku);
            if (getSkufromUpc != null && getSkufromUpc.Any())
            {
                skuWith11Digits.AddRange(getSkufromUpc.Where(s => s?.Length == 11));
                skuWithout11Digits.AddRange(getSkufromUpc.Where(s => s?.Length != 11));
            }

            //get unique sku
            //var uniqueSku = metadata?.AllSku?.GroupBy(x => x)?.Where(g => g.Count() == 1)?.ToList();
            //metadata.AllSku = uniqueSku?.Select(x => x.Key)?.ToList();

            var currentVendor = await _workContext.GetCurrentVendorAsync();
            if (currentVendor == null)
                return;

            var allMasterProducts = new List<Product>();

            //get all master products by upcCode or sku
            //allMasterProducts.AddRange((await _productService.GetMasterProductsByUPCCodeAsync(metadata.AllSku.ToArray(), 0))?.ToList());
            if (masterProductsByUpc != null && masterProductsByUpc.Any())
                allMasterProducts.AddRange(masterProductsByUpc);

            //peformance optimization
            foreach (var skuChunk in allSkuChunks)
            {
                //get sku produts using sku.
                allMasterProducts.AddRange((await _productService.GetMasterProductsBySKUCodeAsync(skuChunk, 0))?.ToList());
            }
            //distinct
            allMasterProducts = allMasterProducts?.DistinctBy(p => p.Id)?.ToList();

            var invalidProducts = new List<ImportFailedProduct>();
            var duplicateProducts = new List<ImportFailedProduct>();
            var processedProducts = new List<Product>();
            for (var iRow = 2; iRow < metadata.EndRow; iRow++)
            {
                metadata.Manager.ReadFromXlsx(worksheet, iRow);

                //current row sku property value
                var sku = metadata.Manager.GetProperty("SKU").StringValue?.Trim();

                //handle duplicate sku, if sku is duplicate products shouldn't import
                if (metadata?.AllSku?.Count > 0)
                {
                    var currentRowSKU = sku;
                    //sku cell can have both sku and upccode value, so check if it is upc code, then find it's sku from masterproduct.
                    if (sku.StartsWith(NopAlchubDefaults.PRODUCT_SKU_PATTERN))
                        currentRowSKU = allMasterProducts.FirstOrDefault(p => p.UPCCode == sku)?.Sku;

                    if (!string.IsNullOrEmpty(currentRowSKU))
                    {
                        //no need to add again, if it is already in duplicate list                  
                        if (duplicateProducts.Any())
                        {
                            var isAlreadyExists = false;

                            //if current row upc is 11 digit, match first 11 digit of master product upc code. 
                            if (currentRowSKU.Length == NopAlchubDefaults.PRODUCT_UPC_11_DIGIT)
                                isAlreadyExists = duplicateProducts.Any(p => p.Sku.Substring(0, Math.Min(p.Sku.Length, NopAlchubDefaults.PRODUCT_UPC_11_DIGIT)) == currentRowSKU);
                            else if (currentRowSKU.Length > NopAlchubDefaults.PRODUCT_UPC_11_DIGIT)
                                isAlreadyExists = duplicateProducts.Any(p => p.Sku.Substring(0, Math.Min(p.Sku.Length, NopAlchubDefaults.PRODUCT_UPC_11_DIGIT)) ==
                                currentRowSKU.Substring(0, Math.Min(p.Sku.Length, NopAlchubDefaults.PRODUCT_UPC_11_DIGIT)));
                            else
                                isAlreadyExists = duplicateProducts.Select(dp => dp.Sku).Contains(currentRowSKU);

                            if (isAlreadyExists)
                                continue;
                        }

                        var skuCount = 0;
                        if (currentRowSKU.Length == NopAlchubDefaults.PRODUCT_UPC_11_DIGIT)
                        {
                            //get count of sku matchs with current row sku.
                            skuCount = skuWith11Digits.Where(s => s.Substring(0, Math.Min(s.Length, NopAlchubDefaults.PRODUCT_UPC_11_DIGIT)) == currentRowSKU).Count();

                            //try to get from 12 digits sku list, if from any sku 11 digits matchs with current row sku.
                            if (skuWithout11Digits.Any())
                                skuCount += skuWithout11Digits.Where(s => s.Substring(0, Math.Min(s.Length, NopAlchubDefaults.PRODUCT_UPC_11_DIGIT)) ==
                                currentRowSKU.Substring(0, Math.Min(s.Length, NopAlchubDefaults.PRODUCT_UPC_11_DIGIT))).Count();
                        }
                        else
                        {
                            //get count of sku matchs with current row sku.
                            skuCount = metadata.AllSku.Where(s => s.Contains(currentRowSKU)).Count();

                            //try to get if from any sku 11 digits matchs with current row sku 11 digits.
                            if (skuWith11Digits.Any() && currentRowSKU.Length > NopAlchubDefaults.PRODUCT_UPC_11_DIGIT)
                                skuCount += skuWith11Digits.Where(s => s.Substring(0, Math.Min(s.Length, NopAlchubDefaults.PRODUCT_UPC_11_DIGIT)) ==
                                currentRowSKU.Substring(0, Math.Min(s.Length, NopAlchubDefaults.PRODUCT_UPC_11_DIGIT))).Count();
                        }

                        if (skuCount > 1)
                        {
                            var duplicateProduct = new ImportFailedProduct
                            {
                                Sku = currentRowSKU,
                                Name = metadata.Manager.GetProperty("Name")?.StringValue,
                                Size = metadata.Manager.GetProperty("Size")?.StringValue,
                                Price = metadata.Manager.GetProperty("Price")?.DecimalValue,
                                Stock = metadata.Manager.GetProperty("Stock")?.IntValueNullable,
                            };
                            duplicateProducts.Add(duplicateProduct);
                            continue;
                        }
                    }
                }

                //sku column can contains both sku/upccode, so finding master products with both.
                Product product = null;
                if (metadata.SkuCellNum > 0 && product == null || product?.Id == 0)
                {
                    //if current row upc is 11 digit, match first 11 digit of master product upc code. 
                    if (sku.Length == NopAlchubDefaults.PRODUCT_UPC_11_DIGIT)
                        product = allMasterProducts.FirstOrDefault(p => p.Sku.Substring(0, Math.Min(p.Sku.Length, NopAlchubDefaults.PRODUCT_UPC_11_DIGIT)) == sku);
                    else
                        product = allMasterProducts.FirstOrDefault(p => p.Sku == sku);
                }
                if (metadata.SkuCellNum > 0 && product == null || product?.Id == 0)
                    product = allMasterProducts.FirstOrDefault(p => p.UPCCode == metadata.Manager.GetProperty("SKU").StringValue.Trim());

                //check with friendly upc - 03/07/2023
                if (product == null)
                    product = await _productFriendlyUpcService.GetMasterProductByFriendlyUPCCodeAsync(currentVendor.Id, sku);

                if (product == null || product?.Id == 0)
                {
                    var importFailedProduct = new ImportFailedProduct()
                    {
                        Name = metadata.Manager.GetProperty("Name")?.StringValue,
                        Size = metadata.Manager.GetProperty("Size")?.StringValue,
                        Price = metadata.Manager.GetProperty("Price")?.DecimalValue,
                        Stock = metadata.Manager.GetProperty("Stock")?.IntValueNullable,
                        Sku = metadata.Manager.GetProperty("SKU")?.StringValue?.Trim(),
                    };

                    invalidProducts.Add(importFailedProduct);
                    continue;
                }

                foreach (var property in metadata.Manager.GetProperties)
                {
                    switch (property.PropertyName)
                    {
                        case "Stock":
                            var stock = property.IntValue < 0 ? 0 : property.IntValue;
                            product.StockQuantity = stock;
                            break;
                        case "Price":
                            product.Price = property.DecimalValue < 0 ? decimal.Zero : property.DecimalValue;
                            break;
                        case "Size":
                            product.Size = string.IsNullOrEmpty(property.StringValue) ? product.Size : property.StringValue;
                            break;
                    }
                }

                //no need to add product with 0 price.
                if (product.Price == 0)
                    continue;

                //check if product exists for vendor then update existing product, otherwise create a copy product.
                var existingProduct = await _productService.GetVendorProduct(vendorId: currentVendor.Id, storeId: storeId, upcCode: product.UPCCode);
                if (existingProduct != null)
                {
                    existingProduct.Price = product.Price;
                    existingProduct.StockQuantity = product.StockQuantity;
                    existingProduct.Size = product.Size;
                    existingProduct.UpdatedOnUtc = DateTime.UtcNow;
                    //associated product handle
                    existingProduct.ParentGroupedProductId = 0;
                    existingProduct.IsMaster = false;

                    await _productService.UpdateProductAsync(existingProduct);
                    processedProducts.Add(existingProduct);
                }
                else
                {
                    var exProduct = product;
                    var newProduct = await _copyProductService.CopyProductAsync(product, product.Name, true, true, vendorId: currentVendor.Id);

                    //update some values for new product which was not copied
                    newProduct.Sku = exProduct.Sku;
                    newProduct.UPCCode = exProduct.UPCCode;
                    newProduct.Size = exProduct.Size;
                    //default config flag
                    newProduct.OverridePrice = true;
                    newProduct.OverrideStock = true;
                    newProduct.IsMaster = false;
                    //associated product handle
                    newProduct.ParentGroupedProductId = 0;
                    await _productService.UpdateProductAsync(newProduct);
                    processedProducts.Add(newProduct);
                }
            }

            //making sure distinct products
            processedProducts = processedProducts.DistinctBy(p => p.Id).ToList();

            //apply markup on product price for current vendor
            await ApplyMarkupOnProductPrice(currentVendor.Id, storeId, processedProducts);

            //send email to super admin for invalid products
            await SendEmailForInvalidProduct(invalidProducts, currentVendor);

            //send email for duplicate sku
            await SendEmailForDuplicateSkuProduct(duplicateProducts, currentVendor);

            //activity log
            await _customerActivityService.InsertActivityAsync("ImportProducts", string.Format(await _localizationService.GetResourceAsync("ActivityLog.Vendor.ImportProducts"), metadata.CountProductsInFile));
        }

        #endregion

        #region Methods 

        protected virtual async Task ImportMasterProductImagesUsingHashAsync(IList<ProductPictureMetadata> productPictureMetadata, IList<Product> allProductsBySku)
        {
            //performance optimization, load all pictures hashes
            //it will only be used if the images are stored in the SQL Server database (not compact)
            var trimByteCount = _dataProvider.SupportedLengthOfBinaryHash - 1;
            var productsImagesIds = await _productService.GetProductsImagesIdsAsync(allProductsBySku.Select(p => p.Id).ToArray());

            var allProductPictureIds = productsImagesIds.SelectMany(p => p.Value);
            var allPicturesHashes = allProductPictureIds.Any() ? await _dataProvider.GetFieldHashesAsync<PictureBinary>(p => allProductPictureIds.Contains(p.PictureId),
                p => p.PictureId, p => p.BinaryData) : new Dictionary<int, string>();

            foreach (var product in productPictureMetadata)
            {
                //delete existing product picture if exists
                var existingProductPicture = await _productService.GetProductPicturesByProductIdAsync(product.ProductItem.Id);
                foreach (var productPicture in existingProductPicture)
                {
                    await _productService.DeleteProductPictureAsync(productPicture);
                }

                foreach (var picturePath in new[] { product.Picture1Path, product.Picture2Path, product.Picture3Path })
                {
                    if (string.IsNullOrEmpty(picturePath))
                        continue;
                    try
                    {
                        var mimeType = GetMimeTypeFromFilePath(picturePath);
                        var newPictureBinary = await _fileProvider.ReadAllBytesAsync(picturePath);
                        var pictureAlreadyExists = false;
                        if (!product.IsNew)
                        {
                            var newImageHash = HashHelper.CreateHash(
                                newPictureBinary,
                                ExportImportDefaults.ImageHashAlgorithm,
                                trimByteCount);

                            var newValidatedImageHash = HashHelper.CreateHash(
                                await _pictureService.ValidatePictureAsync(newPictureBinary, mimeType),
                                ExportImportDefaults.ImageHashAlgorithm,
                                trimByteCount);

                            var imagesIds = productsImagesIds.ContainsKey(product.ProductItem.Id)
                                ? productsImagesIds[product.ProductItem.Id]
                                : Array.Empty<int>();

                            pictureAlreadyExists = allPicturesHashes.Where(p => imagesIds.Contains(p.Key))
                                .Select(p => p.Value)
                                .Any(p =>
                                    p.Equals(newImageHash, StringComparison.OrdinalIgnoreCase) ||
                                    p.Equals(newValidatedImageHash, StringComparison.OrdinalIgnoreCase));
                        }

                        //if (pictureAlreadyExists)
                        //    continue;

                        var newPicture = await _pictureService.InsertPictureAsync(newPictureBinary, mimeType, await _pictureService.GetPictureSeNameAsync(product.ProductItem.Name), null, product.ProductItem.Name);

                        await _productService.InsertProductPictureAsync(new ProductPicture
                        {
                            //EF has some weird issue if we set "Picture = newPicture" instead of "PictureId = newPicture.Id"
                            //pictures are duplicated
                            //maybe because entity size is too large
                            PictureId = newPicture.Id,
                            DisplayOrder = 1,
                            ProductId = product.ProductItem.Id
                        });

                        await _productService.UpdateProductAsync(product.ProductItem);
                    }
                    catch (Exception ex)
                    {
                        await LogPictureInsertErrorAsync(picturePath, ex);
                    }
                }
            }
        }


        /// <returns>A task that represents the asynchronous operation</returns>
        private async Task ImportMasterProductsFromSplitedXlsxAsync(IXLWorksheet worksheet, ImportProductMetadata metadata)
        {
            foreach (var path in SplitProductFile(worksheet, metadata))
            {
                using var scope = _serviceScopeFactory.CreateScope();
                // Resolve
                var importManager = EngineContext.Current.Resolve<IImportManager>(scope);

                using var sr = new StreamReader(path);
                await importManager.ImportMasterProductsFromXlsxAsync(sr.BaseStream);

                try
                {
                    _fileProvider.DeleteFile(path);
                }
                catch
                {
                    // ignored
                }
            }
        }

        private async Task ImportAttributeAsync(PropertyManager<ImportSpecificationAttribute> specificationAttributeManager, Product lastLoadedProduct)
        {
            if (!_catalogSettings.ExportImportProductSpecificationAttributes || lastLoadedProduct == null)
                return;

            //prepare list of allowed attribute options. specified attribute option will be created if it is already not exist in system. 
            var allowedCreateNewAttributeOptions = new List<string>()
            {
                ExportImportDefaults.ATT_COLOR,
                ExportImportDefaults.ATT_TYPE,
                ExportImportDefaults.ATT_COUNTRY_OF_ORIGIN,
                ExportImportDefaults.ATT_STATE_OF_ORIGIN,
                ExportImportDefaults.ATT_SUBTYPE,
                ExportImportDefaults.ATT_REGION,
                ExportImportDefaults.ATT_ABV,
                ExportImportDefaults.ATT_FLAVOR,
                ExportImportDefaults.ATT_VINTAGE,
                ExportImportDefaults.ATT_ALCOHOL_PROOF,
                ExportImportDefaults.ATT_SPECIALTY,
                ExportImportDefaults.ATT_RATINGS,
                ExportImportDefaults.ATT_FOOD_PAIRING,
                ExportImportDefaults.ATT_BODY,
                ExportImportDefaults.ATT_TASTING_NOTES,
                ExportImportDefaults.ATT_CONTAINER,
                ExportImportDefaults.ATT_BASE_UNIT_CLOSURE,
                ExportImportDefaults.ATT_APPELLATION,
                ExportImportDefaults.ATT_BRAND_DESCRIPTION,
                ExportImportDefaults.ATT_SIZE,
                ExportImportDefaults.ATT_VARIANT
            };

            //prepare list of allowed filter attribute options. 
            var allowedFilterAttributeOptions = new List<string>()
            {
                ExportImportDefaults.ATT_SIZE,
                ExportImportDefaults.ATT_TYPE,
                ExportImportDefaults.ATT_COUNTRY_OF_ORIGIN,
                ExportImportDefaults.ATT_STATE_OF_ORIGIN,
                ExportImportDefaults.ATT_REGION,
                ExportImportDefaults.ATT_SUBTYPE,
                ExportImportDefaults.ATT_APPELLATION,
                ExportImportDefaults.ATT_FLAVOR,
                ExportImportDefaults.ATT_ABV,
                ExportImportDefaults.ATT_ALCOHOL_PROOF,
                ExportImportDefaults.ATT_VINTAGE,
                ExportImportDefaults.ATT_SPECIALTY,
                ExportImportDefaults.ATT_COLOR,
                ExportImportDefaults.ATT_RATINGS,
                ExportImportDefaults.ATT_BODY,
                ExportImportDefaults.ATT_CONTAINER,
                ExportImportDefaults.ATT_VARIANT
            };

            foreach (var property in specificationAttributeManager.GetProperties)
            {
                var specificationAttribute = await _specificationAttributeService.GetSpecificationAttributeByNameAsync(property.PropertyName?.Trim());

                if (specificationAttribute == null)
                    continue;

                if (string.IsNullOrEmpty(property.StringValue))
                {
                    //if this attribute is mapped with product, then delete
                    var existingProductSpecificationAttribute = (await _specificationAttributeService.GetProductSpecificationAttributesByProductAndAtributeAsync(lastLoadedProduct.Id, specificationAttribute.Id))?.FirstOrDefault();
                    if (existingProductSpecificationAttribute != null)
                        await _specificationAttributeService.DeleteProductSpecificationAttributeAsync(existingProductSpecificationAttribute);

                    continue;
                }

                var specificationAttributeOption = await _specificationAttributeService.GetSpecificationAttributeOptionsByNameAsync(property.StringValue?.Trim(),
                   specificationAttribute.Id);

                //validate allow create new & map it with current attribute.
                if (specificationAttributeOption == null && !allowedCreateNewAttributeOptions.Select(x => x.ToLowerInvariant()).Contains(specificationAttribute.Name.ToLowerInvariant()))
                    continue;

                if (specificationAttributeOption == null)
                {
                    specificationAttributeOption = new SpecificationAttributeOption
                    {
                        SpecificationAttributeId = specificationAttribute.Id,
                        ColorSquaresRgb = null,
                        Name = property.StringValue.Trim(),
                        DisplayOrder = 0
                    };
                    await _specificationAttributeService.InsertSpecificationAttributeOptionAsync(specificationAttributeOption);
                }

                //var productSpecificationAttribute = (await _specificationAttributeService.GetProductSpecificationAttributesAsync(lastLoadedProduct.Id, specificationAttributeOption.Id))?.FirstOrDefault();
                var productSpecificationAttribute = (await _specificationAttributeService.GetProductSpecificationAttributesByProductAndAtributeAsync(lastLoadedProduct.Id, specificationAttribute.Id))?.FirstOrDefault();
                var isNew = productSpecificationAttribute == null;

                if (isNew)
                    productSpecificationAttribute = new ProductSpecificationAttribute();

                switch (specificationAttribute.Name)
                {
                    case ExportImportDefaults.ATT_COLOR:
                        productSpecificationAttribute.AttributeTypeId = (int)SpecificationAttributeType.Option;
                        break;
                    case ExportImportDefaults.ATT_TYPE:
                        productSpecificationAttribute.AttributeTypeId = (int)SpecificationAttributeType.Option;
                        break;
                    case ExportImportDefaults.ATT_COUNTRY_OF_ORIGIN:
                        productSpecificationAttribute.AttributeTypeId = (int)SpecificationAttributeType.CustomText;
                        break;
                    case ExportImportDefaults.ATT_STATE_OF_ORIGIN:
                        productSpecificationAttribute.AttributeTypeId = (int)SpecificationAttributeType.CustomText;
                        break;
                    case ExportImportDefaults.ATT_SUBTYPE:
                        productSpecificationAttribute.AttributeTypeId = (int)SpecificationAttributeType.CustomText;
                        break;
                    case ExportImportDefaults.ATT_REGION:
                        productSpecificationAttribute.AttributeTypeId = (int)SpecificationAttributeType.CustomText;
                        break;
                    case ExportImportDefaults.ATT_ABV:
                        productSpecificationAttribute.AttributeTypeId = (int)SpecificationAttributeType.CustomText;
                        break;
                    case ExportImportDefaults.ATT_FLAVOR:
                        productSpecificationAttribute.AttributeTypeId = (int)SpecificationAttributeType.CustomText;
                        break;
                    case ExportImportDefaults.ATT_VINTAGE:
                        productSpecificationAttribute.AttributeTypeId = (int)SpecificationAttributeType.CustomText;
                        break;
                    case ExportImportDefaults.ATT_ALCOHOL_PROOF:
                        productSpecificationAttribute.AttributeTypeId = (int)SpecificationAttributeType.CustomText;
                        break;
                    case ExportImportDefaults.ATT_SPECIALTY:
                        productSpecificationAttribute.AttributeTypeId = (int)SpecificationAttributeType.CustomText;
                        break;
                    case ExportImportDefaults.ATT_RATINGS:
                        productSpecificationAttribute.AttributeTypeId = (int)SpecificationAttributeType.Option;
                        break;
                    case ExportImportDefaults.ATT_FOOD_PAIRING:
                        productSpecificationAttribute.AttributeTypeId = (int)SpecificationAttributeType.CustomText;
                        break;
                    case ExportImportDefaults.ATT_BODY:
                        productSpecificationAttribute.AttributeTypeId = (int)SpecificationAttributeType.CustomText;
                        break;
                    case ExportImportDefaults.ATT_TASTING_NOTES:
                        productSpecificationAttribute.AttributeTypeId = (int)SpecificationAttributeType.CustomText;
                        break;
                    case ExportImportDefaults.ATT_CONTAINER:
                        productSpecificationAttribute.AttributeTypeId = (int)SpecificationAttributeType.Option;
                        break;
                    case ExportImportDefaults.ATT_BASE_UNIT_CLOSURE:
                        productSpecificationAttribute.AttributeTypeId = (int)SpecificationAttributeType.CustomText;
                        break;
                    case ExportImportDefaults.ATT_APPELLATION:
                        productSpecificationAttribute.AttributeTypeId = (int)SpecificationAttributeType.CustomText;
                        break;
                    case ExportImportDefaults.ATT_BRAND_DESCRIPTION:
                        productSpecificationAttribute.AttributeTypeId = (int)SpecificationAttributeType.CustomText;
                        break;
                    case ExportImportDefaults.ATT_SIZE:
                        productSpecificationAttribute.AttributeTypeId = (int)SpecificationAttributeType.Option;
                        break;
                    case ExportImportDefaults.ATT_VARIANT:
                        productSpecificationAttribute.AttributeTypeId = (int)SpecificationAttributeType.Option;
                        break;
                }

                productSpecificationAttribute.SpecificationAttributeOptionId = specificationAttributeOption.Id;
                productSpecificationAttribute.ProductId = lastLoadedProduct.Id;
                productSpecificationAttribute.CustomValue = specificationAttributeOption.Name;
                productSpecificationAttribute.AllowFiltering = allowedFilterAttributeOptions.Select(x => x.ToLowerInvariant()).Contains(specificationAttribute.Name.ToLowerInvariant());
                productSpecificationAttribute.ShowOnProductPage = true;
                productSpecificationAttribute.DisplayOrder = 0;

                if (isNew)
                {
                    var specificationAttributeCount = await _specificationAttributeService.GetProductSpecificationAttributeCountAsync(lastLoadedProduct.Id, specificationAttributeOption.Id);
                    if (specificationAttributeCount == 0)
                    {
                        await _specificationAttributeService.InsertProductSpecificationAttributeAsync(productSpecificationAttribute);
                    }
                    else
                    {
                        await _specificationAttributeService.UpdateProductSpecificationAttributeAsync(productSpecificationAttribute);
                    }
                }
                else
                    await _specificationAttributeService.UpdateProductSpecificationAttributeAsync(productSpecificationAttribute);
            }
        }

        private async Task ImportSpecificationAttributeAsync(PropertyManager<ExportSpecificationAttribute> specificationAttributeManager, Product lastLoadedProduct)
        {
            if (!_catalogSettings.ExportImportProductSpecificationAttributes || lastLoadedProduct == null || specificationAttributeManager.IsCaption)
                return;

            var attributeTypeId = specificationAttributeManager.GetProperty("AttributeType").IntValue;
            var allowFiltering = specificationAttributeManager.GetProperty("AllowFiltering").BooleanValue;
            var specificationAttributeOptionId = specificationAttributeManager.GetProperty("SpecificationAttributeOptionId").IntValue;
            var productId = lastLoadedProduct.Id;
            var customValue = specificationAttributeManager.GetProperty("CustomValue").StringValue;
            var displayOrder = specificationAttributeManager.GetProperty("DisplayOrder").IntValue;
            var showOnProductPage = specificationAttributeManager.GetProperty("ShowOnProductPage").BooleanValue;

            //if specification attribute option isn't set, try to get first of possible specification attribute option for current specification attribute
            if (specificationAttributeOptionId == 0)
            {
                var specificationAttribute = specificationAttributeManager.GetProperty("SpecificationAttribute").IntValue;
                specificationAttributeOptionId =
                    (await _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttributeAsync(
                        specificationAttribute))
                    .FirstOrDefault()?.Id ?? specificationAttributeOptionId;
            }

            var productSpecificationAttribute = specificationAttributeOptionId == 0
                ? null
                : (await _specificationAttributeService.GetProductSpecificationAttributesAsync(productId, specificationAttributeOptionId)).FirstOrDefault();

            var isNew = productSpecificationAttribute == null;

            if (isNew)
                productSpecificationAttribute = new ProductSpecificationAttribute();

            if (attributeTypeId != (int)SpecificationAttributeType.Option)
                //we allow filtering only for "Option" attribute type
                allowFiltering = false;

            //we don't allow CustomValue for "Option" attribute type
            if (attributeTypeId == (int)SpecificationAttributeType.Option)
            {
                if (!string.IsNullOrEmpty(customValue))
                {
                    var specificationOption = await _specificationAttributeService.GetSpecificationAttributeOptionsByCustomValueAsync(customValue);
                    if (specificationOption == null)
                    {
                        return;
                    }
                    var productSpecificationAttributeOption = specificationAttributeOptionId == 0 ? null : (await _specificationAttributeService.GetProductSpecificationAttributesAsync(productId, specificationOption.Id)).FirstOrDefault();
                    var isNewOption = productSpecificationAttributeOption == null;
                    if (isNewOption)
                    {
                        customValue = specificationOption.Name;
                        specificationAttributeOptionId = specificationOption.Id;
                        isNew = true;
                    }
                    else
                    {
                        isNew = false;
                    }
                }
            }

            productSpecificationAttribute.AttributeTypeId = attributeTypeId;
            productSpecificationAttribute.SpecificationAttributeOptionId = specificationAttributeOptionId;
            productSpecificationAttribute.ProductId = productId;
            productSpecificationAttribute.CustomValue = customValue;
            productSpecificationAttribute.AllowFiltering = allowFiltering;
            productSpecificationAttribute.ShowOnProductPage = showOnProductPage;
            productSpecificationAttribute.DisplayOrder = displayOrder;

            if (isNew)
            {
                var specificationAttributeCount = await _specificationAttributeService.GetProductSpecificationAttributeCountAsync(productId, specificationAttributeOptionId);
                if (specificationAttributeCount == 0)
                {
                    await _specificationAttributeService.InsertProductSpecificationAttributeAsync(productSpecificationAttribute);
                }
                else
                {
                    await _specificationAttributeService.UpdateProductSpecificationAttributeAsync(productSpecificationAttribute);
                }
            }
            else
                await _specificationAttributeService.UpdateProductSpecificationAttributeAsync(productSpecificationAttribute);
        }

        private async Task<ImportProductMetadata> PrepareImportProductDataAsync(IXLWorksheet worksheet)
        {
            //the columns
            var properties = GetPropertiesByExcelCells<Product>(worksheet);

            var manager = new PropertyManager<Product>(properties, _catalogSettings);

            var productAttributeProperties = new[]
            {
                new PropertyByName<ExportProductAttribute>("AttributeId"),
                new PropertyByName<ExportProductAttribute>("AttributeName"),
                new PropertyByName<ExportProductAttribute>("DefaultValue"),
                new PropertyByName<ExportProductAttribute>("ValidationMinLength"),
                new PropertyByName<ExportProductAttribute>("ValidationMaxLength"),
                new PropertyByName<ExportProductAttribute>("ValidationFileAllowedExtensions"),
                new PropertyByName<ExportProductAttribute>("ValidationFileMaximumSize"),
                new PropertyByName<ExportProductAttribute>("AttributeTextPrompt"),
                new PropertyByName<ExportProductAttribute>("AttributeIsRequired"),
                new PropertyByName<ExportProductAttribute>("AttributeControlType"),
                new PropertyByName<ExportProductAttribute>("AttributeDisplayOrder"),
                new PropertyByName<ExportProductAttribute>("ProductAttributeValueId"),
                new PropertyByName<ExportProductAttribute>("ValueName"),
                new PropertyByName<ExportProductAttribute>("AttributeValueType"),
                new PropertyByName<ExportProductAttribute>("AssociatedProductId"),
                new PropertyByName<ExportProductAttribute>("ColorSquaresRgb"),
                new PropertyByName<ExportProductAttribute>("ImageSquaresPictureId"),
                new PropertyByName<ExportProductAttribute>("PriceAdjustment"),
                new PropertyByName<ExportProductAttribute>("PriceAdjustmentUsePercentage"),
                new PropertyByName<ExportProductAttribute>("WeightAdjustment"),
                new PropertyByName<ExportProductAttribute>("Cost"),
                new PropertyByName<ExportProductAttribute>("CustomerEntersQty"),
                new PropertyByName<ExportProductAttribute>("Quantity"),
                new PropertyByName<ExportProductAttribute>("IsPreSelected"),
                new PropertyByName<ExportProductAttribute>("DisplayOrder"),
                new PropertyByName<ExportProductAttribute>("PictureId")
            };

            var productAttributeManager = new PropertyManager<ExportProductAttribute>(productAttributeProperties, _catalogSettings);

            var specificationAttributeProperties = new[]
            {
                new PropertyByName<ExportSpecificationAttribute>("AttributeType", p => p.AttributeTypeId),
                new PropertyByName<ExportSpecificationAttribute>("SpecificationAttribute", p => p.SpecificationAttributeId),
                new PropertyByName<ExportSpecificationAttribute>("CustomValue", p => p.CustomValue),
                new PropertyByName<ExportSpecificationAttribute>("SpecificationAttributeOptionId", p => p.SpecificationAttributeOptionId),
                new PropertyByName<ExportSpecificationAttribute>("AllowFiltering", p => p.AllowFiltering),
                new PropertyByName<ExportSpecificationAttribute>("ShowOnProductPage", p => p.ShowOnProductPage),
                new PropertyByName<ExportSpecificationAttribute>("DisplayOrder", p => p.DisplayOrder)
            };

            var specificationAttributeManager = new PropertyManager<ExportSpecificationAttribute>(specificationAttributeProperties, _catalogSettings);

            var endRow = 2;
            var allCategories = new List<string>();
            var allSku = new List<string>();

            var tempProperty = manager.GetProperty("Categories");
            var categoryCellNum = tempProperty?.PropertyOrderPosition ?? -1;

            tempProperty = manager.GetProperty("SKU");
            var skuCellNum = tempProperty?.PropertyOrderPosition ?? -1;

            tempProperty = manager.GetProperty("UPCCode");
            var upcCodeCellNum = tempProperty?.PropertyOrderPosition ?? -1;

            tempProperty = manager.GetProperty("Size");
            var sizeCellNum = tempProperty?.PropertyOrderPosition ?? -1;

            tempProperty = manager.GetProperty("Container");
            var containerCellNum = tempProperty?.PropertyOrderPosition ?? -1;

            var allManufacturers = new List<string>();
            tempProperty = manager.GetProperty("Manufacturers");
            var manufacturerCellNum = tempProperty?.PropertyOrderPosition ?? -1;

            var allStores = new List<string>();
            tempProperty = manager.GetProperty("LimitedToStores");
            var limitedToStoresCellNum = tempProperty?.PropertyOrderPosition ?? -1;

            if (_catalogSettings.ExportImportUseDropdownlistsForAssociatedEntities)
            {
                productAttributeManager.SetSelectList("AttributeControlType", await AttributeControlType.TextBox.ToSelectListAsync(useLocalization: false));
                productAttributeManager.SetSelectList("AttributeValueType", await AttributeValueType.Simple.ToSelectListAsync(useLocalization: false));

                specificationAttributeManager.SetSelectList("AttributeType", await SpecificationAttributeType.Option.ToSelectListAsync(useLocalization: false));
                specificationAttributeManager.SetSelectList("SpecificationAttribute", (await _specificationAttributeService
                    .GetSpecificationAttributesAsync())
                    .Select(sa => sa as BaseEntity)
                    .ToSelectList(p => (p as SpecificationAttribute)?.Name ?? string.Empty));

                manager.SetSelectList("ProductType", await ProductType.SimpleProduct.ToSelectListAsync(useLocalization: false));
                manager.SetSelectList("GiftCardType", await GiftCardType.Virtual.ToSelectListAsync(useLocalization: false));
                manager.SetSelectList("DownloadActivationType",
                    await DownloadActivationType.Manually.ToSelectListAsync(useLocalization: false));
                manager.SetSelectList("ManageInventoryMethod",
                    await ManageInventoryMethod.DontManageStock.ToSelectListAsync(useLocalization: false));
                manager.SetSelectList("LowStockActivity",
                    await LowStockActivity.Nothing.ToSelectListAsync(useLocalization: false));
                manager.SetSelectList("BackorderMode", await BackorderMode.NoBackorders.ToSelectListAsync(useLocalization: false));
                manager.SetSelectList("RecurringCyclePeriod",
                    await RecurringProductCyclePeriod.Days.ToSelectListAsync(useLocalization: false));
                manager.SetSelectList("RentalPricePeriod", await RentalPricePeriod.Days.ToSelectListAsync(useLocalization: false));

                manager.SetSelectList("Vendor",
                    (await _vendorService.GetAllVendorsAsync(showHidden: true)).Select(v => v as BaseEntity)
                        .ToSelectList(p => (p as Vendor)?.Name ?? string.Empty));
                manager.SetSelectList("ProductTemplate",
                    (await _productTemplateService.GetAllProductTemplatesAsync()).Select(pt => pt as BaseEntity)
                        .ToSelectList(p => (p as ProductTemplate)?.Name ?? string.Empty));
                manager.SetSelectList("DeliveryDate",
                    (await _dateRangeService.GetAllDeliveryDatesAsync()).Select(dd => dd as BaseEntity)
                        .ToSelectList(p => (p as DeliveryDate)?.Name ?? string.Empty));
                manager.SetSelectList("ProductAvailabilityRange",
                    (await _dateRangeService.GetAllProductAvailabilityRangesAsync()).Select(range => range as BaseEntity)
                        .ToSelectList(p => (p as ProductAvailabilityRange)?.Name ?? string.Empty));
                manager.SetSelectList("TaxCategory",
                    (await _taxCategoryService.GetAllTaxCategoriesAsync()).Select(tc => tc as BaseEntity)
                        .ToSelectList(p => (p as TaxCategory)?.Name ?? string.Empty));
                manager.SetSelectList("BasepriceUnit",
                    (await _measureService.GetAllMeasureWeightsAsync()).Select(mw => mw as BaseEntity)
                        .ToSelectList(p => (p as MeasureWeight)?.Name ?? string.Empty));
                manager.SetSelectList("BasepriceBaseUnit",
                    (await _measureService.GetAllMeasureWeightsAsync()).Select(mw => mw as BaseEntity)
                        .ToSelectList(p => (p as MeasureWeight)?.Name ?? string.Empty));
            }

            var allAttributeIds = new List<int>();
            var allSpecificationAttributeOptionIds = new List<int>();

            var attributeIdCellNum = 1 + ExportProductAttribute.ProducAttributeCellOffset;
            var specificationAttributeOptionIdCellNum =
                specificationAttributeManager.GetIndex("SpecificationAttributeOptionId") +
                ExportProductAttribute.ProducAttributeCellOffset;

            var productsInFile = new List<int>();

            //find end of data
            var typeOfExportedAttribute = ExportedAttributeType.NotSpecified;
            while (true)
            {
                var allColumnsAreEmpty = manager.GetProperties
                    .Select(property => worksheet.Row(endRow).Cell(property.PropertyOrderPosition))
                    .All(cell => string.IsNullOrEmpty(cell?.Value?.ToString()));

                if (allColumnsAreEmpty)
                    break;

                if (new[] { 1, 2 }.Select(cellNum => worksheet.Row(endRow).Cell(cellNum))
                        .All(cell => string.IsNullOrEmpty(cell?.Value?.ToString())) &&
                    worksheet.Row(endRow).OutlineLevel == 0)
                {
                    var cellValue = worksheet.Row(endRow).Cell(attributeIdCellNum).Value;
                    await SetOutLineForProductAttributeRowAsync(cellValue, worksheet, endRow);
                    await SetOutLineForSpecificationAttributeRowAsync(cellValue, worksheet, endRow);
                }

                if (worksheet.Row(endRow).OutlineLevel != 0)
                {
                    var newTypeOfExportedAttribute = GetTypeOfExportedAttribute(worksheet, productAttributeManager, specificationAttributeManager, endRow);

                    //skip caption row
                    if (newTypeOfExportedAttribute != ExportedAttributeType.NotSpecified && newTypeOfExportedAttribute != typeOfExportedAttribute)
                    {
                        typeOfExportedAttribute = newTypeOfExportedAttribute;
                        endRow++;
                        continue;
                    }

                    switch (typeOfExportedAttribute)
                    {
                        case ExportedAttributeType.ProductAttribute:
                            productAttributeManager.ReadFromXlsx(worksheet, endRow,
                                ExportProductAttribute.ProducAttributeCellOffset);
                            if (int.TryParse((worksheet.Row(endRow).Cell(attributeIdCellNum).Value ?? string.Empty).ToString(), out var aid))
                            {
                                allAttributeIds.Add(aid);
                            }

                            break;
                        case ExportedAttributeType.SpecificationAttribute:
                            specificationAttributeManager.ReadFromXlsx(worksheet, endRow, ExportProductAttribute.ProducAttributeCellOffset);

                            if (int.TryParse((worksheet.Row(endRow).Cell(specificationAttributeOptionIdCellNum).Value ?? string.Empty).ToString(), out var saoid))
                            {
                                allSpecificationAttributeOptionIds.Add(saoid);
                            }

                            break;
                    }

                    endRow++;
                    continue;
                }

                if (categoryCellNum > 0)
                {
                    var categoryIds = worksheet.Row(endRow).Cell(categoryCellNum).Value?.ToString() ?? string.Empty;

                    if (!string.IsNullOrEmpty(categoryIds))
                        allCategories.AddRange(categoryIds
                            .Split(new[] { ";", ">>" }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim())
                            .Distinct());
                }

                if (skuCellNum > 0)
                {
                    var sku = worksheet.Row(endRow).Cell(skuCellNum).Value?.ToString() ?? string.Empty;

                    if (!string.IsNullOrEmpty(sku))
                        allSku.Add(sku);
                }

                if (manufacturerCellNum > 0)
                {
                    var manufacturerIds = worksheet.Row(endRow).Cell(manufacturerCellNum).Value?.ToString() ??
                                          string.Empty;
                    if (!string.IsNullOrEmpty(manufacturerIds))
                        allManufacturers.AddRange(manufacturerIds
                            .Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()));
                }

                if (limitedToStoresCellNum > 0)
                {
                    var storeIds = worksheet.Row(endRow).Cell(limitedToStoresCellNum).Value?.ToString() ??
                                          string.Empty;
                    if (!string.IsNullOrEmpty(storeIds))
                        allStores.AddRange(storeIds
                            .Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()));
                }

                //counting the number of products
                productsInFile.Add(endRow);

                endRow++;
            }

            //performance optimization, the check for the existence of the categories in one SQL request
            var notExistingCategories = await _categoryService.GetNotExistingCategoriesAsync(allCategories.ToArray());
            if (notExistingCategories.Any())
            {
                throw new ArgumentException(string.Format(await _localizationService.GetResourceAsync("Admin.Catalog.Products.Import.CategoriesDontExist"), string.Join(", ", notExistingCategories)));
            }

            //performance optimization, the check for the existence of the manufacturers in one SQL request
            var notExistingManufacturers = await _manufacturerService.GetNotExistingManufacturersAsync(allManufacturers.ToArray());
            if (notExistingManufacturers.Any())
            {
                throw new ArgumentException(string.Format(await _localizationService.GetResourceAsync("Admin.Catalog.Products.Import.ManufacturersDontExist"), string.Join(", ", notExistingManufacturers)));
            }

            //performance optimization, the check for the existence of the product attributes in one SQL request
            var notExistingProductAttributes = await _productAttributeService.GetNotExistingAttributesAsync(allAttributeIds.ToArray());
            if (notExistingProductAttributes.Any())
            {
                throw new ArgumentException(string.Format(await _localizationService.GetResourceAsync("Admin.Catalog.Products.Import.ProductAttributesDontExist"), string.Join(", ", notExistingProductAttributes)));
            }

            //performance optimization, the check for the existence of the specification attribute options in one SQL request
            var notExistingSpecificationAttributeOptions = await _specificationAttributeService.GetNotExistingSpecificationAttributeOptionsAsync(allSpecificationAttributeOptionIds.Where(saoId => saoId != 0).ToArray());
            if (notExistingSpecificationAttributeOptions.Any())
            {
                throw new ArgumentException($"The following specification attribute option ID(s) don't exist - {string.Join(", ", notExistingSpecificationAttributeOptions)}");
            }

            //performance optimization, the check for the existence of the stores in one SQL request
            var notExistingStores = await _storeService.GetNotExistingStoresAsync(allStores.ToArray());
            if (notExistingStores.Any())
            {
                throw new ArgumentException(string.Format(await _localizationService.GetResourceAsync("Admin.Catalog.Products.Import.StoresDontExist"), string.Join(", ", notExistingStores)));
            }

            return new ImportProductMetadata
            {
                EndRow = endRow,
                Manager = manager,
                Properties = properties,
                ProductsInFile = productsInFile,
                ProductAttributeManager = productAttributeManager,
                SpecificationAttributeManager = specificationAttributeManager,
                SkuCellNum = skuCellNum,
                AllSku = allSku
            };
        }

        private async Task<ImportProductMetadata> PrepareImportMasterProductDataAsync(IXLWorksheet worksheet)
        {
            //the columns
            var properties = GetPropertiesByExcelCells<Product>(worksheet);

            var manager = new PropertyManager<Product>(properties, _catalogSettings);

            var attributeProperties = new[]
            {
                new PropertyByName<ImportSpecificationAttribute>(ExportImportDefaults.ATT_COUNTRY_OF_ORIGIN, p => p.Att_CountryofOrigin),
                new PropertyByName<ImportSpecificationAttribute>(ExportImportDefaults.ATT_STATE_OF_ORIGIN, p => p.Att_StateofOrigin),
                new PropertyByName<ImportSpecificationAttribute>(ExportImportDefaults.ATT_REGION, p => p.Att_Region),
                new PropertyByName<ImportSpecificationAttribute>(ExportImportDefaults.ATT_SUBTYPE, p => p.Att_SubType),
                new PropertyByName<ImportSpecificationAttribute>(ExportImportDefaults.ATT_ABV, p => (p.Att_ABV)),
                new PropertyByName<ImportSpecificationAttribute>(ExportImportDefaults.ATT_VINTAGE, p => p.Att_Vintage),
                new PropertyByName<ImportSpecificationAttribute>(ExportImportDefaults.ATT_FLAVOR, p => p.Att_Flavor),
                new PropertyByName<ImportSpecificationAttribute>(ExportImportDefaults.ATT_ALCOHOL_PROOF, p => p.Att_AlcoholProof),
                new PropertyByName<ImportSpecificationAttribute>(ExportImportDefaults.ATT_TYPE, p => p.Att_Type),
                new PropertyByName<ImportSpecificationAttribute>(ExportImportDefaults.ATT_SPECIALTY, p => p.Att_Specialty),
                new PropertyByName<ImportSpecificationAttribute>(ExportImportDefaults.ATT_COLOR, p => p.Att_Color),
                new PropertyByName<ImportSpecificationAttribute>(ExportImportDefaults.ATT_RATINGS, p => p.Att_Ratings),
                new PropertyByName<ImportSpecificationAttribute>(ExportImportDefaults.ATT_APPELLATION, p => p.Att_Appellation),
                new PropertyByName<ImportSpecificationAttribute>(ExportImportDefaults.ATT_FOOD_PAIRING, p => p.Att_FoodPairing),
                new PropertyByName<ImportSpecificationAttribute>(ExportImportDefaults.ATT_BODY, p => p.Att_Body),
                new PropertyByName<ImportSpecificationAttribute>(ExportImportDefaults.ATT_TASTING_NOTES, p => p.Att_TastingNotes),
                new PropertyByName<ImportSpecificationAttribute>(ExportImportDefaults.ATT_CONTAINER, p => p.Att_Container),
                new PropertyByName<ImportSpecificationAttribute>(ExportImportDefaults.ATT_BASE_UNIT_CLOSURE, p => p.Att_BaseUnitClosure),
                new PropertyByName<ImportSpecificationAttribute>(ExportImportDefaults.ATT_BRAND_DESCRIPTION, p => p.Att_BrandDescription),
                new PropertyByName<ImportSpecificationAttribute>(ExportImportDefaults.ATT_SIZE, p => p.Att_Size),
                new PropertyByName<ImportSpecificationAttribute>(ExportImportDefaults.ATT_VARIANT, p => p.Att_Variant),
            };
            var attributeManager = new PropertyManager<ImportSpecificationAttribute>(attributeProperties, _catalogSettings);

            var endRow = 2;
            var allCategories = new List<string>();
            var allSku = new List<string>();

            var tempProperty = manager.GetProperty("Categories");
            var categoryCellNum = tempProperty?.PropertyOrderPosition ?? -1;

            tempProperty = manager.GetProperty("SKU");
            var skuCellNum = tempProperty?.PropertyOrderPosition ?? -1;

            tempProperty = manager.GetProperty("UPCCode");
            var upcCodeCellNum = tempProperty?.PropertyOrderPosition ?? -1;

            tempProperty = manager.GetProperty("Size");
            var sizeCellNum = tempProperty?.PropertyOrderPosition ?? -1;

            tempProperty = manager.GetProperty("Container");
            var containerCellNum = tempProperty?.PropertyOrderPosition ?? -1;

            var allManufacturers = new List<string>();
            tempProperty = manager.GetProperty("Manufacturers");
            var manufacturerCellNum = tempProperty?.PropertyOrderPosition ?? -1;

            var allStores = new List<string>();
            tempProperty = manager.GetProperty("LimitedToStores");
            var limitedToStoresCellNum = tempProperty?.PropertyOrderPosition ?? -1;

            if (_catalogSettings.ExportImportUseDropdownlistsForAssociatedEntities)
            {
                manager.SetSelectList("ProductType", await ProductType.SimpleProduct.ToSelectListAsync(useLocalization: false));
                manager.SetSelectList("GiftCardType", await GiftCardType.Virtual.ToSelectListAsync(useLocalization: false));
                manager.SetSelectList("DownloadActivationType",
                    await DownloadActivationType.Manually.ToSelectListAsync(useLocalization: false));
                manager.SetSelectList("ManageInventoryMethod",
                    await ManageInventoryMethod.DontManageStock.ToSelectListAsync(useLocalization: false));
                manager.SetSelectList("LowStockActivity",
                    await LowStockActivity.Nothing.ToSelectListAsync(useLocalization: false));
                manager.SetSelectList("BackorderMode", await BackorderMode.NoBackorders.ToSelectListAsync(useLocalization: false));
                manager.SetSelectList("RecurringCyclePeriod",
                    await RecurringProductCyclePeriod.Days.ToSelectListAsync(useLocalization: false));
                manager.SetSelectList("RentalPricePeriod", await RentalPricePeriod.Days.ToSelectListAsync(useLocalization: false));

                manager.SetSelectList("Vendor",
                    (await _vendorService.GetAllVendorsAsync(showHidden: true)).Select(v => v as BaseEntity)
                        .ToSelectList(p => (p as Vendor)?.Name ?? string.Empty));
                manager.SetSelectList("ProductTemplate",
                    (await _productTemplateService.GetAllProductTemplatesAsync()).Select(pt => pt as BaseEntity)
                        .ToSelectList(p => (p as ProductTemplate)?.Name ?? string.Empty));
                manager.SetSelectList("DeliveryDate",
                    (await _dateRangeService.GetAllDeliveryDatesAsync()).Select(dd => dd as BaseEntity)
                        .ToSelectList(p => (p as DeliveryDate)?.Name ?? string.Empty));
                manager.SetSelectList("ProductAvailabilityRange",
                    (await _dateRangeService.GetAllProductAvailabilityRangesAsync()).Select(range => range as BaseEntity)
                        .ToSelectList(p => (p as ProductAvailabilityRange)?.Name ?? string.Empty));
                manager.SetSelectList("TaxCategory",
                    (await _taxCategoryService.GetAllTaxCategoriesAsync()).Select(tc => tc as BaseEntity)
                        .ToSelectList(p => (p as TaxCategory)?.Name ?? string.Empty));
                manager.SetSelectList("BasepriceUnit",
                    (await _measureService.GetAllMeasureWeightsAsync()).Select(mw => mw as BaseEntity)
                        .ToSelectList(p => (p as MeasureWeight)?.Name ?? string.Empty));
                manager.SetSelectList("BasepriceBaseUnit",
                    (await _measureService.GetAllMeasureWeightsAsync()).Select(mw => mw as BaseEntity)
                        .ToSelectList(p => (p as MeasureWeight)?.Name ?? string.Empty));
            }

            var allAttributeIds = new List<int>();
            var allSpecificationAttributeOptionIds = new List<int>();
            var productsInFile = new List<int>();

            //find end of data
            while (true)
            {
                var allColumnsAreEmpty = manager.GetProperties
                    .Select(property => worksheet.Row(endRow).Cell(property.PropertyOrderPosition))
                    .All(cell => string.IsNullOrEmpty(cell?.Value?.ToString()));

                if (allColumnsAreEmpty)
                    break;

                if (categoryCellNum > 0)
                {
                    var categoryIds = worksheet.Row(endRow).Cell(categoryCellNum).Value?.ToString() ?? string.Empty;

                    if (!string.IsNullOrEmpty(categoryIds))
                        allCategories.AddRange(categoryIds
                            .Split(new[] { ";", ">>" }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim())
                            .Distinct());
                }

                if (skuCellNum > 0)
                {
                    var sku = worksheet.Row(endRow).Cell(skuCellNum).Value?.ToString() ?? string.Empty;

                    if (!string.IsNullOrEmpty(sku))
                        allSku.Add(sku.Trim());
                }

                if (manufacturerCellNum > 0)
                {
                    var manufacturerIds = worksheet.Row(endRow).Cell(manufacturerCellNum).Value?.ToString() ??
                                          string.Empty;
                    if (!string.IsNullOrEmpty(manufacturerIds))
                        allManufacturers.AddRange(manufacturerIds
                            .Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()));
                }

                if (limitedToStoresCellNum > 0)
                {
                    var storeIds = worksheet.Row(endRow).Cell(limitedToStoresCellNum).Value?.ToString() ??
                                          string.Empty;
                    if (!string.IsNullOrEmpty(storeIds))
                        allStores.AddRange(storeIds
                            .Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()));
                }

                //counting the number of products
                productsInFile.Add(endRow);

                endRow++;
            }

            //performance optimization, the check for the existence of the categories in one SQL request
            var notExistingCategories = await _categoryService.GetNotExistingCategoriesAsync(allCategories.ToArray());
            if (notExistingCategories.Any())
            {
                throw new ArgumentException(string.Format(await _localizationService.GetResourceAsync("Admin.Catalog.Products.Import.CategoriesDontExist"), string.Join(", ", notExistingCategories)));
            }

            //performance optimization, the check for the existence of the manufacturers in one SQL request
            var notExistingManufacturers = await _manufacturerService.GetNotExistingManufacturersAsync(allManufacturers.ToArray());
            if (notExistingManufacturers.Any())
            {
                throw new ArgumentException(string.Format(await _localizationService.GetResourceAsync("Admin.Catalog.Products.Import.ManufacturersDontExist"), string.Join(", ", notExistingManufacturers)));
            }

            //performance optimization, the check for the existence of the product attributes in one SQL request
            var notExistingProductAttributes = await _productAttributeService.GetNotExistingAttributesAsync(allAttributeIds.ToArray());
            if (notExistingProductAttributes.Any())
            {
                throw new ArgumentException(string.Format(await _localizationService.GetResourceAsync("Admin.Catalog.Products.Import.ProductAttributesDontExist"), string.Join(", ", notExistingProductAttributes)));
            }

            //performance optimization, the check for the existence of the specification attribute options in one SQL request
            var notExistingSpecificationAttributeOptions = await _specificationAttributeService.GetNotExistingSpecificationAttributeOptionsAsync(allSpecificationAttributeOptionIds.Where(saoId => saoId != 0).ToArray());
            if (notExistingSpecificationAttributeOptions.Any())
            {
                throw new ArgumentException($"The following specification attribute option ID(s) don't exist - {string.Join(", ", notExistingSpecificationAttributeOptions)}");
            }

            //performance optimization, the check for the existence of the stores in one SQL request
            var notExistingStores = await _storeService.GetNotExistingStoresAsync(allStores.ToArray());
            if (notExistingStores.Any())
            {
                throw new ArgumentException(string.Format(await _localizationService.GetResourceAsync("Admin.Catalog.Products.Import.StoresDontExist"), string.Join(", ", notExistingStores)));
            }

            return new ImportProductMetadata
            {
                EndRow = endRow,
                Manager = manager,
                Properties = properties,
                ProductsInFile = productsInFile,
                AttributeManager = attributeManager,
                SkuCellNum = skuCellNum,
                AllSku = allSku
            };
        }

        /// <summary>
        /// Import products from XLSX file
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task ImportProductsFromXlsxAsync(Stream stream)
        {
            using var workbook = new XLWorkbook(stream);
            // get the first worksheet in the workbook
            var worksheet = workbook.Worksheets.FirstOrDefault();
            if (worksheet == null)
                throw new NopException("No worksheet found");

            var downloadedFiles = new List<string>();

            var metadata = await PrepareImportProductDataAsync(worksheet);

            if (_catalogSettings.ExportImportSplitProductsFile && metadata.CountProductsInFile > _catalogSettings.ExportImportProductsCountInOneFile)
            {
                await ImportProductsFromSplitedXlsxAsync(worksheet, metadata);
                return;
            }

            //performance optimization, load all products by SKU in one SQL request
            var currentVendor = await _workContext.GetCurrentVendorAsync();
            var allProductsBySku = await _productService.GetProductsBySkuAsync(metadata.AllSku.ToArray(), currentVendor?.Id ?? 0);

            //validate maximum number of products per vendor
            if (_vendorSettings.MaximumProductNumber > 0 &&
                currentVendor != null)
            {
                var newProductsCount = metadata.CountProductsInFile - allProductsBySku.Count;
                if (await _productService.GetNumberOfProductsByVendorIdAsync(currentVendor.Id) + newProductsCount > _vendorSettings.MaximumProductNumber)
                    throw new ArgumentException(string.Format(await _localizationService.GetResourceAsync("Admin.Catalog.Products.ExceededMaximumNumber"), _vendorSettings.MaximumProductNumber));
            }

            //performance optimization, load all categories IDs for products in one SQL request
            var allProductsCategoryIds = await _categoryService.GetProductCategoryIdsAsync(allProductsBySku.Select(p => p.Id).ToArray());

            //performance optimization, load all categories in one SQL request
            Dictionary<CategoryKey, Category> allCategories;
            try
            {
                var allCategoryList = await _categoryService.GetAllCategoriesAsync(showHidden: true);

                allCategories = await allCategoryList
                    .ToDictionaryAwaitAsync(async c => await CategoryKey.CreateCategoryKeyAsync(c, _categoryService, allCategoryList, _storeMappingService), c => new ValueTask<Category>(c));
            }
            catch (ArgumentException)
            {
                //categories with the same name are not supported in the same category level
                throw new ArgumentException(await _localizationService.GetResourceAsync("Admin.Catalog.Products.Import.CategoriesWithSameNameNotSupported"));
            }

            //performance optimization, load all manufacturers IDs for products in one SQL request
            var allProductsManufacturerIds = await _manufacturerService.GetProductManufacturerIdsAsync(allProductsBySku.Select(p => p.Id).ToArray());

            //performance optimization, load all manufacturers in one SQL request
            var allManufacturers = await _manufacturerService.GetAllManufacturersAsync(showHidden: true);

            //performance optimization, load all stores in one SQL request
            var allStores = await _storeService.GetAllStoresAsync();

            //product to import images
            var productPictureMetadata = new List<ProductPictureMetadata>();

            Product lastLoadedProduct = null;
            var typeOfExportedAttribute = ExportedAttributeType.NotSpecified;

            for (var iRow = 2; iRow < metadata.EndRow; iRow++)
            {
                //imports product attributes
                if (worksheet.Row(iRow).OutlineLevel != 0)
                {
                    if (lastLoadedProduct == null)
                        continue;

                    var newTypeOfExportedAttribute = GetTypeOfExportedAttribute(worksheet, metadata.ProductAttributeManager, metadata.SpecificationAttributeManager, iRow);

                    //skip caption row
                    if (newTypeOfExportedAttribute != ExportedAttributeType.NotSpecified &&
                        newTypeOfExportedAttribute != typeOfExportedAttribute)
                    {
                        typeOfExportedAttribute = newTypeOfExportedAttribute;
                        continue;
                    }

                    switch (typeOfExportedAttribute)
                    {
                        case ExportedAttributeType.ProductAttribute:
                            await ImportProductAttributeAsync(metadata.ProductAttributeManager, lastLoadedProduct);
                            break;
                        case ExportedAttributeType.SpecificationAttribute:
                            await ImportSpecificationAttributeAsync(metadata.SpecificationAttributeManager, lastLoadedProduct);
                            break;
                        case ExportedAttributeType.NotSpecified:
                        default:
                            continue;
                    }

                    continue;
                }

                metadata.Manager.ReadFromXlsx(worksheet, iRow);

                var product = metadata.SkuCellNum > 0 ? allProductsBySku.FirstOrDefault(p => p.Sku == metadata.Manager.GetProperty("SKU").StringValue) : null;
                var isNew = product == null;

                product ??= new Product();

                //some of previous values
                var previousStockQuantity = product.StockQuantity;
                var previousWarehouseId = product.WarehouseId;

                if (isNew)
                    product.CreatedOnUtc = DateTime.UtcNow;


                foreach (var property in metadata.Manager.GetProperties)
                {
                    switch (property.PropertyName)
                    {
                        case "ProductType":
                            product.ProductTypeId = property.IntValue;
                            break;
                        case "ParentGroupedProductId":
                            product.ParentGroupedProductId = property.IntValue;
                            break;
                        case "VisibleIndividually":
                            product.VisibleIndividually = property.BooleanValue;
                            break;
                        case "Name":
                            product.Name = property.StringValue;
                            break;
                        case "ShortDescription":
                            product.ShortDescription = property.StringValue;
                            break;
                        case "FullDescription":
                            product.FullDescription = property.StringValue;
                            break;
                        case "Vendor":
                            //vendor can't change this field
                            if (currentVendor == null)
                                product.VendorId = property.IntValue;
                            break;
                        case "ProductTemplate":
                            product.ProductTemplateId = property.IntValue;
                            break;
                        case "ShowOnHomepage":
                            //vendor can't change this field
                            if (currentVendor == null)
                                product.ShowOnHomepage = property.BooleanValue;
                            break;
                        case "DisplayOrder":
                            //vendor can't change this field
                            if (currentVendor == null)
                                product.DisplayOrder = property.IntValue;
                            break;
                        case "MetaKeywords":
                            product.MetaKeywords = property.StringValue;
                            break;
                        case "MetaDescription":
                            product.MetaDescription = property.StringValue;
                            break;
                        case "MetaTitle":
                            product.MetaTitle = property.StringValue;
                            break;
                        case "AllowCustomerReviews":
                            product.AllowCustomerReviews = property.BooleanValue;
                            break;
                        case "Published":
                            product.Published = property.BooleanValue;
                            break;
                        case "SKU":
                            product.Sku = property.StringValue;
                            break;
                        case "UPCCode":
                            product.UPCCode = property.StringValue;
                            break;
                        case "Size":
                            product.Size = property.StringValue;
                            break;
                        case "Container":
                            product.Container = property.StringValue;
                            break;
                        case "ManufacturerPartNumber":
                            product.ManufacturerPartNumber = property.StringValue;
                            break;
                        case "Gtin":
                            product.Gtin = property.StringValue;
                            break;
                        case "IsGiftCard":
                            product.IsGiftCard = property.BooleanValue;
                            break;
                        case "GiftCardType":
                            product.GiftCardTypeId = property.IntValue;
                            break;
                        case "OverriddenGiftCardAmount":
                            product.OverriddenGiftCardAmount = property.DecimalValue;
                            break;
                        case "RequireOtherProducts":
                            product.RequireOtherProducts = property.BooleanValue;
                            break;
                        case "RequiredProductIds":
                            product.RequiredProductIds = property.StringValue;
                            break;
                        case "AutomaticallyAddRequiredProducts":
                            product.AutomaticallyAddRequiredProducts = property.BooleanValue;
                            break;
                        case "IsDownload":
                            product.IsDownload = property.BooleanValue;
                            break;
                        case "DownloadId":
                            product.DownloadId = property.IntValue;
                            break;
                        case "UnlimitedDownloads":
                            product.UnlimitedDownloads = property.BooleanValue;
                            break;
                        case "MaxNumberOfDownloads":
                            product.MaxNumberOfDownloads = property.IntValue;
                            break;
                        case "DownloadActivationType":
                            product.DownloadActivationTypeId = property.IntValue;
                            break;
                        case "HasSampleDownload":
                            product.HasSampleDownload = property.BooleanValue;
                            break;
                        case "SampleDownloadId":
                            product.SampleDownloadId = property.IntValue;
                            break;
                        case "HasUserAgreement":
                            product.HasUserAgreement = property.BooleanValue;
                            break;
                        case "UserAgreementText":
                            product.UserAgreementText = property.StringValue;
                            break;
                        case "IsRecurring":
                            product.IsRecurring = property.BooleanValue;
                            break;
                        case "RecurringCycleLength":
                            product.RecurringCycleLength = property.IntValue;
                            break;
                        case "RecurringCyclePeriod":
                            product.RecurringCyclePeriodId = property.IntValue;
                            break;
                        case "RecurringTotalCycles":
                            product.RecurringTotalCycles = property.IntValue;
                            break;
                        case "IsRental":
                            product.IsRental = property.BooleanValue;
                            break;
                        case "RentalPriceLength":
                            product.RentalPriceLength = property.IntValue;
                            break;
                        case "RentalPricePeriod":
                            product.RentalPricePeriodId = property.IntValue;
                            break;
                        case "IsShipEnabled":
                            product.IsShipEnabled = property.BooleanValue;
                            break;
                        case "IsFreeShipping":
                            product.IsFreeShipping = property.BooleanValue;
                            break;
                        case "ShipSeparately":
                            product.ShipSeparately = property.BooleanValue;
                            break;
                        case "AdditionalShippingCharge":
                            product.AdditionalShippingCharge = property.DecimalValue;
                            break;
                        case "DeliveryDate":
                            product.DeliveryDateId = property.IntValue;
                            break;
                        case "IsTaxExempt":
                            product.IsTaxExempt = property.BooleanValue;
                            break;
                        case "TaxCategory":
                            product.TaxCategoryId = property.IntValue;
                            break;
                        case "IsTelecommunicationsOrBroadcastingOrElectronicServices":
                            product.IsTelecommunicationsOrBroadcastingOrElectronicServices = property.BooleanValue;
                            break;
                        case "ManageInventoryMethod":
                            product.ManageInventoryMethodId = property.IntValue;
                            break;
                        case "ProductAvailabilityRange":
                            product.ProductAvailabilityRangeId = property.IntValue;
                            break;
                        case "UseMultipleWarehouses":
                            product.UseMultipleWarehouses = property.BooleanValue;
                            break;
                        case "WarehouseId":
                            product.WarehouseId = property.IntValue;
                            break;
                        case "StockQuantity":
                            product.StockQuantity = property.IntValue;
                            break;
                        case "DisplayStockAvailability":
                            product.DisplayStockAvailability = property.BooleanValue;
                            break;
                        case "DisplayStockQuantity":
                            product.DisplayStockQuantity = property.BooleanValue;
                            break;
                        case "MinStockQuantity":
                            product.MinStockQuantity = property.IntValue;
                            break;
                        case "LowStockActivity":
                            product.LowStockActivityId = property.IntValue;
                            break;
                        case "NotifyAdminForQuantityBelow":
                            product.NotifyAdminForQuantityBelow = property.IntValue;
                            break;
                        case "BackorderMode":
                            product.BackorderModeId = property.IntValue;
                            break;
                        case "AllowBackInStockSubscriptions":
                            product.AllowBackInStockSubscriptions = property.BooleanValue;
                            break;
                        case "OrderMinimumQuantity":
                            product.OrderMinimumQuantity = property.IntValue;
                            break;
                        case "OrderMaximumQuantity":
                            product.OrderMaximumQuantity = property.IntValue;
                            break;
                        case "AllowedQuantities":
                            product.AllowedQuantities = property.StringValue;
                            break;
                        case "AllowAddingOnlyExistingAttributeCombinations":
                            product.AllowAddingOnlyExistingAttributeCombinations = property.BooleanValue;
                            break;
                        case "NotReturnable":
                            product.NotReturnable = property.BooleanValue;
                            break;
                        case "DisableBuyButton":
                            product.DisableBuyButton = property.BooleanValue;
                            break;
                        case "DisableWishlistButton":
                            product.DisableWishlistButton = property.BooleanValue;
                            break;
                        case "AvailableForPreOrder":
                            product.AvailableForPreOrder = property.BooleanValue;
                            break;
                        case "PreOrderAvailabilityStartDateTimeUtc":
                            product.PreOrderAvailabilityStartDateTimeUtc = property.DateTimeNullable;
                            break;
                        case "CallForPrice":
                            product.CallForPrice = property.BooleanValue;
                            break;
                        case "Price":
                            product.Price = property.DecimalValue;
                            break;
                        case "OldPrice":
                            product.OldPrice = property.DecimalValue;
                            break;
                        case "ProductCost":
                            product.ProductCost = property.DecimalValue;
                            break;
                        case "CustomerEntersPrice":
                            product.CustomerEntersPrice = property.BooleanValue;
                            break;
                        case "MinimumCustomerEnteredPrice":
                            product.MinimumCustomerEnteredPrice = property.DecimalValue;
                            break;
                        case "MaximumCustomerEnteredPrice":
                            product.MaximumCustomerEnteredPrice = property.DecimalValue;
                            break;
                        case "BasepriceEnabled":
                            product.BasepriceEnabled = property.BooleanValue;
                            break;
                        case "BasepriceAmount":
                            product.BasepriceAmount = property.DecimalValue;
                            break;
                        case "BasepriceUnit":
                            product.BasepriceUnitId = property.IntValue;
                            break;
                        case "BasepriceBaseAmount":
                            product.BasepriceBaseAmount = property.DecimalValue;
                            break;
                        case "BasepriceBaseUnit":
                            product.BasepriceBaseUnitId = property.IntValue;
                            break;
                        case "MarkAsNew":
                            product.MarkAsNew = property.BooleanValue;
                            break;
                        case "MarkAsNewStartDateTimeUtc":
                            product.MarkAsNewStartDateTimeUtc = property.DateTimeNullable;
                            break;
                        case "MarkAsNewEndDateTimeUtc":
                            product.MarkAsNewEndDateTimeUtc = property.DateTimeNullable;
                            break;
                        case "Weight":
                            product.Weight = property.DecimalValue;
                            break;
                        case "Length":
                            product.Length = property.DecimalValue;
                            break;
                        case "Width":
                            product.Width = property.DecimalValue;
                            break;
                        case "Height":
                            product.Height = property.DecimalValue;
                            break;
                        case "IsLimitedToStores":
                            product.LimitedToStores = property.BooleanValue;
                            break;
                    }
                }

                //set some default values if not specified
                if (isNew && metadata.Properties.All(p => p.PropertyName != "ProductType"))
                    product.ProductType = ProductType.SimpleProduct;
                if (isNew && metadata.Properties.All(p => p.PropertyName != "VisibleIndividually"))
                    product.VisibleIndividually = true;
                if (isNew && metadata.Properties.All(p => p.PropertyName != "Published"))
                    product.Published = true;

                //sets the current vendor for the new product
                if (isNew && currentVendor != null)
                    product.VendorId = currentVendor.Id;

                product.UpdatedOnUtc = DateTime.UtcNow;
                product.IsMaster = true;

                if (isNew)
                    await _productService.InsertProductAsync(product);
                else
                    await _productService.UpdateProductAsync(product);

                //quantity change history
                if (isNew || previousWarehouseId == product.WarehouseId)
                {
                    await _productService.AddStockQuantityHistoryEntryAsync(product, product.StockQuantity - previousStockQuantity, product.StockQuantity,
                        product.WarehouseId, await _localizationService.GetResourceAsync("Admin.StockQuantityHistory.Messages.ImportProduct.Edit"));
                }
                //warehouse is changed 
                else
                {
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

                    var message = string.Format(await _localizationService.GetResourceAsync("Admin.StockQuantityHistory.Messages.ImportProduct.EditWarehouse"), oldWarehouseMessage, newWarehouseMessage);

                    //record history
                    await _productService.AddStockQuantityHistoryEntryAsync(product, -previousStockQuantity, 0, previousWarehouseId, message);
                    await _productService.AddStockQuantityHistoryEntryAsync(product, product.StockQuantity, product.StockQuantity, product.WarehouseId, message);
                }

                var tempProperty = metadata.Manager.GetProperty("SeName");

                //search engine name
                var seName = tempProperty?.StringValue ?? (isNew ? string.Empty : await _urlRecordService.GetSeNameAsync(product, 0));
                await _urlRecordService.SaveSlugAsync(product, await _urlRecordService.ValidateSeNameAsync(product, seName, product.Name, true), 0);

                tempProperty = metadata.Manager.GetProperty("Categories");

                if (tempProperty != null)
                {
                    var categoryList = tempProperty.StringValue;

                    //category mappings
                    var categories = isNew || !allProductsCategoryIds.ContainsKey(product.Id) ? Array.Empty<int>() : allProductsCategoryIds[product.Id];

                    var storesIds = product.LimitedToStores
                        ? (await _storeMappingService.GetStoresIdsWithAccessAsync(product)).ToList()
                        : new List<int>();

                    var importedCategories = await categoryList.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(categoryName => new CategoryKey(categoryName, storesIds))
                        .SelectAwait(async categoryKey =>
                        {
                            var rez = (allCategories.ContainsKey(categoryKey) ? allCategories[categoryKey].Id : allCategories.Values.FirstOrDefault(c => c.Name == categoryKey.Key)?.Id) ??
                                      allCategories.FirstOrDefault(p =>
                                    p.Key.Key.Equals(categoryKey.Key, StringComparison.InvariantCultureIgnoreCase))
                                .Value?.Id;

                            if (!rez.HasValue && int.TryParse(categoryKey.Key, out var id))
                                rez = id;

                            if (!rez.HasValue)
                                //database doesn't contain the imported category
                                throw new ArgumentException(string.Format(await _localizationService.GetResourceAsync("Admin.Catalog.Products.Import.DatabaseNotContainCategory"), categoryKey.Key));

                            return rez.Value;
                        }).ToListAsync();

                    foreach (var categoryId in importedCategories)
                    {
                        if (categories.Any(c => c == categoryId))
                            continue;

                        var productCategory = new ProductCategory
                        {
                            ProductId = product.Id,
                            CategoryId = categoryId,
                            IsFeaturedProduct = false,
                            DisplayOrder = 1
                        };
                        await _categoryService.InsertProductCategoryAsync(productCategory);
                    }

                    //delete product categories
                    var deletedProductCategories = await categories.Where(categoryId => !importedCategories.Contains(categoryId))
                        .SelectAwait(async categoryId => (await _categoryService.GetProductCategoriesByProductIdAsync(product.Id, true)).FirstOrDefault(pc => pc.CategoryId == categoryId)).Where(pc => pc != null).ToListAsync();

                    foreach (var deletedProductCategory in deletedProductCategories)
                        await _categoryService.DeleteProductCategoryAsync(deletedProductCategory);
                }

                tempProperty = metadata.Manager.GetProperty("Manufacturers");
                if (tempProperty != null)
                {
                    var manufacturerList = tempProperty.StringValue;

                    //manufacturer mappings
                    var manufacturers = isNew || !allProductsManufacturerIds.ContainsKey(product.Id) ? Array.Empty<int>() : allProductsManufacturerIds[product.Id];
                    var importedManufacturers = manufacturerList.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(x => allManufacturers.FirstOrDefault(m => m.Name == x.Trim())?.Id ?? int.Parse(x.Trim())).ToList();
                    foreach (var manufacturerId in importedManufacturers)
                    {
                        if (manufacturers.Any(c => c == manufacturerId))
                            continue;

                        var productManufacturer = new ProductManufacturer
                        {
                            ProductId = product.Id,
                            ManufacturerId = manufacturerId,
                            IsFeaturedProduct = false,
                            DisplayOrder = 1
                        };
                        await _manufacturerService.InsertProductManufacturerAsync(productManufacturer);
                    }

                    //delete product manufacturers
                    var deletedProductsManufacturers = await manufacturers.Where(manufacturerId => !importedManufacturers.Contains(manufacturerId))
                        .SelectAwait(async manufacturerId => (await _manufacturerService.GetProductManufacturersByProductIdAsync(product.Id)).First(pc => pc.ManufacturerId == manufacturerId)).ToListAsync();
                    foreach (var deletedProductManufacturer in deletedProductsManufacturers)
                        await _manufacturerService.DeleteProductManufacturerAsync(deletedProductManufacturer);
                }

                tempProperty = metadata.Manager.GetProperty("ProductTags");
                if (tempProperty != null)
                {
                    var productTags = tempProperty.StringValue.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToList();

                    //searching existing product tags by their id
                    var productTagIds = productTags.Where(pt => int.TryParse(pt, out var _)).Select(int.Parse);

                    var productTagsByIds = (await _productTagService.GetAllProductTagsByProductIdAsync(product.Id)).Where(pt => productTagIds.Contains(pt.Id)).ToList();

                    productTags.AddRange(productTagsByIds.Select(pt => pt.Name));
                    var filter = productTagsByIds.Select(pt => pt.Id.ToString()).ToList();

                    //product tag mappings
                    await _productTagService.UpdateProductTagsAsync(product, productTags.Where(pt => !filter.Contains(pt)).ToArray());
                }

                tempProperty = metadata.Manager.GetProperty("LimitedToStores");
                if (tempProperty != null)
                {
                    var limitedToStoresList = tempProperty.StringValue;

                    var importedStores = product.LimitedToStores ? limitedToStoresList.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(x => allStores.FirstOrDefault(store => store.Name == x.Trim())?.Id ?? int.Parse(x.Trim())).ToList() : new List<int>();

                    await _productService.UpdateProductStoreMappingsAsync(product, importedStores);
                }

                var picture1 = await DownloadFileAsync(metadata.Manager.GetProperty("Picture1")?.StringValue, downloadedFiles);
                var picture2 = await DownloadFileAsync(metadata.Manager.GetProperty("Picture2")?.StringValue, downloadedFiles);
                var picture3 = await DownloadFileAsync(metadata.Manager.GetProperty("Picture3")?.StringValue, downloadedFiles);

                //ALchub Start Code
                picture1 = await GetThumbLocalPathAsync(picture1);
                picture2 = await GetThumbLocalPathAsync(picture2);
                picture3 = await GetThumbLocalPathAsync(picture3);
                //Alchub End

                productPictureMetadata.Add(new ProductPictureMetadata
                {
                    ProductItem = product,
                    Picture1Path = picture1,
                    Picture2Path = picture2,
                    Picture3Path = picture3,
                    IsNew = isNew
                });


                //Alchub Start
                if (lastLoadedProduct == null)
                {
                    var deleteAllSpecificationAttribute = await _specificationAttributeService.GetProductSpecificationAttributesAsync(productId: product.Id);
                    foreach (var productspecificationAttribute in deleteAllSpecificationAttribute)
                    {
                        await _specificationAttributeService.DeleteProductSpecificationAttributeAsync(productspecificationAttribute);
                    }
                }
                //Associate Product
                if (product != null)
                {
                    List<int> productIds = new List<int>();
                    var varientProductIds = metadata.Manager.GetProperty("VarientProductIds")?.StringValue;
                    if (!string.IsNullOrEmpty(varientProductIds))
                    {
                        productIds = varientProductIds?.Split(';')?.Select(Int32.Parse)?.ToList();
                        var selectedProducts = await _productService.GetProductsByIdsAsync(productIds.ToArray());

                        //++alchub
                        var tryToAddGroupedProduct = selectedProducts
                            .Any(p => p.ProductType == ProductType.GroupedProduct);

                        var tryToAddSelfGroupedProduct = selectedProducts
                            .Select(p => p.Id)
                            .Contains(product.Id);

                        if (selectedProducts.Any())
                        {
                            foreach (var productA in selectedProducts)
                            {
                                if (productA.Id == product.Id)
                                    continue;

                                //++alchub
                                if (productA.ProductType == ProductType.GroupedProduct)
                                    continue;


                                productA.ParentGroupedProductId = product.Id;
                                //default size & container
                                //productA.Size = (await _alchubGeneralService.GetProductSizesAsync(productA))?.FirstOrDefault() ?? string.Empty;
                                //productA.Container = (await _alchubGeneralService.GetProductContainersAsync(productA))?.FirstOrDefault() ?? string.Empty;
                                await _productService.UpdateProductAsync(productA);

                                // update sub products
                                var subproduct = await _alchubGeneralService.GetProductsByUpcCodeAsync(productA.UPCCode, false);
                                foreach (var item in subproduct)
                                {
                                    item.Size = productA.Size;
                                    item.Container = productA.Container;
                                    await _productService.UpdateProductAsync(item);
                                }
                            }
                        }
                    }
                    //if (!string.IsNullOrEmpty(varientProductIds))
                    //{

                    //    var products = await _productService.GetAssociatedProductsAsync(product.Id);
                    //    foreach (var item in products)
                    //    {
                    //        item.ParentGroupedProductId = 0;
                    //        await _productService.UpdateProductAsync(item);
                    //    }
                    //    foreach (var associatedToProductId in productIds)
                    //    {
                    //        var associatedProduct = await _productService.GetProductByIdAsync(associatedToProductId);
                    //        if (associatedProduct != null)
                    //        {
                    //            associatedProduct.ParentGroupedProductId = product.Id;
                    //            await _productService.UpdateProductAsync(associatedProduct);
                    //        }
                    //    }
                    //}
                }

                //Alchub End

                lastLoadedProduct = product;

                //update "HasTierPrices" and "HasDiscountsApplied" properties
                //_productService.UpdateHasTierPricesProperty(product);
                //_productService.UpdateHasDiscountsApplied(product);
            }

            if (_mediaSettings.ImportProductImagesUsingHash && await _pictureService.IsStoreInDbAsync())
                await ImportProductImagesUsingHashAsync(productPictureMetadata, allProductsBySku);
            else
                await ImportProductImagesUsingServicesAsync(productPictureMetadata);

            foreach (var downloadedFile in downloadedFiles)
            {
                if (!_fileProvider.FileExists(downloadedFile))
                    continue;

                try
                {
                    _fileProvider.DeleteFile(downloadedFile);
                }
                catch
                {
                    // ignored
                }
            }

            //activity log
            await _customerActivityService.InsertActivityAsync("ImportProducts", string.Format(await _localizationService.GetResourceAsync("ActivityLog.Vendor.ImportProducts"), metadata.CountProductsInFile));
        }

        /// <summary>
        /// Import master products from XLSX file
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task ImportMasterProductsFromXlsxAsync(Stream stream)
        {
            using var workbook = new XLWorkbook(stream);
            // get the first worksheet in the workbook
            var worksheet = workbook.Worksheets.FirstOrDefault();
            if (worksheet == null)
                throw new NopException("No worksheet found");

            var downloadedFiles = new List<string>();

            var metadata = await PrepareImportMasterProductDataAsync(worksheet);
            var columnMissing = new List<string>();
            try
            {
                if (!metadata.Manager.GetProperties.Any(p => p.PropertyName == "Name"))
                    columnMissing.Add("Name");
                if (!metadata.Manager.GetProperties.Any(p => p.PropertyName == "SKU"))
                    columnMissing.Add("SKU");
                if (!metadata.Manager.GetProperties.Any(p => p.PropertyName == "UPCCode"))
                    columnMissing.Add("UPCCode");
                if (!metadata.Manager.GetProperties.Any(p => p.PropertyName == "ChildProductIds"))
                    columnMissing.Add("ChildProductIds");
                if (!metadata.Manager.GetProperties.Any(p => p.PropertyName == "ShortDescription"))
                    columnMissing.Add("ShortDescription");
                if (!metadata.Manager.GetProperties.Any(p => p.PropertyName == "MetaKeywords"))
                    columnMissing.Add("MetaKeywords");
                if (!metadata.Manager.GetProperties.Any(p => p.PropertyName == "MetaDescription"))
                    columnMissing.Add("MetaDescription");
                if (!metadata.Manager.GetProperties.Any(p => p.PropertyName == "MetaTitle"))
                    columnMissing.Add("MetaTitle");
                if (!metadata.Manager.GetProperties.Any(p => p.PropertyName == "SeName"))
                    columnMissing.Add("SeName");
                if (!metadata.Manager.GetProperties.Any(p => p.PropertyName == "Published"))
                    columnMissing.Add("Published");
                if (!metadata.Manager.GetProperties.Any(p => p.PropertyName == "VisibleIndividually"))
                    columnMissing.Add("VisibleIndividually");
                if (!metadata.Manager.GetProperties.Any(p => p.PropertyName == "Size"))
                    columnMissing.Add("Size");
                if (!metadata.Manager.GetProperties.Any(p => p.PropertyName == "Container"))
                    columnMissing.Add("Container");
                if (!metadata.Manager.GetProperties.Any(p => p.PropertyName == "Category1"))
                    columnMissing.Add("Category1");
                if (!metadata.Manager.GetProperties.Any(p => p.PropertyName == "Category2"))
                    columnMissing.Add("Category2");
                if (!metadata.Manager.GetProperties.Any(p => p.PropertyName == "Category3"))
                    columnMissing.Add("Category3");
                if (!metadata.Manager.GetProperties.Any(p => p.PropertyName == "Manufacturers"))
                    columnMissing.Add("Manufacturers");
                if (!metadata.Manager.GetProperties.Any(p => p.PropertyName == "Picture1"))
                    columnMissing.Add("Picture1");
                if (!metadata.Manager.GetProperties.Any(p => p.PropertyName == "Picture2"))
                    columnMissing.Add("Picture3");
                if (!metadata.Manager.GetProperties.Any(p => p.PropertyName == "Picture3"))
                    columnMissing.Add("Picture3");
            }
            catch (Exception)
            {
                return;
            }
            if (columnMissing.Any())
            {
                var fieldName = string.Join(",", columnMissing);
                throw new NopException(string.Format(await _localizationService.GetResourceAsync("Admin.Import.MasterProduct.FieldMissing"), fieldName));
            }

            if (_catalogSettings.ExportImportSplitProductsFile && metadata.CountProductsInFile > _catalogSettings.ExportImportProductsCountInOneFile)
            {
                await ImportMasterProductsFromSplitedXlsxAsync(worksheet, metadata);
                return;
            }

            //performance optimization, load all products by SKU in one SQL request
            var currentVendor = await _workContext.GetCurrentVendorAsync();
            //var allProductsBySku = await _productService.GetProductsBySkuAsync(metadata.AllSku.ToArray(), currentVendor?.Id ?? 0);
            var allProductsBySku = await _productService.GetMasterProductsByUPCCodeAsync(metadata.AllSku.ToArray(), currentVendor?.Id ?? 0);

            //validate maximum number of products per vendor
            if (_vendorSettings.MaximumProductNumber > 0 &&
                currentVendor != null)
            {
                var newProductsCount = metadata.CountProductsInFile - allProductsBySku.Count;
                if (await _productService.GetNumberOfProductsByVendorIdAsync(currentVendor.Id) + newProductsCount > _vendorSettings.MaximumProductNumber)
                    throw new ArgumentException(string.Format(await _localizationService.GetResourceAsync("Admin.Catalog.Products.ExceededMaximumNumber"), _vendorSettings.MaximumProductNumber));
            }

            //performance optimization, load all categories IDs for products in one SQL request
            var allProductsCategoryIds = await _categoryService.GetProductCategoryIdsAsync(allProductsBySku.Select(p => p.Id).ToArray());

            //performance optimization, load all categories in one SQL request
            Dictionary<CategoryKey, Category> allCategories;
            try
            {
                var allCategoryList = await _categoryService.GetAllCategoriesAsync(showHidden: true);

                allCategories = await allCategoryList
                    .ToDictionaryAwaitAsync(async c => await CategoryKey.CreateCategoryKeyAsync(c, _categoryService, allCategoryList, _storeMappingService), c => new ValueTask<Category>(c));
            }
            catch (ArgumentException)
            {
                //categories with the same name are not supported in the same category level
                throw new ArgumentException(await _localizationService.GetResourceAsync("Admin.Catalog.Products.Import.CategoriesWithSameNameNotSupported"));
            }

            //performance optimization, load all manufacturers IDs for products in one SQL request
            var allProductsManufacturerIds = await _manufacturerService.GetProductManufacturerIdsAsync(allProductsBySku.Select(p => p.Id).ToArray());

            //performance optimization, load all manufacturers in one SQL request
            var allManufacturers = await _manufacturerService.GetAllManufacturersAsync(showHidden: true);

            //performance optimization, load all stores in one SQL request
            var allStores = await _storeService.GetAllStoresAsync();

            Product lastLoadedProduct = null;
            //products not imported for any reasons
            var productsNotImported = new List<ImportFailedProduct>();
            var insertedProducts = 0;
            var updatedProducts = 0;

            for (var iRow = 2; iRow < metadata.EndRow; iRow++)
            {
                metadata.Manager.ReadFromXlsx(worksheet, iRow);
                var product = metadata.SkuCellNum > 0 ? allProductsBySku.FirstOrDefault(p => p.UPCCode == metadata.Manager.GetProperty("SKU").StringValue?.Trim()) : null;
                if (product == null)
                {
                    int? tempProductId = metadata.Manager.GetProperty("ProductId")?.IntValue ?? null;
                    product = tempProductId.HasValue ? await _productService.GetProductByIdAsync(tempProductId.Value) : null;
                }

                var isNew = product == null;
                product ??= new Product();

                //some of previous values
                var previousStockQuantity = product.StockQuantity;
                var previousWarehouseId = product.WarehouseId;
                var previousProductType = product.ProductType;

                //check for group product
                var isGroupProduct = false;
                var firstAssociatedProductId = 0; //this will be usefull when we have to update group product details.
                var varientIds = metadata.Manager.GetProperty("ChildProductIds")?.StringValue;
                if (!string.IsNullOrWhiteSpace(varientIds))
                    isGroupProduct = true;

                if (isNew)
                    product.CreatedOnUtc = DateTime.UtcNow;
                try
                {
                    if (!isNew)
                    {
                        //validate previouse product type is group? then cannot switch to simple product - 19-06-23
                        var currentProductType = isGroupProduct ? ProductType.GroupedProduct : ProductType.SimpleProduct;
                        if (previousProductType == ProductType.GroupedProduct && currentProductType == ProductType.SimpleProduct)
                            throw new ArgumentException(await _localizationService.GetResourceAsync("Alchub.Admin.Import.MasterProduct.GroupToSimpleType.NotAllowed"));
                    }

                    foreach (var property in metadata.Manager.GetProperties)
                    {
                        switch (property.PropertyName)
                        {
                            case "Name":
                                //validate name
                                var productName = property.StringValue?.Trim();
                                if (string.IsNullOrEmpty(productName) || string.IsNullOrWhiteSpace(productName))
                                    throw new ArgumentException(await _localizationService.GetResourceAsync("Admin.Import.MasterProduct.Field.ProductName.Empty"));

                                product.Name = productName;
                                break;
                            case "ShortDescription":
                                product.ShortDescription = property.StringValue;
                                break;
                            case "FullDescription":
                                product.FullDescription = property.StringValue;
                                break;
                            case "DisplayOrder":
                                //vendor can't change this field
                                if (currentVendor == null)
                                    product.DisplayOrder = property.IntValue;
                                break;
                            case "MetaKeywords":
                                product.MetaKeywords = property.StringValue;
                                break;
                            case "MetaDescription":
                                product.MetaDescription = property.StringValue;
                                break;
                            case "MetaTitle":
                                product.MetaTitle = property.StringValue;
                                break;
                            case "Published":
                                //set true if empty for group product(08-06-23)
                                if (isGroupProduct && string.IsNullOrEmpty(property.StringValue?.Trim()))
                                    product.Published = true;
                                else
                                    product.Published = property.BooleanValue;
                                break;
                            //added VisibleIndividually in excel - 05-07-23
                            case "VisibleIndividually":
                                //set true for group product
                                if (isGroupProduct)
                                    product.VisibleIndividually = true;
                                else
                                    product.VisibleIndividually = property.BooleanValue;
                                break;
                            //Note:(10-12-22) Now UPCCODE is SKU
                            case "UPCCode":
                                if (!isGroupProduct) //ignore for group product
                                {
                                    //Note : (28-06-23) restrict UPCCode to update, only insert for new product
                                    if (isNew)
                                    {
                                        product.Sku = property.StringValue.Trim();

                                        //validate duplicate upc(sku)
                                        var productBySku = await _productService.GetProductBySkuAsync(property.StringValue.Trim());
                                        if (productBySku != null)
                                        {
                                            throw new ArgumentException(await _localizationService.GetResourceAsync("Admin.Import.MasterProduct.Products.Already.Exist.Sku"));

                                            //if (isNew)
                                            //throw new ArgumentException(await _localizationService.GetResourceAsync("Admin.Import.MasterProduct.Products.Already.Exist.Sku"));
                                            //else
                                            //{
                                            //    //on update, validate exept same product
                                            //    if (product.Id != productBySku.Id)
                                            //        throw new ArgumentException(await _localizationService.GetResourceAsync("Admin.Import.MasterProduct.Products.Already.Exist.Sku"));
                                            //}
                                        }
                                    }
                                }
                                else
                                    product.Sku = string.Empty; //upc will be empty for grouped product

                                break;
                            //Note:(10-12-22) Now UPCCODE is SKU
                            case "SKU":
                                //sku will be auto generated and will never change or get empty.
                                //product.UPCCode = property.StringValue;
                                break;
                            case "Size":
                                if (!isGroupProduct) //ignore for group product
                                {
                                    //add size if available, else do not create new (20/12/22)
                                    //get existing sizes
                                    var sizesStr = _alchubSettings.ProductSizes;
                                    var sizes = sizesStr.Split(",", StringSplitOptions.RemoveEmptyEntries)?.ToList() ?? new List<string>();
                                    var lowerSizes = sizes?.Select(x => x.ToLowerInvariant())?.ToList();
                                    var importedSize = property.StringValue?.Trim()?.ToLowerInvariant();
                                    if (lowerSizes.Contains(importedSize))
                                        product.Size = property.StringValue.Trim();
                                    else
                                    {
                                        //create new & append in setting.
                                        sizes.Add(property.StringValue.Trim());
                                        _alchubSettings.ProductSizes = string.Join(",", sizes);
                                        await _settingService.SaveSettingAsync(_alchubSettings);
                                        product.Size = property.StringValue.Trim();
                                    }
                                }
                                break;
                            case "Container":
                                if (!isGroupProduct) //ignore for group product
                                {
                                    //add container if available, else do not create new (20/12/22)
                                    //get setting value
                                    var containersStr = _alchubSettings.ProductContainers;
                                    var containers = containersStr.Split(",", StringSplitOptions.RemoveEmptyEntries)?.ToList() ?? new List<string>();
                                    var lowerContainers = containers?.Select(x => x.ToLowerInvariant())?.ToList();
                                    var importedContainer = property.StringValue?.Trim()?.ToLowerInvariant();
                                    if (lowerContainers.Contains(importedContainer))
                                        product.Container = property.StringValue.Trim();
                                    else
                                    {
                                        //create new & append in setting.
                                        containers.Add(property.StringValue.Trim());
                                        _alchubSettings.ProductContainers = string.Join(",", containers);
                                        await _settingService.SaveSettingAsync(_alchubSettings);
                                        product.Container = property.StringValue.Trim();
                                    }
                                }
                                break;
                        }
                    }

                    //set some default values if not specified
                    //var varientIds = metadata.Manager.GetProperty("ChildProductIds")?.StringValue;
                    if (!isGroupProduct)
                        product.ProductType = ProductType.SimpleProduct;
                    else
                    {
                        product.ProductType = ProductType.GroupedProduct;
                        product.ProductTypeId = (int)ProductType.GroupedProduct;
                        //assign default grouped product template
                        var productTemplateGrouped = (await _productTemplateService.GetAllProductTemplatesAsync())?.FirstOrDefault(pt => pt.Name.StartsWith("Grouped", StringComparison.InvariantCultureIgnoreCase));
                        if (productTemplateGrouped != null)
                            product.ProductTemplateId = productTemplateGrouped.Id;
                    }

                    if (isNew && metadata.Properties.All(p => p.PropertyName != "VisibleIndividually"))
                        product.VisibleIndividually = true;
                    if (isNew && metadata.Properties.All(p => p.PropertyName != "Published"))
                        product.Published = true;

                    //sets the current vendor for the new product
                    //if (isNew && currentVendor != null)
                    //    product.VendorId = currentVendor.Id;

                    product.UpdatedOnUtc = DateTime.UtcNow;
                    //default values
                    product.IsMaster = true;
                    product.OrderMinimumQuantity = 1;
                    product.OrderMaximumQuantity = 10000;
                    product.IsShipEnabled = true;
                    product.AllowCustomerReviews = true;

                    if (isNew)
                    {
                        if (product.ProductTypeId == (int)ProductType.SimpleProduct)
                        {
                            //Note:(10-12-22) Now UPCCODE is SKU & SKU will autogenerated & so lets assign autogenerated value to UPCCODE.
                            //autogenerate SKU (mean UPCCODE) for simple product
                            product.UPCCode = await _productService.GenerateMasterProductSKU(); //{AH000000}
                        }

                        await _productService.InsertProductAsync(product);
                        insertedProducts++;
                    }
                    else
                    {
                        //update
                        await _productService.UpdateProductAsync(product);

                        ////remove associated products
                        //if (previousProductType == ProductType.GroupedProduct && product.ProductType == ProductType.SimpleProduct)
                        //{
                        //    var store = await _storeContext.GetCurrentStoreAsync();
                        //    var storeId = store?.Id ?? 0;
                        //    var vendorId = currentVendor?.Id ?? 0;

                        //    var associatedProducts = await _productService.GetAssociatedProductsAsync(product.Id, storeId, vendorId);
                        //    foreach (var associatedProduct in associatedProducts)
                        //    {
                        //        associatedProduct.ParentGroupedProductId = 0;
                        //        await _productService.UpdateProductAsync(associatedProduct);
                        //    }

                        //    //also generate SKU for this Product, as simple product will always have SKU
                        //    product.UPCCode = await _productService.GenerateMasterProductSKU(); //{AH000000}
                        //    await _productService.UpdateProductAsync(product);
                        //}
                        updatedProducts++;
                    }
                }
                catch (Exception exc)
                {
                    var importFailedProduct = new ImportFailedProduct()
                    {
                        Name = product.Name,
                        Sku = product.UPCCode,
                        Upc = product.Sku,
                        Exception = exc.ToString()
                    };
                    productsNotImported.Add(importFailedProduct);
                    continue;
                }

                if (!isGroupProduct) //ignore for group product
                {
                    //imports product attributes
                    var cellPosition = metadata.Properties.Where(p => p.PropertyName.Contains("Att_"))?.Select(x => x.PropertyOrderPosition)?.FirstOrDefault();
                    var attributeCellPosition = cellPosition == null ? 0 : Convert.ToInt32(cellPosition);

                    metadata.AttributeManager.ReadFromSpecificationAttributesXlsx(worksheet, iRow, attributeCellPosition);
                    await ImportAttributeAsync(metadata.AttributeManager, product);
                }

                //quantity change history
                if (isNew || previousWarehouseId == product.WarehouseId)
                {
                    await _productService.AddStockQuantityHistoryEntryAsync(product, product.StockQuantity - previousStockQuantity, product.StockQuantity,
                        product.WarehouseId, await _localizationService.GetResourceAsync("Admin.StockQuantityHistory.Messages.ImportProduct.Edit"));
                }
                //warehouse is changed 
                else
                {
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

                    var message = string.Format(await _localizationService.GetResourceAsync("Admin.StockQuantityHistory.Messages.ImportProduct.EditWarehouse"), oldWarehouseMessage, newWarehouseMessage);

                    //record history
                    await _productService.AddStockQuantityHistoryEntryAsync(product, -previousStockQuantity, 0, previousWarehouseId, message);
                    await _productService.AddStockQuantityHistoryEntryAsync(product, product.StockQuantity, product.StockQuantity, product.WarehouseId, message);
                }

                var tempProperty = metadata.Manager.GetProperty("SeName");

                //search engine name
                var seName = tempProperty?.StringValue ?? (isNew ? string.Empty : await _urlRecordService.GetSeNameAsync(product, 0));
                await _urlRecordService.SaveSlugAsync(product, await _urlRecordService.ValidateSeNameAsync(product, seName, product.Name, true), 0);

                if (!isGroupProduct) //ignore for group product
                {
                    var category1 = metadata.Manager.GetProperty("Category1");
                    var category2 = metadata.Manager.GetProperty("Category2");
                    var category3 = metadata.Manager.GetProperty("Category3");

                    var categoryList = category1.StringValue + ";" + category2.StringValue + ";" + category3.StringValue;
                    if (categoryList != null)
                    {
                        //category mappings
                        var categories = isNew || !allProductsCategoryIds.ContainsKey(product.Id) ? Array.Empty<int>() : allProductsCategoryIds[product.Id];

                        var storesIds = product.LimitedToStores
                            ? (await _storeMappingService.GetStoresIdsWithAccessAsync(product)).ToList()
                            : new List<int>();

                        var importedCategories = await categoryList.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                            .Select(categoryName => new CategoryKey(categoryName, storesIds))
                            .SelectAwait(async categoryKey =>
                            {
                                var rez = (allCategories.ContainsKey(categoryKey) ? allCategories[categoryKey].Id : allCategories.Values.FirstOrDefault(c => c.Name == categoryKey.Key)?.Id) ??
                                          allCategories.FirstOrDefault(p =>
                                        p.Key.Key.Equals(categoryKey.Key, StringComparison.InvariantCultureIgnoreCase))
                                    .Value?.Id;

                                if (!rez.HasValue && int.TryParse(categoryKey.Key, out var id))
                                    rez = id;

                                if (!rez.HasValue)
                                    //database doesn't contain the imported category
                                    throw new ArgumentException(string.Format(await _localizationService.GetResourceAsync("Admin.Catalog.Products.Import.DatabaseNotContainCategory"), categoryKey.Key));

                                return rez.Value;
                            }).ToListAsync();

                        foreach (var categoryId in importedCategories)
                        {
                            if (categories.Any(c => c == categoryId))
                                continue;

                            var productCategory = new ProductCategory
                            {
                                ProductId = product.Id,
                                CategoryId = categoryId,
                                IsFeaturedProduct = false,
                                DisplayOrder = 1
                            };
                            await _categoryService.InsertProductCategoryAsync(productCategory);
                        }

                        //delete product categories
                        var deletedProductCategories = await categories.Where(categoryId => !importedCategories.Contains(categoryId))
                            .SelectAwait(async categoryId => (await _categoryService.GetProductCategoriesByProductIdAsync(product.Id, true)).FirstOrDefault(pc => pc.CategoryId == categoryId)).Where(pc => pc != null).ToListAsync();

                        foreach (var deletedProductCategory in deletedProductCategories)
                            await _categoryService.DeleteProductCategoryAsync(deletedProductCategory);
                    }

                    //product can have only a single manufacturer/brand.
                    tempProperty = metadata.Manager.GetProperty("Manufacturers");
                    if (tempProperty != null)
                    {
                        var manufacturer = tempProperty.StringValue;
                        //manufacturer mappings
                        var manufacturers = isNew || !allProductsManufacturerIds.ContainsKey(product.Id) ? Array.Empty<int>() : allProductsManufacturerIds[product.Id];
                        var importedManufacturers = allManufacturers.FirstOrDefault(m => m.Name == manufacturer.Trim())?.Id;
                        if (importedManufacturers != null && importedManufacturers != 0)
                        {
                            if (!manufacturers.Any(c => c == importedManufacturers))
                            {
                                var productManufacturer = new ProductManufacturer
                                {
                                    ProductId = product.Id,
                                    ManufacturerId = Convert.ToInt32(importedManufacturers),
                                    IsFeaturedProduct = false,
                                    DisplayOrder = 1
                                };
                                await _manufacturerService.InsertProductManufacturerAsync(productManufacturer);

                                //delete product manufacturers
                                var deletedProductsManufacturers = await manufacturers.Where(mid => mid != importedManufacturers)
                                    .SelectAwait(async manufacturerId => (await _manufacturerService.GetProductManufacturersByProductIdAsync(product.Id)).First(pc => pc.ManufacturerId == manufacturerId)).ToListAsync();
                                foreach (var deletedProductManufacturer in deletedProductsManufacturers)
                                    await _manufacturerService.DeleteProductManufacturerAsync(deletedProductManufacturer);
                            }
                        }
                    }
                }

                var urlString1 = string.Empty;
                var urlString2 = string.Empty;
                var urlString3 = string.Empty;

                if (!string.IsNullOrWhiteSpace(metadata.Manager.GetProperty("Picture1")?.StringValue))
                    urlString1 = metadata.Manager.GetProperty("Picture1")?.StringValue;
                if (!string.IsNullOrWhiteSpace(metadata.Manager.GetProperty("Picture2")?.StringValue))
                    urlString2 = metadata.Manager.GetProperty("Picture2")?.StringValue;
                if (!string.IsNullOrWhiteSpace(metadata.Manager.GetProperty("Picture3")?.StringValue))
                    urlString3 = metadata.Manager.GetProperty("Picture3")?.StringValue;

                var picture1 = await DownloadFileAsync(urlString1, downloadedFiles);
                var picture2 = await DownloadFileAsync(urlString2, downloadedFiles);
                var picture3 = await DownloadFileAsync(urlString3, downloadedFiles);

                if (!string.IsNullOrEmpty(picture1))
                    picture1 = await GetThumbLocalPathAsync(picture1);
                if (!string.IsNullOrEmpty(picture2))
                    picture2 = await GetThumbLocalPathAsync(picture2);
                if (!string.IsNullOrEmpty(picture3))
                    picture3 = await GetThumbLocalPathAsync(picture3);

                //product to import images
                var productPictureMetadata = new List<ProductPictureMetadata>
                {
                    new ProductPictureMetadata
                    {
                        ProductItem = product,
                        Picture1Path = picture1,
                        Picture2Path = picture2,
                        Picture3Path = picture3,
                        IsNew = isNew
                    }
                };

                //Associate Product
                if (product != null)
                {
                    List<int> productIds = new List<int>();
                    var varientProductIds = metadata.Manager.GetProperty("ChildProductIds")?.StringValue;
                    if (!string.IsNullOrEmpty(varientProductIds))
                    {
                        productIds = varientProductIds?.Split(',')?.Select(int.Parse)?.ToList();
                        var selectedProducts = await _productService.GetProductsByIdsAsync(productIds.ToArray());

                        //++alchub
                        var tryToAddGroupedProduct = selectedProducts
                            .Any(p => p.ProductType == ProductType.GroupedProduct);

                        var tryToAddSelfGroupedProduct = selectedProducts
                            .Select(p => p.Id)
                            .Contains(product.Id);

                        if (selectedProducts.Any())
                        {
                            //update existing varient products if exists
                            if (isNew)
                            {
                                var exVarientProducts = await _productService.GetAssociatedProductsAsync(product.Id);
                                foreach (var vp in exVarientProducts)
                                {
                                    vp.ParentGroupedProductId = 0;
                                    vp.CreatedOnUtc = DateTime.UtcNow;
                                    await _productService.UpdateProductAsync(vp);
                                }
                            }

                            //Set associated product display order as ChildProductId sequence (06-06-23)
                            var displayOrder = 0;
                            foreach (var productA in selectedProducts)
                            {
                                if (productA.Id == product.Id)
                                    continue;

                                //++alchub
                                if (productA.ProductType == ProductType.GroupedProduct)
                                    continue;

                                productA.ParentGroupedProductId = product.Id;
                                //set display order according sequance.
                                productA.DisplayOrder = displayOrder;

                                //assign first associated product id for later use
                                if (firstAssociatedProductId == 0)
                                    firstAssociatedProductId = productA.Id;

                                //default size & container
                                //productA.Size = (await _alchubGeneralService.GetProductSizesAsync(productA))?.FirstOrDefault() ?? string.Empty;
                                //productA.Container = (await _alchubGeneralService.GetProductContainersAsync(productA))?.FirstOrDefault() ?? string.Empty;
                                await _productService.UpdateProductAsync(productA);

                                displayOrder++;

                                // update sub products
                                var subproduct = await _alchubGeneralService.GetProductsByUpcCodeAsync(productA.UPCCode, false);
                                foreach (var item in subproduct)
                                {
                                    item.Size = productA.Size;
                                    item.Container = productA.Container;
                                    await _productService.UpdateProductAsync(item);
                                }
                            }
                        }
                    }
                    //Alchub End
                    lastLoadedProduct = product;
                }

                if (_mediaSettings.ImportProductImagesUsingHash && await _pictureService.IsStoreInDbAsync())
                    await ImportMasterProductImagesUsingHashAsync(productPictureMetadata, allProductsBySku);
                else
                {
                    if (!isGroupProduct)
                        await ImportProductImagesUsingServicesAsync(productPictureMetadata);
                }

                foreach (var downloadedFile in downloadedFiles)
                {
                    if (!_fileProvider.FileExists(downloadedFile))
                        continue;

                    try
                    {
                        _fileProvider.DeleteFile(downloadedFile);
                    }
                    catch
                    {
                        // ignored
                    }
                }

                //migrate data to group product
                if (isGroupProduct && firstAssociatedProductId > 0)
                {
                    //copy associated product data to group product
                    await CopyAssociatedProductDataToGroupProduct(product, firstAssociatedProductId);
                }
            }

            //activity log
            await _customerActivityService.InsertActivityAsync("ImportProducts", string.Format(await _localizationService.GetResourceAsync("ActivityLog.ImportProducts"), metadata.CountProductsInFile, insertedProducts, updatedProducts));

            //send email for products not imported
            await SendEmailForInvalidProduct(productsNotImported);
        }
        #endregion

        #region PictureThumb Path
        protected virtual Task<string> GetThumbLocalPathAsync(string thumbFileName)
        {
            var thumbsDirectoryPath = _fileProvider.GetAbsolutePath(NopMediaDefaults.ImageThumbsPath);

            if (_mediaSettings.MultipleThumbDirectories)
            {
                //get the first two letters of the file name
                var fileNameWithoutExtension = _fileProvider.GetFileNameWithoutExtension(thumbFileName);
                if (fileNameWithoutExtension != null && fileNameWithoutExtension.Length > NopMediaDefaults.MultipleThumbDirectoriesLength)
                {
                    var subDirectoryName = fileNameWithoutExtension[0..NopMediaDefaults.MultipleThumbDirectoriesLength];
                    thumbsDirectoryPath = _fileProvider.GetAbsolutePath(NopMediaDefaults.ImageThumbsPath, subDirectoryName);
                    _fileProvider.CreateDirectory(thumbsDirectoryPath);
                }
            }

            var thumbFilePath = _fileProvider.Combine(thumbsDirectoryPath, thumbFileName);
            return Task.FromResult(thumbFilePath);
        }
        #endregion

        #region Sync vendor products from excel

        /// <summary>
        /// Creates a table for unprocessed products
        /// </summary>
        /// <param name="sb"></param>
        /// <param name="unprocessedProducts"></param>
        /// <returns></returns>
        private Task<string> UnprocessedProductHtmlTable(List<ImportFailedProduct> unprocessedProducts)
        {
            var sb = new StringBuilder();
            if (unprocessedProducts != null && unprocessedProducts.Any())
            {
                var sNo = 1;
                sb.AppendLine("<table border=\"1\" style=\"border-collapse: collapse;\">");
                sb.AppendLine($"<tr>");
                sb.AppendLine($"<th>S. NO.</th>");
                sb.AppendLine($"<th>{SCAN_CODE}</th>");
                sb.AppendLine($"<th>Product name</th>");
                sb.AppendLine($"<th>{CURRENT_QTY}</th>");
                sb.AppendLine($"<th>{RETAIL}</th>");
                sb.AppendLine($"<th>Reason of failure</th>");
                sb.AppendLine("</tr>");

                var table = unprocessedProducts;
                for (var i = 0; i <= table.Count - 1; i++)
                {
                    var unprocessedProduct = table[i];

                    sb.AppendLine($"<tr style=\"text-align: center;\">");
                    sb.AppendLine("<td style=\"padding: 0.6em 0.4em;text-align: left;\">" + sNo + "</td>");
                    sb.AppendLine("<td style=\"padding: 0.6em 0.4em;text-align: left;\">" + unprocessedProduct.Upc + "</td>");
                    sb.AppendLine("<td style=\"padding: 0.6em 0.4em;text-align: left;\">" + unprocessedProduct.Name + "</td>");
                    sb.AppendLine("<td style=\"padding: 0.6em 0.4em;text-align: left;\">" + unprocessedProduct.Stock + "</td>");
                    sb.AppendLine("<td style=\"padding: 0.6em 0.4em;text-align: left;\">" + unprocessedProduct.Price + "</td>");
                    sb.AppendLine("<td style=\"padding: 0.6em 0.4em;text-align: left;\">" + unprocessedProduct.Exception + "</td>");
                    sb.AppendLine("</tr>");
                    sNo++;
                }
                sb.AppendLine("</table>");
            }

            return Task.FromResult(sb?.ToString());
        }


        /// <summary>
        /// Sends email for unprocessed vendor products
        /// </summary>
        /// <param name="unprocessedProducts"></param>
        /// <param name="vendor"></param>
        /// <returns></returns>
        private async Task SendEmailForUnprocessedProduct(List<ImportFailedProduct> unprocessedProducts, Vendor vendor)
        {
            if (unprocessedProducts == null || vendor == null)
                return;

            var languageId = (await _workContext.GetWorkingLanguageAsync()).Id;
            //send unprocessed products email
            if (unprocessedProducts.Any())
            {
                var body = await UnprocessedProductHtmlTable(unprocessedProducts);

                //send email to admin
                await _workflowMessageService.SendUnprocessedProductMessageToAdmin(languageId, vendor, body);

                //send email to vendor
                await _workflowMessageService.SendUnprocessedProductMessageForVendor(languageId, vendor, body);
            }
        }

        /// <summary>
        /// Get property list by vendor excel cells
        /// </summary>
        /// <typeparam name="T">Type of object</typeparam>
        /// <param name="worksheet">Excel worksheet</param>
        /// <returns>Property list</returns>
        public static IList<PropertyByName<T>> GetPropertiesByVendorExcelCells<T>(IXLWorksheet worksheet)
        {
            var properties = new List<PropertyByName<T>>();
            var poz = 2;
            while (true)
            {
                try
                {
                    var cell = worksheet.Row(4).Cell(poz);

                    if (string.IsNullOrEmpty(cell?.Value?.ToString()))
                        break;

                    poz += 1;
                    properties.Add(new PropertyByName<T>(cell.Value.ToString()));
                }
                catch
                {
                    break;
                }
            }

            return properties;
        }

        /// <summary>
        /// Prepare vendor products from XLSX file
        /// </summary>
        /// <param name="worksheet"></param>
        /// <returns>A task that represents the asynchronous operation</returns>
        private Task<ImportProductMetadata> PrepareVendorProductSyncDataAsync(IXLWorksheet worksheet)
        {
            //the columns
            var properties = GetPropertiesByVendorExcelCells<VendorInventoryByItem>(worksheet);
            var manager = new PropertyManager<VendorInventoryByItem>(properties, _catalogSettings);
            var endRow = 6;
            var allUPC = new List<string>();
            var upcProperty = manager.GetProperty(SCAN_CODE);
            var upcCellNum = upcProperty?.PropertyOrderPosition ?? -1;
            upcCellNum += 1;
            var productsInFile = new List<int>();

            //find end of data
            while (true)
            {
                var allColumnsAreEmpty = manager.GetProperties
                    .Select(property => worksheet.Row(endRow).Cell(property.PropertyOrderPosition))
                    .All(cell => string.IsNullOrEmpty(cell?.Value?.ToString()));

                if (allColumnsAreEmpty)
                    break;

                if (upcCellNum > 0)
                {
                    var upcCode = worksheet.Row(endRow).Cell(upcCellNum).Value?.ToString() ?? string.Empty;
                    if (!string.IsNullOrEmpty(upcCode))
                    {
                        upcCode = upcCode.Trim();
                        allUPC.Add(upcCode);
                    }
                }

                //counting the number of products
                productsInFile.Add(endRow);

                endRow++;
            }

            return Task.FromResult(new ImportProductMetadata
            {
                EndRow = endRow,
                VendorInventoryByItemManager = manager,
                VendorInventoryByItemProperties = properties,
                ProductsInFile = productsInFile,
                UpcCellNum = upcCellNum,
                AllUpc = allUPC
            });
        }

        /// <summary>
        /// Sync products from vendor ftp xlsx file
        /// </summary>
        /// <param name="file"></param>
        /// <param name="vendor"></param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task<bool> SyncVendorProductsFromFtpXlsxAsync(Vendor vendor, string file)
        {
            var syncedSuccessfully = false;
            if (vendor == null)
                return syncedSuccessfully;

            var unProcessedProducts = new List<ImportFailedProduct>();
            var processedProducts = new List<Product>();

            try
            {
                using var workbook = new XLWorkbook(file);
                // get the first worksheet in the workbook
                var worksheet = workbook.Worksheets.FirstOrDefault();
                if (worksheet == null)
                    throw new NopException("No worksheet found");

                var downloadedFiles = new List<string>();
                var storeId = (await _storeContext.GetCurrentStoreAsync()).Id;
                var metadata = await PrepareVendorProductSyncDataAsync(worksheet);

                var allMasterProducts = new List<Product>();

                //to optimize performace, lets load data in chunks.
                int chunkSize = 3000;
                var allUpcChunks = metadata?.AllUpc?.Chunk(chunkSize)?.ToList() ?? new List<string[]>();
                foreach (var upcChunk in allUpcChunks)
                {
                    //get all master products by upcCode
                    allMasterProducts.AddRange((await _productService.GetMasterProductsBySKUCodeAsync(upcChunk, 0))?.ToList());
                }
                //allMasterProducts.AddRange((await _productService.GetMasterProductsBySKUCodeAsync(metadata.AllUpc.ToArray(), 0))?.ToList());
                allMasterProducts = allMasterProducts?.DistinctBy(p => p.Id)?.ToList();

                //check for duplicate upc code/sku
                var duplicatesSku = metadata?.AllUpc?.GroupBy(x => x)?.Where(g => g.Count() > 1)?.ToList();

                for (var iRow = 6; iRow < metadata.EndRow; iRow++)
                {
                    try
                    {
                        metadata.VendorInventoryByItemManager.ReadFromXlsx(worksheet, iRow, 1);
                        var currentRow = metadata.VendorInventoryByItemManager.GetProperty(SCAN_CODE);
                        var currentRowUpcCode = currentRow?.StringValue?.Trim();

                        //if (duplicatesSku.Any())
                        //{
                        //    //check if duplicate sku list has current row sku value
                        //    if (duplicatesSku.Select(s => s.Key).Contains(currentRowUpcCode))
                        //    {
                        //        var priceRow = metadata.VendorInventoryByItemManager.GetProperty(RETAIL);
                        //        var stockRow = metadata.VendorInventoryByItemManager.GetProperty(CURRENT_QTY);
                        //        var duplicateProduct = new ImportFailedProduct()
                        //        {
                        //            Upc = currentRowUpcCode,
                        //            Price = priceRow?.DecimalValue,
                        //            Stock = stockRow?.IntValueNullable,
                        //            Exception = await _localizationService.GetResourceAsync("Alchub.Duplicate.UPCCode")
                        //        };
                        //        unProcessedProducts.Add(duplicateProduct);
                        //        continue;
                        //    }
                        //}

                        //if excelsheet has duplicate upccode then sync only first product
                        if (processedProducts.Any())
                        {
                            //no need to add again, if it is already in unProcessedProducts list
                            if (unProcessedProducts.Any() && unProcessedProducts.Select(s => s.Upc).Contains(currentRowUpcCode))
                                continue;

                            //check if duplicate
                            var isDuplicate = false;
                            //if current row upc is 11 digit, match first 11 digit of master product upc code. 
                            if (currentRowUpcCode.Length == NopAlchubDefaults.PRODUCT_UPC_11_DIGIT)
                                isDuplicate = processedProducts.Any(p => p.Sku.Substring(0, Math.Min(p.Sku.Length, NopAlchubDefaults.PRODUCT_UPC_11_DIGIT)) == currentRowUpcCode);
                            else
                                isDuplicate = processedProducts.Any(p => p.Sku == currentRowUpcCode);

                            if (isDuplicate)
                            {
                                var priceRow = metadata.VendorInventoryByItemManager.GetProperty(RETAIL);
                                var stockRow = metadata.VendorInventoryByItemManager.GetProperty(CURRENT_QTY);
                                var productNameRow = metadata.VendorInventoryByItemManager.GetProperty(PRODUCT_NAME);

                                var duplicateProduct = new ImportFailedProduct()
                                {
                                    Upc = currentRowUpcCode,
                                    Name = productNameRow?.StringValue,
                                    Price = priceRow?.DecimalValue,
                                    Stock = stockRow?.IntValueNullable,
                                    Exception = await _localizationService.GetResourceAsync("SyncVendorProduct.Duplicate.UPCCode")
                                };
                                unProcessedProducts.Add(duplicateProduct);
                                continue;
                            }
                        }

                        //find current row master product by upc code from master products.
                        var product = new Product();
                        if (metadata.UpcCellNum > 0 && product == null || product?.Id == 0)
                        {
                            //if current row upc is 11 digit, match first 11 digit of master product upc code. 
                            if (currentRowUpcCode.Length == NopAlchubDefaults.PRODUCT_UPC_11_DIGIT)
                                product = allMasterProducts.FirstOrDefault(p => p.Sku.Substring(0, Math.Min(p.Sku.Length, NopAlchubDefaults.PRODUCT_UPC_11_DIGIT)) == currentRowUpcCode);
                            else
                                product = allMasterProducts.FirstOrDefault(p => p.Sku == currentRowUpcCode);

                            //check with friendly upc - 03/07/2023
                            if (product == null || product?.Id == 0)
                                product = await _productFriendlyUpcService.GetMasterProductByFriendlyUPCCodeAsync(vendor.Id, currentRowUpcCode);
                        }

                        if (product == null || product?.Id == 0)
                        {
                            var priceRow = metadata.VendorInventoryByItemManager.GetProperty(RETAIL);
                            var stockRow = metadata.VendorInventoryByItemManager.GetProperty(CURRENT_QTY);
                            var productNameRow = metadata.VendorInventoryByItemManager.GetProperty(PRODUCT_NAME);

                            var duplicateProduct = new ImportFailedProduct()
                            {
                                Upc = currentRowUpcCode,
                                Name = productNameRow?.StringValue,
                                Price = priceRow?.DecimalValue,
                                Stock = stockRow?.IntValueNullable,
                                Exception = await _localizationService.GetResourceAsync("SyncVendorProduct.Invalid.UPCCode")
                            };
                            unProcessedProducts.Add(duplicateProduct);
                            continue;
                        }
                        var stockQuantity = 0;
                        var price = decimal.Zero;
                        foreach (var property in metadata.VendorInventoryByItemManager.GetProperties)
                        {
                            switch (property.PropertyName)
                            {
                                case CURRENT_QTY:
                                    stockQuantity = property.IntValue;
                                    break;
                                case RETAIL:
                                    price = property.DecimalValue;
                                    break;
                            }
                        }

                        //check if product exists for vendor then update existing product, otherwise create a copy product.
                        var existingProduct = (await _productService.SearchProductsAsync(showHidden: true, storeId: storeId, vendorId: vendor.Id,
                            upccode: product.Sku))?.FirstOrDefault();

                        if (existingProduct != null)
                        {
                            //if product is not published then updation will not perform
                            if (!existingProduct.Published)
                                continue;

                            //update product price if product price updation is allowed
                            if (existingProduct.OverridePrice)
                                existingProduct.Price = price;

                            //update product stock if product stock updation is allowed
                            if (existingProduct.OverrideStock)
                            {
                                //if new stock quantity is negative & if product stock negative stock is true then set negative stock as postive
                                //otherwise set stock quantity is 0.
                                if (stockQuantity < 0)
                                {
                                    if (existingProduct.OverrideNegativeStock)
                                        existingProduct.StockQuantity = Math.Abs(stockQuantity);
                                    else
                                        existingProduct.StockQuantity = 0;
                                }
                                else
                                    existingProduct.StockQuantity = stockQuantity;
                            }

                            existingProduct.UpdatedOnUtc = DateTime.UtcNow;
                            //associated product handle
                            existingProduct.ParentGroupedProductId = 0;
                            existingProduct.IsMaster = false;

                            await _productService.UpdateProductAsync(existingProduct);

                            //add into processed products only when product price update
                            if (existingProduct.OverridePrice)
                                processedProducts.Add(existingProduct);
                        }
                        else
                        {
                            var exProduct = product;
                            product.StockQuantity = stockQuantity < 0 ? 0 : stockQuantity;
                            product.Price = price < 0 ? 0 : price;
                            var newProduct = await _copyProductService.CopyProductAsync(product, product.Name, true, true, vendorId: vendor.Id);

                            //update some values for new product which was not copied
                            newProduct.Sku = exProduct.Sku;
                            newProduct.UPCCode = exProduct.UPCCode;
                            newProduct.IsMaster = false;
                            //default config flag
                            newProduct.OverridePrice = true;
                            newProduct.OverrideStock = true;
                            newProduct.Published = true;
                            //associated product handle
                            newProduct.ParentGroupedProductId = 0;
                            await _productService.UpdateProductAsync(newProduct);
                            processedProducts.Add(newProduct);
                        }
                    }
                    catch (Exception exc)
                    {
                        var currentRow = metadata.VendorInventoryByItemManager.GetProperty(SCAN_CODE);
                        var currentRowUpcCode = currentRow?.StringValue?.Trim();

                        var duplicateProduct = new ImportFailedProduct()
                        {
                            Upc = currentRowUpcCode,
                            Name = metadata.VendorInventoryByItemManager.GetProperty(PRODUCT_NAME)?.StringValue,
                            Price = metadata.VendorInventoryByItemManager.GetProperty(RETAIL)?.DecimalValue,
                            Stock = metadata.VendorInventoryByItemManager.GetProperty(CURRENT_QTY)?.IntValueNullable,
                            Exception = exc.Message
                        };
                        unProcessedProducts.Add(duplicateProduct);
                    }
                }

                //making sure distinct products
                processedProducts = processedProducts.DistinctBy(p => p.Id).ToList();

                //apply markup on product price for current vendor
                await ApplyMarkupOnProductPrice(vendor.Id, storeId, processedProducts);

                //send email to vendor for unprocessed products
                if (unProcessedProducts.Any())
                    await SendEmailForUnprocessedProduct(unProcessedProducts.DistinctBy(p => p.Upc).ToList(), vendor);

                //activity log
                await _customerActivityService.InsertActivityAsync("SyncVendorProducts", string.Format(await _localizationService.GetResourceAsync("ScheduleTask.ActivityLog.SyncVendorProducts"), metadata.CountProductsInFile));
            }
            catch (Exception)
            {
                throw;
            }

            if (!unProcessedProducts.Any())
                syncedSuccessfully = true;

            return syncedSuccessfully;
        }

        #endregion

        #region Manufacturer

        /// <summary>
        /// Import manufacturers from XLSX file
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task ImportManufacturersFromXlsxAsync(Stream stream)
        {
            using var workbook = new XLWorkbook(stream);
            // get the first worksheet in the workbook
            var worksheet = workbook.Worksheets.FirstOrDefault();
            if (worksheet == null)
                throw new NopException("No worksheet found");

            //the columns
            var properties = GetPropertiesByExcelCells<Manufacturer>(worksheet);

            var manager = new PropertyManager<Manufacturer>(properties, _catalogSettings);

            var iRow = 2;
            var setSeName = properties.Any(p => p.PropertyName == "SeName");

            while (true)
            {
                var allColumnsAreEmpty = manager.GetProperties
                    .Select(property => worksheet.Row(iRow).Cell(property.PropertyOrderPosition))
                    .All(cell => cell?.Value == null || string.IsNullOrEmpty(cell.Value.ToString()));

                if (allColumnsAreEmpty)
                    break;

                manager.ReadFromXlsx(worksheet, iRow);

                var manufacturer = await _manufacturerService.GetManufacturerByIdAsync(manager.GetProperty("Id").IntValue);

                var isNew = manufacturer == null;

                manufacturer ??= new Manufacturer();

                if (isNew)
                {
                    manufacturer.CreatedOnUtc = DateTime.UtcNow;

                    //default values
                    manufacturer.PageSize = _catalogSettings.DefaultManufacturerPageSize;
                    manufacturer.PageSizeOptions = _catalogSettings.DefaultManufacturerPageSizeOptions;
                    manufacturer.Published = true;
                    manufacturer.AllowCustomersToSelectPageSize = true;
                }

                var seName = string.Empty;

                foreach (var property in manager.GetProperties)
                {
                    switch (property.PropertyName)
                    {
                        case "Name":
                            manufacturer.Name = property.StringValue;
                            break;
                        case "Description":
                            manufacturer.Description = property.StringValue;
                            break;
                        case "ManufacturerTemplateId":
                            manufacturer.ManufacturerTemplateId = property.IntValue;
                            break;
                        case "MetaKeywords":
                            manufacturer.MetaKeywords = property.StringValue;
                            break;
                        case "MetaDescription":
                            manufacturer.MetaDescription = property.StringValue;
                            break;
                        case "MetaTitle":
                            manufacturer.MetaTitle = property.StringValue;
                            break;
                        case "Picture":
                            var picture = await LoadPictureAsync(manager.GetProperty("Picture").StringValue, manufacturer.Name, isNew ? null : (int?)manufacturer.PictureId);

                            if (picture != null)
                                manufacturer.PictureId = picture.Id;

                            break;
                        case "PageSize":
                            manufacturer.PageSize = property.IntValue;
                            break;
                        case "AllowCustomersToSelectPageSize":
                            manufacturer.AllowCustomersToSelectPageSize = property.BooleanValue;
                            break;
                        case "PageSizeOptions":
                            manufacturer.PageSizeOptions = property.StringValue;
                            break;
                        case "PriceRangeFiltering":
                            manufacturer.PriceRangeFiltering = property.BooleanValue;
                            break;
                        case "PriceFrom":
                            manufacturer.PriceFrom = property.DecimalValue;
                            break;
                        case "PriceTo":
                            manufacturer.PriceTo = property.DecimalValue;
                            break;
                        case "AutomaticallyCalculatePriceRange":
                            manufacturer.ManuallyPriceRange = property.BooleanValue;
                            break;
                        case "Published":
                            manufacturer.Published = property.BooleanValue;
                            break;
                        case "DisplayOrder":
                            manufacturer.DisplayOrder = property.IntValue;
                            break;
                        case "SeName":
                            seName = property.StringValue;
                            break;
                    }
                }

                manufacturer.UpdatedOnUtc = DateTime.UtcNow;

                //++Alchub

                //do not insert/update if manufacture name is empty.
                if (string.IsNullOrEmpty(manufacturer.Name) || string.IsNullOrWhiteSpace(manufacturer.Name))
                {
                    await _logger.ErrorAsync($"Import brands error: Brand name is empty at row {iRow}. Hence it was not inserted/updated.");
                    iRow++;
                    continue;
                }

                //--Alchub

                if (isNew)
                    await _manufacturerService.InsertManufacturerAsync(manufacturer);
                else
                    await _manufacturerService.UpdateManufacturerAsync(manufacturer);

                //search engine name
                if (setSeName)
                    await _urlRecordService.SaveSlugAsync(manufacturer, await _urlRecordService.ValidateSeNameAsync(manufacturer, seName, manufacturer.Name, true), 0);

                iRow++;
            }

            //activity log
            await _customerActivityService.InsertActivityAsync("ImportManufacturers",
                string.Format(await _localizationService.GetResourceAsync("ActivityLog.ImportManufacturers"), iRow - 2));
        }

        #endregion
    }
}