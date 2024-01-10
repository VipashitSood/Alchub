using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Alchub.Domain.Orders;
using Nop.Core.Domain.Orders;
using Nop.Services.Alchub.Twillio;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Web.Areas.Admin.Alchub.Models.Dispatch;

namespace Nop.Web.Controllers
{
    public partial class DoorDashController : BasePublicController
    {
        #region Fields

        private readonly IOrderDispatchService _orderDispatchService;
        private readonly ILogger _logger;
        private readonly ILocalizationService _localizationService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly ITwillioService _twillioService;
        private readonly IOrderService _orderService;
        private readonly IWorkContext _workContext;
        private readonly ICustomerActivityService _customerActivityService;
        #endregion

        #region Ctor

        public DoorDashController(
            IOrderDispatchService orderDispatchService,
            ILogger logger,
            ILocalizationService localizationService,
            IOrderProcessingService orderProcessingService,
            IWorkflowMessageService workflowMessageService,
            ITwillioService twillioService,
            IOrderService orderService,
            IWorkContext workContext,
            ICustomerActivityService customerActivityService)
        {
            _orderDispatchService = orderDispatchService;
            _logger = logger;
            _localizationService = localizationService;
            _orderProcessingService = orderProcessingService;
            _workflowMessageService = workflowMessageService;
            _twillioService = twillioService;
            _orderService = orderService;
            _workContext = workContext;
            _customerActivityService = customerActivityService;
        }

        #endregion

        #region 
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


        protected virtual async Task LogEditOrderAsync(int orderId)
        {
            var order = await _orderService.GetOrderByIdAsync(orderId);

            await _customerActivityService.InsertActivityAsync("EditOrder",
                string.Format(await _localizationService.GetResourceAsync("ActivityLog.EditOrder"), order.CustomOrderNumber), order);
        }

        #endregion

        #region Door Dash

        /// <summary>
        /// Door Dash Webhook Hit Update Status
        /// </summary>
        /// <returns></returns>
        public virtual async Task<IActionResult> UpdateStatus()
        {
            try
            {
                await _logger.InformationAsync(string.Format("Alchub Update Status Door Dash"));
                //Get All Dispatch Order One Week 
                var getAllDispatchOrdersOneWeekList = await _orderDispatchService.GetDispatchOrderOneWeekAsync();

                foreach (var dispatchOrder in getAllDispatchOrdersOneWeekList)
                {
                    try
                    {

                        //order get 
                        var order = await _orderService.GetOrderByIdAsync(dispatchOrder.OrderNumber);

                        //order items
                        var orderItem = await _orderService.GetOrderItemByIdAsync(dispatchOrder.OrderItemId);
                        IList<OrderItem> orderItems= new List<OrderItem>();

                        // Get delivery order status 
                        var resultDelivery = await _orderDispatchService.GetDoorDashDeliveryAsync(dispatchOrder.ExtrnalDeliveryId);

                        // response code 
                        var statusresultDelivery = resultDelivery.StatusCode;

                        if (statusresultDelivery == System.Net.HttpStatusCode.BadRequest)
                        {
                            await _logger.ErrorAsync(await _localizationService.GetResourceAsync("Alchub.Admin.Dispatch.BadRequest"));
                        }
                        else if (statusresultDelivery == System.Net.HttpStatusCode.Conflict)
                        {
                            await _logger.ErrorAsync(await _localizationService.GetResourceAsync("Alchub.Admin.Dispatch.Conflict"));
                        }
                        else if (statusresultDelivery == System.Net.HttpStatusCode.OK)
                        {
                            //read string 
                            var resultStringAcceptQuotes = await resultDelivery.Content.ReadAsStringAsync();

                            // Get the response from the external API (if needed)
                            var responseQuotes = Newtonsoft.Json.JsonConvert.DeserializeObject<Root>(resultStringAcceptQuotes);

                            dispatchOrder.Id = dispatchOrder.Id;
                            dispatchOrder.TrackingUrl = responseQuotes.tracking_url;
                            dispatchOrder.DasherName = responseQuotes.dasher_name;
                            dispatchOrder.DashPhoneNumber = responseQuotes.dasher_dropoff_phone_number;
                            dispatchOrder.DashVehicleNumber = $"{responseQuotes.dasher_vehicle_make}_{responseQuotes.dasher_vehicle_model}_{responseQuotes.dasher_vehicle_year}" ;
                            dispatchOrder.CustomerSignature = responseQuotes.dropoff_signature_image_url;


                            // Map delivery status
                            switch (responseQuotes.delivery_status)
                            {
                                case "created":
                                    dispatchOrder.DeliveryStatus = "Created";
                                    break;
                                case "enroute_to_pickup":
                                    dispatchOrder.DeliveryStatus = "Dasher Assigned";
                                    break;
                                case "arrived_at_pickup":
                                    dispatchOrder.DeliveryStatus = "Dasher Arrived at Pickup";
                                    break;
                                case "enroute_to_dropoff":
                                    dispatchOrder.DeliveryStatus = "Dasher Arrived at Dropoff";
                                    break;
                                case "picked_up":
                                    dispatchOrder.DeliveryStatus = "Delivery Picked Up";
                                    orderItems.Add(orderItem);
                                    //mark order items as dispatched
                                    var slotOrderItems = (await _orderService.GetOrderItemsAsync(order.Id)).Where(o => o.SlotId == orderItem.SlotId && o.SlotTime.Equals(orderItem.SlotTime) && o.OrderItemStatus == OrderItemStatus.Pending).ToList();
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
                                        await SendItemDispacthedNotificationsAndSaveNotesAsync(order, orderItems);
                                    }
                                    break;
                                case "arrived_at_dropoff":
                                    dispatchOrder.DeliveryStatus = "Dasher Arrived at Dropoff";
                                    break;
                                case "delivered":
                                    dispatchOrder.DeliveryStatus = "Delivered";
                                    await _orderProcessingService.OrderItemDeliveredAsync(order, orderItem);
                                    break;
                                case "cancelled":
                                    dispatchOrder.DeliveryStatus = "";
                                    dispatchOrder.TrackingUrl = "";
                                    break;
                                default:
                                    dispatchOrder.DeliveryStatus = responseQuotes.delivery_status;
                                    break;
                            }

                            //Update dispatch order 
                            await _orderDispatchService.UpdateDispatchAsync(dispatchOrder);
                            await _logger.InformationAsync(await _localizationService.GetResourceAsync("Alchub.Admin.Dispatch.Successfully"));
                        }
                    }
                    catch (Exception exc)
                    {
                        await _logger.ErrorAsync(string.Format("Alchub Update Status Door Dash Error Occur External delivery Id:- " + dispatchOrder.ExtrnalDeliveryId), exc);
                        continue;
                    }
                }
                return Ok();
            }
            catch (Exception exc)
            {
                // Handle any exceptions that occurred during the sync operation
                await _logger.ErrorAsync(string.Format("Alchub Update Status Door Dash Error Occur"), exc);
                return Ok();
            }
        }

        #endregion
    }
}