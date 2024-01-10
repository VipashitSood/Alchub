using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Alchub.Domain.Orders;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Tax;
using Nop.Core.Events;
using Nop.Plugin.Payments.StripeConnectRedirect.Domain;
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
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Security;
using Nop.Services.Shipping;
using Nop.Services.Slots;
using Nop.Services.Tax;
using Nop.Services.TipFees;
using Nop.Services.Vendors;
using Stripe;

namespace Nop.Plugin.Payments.StripeConnectRedirect.Services
{
    /// <summary>
    /// prepare order processing service
    /// </summary>
    public partial class OverrideOrderProcessingService : OrderProcessingService
    {
        #region Fields


        private readonly IEventPublisher _eventPublisher;
        private readonly IGiftCardService _giftCardService;
        private readonly ILocalizationService _localizationService;
        private readonly IOrderService _orderService;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly IProductService _productService;
        private readonly IVendorService _vendorService;
        private readonly IWorkContext _workContext;
        private readonly OrderSettings _orderSettings;
        private readonly IOrderItemRefundService _orderItemRefundService;
        private readonly IStripeConnectRedirectService _stripeConnectRedirectService;
        private readonly ILogger _logger;
        private readonly IDeliveryFeeService _deliveryFeeService;

        #endregion

        #region Ctor

        public OverrideOrderProcessingService(CurrencySettings currencySettings,
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
            IStripeConnectRedirectService stripeConnectRedirectService) : base
            (
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
                 orderItemRefundService
           )
        {
            _eventPublisher = eventPublisher;
            _giftCardService = giftCardService;
            _localizationService = localizationService;
            _orderService = orderService;
            _priceCalculationService = priceCalculationService;
            _productService = productService;
            _vendorService = vendorService;
            _workContext = workContext;
            _orderSettings = orderSettings;
            _orderItemRefundService = orderItemRefundService;
            _stripeConnectRedirectService = stripeConnectRedirectService;
            _logger = logger;
            _deliveryFeeService = deliveryFeeService;
        }

        #endregion

        #region Order Cancel and Delivery denied

        /// <summary>
        ///  order status changes 
        /// </summary>
        /// <param name="order">Order</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public override async Task OrderStatusCancelledChangedAsync(Order order)
        {
            // Stripe capture payment 
            if (order.PaymentMethodSystemName == StripeConnectRedirectPaymentDefaults.SystemName)
            {
                await _stripeConnectRedirectService.ReleasePaymentAsync(order.Id);
            }
            //add a note
            await AddOrderNoteAsync(order, $"Order status has been edited. New status: {await _localizationService.GetLocalizedEnumAsync(order.OrderStatus)}");
        }

        /// <summary>
        ///  order status changes 
        /// </summary>
        /// <param name="order">Order</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public override async Task OrderStatusCompleteChangedAsync(Order order)
        {
            // Stripe capture payment 
            if (order.PaymentMethodSystemName == StripeConnectRedirectPaymentDefaults.SystemName)
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

                //capture 
                await _stripeConnectRedirectService.CapturePaymentAsync(order.Id);
            }
            //add a note
            await AddOrderNoteAsync(order, $"Order status has been edited. New status: {await _localizationService.GetLocalizedEnumAsync(order.OrderStatus)}");
        }


        /// <summary>
        /// Cancels order
        /// </summary>
        /// <param name="order">Order</param>
        /// <param name="notifyCustomer">True to notify customer</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public override async Task CancelOrderAsync(Order order, bool notifyCustomer)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            if (!CanCancelOrder(order))
                throw new NopException("Cannot do cancel for order.");

            //cancel order
            await SetOrderStatusAsync(order, OrderStatus.Cancelled, notifyCustomer);

            //add a note
            await AddOrderNoteAsync(order, "Order has been cancelled");

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

