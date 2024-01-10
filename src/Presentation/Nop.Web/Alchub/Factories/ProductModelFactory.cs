using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Alchub.Domain;
using Nop.Core.Alchub.Domain.Google;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.DeliveryFees;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Security;
using Nop.Core.Domain.Seo;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Slots;
using Nop.Core.Domain.Stores;
using Nop.Core.Domain.Vendors;
using Nop.Services;
using Nop.Services.Alchub.General;
using Nop.Services.Alchub.Google;
using Nop.Services.Alchub.Slots;
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
using Nop.Web.Alchub.Models.Catalog;
using Nop.Web.Infrastructure.Cache;
using Nop.Web.Models.Catalog;
using Nop.Web.Models.Media;
using Nop.Web.Models.Slots;

namespace Nop.Web.Factories
{
    /// <summary>
    /// Represents the product model factory
    /// </summary>
    public partial class ProductModelFactory : IProductModelFactory
    {
        #region Fields

        private readonly CaptchaSettings _captchaSettings;
        private readonly CatalogSettings _catalogSettings;
        private readonly CustomerSettings _customerSettings;
        private readonly ICategoryService _categoryService;
        private readonly ICurrencyService _currencyService;
        private readonly ICustomerService _customerService;
        private readonly IDateRangeService _dateRangeService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IDownloadService _downloadService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ILocalizationService _localizationService;
        private readonly IManufacturerService _manufacturerService;
        private readonly IPermissionService _permissionService;
        private readonly IPictureService _pictureService;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly IProductAttributeService _productAttributeService;
        private readonly IProductService _productService;
        private readonly IProductTagService _productTagService;
        private readonly IProductTemplateService _productTemplateService;
        private readonly IReviewTypeService _reviewTypeService;
        private readonly ISpecificationAttributeService _specificationAttributeService;
        private readonly IStaticCacheManager _staticCacheManager;
        private readonly IStoreContext _storeContext;
        private readonly IShoppingCartModelFactory _shoppingCartModelFactory;
        private readonly ITaxService _taxService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IVendorService _vendorService;
        private readonly IWebHelper _webHelper;
        private readonly IWorkContext _workContext;
        private readonly MediaSettings _mediaSettings;
        private readonly OrderSettings _orderSettings;
        private readonly SeoSettings _seoSettings;
        private readonly ShippingSettings _shippingSettings;
        private readonly VendorSettings _vendorSettings;
        private readonly IAlchubGeneralService _alchubGeneralService;
        private readonly ISettingService _settingService;
        private readonly ISlotService _slotService;
        private readonly IGeoService _geoService;
        private CustomerProductSlot cutomerProductSlot;
        private readonly IDeliveryFeeService _deliveryFeeService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly AlchubSettings _alchubSettings;
        private readonly IAlchubSameNameProductService _alchubSameNameProductService;
        #endregion

        #region Ctor

        public ProductModelFactory(CaptchaSettings captchaSettings,
            CatalogSettings catalogSettings,
            CustomerSettings customerSettings,
            ICategoryService categoryService,
            ICurrencyService currencyService,
            ICustomerService customerService,
            IDateRangeService dateRangeService,
            IDateTimeHelper dateTimeHelper,
            IDownloadService downloadService,
            IGenericAttributeService genericAttributeService,
            ILocalizationService localizationService,
            IManufacturerService manufacturerService,
            IPermissionService permissionService,
            IPictureService pictureService,
            IPriceCalculationService priceCalculationService,
            IPriceFormatter priceFormatter,
            IProductAttributeParser productAttributeParser,
            IProductAttributeService productAttributeService,
            IProductService productService,
            IProductTagService productTagService,
            IProductTemplateService productTemplateService,
            IReviewTypeService reviewTypeService,
            ISpecificationAttributeService specificationAttributeService,
            IStaticCacheManager staticCacheManager,
            IStoreContext storeContext,
            IShoppingCartModelFactory shoppingCartModelFactory,
            ITaxService taxService,
            IUrlRecordService urlRecordService,
            IVendorService vendorService,
            IWebHelper webHelper,
            IWorkContext workContext,
            MediaSettings mediaSettings,
            OrderSettings orderSettings,
            SeoSettings seoSettings,
            ShippingSettings shippingSettings,
            VendorSettings vendorSettings,
            IAlchubGeneralService alchubGeneralService,
            ISettingService settingService,
            ISlotService slotService,
            IGeoService geoService,
            IDeliveryFeeService deliveryFeeService,
            IShoppingCartService shoppingCartService,
            AlchubSettings alchubSettings,
            IAlchubSameNameProductService alchubSameNameProductService)
        {
            _captchaSettings = captchaSettings;
            _catalogSettings = catalogSettings;
            _customerSettings = customerSettings;
            _categoryService = categoryService;
            _currencyService = currencyService;
            _customerService = customerService;
            _dateRangeService = dateRangeService;
            _dateTimeHelper = dateTimeHelper;
            _downloadService = downloadService;
            _genericAttributeService = genericAttributeService;
            _localizationService = localizationService;
            _manufacturerService = manufacturerService;
            _permissionService = permissionService;
            _pictureService = pictureService;
            _priceCalculationService = priceCalculationService;
            _priceFormatter = priceFormatter;
            _productAttributeParser = productAttributeParser;
            _productAttributeService = productAttributeService;
            _productService = productService;
            _productTagService = productTagService;
            _productTemplateService = productTemplateService;
            _reviewTypeService = reviewTypeService;
            _specificationAttributeService = specificationAttributeService;
            _staticCacheManager = staticCacheManager;
            _storeContext = storeContext;
            _shoppingCartModelFactory = shoppingCartModelFactory;
            _taxService = taxService;
            _urlRecordService = urlRecordService;
            _vendorService = vendorService;
            _webHelper = webHelper;
            _workContext = workContext;
            _mediaSettings = mediaSettings;
            _orderSettings = orderSettings;
            _seoSettings = seoSettings;
            _shippingSettings = shippingSettings;
            _vendorSettings = vendorSettings;
            _alchubGeneralService = alchubGeneralService;
            _settingService = settingService;
            _slotService = slotService;
            _geoService = geoService;
            _deliveryFeeService = deliveryFeeService;
            _shoppingCartService = shoppingCartService;
            _alchubSettings = alchubSettings;
            _alchubSameNameProductService = alchubSameNameProductService;
        }

        #endregion

        #region Utilities

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

            var pictureModel = new PictureModel
            {
                ImageUrl = imageUrl,
                FullSizeImageUrl = fullSizeImageUrl,
                //"title" attribute
                Title = string.Format(await _localizationService.GetResourceAsync("Media.Product.ImageLinkTitleFormat"),
                            product.Name),
                //"alt" attribute
                AlternateText = string.Format(await _localizationService.GetResourceAsync("Media.Product.ImageAlternateTextFormat"),
                            product.Name)
            };

            return pictureModel;
            //End Alchub
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
                        var customer = await _workContext.GetCurrentCustomerAsync();
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

