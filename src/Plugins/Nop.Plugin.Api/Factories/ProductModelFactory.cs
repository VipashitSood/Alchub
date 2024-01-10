using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Alchub.Domain;
using Nop.Core.Alchub.Domain.Google;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.DeliveryFees;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Seo;
using Nop.Core.Domain.Slots;
using Nop.Core.Domain.Vendors;
using Nop.Plugin.Api.Cache;
using Nop.Plugin.Api.DTO.Images;
using Nop.Plugin.Api.MappingExtensions;
using Nop.Plugin.Api.Models.ProductsParameters;
using Nop.Plugin.Api.Models.Slots;
using Nop.Services.Alchub.General;
using Nop.Services.Alchub.Slots;
using Nop.Services.Authentication;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.DeliveryFees;
using Nop.Services.Directory;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Shipping.Date;
using Nop.Services.Slots;
using Nop.Services.Tax;
using Nop.Services.Vendors;
using static Nop.Plugin.Api.Models.Slots.BookingSlotModel;
using PictureModel = Nop.Plugin.Api.Models.ProductsParameters.PictureModel;

namespace Nop.Plugin.Api.Factories
{
    /// <summary>
    /// Represents the product model factory
    /// </summary>
    public partial class ProductModelFactory : IProductModelFactory
    {
        #region Fields

        private readonly IShoppingCartService _shoppingCartService;
        private readonly VendorSettings _vendorSettings;
        private readonly CatalogSettings _catalogSettings;
        private readonly ICurrencyService _currencyService;
        private readonly ICustomerService _customerService;
        private readonly IDateRangeService _dateRangeService;
        private readonly IDownloadService _downloadService;
        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;
        private readonly IPictureService _pictureService;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly IProductAttributeService _productAttributeService;
        private readonly IProductService _productService;
        private readonly ISpecificationAttributeService _specificationAttributeService;
        private readonly IStaticCacheManager _staticCacheManager;
        private readonly IStoreContext _storeContext;
        private readonly ITaxService _taxService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IWebHelper _webHelper;
        private readonly IWorkContext _workContext;
        private readonly MediaSettings _mediaSettings;
        private readonly OrderSettings _orderSettings;
        private readonly SeoSettings _seoSettings;
        private readonly IAlchubGeneralService _alchubGeneralService;
        private readonly IDeliveryFeeService _deliveryFeeService;
        private readonly ISlotService _slotService;
        private readonly ICategoryService _categoryService;
        private readonly ISettingService _settingService;
        private readonly IVendorService _vendorService;
        private CustomerProductSlot cutomerProductSlot;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly Web.Factories.IProductModelFactory _productModelFactory;
        private readonly IAuthenticationService _authenticationService;
        private readonly AlchubSettings _alchubSettings;
        #endregion

        #region Ctor

        public ProductModelFactory(
            IShoppingCartService shoppingCartService,
            VendorSettings vendorSettings,
            CatalogSettings catalogSettings,
            ICurrencyService currencyService,
            ICustomerService customerService,
            IDateRangeService dateRangeService,
            IDownloadService downloadService,
            ILocalizationService localizationService,
            IPermissionService permissionService,
            IPictureService pictureService,
            IPriceCalculationService priceCalculationService,
            IPriceFormatter priceFormatter,
            IProductAttributeParser productAttributeParser,
            IProductAttributeService productAttributeService,
            IProductService productService,
            ISpecificationAttributeService specificationAttributeService,
            IStaticCacheManager staticCacheManager,
            IStoreContext storeContext,
            ITaxService taxService,
            IUrlRecordService urlRecordService,
            IWebHelper webHelper,
            IWorkContext workContext,
            MediaSettings mediaSettings,
            OrderSettings orderSettings,
            SeoSettings seoSettings,
            IAlchubGeneralService alchubGeneralService,
            IDeliveryFeeService deliveryFeeService,
            ISlotService slotService,
            ICategoryService categoryService,
            ISettingService settingService,
            IVendorService vendorService,
            IDateTimeHelper dateTimeHelper,
            Nop.Web.Factories.IProductModelFactory productModelFactory,
            IAuthenticationService authenticationService,
            AlchubSettings alchubSettings)
        {
            _shoppingCartService = shoppingCartService;
            _vendorSettings = vendorSettings;
            _catalogSettings = catalogSettings;
            _currencyService = currencyService;
            _customerService = customerService;
            _dateRangeService = dateRangeService;
            _downloadService = downloadService;
            _localizationService = localizationService;
            _permissionService = permissionService;
            _pictureService = pictureService;
            _priceCalculationService = priceCalculationService;
            _priceFormatter = priceFormatter;
            _productAttributeParser = productAttributeParser;
            _productAttributeService = productAttributeService;
            _productService = productService;
            _specificationAttributeService = specificationAttributeService;
            _staticCacheManager = staticCacheManager;
            _storeContext = storeContext;
            _taxService = taxService;
            _urlRecordService = urlRecordService;
            _webHelper = webHelper;
            _workContext = workContext;
            _mediaSettings = mediaSettings;
            _orderSettings = orderSettings;
            _seoSettings = seoSettings;
            _alchubGeneralService = alchubGeneralService;
            _deliveryFeeService = deliveryFeeService;
            _slotService = slotService;
            _categoryService = categoryService;
            _settingService = settingService;
            _vendorService = vendorService;
            _dateTimeHelper = dateTimeHelper;
            _productModelFactory = productModelFactory;
            _authenticationService = authenticationService;
            _alchubSettings = alchubSettings;
        }

        #endregion

        #region Utilities

