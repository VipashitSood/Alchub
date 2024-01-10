using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using Nop.Core.Alchub.Domain;
using Nop.Core.Alchub.Domain.Orders;
using Nop.Core.Alchub.Domain.Twillio;
using Nop.Core.Alchub.Domain.Vendors;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Vendors;
using Nop.Core.Events;
using Nop.Data;
using Nop.Services.Alchub.Slots;
using Nop.Services.Common;
using Nop.Services.Directory;
using Nop.Services.Logging;
using Nop.Services.Vendors;

namespace Nop.Services.Orders
{
    /// <summary>
    /// Dispatch service
    /// </summary>
    public partial class OrderDispatchService : IOrderDispatchService
    {
        #region Fields

        private readonly IRepository<Dispatch> _dispatchRepository;
        private readonly IEventPublisher _eventPublisher;
        private readonly IRepository<Address> _addressRepository;
        private readonly IRepository<Order> _orderRepository;
        private readonly IRepository<OrderItem> _orderItemRepository;
        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<Vendor> _vendorRepository;
        private readonly AlchubSettings _alchubSettings;
        private readonly IAddressService _addressService;
        private readonly ICountryService _countryService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ILogger _logger;
        private readonly TwillioSettings _twillioSettings;
        private readonly ICurrencyService _currencyService;
        private readonly CurrencySettings _currencySettings;
        private readonly IOrderService _orderService;
        private readonly IVendorAttributeService _vendorAttributeService;
        private readonly IVendorAttributeParser _vendorAttributeParser;
        #endregion

        #region Ctor

        public OrderDispatchService
        (
            IRepository<Dispatch> dispatchRepository,
            IEventPublisher eventPublisher,
            IRepository<Address> addressRepository,
            IRepository<Order> orderRepository,
            IRepository<OrderItem> orderItemRepository,
            IRepository<Product> productRepository,
            IRepository<Vendor> vendorRepository,
            AlchubSettings alchubSettings,
            IAddressService addressService,
            ICountryService countryService,
            IGenericAttributeService genericAttributeService,
            ILogger logger,
            TwillioSettings twillioSettings,
            CurrencySettings currencySettings,
            ICurrencyService currencyService,
            IOrderService orderService,
            IVendorAttributeService vendorAttributeService,
            IVendorAttributeParser vendorAttributeParser)
        {

            _dispatchRepository = dispatchRepository;
            _eventPublisher = eventPublisher;
            _addressRepository = addressRepository;
            _orderRepository = orderRepository;
            _orderItemRepository = orderItemRepository;
            _productRepository = productRepository;
            _vendorRepository = vendorRepository;
            _alchubSettings = alchubSettings;
            _addressService = addressService;
            _countryService = countryService;
            _genericAttributeService = genericAttributeService;
            _logger = logger;
            _twillioSettings = twillioSettings;
            _currencySettings = currencySettings;
            _currencyService = currencyService;
            _orderService = orderService;
            _vendorAttributeService = vendorAttributeService;
            _vendorAttributeParser = vendorAttributeParser;
        }

        #endregion

        #region Utils

        //Convert To Time details
        private string TimeDetailsToISO8601(DateTime time)
        {
            // Get the time in the timestamps are in UTC
            string timeDetails = time.ToString("yyyy-MM-ddTHH:mm:ssZ");

            return timeDetails;
        }

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