        protected virtual async Task<string> PrepareVendorAvailableSlotsAsync(int productId, int vendorId, bool pickUp = false, bool delivery = false, bool deliveryAvailable = false)
        {
            var addDay = await _settingService.GetSettingByKeyAsync<int>("Alchub.Slot.Visible.After.Day", defaultValue: 1);
            var addHour = await _settingService.GetSettingByKeyAsync<int>("Alchub.Slot.Add.Hour", defaultValue: 0);
            var addMintues = await _settingService.GetSettingByKeyAsync<int>("Alchub.Slot.Add.Mintues", defaultValue: 0);
            var dateTime = await _dateTimeHelper.ConvertToUserTimeAsync(DateTime.Now);

            //available slots
            dateTime = dateTime.AddDays(addDay).AddHours(addHour).AddMinutes(addMintues);
            //dateTime = dateTime.AddDays(addDay).AddHours(addHour).AddMinutes(addMintues);
            var slots = new List<dynamic>();
            var dynamicSlots = slots.AsEnumerable();
            var start = string.Empty;
            if (deliveryAvailable)
            {
                //define manage delivery 
                if (delivery)
                {
                    dynamicSlots = await PreparepareSlotListModel(0, dateTime.ToString(), dateTime.AddDays(7).ToString(),
                       productId, vendorId, false);
                    foreach (var slotsDate in dynamicSlots)
                    {
                        foreach (var item in slotsDate)
                        {
                            if (item.IsAvailable && item.IsBook)
                                start = Convert.ToString(item.Start);
                        }
                    }
                    //return start?.ToString();
                }

                else
                {
                    dynamicSlots = await PreparepareAdminSlotListModel(0, dateTime.ToString(), dateTime.AddDays(7).ToString(),
                    productId, vendorId, false);
                    foreach (var slotsDate in dynamicSlots)
                    {
                        foreach (var item in slotsDate)
                        {
                            if (item.IsAvailable && item.IsBook)
                                start = Convert.ToString(item.Start);
                        }
                    }
                    //return start?.ToString();
                }
            }
            if (string.IsNullOrEmpty(start) && pickUp)
            {
                dynamicSlots = await PrepareparePickupSlotListModel(0, dateTime.ToString(), dateTime.AddDays(7).ToString(),
                                productId, vendorId, true);
                foreach (var slotsDate in dynamicSlots)
                {
                    foreach (var item in slotsDate)
                    {
                        if (item.IsAvailable && item.IsBook)
                            start = Convert.ToString(item.Start);
                    }
                }
                return start?.ToString();
            }

            return start?.ToString();
        }

        /// <returns>A task that represents the asynchronous operation</returns>
        protected virtual async Task PrepareSimpleProductOverviewPriceModelAsync(Product product, ProductOverviewModel.ProductPriceModel priceModel)
        {
            //add to cart button
            //commenting for performance optimization  ++Alchub
            //priceModel.DisableBuyButton = product.DisableBuyButton ||
            //                              !await _permissionService.AuthorizeAsync(StandardPermissionProvider.EnableShoppingCart) ||
            //                              !await _permissionService.AuthorizeAsync(StandardPermissionProvider.DisplayPrices);

            ////add to wishlist button
            //priceModel.DisableWishlistButton = product.DisableWishlistButton ||
            //                                   !await _permissionService.AuthorizeAsync(StandardPermissionProvider.EnableWishlist) ||
            //                                   !await _permissionService.AuthorizeAsync(StandardPermissionProvider.DisplayPrices);
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

            //++Alchub
            //Performance optimization: since we do not use default pricing, lets skip default price calculation

            ////prices
            //if (await _permissionService.AuthorizeAsync(StandardPermissionProvider.DisplayPrices))
            //{
            //    if (product.CustomerEntersPrice)
            //        return;

            //    if (product.CallForPrice &&
            //        //also check whether the current user is impersonated
            //        (!_orderSettings.AllowAdminsToBuyCallForPriceProducts ||
            //         _workContext.OriginalCustomerIfImpersonated == null))
            //    {
            //        //call for price
            //        priceModel.OldPrice = null;
            //        priceModel.OldPriceValue = null;
            //        priceModel.Price = await _localizationService.GetResourceAsync("Products.CallForPrice");
            //        priceModel.PriceValue = null;
            //    }
            //    else
            //    {
            //        //prices
            //        var customer = await _workContext.GetCurrentCustomerAsync();
            //        var (minPossiblePriceWithoutDiscount, minPossiblePriceWithDiscount, _, _) = await _priceCalculationService.GetFinalPriceAsync(product, customer);

            //        if (product.HasTierPrices)
            //        {
            //            var (tierPriceMinPossiblePriceWithoutDiscount, tierPriceMinPossiblePriceWithDiscount, _, _) = await _priceCalculationService.GetFinalPriceAsync(product, customer, quantity: int.MaxValue);

            //            //calculate price for the maximum quantity if we have tier prices, and choose minimal
            //            minPossiblePriceWithoutDiscount = Math.Min(minPossiblePriceWithoutDiscount, tierPriceMinPossiblePriceWithoutDiscount);
            //            minPossiblePriceWithDiscount = Math.Min(minPossiblePriceWithDiscount, tierPriceMinPossiblePriceWithDiscount);
            //        }

            //        var (oldPriceBase, _) = await _taxService.GetProductPriceAsync(product, product.OldPrice);
            //        var (finalPriceWithoutDiscountBase, _) = await _taxService.GetProductPriceAsync(product, minPossiblePriceWithoutDiscount);
            //        var (finalPriceWithDiscountBase, _) = await _taxService.GetProductPriceAsync(product, minPossiblePriceWithDiscount);
            //        var currentCurrency = await _workContext.GetWorkingCurrencyAsync();
            //        var oldPrice = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(oldPriceBase, currentCurrency);
            //        var finalPriceWithoutDiscount = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(finalPriceWithoutDiscountBase, currentCurrency);
            //        var finalPriceWithDiscount = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(finalPriceWithDiscountBase, currentCurrency);

            //        //do we have tier prices configured?
            //        var tierPrices = new List<TierPrice>();
            //        if (product.HasTierPrices)
            //        {
            //            var store = await _storeContext.GetCurrentStoreAsync();
            //            tierPrices.AddRange(await _productService.GetTierPricesAsync(product, customer, store.Id));
            //        }
            //        //When there is just one tier price (with  qty 1), there are no actual savings in the list.
            //        var displayFromMessage = tierPrices.Any() && !(tierPrices.Count == 1 && tierPrices[0].Quantity <= 1);
            //        if (displayFromMessage)
            //        {
            //            priceModel.OldPrice = null;
            //            priceModel.OldPriceValue = null;
            //            priceModel.Price = string.Format(await _localizationService.GetResourceAsync("Products.PriceRangeFrom"), await _priceFormatter.FormatPriceAsync(finalPriceWithDiscount));
            //            priceModel.PriceValue = finalPriceWithDiscount;
            //        }
            //        else
            //        {
            //            var strikeThroughPrice = decimal.Zero;

            //            if (finalPriceWithoutDiscountBase != oldPriceBase && oldPriceBase > decimal.Zero)
            //                strikeThroughPrice = oldPrice;

            //            if (finalPriceWithoutDiscountBase != finalPriceWithDiscountBase)
            //                strikeThroughPrice = finalPriceWithoutDiscount;

            //            if (strikeThroughPrice > decimal.Zero)
            //            {
            //                priceModel.OldPrice = await _priceFormatter.FormatPriceAsync(strikeThroughPrice);
            //                priceModel.OldPriceValue = strikeThroughPrice;
            //            }

            //            priceModel.Price = await _priceFormatter.FormatPriceAsync(finalPriceWithDiscount);
            //            priceModel.PriceValue = finalPriceWithDiscount;
            //        }

            //        if (product.IsRental)
            //        {
            //            //rental product
            //            priceModel.OldPrice = await _priceFormatter.FormatRentalProductPeriodAsync(product, priceModel.OldPrice);
            //            priceModel.OldPriceValue = priceModel.OldPriceValue;
            //            priceModel.Price = await _priceFormatter.FormatRentalProductPeriodAsync(product, priceModel.Price);
            //            priceModel.PriceValue = priceModel.PriceValue;
            //        }

            //        //property for German market
            //        //we display tax/shipping info only with "shipping enabled" for this product
            //        //we also ensure this it's not free shipping
            //        priceModel.DisplayTaxShippingInfo = _catalogSettings.DisplayTaxShippingInfoProductBoxes && product.IsShipEnabled && !product.IsFreeShipping;

            //        //PAngV default baseprice (used in Germany)
            //        priceModel.BasePricePAngV = await _priceFormatter.FormatBasePriceAsync(product, finalPriceWithDiscount);
            //        priceModel.BasePricePAngVValue = finalPriceWithDiscount;
            //    }
            //}
            //else
            //{
            //    //hide prices
            //    priceModel.OldPrice = null;
            //    priceModel.OldPriceValue = null;
            //    priceModel.Price = null;
            //    priceModel.PriceValue = null;
            //}

            //price range
            await PrepareProductPriceRangeAsync(product, priceModel);

            //Fastest slot
            //await PrepareProductFastestSlotAsync(product, priceModel);

            ///wishlist
            //var customerWishlist = await _workContext.GetCurrentCustomerAsync();
            //var storeWishlist = await _storeContext.GetCurrentStoreAsync();
            //if (customerWishlist.HasShoppingCartItems)
            //{
            //    var wishList = await _shoppingCartService.GetShoppingCartAsync(customerWishlist, ShoppingCartType.Wishlist, storeWishlist.Id);
            //    priceModel.IsWishlist = wishList.Any(x => x.ProductId == product.Id);
            //}
        }

