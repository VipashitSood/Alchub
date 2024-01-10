using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Alchub.Domain;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Discounts;
using Nop.Core.Domain.Tax;
using Nop.Core.Domain.Vendors;
using Nop.Services;
using Nop.Services.Alchub.General;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Discounts;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Shipping;
using Nop.Services.Stores;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Areas.Admin.Models.Orders;
using Nop.Web.Framework.Extensions;
using Nop.Web.Framework.Factories;
using Nop.Web.Framework.Models.Extensions;

namespace Nop.Web.Areas.Admin.Factories
{
    /// <summary>
    /// Represents the product model factory implementation
    /// </summary>
    public partial class ProductModelFactory : IProductModelFactory
    {
        #region Fields

        private readonly CatalogSettings _catalogSettings;
        private readonly CurrencySettings _currencySettings;
        private readonly IAclSupportedModelFactory _aclSupportedModelFactory;
        private readonly IAddressService _addressService;
        private readonly IBaseAdminModelFactory _baseAdminModelFactory;
        private readonly ICategoryService _categoryService;
        private readonly ICurrencyService _currencyService;
        private readonly ICustomerService _customerService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IDiscountService _discountService;
        private readonly IDiscountSupportedModelFactory _discountSupportedModelFactory;
        private readonly ILocalizationService _localizationService;
        private readonly ILocalizedModelFactory _localizedModelFactory;
        private readonly IManufacturerService _manufacturerService;
        private readonly IMeasureService _measureService;
        private readonly IOrderService _orderService;
        private readonly IPictureService _pictureService;
        private readonly IProductAttributeFormatter _productAttributeFormatter;
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly IProductAttributeService _productAttributeService;
        private readonly IProductService _productService;
        private readonly IProductTagService _productTagService;
        private readonly IProductTemplateService _productTemplateService;
        private readonly ISettingModelFactory _settingModelFactory;
        private readonly IShipmentService _shipmentService;
        private readonly IShippingService _shippingService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly ISpecificationAttributeService _specificationAttributeService;
        private readonly IStoreMappingSupportedModelFactory _storeMappingSupportedModelFactory;
        private readonly IStoreService _storeService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IWorkContext _workContext;
        private readonly MeasureSettings _measureSettings;
        private readonly TaxSettings _taxSettings;
        private readonly VendorSettings _vendorSettings;
        private readonly IAlchubGeneralService _alchubGeneralService;
        private readonly AlchubSettings _alchubSettings;
        private readonly IPermissionService _permissionService;
        #endregion

        #region Ctor

        public ProductModelFactory(CatalogSettings catalogSettings,
            CurrencySettings currencySettings,
            IAclSupportedModelFactory aclSupportedModelFactory,
            IAddressService addressService,
            IBaseAdminModelFactory baseAdminModelFactory,
            ICategoryService categoryService,
            ICurrencyService currencyService,
            ICustomerService customerService,
            IDateTimeHelper dateTimeHelper,
            IDiscountService discountService,
            IDiscountSupportedModelFactory discountSupportedModelFactory,
            ILocalizationService localizationService,
            ILocalizedModelFactory localizedModelFactory,
            IManufacturerService manufacturerService,
            IMeasureService measureService,
            IOrderService orderService,
            IPictureService pictureService,
            IProductAttributeFormatter productAttributeFormatter,
            IProductAttributeParser productAttributeParser,
            IProductAttributeService productAttributeService,
            IProductService productService,
            IProductTagService productTagService,
            IProductTemplateService productTemplateService,
            ISettingModelFactory settingModelFactory,
            IShipmentService shipmentService,
            IShippingService shippingService,
            IShoppingCartService shoppingCartService,
            ISpecificationAttributeService specificationAttributeService,
            IStoreMappingSupportedModelFactory storeMappingSupportedModelFactory,
            IStoreService storeService,
            IUrlRecordService urlRecordService,
            IWorkContext workContext,
            MeasureSettings measureSettings,
            TaxSettings taxSettings,
            VendorSettings vendorSettings,
            IAlchubGeneralService alchubGeneralService,
            AlchubSettings alchubSettings,
            IPermissionService permissionService)
        {
            _catalogSettings = catalogSettings;
            _currencySettings = currencySettings;
            _aclSupportedModelFactory = aclSupportedModelFactory;
            _addressService = addressService;
            _baseAdminModelFactory = baseAdminModelFactory;
            _categoryService = categoryService;
            _currencyService = currencyService;
            _customerService = customerService;
            _dateTimeHelper = dateTimeHelper;
            _discountService = discountService;
            _discountSupportedModelFactory = discountSupportedModelFactory;
            _localizationService = localizationService;
            _localizedModelFactory = localizedModelFactory;
            _manufacturerService = manufacturerService;
            _measureService = measureService;
            _measureSettings = measureSettings;
            _orderService = orderService;
            _pictureService = pictureService;
            _productAttributeFormatter = productAttributeFormatter;
            _productAttributeParser = productAttributeParser;
            _productAttributeService = productAttributeService;
            _productService = productService;
            _productTagService = productTagService;
            _productTemplateService = productTemplateService;
            _settingModelFactory = settingModelFactory;
            _shipmentService = shipmentService;
            _shippingService = shippingService;
            _shoppingCartService = shoppingCartService;
            _specificationAttributeService = specificationAttributeService;
            _storeMappingSupportedModelFactory = storeMappingSupportedModelFactory;
            _storeService = storeService;
            _urlRecordService = urlRecordService;
            _workContext = workContext;
            _taxSettings = taxSettings;
            _vendorSettings = vendorSettings;
            _alchubGeneralService = alchubGeneralService;
            _alchubSettings = alchubSettings;
            _permissionService = permissionService;
        }