        private async Task<(string externalBussinessId, string externalStoreId)> GetVendorAttributeValuesParam(Vendor vendor)
        {
            //get door dash vendor attribute details
            //get available vendor attributes
            var pickup_external_business_id = string.Empty;
            var pickup_external_store_id = string.Empty;
            var vendorAttributes = await _vendorAttributeService.GetAllVendorAttributesAsync();
            if (vendorAttributes?.Any() == true)
            {
                //vendor attribute in generic table
                var selectedVendorAttributes = await _genericAttributeService.GetAttributeAsync<string>(vendor, NopVendorDefaults.VendorAttributes);

                //external bussiness id
                if (!string.IsNullOrEmpty(selectedVendorAttributes))
                {
                    var doorDashExternalBusinessIdAtt = vendorAttributes.FirstOrDefault(x => x.Name == AlchubVendorDefaults.DoorDashExternalBusinessId);
                    if (doorDashExternalBusinessIdAtt != null)
                    {
                        var enteredText = _vendorAttributeParser.ParseValues(selectedVendorAttributes, doorDashExternalBusinessIdAtt.Id);
                        if (enteredText.Any())
                            pickup_external_business_id = enteredText[0];
                    }

                    //external store id
                    var doorDashExternalStoreIdAtt = vendorAttributes.FirstOrDefault(x => x.Name == AlchubVendorDefaults.DoorDashExternalStoreId);
                    if (doorDashExternalStoreIdAtt != null)
                    {
                        var enteredText = _vendorAttributeParser.ParseValues(selectedVendorAttributes, doorDashExternalStoreIdAtt.Id);
                        if (enteredText.Any())
                            pickup_external_store_id = enteredText[0];
                    }
                }
            }

            return (pickup_external_business_id, pickup_external_store_id);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Insert Dispatch 
        /// </summary>
        /// <param name="dispatch">Dispatch</param>
        /// <returns></returns>
        public virtual async Task InsertDispatchAsync(Dispatch dispatch)
        {
            if (dispatch == null)
                throw new ArgumentNullException(nameof(dispatch));

            await _dispatchRepository.InsertAsync(dispatch);

            //event notification
            await _eventPublisher.EntityInsertedAsync(dispatch);
        }

        /// <summary>
        /// Update Dispatch
        /// </summary>
        /// <param name="dispatch">Dispatch</param>
        /// <returns></returns>
        public virtual async Task UpdateDispatchAsync(Dispatch dispatch)
        {
            if (dispatch == null)
                throw new ArgumentNullException(nameof(dispatch));

            await _dispatchRepository.UpdateAsync(dispatch);

            //event notification
            await _eventPublisher.EntityUpdatedAsync(dispatch);
        }


        /// <summary>
        /// Delete multiple list of dispatch
        /// </summary>
        /// <param name="dispatch">Dispatch</param>
        /// <returns></returns>
        public virtual async Task DeleteDispatchAsync(IList<Dispatch> dispatch)
        {
            if (dispatch == null)
                throw new ArgumentNullException(nameof(dispatch));

            foreach (var item in dispatch)
            {
                await _dispatchRepository.DeleteAsync(item);
            }
        }

        /// <summary>
        ///  Delete Dispatch
        /// </summary>
        /// <param name="dispatch">Dispatch</param>
        /// <returns></returns>
        public virtual async Task DeleteDispatchAsync(Dispatch dispatch)
        {
            if (dispatch == null)
                throw new ArgumentNullException(nameof(dispatch));

            await _dispatchRepository.DeleteAsync(dispatch);

        }


        /// <summary>
        /// Search Order with Vendor Name and Time Slot
        /// </summary>
        /// <param name="vendorId">Vendor identifier; 0 to load all records</param>
        /// <returns>Result query get list of order and vendor its items</returns>
        public virtual async Task<IList<Dispatch>> GetOrderItemVendorTimeSlotsListAysc(int vendorId = 0)
        {

            var currentDate = DateTime.UtcNow;

            var orderItemsWithVendorInfo = from orderItem in _orderItemRepository.Table
                                           join o in _orderRepository.Table on orderItem.OrderId equals o.Id
                                           join p in _productRepository.Table on orderItem.ProductId equals p.Id
                                           join oba in _addressRepository.Table on o.BillingAddressId equals oba.Id
                                           join v in _vendorRepository.Table on p.VendorId equals v.Id
                                           join d in _dispatchRepository.Table on orderItem.Id equals d.OrderItemId into dispatchGroup
                                           from d in dispatchGroup.DefaultIfEmpty()
                                           where !o.Deleted && o.OrderStatusId != (int)OrderStatus.Cancelled &&
                                                 !p.Deleted &&
                                                 (vendorId == 0 || p.VendorId == vendorId) &&
                                                 (orderItem.OrderItemStatusId == (int)Core.Alchub.Domain.Orders.OrderItemStatus.Pending ||
                                                 orderItem.OrderItemStatusId == (int)Core.Alchub.Domain.Orders.OrderItemStatus.Dispatch) &&
                                                 orderItem.InPickup == false &&
                                                 (d == null || d.DeliveryStatus != "Delivered") // Exclude records with DeliveryStatus = "delivery"
                                           orderby orderItem.SlotStartTime descending, o.Id descending // Order by SlotStartTime descending and OrderNumber (o.Id) descending
                                           select new
                                           {
                                               OrderItemId = orderItem.Id,
                                               OrderNumber = o.Id,
                                               VendorId = v.Id,
                                               VendorName = v.Name,
                                               orderItem.SlotStartTime,
                                               orderItem.SlotTime,
                                               TrackingUrl = (d != null) ? d.TrackingUrl : "",
                                               DeliveryStatus = (d != null) ? d.DeliveryStatus : "",
                                               DasherName = (d != null) ? d.DasherName : "",
                                               DashPhoneNumber = (d != null) ? d.DashPhoneNumber : "",
                                               DashVehicleNumber = (d != null) ? d.DashVehicleNumber : "",
                                               CustomerSignature = (d != null) ? d.CustomerSignature : "",
                                           };


            var uniqueTimeSlotOrders = orderItemsWithVendorInfo.ToList()
                                                              .GroupBy(x => new { x.VendorId, x.SlotStartTime, x.OrderNumber }) // Group by VendorId, SlotStartTime, and OrderNumber
                                                              .Select(group => new Dispatch
                                                              {
                                                                  OrderItemId = group.First().OrderItemId,
                                                                  OrderNumber = group.First().OrderNumber,
                                                                  VendorId = group.First().VendorId,
                                                                  VendorName = group.First().VendorName,
                                                                  TimeSlot = group.First().SlotStartTime.ToString("MM/dd/yyyy") + " - " + SlotHelper.ConvertTo12hoursSlotTime(group.First().SlotTime),
                                                                  TrackingUrl = group.First().TrackingUrl, // Include the TrackingUrl property
                                                                  DeliveryStatus = group.First().DeliveryStatus, // Include the DeliveryStatus property
                                                                  DasherName = group.First().DasherName,  // Include the DasherName property
                                                                  DashPhoneNumber = group.First().DashPhoneNumber, // Include the DashPhoneNumber property
                                                                  DashVehicleNumber = group.First().DashVehicleNumber,  // Include the DashVehicleNumber property
                                                                  CustomerSignature = group.First().CustomerSignature,   // Include the CustomerSignature property
                                                              }).ToList();

            return uniqueTimeSlotOrders;
        }

        /// <summary>
        ///  Get Dispatch External Delivery Id
        /// </summary>
        /// <returns></returns>
        public virtual async Task<IList<Dispatch>> GetDispatchOrderOneWeekAsync()
        {
            DateTime oneWeekAgo = DateTime.UtcNow.AddDays(-7);
            var query = _dispatchRepository.Table;
            query = query.Where(c => c.DeliveryStatus.ToLower() != "delivered" && c.DeliveryStatus.ToLower() != "cancelled" && c.CreatedOnUtc >= oneWeekAgo);
            return await query.ToListAsync();
        }

        /// <summary>
        ///  Get Dispatch OrderItem TrackingUrl
        /// </summary>
        /// <param name="orderItemId">orderItemId</param>
        /// <returns></returns>
        public virtual async Task<string> GetDispatchOrderItemIdTrackingUrlAsync(int orderItemId)
        {
            var query = _dispatchRepository.Table;
            if (orderItemId > 0)
            {
                query = query.Where(c => c.OrderItemId == orderItemId);
            }

            var dispatch = await query.FirstOrDefaultAsync();
            if (dispatch != null)
            {
                // Check if the TrackingUrl is not null and return it
                if (dispatch.TrackingUrl != null)
                {
                    return dispatch.TrackingUrl;
                }
            }
            // Return an empty string if dispatch is null or TrackingUrl is null
            return string.Empty;
        }

        #endregion

        #region Door Dash API Request


        /// <summary>
        ///  Get Quotes Request Door Dash
        /// </summary>
        /// <param name="customer">customer</param>
        /// <param name="vendor">vendor</param>
        /// <param name="order">order</param>
        /// <param name="orderItem">orderItem</param>
        /// <returns> Return Request Get Door Dash Quotes details like Fees</returns>
        public virtual async Task<HttpResponseMessage> GetDoorDashQuotesAsync(Customer customer, Vendor vendor, Order order, OrderItem orderItem)
        {
            try
            {
                var currentTime = DateTime.Now;
                bool isAlcohol = false;
                var shippingAddress = await _addressService.GetAddressByIdAsync(order.ShippingAddressId.Value);
                var shippingCountry = await _countryService.GetCountryByAddressAsync(shippingAddress);

                var firstName = await _genericAttributeService.GetAttributeAsync<string>(customer, NopCustomerDefaults.FirstNameAttribute);
                var lastName = await _genericAttributeService.GetAttributeAsync<string>(customer, NopCustomerDefaults.LastNameAttribute);

                //get product 
                var productItems = new List<object>();
                var slotOrderItems = (await _orderService.GetOrderItemsAsync(order.Id)).Where(o => o.SlotId == orderItem.SlotId && o.SlotTime.Equals(orderItem.SlotTime) && o.OrderItemStatus == OrderItemStatus.Pending).ToList();

                //create object of items which sent to door dash
                if (slotOrderItems.Any())
                {
                    foreach (var item in slotOrderItems)
                    {
                        var product = await _productRepository.GetByIdAsync(item.ProductId);

                        // Create an item object and add it to the list
                        var productItem = new
                        {
                            name = product != null ? product.Name : "",
                            quantity = item.Quantity,

                        };
                        if (product.IsAlcohol)
                        {
                            isAlcohol = true;
                        }
                        productItems.Add(productItem);
                    }
                }


                var addseconds = _alchubSettings.AlchubDoorDashExternalDeliveryAddSeconds;
                if (addseconds == 0)
                {
                    addseconds = 60; // Default value: 60 seconds
                }
                var externalDeliveryId = $"{order.Id}_{orderItem.Id}_{currentTime.AddSeconds(addseconds).ToString("yyyyMMddHHmmss")}";

                var accessKey = new Dictionary<string, string>
                {
                {"developer_id", _alchubSettings.AlchubDoorDashDeveloperId},
                {"key_id", _alchubSettings.AlchubDoorDashKeyId},
                {"signing_secret", _alchubSettings.AlchubDoorDashSigningSecret}
                };

                var decodedSecret = Base64UrlEncoder.DecodeBytes(accessKey["signing_secret"]);
                var securityKey = new SymmetricSecurityKey(decodedSecret);
                var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
                var header = new JwtHeader(credentials);
                header["dd-ver"] = "DD-JWT-V1";

                var payload = new JwtPayload(
                    issuer: accessKey["developer_id"],
                    audience: "doordash",
                    claims: new List<Claim> { new Claim("kid", accessKey["key_id"]) },
                    notBefore: null,
                    expires: System.DateTime.UtcNow.AddSeconds(300),
                    issuedAt: System.DateTime.UtcNow);

                var securityToken = new JwtSecurityToken(header, payload);
                var token = new JwtSecurityTokenHandler().WriteToken(securityToken);

                //get door dash vendor attribute details
                var (pickup_external_business_id, pickup_external_store_id) = await GetVendorAttributeValuesParam(vendor);

                ///Get a quote
                var jsonContentQuotes = JsonSerializer.Serialize(new
                {
                    external_delivery_id = externalDeliveryId,
                    pickup_address = vendor.PickupAddress,
                    pickup_business_name = vendor.Name,
                    pickup_phone_number = $"{_twillioSettings.DefaultCountryCode}{vendor.PhoneNumber}",
                    pickup_instructions = vendor.Description,
                    pickup_reference_tag = order.Id + "_" + orderItem.Id,
                    dropoff_address = (shippingAddress?.Address2 ?? string.Empty) + " " + (shippingAddress?.Address1 ?? string.Empty) + " " + (shippingAddress?.ZipPostalCode ?? string.Empty) + " " + (shippingAddress?.City ?? string.Empty) + " " + (shippingCountry?.Name ?? string.Empty),
                    dropoff_business_name = firstName + " " + lastName,
                    dropoff_phone_number = $"{_twillioSettings.DefaultCountryCode}{shippingAddress.PhoneNumber}",
                    delivery_status = "quote",
                    currency = (await _currencyService.GetCurrencyByIdAsync(_currencySettings.PrimaryStoreCurrencyId))?.CurrencyCode, //get the primary store currency
                    pickup_time = TimeDetailsToISO8601(DateTime.UtcNow),
                    action_if_undeliverable = "return_to_pickup",
                    order_contains = new { alcohol = isAlcohol },
                    dropoff_contact_send_notifications = false,
                    dropoff_requires_signature = true,
                    dropoff_contact_given_name = firstName + " " + lastName,
                    dropoff_contact_family_name = firstName + " " + lastName,
                    // Add the list of product items to the "items" field
                    items = productItems,
                    //22-11-23
                    pickup_external_business_id = pickup_external_business_id,
                    pickup_external_store_id = pickup_external_store_id
                });
                var contentQuotes = new StringContent(jsonContentQuotes, Encoding.UTF8, "application/json");

                HttpClient clientQuotes = new HttpClient();
                clientQuotes.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                var resultQuotes = await clientQuotes.PostAsync("https://openapi.doordash.com/drive/v2/quotes", contentQuotes);
                return resultQuotes;
            }
            catch (Exception exc)
            {
                // If there's an exception, create an error response
                var errorResponse = new ErrorResponse
                {
                    Success = false,
                    Message = "Error processing the request." // You can include an error message if needed
                };

                // Convert the error response to JSON
                var errorResponseJson = Newtonsoft.Json.JsonConvert.SerializeObject(errorResponse);

                // Create a new HttpResponseMessage with the JSON content
                var httpResponse = new HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent(errorResponseJson, Encoding.UTF8, "application/json")
                };

                await _logger.ErrorAsync(string.Format("Door Dash Error processing the request create order id :- ." + orderItem.Id), exc);

                // Return the error response as a HttpResponseMessage
                return httpResponse;
            }
        }

