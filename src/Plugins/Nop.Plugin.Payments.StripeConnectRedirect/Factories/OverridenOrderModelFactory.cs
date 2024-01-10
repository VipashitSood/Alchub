using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.DeliveryFees;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Tax;
using Nop.Core.Domain.TipFees;
using Nop.Plugin.Payments.StripeConnectRedirect.Services;
using Nop.Services.Affiliates;
using Nop.Services.Catalog;
using Nop.Services.Common;
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
using Nop.Services.Stores;
using Nop.Services.Tax;
using Nop.Services.TipFees;
using Nop.Services.Vendors;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Models.Orders;

namespace Nop.Plugin.Payments.StripeConnectRedirect.Factories
{
    /// <summary>
    /// Represent overriden order model factory.
    /// </summary>
    public class OverridenOrderModelFactory : OrderModelFactory
    {
        #region Fields

        private readonly CurrencySettings _currencySettings;
        private readonly ICurrencyService _currencyService;
        private readonly IDiscountService _discountService;
        private readonly IGiftCardService _giftCardService;
        private readonly IOrderReportService _orderReportService;
        private readonly IOrderService _orderService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IRewardPointService _rewardPointService;
        private readonly IVendorService _vendorService;
        private readonly IWorkContext _workContext;
        private readonly OrderSettings _orderSettings;
        private readonly TaxSettings _taxSettings;
        private readonly IDeliveryFeeService _deliveryFeeService;
        private readonly ITipFeeService _tipFeeService;
        private readonly IOrderItemRefundService _orderItemRefundService;
        private readonly IStripeConnectRedirectService _stripeConnectRedirectService;

        #endregion

        #region Ctor

        public OverridenOrderModelFactory(AddressSettings addressSettings,
            CatalogSettings catalogSettings,
            CurrencySettings currencySettings,
            IActionContextAccessor actionContextAccessor,
            IAddressModelFactory addressModelFactory,
            IAddressService addressService,
            IAffiliateService affiliateService,
            IBaseAdminModelFactory baseAdminModelFactory,
            ICountryService countryService,
            ICurrencyService currencyService,
            ICustomerService customerService,
            IDateTimeHelper dateTimeHelper,
            IDiscountService discountService,
            IDownloadService downloadService,
            IEncryptionService encryptionService,
            IGiftCardService giftCardService,
            ILocalizationService localizationService,
            IMeasureService measureService,
            IOrderProcessingService orderProcessingService,
            IOrderReportService orderReportService,
            IOrderService orderService,
            IPaymentPluginManager paymentPluginManager,
            IPaymentService paymentService,
            IPictureService pictureService,
            IPriceCalculationService priceCalculationService,
            IPriceFormatter priceFormatter,
            IProductAttributeService productAttributeService,
            IProductService productService,
            IReturnRequestService returnRequestService,
            IRewardPointService rewardPointService,
            IShipmentService shipmentService,
            IShippingService shippingService,
            IStateProvinceService stateProvinceService,
            IStoreService storeService,
            ITaxService taxService,
            IUrlHelperFactory urlHelperFactory,
            IVendorService vendorService,
            IWorkContext workContext,
            MeasureSettings measureSettings,
            OrderSettings orderSettings,
            ShippingSettings shippingSettings,
            IUrlRecordService urlRecordService,
            TaxSettings taxSettings,
            IDeliveryFeeService deliveryFeeService,
            ITipFeeService tipFeeService,
            IOrderItemRefundService orderItemRefundService,
            IStripeConnectRedirectService stripeConnectRedirectService) : base(addressSettings,
                catalogSettings,
                currencySettings,
                actionContextAccessor,
                addressModelFactory,
                addressService,
                affiliateService,
                baseAdminModelFactory,
                countryService,
                currencyService,
                customerService,
                dateTimeHelper,
                discountService,
                downloadService,
                encryptionService,
                giftCardService,
                localizationService,
                measureService,
                orderProcessingService,
                orderReportService,
                orderService,
                paymentPluginManager,
                paymentService,
                pictureService,
                priceCalculationService,
                priceFormatter,
                productAttributeService,
                productService,
                returnRequestService,
                rewardPointService,
                shipmentService,
                shippingService,
                stateProvinceService,
                storeService,
                taxService,
                urlHelperFactory,
                vendorService,
                workContext,
                measureSettings,
                orderSettings,
                shippingSettings,
                urlRecordService,
                taxSettings,
                deliveryFeeService,
                tipFeeService,
                orderItemRefundService)
        {
            _currencySettings = currencySettings;
            _currencyService = currencyService;
            _discountService = discountService;
            _giftCardService = giftCardService;
            _orderReportService = orderReportService;
            _orderService = orderService;
            _priceFormatter = priceFormatter;
            _rewardPointService = rewardPointService;
            _vendorService = vendorService;
            _workContext = workContext;
            _orderSettings = orderSettings;
            _taxSettings = taxSettings;
            _deliveryFeeService = deliveryFeeService;
            _tipFeeService = tipFeeService;
            _orderItemRefundService = orderItemRefundService;
            _stripeConnectRedirectService = stripeConnectRedirectService;
        }

