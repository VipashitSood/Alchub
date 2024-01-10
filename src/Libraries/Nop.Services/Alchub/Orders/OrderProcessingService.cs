using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Alchub.Domain.Orders;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.DeliveryFees;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Discounts;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Slots;
using Nop.Core.Domain.Tax;
using Nop.Core.Domain.TipFees;
using Nop.Core.Domain.Vendors;
using Nop.Core.Events;
using Nop.Services.Affiliates;
using Nop.Services.Alchub.ServiceFee;
using Nop.Services.Alchub.Twillio;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.DeliveryFees;
using Nop.Services.Directory;
using Nop.Services.Discounts;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Payments;
using Nop.Services.Security;
using Nop.Services.Shipping;
using Nop.Services.Slots;
using Nop.Services.Tax;
using Nop.Services.TipFees;
using Nop.Services.Vendors;

namespace Nop.Services.Orders
{
    /// <summary>
    /// prepare order processing service
    /// </summary>
    public partial class OrderProcessingService : IOrderProcessingService
    {
        #region Fields

        private readonly CurrencySettings _currencySettings;
        private readonly IAddressService _addressService;
        private readonly IAffiliateService _affiliateService;
        private readonly ICheckoutAttributeFormatter _checkoutAttributeFormatter;
        private readonly ICountryService _countryService;
        private readonly ICurrencyService _currencyService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly ICustomerService _customerService;
        private readonly ICustomNumberFormatter _customNumberFormatter;
        private readonly IDiscountService _discountService;
        private readonly IEncryptionService _encryptionService;
        private readonly IEventPublisher _eventPublisher;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IGiftCardService _giftCardService;
        private readonly ILanguageService _languageService;
        private readonly ILocalizationService _localizationService;
        private readonly ILogger _logger;
        private readonly IOrderService _orderService;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly IPaymentPluginManager _paymentPluginManager;
        private readonly IPaymentService _paymentService;
        private readonly IPdfService _pdfService;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IProductAttributeFormatter _productAttributeFormatter;
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly IProductService _productService;
        private readonly IReturnRequestService _returnRequestService;
        private readonly IRewardPointService _rewardPointService;
        private readonly IShipmentService _shipmentService;
        private readonly IShippingService _shippingService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly ITaxService _taxService;
        private readonly IVendorService _vendorService;
        private readonly IWebHelper _webHelper;
        private readonly IWorkContext _workContext;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly LocalizationSettings _localizationSettings;
        private readonly OrderSettings _orderSettings;
        private readonly PaymentSettings _paymentSettings;
        private readonly RewardPointsSettings _rewardPointsSettings;
        private readonly ShippingSettings _shippingSettings;
        private readonly TaxSettings _taxSettings;
        private readonly IServiceFeeManager _serviceFeeManager;
        private readonly IDeliveryFeeService _deliveryFeeService;
        private readonly ITipFeeService _tipFeeService;
        private readonly ISlotService _slotService;
        private readonly ICategoryService _categoryService;
        private readonly ITwillioService _twillioService;
        private readonly IOrderItemRefundService _orderItemRefundService;
        #endregion

        #region Ctor