        /// <summary>
        ///  Accept Quote send request to door dash
        /// </summary>
        /// <param name="externalDeliveryId">externalDeliveryId</param>
        /// <param name="tipAmount">tipAmount</param>
        /// <param name="dropOffInstruction">dropOffInstruction</param>
        /// <returns>Return get tracking url Dasher details</returns>
        public virtual async Task<HttpResponseMessage> AcceptDoorDashQuotesAsync(Vendor vendor, string externalDeliveryId, int tipAmount = 0, string dropOffInstruction = "")
        {
            try
            {
                /// External delivery Id
                if (string.IsNullOrEmpty(externalDeliveryId))
                {
                    throw new ArgumentException("No External Delivery Id Found");
                }


                var accessKey = new Dictionary<string, string>
                {
                {"developer_id", _alchubSettings.AlchubDoorDashDeveloperId},
                {"key_id", _alchubSettings.AlchubDoorDashKeyId},
                {"signing_secret", _alchubSettings.AlchubDoorDashSigningSecret}
                };

                var decodedSecret = Base64UrlEncoder.DecodeBytes(accessKey["signing_secret"]);
                var securityKey = new SymmetricSecurityKey(decodedSecret);
                var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
                var header = new JwtHeader(credentials);
                header["dd-ver"] = "DD-JWT-V1";

                var payload = new JwtPayload(
                    issuer: accessKey["developer_id"],
                    audience: "doordash",
                    claims: new List<Claim> { new Claim("kid", accessKey["key_id"]) },
                    notBefore: null,
                    expires: System.DateTime.UtcNow.AddSeconds(300),
                    issuedAt: System.DateTime.UtcNow);

                var securityToken = new JwtSecurityToken(header, payload);
                var token = new JwtSecurityTokenHandler().WriteToken(securityToken);

                //Converted into cents.
                tipAmount = tipAmount * 100;

                //get door dash vendor attribute details
                var (pickup_external_business_id, pickup_external_store_id) = await GetVendorAttributeValuesParam(vendor);

                //Accept Quote
                var jsonContentAcceptQuotes = JsonSerializer.Serialize(new
                {
                    tip = tipAmount,
                    dropoff_instructions = dropOffInstruction,
                    //22-11-23
                    pickup_external_business_id = pickup_external_business_id,
                    pickup_external_store_id = pickup_external_store_id
                });
                var contentAcceptQuotes = new StringContent(jsonContentAcceptQuotes, Encoding.UTF8, "application/json");

                HttpClient clientAcceptQuotes = new HttpClient();
                clientAcceptQuotes.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                var resultAcceptQuotes = await clientAcceptQuotes.PostAsync("https://openapi.doordash.com/drive/v2/quotes/" + externalDeliveryId + "/accept", contentAcceptQuotes);
                return resultAcceptQuotes;
            }
            catch (Exception exc)
            {
                // If there's an exception, create an error response
                var errorResponse = new ErrorResponse
                {
                    Success = false,
                    Message = "Door Dash Error processing the request." // You can include an error message if needed
                };

                // Convert the error response to JSON
                var errorResponseJson = Newtonsoft.Json.JsonConvert.SerializeObject(errorResponse);

                // Create a new HttpResponseMessage with the JSON content
                var httpResponse = new HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent(errorResponseJson, Encoding.UTF8, "application/json")
                };

                await _logger.ErrorAsync(string.Format("Door Dash Error processing the request external delivery id :- ." + externalDeliveryId), exc);
                // Return the error response as a HttpResponseMessage
                return httpResponse;
            }
        }


