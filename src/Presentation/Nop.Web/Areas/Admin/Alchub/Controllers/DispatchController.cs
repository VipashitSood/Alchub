using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Alchub.Domain.Twillio;
using Nop.Core.Domain.Orders;
using Nop.Services.Alchub.Slots;
using Nop.Services.Alchub.Twillio;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Services.TipFees;
using Nop.Services.Vendors;
using Nop.Web.Areas.Admin.Alchub.Models.Dispatch;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Models.Dispatch;

namespace Nop.Web.Areas.Admin.Controllers
{
    public partial class DispatchController : BaseAdminController
    {
        #region Fields
        private readonly IPermissionService _permissionService;
        private readonly IDispatchModelFactory _dispatchModelFactory;
        private readonly IOrderService _orderService;
        private readonly IProductService _productService;
        private readonly IVendorService _vendorService;
        private readonly IAddressService _addressService;
        private readonly ICountryService _countryService;
        private readonly INotificationService _notificationService;
        private readonly ILocalizationService _localizationService;
        private readonly ICustomerService _customerService;
        private readonly IOrderDispatchService _orderDispatchService;
        private readonly ITipFeeService _tipFeeService;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly ILogger _logger;
        private readonly TwillioSettings _twillioSettings;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly ITwillioService _twillioService;
        private readonly IDateTimeHelper _dateTimeHelper;
        #endregion

        #region Ctor

        public DispatchController(
            IPermissionService permissionService,
            IDispatchModelFactory dispatchModelFactory,
            IOrderService orderService,
            IProductService productService,
            IVendorService vendorService,
            IAddressService addressService,
            ICountryService countryService,
            INotificationService notificationService,
            ILocalizationService localizationService,
            ICustomerService customerService,
            IOrderDispatchService orderDispatchService,
            ITipFeeService tipFeeService,
            IPriceCalculationService priceCalculationService,
            ILogger logger,
            TwillioSettings twillioSettings,
            IOrderProcessingService orderProcessingService,
            IWorkflowMessageService workflowMessageService,
            ITwillioService twillioService,
            IDateTimeHelper dateTimeHelper
          )
        {
            _permissionService = permissionService;
            _dispatchModelFactory = dispatchModelFactory;
            _orderService = orderService;
            _productService = productService;
            _vendorService = vendorService;
            _addressService = addressService;
            _countryService = countryService;
            _notificationService = notificationService;
            _localizationService = localizationService;
            _customerService = customerService;
            _orderDispatchService = orderDispatchService;
            _tipFeeService = tipFeeService;
            _priceCalculationService = priceCalculationService;
            _logger = logger;
            _twillioSettings = twillioSettings;
            _orderProcessingService = orderProcessingService;
            _workflowMessageService = workflowMessageService;
            _twillioService = twillioService;
            _dateTimeHelper = dateTimeHelper;
        }

        #endregion

        #region Utils

        private class ErrorResponse
        {
            public bool Success { get; set; }
            public string Message { get; set; }
            public List<FieldError> FieldErrors { get; set; }
        }

        private class FieldError
        {
            public string Field { get; set; }
            public string Error { get; set; }
        }

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

        #endregion

        #region Methods

        public virtual IActionResult Index()
        {
            return RedirectToAction("List");
        }