        protected virtual async Task<DateTime?> PrepareVendorAvailableSlotsAsync(int productId, int vendorId, bool pickUp = false, bool delivery = false, bool deliveryAvailable = false)
        {
            var addDay = await _settingService.GetSettingByKeyAsync<int>("Alchub.Slot.Visible.After.Day", defaultValue: 1);
            var addHour = await _settingService.GetSettingByKeyAsync<int>("Alchub.Slot.Add.Hour", defaultValue: 0);
            var addMintues = await _settingService.GetSettingByKeyAsync<int>("Alchub.Slot.Add.Mintues", defaultValue: 0);
            var dateTime = await _dateTimeHelper.ConvertToUserTimeAsync(DateTime.Now);

            //available slots
            dateTime = dateTime.AddDays(addDay).AddHours(addHour).AddMinutes(addMintues);
            //dateTime = dateTime.AddDays(addDay).AddHours(addHour).AddMinutes(addMintues);
            IList<BookingSlotModel> model = new List<BookingSlotModel>();
            DateTime? startTime = null;
            var slots = new List<dynamic>();
            var dynamicSlots = slots.AsEnumerable();
            if (deliveryAvailable)
            {
                //define manage delivery 
                if (delivery)
                {
                    //model = await PreparepareSlotListModel(0, dateTime.ToString(), dateTime.AddDays(7).ToString(),
                    //   productId, vendorId, false);

                    ////return model.Any() ? model.Select(x => x.SlotModel?.FirstOrDefault(s => s.IsAvailable)?.Start)?.FirstOrDefault() : null;
                    //startTime = model.Any() ? model.Select(x => x.SlotModel?.FirstOrDefault(s => s.IsAvailable)?.Start)?.FirstOrDefault() : null;

                    //new (used web method to get StartTime)
                    dynamicSlots = await _productModelFactory.PreparepareSlotListModel(0, dateTime.ToString(), dateTime.AddDays(7).ToString(),
                      productId, vendorId, false);
                    foreach (var slotsDate in dynamicSlots)
                    {
                        foreach (var item in slotsDate)
                        {
                            if (item.IsAvailable && item.IsBook)
                                startTime = item.Start;
                        }
                    }
                }
                else
                {
                    //model = await PreparepareAdminSlotListModel(0, dateTime.ToString(), dateTime.AddDays(7).ToString(),
                    //productId, vendorId, false);

                    ////return model.Any() ? model.Select(x => x.SlotModel?.FirstOrDefault(s => s.IsAvailable)?.Start)?.FirstOrDefault() : null;
                    //startTime = model.Any() ? model.Select(x => x.SlotModel?.FirstOrDefault(s => s.IsAvailable)?.Start)?.FirstOrDefault() : null;

                    //new (used web method to get StartTime)
                    dynamicSlots = await _productModelFactory.PreparepareAdminSlotListModel(0, dateTime.ToString(), dateTime.AddDays(7).ToString(),
                    productId, vendorId, false);
                    foreach (var slotsDate in dynamicSlots)
                    {
                        foreach (var item in slotsDate)
                        {
                            if (item.IsAvailable && item.IsBook)
                                startTime = item.Start;
                        }
                    }
                }
            }

            if (!startTime.HasValue && pickUp)
            {
                //model = await PrepareparePickupSlotListModel(0, dateTime.ToString(), dateTime.AddDays(7).ToString(),
                //                productId, vendorId, true);

                ////return model.Any() ? model.Select(x => x.SlotModel?.FirstOrDefault(s => s.IsAvailable)?.Start)?.FirstOrDefault() : null;
                //startTime = model.Any() ? model.Select(x => x.SlotModel?.FirstOrDefault(s => s.IsAvailable)?.Start)?.FirstOrDefault() : null;

                //new (used web method to get StartTime)
                dynamicSlots = await _productModelFactory.PrepareparePickupSlotListModel(0, dateTime.ToString(), dateTime.AddDays(7).ToString(),
                                productId, vendorId, true);
                foreach (var slotsDate in dynamicSlots)
                {
                    foreach (var item in slotsDate)
                    {
                        if (item.IsAvailable && item.IsBook)
                            startTime = item.Start;
                    }
                }
            }

            //return model.Any() ? model.Select(x => x.SlotModel?.FirstOrDefault(s => s.IsAvailable)?.Start)?.FirstOrDefault() : null;
            return startTime;
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

        /// <summary>
        /// Prepare the product review overview model
        /// </summary>
        /// <param name="product">Product</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the product review overview model
        /// </returns>
        protected virtual async Task<ProductReviewOverviewModel> PrepareProductReviewOverviewModelAsync(Product product)
        {
            ProductReviewOverviewModel productReview;
            var currentStore = await _storeContext.GetCurrentStoreAsync();

            if (_catalogSettings.ShowProductReviewsPerStore)
            {
                var cacheKey = _staticCacheManager.PrepareKeyForDefaultCache(NopModelCacheDefaults.ProductReviewsModelKey, product, currentStore);

                productReview = await _staticCacheManager.GetAsync(cacheKey, async () =>
                {
                    var productReviews = await _productService.GetAllProductReviewsAsync(productId: product.Id, approved: true, storeId: currentStore.Id);

                    return new ProductReviewOverviewModel
                    {
                        RatingSum = productReviews.Sum(pr => pr.Rating),
                        TotalReviews = productReviews.Count
                    };
                });
            }
            else
            {
                productReview = new ProductReviewOverviewModel
                {
                    RatingSum = product.ApprovedRatingSum,
                    TotalReviews = product.ApprovedTotalReviews
                };
            }

            if (productReview != null)
            {
                productReview.ProductId = product.Id;
                productReview.AllowCustomerReviews = product.AllowCustomerReviews;
                productReview.CanAddNewReview = await _productService.CanAddReviewAsync(product.Id, _catalogSettings.ShowProductReviewsPerStore ? currentStore.Id : 0);
            }

            decimal ratingAvg = 0;
            ratingAvg = productReview.RatingSum / (decimal)productReview.TotalReviews;
            ratingAvg = Math.Round(ratingAvg, 1);
            productReview.RatingAvg = ratingAvg;

            return productReview;
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
        protected virtual async Task<ProductOverviewModel.ProductPriceModel> PrepareProductOverviewPriceModelAsync(Product product, bool forceRedirectionAfterAddingToCart = false)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            var priceModel = new ProductOverviewModel.ProductPriceModel
            {
                ForceRedirectionAfterAddingToCart = forceRedirectionAfterAddingToCart
            };

            switch (product.ProductType)
            {
                case ProductType.GroupedProduct:
                    //grouped product
                    await PrepareGroupedProductOverviewPriceModelAsync(product, priceModel);

                    break;
                case ProductType.SimpleProduct:
                default:
                    //simple product
                    await PrepareSimpleProductOverviewPriceModelAsync(product, priceModel);

                    break;
            }

            return priceModel;
        }

        /// <returns>A task that represents the asynchronous operation</returns>
        protected virtual async Task PrepareSimpleProductOverviewPriceModelAsync(Product product, ProductOverviewModel.ProductPriceModel priceModel)
        {
            //add to cart button
            priceModel.DisableBuyButton = product.DisableBuyButton ||
                                          !await _permissionService.AuthorizeAsync(StandardPermissionProvider.EnableShoppingCart) ||
                                          !await _permissionService.AuthorizeAsync(StandardPermissionProvider.DisplayPrices);

            //add to wishlist button
            priceModel.DisableWishlistButton = product.DisableWishlistButton ||
                                               !await _permissionService.AuthorizeAsync(StandardPermissionProvider.EnableWishlist) ||
                                               !await _permissionService.AuthorizeAsync(StandardPermissionProvider.DisplayPrices);
            //compare products
            priceModel.DisableAddToCompareListButton = !_catalogSettings.CompareProductsEnabled;

            //rental
            priceModel.IsRental = product.IsRental;

            //pre-order
            if (product.AvailableForPreOrder)
            {
                priceModel.AvailableForPreOrder = !product.PreOrderAvailabilityStartDateTimeUtc.HasValue ||
                                                  product.PreOrderAvailabilityStartDateTimeUtc.Value >=
                                                  DateTime.UtcNow;
                priceModel.PreOrderAvailabilityStartDateTimeUtc = product.PreOrderAvailabilityStartDateTimeUtc;
            }

            //prices
            if (await _permissionService.AuthorizeAsync(StandardPermissionProvider.DisplayPrices))
            {
                if (product.CustomerEntersPrice)
                    return;

                if (product.CallForPrice &&
                    //also check whether the current user is impersonated
                    (!_orderSettings.AllowAdminsToBuyCallForPriceProducts ||
                     _workContext.OriginalCustomerIfImpersonated == null))
                {
                    //call for price
                    priceModel.OldPrice = null;
                    priceModel.OldPriceValue = null;
                    priceModel.Price = await _localizationService.GetResourceAsync("Products.CallForPrice");
                    priceModel.PriceValue = null;
                }
                else
                {
                    //prices
                    var customer = await _authenticationService.GetAuthenticatedCustomerAsync();
                    var (minPossiblePriceWithoutDiscount, minPossiblePriceWithDiscount, _, _) = await _priceCalculationService.GetFinalPriceAsync(product, customer);

                    if (product.HasTierPrices)
                    {
                        var (tierPriceMinPossiblePriceWithoutDiscount, tierPriceMinPossiblePriceWithDiscount, _, _) = await _priceCalculationService.GetFinalPriceAsync(product, customer, quantity: int.MaxValue);

                        //calculate price for the maximum quantity if we have tier prices, and choose minimal
                        minPossiblePriceWithoutDiscount = Math.Min(minPossiblePriceWithoutDiscount, tierPriceMinPossiblePriceWithoutDiscount);
                        minPossiblePriceWithDiscount = Math.Min(minPossiblePriceWithDiscount, tierPriceMinPossiblePriceWithDiscount);
                    }

                    var (oldPriceBase, _) = await _taxService.GetProductPriceAsync(product, product.OldPrice);
                    var (finalPriceWithoutDiscountBase, _) = await _taxService.GetProductPriceAsync(product, minPossiblePriceWithoutDiscount);
                    var (finalPriceWithDiscountBase, _) = await _taxService.GetProductPriceAsync(product, minPossiblePriceWithDiscount);
                    var currentCurrency = await _workContext.GetWorkingCurrencyAsync();
                    var oldPrice = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(oldPriceBase, currentCurrency);
                    var finalPriceWithoutDiscount = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(finalPriceWithoutDiscountBase, currentCurrency);
                    var finalPriceWithDiscount = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(finalPriceWithDiscountBase, currentCurrency);

                    //do we have tier prices configured?
                    var tierPrices = new List<TierPrice>();
                    if (product.HasTierPrices)
                    {
                        var store = await _storeContext.GetCurrentStoreAsync();
                        tierPrices.AddRange(await _productService.GetTierPricesAsync(product, customer, store.Id));
                    }
                    //When there is just one tier price (with  qty 1), there are no actual savings in the list.
                    var displayFromMessage = tierPrices.Any() && !(tierPrices.Count == 1 && tierPrices[0].Quantity <= 1);
                    if (displayFromMessage)
                    {
                        priceModel.OldPrice = null;
                        priceModel.OldPriceValue = null;
                        priceModel.Price = string.Format(await _localizationService.GetResourceAsync("Products.PriceRangeFrom"), await _priceFormatter.FormatPriceAsync(finalPriceWithDiscount));
                        priceModel.PriceValue = finalPriceWithDiscount;
                    }
                    else
                    {
                        var strikeThroughPrice = decimal.Zero;

                        if (finalPriceWithoutDiscountBase != oldPriceBase && oldPriceBase > decimal.Zero)
                            strikeThroughPrice = oldPrice;

                        if (finalPriceWithoutDiscountBase != finalPriceWithDiscountBase)
                            strikeThroughPrice = finalPriceWithoutDiscount;

                        if (strikeThroughPrice > decimal.Zero)
                        {
                            priceModel.OldPrice = await _priceFormatter.FormatPriceAsync(strikeThroughPrice);
                            priceModel.OldPriceValue = strikeThroughPrice;
                        }

                        priceModel.Price = await _priceFormatter.FormatPriceAsync(finalPriceWithDiscount);
                        priceModel.PriceValue = finalPriceWithDiscount;
                    }

                    if (product.IsRental)
                    {
                        //rental product
                        priceModel.OldPrice = await _priceFormatter.FormatRentalProductPeriodAsync(product, priceModel.OldPrice);
                        priceModel.OldPriceValue = priceModel.OldPriceValue;
                        priceModel.Price = await _priceFormatter.FormatRentalProductPeriodAsync(product, priceModel.Price);
                        priceModel.PriceValue = priceModel.PriceValue;
                    }

                    //property for German market
                    //we display tax/shipping info only with "shipping enabled" for this product
                    //we also ensure this it's not free shipping
                    priceModel.DisplayTaxShippingInfo = _catalogSettings.DisplayTaxShippingInfoProductBoxes && product.IsShipEnabled && !product.IsFreeShipping;

                    //PAngV default baseprice (used in Germany)
                    priceModel.BasePricePAngV = await _priceFormatter.FormatBasePriceAsync(product, finalPriceWithDiscount);
                    priceModel.BasePricePAngVValue = finalPriceWithDiscount;
                }
            }
            else
            {
                //hide prices
                priceModel.OldPrice = null;
                priceModel.OldPriceValue = null;
                priceModel.Price = null;
                priceModel.PriceValue = null;
            }
        }

        /// <returns>A task that represents the asynchronous operation</returns>
        protected virtual async Task PrepareGroupedProductOverviewPriceModelAsync(Product product, ProductOverviewModel.ProductPriceModel priceModel)
        {
            var customer = await _authenticationService.GetAuthenticatedCustomerAsync();
            var store = await _storeContext.GetCurrentStoreAsync();
            var associatedProducts = await _productService.GetAssociatedProductsAsync(product.Id, store.Id,
                                                                                             //++Alchub geovendor
                                                                                             geoVendorIds: await _workContext.GetCurrentCustomerGeoRadiusVendorIdsAsync(customer));

            //var associatedProducts = await _productService.GetAssociatedProductsAsync(product.Id,
            //    store.Id);

            //add to cart button (ignore "DisableBuyButton" property for grouped products)
            priceModel.DisableBuyButton =
                !await _permissionService.AuthorizeAsync(StandardPermissionProvider.EnableShoppingCart) ||
                !await _permissionService.AuthorizeAsync(StandardPermissionProvider.DisplayPrices);

            //add to wishlist button (ignore "DisableWishlistButton" property for grouped products)
            priceModel.DisableWishlistButton =
                !await _permissionService.AuthorizeAsync(StandardPermissionProvider.EnableWishlist) ||
                !await _permissionService.AuthorizeAsync(StandardPermissionProvider.DisplayPrices);

            //compare products
            priceModel.DisableAddToCompareListButton = !_catalogSettings.CompareProductsEnabled;
            if (!associatedProducts.Any())
                return;

            //we have at least one associated product
            if (await _permissionService.AuthorizeAsync(StandardPermissionProvider.DisplayPrices))
            {
                //find a minimum possible price
                decimal? minPossiblePrice = null;
                Product minPriceProduct = null;
                foreach (var associatedProduct in associatedProducts)
                {
                    var (_, tmpMinPossiblePrice, _, _) = await _priceCalculationService.GetFinalPriceAsync(associatedProduct, customer);

                    if (associatedProduct.HasTierPrices)
                    {
                        //calculate price for the maximum quantity if we have tier prices, and choose minimal
                        tmpMinPossiblePrice = Math.Min(tmpMinPossiblePrice,
                            (await _priceCalculationService.GetFinalPriceAsync(associatedProduct, customer, quantity: int.MaxValue)).priceWithoutDiscounts);
                    }

                    if (minPossiblePrice.HasValue && tmpMinPossiblePrice >= minPossiblePrice.Value)
                        continue;
                    minPriceProduct = associatedProduct;
                    minPossiblePrice = tmpMinPossiblePrice;
                }

                if (minPriceProduct == null || minPriceProduct.CustomerEntersPrice)
                    return;

                if (minPriceProduct.CallForPrice &&
                    //also check whether the current user is impersonated
                    (!_orderSettings.AllowAdminsToBuyCallForPriceProducts ||
                     _workContext.OriginalCustomerIfImpersonated == null))
                {
                    priceModel.OldPrice = null;
                    priceModel.OldPriceValue = null;
                    priceModel.Price = await _localizationService.GetResourceAsync("Products.CallForPrice");
                    priceModel.PriceValue = null;
                }
                else
                {
                    //calculate prices
                    var (finalPriceBase, _) = await _taxService.GetProductPriceAsync(minPriceProduct, minPossiblePrice.Value);
                    var finalPrice = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(finalPriceBase, await _workContext.GetWorkingCurrencyAsync());

                    priceModel.OldPrice = null;
                    priceModel.OldPriceValue = null;
                    priceModel.Price = string.Format(await _localizationService.GetResourceAsync("Products.PriceRangeFrom"), await _priceFormatter.FormatPriceAsync(finalPrice));
                    priceModel.PriceValue = finalPrice;

                    //PAngV default baseprice (used in Germany)
                    priceModel.BasePricePAngV = await _priceFormatter.FormatBasePriceAsync(product, finalPriceBase);
                    priceModel.BasePricePAngVValue = finalPriceBase;
                }
            }
            else
            {
                //hide prices
                priceModel.OldPrice = null;
                priceModel.OldPriceValue = null;
                priceModel.Price = null;
                priceModel.PriceValue = null;
            }
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
        protected virtual async Task<PictureModel> PrepareProductOverviewPictureModelAsync(Product product, int? productThumbPictureSize = null)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            var productName = await _localizationService.GetLocalizedAsync(product, x => x.Name);
            //If a size has been set in the view, we use it in priority
            var pictureSize = productThumbPictureSize ?? _mediaSettings.ProductThumbPictureSize;

            //Alchub -- Get product picture from only cdn
            var sku = string.Empty;
            string fullSizeImageUrl, imageUrl;
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
                    sku = associatedProducts.Select(ap => ap.Sku)?.FirstOrDefault();
            }
            else
                sku = product.Sku;

            //imageUrl = await _pictureService.GetProductPictureUrlAsync(sku, pictureSize);
            //fullSizeImageUrl = await _pictureService.GetProductPictureUrlAsync(sku, 0);
            imageUrl = product.ImageUrl;
            fullSizeImageUrl = product.ImageUrl;
            var pictureModel = new PictureModel
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
            //End Alchub

            ////prepare picture model
            //var cacheKey = _staticCacheManager.PrepareKeyForDefaultCache(NopModelCacheDefaults.ProductDefaultPictureModelKey,
            //    product, pictureSize, true, await _workContext.GetWorkingLanguageAsync(), _webHelper.IsCurrentConnectionSecured(),
            //    await _storeContext.GetCurrentStoreAsync());

            //var defaultPictureModel = await _staticCacheManager.GetAsync(cacheKey, async () =>
            //{
            //    var picture = (await _pictureService.GetPicturesByProductIdAsync(product.Id, 1)).FirstOrDefault();
            //    string fullSizeImageUrl, imageUrl;
            //    (imageUrl, picture) = await _pictureService.GetPictureUrlAsync(picture, pictureSize);
            //    (fullSizeImageUrl, picture) = await _pictureService.GetPictureUrlAsync(picture);

            //    var pictureModel = new PictureModel
            //    {
            //        ImageUrl = imageUrl,
            //        FullSizeImageUrl = fullSizeImageUrl,
            //        //"title" attribute
            //        Title = (picture != null && !string.IsNullOrEmpty(picture.TitleAttribute))
            //            ? picture.TitleAttribute
            //            : string.Format(await _localizationService.GetResourceAsync("Media.Product.ImageLinkTitleFormat"),
            //                productName),
            //        //"alt" attribute
            //        AlternateText = (picture != null && !string.IsNullOrEmpty(picture.AltAttribute))
            //            ? picture.AltAttribute
            //            : string.Format(await _localizationService.GetResourceAsync("Media.Product.ImageAlternateTextFormat"),
            //                productName)
            //    };

            //    return pictureModel;
            //});

            //return defaultPictureModel;
        }
        /// <summary>
        /// Prepare the product price model
        /// </summary>
        /// <param name="product">Product</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the product price model
        /// </returns>
        protected virtual async Task<ProductDetailsModel.ProductPriceModel> PrepareProductPriceModelAsync(Product product)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            var model = new ProductDetailsModel.ProductPriceModel
            {
                ProductId = product.Id
            };

