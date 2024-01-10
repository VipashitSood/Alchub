using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Nop.Core;
using Nop.Core.Alchub.Domain;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.DeliveryFees;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Stores;
using Nop.Core.Domain.Tax;
using Nop.Core.Domain.TipFees;
using Nop.Core.Domain.Topics;
using Nop.Data;
using Nop.Plugin.Api.Domain;
using Nop.Plugin.Api.DTO;
using Nop.Plugin.Api.DTO.Categories;
using Nop.Plugin.Api.DTO.Checkout;
using Nop.Plugin.Api.DTO.HomeScreen;
using Nop.Plugin.Api.DTO.Images;
using Nop.Plugin.Api.DTO.Languages;
using Nop.Plugin.Api.DTO.Manufacturers;
using Nop.Plugin.Api.DTO.OrderItems;
using Nop.Plugin.Api.DTO.Orders;
using Nop.Plugin.Api.DTO.ProductAttributes;
using Nop.Plugin.Api.DTO.Products;
using Nop.Plugin.Api.DTO.ShoppingCarts;
using Nop.Plugin.Api.DTO.SpecificationAttributes;
using Nop.Plugin.Api.DTO.Stores;
using Nop.Plugin.Api.DTOs.Categories;
using Nop.Plugin.Api.DTOs.HomeScreen.V1;
using Nop.Plugin.Api.DTOs.JCarousel;
using Nop.Plugin.Api.DTOs.Orders;
using Nop.Plugin.Api.DTOs.Products;
using Nop.Plugin.Api.DTOs.ShoppingCarts;
using Nop.Plugin.Api.DTOs.Topics;
using Nop.Plugin.Api.Factories;
using Nop.Plugin.Api.MappingExtensions;
using Nop.Plugin.Api.Models.OrdersParameters;
using Nop.Plugin.Api.Models.ProductsParameters;
using Nop.Plugin.Api.Models.Slots;
using Nop.Plugin.Api.Services;
using Nop.Plugin.Widgets.JCarousel;
using Nop.Plugin.Widgets.JCarousel.Domain;
using Nop.Plugin.Widgets.JCarousel.Factories;
using Nop.Plugin.Widgets.JCarousel.Services;
using Nop.Services.Alchub.ElasticSearch;
using Nop.Services.Alchub.General;
using Nop.Services.Alchub.ServiceFee;
using Nop.Services.Alchub.Slots;
using Nop.Services.Authentication;
using Nop.Services.Catalog;
using Nop.Services.Cms;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.DeliveryFees;
using Nop.Services.Directory;
using Nop.Services.Discounts;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Shipping;
using Nop.Services.Slots;
using Nop.Services.Stores;
using Nop.Services.Tax;
using Nop.Services.TipFees;
using Nop.Services.Vendors;

namespace Nop.Plugin.Api.Helpers
{
    public class DTOHelper : IDTOHelper
    {
        private readonly IAclService _aclService;
        private readonly IOrderItemRefundService _orderItemRefundService;
        private readonly ICustomerService _customerService;
        private readonly ILanguageService _languageService;
        private readonly ILocalizationService _localizationService;
        private readonly IPictureService _pictureService;
        private readonly IProductAttributeService _productAttributeService;
        private readonly IProductService _productService;
        private readonly IProductTagService _productTagService;
        private readonly IDiscountService _discountService;
        private readonly IManufacturerService _manufacturerService;
        private readonly IOrderService _orderService;
        private readonly IProductAttributeConverter _productAttributeConverter;
        private readonly IAddressService _addressService;
        private readonly IAuthenticationService _authenticationService;
        private readonly ICustomerApiService _customerApiService;
        private readonly ICurrencyService _currencyService;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IStoreService _storeService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IStaticCacheManager _staticCacheManager;
        private readonly Lazy<Task<Language>> _customerLanguage;
        private readonly IWebHelper _webHelper;
        private readonly ISettingService _settingService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IRepository<ShoppingCartItem> _sciRepository;
        private readonly ShoppingCartSettings _shoppingCartSettings;
        private readonly MediaSettings _mediaSettings;
        private readonly IWorkContext _workContext;
        private readonly ITaxService _taxService;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly OrderSettings _orderSettings;
        private readonly IUrlHelperFactory _urlHelperFactory;
        private readonly IActionContextAccessor _actionContextAccessor;
        private readonly TaxSettings _taxSettings;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly IGiftCardService _giftCardService;
        private readonly RewardPointsSettings _rewardPointsSettings;
        private readonly IRewardPointService _rewardPointService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IShippingService _shippingService;
        private readonly ShippingSettings _shippingSettings;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IPaymentPluginManager _paymentPluginManager;
        private readonly IPaymentService _paymentService;
        private readonly IReturnRequestService _returnRequestService;
        private readonly IJCarouselService _jCarouselService;
        private readonly IPublicJCarouselModelFactory _publicJCarouselModelFactory;
        private readonly IWidgetPluginManager _widgetPluginManager;
        private readonly IProductModelFactory _apiProductModelFactory;
        private readonly IStoreContext _storeContext;
        private readonly IProductApiService _productApiService;
        private readonly CatalogSettings _catalogSettings;
        private readonly ISpecificationAttributeService _specificationAttributeService;
        private readonly IWebHostEnvironment _env;
        private readonly IAlchubGeneralService _alchubGeneralService;
        private readonly ISpecificationAttributeApiService _specificationAttributeApiService;
        private readonly IProductAttributeFormatter _productAttributeFormatter;
        private readonly IVendorService _vendorService;
        private readonly IDeliveryFeeService _deliveryFeeService;
        private readonly ITipFeeService _tipFeeService;
        private readonly ISlotService _slotService;
        private readonly IServiceFeeManager _serviceFeeManager;
        private readonly ICategoryApiService _categoryApiService;
        private readonly CurrencySettings _currencySettings;
        private readonly ICategoryService _categoryService;
        private readonly IManufacturerApiService _manufacturerApiService;
        private readonly AlchubSettings _alchubSettings;
        private readonly IElasticsearchManagerService _elasticsearchManager;
        private readonly IAlchubSameNameProductService _alchubSameNameProductService;
        private readonly IOrderDispatchService _orderDispatchService;

        public DTOHelper(IStaticCacheManager staticCacheManager,
            IOrderItemRefundService orderItemRefundService,
            IProductService productService,
            IAclService aclService,
            IStoreMappingService storeMappingService,
            IPictureService pictureService,
            IProductAttributeService productAttributeService,
            ICustomerService customerService,
            IWebHostEnvironment env,
            ILanguageService languageService,
            IStoreService storeService,
            ILocalizationService localizationService,
            IUrlRecordService urlRecordService,
            IProductTagService productTagService,
            IDiscountService discountService,
            IManufacturerService manufacturerService,
            IOrderService orderService,
            IProductAttributeConverter productAttributeConverter,
            IAddressService addressService,
            IAuthenticationService authenticationService,
            ICustomerApiService customerApiService,
            ICurrencyService currencyService,
            IWebHelper webHelper,
            IStoreContext storeContext,
            IProductApiService productApiService,
            ISettingService settingService,
            IShoppingCartService shoppingCartService,
            IRepository<ShoppingCartItem> sciRepository,
            ShoppingCartSettings shoppingCartSettings,
            MediaSettings mediaSettings,
            IWorkContext workContext,
            ITaxService taxService,
            IPriceCalculationService priceCalculationService,
            IPriceFormatter priceFormatter,
            IOrderProcessingService orderProcessingService,
            OrderSettings orderSettings,
            IUrlHelperFactory urlHelperFactory,
            IActionContextAccessor actionContextAccessor,
            TaxSettings taxSettings,
            IOrderTotalCalculationService orderTotalCalculationService,
            IGiftCardService giftCardService,
            RewardPointsSettings rewardPointsSettings,
            IRewardPointService rewardPointService,
            IDateTimeHelper dateTimeHelper,
            IShippingService shippingService,
            ShippingSettings shippingSettings,
            IGenericAttributeService genericAttributeService,
            IPaymentPluginManager paymentPluginManager,
            IPaymentService paymentService,
            ISpecificationAttributeService specificationAttributeService,
            CatalogSettings catalogSettings,
            IReturnRequestService returnRequestService,
            IAlchubGeneralService alchubGeneralService,
            Widgets.JCarousel.Services.IJCarouselService jCarouselService,
            Widgets.JCarousel.Factories.IPublicJCarouselModelFactory publicJCarouselModelFactory,
            ISpecificationAttributeApiService specificationAttributeApiService,
            IProductAttributeFormatter productAttributeFormatter,
            IVendorService vendorService,
            IDeliveryFeeService deliveryFeeService,
            ITipFeeService tipFeeService,
            ISlotService slotService,
            IServiceFeeManager serviceFeeManager,
            Nop.Services.Cms.IWidgetPluginManager widgetPluginManager,
            Nop.Plugin.Api.Factories.IProductModelFactory apiProductModelFactory,
            ICategoryApiService categoryApiService,
            CurrencySettings currencySettings,
            ICategoryService categoryService,
            IManufacturerApiService manufacturerApiService,
            AlchubSettings alchubSettings,
            IElasticsearchManagerService elasticsearchManager,
            IAlchubSameNameProductService alchubSameNameProductService,
            IOrderDispatchService orderDispatchService)
        {
            _productService = productService;
            _orderItemRefundService = orderItemRefundService;
            _aclService = aclService;
            _storeMappingService = storeMappingService;
            _pictureService = pictureService;
            _productAttributeService = productAttributeService;
            _customerService = customerService;
            _languageService = languageService;
            _storeService = storeService;
            _localizationService = localizationService;
            _urlRecordService = urlRecordService;
            _productTagService = productTagService;
            _specificationAttributeService = specificationAttributeService;
            _discountService = discountService;
            _manufacturerService = manufacturerService;
            _orderService = orderService;
            _productAttributeConverter = productAttributeConverter;
            _addressService = addressService;
            _authenticationService = authenticationService;
            _productApiService = productApiService;
            _customerApiService = customerApiService;
            _currencyService = currencyService;
            _staticCacheManager = staticCacheManager;
            _customerLanguage = new Lazy<Task<Language>>(GetAuthenticatedCustomerLanguage);
            _webHelper = webHelper;
            _storeContext = storeContext;
            _settingService = settingService;
            _shoppingCartService = shoppingCartService;
            _sciRepository = sciRepository;
            _shoppingCartSettings = shoppingCartSettings;
            _mediaSettings = mediaSettings;
            _workContext = workContext;
            _taxService = taxService;
            _priceCalculationService = priceCalculationService;
            _priceFormatter = priceFormatter;
            _orderProcessingService = orderProcessingService;
            _orderSettings = orderSettings;
            _urlHelperFactory = urlHelperFactory;
            _actionContextAccessor = actionContextAccessor;
            _rewardPointsSettings = rewardPointsSettings;
            _rewardPointService = rewardPointService;
            _dateTimeHelper = dateTimeHelper;
            _shippingService = shippingService;
            _shippingSettings = shippingSettings;
            _genericAttributeService = genericAttributeService;
            _paymentPluginManager = paymentPluginManager;
            _paymentService = paymentService;
            _returnRequestService = returnRequestService;
            _jCarouselService = jCarouselService;
            _publicJCarouselModelFactory = publicJCarouselModelFactory;
            _widgetPluginManager = widgetPluginManager;
            _apiProductModelFactory = apiProductModelFactory;
            _taxSettings = taxSettings;
            _orderTotalCalculationService = orderTotalCalculationService;
            _giftCardService = giftCardService;
            _catalogSettings = catalogSettings;
            _env = env;
            _alchubGeneralService = alchubGeneralService;
            _specificationAttributeApiService = specificationAttributeApiService;
            _productAttributeFormatter = productAttributeFormatter;
            _vendorService = vendorService;
            _deliveryFeeService = deliveryFeeService;
            _tipFeeService = tipFeeService;
            _slotService = slotService;
            _serviceFeeManager = serviceFeeManager;
            _categoryApiService = categoryApiService;
            _currencySettings = currencySettings;
            _categoryService = categoryService;
            _manufacturerApiService = manufacturerApiService;
            _alchubSettings = alchubSettings;
            _elasticsearchManager = elasticsearchManager;
            _alchubSameNameProductService = alchubSameNameProductService;
            _orderDispatchService = orderDispatchService;
        }

        #region Utilities

        /// <summary>
        /// Get discount text based on original & old proce of product
        /// </summary>
        /// <param name="price"></param>
        /// <param name="oldPrice"></param>
        /// <returns></returns>
        protected string GetDiscountText(decimal? price, decimal? oldPrice)
        {
            var discountstr = string.Empty;

            //discount
            if (oldPrice.HasValue && (oldPrice.Value > decimal.Zero))
            {
                var originalPrice = price ?? decimal.Zero;
                var discountPer = 100 - ((float)originalPrice * 100 / (float)oldPrice.Value);
                CultureInfo ci = new CultureInfo("en-us");
                discountstr = (discountPer / 100).ToString("P01", ci);
            }

            return discountstr;
        }

        protected virtual async Task<IList<ProductManufacturer>> PrepareProductManufacturerIdsAsync(Product product, Customer customer)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            var manufactores = new List<ProductManufacturer>();
            if (product.ProductType == ProductType.GroupedProduct)
            {
                //gropuproduct manufacture = distinct list of all associated products manufactures (08-06-23)
                var store = _storeContext.GetCurrentStore();
                var associatedProducts = await _productService.GetAssociatedProductsAsync(product.Id, store.Id,
                                                                                             //++Alchub geovendor
                                                                                             geoVendorIds: await _workContext.GetCurrentCustomerGeoRadiusVendorIdsAsync(customer));
                foreach (var associatedProduct in associatedProducts)
                {
                    manufactores.AddRange((await _manufacturerService.GetProductManufacturersByProductIdAsync(associatedProduct.Id))?.ToList());
                }
            }
            else
                manufactores = (await _manufacturerService.GetProductManufacturersByProductIdAsync(product.Id))?.ToList();

            //distinct manufactures (08-06-23)
            manufactores = manufactores?.DistinctBy(m => m.ManufacturerId)?.ToList();

            return manufactores.ToList();
        }

        #endregion

        public async Task<ProductDto> PrepareProductDTOAsync(Product product)
        {
            //current customer
            var customer = await _authenticationService.GetAuthenticatedCustomerAsync();
            if (customer is null)
                throw new ArgumentNullException(nameof(customer));

            var productDto = product.ToDto();
            var productPictures = await _productService.GetProductPicturesByProductIdAsync(product.Id);
            await PrepareProductImagesAsync(productPictures, productDto);

            productDto.SeName = await _urlRecordService.GetSeNameAsync(product);
            productDto.DiscountIds = (await _discountService.GetAppliedDiscountsAsync(product)).Select(discount => discount.Id).ToList();
            //productDto.ManufacturerIds = (await _manufacturerService.GetProductManufacturersByProductIdAsync(product.Id)).Select(pm => pm.ManufacturerId).ToList();
            productDto.ManufacturerIds = (await PrepareProductManufacturerIdsAsync(product, customer)).Select(pm => pm.ManufacturerId).ToList();
            productDto.RoleIds = (await _aclService.GetAclRecordsAsync(product)).Select(acl => acl.CustomerRoleId).ToList();
            productDto.StoreIds = (await _storeMappingService.GetStoreMappingsAsync(product)).Select(mapping => mapping.StoreId).ToList();
            productDto.Tags = (await _productTagService.GetAllProductTagsByProductIdAsync(product.Id)).Select(tag => tag.Name).ToList();

            var store = await _storeContext.GetCurrentStoreAsync();
            var associatedProducts = await _productService.GetAssociatedProductsAsync(product.Id,
               store.Id,
               //++Alchub geovendor
               geoVendorIds: await _workContext.GetCurrentCustomerGeoRadiusVendorIdsAsync(customer));

            productDto.AssociatedProductIds = associatedProducts?.Select(associatedProduct => associatedProduct.Id)?.ToList(); //associated products according geofence - 06-07-23

            //related - featured product
            productDto.RelatedProductIds = (await _productService.GetRelatedProductsByProductId1Async(product.Id)).Select(x => x.ProductId2).ToList();

            var allLanguages = await _languageService.GetAllLanguagesAsync();

            productDto.RequiredProductIds = _productService.ParseRequiredProductIds(product);

            await PrepareProductAttributesAsync(productDto);

            // localization
            if (await _customerLanguage.Value is { Id: var languageId })
            {
                productDto.Name = await _localizationService.GetLocalizedAsync(product, x => x.Name, languageId);
                productDto.ShortDescription = await _localizationService.GetLocalizedAsync(product, x => x.ShortDescription, languageId);
                productDto.FullDescription = await _localizationService.GetLocalizedAsync(product, x => x.FullDescription, languageId);
            }

            //custom prepare
            //share url (product url)
            productDto.ShareUrl = await RouteUrlAsync(routeName: "Product", routeValues: new { SeName = await _urlRecordService.GetSeNameAsync(product) });

            //price
            var priceModel = await PrepareProductOverviewPriceModelAsync(product, customer);
            productDto.Price = priceModel.Price;
            productDto.PriceValue = priceModel.PriceValue;
            productDto.OldPrice = priceModel.OldPrice;
            productDto.OldPriceValue = priceModel.OldPriceValue;

            //discount
            productDto.Discount = GetDiscountText(priceModel.PriceValue, priceModel.OldPriceValue);

            //favorite
            productDto.IsInFavorite = await IsProductInWishList(product.Id, customer);

            //custom alchub fields
            await PrepareProductOverviewAlchubCustomFields(product, productDto);


            //var productSpecificationAttribtues = _specificationAttributeApiService.GetProductSpecificationAttributes(product.Id);

            //if (productSpecificationAttribtues != null)
            //{
            //    var productSpecificationAttributeDtos = productSpecificationAttribtues.Select(x => PrepareProductSpecificationAttributeDto(x)).ToList();
            //    productDto.ProductSpecificationAttributes = productSpecificationAttributeDtos;
            //}

            //specs
            productDto.ProductSpecificationModel = await PrepareProductSpecificationModelAsync(product);

            //Rating Avg
            decimal ratingAvg = 0;
            if (productDto.ApprovedRatingSum != null && productDto.ApprovedRatingSum != 0)
                ratingAvg = ((decimal)productDto.ApprovedRatingSum / (decimal)productDto.ApprovedTotalReviews);
            ratingAvg = Math.Round(ratingAvg, 1);
            productDto.RatingAvg = ratingAvg;

            //manufactures : //gropuproduct manufacture = distinct list of all associated products manufactures (08-06-23)
            if (productDto.ManufacturerIds != null && productDto.ManufacturerIds.Any())
            {
                //var manufacturer = _manufacturerApiService.GetManufacturerById(productDto.ManufacturerIds.First());
                //if (manufacturer != null)
                //    productDto.Manufacturer = await PrepareManufacturerDtoAsync(manufacturer);

                var manufactureDtos = new List<ManufacturerDto>();
                foreach (var manufactureId in productDto.ManufacturerIds)
                {
                    var manufacturer = _manufacturerApiService.GetManufacturerById(manufactureId);
                    if (manufacturer != null)
                        manufactureDtos.Add(await PrepareManufacturerDtoAsync(manufacturer));
                }

                productDto.Manufacturers = manufactureDtos;
            }

            //prepare variant + manufacturer combination for group product
            if (product.ProductType == ProductType.GroupedProduct)
            {
                //get manufacturer
                var manufacturerId = productDto.ManufacturerIds?.FirstOrDefault() ?? 0;
                if (manufacturerId > 0)
                    productDto.GroupedProductBaseVariants = await PrepareGroupedProductVariantsModelsAsync(product, manufacturerId);
            }

            return productDto;
        }

