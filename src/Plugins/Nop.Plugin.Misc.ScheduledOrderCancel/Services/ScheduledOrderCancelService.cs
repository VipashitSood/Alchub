using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Tax;
using Nop.Core.Events;
using Nop.Services.Affiliates;
using Nop.Services.Alchub.ServiceFee;
using Nop.Services.Alchub.Twillio;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.DeliveryFees;
using Nop.Services.Directory;
using Nop.Services.Discounts;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Security;
using Nop.Services.Shipping;
using Nop.Services.Slots;
using Nop.Services.Tax;
using Nop.Services.TipFees;
using Nop.Services.Vendors;

namespace Nop.Plugin.Misc.ScheduledOrderCancel.Services
{
    /// <summary>
    /// Represents the plugin service implementation
    /// </summary>
    public class ScheduledOrderCancelService : OrderProcessingService
    {
        private readonly IEventPublisher _eventPublisher;
        private readonly IGiftCardService _giftCardService;
        private readonly ILocalizationService _localizationService;

        #region Fields

        private readonly ILogger _logger;
        private readonly IOrderService _orderService;
        private readonly IPdfService _pdfService;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly OrderSettings _orderSettings;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;
        private readonly CustomWorkflowMessageService _customWorkflowMessageService;

        public ScheduledOrderCancelService(
            CurrencySettings currencySettings,
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
            ITwillioService twillioService,
            IOrderItemRefundService orderItemRefundService,
            ISettingService settingService,
            IStoreContext storeContext,
            CustomWorkflowMessageService customWorkflowMessageService)
            : base(
                  currencySettings,
                  addressService,
                  affiliateService,
                  checkoutAttributeFormatter,
                  countryService,
                  currencyService,
                  customerActivityService,
                  customerService,
                  customNumberFormatter,
                  discountService,
                  encryptionService,
                  eventPublisher,
                  genericAttributeService,
                  giftCardService,
                  languageService,
                  localizationService,
                  logger,
                  orderService,
                  orderTotalCalculationService,
                  paymentPluginManager,
                  paymentService,
                  pdfService,
                  priceCalculationService,
                  priceFormatter,
                  productAttributeFormatter,
                  productAttributeParser,
                  productService,
                  returnRequestService,
                  rewardPointService,
                  shipmentService,
                  shippingService,
                  shoppingCartService,
                  stateProvinceService,
                  taxService,
                  vendorService,
                  webHelper,
                  workContext,
                  workflowMessageService,
                  localizationSettings,
                  orderSettings,
                  paymentSettings,
                  rewardPointsSettings,
                  shippingSettings,
                  taxSettings,
                  serviceFeeManager,
                  deliveryFeeService,
                  tipFeeService,
                  slotService,
                  categoryService,
                  twillioService,
                  orderItemRefundService)
        {
            _eventPublisher = eventPublisher;
            _giftCardService = giftCardService;
            _localizationService = localizationService;
            _logger = logger;
            _orderService = orderService;
            _pdfService = pdfService;
            _workflowMessageService = workflowMessageService;
            _orderSettings = orderSettings;
            _settingService = settingService;
            _storeContext = storeContext;
            _customWorkflowMessageService = customWorkflowMessageService;
        }

        #endregion

        #region Method

        /// <summary>
        /// Process order cancelation 
        /// </summary>
        /// <returns></returns>
        public virtual async Task ProcessOrderCancellationAsync()
        {
            var storeId = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var pluginSettings = await _settingService.LoadSettingAsync<ScheduledOrderCancelSettings>(storeId);
            //temp change
            var temp = 1;

            //check plauin enabled & time value
            if (pluginSettings.Enabled && pluginSettings.Time > 0)
            {
                var psids = new List<int> { (int)PaymentStatus.Pending };
                var osids = new List<int> { (int)OrderStatus.Pending };
                //note: get orders with stripe payment method
                var pendingOrders = await _orderService.SearchOrdersAsync(osIds: osids, psIds: psids, paymentMethodSystemName: "Payments.StripeConnectRedirect");

                var canceledOrders = new List<int>();
                foreach (var order in pendingOrders)
                {
                    TimeSpan ts = DateTime.UtcNow - order.CreatedOnUtc;
                    var minutepasses = ts.TotalMinutes;
                    if (minutepasses > pluginSettings.Time)
                    {
                        try
                        {
                            //cancel the order 
                            await CancelOrderCustomAsync(order, true);
                            canceledOrders.Add(order.Id);
                        }
                        catch (Exception ex)
                        {
                            await _logger.ErrorAsync($"Scheduled order cancel Error: Order #{order.Id}, error: {ex} ", ex);
                        }
                    }
                }

                //log canceld order information
                if (canceledOrders.Count > 0)
                    await _logger.InformationAsync($"Order cancel Scheduler has canceled {canceledOrders.Count} pending orders. OrderIds: {string.Join(",", canceledOrders.ToArray())}");
            }
        }

