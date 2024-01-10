using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.DeliveryFees;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Security;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Tax;
using Nop.Core.Domain.TipFees;
using Nop.Core.Domain.Vendors;
using Nop.Services.Alchub.General;
using Nop.Services.Alchub.ServiceFee;
using Nop.Services.Alchub.Slots;
using Nop.Services.Catalog;
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
using Nop.Web.Infrastructure.Cache;
using Nop.Web.Models.Media;
using Nop.Web.Models.ShoppingCart;
using Nop.Web.Models.Slots;

namespace Nop.Web.Factories
{
    public partial class ShoppingCartModelFactory : IShoppingCartModelFactory
    {
        #region Fields

        private readonly IServiceFeeManager _serviceFeeManager;
        private readonly AddressSettings _addressSettings;
        private readonly CaptchaSettings _captchaSettings;
        private readonly CatalogSettings _catalogSettings;
        private readonly CommonSettings _commonSettings;
        private readonly CustomerSettings _customerSettings;
        private readonly IAddressModelFactory _addressModelFactory;
        private readonly ICheckoutAttributeFormatter _checkoutAttributeFormatter;
        private readonly ICheckoutAttributeParser _checkoutAttributeParser;
        private readonly ICheckoutAttributeService _checkoutAttributeService;
        private readonly ICountryService _countryService;
        private readonly ICurrencyService _currencyService;
        private readonly ICustomerService _customerService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IDiscountService _discountService;
        private readonly IDownloadService _downloadService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IGiftCardService _giftCardService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILocalizationService _localizationService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly IPaymentPluginManager _paymentPluginManager;
        private readonly IPaymentService _paymentService;
        private readonly IPermissionService _permissionService;
        private readonly IPictureService _pictureService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IProductAttributeFormatter _productAttributeFormatter;
        private readonly IProductService _productService;
        private readonly IShippingService _shippingService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly IStaticCacheManager _staticCacheManager;
        private readonly IStoreContext _storeContext;
        private readonly IStoreMappingService _storeMappingService;
        private readonly ITaxService _taxService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IVendorService _vendorService;
        private readonly IWebHelper _webHelper;
        private readonly IWorkContext _workContext;
        private readonly MediaSettings _mediaSettings;
        private readonly OrderSettings _orderSettings;
        private readonly RewardPointsSettings _rewardPointsSettings;
        private readonly ShippingSettings _shippingSettings;
        private readonly ShoppingCartSettings _shoppingCartSettings;
        private readonly TaxSettings _taxSettings;
        private readonly VendorSettings _vendorSettings;
        private readonly ISettingService _settingService;
        private readonly IDeliveryFeeService _deliveryFeeService;
        private readonly ITipFeeService _tipFeeService;
        private readonly ISlotService _slotService;
        private readonly IAlchubGeneralService _alchubGeneralService;
        #endregion

        #region Ctor

        public ShoppingCartModelFactory(IServiceFeeManager serviceFeeManager,
            AddressSettings addressSettings,
            CaptchaSettings captchaSettings,
            CatalogSettings catalogSettings,
            CommonSettings commonSettings,
            CustomerSettings customerSettings,
            IAddressModelFactory addressModelFactory,
            ICheckoutAttributeFormatter checkoutAttributeFormatter,
            ICheckoutAttributeParser checkoutAttributeParser,
            ICheckoutAttributeService checkoutAttributeService,
            ICountryService countryService,
            ICurrencyService currencyService,
            ICustomerService customerService,
            IDateTimeHelper dateTimeHelper,
            IDiscountService discountService,
            IDownloadService downloadService,
            IGenericAttributeService genericAttributeService,
            IGiftCardService giftCardService,
            IHttpContextAccessor httpContextAccessor,
            ILocalizationService localizationService,
            IOrderProcessingService orderProcessingService,
            IOrderTotalCalculationService orderTotalCalculationService,
            IPaymentPluginManager paymentPluginManager,
            IPaymentService paymentService,
            IPermissionService permissionService,
            IPictureService pictureService,
            IPriceFormatter priceFormatter,
            IProductAttributeFormatter productAttributeFormatter,
            IProductService productService,
            IShippingService shippingService,
            IShoppingCartService shoppingCartService,
            IStateProvinceService stateProvinceService,
            IStaticCacheManager staticCacheManager,
            IStoreContext storeContext,
            IStoreMappingService storeMappingService,
            ITaxService taxService,
            IUrlRecordService urlRecordService,
            IVendorService vendorService,
            IWebHelper webHelper,
            IWorkContext workContext,
            MediaSettings mediaSettings,
            OrderSettings orderSettings,
            RewardPointsSettings rewardPointsSettings,
            ShippingSettings shippingSettings,
            ShoppingCartSettings shoppingCartSettings,
            TaxSettings taxSettings,
            VendorSettings vendorSettings,
            ISettingService settingService,
            IDeliveryFeeService deliveryFeeService,
            ITipFeeService tipFeeService,
            ISlotService slotService,
            IAlchubGeneralService alchubGeneralService)
        {
            _serviceFeeManager = serviceFeeManager;
            _addressSettings = addressSettings;
            _captchaSettings = captchaSettings;
            _catalogSettings = catalogSettings;
            _commonSettings = commonSettings;
            _customerSettings = customerSettings;
            _addressModelFactory = addressModelFactory;
            _checkoutAttributeFormatter = checkoutAttributeFormatter;
            _checkoutAttributeParser = checkoutAttributeParser;
            _checkoutAttributeService = checkoutAttributeService;
            _countryService = countryService;
            _currencyService = currencyService;
            _customerService = customerService;
            _dateTimeHelper = dateTimeHelper;
            _discountService = discountService;
            _downloadService = downloadService;
            _genericAttributeService = genericAttributeService;
            _giftCardService = giftCardService;
            _httpContextAccessor = httpContextAccessor;
            _localizationService = localizationService;
            _orderProcessingService = orderProcessingService;
            _orderTotalCalculationService = orderTotalCalculationService;
            _paymentPluginManager = paymentPluginManager;
            _paymentService = paymentService;
            _permissionService = permissionService;
            _pictureService = pictureService;
            _priceFormatter = priceFormatter;
            _productAttributeFormatter = productAttributeFormatter;
            _productService = productService;
            _shippingService = shippingService;
            _shoppingCartService = shoppingCartService;
            _stateProvinceService = stateProvinceService;
            _staticCacheManager = staticCacheManager;
            _storeContext = storeContext;
            _storeMappingService = storeMappingService;
            _taxService = taxService;
            _urlRecordService = urlRecordService;
            _vendorService = vendorService;
            _webHelper = webHelper;
            _workContext = workContext;
            _mediaSettings = mediaSettings;
            _orderSettings = orderSettings;
            _rewardPointsSettings = rewardPointsSettings;
            _shippingSettings = shippingSettings;
            _shoppingCartSettings = shoppingCartSettings;
            _taxSettings = taxSettings;
            _vendorSettings = vendorSettings;
            _settingService = settingService;
            _deliveryFeeService = deliveryFeeService;
            _tipFeeService = tipFeeService;
            _slotService = slotService;
            _alchubGeneralService = alchubGeneralService;
        }