        #endregion

        #region Const
        private const int DEFAULT_PICTURE_SIZE = 75;
        #endregion

        #region Methods

        public virtual async Task<ProductSearchModel> PrepareProductSearchModelAsync(ProductSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //a vendor should have access only to his products
            searchModel.IsLoggedInAsVendor = await _workContext.GetCurrentVendorAsync() != null;
            searchModel.AllowVendorsToImportProducts = _vendorSettings.AllowVendorsToImportProducts;

            //prepare available categories
            await _baseAdminModelFactory.PrepareCategoriesAsync(searchModel.AvailableCategories);

            //prepare available manufacturers
            await _baseAdminModelFactory.PrepareManufacturersAsync(searchModel.AvailableManufacturers);

            //prepare available stores
            await _baseAdminModelFactory.PrepareStoresAsync(searchModel.AvailableStores);

            //prepare available vendors
            await _baseAdminModelFactory.PrepareVendorsAsync(searchModel.AvailableVendors);

            //prepare available product types
            await _baseAdminModelFactory.PrepareProductTypesAsync(searchModel.AvailableProductTypes);

            //prepare available warehouses
            await _baseAdminModelFactory.PrepareWarehousesAsync(searchModel.AvailableWarehouses);

            searchModel.HideStoresList = _catalogSettings.IgnoreStoreLimitations || searchModel.AvailableStores.SelectionIsNotPossible();

            //prepare "published" filter (0 - all; 1 - published only; 2 - unpublished only)
            searchModel.AvailablePublishedOptions.Add(new SelectListItem
            {
                Value = "0",
                Text = await _localizationService.GetResourceAsync("Admin.Catalog.Products.List.SearchPublished.All")
            });
            searchModel.AvailablePublishedOptions.Add(new SelectListItem
            {
                Value = "1",
                Text = await _localizationService.GetResourceAsync("Admin.Catalog.Products.List.SearchPublished.PublishedOnly")
            });
            searchModel.AvailablePublishedOptions.Add(new SelectListItem
            {
                Value = "2",
                Text = await _localizationService.GetResourceAsync("Admin.Catalog.Products.List.SearchPublished.UnpublishedOnly")
            });

            //prepare grid
            searchModel.SetGridPageSize();

            //prepare size
            //get setting value
            var sizesStr = _alchubSettings.ProductSizes;

            var productSizes = sizesStr.Split(",", StringSplitOptions.RemoveEmptyEntries)?.ToList() ?? new List<string>();
            var availbleSizes = new List<SelectListItem>
            {
                new SelectListItem { Text = "All", Value = "" }
            };

            foreach (var size in productSizes)
                availbleSizes.Add(new SelectListItem { Text = size, Value = size });

            searchModel.AvailableSize = availbleSizes;

            //bulk import/export access
            searchModel.AllowBulkImportExport = await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessMasterBulkImportExport);
            //vendor prodcut actions access
            searchModel.AllowVendorProductActions = await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessVendorProductActions);