        public virtual async Task<IActionResult> List()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageDispatch))
                return AccessDeniedView();

            //prepare model
            DispatchSearchModel searchModel = new DispatchSearchModel();
            var model = await _dispatchModelFactory.PrepareOrderItemsSlotTimeListModelAsync(searchModel);

            return View(model);
        }



        [HttpPost]
        public virtual async Task<IActionResult> FindDriverOrderItem(int orderItemId)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            if (orderItemId == 0)
                return RedirectToAction("List");

            try
            {
                var currentTime = DateTime.Now;
                var orderItem = await _orderService.GetOrderItemByIdAsync(orderItemId)
                  ?? throw new ArgumentException("No order item found with the specified id");

                var order = await _orderService.GetOrderByIdAsync(orderItem.OrderId)
               ?? throw new ArgumentException("No order found with the specified id");

                var product = _productService.GetProductByIdAsync(orderItem.ProductId)
                    ?? throw new ArgumentException("No product found with the specified id");

                var vendor = await _vendorService.GetVendorByProductIdAsync(orderItem.ProductId)
                ?? throw new ArgumentException("No vendor found with the specified id");

                var shippingAddress = await _addressService.GetAddressByIdAsync(order.ShippingAddressId.Value);
                var shippingCountry = await _countryService.GetCountryByAddressAsync(shippingAddress);

                var customer = await _customerService.GetCustomerByIdAsync(order.CustomerId);

                //Request to Door Dash Order Quotes
                var resultQuotes = await _orderDispatchService.GetDoorDashQuotesAsync(customer, vendor, order, orderItem);

                //Get Status 
                var statusQuotes = resultQuotes.StatusCode;

                //Read string
                var resultStringQuotes = await resultQuotes.Content.ReadAsStringAsync();

                if (statusQuotes == System.Net.HttpStatusCode.BadRequest)
                {
                    var errorResponse = new ErrorResponse
                    {
                        Success = false,
                        Message = "Door Dash Error processing the request.", // You can include an error message if needed
                        FieldErrors = new List<FieldError>() // Initialize an empty list for field errors
                    };

                    // Deserialize the error response from the server
                    var errorResponseData = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(resultStringQuotes);

                    // Check if the "field_errors" key exists in the error response
                    if (errorResponseData.ContainsKey("field_errors"))
                    {
                        // Get the "field_errors" value from the response and include them in the error response object
                        var fieldErrors = Newtonsoft.Json.JsonConvert.DeserializeObject<List<FieldError>>(errorResponseData["field_errors"].ToString());
                        errorResponse.FieldErrors = fieldErrors;

                        // Check if the error message matches the specific error condition
                        var errorMessage = fieldErrors.FirstOrDefault()?.Error;
                        if (errorMessage == "Allowed distance between addresses exceeded")
                        {
                            // Handle the specific error condition here, if needed
                            // You can add specific error handling code or return a custom error response
                            // For example, you can add a custom error message to the errorResponse
                            errorResponse.Message = "Oops ! Distance too far. Please check for another location";
                        }
                    }
                    // Return the JSON error response to the client-side JavaScript code
                    return Json(errorResponse);
                }

                else if (statusQuotes == System.Net.HttpStatusCode.Conflict)
                {
                    var errorResponse = new ErrorResponse
                    {
                        Success = false,
                        Message = "Door Dash Error processing the request.", // You can include an error message if needed
                        FieldErrors = new List<FieldError>() // Initialize an empty list for field errors
                    };

                    // Deserialize the error response from the server
                    var errorResponseData = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(resultStringQuotes);

                    // Check if the "field_errors" key exists in the error response
                    if (errorResponseData.ContainsKey("message"))
                    {
                        // Get the "field_errors" value from the response
                        var fieldErrorsData = errorResponseData["message"];

                        // Check if the fieldErrorsData is a string (single field error) or an array (multiple field errors)
                        if (fieldErrorsData is string singleError)
                        {
                            // Create a custom FieldError object with the custom message and add it to the errorResponse
                            var fieldError = new FieldError
                            {
                                Field = "Duplicate external id please try after some time", // Replace with the actual field name causing the conflict
                                Error = singleError // Use the custom error message from the response
                            };
                            errorResponse.FieldErrors.Add(fieldError);
                        }
                        else if (fieldErrorsData is Newtonsoft.Json.Linq.JArray arrayErrors) // JArray is from Newtonsoft.Json
                        {
                            // Deserialize the array of field errors into a List<FieldError> object
                            var fieldErrors = arrayErrors.ToObject<List<FieldError>>();

                            // Add the field errors to the error response
                            errorResponse.FieldErrors.AddRange(fieldErrors);
                        }
                    }


                    // Return the JSON error response to the client-side JavaScript code
                    return Json(errorResponse);
                }
                else if (statusQuotes == System.Net.HttpStatusCode.OK)
                {
                    // Get the response from the external API (if needed)
                    var responseQuotes = Newtonsoft.Json.JsonConvert.DeserializeObject<Root>(resultStringQuotes);

                    //fee : conver to cent - 04-12-23k
                    var feeAmount = Convert.ToDecimal(responseQuotes.fee);
                    if (feeAmount > decimal.Zero)
                    {
                        feeAmount = feeAmount / 100;
                        feeAmount = await _priceCalculationService.RoundPriceAsync(feeAmount);
                    }
                    responseQuotes.feeStr = feeAmount.ToString("0:00");

                    responseQuotes.orderItemId = orderItemId;
                    var vendorWiseTipFees = await _tipFeeService.GetVendorWiseOrderTipFeesByOrderIdAsync(order.Id);
                    var resultTemp = vendorWiseTipFees.Where(x => x.VendorId == vendor.Id);
                    var tip = resultTemp?.Sum(x => x.TipFeeValue) ?? decimal.Zero;
                    responseQuotes.tip = await _priceCalculationService.RoundPriceAsync(tip);

                    //// Format the date and time strings
                    //responseQuotes.pickupdatetime =  responseQuotes.pickup_time_estimated.ToString("MM/dd/yyyy hh:mm tt").Replace('/', '-').ToLower();
                    //responseQuotes.dropoffdatetime = responseQuotes.dropoff_time_estimated.ToString("MM/dd/yyyy hh:mm tt").Replace('/', '-').ToLower();

                    responseQuotes.pickupdatetime = (await _dateTimeHelper.ConvertToUserTimeAsync(responseQuotes.pickup_time_estimated, DateTimeKind.Utc)).ToString("MM/dd/yyyy hh:mm tt").Replace('/', '-').ToLower();
                    responseQuotes.dropoffdatetime = (await _dateTimeHelper.ConvertToUserTimeAsync(responseQuotes.dropoff_time_estimated, DateTimeKind.Utc)).ToString("MM/dd/yyyy hh:mm tt").Replace('/', '-').ToLower();

                    // Create an object to represent the response to send to the client-side
                    var jsonResponse = new
                    {
                        Success = true, // You can set this based on the logic in your action method
                        Message = "Request processed successfully.", // An optional success message
                        Data = responseQuotes // Include the data from the external API if needed
                    };

                    return Json(jsonResponse);
                }
                else
                {
                    // Default return statement for other status codes (e.g., InternalServerError, NotFound, etc.)
                    var errorResponse = new ErrorResponse
                    {
                        Success = false,
                        Message = "Door Dash Unknown error occurred.", // You can include an error message if needed
                        FieldErrors = new List<FieldError>() // Initialize an empty list for field errors
                    };

                    // Deserialize the error response from the server
                    var errorResponseData = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(resultStringQuotes);

                    if (errorResponseData.ContainsKey("message"))
                    {
                        var errorMessage = errorResponseData["message"].ToString();
                        if (errorMessage == "Allowed distance between addresses exceeded")
                        {
                            // Handle the specific error condition here, if needed
                            // You can add specific error handling code or return a custom error response
                            // For example, you can add a custom error message to the errorResponse
                            var fieldError = new FieldError
                            {
                                Field = "distance", // Replace with the appropriate field name causing the error
                                Error = "Oops ! Distance too far. Please check for another location"
                            };
                            errorResponse.FieldErrors.Add(fieldError);
                        }
                    }

                    // Return the JSON error response to the client-side JavaScript code
                    return Json(errorResponse);
                }

            }
            catch (Exception exc)
            {
                // If there's an exception, create an error response
                var errorResponse = new
                {
                    Success = false,
                    Message = "Door Dash Error processing the request." // You can include an error message if needed
                };

                await _logger.ErrorAsync(string.Format("Door Dash Error processing the request order Item Id :- ." + orderItemId), exc);

                // Return the JSON error response to the client-side JavaScript code
                return Json(errorResponse);
            }
        }

        [HttpPost]
        public virtual async Task<IActionResult> AcceptDispatchOrderItem(int orderItemId, string externalDeliveryId, int tipAmount, string dropOffInstruction)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            if (string.IsNullOrWhiteSpace(externalDeliveryId))
                return RedirectToAction("List");

            try
            {
                var orderItem = await _orderService.GetOrderItemByIdAsync(orderItemId)
                  ?? throw new ArgumentException("No order item found with the specified id");

                var order = await _orderService.GetOrderByIdAsync(orderItem.OrderId)
               ?? throw new ArgumentException("No order found with the specified id");

                var product = _productService.GetProductByIdAsync(orderItem.ProductId)
                    ?? throw new ArgumentException("No product found with the specified id");

                var vendor = await _vendorService.GetVendorByProductIdAsync(orderItem.ProductId)
                ?? throw new ArgumentException("No vendor found with the specified id");

                //Accept Quotes sent request to door dash
                var resultAcceptQuotes = await _orderDispatchService.AcceptDoorDashQuotesAsync(vendor, externalDeliveryId, tipAmount, dropOffInstruction);

                //Door Dash Status Code
                var statusAcceptQuotes = resultAcceptQuotes.StatusCode;

                if (statusAcceptQuotes == System.Net.HttpStatusCode.BadRequest)
                {
                    _notificationService.WarningNotification("Alchub.Admin.Dispatch.BadRequest");
                }
                else if (statusAcceptQuotes == System.Net.HttpStatusCode.Conflict)
                {
                    _notificationService.WarningNotification("Alchub.Admin.Dispatch.Conflict");
                }
                else if (statusAcceptQuotes == System.Net.HttpStatusCode.OK)
                {
                    //read string accept quotes
                    var resultStringAcceptQuotes = await resultAcceptQuotes.Content.ReadAsStringAsync();

                    // Get the response from the external API (if needed)
                    var responseQuotes = Newtonsoft.Json.JsonConvert.DeserializeObject<Root>(resultStringAcceptQuotes);

                    var slotOrderItems = (await _orderService.GetOrderItemsAsync(order.Id)).Where(o => o.SlotId == orderItem.SlotId && o.SlotTime.Equals(orderItem.SlotTime)).ToList();
                    foreach (var item in slotOrderItems)
                    {
                        Dispatch dispatch = new Dispatch();
                        dispatch.ExtrnalDeliveryId = externalDeliveryId;
                        dispatch.OrderItemId = item.Id;
                        dispatch.OrderNumber = order.Id;
                        dispatch.VendorId = vendor.Id;
                        dispatch.VendorName = vendor.Name;
                        dispatch.TimeSlot = orderItem.SlotStartTime.ToString("MM/dd/yyyy").Replace('/', '-') + " " + SlotHelper.ConvertTo12hoursSlotTime(orderItem.SlotTime);
                        dispatch.Fee = responseQuotes.fee;
                        dispatch.Tip = responseQuotes.tip;
                        dispatch.TrackingUrl = responseQuotes.tracking_url;
                        dispatch.DasherName = responseQuotes.dasher_name;
                        dispatch.DashPhoneNumber = $"{_twillioSettings.DefaultCountryCode}{responseQuotes.dasher_dropoff_phone_number}";
                        dispatch.DashVehicleNumber = $"{responseQuotes.dasher_vehicle_make}_{responseQuotes.dasher_vehicle_model}_{responseQuotes.dasher_vehicle_year}";
                        dispatch.CustomerSignature = responseQuotes.dropoff_signature_image_url;
                        dispatch.CreatedOnUtc = DateTime.UtcNow;

                        // Map delivery status
                        switch (responseQuotes.delivery_status)
                        {
                            case "created":
                                dispatch.DeliveryStatus = "Created";
                                break;
                            case "enroute_to_pickup":
                                dispatch.DeliveryStatus = "Dasher Assigned";
                                break;
                            case "picked_up":
                                dispatch.DeliveryStatus = "Delivery Picked Up";
                                //send email to customer.
                                await SendItemDispacthedNotificationsAndSaveNotesAsync(order, slotOrderItems);
                                break;
                            case "arrived_at_dropoff":
                                dispatch.DeliveryStatus = "Dasher arrived at Dropoff";
                                break;
                            case "delivered":
                                dispatch.DeliveryStatus = "Delivered";
                                await _orderProcessingService.OrderItemDeliveredAsync(order, orderItem);
                                break;
                            case "cancelled":
                                dispatch.DeliveryStatus = "Cancelled";
                                await _orderProcessingService.OrderItemCancelOrderAsync(order, item);
                                break;
                            default:
                                dispatch.DeliveryStatus = responseQuotes.delivery_status;
                                break;
                        }


                        //insert dispatch order details 
                        await _orderDispatchService.InsertDispatchAsync(dispatch);
                    }
                    _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Alchub.Admin.Dispatch.Successfully"));
                }
                return RedirectToAction("List");
            }
            catch (Exception exc)
            {
                await _logger.ErrorAsync(string.Format("Door Dash Error processing the request order Item Id :- ." + orderItemId), exc);
                return RedirectToAction("List");
            }
        }
        #endregion
    }
}