        /// <returns>A task that represents the asynchronous operation</returns>
        protected virtual async Task PrepareGroupedProductOverviewPriceModelAsync(Product product, ProductOverviewModel.ProductPriceModel priceModel)
        {
            var store = await _storeContext.GetCurrentStoreAsync();
            var associatedProducts = await _productService.GetAssociatedProductsAsync(product.Id,
                store.Id,
                //++Alchub geovendor
                geoVendorIds: await _workContext.GetCurrentCustomerGeoRadiusVendorIdsAsync());

            //commenting for performance optimization ++Alchub
            ////add to cart button (ignore "DisableBuyButton" property for grouped products)
            //priceModel.DisableBuyButton =
            //                !await _permissionService.AuthorizeAsync(StandardPermissionProvider.EnableShoppingCart) ||
            //                !await _permissionService.AuthorizeAsync(StandardPermissionProvider.DisplayPrices);

            ////add to wishlist button (ignore "DisableWishlistButton" property for grouped products)
            //priceModel.DisableWishlistButton =
            //                !await _permissionService.AuthorizeAsync(StandardPermissionProvider.EnableWishlist) ||
            //                !await _permissionService.AuthorizeAsync(StandardPermissionProvider.DisplayPrices);

            //compare products
            priceModel.DisableAddToCompareListButton = !_catalogSettings.CompareProductsEnabled;
            if (!associatedProducts.Any())
                return;

            //++Alchub
            //Performance optimization: since we do not use default pricing, lets skip default price calculation

            ////we have at least one associated product
            //if (await _permissionService.AuthorizeAsync(StandardPermissionProvider.DisplayPrices) && !skipPricePreparation)
            //{
            //    //find a minimum possible price
            //    decimal? minPossiblePrice = null;
            //    Product minPriceProduct = null;
            //    var customer = await _workContext.GetCurrentCustomerAsync();
            //    foreach (var associatedProduct in associatedProducts)
            //    {
            //        var (_, tmpMinPossiblePrice, _, _) = await _priceCalculationService.GetFinalPriceAsync(associatedProduct, customer);

            //        if (associatedProduct.HasTierPrices)
            //        {
            //            //calculate price for the maximum quantity if we have tier prices, and choose minimal
            //            tmpMinPossiblePrice = Math.Min(tmpMinPossiblePrice,
            //                (await _priceCalculationService.GetFinalPriceAsync(associatedProduct, customer, quantity: int.MaxValue)).finalPrice);
            //        }

            //        if (minPossiblePrice.HasValue && tmpMinPossiblePrice >= minPossiblePrice.Value)
            //            continue;
            //        minPriceProduct = associatedProduct;
            //        minPossiblePrice = tmpMinPossiblePrice;
            //    }

            //    if (minPriceProduct == null || minPriceProduct.CustomerEntersPrice)
            //        return;

            //    if (minPriceProduct.CallForPrice &&
            //        //also check whether the current user is impersonated
            //        (!_orderSettings.AllowAdminsToBuyCallForPriceProducts ||
            //         _workContext.OriginalCustomerIfImpersonated == null))
            //    {
            //        priceModel.OldPrice = null;
            //        priceModel.OldPriceValue = null;
            //        priceModel.Price = await _localizationService.GetResourceAsync("Products.CallForPrice");
            //        priceModel.PriceValue = null;
            //    }
            //    else
            //    {
            //        //calculate prices
            //        var (finalPriceBase, _) = await _taxService.GetProductPriceAsync(minPriceProduct, minPossiblePrice.Value);
            //        var finalPrice = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(finalPriceBase, await _workContext.GetWorkingCurrencyAsync());

            //        priceModel.OldPrice = null;
            //        priceModel.OldPriceValue = null;
            //        priceModel.Price = string.Format(await _localizationService.GetResourceAsync("Products.PriceRangeFrom"), await _priceFormatter.FormatPriceAsync(finalPrice));
            //        priceModel.PriceValue = finalPrice;

            //        //PAngV default baseprice (used in Germany)
            //        priceModel.BasePricePAngV = await _priceFormatter.FormatBasePriceAsync(product, finalPriceBase);
            //        priceModel.BasePricePAngVValue = finalPriceBase;
            //    }
            //}
            //else
            //{
            //    //hide prices
            //    priceModel.OldPrice = null;
            //    priceModel.OldPriceValue = null;
            //    priceModel.Price = null;
            //    priceModel.PriceValue = null;
            //}

            //price range
            await PrepareGroupedProductPriceRangeAsync(associatedProducts, priceModel);

            //await PrepareProductPriceRangeAsync(product, priceModel);

            //await PrepareProductFastestSlotAsync(product, priceModel);

            ///wishlist
            var customerWishlist = await _workContext.GetCurrentCustomerAsync();
            if (customerWishlist.HasShoppingCartItems)
            {
                var wishList = await _shoppingCartService.GetShoppingCartAsync(customerWishlist, ShoppingCartType.Wishlist, store.Id);
                priceModel.IsWishlist = wishList.Any(x => x.ProductId == product.Id);
            }
        }

        /// <returns>A task that represents the asynchronous operation</returns>
        protected virtual async Task PrepareProductPriceRangeAsync(Product product, ProductOverviewModel.ProductPriceModel priceModel)
        {
            //get price range
            var priceRangeDisc = await _alchubGeneralService.GetProductPriceRangeAsync(product);
            if (priceRangeDisc != null && priceRangeDisc.Any())
            {
                //price range formate string
                //show lowest variant price - 24-08-22
                priceModel.PriceRange = await _priceFormatter.FormatPriceAsync(priceRangeDisc.First().Value);

                var priceRangeProducts = new List<ProductOverviewModel.ProductPriceModel.PriceRangeProductModel>();
                foreach (var priceRangeProduct in priceRangeDisc)
                {
                    var model = new ProductOverviewModel.ProductPriceModel.PriceRangeProductModel()
                    {
                        ProductId = priceRangeProduct.Key,
                        Price = await _priceFormatter.FormatPriceAsync(priceRangeProduct.Value),
                        PrcieValue = priceRangeProduct.Value
                    };
                    priceRangeProducts.Add(model);
                }

                //price range list
                priceModel.PriceRangeProducts = priceRangeProducts;
            }
        }