        #endregion

        #region Utilities

        /// <returns>A task that represents the asynchronous operation</returns>
        protected virtual async Task PrepareProductPriceRangeAsync(Product product, WishlistModel.ShoppingCartItemModel wishlistItemModel)
        {
            //get price range
            var priceRangeDisc = await _alchubGeneralService.GetProductPriceRangeAsync(product);
            if (priceRangeDisc != null && priceRangeDisc.Any())
            {
                wishlistItemModel.UnitPrice = await _priceFormatter.FormatPriceAsync(priceRangeDisc.First().Value);
                wishlistItemModel.UnitPriceValue = priceRangeDisc.First().Value;
            }
        }

        /// <returns>A task that represents the asynchronous operation</returns>
        protected virtual async Task PrepareGroupedProductPriceRangeAsync(IList<Product> associatedProducts, WishlistModel.ShoppingCartItemModel wishlistItemModel)
        {
            if (!associatedProducts.Any())
                return;

            //groupproduct price = default variants-> sub products-> product with minimum price.
            var defaultAssociatedProduct = await _alchubGeneralService.GetGroupedProductDefaultVariantAsync(associatedProducts);

            if (defaultAssociatedProduct != null)
                await PrepareProductPriceRangeAsync(defaultAssociatedProduct, wishlistItemModel);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Prepare the shopping cart model
        /// </summary>
        /// <param name="model">Shopping cart model</param>
        /// <param name="cart">List of the shopping cart item</param>
        /// <param name="isEditable">Whether model is editable</param>
        /// <param name="validateCheckoutAttributes">Whether to validate checkout attributes</param>
        /// <param name="prepareAndDisplayOrderReviewData">Whether to prepare and display order review data</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the shopping cart model
        /// </returns>
        public virtual async Task<ShoppingCartModel> PrepareShoppingCartModelAsync(ShoppingCartModel model,
            IList<ShoppingCartItem> cart, bool isEditable = true,
            bool validateCheckoutAttributes = false,
            bool prepareAndDisplayOrderReviewData = false)
        {
            if (cart == null)
                throw new ArgumentNullException(nameof(cart));

            if (model == null)
                throw new ArgumentNullException(nameof(model));

            //simple properties
            model.OnePageCheckoutEnabled = _orderSettings.OnePageCheckoutEnabled;

            if (!cart.Any())
                return model;

            model.IsEditable = isEditable;
            model.ShowProductImages = _shoppingCartSettings.ShowProductImagesOnShoppingCart;
            model.ShowSku = _catalogSettings.ShowSkuOnProductDetailsPage;
            model.ShowVendorName = _vendorSettings.ShowVendorOnOrderDetailsPage;
            var customer = await _workContext.GetCurrentCustomerAsync();
            var store = await _storeContext.GetCurrentStoreAsync();
            var checkoutAttributesXml = await _genericAttributeService.GetAttributeAsync<string>(customer,
                NopCustomerDefaults.CheckoutAttributes, store.Id);
            var minOrderSubtotalAmountOk = await _orderProcessingService.ValidateMinOrderSubtotalAmountAsync(cart);
            if (!minOrderSubtotalAmountOk)
            {
                var minOrderSubtotalAmount = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(_orderSettings.MinOrderSubtotalAmount, await _workContext.GetWorkingCurrencyAsync());
                model.MinOrderSubtotalWarning = string.Format(await _localizationService.GetResourceAsync("Checkout.MinOrderSubtotalAmount"), await _priceFormatter.FormatPriceAsync(minOrderSubtotalAmount, true, false));
            }

            model.TermsOfServiceOnShoppingCartPage = _orderSettings.TermsOfServiceOnShoppingCartPage;
            model.TermsOfServiceOnOrderConfirmPage = _orderSettings.TermsOfServiceOnOrderConfirmPage;
            model.TermsOfServicePopup = _commonSettings.PopupForTermsOfServiceLinks;
            model.DisplayTaxShippingInfo = _catalogSettings.DisplayTaxShippingInfoShoppingCart;

            //discount and gift card boxes
            model.DiscountBox.Display = _shoppingCartSettings.ShowDiscountBox;
            var discountCouponCodes = await _customerService.ParseAppliedDiscountCouponCodesAsync(customer);
            foreach (var couponCode in discountCouponCodes)
            {
                var discount = await (await _discountService.GetAllDiscountsAsync(couponCode: couponCode))
                    .FirstOrDefaultAwaitAsync(async d => d.RequiresCouponCode && (await _discountService.ValidateDiscountAsync(d, customer)).IsValid);

                if (discount != null)
                {
                    model.DiscountBox.AppliedDiscountsWithCodes.Add(new ShoppingCartModel.DiscountBoxModel.DiscountInfoModel
                    {
                        Id = discount.Id,
                        CouponCode = discount.CouponCode
                    });
                }
            }

            model.GiftCardBox.Display = _shoppingCartSettings.ShowGiftCardBox;

            //cart warnings
            var cartWarnings = await _shoppingCartService.GetShoppingCartWarningsAsync(cart, checkoutAttributesXml, validateCheckoutAttributes);
            foreach (var warning in cartWarnings)
                model.Warnings.Add(warning);

            //checkout attributes
            model.CheckoutAttributes = await PrepareCheckoutAttributeModelsAsync(cart);

            /*Alchub Start*/
            var vendors = await _vendorService.GetVendorsByProductIdsAsync(cart.Select(x => x.ProductId).ToArray());

            if (vendors != null)
            {
                foreach (var vendor in vendors)
                {
                    model.ShoppingCartVendors.Add(new ShoppingCartModel.ShoppingCartVendorModel
                    {
                        Id = vendor.Id,
                        Name = string.Format(await _localizationService.GetResourceAsync("Alchub.Vendor.MinimumOrderSubtotal.Vendor.Name"), vendor.Name),
                        Warnings = await _shoppingCartService.GetVendorMinimumOrderAmountWarningsAsync(cart, vendor)
                    });
                }

                model.ShoppingCartVendors = model.ShoppingCartVendors?.OrderBy(x => x.Name).ToList();
            }

            //Add for all items which have no vendor or whose vendor is not active/deleted
            model.ShoppingCartVendors.Add(new ShoppingCartModel.ShoppingCartVendorModel
            {
                Id = 0,
                Name = string.Format(await _localizationService.GetResourceAsync("Alchub.Vendor.MinimumOrderSubtotal.Vendor.Name"),
                        await _localizationService.GetResourceAsync("Alchub.TipFee.Admin")),
                Warnings = null
            });

            /*Alchub End*/

            //cart items
            foreach (var sci in cart)
            {
                var cartItemModel = await PrepareShoppingCartItemModelAsync(cart, sci);
                model.Items.Add(cartItemModel);
            }

            //payment methods
            //all payment methods (do not filter by country here as it could be not specified yet)
            var paymentMethods = await (await _paymentPluginManager
                .LoadActivePluginsAsync(customer, store.Id))
                .WhereAwait(async pm => !await pm.HidePaymentMethodAsync(cart)).ToListAsync();
            //payment methods displayed during checkout (not with "Button" type)
            var nonButtonPaymentMethods = paymentMethods
                .Where(pm => pm.PaymentMethodType != PaymentMethodType.Button)
                .ToList();
            //"button" payment methods(*displayed on the shopping cart page)
            var buttonPaymentMethods = paymentMethods
                .Where(pm => pm.PaymentMethodType == PaymentMethodType.Button)
                .ToList();
            foreach (var pm in buttonPaymentMethods)
            {
                if (await _shoppingCartService.ShoppingCartIsRecurringAsync(cart) && pm.RecurringPaymentType == RecurringPaymentType.NotSupported)
                    continue;

                var viewComponentName = pm.GetPublicViewComponentName();
                model.ButtonPaymentMethodViewComponentNames.Add(viewComponentName);
            }
            //hide "Checkout" button if we have only "Button" payment methods
            model.HideCheckoutButton = !nonButtonPaymentMethods.Any() && model.ButtonPaymentMethodViewComponentNames.Any();

            //order review data
            if (prepareAndDisplayOrderReviewData)
            {
                model.OrderReviewData = await PrepareOrderReviewDataModelAsync(cart);
            }

            return model;
        }

        /// <summary>
        /// Prepare the mini shopping cart model
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the mini shopping cart model
        /// </returns>
        public virtual async Task<MiniShoppingCartModel> PrepareMiniShoppingCartModelAsync()
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            var model = new MiniShoppingCartModel
            {
                ShowProductImages = _shoppingCartSettings.ShowProductImagesInMiniShoppingCart,
                //let's always display it
                DisplayShoppingCartButton = true,
                CurrentCustomerIsGuest = await _customerService.IsGuestAsync(customer),
                AnonymousCheckoutAllowed = _orderSettings.AnonymousCheckoutAllowed,
            };

            //performance optimization (use "HasShoppingCartItems" property)
            if (customer.HasShoppingCartItems)
            {
                var store = await _storeContext.GetCurrentStoreAsync();
                var cart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, store.Id);

                if (cart.Any())
                {
                    model.TotalProducts = cart.Sum(item => item.Quantity);

                    //subtotal
                    var subTotalIncludingTax = await _workContext.GetTaxDisplayTypeAsync() == TaxDisplayType.IncludingTax && !_taxSettings.ForceTaxExclusionFromOrderSubtotal;
                    var (_, _, _, subTotalWithoutDiscountBase, _) = await _orderTotalCalculationService.GetShoppingCartSubTotalAsync(cart, subTotalIncludingTax);
                    var subtotalBase = subTotalWithoutDiscountBase;
                    var currentCurrency = await _workContext.GetWorkingCurrencyAsync();
                    var subtotal = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(subtotalBase, currentCurrency);
                    model.SubTotal = await _priceFormatter.FormatPriceAsync(subtotal, false, currentCurrency, (await _workContext.GetWorkingLanguageAsync()).Id, subTotalIncludingTax);
                    model.SubTotalValue = subtotal;

                    var requiresShipping = await _shoppingCartService.ShoppingCartRequiresShippingAsync(cart);
                    //a customer should visit the shopping cart page (hide checkout button) before going to checkout if:
                    //1. "terms of service" are enabled
                    //2. min order sub-total is OK
                    //3. we have at least one checkout attribute
                    var checkoutAttributesExist = (await _checkoutAttributeService
                        .GetAllCheckoutAttributesAsync(store.Id, !requiresShipping))
                        .Any();

                    var minOrderSubtotalAmountOk = await _orderProcessingService.ValidateMinOrderSubtotalAmountAsync(cart);

                    var cartProductIds = cart.Select(ci => ci.ProductId).ToArray();

                    var downloadableProductsRequireRegistration =
                        _customerSettings.RequireRegistrationForDownloadableProducts && await _productService.HasAnyDownloadableProductAsync(cartProductIds);

                    model.DisplayCheckoutButton = !_orderSettings.TermsOfServiceOnShoppingCartPage &&
                        minOrderSubtotalAmountOk &&
                        !checkoutAttributesExist &&
                        !(downloadableProductsRequireRegistration
                            && await _customerService.IsGuestAsync(customer));

                    //products. sort descending (recently added products)
                    foreach (var sci in cart
                        .OrderByDescending(x => x.Id)
                        .Take(_shoppingCartSettings.MiniShoppingCartProductNumber)
                        .ToList())
                    {
                        var product = await _productService.GetProductByIdAsync(sci.ProductId);

                        var cartItemModel = new MiniShoppingCartModel.ShoppingCartItemModel
                        {
                            Id = sci.Id,
                            ProductId = sci.ProductId,
                            ProductName = await _productService.GetProductItemName(product, sci), //++Alchub
                            ProductSeName = await _urlRecordService.GetSeNameAsync(product),
                            Quantity = sci.Quantity,
                            AttributeInfo = await _productAttributeFormatter.FormatAttributesAsync(product, sci.AttributesXml),
                            CustomAttributeInfo = await _productAttributeFormatter.FormatCustomAttributesAsync(sci.CustomAttributesXml)
                        };

                        //unit prices
                        if (product.CallForPrice &&
                            //also check whether the current user is impersonated
                            (!_orderSettings.AllowAdminsToBuyCallForPriceProducts || _workContext.OriginalCustomerIfImpersonated == null))
                        {
                            cartItemModel.UnitPrice = await _localizationService.GetResourceAsync("Products.CallForPrice");
                            cartItemModel.UnitPriceValue = 0;
                        }
                        else
                        {
                            var (shoppingCartUnitPriceWithDiscountBase, _) = await _taxService.GetProductPriceAsync(product, (await _shoppingCartService.GetUnitPriceAsync(sci, true)).unitPrice);
                            var shoppingCartUnitPriceWithDiscount = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(shoppingCartUnitPriceWithDiscountBase, currentCurrency);
                            cartItemModel.UnitPrice = await _priceFormatter.FormatPriceAsync(shoppingCartUnitPriceWithDiscount);
                            cartItemModel.UnitPriceValue = shoppingCartUnitPriceWithDiscount;
                        }

                        //picture
                        if (_shoppingCartSettings.ShowProductImagesInMiniShoppingCart)
                        {
                            cartItemModel.Picture = await PrepareCartItemPictureModelAsync(sci,
                                _mediaSettings.MiniCartThumbPictureSize, true, cartItemModel.ProductName);
                        }

                        model.Items.Add(cartItemModel);
                    }
                }
            }