        public OrderProcessingService(CurrencySettings currencySettings,
            IAddressService addressService,
            IAffiliateService affiliateService,
            ICheckoutAttributeFormatter checkoutAttributeFormatter,
            ICountryService countryService,
            ICurrencyService currencyService,
            ICustomerActivityService customerActivityService,
            ICustomerService customerService,
            ICustomNumberFormatter customNumberFormatter,
            IDiscountService discountService,
            IEncryptionService encryptionService,
            IEventPublisher eventPublisher,
            IGenericAttributeService genericAttributeService,
            IGiftCardService giftCardService,
            ILanguageService languageService,
            ILocalizationService localizationService,
            ILogger logger,
            IOrderService orderService,
            IOrderTotalCalculationService orderTotalCalculationService,
            IPaymentPluginManager paymentPluginManager,
            IPaymentService paymentService,
            IPdfService pdfService,
            IPriceCalculationService priceCalculationService,
            IPriceFormatter priceFormatter,
            IProductAttributeFormatter productAttributeFormatter,
            IProductAttributeParser productAttributeParser,
            IProductService productService,
            IReturnRequestService returnRequestService,
            IRewardPointService rewardPointService,
            IShipmentService shipmentService,
            IShippingService shippingService,
            IShoppingCartService shoppingCartService,
            IStateProvinceService stateProvinceService,
            ITaxService taxService,
            IVendorService vendorService,
            IWebHelper webHelper,
            IWorkContext workContext,
            IWorkflowMessageService workflowMessageService,
            LocalizationSettings localizationSettings,
            OrderSettings orderSettings,
            PaymentSettings paymentSettings,
            RewardPointsSettings rewardPointsSettings,
            ShippingSettings shippingSettings,
            TaxSettings taxSettings,
            IServiceFeeManager serviceFeeManager,
            IDeliveryFeeService deliveryFeeService,
            ITipFeeService tipFeeService,
            ISlotService slotService,
            ICategoryService categoryService,
            ITwillioService twillioService, IOrderItemRefundService orderItemRefundService)
        {
            _currencySettings = currencySettings;
            _addressService = addressService;
            _affiliateService = affiliateService;
            _checkoutAttributeFormatter = checkoutAttributeFormatter;
            _countryService = countryService;
            _currencyService = currencyService;
            _customerActivityService = customerActivityService;
            _customerService = customerService;
            _customNumberFormatter = customNumberFormatter;
            _discountService = discountService;
            _encryptionService = encryptionService;
            _eventPublisher = eventPublisher;
            _genericAttributeService = genericAttributeService;
            _giftCardService = giftCardService;
            _languageService = languageService;
            _localizationService = localizationService;
            _logger = logger;
            _orderService = orderService;
            _orderTotalCalculationService = orderTotalCalculationService;
            _paymentPluginManager = paymentPluginManager;
            _paymentService = paymentService;
            _pdfService = pdfService;
            _priceCalculationService = priceCalculationService;
            _priceFormatter = priceFormatter;
            _productAttributeFormatter = productAttributeFormatter;
            _productAttributeParser = productAttributeParser;
            _productService = productService;
            _returnRequestService = returnRequestService;
            _rewardPointService = rewardPointService;
            _shipmentService = shipmentService;
            _shippingService = shippingService;
            _shoppingCartService = shoppingCartService;
            _stateProvinceService = stateProvinceService;
            _taxService = taxService;
            _vendorService = vendorService;
            _webHelper = webHelper;
            _workContext = workContext;
            _workflowMessageService = workflowMessageService;
            _localizationSettings = localizationSettings;
            _orderSettings = orderSettings;
            _paymentSettings = paymentSettings;
            _rewardPointsSettings = rewardPointsSettings;
            _shippingSettings = shippingSettings;
            _taxSettings = taxSettings;
            _serviceFeeManager = serviceFeeManager;
            _deliveryFeeService = deliveryFeeService;
            _tipFeeService = tipFeeService;
            _slotService = slotService;
            _categoryService = categoryService;
            _twillioService = twillioService;
            _orderItemRefundService = orderItemRefundService;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Prepare details to place an order. It also sets some properties to "processPaymentRequest"
        /// </summary>
        /// <param name="processPaymentRequest">Process payment request</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the details
        /// </returns>
        protected virtual async Task<PlaceOrderContainer> PreparePlaceOrderDetailsAsync(ProcessPaymentRequest processPaymentRequest)
        {
            var details = new PlaceOrderContainer
            {
                //customer
                Customer = await _customerService.GetCustomerByIdAsync(processPaymentRequest.CustomerId)
            };
            if (details.Customer == null)
                throw new ArgumentException("Customer is not set");

            //affiliate
            var affiliate = await _affiliateService.GetAffiliateByIdAsync(details.Customer.AffiliateId);
            if (affiliate != null && affiliate.Active && !affiliate.Deleted)
                details.AffiliateId = affiliate.Id;

            //check whether customer is guest
            if (await _customerService.IsGuestAsync(details.Customer) && !_orderSettings.AnonymousCheckoutAllowed)
                throw new NopException("Anonymous checkout is not allowed");

            //customer currency
            var currencyTmp = await _currencyService.GetCurrencyByIdAsync(
                await _genericAttributeService.GetAttributeAsync<int>(details.Customer, NopCustomerDefaults.CurrencyIdAttribute, processPaymentRequest.StoreId));
            var currentCurrency = await _workContext.GetWorkingCurrencyAsync();
            var customerCurrency = currencyTmp != null && currencyTmp.Published ? currencyTmp : currentCurrency;
            var primaryStoreCurrency = await _currencyService.GetCurrencyByIdAsync(_currencySettings.PrimaryStoreCurrencyId);
            details.CustomerCurrencyCode = customerCurrency.CurrencyCode;
            details.CustomerCurrencyRate = customerCurrency.Rate / primaryStoreCurrency.Rate;

            //customer language
            details.CustomerLanguage = await _languageService.GetLanguageByIdAsync(
                await _genericAttributeService.GetAttributeAsync<int>(details.Customer, NopCustomerDefaults.LanguageIdAttribute, processPaymentRequest.StoreId));
            if (details.CustomerLanguage == null || !details.CustomerLanguage.Published)
                details.CustomerLanguage = await _workContext.GetWorkingLanguageAsync();

            //billing address
            if (details.Customer.BillingAddressId is null)
                throw new NopException("Address is not provided");

            var billingAddress = await _customerService.GetCustomerBillingAddressAsync(details.Customer);

            if (!CommonHelper.IsValidEmail(billingAddress?.Email))
                throw new NopException("Email is not valid");

            details.BillingAddress = _addressService.CloneAddress(billingAddress);

            if (await _countryService.GetCountryByAddressAsync(details.BillingAddress) is Country billingCountry && !billingCountry.AllowsBilling)
                throw new NopException($"Country '{billingCountry.Name}' is not allowed for billing");

            //checkout attributes
            details.CheckoutAttributesXml = await _genericAttributeService.GetAttributeAsync<string>(details.Customer, NopCustomerDefaults.CheckoutAttributes, processPaymentRequest.StoreId);
            details.CheckoutAttributeDescription = await _checkoutAttributeFormatter.FormatAttributesAsync(details.CheckoutAttributesXml, details.Customer);

            //load shopping cart
            details.Cart = await _shoppingCartService.GetShoppingCartAsync(details.Customer, ShoppingCartType.ShoppingCart, processPaymentRequest.StoreId);

            if (!details.Cart.Any())
                throw new NopException("Cart is empty");

            //validate the entire shopping cart
            var warnings = await _shoppingCartService.GetShoppingCartWarningsAsync(details.Cart, details.CheckoutAttributesXml, true);
            if (warnings.Any())
                throw new NopException(warnings.Aggregate(string.Empty, (current, next) => $"{current}{next};"));

            //validate individual cart items
            foreach (var sci in details.Cart)
            {
                var product = await _productService.GetProductByIdAsync(sci.ProductId);

                var sciWarnings = await _shoppingCartService.GetShoppingCartItemWarningsAsync(details.Customer,
                    sci.ShoppingCartType, product, processPaymentRequest.StoreId, sci.AttributesXml,
                    sci.CustomerEnteredPrice, sci.RentalStartDateUtc, sci.RentalEndDateUtc, sci.Quantity, false, sci.Id);
                if (sciWarnings.Any())
                    throw new NopException(sciWarnings.Aggregate(string.Empty, (current, next) => $"{current}{next};"));
            }

            /*Alchub Start*/
            var vendorMinimumOrderAmountWarnings = await _shoppingCartService.GetVendorMinimumOrderAmountWarningsAsync(details.Cart);
            if (vendorMinimumOrderAmountWarnings.Any())
                throw new NopException(vendorMinimumOrderAmountWarnings.Aggregate(string.Empty, (current, next) => $"{current}{next};"));
            /*Alchub End*/

            //min totals validation
            if (!await ValidateMinOrderSubtotalAmountAsync(details.Cart))
            {
                var minOrderSubtotalAmount = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(_orderSettings.MinOrderSubtotalAmount, currentCurrency);
                throw new NopException(string.Format(await _localizationService.GetResourceAsync("Checkout.MinOrderSubtotalAmount"),
                    await _priceFormatter.FormatPriceAsync(minOrderSubtotalAmount, true, false)));
            }

            if (!await ValidateMinOrderTotalAmountAsync(details.Cart))
            {
                var minOrderTotalAmount = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(_orderSettings.MinOrderTotalAmount, currentCurrency);
                throw new NopException(string.Format(await _localizationService.GetResourceAsync("Checkout.MinOrderTotalAmount"),
                    await _priceFormatter.FormatPriceAsync(minOrderTotalAmount, true, false)));
            }

            //tax display type
            if (_taxSettings.AllowCustomersToSelectTaxDisplayType)
                details.CustomerTaxDisplayType = (TaxDisplayType)await _genericAttributeService.GetAttributeAsync<int>(details.Customer, NopCustomerDefaults.TaxDisplayTypeIdAttribute, processPaymentRequest.StoreId);
            else
                details.CustomerTaxDisplayType = _taxSettings.TaxDisplayType;

            //sub total (incl tax)
            var (orderSubTotalDiscountAmount, orderSubTotalAppliedDiscounts, subTotalWithoutDiscountBase, _, _) = await _orderTotalCalculationService.GetShoppingCartSubTotalAsync(details.Cart, true);
            details.OrderSubTotalInclTax = subTotalWithoutDiscountBase;
            details.OrderSubTotalDiscountInclTax = orderSubTotalDiscountAmount;

            //discount history
            foreach (var disc in orderSubTotalAppliedDiscounts)
                if (!_discountService.ContainsDiscount(details.AppliedDiscounts, disc))
                    details.AppliedDiscounts.Add(disc);

            //sub total (excl tax)
            (orderSubTotalDiscountAmount, _, subTotalWithoutDiscountBase, _, _) = await _orderTotalCalculationService.GetShoppingCartSubTotalAsync(details.Cart, false);
            details.OrderSubTotalExclTax = subTotalWithoutDiscountBase;
            details.OrderSubTotalDiscountExclTax = orderSubTotalDiscountAmount;

            //shipping info
            if (await _shoppingCartService.ShoppingCartRequiresShippingAsync(details.Cart))
            {
                var pickupPoint = await _genericAttributeService.GetAttributeAsync<PickupPoint>(details.Customer,
                    NopCustomerDefaults.SelectedPickupPointAttribute, processPaymentRequest.StoreId);
                if (_shippingSettings.AllowPickupInStore && pickupPoint != null)
                {
                    var country = await _countryService.GetCountryByTwoLetterIsoCodeAsync(pickupPoint.CountryCode);
                    var state = await _stateProvinceService.GetStateProvinceByAbbreviationAsync(pickupPoint.StateAbbreviation, country?.Id);

                    details.PickupInStore = true;
                    details.PickupAddress = new Address
                    {
                        Address1 = pickupPoint.Address,
                        City = pickupPoint.City,
                        County = pickupPoint.County,
                        CountryId = country?.Id,
                        StateProvinceId = state?.Id,
                        ZipPostalCode = pickupPoint.ZipPostalCode,
                        CreatedOnUtc = DateTime.UtcNow
                    };
                }
                else
                {
                    if (details.Customer.ShippingAddressId == null)
                        throw new NopException("Shipping address is not provided");

                    var shippingAddress = await _customerService.GetCustomerShippingAddressAsync(details.Customer);

                    if (!CommonHelper.IsValidEmail(shippingAddress?.Email))
                        throw new NopException("Email is not valid");

                    //clone shipping address
                    details.ShippingAddress = _addressService.CloneAddress(shippingAddress);

                    if (await _countryService.GetCountryByAddressAsync(details.ShippingAddress) is Country shippingCountry && !shippingCountry.AllowsShipping)
                        throw new NopException($"Country '{shippingCountry.Name}' is not allowed for shipping");
                }

                var shippingOption = await _genericAttributeService.GetAttributeAsync<ShippingOption>(details.Customer,
                    NopCustomerDefaults.SelectedShippingOptionAttribute, processPaymentRequest.StoreId);
                if (shippingOption != null)
                {
                    details.ShippingMethodName = shippingOption.Name;
                    details.ShippingRateComputationMethodSystemName = shippingOption.ShippingRateComputationMethodSystemName;
                }

                details.ShippingStatus = ShippingStatus.NotYetShipped;
            }
            else
                details.ShippingStatus = ShippingStatus.ShippingNotRequired;

            //shipping total
            var (orderShippingTotalInclTax, _, shippingTotalDiscounts) = await _orderTotalCalculationService.GetShoppingCartShippingTotalAsync(details.Cart, true);
            var (orderShippingTotalExclTax, _, _) = await _orderTotalCalculationService.GetShoppingCartShippingTotalAsync(details.Cart, false);
            if (!orderShippingTotalInclTax.HasValue || !orderShippingTotalExclTax.HasValue)
                throw new NopException("Shipping total couldn't be calculated");

            details.OrderShippingTotalInclTax = orderShippingTotalInclTax.Value;
            details.OrderShippingTotalExclTax = orderShippingTotalExclTax.Value;

            foreach (var disc in shippingTotalDiscounts)
                if (!_discountService.ContainsDiscount(details.AppliedDiscounts, disc))
                    details.AppliedDiscounts.Add(disc);

            //payment total
            var paymentAdditionalFee = await _paymentService.GetAdditionalHandlingFeeAsync(details.Cart, processPaymentRequest.PaymentMethodSystemName);
            details.PaymentAdditionalFeeInclTax = (await _taxService.GetPaymentMethodAdditionalFeeAsync(paymentAdditionalFee, true, details.Customer)).price;
            details.PaymentAdditionalFeeExclTax = (await _taxService.GetPaymentMethodAdditionalFeeAsync(paymentAdditionalFee, false, details.Customer)).price;

            //tax amount
            SortedDictionary<decimal, decimal> taxRatesDictionary;
            (details.OrderTaxTotal, taxRatesDictionary) = await _orderTotalCalculationService.GetTaxTotalAsync(details.Cart);

            //VAT number
            var customerVatStatus = (VatNumberStatus)await _genericAttributeService.GetAttributeAsync<int>(details.Customer, NopCustomerDefaults.VatNumberStatusIdAttribute);
            if (_taxSettings.EuVatEnabled && customerVatStatus == VatNumberStatus.Valid)
                details.VatNumber = await _genericAttributeService.GetAttributeAsync<string>(details.Customer, NopCustomerDefaults.VatNumberAttribute);

            //tax rates
            details.TaxRates = taxRatesDictionary.Aggregate(string.Empty, (current, next) =>
                $"{current}{next.Key.ToString(CultureInfo.InvariantCulture)}:{next.Value.ToString(CultureInfo.InvariantCulture)};   ");

            //order total (and applied discounts, gift cards, reward points)
            var (orderTotal, orderDiscountAmount, orderAppliedDiscounts, appliedGiftCards, redeemedRewardPoints, redeemedRewardPointsAmount) = await _orderTotalCalculationService.GetShoppingCartTotalAsync(details.Cart);
            if (!orderTotal.HasValue)
                throw new NopException("Order total couldn't be calculated");

            details.OrderDiscountAmount = orderDiscountAmount;
            details.RedeemedRewardPoints = redeemedRewardPoints;
            details.RedeemedRewardPointsAmount = redeemedRewardPointsAmount;
            details.AppliedGiftCards = appliedGiftCards;
            details.OrderTotal = orderTotal.Value;

            //discount history
            foreach (var disc in orderAppliedDiscounts)
                if (!_discountService.ContainsDiscount(details.AppliedDiscounts, disc))
                    details.AppliedDiscounts.Add(disc);

            processPaymentRequest.OrderTotal = details.OrderTotal;

            //recurring or standard shopping cart?
            details.IsRecurringShoppingCart = await _shoppingCartService.ShoppingCartIsRecurringAsync(details.Cart);
            if (!details.IsRecurringShoppingCart)
                return details;

            var (recurringCyclesError, recurringCycleLength, recurringCyclePeriod, recurringTotalCycles) = await _shoppingCartService.GetRecurringCycleInfoAsync(details.Cart);

            if (!string.IsNullOrEmpty(recurringCyclesError))
                throw new NopException(recurringCyclesError);

            processPaymentRequest.RecurringCycleLength = recurringCycleLength;
            processPaymentRequest.RecurringCyclePeriod = recurringCyclePeriod;
            processPaymentRequest.RecurringTotalCycles = recurringTotalCycles;

            return details;
        }

        /// <summary>
        /// Save order and add order notes
        /// </summary>
        /// <param name="processPaymentRequest">Process payment request</param>
        /// <param name="processPaymentResult">Process payment result</param>
        /// <param name="details">Details</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the order
        /// </returns>
        protected virtual async Task<Order> SaveOrderDetailsAsync(ProcessPaymentRequest processPaymentRequest,
            ProcessPaymentResult processPaymentResult, PlaceOrderContainer details)
        {
            var order = new Order
            {
                StoreId = processPaymentRequest.StoreId,
                OrderGuid = processPaymentRequest.OrderGuid,
                CustomerId = details.Customer.Id,
                CustomerLanguageId = details.CustomerLanguage.Id,
                CustomerTaxDisplayType = details.CustomerTaxDisplayType,
                CustomerIp = _webHelper.GetCurrentIpAddress(),
                OrderSubtotalInclTax = details.OrderSubTotalInclTax,
                OrderSubtotalExclTax = details.OrderSubTotalExclTax,
                OrderSubTotalDiscountInclTax = details.OrderSubTotalDiscountInclTax,
                OrderSubTotalDiscountExclTax = details.OrderSubTotalDiscountExclTax,
                OrderShippingInclTax = details.OrderShippingTotalInclTax,
                OrderShippingExclTax = details.OrderShippingTotalExclTax,
                PaymentMethodAdditionalFeeInclTax = details.PaymentAdditionalFeeInclTax,
                PaymentMethodAdditionalFeeExclTax = details.PaymentAdditionalFeeExclTax,
                TaxRates = details.TaxRates,
                OrderTax = details.OrderTaxTotal,
                OrderTotal = details.OrderTotal,
                RefundedAmount = decimal.Zero,
                OrderDiscount = details.OrderDiscountAmount,
                CheckoutAttributeDescription = details.CheckoutAttributeDescription,
                CheckoutAttributesXml = details.CheckoutAttributesXml,
                CustomerCurrencyCode = details.CustomerCurrencyCode,
                CurrencyRate = details.CustomerCurrencyRate,
                AffiliateId = details.AffiliateId,
                OrderStatus = OrderStatus.Pending,
                AllowStoringCreditCardNumber = processPaymentResult.AllowStoringCreditCardNumber,
                CardType = processPaymentResult.AllowStoringCreditCardNumber ? _encryptionService.EncryptText(processPaymentRequest.CreditCardType) : string.Empty,
                CardName = processPaymentResult.AllowStoringCreditCardNumber ? _encryptionService.EncryptText(processPaymentRequest.CreditCardName) : string.Empty,
                CardNumber = processPaymentResult.AllowStoringCreditCardNumber ? _encryptionService.EncryptText(processPaymentRequest.CreditCardNumber) : string.Empty,
                MaskedCreditCardNumber = _encryptionService.EncryptText(_paymentService.GetMaskedCreditCardNumber(processPaymentRequest.CreditCardNumber)),
                CardCvv2 = processPaymentResult.AllowStoringCreditCardNumber ? _encryptionService.EncryptText(processPaymentRequest.CreditCardCvv2) : string.Empty,
                CardExpirationMonth = processPaymentResult.AllowStoringCreditCardNumber ? _encryptionService.EncryptText(processPaymentRequest.CreditCardExpireMonth.ToString()) : string.Empty,
                CardExpirationYear = processPaymentResult.AllowStoringCreditCardNumber ? _encryptionService.EncryptText(processPaymentRequest.CreditCardExpireYear.ToString()) : string.Empty,
                PaymentMethodSystemName = processPaymentRequest.PaymentMethodSystemName,
                AuthorizationTransactionId = processPaymentResult.AuthorizationTransactionId,
                AuthorizationTransactionCode = processPaymentResult.AuthorizationTransactionCode,
                AuthorizationTransactionResult = processPaymentResult.AuthorizationTransactionResult,
                CaptureTransactionId = processPaymentResult.CaptureTransactionId,
                CaptureTransactionResult = processPaymentResult.CaptureTransactionResult,
                SubscriptionTransactionId = processPaymentResult.SubscriptionTransactionId,
                PaymentStatus = processPaymentResult.NewPaymentStatus,
                PaidDateUtc = null,
                PickupInStore = details.PickupInStore,
                ShippingStatus = details.ShippingStatus,
                ShippingMethod = details.ShippingMethodName,
                ShippingRateComputationMethodSystemName = details.ShippingRateComputationMethodSystemName,
                CustomValuesXml = _paymentService.SerializeCustomValues(processPaymentRequest),
                VatNumber = details.VatNumber,
                CreatedOnUtc = DateTime.UtcNow,
                CustomOrderNumber = string.Empty
            };

            if (details.BillingAddress is null)
                throw new NopException("Address is not provided");

            await _addressService.InsertAddressAsync(details.BillingAddress);
            order.BillingAddressId = details.BillingAddress.Id;

            if (details.PickupAddress != null)
            {
                await _addressService.InsertAddressAsync(details.PickupAddress);
                order.PickupAddressId = details.PickupAddress.Id;
            }

            if (details.ShippingAddress != null)
            {
                await _addressService.InsertAddressAsync(details.ShippingAddress);
                order.ShippingAddressId = details.ShippingAddress.Id;
            }

            //service fee
            order.ServiceFee = await _serviceFeeManager.GetServiceFeeAsync(order.OrderSubtotalExclTax);

            //slot fee
            order.SlotFee = await PrepareSlotTotalAsync(details.Cart);

            await _orderService.InsertOrderAsync(order);

            //generate and set custom order number
            order.CustomOrderNumber = _customNumberFormatter.GenerateOrderCustomNumber(order);
            await _orderService.UpdateOrderAsync(order);

            //reward points history
            if (details.RedeemedRewardPointsAmount <= decimal.Zero)
                return order;

            order.RedeemedRewardPointsEntryId = await _rewardPointService.AddRewardPointsHistoryEntryAsync(details.Customer, -details.RedeemedRewardPoints, order.StoreId,
                string.Format(await _localizationService.GetResourceAsync("RewardPoints.Message.RedeemedForOrder", order.CustomerLanguageId), order.CustomOrderNumber),
                order, details.RedeemedRewardPointsAmount);
            await _customerService.UpdateCustomerAsync(details.Customer);
            await _orderService.UpdateOrderAsync(order);

            return order;
        }

        /// <summary>
        /// Move shopping cart items to order items
        /// </summary>
        /// <param name="details">Place order container</param>
        /// <param name="order">Order</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        protected virtual async Task MoveShoppingCartItemsToOrderItemsAsync(PlaceOrderContainer details, Order order)
        {
            foreach (var sc in details.Cart)
            {
                var product = await _productService.GetProductByIdAsync(sc.ProductId);
                var vendor = await _vendorService.GetVendorByProductIdAsync(sc.ProductId);

                //prices
                var scUnitPrice = (await _shoppingCartService.GetUnitPriceAsync(sc, true)).unitPrice;
                var (scSubTotal, discountAmount, scDiscounts, _) = await _shoppingCartService.GetSubTotalAsync(sc, true);
                var scUnitPriceInclTax = await _taxService.GetProductPriceAsync(product, scUnitPrice, true, details.Customer);
                var scUnitPriceExclTax = await _taxService.GetProductPriceAsync(product, scUnitPrice, false, details.Customer);
                var scSubTotalInclTax = await _taxService.GetProductPriceAsync(product, scSubTotal, true, details.Customer);
                var scSubTotalExclTax = await _taxService.GetProductPriceAsync(product, scSubTotal, false, details.Customer);
                var discountAmountInclTax = await _taxService.GetProductPriceAsync(product, discountAmount, true, details.Customer);
                var discountAmountExclTax = await _taxService.GetProductPriceAsync(product, discountAmount, false, details.Customer);
                foreach (var disc in scDiscounts)
                    if (!_discountService.ContainsDiscount(details.AppliedDiscounts, disc))
                        details.AppliedDiscounts.Add(disc);

                //attributes
                var attributeDescription =
                    await _productAttributeFormatter.FormatAttributesAsync(product, sc.AttributesXml, details.Customer);

                //custom att description
                var customAttributesDescription = await _productAttributeFormatter.FormatCustomAttributesAsync(sc.CustomAttributesXml);

                var itemWeight = await _shippingService.GetShoppingCartItemWeightAsync(sc);

                //save order item
                var orderItem = new OrderItem
                {
                    OrderItemGuid = Guid.NewGuid(),
                    OrderId = order.Id,
                    ProductId = product.Id,
                    UnitPriceInclTax = scUnitPriceInclTax.price,
                    UnitPriceExclTax = scUnitPriceExclTax.price,
                    PriceInclTax = scSubTotalInclTax.price,
                    PriceExclTax = scSubTotalExclTax.price,
                    OriginalProductCost = await _priceCalculationService.GetProductCostAsync(product, sc.AttributesXml),
                    AttributeDescription = attributeDescription,
                    AttributesXml = sc.AttributesXml,
                    Quantity = sc.Quantity,
                    DiscountAmountInclTax = discountAmountInclTax.price,
                    DiscountAmountExclTax = discountAmountExclTax.price,
                    DownloadCount = 0,
                    IsDownloadActivated = false,
                    LicenseDownloadId = 0,
                    ItemWeight = itemWeight,
                    RentalStartDateUtc = sc.RentalStartDateUtc,
                    RentalEndDateUtc = sc.RentalEndDateUtc,
                    SlotId = sc.SlotId,
                    SlotStartTime = sc.SlotStartTime,
                    SlotEndTime = sc.SlotEndTime,
                    SlotTime = sc.SlotTime,
                    SlotPrice = sc.SlotPrice,
                    InPickup = sc.IsPickup,
                    //master & grouped product fields
                    MasterProductId = sc.MasterProductId,
                    GroupedProductId = sc.GroupedProductId,
                    CustomAttributesXml = sc.CustomAttributesXml,
                    CustomAttributesDescription = customAttributesDescription,
                    //order item satus
                    OrderItemStatus = OrderItemStatus.Pending,
                    VendorManageDelivery = vendor?.ManageDelivery ?? true,
                    DeliveryFee = await _deliveryFeeService.GetOrderItemDeliveryFeeAsync(sc)
                };

                await _orderService.InsertOrderItemAsync(orderItem);

                //gift cards
                await AddGiftCardsAsync(product, sc.AttributesXml, sc.Quantity, orderItem, scUnitPriceExclTax.price);

                //inventory
                await _productService.AdjustInventoryAsync(product, -sc.Quantity, sc.AttributesXml,
                    string.Format(await _localizationService.GetResourceAsync("Admin.StockQuantityHistory.Messages.PlaceOrder"), order.Id));
            }

            //clear shopping cart
            details.Cart.ToList().ForEach(async sci => await _shoppingCartService.DeleteShoppingCartItemAsync(sci, false));
        }

        /// <summary>
        /// Send "order placed" notifications and save order notes
        /// </summary>
        /// <param name="order">Order</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        protected virtual async Task SendNotificationsAndSaveNotesAsync(Order order)
        {
            ////notes, messages
            //await AddOrderNoteAsync(order, _workContext.OriginalCustomerIfImpersonated != null
            //    ? $"Order placed by a store owner ('{_workContext.OriginalCustomerIfImpersonated.Email}'. ID = {_workContext.OriginalCustomerIfImpersonated.Id}) impersonating the customer."
            //    : "Order placed");

            //send email notifications
            var orderPlacedStoreOwnerNotificationQueuedEmailIds = await _workflowMessageService.SendOrderPlacedStoreOwnerNotificationAsync(order, _localizationSettings.DefaultAdminLanguageId);
            if (orderPlacedStoreOwnerNotificationQueuedEmailIds.Any())
                await AddOrderNoteAsync(order, $"\"Order placed\" email (to store owner) has been queued. Queued email identifiers: {string.Join(", ", orderPlacedStoreOwnerNotificationQueuedEmailIds)}.");

            var orderPlacedAttachmentFilePath = _orderSettings.AttachPdfInvoiceToOrderPlacedEmail ?
                (await _pdfService.PrintOrderToPdfAsync(order)) : null;
            var orderPlacedAttachmentFileName = _orderSettings.AttachPdfInvoiceToOrderPlacedEmail ?
                (string.Format(await _localizationService.GetResourceAsync("PDFInvoice.FileName"), order.CustomOrderNumber) + ".pdf") : null;
            var orderPlacedCustomerNotificationQueuedEmailIds = await _workflowMessageService
                .SendOrderPlacedCustomerNotificationAsync(order, order.CustomerLanguageId, orderPlacedAttachmentFilePath, orderPlacedAttachmentFileName);
            if (orderPlacedCustomerNotificationQueuedEmailIds.Any())
                await AddOrderNoteAsync(order, $"\"Order placed\" email (to customer) has been queued. Queued email identifiers: {string.Join(", ", orderPlacedCustomerNotificationQueuedEmailIds)}.");

            //++Alchub

            //send order placed SMS notification to customer
            await _twillioService.SendOrderPlacedCustomerSMSAsync(order, order.CustomerLanguageId);

            //send order placed/received SMS notification to vendor
            await _twillioService.SendOrderPlacedVendorSMSAsync(order, order.CustomerLanguageId);

            //--Alchub

            var vendors = await GetVendorsInOrderAsync(order);
            foreach (var vendor in vendors)
            {
                var orderPlacedVendorNotificationQueuedEmailIds = await _workflowMessageService.SendOrderPlacedVendorNotificationAsync(order, vendor, _localizationSettings.DefaultAdminLanguageId);
                if (orderPlacedVendorNotificationQueuedEmailIds.Any())
                    await AddOrderNoteAsync(order, $"\"Order placed\" email (to vendor) has been queued. Queued email identifiers: {string.Join(", ", orderPlacedVendorNotificationQueuedEmailIds)}.");
            }

            if (order.AffiliateId == 0)
                return;

            var orderPlacedAffiliateNotificationQueuedEmailIds = await _workflowMessageService.SendOrderPlacedAffiliateNotificationAsync(order, _localizationSettings.DefaultAdminLanguageId);
            if (orderPlacedAffiliateNotificationQueuedEmailIds.Any())
                await AddOrderNoteAsync(order, $"\"Order placed\" email (to affiliate) has been queued. Queued email identifiers: {string.Join(", ", orderPlacedAffiliateNotificationQueuedEmailIds)}.");
        }

        /// <summary>
        /// Process order paid status
        /// </summary>
        /// <param name="order">Order</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        protected virtual async Task ProcessOrderPaidAsync(Order order, bool sendOrderPlaceNotification = false)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            //raise event
            await _eventPublisher.PublishAsync(new OrderPaidEvent(order));

            //order paid email notification
            if (order.OrderTotal != decimal.Zero)
            {
                //++Alchub

                //Note: Send Order place mail only after order gets paid. (08-12-22)
                if (sendOrderPlaceNotification)
                    await SendNotificationsAndSaveNotesAsync(order);

                //--Alchub

                //we should not send it for free ($0 total) orders?
                //remove this "if" statement if you want to send it in this case

                var orderPaidAttachmentFilePath = _orderSettings.AttachPdfInvoiceToOrderPaidEmail ?
                    await _pdfService.PrintOrderToPdfAsync(order) : null;
                var orderPaidAttachmentFileName = _orderSettings.AttachPdfInvoiceToOrderPaidEmail ?
                    (string.Format(await _localizationService.GetResourceAsync("PDFInvoice.FileName"), order.CustomOrderNumber) + ".pdf") : null;
                var orderPaidCustomerNotificationQueuedEmailIds = await _workflowMessageService.SendOrderPaidCustomerNotificationAsync(order, order.CustomerLanguageId,
                    orderPaidAttachmentFilePath, orderPaidAttachmentFileName);

                if (orderPaidCustomerNotificationQueuedEmailIds.Any())
                    await AddOrderNoteAsync(order, $"\"Order paid\" email (to customer) has been queued. Queued email identifiers: {string.Join(", ", orderPaidCustomerNotificationQueuedEmailIds)}.");

                var orderPaidStoreOwnerNotificationQueuedEmailIds = await _workflowMessageService.SendOrderPaidStoreOwnerNotificationAsync(order, _localizationSettings.DefaultAdminLanguageId);
                if (orderPaidStoreOwnerNotificationQueuedEmailIds.Any())
                    await AddOrderNoteAsync(order, $"\"Order paid\" email (to store owner) has been queued. Queued email identifiers: {string.Join(", ", orderPaidStoreOwnerNotificationQueuedEmailIds)}.");

                var vendors = await GetVendorsInOrderAsync(order);
                foreach (var vendor in vendors)
                {
                    var orderPaidVendorNotificationQueuedEmailIds = await _workflowMessageService.SendOrderPaidVendorNotificationAsync(order, vendor, _localizationSettings.DefaultAdminLanguageId);

                    if (orderPaidVendorNotificationQueuedEmailIds.Any())
                        await AddOrderNoteAsync(order, $"\"Order paid\" email (to vendor) has been queued. Queued email identifiers: {string.Join(", ", orderPaidVendorNotificationQueuedEmailIds)}.");
                }

                if (order.AffiliateId != 0)
                {
                    var orderPaidAffiliateNotificationQueuedEmailIds = await _workflowMessageService.SendOrderPaidAffiliateNotificationAsync(order,
                        _localizationSettings.DefaultAdminLanguageId);
                    if (orderPaidAffiliateNotificationQueuedEmailIds.Any())
                        await AddOrderNoteAsync(order, $"\"Order paid\" email (to affiliate) has been queued. Queued email identifiers: {string.Join(", ", orderPaidAffiliateNotificationQueuedEmailIds)}.");
                }
            }

            //customer roles with "purchased with product" specified
            await ProcessCustomerRolesWithPurchasedProductSpecifiedAsync(order, true);
        }