        /// <returns>A task that represents the asynchronous operation</returns>
        protected virtual async Task PrepareGroupedProductPriceRangeAsync(IList<Product> associatedProducts, ProductOverviewModel.ProductPriceModel priceModel)
        {
            if (!associatedProducts.Any())
                return;

            //groupproduct price = default variants-> sub products-> product with minimum price.
            var defaultAssociatedProduct = await _alchubGeneralService.GetGroupedProductDefaultVariantAsync(associatedProducts);

            if (defaultAssociatedProduct != null)
                await PrepareProductPriceRangeAsync(defaultAssociatedProduct, priceModel);
        }

        ///// <summary>
        ///// Prepare product details vendors
        ///// </summary>
        ///// <param name="product"></param>
        ///// <param name="prepareDefaultValue"></param>
        ///// <returns></returns>
        //protected virtual async Task<IList<SelectListItem>> PrepareProductDetailVendors(Product product, IList<Vendor> availableVendors, bool prepareDefaultValue = false)
        //{
        //    var vendorItems = new List<SelectListItem>();

        //    //default value
        //    if (prepareDefaultValue)
        //    {
        //        vendorItems.Add(new SelectListItem
        //        {
        //            Text = await _localizationService.GetResourceAsync("Alchub.Products.Detail.SelectVendor"),
        //            Value = "0"
        //        });
        //    }

        //    foreach (var vendor in availableVendors)
        //        vendorItems.Add(new SelectListItem
        //        {
        //            Text = await _localizationService.GetLocalizedAsync(vendor, x => x.Name),
        //            Value = vendor.Id.ToString(),
        //            Selected = vendor.Id == product.VendorId
        //        });

        //    return vendorItems;
        //}

        /// <returns>A task that represents the asynchronous operation</returns>
        protected virtual async Task PrepareProductOverviewAlchubCustomFields(Product product, ProductOverviewModel productOverviewModel)
        {
            //prepare size & container details
            if (product.ProductType == ProductType.GroupedProduct)
            {
                var store = await _storeContext.GetCurrentStoreAsync();
                var associatedProducts = await _productService.GetAssociatedProductsAsync(product.Id,
                    store.Id,
                    //++Alchub geovendor
                    geoVendorIds: await _workContext.GetCurrentCustomerGeoRadiusVendorIdsAsync());

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

        /// <summary>
        /// Prepare the product add to cart model
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="updatecartitem">Updated shopping cart item</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the product add to cart model
        /// </returns>
        protected virtual async Task<ProductDetailsModel.AddToCartModel> PrepareProductAddToCartModelAsync(Product product, ShoppingCartItem updatecartitem)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            var model = new ProductDetailsModel.AddToCartModel
            {
                ProductId = product.Id
            };

            if (updatecartitem != null)
            {
                model.UpdatedShoppingCartItemId = updatecartitem.Id;
                model.UpdateShoppingCartItemType = updatecartitem.ShoppingCartType;
            }

            //quantity
            model.EnteredQuantity = updatecartitem != null ? updatecartitem.Quantity : product.OrderMinimumQuantity;
            //allowed quantities
            var allowedQuantities = _productService.ParseAllowedQuantities(product);
            foreach (var qty in allowedQuantities)
            {
                model.AllowedQuantities.Add(new SelectListItem
                {
                    Text = qty.ToString(),
                    Value = qty.ToString(),
                    Selected = updatecartitem != null && updatecartitem.Quantity == qty
                });
            }
            //minimum quantity notification
            if (product.OrderMinimumQuantity > 1)
            {
                model.MinimumQuantityNotification = string.Format(await _localizationService.GetResourceAsync("Products.MinimumQuantityNotification"), product.OrderMinimumQuantity);
            }

            //'add to cart', 'add to wishlist' buttons
            model.DisableBuyButton = product.DisableBuyButton || !await _permissionService.AuthorizeAsync(StandardPermissionProvider.EnableShoppingCart);
            model.DisableWishlistButton = product.DisableWishlistButton || !await _permissionService.AuthorizeAsync(StandardPermissionProvider.EnableWishlist);
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.DisplayPrices))
            {
                model.DisableBuyButton = true;
                model.DisableWishlistButton = true;
            }
            //pre-order
            if (product.AvailableForPreOrder)
            {
                model.AvailableForPreOrder = !product.PreOrderAvailabilityStartDateTimeUtc.HasValue ||
                    product.PreOrderAvailabilityStartDateTimeUtc.Value >= DateTime.UtcNow;
                model.PreOrderAvailabilityStartDateTimeUtc = product.PreOrderAvailabilityStartDateTimeUtc;

                if (model.AvailableForPreOrder &&
                    model.PreOrderAvailabilityStartDateTimeUtc.HasValue &&
                    _catalogSettings.DisplayDatePreOrderAvailability)
                {
                    model.PreOrderAvailabilityStartDateTimeUserTime =
                        (await _dateTimeHelper.ConvertToUserTimeAsync(model.PreOrderAvailabilityStartDateTimeUtc.Value)).ToString("D");
                }
            }
            //rental
            model.IsRental = product.IsRental;

            ///wishlist
            var customerWishlist = await _workContext.GetCurrentCustomerAsync();
            var storeWishlist = await _storeContext.GetCurrentStoreAsync();
            if (customerWishlist.HasShoppingCartItems)
            {
                var wishList = await _shoppingCartService.GetShoppingCartAsync(customerWishlist, ShoppingCartType.Wishlist, storeWishlist.Id);
                model.IsWishlist = wishList.Any(x => x.ProductId == product.Id);
            }


            //customer entered price
            model.CustomerEntersPrice = product.CustomerEntersPrice;
            if (!model.CustomerEntersPrice)
                return model;

            var currentCurrency = await _workContext.GetWorkingCurrencyAsync();
            var minimumCustomerEnteredPrice = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(product.MinimumCustomerEnteredPrice, currentCurrency);
            var maximumCustomerEnteredPrice = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(product.MaximumCustomerEnteredPrice, currentCurrency);

            model.CustomerEnteredPrice = updatecartitem != null ? updatecartitem.CustomerEnteredPrice : minimumCustomerEnteredPrice;
            model.CustomerEnteredPriceRange = string.Format(await _localizationService.GetResourceAsync("Products.EnterProductPrice.Range"),
                await _priceFormatter.FormatPriceAsync(minimumCustomerEnteredPrice, false, false),
                await _priceFormatter.FormatPriceAsync(maximumCustomerEnteredPrice, false, false));


            return model;
        }

        /// <summary>
        /// Prepare the product manufacturer models
        /// </summary>
        /// <param name="product">Product</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the list of manufacturer brief info model
        /// </returns>
        protected virtual async Task<IList<ManufacturerBriefInfoModel>> PrepareProductManufacturerModelsAsync(Product product)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            var manufactores = new List<ProductManufacturer>();
            if (product.ProductType == ProductType.GroupedProduct)
            {
                //gropuproduct manufacture = distinct list of all associated products manufactures (08-06-23)
                var store = _storeContext.GetCurrentStore();
                var associatedProducts = await _productService.GetAssociatedProductsAsync(product.Id, store.Id,
                                                                                             //++Alchub geovendor
                                                                                             geoVendorIds: await _workContext.GetCurrentCustomerGeoRadiusVendorIdsAsync());
                foreach (var associatedProduct in associatedProducts)
                {
                    manufactores.AddRange((await _manufacturerService.GetProductManufacturersByProductIdAsync(associatedProduct.Id))?.ToList());
                }
            }
            else
                manufactores = (await _manufacturerService.GetProductManufacturersByProductIdAsync(product.Id))?.ToList();

            //distinct manufactures (08-06-23)
            manufactores = manufactores?.DistinctBy(m => m.ManufacturerId)?.ToList();

            var model = await manufactores
                .SelectAwait(async pm =>
                {
                    var manufacturer = await _manufacturerService.GetManufacturerByIdAsync(pm.ManufacturerId);
                    var modelMan = new ManufacturerBriefInfoModel
                    {
                        Id = manufacturer.Id,
                        Name = await _localizationService.GetLocalizedAsync(manufacturer, x => x.Name),
                        SeName = await _urlRecordService.GetSeNameAsync(manufacturer)
                    };

                    return modelMan;
                }).ToListAsync();