            return model;
        }

        /// <summary>
        /// Prepare the order totals model
        /// </summary>
        /// <param name="cart">List of the shopping cart item</param>
        /// <param name="isEditable">Whether model is editable</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the order totals model
        /// </returns>
        public virtual async Task<OrderTotalsModel> PrepareOrderTotalsModelAsync(IList<ShoppingCartItem> cart, bool isEditable)
        {
            var model = new OrderTotalsModel
            {
                IsEditable = isEditable
            };

            if (cart.Any())
            {
                //subtotal
                var subTotalIncludingTax = await _workContext.GetTaxDisplayTypeAsync() == TaxDisplayType.IncludingTax && !_taxSettings.ForceTaxExclusionFromOrderSubtotal;
                var (orderSubTotalDiscountAmountBase, _, subTotalWithoutDiscountBase, _, _) = await _orderTotalCalculationService.GetShoppingCartSubTotalAsync(cart, subTotalIncludingTax);
                var subtotalBase = subTotalWithoutDiscountBase;
                var currentCurrency = await _workContext.GetWorkingCurrencyAsync();
                var subtotal = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(subtotalBase, currentCurrency);
                var currentLanguage = await _workContext.GetWorkingLanguageAsync();
                model.SubTotal = await _priceFormatter.FormatPriceAsync(subtotal, true, currentCurrency, currentLanguage.Id, subTotalIncludingTax);

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

                //do not show if 0
                if (deliveryFee > 0)
                    model.DeliveryFee = await _priceFormatter.FormatShippingPriceAsync(deliveryFee, true);

                if (vendorWiseDeliveryFees != null)
                {
                    vendorWiseDeliveryFees.ToList().ForEach(async x =>
                    {
                        var deliveryFee = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(x.DeliveryFeeValue, currentCurrency);

                        model.VendorWiseDeliveryFees.Add(
                        new VendorWiseDeliveryFee
                        {
                            VendorId = x.VendorId,
                            VendorName = x.VendorName,
                            DeliveryFeeValue = deliveryFee,
                            DeliveryFee = await _priceFormatter.FormatShippingPriceAsync(deliveryFee, true)
                        });
                    });
                }

                //Tip fee
                var vendorWiseTipFees = await _tipFeeService.GetVendorWiseTipFeeAsync(cart, subtotal);

                decimal tipFee = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync
                    (vendorWiseTipFees?.Sum(x => x.TipFeeValue) ?? decimal.Zero, currentCurrency);

                model.TipFee = await _priceFormatter.FormatShippingPriceAsync(tipFee, true);

                if (vendorWiseTipFees != null)
                {
                    vendorWiseTipFees.ToList().ForEach(async x =>
                    {
                        var tipFee = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(x.TipFeeValue, currentCurrency);

                        model.VendorWiseTipFees.Add(
                        new VendorWiseTipFee
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

                model.SlotFeesList = await PrepareSlotListAsync(cart);
                /*Alchub End*/

                //shipping info
                model.RequiresShipping = await _shoppingCartService.ShoppingCartRequiresShippingAsync(cart);
                var customer = await _workContext.GetCurrentCustomerAsync();
                var store = await _storeContext.GetCurrentStoreAsync();
                if (model.RequiresShipping)
                {
                    var shoppingCartShippingBase = await _orderTotalCalculationService.GetShoppingCartShippingTotalAsync(cart);
                    if (shoppingCartShippingBase.HasValue)
                    {
                        var shoppingCartShipping = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(shoppingCartShippingBase.Value, currentCurrency);
                        model.Shipping = await _priceFormatter.FormatShippingPriceAsync(shoppingCartShipping, true);

                        //selected shipping method
                        var shippingOption = await _genericAttributeService.GetAttributeAsync<ShippingOption>(customer,
                            NopCustomerDefaults.SelectedShippingOptionAttribute, store.Id);
                        if (shippingOption != null)
                            model.SelectedShippingMethod = shippingOption.Name;
                    }
                }
                else
                {
                    model.HideShippingTotal = _shippingSettings.HideShippingTotal;
                }

                //payment method fee
                var paymentMethodSystemName = await _genericAttributeService.GetAttributeAsync<string>(customer, NopCustomerDefaults.SelectedPaymentMethodAttribute, store.Id);
                var paymentMethodAdditionalFee = await _paymentService.GetAdditionalHandlingFeeAsync(cart, paymentMethodSystemName);
                var (paymentMethodAdditionalFeeWithTaxBase, _) = await _taxService.GetPaymentMethodAdditionalFeeAsync(paymentMethodAdditionalFee, customer);
                if (paymentMethodAdditionalFeeWithTaxBase > decimal.Zero)
                {
                    var paymentMethodAdditionalFeeWithTax = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(paymentMethodAdditionalFeeWithTaxBase, currentCurrency);
                    model.PaymentMethodAdditionalFee = await _priceFormatter.FormatPaymentMethodAdditionalFeeAsync(paymentMethodAdditionalFeeWithTax, true);
                }

                //tax
                bool displayTax;
                bool displayTaxRates;
                if (_taxSettings.HideTaxInOrderSummary && await _workContext.GetTaxDisplayTypeAsync() == TaxDisplayType.IncludingTax)
                {
                    displayTax = false;
                    displayTaxRates = false;
                }
                else
                {
                    var (shoppingCartTaxBase, taxRates) = await _orderTotalCalculationService.GetTaxTotalAsync(cart);
                    var shoppingCartTax = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(shoppingCartTaxBase, currentCurrency);

                    if (shoppingCartTaxBase == 0 && _taxSettings.HideZeroTax)
                    {
                        displayTax = false;
                        displayTaxRates = false;
                    }
                    else
                    {
                        displayTaxRates = _taxSettings.DisplayTaxRates && taxRates.Any();
                        displayTax = !displayTaxRates;

                        model.Tax = await _priceFormatter.FormatPriceAsync(shoppingCartTax, true, false);
                        foreach (var tr in taxRates)
                        {
                            model.TaxRates.Add(new OrderTotalsModel.TaxRate
                            {
                                Rate = _priceFormatter.FormatTaxRate(tr.Key),
                                Value = await _priceFormatter.FormatPriceAsync(await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(tr.Value, currentCurrency), true, false),
                            });
                        }
                    }
                }

                model.DisplayTaxRates = displayTaxRates;
                model.DisplayTax = displayTax;

                //service fee
                var serviceFee = await _serviceFeeManager.GetServiceFeeAsync(subtotal);
                //do not show if 0
                if (serviceFee > 0)
                    model.ServiceFee = await _priceFormatter.FormatPriceAsync(serviceFee, true, false);

                //slot fee
                var slotFee = await PrepareSlotTotalAsync(cart);
                //do not show if 0
                if (slotFee > 0)
                    model.SlotFee = await _priceFormatter.FormatPriceAsync(slotFee);


                //total
                var (shoppingCartTotalBase, orderTotalDiscountAmountBase, _, appliedGiftCards, redeemedRewardPoints, redeemedRewardPointsAmount) = await _orderTotalCalculationService.GetShoppingCartTotalAsync(cart);
                if (shoppingCartTotalBase.HasValue)
                {
                    var shoppingCartTotal = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(shoppingCartTotalBase.Value, currentCurrency);
                    model.OrderTotal = await _priceFormatter.FormatPriceAsync(shoppingCartTotal, true, false);
                }

                //discount
                if (orderTotalDiscountAmountBase > decimal.Zero)
                {
                    var orderTotalDiscountAmount = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(orderTotalDiscountAmountBase, currentCurrency);
                    model.OrderTotalDiscount = await _priceFormatter.FormatPriceAsync(-orderTotalDiscountAmount, true, false);
                }

                //gift cards
                if (appliedGiftCards != null && appliedGiftCards.Any())
                {
                    foreach (var appliedGiftCard in appliedGiftCards)
                    {
                        var gcModel = new OrderTotalsModel.GiftCard
                        {
                            Id = appliedGiftCard.GiftCard.Id,
                            CouponCode = appliedGiftCard.GiftCard.GiftCardCouponCode,
                        };
                        var amountCanBeUsed = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(appliedGiftCard.AmountCanBeUsed, currentCurrency);
                        gcModel.Amount = await _priceFormatter.FormatPriceAsync(-amountCanBeUsed, true, false);

                        var remainingAmountBase = await _giftCardService.GetGiftCardRemainingAmountAsync(appliedGiftCard.GiftCard) - appliedGiftCard.AmountCanBeUsed;
                        var remainingAmount = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(remainingAmountBase, currentCurrency);
                        gcModel.Remaining = await _priceFormatter.FormatPriceAsync(remainingAmount, true, false);

                        model.GiftCards.Add(gcModel);
                    }
                }

                //reward points to be spent (redeemed)
                if (redeemedRewardPointsAmount > decimal.Zero)
                {
                    var redeemedRewardPointsAmountInCustomerCurrency = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(redeemedRewardPointsAmount, currentCurrency);
                    model.RedeemedRewardPoints = redeemedRewardPoints;
                    model.RedeemedRewardPointsAmount = await _priceFormatter.FormatPriceAsync(-redeemedRewardPointsAmountInCustomerCurrency, true, false);
                }

                //reward points to be earned
                if (_rewardPointsSettings.Enabled && _rewardPointsSettings.DisplayHowMuchWillBeEarned && shoppingCartTotalBase.HasValue)
                {
                    //get shipping total
                    var shippingBaseInclTax = !model.RequiresShipping ? 0 : (await _orderTotalCalculationService.GetShoppingCartShippingTotalAsync(cart, true)).shippingTotal ?? 0;

                    //get total for reward points
                    var totalForRewardPoints = _orderTotalCalculationService
                        .CalculateApplicableOrderTotalForRewardPoints(shippingBaseInclTax, shoppingCartTotalBase.Value);
                    if (totalForRewardPoints > decimal.Zero)
                        model.WillEarnRewardPoints = await _orderTotalCalculationService.CalculateRewardPointsAsync(customer, totalForRewardPoints);
                }
            }

            return model;
        }

        protected virtual async Task<ShoppingCartModel.ShoppingCartItemModel> PrepareShoppingCartItemModelAsync(IList<ShoppingCartItem> cart, ShoppingCartItem sci)
        {
            if (cart == null)
                throw new ArgumentNullException(nameof(cart));

            if (sci == null)
                throw new ArgumentNullException(nameof(sci));

            var product = await _productService.GetProductByIdAsync(sci.ProductId);
            var masterProduct = sci.GroupedProductId > 0 ? await _productService.GetProductByIdAsync(sci.GroupedProductId) :
                                                                 await _productService.GetProductByIdAsync(sci.MasterProductId);
            if (masterProduct == null)
                masterProduct = product;

            var cartItemModel = new ShoppingCartModel.ShoppingCartItemModel
            {
                Id = sci.Id,
                Sku = await _productService.FormatSkuAsync(product, sci.AttributesXml),
                VendorName = _vendorSettings.ShowVendorOnOrderDetailsPage ? (await _vendorService.GetVendorByProductIdAsync(product.Id))?.Name : string.Empty,
                ProductId = sci.ProductId,
                ProductName = await _productService.GetProductItemName(product, sci), //++Alchub
                ProductSeName = await _urlRecordService.GetSeNameAsync(masterProduct), //++Alchub
                Quantity = sci.Quantity,
                AttributeInfo = await _productAttributeFormatter.FormatAttributesAsync(product, sci.AttributesXml),
                /*Alchub Start*/
                VendorId = product.VendorId,
                CustomAttributeInfo = await _productAttributeFormatter.FormatCustomAttributesAsync(sci.CustomAttributesXml)
                /*Alchub End*/
            };

            //allow editing?
            //1. setting enabled?
            //2. simple product?
            //3. has attribute or gift card?
            //4. visible individually?
            cartItemModel.AllowItemEditing = _shoppingCartSettings.AllowCartItemEditing &&
                                             product.ProductType == ProductType.SimpleProduct &&
                                             (!string.IsNullOrEmpty(cartItemModel.AttributeInfo) ||
                                              product.IsGiftCard) &&
                                             product.VisibleIndividually;

            //disable removal?
            //1. do other items require this one?
            cartItemModel.DisableRemoval = (await _shoppingCartService.GetProductsRequiringProductAsync(cart, product)).Any();

            //allowed quantities
            var allowedQuantities = _productService.ParseAllowedQuantities(product);
            foreach (var qty in allowedQuantities)
            {
                cartItemModel.AllowedQuantities.Add(new SelectListItem
                {
                    Text = qty.ToString(),
                    Value = qty.ToString(),
                    Selected = sci.Quantity == qty
                });
            }

            //recurring info
            if (product.IsRecurring)
                cartItemModel.RecurringInfo = string.Format(await _localizationService.GetResourceAsync("ShoppingCart.RecurringPeriod"),
                        product.RecurringCycleLength, await _localizationService.GetLocalizedEnumAsync(product.RecurringCyclePeriod));

            //rental info
            if (product.IsRental)
            {
                var rentalStartDate = sci.RentalStartDateUtc.HasValue
                    ? _productService.FormatRentalDate(product, sci.RentalStartDateUtc.Value)
                    : string.Empty;
                var rentalEndDate = sci.RentalEndDateUtc.HasValue
                    ? _productService.FormatRentalDate(product, sci.RentalEndDateUtc.Value)
                    : string.Empty;
                cartItemModel.RentalInfo =
                    string.Format(await _localizationService.GetResourceAsync("ShoppingCart.Rental.FormattedDate"),
                        rentalStartDate, rentalEndDate);
            }

            //unit prices
            var currentCurrency = await _workContext.GetWorkingCurrencyAsync();
            if (product.CallForPrice &&
                //also check whether the current user is impersonated
                (!_orderSettings.AllowAdminsToBuyCallForPriceProducts || _workContext.OriginalCustomerIfImpersonated == null))
            {
                cartItemModel.UnitPrice = await _localizationService.GetResourceAsync("Products.CallForPrice");
                cartItemModel.UnitPriceValue = 0;
            }
            else
            {
                var (shoppingCartUnitPriceWithDiscountBase, _) = await _taxService.GetProductPriceAsync(product, (await _shoppingCartService.GetUnitPriceAsync(sci, true)).unitPrice);
                var shoppingCartUnitPriceWithDiscount = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(shoppingCartUnitPriceWithDiscountBase, currentCurrency);
                cartItemModel.UnitPrice = await _priceFormatter.FormatPriceAsync(shoppingCartUnitPriceWithDiscount);
                cartItemModel.UnitPriceValue = shoppingCartUnitPriceWithDiscount;
            }
            //subtotal, discount
            if (product.CallForPrice &&
                //also check whether the current user is impersonated
                (!_orderSettings.AllowAdminsToBuyCallForPriceProducts || _workContext.OriginalCustomerIfImpersonated == null))
            {
                cartItemModel.SubTotal = await _localizationService.GetResourceAsync("Products.CallForPrice");
                cartItemModel.SubTotalValue = 0;
            }
            else
            {
                //sub total
                var (subTotal, shoppingCartItemDiscountBase, _, maximumDiscountQty) = await _shoppingCartService.GetSubTotalAsync(sci, true);
                var (shoppingCartItemSubTotalWithDiscountBase, _) = await _taxService.GetProductPriceAsync(product, subTotal);
                var shoppingCartItemSubTotalWithDiscount = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(shoppingCartItemSubTotalWithDiscountBase, currentCurrency);
                cartItemModel.SubTotal = await _priceFormatter.FormatPriceAsync(shoppingCartItemSubTotalWithDiscount);
                cartItemModel.SubTotalValue = shoppingCartItemSubTotalWithDiscount;
                cartItemModel.MaximumDiscountedQty = maximumDiscountQty;

                //display an applied discount amount
                if (shoppingCartItemDiscountBase > decimal.Zero)
                {
                    (shoppingCartItemDiscountBase, _) = await _taxService.GetProductPriceAsync(product, shoppingCartItemDiscountBase);
                    if (shoppingCartItemDiscountBase > decimal.Zero)
                    {
                        var shoppingCartItemDiscount = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(shoppingCartItemDiscountBase, currentCurrency);
                        cartItemModel.Discount = await _priceFormatter.FormatPriceAsync(shoppingCartItemDiscount);
                        cartItemModel.DiscountValue = shoppingCartItemDiscount;
                    }
                }
            }

            //picture
            if (_shoppingCartSettings.ShowProductImagesOnShoppingCart)
            {
                cartItemModel.Picture = await PrepareCartItemPictureModelAsync(sci,
                    _mediaSettings.CartThumbPictureSize, true, cartItemModel.ProductName);
            }

            //Slot
            cartItemModel.SlotPrice = sci.SlotPrice != null ? await _priceFormatter.FormatPriceAsync(sci.SlotPrice) : "";

            if (sci.SlotStartTime != null)
            {
                if (sci.SlotStartTime.Date == DateTime.UtcNow.Date)
                    cartItemModel.SlotStartDate = "Today";
                else if (sci.SlotStartTime.Date == DateTime.UtcNow.AddDays(1).Date)
                    cartItemModel.SlotStartDate = "Tomorrow";
                else
                    cartItemModel.SlotStartDate = sci.SlotStartTime.ToString("dd MMMM");
            }
            else
                cartItemModel.SlotStartDate = string.Empty;

            //convert slot time to 12hours time (11-01-23)
            cartItemModel.SlotTime = sci.SlotTime != null ? SlotHelper.ConvertTo12hoursSlotTime(sci.SlotTime) : "";
            cartItemModel.IsPickup = sci.IsPickup;

            //item warnings
            var itemWarnings = await _shoppingCartService.GetShoppingCartItemWarningsAsync(
                await _workContext.GetCurrentCustomerAsync(),
                sci.ShoppingCartType,
                product,
                sci.StoreId,
                sci.AttributesXml,
                sci.CustomerEnteredPrice,
                sci.RentalStartDateUtc,
                sci.RentalEndDateUtc,
                sci.Quantity,
                false,
                sci.Id);
            foreach (var warning in itemWarnings)
                cartItemModel.Warnings.Add(warning);

            return cartItemModel;
        }

        /// <summary>
        /// Prepare the wishlist item model
        /// </summary>
        /// <param name="sci">Shopping cart item</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the shopping cart item model
        /// </returns>
        protected virtual async Task<WishlistModel.ShoppingCartItemModel> PrepareWishlistItemModelAsync(ShoppingCartItem sci)
        {
            if (sci == null)
                throw new ArgumentNullException(nameof(sci));

            var product = await _productService.GetProductByIdAsync(sci.ProductId);
            var masterProduct = sci.GroupedProductId > 0 ? await _productService.GetProductByIdAsync(sci.GroupedProductId) :
                                                                 await _productService.GetProductByIdAsync(sci.MasterProductId);
            if (masterProduct == null)
                masterProduct = product;

            var cartItemModel = new WishlistModel.ShoppingCartItemModel
            {
                Id = sci.Id,
                Sku = await _productService.FormatSkuAsync(product, sci.AttributesXml),
                ProductId = product.Id,
                ProductName = await _productService.GetProductItemName(product, sci), //++Alchub
                ProductSeName = await _urlRecordService.GetSeNameAsync(masterProduct), //++Alchub
                Quantity = sci.Quantity,
                AttributeInfo = await _productAttributeFormatter.FormatAttributesAsync(product, sci.AttributesXml),
                /*Alchub Start*/
                VendorId = product.VendorId,
                CustomAttributeInfo = await _productAttributeFormatter.FormatCustomAttributesAsync(sci.CustomAttributesXml),
                Size = product.Size
                /*Alchub End*/
            };

            //allow editing?
            //1. setting enabled?
            //2. simple product?
            //3. has attribute or gift card?
            //4. visible individually?
            cartItemModel.AllowItemEditing = _shoppingCartSettings.AllowCartItemEditing &&
                                             product.ProductType == ProductType.SimpleProduct &&
                                             (!string.IsNullOrEmpty(cartItemModel.AttributeInfo) ||
                                              product.IsGiftCard) &&
                                             product.VisibleIndividually;

            //allowed quantities
            var allowedQuantities = _productService.ParseAllowedQuantities(product);
            foreach (var qty in allowedQuantities)
            {
                cartItemModel.AllowedQuantities.Add(new SelectListItem
                {
                    Text = qty.ToString(),
                    Value = qty.ToString(),
                    Selected = sci.Quantity == qty
                });
            }

            //recurring info
            if (product.IsRecurring)
                cartItemModel.RecurringInfo = string.Format(await _localizationService.GetResourceAsync("ShoppingCart.RecurringPeriod"),
                        product.RecurringCycleLength, await _localizationService.GetLocalizedEnumAsync(product.RecurringCyclePeriod));

            //rental info
            if (product.IsRental)
            {
                var rentalStartDate = sci.RentalStartDateUtc.HasValue
                    ? _productService.FormatRentalDate(product, sci.RentalStartDateUtc.Value)
                    : string.Empty;
                var rentalEndDate = sci.RentalEndDateUtc.HasValue
                    ? _productService.FormatRentalDate(product, sci.RentalEndDateUtc.Value)
                    : string.Empty;
                cartItemModel.RentalInfo =
                    string.Format(await _localizationService.GetResourceAsync("ShoppingCart.Rental.FormattedDate"),
                        rentalStartDate, rentalEndDate);
            }

            //unit prices
            var currentCurrency = await _workContext.GetWorkingCurrencyAsync();
            if (product.CallForPrice &&
                //also check whether the current user is impersonated
                (!_orderSettings.AllowAdminsToBuyCallForPriceProducts || _workContext.OriginalCustomerIfImpersonated == null))
            {
                cartItemModel.UnitPrice = await _localizationService.GetResourceAsync("Products.CallForPrice");
                cartItemModel.UnitPriceValue = 0;
            }
            else
            {
                var (shoppingCartUnitPriceWithDiscountBase, _) = await _taxService.GetProductPriceAsync(product, (await _shoppingCartService.GetUnitPriceAsync(sci, true)).unitPrice);
                var shoppingCartUnitPriceWithDiscount = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(shoppingCartUnitPriceWithDiscountBase, currentCurrency);
                cartItemModel.UnitPrice = await _priceFormatter.FormatPriceAsync(shoppingCartUnitPriceWithDiscount);
                cartItemModel.UnitPriceValue = shoppingCartUnitPriceWithDiscount;

                //++Alchub
                //prepare price as shows in catalog.
                if (product.ProductType == ProductType.SimpleProduct)
                {
                    //simple product
                    await PrepareProductPriceRangeAsync(product, cartItemModel);
                }
                else
                {
                    var store = await _storeContext.GetCurrentStoreAsync();
                    var associatedProducts = await _productService.GetAssociatedProductsAsync(product.Id,
                        store.Id,
                        //++Alchub geovendor
                        geoVendorIds: await _workContext.GetCurrentCustomerGeoRadiusVendorIdsAsync());

                    //grouped product
                    await PrepareGroupedProductPriceRangeAsync(associatedProducts, cartItemModel);
                }
            }
            //subtotal, discount
            if (product.CallForPrice &&
                //also check whether the current user is impersonated
                (!_orderSettings.AllowAdminsToBuyCallForPriceProducts || _workContext.OriginalCustomerIfImpersonated == null))
            {
                cartItemModel.SubTotal = await _localizationService.GetResourceAsync("Products.CallForPrice");
                cartItemModel.SubTotalValue = 0;
            }
            else
            {
                //sub total
                var (subTotal, shoppingCartItemDiscountBase, _, maximumDiscountQty) = await _shoppingCartService.GetSubTotalAsync(sci, true);
                var (shoppingCartItemSubTotalWithDiscountBase, _) = await _taxService.GetProductPriceAsync(product, subTotal);
                var shoppingCartItemSubTotalWithDiscount = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(shoppingCartItemSubTotalWithDiscountBase, currentCurrency);
                cartItemModel.SubTotal = await _priceFormatter.FormatPriceAsync(shoppingCartItemSubTotalWithDiscount);
                cartItemModel.SubTotalValue = shoppingCartItemSubTotalWithDiscount;
                cartItemModel.MaximumDiscountedQty = maximumDiscountQty;

                //display an applied discount amount
                if (shoppingCartItemDiscountBase > decimal.Zero)
                {
                    (shoppingCartItemDiscountBase, _) = await _taxService.GetProductPriceAsync(product, shoppingCartItemDiscountBase);
                    if (shoppingCartItemDiscountBase > decimal.Zero)
                    {
                        var shoppingCartItemDiscount = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(shoppingCartItemDiscountBase, currentCurrency);
                        cartItemModel.Discount = await _priceFormatter.FormatPriceAsync(shoppingCartItemDiscount);
                        cartItemModel.DiscountValue = shoppingCartItemDiscount;
                    }
                }
            }

            //picture
            if (_shoppingCartSettings.ShowProductImagesOnWishList)
            {
                cartItemModel.Picture = await PrepareCartItemPictureModelAsync(sci,
                    _mediaSettings.CartThumbPictureSize, true, cartItemModel.ProductName);
            }

            //item warnings
            var itemWarnings = await _shoppingCartService.GetShoppingCartItemWarningsAsync(
                await _workContext.GetCurrentCustomerAsync(),
                sci.ShoppingCartType,
                product,
                sci.StoreId,
                sci.AttributesXml,
                sci.CustomerEnteredPrice,
                sci.RentalStartDateUtc,
                sci.RentalEndDateUtc,
                sci.Quantity,
                false,
                sci.Id);
            foreach (var warning in itemWarnings)
                cartItemModel.Warnings.Add(warning);

            return cartItemModel;
        }

        protected virtual async Task<decimal> PrepareSlotTotalAsync(IList<ShoppingCartItem> cart)
        {
            decimal customerSlotFee = 0;
            var slotList = cart.GroupBy(x => new { x.SlotId, x.IsPickup }).Select(x => new BookingSlotModel { Id = x.Key.SlotId, IsPickup = x.Key.IsPickup }).ToList();
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

        protected virtual async Task<IList<SlotFeeModel>> PrepareSlotListAsync(IList<ShoppingCartItem> cart)
        {
            IList<SlotFeeModel> slotFeeModels = new List<SlotFeeModel>();
            var slotList = cart.GroupBy(x => new { x.SlotId, x.IsPickup }).Select(x => new BookingSlotModel { Id = x.Key.SlotId, IsPickup = x.Key.IsPickup }).ToList();
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
                        SlotFeeModel slotFeeModel = new SlotFeeModel();
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
                        SlotFeeModel slotFeeModel = new SlotFeeModel();
                        slotFeeModel.SlotFee = await _priceFormatter.FormatPriceAsync(slot.Price, true, false);
                        slotFeeModel.VendorName = vendor != null ? vendor.Name : "";
                        slotFeeModels.Add(slotFeeModel);
                    }
                }

            }
            return slotFeeModels;
        }

        /// <summary>
        /// Prepare the cart item picture model
        /// </summary>
        /// <param name="sci">Shopping cart item</param>
        /// <param name="pictureSize">Picture size</param>
        /// <param name="showDefaultPicture">Whether to show the default picture</param>
        /// <param name="productName">Product name</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the picture model
        /// </returns>
        public virtual async Task<PictureModel> PrepareCartItemPictureModelAsync(ShoppingCartItem sci, int pictureSize, bool showDefaultPicture, string productName)
        {
            var pictureCacheKey = _staticCacheManager.PrepareKeyForShortTermCache(NopModelCacheDefaults.CartPictureModelKey
                , sci, pictureSize, true, await _workContext.GetWorkingLanguageAsync(), _webHelper.IsCurrentConnectionSecured(), await _storeContext.GetCurrentStoreAsync());

            var model = await _staticCacheManager.GetAsync(pictureCacheKey, async () =>
            {
                var product = await _productService.GetProductByIdAsync(sci.ProductId);
                //++Alchub
                var masterProduct = await _productService.GetProductByIdAsync(sci.MasterProductId);
                if (masterProduct == null)
                    masterProduct = product;

                //shopping cart item picture
                //var sciPicture = await _pictureService.GetProductPictureAsync(masterProduct, sci.AttributesXml);

                return new PictureModel
                {
                    //ImageUrl = (await _pictureService.GetPictureUrlAsync(sciPicture, pictureSize, showDefaultPicture)).Url,
                    //ImageUrl = await _pictureService.GetProductPictureUrlAsync(masterProduct.Sku, pictureSize),
                    ImageUrl = masterProduct?.ImageUrl ?? product.ImageUrl,
                    Title = string.Format(await _localizationService.GetResourceAsync("Media.Product.ImageLinkTitleFormat"), productName),
                    AlternateText = string.Format(await _localizationService.GetResourceAsync("Media.Product.ImageAlternateTextFormat"), productName),
                };
            });

            return model;
        }

        #endregion Methods
    }
}