        #endregion

        #region Utilities
        
        /// <summary>
        /// Prepare order model totals
        /// </summary>
        /// <param name="model">Order model</param>
        /// <param name="order">Order</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        protected override async Task PrepareOrderModelTotalsAsync(OrderModel model, Order order)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            if (order == null)
                throw new ArgumentNullException(nameof(order));

            var primaryStoreCurrency = await _currencyService.GetCurrencyByIdAsync(_currencySettings.PrimaryStoreCurrencyId);
            var languageId = (await _workContext.GetWorkingLanguageAsync()).Id;

            //subtotal
            model.OrderSubtotalInclTax = await _priceFormatter
                .FormatOrderPriceAsync(order.OrderSubtotalInclTax, order.CurrencyRate, order.CustomerCurrencyCode,
                _orderSettings.DisplayCustomerCurrencyOnOrders, primaryStoreCurrency, languageId, true);
            model.OrderSubtotalExclTax = await _priceFormatter
                .FormatOrderPriceAsync(order.OrderSubtotalExclTax, order.CurrencyRate, order.CustomerCurrencyCode,
                _orderSettings.DisplayCustomerCurrencyOnOrders, primaryStoreCurrency, languageId, false);
            model.OrderSubtotalInclTaxValue = order.OrderSubtotalInclTax;
            model.OrderSubtotalExclTaxValue = order.OrderSubtotalExclTax;

            //discount (applied to order subtotal)
            var orderSubtotalDiscountInclTaxStr = await _priceFormatter
                .FormatOrderPriceAsync(order.OrderSubTotalDiscountInclTax, order.CurrencyRate, order.CustomerCurrencyCode,
                _orderSettings.DisplayCustomerCurrencyOnOrders, primaryStoreCurrency, languageId, true);
            var orderSubtotalDiscountExclTaxStr = await _priceFormatter
                .FormatOrderPriceAsync(order.OrderSubTotalDiscountExclTax, order.CurrencyRate, order.CustomerCurrencyCode,
                _orderSettings.DisplayCustomerCurrencyOnOrders, primaryStoreCurrency, languageId, false);
            if (order.OrderSubTotalDiscountInclTax > decimal.Zero)
                model.OrderSubTotalDiscountInclTax = orderSubtotalDiscountInclTaxStr;
            if (order.OrderSubTotalDiscountExclTax > decimal.Zero)
                model.OrderSubTotalDiscountExclTax = orderSubtotalDiscountExclTaxStr;
            model.OrderSubTotalDiscountInclTaxValue = order.OrderSubTotalDiscountInclTax;
            model.OrderSubTotalDiscountExclTaxValue = order.OrderSubTotalDiscountExclTax;

            //shipping
            model.OrderShippingInclTax = await _priceFormatter
                .FormatOrderPriceAsync(order.OrderShippingInclTax, order.CurrencyRate, order.CustomerCurrencyCode,
                _orderSettings.DisplayCustomerCurrencyOnOrders, primaryStoreCurrency, languageId, true,
                _taxSettings.ShippingIsTaxable && _taxSettings.DisplayTaxSuffix);
            model.OrderShippingExclTax = await _priceFormatter
                .FormatOrderPriceAsync(order.OrderShippingExclTax, order.CurrencyRate, order.CustomerCurrencyCode,
                _orderSettings.DisplayCustomerCurrencyOnOrders, primaryStoreCurrency, languageId, false,
                _taxSettings.ShippingIsTaxable && _taxSettings.DisplayTaxSuffix);
            model.OrderShippingInclTaxValue = order.OrderShippingInclTax;
            model.OrderShippingExclTaxValue = order.OrderShippingExclTax;