            return model;
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
        protected virtual async Task<IList<ProductDetailsModel.GroupedProductVariantBrifInfoModel>> PrepareGroupedProductVariantsModelsAsync(Product groupedProduct, int manufacturerId)
        {
            if (groupedProduct == null)
                throw new ArgumentNullException(nameof(groupedProduct));

            //get grouped product variants
            var groupedProductVariants = await _alchubSameNameProductService.GetGroupedProductVariants(groupedProduct, manufacturerId);

            var model = await groupedProductVariants
                .SelectAwait(async gpv =>
                {
                    var modelVariant = new ProductDetailsModel.GroupedProductVariantBrifInfoModel
                    {
                        GroupedProductId = gpv.Key.Id,
                        VariantName = gpv.Value,
                        SeName = await _urlRecordService.GetSeNameAsync(gpv.Key),
                        IsActive = groupedProduct.Id == gpv.Key.Id
                    };

                    return modelVariant;
                }).ToListAsync();

            return model;
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
                //productReview.CanAddNewReview = await _productService.CanAddReviewAsync(product.Id, _catalogSettings.ShowProductReviewsPerStore ? currentStore.Id : 0);
                productReview.CanAddNewReview = true; //++Alchub default value
            }

            return productReview;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Prepare product vendors
        /// </summary>
        /// <param name="product"></param>
        /// <param name="availableVendors"></param>
        /// <returns></returns>
        public virtual async Task<IList<ProductDetailsModel.ProductVendorModel>> PrepareProductVendorsAsync(Product product, IList<Vendor> availableVendors, bool loadStartTime = true, Customer customer = null)
        {
            if (customer == null)
                customer = await _workContext.GetCurrentCustomerAsync();
            //Note: show products all vendors if location not searched. (16-09-22)
            bool ignoreGeoVendors = string.IsNullOrEmpty(customer.LastSearchedCoordinates);
            if (ignoreGeoVendors)
            {
                //get available vendors for product
                availableVendors = await _alchubGeneralService.GetVendorByMasterProductIdAsync(product);
            }

            var productVendors = new List<ProductDetailsModel.ProductVendorModel>();
            if (availableVendors == null || !availableVendors.Any())
                return productVendors;

            //get vendors who provides delivery 
            var availableDeliverableVendors = await _vendorService.GetAvailableGeoFenceVendorsAsync((await _workContext.GetCurrentCustomerAsync()), false, product);

            //get upc products
            var upcProducts = await _alchubGeneralService.GetProductsByUpcCodeAsync(product.UPCCode, true, showPublished: true);

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
                var currentCustomer = await _workContext.GetCurrentCustomerAsync();
                var store = await _storeContext.GetCurrentStoreAsync();
                var distance = "";
                decimal distanceValue = 0;
                distanceValue = await _deliveryFeeService.GetDistanceAsync(vendor.GeoLocationCoordinates, currentCustomer.LastSearchedCoordinates);

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
                if (string.IsNullOrEmpty(startTime))
                    continue;

                var productVendorModel = new ProductDetailsModel.ProductVendorModel()
                {
                    ProductId = vendorProduct.Id,
                    VendorId = vendor.Id,
                    VendorName = await _localizationService.GetLocalizedAsync(vendor, x => x.Name),
                    ManageDelivery = manageDelivery,
                    PickAvailable = vendor.PickAvailable,
                    DistanceValue = distanceValue,
                    Distance = distance,
                    StartTime = string.IsNullOrEmpty(startTime) ? DateTime.Now.AddYears(1) : Convert.ToDateTime(startTime),
                    DeliveryAvailable = deliveryAvailable,
                    VendorProductPrice = await PrepareProductPriceModelAsync(vendorProduct)
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
                //Order minimum amount
                productVendorModel.OrderAmount = await _priceFormatter.FormatPriceAsync(vendor.MinimumOrderAmount);

                productVendors.Add(productVendorModel);
            }
            return productVendors;
        }


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

                //custom alchub fields
                await PrepareProductOverviewAlchubCustomFields(product, model);