        /// <summary>
        /// Cancels order
        /// </summary>
        /// <param name="order">Order</param>
        /// <param name="notifyCustomer">True to notify customer</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        private async Task CancelOrderCustomAsync(Order order, bool notifyCustomer)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            if (!CanCancelOrder(order))
                throw new NopException("Cannot do cancel for order.");

            //cancel order - custom
            await SetOrderStatusCustomAsync(order, OrderStatus.Cancelled, notifyCustomer);

            //add a note - custom
            await AddOrderNoteAsync(order, "Order has been canceled, due to payment is failed to process.");

            //return (add) back redeemded reward points
            await ReturnBackRedeemedRewardPointsAsync(order);

            //delete gift card usage history
            if (_orderSettings.DeleteGiftCardUsageHistory)
                await _giftCardService.DeleteGiftCardUsageHistoryAsync(order);

            //cancel recurring payments
            var recurringPayments = await _orderService.SearchRecurringPaymentsAsync(initialOrderId: order.Id);
            foreach (var rp in recurringPayments)
                await CancelRecurringPaymentAsync(rp);

            //Adjust inventory for already shipped shipments
            //only products with "use multiple warehouses"
            await ReverseBookedInventoryAsync(order, string.Format(await _localizationService.GetResourceAsync("Admin.StockQuantityHistory.Messages.CancelOrder"), order.Id));

            //Adjust inventory
            await ReturnOrderStockAsync(order, string.Format(await _localizationService.GetResourceAsync("Admin.StockQuantityHistory.Messages.CancelOrder"), order.Id));

            await _eventPublisher.PublishAsync(new OrderCancelledEvent(order));
        }

        /// <summary>
        /// Sets an order status
        /// </summary>
        /// <param name="order">Order</param>
        /// <param name="os">New order status</param>
        /// <param name="notifyCustomer">True to notify customer</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        protected async Task SetOrderStatusCustomAsync(Order order, OrderStatus os, bool notifyCustomer)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            var prevOrderStatus = order.OrderStatus;
            if (prevOrderStatus == os)
                return;

            //set and save new order status
            order.OrderStatusId = (int)os;
            await _orderService.UpdateOrderAsync(order);

            //order notes, notifications
            await AddOrderNoteAsync(order, $"Order status has been changed to {await _localizationService.GetLocalizedEnumAsync(os)}");

            if (prevOrderStatus != OrderStatus.Cancelled &&
                os == OrderStatus.Cancelled
                && notifyCustomer)
            {
                //notification
                var orderCancelledCustomerNotificationQueuedEmailIds = await _customWorkflowMessageService.SendOrderCancelledCustomerCustomNotificationAsync(order, order.CustomerLanguageId);
                if (orderCancelledCustomerNotificationQueuedEmailIds.Any())
                    await AddOrderNoteAsync(order, $"\"Order cancelled\" due to payment failed to process email (to customer) has been queued. Queued email identifiers: {string.Join(", ", orderCancelledCustomerNotificationQueuedEmailIds)}.");
            }

            if (order.OrderStatus == OrderStatus.Cancelled)
                await ReduceRewardPointsAsync(order);

            //gift cards deactivation
            if (_orderSettings.DeactivateGiftCardsAfterCancellingOrder && order.OrderStatus == OrderStatus.Cancelled)
                await SetActivatedValueForPurchasedGiftCardsAsync(order, false);
        }

        #endregion
    }
}