            //payment method additional fee
            if (order.PaymentMethodAdditionalFeeInclTax > decimal.Zero)
            {
                model.PaymentMethodAdditionalFeeInclTax = await _priceFormatter
                    .FormatOrderPriceAsync(order.PaymentMethodAdditionalFeeInclTax, order.CurrencyRate, order.CustomerCurrencyCode,
                    _orderSettings.DisplayCustomerCurrencyOnOrders, primaryStoreCurrency, languageId, true,
                    _taxSettings.PaymentMethodAdditionalFeeIsTaxable && _taxSettings.DisplayTaxSuffix);
                model.PaymentMethodAdditionalFeeExclTax = await _priceFormatter
                    .FormatOrderPriceAsync(order.PaymentMethodAdditionalFeeExclTax, order.CurrencyRate, order.CustomerCurrencyCode,
                    _orderSettings.DisplayCustomerCurrencyOnOrders, primaryStoreCurrency, languageId, false,
                    _taxSettings.PaymentMethodAdditionalFeeIsTaxable && _taxSettings.DisplayTaxSuffix);
            }

            model.PaymentMethodAdditionalFeeInclTaxValue = order.PaymentMethodAdditionalFeeInclTax;
            model.PaymentMethodAdditionalFeeExclTaxValue = order.PaymentMethodAdditionalFeeExclTax;

            //tax
            model.Tax = await _priceFormatter
                .FormatOrderPriceAsync(order.OrderTax, order.CurrencyRate, order.CustomerCurrencyCode,
                _orderSettings.DisplayCustomerCurrencyOnOrders, primaryStoreCurrency, languageId, null, false);
            var taxRates = _orderService.ParseTaxRates(order, order.TaxRates);
            var displayTaxRates = _taxSettings.DisplayTaxRates && taxRates.Any();
            var displayTax = !displayTaxRates;
            foreach (var tr in taxRates)
            {
                model.TaxRates.Add(new OrderModel.TaxRate
                {
                    Rate = _priceFormatter.FormatTaxRate(tr.Key),
                    Value = await _priceFormatter
                        .FormatOrderPriceAsync(tr.Value, order.CurrencyRate, order.CustomerCurrencyCode,
                        _orderSettings.DisplayCustomerCurrencyOnOrders, primaryStoreCurrency, languageId, null, false)
                });
            }

            model.DisplayTaxRates = displayTaxRates;
            model.DisplayTax = displayTax;
            model.TaxValue = order.OrderTax;
            model.TaxRatesValue = order.TaxRates;

            //service fee
            model.ServiceFee = await _priceFormatter.FormatOrderPriceAsync(order.ServiceFee, order.CurrencyRate, order.CustomerCurrencyCode,
                _orderSettings.DisplayCustomerCurrencyOnOrders, primaryStoreCurrency, languageId, null, false);

            model.SlotFee = await _priceFormatter.FormatOrderPriceAsync(order.SlotFee, order.CurrencyRate, order.CustomerCurrencyCode,
                _orderSettings.DisplayCustomerCurrencyOnOrders, primaryStoreCurrency, languageId, null, false);

            /*Alchub Start*/

            //Delivery Fee
            model.DeliveryFee = await _priceFormatter.FormatOrderPriceAsync(order.DeliveryFee, order.CurrencyRate, order.CustomerCurrencyCode,
                _orderSettings.DisplayCustomerCurrencyOnOrders, primaryStoreCurrency, languageId, null, false);

            var vendorWiseDeliveryFees = await _deliveryFeeService.GetOrderDeliveryFeesByOrderIdAsync(order.Id);

            if (vendorWiseDeliveryFees != null)
            {
                vendorWiseDeliveryFees.ToList().ForEach(async x =>
                {
                    var deliveryFeeValue = _currencyService.ConvertCurrency(x.DeliveryFee, order.CurrencyRate);
                    var vendor = await _vendorService.GetVendorByIdAsync(x.VendorId);
                    model.VendorWiseDeliveryFees.Add(
                        new VendorWiseDeliveryFee
                        {
                            VendorId = x.VendorId,
                            VendorName = vendor != null ? vendor.Name : "Admin",
                            DeliveryFeeValue = deliveryFeeValue,
                            DeliveryFee = await _priceFormatter.FormatPriceAsync(deliveryFeeValue, true, order.CustomerCurrencyCode, false, languageId)
                        });
                });
            }

            //Tip Fee
            model.TipFee = await _priceFormatter.FormatOrderPriceAsync(order.TipFee, order.CurrencyRate, order.CustomerCurrencyCode,
                _orderSettings.DisplayCustomerCurrencyOnOrders, primaryStoreCurrency, languageId, null, false);

            var vendorWiseTipFees = await _tipFeeService.GetVendorWiseOrderTipFeesByOrderIdAsync(order.Id);