            return searchModel;
        }

        /// <summary>
        /// Prepare paged product list model
        /// </summary>
        /// <param name="searchModel">Product search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the product list model
        /// </returns>
        public virtual async Task<ProductListModel> PrepareProductListModelAsync(ProductSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get parameters to filter comments
            var overridePublished = searchModel.SearchPublishedId == 0 ? null : (bool?)(searchModel.SearchPublishedId == 1);
            var currentVendor = await _workContext.GetCurrentVendorAsync();
            if (currentVendor != null)
            {
                searchModel.SearchVendorId = currentVendor.Id;
                //exclude grouped product for vendor.(20-12-22)
                searchModel.SearchProductTypeId = (int)ProductType.SimpleProduct;
            }
            var categoryIds = new List<int> { searchModel.SearchCategoryId };
            if (searchModel.SearchIncludeSubCategories && searchModel.SearchCategoryId > 0)
            {
                var childCategoryIds = await _categoryService.GetChildCategoryIdsAsync(parentCategoryId: searchModel.SearchCategoryId, showHidden: true);
                categoryIds.AddRange(childCategoryIds);
            }

            //get products
            var products = await _productService.SearchProductsAsync(showHidden: true,
                categoryIds: categoryIds,
                manufacturerIds: new List<int> { searchModel.SearchManufacturerId },
                storeId: searchModel.SearchStoreId,
                vendorId: searchModel.SearchVendorId,
                warehouseId: searchModel.SearchWarehouseId,
                productType: searchModel.SearchProductTypeId > 0 ? (ProductType?)searchModel.SearchProductTypeId : null,
                keywords: searchModel.SearchProductName,
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize,
                overridePublished: overridePublished,
                isMaster: false);

            //prepare list model
            var model = await new ProductListModel().PrepareToGridAsync(searchModel, products, () =>
            {
                return products.SelectAwait(async product =>
                {
                    //fill in model values from the entity
                    var productModel = product.ToModel<ProductModel>();

                    //little performance optimization: ensure that "FullDescription" is not returned
                    productModel.FullDescription = string.Empty;

                    //fill in additional values (not existing in the entity)
                    productModel.SeName = await _urlRecordService.GetSeNameAsync(product, 0, true, false);
                    //var defaultProductPicture = (await _pictureService.GetPicturesByProductIdAsync(product.Id, 1)).FirstOrDefault();

                    var sku = string.Empty;
                    //if group product then try to get picture by it's assosiated product sku.
                    if (product.ProductType == ProductType.GroupedProduct)
                    {
                        //associated products
                        var associatedProducts = (await _productService.GetAssociatedProductsAsync(product.Id))?.Where(gp => gp.ProductType == ProductType.SimpleProduct);

                        if (associatedProducts.Any())
                            sku = associatedProducts?.Select(ap => ap.Sku)?.FirstOrDefault();
                    }
                    else
                        sku = product.Sku;

                    //productModel.PictureThumbnailUrl = await _pictureService.GetProductPictureUrlAsync(sku, DEFAULT_PICTURE_SIZE);
                    productModel.PictureThumbnailUrl = product.ImageUrl;
                    productModel.ProductTypeName = await _localizationService.GetLocalizedEnumAsync(product.ProductType);
                    if (product.ProductType == ProductType.SimpleProduct && product.ManageInventoryMethod == ManageInventoryMethod.ManageStock)
                        productModel.StockQuantityStr = (await _productService.GetTotalStockQuantityAsync(product)).ToString();

                    productModel.UPCCode = product.UPCCode != null ? product.UPCCode : "";
                    productModel.Size = product.Size;
                    productModel.OverrideNegativeStock = product.OverrideNegativeStock;
                    productModel.OverridePrice = product.OverridePrice;
                    productModel.OverrideStock = product.OverrideStock;

                    //set Size & Container of master product (To vendor) 05-07-23
                    if (product.VendorId > 0)
                    {
                        var masterProduct = await _alchubGeneralService.GetMasterProductByUpcCodeAsync(product.UPCCode);
                        if (masterProduct != null && !masterProduct.Deleted)
                        {
                            productModel.UPCCode = masterProduct?.UPCCode;
                            productModel.Size = masterProduct?.Size;
                        }
                    }

                    return productModel;
                });
            });

            return model;
        }

        /// <summary>
        /// Prepare paged product list model
        /// </summary>
        /// <param name="searchModel">Product search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the product list model
        /// </returns>
        public virtual async Task<ProductListModel> PrepareMasterProductListModelAsync(ProductSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get parameters to filter comments
            var overridePublished = searchModel.SearchPublishedId == 0 ? null : (bool?)(searchModel.SearchPublishedId == 1);
            var currentVendor = await _workContext.GetCurrentVendorAsync();
            if (currentVendor != null)
                searchModel.SearchVendorId = currentVendor.Id;
            var categoryIds = new List<int> { searchModel.SearchCategoryId };
            if (searchModel.SearchIncludeSubCategories && searchModel.SearchCategoryId > 0)
            {
                var childCategoryIds = await _categoryService.GetChildCategoryIdsAsync(parentCategoryId: searchModel.SearchCategoryId, showHidden: true);
                categoryIds.AddRange(childCategoryIds);
            }

            //get products
            var products = await _productService.SearchProductsAsync(showHidden: true,
                categoryIds: categoryIds,
                manufacturerIds: new List<int> { searchModel.SearchManufacturerId },
                storeId: searchModel.SearchStoreId,
                vendorId: searchModel.SearchVendorId,
                warehouseId: searchModel.SearchWarehouseId,
                productType: searchModel.SearchProductTypeId > 0 ? (ProductType?)searchModel.SearchProductTypeId : null,
                keywords: searchModel.SearchProductName,
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize,
                overridePublished: overridePublished,
                isMaster: true,
                upccode: searchModel.SearchUPCCode,
                size: searchModel.SearchSize,
                orderBy: ProductSortingEnum.NameAsc);

            //prepare list model
            var model = await new ProductListModel().PrepareToGridAsync(searchModel, products, () =>
            {
                return products.SelectAwait(async product =>
                {
                    //fill in model values from the entity
                    var productModel = product.ToModel<ProductModel>();

                    //little performance optimization: ensure that "FullDescription" is not returned
                    productModel.FullDescription = string.Empty;

                    //fill in additional values (not existing in the entity)
                    productModel.SeName = await _urlRecordService.GetSeNameAsync(product, 0, true, false);
                    var defaultProductPicture = (await _pictureService.GetPicturesByProductIdAsync(product.Id, 1)).FirstOrDefault();

                    var sku = string.Empty;
                    //if group product then try to get picture by it's assosiated product sku.
                    if (product.ProductType == ProductType.GroupedProduct)
                    {
                        //associated products
                        var associatedProducts = (await _productService.GetAssociatedProductsAsync(product.Id))?.Where(gp => gp.ProductType == ProductType.SimpleProduct);

                        if (associatedProducts.Any())
                            sku = associatedProducts?.Select(ap => ap.Sku)?.FirstOrDefault();
                    }
                    else
                        sku = product.Sku;

                    //(productModel.PictureThumbnailUrl, _) = await _pictureService.GetPictureUrlAsync(defaultProductPicture, 75);
                    //productModel.PictureThumbnailUrl = await _pictureService.GetProductPictureUrlAsync(sku, DEFAULT_PICTURE_SIZE);
                    productModel.PictureThumbnailUrl = product.ImageUrl;
                    productModel.ProductTypeName = await _localizationService.GetLocalizedEnumAsync(product.ProductType);
                    if (product.ProductType == ProductType.SimpleProduct && product.ManageInventoryMethod == ManageInventoryMethod.ManageStock)
                        productModel.StockQuantityStr = string.Empty;//05-06-23 - master product will not have quantity
                                                                     //productModel.StockQuantityStr = (await _productService.GetTotalStockQuantityAsync(product)).ToString();

                    productModel.UPCCode = product.UPCCode;

                    productModel.Size = product.Size;

                    //display product average price from subproducts price -- 09/05/23
                    productModel.Price = await _productService.GetProductAveragePriceByUPCCode(product.UPCCode);

                    return productModel;
                });
            });

            return model;
        }

        /// <summary>
        /// Prepare product model
        /// </summary>
        /// <param name="model">Product model</param>
        /// <param name="product">Product</param>
        /// <param name="excludeProperties">Whether to exclude populating of some properties of model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the product model
        /// </returns>
        public virtual async Task<ProductModel> PrepareProductModelAsync(ProductModel model, Product product, bool excludeProperties = false)
        {
            Func<ProductLocalizedModel, int, Task> localizedModelConfiguration = null;

            if (product != null)
            {
                //fill in model values from the entity
                if (model == null)
                {
                    model = product.ToModel<ProductModel>();
                    model.SeName = await _urlRecordService.GetSeNameAsync(product, 0, true, false);
                }

                var parentGroupedProduct = await _productService.GetProductByIdAsync(product.ParentGroupedProductId);
                if (parentGroupedProduct != null)
                {
                    model.AssociatedToProductId = product.ParentGroupedProductId;
                    model.AssociatedToProductName = parentGroupedProduct.Name;
                }

                model.LastStockQuantity = product.StockQuantity;
                model.ProductTags = string.Join(", ", (await _productTagService.GetAllProductTagsByProductIdAsync(product.Id)).Select(tag => tag.Name));
                model.ProductAttributesExist = (await _productAttributeService.GetAllProductAttributesAsync()).Any();

                model.CanCreateCombinations = await (await _productAttributeService
                    .GetProductAttributeMappingsByProductIdAsync(product.Id)).AnyAwaitAsync(async pam => (await _productAttributeService.GetProductAttributeValuesAsync(pam.Id)).Any());

                if (!excludeProperties)
                {
                    model.SelectedCategoryIds = (await _categoryService.GetProductCategoriesByProductIdAsync(product.Id, true))
                        .Select(productCategory => productCategory.CategoryId).ToList();
                    model.SelectedManufacturerIds = (await _manufacturerService.GetProductManufacturersByProductIdAsync(product.Id, true))
                        .Select(productManufacturer => productManufacturer.ManufacturerId).ToList();
                }

                //prepare copy product model
                await PrepareCopyProductModelAsync(model.CopyProductModel, product);

                //prepare nested search model
                PrepareRelatedProductSearchModel(model.RelatedProductSearchModel, product);
                PrepareCrossSellProductSearchModel(model.CrossSellProductSearchModel, product);
                PrepareAssociatedProductSearchModel(model.AssociatedProductSearchModel, product);
                PrepareProductPictureSearchModel(model.ProductPictureSearchModel, product);
                PrepareProductSpecificationAttributeSearchModel(model.ProductSpecificationAttributeSearchModel, product);
                PrepareProductOrderSearchModel(model.ProductOrderSearchModel, product);
                PrepareTierPriceSearchModel(model.TierPriceSearchModel, product);
                await PrepareStockQuantityHistorySearchModelAsync(model.StockQuantityHistorySearchModel, product);
                PrepareProductAttributeMappingSearchModel(model.ProductAttributeMappingSearchModel, product);
                PrepareProductAttributeCombinationSearchModel(model.ProductAttributeCombinationSearchModel, product);

                //define localized model configuration action
                localizedModelConfiguration = async (locale, languageId) =>
                {
                    locale.Name = await _localizationService.GetLocalizedAsync(product, entity => entity.Name, languageId, false, false);
                    locale.FullDescription = await _localizationService.GetLocalizedAsync(product, entity => entity.FullDescription, languageId, false, false);
                    locale.ShortDescription = await _localizationService.GetLocalizedAsync(product, entity => entity.ShortDescription, languageId, false, false);
                    locale.MetaKeywords = await _localizationService.GetLocalizedAsync(product, entity => entity.MetaKeywords, languageId, false, false);
                    locale.MetaDescription = await _localizationService.GetLocalizedAsync(product, entity => entity.MetaDescription, languageId, false, false);
                    locale.MetaTitle = await _localizationService.GetLocalizedAsync(product, entity => entity.MetaTitle, languageId, false, false);
                    locale.SeName = await _urlRecordService.GetSeNameAsync(product, languageId, false, false);
                };

                model.UPCCode = product.UPCCode;
            }

            //set default values for the new model
            if (product == null)
            {
                model.MaximumCustomerEnteredPrice = 1000;
                model.MaxNumberOfDownloads = 10;
                model.RecurringCycleLength = 100;
                model.RecurringTotalCycles = 10;
                model.RentalPriceLength = 1;
                model.StockQuantity = 10000;
                model.NotifyAdminForQuantityBelow = 1;
                model.OrderMinimumQuantity = 1;
                model.OrderMaximumQuantity = 10000;
                model.TaxCategoryId = _taxSettings.DefaultTaxCategoryId;
                model.UnlimitedDownloads = true;
                model.IsShipEnabled = true;
                model.AllowCustomerReviews = true;
                model.Published = true;
                model.VisibleIndividually = true;
            }

            model.PrimaryStoreCurrencyCode = (await _currencyService.GetCurrencyByIdAsync(_currencySettings.PrimaryStoreCurrencyId)).CurrencyCode;
            model.BaseWeightIn = (await _measureService.GetMeasureWeightByIdAsync(_measureSettings.BaseWeightId)).Name;
            model.BaseDimensionIn = (await _measureService.GetMeasureDimensionByIdAsync(_measureSettings.BaseDimensionId)).Name;
            model.IsLoggedInAsVendor = await _workContext.GetCurrentVendorAsync() != null;
            model.HasAvailableSpecificationAttributes =
                (await _specificationAttributeService.GetSpecificationAttributesWithOptionsAsync()).Any();

            //prepare localized models
            if (!excludeProperties)
                model.Locales = await _localizedModelFactory.PrepareLocalizedModelsAsync(localizedModelConfiguration);

            //prepare editor settings
            model.ProductEditorSettingsModel = await _settingModelFactory.PrepareProductEditorSettingsModelAsync();

            //prepare available product templates
            await _baseAdminModelFactory.PrepareProductTemplatesAsync(model.AvailableProductTemplates, false);

            //prepare available product types
            var productTemplates = await _productTemplateService.GetAllProductTemplatesAsync();
            foreach (var productType in Enum.GetValues(typeof(ProductType)).OfType<ProductType>())
            {
                model.ProductsTypesSupportedByProductTemplates.Add((int)productType, new List<SelectListItem>());
                foreach (var template in productTemplates)
                {
                    var list = (IList<int>)TypeDescriptor.GetConverter(typeof(List<int>)).ConvertFrom(template.IgnoredProductTypes) ?? new List<int>();
                    if (string.IsNullOrEmpty(template.IgnoredProductTypes) || !list.Contains((int)productType))
                    {
                        model.ProductsTypesSupportedByProductTemplates[(int)productType].Add(new SelectListItem
                        {
                            Text = template.Name,
                            Value = template.Id.ToString()
                        });
                    }
                }
            }

            //prepare available delivery dates
            await _baseAdminModelFactory.PrepareDeliveryDatesAsync(model.AvailableDeliveryDates,
                defaultItemText: await _localizationService.GetResourceAsync("Admin.Catalog.Products.Fields.DeliveryDate.None"));

            //prepare available product availability ranges
            await _baseAdminModelFactory.PrepareProductAvailabilityRangesAsync(model.AvailableProductAvailabilityRanges,
                defaultItemText: await _localizationService.GetResourceAsync("Admin.Catalog.Products.Fields.ProductAvailabilityRange.None"));

            //prepare available vendors
            await _baseAdminModelFactory.PrepareVendorsAsync(model.AvailableVendors,
                defaultItemText: await _localizationService.GetResourceAsync("Admin.Catalog.Products.Fields.Vendor.None"));

            //prepare available tax categories
            await _baseAdminModelFactory.PrepareTaxCategoriesAsync(model.AvailableTaxCategories);

            //prepare available warehouses
            await _baseAdminModelFactory.PrepareWarehousesAsync(model.AvailableWarehouses,
                defaultItemText: await _localizationService.GetResourceAsync("Admin.Catalog.Products.Fields.Warehouse.None"));
            await PrepareProductWarehouseInventoryModelsAsync(model.ProductWarehouseInventoryModels, product);

            //prepare available base price units
            var availableMeasureWeights = (await _measureService.GetAllMeasureWeightsAsync())
                .Select(weight => new SelectListItem { Text = weight.Name, Value = weight.Id.ToString() }).ToList();
            model.AvailableBasepriceUnits = availableMeasureWeights;
            model.AvailableBasepriceBaseUnits = availableMeasureWeights;

            //prepare model categories
            await _baseAdminModelFactory.PrepareCategoriesAsync(model.AvailableCategories, false);
            foreach (var categoryItem in model.AvailableCategories)
            {
                categoryItem.Selected = int.TryParse(categoryItem.Value, out var categoryId)
                    && model.SelectedCategoryIds.Contains(categoryId);
            }

            //prepare model manufacturers
            await _baseAdminModelFactory.PrepareManufacturersAsync(model.AvailableManufacturers, false);
            foreach (var manufacturerItem in model.AvailableManufacturers)
            {
                manufacturerItem.Selected = int.TryParse(manufacturerItem.Value, out var manufacturerId)
                    && model.SelectedManufacturerIds.Contains(manufacturerId);
            }

            //prepare model discounts
            var availableDiscounts = await _discountService.GetAllDiscountsAsync(DiscountType.AssignedToSkus, showHidden: true);
            await _discountSupportedModelFactory.PrepareModelDiscountsAsync(model, product, availableDiscounts, excludeProperties);

            //prepare model customer roles
            await _aclSupportedModelFactory.PrepareModelCustomerRolesAsync(model, product, excludeProperties);

            //prepare model stores
            await _storeMappingSupportedModelFactory.PrepareModelStoresAsync(model, product, excludeProperties);

            var productTags = await _productTagService.GetAllProductTagsAsync();
            var productTagsSb = new StringBuilder();
            productTagsSb.Append("var initialProductTags = [");
            for (var i = 0; i < productTags.Count; i++)
            {
                var tag = productTags[i];
                productTagsSb.Append('\'');
                productTagsSb.Append(JavaScriptEncoder.Default.Encode(tag.Name));
                productTagsSb.Append('\'');
                if (i != productTags.Count - 1)
                    productTagsSb.Append(',');
            }
            productTagsSb.Append(']');

            model.InitialProductTags = productTagsSb.ToString();


            var availableManageInventoryItems = await ManageInventoryMethod.ManageStock.ToSelectListAsync(false);
            foreach (var item in availableManageInventoryItems)
            {
                if (item.Text == "Track inventory")
                    model.AvailableManageInventory.Add(item);
            }

            //prepare size
            var productSizes = await _alchubGeneralService.GetProductSizesAsync();
            var availbleSizes = new List<SelectListItem>();
            foreach (var size in productSizes)
                availbleSizes.Add(new SelectListItem { Text = size, Value = size });
            model.AvailableSize = availbleSizes;

            return model;
        }

        /// <summary>
        /// Prepare paged associated product list model to add to the product
        /// </summary>
        /// <param name="searchModel">Associated product search model to add to the product</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the associated product list model to add to the product
        /// </returns>
        public virtual async Task<AddAssociatedProductListModel> PrepareAddAssociatedProductListModelAsync(AddAssociatedProductSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //a vendor should have access only to his products
            var currentVendor = await _workContext.GetCurrentVendorAsync();
            if (currentVendor != null)
                searchModel.SearchVendorId = currentVendor.Id;

            //get products
            var products = await _productService.SearchProductsAsync(showHidden: true,
                categoryIds: new List<int> { searchModel.SearchCategoryId },
                manufacturerIds: new List<int> { searchModel.SearchManufacturerId },
                storeId: searchModel.SearchStoreId,
                vendorId: searchModel.SearchVendorId,
                productType: searchModel.SearchProductTypeId > 0 ? (ProductType?)searchModel.SearchProductTypeId : null,
                keywords: searchModel.SearchProductName,
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize,
                //++alhcub custom filter
                isMaster: true);

            //prepare grid model
            var model = await new AddAssociatedProductListModel().PrepareToGridAsync(searchModel, products, () =>
            {
                return products.SelectAwait(async product =>
                {
                    //fill in model values from the entity
                    var productModel = product.ToModel<ProductModel>();

                    //fill in additional values (not existing in the entity)
                    productModel.SeName = await _urlRecordService.GetSeNameAsync(product, 0, true, false);
                    var parentGroupedProduct = await _productService.GetProductByIdAsync(product.ParentGroupedProductId);

                    if (parentGroupedProduct == null)
                        return productModel;

                    productModel.AssociatedToProductId = product.ParentGroupedProductId;
                    productModel.AssociatedToProductName = parentGroupedProduct.Name;

                    return productModel;
                });
            });

            return model;
        }

        /// <summary>
        /// Prepare associated product model
        /// </summary>
        /// <param name="associatedProduct">Product</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the associated product model
        /// </returns>
        public virtual async Task<AssociatedProductModel> PrepareAssociatedProductModelAsync(Product associatedProduct)
        {
            if (associatedProduct == null)
                throw new ArgumentNullException(nameof(associatedProduct));

            var associatedProductModel = associatedProduct.ToModel<AssociatedProductModel>();
            associatedProductModel.ProductName = associatedProduct.Name;
            associatedProductModel.BaseProductId = associatedProduct.ParentGroupedProductId;

            //prepare size
            //var productSizes = await _alchubGeneralService.GetProductSizesAsync(associatedProduct);
            var productSizes = await _alchubGeneralService.GetProductSizesAsync();
            var availbleSizes = new List<SelectListItem>();
            foreach (var size in productSizes)
                availbleSizes.Add(new SelectListItem { Text = size, Value = size });

            //prepare container
            var productContainers = await _alchubGeneralService.GetProductContainersAsync(associatedProduct);
            var availbleContainers = new List<SelectListItem>();
            foreach (var container in productContainers)
                availbleContainers.Add(new SelectListItem { Text = container, Value = container });

            associatedProductModel.AvailableSize = availbleSizes;
            associatedProductModel.AvailableContainer = availbleContainers;

            return associatedProductModel;
        }


        /// <summary>
        /// Prepare paged product list model
        /// </summary>
        /// <param name="searchModel">Product search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the product list model
        /// </returns>
        public virtual async Task<ProductListModel> PrepareVendorMasterProductListModelAsync(ProductSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get parameters to filter comments
            var overridePublished = searchModel.SearchPublishedId == 0 ? null : (bool?)(searchModel.SearchPublishedId == 1);
            var currentVendor = await _workContext.GetCurrentVendorAsync();
            if (currentVendor != null)
                searchModel.SearchVendorId = currentVendor.Id;
            var categoryIds = new List<int> { searchModel.SearchCategoryId };
            if (searchModel.SearchIncludeSubCategories && searchModel.SearchCategoryId > 0)
            {
                var childCategoryIds = await _categoryService.GetChildCategoryIdsAsync(parentCategoryId: searchModel.SearchCategoryId, showHidden: true);
                categoryIds.AddRange(childCategoryIds);
            }

            //get products
            var products = await _productService.SearchProductsAsync(showHidden: true,
                categoryIds: categoryIds,
                manufacturerIds: new List<int> { searchModel.SearchManufacturerId },
                storeId: searchModel.SearchStoreId,
                vendorId: searchModel.SearchVendorId,
                warehouseId: searchModel.SearchWarehouseId,
                productType: searchModel.SearchProductTypeId > 0 ? (ProductType?)searchModel.SearchProductTypeId : null,
                keywords: searchModel.SearchProductName,
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize,
                overridePublished: overridePublished,
                isMaster: true,
                upccode: searchModel.SearchUPCCode,
                size: searchModel.SearchSize);

            //prepare list model
            var model = await new ProductListModel().PrepareToGridAsync(searchModel, products, () =>
            {
                return products.SelectAwait(async product =>
                {
                    //fill in model values from the entity
                    var productModel = product.ToModel<ProductModel>();

                    //little performance optimization: ensure that "FullDescription" is not returned
                    productModel.FullDescription = string.Empty;

                    //fill in additional values (not existing in the entity)
                    productModel.SeName = await _urlRecordService.GetSeNameAsync(product, 0, true, false);
                    var defaultProductPicture = (await _pictureService.GetPicturesByProductIdAsync(product.Id, 1)).FirstOrDefault();

                    var sku = string.Empty;
                    //if group product then try to get picture by it's assosiated product sku.
                    if (product.ProductType == ProductType.GroupedProduct)
                    {
                        //associated products
                        var associatedProducts = (await _productService.GetAssociatedProductsAsync(product.Id))?.Where(gp => gp.ProductType == ProductType.SimpleProduct);

                        if (associatedProducts.Any())
                            sku = associatedProducts?.Select(ap => ap.Sku)?.FirstOrDefault();
                    }
                    else
                        sku = product.Sku;

                    //productModel.PictureThumbnailUrl = await _pictureService.GetProductPictureUrlAsync(sku, DEFAULT_PICTURE_SIZE);
                    productModel.PictureThumbnailUrl = product.ImageUrl;
                    productModel.ProductTypeName = await _localizationService.GetLocalizedEnumAsync(product.ProductType);
                    if (product.ProductType == ProductType.SimpleProduct && product.ManageInventoryMethod == ManageInventoryMethod.ManageStock)
                        productModel.StockQuantityStr = (await _productService.GetTotalStockQuantityAsync(product)).ToString();

                    productModel.UPCCode = product.UPCCode;
                    var categoryName = "";
                    var productCategories = (await _categoryService.GetProductCategoriesByProductIdAsync(product.Id)).FirstOrDefault();
                    if (productCategories != null)
                    {
                        categoryName = (await _categoryService.GetCategoryByIdAsync((int)productCategories.CategoryId)).Name;
                    }
                    productModel.Category = categoryName != null ? categoryName : "";
                    productModel.Size = product.Size;

                    return productModel;
                });
            });

            return model;
        }



        /// <summary>
        /// Prepare paged product list model
        /// </summary>
        /// <param name="searchModel">Product search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the product list model
        /// </returns>
        public virtual async Task<ProductListModel> PrepareVendorForMasterProductListModelAsync(ProductSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get parameters to filter comments
            var overridePublished = searchModel.SearchPublishedId == 0 ? null : (bool?)(searchModel.SearchPublishedId == 1);
            var currentVendor = await _workContext.GetCurrentVendorAsync();
            if (currentVendor != null)
                searchModel.SearchVendorId = 0;
            var categoryIds = new List<int> { searchModel.SearchCategoryId };
            if (searchModel.SearchIncludeSubCategories && searchModel.SearchCategoryId > 0)
            {
                var childCategoryIds = await _categoryService.GetChildCategoryIdsAsync(parentCategoryId: searchModel.SearchCategoryId, showHidden: true);
                categoryIds.AddRange(childCategoryIds);
            }

            //get products
            var products = await _productService.SearchProductsAsync(showHidden: true,
                categoryIds: categoryIds,
                manufacturerIds: new List<int> { searchModel.SearchManufacturerId },
                storeId: searchModel.SearchStoreId,
                vendorId: searchModel.SearchVendorId,
                warehouseId: searchModel.SearchWarehouseId,
                productType: ProductType.SimpleProduct, //exclude grouped product (20-12-22)
                keywords: searchModel.SearchProductName,
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize,
                overridePublished: overridePublished,
                isMaster: true,
                upccode: searchModel.SearchUPCCode,
                size: searchModel.SearchSize);

            //prepare list model
            var model = await new ProductListModel().PrepareToGridAsync(searchModel, products, () =>
            {
                return products.SelectAwait(async product =>
                {
                    //fill in model values from the entity
                    var productModel = product.ToModel<ProductModel>();

                    //little performance optimization: ensure that "FullDescription" is not returned
                    productModel.FullDescription = string.Empty;

                    //fill in additional values (not existing in the entity)
                    productModel.SeName = await _urlRecordService.GetSeNameAsync(product, 0, true, false);
                    var defaultProductPicture = (await _pictureService.GetPicturesByProductIdAsync(product.Id, 1)).FirstOrDefault();

                    //(productModel.PictureThumbnailUrl, _) = await _pictureService.GetPictureUrlAsync(defaultProductPicture, 75);

                    var sku = string.Empty;
                    //if group product then try to get picture by it's assosiated product sku.
                    if (product.ProductType == ProductType.GroupedProduct)
                    {
                        //associated products
                        var associatedProducts = (await _productService.GetAssociatedProductsAsync(product.Id))?.Where(gp => gp.ProductType == ProductType.SimpleProduct);

                        if (associatedProducts.Any())
                            sku = associatedProducts?.Select(ap => ap.Sku)?.FirstOrDefault();
                    }
                    else
                        sku = product.Sku;

                    //productModel.PictureThumbnailUrl = await _pictureService.GetProductPictureUrlAsync(sku, DEFAULT_PICTURE_SIZE);
                    productModel.PictureThumbnailUrl = product.ImageUrl;
                    productModel.ProductTypeName = await _localizationService.GetLocalizedEnumAsync(product.ProductType);
                    if (product.ProductType == ProductType.SimpleProduct && product.ManageInventoryMethod == ManageInventoryMethod.ManageStock)
                        productModel.StockQuantityStr = (await _productService.GetTotalStockQuantityAsync(product)).ToString();

                    productModel.UPCCode = product.UPCCode;
                    var categoryName = "";
                    var productCategories = (await _categoryService.GetProductCategoriesByProductIdAsync(product.Id)).FirstOrDefault();
                    if (productCategories != null)
                    {
                        categoryName = (await _categoryService.GetCategoryByIdAsync((int)productCategories.CategoryId)).Name;
                    }
                    productModel.Category = categoryName != null ? categoryName : "";
                    productModel.Size = product.Size;

                    return productModel;
                });
            });

            return model;
        }
        #endregion
    }
}