            if (await _permissionService.AuthorizeAsync(StandardPermissionProvider.DisplayPrices))
            {
                model.HidePrices = false;
                if (product.CustomerEntersPrice)
                {
                    model.CustomerEntersPrice = true;
                }
                else
                {
                    if (product.CallForPrice &&
                        //also check whether the current user is impersonated
                        (!_orderSettings.AllowAdminsToBuyCallForPriceProducts || _workContext.OriginalCustomerIfImpersonated == null))
                    {
                        model.CallForPrice = true;
                    }
                    else
                    {
                        var customer = await _authenticationService.GetAuthenticatedCustomerAsync();
                        var (oldPriceBase, _) = await _taxService.GetProductPriceAsync(product, product.OldPrice);
                        var (finalPriceWithoutDiscountBase, _) = await _taxService.GetProductPriceAsync(product, (await _priceCalculationService.GetFinalPriceAsync(product, customer, includeDiscounts: false)).finalPrice);
                        var (finalPriceWithDiscountBase, _) = await _taxService.GetProductPriceAsync(product, (await _priceCalculationService.GetFinalPriceAsync(product, customer)).finalPrice);
                        var currentCurrency = await _workContext.GetWorkingCurrencyAsync();
                        var oldPrice = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(oldPriceBase, currentCurrency);
                        var finalPriceWithoutDiscount = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(finalPriceWithoutDiscountBase, currentCurrency);
                        var finalPriceWithDiscount = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(finalPriceWithDiscountBase, currentCurrency);

                        if (finalPriceWithoutDiscountBase != oldPriceBase && oldPriceBase > decimal.Zero)
                        {
                            model.OldPrice = await _priceFormatter.FormatPriceAsync(oldPrice);
                            model.OldPriceValue = oldPrice;
                        }

                        model.Price = await _priceFormatter.FormatPriceAsync(finalPriceWithoutDiscount);
                        model.PriceWithoutDiscount = finalPriceWithoutDiscount;

                        if (finalPriceWithoutDiscountBase != finalPriceWithDiscountBase)
                        {
                            model.PriceWithDiscount = await _priceFormatter.FormatPriceAsync(finalPriceWithDiscount);
                            model.PriceWithDiscountValue = finalPriceWithDiscount;
                        }

                        model.PriceValue = finalPriceWithDiscount;

                        //property for German market
                        //we display tax/shipping info only with "shipping enabled" for this product
                        //we also ensure this it's not free shipping
                        model.DisplayTaxShippingInfo = _catalogSettings.DisplayTaxShippingInfoProductDetailsPage
                            && product.IsShipEnabled &&
                            !product.IsFreeShipping;

                        //PAngV baseprice (used in Germany)
                        model.BasePricePAngV = await _priceFormatter.FormatBasePriceAsync(product, finalPriceWithDiscountBase);
                        model.BasePricePAngVValue = finalPriceWithDiscountBase;
                        //currency code
                        model.CurrencyCode = (await _workContext.GetWorkingCurrencyAsync()).CurrencyCode;

                        //rental
                        if (product.IsRental)
                        {
                            model.IsRental = true;
                            var priceStr = await _priceFormatter.FormatPriceAsync(finalPriceWithDiscount);
                            model.RentalPrice = await _priceFormatter.FormatRentalProductPeriodAsync(product, priceStr);
                            model.RentalPriceValue = finalPriceWithDiscount;
                        }
                    }
                }
            }
            else
            {
                model.HidePrices = true;
                model.OldPrice = null;
                model.OldPriceValue = null;
                model.Price = null;
            }

            return model;
        }


        /// <summary>
        /// Prepare the product attribute models
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="updatecartitem">Updated shopping cart item</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the list of product attribute model
        /// </returns>
        protected virtual async Task<IList<ProductDetailsModel.ProductAttributeModel>> PrepareProductAttributeModelsAsync(Product product, ShoppingCartItem updatecartitem)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            var model = new List<ProductDetailsModel.ProductAttributeModel>();

            var productAttributeMapping = await _productAttributeService.GetProductAttributeMappingsByProductIdAsync(product.Id);
            foreach (var attribute in productAttributeMapping)
            {
                var productAttrubute = await _productAttributeService.GetProductAttributeByIdAsync(attribute.ProductAttributeId);

                var attributeModel = new ProductDetailsModel.ProductAttributeModel
                {
                    Id = attribute.Id,
                    ProductId = product.Id,
                    ProductAttributeId = attribute.ProductAttributeId,
                    Name = await _localizationService.GetLocalizedAsync(productAttrubute, x => x.Name),
                    Description = await _localizationService.GetLocalizedAsync(productAttrubute, x => x.Description),
                    TextPrompt = await _localizationService.GetLocalizedAsync(attribute, x => x.TextPrompt),
                    IsRequired = attribute.IsRequired,
                    AttributeControlType = attribute.AttributeControlType,
                    DefaultValue = updatecartitem != null ? null : await _localizationService.GetLocalizedAsync(attribute, x => x.DefaultValue),
                    HasCondition = !string.IsNullOrEmpty(attribute.ConditionAttributeXml)
                };
                if (!string.IsNullOrEmpty(attribute.ValidationFileAllowedExtensions))
                {
                    attributeModel.AllowedFileExtensions = attribute.ValidationFileAllowedExtensions
                        .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                        .ToList();
                }

                if (attribute.ShouldHaveValues())
                {
                    //values
                    var attributeValues = await _productAttributeService.GetProductAttributeValuesAsync(attribute.Id);
                    foreach (var attributeValue in attributeValues)
                    {
                        var valueModel = new ProductDetailsModel.ProductAttributeValueModel
                        {
                            Id = attributeValue.Id,
                            Name = await _localizationService.GetLocalizedAsync(attributeValue, x => x.Name),
                            ColorSquaresRgb = attributeValue.ColorSquaresRgb, //used with "Color squares" attribute type
                            IsPreSelected = attributeValue.IsPreSelected,
                            CustomerEntersQty = attributeValue.CustomerEntersQty,
                            Quantity = attributeValue.Quantity
                        };
                        attributeModel.Values.Add(valueModel);

                        //display price if allowed
                        if (await _permissionService.AuthorizeAsync(StandardPermissionProvider.DisplayPrices))
                        {
                            var customer = updatecartitem?.CustomerId is null ? await _authenticationService.GetAuthenticatedCustomerAsync() : await _customerService.GetCustomerByIdAsync(updatecartitem.CustomerId);

                            var attributeValuePriceAdjustment = await _priceCalculationService.GetProductAttributeValuePriceAdjustmentAsync(product, attributeValue, customer);
                            var (priceAdjustmentBase, _) = await _taxService.GetProductPriceAsync(product, attributeValuePriceAdjustment);
                            var priceAdjustment = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(priceAdjustmentBase, await _workContext.GetWorkingCurrencyAsync());

                            if (attributeValue.PriceAdjustmentUsePercentage)
                            {
                                var priceAdjustmentStr = attributeValue.PriceAdjustment.ToString("G29");
                                if (attributeValue.PriceAdjustment > decimal.Zero)
                                    valueModel.PriceAdjustment = "+";
                                valueModel.PriceAdjustment += priceAdjustmentStr + "%";
                            }
                            else
                            {
                                if (priceAdjustmentBase > decimal.Zero)
                                    valueModel.PriceAdjustment = "+" + await _priceFormatter.FormatPriceAsync(priceAdjustment, false, false);
                                else if (priceAdjustmentBase < decimal.Zero)
                                    valueModel.PriceAdjustment = "-" + await _priceFormatter.FormatPriceAsync(-priceAdjustment, false, false);
                            }

                            valueModel.PriceAdjustmentValue = priceAdjustment;
                        }

                        //"image square" picture (with with "image squares" attribute type only)
                        if (attributeValue.ImageSquaresPictureId > 0)
                        {
                            var productAttributeImageSquarePictureCacheKey = _staticCacheManager.PrepareKeyForDefaultCache(NopModelCacheDefaults.ProductAttributeImageSquarePictureModelKey
                                , attributeValue.ImageSquaresPictureId,
                                    _webHelper.IsCurrentConnectionSecured(),
                                    await _storeContext.GetCurrentStoreAsync());
                            valueModel.ImageSquaresPictureModel = await _staticCacheManager.GetAsync(productAttributeImageSquarePictureCacheKey, async () =>
                            {
                                var imageSquaresPicture = await _pictureService.GetPictureByIdAsync(attributeValue.ImageSquaresPictureId);
                                string fullSizeImageUrl, imageUrl;
                                (imageUrl, imageSquaresPicture) = await _pictureService.GetPictureUrlAsync(imageSquaresPicture, _mediaSettings.ImageSquarePictureSize);
                                (fullSizeImageUrl, imageSquaresPicture) = await _pictureService.GetPictureUrlAsync(imageSquaresPicture);

                                if (imageSquaresPicture != null)
                                {
                                    return new PictureModel
                                    {
                                        FullSizeImageUrl = fullSizeImageUrl,
                                        ImageUrl = imageUrl
                                    };
                                }

                                return new PictureModel();
                            });
                        }

                        //picture of a product attribute value
                        valueModel.PictureId = attributeValue.PictureId;
                    }
                }

                //set already selected attributes (if we're going to update the existing shopping cart item)
                if (updatecartitem != null)
                {
                    switch (attribute.AttributeControlType)
                    {
                        case AttributeControlType.DropdownList:
                        case AttributeControlType.RadioList:
                        case AttributeControlType.Checkboxes:
                        case AttributeControlType.ColorSquares:
                        case AttributeControlType.ImageSquares:
                            {
                                if (!string.IsNullOrEmpty(updatecartitem.AttributesXml))
                                {
                                    //clear default selection
                                    foreach (var item in attributeModel.Values)
                                        item.IsPreSelected = false;

                                    //select new values
                                    var selectedValues = await _productAttributeParser.ParseProductAttributeValuesAsync(updatecartitem.AttributesXml);
                                    foreach (var attributeValue in selectedValues)
                                        foreach (var item in attributeModel.Values)
                                            if (attributeValue.Id == item.Id)
                                            {
                                                item.IsPreSelected = true;

                                                //set customer entered quantity
                                                if (attributeValue.CustomerEntersQty)
                                                    item.Quantity = attributeValue.Quantity;
                                            }
                                }
                            }

                            break;
                        case AttributeControlType.ReadonlyCheckboxes:
                            {
                                //values are already pre-set

                                //set customer entered quantity
                                if (!string.IsNullOrEmpty(updatecartitem.AttributesXml))
                                {
                                    foreach (var attributeValue in (await _productAttributeParser.ParseProductAttributeValuesAsync(updatecartitem.AttributesXml))
                                        .Where(value => value.CustomerEntersQty))
                                    {
                                        var item = attributeModel.Values.FirstOrDefault(value => value.Id == attributeValue.Id);
                                        if (item != null)
                                            item.Quantity = attributeValue.Quantity;
                                    }
                                }
                            }

                            break;
                        case AttributeControlType.TextBox:
                        case AttributeControlType.MultilineTextbox:
                            {
                                if (!string.IsNullOrEmpty(updatecartitem.AttributesXml))
                                {
                                    var enteredText = _productAttributeParser.ParseValues(updatecartitem.AttributesXml, attribute.Id);
                                    if (enteredText.Any())
                                        attributeModel.DefaultValue = enteredText[0];
                                }
                            }

                            break;
                        case AttributeControlType.Datepicker:
                            {
                                //keep in mind my that the code below works only in the current culture
                                var selectedDateStr = _productAttributeParser.ParseValues(updatecartitem.AttributesXml, attribute.Id);
                                if (selectedDateStr.Any())
                                {
                                    if (DateTime.TryParseExact(selectedDateStr[0], "D", CultureInfo.CurrentCulture, DateTimeStyles.None, out var selectedDate))
                                    {
                                        //successfully parsed
                                        attributeModel.SelectedDay = selectedDate.Day;
                                        attributeModel.SelectedMonth = selectedDate.Month;
                                        attributeModel.SelectedYear = selectedDate.Year;
                                    }
                                }
                            }

                            break;
                        case AttributeControlType.FileUpload:
                            {
                                if (!string.IsNullOrEmpty(updatecartitem.AttributesXml))
                                {
                                    var downloadGuidStr = _productAttributeParser.ParseValues(updatecartitem.AttributesXml, attribute.Id).FirstOrDefault();
                                    _ = Guid.TryParse(downloadGuidStr, out var downloadGuid);
                                    var download = await _downloadService.GetDownloadByGuidAsync(downloadGuid);
                                    if (download != null)
                                        attributeModel.DefaultValue = download.DownloadGuid.ToString();
                                }
                            }

                            break;
                        default:
                            break;
                    }
                }

                model.Add(attributeModel);
            }