            if (vendorWiseTipFees != null)
            {
                vendorWiseTipFees.ToList().ForEach(async x =>
                {
                    var tipFeeValue = _currencyService.ConvertCurrency(x.TipFeeValue, order.CurrencyRate);

                    model.VendorWiseTipFees.Add(
                        new VendorWiseTipFee
                        {
                            VendorId = x.VendorId,
                            VendorName = x.VendorName,
                            TipFeeValue = tipFeeValue,
                            TipFee = await _priceFormatter.FormatPriceAsync(tipFeeValue, true, order.CustomerCurrencyCode, false, languageId)
                        });
                });
            }


            decimal totalRefundAmount = (await _orderItemRefundService.GetOrderItemRefundByOrderIdAsync(order.Id))?.Sum(x => x.TotalAmount) ?? 0;
            model.TotalRefundAmount = await _priceFormatter.FormatPriceAsync(totalRefundAmount, true, false);

            //credit amount
            //get amount based on admin/vendor
            var amountDctionary = await _stripeConnectRedirectService.GetOrderCreditAmounts(order);
            var totalCreditAmount = decimal.Zero;
            if (await _workContext.GetCurrentVendorAsync() != null)
                //vendor
                amountDctionary.TryGetValue((await _workContext.GetCurrentVendorAsync()).Id, out totalCreditAmount);
            else
                //admin
                amountDctionary.TryGetValue(0, out totalCreditAmount);

            model.TotalCreditAmount = await _priceFormatter.FormatPriceAsync(totalCreditAmount, true, false);
            /*Alchub End*/

            //discount
            if (order.OrderDiscount > 0)
            {
                model.OrderTotalDiscount = await _priceFormatter
                    .FormatOrderPriceAsync(-order.OrderDiscount, order.CurrencyRate, order.CustomerCurrencyCode,
                    _orderSettings.DisplayCustomerCurrencyOnOrders, primaryStoreCurrency, languageId, null, false);
            }
            model.OrderTotalDiscountValue = order.OrderDiscount;

            //gift cards
            foreach (var gcuh in await _giftCardService.GetGiftCardUsageHistoryAsync(order))
            {
                model.GiftCards.Add(new OrderModel.GiftCard
                {
                    CouponCode = (await _giftCardService.GetGiftCardByIdAsync(gcuh.GiftCardId)).GiftCardCouponCode,
                    Amount = await _priceFormatter.FormatPriceAsync(-gcuh.UsedValue, true, false)
                });
            }

            //reward points
            if (order.RedeemedRewardPointsEntryId.HasValue && await _rewardPointService.GetRewardPointsHistoryEntryByIdAsync(order.RedeemedRewardPointsEntryId.Value) is RewardPointsHistory redeemedRewardPointsEntry)
            {
                model.RedeemedRewardPoints = -redeemedRewardPointsEntry.Points;
                model.RedeemedRewardPointsAmount =
                    await _priceFormatter.FormatPriceAsync(-redeemedRewardPointsEntry.UsedAmount, true, false);
            }

            //total
            model.OrderTotal = await _priceFormatter
                .FormatOrderPriceAsync(order.OrderTotal, order.CurrencyRate, order.CustomerCurrencyCode,
                _orderSettings.DisplayCustomerCurrencyOnOrders, primaryStoreCurrency, languageId, null, false);
            model.OrderTotalValue = order.OrderTotal;

            //refunded amount
            if (order.RefundedAmount > decimal.Zero)
                model.RefundedAmount = await _priceFormatter.FormatPriceAsync(order.RefundedAmount, true, false);

            //used discounts
            var duh = await _discountService.GetAllDiscountUsageHistoryAsync(orderId: order.Id);
            foreach (var d in duh)
            {
                var discount = await _discountService.GetDiscountByIdAsync(d.DiscountId);

                model.UsedDiscounts.Add(new OrderModel.UsedDiscountModel
                {
                    DiscountId = d.DiscountId,
                    DiscountName = discount.Name
                });
            }

            //profit (hide for vendors)
            if (await _workContext.GetCurrentVendorAsync() != null)
                return;

            var profit = await _orderReportService.ProfitReportAsync(orderId: order.Id);
            model.Profit = await _priceFormatter
                .FormatOrderPriceAsync(profit, order.CurrencyRate, order.CustomerCurrencyCode,
                _orderSettings.DisplayCustomerCurrencyOnOrders, primaryStoreCurrency, languageId, null, false);
        }

        #endregion
    }
}