            // Stripe refund payment back to customer
            if (order.PaymentMethodSystemName == StripeConnectRedirectPaymentDefaults.SystemName)
            {
                await _stripeConnectRedirectService.ReleasePaymentAsync(order.Id);
            }
        }

        /// <summary>
        ///  order Item Delivered 
        /// </summary>
        /// <param name="order">Order</param>
        /// <param name="notifyCustomer">True to notify customer</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public override async Task OrderItemDeliveredAsync(Order order, OrderItem orderItem)
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
                var newItems = (await _orderService.GetOrderItemsAsync(order.Id));
                if (newItems != null && newItems.Count > 0 &&
                    newItems.All(x => x.OrderItemStatusId == (int)OrderItemStatus.DeliveryDenied
                    || x.OrderItemStatusId == (int)OrderItemStatus.Cancelled || x.OrderItemStatusId == (int)OrderItemStatus.Delivered))
                {
                    //update order status to complte 
                    order.OrderStatusId = (int)OrderStatus.Complete;
                    //set delivered
                    order.ShippingStatusId = (int)ShippingStatus.Delivered;
                    await _orderService.UpdateOrderAsync(order);

                    //add a note
                    await AddOrderNoteAsync(order, $"Order status has been edited. New status: {await _localizationService.GetLocalizedEnumAsync(order.OrderStatus)}");

                    // Stripe refund payment back to customer
                    if (order.PaymentMethodSystemName == StripeConnectRedirectPaymentDefaults.SystemName)
                    {
                        await _stripeConnectRedirectService.CapturePaymentAsync(order.Id);
                    }
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
        public override async Task OrderItemCancelOrderAsync(Order order, OrderItem item)
        {
            var product = await _productService.GetProductByIdAsync(item.ProductId);
            var vendorId = product != null ? product.VendorId : 0;
            var vendor = await _vendorService.GetVendorByIdAsync(vendorId);

            //marke each item as Cancelled
            item.OrderItemStatus = OrderItemStatus.Cancelled;
            await _orderService.UpdateOrderItemAsync(item);

            //add a note
            await AddOrderNoteAsync(order, $"Order Item status has been edited. New status: {await _localizationService.GetLocalizedEnumAsync(item.OrderItemStatus)}");

            #region refund amount order item

            var totalRefundAmount = await InsertOrderItemRefundAsync(order, item);

            //add a note - dispatched
            await AddOrderNoteAsync(order, $"Order items(Id: {string.Join(", ", item.Id)}) has been marked as cancel by the user: {(await _workContext.GetCurrentCustomerAsync()).Email}");

            // add a note - refund amount
            await AddOrderNoteAsync(order, $"Order Item Cancel Total Refund amount:- {totalRefundAmount}");

            #endregion

            #region Adjust inventory

            //get product
            await _productService.AdjustInventoryAsync(product, item.Quantity, item.AttributesXml, $"Order items(Ids: {string.Join(", ", item.Id)}) has been marked as cancel: {(await _workContext.GetCurrentCustomerAsync()).Email}");

            #endregion

            //send email customer
            await SendOrderItemCancelNotificationsCustomerAndSaveNotesAsync(order, item, totalRefundAmount);
            //send email vendor
            await SendOrderItemCancelNotificationsVendorAndSaveNotesAsync(order, item, vendor);

            //set base order status
            var items = (await _orderService.GetOrderItemsAsync(order.Id));
            if (items != null && items.All(x => x.OrderItemStatusId == (int)OrderItemStatus.Cancelled))
            {
                await CancelOrderAsync(order, true);
            }
            else if (items != null && items.All(x => x.OrderItemStatusId == (int)OrderItemStatus.DeliveryDenied
                || x.OrderItemStatusId == (int)OrderItemStatus.Cancelled || x.OrderItemStatusId == (int)OrderItemStatus.Delivered))
            {
                //update order status to complte 
                order.OrderStatusId = (int)OrderStatus.Complete;
                //set delivered
                order.ShippingStatusId = (int)ShippingStatus.Delivered;
                await _orderService.UpdateOrderAsync(order);

                //add a note
                await AddOrderNoteAsync(order, $"Order status has been edited. New status: {await _localizationService.GetLocalizedEnumAsync(order.OrderStatus)}");

                // Stripe refund payment back to customer
                if (order.PaymentMethodSystemName == StripeConnectRedirectPaymentDefaults.SystemName)
                {
                    await _stripeConnectRedirectService.CapturePaymentAsync(order.Id);
                }
            }

        }

        /// <summary>
        ///  OrderItem Delivery Denied
        /// </summary>
        /// <param name="order"></param>
        /// <param name="orderItem"></param>
        /// <param name="addOrderNoteMessage"></param>
        /// <returns></returns>
        public override async Task OrderItemDeliveryDeniedOrderAsync(Order order, OrderItem orderItem, string addOrderNoteMessage)
        {

            var product = await _productService.GetProductByIdAsync(orderItem.ProductId);
            var vendorId = product != null ? product.VendorId : 0;
            var vendor = await _vendorService.GetVendorByIdAsync(vendorId);
            //mark order items as delivered
            var slotOrderItems = (await _orderService.GetOrderItemsAsync(order.Id)).Where(o => o.SlotId == orderItem.SlotId && o.SlotTime.Equals(orderItem.SlotTime) && o.OrderItemStatusId != (int)OrderItemStatus.Cancelled).ToList();
            if (slotOrderItems.Any())
            {
                foreach (var item in slotOrderItems)
                {
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

                    var totalRefundAmount = await InsertOrderItemRefundAsync(order, item);

                    //add a note - dispatched
                    await AddOrderNoteAsync(order, $"Order item(Id: {string.Join(", ", orderItem.Id)}) has been marked as delivery denied by the user: {(await _workContext.GetCurrentCustomerAsync()).Email}");

                    // add a note - refund amount
                    await AddOrderNoteAsync(order, $"Order Item Cancel Total Refund amount:- {totalRefundAmount}");
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
                if (items != null && items.All(x => x.OrderItemStatusId == (int)OrderItemStatus.Cancelled))
                {
                    await CancelOrderAsync(order, true);
                }
                else if (items != null && items.All(x => x.OrderItemStatusId == (int)OrderItemStatus.DeliveryDenied
                    || x.OrderItemStatusId == (int)OrderItemStatus.Cancelled || x.OrderItemStatusId == (int)OrderItemStatus.Delivered))
                {
                    //update order status to complete 
                    order.OrderStatusId = (int)OrderStatus.Complete;
                    //set delivered
                    order.ShippingStatusId = (int)ShippingStatus.Delivered;
                    await _orderService.UpdateOrderAsync(order);

                    //add a note
                    await AddOrderNoteAsync(order, $"Order status has been edited. New status: {await _localizationService.GetLocalizedEnumAsync(order.OrderStatus)}");

                    // Stripe refund payment back to customer
                    if (order.PaymentMethodSystemName == StripeConnectRedirectPaymentDefaults.SystemName)
                    {
                        await _stripeConnectRedirectService.CapturePaymentAsync(order.Id);
                    }
                }
            }
        }


        public virtual async Task<decimal> InsertOrderItemRefundAsync(Order order, OrderItem item)
        {
            if (order == null || item == null)
                return decimal.Zero;

            var slotOrderItems = new List<OrderItem>();
            var vendorOrderItems = new List<OrderItem>();
            var adminDeliveryOrderItems = new List<OrderItem>();

            var totalRefundAmount = decimal.Zero;
            var itemSlotId = item.SlotId;
            var product = await _productService.GetProductByIdAsync(item.ProductId);
            var vendorId = product?.VendorId ?? 0;
            var vendor = await _vendorService.GetVendorByIdAsync(vendorId);

            var orderItems = await _orderService.GetOrderItemsAsync(order.Id);
            foreach (var orderItem in orderItems)
            {
                //Add to Vendor List
                var productVendorId = (await _productService.GetProductByIdAsync(orderItem.ProductId))?.VendorId ?? 0;
                if (productVendorId == vendorId)
                    vendorOrderItems.Add(orderItem);

                if (!orderItem.VendorManageDelivery)
                    adminDeliveryOrderItems.Add(orderItem);

                //Add to Slot list
                if (orderItem.SlotId == itemSlotId)
                    slotOrderItems.Add(orderItem);
            }

            vendorOrderItems = vendorOrderItems.Distinct().ToList();
            slotOrderItems = slotOrderItems.Distinct().ToList();
            adminDeliveryOrderItems = adminDeliveryOrderItems.Distinct().ToList();

            bool refundDeliveryFee = slotOrderItems.All(x => x.OrderItemStatusId == (int)OrderItemStatus.Cancelled);
            bool refundSlotFee = slotOrderItems.All(x => x.OrderItemStatusId == (int)OrderItemStatus.Cancelled);
            bool refundTipFee = vendorOrderItems.All(x => x.OrderItemStatusId == (int)OrderItemStatus.Cancelled);
            bool refundServiceFee = orderItems.All(x => x.OrderItemStatusId == (int)OrderItemStatus.Cancelled);
            decimal deliveryFee = decimal.Zero;
            decimal slotFee = decimal.Zero;
            decimal tipFee = decimal.Zero;
            decimal serviceFee = decimal.Zero;

            //order item refund amount
            var orderItemRefund = new OrderItemRefund
            {
                OrderId = order.Id,
                OrderItemId = item.Id,
                VendorId = vendorId,
                IsRefunded = false,
                CreatedOnUtc = DateTime.UtcNow,

                PriceIncltax = item.PriceInclTax,
                TaxFee = item.PriceInclTax - item.PriceExclTax
            };

            //Calculate Commission
            var commissionPercentage = decimal.Zero;
            var transfers = await _stripeConnectRedirectService.GetStripeTransfersByOrderIdVendorIdAsync(orderId: order.Id, vendorId: vendorId);
            var transfer = transfers?.FirstOrDefault();

            if (item.InPickup)
                commissionPercentage = transfers?.FirstOrDefault()?.AdminPickupCommissionPercentage ?? decimal.Zero;
            else
                commissionPercentage = transfers?.FirstOrDefault()?.AdminDeliveryCommissionPercentage ?? decimal.Zero;

            if (commissionPercentage > decimal.Zero)
                orderItemRefund.AdminCommission = (item.PriceExclTax * commissionPercentage) / 100;

            //Calculate fees
            if (refundDeliveryFee)
            {
                if (!item.InPickup)
                    deliveryFee = item.DeliveryFee;
            }
            if (refundSlotFee)
            {
                slotFee = item.SlotPrice;
            }
            if (refundTipFee)
            {
                var slotSubTotalExclTax = vendorOrderItems?.Sum(x => x.PriceExclTax) ?? decimal.Zero;

                tipFee = order.OrderSubtotalExclTax > 0
                    ? ((order.TipFee * slotSubTotalExclTax) / order.OrderSubtotalExclTax)
                    : decimal.Zero;
            }
            if (refundServiceFee)
            {
                serviceFee = order.ServiceFee;
            }

            if (item.VendorManageDelivery)
            {
                orderItemRefund.DeliveryFee = deliveryFee;
                orderItemRefund.TipFee = tipFee;
                orderItemRefund.SlotFee = slotFee;
                orderItemRefund.ServiceFee = decimal.Zero;  //Service fee always be refunded to Admin
            }

            orderItemRefund.TotalAmount =
                  orderItemRefund.PriceIncltax
                + orderItemRefund.ServiceFee
                + orderItemRefund.DeliveryFee
                + orderItemRefund.SlotFee
                + orderItemRefund.TipFee;

            totalRefundAmount += orderItemRefund.TotalAmount;
            //Insert refund amount
            await _orderItemRefundService.InsertOrderItemRefundAsync(orderItemRefund);

            //Admin Refund
            if (refundServiceFee || (!item.VendorManageDelivery && (refundDeliveryFee || refundSlotFee || refundTipFee)))
            {
                var orderItemAdminRefund = new OrderItemRefund
                {
                    OrderId = order.Id,
                    OrderItemId = item.Id,
                    VendorId = 0,
                    IsRefunded = false,
                    CreatedOnUtc = DateTime.UtcNow
                };

                if (!item.VendorManageDelivery)
                {
                    orderItemAdminRefund.PriceIncltax = decimal.Zero;   //Product price will always be refunded to Vendor
                    orderItemAdminRefund.DeliveryFee = deliveryFee;
                    orderItemAdminRefund.SlotFee = slotFee;
                    orderItemAdminRefund.TipFee = tipFee;
                }

                orderItemAdminRefund.ServiceFee = serviceFee;

                orderItemAdminRefund.TotalAmount =
                  orderItemAdminRefund.PriceIncltax
                + orderItemAdminRefund.ServiceFee
                + orderItemAdminRefund.DeliveryFee
                + orderItemAdminRefund.SlotFee
                + orderItemAdminRefund.TipFee;

                totalRefundAmount += orderItemAdminRefund.TotalAmount;
                //Insert information refund amount
                await _orderItemRefundService.InsertOrderItemRefundAsync(orderItemAdminRefund);
            }

            //order note
            var sb = new StringBuilder();
            sb.AppendLine("Order Item Id: " + item.Id);
            sb.AppendLine("Order Item Refund amount Item Price: " + orderItemRefund.PriceIncltax);
            sb.AppendLine("Order Item Refund amount Item Tax Fee: " + orderItemRefund.TaxFee);
            sb.AppendLine("Order Item Refund amount Item Delivery Fee: " + deliveryFee);
            sb.AppendLine("Order Item Refund amount Item Slot Fee: " + slotFee);
            sb.AppendLine("Order Item Refund amount Item Tip Fee: " + tipFee);
            sb.AppendLine("Order Item Refund amount Item Service Fee: " + serviceFee);

            await _orderService.InsertOrderNoteAsync(new OrderNote
            {
                OrderId = order.Id,
                Note = Convert.ToString(sb),
                DisplayToCustomer = false,
                CreatedOnUtc = DateTime.UtcNow
            });

            return totalRefundAmount;
        }

        #endregion
    }
}