        /// <summary>
        /// Marks order as authorized
        /// </summary>
        /// <param name="order">Order</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task MarkAsAuthorizedAsync(Order order, bool performPaidAction = false)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            order.PaymentStatusId = (int)PaymentStatus.Authorized;
            await _orderService.UpdateOrderAsync(order);

            //add a note
            await AddOrderNoteAsync(order, "Order has been marked as authorized");

            //check order status
            await CheckOrderStatusAsync(order);

            /*Alchub Start*/
            //We will do all action performed on paid action here on Authorized
            if (performPaidAction)
                await ProcessOrderPaidAsync(order, sendOrderPlaceNotification: true);
            /*Alchub End*/

            await _eventPublisher.PublishAsync(new OrderAuthorizedEvent(order));
        }

        /// <summary>
        /// Marks order as paid
        /// </summary>
        /// <param name="order">Order</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task MarkOrderAsPaidAsync(Order order, bool performPaidAction = true)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            if (!CanMarkOrderAsPaid(order))
                throw new NopException("You can't mark this order as paid");

            //++Alchub (12-01-23)
            //if priviously order payment status is pending, then payment must not have authorized, hence order place email are still pending to send.
            bool sendOrderPlaceNotification = order.PaymentStatusId == (int)PaymentStatus.Pending;
            //--Alchub

