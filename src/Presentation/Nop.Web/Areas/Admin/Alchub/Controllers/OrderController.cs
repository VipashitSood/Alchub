using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Alchub.Domain.Orders;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Vendors;
using Nop.Core.Events;
using Nop.Services.Alchub.Twillio;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.DeliveryFees;
using Nop.Services.ExportImport;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Security;
using Nop.Services.Shipping;
using Nop.Services.TipFees;
using Nop.Services.Vendors;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Models.Orders;
using Nop.Web.Framework.Controllers;

namespace Nop.Web.Areas.Admin.Controllers
{
    public partial class OrderController : BaseAdminController
    {
        #region Fields

        private readonly IAddressAttributeParser _addressAttributeParser;
        private readonly IAddressService _addressService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly ICustomerService _customerService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IDownloadService _downloadService;
        private readonly IEncryptionService _encryptionService;
        private readonly IEventPublisher _eventPublisher;
        private readonly IExportManager _exportManager;
        private readonly IGiftCardService _giftCardService;
        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly IOrderModelFactory _orderModelFactory;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IOrderService _orderService;
        private readonly IPaymentService _paymentService;
        private readonly IPdfService _pdfService;
        private readonly IPermissionService _permissionService;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly IProductAttributeFormatter _productAttributeFormatter;
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly IProductAttributeService _productAttributeService;
        private readonly IProductService _productService;
        private readonly IShipmentService _shipmentService;
        private readonly IShippingService _shippingService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IWorkContext _workContext;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly OrderSettings _orderSettings;
        private readonly ITwillioService _twillioService;
        private readonly IDeliveryFeeService _deliveryFeeService;
        private readonly ITipFeeService _tipFeeService;
        private readonly IVendorService _vendorService;
        private readonly IOrderItemRefundService _orderItemRefundService;
        #endregion

        #region Ctor