        /// <summary>
        /// Prepare the grouped product variant models
        /// </summary>
        /// <param name="groupedProduct">Product</param>
        /// <param name="manufacturerId">Maufacturer identifiers</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the list of grouped product variant brief info model
        /// </returns>
        protected virtual async Task<IList<GroupedProductVariantBrifInfoModel>> PrepareGroupedProductVariantsModelsAsync(Product groupedProduct, int manufacturerId)
        {
            if (groupedProduct == null)
                throw new ArgumentNullException(nameof(groupedProduct));

            //get grouped product variants
            var groupedProductVariants = await _alchubSameNameProductService.GetGroupedProductVariants(groupedProduct, manufacturerId);

            var model = await groupedProductVariants
                .SelectAwait(async gpv =>
                {
                    var modelVariant = new GroupedProductVariantBrifInfoModel
                    {
                        GroupedProductId = gpv.Key.Id,
                        VariantName = gpv.Value,
                        DisplayOrder = gpv.Key.DisplayOrder, //Pending
                        IsActive = groupedProduct.Id == gpv.Key.Id
                    };

                    return modelVariant;
                }).ToListAsync();

            return model;
        }

        /// <returns>A task that represents the asynchronous operation</returns>
        protected virtual async Task PrepareProductOverviewAlchubCustomFields(Product product, ProductDto productOverviewModel)
        {
            //prepare size & container details
            if (product.ProductType == ProductType.GroupedProduct)
            {
                var store = await _storeContext.GetCurrentStoreAsync();
                var associatedProducts = await _productService.GetAssociatedProductsAsync(product.Id,
                    store.Id,
                    //++Alchub geovendor
                    geoVendorIds: await _workContext.GetCurrentCustomerGeoRadiusVendorIdsAsync(await _authenticationService.GetAuthenticatedCustomerAsync()));

                if (associatedProducts.Any())
                {
                    //get default variant.
                    var defaultAssociatedProduct = await _alchubGeneralService.GetGroupedProductDefaultVariantAsync(associatedProducts);
                    if (defaultAssociatedProduct != null)
                    {
                        productOverviewModel.Size = defaultAssociatedProduct.Size;
                        productOverviewModel.Container = defaultAssociatedProduct.Container;
                    }
                }
            }
            else
            {
                productOverviewModel.Size = product.Size;
                productOverviewModel.Container = product.Container;
            }
        }

        /// <returns>A task that represents the asynchronous operation</returns>
        protected virtual async Task PrepareProductOverviewAlchubCustomFields(Product product, ProductListDto productListDto)
        {
            var productDto = new ProductDto();
            await PrepareProductOverviewAlchubCustomFields(product, productDto);
            productListDto.Size = productDto.Size;
            productListDto.Container = productDto.Container;
        }

        /// <summary>
        /// Prepare the product email a friend model
        /// </summary>
        /// <param name="model">Product email a friend model</param>
        /// <param name="product">Product</param>
        /// <param name="excludeProperties">Whether to exclude populating of model properties from the entity</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the product email a friend model
        /// </returns>

        protected virtual async Task<ProductSpecificationModel> PrepareProductSpecificationModelAsync(Product product)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            var model = new ProductSpecificationModel();

            // Add non-grouped attributes first
            model.Groups.Add(new ProductSpecificationAttributeGroupModel
            {
                Attributes = await PrepareProductSpecificationAttributeModelAsync(product, null)
            });

            // Add grouped attributes
            var groups = await _specificationAttributeService.GetProductSpecificationAttributeGroupsAsync(product.Id);
            foreach (var group in groups)
            {
                model.Groups.Add(new ProductSpecificationAttributeGroupModel
                {
                    Id = group.Id,
                    Name = await _localizationService.GetLocalizedAsync(group, x => x.Name),
                    Attributes = await PrepareProductSpecificationAttributeModelAsync(product, group)
                });
            }