            return model;
        }

        /// <summary>
        /// Prepare the product tier price models
        /// </summary>
        /// <param name="product">Product</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the list of tier price model
        /// </returns>
        protected virtual async Task<IList<ProductDetailsModel.TierPriceModel>> PrepareProductTierPriceModelsAsync(Product product)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            var customer = await _authenticationService.GetAuthenticatedCustomerAsync();
            var store = await _storeContext.GetCurrentStoreAsync();
            var model = await (await _productService.GetTierPricesAsync(product, customer, store.Id))
                .SelectAwait(async tierPrice =>
                {
                    var priceBase = (await _taxService.GetProductPriceAsync(product, (await _priceCalculationService.GetFinalPriceAsync(product,
                        customer, decimal.Zero, _catalogSettings.DisplayTierPricesWithDiscounts,
                        tierPrice.Quantity)).priceWithoutDiscounts)).price;

                    var price = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(priceBase, await _workContext.GetWorkingCurrencyAsync());

                    return new ProductDetailsModel.TierPriceModel
                    {
                        Quantity = tierPrice.Quantity,
                        Price = await _priceFormatter.FormatPriceAsync(price, false, false),
                        PriceValue = price
                    };
                }).ToListAsync();

            return model;
        }


        /// <summary>
        /// Prepare the product details picture model
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="isAssociatedProduct">Whether the product is associated</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the picture model for the default picture; All picture models
        /// </returns>
        protected virtual async Task<(PictureModel pictureModel, IList<PictureModel> allPictureModels)> PrepareProductDetailsPictureModelAsync(Product product, bool isAssociatedProduct)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            //default picture size
            var defaultPictureSize = isAssociatedProduct ?
                _mediaSettings.AssociatedProductPictureSize :
                _mediaSettings.ProductDetailsPictureSize;

            //Alchub -- Get product picture from only cdn
            var sku = string.Empty;
            string fullSizeImageUrl, imageUrl;
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
                    sku = associatedProducts.Select(ap => ap.Sku)?.FirstOrDefault();
            }
            else
                sku = product.Sku;

            //imageUrl = await _pictureService.GetProductPictureUrlAsync(sku, defaultPictureSize);
            //fullSizeImageUrl = await _pictureService.GetProductPictureUrlAsync(sku, 0);
            imageUrl = product.ImageUrl;
            fullSizeImageUrl = product.ImageUrl;
            var defaultPictureModel = new PictureModel
            {
                ImageUrl = imageUrl,
                FullSizeImageUrl = fullSizeImageUrl
            };

            return (defaultPictureModel, new List<PictureModel>());
            //End Alchub

            ////prepare picture models
            //var productPicturesCacheKey = _staticCacheManager.PrepareKeyForDefaultCache(NopModelCacheDefaults.ProductDetailsPicturesModelKey
            //    , product, defaultPictureSize, isAssociatedProduct,
            //    await _workContext.GetWorkingLanguageAsync(), _webHelper.IsCurrentConnectionSecured(), await _storeContext.GetCurrentStoreAsync());
            //var cachedPictures = await _staticCacheManager.GetAsync(productPicturesCacheKey, async () =>
            //{
            //    var productName = await _localizationService.GetLocalizedAsync(product, x => x.Name);

            //    var pictures = await _pictureService.GetPicturesByProductIdAsync(product.Id);
            //    var defaultPicture = pictures.FirstOrDefault();

            //    string fullSizeImageUrl, imageUrl, thumbImageUrl;
            //    (imageUrl, defaultPicture) = await _pictureService.GetPictureUrlAsync(defaultPicture, defaultPictureSize, !isAssociatedProduct);
            //    (fullSizeImageUrl, defaultPicture) = await _pictureService.GetPictureUrlAsync(defaultPicture, 0, !isAssociatedProduct);

            //    var defaultPictureModel = new PictureModel
            //    {
            //        ImageUrl = imageUrl,
            //        FullSizeImageUrl = fullSizeImageUrl
            //    };
            //    //"title" attribute
            //    defaultPictureModel.Title = (defaultPicture != null && !string.IsNullOrEmpty(defaultPicture.TitleAttribute)) ?
            //        defaultPicture.TitleAttribute :
            //        string.Format(await _localizationService.GetResourceAsync("Media.Product.ImageLinkTitleFormat.Details"), productName);
            //    //"alt" attribute
            //    defaultPictureModel.AlternateText = (defaultPicture != null && !string.IsNullOrEmpty(defaultPicture.AltAttribute)) ?
            //        defaultPicture.AltAttribute :
            //        string.Format(await _localizationService.GetResourceAsync("Media.Product.ImageAlternateTextFormat.Details"), productName);

            //    //all pictures
            //    var pictureModels = new List<PictureModel>();
            //    for (var i = 0; i < pictures.Count; i++)
            //    {
            //        var picture = pictures[i];

            //        (imageUrl, picture) = await _pictureService.GetPictureUrlAsync(picture, defaultPictureSize, !isAssociatedProduct);
            //        (fullSizeImageUrl, picture) = await _pictureService.GetPictureUrlAsync(picture);
            //        (thumbImageUrl, picture) = await _pictureService.GetPictureUrlAsync(picture, _mediaSettings.ProductThumbPictureSizeOnProductDetailsPage);

            //        var pictureModel = new PictureModel
            //        {
            //            ImageUrl = imageUrl,
            //            ThumbImageUrl = thumbImageUrl,
            //            FullSizeImageUrl = fullSizeImageUrl,
            //            Title = string.Format(await _localizationService.GetResourceAsync("Media.Product.ImageLinkTitleFormat.Details"), productName),
            //            AlternateText = string.Format(await _localizationService.GetResourceAsync("Media.Product.ImageAlternateTextFormat.Details"), productName),
            //        };
            //        //"title" attribute
            //        pictureModel.Title = !string.IsNullOrEmpty(picture.TitleAttribute) ?
            //            picture.TitleAttribute :
            //            string.Format(await _localizationService.GetResourceAsync("Media.Product.ImageLinkTitleFormat.Details"), productName);
            //        //"alt" attribute
            //        pictureModel.AlternateText = !string.IsNullOrEmpty(picture.AltAttribute) ?
            //            picture.AltAttribute :
            //            string.Format(await _localizationService.GetResourceAsync("Media.Product.ImageAlternateTextFormat.Details"), productName);

            //        pictureModels.Add(pictureModel);
            //    }

            //    return new { DefaultPictureModel = defaultPictureModel, PictureModels = pictureModels };
            //});

            //var allPictureModels = cachedPictures.PictureModels;
            //return (cachedPictures.DefaultPictureModel, allPictureModels);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Prepare the product overview models
        /// </summary>
        /// <param name="products">Collection of products</param>
        /// <param name="preparePriceModel">Whether to prepare the price model</param>
        /// <param name="preparePictureModel">Whether to prepare the picture model</param>
        /// <param name="productThumbPictureSize">Product thumb picture size (longest side); pass null to use the default value of media settings</param>
        /// <param name="prepareSpecificationAttributes">Whether to prepare the specification attribute models</param>
        /// <param name="forceRedirectionAfterAddingToCart">Whether to force redirection after adding to cart</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the collection of product overview model
        /// </returns>
        public virtual async Task<IEnumerable<ProductOverviewModel>> PrepareProductOverviewModelsAsync(IEnumerable<Product> products,
            bool preparePriceModel = true, bool preparePictureModel = true,
            int? productThumbPictureSize = null, bool prepareSpecificationAttributes = false,
            bool forceRedirectionAfterAddingToCart = false)
        {
            if (products == null)
                throw new ArgumentNullException(nameof(products));

            var models = new List<ProductOverviewModel>();
            foreach (var product in products)
            {
                var model = new ProductOverviewModel
                {
                    Id = product.Id,
                    Name = await _localizationService.GetLocalizedAsync(product, x => x.Name),
                    ShortDescription = await _localizationService.GetLocalizedAsync(product, x => x.ShortDescription),
                    FullDescription = await _localizationService.GetLocalizedAsync(product, x => x.FullDescription),
                    SeName = await _urlRecordService.GetSeNameAsync(product),
                    Sku = product.Sku,
                    ProductType = product.ProductType,
                    MarkAsNew = product.MarkAsNew &&
                        (!product.MarkAsNewStartDateTimeUtc.HasValue || product.MarkAsNewStartDateTimeUtc.Value < DateTime.UtcNow) &&
                        (!product.MarkAsNewEndDateTimeUtc.HasValue || product.MarkAsNewEndDateTimeUtc.Value > DateTime.UtcNow)
                };

                //price
                if (preparePriceModel)
                {
                    model.ProductPrice = await PrepareProductOverviewPriceModelAsync(product, forceRedirectionAfterAddingToCart);
                }

                //picture
                if (preparePictureModel)
                {
                    model.DefaultPictureModel = await PrepareProductOverviewPictureModelAsync(product, productThumbPictureSize);
                }

                //specs
                if (prepareSpecificationAttributes)
                {
                    model.ProductSpecificationModel = await PrepareProductSpecificationModelAsync(product);
                }

                //reviews
                model.ReviewOverviewModel = await PrepareProductReviewOverviewModelAsync(product);

                models.Add(model);
            }

            return models;
        }

        /// <summary>
        /// Prepare the product details model
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="updatecartitem">Updated shopping cart item</param>
        /// <param name="isAssociatedProduct">Whether the product is associated</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the product details model
        /// </returns>
        public virtual async Task<ProductDetailsModel> PrepareProductDetailsModelAsync(Product product,
            ShoppingCartItem updatecartitem = null, bool isAssociatedProduct = false)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            //standard properties
            var model = new ProductDetailsModel
            {
                Id = product.Id,
                Name = await _localizationService.GetLocalizedAsync(product, x => x.Name),
                ShortDescription = await _localizationService.GetLocalizedAsync(product, x => x.ShortDescription),
                FullDescription = await _localizationService.GetLocalizedAsync(product, x => x.FullDescription),
                MetaKeywords = await _localizationService.GetLocalizedAsync(product, x => x.MetaKeywords),
                MetaDescription = await _localizationService.GetLocalizedAsync(product, x => x.MetaDescription),
                MetaTitle = await _localizationService.GetLocalizedAsync(product, x => x.MetaTitle),
                SeName = await _urlRecordService.GetSeNameAsync(product),
                ProductType = product.ProductType,
                ShowSku = _catalogSettings.ShowSkuOnProductDetailsPage,
                Sku = product.Sku,
                ShowManufacturerPartNumber = _catalogSettings.ShowManufacturerPartNumber,
                FreeShippingNotificationEnabled = _catalogSettings.ShowFreeShippingNotification,
                ManufacturerPartNumber = product.ManufacturerPartNumber,
                ShowGtin = _catalogSettings.ShowGtin,
                Gtin = product.Gtin,
                ManageInventoryMethod = product.ManageInventoryMethod,
                StockAvailability = await _productService.FormatStockMessageAsync(product, string.Empty),
                HasSampleDownload = product.IsDownload && product.HasSampleDownload,
                DisplayDiscontinuedMessage = !product.Published && _catalogSettings.DisplayDiscontinuedMessageForUnpublishedProducts,
                AvailableEndDate = product.AvailableEndDateTimeUtc,
                VisibleIndividually = product.VisibleIndividually,
                AllowAddingOnlyExistingAttributeCombinations = product.AllowAddingOnlyExistingAttributeCombinations
            };

            //automatically generate product description?
            if (_seoSettings.GenerateProductMetaDescription && string.IsNullOrEmpty(model.MetaDescription))
            {
                //based on short description
                model.MetaDescription = model.ShortDescription;
            }

            //shipping info
            model.IsShipEnabled = product.IsShipEnabled;
            if (product.IsShipEnabled)
            {
                model.IsFreeShipping = product.IsFreeShipping;
                //delivery date
                var deliveryDate = await _dateRangeService.GetDeliveryDateByIdAsync(product.DeliveryDateId);
                if (deliveryDate != null)
                {
                    model.DeliveryDate = await _localizationService.GetLocalizedAsync(deliveryDate, dd => dd.Name);
                }
            }

            var store = await _storeContext.GetCurrentStoreAsync();
            //email a friend
            model.EmailAFriendEnabled = _catalogSettings.EmailAFriendEnabled;
            //compare products
            model.CompareProductsEnabled = _catalogSettings.CompareProductsEnabled;
            //store name
            model.CurrentStoreName = await _localizationService.GetLocalizedAsync(store, x => x.Name);

            //page sharing
            if (_catalogSettings.ShowShareButton && !string.IsNullOrEmpty(_catalogSettings.PageShareCode))
            {
                var shareCode = _catalogSettings.PageShareCode;
                if (_webHelper.IsCurrentConnectionSecured())
                {
                    //need to change the add this link to be https linked when the page is, so that the page doesn't ask about mixed mode when viewed in https...
                    shareCode = shareCode.Replace("http://", "https://");
                }

                model.PageShareCode = shareCode;
            }

            switch (product.ManageInventoryMethod)
            {
                case ManageInventoryMethod.DontManageStock:
                    model.InStock = true;
                    break;

                case ManageInventoryMethod.ManageStock:
                    model.InStock = product.BackorderMode != BackorderMode.NoBackorders
                        || await _productService.GetTotalStockQuantityAsync(product) > 0;
                    model.DisplayBackInStockSubscription = !model.InStock && product.AllowBackInStockSubscriptions;
                    break;

                case ManageInventoryMethod.ManageStockByAttributes:
                    model.InStock = (await _productAttributeService
                        .GetAllProductAttributeCombinationsAsync(product.Id))
                        ?.Any(c => c.StockQuantity > 0 || c.AllowOutOfStockOrders)
                        ?? false;
                    break;
            }

            //pictures
            model.DefaultPictureZoomEnabled = _mediaSettings.DefaultPictureZoomEnabled;
            IList<PictureModel> allPictureModels;
            (model.DefaultPictureModel, allPictureModels) = await PrepareProductDetailsPictureModelAsync(product, isAssociatedProduct);
            model.PictureModels = allPictureModels;

            //price
            model.ProductPrice = await PrepareProductPriceModelAsync(product);

            var customer = await _authenticationService.GetAuthenticatedCustomerAsync();
            //gift card
            if (product.IsGiftCard)
            {
                model.GiftCard.IsGiftCard = true;
                model.GiftCard.GiftCardType = product.GiftCardType;

                if (updatecartitem == null)
                {
                    model.GiftCard.SenderName = await _customerService.GetCustomerFullNameAsync(customer);
                    model.GiftCard.SenderEmail = customer.Email;
                }
                else
                {
                    _productAttributeParser.GetGiftCardAttribute(updatecartitem.AttributesXml,
                        out var giftCardRecipientName, out var giftCardRecipientEmail,
                        out var giftCardSenderName, out var giftCardSenderEmail, out var giftCardMessage);

                    model.GiftCard.RecipientName = giftCardRecipientName;
                    model.GiftCard.RecipientEmail = giftCardRecipientEmail;
                    model.GiftCard.SenderName = giftCardSenderName;
                    model.GiftCard.SenderEmail = giftCardSenderEmail;
                    model.GiftCard.Message = giftCardMessage;
                }
            }

            //product attributes
            model.ProductAttributes = await PrepareProductAttributeModelsAsync(product, updatecartitem);

            //product review overview
            model.ProductReviewOverview = await PrepareProductReviewOverviewModelAsync(product);

            //tier prices
            if (product.HasTierPrices && await _permissionService.AuthorizeAsync(StandardPermissionProvider.DisplayPrices))
            {
                model.TierPrices = await PrepareProductTierPriceModelsAsync(product);
            }



            //rental products
            if (product.IsRental)
            {
                model.IsRental = true;
                //set already entered dates attributes (if we're going to update the existing shopping cart item)
                if (updatecartitem != null)
                {
                    model.RentalStartDate = updatecartitem.RentalStartDateUtc;
                    model.RentalEndDate = updatecartitem.RentalEndDateUtc;
                }
            }

            //associated products
            if (product.ProductType == ProductType.GroupedProduct)
            {
                //ensure no circular references
                if (!isAssociatedProduct)
                {
                    var associatedProducts = await _productService.GetAssociatedProductsAsync(product.Id, store.Id,
                                                                                             //++Alchub geovendor
                                                                                             geoVendorIds: await _workContext.GetCurrentCustomerGeoRadiusVendorIdsAsync(customer));

                    //var associatedProducts = await _productService.GetAssociatedProductsAsync(product.Id, store.Id);
                    foreach (var associatedProduct in associatedProducts)
                        model.AssociatedProducts.Add(await PrepareProductDetailsModelAsync(associatedProduct, null, true));
                }
                model.InStock = model.AssociatedProducts.Any(associatedProduct => associatedProduct.InStock);
            }

            //get available vendors
            var availableVendors = await _vendorService.GetAvailableGeoFenceVendorsAsync(customer, true, product);
            //prepare availbale vendors products
            model.ProductVendorModel.ProductDetailVendor = (await PrepareProductVendorListAsync(product, availableVendors, customer))?.OrderBy(x => x.StartTime)?.ToList();
            var cart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, store.Id);
            if (cart.Any())
            {
                //cart product vendor ids
                var cartProductVendorIds = (await _productService.GetProductsByIdsAsync(cart.OrderBy(c => c.CreatedOnUtc)?.
                    Select(x => x.ProductId).ToArray())).Select(x => x.VendorId).ToList();

                //vendor sorting according to cart product vendor(s).
                if (cartProductVendorIds.Any())
                {
                    cartProductVendorIds.AddRange(model.ProductVendorModel.ProductDetailVendor.Select(x => x.VendorId));
                    var avIds = cartProductVendorIds.Intersect(availableVendors.Select(av => av.Id).ToList()).ToArray();
                    var sortedVendors = await _vendorService.GetVendorsByIdsAsync(avIds.Distinct().ToArray());
                    model.ProductVendorModel.ProductDetailVendor = await PrepareProductVendorListAsync(product, sortedVendors, customer);
                }
            }

            return model;
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

        public virtual async Task<ProductSpecificationModel> PrepareProductSpecificationModelAsync(Product product)
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
        /// Prepare product vendors
        /// </summary>
        /// <param name="product"></param>
        /// <param name="availableVendors"></param>
        /// <returns></returns>
        public virtual async Task<IList<ProductVendorModel.ProductDetailVendors>> PrepareProductVendorListAsync(Product product, IList<Vendor> availableVendors, Customer customer, bool preLoadFastestSlot = true, bool loadStartTime = true)
        {
            //Note: show products all vendors if location not searched. (16-09-22)
            bool ignoreGeoVendors = string.IsNullOrEmpty(customer.LastSearchedCoordinates);
            if (ignoreGeoVendors)
            {
                //get available vendors for product
                availableVendors = await _alchubGeneralService.GetVendorByMasterProductIdAsync(product);
            }

            var productVendors = new List<ProductVendorModel.ProductDetailVendors>();
            if (availableVendors == null || !availableVendors.Any())
                return productVendors;

            //get vendors who provides delivery 
            var availableDeliverableVendors = await _vendorService.GetAvailableGeoFenceVendorsAsync(customer, false, product);

            //get upc products
            var upcProducts = await _alchubGeneralService.GetProductsByUpcCodeAsync(product.UPCCode, true, true);

            //25-04-23 - toggle favorite vendor filter
            availableVendors = (await _vendorService.ApplyFavoriteToggleFilterAsync(availableVendors, customer)).ToList();
            foreach (var vendor in availableVendors)
            {
                //get vendor product
                var vendorProduct = upcProducts?.FirstOrDefault(p => p.VendorId == vendor.Id);
                if (vendorProduct == null)
                    continue;

                //check for in stock - 17-11-22
                if (await _productService.GetTotalStockQuantityAsync(vendorProduct) <= 0)
                    continue;

                //check if no slot created for delivery/pickup then do not include vendor - 23-12-22
                if (!await _slotService.HasVendorCreatedAnySlot(vendor))
                    continue;

                //prepare for nearest vendors filter
                var store = await _storeContext.GetCurrentStoreAsync();
                var distance = "";
                decimal distanceValue = 0;
                distanceValue = await _deliveryFeeService.GetDistanceAsync(vendor.GeoLocationCoordinates, customer.LastSearchedCoordinates);

                //Convert Distance from Meter to Miles
                //distanceValue = Math.Round(distanceValue / Convert.ToDecimal(1609.34), 1);
                //set distance format
                switch (_vendorSettings.DistanceUnit)
                {
                    case DistanceUnit.Mile:
                        distanceValue = distanceValue * Convert.ToDecimal(0.000621371192);
                        distance = string.Format("{0} {1}", distanceValue.ToString("F1"), "miles");
                        break;
                    case DistanceUnit.Kilometer:
                        distanceValue = distanceValue / 1000;
                        distance = string.Format("{0} {1}", distanceValue.ToString("F1"), "km");
                        break;
                    default:
                        distance = string.Format("{0} {1}", distanceValue.ToString("F1"), "m");
                        break;
                }

                //vendor provides & manage delivery, but according current searched location, vendors is only able to provide pickup, handel that
                var manageDelivery = vendor.ManageDelivery && (availableDeliverableVendors.Select(x => x.Id).Contains(vendor.Id) || string.IsNullOrEmpty(customer.LastSearchedCoordinates));
                var deliveryAvailable = vendor.DeliveryAvailable && (availableDeliverableVendors.Select(x => x.Id).Contains(vendor.Id) || string.IsNullOrEmpty(customer.LastSearchedCoordinates));

                var startTime = await PrepareVendorAvailableSlotsAsync(product.Id, vendor.Id, vendor.PickAvailable, manageDelivery, deliveryAvailable);
                if (startTime.HasValue)
                {
                    //startTime from & To
                    var from = startTime.Value.AddMinutes(0).ToString("HH:mm");
                    var to = startTime.Value.AddMinutes(60).ToString("HH:mm");
                    var originalTime = $"{from}-{to}";

                    var productVendorModel = new ProductVendorModel.ProductDetailVendors()
                    {
                        ProductId = vendorProduct.Id,
                        VendorId = vendor.Id,
                        VendorName = await _localizationService.GetLocalizedAsync(vendor, x => x.Name),
                        ManageDelivery = manageDelivery,
                        PickAvailable = vendor.PickAvailable,
                        Time = startTime,
                        Date = startTime.Value.ToString("MM/dd/yyyy").Replace('/', '-'),
                        StartTime = originalTime,
                        StartTime2 = SlotHelper.ConvertTo12hoursSlotTime(originalTime),
                        VendorProductPrice = await PrepareProductPriceModelAsync(vendorProduct),
                        DeliveryAvailable = deliveryAvailable,
                        Distance = distance,
                        DistanceValue = distanceValue
                    };

                    //in available vendors, we have combination of delivery & pickup vendors, so lets check whether this vendor can actually provide delivery. 
                    if (availableDeliverableVendors.Any(x => x.Id == vendor.Id) && vendor.DeliveryAvailable)
                    {
                        //Delivery fee 
                        var deliveryFee = await _deliveryFeeService.GetDeliveryFeeByVendorIdAsync(vendor.Id);
                        if (deliveryFee != null)
                        {
                            if (deliveryFee.DeliveryFeeTypeId == (int)DeliveryFeeType.Fixed)
                            {
                                productVendorModel.DeliveryFee = await _priceFormatter.FormatPriceAsync(deliveryFee.FixedFee);
                            }
                            else
                            {

                                productVendorModel.DeliveryFee = await _priceFormatter.FormatPriceAsync(deliveryFee.DynamicBaseFee);

                            }
                        }
                    }
                    //else
                    //{
                    //    productVendorModel.ManageDelivery = false;
                    //}
                    //Order minimum amount
                    productVendorModel.OrderAmount = await _priceFormatter.FormatPriceAsync(vendor.MinimumOrderAmount);

                    //Fastest Slot
                    //await PrepareProductFastestSlotAsync(productVendorModel);

                    productVendors.Add(productVendorModel);
                }
            }
            return productVendors;
        }


        /// <summary>
        /// Prepare product vendors
        /// </summary>
        /// <param name="product"></param>
        /// <param name="availableVendors"></param>
        /// <returns></returns>
        public virtual async Task<ProductVendorModel> PrepareProductVendorsAsync(Product product, IList<Vendor> availableVendors, Customer customer, int sortBy)
        {
            //get product vendors
            var productVendors = (await PrepareProductVendorListAsync(product, availableVendors, customer))?.OrderBy(v => v.Time)
                                                                                                           ?.ThenBy(v => v.DistanceValue)
                                                                                                           ?.ToList();
            var store = await _storeContext.GetCurrentStoreAsync();

            if (productVendors.Any())
            {
                if (sortBy > 0)
                {
                    if (sortBy == (int)VendorSort.Cheapest)
                        productVendors = productVendors.OrderBy(x => x.VendorProductPrice.PriceWithoutDiscount).ToList();
                    else if (sortBy == (int)VendorSort.Proximity)
                        productVendors = productVendors.OrderBy(x => x.DistanceValue).ToList();
                    else if (sortBy == (int)VendorSort.Fastest)
                        productVendors = productVendors.OrderBy(x => x.Time).ThenBy(v => v.DistanceValue).ToList();
                    else if (sortBy == (int)VendorSort.Recommended)
                    {
                        var cart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, store.Id);
                        if (cart.Any())
                        {
                            //cart product vendor ids
                            var cartProductVendorIds = (await _productService.GetProductsByIdsAsync(cart.OrderBy(c => c.CreatedOnUtc)?.
                                Select(x => x.ProductId).ToArray())).Select(x => x.VendorId).ToList();

                            //vendor sorting according to cart product vendor(s).
                            if (cartProductVendorIds.Any())
                            {
                                cartProductVendorIds.AddRange(productVendors.Select(x => x.VendorId));
                                var avIds = cartProductVendorIds.Intersect(availableVendors.Select(av => av.Id).ToList()).ToArray();
                                var availableVendorsSorted = await _vendorService.GetVendorsByIdsAsync(avIds.Distinct().ToArray());
                                productVendors = (await PrepareProductVendorListAsync(product, availableVendorsSorted, customer))?.ToList();
                            }
                        }
                    }
                }
            }

            //prepare product vendor model
            var productVendorModel = new ProductVendorModel
            {
                ProductDetailVendor = productVendors
            };

            if (productVendorModel == null && productVendorModel.ProductDetailVendor.Any())
                productVendorModel.ProductDetailVendor.FirstOrDefault(x => x.IsDefaultVendor == true);

            //Check with alchub details page setting true only vendor show
            if (productVendorModel.ProductDetailVendor != null && productVendorModel.ProductDetailVendor.Any() && _alchubSettings.AlchubProductDetailPageVendorTakeOne)
            {
                productVendorModel.ProductDetailVendor = productVendorModel.ProductDetailVendor.Take(1).ToList();
            }

            //variant product images. 
            //Note: This only for grouped product variant.
            var productDto = product.ToDto();
            var productPictures = await _productService.GetProductPicturesByProductIdAsync(product.Id);
            await PrepareProductImagesAsync(productPictures, productVendorModel.Images);

            return productVendorModel;
        }

        private async Task PrepareProductImagesAsync(IEnumerable<ProductPicture> productPictures, List<ImageMappingDto> images)
        {
            if (images == null)
            {
                images = new List<ImageMappingDto>();
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

                    images.Add(productImageDto);
                }
            }
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

        #region Slot Management
        public async Task<List<BookingSlotModel>> PreparepareSlotListModel(int slotId, string startDate, string endDate, int productId, int vendorId, bool isPickup = false)
        {
            var customer = await _authenticationService.GetAuthenticatedCustomerAsync();
            var productCategories = (await _categoryService.GetProductCategoriesByProductIdAsync(productId)).Select(x => x.CategoryId).ToList();
            var store = _storeContext.GetCurrentStore();
            IList<SlotModel> formatSlots = new List<SlotModel>();
            var zone = await _slotService.GetVendorZones(true, vendorId, isPickup, createdBy: 0);
            // Get 7 days of week by start date
            DateTime startD = Convert.ToDateTime(startDate);
            var Weeks = new List<DateTime>();
            for (int i = 0; i < 7; i++)
            {
                Weeks.Add(startD.AddDays(i));
            }
            var resultModel = new List<BookingSlotModel>();
            if (zone != null)
            {

                var zoneId = zone != null ? zone.Id : 0;
                var slots = await _slotService.GetFreeSlotByZoneAndDate(Convert.ToDateTime(startDate), Convert.ToDateTime(endDate), zoneId);
                if (slots != null)
                {
                    // create hourly slots from available data
                    foreach (var item in slots)
                    {
                        int start = item.Start.TimeOfDay.Hours, end = (item.End.TimeOfDay.Hours == 23 && item.End.TimeOfDay.Minutes == 59) ? 24 : item.End.TimeOfDay.Hours; //handel 24h
                        int addhour = 0;
                        if (!await _slotService.FindSlotCategoryIdExist(item.Id, productCategories))
                        {
                            for (int i = start; i < end; i++)
                            {
                                SlotModel slotItem = new SlotModel();
                                slotItem.Start = item.Start.AddHours(addhour);
                                slotItem.End = slotItem.Start.AddHours(1);
                                slotItem.Id = item.Id;
                                slotItem.Capacity = item.Capacity;
                                addhour++;
                                slotItem.Price = item.Price;
                                formatSlots.Add(slotItem);
                            }
                        }
                    }
                    formatSlots = formatSlots.OrderBy(x => x.Start).ToList();
                }

                // Bind table with slots and data
                var startSlot = await _settingService.GetSettingByKeyAsync<int>("Alchub.Slot.Start.Time", defaultValue: 2);
                var endSlot = await _settingService.GetSettingByKeyAsync<int>("Alchub.Slot.End.Time", defaultValue: 23);

                for (int i = 0; i < Weeks.Count; i++)
                {
                    var bookSlot = new BookingSlotModel();
                    bookSlot.Start = Weeks[i];
                    bookSlot.End = Weeks[i];

                    for (int j = startSlot; j < endSlot; j++)
                    {
                        SlotDefault slotDefault = new SlotDefault();
                        var available = formatSlots.FirstOrDefault(x => x.Start.Equals(Weeks[i].Date.AddHours(j)));

                        if (available != null)
                        {
                            if (available.Start > Convert.ToDateTime(startDate) && await CheckSlotAvailability(available.Id, available.Start, available.End, available.Capacity, false))
                            {
                                var originalSlotTime = (Convert.ToInt32("0") + Convert.ToInt32(j) + ":00" + "-" + (Convert.ToInt32(j) + Convert.ToInt32(1)) + ":00").ToString();
                                slotDefault.Slot = originalSlotTime;
                                slotDefault.SlotTime2 = SlotHelper.ConvertTo12hoursSlotTime(originalSlotTime);
                                slotDefault.IsAvailable = true;
                                slotDefault.IsBook = false;
                                slotDefault.Price = await _priceFormatter.FormatPriceAsync(available.Price);
                                slotDefault.Id = available.Id;
                                slotDefault.BlockId = j;
                                slotDefault.Capacity = available.Capacity;
                                slotDefault.IsPickup = false;
                                slotDefault.Start = available.Start;
                                bookSlot.SlotModel.Add(slotDefault);
                            }
                            else
                            {

                                slotDefault.IsPickup = true;
                                slotDefault.IsAvailable = false;
                                var originalSlotTime = (Convert.ToInt32("0") + Convert.ToInt32(j) + ":00" + "-" + (Convert.ToInt32(j) + Convert.ToInt32(1)) + ":00").ToString();
                                slotDefault.Slot = originalSlotTime;
                                slotDefault.SlotTime2 = SlotHelper.ConvertTo12hoursSlotTime(originalSlotTime);
                                slotDefault.Start = Weeks[i];
                                bookSlot.SlotModel.Add(slotDefault);
                            }
                        }
                        else
                        {
                            slotDefault.IsPickup = true;
                            slotDefault.IsAvailable = false;
                            var originalSlotTime = (Convert.ToInt32("0") + Convert.ToInt32(j) + ":00" + "-" + (Convert.ToInt32(j) + Convert.ToInt32(1)) + ":00").ToString();
                            slotDefault.Slot = originalSlotTime;
                            slotDefault.SlotTime2 = SlotHelper.ConvertTo12hoursSlotTime(originalSlotTime);
                            slotDefault.Start = Weeks[i];
                            bookSlot.SlotModel.Add(slotDefault);
                        }
                    }
                    //add if at least 1 slot is available
                    var anySlotAvailable = bookSlot.SlotModel.Any(x => x.IsAvailable);
                    if (anySlotAvailable)
                        resultModel.Add(bookSlot);
                }


                //Selected insert into table
                IList<CustomerProductSlot> cutomerProductSlotList = new List<CustomerProductSlot>();
                cutomerProductSlotList = await _slotService.GetCustomerProductSlot(productId, customer.Id, Convert.ToDateTime(startDate), isPickup);
                if (cutomerProductSlotList.Count == 0)
                {
                    var bookSlot = resultModel.Select(x => x.SlotModel.FirstOrDefault(x => x.IsAvailable == true)).FirstOrDefault();
                    if (bookSlot != null)
                    {
                        CustomerProductSlot customerProductSlot = new CustomerProductSlot();
                        bookSlot.IsBook = true;
                        bookSlot.IsSelected = false;
                        var slot = await _slotService.GetSlotById(bookSlot.Id);
                        customerProductSlot.SlotId = bookSlot.Id;
                        customerProductSlot.BlockId = bookSlot.BlockId;
                        customerProductSlot.StartTime = bookSlot.Start;
                        customerProductSlot.EndDateTime = bookSlot.Start;
                        customerProductSlot.EndTime = bookSlot.Slot;
                        customerProductSlot.CustomerId = customer.Id;
                        customerProductSlot.Price = Convert.ToDecimal(slot != null ? slot.Price : 0);
                        customerProductSlot.ProductId = productId;
                        customerProductSlot.IsPickup = isPickup;
                        customerProductSlot.LastUpdated = DateTime.UtcNow;
                        customerProductSlot.IsSelected = false;
                        await _slotService.InsertCustomerProductSlot(customerProductSlot);
                    }
                }
                else
                {
                    foreach (var cutomerProductSlot in cutomerProductSlotList)
                    {
                        var slotElement = resultModel.Select(x => x.SlotModel.FirstOrDefault(x => x.Id == cutomerProductSlot.SlotId && x.Slot == cutomerProductSlot.EndTime)).FirstOrDefault();
                        if (slotElement != null)
                        {
                            if (cutomerProductSlot.IsSelected)
                            {
                                slotElement.IsBook = false;
                                slotElement.IsSelected = true;
                            }
                        }
                    }
                    var bookSlot = resultModel.Select(x => x.SlotModel.FirstOrDefault(x => x.IsAvailable == true)).FirstOrDefault();
                    if (bookSlot != null)
                    {
                        if (!await _slotService.GetCustomerProductSlotByDate(productId, customer.Id, Convert.ToDateTime(bookSlot.Start), bookSlot.Slot, isPickup))
                        {
                            await _slotService.DeleteCustomerProductSlot(productId, customer.Id);
                            CustomerProductSlot customerProductSlot = new CustomerProductSlot();
                            bookSlot.IsBook = true;
                            bookSlot.IsSelected = false;
                            var slot = await _slotService.GetSlotById(bookSlot.Id);
                            customerProductSlot.SlotId = bookSlot.Id;
                            customerProductSlot.BlockId = bookSlot.BlockId;
                            customerProductSlot.StartTime = bookSlot.Start;
                            customerProductSlot.EndDateTime = bookSlot.Start;
                            customerProductSlot.EndTime = bookSlot.Slot;
                            customerProductSlot.CustomerId = customer.Id;
                            customerProductSlot.Price = Convert.ToDecimal(slot != null ? slot.Price : 0);
                            customerProductSlot.ProductId = productId;
                            customerProductSlot.IsPickup = isPickup;
                            customerProductSlot.LastUpdated = DateTime.UtcNow;
                            customerProductSlot.IsSelected = false;
                            await _slotService.InsertCustomerProductSlot(customerProductSlot);
                        }
                        else
                        {
                            bookSlot.IsBook = true;
                            bookSlot.IsSelected = false;
                        }
                    }
                }
            }
            return resultModel.ToList();
        }
        public async Task<List<BookingSlotModel>> PrepareparePickupSlotListModel(int slotId, string startDate, string endDate, int productId, int vendorId, bool isPickup = true)
        {
            var customer = await _authenticationService.GetAuthenticatedCustomerAsync();
            var productCategories = (await _categoryService.GetProductCategoriesByProductIdAsync(productId)).Select(x => x.CategoryId).ToList();
            var store = _storeContext.GetCurrentStore();
            IList<SlotModel> formatSlots = new List<SlotModel>();
            var zone = await _slotService.GetVendorZones(true, vendorId, isPickup);
            // Get 7 days of week by start date
            DateTime startD = Convert.ToDateTime(startDate);
            var Weeks = new List<DateTime>();
            for (int i = 0; i < 7; i++)
            {
                Weeks.Add(startD.AddDays(i));
            }
            var resultModel = new List<BookingSlotModel>();
            if (zone != null)
            {

                var zoneId = zone != null ? zone.Id : 0;
                var slots = await _slotService.GetFreePickupSlotByZoneAndDate(Convert.ToDateTime(startDate), Convert.ToDateTime(endDate), zoneId);
                if (slots != null)
                {
                    // create hourly slots from available data
                    foreach (var item in slots)
                    {
                        int start = item.Start.TimeOfDay.Hours, end = (item.End.TimeOfDay.Hours == 23 && item.End.TimeOfDay.Minutes == 59) ? 24 : item.End.TimeOfDay.Hours;//handel 24h
                        int addhour = 0;
                        if (!await _slotService.FindSlotCategoryIdExist(item.Id, productCategories))
                        {
                            for (int i = start; i < end; i++)
                            {
                                SlotModel slotItem = new SlotModel();
                                slotItem.Start = item.Start.AddHours(addhour);
                                slotItem.End = slotItem.Start.AddHours(1);
                                slotItem.Id = item.Id;
                                slotItem.Capacity = item.Capacity;
                                addhour++;
                                slotItem.Price = item.Price;
                                formatSlots.Add(slotItem);
                            }
                        }
                    }
                    formatSlots = formatSlots.OrderBy(x => x.Start).ToList();
                }

                // Bind table with slots and data
                var startSlot = await _settingService.GetSettingByKeyAsync<int>("Alchub.Slot.Start.Time", defaultValue: 2);
                var endSlot = await _settingService.GetSettingByKeyAsync<int>("Alchub.Slot.End.Time", defaultValue: 23);

                for (int i = 0; i < Weeks.Count; i++)
                {
                    var bookSlot = new BookingSlotModel();
                    bookSlot.Start = Weeks[i];
                    bookSlot.End = Weeks[i];

                    for (int j = startSlot; j < endSlot; j++)
                    {
                        SlotDefault slotDefault = new SlotDefault();
                        var available = formatSlots.FirstOrDefault(x => x.Start.Equals(Weeks[i].Date.AddHours(j)));

                        if (available != null)
                        {
                            if (available.Start > Convert.ToDateTime(startDate) && await CheckSlotAvailability(available.Id, available.Start, available.End, available.Capacity, false))
                            {

                                var originalSlotTime = (Convert.ToInt32("0") + Convert.ToInt32(j) + ":00" + "-" + (Convert.ToInt32(j) + Convert.ToInt32(1)) + ":00").ToString();
                                slotDefault.Slot = originalSlotTime;
                                slotDefault.SlotTime2 = SlotHelper.ConvertTo12hoursSlotTime(originalSlotTime);
                                slotDefault.IsAvailable = true;
                                slotDefault.IsBook = false;
                                slotDefault.Price = await _priceFormatter.FormatPriceAsync(available.Price);
                                slotDefault.Id = available.Id;
                                slotDefault.BlockId = j;
                                slotDefault.Capacity = available.Capacity;
                                slotDefault.IsPickup = false;
                                slotDefault.Start = available.Start;
                                bookSlot.SlotModel.Add(slotDefault);
                            }
                            else
                            {

                                slotDefault.IsPickup = true;
                                slotDefault.IsAvailable = false;
                                var originalSlotTime = (Convert.ToInt32("0") + Convert.ToInt32(j) + ":00" + "-" + (Convert.ToInt32(j) + Convert.ToInt32(1)) + ":00").ToString();
                                slotDefault.Slot = originalSlotTime;
                                slotDefault.SlotTime2 = SlotHelper.ConvertTo12hoursSlotTime(originalSlotTime);
                                slotDefault.Start = Weeks[i];
                                bookSlot.SlotModel.Add(slotDefault);
                            }
                        }
                        else
                        {
                            slotDefault.IsPickup = true;
                            slotDefault.IsAvailable = false;
                            var originalSlotTime = (Convert.ToInt32("0") + Convert.ToInt32(j) + ":00" + "-" + (Convert.ToInt32(j) + Convert.ToInt32(1)) + ":00").ToString();
                            slotDefault.Slot = originalSlotTime;
                            slotDefault.SlotTime2 = SlotHelper.ConvertTo12hoursSlotTime(originalSlotTime);
                            slotDefault.Start = Weeks[i];
                            bookSlot.SlotModel.Add(slotDefault);
                        }
                    }
                    //add if at least 1 slot is available
                    var anySlotAvailable = bookSlot.SlotModel.Any(x => x.IsAvailable);
                    if (anySlotAvailable)
                        resultModel.Add(bookSlot);
                }


                //Selected insert into table
                //Selected insert into table
                IList<CustomerProductSlot> cutomerProductSlotList = new List<CustomerProductSlot>();
                cutomerProductSlotList = await _slotService.GetCustomerProductSlot(productId, customer.Id, Convert.ToDateTime(startDate), isPickup);
                if (cutomerProductSlotList.Count == 0)
                {
                    var bookSlot = resultModel.Select(x => x.SlotModel.FirstOrDefault(x => x.IsAvailable == true)).FirstOrDefault();
                    if (bookSlot != null)
                    {
                        CustomerProductSlot customerProductSlot = new CustomerProductSlot();
                        bookSlot.IsBook = true;
                        bookSlot.IsSelected = false;
                        var slot = await _slotService.GetSlotById(bookSlot.Id);
                        customerProductSlot.SlotId = bookSlot.Id;
                        customerProductSlot.BlockId = bookSlot.BlockId;
                        customerProductSlot.StartTime = bookSlot.Start;
                        customerProductSlot.EndDateTime = bookSlot.Start;
                        customerProductSlot.EndTime = bookSlot.Slot;
                        customerProductSlot.CustomerId = customer.Id;
                        customerProductSlot.Price = Convert.ToDecimal(slot != null ? slot.Price : 0);
                        customerProductSlot.ProductId = productId;
                        customerProductSlot.IsPickup = isPickup;
                        customerProductSlot.LastUpdated = DateTime.UtcNow;
                        customerProductSlot.IsSelected = false;
                        await _slotService.InsertCustomerProductSlot(customerProductSlot);
                    }
                }
                else
                {
                    foreach (var cutomerProductSlot in cutomerProductSlotList)
                    {
                        var slotElement = resultModel.Select(x => x.SlotModel.FirstOrDefault(x => x.Id == cutomerProductSlot.SlotId && x.Slot == cutomerProductSlot.EndTime)).FirstOrDefault();
                        if (slotElement != null)
                        {
                            if (cutomerProductSlot.IsSelected)
                            {
                                slotElement.IsBook = false;
                                slotElement.IsSelected = true;
                            }
                        }
                    }
                    var bookSlot = resultModel.Select(x => x.SlotModel.FirstOrDefault(x => x.IsAvailable == true)).FirstOrDefault();
                    if (bookSlot != null)
                    {
                        if (!await _slotService.GetCustomerProductSlotByDate(productId, customer.Id, Convert.ToDateTime(bookSlot.Start), bookSlot.Slot, isPickup))
                        {
                            await _slotService.DeleteCustomerProductSlot(productId, customer.Id);
                            CustomerProductSlot customerProductSlot = new CustomerProductSlot();
                            bookSlot.IsBook = true;
                            bookSlot.IsSelected = false;
                            var slot = await _slotService.GetSlotById(bookSlot.Id);
                            customerProductSlot.SlotId = bookSlot.Id;
                            customerProductSlot.BlockId = bookSlot.BlockId;
                            customerProductSlot.StartTime = bookSlot.Start;
                            customerProductSlot.EndDateTime = bookSlot.Start;
                            customerProductSlot.EndTime = bookSlot.Slot;
                            customerProductSlot.CustomerId = customer.Id;
                            customerProductSlot.Price = Convert.ToDecimal(slot != null ? slot.Price : 0);
                            customerProductSlot.ProductId = productId;
                            customerProductSlot.IsPickup = isPickup;
                            customerProductSlot.LastUpdated = DateTime.UtcNow;
                            customerProductSlot.IsSelected = false;
                            await _slotService.InsertCustomerProductSlot(customerProductSlot);
                        }
                        else
                        {
                            bookSlot.IsBook = true;
                            bookSlot.IsSelected = false;
                        }
                    }
                }
            }
            return resultModel.ToList();
        }

        public async Task<List<BookingSlotModel>> PreparepareAdminSlotListModel(int slotId, string startDate, string endDate, int productId, int vendorId, bool isPickup = false)
        {
            var customer = await _authenticationService.GetAuthenticatedCustomerAsync();
            var productCategories = (await _categoryService.GetProductCategoriesByProductIdAsync(productId)).Select(x => x.CategoryId).ToList();
            var store = _storeContext.GetCurrentStore();
            IList<SlotModel> formatSlots = new List<SlotModel>();
            var zone = await _slotService.GetAdminCreateVendorZones(true, vendorId, false, createdBy: 1);
            if (zone == null)
            {
                zone = await _slotService.GetVendorZones(true, 0, false);
            }
            // Get 7 days of week by start date
            DateTime startD = Convert.ToDateTime(startDate);
            var Weeks = new List<DateTime>();
            for (int i = 0; i < 7; i++)
            {
                Weeks.Add(startD.AddDays(i));
            }
            var resultModel = new List<BookingSlotModel>();
            if (zone != null)
            {

                var zoneId = zone != null ? zone.Id : 0;
                var slots = await _slotService.GetFreeSlotByZoneAndDate(Convert.ToDateTime(startDate), Convert.ToDateTime(endDate), zoneId);
                if (slots != null)
                {
                    // create hourly slots from available data
                    foreach (var item in slots)
                    {
                        int start = item.Start.TimeOfDay.Hours, end = (item.End.TimeOfDay.Hours == 23 && item.End.TimeOfDay.Minutes == 59) ? 24 : item.End.TimeOfDay.Hours;//handel 24h
                        int addhour = 0;
                        if (!await _slotService.FindSlotCategoryIdExist(item.Id, productCategories))
                        {
                            for (int i = start; i < end; i++)
                            {
                                SlotModel slotItem = new SlotModel();
                                slotItem.Start = item.Start.AddHours(addhour);
                                slotItem.End = slotItem.Start.AddHours(1);
                                slotItem.Id = item.Id;
                                slotItem.Capacity = item.Capacity;
                                addhour++;
                                slotItem.Price = item.Price;
                                formatSlots.Add(slotItem);
                            }
                        }
                    }
                    formatSlots = formatSlots.OrderBy(x => x.Start).ToList();
                }

                // Bind table with slots and data
                var startSlot = await _settingService.GetSettingByKeyAsync<int>("Alchub.Slot.Start.Time", defaultValue: 2);
                var endSlot = await _settingService.GetSettingByKeyAsync<int>("Alchub.Slot.End.Time", defaultValue: 23);

                for (int i = 0; i < Weeks.Count; i++)
                {
                    var bookSlot = new BookingSlotModel();
                    bookSlot.Start = Weeks[i];
                    bookSlot.End = Weeks[i];

                    for (int j = startSlot; j < endSlot; j++)
                    {
                        SlotDefault slotDefault = new SlotDefault();
                        var available = formatSlots.FirstOrDefault(x => x.Start.Equals(Weeks[i].Date.AddHours(j)));

                        if (available != null)
                        {
                            if (available.Start > Convert.ToDateTime(startDate) && await CheckSlotAvailability(available.Id, available.Start, available.End, available.Capacity, false))
                            {

                                var originalSlotTime = (Convert.ToInt32("0") + Convert.ToInt32(j) + ":00" + "-" + (Convert.ToInt32(j) + Convert.ToInt32(1)) + ":00").ToString();
                                slotDefault.Slot = originalSlotTime;
                                slotDefault.SlotTime2 = SlotHelper.ConvertTo12hoursSlotTime(originalSlotTime);
                                slotDefault.IsAvailable = true;
                                slotDefault.IsBook = false;
                                slotDefault.Price = await _priceFormatter.FormatPriceAsync(available.Price);
                                slotDefault.Id = available.Id;
                                slotDefault.BlockId = j;
                                slotDefault.Capacity = available.Capacity;
                                slotDefault.IsPickup = false;
                                slotDefault.Start = available.Start;
                                bookSlot.SlotModel.Add(slotDefault);
                            }
                            else
                            {

                                slotDefault.IsPickup = true;
                                slotDefault.IsAvailable = false;
                                var originalSlotTime = (Convert.ToInt32("0") + Convert.ToInt32(j) + ":00" + "-" + (Convert.ToInt32(j) + Convert.ToInt32(1)) + ":00").ToString();
                                slotDefault.Slot = originalSlotTime;
                                slotDefault.SlotTime2 = SlotHelper.ConvertTo12hoursSlotTime(originalSlotTime);
                                slotDefault.Start = Weeks[i];
                                bookSlot.SlotModel.Add(slotDefault);
                            }
                        }
                        else
                        {
                            slotDefault.IsPickup = true;
                            slotDefault.IsAvailable = false;
                            var originalSlotTime = (Convert.ToInt32("0") + Convert.ToInt32(j) + ":00" + "-" + (Convert.ToInt32(j) + Convert.ToInt32(1)) + ":00").ToString();
                            slotDefault.Slot = originalSlotTime;
                            slotDefault.SlotTime2 = SlotHelper.ConvertTo12hoursSlotTime(originalSlotTime);
                            slotDefault.Start = Weeks[i];
                            bookSlot.SlotModel.Add(slotDefault);
                        }
                    }
                    //add if at least 1 slot is available
                    var anySlotAvailable = bookSlot.SlotModel.Any(x => x.IsAvailable);
                    if (anySlotAvailable)
                        resultModel.Add(bookSlot);
                }


                //Selected insert into table
                //Selected insert into table
                IList<CustomerProductSlot> cutomerProductSlotList = new List<CustomerProductSlot>();
                cutomerProductSlotList = await _slotService.GetCustomerProductSlot(productId, customer.Id, Convert.ToDateTime(startDate), isPickup);
                if (cutomerProductSlotList.Count == 0)
                {
                    var bookSlot = resultModel.Select(x => x.SlotModel.FirstOrDefault(x => x.IsAvailable == true)).FirstOrDefault();
                    if (bookSlot != null)
                    {
                        CustomerProductSlot customerProductSlot = new CustomerProductSlot();
                        bookSlot.IsBook = true;
                        bookSlot.IsSelected = false;
                        var slot = await _slotService.GetSlotById(bookSlot.Id);
                        customerProductSlot.SlotId = bookSlot.Id;
                        customerProductSlot.BlockId = bookSlot.BlockId;
                        customerProductSlot.StartTime = bookSlot.Start;
                        customerProductSlot.EndDateTime = bookSlot.Start;
                        customerProductSlot.EndTime = bookSlot.Slot;
                        customerProductSlot.CustomerId = customer.Id;
                        customerProductSlot.Price = Convert.ToDecimal(slot != null ? slot.Price : 0);
                        customerProductSlot.ProductId = productId;
                        customerProductSlot.IsPickup = isPickup;
                        customerProductSlot.LastUpdated = DateTime.UtcNow;
                        customerProductSlot.IsSelected = false;
                        await _slotService.InsertCustomerProductSlot(customerProductSlot);
                    }
                }
                else
                {
                    foreach (var cutomerProductSlot in cutomerProductSlotList)
                    {
                        var slotElement = resultModel.Select(x => x.SlotModel.FirstOrDefault(x => x.Id == cutomerProductSlot.SlotId && x.Slot == cutomerProductSlot.EndTime)).FirstOrDefault();
                        if (slotElement != null)
                        {
                            if (cutomerProductSlot.IsSelected)
                            {
                                slotElement.IsBook = false;
                                slotElement.IsSelected = true;
                            }
                        }
                    }
                    var bookSlot = resultModel.Select(x => x.SlotModel.FirstOrDefault(x => x.IsAvailable == true)).FirstOrDefault();
                    if (bookSlot != null)
                    {
                        if (!await _slotService.GetCustomerProductSlotByDate(productId, customer.Id, Convert.ToDateTime(bookSlot.Start), bookSlot.Slot, isPickup))
                        {
                            await _slotService.DeleteCustomerProductSlot(productId, customer.Id);
                            CustomerProductSlot customerProductSlot = new CustomerProductSlot();
                            bookSlot.IsBook = true;
                            bookSlot.IsSelected = false;
                            var slot = await _slotService.GetSlotById(bookSlot.Id);
                            customerProductSlot.SlotId = bookSlot.Id;
                            customerProductSlot.BlockId = bookSlot.BlockId;
                            customerProductSlot.StartTime = bookSlot.Start;
                            customerProductSlot.EndDateTime = bookSlot.Start;
                            customerProductSlot.EndTime = bookSlot.Slot;
                            customerProductSlot.CustomerId = customer.Id;
                            customerProductSlot.Price = Convert.ToDecimal(slot != null ? slot.Price : 0);
                            customerProductSlot.ProductId = productId;
                            customerProductSlot.IsPickup = isPickup;
                            customerProductSlot.LastUpdated = DateTime.UtcNow;
                            customerProductSlot.IsSelected = false;
                            await _slotService.InsertCustomerProductSlot(customerProductSlot);
                        }
                        else
                        {
                            bookSlot.IsBook = true;
                            bookSlot.IsSelected = false;
                        }
                    }
                }
            }
            return resultModel.ToList();
        }

        public async Task<SlotModel> PrepareTodaySlotListModel(int slotId, string startDate, string endDate, int productId, int vendorId, bool isPickup = false, bool isDelivery = false, bool isAdmin = false)
        {
            var productCategories = (await _categoryService.GetProductCategoriesByProductIdAsync(productId)).Select(x => x.CategoryId).ToList();
            IList<SlotModel> formatSlots = new List<SlotModel>();
            Zone zone = new Zone();
            // Get 7 days of week by start date
            DateTime startD = Convert.ToDateTime(startDate);
            var Weeks = new List<DateTime>();
            for (int i = 0; i < 7; i++)
            {
                Weeks.Add(startD.AddDays(i));
            }
            if (isPickup)
            {
                zone = await _slotService.GetVendorZones(true, vendorId, isPickup);
            }
            if (isDelivery)
            {
                zone = await _slotService.GetVendorZones(true, vendorId, isPickup, createdBy: 0);
            }
            if (isAdmin)
            {
                zone = await _slotService.GetAdminCreateVendorZones(true, vendorId, false, createdBy: 1);
            }

            // Get 7 days of week by start dat
            var resultModel = new List<BookingSlotModel>();
            if (zone != null && zone.Id > 0)
            {
                var zoneId = zone != null ? zone.Id : 0;
                if (isPickup)
                {

                    List<PickupSlot> slots = new List<PickupSlot>();
                    slots = await _slotService.GetFreePickupSlotByZoneAndDate(Convert.ToDateTime(startDate), Convert.ToDateTime(endDate), zoneId);
                    if (slots != null)
                    {
                        // create hourly slots from available data
                        foreach (var item in slots)
                        {
                            int start = item.Start.TimeOfDay.Hours, end = item.End.TimeOfDay.Hours;
                            int addhour = 0;
                            if (!await _slotService.FindSlotCategoryIdExist(item.Id, productCategories))
                            {
                                for (int i = start; i < end; i++)
                                {
                                    SlotModel slotItem = new SlotModel();
                                    slotItem.Start = item.Start.AddHours(addhour);
                                    slotItem.End = slotItem.Start.AddHours(1);
                                    slotItem.Id = item.Id;
                                    slotItem.Capacity = item.Capacity;
                                    addhour++;
                                    slotItem.Price = item.Price;
                                    formatSlots.Add(slotItem);
                                }
                            }
                        }
                        formatSlots = formatSlots.OrderBy(x => x.Start).ToList();
                    }
                }
                else
                {
                    List<Slot> slots = new List<Slot>();
                    slots = await _slotService.GetFreeSlotByZoneAndDate(Convert.ToDateTime(startDate), Convert.ToDateTime(endDate), zoneId);
                    // create hourly slots from available data
                    foreach (var item in slots)
                    {
                        int start = item.Start.TimeOfDay.Hours, end = item.End.TimeOfDay.Hours;
                        int addhour = 0;
                        if (!await _slotService.FindSlotCategoryIdExist(item.Id, productCategories))
                        {
                            for (int i = start; i < end; i++)
                            {
                                SlotModel slotItem = new SlotModel();
                                slotItem.Start = item.Start.AddHours(addhour);
                                slotItem.End = slotItem.Start.AddHours(1);
                                slotItem.Id = item.Id;
                                slotItem.Capacity = item.Capacity;
                                addhour++;
                                slotItem.Price = item.Price;
                                formatSlots.Add(slotItem);
                            }
                        }
                    }
                    formatSlots = formatSlots.OrderBy(x => x.Start).ToList();
                }
            }

            // Bind table with slots and data
            var startSlot = await _settingService.GetSettingByKeyAsync<int>("Alchub.Slot.Start.Time", defaultValue: 2);
            var endSlot = await _settingService.GetSettingByKeyAsync<int>("Alchub.Slot.End.Time", defaultValue: 23);

            for (int i = 0; i < Weeks.Count; i++)
            {
                for (int j = startSlot; j < endSlot; j++)
                {
                    var available = formatSlots.FirstOrDefault(x => x.Start.Equals(Weeks[i].Date.AddHours(j)));

                    var bookSlot = new BookingSlotModel();

                    if (available != null)
                    {
                        if (available.Start > Convert.ToDateTime(startDate) && await CheckSlotAvailability(available.Id, available.Start, available.End, available.Capacity, false))
                        {
                            return available;
                        }

                    }
                }
            }
            return null;
        }

        public async Task<bool> CheckSlotAvailability(int slotId, DateTime start, DateTime end, int capacity, bool fromOrder)
        {
            bool result = true;
            try
            {
                int bookedCustomerSlot = await _slotService.GetCustomerSlotsBySlotIdAndDate(slotId, start, end);
                if (bookedCustomerSlot < capacity)
                {
                    //Check temporary booked slots
                    var tempBookedSlots = await _slotService.GetCustomerBookSlotId(slotId);
                    if (tempBookedSlots.Count > 0)
                    {
                        int count = 0;
                        foreach (var item in tempBookedSlots)
                        {
                            var time = item.EndTime;
                            DateTime st = Convert.ToDateTime(time);
                            DateTime tempStartTime = new DateTime(start.Year, start.Month, start.Day, st.Hour, st.Minute, st.Second);

                            DateTime et = Convert.ToDateTime(time);
                            DateTime tempEndTime = new DateTime(end.Year, end.Month, end.Day, et.Hour, et.Minute, et.Second);
                            if (slotId == item.SlotId && start == tempStartTime && end == tempEndTime)
                            {
                                count++;
                            }

                        }
                        if (bookedCustomerSlot + count >= capacity)
                            result = false;
                        else
                            result = true;
                    }
                }
                else
                {
                    result = false;
                }
                return result;
            }
            catch (Exception exc)
            {
                return result;
            }

        }

        private async Task PrepareProductFastestSlotAsync(ProductVendorModel.ProductDetailVendors productVendorModel, Product product, Customer customer)
        {
            //load fastest slot
            var (fastestSlotTime, slot) = await PrepareProductFastestSlotAsync(product, customer);
            var slotStartEndTime = slot != null ? $"{slot.Start.ToString("HH:mm")}-{slot.End.ToString("HH:mm")}" : string.Empty;
            if (productVendorModel.DeliveryAvailable)
            {
                if (productVendorModel.ManageDelivery)
                {
                    productVendorModel.Date = slot != null ? slot.Start.ToString("MM/dd/yyyy").Replace('/', '-') : "";
                    productVendorModel.StartTime = slotStartEndTime;
                    productVendorModel.StartTime2 = SlotHelper.ConvertTo12hoursSlotTime(slotStartEndTime);
                    return;
                }
                else
                {
                    productVendorModel.Date = slot != null ? slot.Start.ToString("MM/dd/yyyy").Replace('/', '-') : "";
                    productVendorModel.StartTime = slotStartEndTime;
                    productVendorModel.StartTime2 = SlotHelper.ConvertTo12hoursSlotTime(slotStartEndTime);
                    return;
                }
            }
            if (productVendorModel.PickAvailable)
            {
                productVendorModel.Date = slot != null ? slot.Start.ToString("MM/dd/yyyy").Replace('/', '-') : "";
                productVendorModel.StartTime = slotStartEndTime;
                productVendorModel.StartTime2 = SlotHelper.ConvertTo12hoursSlotTime(slotStartEndTime);
                return;
            }
        }

        /// <summary>
        /// Prepare fastest slot string
        /// </summary>
        /// <param name="product"></param>
        /// <returns></returns>
        public async Task<(string slotTiming, SlotModel slotModel)> PrepareProductFastestSlotAsync(Product product, Customer customer)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            //check for grouped product - pass default product variant if so.
            if (product.ProductType == ProductType.GroupedProduct)
            {
                var store = await _storeContext.GetCurrentStoreAsync();
                var associatedProducts = await _productService.GetAssociatedProductsAsync(product.Id,
                store.Id,
                //++Alchub geovendor
                geoVendorIds: await _workContext.GetCurrentCustomerGeoRadiusVendorIdsAsync(customer));
                if (associatedProducts != null && associatedProducts.Any())
                {
                    product = await _alchubGeneralService.GetGroupedProductDefaultVariantAsync(associatedProducts);
                }
            }

            var availableVendors = await _vendorService.GetAvailableGeoFenceVendorsAsync(customer, true, product);
            //prepare availbale vendors productss
            var productVendors = await PrepareProductVendorListAsync(product, availableVendors, customer, false, false);
            var addDay = await _settingService.GetSettingByKeyAsync<int>("Alchub.Slot.Visible.After.Day", defaultValue: 1);
            var addHour = await _settingService.GetSettingByKeyAsync<int>("Alchub.Slot.Add.Hour", defaultValue: 0);
            var addMintues = await _settingService.GetSettingByKeyAsync<int>("Alchub.Slot.Add.Mintues", defaultValue: 0);
            var dateTime = await _dateTimeHelper.ConvertToUserTimeAsync(DateTime.Now);
            dateTime = dateTime.AddDays(addDay).AddHours(addHour).AddMinutes(addMintues);

            string slotTiming = string.Empty;
            if (productVendors.Count > 0)
            {
                //sort by time
                productVendors = productVendors.OrderBy(x => x?.Time).ThenBy(v => v.DistanceValue).ToList();
                if (productVendors.FirstOrDefault().DeliveryAvailable)
                {
                    if (productVendors.FirstOrDefault().ManageDelivery)
                    {
                        var slot = await PrepareTodaySlotListModel(0, dateTime.ToString(), dateTime.AddDays(7).ToString(), productVendors?.FirstOrDefault()?.ProductId ?? 0, productVendors?.FirstOrDefault()?.VendorId ?? 0, false, true, false);
                        if (slot != null)
                        {
                            var slotStartEndTime = $"{slot.Start.ToString("HH:mm")}-{slot.End.ToString("HH:mm")}";
                            slotTiming = $"{slot.Start.DayOfWeek.ToString()}, {SlotHelper.ConvertTo12hoursSlotTime(slotStartEndTime)}";
                        }
                        return (slotTiming, slot);
                    }
                    else
                    {
                        var slot = await PrepareTodaySlotListModel(0, dateTime.ToString(), dateTime.AddDays(7).ToString(), productVendors?.FirstOrDefault()?.ProductId ?? 0, productVendors?.FirstOrDefault()?.VendorId ?? 0, false, false, true);
                        if (slot != null)
                        {
                            var slotStartEndTime = $"{slot.Start.ToString("HH:mm")}-{slot.End.ToString("HH:mm")}";
                            slotTiming = $"{slot.Start.DayOfWeek.ToString()}, {SlotHelper.ConvertTo12hoursSlotTime(slotStartEndTime)}";
                        }
                        return (slotTiming, slot);
                    }
                }
                if (productVendors.FirstOrDefault().PickAvailable)
                {
                    var slot = await PrepareTodaySlotListModel(0, dateTime.ToString(), dateTime.AddDays(7).ToString(), productVendors?.FirstOrDefault()?.ProductId ?? 0, productVendors?.FirstOrDefault()?.VendorId ?? 0, true, false, false);
                    if (slot != null)
                    {
                        var slotStartEndTime = $"{slot.Start.ToString("HH:mm")}-{slot.End.ToString("HH:mm")}";
                        slotTiming = $"{slot.Start.DayOfWeek.ToString()}, {SlotHelper.ConvertTo12hoursSlotTime(slotStartEndTime)}";
                    }
                    return (slotTiming, slot);
                }
            }

            return (slotTiming, null);
        }

        #endregion
        #endregion
    }
}