        /// <summary>
        ///  Get status of delivery from Door dash
        /// </summary>
        /// <param name="externalDeliveryId">ExternalDeliveryId</param>
        /// <returns>Return delivery status and Dasher details</returns>
        public virtual async Task<HttpResponseMessage> GetDoorDashDeliveryAsync(string externalDeliveryId = "")
        {
            try
            {
                var deliveryId = externalDeliveryId
                ?? throw new ArgumentException("No External Delivery Id Found");

                var accessKey = new Dictionary<string, string>
                {
                {"developer_id", _alchubSettings.AlchubDoorDashDeveloperId},
                {"key_id", _alchubSettings.AlchubDoorDashKeyId},
                {"signing_secret", _alchubSettings.AlchubDoorDashSigningSecret}
                };

                var decodedSecret = Base64UrlEncoder.DecodeBytes(accessKey["signing_secret"]);
                var securityKey = new SymmetricSecurityKey(decodedSecret);
                var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
                var header = new JwtHeader(credentials);
                header["dd-ver"] = "DD-JWT-V1";

                var payload = new JwtPayload(
                    issuer: accessKey["developer_id"],
                    audience: "doordash",
                    claims: new List<Claim> { new Claim("kid", accessKey["key_id"]) },
                    notBefore: null,
                    expires: System.DateTime.UtcNow.AddSeconds(300),
                    issuedAt: System.DateTime.UtcNow);

                var securityToken = new JwtSecurityToken(header, payload);
                var token = new JwtSecurityTokenHandler().WriteToken(securityToken);

                HttpClient clientGetQuotes = new HttpClient();
                clientGetQuotes.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                var resultGetQuotes = await clientGetQuotes.GetAsync("https://openapi.doordash.com/drive/v2/deliveries/" + deliveryId);
                return resultGetQuotes;
            }
            catch (Exception exc)
            {
                // If there's an exception, create an error response
                var errorResponse = new ErrorResponse
                {
                    Success = false,
                    Message = "Error processing the request." // You can include an error message if needed
                };

                // Convert the error response to JSON
                var errorResponseJson = Newtonsoft.Json.JsonConvert.SerializeObject(errorResponse);

                // Create a new HttpResponseMessage with the JSON content
                var httpResponse = new HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent(errorResponseJson, Encoding.UTF8, "application/json")
                };

                await _logger.ErrorAsync(string.Format("Door Dash Error processing the request external delivery id :- ." + externalDeliveryId), exc);
                // Return the error response as a HttpResponseMessage
                return httpResponse;
            }
        }
        #endregion


    }
}