            order.PaymentStatusId = (int)PaymentStatus.Paid;
            order.PaidDateUtc = DateTime.UtcNow;
            await _orderService.UpdateOrderAsync(order);

            //add a note
            await AddOrderNoteAsync(order, "Order has been marked as paid");

            await CheckOrderStatusAsync(order);

            /*Alchub Start*/
            //We will do all action performed on paid action here on Paid
            if (performPaidAction && order.PaymentStatus == PaymentStatus.Paid)
                await ProcessOrderPaidAsync(order, sendOrderPlaceNotification);
            /*Alchub End*/
        }

        #endregion Utilities

        #region methods

        protected virtual async Task<decimal> PrepareSlotTotalAsync(IList<ShoppingCartItem> cart)
        {
            decimal customerSlotFee = 0;
            var slotList = cart.GroupBy(x => new { x.SlotId, x.IsPickup }).Select(x => new CustomerProductSlot { Id = x.Key.SlotId, IsPickup = x.Key.IsPickup }).ToList();
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

        /// <summary>
        /// Save Order Delivery Fee
        /// </summary>
        /// <param name="details"></param>
        /// <param name="order"></param>
        /// <returns></returns>
        protected virtual async Task SaveOrderDeliveryFeeAsync(PlaceOrderContainer details, Order order)
        {
            //Delivery fee info
            var vendorWiseDeliveryFees = await _deliveryFeeService.GetVendorWiseDeliveryFeeAsync(details.Cart);

            if (vendorWiseDeliveryFees != null)
            {
                //Delivery Fee
                order.DeliveryFee = vendorWiseDeliveryFees?.Sum(x => x.DeliveryFeeValue) ?? decimal.Zero;
                await _orderService.UpdateOrderAsync(order);

                var orderDeliveryFees = new List<OrderDeliveryFee>();

                vendorWiseDeliveryFees.ToList().ForEach(x =>
                {
                    orderDeliveryFees.Add(new OrderDeliveryFee
                    {
                        OrderId = order.Id,
                        VendorId = x.VendorId,
                        DeliveryFee = x.DeliveryFeeValue
                    });
                });

                await _deliveryFeeService.InsertOrderDeliveryFeesAsync(orderDeliveryFees);
            }
        }

        /// <summary>
        /// Save Order Tip Fee
        /// </summary>
        /// <param name="details"></param>
        /// <param name="order"></param>
        /// <returns></returns>
        protected virtual async Task SaveOrderTipFeeAsync(PlaceOrderContainer details, Order order)
        {
            //Tip fee info
            var vendorWiseTipFees = await _tipFeeService.GetVendorWiseTipFeeAsync(details.Cart, order.OrderSubtotalExclTax);

            if (vendorWiseTipFees != null)
            {
                //Tip Fee
                order.TipFee = vendorWiseTipFees.Sum(x => x.TipFeeValue);
                await _orderService.UpdateOrderAsync(order);

                var orderTipFees = new List<OrderTipFee>();

                vendorWiseTipFees.ToList().ForEach(x =>
                {
                    orderTipFees.Add(new OrderTipFee
                    {
                        OrderId = order.Id,
                        VendorId = x.VendorId,
                        TipFee = x.TipFeeValue
                    });
                });

                await _tipFeeService.InsertOrderTipFeesAsync(orderTipFees);
            }

            //Remove Customer selected tips
            await _tipFeeService.RemoveCustomerTipFeeDetailsAsync();
        }

        /// <summary>
        /// Places an order
        /// </summary>
        /// <param name="processPaymentRequest">Process payment request</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the place order result
        /// </returns>
        public virtual async Task<PlaceOrderResult> PlaceOrderAsync(ProcessPaymentRequest processPaymentRequest)
        {
            if (processPaymentRequest == null)
                throw new ArgumentNullException(nameof(processPaymentRequest));

            var result = new PlaceOrderResult();
            try
            {
                if (processPaymentRequest.OrderGuid == Guid.Empty)
                    throw new Exception("Order GUID is not generated");

                //prepare order details
                var details = await PreparePlaceOrderDetailsAsync(processPaymentRequest);

                var processPaymentResult = await GetProcessPaymentResultAsync(processPaymentRequest, details);

                if (processPaymentResult == null)
                    throw new NopException("processPaymentResult is not available");

                foreach (var item in details.Cart)
                {

                    if (item.IsPickup)
                    {
                        var productCategories = (await _categoryService.GetProductCategoriesByProductIdAsync(item.ProductId)).Select(x => x.CategoryId).ToList();
                        var slotDetails = await _slotService.GetPickupSlotById(item.SlotId);
                        var product = await _productService.GetProductByIdAsync(item.ProductId);
                        var vendor = await _vendorService.GetVendorByIdAsync(product.VendorId);
                        if (!await _slotService.CheckSlotAvailability(slotDetails.Id, slotDetails.Start, slotDetails.End, slotDetails.Capacity, false) || await _slotService.FindPickupSlotCategoryIdExist(item.SlotId, productCategories))
                        {
                            throw new NopException(string.Format(await _localizationService.GetResourceAsync("Alchub.Product.Slot.UnAvailable.Some.Products"), product != null ? product.Name : "", vendor != null ? vendor.Name : "", item.SlotStartTime.ToString("dd MM yyyy") + " " + item.SlotTime));
                        }
                    }
                    else
                    {
                        var productCategories = (await _categoryService.GetProductCategoriesByProductIdAsync(item.ProductId)).Select(x => x.CategoryId).ToList();
                        var slotDetails = await _slotService.GetSlotById(item.SlotId);
                        var product = await _productService.GetProductByIdAsync(item.ProductId);
                        var vendor = await _vendorService.GetVendorByIdAsync(product.VendorId);
                        if (!await _slotService.CheckSlotAvailability(slotDetails.Id, slotDetails.Start, slotDetails.End, slotDetails.Capacity, false) || await _slotService.FindSlotCategoryIdExist(item.SlotId, productCategories))
                        {
                            throw new NopException(string.Format(await _localizationService.GetResourceAsync("Alchub.Product.Slot.UnAvailable.Some.Products"), product != null ? product.Name : "", vendor != null ? vendor.Name : "", item.SlotStartTime.ToString("dd MM yyyy") + " " + item.SlotTime));
                        }
                    }

                }

                if (processPaymentResult.Success)
                {
                    var order = await SaveOrderDetailsAsync(processPaymentRequest, processPaymentResult, details);
                    result.PlacedOrder = order;

                    /*Alchub Start*/
                    //Order delivery fee details save
                    await SaveOrderDeliveryFeeAsync(details, order);

                    //Order tip fee details save
                    await SaveOrderTipFeeAsync(details, order);
                    /*Alchub End*/

                    //move shopping cart items to order items
                    await MoveShoppingCartItemsToOrderItemsAsync(details, order);

                    //discount usage history
                    await SaveDiscountUsageHistoryAsync(details, order);

                    //gift card usage history
                    await SaveGiftCardUsageHistoryAsync(details, order);

                    //recurring orders
                    if (details.IsRecurringShoppingCart)
                        await CreateFirstRecurringPaymentAsync(processPaymentRequest, order);

                    //++Alchub

                    //Note: Send Order place notifications when order gets paid.(8-12-22)

                    //order place notes, messages
                    await AddOrderNoteAsync(order, _workContext.OriginalCustomerIfImpersonated != null
                        ? $"Order placed by a store owner ('{_workContext.OriginalCustomerIfImpersonated.Email}'. ID = {_workContext.OriginalCustomerIfImpersonated.Id}) impersonating the customer."
                        : "Order placed");

                    //notifications
                    //await SendNotificationsAndSaveNotesAsync(order);

                    //--Alchub

                    //reset checkout data
                    await _customerService.ResetCheckoutDataAsync(details.Customer, processPaymentRequest.StoreId, clearCouponCodes: true, clearCheckoutAttributes: true);
                    await _customerActivityService.InsertActivityAsync("PublicStore.PlaceOrder",
                        string.Format(await _localizationService.GetResourceAsync("ActivityLog.PublicStore.PlaceOrder"), order.Id), order);

                    //check order status
                    await CheckOrderStatusAsync(order);

                    //raise event       
                    await _eventPublisher.PublishAsync(new OrderPlacedEvent(order));

                    if (order.PaymentStatus == PaymentStatus.Paid)
                        await ProcessOrderPaidAsync(order);
                }
                else
                    foreach (var paymentError in processPaymentResult.Errors)
                        result.AddError(string.Format(await _localizationService.GetResourceAsync("Checkout.PaymentError"), paymentError));
            }
            catch (Exception exc)
            {
                await _logger.ErrorAsync(exc.Message, exc);
                result.AddError(exc.Message);
            }

            if (result.Success)
                return result;

            //log errors
            var logError = result.Errors.Aggregate("Error while placing order. ",
                (current, next) => $"{current}Error {result.Errors.IndexOf(next) + 1}: {next}. ");
            var customer = await _customerService.GetCustomerByIdAsync(processPaymentRequest.CustomerId);
            await _logger.ErrorAsync(logError, customer: customer);

            return result;
        }

        /// <summary>
        /// Update order totals
        /// </summary>
        /// <param name="updateOrderParameters">Parameters for the updating order</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task UpdateOrderTotalsAsync(UpdateOrderParameters updateOrderParameters)
        {
            if (!_orderSettings.AutoUpdateOrderTotalsOnEditingOrder)
                return;

            var updatedOrder = updateOrderParameters.UpdatedOrder;
            var updatedOrderItem = updateOrderParameters.UpdatedOrderItem;

            //restore shopping cart from order items
            var (restoredCart, updatedShoppingCartItem) = await restoreShoppingCartAsync(updatedOrder, updatedOrderItem.Id);

            var itemDeleted = updatedShoppingCartItem is null;

            //validate shopping cart for warnings
            updateOrderParameters.Warnings.AddRange(await _shoppingCartService.GetShoppingCartWarningsAsync(restoredCart, string.Empty, false));

            var customer = await _customerService.GetCustomerByIdAsync(updatedOrder.CustomerId);

            if (!itemDeleted)
            {
                var product = await _productService.GetProductByIdAsync(updatedShoppingCartItem.ProductId);

                updateOrderParameters.Warnings.AddRange(await _shoppingCartService.GetShoppingCartItemWarningsAsync(customer, updatedShoppingCartItem.ShoppingCartType,
                    product, updatedOrder.StoreId, updatedShoppingCartItem.AttributesXml, updatedShoppingCartItem.CustomerEnteredPrice,
                    updatedShoppingCartItem.RentalStartDateUtc, updatedShoppingCartItem.RentalEndDateUtc, updatedShoppingCartItem.Quantity, false, updatedShoppingCartItem.Id));

                updatedOrderItem.ItemWeight = await _shippingService.GetShoppingCartItemWeightAsync(updatedShoppingCartItem);
                updatedOrderItem.OriginalProductCost = await _priceCalculationService.GetProductCostAsync(product, updatedShoppingCartItem.AttributesXml);
                updatedOrderItem.AttributeDescription = await _productAttributeFormatter.FormatAttributesAsync(product,
                    updatedShoppingCartItem.AttributesXml, customer);

                //++Alchub
                updatedOrderItem.CustomAttributesDescription = await _productAttributeFormatter.FormatCustomAttributesAsync(updatedShoppingCartItem.CustomAttributesXml);

                //gift cards
                await AddGiftCardsAsync(product, updatedShoppingCartItem.AttributesXml, updatedShoppingCartItem.Quantity, updatedOrderItem, updatedOrderItem.UnitPriceExclTax);
            }

            await _orderTotalCalculationService.UpdateOrderTotalsAsync(updateOrderParameters, restoredCart);

            if (updateOrderParameters.PickupPoint != null)
            {
                updatedOrder.PickupInStore = true;

                var pickupAddress = new Address
                {
                    Address1 = updateOrderParameters.PickupPoint.Address,
                    City = updateOrderParameters.PickupPoint.City,
                    County = updateOrderParameters.PickupPoint.County,
                    CountryId = (await _countryService.GetCountryByTwoLetterIsoCodeAsync(updateOrderParameters.PickupPoint.CountryCode))?.Id,
                    ZipPostalCode = updateOrderParameters.PickupPoint.ZipPostalCode,
                    CreatedOnUtc = DateTime.UtcNow
                };

                await _addressService.InsertAddressAsync(pickupAddress);

                updatedOrder.PickupAddressId = pickupAddress.Id;
                var shippingMethod = !string.IsNullOrEmpty(updateOrderParameters.PickupPoint.Name) ?
                    string.Format(await _localizationService.GetResourceAsync("Checkout.PickupPoints.Name"), updateOrderParameters.PickupPoint.Name) :
                    await _localizationService.GetResourceAsync("Checkout.PickupPoints.NullName");
                updatedOrder.ShippingMethod = shippingMethod;
                updatedOrder.ShippingRateComputationMethodSystemName = updateOrderParameters.PickupPoint.ProviderSystemName;
            }

            await _orderService.UpdateOrderAsync(updatedOrder);

            //discount usage history
            var discountUsageHistoryForOrder = await _discountService.GetAllDiscountUsageHistoryAsync(null, customer.Id, updatedOrder.Id);
            foreach (var discount in updateOrderParameters.AppliedDiscounts)
            {
                if (discountUsageHistoryForOrder.Any(history => history.DiscountId == discount.Id))
                    continue;

                var d = await _discountService.GetDiscountByIdAsync(discount.Id);
                if (d != null)
                {
                    await _discountService.InsertDiscountUsageHistoryAsync(new DiscountUsageHistory
                    {
                        DiscountId = d.Id,
                        OrderId = updatedOrder.Id,
                        CreatedOnUtc = DateTime.UtcNow
                    });
                }
            }

            await CheckOrderStatusAsync(updatedOrder);

            async Task<(List<ShoppingCartItem> restoredCart, ShoppingCartItem updatedShoppingCartItem)> restoreShoppingCartAsync(Order order, int updatedOrderItemId)
            {
                if (order is null)
                    throw new ArgumentNullException(nameof(order));

                var cart = (await _orderService.GetOrderItemsAsync(order.Id)).Select(item => new ShoppingCartItem
                {
                    Id = item.Id,
                    AttributesXml = item.AttributesXml,
                    CustomerId = order.CustomerId,
                    ProductId = item.ProductId,
                    Quantity = item.Id == updatedOrderItemId ? updateOrderParameters.Quantity : item.Quantity,
                    RentalEndDateUtc = item.RentalEndDateUtc,
                    RentalStartDateUtc = item.RentalStartDateUtc,
                    ShoppingCartType = ShoppingCartType.ShoppingCart,
                    StoreId = order.StoreId
                }).ToList();

                //get shopping cart item which has been updated
                var cartItem = cart.FirstOrDefault(shoppingCartItem => shoppingCartItem.Id == updatedOrderItemId);

                return (cart, cartItem);
            }
        }

        #endregion

        #region Order Delivered / Cancel and Delivery denied

        /// <summary>
        ///  order status changes 
        /// </summary>
        /// <param name="order">Order</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task OrderStatusCancelledChangedAsync(Order order)
        {
            //add a note
            await AddOrderNoteAsync(order, $"Order status has been edited. New status: {await _localizationService.GetLocalizedEnumAsync(order.OrderStatus)}");
        }

        /// <summary>
        ///  order status changes 
        /// </summary>
        /// <param name="order">Order</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task OrderStatusCompleteChangedAsync(Order order)
        {
            try
            {
                //update order
                await MarkOrderAsPaidAsync(order);
            }
            catch (Exception exc)
            {
                await _logger.ErrorAsync(string.Format("Order Update: Error while mark as paid order Order Id: {0}", order.Id));
            }

            var allOrderItems = (await _orderService.GetOrderItemsAsync(order.Id));
            allOrderItems = allOrderItems.Where(x => x.OrderItemStatusId != (int)OrderItemStatus.DeliveryDenied && x.OrderItemStatusId != (int)OrderItemStatus.Cancelled).ToList();
            foreach (var item in allOrderItems)
            {
                item.OrderItemStatusId = (int)OrderItemStatus.Delivered;
                await _orderService.UpdateOrderItemAsync(item);
            }

            await AddOrderNoteAsync(order, $"Order status has been edited. New status: {await _localizationService.GetLocalizedEnumAsync(order.OrderStatus)}");
        }


        /// <summary>
        ///  order Item Delivered 
        /// </summary>
        /// <param name="order">Order</param>
        /// <param name="notifyCustomer">True to notify customer</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task OrderItemDeliveredAsync(Order order, OrderItem orderItem)
        {
            //mark order items as delivered
            var slotOrderItems = (await _orderService.GetOrderItemsAsync(order.Id)).Where(o => o.SlotId == orderItem.SlotId && o.SlotTime.Equals(orderItem.SlotTime) && o.OrderItemStatus != OrderItemStatus.Delivered && o.OrderItemStatus != OrderItemStatus.Cancelled).ToList();
            if (slotOrderItems.Any())
            {
                //is pickup or delivery?
                var isPickup = false;

                foreach (var item in slotOrderItems)
                {
                    //set pickup value
                    isPickup = item.InPickup;

                    //marke each item as delivered
                    item.OrderItemStatus = OrderItemStatus.Delivered;
                    await _orderService.UpdateOrderItemAsync(item);
                }

                if (isPickup)
                    //add a note - pickup completed
                    await AddOrderNoteAsync(order, $"Order items(Ids: {string.Join(", ", slotOrderItems.Select(x => x.Id))}) has been marked as pickup completed by the user: {(await _workContext.GetCurrentCustomerAsync()).Email}");
                else
                    //add a note - delivered
                    await AddOrderNoteAsync(order, $"Order items(Ids: {string.Join(", ", slotOrderItems.Select(x => x.Id))}) has been marked as delivered by the user: {(await _workContext.GetCurrentCustomerAsync()).Email}");

                //set base order status

                var allOrderItems = (await _orderService.GetOrderItemsAsync(order.Id));
                allOrderItems = allOrderItems.Where(x => x.OrderItemStatusId != (int)OrderItemStatus.DeliveryDenied && x.OrderItemStatusId != (int)OrderItemStatus.Cancelled).ToList();
                if (allOrderItems.Count > 0 && allOrderItems.All(x => x.OrderItemStatus == OrderItemStatus.Delivered))
                {
                    //update order status to complte 
                    order.OrderStatusId = (int)OrderStatus.Complete;
                    //set delivered
                    order.ShippingStatusId = (int)ShippingStatus.Delivered;
                    await _orderService.UpdateOrderAsync(order);

                    //add a note
                    await AddOrderNoteAsync(order, $"Order status has been edited. New status: {await _localizationService.GetLocalizedEnumAsync(order.OrderStatus)}");
                }

                //send email to customer.
                if (isPickup)
                    await SendItemPickupCompletedNotificationsAndSaveNotesAsync(order, slotOrderItems);
                else
                    await SendItemDeliveredNotificationsAndSaveNotesAsync(order, slotOrderItems);

                //send SMS to customer - pending.

                //page notification

            }

        }



        /// <summary>
        ///  order Item Cancels 
        /// </summary>
        /// <param name="order">Order</param>
        /// <param name="notifyCustomer">True to notify customer</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task OrderItemCancelOrderAsync(Order order, OrderItem item)
        {
            var allOrderItems = (await _orderService.GetOrderItemsAsync(order.Id));
            var product = await _productService.GetProductByIdAsync(item.ProductId);
            var selectedVendorProductOrderItemsId = product != null ? product.VendorId : 0;
            var vendor = await _vendorService.GetVendorByIdAsync(selectedVendorProductOrderItemsId);
            var vendorOrderItems = new List<OrderItem>();
            bool lastVendorProductItem = false;
            foreach (var orderitem in allOrderItems)
            {
                var productVendor = await _productService.GetProductByIdAsync(orderitem.ProductId);
                if (productVendor.VendorId == selectedVendorProductOrderItemsId)
                {
                    vendorOrderItems.Add(orderitem);
                }
            }
            if (vendorOrderItems.Count(x => x.OrderItemStatusId == (int)OrderItemStatus.Cancelled) == vendorOrderItems.Count() - 1)
            {

                lastVendorProductItem = true;
            }
            else
            {
                lastVendorProductItem = false;
            }

            //marke each item as Cancelled
            item.OrderItemStatus = OrderItemStatus.Cancelled;
            await _orderService.UpdateOrderItemAsync(item);

            //add a note
            await AddOrderNoteAsync(order, $"Order Item status has been edited. New status: {await _localizationService.GetLocalizedEnumAsync(item.OrderItemStatus)}");

            #region refund amount order item

            //order item refund  amount
            var orderItemRefund = new OrderItemRefund();

            orderItemRefund.OrderId = order.Id;
            orderItemRefund.OrderItemId = item.Id;
            orderItemRefund.IsRefunded = false;
            orderItemRefund.CreatedOnUtc = DateTime.UtcNow;
            orderItemRefund.VendorId = selectedVendorProductOrderItemsId;
            orderItemRefund.PriceIncltax = item.PriceInclTax;
            if (lastVendorProductItem)
            {
                orderItemRefund.DeliveryFee = await _priceCalculationService.RoundPriceAsync(CalculateOrderItemFee(vendorOrderItems.Count(), allOrderItems.Count, order.DeliveryFee));
                orderItemRefund.SlotFee = await _priceCalculationService.RoundPriceAsync(CalculateOrderItemFee(vendorOrderItems.Count(), allOrderItems.Count, order.SlotFee));
                orderItemRefund.ServiceFee = await _priceCalculationService.RoundPriceAsync(CalculateOrderItemFee(vendorOrderItems.Count(), allOrderItems.Count, order.ServiceFee));
                orderItemRefund.TipFee = await _priceCalculationService.RoundPriceAsync(CalculateOrderItemFee(vendorOrderItems.Count(), allOrderItems.Count, order.TipFee));

            }
            orderItemRefund.TaxFee = await _priceCalculationService.RoundPriceAsync(item.PriceInclTax - item.PriceExclTax);
            orderItemRefund.TotalAmount = await _priceCalculationService.RoundPriceAsync(orderItemRefund.PriceIncltax + orderItemRefund.ServiceFee + orderItemRefund.DeliveryFee + orderItemRefund.SlotFee + orderItemRefund.TipFee);

            //add a note - dispatched
            await AddOrderNoteAsync(order, $"Order items(Id: {string.Join(", ", item.Id)}) has been marked as cancel by the user: {(await _workContext.GetCurrentCustomerAsync()).Email}");

            //Insert information refund amount
            await _orderItemRefundService.InsertOrderItemRefundAsync(orderItemRefund);

            //order note
            var sb = new StringBuilder();
            sb.AppendLine("Order Item Cancel Refund amount Item Price: " + orderItemRefund.PriceIncltax);
            sb.AppendLine("Order Item Cancel Refund amount Item Tax Fee: " + orderItemRefund.TaxFee);
            sb.AppendLine("Order Item Cancel Refund amount Item Service Fee: " + orderItemRefund.ServiceFee);
            sb.AppendLine("Order Item Cancel Refund amount Item Delivery Fee: " + orderItemRefund.DeliveryFee);
            sb.AppendLine("Order Item Cancel Refund amount Item Slot Fee: " + orderItemRefund.SlotFee);
            sb.AppendLine("Order Item Cancel Refund amount Item Tip Fee: " + orderItemRefund.TipFee);

            await _orderService.InsertOrderNoteAsync(new OrderNote
            {
                OrderId = order.Id,
                Note = Convert.ToString(sb),
                DisplayToCustomer = false,
                CreatedOnUtc = DateTime.UtcNow
            });

            // add a note - refund amount
            await AddOrderNoteAsync(order, $"Order Item Cancel Total Refund amount:- {(orderItemRefund.TotalAmount)}");
            #endregion

            #region Adjust inventory

            //get product
            await _productService.AdjustInventoryAsync(product, item.Quantity, item.AttributesXml, $"Order items(Ids: {string.Join(", ", item.Id)}) has been marked as cancel: {(await _workContext.GetCurrentCustomerAsync()).Email}");


            #endregion

            //send email customer
            await SendOrderItemCancelNotificationsCustomerAndSaveNotesAsync(order, item, orderItemRefund.TotalAmount);
            //send email vendor
            await SendOrderItemCancelNotificationsVendorAndSaveNotesAsync(order, item, vendor);

            //set base order status
            var items = (await _orderService.GetOrderItemsAsync(order.Id));
            var alltemscount = items.Count();
            var deniedCount = items.Count(x => x.OrderItemStatusId == (int)OrderItemStatus.DeliveryDenied);
            var cancelCount = items.Count(x => x.OrderItemStatusId == (int)OrderItemStatus.Cancelled);

            if (alltemscount == (deniedCount + cancelCount))
            {
                await CancelOrderAsync(order, true);
            }

            //set base order status
            var newitems = (await _orderService.GetOrderItemsAsync(order.Id));
            newitems = newitems.Where(x => x.OrderItemStatusId != (int)OrderItemStatus.DeliveryDenied && x.OrderItemStatusId != (int)OrderItemStatus.Cancelled).ToList();
            if (newitems.Count > 0 && newitems.All(x => x.OrderItemStatus == OrderItemStatus.Delivered))
            {
                //update order status to complte 
                order.OrderStatusId = (int)OrderStatus.Complete;
                //set delivered
                order.ShippingStatusId = (int)ShippingStatus.Delivered;
                await _orderService.UpdateOrderAsync(order);

                //add a note
                await AddOrderNoteAsync(order, $"Order status has been edited. New status: {await _localizationService.GetLocalizedEnumAsync(order.OrderStatus)}");

            }

        }

        /// <summary>
        ///  OrderItem Delivery Denied
        /// </summary>
        /// <param name="order"></param>
        /// <param name="orderItem"></param>
        /// <param name="addOrderNoteMessage"></param>
        /// <returns></returns>
        public virtual async Task OrderItemDeliveryDeniedOrderAsync(Order order, OrderItem orderItem, string addOrderNoteMessage)
        {
            var product = await _productService.GetProductByIdAsync(orderItem.ProductId);
            var selectedVendorProductOrderItemsId = product != null ? product.VendorId : 0;
            var vendor = await _vendorService.GetVendorByIdAsync(selectedVendorProductOrderItemsId);
            bool lastVendorProductItem = false;
            //mark order items as delivered
            var slotOrderItems = (await _orderService.GetOrderItemsAsync(order.Id)).Where(o => o.SlotId == orderItem.SlotId && o.SlotTime.Equals(orderItem.SlotTime) && o.OrderItemStatusId != (int)OrderItemStatus.Cancelled).ToList();
            if (slotOrderItems.Any())
            {
                foreach (var item in slotOrderItems)
                {
                    var allOrderItems = (await _orderService.GetOrderItemsAsync(order.Id));
                    var vendorOrderItems = new List<OrderItem>();
                    foreach (var orderitem in allOrderItems)
                    {
                        var productVendor = await _productService.GetProductByIdAsync(orderitem.ProductId);
                        if (productVendor.VendorId == selectedVendorProductOrderItemsId)
                        {
                            vendorOrderItems.Add(orderitem);
                        }
                    }
                    if (vendorOrderItems.Count(x => x.OrderItemStatus == OrderItemStatus.DeliveryDenied) == vendorOrderItems.Count() - 1)
                    {

                        lastVendorProductItem = true;
                    }
                    else
                    {
                        lastVendorProductItem = false;
                    }


                    //marke each item as Delivery Denied
                    item.OrderItemStatus = OrderItemStatus.DeliveryDenied;
                    await _orderService.UpdateOrderItemAsync(item);

                    if (!string.IsNullOrEmpty(addOrderNoteMessage))
                    {
                        // add a note
                        await AddOrderNoteAsync(order, $"Order Item Delivery Denied Reason:- {addOrderNoteMessage}");
                    }

                    //add a note
                    await AddOrderNoteAsync(order, $"Order Item status has been edited. New status: {await _localizationService.GetLocalizedEnumAsync(item.OrderItemStatus)}");

                    #region refund amount order item

                    //order item refund  amount
                    var orderItemRefund = new OrderItemRefund();

                    orderItemRefund.OrderId = order.Id;
                    orderItemRefund.IsRefunded = false;
                    orderItemRefund.CreatedOnUtc = DateTime.UtcNow;
                    orderItemRefund.PriceIncltax = item.PriceInclTax;
                    if (lastVendorProductItem)
                    {
                        //orderItemRefund.DeliveryFee = await _priceCalculationService.RoundPriceAsync(CalculateOrderItemFee(vendorOrderItems.Count(), allOrderItems.Count, order.DeliveryFee));
                        orderItemRefund.SlotFee = await _priceCalculationService.RoundPriceAsync(CalculateOrderItemFee(vendorOrderItems.Count(), allOrderItems.Count, order.SlotFee));
                        orderItemRefund.ServiceFee = await _priceCalculationService.RoundPriceAsync(CalculateOrderItemFee(vendorOrderItems.Count(), allOrderItems.Count, order.ServiceFee));
                        orderItemRefund.TipFee = await _priceCalculationService.RoundPriceAsync(CalculateOrderItemFee(vendorOrderItems.Count(), allOrderItems.Count, order.TipFee));
                    }
                    orderItemRefund.OrderItemId = item.Id;
                    orderItemRefund.VendorId = selectedVendorProductOrderItemsId;
                    orderItemRefund.TaxFee = await _priceCalculationService.RoundPriceAsync(item.PriceInclTax - item.PriceExclTax);
                    orderItemRefund.TotalAmount = await _priceCalculationService.RoundPriceAsync(orderItemRefund.PriceIncltax + orderItemRefund.ServiceFee + orderItemRefund.DeliveryFee + orderItemRefund.SlotFee + orderItemRefund.TipFee);

                    //Insert information refund amount
                    await _orderItemRefundService.InsertOrderItemRefundAsync(orderItemRefund);

                    //add a note - dispatched
                    await AddOrderNoteAsync(order, $"Order item(Id: {string.Join(", ", orderItem.Id)}) has been marked as delivery denied by the user: {(await _workContext.GetCurrentCustomerAsync()).Email}");

                    //order note
                    var sb = new StringBuilder();
                    sb.AppendLine("Order Item Delivery Denied Refund amount Item Price: " + orderItemRefund.PriceIncltax);
                    sb.AppendLine("Order Item Delivery Denied Refund amount Item Tax Fee: " + orderItemRefund.TaxFee);
                    sb.AppendLine("Order Item Delivery Denied Refund amount Item Service Fee: " + orderItemRefund.ServiceFee);
                    sb.AppendLine("Order Item Delivery Denied Refund amount Item Delivery Fee: " + orderItemRefund.DeliveryFee);
                    sb.AppendLine("Order Item Delivery Denied Refund amount Item Slot Fee: " + orderItemRefund.SlotFee);
                    sb.AppendLine("Order Item Delivery Denied Refund amount Item Tip Fee: " + orderItemRefund.TipFee);

                    await _orderService.InsertOrderNoteAsync(new OrderNote
                    {
                        OrderId = order.Id,
                        Note = Convert.ToString(sb),
                        DisplayToCustomer = false,
                        CreatedOnUtc = DateTime.UtcNow
                    });

                    // add a note - refund amount
                    await AddOrderNoteAsync(order, $"Order Item Cancel Total Refund amount:- {(orderItemRefund.TotalAmount)}");
                    #endregion

                    #region Adjust inventory

                    //get product

                    await _productService.AdjustInventoryAsync(product, orderItem.Quantity, orderItem.AttributesXml, $"Order items(Ids: {string.Join(", ", orderItem.Id)}) has been marked as delivery denied by the user: {(await _workContext.GetCurrentCustomerAsync()).Email}");


                    #endregion
                }

                if (slotOrderItems.Count > 0)
                {
                    //send email customer
                    await SendOrderItemDeliveryDeniedNotificationsCustomerAndSaveNotesAsync(order, slotOrderItems, orderItem);
                    //send email vendor
                    await SendOrderItemDeliveryDeniedNotificationsVendorAndSaveNotesAsync(order, slotOrderItems, vendor);
                }

                //set base order status
                var items = (await _orderService.GetOrderItemsAsync(order.Id));

                var alltemscount = items.Count();
                var deniedCount = items.Count(x => x.OrderItemStatusId == (int)OrderItemStatus.DeliveryDenied);
                var cancelCount = items.Count(x => x.OrderItemStatusId == (int)OrderItemStatus.Cancelled);

                if (alltemscount == (deniedCount + cancelCount))
                {
                    await CancelOrderAsync(order, true);
                }

                //set base order status
                var newitems = (await _orderService.GetOrderItemsAsync(order.Id));
                newitems = newitems.Where(x => x.OrderItemStatusId != (int)OrderItemStatus.DeliveryDenied && x.OrderItemStatusId != (int)OrderItemStatus.Cancelled).ToList();
                if (items.Count > 0 && items.All(x => x.OrderItemStatus == OrderItemStatus.Delivered))
                {
                    //update order status to complte 
                    order.OrderStatusId = (int)OrderStatus.Complete;
                    //set delivered
                    order.ShippingStatusId = (int)ShippingStatus.Delivered;
                    await _orderService.UpdateOrderAsync(order);

                    //add a note
                    await AddOrderNoteAsync(order, $"Order status has been edited. New status: {await _localizationService.GetLocalizedEnumAsync(order.OrderStatus)}");
                }


            }
        }

        protected decimal CalculateOrderItemFee(int vendorOrderItemCount, int count, decimal fee)
        {
            return (vendorOrderItemCount * fee) / count;
        }

        #endregion

        #region Message Template
        /// <summary>
        /// Send "order item delivered" notifications and save order notes
        /// </summary>
        /// <param name="order">Order</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        protected virtual async Task SendItemDeliveredNotificationsAndSaveNotesAsync(Order order, IList<OrderItem> orderItems)
        {
            //send email notification
            var queuedEmailIds = await _workflowMessageService.SendOrderItemsDeliveredCustomerNotificationAsync(order, orderItems, order.CustomerLanguageId);
            if (queuedEmailIds.Any())
                await AddOrderNoteAsync(order, $"\"Order Items Delivered\" email (to customer) has been queued. Queued email identifiers: {string.Join(", ", queuedEmailIds)}.");

            //send SMS notification
            await _twillioService.SendOrderItemsStatusUpdatedCustomerSMSAsync(order, orderItems, order.CustomerLanguageId, OrderItemStatusActionType.Delivered);
        }

        /// <summary>
        /// Send "order items pickup completed" notifications and save order notes
        /// </summary>
        /// <param name="order">Order</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        protected virtual async Task SendItemPickupCompletedNotificationsAndSaveNotesAsync(Order order, IList<OrderItem> orderItems)
        {
            //send email notification
            var queuedEmailIds = await _workflowMessageService.SendOrderItemsPickupCompletedCustomerNotificationAsync(order, orderItems, order.CustomerLanguageId);
            if (queuedEmailIds.Any())
                await AddOrderNoteAsync(order, $"\"Order Items Pickup completed\" email (to customer) has been queued. Queued email identifiers: {string.Join(", ", queuedEmailIds)}.");

            //send SMS notification
            await _twillioService.SendOrderItemsStatusUpdatedCustomerSMSAsync(order, orderItems, order.CustomerLanguageId, OrderItemStatusActionType.PickedUp);
        }

        protected virtual async Task SendOrderItemCancelNotificationsCustomerAndSaveNotesAsync(Order order, OrderItem orderItem, decimal orderItemRefundTotalAmount)
        {
            //send email notification
            var queuedEmailIds = await _workflowMessageService.SendOrderItemsCancelCustomerNotificationAsync(order, orderItem, orderItemRefundTotalAmount, order.CustomerLanguageId);
            if (queuedEmailIds.Any())
                await AddOrderNoteAsync(order, $"\"Order Item cancel \" email (to customer) has been queued. Queued email identifiers: {string.Join(", ", queuedEmailIds)}.");

            //send SMS notification
            await _twillioService.SendOrderItemCancelCustomerSMSAsync(order, orderItem, order.CustomerLanguageId);
        }

        protected virtual async Task SendOrderItemCancelNotificationsVendorAndSaveNotesAsync(Order order, OrderItem orderItems, Vendor vendor)
        {
            //send email notification
            var queuedEmailIds = await _workflowMessageService.SendOrderItemsCancelVendorNotificationAsync(order, orderItems, vendor, order.CustomerLanguageId);
            if (queuedEmailIds.Any())
                await AddOrderNoteAsync(order, $"\"Order Item cancel\" email (to customer) has been queued. Queued email identifiers: {string.Join(", ", queuedEmailIds)}.");


        }

        protected virtual async Task SendOrderItemDeliveryDeniedNotificationsCustomerAndSaveNotesAsync(Order order, IList<OrderItem> orderItems, OrderItem orderItem)
        {
            //send email notification
            var queuedEmailIds = await _workflowMessageService.SendOrderItemsDeliveryDeniedCustomerNotificationAsync(order, orderItems, order.CustomerLanguageId);
            if (queuedEmailIds.Any())
                await AddOrderNoteAsync(order, $"\"Order Item Delivery Denied\" email (to customer) has been queued. Queued email identifiers: {string.Join(", ", queuedEmailIds)}.");

            //send SMS notification
            await _twillioService.SendOrderItemDeliveryDeniedCustomerSMSAsync(order, orderItem, order.CustomerLanguageId);
        }

        protected virtual async Task SendOrderItemDeliveryDeniedNotificationsVendorAndSaveNotesAsync(Order order, IList<OrderItem> orderItems, Vendor vendor)
        {
            //send email notification
            var queuedEmailIds = await _workflowMessageService.SendOrderItemsDeliveryDeniedVendorNotificationAsync(order, orderItems, vendor, order.CustomerLanguageId);
            if (queuedEmailIds.Any())
                await AddOrderNoteAsync(order, $"\"Order Item Delivery Denied\" email (to customer) has been queued. Queued email identifiers: {string.Join(", ", queuedEmailIds)}.");

        }


        #endregion
    }
}