        public OrderController(IAddressAttributeParser addressAttributeParser,
            IAddressService addressService,
            ICustomerActivityService customerActivityService,
            ICustomerService customerService,
            IDateTimeHelper dateTimeHelper,
            IDownloadService downloadService,
            IEncryptionService encryptionService,
            IEventPublisher eventPublisher,
            IExportManager exportManager,
            IGiftCardService giftCardService,
            ILocalizationService localizationService,
            INotificationService notificationService,
            IOrderModelFactory orderModelFactory,
            IOrderProcessingService orderProcessingService,
            IOrderService orderService,
            IPaymentService paymentService,
            IPdfService pdfService,
            IPermissionService permissionService,
            IPriceCalculationService priceCalculationService,
            IProductAttributeFormatter productAttributeFormatter,
            IProductAttributeParser productAttributeParser,
            IProductAttributeService productAttributeService,
            IProductService productService,
            IShipmentService shipmentService,
            IShippingService shippingService,
            IShoppingCartService shoppingCartService,
            IWorkContext workContext,
            IWorkflowMessageService workflowMessageService,
            OrderSettings orderSettings,
            ITwillioService twillioService,
            IDeliveryFeeService deliveryFeeService,
            ITipFeeService tipFeeService,
            IVendorService vendorService,
            IOrderItemRefundService orderItemRefundService)
        {
            _addressAttributeParser = addressAttributeParser;
            _addressService = addressService;
            _customerActivityService = customerActivityService;
            _customerService = customerService;
            _dateTimeHelper = dateTimeHelper;
            _downloadService = downloadService;
            _encryptionService = encryptionService;
            _eventPublisher = eventPublisher;
            _exportManager = exportManager;
            _giftCardService = giftCardService;
            _localizationService = localizationService;
            _notificationService = notificationService;
            _orderModelFactory = orderModelFactory;
            _orderProcessingService = orderProcessingService;
            _orderService = orderService;
            _paymentService = paymentService;
            _pdfService = pdfService;
            _permissionService = permissionService;
            _priceCalculationService = priceCalculationService;
            _productAttributeFormatter = productAttributeFormatter;
            _productAttributeParser = productAttributeParser;
            _productAttributeService = productAttributeService;
            _productService = productService;
            _shipmentService = shipmentService;
            _shippingService = shippingService;
            _shoppingCartService = shoppingCartService;
            _workContext = workContext;
            _workflowMessageService = workflowMessageService;
            _orderSettings = orderSettings;
            _twillioService = twillioService;
            _deliveryFeeService = deliveryFeeService;
            _tipFeeService = tipFeeService;
            _vendorService = vendorService;
            _orderItemRefundService = orderItemRefundService;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Add order note
        /// </summary>
        /// <param name="order">Order</param>
        /// <param name="note">Note text</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        protected virtual async Task AddOrderNoteAsync(Order order, string note)
        {
            await _orderService.InsertOrderNoteAsync(new OrderNote
            {
                OrderId = order.Id,
                Note = note,
                DisplayToCustomer = false,
                CreatedOnUtc = DateTime.UtcNow
            });
        }

 

        /// <summary>
        /// Send "order items dispatched" notifications and save order notes
        /// </summary>
        /// <param name="order">Order</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        protected virtual async Task SendItemDispacthedNotificationsAndSaveNotesAsync(Order order, IList<OrderItem> orderItems)
        {
            //send email notification
            var queuedEmailIds = await _workflowMessageService.SendOrderItemsDispachedCustomerNotificationAsync(order, orderItems, order.CustomerLanguageId);
            if (queuedEmailIds.Any())
                await AddOrderNoteAsync(order, $"\"Order Items Dispatched\" email (to customer) has been queued. Queued email identifiers: {string.Join(", ", queuedEmailIds)}.");

            //send SMS notification
            await _twillioService.SendOrderItemsStatusUpdatedCustomerSMSAsync(order, orderItems, order.CustomerLanguageId, OrderItemStatusActionType.Dispatched);
        }


        #region Refund Email Template


        protected virtual async Task SendOrderItemCancelNotificationsCustomerAndSaveNotesAsync(Order order, OrderItem orderItem,decimal orderItemRefundTotalAmount)
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

        protected virtual async Task SendOrderItemDeliveryDeniedNotificationsCustomerAndSaveNotesAsync(Order order, IList<OrderItem> orderItems,OrderItem smsOrderItem)
        {
            //send email notification
            var queuedEmailIds = await _workflowMessageService.SendOrderItemsDeliveryDeniedCustomerNotificationAsync(order, orderItems,order.CustomerLanguageId);
            if (queuedEmailIds.Any())
                await AddOrderNoteAsync(order, $"\"Order Item Delivery Denied\" email (to customer) has been queued. Queued email identifiers: {string.Join(", ", queuedEmailIds)}.");

            //send SMS notification
             await _twillioService.SendOrderItemDeliveryDeniedCustomerSMSAsync(order, smsOrderItem, order.CustomerLanguageId);
        }

        protected virtual async Task SendOrderItemDeliveryDeniedNotificationsVendorAndSaveNotesAsync(Order order, IList<OrderItem> orderItems, Vendor vendor)
        {
            //send email notification
            var queuedEmailIds = await _workflowMessageService.SendOrderItemsDeliveryDeniedVendorNotificationAsync(order, orderItems, vendor, order.CustomerLanguageId);
            if (queuedEmailIds.Any())
                await AddOrderNoteAsync(order, $"\"Order Item Delivery Denied\" email (to customer) has been queued. Queued email identifiers: {string.Join(", ", queuedEmailIds)}.");

        }


        protected virtual async Task SendOrderItemPickupEmailNotificationsCustomerAndSaveNotesAsync(Order order, OrderItem orderItems)
        {
            //send email notification
            var queuedEmailIds = await _workflowMessageService.SendPickupOrderItemCustomerNotificationAsync(order, orderItems, order.CustomerLanguageId);
            if (queuedEmailIds.Any())
                await AddOrderNoteAsync(order, $"\"Send Email Order Item Pickup\" email (to customer) has been queued. Queued email identifiers: {string.Join(", ", queuedEmailIds)}.");

            //send SMS notification
            await _twillioService.SendOrderItemPickupCustomerSMSAsync(order, orderItems, order.CustomerLanguageId);
        }
        #endregion

        #endregion

        #region Order details


        [HttpPost, ActionName("Edit")]
        [FormValueRequired("btnSaveOrderStatus")]
        public virtual async Task<IActionResult> ChangeOrderStatus(int id, OrderModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            //try to get an order with the specified id
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null)
                return RedirectToAction("List");

            //a vendor does not have access to this functionality
            if (await _workContext.GetCurrentVendorAsync() != null)
                return RedirectToAction("Edit", "Order", new { id });

            try
            {
                order.OrderStatusId = model.OrderStatusId;
                await _orderService.UpdateOrderAsync(order);

                //add a note
                await _orderService.InsertOrderNoteAsync(new OrderNote
                {
                    OrderId = order.Id,
                    Note = $"Order status has been edited. New status: {await _localizationService.GetLocalizedEnumAsync(order.OrderStatus)}",
                    DisplayToCustomer = false,
                    CreatedOnUtc = DateTime.UtcNow
                });

                if (order.OrderStatusId == (int)OrderStatus.Cancelled)
                   await  _orderProcessingService.OrderStatusCancelledChangedAsync(order);

                if (order.OrderStatusId == (int)OrderStatus.Complete)
                    await _orderProcessingService.OrderStatusCompleteChangedAsync(order);

                await LogEditOrderAsync(order.Id);

                //prepare model
                model = await _orderModelFactory.PrepareOrderModelAsync(model, order);

                return View(model);
            }
            catch (Exception exc)
            {
                //prepare model
                model = await _orderModelFactory.PrepareOrderModelAsync(model, order);

                await _notificationService.ErrorNotificationAsync(exc);
                return View(model);
            }
        }