            return model;
        }

        /// <summary>
        /// Prepare the product specification models
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="group">Specification attribute group</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the list of product specification model
        /// </returns>
        protected virtual async Task<IList<ProductSpecificationAttributeModel>> PrepareProductSpecificationAttributeModelAsync(Product product, SpecificationAttributeGroup group)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            var productSpecificationAttributes = await _specificationAttributeService.GetProductSpecificationAttributesAsync(
                    product.Id, specificationAttributeGroupId: group?.Id, showOnProductPage: true);

            var result = new List<ProductSpecificationAttributeModel>();

            foreach (var psa in productSpecificationAttributes)
            {
                var option = await _specificationAttributeService.GetSpecificationAttributeOptionByIdAsync(psa.SpecificationAttributeOptionId);

                var model = result.FirstOrDefault(model => model.Id == option.SpecificationAttributeId);
                if (model == null)
                {
                    var attribute = await _specificationAttributeService.GetSpecificationAttributeByIdAsync(option.SpecificationAttributeId);
                    model = new ProductSpecificationAttributeModel
                    {
                        Id = attribute.Id,
                        Name = await _localizationService.GetLocalizedAsync(attribute, x => x.Name)
                    };
                    result.Add(model);
                }

                var value = new ProductSpecificationAttributeValueModel
                {
                    AttributeTypeId = psa.AttributeTypeId,
                    ColorSquaresRgb = option.ColorSquaresRgb,
                    ValueRaw = psa.AttributeType switch
                    {
                        SpecificationAttributeType.Option => WebUtility.HtmlEncode(await _localizationService.GetLocalizedAsync(option, x => x.Name)),
                        SpecificationAttributeType.CustomText => WebUtility.HtmlEncode(await _localizationService.GetLocalizedAsync(psa, x => x.CustomValue)),
                        SpecificationAttributeType.CustomHtmlText => await _localizationService.GetLocalizedAsync(psa, x => x.CustomValue),
                        SpecificationAttributeType.Hyperlink => $"<a href='{psa.CustomValue}' target='_blank'>{psa.CustomValue}</a>",
                        _ => null
                    }
                };

                model.Values.Add(value);
            }

            return result;
        }




        public async Task<CategoryDto> PrepareCategoryDTOAsync(Category category)
        {
            var categoryDto = category.ToDto();

            var picture = await _pictureService.GetPictureByIdAsync(category.PictureId);
            var imageDto = await PrepareImageDtoAsync(picture);

            if (imageDto != null)
            {
                categoryDto.Image = imageDto;
            }

            categoryDto.SeName = await _urlRecordService.GetSeNameAsync(category);
            categoryDto.DiscountIds = (await _discountService.GetAppliedDiscountsAsync(category)).Select(discount => discount.Id).ToList();
            categoryDto.RoleIds = (await _aclService.GetAclRecordsAsync(category)).Select(acl => acl.CustomerRoleId).ToList();
            categoryDto.StoreIds = (await _storeMappingService.GetStoreMappingsAsync(category)).Select(mapping => mapping.StoreId)
                                                       .ToList();

            // localization
            if (await _customerLanguage.Value is { Id: var languageId })
            {
                categoryDto.Name = await _localizationService.GetLocalizedAsync(category, x => x.Name, languageId);
                categoryDto.Description = await _localizationService.GetLocalizedAsync(category, x => x.Description, languageId);
            }

            return categoryDto;
        }

        /// <summary>
        /// Prepare mobile category hirachy
        /// </summary>
        /// <param name="categoriesDtos"></param>
        /// <param name="rootCategoryId"></param>
        /// <returns></returns>
        public async Task<List<CategoryHierarchyModel>> PrepareCategoryHierarchyAsync(List<CategoryDto> categoriesDtos, int rootCategoryId, bool includeProductCount = false)
        {
            if (includeProductCount)
            {
                //prepare default params & use extended model factory to return output.
                var store = await _storeContext.GetCurrentStoreAsync();
                var customer = await _authenticationService.GetAuthenticatedCustomerAsync();

                //prepar categoryIds
                var categoryIds = new List<int>();
                var selectedCategoryId = rootCategoryId;
                if (selectedCategoryId > 0)
                    categoryIds.Add(selectedCategoryId);

                //include subcategories
                if (_catalogSettings.ShowProductsFromSubcategories)
                {
                    var tempCategoryIds = new List<int>();
                    foreach (var id in categoryIds)
                    {
                        tempCategoryIds.AddRange(await _categoryService.GetChildCategoryIdsAsync(id, store.Id));
                    }
                    categoryIds.AddRange(tempCategoryIds);
                }
                categoryIds = categoryIds.Distinct().ToList();

                //assign categoryIds to make filter check work correctly
                var filteredCategoryIds = new List<int>();
                filteredCategoryIds.AddRange(categoryIds);

                var vendorIds = new List<int>();
                vendorIds.AddRange(await _workContext.GetCurrentCustomerGeoRadiusVendorIdsAsync(customer));

                var manufacturerIds = new List<int>();

                return await PrepareFilterCategoryHierarchyAsync(filteredCategoryIds, categoriesDtos, rootCategoryId, manufacturerIds, vendorIds,
                null, null, null, null);
            }
            else
            {

                var result = new List<CategoryHierarchyModel>();
                //root categories
                var categories = categoriesDtos.Where(c => c.ParentCategoryId == rootCategoryId).OrderBy(c => c.DisplayOrder).ToList();
                foreach (var category in categories)
                {
                    //prepare model
                    var categoryModel = category.ToCategoryHierarchyModel();

                    //load sub categories (recursion)
                    var subCategories = await PrepareCategoryHierarchyAsync(categoriesDtos, category.Id);
                    categoryModel.SubCategories.AddRange(subCategories);

                    result.Add(categoryModel);
                }

                return result;
            }
        }

        public async Task<OrderDto> PrepareOrderDTOAsync(Order order)
        {
            var primaryStoreCurrency = await _currencyService.GetCurrencyByIdAsync(_currencySettings.PrimaryStoreCurrencyId);
            var languageId = (await _workContext.GetWorkingLanguageAsync()).Id;

            var orderDto = order.ToDto();

            var orderItems = await _orderService.GetOrderItemsAsync(order.Id);
            orderDto.OrderItems = await (orderItems).SelectAwait(async item => await PrepareOrderItemDTOAsync(item, order)).ToListAsync();

            orderDto.BillingAddress = (await _addressService.GetAddressByIdAsync(order.BillingAddressId))?.ToDto();
            orderDto.ShippingAddress = (await _addressService.GetAddressByIdAsync(order.ShippingAddressId ?? 0))?.ToDto();

            //prepare return order request details
            var returnRequestMessages = string.Empty;
            var returnAllowed = await _orderProcessingService.IsReturnRequestAllowedAsync(order);
            if (!returnAllowed)
                //default text
                returnRequestMessages = await _localizationService.GetResourceAsync("Nop.Api.Order.ReturnRequest.NotAllowed");

            //check return request expired or not. 
            var isReturnRequestExpired = false;
            var daysPassed = (DateTime.UtcNow - order.CreatedOnUtc).TotalDays;
            if (daysPassed >= _orderSettings.NumberOfDaysReturnRequestAvailable)
                isReturnRequestExpired = true;

            if (isReturnRequestExpired)
            {
                var endDate = order.CreatedOnUtc.AddDays(_orderSettings.NumberOfDaysReturnRequestAvailable);
                returnRequestMessages = string.Format(await _localizationService.GetResourceAsync("Nop.Api.Order.ReturnRequest.Expired"), endDate.ToString("dd MMM"));
            }

            var orderReturnRequest = new OrderReturnRequestDto()
            {
                IsReturnRequestAllowed = returnAllowed,
                ReturnRequestMessage = returnRequestMessages
            };

            orderDto.OrderReturnRequests = orderReturnRequest;

            orderDto.CreatedOnUtc = order.CreatedOnUtc.ToString("MM/dd/yyyy").Replace("-", "/");

            /*Alchub Start*/

            //payment method name
            var customer = await _customerService.GetCustomerByIdAsync(order.CustomerId);
            var paymentMethod = await _paymentPluginManager
                .LoadPluginBySystemNameAsync(order.PaymentMethodSystemName, customer, order.StoreId);
            orderDto.PaymentMethod = paymentMethod != null ? await _localizationService.GetLocalizedFriendlyNameAsync(paymentMethod, languageId) : order.PaymentMethodSystemName;

            //Delivery Fee
            orderDto.DeliveryFee = await _priceFormatter.FormatOrderPriceAsync(order.DeliveryFee, order.CurrencyRate, order.CustomerCurrencyCode, _orderSettings.DisplayCustomerCurrencyOnOrders, primaryStoreCurrency, languageId, null, false);

            var vendorWiseDeliveryFees = await _deliveryFeeService.GetVendorWiseOrderDeliveryFeesByOrderIdAsync(order.Id);

            if (vendorWiseDeliveryFees != null)
            {
                vendorWiseDeliveryFees.ToList().ForEach(async x =>
                {
                    var deliveryFeeValue = _currencyService.ConvertCurrency(x.DeliveryFeeValue, order.CurrencyRate);

                    orderDto.VendorWiseDeliveryFees.Add(
                        new VendorWiseDeliveryFee
                        {
                            VendorId = x.VendorId,
                            VendorName = x.VendorName,
                            DeliveryFeeValue = deliveryFeeValue,
                            DeliveryFee = await _priceFormatter.FormatPriceAsync(deliveryFeeValue, true, order.CustomerCurrencyCode, false, languageId)
                        });
                });
            }

            //Tip Fee
            orderDto.TipFee = await _priceFormatter.FormatOrderPriceAsync(order.TipFee, order.CurrencyRate, order.CustomerCurrencyCode, _orderSettings.DisplayCustomerCurrencyOnOrders, primaryStoreCurrency, languageId, null, false);

            var vendorWiseTipFees = await _tipFeeService.GetVendorWiseOrderTipFeesByOrderIdAsync(order.Id);

            if (vendorWiseTipFees != null)
            {
                vendorWiseTipFees.ToList().ForEach(async x =>
                {
                    var tipFeeValue = _currencyService.ConvertCurrency(x.TipFeeValue, order.CurrencyRate);

                    orderDto.VendorWiseTipFees.Add(
                        new VendorWiseTipFee
                        {
                            VendorId = x.VendorId,
                            VendorName = x.VendorName,
                            TipFeeValue = tipFeeValue,
                            TipFee = await _priceFormatter.FormatPriceAsync(tipFeeValue, true, order.CustomerCurrencyCode, false, languageId)
                        });
                });
            }
            orderDto.SlotFeesList = await PrepareOrderSlotListAsync(order);
            var refunds = await _orderItemRefundService.GetOrderItemRefundByOrderIdAsync(order.Id);
            orderDto.RefundedAmount = refunds?.Sum(x => x.TotalAmount) ?? decimal.Zero;

            //vendor pickup addresses
            foreach (var orderItem in orderItems.Where(oi => oi.InPickup))
            {
                var product = await _productService.GetProductByIdAsync(orderItem.ProductId);
                if (product == null)
                    continue;

                //vendor
                var vendor = await _vendorService.GetVendorByIdAsync(product.VendorId);
                if (vendor == null)
                    continue;

                orderDto.VendorPickupAddresses.Add(new Nop.Plugin.Api.DTOs.Orders.VendorPickupAddressModel
                {
                    VendorId = vendor.Id,
                    VendorName = vendor.Name,
                    PickupAddress = vendor.PickupAddress?.Replace(", USA", "")//remove USA from store name - 14-06-23
                });
            }

            /*Alchub End*/

            return orderDto;
        }

        public TopicDto PrepareTopicDTO(Topic topic)
        {
            var topicDto = topic.ToDto();
            return topicDto;
        }

        public async Task<ShoppingCartItemDto> PrepareShoppingCartItemDTOAsync(ShoppingCartItem shoppingCartItem)
        {
            //current customer
            var customer = await _authenticationService.GetAuthenticatedCustomerAsync();
            if (customer is null)
                throw new ArgumentNullException(nameof(customer));

            var dto = shoppingCartItem.ToDto();
            var product = await _productService.GetProductByIdAsync(shoppingCartItem.ProductId);

            //Alchub Start
            var masterProduct = shoppingCartItem.GroupedProductId > 0 ? await _productService.GetProductByIdAsync(shoppingCartItem.GroupedProductId) :
                                                                await _productService.GetProductByIdAsync(shoppingCartItem.MasterProductId);
            if (masterProduct == null)
                masterProduct = product;


            dto.VendorId = product.VendorId;
            dto.CustomAttributeInfo = await _productAttributeFormatter.FormatCustomAttributesAsync(shoppingCartItem.CustomAttributesXml);
            //Alchub End

            dto.ProductDto = await PrepareProductDTOAsync(product);

            //++Alchub
            //set name according simple/group product
            dto.ProductDto.Name = await _productService.GetProductItemName(product, shoppingCartItem);

            await PrepareShoppingCartItemImagesAsync(shoppingCartItem, dto.ProductDto);

            //--Alchub

            dto.Attributes = _productAttributeConverter.Parse(shoppingCartItem.AttributesXml);

            //set currency
            var currentCurrency = await _customerApiService.GetCustomerCurrencyAsync(customer);
            if (currentCurrency is null)
                currentCurrency = await _workContext.GetWorkingCurrencyAsync();
            //unit prices
            if (product.CallForPrice &&
                //also check whether the current user is impersonated
                (!_orderSettings.AllowAdminsToBuyCallForPriceProducts || _workContext.OriginalCustomerIfImpersonated == null))
            {
                dto.UnitPrice = await _localizationService.GetResourceAsync("Products.CallForPrice");
                dto.UnitPriceValue = 0;
            }
            else
            {
                var (shoppingCartUnitPriceWithDiscountBase, _) = await _taxService.GetProductPriceAsync(product, (await _shoppingCartService.GetUnitPriceAsync(shoppingCartItem, true)).unitPrice);
                var shoppingCartUnitPriceWithDiscount = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(shoppingCartUnitPriceWithDiscountBase, currentCurrency);
                dto.UnitPrice = await _priceFormatter.FormatPriceAsync(shoppingCartUnitPriceWithDiscount);
                dto.UnitPriceValue = shoppingCartUnitPriceWithDiscount;

                //Wishlist item price - same as catalog
                if (shoppingCartItem.ShoppingCartType == Core.Domain.Orders.ShoppingCartType.Wishlist)
                {
                    //prepare price as shows in catalog.
                    if (product.ProductType == ProductType.SimpleProduct)
                    {
                        //simple product
                        await PrepareProductPriceRangeAsync(product, dto, customer);
                    }
                    else
                    {
                        var store = await _storeContext.GetCurrentStoreAsync();
                        var associatedProducts = await _productService.GetAssociatedProductsAsync(product.Id,
                            store.Id,
                            //++Alchub geovendor
                            geoVendorIds: await _workContext.GetCurrentCustomerGeoRadiusVendorIdsAsync(await _authenticationService.GetAuthenticatedCustomerAsync()));

                        //grouped product
                        await PrepareGroupedProductPriceRangeAsync(associatedProducts, dto, customer);
                    }
                }
            }
            //subtotal, discount
            if (product.CallForPrice &&
                //also check whether the current user is impersonated
                (!_orderSettings.AllowAdminsToBuyCallForPriceProducts || _workContext.OriginalCustomerIfImpersonated == null))
            {
                dto.SubTotal = await _localizationService.GetResourceAsync("Products.CallForPrice");
                dto.SubTotalValue = 0;
            }
            else
            {
                //sub total
                var (subTotal, shoppingCartItemDiscountBase, _, maximumDiscountQty) = await _shoppingCartService.GetSubTotalAsync(shoppingCartItem, true);
                var (shoppingCartItemSubTotalWithDiscountBase, _) = await _taxService.GetProductPriceAsync(product, subTotal);
                var shoppingCartItemSubTotalWithDiscount = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(shoppingCartItemSubTotalWithDiscountBase, currentCurrency);
                dto.SubTotal = await _priceFormatter.FormatPriceAsync(shoppingCartItemSubTotalWithDiscount);
                dto.SubTotalValue = shoppingCartItemSubTotalWithDiscount;

                //display an applied discount amount
                if (shoppingCartItemDiscountBase > decimal.Zero)
                {
                    (shoppingCartItemDiscountBase, _) = await _taxService.GetProductPriceAsync(product, shoppingCartItemDiscountBase);
                    if (shoppingCartItemDiscountBase > decimal.Zero)
                    {
                        var shoppingCartItemDiscount = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(shoppingCartItemDiscountBase, currentCurrency);
                        dto.Discount = await _priceFormatter.FormatPriceAsync(shoppingCartItemDiscount);
                        dto.DiscountValue = shoppingCartItemDiscount;
                    }
                }
            }

            //Alchub Start 
            dto.SlotPrice = shoppingCartItem.SlotPrice != null ? await _priceFormatter.FormatPriceAsync(shoppingCartItem.SlotPrice) : "";

            if (shoppingCartItem.SlotStartTime != null)
            {
                if (shoppingCartItem.SlotStartTime.Date == DateTime.UtcNow.Date)
                    dto.SlotStartDate = "Today";
                else if (shoppingCartItem.SlotStartTime.Date == DateTime.UtcNow.AddDays(1).Date)
                    dto.SlotStartDate = "Tomorrow";
                else
                    dto.SlotStartDate = shoppingCartItem.SlotStartTime.ToString("dd MMMM");
            }
            else
                dto.SlotStartDate = string.Empty;

            dto.SlotTime = shoppingCartItem.SlotTime != null ? SlotHelper.ConvertTo12hoursSlotTime(shoppingCartItem.SlotTime) : "";
            dto.IsPickup = shoppingCartItem.IsPickup;
            //Alchub ENd

            //item warnings
            var itemWarnings = await _shoppingCartService.GetShoppingCartItemWarningsAsync(
                customer,
                shoppingCartItem.ShoppingCartType,
                product,
                shoppingCartItem.StoreId,
                shoppingCartItem.AttributesXml,
                shoppingCartItem.CustomerEnteredPrice,
                shoppingCartItem.RentalStartDateUtc,
                shoppingCartItem.RentalEndDateUtc,
                shoppingCartItem.Quantity,
                false,
                shoppingCartItem.Id);
            foreach (var warning in itemWarnings)
                dto.Warnings.Add(warning);

            return dto;
        }

        public async Task<OrderItemDto> PrepareOrderItemDTOAsync(OrderItem orderItem, Order order)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            var dto = orderItem.ToDto();
            dto.Product = await PrepareProductDTOAsync(await _productService.GetProductByIdAsync(orderItem.ProductId));

            //++Alchub
            //set name according simple/group product
            var product = await _productService.GetProductByIdAsync(orderItem.ProductId);
            dto.Product.Name = await _productService.GetProductItemName(product, orderItem);

            await PrepareOrderItemImagesAsync(orderItem, dto.Product);

            //--Alchub

            dto.Attributes = _productAttributeConverter.Parse(orderItem.AttributesXml);

            //set orderItem return allowed
            var returnAllowed = false;
            if (await _orderProcessingService.IsReturnRequestAllowedAsync(order))
            {
                var returnRequestAvailability = await _returnRequestService.GetReturnRequestAvailabilityAsync(order.Id);
                if (returnRequestAvailability != null && returnRequestAvailability.IsAllowed)
                    returnAllowed = returnRequestAvailability?.ReturnableOrderItems?.FirstOrDefault(x => x.OrderItem.Id == orderItem.Id)?.AvailableQuantityForReturn > 0;
            }
            dto.IsReturnAllowed = returnAllowed;
            dto.OrderItemStatus = await _localizationService.GetLocalizedEnumAsync(orderItem.OrderItemStatus);
            dto.SlotId = orderItem.SlotId;
            dto.SlotPrice = orderItem.SlotPrice;
            dto.SlotStartTime = orderItem.SlotStartTime.ToString("MM/dd/yyyy").Replace('/', '-');
            dto.SlotEndTime = orderItem.SlotEndTime;
            dto.SlotTime = SlotHelper.ConvertTo12hoursSlotTime(orderItem.SlotTime);
            dto.InPickup = orderItem.InPickup;

            //dispatch tracking
            dto.TrackingUrl = await _orderDispatchService.GetDispatchOrderItemIdTrackingUrlAsync(orderItem.Id);

            return dto;
        }

        public async Task<StoreDto> PrepareStoreDTOAsync(Store store)
        {
            var storeDto = store.ToDto();

            storeDto.Languages = (await _languageService.GetAllLanguagesAsync(storeId: store.Id)).Select(x => x.ToDto()).ToList();

            // localization
            if (await _customerLanguage.Value is { Id: var languageId })
            {
                storeDto.Name = await _localizationService.GetLocalizedAsync(store, x => x.Name, languageId);

                storeDto.Currencies = await (await _currencyService.GetAllCurrenciesAsync(storeId: store.Id)).SelectAwait(currency => prepareCurrencyDtoAsync(currency, languageId, _localizationService)).ToListAsync();

                static async ValueTask<CurrencyDto> prepareCurrencyDtoAsync(Currency currency, int languageId, ILocalizationService localizationService)
                {
                    var currencyDto = currency.ToDto();
                    // localization
                    currencyDto.Name = await localizationService.GetLocalizedAsync(currency, x => x.Name, languageId);
                    return currencyDto;
                }
            }
            else
            {
                storeDto.Currencies = (await _currencyService.GetAllCurrenciesAsync(storeId: store.Id)).Select(currency => currency.ToDto()).ToList();
            }

            return storeDto;

        }

        public async Task<LanguageDto> PrepareLanguageDtoAsync(Language language)
        {
            var languageDto = language.ToDto();
            var storeUrl = _webHelper.GetStoreLocation();
            // string filePath = Path.Combine(_env.ContentRootPath, "wwwroot/flag/");
            languageDto.FlagImageFileName = storeUrl + "images/flags/" + languageDto.FlagImageFileName;

            languageDto.StoreIds = (await _storeMappingService.GetStoreMappingsAsync(language)).Select(mapping => mapping.StoreId).ToList();

            if (languageDto.StoreIds.Count == 0)
            {
                languageDto.StoreIds = (await _storeService.GetAllStoresAsync()).Select(s => s.Id).ToList();
            }

            return languageDto;
        }

        public async Task<CurrencyDto> PrepareCurrencyDtoAsync(Currency currency)
        {
            var currencyDto = currency.ToDto();

            currencyDto.StoreIds = (await _storeMappingService.GetStoreMappingsAsync(currency)).Select(mapping => mapping.StoreId).ToList();

            if (currencyDto.StoreIds.Count == 0)
            {
                currencyDto.StoreIds = (await _storeService.GetAllStoresAsync()).Select(s => s.Id).ToList();
            }

            // localization
            if (await _customerLanguage.Value is { Id: var languageId })
            {
                currencyDto.Name = await _localizationService.GetLocalizedAsync(currency, x => x.Name, languageId);
            }

            return currencyDto;
        }

        public ProductAttributeDto PrepareProductAttributeDTO(ProductAttribute productAttribute)
        {
            return productAttribute.ToDto();
        }

        public ProductSpecificationAttributeDto PrepareProductSpecificationAttributeDto(ProductSpecificationAttribute productSpecificationAttribute)
        {
            return productSpecificationAttribute.ToDto();
        }

        public SpecificationAttributeDto PrepareSpecificationAttributeDto(SpecificationAttribute specificationAttribute)
        {
            return specificationAttribute.ToDto();
        }

        public async Task<ManufacturerDto> PrepareManufacturerDtoAsync(Manufacturer manufacturer)
        {
            var manufacturerDto = manufacturer.ToDto();

            var picture = await _pictureService.GetPictureByIdAsync(manufacturer.PictureId);
            var imageDto = await PrepareImageDtoAsync(picture);

            if (imageDto != null)
            {
                manufacturerDto.Image = imageDto;
            }

            manufacturerDto.SeName = await _urlRecordService.GetSeNameAsync(manufacturer);
            manufacturerDto.DiscountIds = (await _discountService.GetAppliedDiscountsAsync(manufacturer)).Select(discount => discount.Id).ToList();
            manufacturerDto.RoleIds = (await _aclService.GetAclRecordsAsync(manufacturer)).Select(acl => acl.CustomerRoleId).ToList();
            manufacturerDto.StoreIds = (await _storeMappingService.GetStoreMappingsAsync(manufacturer)).Select(mapping => mapping.StoreId)
                                                           .ToList();

            var allLanguages = await _languageService.GetAllLanguagesAsync();

            // localization
            if (await _customerLanguage.Value is { Id: var languageId })
            {
                manufacturerDto.Name = await _localizationService.GetLocalizedAsync(manufacturer, x => x.Name, languageId);
                manufacturerDto.Description = await _localizationService.GetLocalizedAsync(manufacturer, x => x.Description, languageId);
            }

            return manufacturerDto;
        }

        //public void PrepareProductSpecificationAttributes(List<ProductSpecificationAttribute> productSpecificationAttributes, ProductDto productDto)
        //{
        //    if (productDto.ProductSpecificationAttributes == null)
        //    {
        //        productDto.ProductSpecificationAttributes = new List<ProductSpecificationAttributeDto>();
        //    }

        //    foreach (var productSpecificationAttribute in productSpecificationAttributes)
        //    {
        //        var productSpecificationAttributeDto = PrepareProductSpecificationAttributeDto(productSpecificationAttribute);

        //        if (productSpecificationAttributeDto != null)
        //        {
        //            productDto.ProductSpecificationAttributes.Add(productSpecificationAttributeDto);
        //        }
        //    }
        //}

        public AddressDto PrepareAddressDTO(Address address)
        {
            var addressDto = address.ToDto();
            return addressDto;
        }

        public async Task<TopCategoryDto> PrepareTopCategoryDTOAsync(Category category)
        {
            var topCategoryDto = category.TopDto();

            var picture = await _pictureService.GetPictureByIdAsync(category.PictureId);
            var imageDto = await PrepareTopImageDtoAsync(picture);

            if (imageDto != null && imageDto.Image != null)
            {
                topCategoryDto.Image = imageDto.Image;
            }

            // localization
            if (await _customerLanguage.Value is { Id: var languageId })
            {
                topCategoryDto.Name = await _localizationService.GetLocalizedAsync(category, x => x.Name, languageId);
            }

            //parent category id - 20-07-23
            topCategoryDto.ParentCategoryId = category.ParentCategoryId;

            //parent category name - 01-08-23
            if (category.ParentCategoryId > 0)
            {
                var parentCategory = await _categoryService.GetCategoryByIdAsync(category.ParentCategoryId);
                if (parentCategory != null)
                    topCategoryDto.ParentCategoryName = await _localizationService.GetLocalizedAsync(parentCategory, x => x.Name);
            }

            return topCategoryDto;
        }

        #region Private methods

        private async Task PrepareProductImagesAsync(IEnumerable<ProductPicture> productPictures, ProductDto productDto)
        {
            if (productDto.Images == null)
            {
                productDto.Images = new List<ImageMappingDto>();
            }

            if (!string.IsNullOrEmpty(productDto.ImageUrl))
            {
                //set only src
                var productImageDto = new ImageMappingDto
                {
                    Src = productDto.ImageUrl
                };

                productDto.Images.Add(productImageDto);
            }

            await Task.CompletedTask;

            //// Here we prepare the resulted dto image.
            //foreach (var productPicture in productPictures)
            //{
            //    var imageDto = await PrepareImageDtoAsync(await _pictureService.GetPictureByIdAsync(productPicture.PictureId));

            //    if (imageDto != null)
            //    {
            //        var productImageDto = new ImageMappingDto
            //        {
            //            Id = productPicture.Id,
            //            PictureId = productPicture.PictureId,
            //            Position = productPicture.DisplayOrder,
            //            Src = imageDto.Src,
            //            Attachment = imageDto.Attachment
            //        };

            //        productDto.Images.Add(productImageDto);
            //    }
            //}
        }

        private async Task<ImageDto> PrepareImageDtoAsync(Picture picture)
        {
            ImageDto image = null;

            if (picture != null)
            {
                (string url, _) = await _pictureService.GetPictureUrlAsync(picture);

                // We don't use the image from the passed dto directly 
                // because the picture may be passed with src and the result should only include the base64 format.
                image = new ImageDto
                {
                    //Attachment = Convert.ToBase64String(picture.PictureBinary),
                    Src = url
                };
            }

            return image;
        }

        private async Task PrepareShoppingCartItemImagesAsync(ShoppingCartItem sci, ProductDto productDto)
        {
            //reset old images
            productDto.Images = new List<ImageMappingDto>();

            var product = await _productService.GetProductByIdAsync(sci.ProductId);
            //++Alchub
            var masterProduct = await _productService.GetProductByIdAsync(sci.MasterProductId);
            if (masterProduct == null)
                masterProduct = product;

            if (!string.IsNullOrEmpty(masterProduct.ImageUrl))
            {
                //set only src
                var productImageDto = new ImageMappingDto
                {
                    Src = masterProduct.ImageUrl
                };

                productDto.Images.Add(productImageDto);
            }

            //var productPictures = await _productService.GetProductPicturesByProductIdAsync(masterProduct.Id);

            //await PrepareProductImagesAsync(productPictures, productDto);
        }

        private async Task PrepareOrderItemImagesAsync(OrderItem orderItem, ProductDto productDto)
        {
            //reset old images
            productDto.Images = new List<ImageMappingDto>();

            var product = await _productService.GetProductByIdAsync(orderItem.ProductId);
            //++Alchub
            var masterProduct = await _productService.GetProductByIdAsync(orderItem.MasterProductId);
            if (masterProduct == null)
                masterProduct = product;

            if (!string.IsNullOrEmpty(masterProduct.ImageUrl))
            {
                //set only src
                var productImageDto = new ImageMappingDto
                {
                    Src = masterProduct.ImageUrl
                };

                productDto.Images.Add(productImageDto);
            }

            //var productPictures = await _productService.GetProductPicturesByProductIdAsync(masterProduct.Id);

            //await PrepareProductImagesAsync(productPictures, productDto);
        }

        private async Task PrepareProductAttributesAsync(ProductDto productDto)
        {
            var productAttributeMappings = await _productAttributeService.GetProductAttributeMappingsByProductIdAsync(productDto.Id);

            if (productDto.ProductAttributeMappings == null)
            {
                productDto.ProductAttributeMappings = new List<ProductAttributeMappingDto>();
            }

            foreach (var productAttributeMapping in productAttributeMappings)
            {
                var productAttributeMappingDto = await PrepareProductAttributeMappingDtoAsync(productAttributeMapping);

                if (productAttributeMappingDto != null)
                {
                    productDto.ProductAttributeMappings.Add(productAttributeMappingDto);
                }
            }
        }

        private async Task<ProductAttributeMappingDto> PrepareProductAttributeMappingDtoAsync(
            ProductAttributeMapping productAttributeMapping)
        {
            ProductAttributeMappingDto productAttributeMappingDto = null;

            if (productAttributeMapping != null)
            {
                productAttributeMappingDto = new ProductAttributeMappingDto
                {
                    Id = productAttributeMapping.Id,
                    ProductAttributeId = productAttributeMapping.ProductAttributeId,
                    ProductAttributeName = (await _productAttributeService.GetProductAttributeByIdAsync(productAttributeMapping.ProductAttributeId)).Name,
                    TextPrompt = productAttributeMapping.TextPrompt,
                    DefaultValue = productAttributeMapping.DefaultValue,
                    AttributeControlTypeId = productAttributeMapping.AttributeControlTypeId,
                    DisplayOrder = productAttributeMapping.DisplayOrder,
                    IsRequired = productAttributeMapping.IsRequired,
                    ProductAttributeValues = await (await _productAttributeService.GetProductAttributeValuesAsync(productAttributeMapping.Id))
                                                    .SelectAwait(async attributeValue => await PrepareProductAttributeValueDtoAsync(attributeValue,
                                                        await _productService.GetProductByIdAsync(productAttributeMapping.ProductId)))
                                                    .ToListAsync()
                };
            }

            return productAttributeMappingDto;
        }

        private async Task<ProductAttributeValueDto> PrepareProductAttributeValueDtoAsync(
            ProductAttributeValue productAttributeValue,
            Product product)
        {
            ProductAttributeValueDto productAttributeValueDto = null;

            if (productAttributeValue != null)
            {
                productAttributeValueDto = productAttributeValue.ToDto();
                if (productAttributeValue.ImageSquaresPictureId > 0)
                {
                    var imageSquaresPicture = await _pictureService.GetPictureByIdAsync(productAttributeValue.ImageSquaresPictureId);
                    var imageDto = await PrepareImageDtoAsync(imageSquaresPicture);
                    productAttributeValueDto.ImageSquaresImage = imageDto;
                }

                if (productAttributeValue.PictureId > 0)
                {
                    // make sure that the picture is mapped to the product
                    // This is needed since if you delete the product picture mapping from the nopCommerce administrationthe
                    // then the attribute value is not updated and it will point to a picture that has been deleted
                    var productPicture = (await _productService.GetProductPicturesByProductIdAsync(product.Id)).FirstOrDefault(pp => pp.PictureId == productAttributeValue.PictureId);
                    if (productPicture != null)
                    {
                        productAttributeValueDto.ProductPictureId = productPicture.Id;
                    }
                }
            }

            return productAttributeValueDto;
        }

        private void PrepareProductAttributeCombinations(
            IEnumerable<ProductAttributeCombination> productAttributeCombinations,
            ProductDto productDto)
        {
            productDto.ProductAttributeCombinations = productDto.ProductAttributeCombinations ?? new List<ProductAttributeCombinationDto>();

            foreach (var productAttributeCombination in productAttributeCombinations)
            {
                var productAttributeCombinationDto = PrepareProductAttributeCombinationDto(productAttributeCombination);
                if (productAttributeCombinationDto != null)
                {
                    productDto.ProductAttributeCombinations.Add(productAttributeCombinationDto);
                }
            }
        }

        private ProductAttributeCombinationDto PrepareProductAttributeCombinationDto(ProductAttributeCombination productAttributeCombination)
        {
            return productAttributeCombination.ToDto();
        }

        private async Task<Language> GetAuthenticatedCustomerLanguage()
        {
            var customer = await _authenticationService.GetAuthenticatedCustomerAsync();
            return (customer is not null) ? await _customerApiService.GetCustomerLanguageAsync(customer) : null;
        }

        private async Task<TopCategoryDto> PrepareTopImageDtoAsync(Picture picture)
        {
            TopCategoryDto image = null;

            if (picture != null)
            {
                (string url, _) = await _pictureService.GetPictureUrlAsync(picture);

                // We don't use the image from the passed dto directly 
                // because the picture may be passed with src and the result should only include the base64 format.
                image = new TopCategoryDto
                {
                    //Attachment = Convert.ToBase64String(picture.PictureBinary),
                    Image = url
                };
            }

            return image;
        }

        public async Task<FeaturedProductDto> PrepareFeaturedProductDTOAsync(Product product)
        {
            var productDto = product.ToFeaturedDto();
            var productPictures = await _productService.GetProductPicturesByProductIdAsync(product.Id);
            await PrepareFeaturedProductImagesAsync(productPictures, productDto);

            var store = await _storeContext.GetCurrentStoreAsync();
            var associatedProducts = await _productService.GetAssociatedProductsAsync(product.Id,
                store.Id,
                //++Alchub geovendor
                geoVendorIds: await _workContext.GetCurrentCustomerGeoRadiusVendorIdsAsync(await _authenticationService.GetAuthenticatedCustomerAsync()));

            productDto.AssociatedProductIds = associatedProducts?.Select(associatedProduct => associatedProduct.Id)?.ToList();

            var allLanguages = await _languageService.GetAllLanguagesAsync();

            productDto.RequiredProductIds = _productService.ParseRequiredProductIds(product);

            //await PrepareProductAttributesAsync(productDto);

            // localization
            if (await _customerLanguage.Value is { Id: var languageId })
            {
                productDto.Name = await _localizationService.GetLocalizedAsync(product, x => x.Name, languageId);
            }

            return productDto;
        }

        private async Task PrepareFeaturedProductImagesAsync(IEnumerable<ProductPicture> productPictures, FeaturedProductDto featuredProductDto)
        {
            if (featuredProductDto.Images == null)
            {
                featuredProductDto.Images = new List<ImageMappingDto>();
            }

            // Here we prepare the resulted dto image.
            foreach (var productPicture in productPictures)
            {
                var imageDto = await PrepareImageDtoAsync(await _pictureService.GetPictureByIdAsync(productPicture.PictureId));

                if (imageDto != null)
                {
                    var productImageDto = new ImageMappingDto
                    {
                        Id = productPicture.Id,
                        PictureId = productPicture.PictureId,
                        Position = productPicture.DisplayOrder,
                        Src = imageDto.Src,
                        Attachment = imageDto.Attachment
                    };

                    featuredProductDto.Images.Add(productImageDto);
                }
            }
        }

        public async Task<DealsOfTheDayDto> PrepareDealsOfTheDayProductDTOAsync(Product product)
        {
            var dealsOfTheDayDto = product.ToDealsOfTheDayDto();
            var productPictures = await _productService.GetProductPicturesByProductIdAsync(product.Id);
            await PrepareDealsOfTheDayProductImagesAsync(productPictures, dealsOfTheDayDto);

            var store = await _storeContext.GetCurrentStoreAsync();
            var associatedProducts = await _productService.GetAssociatedProductsAsync(product.Id,
                store.Id,
                //++Alchub geovendor
                geoVendorIds: await _workContext.GetCurrentCustomerGeoRadiusVendorIdsAsync(await _authenticationService.GetAuthenticatedCustomerAsync()));

            dealsOfTheDayDto.AssociatedProductIds = associatedProducts?.Select(associatedProduct => associatedProduct.Id)?.ToList();

            var allLanguages = await _languageService.GetAllLanguagesAsync();

            dealsOfTheDayDto.RequiredProductIds = _productService.ParseRequiredProductIds(product);

            // localization
            if (await _customerLanguage.Value is { Id: var languageId })
            {
                dealsOfTheDayDto.Name = await _localizationService.GetLocalizedAsync(product, x => x.Name, languageId);
            }

            return dealsOfTheDayDto;
        }

        private async Task PrepareDealsOfTheDayProductImagesAsync(IEnumerable<ProductPicture> productPictures, DealsOfTheDayDto dealsOfTheDayDto)
        {
            if (dealsOfTheDayDto.Images == null)
            {
                dealsOfTheDayDto.Images = new List<ImageMappingDto>();
            }

            // Here we prepare the resulted dto image.
            foreach (var productPicture in productPictures)
            {
                var imageDto = await PrepareImageDtoAsync(await _pictureService.GetPictureByIdAsync(productPicture.PictureId));

                if (imageDto != null)
                {
                    var productImageDto = new ImageMappingDto
                    {
                        Id = productPicture.Id,
                        PictureId = productPicture.PictureId,
                        Position = productPicture.DisplayOrder,
                        Src = imageDto.Src,
                        Attachment = imageDto.Attachment
                    };

                    dealsOfTheDayDto.Images.Add(productImageDto);
                }
            }
        }

        public async Task<IList<HomeBannerDto>> PrepareHomeBannerDtoAsync()
        {
            var store = await _storeContext.GetCurrentStoreAsync();
            var homeBannerDto = new List<HomeBannerDto>();

            //since we have 5 picture in nivo slider
            for (int i = 1; i <= 5; i++)
            {
                var pictureId = await _settingService.GetSettingByKeyAsync($"nivoslidersettings.picture{i}id", store.Id);
                if (pictureId > 0)
                {
                    var model = new HomeBannerDto
                    {
                        Id = pictureId,
                        PictureUrl = await _pictureService.GetPictureUrlAsync(pictureId),
                        Text = await _settingService.GetSettingByKeyAsync($"nivoslidersettings.Text{i}", "", store.Id),
                        Link = await _settingService.GetSettingByKeyAsync($"nivoslidersettings.Link{i}", "", store.Id),
                        AltText = await _settingService.GetSettingByKeyAsync($"nivoslidersettings.AltText{i}", "", store.Id),
                    };

                    homeBannerDto.Add(model);
                }
            }

            if (homeBannerDto.All(x => string.IsNullOrEmpty(x.PictureUrl)))
                //no pictures uploaded
                return null;

            return homeBannerDto;
        }

        private async Task PrepareProductImagesAsync(IEnumerable<ProductPicture> productPictures, ProductListDto productListDto)
        {
            if (productListDto.Images == null)
            {
                productListDto.Images = new List<ImageMappingDto>();
            }

            // Here we prepare the resulted dto image.
            foreach (var productPicture in productPictures)
            {
                var imageDto = await PrepareImageDtoAsync(await _pictureService.GetPictureByIdAsync(productPicture.PictureId));

                if (imageDto != null)
                {
                    var productImageDto = new ImageMappingDto
                    {
                        Id = productPicture.Id,
                        PictureId = productPicture.PictureId,
                        Position = productPicture.DisplayOrder,
                        Src = imageDto.Src,
                        Attachment = imageDto.Attachment
                    };

                    productListDto.Images.Add(productImageDto);
                }
            }
        }

        /// <summary>
        /// Generates an absolute URL for the specified store, routeName and route values
        /// </summary>
        /// <param name="storeId">Store identifier; Pass 0 to load URL of the current store</param>
        /// <param name="routeName">The name of the route that is used to generate URL</param>
        /// <param name="routeValues">An object that contains route values</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the generated URL
        /// </returns>
        protected virtual async Task<string> RouteUrlAsync(int storeId = 0, string routeName = null, object routeValues = null)
        {
            //try to get a store by the passed identifier
            var store = await _storeService.GetStoreByIdAsync(storeId) ?? await _storeContext.GetCurrentStoreAsync()
                ?? throw new Exception("No store could be loaded");

            //ensure that the store URL is specified
            if (string.IsNullOrEmpty(store.Url))
                throw new Exception("URL cannot be null");

            //generate a URL with an absolute path
            var urlHelper = _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext);
            var url = new PathString(urlHelper.RouteUrl(routeName, routeValues));

            //remove the application path from the generated URL if exists
            var pathBase = _actionContextAccessor.ActionContext?.HttpContext?.Request?.PathBase ?? PathString.Empty;
            url.StartsWithSegments(pathBase, out url);

            //compose the result
            return new Uri(WebUtility.UrlDecode($"{store.Url.TrimEnd('/')}{url}"), UriKind.Absolute).AbsoluteUri;
        }

        #endregion

        #region Product List / Search

        public async Task<ProductListDto> PrepareProductListDTOAsync(Product product, Customer customer)
        {
            var productListDto = product.ToListDto();

            ////commenting as not required in product list - 20-06-23
            //var productPictures = await _productService.GetProductPicturesByProductIdAsync(product.Id);
            //await PrepareProductImagesAsync(productPictures, productListDto);

            ////commenting as not required in product list
            //productListDto.AssociatedProductIds = (await _productService.GetAssociatedProductsAsync(product.Id, showHidden: true,
            //                    //++Alchub geovendor
            //                    geoVendorIds: await _workContext.GetCurrentCustomerGeoRadiusVendorIdsAsync(await _authenticationService.GetAuthenticatedCustomerAsync())))
            //                   .Select(associatedProduct => associatedProduct.Id)
            //                   .ToList();
            var allLanguages = await _languageService.GetAllLanguagesAsync();

            //productListDto.RequiredProductIds = _productService.ParseRequiredProductIds(product);

            // localization
            if (await _customerLanguage.Value is { Id: var languageId })
            {
                productListDto.Name = await _localizationService.GetLocalizedAsync(product, x => x.Name, languageId);
            }

            //price
            var priceModel = await PrepareProductOverviewPriceModelAsync(product, customer);
            productListDto.Price = priceModel.Price;

            //fastest slot
            //productListDto.FastestSlotTime = await PrepareProductFastestSlotAsync(product);

            //alchub custom fields
            await PrepareProductOverviewAlchubCustomFields(product, productListDto);

            //image - use web cache model to load image urls (20-06-23)
            var pictureModel = await PrepareProductOverviewPictureModelAsync(product);
            if (pictureModel != null)
            {
                //var pictureSize = _mediaSettings.ProductThumbPictureSize;
                //var picture = (await _pictureService.GetPicturesByProductIdAsync(product.Id, 1)).FirstOrDefault();
                //string fullSizeImageUrl, imageUrl;
                //(imageUrl, picture) = await _pictureService.GetPictureUrlAsync(picture, pictureSize);
                //(fullSizeImageUrl, picture) = await _pictureService.GetPictureUrlAsync(picture);

                productListDto.PictureUrl = pictureModel.ImageUrl;
                productListDto.FullSizePictureUrl = pictureModel.FullSizeImageUrl;
            }

            //favorite
            productListDto.IsInFavorite = await IsProductInWishList(product.Id, customer);

            decimal ratingAvg = 0;
            if (productListDto.ApprovedRatingSum != null && productListDto.ApprovedRatingSum != 0)
                ratingAvg = ((decimal)productListDto.ApprovedRatingSum / (decimal)productListDto.ApprovedTotalReviews);
            ratingAvg = Math.Round(ratingAvg, 1);
            productListDto.RatingAvg = ratingAvg;

            return productListDto;
        }

        /// <summary>
        /// Prepare product search result model
        /// </summary>
        /// <param name="product"></param>
        /// <param name="customer"></param>
        /// <param name="preparePictureModel"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public async Task<ProductsSearchResultDto> PrepareProductSearchResultDTOAsync(Product product, Customer customer, bool preparePictureModel = true)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            var model = new ProductsSearchResultDto
            {
                Id = product.Id,
                Name = await _localizationService.GetLocalizedAsync(product, x => x.Name),
            };

            //picture
            if (preparePictureModel)
            {
                //image - use web cache model to load image urls (20-06-23)
                var pictureModel = await PrepareProductOverviewPictureModelAsync(product);
                if (pictureModel != null)
                {
                    model.PictureUrl = pictureModel.ImageUrl;
                }
            }

            return model;
        }

        /// <summary>
        /// Prepare search reult dto 
        /// </summary>
        /// <param name="product"></param>
        /// <param name="customer"></param>
        /// <param name="preparePictureModel"></param>
        /// <returns></returns>
        public virtual async Task<ProductsSearchResultDto> PrepareProductSearchResultDtoElasticAsync(Product product, Customer customer, bool preparePictureModel = true)
        {
            var productModel = new ProductsSearchResultDto
            {
                Id = product.Id,
                Name = product.Name,
                PictureUrl = product.ImageUrl
            };

            return await Task.FromResult(productModel);
        }

        /// <summary>
        /// Prepare the product overview picture model
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="productThumbPictureSize">Product thumb picture size (longest side); pass null to use the default value of media settings</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the picture model
        /// </returns>
        protected virtual async Task<Nop.Web.Models.Media.PictureModel> PrepareProductOverviewPictureModelAsync(Product product, int? productThumbPictureSize = null)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            var productName = await _localizationService.GetLocalizedAsync(product, x => x.Name);
            //If a size has been set in the view, we use it in priority
            var pictureSize = productThumbPictureSize ?? _mediaSettings.ProductThumbPictureSize;

            //Alchub -- Get product picture from only cdn
            string fullSizeImageUrl = string.Empty;
            string imageUrl = string.Empty;
            var store = await _storeContext.GetCurrentStoreAsync();

            //if group product then try to get picture by it's assosiated product sku.
            if (product.ProductType == ProductType.GroupedProduct)
            {
                //associated products
                var associatedProducts = (await _productService.GetAssociatedProductsAsync(product.Id, store.Id,
                                                                                         //++Alchub geovendor
                                                                                         geoVendorIds: await _workContext.GetCurrentCustomerGeoRadiusVendorIdsAsync()))
                                                                                         ?.Where(gp => gp.ProductType == ProductType.SimpleProduct);
                if (associatedProducts.Any())
                {
                    imageUrl = associatedProducts.FirstOrDefault().ImageUrl;
                    fullSizeImageUrl = associatedProducts.FirstOrDefault().ImageUrl;
                }
            }

            if (string.IsNullOrEmpty(imageUrl))
            {
                imageUrl = product.ImageUrl;
                fullSizeImageUrl = product.ImageUrl;
            }

            var pictureModel = new Nop.Web.Models.Media.PictureModel
            {
                ImageUrl = imageUrl,
                FullSizeImageUrl = fullSizeImageUrl,
                //"title" attribute
                Title = string.Format(await _localizationService.GetResourceAsync("Media.Product.ImageLinkTitleFormat"),
                            productName),
                //"alt" attribute
                AlternateText = string.Format(await _localizationService.GetResourceAsync("Media.Product.ImageAlternateTextFormat"),
                            productName)
            };

            return pictureModel;
        }

        /// <summary>
        /// Value indicates wether product is availbale in customers wishlist or not.
        /// </summary>
        /// <param name="product"></param>
        /// <param name="customer"></param>
        /// <param name="storeId"></param>
        /// <returns></returns>
        private async Task<bool> IsProductInWishList(int productId, Customer customer, int storeId = 0)
        {
            var wishList = await _shoppingCartService.GetShoppingCartAsync(customer, Nop.Core.Domain.Orders.ShoppingCartType.Wishlist, storeId);
            return wishList.Any(x => x.ProductId == productId);

            //var items = _sciRepository.Table.Where(sci => sci.CustomerId == customerId);

            ////filter by type
            //items = items.Where(item => item.ShoppingCartTypeId == (int)Core.Domain.Orders.ShoppingCartType.Wishlist);

            ////filter shopping cart items by store
            //if (storeId > 0 && !_shoppingCartSettings.CartsSharedBetweenStores)
            //    items = items.Where(item => item.StoreId == storeId);

            ////filter shopping cart items by product
            //if (productId > 0)
            //    items = items.Where(item => item.ProductId == productId);

            //return items?.Any() ?? false;
        }

        /// <summary>
        /// Prepare the product overview price model
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="forceRedirectionAfterAddingToCart">Whether to force redirection after adding to cart</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the product overview price model
        /// </returns>
        protected virtual async Task<ProductPriceModel> PrepareProductOverviewPriceModelAsync(Product product, Customer customer)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            var priceModel = new ProductPriceModel();

            switch (product.ProductType)
            {
                case ProductType.GroupedProduct:
                    //grouped product
                    await PrepareGroupedProductOverviewPriceModelAsync(product, priceModel, customer);

                    break;
                case ProductType.SimpleProduct:
                default:
                    //simple product
                    await PrepareSimpleProductOverviewPriceModelAsync(product, priceModel, customer);

                    break;
            }

            return priceModel;
        }

        /// <returns>A task that represents the asynchronous operation</returns>
        protected virtual async Task PrepareSimpleProductOverviewPriceModelAsync(Product product, ProductPriceModel priceModel, Customer customer)
        {
            //price range
            await PrepareProductPriceRangeAsync(product, priceModel, customer);
        }

        /// <returns>A task that represents the asynchronous operation</returns>
        protected virtual async Task PrepareGroupedProductOverviewPriceModelAsync(Product product, ProductPriceModel priceModel, Customer customer)
        {
            var store = await _storeContext.GetCurrentStoreAsync();
            var associatedProducts = await _productService.GetAssociatedProductsAsync(product.Id,
                store.Id,
                //++Alchub geovendor
                geoVendorIds: await _workContext.GetCurrentCustomerGeoRadiusVendorIdsAsync(await _authenticationService.GetAuthenticatedCustomerAsync()));

            //price range
            await PrepareGroupedProductPriceRangeAsync(associatedProducts, priceModel, customer);
        }

        /// <returns>A task that represents the asynchronous operation</returns>
        protected virtual async Task PrepareProductPriceRangeAsync(Product product, ProductPriceModel priceModel, Customer customer)
        {
            //get price range
            var priceRangeDisc = await _alchubGeneralService.GetProductPriceRangeAsync(product, customer);
            if (priceRangeDisc != null && priceRangeDisc.Any())
            {
                //price range formate string
                //show lowest variant price - 24-08-22
                priceModel.Price = await _priceFormatter.FormatPriceAsync(priceRangeDisc.First().Value);
            }
        }

        /// <returns>A task that represents the asynchronous operation</returns>
        protected virtual async Task PrepareGroupedProductPriceRangeAsync(IList<Product> associatedProducts, ProductPriceModel priceModel, Customer customer)
        {
            if (!associatedProducts.Any())
                return;

            //groupproduct price = default variants-> sub products-> product with minimum price.
            var defaultAssociatedProduct = await _alchubGeneralService.GetGroupedProductDefaultVariantAsync(associatedProducts);

            if (defaultAssociatedProduct != null)
                await PrepareProductPriceRangeAsync(defaultAssociatedProduct, priceModel, customer);
        }

        /// <returns>A task that represents the asynchronous operation</returns>
        protected virtual async Task PrepareProductPriceRangeAsync(Product product, ShoppingCartItemDto shoppingCartItemDto, Customer customer)
        {
            //get price range
            var priceRangeDisc = await _alchubGeneralService.GetProductPriceRangeAsync(product, customer);
            if (priceRangeDisc != null && priceRangeDisc.Any())
            {
                shoppingCartItemDto.UnitPrice = await _priceFormatter.FormatPriceAsync(priceRangeDisc.First().Value);
                shoppingCartItemDto.UnitPriceValue = priceRangeDisc.First().Value;
            }
        }

        /// <returns>A task that represents the asynchronous operation</returns>
        protected virtual async Task PrepareGroupedProductPriceRangeAsync(IList<Product> associatedProducts, ShoppingCartItemDto shoppingCartItemDto, Customer customer)
        {
            if (!associatedProducts.Any())
                return;

            //groupproduct price = default variants-> sub products-> product with minimum price.
            var defaultAssociatedProduct = await _alchubGeneralService.GetGroupedProductDefaultVariantAsync(associatedProducts);

            if (defaultAssociatedProduct != null)
                await PrepareProductPriceRangeAsync(defaultAssociatedProduct, shoppingCartItemDto, customer);
        }

        //protected virtual async Task<string> PrepareProductFastestSlotAsync(Product product)
        //{
        //    string fastestSlotTiming = string.Empty;

        //    var availableVendors = await _vendorService.GetAvailableGeoFenceVendorsAsync((await _workContext.GetCurrentCustomerAsync()), true, product);
        //    //prepare availbale vendors productss
        //    var productVendorModel = await _apiProductModelFactory.PrepareProductVendorsAsync(product, availableVendors);
        //    var productVendors = productVendorModel.ProductDetailVendor;
        //    var vendorId = (await _alchubGeneralService.GetVendorByMasterProductIdAsync(product)).Select(x => x.Id).FirstOrDefault();
        //    var addDay = await _settingService.GetSettingByKeyAsync<int>("Alchub.Slot.Visible.After.Day", defaultValue: 1);
        //    var addHour = await _settingService.GetSettingByKeyAsync<int>("Alchub.Slot.Add.Hour", defaultValue: 0);
        //    var addMintues = await _settingService.GetSettingByKeyAsync<int>("Alchub.Slot.Add.Mintues", defaultValue: 0);
        //    var dateTime = await _dateTimeHelper.ConvertToUserTimeAsync(DateTime.Now);
        //    dateTime = dateTime.AddDays(addDay).AddHours(addHour).AddMinutes(addMintues);
        //    if (productVendors.Count > 0)
        //    {
        //        if (productVendors.FirstOrDefault().ManageDelivery)
        //        {
        //            var slot = await _apiProductModelFactory.PrepareTodaySlotListModel(0, dateTime.ToString(), dateTime.AddDays(7).ToString(), productVendors?.FirstOrDefault()?.ProductId ?? 0, productVendors?.FirstOrDefault()?.VendorId ?? 0, false, true, false);
        //            fastestSlotTiming = slot != null ? slot.Start.DayOfWeek.ToString() + "," + slot.Start.ToString("HH tt") + "-" + slot.End.ToString("HH tt") : "";
        //        }
        //        else if (productVendors.FirstOrDefault().PickAvailable)
        //        {
        //            var slot = await _apiProductModelFactory.PrepareTodaySlotListModel(0, dateTime.ToString(), dateTime.AddDays(7).ToString(), productVendors?.FirstOrDefault()?.ProductId ?? 0, productVendors?.FirstOrDefault()?.VendorId ?? 0, true, false, false);
        //            fastestSlotTiming = slot != null ? slot.Start.DayOfWeek.ToString() + "," + slot.Start.ToString("HH tt") + "-" + slot.End.ToString("HH tt") : "";
        //        }
        //        else
        //        {
        //            var slot = await _apiProductModelFactory.PrepareTodaySlotListModel(0, dateTime.ToString(), dateTime.AddDays(7).ToString(), productVendors?.FirstOrDefault()?.ProductId ?? 0, productVendors?.FirstOrDefault()?.VendorId ?? 0, false, false, true);
        //            fastestSlotTiming = slot != null ? slot.Start.DayOfWeek.ToString() + "," + slot.Start.ToString("HH tt") + "-" + slot.End.ToString("HH tt") : "";
        //        }
        //    }

        //    return fastestSlotTiming;
        //}

        #endregion

        #region Customer Reward Points List

        /// <summary>
        /// Prepare the customer reward points model
        /// </summary>
        /// <param name="customerRewardPointsModel">CustomerRewardPointsModel</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the customer reward points model
        /// </returns>
        public virtual async Task<CustomerRewardPointsDto> PrepareCustomerRewardPointsAsync(CustomerRewardPointsModel customerRewardPointsModel)
        {
            //get reward points history
            var storeId = _storeContext.GetCurrentStoreAsync().Id;
            var rewardPoints = await _rewardPointService.GetRewardPointsHistoryAsync(customerRewardPointsModel.UserId, storeId, true,
                pageIndex: customerRewardPointsModel.PageIndex ?? 0, pageSize: customerRewardPointsModel.PageSize ?? int.MaxValue);

            //prepare model
            var model = new CustomerRewardPointsDto
            {
                RewardPoints = await rewardPoints.SelectAwait(async historyEntry =>
                {
                    var activatingDate = await _dateTimeHelper.ConvertToUserTimeAsync(historyEntry.CreatedOnUtc, DateTimeKind.Utc);
                    return new CustomerRewardPointsDto.RewardPointsHistoryModel
                    {
                        UserId = historyEntry.CustomerId,
                        Points = historyEntry.Points,
                        PointsBalance = historyEntry.PointsBalance.HasValue ? historyEntry.PointsBalance.ToString()
                            : string.Format(await _localizationService.GetResourceAsync("RewardPoints.ActivatedLater"), activatingDate),
                        Message = historyEntry.Message,
                        CreatedOn = activatingDate,
                        EndDate = !historyEntry.EndDateUtc.HasValue ? null :
                            (DateTime?)(await _dateTimeHelper.ConvertToUserTimeAsync(historyEntry.EndDateUtc.Value, DateTimeKind.Utc))
                    };
                }).ToListAsync(),
            };

            //current amount/balance
            var rewardPointsBalance = await _rewardPointService.GetRewardPointsBalanceAsync(customerRewardPointsModel.UserId, storeId);
            var rewardPointsAmountBase = await _orderTotalCalculationService.ConvertRewardPointsToAmountAsync(rewardPointsBalance);
            var currentCurrency = await _workContext.GetWorkingCurrencyAsync();
            var rewardPointsAmount = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(rewardPointsAmountBase, currentCurrency);
            model.RewardPointsBalance = rewardPointsBalance;
            model.RewardPointsAmount = await _priceFormatter.FormatPriceAsync(rewardPointsAmount, true, false);

            //minimum amount/balance
            var minimumRewardPointsBalance = _rewardPointsSettings.MinimumRewardPointsToUse;
            var minimumRewardPointsAmountBase = await _orderTotalCalculationService.ConvertRewardPointsToAmountAsync(minimumRewardPointsBalance);
            var minimumRewardPointsAmount = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(minimumRewardPointsAmountBase, currentCurrency);
            model.MinimumRewardPointsBalance = minimumRewardPointsBalance;
            model.MinimumRewardPointsAmount = await _priceFormatter.FormatPriceAsync(minimumRewardPointsAmount, true, false);
            model.PageIndex = rewardPoints.PageIndex + 1;
            model.PageSize = rewardPoints.PageSize;
            model.TotalRecords = rewardPoints.TotalCount;

            return model;
        }

        #endregion

        #region ShoppingCart

        /// <summary>
        /// Prepare the order totals dto
        /// </summary>
        /// <param name="customer">customer</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the order totals model
        /// </returns>
        public virtual async Task<OrderTotalDto> PrepareOrderTotalsDtoAsync(Customer customer, Store store)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            if (store == null)
                throw new ArgumentNullException(nameof(store));

            var cart = await _shoppingCartService.GetShoppingCartAsync(customer, Nop.Core.Domain.Orders.ShoppingCartType.ShoppingCart, store.Id);

            var model = new OrderTotalDto();

            if (cart.Any())
            {
                //set currency
                var currentCurrency = await _customerApiService.GetCustomerCurrencyAsync(customer);
                if (currentCurrency is null)
                    currentCurrency = await _workContext.GetWorkingCurrencyAsync();

                //set lannguage
                Language currentLanguage = null;
                if (await _customerLanguage.Value is { Id: var languageId })
                    currentLanguage = await _languageService.GetLanguageByIdAsync(languageId);
                if (currentLanguage is null)
                    currentLanguage = await _workContext.GetWorkingLanguageAsync();

                //subtotal
                var subTotalIncludingTax = await _workContext.GetTaxDisplayTypeAsync() == TaxDisplayType.IncludingTax && !_taxSettings.ForceTaxExclusionFromOrderSubtotal;
                var (orderSubTotalDiscountAmountBase, _, subTotalWithoutDiscountBase, _, _) = await _orderTotalCalculationService.GetShoppingCartSubTotalAsync(cart, subTotalIncludingTax);
                var subtotalBase = subTotalWithoutDiscountBase;
                var subtotal = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(subtotalBase, currentCurrency);
                model.SubTotal = await _priceFormatter.FormatPriceAsync(subtotal, true, currentCurrency, currentLanguage.Id, subTotalIncludingTax);

                //subtotal-discount
                if (orderSubTotalDiscountAmountBase > decimal.Zero)
                {
                    var orderSubTotalDiscountAmount = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(orderSubTotalDiscountAmountBase, currentCurrency);
                    model.SubTotalDiscount = await _priceFormatter.FormatPriceAsync(-orderSubTotalDiscountAmount, true, currentCurrency, currentLanguage.Id, subTotalIncludingTax);
                }

                /*Alchub Start*/
                //Delivery fee
                var vendorWiseDeliveryFees = await _deliveryFeeService.GetVendorWiseDeliveryFeeAsync(cart);

                decimal deliveryFee = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync
                    (vendorWiseDeliveryFees?.Sum(x => x.DeliveryFeeValue) ?? decimal.Zero, currentCurrency);

                model.DeliveryFee = await _priceFormatter.FormatShippingPriceAsync(deliveryFee, true);

                if (vendorWiseDeliveryFees != null)
                {
                    vendorWiseDeliveryFees.ToList().ForEach(async x =>
                    {
                        var deliveryFee = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(x.DeliveryFeeValue, currentCurrency);

                        model.VendorWiseDeliveryFees.Add(
                        new VendorWiseDeliveryFeeDto
                        {
                            VendorId = x.VendorId,
                            VendorName = x.VendorName,
                            DeliveryFeeValue = deliveryFee,
                            DeliveryFee = await _priceFormatter.FormatShippingPriceAsync(deliveryFee, true)
                        });
                    });
                }

                //Tip fee
                //subtotal without tax
                var (_, _, _, subTotalWithDiscountBase, _) = await _orderTotalCalculationService.GetShoppingCartSubTotalAsync(cart, false);
                var vendorWiseTipFees = await _tipFeeService.GetVendorWiseTipFeeAsync(cart, subTotalWithDiscountBase);

                decimal tipFee = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync
                    (vendorWiseTipFees?.Sum(x => x.TipFeeValue) ?? decimal.Zero, currentCurrency);

                model.TipFee = await _priceFormatter.FormatShippingPriceAsync(tipFee, true);

                if (vendorWiseTipFees != null)
                {
                    vendorWiseTipFees.ToList().ForEach(async x =>
                    {
                        var tipFee = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(x.TipFeeValue, currentCurrency);

                        model.VendorWiseTipFees.Add(
                        new VendorWiseTipFeeDto
                        {
                            VendorId = x.VendorId,
                            VendorName = x.VendorName,
                            TipFeeValue = tipFee,
                            TipFee = await _priceFormatter.FormatShippingPriceAsync(tipFee, true)
                        });
                    });
                }

                var customerTipFeeDetails = await _tipFeeService.GetCustomerTipFeeDetailsAsync();
                model.TipTypeId = customerTipFeeDetails.Item1;
                model.CustomTipAmount = customerTipFeeDetails.Item2;

                //Add Tip types
                model.AvailableTipTypes.Add(new SelectListItem { Text = "10%", Value = "10" });
                model.AvailableTipTypes.Add(new SelectListItem { Text = "15%", Value = "15" });
                model.AvailableTipTypes.Add(new SelectListItem { Text = "20%", Value = "20" });
                model.AvailableTipTypes.Add(new SelectListItem { Text = (await _localizationService.GetResourceAsync("Alchub.TipFee.Custom.Text")), Value = "0" });


                //service fee
                var serviceFee = await _serviceFeeManager.GetServiceFeeAsync(subtotal);
                model.ServiceFee = await _priceFormatter.FormatPriceAsync(serviceFee, true, false);

                //slot fee
                var slotFee = await PrepareSlotTotalAsync(cart);
                model.SlotFee = await _priceFormatter.FormatPriceAsync(slotFee);
                model.SlotWiseFee = await PrepareSlotListAsync(cart);

                /*Alchub End*/

                //shipping info
                var shoppingCartShippingBase = await _orderTotalCalculationService.GetShoppingCartShippingTotalAsync(cart);
                if (shoppingCartShippingBase.HasValue)
                {
                    var shoppingCartShipping = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(shoppingCartShippingBase.Value, currentCurrency);
                    model.Shipping = await _priceFormatter.FormatShippingPriceAsync(shoppingCartShipping, true);
                }
                else
                    model.Shipping = await _localizationService.GetResourceAsync("Nop.Api.ShoppingCart.Totals.CalculatedDuringCheckout");

                //tax
                var (shoppingCartTaxBase, taxRates) = await _orderTotalCalculationService.GetTaxTotalAsync(cart);
                var shoppingCartTax = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(shoppingCartTaxBase, currentCurrency);
                model.Tax = await _priceFormatter.FormatPriceAsync(shoppingCartTax, true, false);


                //total
                var (shoppingCartTotalBase, orderTotalDiscountAmountBase, _, appliedGiftCards, redeemedRewardPoints, redeemedRewardPointsAmount) = await _orderTotalCalculationService.GetShoppingCartTotalAsync(cart);
                if (shoppingCartTotalBase.HasValue)
                {
                    var shoppingCartTotal = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(shoppingCartTotalBase.Value, currentCurrency);
                    model.OrderTotal = await _priceFormatter.FormatPriceAsync(shoppingCartTotal, true, false);
                }
                else
                    model.OrderTotal = await _localizationService.GetResourceAsync("Nop.Api.ShoppingCart.Totals.CalculatedDuringCheckout");

                //discount
                if (orderTotalDiscountAmountBase > decimal.Zero)
                {
                    var orderTotalDiscountAmount = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(orderTotalDiscountAmountBase, currentCurrency);
                    model.OrderTotalDiscount = await _priceFormatter.FormatPriceAsync(-orderTotalDiscountAmount, true, false);
                }

                //gift cards
                var (giftCardDtos, totalGiftDiscount) = await PrepareGiftCardDtos(appliedGiftCards, currentCurrency);
                model.GiftCards = giftCardDtos;
                model.GiftCardTotalDiscount = totalGiftDiscount;
            }

            //shipping required
            model.ShippingRequired = await _shoppingCartService.ShoppingCartRequiresShippingAsync(cart);

            return model;
        }

        /// <summary>
        /// Prepare discountbox dto
        /// </summary>
        /// <param name="customer"></param>
        /// <returns></returns>
        public async Task<DiscountBoxDto> PrepareDiscountBoxDtoAsync(Customer customer)
        {
            var model = new DiscountBoxDto();
            //discount and gift card boxes
            model.Display = _shoppingCartSettings.ShowDiscountBox;
            var discountCouponCodes = await _customerService.ParseAppliedDiscountCouponCodesAsync(customer);
            foreach (var couponCode in discountCouponCodes)
            {
                var discount = await (await _discountService.GetAllDiscountsAsync(couponCode: couponCode))
                    .FirstOrDefaultAwaitAsync(async d => d.RequiresCouponCode && (await _discountService.ValidateDiscountAsync(d, customer)).IsValid);

                if (discount != null)
                {
                    model.AppliedDiscountsWithCodes.Add(new DiscountBoxDto.DiscountInfoDto
                    {
                        Id = discount.Id,
                        CouponCode = discount.CouponCode
                    });
                }
            }

            return model;
        }

        /// <summary>
        /// Prepare giftcard box dto
        /// </summary>
        /// <param name="customer"></param>
        /// <param name="store"></param>
        /// <returns></returns>
        public async Task<GiftCardBoxDto> PrepareGiftCardBoxDto(Customer customer, Store store)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            if (store == null)
                throw new ArgumentNullException(nameof(store));

            var model = new GiftCardBoxDto()
            {
                Display = _shoppingCartSettings.ShowGiftCardBox
            };

            var cart = await _shoppingCartService.GetShoppingCartAsync(customer, Nop.Core.Domain.Orders.ShoppingCartType.ShoppingCart, store.Id);

            if (cart.Any())
            {
                //set currency
                var currentCurrency = await _customerApiService.GetCustomerCurrencyAsync(customer);
                if (currentCurrency is null)
                    currentCurrency = await _workContext.GetWorkingCurrencyAsync();

                //get applied giftcards
                var (_, _, _, appliedGiftCards, _, _) = await _orderTotalCalculationService.GetShoppingCartTotalAsync(cart);

                //gift cards
                var (giftCardDtos, totalGiftDiscount) = await PrepareGiftCardDtos(appliedGiftCards, currentCurrency);
                model.GiftCards = giftCardDtos;
                model.GiftCardTotalDiscount = totalGiftDiscount;
            }

            return model;
        }

        /// <summary>
        /// Prepare gift card dtos list
        /// </summary>
        /// <param name="appliedGiftCards"></param>
        /// <returns></returns>
        private async Task<(IList<GiftCardDto>, string totalGiftCardAmount)> PrepareGiftCardDtos(List<AppliedGiftCard> appliedGiftCards, Currency currentCurrency)
        {
            if (currentCurrency is null)
                currentCurrency = await _workContext.GetWorkingCurrencyAsync();

            var giftCardDtos = new List<GiftCardDto>();
            var totalGiftCardAmount = string.Empty;

            //gift cards
            if (appliedGiftCards != null && appliedGiftCards.Any())
            {
                foreach (var appliedGiftCard in appliedGiftCards)
                {
                    var gcModel = new GiftCardDto
                    {
                        Id = appliedGiftCard.GiftCard.Id,
                        CouponCode = appliedGiftCard.GiftCard.GiftCardCouponCode,
                    };
                    var amountCanBeUsed = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(appliedGiftCard.AmountCanBeUsed, currentCurrency);
                    gcModel.Amount = await _priceFormatter.FormatPriceAsync(-amountCanBeUsed, true, false);

                    var remainingAmountBase = await _giftCardService.GetGiftCardRemainingAmountAsync(appliedGiftCard.GiftCard) - appliedGiftCard.AmountCanBeUsed;
                    var remainingAmount = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(remainingAmountBase, currentCurrency);
                    gcModel.Remaining = await _priceFormatter.FormatPriceAsync(remainingAmount, true, false);

                    giftCardDtos.Add(gcModel);
                }
                //gift total discount
                var totalGiftDiscountBase = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(appliedGiftCards.Sum(x => x.AmountCanBeUsed), currentCurrency);
                totalGiftCardAmount = await _priceFormatter.FormatPriceAsync(totalGiftDiscountBase, true, false);
            }

            return (giftCardDtos, totalGiftCardAmount);
        }

        #endregion

        #region Filter

        private class AllFilterParametors
        {
            public AllFilterParametors()
            {
                SelectedCategoryIds = new List<int>();
                FinalFilteredCategoryIds = new List<int>();
                AllCategories = new List<Category>();
            }
            public ProductsListModel ProductsListModel { get; set; }
            public IList<int> SelectedCategoryIds { get; set; }
            public IList<int> FinalFilteredCategoryIds { get; set; }
            public IList<Category> AllCategories { get; set; }
        }

        private async Task<AllFilterParametors> PrepareAllFilterInitialParametors(int categoryId, int? orderById)
        {
            //param model
            var productsListModel = new ProductsListModel();
            //productsListModel.CustomerId = customer.Id;
            productsListModel.CategoryId = categoryId;
            productsListModel.OrderBy = orderById;

            //prepare required params
            var store = await _storeContext.GetCurrentStoreAsync();

            //prepar categoryIds
            var categoryIds = new List<int>();
            var selectedCategoryId = productsListModel.CategoryId;
            if (selectedCategoryId > 0)
                categoryIds.Add(selectedCategoryId);

            if (selectedCategoryId > 0 && productsListModel.SubCategoryIds != null && productsListModel.SubCategoryIds.Any())
            {
                categoryIds.AddRange(productsListModel.SubCategoryIds);
            }
            else
            {
                //include subcategories
                if (_catalogSettings.ShowProductsFromSubcategories)
                {
                    var tempCategoryIds = new List<int>();
                    foreach (var id in categoryIds)
                    {
                        tempCategoryIds.AddRange(await _categoryService.GetChildCategoryIdsAsync(id, store.Id));
                    }
                    categoryIds.AddRange(tempCategoryIds);
                }
            }
            categoryIds = categoryIds.Distinct().ToList();

            //assign categoryIds to make filter check work correctly
            var filteredCategoryIds = new List<int>();
            filteredCategoryIds.AddRange(categoryIds);

            //prepare final category to be included in search query
            var allCategories = await _categoryService.GetAllCategoriesAsync(storeId: store.Id);
            if (selectedCategoryId > 0 && productsListModel.SubCategoryIds != null && productsListModel.SubCategoryIds.Any() && _catalogSettings.ShowProductsFromSubcategories)
            {
                var removeCategoryIds = new List<int>();
                if (productsListModel.SubCategoryIds != null && productsListModel.SubCategoryIds.Any())
                    removeCategoryIds.Add(selectedCategoryId);

                foreach (var id in categoryIds)
                {
                    var category = allCategories.FirstOrDefault(x => x.Id == id);
                    if (category != null)
                    {
                        if (category.ParentCategoryId > 0 && !removeCategoryIds.Contains(category.ParentCategoryId))
                            removeCategoryIds.Add(category.ParentCategoryId);
                    }
                }
                //flush category ids
                if (removeCategoryIds.Contains(0))
                    removeCategoryIds.Remove(0);

                removeCategoryIds = removeCategoryIds.Distinct().ToList();

                //final category ids
                categoryIds.RemoveAll(x => removeCategoryIds.Contains(x));

                //add chile categories for option
                //include subcategories
                if (_catalogSettings.ShowProductsFromSubcategories)
                {
                    var tempCategoryIds = new List<int>();
                    foreach (var id in categoryIds)
                    {
                        tempCategoryIds.AddRange(await _categoryService.GetChildCategoryIdsAsync(id, store.Id));
                    }
                    categoryIds.AddRange(tempCategoryIds);
                }
            }

            //prepare model
            var paramModel = new AllFilterParametors
            {
                ProductsListModel = productsListModel,
                SelectedCategoryIds = filteredCategoryIds,
                FinalFilteredCategoryIds = categoryIds,
                AllCategories = allCategories,
            };

            return paramModel;
        }

        public async Task<CatalogProductsModel> PrepareAllFilters(int selectedCategoyId, int? orderById, List<int> availableVendorIds)
        {
            //var vendorIds = (await _workContext.GetCurrentCustomerGeoRadiusVendorIdsAsync(customer))?.ToList();
            availableVendorIds = availableVendorIds?.OrderBy(x => x)?.ToList(); //for caching purpose

            //Note:(29-05-23) Caching implemented as api taking too much time every time, but we Caching cleare management is not possible with all these param.
            //So proper caching management is not done for this. Removed caching only at category level.
            var cacheKey = _staticCacheManager.PrepareKeyForDefaultCache(NopCatalogDefaults.ApiAllFilterDataKey, selectedCategoyId, availableVendorIds);

            //assign cache time if defined
            if (_alchubSettings.AllFilterApiCacheTime > 0 && _alchubSettings.AllFilterApiCacheTime <= 1440) //max 24h
                cacheKey.CacheTime = _alchubSettings.AllFilterApiCacheTime;

            var allFilterData = await _staticCacheManager.GetAsync(cacheKey, async () =>
            {
                //prepare initial parametors
                var initialParametor = await PrepareAllFilterInitialParametors(selectedCategoyId, orderById);
                var finalFilteredCategoryIds = initialParametor.FinalFilteredCategoryIds;
                var productsListModel = initialParametor.ProductsListModel;
                var allCategories = initialParametor.AllCategories;

                CatalogProductsModel allfilter = new CatalogProductsModel();
                var language = await _workContext.GetWorkingLanguageAsync();

                ////prepare required param to get product count
                //if (customer == null)
                //    customer = await _authenticationService.GetAuthenticatedCustomerAsync();

                //note: This categoryIds will be final categoryIds which will be use to get product listing & count.
                var categoryIds = finalFilteredCategoryIds;

                //if (productsListModel.VendorIds != null && productsListModel.VendorIds.Any())
                //    availableVendorIds.AddRange(productsListModel.VendorIds);
                //else
                //    vendorIds.AddRange(await _workContext.GetCurrentCustomerGeoRadiusVendorIdsAsync(customer));

                var manufacturerIds = new List<int>();
                if (productsListModel.ManufacturerIds != null && productsListModel.ManufacturerIds.Any())
                    manufacturerIds.AddRange(productsListModel.ManufacturerIds);

                decimal? priceFrom = null;
                decimal? priceTo = null;
                if (productsListModel.From != null && productsListModel.To != null)
                {
                    priceFrom = Convert.ToDecimal(productsListModel.From);
                    priceTo = Convert.ToDecimal(productsListModel.To);
                }

                // Sorting Start

                //get active sorting options
                var activeSortingOptionsIds = Enum.GetValues(typeof(ProductSortingEnum)).Cast<int>()
                    .Except(_catalogSettings.ProductSortingEnumDisabled).ToList();

                if (activeSortingOptionsIds?.Any() == true)
                {
                    //order sorting options
                    var orderedActiveSortingOptions = activeSortingOptionsIds
                        .Select(id => new { Id = id, Order = _catalogSettings.ProductSortingEnumDisplayOrder.TryGetValue(id, out var order) ? order : id })
                        .OrderBy(option => option.Order).ToList();

                    var orderBy = productsListModel.OrderBy ?? orderedActiveSortingOptions.FirstOrDefault().Id;
                    //prepare available model sorting options
                    foreach (var option in orderedActiveSortingOptions)
                    {
                        allfilter.AvailableSortOptions.Add(new SelectListItem
                        {
                            Text = await _localizationService.GetLocalizedEnumAsync((ProductSortingEnum)option.Id),
                            Value = option.Id.ToString(),
                            Selected = option.Id == orderBy
                        });
                    }
                }
                //End

                //// View Modes Start
                ////grid
                //allfilter.AvailableViewModes.Add(new SelectListItem
                //{
                //    Text = await _localizationService.GetResourceAsync("Catalog.ViewMode.Grid"),
                //    Value = "grid"
                //});
                ////list
                //allfilter.AvailableViewModes.Add(new SelectListItem
                //{
                //    Text = await _localizationService.GetResourceAsync("Catalog.ViewMode.List"),
                //    Value = "list"
                //});
                //// End 

                //Specification Filter Start
                var filterableOptions = new List<SpecificationAttributeOption>();
                if (categoryIds != null && categoryIds.Any())
                {
                    var specificationFilter = new SpecificationFilterModel();
                    foreach (var categoryId in categoryIds)
                    {
                        var filterOptions = await _specificationAttributeService
                          .GetFiltrableSpecificationAttributeOptionsByCategoryIdAsync(categoryId);

                        filterableOptions.AddRange(filterOptions);
                    }

                    if (filterableOptions?.Any() == true)
                    {
                        filterableOptions = filterableOptions.DistinctBy(fo => fo.Id).ToList();
                        specificationFilter.Enabled = true;

                        var workingLanguage = await _workContext.GetWorkingLanguageAsync();

                        foreach (var option in filterableOptions)
                        {
                            var attributeFilter = specificationFilter.Attributes.FirstOrDefault(model => model.Id == option.SpecificationAttributeId);
                            if (attributeFilter == null)
                            {
                                var attribute = await _specificationAttributeService
                                    .GetSpecificationAttributeByIdAsync(option.SpecificationAttributeId);
                                attributeFilter = new SpecificationAttributeFilterModel
                                {
                                    Id = attribute.Id,
                                    Name = await _localizationService
                                        .GetLocalizedAsync(attribute, x => x.Name, workingLanguage.Id)
                                };
                                specificationFilter.Attributes.Add(attributeFilter);
                            }

                            attributeFilter.Values.Add(new SpecificationAttributeValueFilterModel
                            {
                                Id = option.Id,
                                Name = await _localizationService
                                    .GetLocalizedAsync(option, x => x.Name, workingLanguage.Id),
                                //Selected = productsListModel.SpecificationOptionIds?.Any(optionId => optionId == option.Id) == true,
                                //ColorSquaresRgb = option.ColorSquaresRgb
                            });
                        }
                        allfilter.SpecificationFilter = specificationFilter;
                    }
                }
                //End

                // Manufacturers Start
                if (_catalogSettings.EnableManufacturerFiltering)
                {
                    //var availableManufacturers = await _manufacturerService.GetManufacturersByCategoryIdAsync(productsListModel.CategoryId);
                    //var manufacturerFilter = new ManufacturerFilterModel();

                    //if (availableManufacturers?.Any() == true)
                    //{
                    //    manufacturerFilter.Enabled = true;

                    //    var language = await _workContext.GetWorkingLanguageAsync();

                    //    foreach (var manufacturer in availableManufacturers)
                    //    {
                    //        manufacturerFilter.Manufacturers.Add(new SelectListItem
                    //        {
                    //            Value = manufacturer.Id.ToString(),
                    //            Text = await _localizationService
                    //                .GetLocalizedAsync(manufacturer, x => x.Name, language.Id)
                    //        });
                    //    }
                    //}
                    //allfilter.ManufacturerFilter = manufacturerFilter;

                    var manufacturers = new List<Manufacturer>();
                    if (!categoryIds.Any())
                        manufacturers.AddRange(await _manufacturerService.GetManufacturersByIdsAsync(manufacturerIds.ToArray()));
                    else
                    {
                        foreach (var catId in categoryIds)
                            manufacturers.AddRange(await _manufacturerService.GetManufacturersByCategoryIdAsync(catId));
                    }

                    var manufacturerFilter = new ManufacturerFilterModel();
                    if (manufacturers.Any())
                    {
                        manufacturers = manufacturers.DistinctBy(m => m.Id).ToList();
                        manufacturerFilter.Enabled = true;


                        foreach (var manufacturer in manufacturers)
                        {
                            manufacturerFilter.Manufacturers.Add(new SelectListItem
                            {
                                Value = manufacturer.Id.ToString(),
                                Text = await _localizationService
                                    .GetLocalizedAsync(manufacturer, x => x.Name, language.Id),
                                Selected = productsListModel.ManufacturerIds?
                                .Any(manufacturerId => manufacturerId == manufacturer.Id) == true
                            });
                        }
                    }
                    allfilter.ManufacturerFilter = manufacturerFilter;
                }
                //End

                //Note: Commenting as we are not using price range in filter - 22-05-23
                //// Price Filter Start
                //if (productsListModel.CategoryId > 0)
                //{
                //    var priceRange = new PriceRangeFilterModel();

                //    var availablePriceRangeMax = await _productApiService.GetProductsPriceMaxAsync(productsListModel.CategoryId);

                //    var availablePriceRangeMin = await _productApiService.GetProductsPriceMinAsync(productsListModel.CategoryId);

                //    priceRange.Enabled = true;
                //    priceRange.AvailablePriceRange.From = availablePriceRangeMin > decimal.Zero ? Math.Floor(availablePriceRangeMin)
                //        : decimal.Zero;
                //    priceRange.AvailablePriceRange.To = Math.Ceiling(availablePriceRangeMax);

                //    allfilter.PriceRangeFilter = priceRange;
                //}

                //vendor start
                //get vendors who provides delivery 
                //var availableDeliverableVendors = await _vendorService.GetAvailableGeoFenceVendorsAsync(customer, false, null);

                //get available vendors
                var availableVendors = await _vendorService.GetVendorsByIdsAsync(availableVendorIds?.ToArray());
                var vendorFilterModels = new List<VendorFilterModel>();
                foreach (var vendor in availableVendors)
                {
                    vendorFilterModels.Add(new VendorFilterModel()
                    {
                        Id = vendor.Id,
                        Name = vendor.Name
                    });
                }
                allfilter.Vendors = vendorFilterModels;
                //vendor End

                //++Category Start++

                //specs
                var filteredSpecs = productsListModel.SpecificationOptionIds is null ? null : filterableOptions.Where(fo => productsListModel.SpecificationOptionIds.Contains(fo.Id)).ToList();

                //get categories
                //var allCategories = (await _categoryApiService.GetCategories(publishedStatus: true))?.ToList();
                var currentStore = await _storeContext.GetCurrentStoreAsync();
                if (allCategories == null || !allCategories.Any())
                    allCategories = (await _categoryService.GetAllCategoriesAsync(currentStore.Id)).ToList(); // use cached default service for performace optimization. 22-05-23

                //category filter
                allfilter.FilterCategoriesObject = await PrepareFilterCategoryHierarchyAsync(finalFilteredCategoryIds, allCategories?.ToList(), 0, manufacturerIds, availableVendorIds, selectedCategoyId,
                    priceFrom, priceTo, productsListModel.Keyword, filteredSpecs, currentStore);

                ////prepare hirachy model
                //var categoryHirachyModel = await PrepareCategoryHierarchyAsync(categoriesAsDtos, 0);

                //allfilter.CategoriesHierarchybject = categoryHirachyModel;

                //--Category End--

                //++Price options

                //There are 3 options available for now
                var prOptionValue1 = _catalogSettings.FilterPriceRangeOption1 > 0 ? _catalogSettings.FilterPriceRangeOption1 : 10;
                var prOptionValue2 = _catalogSettings.FilterPriceRangeOption2 > 0 ? _catalogSettings.FilterPriceRangeOption2 : 20;
                var prOptionValue3 = _catalogSettings.FilterPriceRangeOption3 > 0 ? _catalogSettings.FilterPriceRangeOption3 : 30;
                var priceRangeOption1 = string.Format(await _localizationService.GetResourceAsync("Alchub.Catalog.Filter.PriceRange.Option.Text"), prOptionValue1);
                var priceRangeOption2 = string.Format(await _localizationService.GetResourceAsync("Alchub.Catalog.Filter.PriceRange.Option.Text"), prOptionValue2);
                var priceRangeOption3 = string.Format(await _localizationService.GetResourceAsync("Alchub.Catalog.Filter.PriceRange.Option.Text"), prOptionValue3);

                var priceOptions = new List<SelectListItem>
            {
                new SelectListItem { Text = priceRangeOption1, Value = prOptionValue1.ToString(), Selected = productsListModel?.To == prOptionValue1 },
                new SelectListItem { Text = priceRangeOption2, Value = prOptionValue2.ToString(), Selected = productsListModel?.To == prOptionValue2 },
                new SelectListItem { Text = priceRangeOption3, Value = prOptionValue3.ToString(), Selected = productsListModel?.To == prOptionValue3 }
            };

                allfilter.PriceOptions = priceOptions;

                //--Price options

                return allfilter;
            });

            return allFilterData;
        }

        /// <summary>
        /// Prepare mobile category hierachy with filter values
        /// </summary>
        /// <param name="selectedCategoriesIds"></param>
        /// <param name="categoriesDtos"></param>
        /// <param name="rootCategoryId"></param>
        /// <param name="manufacturerIds"></param>
        /// <param name="vendorIds"></param>
        /// <param name="priceMin"></param>
        /// <param name="priceMax"></param>
        /// <param name="keywords"></param>
        /// <param name="filteredSpecOptions"></param>
        /// <returns></returns>
        public async Task<List<CategoryHierarchyModel>> PrepareFilterCategoryHierarchyAsync(IList<int> selectedCategoriesIds, List<CategoryDto> categoriesDtos, int rootCategoryId,
            IList<int> manufacturerIds, IList<int> vendorIds, decimal? priceMin = null,
            decimal? priceMax = null,
            string keywords = null,
            IList<SpecificationAttributeOption> filteredSpecOptions = null)
        {
            var result = new List<CategoryHierarchyModel>();
            var currentStore = await _storeContext.GetCurrentStoreAsync();
            //root categories
            var categories = categoriesDtos.Where(c => c.ParentCategoryId == rootCategoryId).OrderBy(c => c.DisplayOrder).ToList();
            foreach (var category in categories)
            {
                //prepare model
                var categoryModel = category.ToCategoryHierarchyModel();

                //filter options values
                var numberOfProducts = new int?();

                if (_catalogSettings.ShowCategoryProductNumber)
                {
                    var categoryIds = new List<int> { category.Id };
                    //include subcategories
                    if (_catalogSettings.ShowCategoryProductNumberIncludingSubcategories)
                        categoryIds.AddRange(
                            await _categoryService.GetChildCategoryIdsAsync(category.Id, currentStore.Id));

                    numberOfProducts = await _productService.GetNumberOfProductsInCategoryByGeoVendorIdsAsync(categoryIds, currentStore.Id,
                        manufacturerIds, vendorIds, priceMin, priceMax, keywords, filteredSpecOptions, true);
                }
                categoryModel.IsSelected = selectedCategoriesIds?.Any(categoryId => categoryId == category.Id) == true;
                categoryModel.NumberOfProducts = numberOfProducts;

                //load sub categories (recursion)
                var subCategories = await PrepareFilterCategoryHierarchyAsync(selectedCategoriesIds, categoriesDtos, category.Id, manufacturerIds, vendorIds, priceMin, priceMax, keywords, filteredSpecOptions);
                categoryModel.SubCategories.AddRange(subCategories);

                result.Add(categoryModel);
            }

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="selectedCategoriesIds"></param>
        /// <param name="allCategories"></param>
        /// <param name="rootCategoryId"></param>
        /// <param name="manufacturerIds"></param>
        /// <param name="vendorIds"></param>
        /// <param name="priceMin"></param>
        /// <param name="priceMax"></param>
        /// <param name="keywords"></param>
        /// <param name="filteredSpecOptions"></param>
        /// <returns></returns>
        public async Task<List<FilterCategoryHierarchyModel>> PrepareFilterCategoryHierarchyAsync(
            IList<int> selectedCategoriesIds,
            List<Category> allCategories,
            int rootCategoryId,
            IList<int> manufacturerIds,
            IList<int> vendorIds,
            int selectedCategoryId,
            decimal? priceMin = null,
            decimal? priceMax = null,
            string keywords = null,
            IList<SpecificationAttributeOption> filteredSpecOptions = null,
            Store currentStore = null)
        {
            var result = new List<FilterCategoryHierarchyModel>();

            if (allCategories == null || allCategories.Count == 0)
                return result;

            //root categories
            var categories = allCategories.Where(c => c.ParentCategoryId == rootCategoryId).OrderBy(c => c.DisplayOrder).ToList();

            foreach (var category in categories)
            {
                var categoryModel = new FilterCategoryHierarchyModel
                {
                    Id = category.Id,
                    Name = category.Name
                };

                // localization
                if (await _customerLanguage.Value is { Id: var languageId })
                {
                    categoryModel.Name = await _localizationService.GetLocalizedAsync(category, x => x.Name, languageId);
                }

                //Performance improvement: mobile app devs only showing product count for sub-categories of the selected category. So lets not calculate count for rest of the other categories. 01-06-23
                var isChildCategoryOfSelectedBaseCategory = category.ParentCategoryId == selectedCategoryId;

                //filter options values
                if (_catalogSettings.ShowCategoryProductNumber && isChildCategoryOfSelectedBaseCategory)
                {
                    var categoryIds = new List<int> { category.Id };
                    //include subcategories
                    if (_catalogSettings.ShowCategoryProductNumberIncludingSubcategories)
                        categoryIds.AddRange(
                            await _categoryService.GetChildCategoryIdsAsync(category.Id, currentStore.Id));

                    categoryModel.NumberOfProducts = await _productService.GetNumberOfProductsInCategoryByGeoVendorIdsAsync(categoryIds, currentStore.Id,
                        manufacturerIds, vendorIds, priceMin, priceMax, keywords, filteredSpecOptions, true);
                }

                categoryModel.IsSelected = selectedCategoriesIds?.Any(categoryId => categoryId == category.Id) == true;

                //load sub categories (recursion)
                var subCategories = await PrepareFilterCategoryHierarchyAsync(selectedCategoriesIds, allCategories, category.Id, manufacturerIds, vendorIds, selectedCategoryId, priceMin, priceMax, keywords, filteredSpecOptions, currentStore);
                categoryModel.SubCategories.AddRange(subCategories);

                result.Add(categoryModel);
            }

            return result;
        }

        #endregion

        #region Checkout

        /// <summary>
        /// Prepare checkout shipping method dto
        /// </summary>
        /// <param name="cart"></param>
        /// <param name="shippingAddress"></param>
        /// <param name="customer"></param>
        /// <param name="store"></param>
        /// <returns></returns>
        public async Task<CheckoutShippingMethodDto> PrepareCheckoutShippingMethodDtoAsync(IList<ShoppingCartItem> cart, Address shippingAddress, Customer customer, Store store)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            if (store == null)
                throw new ArgumentNullException(nameof(store));

            var model = new CheckoutShippingMethodDto();

            //set currency
            var currentCurrency = await _customerApiService.GetCustomerCurrencyAsync(customer);
            if (currentCurrency is null)
                currentCurrency = await _workContext.GetWorkingCurrencyAsync();

            var getShippingOptionResponse = await _shippingService.GetShippingOptionsAsync(cart, shippingAddress, customer, storeId: store.Id);
            if (getShippingOptionResponse.Success)
            {
                //performance optimization. cache returned shipping options.
                //we'll use them later (after a customer has selected an option).
                await _genericAttributeService.SaveAttributeAsync(customer,
                                                       NopCustomerDefaults.OfferedShippingOptionsAttribute,
                                                       getShippingOptionResponse.ShippingOptions,
                                                       store.Id);

                foreach (var shippingOption in getShippingOptionResponse.ShippingOptions)
                {
                    var soModel = new CheckoutShippingMethodDto.ShippingMethodDto
                    {
                        Name = shippingOption.Name,
                        Description = shippingOption.Description,
                        DisplayOrder = shippingOption.DisplayOrder ?? 0,
                        ShippingRateComputationMethodSystemName = shippingOption.ShippingRateComputationMethodSystemName,
                    };

                    //adjust rate
                    var (shippingTotal, _) = await _orderTotalCalculationService.AdjustShippingRateAsync(shippingOption.Rate, cart, shippingOption.IsPickupInStore);

                    var (rateBase, _) = await _taxService.GetShippingPriceAsync(shippingTotal, customer);
                    var rate = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(rateBase, currentCurrency);
                    soModel.Fee = await _priceFormatter.FormatShippingPriceAsync(rate, true);
                    soModel.Rate = rate;
                    model.ShippingMethods.Add(soModel);
                }

                //sort shipping methods
                if (model.ShippingMethods.Count > 1)
                {
                    model.ShippingMethods = (_shippingSettings.ShippingSorting switch
                    {
                        ShippingSortingEnum.ShippingCost => model.ShippingMethods.OrderBy(option => option.Rate),
                        _ => model.ShippingMethods.OrderBy(option => option.DisplayOrder)
                    }).ToList();
                }
            }
            else
            {
                foreach (var error in getShippingOptionResponse.Errors)
                    model.Warnings.Add(error);
            }

            return model;
        }

        /// <summary>
        /// Prepare payment method dto
        /// </summary>
        /// <param name="cart">Cart</param>
        /// <param name="filterByCountryId">Filter by country identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the payment method model
        /// </returns>
        public async Task<CheckoutPaymentMethodDto> PrepareCheckoutPaymentMethodDtoAsync(IList<ShoppingCartItem> cart, int filterByCountryId, Customer customer, Store store)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            if (store == null)
                throw new ArgumentNullException(nameof(store));

            var model = new CheckoutPaymentMethodDto();

            //set currency
            var currentCurrency = await _customerApiService.GetCustomerCurrencyAsync(customer);
            if (currentCurrency is null)
                currentCurrency = await _workContext.GetWorkingCurrencyAsync();

            //set lannguage
            Language currentLanguage = null;
            if (await _customerLanguage.Value is { Id: var languageId })
                currentLanguage = await _languageService.GetLanguageByIdAsync(languageId);
            if (currentLanguage is null)
                currentLanguage = await _workContext.GetWorkingLanguageAsync();

            //reward points (note: not included)

            //filter by country
            var paymentMethods = await (await _paymentPluginManager
                .LoadActivePluginsAsync(customer, store.Id, filterByCountryId))
                .Where(pm => pm.PaymentMethodType == PaymentMethodType.Standard || pm.PaymentMethodType == PaymentMethodType.Redirection)
                .WhereAwait(async pm => !await pm.HidePaymentMethodAsync(cart))
                .ToListAsync();

            //get restricted payment method systemnames
            var apiSetting = await _settingService.LoadSettingAsync<ApiSettings>();
            foreach (var pm in paymentMethods)
            {
                if (await _shoppingCartService.ShoppingCartIsRecurringAsync(cart) && pm.RecurringPaymentType == RecurringPaymentType.NotSupported)
                    continue;

                //check restricted payment method list
                if (apiSetting.Restricted_PaymentMethodsSystemNames != null &&
                    apiSetting.Restricted_PaymentMethodsSystemNames.Any() &&
                    !apiSetting.Restricted_PaymentMethodsSystemNames.Contains(pm.PluginDescriptor.SystemName))
                    continue;

                var pmModel = new CheckoutPaymentMethodDto.PaymentMethodDto
                {
                    Name = await _localizationService.GetLocalizedFriendlyNameAsync(pm, currentLanguage.Id),
                    Description = await pm.GetPaymentMethodDescriptionAsync(),
                    PaymentMethodSystemName = pm.PluginDescriptor.SystemName,
                    LogoUrl = await _paymentPluginManager.GetPluginLogoUrlAsync(pm)
                };
                //payment method additional fee
                var paymentMethodAdditionalFee = await _paymentService.GetAdditionalHandlingFeeAsync(cart, pm.PluginDescriptor.SystemName);
                var (rateBase, _) = await _taxService.GetPaymentMethodAdditionalFeeAsync(paymentMethodAdditionalFee, customer);
                var rate = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(rateBase, currentCurrency);

                if (rate > decimal.Zero)
                    pmModel.Fee = await _priceFormatter.FormatPaymentMethodAdditionalFeeAsync(rate, true);

                model.PaymentMethods.Add(pmModel);
            }

            return model;

        }

        #endregion

        #region Local Resources

        /// <summary>
        /// Prepare local resource values
        /// </summary>
        /// <param name="language"></param>
        /// <param name="resourceNamePrefix"></param>
        /// <returns></returns>
        public async Task<LocalResourceDto> PrepareLocalResourceDtosAsync(Language language, string resourceNamePrefix = "")
        {
            if (language == null)
                throw new ArgumentNullException(nameof(language));

            //get locale resources - we know they are cached
            var localeResources = (await _localizationService.GetAllResourceValuesAsync(language.Id, loadPublicLocales: null))
                .OrderBy(localeResource => localeResource.Key).AsQueryable();

            //filter locale resources by prefix
            if (!string.IsNullOrEmpty(resourceNamePrefix))
                localeResources = localeResources.Where(l => l.Key.ToLowerInvariant().StartsWith(resourceNamePrefix.ToLowerInvariant()));

            //prepare model.
            var model = new LocalResourceDto()
            {
                LanguageId = language.Id
            };
            //disctionary
            foreach (var locale in localeResources)
            {
                if (!model.ResourseValues.ContainsKey(locale.Key))
                    model.ResourseValues.Add(new KeyValuePair<string, string>(locale.Key, locale.Value.Value));
            }

            return model;
        }

        #endregion

        #region JCarousel

        /// <summary>
        /// Prepare jcarousel dtos 
        /// </summary>
        /// <param name="customer"></param>
        /// <returns></returns>
        public async Task<IList<JCarouselDto>> PrepareJCarouselDTOsAsync(Customer customer)
        {
            //check jcarousel plugin is active.
            if (!await _widgetPluginManager.IsPluginActiveAsync(JCarouselDefaults.SystemName, customer))
                return new List<JCarouselDto>();

            //list of jacrousel ids
            var jcarousels = await _jCarouselService.GetAllJcarouselIds();
            if (jcarousels == null || !jcarousels.Any())
                return new List<JCarouselDto>();

            ////get jcarsouel records
            //var jcarousels = new List<JCarouselLog>();
            //foreach (var id in jcarouselIds)
            //{
            //    var jcarousel = await _jCarouselService.GetJcarouselByIdAsync(id);
            //    if (jcarousel != null)
            //        jcarousels.Add(jcarousel);
            //}

            //prepare jcarousel dtos
            var jcarsouelDtos = new List<JCarouselDto>();

            //lets take advantage of web development method, to prepare api dtos
            var jcarouselModels = await _publicJCarouselModelFactory.PrepareJcarouselOverviewModelsAsync(jcarousels, customer);
            foreach (var model in jcarouselModels)
            {
                //check for products
                if (!model.JcarouselProductsModel.Products.Any())
                    continue;

                var jcarouselDto = new JCarouselDto()
                {
                    Id = model.Id,
                    Name = model.Name,
                    DisplayOrder = model.DisplayOrder,
                    CategoryId = model.CategoryId,
                    ParentCategoryId = model.ParentCategoryId,
                    ParentCategoryName = model.ParentCategoryName
                };

                //prepare products
                foreach (var productOverviewModel in model.JcarouselProductsModel.Products)
                {
                    var product = await _productService.GetProductByIdAsync(productOverviewModel.Id);
                    var productListDto = product.ToListDto();

                    //assign model values
                    productListDto.Name = productOverviewModel.Name;
                    productListDto.Price = productOverviewModel.ProductPrice.PriceRange;
                    productListDto.PictureUrl = productOverviewModel.DefaultPictureModel.ImageUrl;
                    productListDto.FullSizePictureUrl = productOverviewModel.DefaultPictureModel.FullSizeImageUrl;
                    productListDto.Size = productOverviewModel.Size;
                    productListDto.Container = productOverviewModel.Container;
                    productListDto.FastestSlotTime = ""; //Pending (Individual API pending.)
                    //favorite
                    productListDto.IsInFavorite = await IsProductInWishList(product.Id, customer);

                    decimal ratingAvg = 0;
                    if (productListDto.ApprovedRatingSum != null && productListDto.ApprovedRatingSum != 0)
                        ratingAvg = ((decimal)productListDto.ApprovedRatingSum / (decimal)productListDto.ApprovedTotalReviews);
                    ratingAvg = Math.Round(ratingAvg, 1);
                    productListDto.RatingAvg = ratingAvg;
                    //pictures (optional)
                    //var productPictures = await _productService.GetProductPicturesByProductIdAsync(product.Id);
                    //await PrepareProductImagesAsync(productPictures, productListDto);

                    jcarouselDto.Products.Add(productListDto);
                }

                jcarsouelDtos.Add(jcarouselDto);
            }

            return jcarsouelDtos;
        }

        /// <summary>
        /// Prepare jcarousel info dtos 
        /// </summary>
        /// <param name="customer"></param>
        /// <returns></returns>
        public async Task<IList<JCarouselInfoDto>> PrepareJCarouselInfoDTOsAsync(Customer customer)
        {
            //check jcarousel plugin is active.
            if (!await _widgetPluginManager.IsPluginActiveAsync(JCarouselDefaults.SystemName, customer))
                return new List<JCarouselInfoDto>();

            //list of jacrousel ids
            var jcarousels = await _jCarouselService.GetAllJcarouselIds();
            if (jcarousels == null || !jcarousels.Any())
                return new List<JCarouselInfoDto>();

            //prepare list
            var jcarsouelDtos = jcarousels.Select(c =>
            {
                return new JCarouselInfoDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    DisplayOrder = c.DisplayOrder,
                    DataSourceTypeId = c.DataSourceTypeId,
                };
            })?.ToList();

            return jcarsouelDtos ?? new List<JCarouselInfoDto>();
        }

        /// <summary>
        /// Prepare jcarousel dto
        /// </summary>
        /// <param name="customer"></param>
        /// <returns></returns>
        public async Task<JCarouselDto> PrepareJCarouselDTOAsync(Customer customer, JCarouselLog jCarouselLog)
        {
            ////check jcarousel plugin is active.
            //if (!await _widgetPluginManager.IsPluginActiveAsync(JCarouselDefaults.SystemName, customer))
            //    return null;

            if (jCarouselLog == null)
                throw new ArgumentNullException(nameof(jCarouselLog));

            //list of jacrousels
            var jcarousels = new List<JCarouselLog>() { jCarouselLog };

            //prepare jcarousel dtos
            var jcarsouelDtos = new List<JCarouselDto>();

            //lets take advantage of web development method, to prepare api dtos
            var jcarouselModels = await _publicJCarouselModelFactory.PrepareJcarouselOverviewModelsAsync(jcarousels, customer);
            foreach (var model in jcarouselModels)
            {
                //check for products
                if (!model.JcarouselProductsModel.Products.Any())
                    continue;

                var jcarouselDto = new JCarouselDto()
                {
                    Id = model.Id,
                    Name = model.Name,
                    DisplayOrder = model.DisplayOrder,
                    CategoryId = model.CategoryId,
                    ParentCategoryId = model.ParentCategoryId,
                    ParentCategoryName = model.ParentCategoryName
                };

                //prepare products
                foreach (var productOverviewModel in model.JcarouselProductsModel.Products)
                {
                    var product = await _productService.GetProductByIdAsync(productOverviewModel.Id);
                    var productListDto = product.ToListDto();

                    //assign model values
                    productListDto.Name = productOverviewModel.Name;
                    productListDto.Price = productOverviewModel.ProductPrice.PriceRange;
                    productListDto.PictureUrl = productOverviewModel.DefaultPictureModel.ImageUrl;
                    productListDto.FullSizePictureUrl = productOverviewModel.DefaultPictureModel.FullSizeImageUrl;
                    productListDto.Size = productOverviewModel.Size;
                    productListDto.Container = productOverviewModel.Container;
                    productListDto.FastestSlotTime = ""; //Pending (Individual API pending.)
                    //favorite
                    productListDto.IsInFavorite = productOverviewModel.ProductPrice.IsWishlist;

                    decimal ratingAvg = 0;
                    if (productListDto.ApprovedRatingSum != null && productListDto.ApprovedRatingSum != 0)
                        ratingAvg = ((decimal)productListDto.ApprovedRatingSum / (decimal)productListDto.ApprovedTotalReviews);
                    ratingAvg = Math.Round(ratingAvg, 1);
                    productListDto.RatingAvg = ratingAvg;

                    jcarouselDto.Products.Add(productListDto);
                }

                jcarsouelDtos.Add(jcarouselDto);
            }

            return jcarsouelDtos?.FirstOrDefault();
        }

        #endregion

        #region Alchub Shopping Cart

        private async Task<decimal> PrepareSlotTotalAsync(IList<ShoppingCartItem> cart)
        {
            decimal customerSlotFee = 0;
            var slotList = cart.GroupBy(x => new { x.SlotId, x.IsPickup }).Select(x => new BookingSlotModel.SlotDefault { Id = x.Key.SlotId, IsPickup = x.Key.IsPickup }).ToList();
            foreach (var item in slotList)
            {
                if (item.IsPickup)
                {
                    var pickupSlot = await _slotService.GetPickupSlotById(item.Id);
                    customerSlotFee += pickupSlot != null ? pickupSlot.Price : 0;
                }
                else
                {
                    var slot = await _slotService.GetSlotById(item.Id);
                    customerSlotFee += slot != null ? slot.Price : 0;
                }

            }
            return customerSlotFee;
        }

        private async Task<IList<SlotWiseFeeDto>> PrepareSlotListAsync(IList<ShoppingCartItem> cart)
        {
            IList<SlotWiseFeeDto> slotFeeModels = new List<SlotWiseFeeDto>();
            var slotList = cart.GroupBy(x => new { x.SlotId, x.IsPickup }).Select(x => new BookingSlotModel.SlotDefault { Id = x.Key.SlotId, IsPickup = x.Key.IsPickup }).ToList();
            foreach (var item in slotList)
            {
                if (item.IsPickup)
                {
                    var pickupSlot = await _slotService.GetPickupSlotById(item.Id);
                    if (pickupSlot != null)
                    {
                        var cartItem = cart.FirstOrDefault(x => x.SlotId == item.Id);
                        var product = await _productService.GetProductByIdAsync(cartItem.ProductId);
                        var vendor = await _vendorService.GetVendorByIdAsync(product.VendorId);
                        SlotWiseFeeDto slotFeeModel = new SlotWiseFeeDto();
                        slotFeeModel.SlotFee = await _priceFormatter.FormatPriceAsync(pickupSlot.Price, true, false);
                        slotFeeModel.VendorName = vendor != null ? vendor.Name : "";
                        slotFeeModels.Add(slotFeeModel);
                    }
                }
                else
                {
                    var slot = await _slotService.GetSlotById(item.Id);
                    if (slot != null)
                    {
                        var cartItem = cart.FirstOrDefault(x => x.SlotId == item.Id);
                        var product = await _productService.GetProductByIdAsync(cartItem.ProductId);
                        var vendor = await _vendorService.GetVendorByIdAsync(product.VendorId);
                        SlotWiseFeeDto slotFeeModel = new SlotWiseFeeDto();
                        slotFeeModel.SlotFee = await _priceFormatter.FormatPriceAsync(slot.Price, true, false);
                        slotFeeModel.VendorName = vendor != null ? vendor.Name : "";
                        slotFeeModels.Add(slotFeeModel);
                    }
                }

            }
            return slotFeeModels;
        }


        private async Task<IList<SlotWiseFeeDto>> PrepareOrderSlotListAsync(Order order)
        {
            IList<SlotWiseFeeDto> slotFeeModels = new List<SlotWiseFeeDto>();
            var orderitem = await _orderService.GetOrderItemsAsync(order.Id);
            var slotList = orderitem.GroupBy(x => new { x.SlotId, x.InPickup }).Select(x => new BookingSlotModel.SlotDefault { Id = x.Key.SlotId, IsPickup = x.Key.InPickup }).ToList();
            foreach (var item in slotList)
            {
                if (item.IsPickup)
                {
                    var pickupSlot = await _slotService.GetPickupSlotById(item.Id);
                    if (pickupSlot != null)
                    {
                        var cartItem = orderitem.FirstOrDefault(x => x.SlotId == item.Id);
                        var product = await _productService.GetProductByIdAsync(cartItem.ProductId);
                        var vendor = await _vendorService.GetVendorByIdAsync(product.VendorId);
                        SlotWiseFeeDto slotFeeModel = new SlotWiseFeeDto();
                        slotFeeModel.SlotFee = await _priceFormatter.FormatPriceAsync(pickupSlot.Price, true, false);
                        slotFeeModel.VendorName = vendor != null ? vendor.Name : "";
                        slotFeeModels.Add(slotFeeModel);
                    }
                }
                else
                {
                    var slot = await _slotService.GetSlotById(item.Id);
                    if (slot != null)
                    {
                        var cartItem = orderitem.FirstOrDefault(x => x.SlotId == item.Id);
                        var product = await _productService.GetProductByIdAsync(cartItem.ProductId);
                        var vendor = await _vendorService.GetVendorByIdAsync(product.VendorId);
                        SlotWiseFeeDto slotFeeModel = new SlotWiseFeeDto();
                        slotFeeModel.SlotFee = await _priceFormatter.FormatPriceAsync(slot.Price, true, false);
                        slotFeeModel.VendorName = vendor != null ? vendor.Name : "";
                        slotFeeModels.Add(slotFeeModel);
                    }
                }

            }
            return slotFeeModels;
        }
        #endregion
    }
}