                models.Add(model);
            }

            //in wishlist (03-02-23)
            var customer = await _workContext.GetCurrentCustomerAsync();
            if (customer.HasShoppingCartItems)
            {
                var store = await _storeContext.GetCurrentStoreAsync();
                var wishList = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.Wishlist, store.Id);
                foreach (var model in models)
                {
                    model.ProductPrice.IsWishlist = wishList.Any(x => x.ProductId == model.Id);
                }
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
                AllowAddingOnlyExistingAttributeCombinations = product.AllowAddingOnlyExistingAttributeCombinations,
                //++Alhcub custom fields
                Size = product.Size,
                Container = product.Container,
                Published = product.Published
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

            //vendor details
            if (_vendorSettings.ShowVendorOnProductDetailsPage)
            {
                var vendor = await _vendorService.GetVendorByIdAsync(product.VendorId);
                if (vendor != null && !vendor.Deleted && vendor.Active)
                {
                    model.ShowVendor = true;

                    model.VendorModel = new VendorBriefInfoModel
                    {
                        Id = vendor.Id,
                        Name = await _localizationService.GetLocalizedAsync(vendor, x => x.Name),
                        SeName = await _urlRecordService.GetSeNameAsync(vendor),
                    };
                }
            }

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

            //breadcrumb
            //do not prepare this model for the associated products. anyway it's not used
            if (_catalogSettings.CategoryBreadcrumbEnabled && !isAssociatedProduct)
            {
                model.Breadcrumb = await PrepareProductBreadcrumbModelAsync(product);
            }

            //product tags
            //do not prepare this model for the associated products. anyway it's not used
            if (!isAssociatedProduct)
            {
                model.ProductTags = await PrepareProductTagModelsAsync(product);
            }

            //pictures
            model.DefaultPictureZoomEnabled = _mediaSettings.DefaultPictureZoomEnabled;
            IList<PictureModel> allPictureModels;
            (model.DefaultPictureModel, allPictureModels) = await PrepareProductDetailsPictureModelAsync(product, isAssociatedProduct);
            model.PictureModels = allPictureModels;

            //price
            model.ProductPrice = await PrepareProductPriceModelAsync(product);

            //'Add to cart' model
            model.AddToCart = await PrepareProductAddToCartModelAsync(product, updatecartitem);
            var customer = await _workContext.GetCurrentCustomerAsync();
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

            //product specifications
            //do not prepare this model for the associated products. anyway it's not used
            if (!isAssociatedProduct)
            {
                model.ProductSpecificationModel = await PrepareProductSpecificationModelAsync(product);
            }

            //product review overview
            model.ProductReviewOverview = await PrepareProductReviewOverviewModelAsync(product);

            //product add review model
            model.ProductReviews = await PrepareProductReviewsModelAsync(model.ProductReviews, product);

            //tier prices
            if (product.HasTierPrices && await _permissionService.AuthorizeAsync(StandardPermissionProvider.DisplayPrices))
            {
                model.TierPrices = await PrepareProductTierPriceModelsAsync(product);
            }

            //manufacturers
            model.ProductManufacturers = await PrepareProductManufacturerModelsAsync(product);

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

            //estimate shipping
            if (_shippingSettings.EstimateShippingProductPageEnabled && !model.IsFreeShipping)
            {
                var wrappedProduct = new ShoppingCartItem
                {
                    StoreId = store.Id,
                    ShoppingCartTypeId = (int)ShoppingCartType.ShoppingCart,
                    CustomerId = customer.Id,
                    ProductId = product.Id,
                    CreatedOnUtc = DateTime.UtcNow
                };

                var estimateShippingModel = await _shoppingCartModelFactory.PrepareEstimateShippingModelAsync(new[] { wrappedProduct });

                model.ProductEstimateShipping.ProductId = product.Id;
                model.ProductEstimateShipping.RequestDelay = estimateShippingModel.RequestDelay;
                model.ProductEstimateShipping.Enabled = estimateShippingModel.Enabled;
                model.ProductEstimateShipping.CountryId = estimateShippingModel.CountryId;
                model.ProductEstimateShipping.StateProvinceId = estimateShippingModel.StateProvinceId;
                model.ProductEstimateShipping.ZipPostalCode = estimateShippingModel.ZipPostalCode;
                model.ProductEstimateShipping.UseCity = estimateShippingModel.UseCity;
                model.ProductEstimateShipping.City = estimateShippingModel.City;
                model.ProductEstimateShipping.AvailableCountries = estimateShippingModel.AvailableCountries;
                model.ProductEstimateShipping.AvailableStates = estimateShippingModel.AvailableStates;
            }

            //associated products
            if (product.ProductType == ProductType.GroupedProduct)
            {
                //ensure no circular references
                if (!isAssociatedProduct)
                {
                    var associatedProducts = await _productService.GetAssociatedProductsAsync(product.Id, store.Id,
                                                                                              //++Alchub geovendor
                                                                                              geoVendorIds: await _workContext.GetCurrentCustomerGeoRadiusVendorIdsAsync());
                    foreach (var associatedProduct in associatedProducts)
                        model.AssociatedProducts.Add(await PrepareProductDetailsModelAsync(associatedProduct, null, true));
                }
                model.InStock = model.AssociatedProducts.Any(associatedProduct => associatedProduct.InStock);
            }

            //get available vendors
            var availableVendors = await _vendorService.GetAvailableGeoFenceVendorsAsync((await _workContext.GetCurrentCustomerAsync()), true, product);
            //prepare availbale vendors products
            model.ProductVendors = (await PrepareProductVendorsAsync(product, availableVendors))?.OrderBy(x => x.StartTime)
                                                                                                ?.ThenBy(x => x.DistanceValue)
                                                                                               ?.ToList();


            //Check with alchub details page setting true only vendor show
            if (model.ProductVendors != null && model.ProductVendors.Any() && _alchubSettings.AlchubProductDetailPageVendorTakeOne)
            {
                model.ProductVendors = model.ProductVendors.Take(1).ToList();
            }

            //prepare vendor sorting options, by default fastest
            model.VendorSortingOptions = await PrepareProductDetailsVendorSortingOptions();
            model.VendorSort = Convert.ToInt32(VendorSort.Fastest);

            var cart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, store.Id);
            if (cart.Any())
            {
                //cart product vendor ids
                var cartProductVendorIds = (await _productService.GetProductsByIdsAsync(cart.OrderBy(c => c.CreatedOnUtc)?.
                    Select(x => x.ProductId).ToArray())).Select(x => x.VendorId).ToList();

                //vendor sorting according to cart product vendor(s).
                if (cartProductVendorIds.Any())
                {
                    cartProductVendorIds.AddRange(model.ProductVendors.Select(x => x.VendorId));
                    var avIds = cartProductVendorIds.Intersect(availableVendors.Select(av => av.Id).ToList()).ToArray();
                    var sortedVendors = await _vendorService.GetVendorsByIdsAsync(avIds.Distinct().ToArray());
                    model.ProductVendors = await PrepareProductVendorsAsync(product, sortedVendors);
                    model.VendorSort = 40;
                }
            }

            //prepare variant + manufacturer combination for group product
            if (product.ProductType == ProductType.GroupedProduct)
            {
                //get manufacturer
                var productManufacturer = model?.ProductManufacturers?.FirstOrDefault();
                if (productManufacturer != null)
                    model.GroupedProductVariants = await PrepareGroupedProductVariantsModelsAsync(product, productManufacturer.Id);
            }

            return model;
        }

        /// <summary>
        /// Prepare the vendor product price model
        /// </summary>
        /// <param name="product">Product</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the product price model
        /// </returns>
        public virtual async Task<ProductDetailsModel.ProductPriceModel> PrepareVendorProductPriceModelAsync(Product product)
        {
            return await PrepareProductPriceModelAsync(product);
        }


        public async Task<IEnumerable<dynamic>> PreparepareSlotListModel(int slotId, string startDate, string endDate, int productId, int vendorId, bool isPickup = false)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
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
                    for (int j = startSlot; j < endSlot; j++)
                    {
                        var available = formatSlots.FirstOrDefault(x => x.Start.Equals(Weeks[i].Date.AddHours(j)));

                        var bookSlot = new BookingSlotModel();

                        if (available != null)
                        {
                            if (available.Start > Convert.ToDateTime(startDate) && await CheckSlotAvailability(available.Id, available.Start, available.End, available.Capacity, false))
                            {
                                bookSlot.Slot = ((Convert.ToInt32("0") + Convert.ToInt32(j)) + ":00" + "-" + (Convert.ToInt32(j) + Convert.ToInt32(1)) + ":00").ToString();
                                bookSlot.IsAvailable = true;
                                bookSlot.Start = available.Start;
                                bookSlot.End = available.End;
                                bookSlot.IsBook = false;
                                bookSlot.Price = await _priceFormatter.FormatPriceAsync(available.Price);
                                bookSlot.Id = available.Id;
                                bookSlot.BlockId = j;
                                bookSlot.Capacity = available.Capacity;
                                bookSlot.IsPickup = false;
                                resultModel.Add(bookSlot);
                            }
                            else
                            {
                                resultModel.Add(new BookingSlotModel { IsPickup = true, IsAvailable = false, Start = Weeks[i], Slot = ((Convert.ToInt32("0") + Convert.ToInt32(j)) + ":00" + "-" + (Convert.ToInt32(j) + Convert.ToInt32(1)) + ":00").ToString() });
                            }
                        }
                        else
                        {
                            resultModel.Add(new BookingSlotModel { IsPickup = true, Start = Weeks[i], Slot = ((Convert.ToInt32("0") + Convert.ToInt32(j)) + ":00" + "-" + (Convert.ToInt32(j) + Convert.ToInt32(1)) + ":00").ToString() });
                        }
                    }
                }


                //Selected insert into table
                IList<CustomerProductSlot> customerProductSlotList = new List<CustomerProductSlot>();
                //if (slotId == 0)
                //{
                customerProductSlotList = await _slotService.GetCustomerProductSlot(productId, customer.Id, Convert.ToDateTime(startDate), isPickup);
                //}
                //else
                //{
                //customerProductSlotList = await _slotService.GetCustomerProductSlotId(slotId, productId, customer.Id, Convert.ToDateTime(startDate), isPickup);
                //}
                if (customerProductSlotList.Count == 0)
                {
                    var bookSlot = resultModel.FirstOrDefault(x => x.IsAvailable == true);
                    if (bookSlot != null)
                    {
                        CustomerProductSlot customerProductSlot = new CustomerProductSlot();
                        bookSlot.IsBook = true;
                        bookSlot.IsSelected = false;
                        var slot = await _slotService.GetSlotById(bookSlot.Id);
                        customerProductSlot.SlotId = bookSlot.Id;
                        customerProductSlot.BlockId = bookSlot.BlockId;
                        customerProductSlot.StartTime = bookSlot.Start;
                        customerProductSlot.EndDateTime = bookSlot.End;
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
                    foreach (var customerSlot in customerProductSlotList)
                    {
                        var slotElement = resultModel.FirstOrDefault(x => x.Id == customerSlot.SlotId && x.Slot == customerSlot.EndTime);
                        if (slotElement != null)
                        {
                            if (customerSlot.IsSelected)
                            {
                                slotElement.IsBook = false;
                                slotElement.IsSelected = true;
                            }
                        }
                    }
                    var bookSlot = resultModel.FirstOrDefault(x => x.IsAvailable == true);
                    if (bookSlot != null)
                    {

                        if (!await _slotService.GetCustomerProductSlotByDate(productId, customer.Id, Convert.ToDateTime(bookSlot.Start), bookSlot.Slot, isPickup))
                        {
                            // await _slotService.DeleteCustomerProductSlot(productId, customer.Id);
                            CustomerProductSlot customerProductSlot = new CustomerProductSlot();
                            bookSlot.IsBook = true;
                            bookSlot.IsSelected = false;
                            var slot = await _slotService.GetSlotById(bookSlot.Id);
                            customerProductSlot.SlotId = bookSlot.Id;
                            customerProductSlot.BlockId = bookSlot.BlockId;
                            customerProductSlot.StartTime = bookSlot.Start;
                            customerProductSlot.EndDateTime = bookSlot.End;
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
            return resultModel.GroupBy(x => x.Slot);
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

        public async Task<IEnumerable<dynamic>> PrepareparePickupSlotListModel(int slotId, string startDate, string endDate, int productId, int vendorId, bool isPickup = true)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
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
                    for (int j = startSlot; j < endSlot; j++)
                    {
                        var available = formatSlots.FirstOrDefault(x => x.Start.Equals(Weeks[i].Date.AddHours(j)));

                        var bookSlot = new BookingSlotModel();

                        if (available != null)
                        {
                            if (available.Start > Convert.ToDateTime(startDate) && await CheckSlotAvailability(available.Id, available.Start, available.End, available.Capacity, false))
                            {
                                bookSlot.Slot = ((Convert.ToInt32("0") + Convert.ToInt32(j)) + ":00" + "-" + (Convert.ToInt32(j) + Convert.ToInt32(1)) + ":00").ToString();
                                bookSlot.IsAvailable = true;
                                bookSlot.Start = available.Start;
                                bookSlot.End = available.End;
                                bookSlot.IsBook = false;
                                bookSlot.Price = await _priceFormatter.FormatPriceAsync(available.Price);
                                bookSlot.Id = available.Id;
                                bookSlot.BlockId = j;
                                bookSlot.Capacity = available.Capacity;
                                bookSlot.IsPickup = true;
                                resultModel.Add(bookSlot);
                            }
                            else
                            {
                                resultModel.Add(new BookingSlotModel { IsPickup = true, IsAvailable = false, Start = Weeks[i], Slot = ((Convert.ToInt32("0") + Convert.ToInt32(j)) + ":00" + "-" + (Convert.ToInt32(j) + Convert.ToInt32(1)) + ":00").ToString() });
                            }
                        }
                        else
                        {
                            resultModel.Add(new BookingSlotModel { IsPickup = true, IsAvailable = false, Start = Weeks[i], Slot = ((Convert.ToInt32("0") + Convert.ToInt32(j)) + ":00" + "-" + (Convert.ToInt32(j) + Convert.ToInt32(1)) + ":00").ToString() });
                        }
                    }
                }


                //Selected insert into table
                IList<CustomerProductSlot> customerProductSlotList = new List<CustomerProductSlot>();
                //if (slotId == 0)
                //{
                customerProductSlotList = await _slotService.GetCustomerProductSlot(productId, customer.Id, Convert.ToDateTime(startDate), isPickup);
                //}
                //else
                //{
                //customerProductSlotList = await _slotService.GetCustomerProductSlotId(slotId, productId, customer.Id, Convert.ToDateTime(startDate), isPickup);
                //}
                if (customerProductSlotList.Count == 0)
                {
                    var bookSlot = resultModel.FirstOrDefault(x => x.IsAvailable == true);
                    if (bookSlot != null)
                    {
                        CustomerProductSlot customerProductSlot = new CustomerProductSlot();
                        bookSlot.IsBook = true;
                        bookSlot.IsSelected = false;
                        var slot = await _slotService.GetSlotById(bookSlot.Id);
                        customerProductSlot.SlotId = bookSlot.Id;
                        customerProductSlot.BlockId = bookSlot.BlockId;
                        customerProductSlot.StartTime = bookSlot.Start;
                        customerProductSlot.EndDateTime = bookSlot.End;
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
                    foreach (var customerSlot in customerProductSlotList)
                    {
                        var slotElement = resultModel.FirstOrDefault(x => x.Id == customerSlot.SlotId && x.Slot == customerSlot.EndTime);
                        if (slotElement != null)
                        {
                            if (customerSlot.IsSelected)
                            {
                                slotElement.IsBook = false;
                                slotElement.IsSelected = true;
                            }
                        }
                    }
                    var bookSlot = resultModel.FirstOrDefault(x => x.IsAvailable == true);
                    if (bookSlot != null)
                    {
                        if (!await _slotService.GetCustomerProductSlotByDate(productId, customer.Id, Convert.ToDateTime(bookSlot.Start), bookSlot.Slot, isPickup))
                        {
                            //await _slotService.DeleteCustomerProductSlot(productId, customer.Id);
                            CustomerProductSlot customerProductSlot = new CustomerProductSlot();
                            bookSlot.IsBook = true;
                            bookSlot.IsSelected = false;
                            var slot = await _slotService.GetSlotById(bookSlot.Id);
                            customerProductSlot.SlotId = bookSlot.Id;
                            customerProductSlot.BlockId = bookSlot.BlockId;
                            customerProductSlot.StartTime = bookSlot.Start;
                            customerProductSlot.EndDateTime = bookSlot.End;
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
            return resultModel.GroupBy(x => x.Slot);
        }

        public async Task<IEnumerable<dynamic>> PreparepareAdminSlotListModel(int slotId, string startDate, string endDate, int productId, int vendorId, bool isPickup = false)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            var productCategories = (await _categoryService.GetProductCategoriesByProductIdAsync(productId)).Select(x => x.CategoryId).ToList();
            var store = _storeContext.GetCurrentStore();
            IList<SlotModel> formatSlots = new List<SlotModel>();
            Zone zone = new Zone();
            zone = await _slotService.GetAdminCreateVendorZones(true, vendorId, false, createdBy: 1);
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
                    for (int j = startSlot; j < endSlot; j++)
                    {
                        var available = formatSlots.FirstOrDefault(x => x.Start.Equals(Weeks[i].Date.AddHours(j)));

                        var bookSlot = new BookingSlotModel();

                        if (available != null)
                        {
                            if (available.Start > Convert.ToDateTime(startDate) && await CheckSlotAvailability(available.Id, available.Start, available.End, available.Capacity, false))
                            {
                                bookSlot.Slot = ((Convert.ToInt32("0") + Convert.ToInt32(j)) + ":00" + "-" + (Convert.ToInt32(j) + Convert.ToInt32(1)) + ":00").ToString();
                                bookSlot.IsAvailable = true;
                                bookSlot.Start = available.Start;
                                bookSlot.End = available.End;
                                bookSlot.IsBook = false;
                                bookSlot.Price = await _priceFormatter.FormatPriceAsync(available.Price);
                                bookSlot.Id = available.Id;
                                bookSlot.BlockId = j;
                                bookSlot.Capacity = available.Capacity;
                                bookSlot.IsPickup = false;
                                resultModel.Add(bookSlot);
                            }
                            else
                            {
                                resultModel.Add(new BookingSlotModel { IsPickup = false, IsAvailable = false, Start = Weeks[i], Slot = ((Convert.ToInt32("0") + Convert.ToInt32(j)) + ":00" + "-" + (Convert.ToInt32(j) + Convert.ToInt32(1)) + ":00").ToString() });
                            }
                        }
                        else
                        {
                            resultModel.Add(new BookingSlotModel { IsPickup = false, Start = Weeks[i], Slot = ((Convert.ToInt32("0") + Convert.ToInt32(j)) + ":00" + "-" + (Convert.ToInt32(j) + Convert.ToInt32(1)) + ":00").ToString() });
                        }
                    }
                }


                //Selected insert into table
                IList<CustomerProductSlot> customerProductSlotList = new List<CustomerProductSlot>();
                //if (slotId == 0)
                //{
                customerProductSlotList = await _slotService.GetCustomerProductSlot(productId, customer.Id, Convert.ToDateTime(startDate), isPickup);
                //}
                //else
                //{
                //customerProductSlotList = await _slotService.GetCustomerProductSlotId(slotId, productId, customer.Id, Convert.ToDateTime(startDate), isPickup);
                //}
                if (customerProductSlotList.Count == 0)
                {
                    var bookSlot = resultModel.FirstOrDefault(x => x.IsAvailable == true);
                    if (bookSlot != null)
                    {
                        await _slotService.DeleteCustomerProductSlot(productId, customer.Id);
                        CustomerProductSlot customerProductSlot = new CustomerProductSlot();
                        bookSlot.IsBook = true;
                        bookSlot.IsSelected = false;
                        var slot = await _slotService.GetSlotById(bookSlot.Id);
                        customerProductSlot.SlotId = bookSlot.Id;
                        customerProductSlot.BlockId = bookSlot.BlockId;
                        customerProductSlot.StartTime = bookSlot.Start;
                        customerProductSlot.EndDateTime = bookSlot.End;
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
                    foreach (var customerSlot in customerProductSlotList)
                    {
                        var slotElement = resultModel.FirstOrDefault(x => x.Id == customerSlot.SlotId && x.Slot == customerSlot.EndTime);
                        if (slotElement != null)
                        {
                            if (customerSlot.IsSelected)
                            {
                                slotElement.IsBook = false;
                                slotElement.IsSelected = true;
                            }
                        }
                    }
                    var bookSlot = resultModel.FirstOrDefault(x => x.IsAvailable == true);
                    if (bookSlot != null)
                    {
                        if (!await _slotService.GetCustomerProductSlotByDate(productId, customer.Id, Convert.ToDateTime(bookSlot.Start), bookSlot.Slot, isPickup))
                        {
                            //await _slotService.DeleteCustomerProductSlot(productId, customer.Id);
                            CustomerProductSlot customerProductSlot = new CustomerProductSlot();
                            bookSlot.IsBook = true;
                            bookSlot.IsSelected = false;
                            var slot = await _slotService.GetSlotById(bookSlot.Id);
                            customerProductSlot.SlotId = bookSlot.Id;
                            customerProductSlot.BlockId = bookSlot.BlockId;
                            customerProductSlot.StartTime = bookSlot.Start;
                            customerProductSlot.EndDateTime = bookSlot.End;
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
            return resultModel.GroupBy(x => x.Slot);
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
                zone = await _slotService.GetVendorZones(true, vendorId, true); //cashing possible
            }
            if (isDelivery)
            {
                zone = await _slotService.GetVendorZones(true, vendorId, false, 0);
            }
            if (isAdmin)
            {
                zone = await _slotService.GetVendorZones(true, vendorId, false, 1);
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

        /// <summary>
        /// Prepare fastest slot string
        /// </summary>
        /// <param name="product"></param>
        /// <returns></returns>
        public virtual async Task<string> PrepareProductFastestSlotAsync(Product product, Customer customer)
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
                geoVendorIds: await _workContext.GetCurrentCustomerGeoRadiusVendorIdsAsync());
                if (associatedProducts != null && associatedProducts.Any())
                {
                    product = await _alchubGeneralService.GetGroupedProductDefaultVariantAsync(associatedProducts);
                }
            }

            var availableVendors = await _vendorService.GetAvailableGeoFenceVendorsAsync(customer, true, product);
            //prepare availbale vendors productss
            var productVendors = await PrepareProductVendorsAsync(product, availableVendors);
            var addDay = await _settingService.GetSettingByKeyAsync<int>("Alchub.Slot.Visible.After.Day", defaultValue: 1);
            var addHour = await _settingService.GetSettingByKeyAsync<int>("Alchub.Slot.Add.Hour", defaultValue: 0);
            var addMintues = await _settingService.GetSettingByKeyAsync<int>("Alchub.Slot.Add.Mintues", defaultValue: 0);
            var dateTime = await _dateTimeHelper.ConvertToUserTimeAsync(DateTime.Now);
            dateTime = dateTime.AddDays(addDay).AddHours(addHour).AddMinutes(addMintues);

            string slotTiming = string.Empty;
            if (productVendors.Count > 0)
            {
                //sort by time
                productVendors = productVendors.OrderBy(x => x?.StartTime).ThenBy(x => x.DistanceValue).ToList();
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
                        return slotTiming;
                    }
                    else
                    {
                        var slot = await PrepareTodaySlotListModel(0, dateTime.ToString(), dateTime.AddDays(7).ToString(), productVendors?.FirstOrDefault()?.ProductId ?? 0, productVendors?.FirstOrDefault()?.VendorId ?? 0, false, false, true);
                        if (slot != null)
                        {
                            var slotStartEndTime = $"{slot.Start.ToString("HH:mm")}-{slot.End.ToString("HH:mm")}";
                            slotTiming = $"{slot.Start.DayOfWeek.ToString()}, {SlotHelper.ConvertTo12hoursSlotTime(slotStartEndTime)}";
                        }
                        return slotTiming;
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
                    return slotTiming;
                }
            }

            return slotTiming;
        }

        /// <summary>
        /// prepare product details vendor sorting options.
        /// </summary>
        /// <returns></returns>
        public async Task<IList<SelectListItem>> PrepareProductDetailsVendorSortingOptions()
        {
            var exludeOptionIds = new List<int>() { Convert.ToInt32(VendorSort.Recommended) }.ToArray();
            //show Recommended only if there's cart has items
            var cart = await _shoppingCartService.GetShoppingCartAsync(
                                           await _workContext.GetCurrentCustomerAsync(),
                                           ShoppingCartType.ShoppingCart,
                                           (await _storeContext.GetCurrentStoreAsync()).Id);

            //prepare items from enum
            var options = (await VendorSort.Fastest.ToSelectListAsync(markCurrentAsSelected: false, valuesToExclude: !cart.Any() ? exludeOptionIds : null)).ToList();

            return options;
        }

        /// <summary>
        /// Prepare the product overview models
        /// </summary>
        /// <param name="products">Collection of products</param>
        /// <param name="preparePictureModel">Whether to prepare the picture model</param>
        /// <param name="productThumbPictureSize">Product thumb picture size (longest side); pass null to use the default value of media settings</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the collection of product overview model
        /// </returns>
        public virtual async Task<IEnumerable<ProductSearchOverviewModel>> PrepareProductSearchOverviewModelsAsync(IEnumerable<Product> products,
            bool preparePictureModel = true,
            int? productThumbPictureSize = null)
        {
            if (products == null)
                throw new ArgumentNullException(nameof(products));

            var models = new List<ProductSearchOverviewModel>();
            foreach (var product in products)
            {
                var model = new ProductSearchOverviewModel
                {
                    Id = product.Id,
                    Name = await _localizationService.GetLocalizedAsync(product, x => x.Name),
                    SeName = await _urlRecordService.GetSeNameAsync(product),
                };

                //picture
                if (preparePictureModel)
                {
                    model.DefaultPictureModel = await PrepareProductOverviewPictureModelAsync(product, productThumbPictureSize);
                }

                models.Add(model);
            }

            return models;
        }

        #endregion
    }
}