        #region Order item

        [HttpPost, ActionName("Edit")]
        [FormValueRequired(FormValueRequirement.StartsWith, "btnMarkAsDeliveredOrderItem")]
        public virtual async Task<IActionResult> MarkAsDeliveredOrderItem(int id, IFormCollection form)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            //try to get an order with the specified id
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null)
                return RedirectToAction("List");

            try
            {
                //get order item identifier
                var orderItemId = 0;
                foreach (var formValue in form.Keys)
                    if (formValue.StartsWith("btnMarkAsDeliveredOrderItem", StringComparison.InvariantCultureIgnoreCase))
                        orderItemId = Convert.ToInt32(formValue["btnMarkAsDeliveredOrderItem".Length..]);

                var orderItem = await _orderService.GetOrderItemByIdAsync(orderItemId)
                    ?? throw new ArgumentException("No order item found with the specified id");

                await _orderProcessingService.OrderItemDeliveredAsync(order, orderItem);

                await LogEditOrderAsync(order.Id);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Alchub.Admin.Order.Products.OrderItem.OrderItemStatus.Changed"));
                //prepare model
                var model = await _orderModelFactory.PrepareOrderModelAsync(null, order);

                //selected card
                //SaveSelectedCardName("order-products", persistForTheNextRequest: false);

                return View(model);
            }
            catch (Exception exc)
            {
                //prepare model
                var model = await _orderModelFactory.PrepareOrderModelAsync(null, order);

                //selected card
                //SaveSelectedCardName("order-products", persistForTheNextRequest: false);

                await _notificationService.ErrorNotificationAsync(exc);
                return View(model);
            }
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired(FormValueRequirement.StartsWith, "btnMarkAsDispatchedOrderItem")]
        public virtual async Task<IActionResult> MarkAsDispatchedOrderItem(int id, IFormCollection form)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            //try to get an order with the specified id
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null)
                return RedirectToAction("List");

            try
            {
                //get order item identifier
                var orderItemId = 0;
                foreach (var formValue in form.Keys)
                    if (formValue.StartsWith("btnMarkAsDispatchedOrderItem", StringComparison.InvariantCultureIgnoreCase))
                        orderItemId = Convert.ToInt32(formValue["btnMarkAsDispatchedOrderItem".Length..]);

                var orderItem = await _orderService.GetOrderItemByIdAsync(orderItemId)
                    ?? throw new ArgumentException("No order item found with the specified id");

                if (orderItem.InPickup)
                    throw new ArgumentException("Can't set Dispacheds status to pickup products");

                //mark order items as dispatched
                var slotOrderItems = (await _orderService.GetOrderItemsAsync(id)).Where(o => o.SlotId == orderItem.SlotId && o.SlotTime.Equals(orderItem.SlotTime) && o.OrderItemStatus == OrderItemStatus.Pending).ToList();
                if (slotOrderItems.Any())
                {
                    foreach (var item in slotOrderItems)
                    {
                        //marke each item as dispached
                        item.OrderItemStatus = OrderItemStatus.Dispatch;
                        await _orderService.UpdateOrderItemAsync(item);
                    }

                    //add a note - dispatched
                    await AddOrderNoteAsync(order, $"Order items(Ids: {string.Join(", ", slotOrderItems.Select(x => x.Id))}) has been marked as dispached by the user: {(await _workContext.GetCurrentCustomerAsync()).Email}");

                    await LogEditOrderAsync(order.Id);

                    //send email to customer.
                    await SendItemDispacthedNotificationsAndSaveNotesAsync(order, slotOrderItems);

                    //page notification
                    _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Alchub.Admin.Order.Products.OrderItem.OrderItemStatus.Changed"));
                }

                //prepare model
                var model = await _orderModelFactory.PrepareOrderModelAsync(null, order);

                //selected card
                //SaveSelectedCardName("order-products", persistForTheNextRequest: false);

                return View(model);
            }
            catch (Exception exc)
            {
                //prepare model
                var model = await _orderModelFactory.PrepareOrderModelAsync(null, order);

                //selected card
                //SaveSelectedCardName("order-products", persistForTheNextRequest: false);

                await _notificationService.ErrorNotificationAsync(exc);
                return View(model);
            }
        }


        [HttpPost, ActionName("Edit")]
        [FormValueRequired(FormValueRequirement.StartsWith, "btnMarkAsCancelOrderItem")]
        public virtual async Task<IActionResult> MarkAsCancelOrderItem(int id, IFormCollection form)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            //try to get an order with the specified id
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null)
                return RedirectToAction("List");

            try
            {
                //get order item identifier
                var orderItemId = 0;
                foreach (var formValue in form.Keys)
                    if (formValue.StartsWith("btnMarkAsCancelOrderItem", StringComparison.InvariantCultureIgnoreCase))
                        orderItemId = Convert.ToInt32(formValue["btnMarkAsCancelOrderItem".Length..]);

                var item = await _orderService.GetOrderItemByIdAsync(orderItemId)
                    ?? throw new ArgumentException("No order item found with the specified id");

                await _orderProcessingService.OrderItemCancelOrderAsync(order, item);
                await LogEditOrderAsync(order.Id);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Alchub.Admin.Order.Products.OrderItem.OrderItemStatus.Changed"));

                //prepare model
                var model = await _orderModelFactory.PrepareOrderModelAsync(null, order);

                return View(model);
            }
            catch (Exception exc)
            {
                //prepare model
                var model = await _orderModelFactory.PrepareOrderModelAsync(null, order);

                await _notificationService.ErrorNotificationAsync(exc);
                return View(model);
            }
        }


        [HttpPost, ActionName("Edit")]
        [FormValueRequired(FormValueRequirement.StartsWith, "btnMarkAsDeliveryDeniedOrderItem")]
        public virtual async Task<IActionResult> MarkAsDeliveryDeniedOrderItem(int id,string addOrderNoteMessage , IFormCollection form)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            //try to get an order with the specified id
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null)
                return RedirectToAction("List");

            try
            {
                //get order item identifier
                var orderItemId = 0;
                foreach (var formValue in form.Keys)
                    if (formValue.StartsWith("btnMarkAsDeliveryDeniedOrderItem", StringComparison.InvariantCultureIgnoreCase))
                        orderItemId = Convert.ToInt32(formValue["btnMarkAsDeliveryDeniedOrderItem".Length..]);

                var orderItem = await _orderService.GetOrderItemByIdAsync(orderItemId)
                    ?? throw new ArgumentException("No order item found with the specified id");

                   await _orderProcessingService.OrderItemDeliveryDeniedOrderAsync(order, orderItem, addOrderNoteMessage);

                    await LogEditOrderAsync(order.Id);
                    //page notification
                    _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Alchub.Admin.Order.Products.OrderItem.OrderItemStatus.Changed"));
                

                //prepare model
                var model = await _orderModelFactory.PrepareOrderModelAsync(null, order);

                return View(model);
            }
            catch (Exception exc)
            {
                //prepare model
                var model = await _orderModelFactory.PrepareOrderModelAsync(null, order);

                //selected card
                //SaveSelectedCardName("order-products", persistForTheNextRequest: false);

                await _notificationService.ErrorNotificationAsync(exc);
                return View(model);
            }
        }


        [HttpPost, ActionName("Edit")]
        [FormValueRequired(FormValueRequirement.StartsWith, "btnSendEmailPickupOrderItem")]
        public virtual async Task<IActionResult> SendEmailPickupOrderItem(int id, IFormCollection form)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            //try to get an order with the specified id
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null)
                return RedirectToAction("List");

            try
            {
                //get order item identifier
                var orderItemId = 0;
                foreach (var formValue in form.Keys)
                    if (formValue.StartsWith("btnSendEmailPickupOrderItem", StringComparison.InvariantCultureIgnoreCase))
                        orderItemId = Convert.ToInt32(formValue["btnSendEmailPickupOrderItem".Length..]);

                var orderItem = await _orderService.GetOrderItemByIdAsync(orderItemId)
                    ?? throw new ArgumentException("No order item found with the specified id");

                //send email customer order item to pickup
                await SendOrderItemPickupEmailNotificationsCustomerAndSaveNotesAsync(order, orderItem);

                //prepare model
                var model = await _orderModelFactory.PrepareOrderModelAsync(null, order);

                return View(model);
            }
            catch (Exception exc)
            {
                //prepare model
                var model = await _orderModelFactory.PrepareOrderModelAsync(null, order);

                await _notificationService.ErrorNotificationAsync(exc);
                return View(model);
            }
        }


        private decimal CalculateOrderItemFee(int vendorOrderItemCount,int count, decimal fee)
        {
            return (vendorOrderItemCount * fee) / count;
        }
        #endregion

        #endregion
    }
}
