using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Alchub.Domain.Twillio;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Messages;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Stores;
using Nop.Core.Domain.Vendors;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Stores;
using Nop.Services.Vendors;
using Twilio;
using Twilio.Exceptions;
using Twilio.Rest.Api.V2010.Account;

namespace Nop.Services.Alchub.Twillio
{
    public class TwillioService : ITwillioService
    {
        #region Fields

        private readonly ISettingService _settingService;
        private readonly ILogger _logger;
        private readonly IStoreService _storeService;
        private readonly IStoreContext _storeContext;
        private readonly IMessageTokenProvider _messageTokenProvider;
        private readonly ITokenizer _tokenizer;
        private readonly ICustomerService _customerService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IOrderService _orderService;
        private readonly IEmailAccountService _emailAccountService;
        private readonly EmailAccountSettings _emailAccountSettings;
        private readonly IAddressService _addressService;
        private readonly INotificationService _notificationService;
        private readonly TwillioSettings _twillioSettings;
        private readonly IProductService _productService;
        private readonly IVendorService _vendorService;

        #endregion

        #region Ctor

        public TwillioService(ISettingService settingService,
            ILogger logger,
            IStoreService storeService,
            IStoreContext storeContext,
            IMessageTokenProvider messageTokenProvider,
            ITokenizer tokenizer,
            ICustomerService customerService,
            IGenericAttributeService genericAttributeService,
            IOrderService orderService,
            IEmailAccountService emailAccountService,
            EmailAccountSettings emailAccountSettings,
            IAddressService addressService,
            INotificationService notificationService,
            TwillioSettings twillioSettings,
            IProductService productService,
            IVendorService vendorService)
        {
            _settingService = settingService;
            _logger = logger;
            _storeService = storeService;
            _storeContext = storeContext;
            _messageTokenProvider = messageTokenProvider;
            _tokenizer = tokenizer;
            _customerService = customerService;
            _genericAttributeService = genericAttributeService;
            _orderService = orderService;
            _emailAccountService = emailAccountService;
            _emailAccountSettings = emailAccountSettings;
            _addressService = addressService;
            _notificationService = notificationService;
            _twillioSettings = twillioSettings;
            _productService = productService;
            _vendorService = vendorService;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Twillio sms service interface
        /// </summary>
        private async Task<string> SendSMS(string accountSid, string authToken, string body, string fromPhone, string toPhone, IList<Token> tokens, Order order = null)
        {
            //get email
            var emailAccount = (await _emailAccountService.GetEmailAccountByIdAsync(_emailAccountSettings.DefaultEmailAccountId)) ??
                               (await _emailAccountService.GetAllEmailAccountsAsync()).FirstOrDefault();

            //store
            var store = await _storeService.GetStoreByIdAsync(order.StoreId) ?? await _storeContext.GetCurrentStoreAsync();

            //add store tokens.
            var allTokens = new List<Token>(tokens);
            await _messageTokenProvider.AddStoreTokensAsync(allTokens, store, emailAccount);

            //Replace body tokens 
            var bodyReplaced = _tokenizer.Replace(body, allTokens, true);

            return await SendSMS(accountSid, authToken, bodyReplaced, fromPhone, toPhone, order);
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
        /// Get valid customer phone number to send SMS using twilio
        /// </summary>
        /// <param name="customer"></param>
        /// <param name="twillioSettings"></param>
        /// <returns></returns>
        private async Task<string> GetCustomerPhoneNumberWithISD(Order order)
        {
            //customer phone
            //var customerPhone = await _genericAttributeService.GetAttributeAsync<string>(customer, NopCustomerDefaults.PhoneAttribute);

            var billingAddress = await _addressService.GetAddressByIdAsync(order.BillingAddressId);
            if (billingAddress == null)
                throw new ArgumentNullException(nameof(billingAddress));

            if (string.IsNullOrEmpty(billingAddress.PhoneNumber))
                return billingAddress.PhoneNumber;

            //temp
            //billingAddress.PhoneNumber = "+917069845107"; //bhautik num

            //check if phone already contains isd prefix
            var isdCodeRegex = new Regex(@"^\+[1-9]\d{10,14}$", RegexOptions.IgnoreCase);
            if (isdCodeRegex.IsMatch(billingAddress.PhoneNumber))
                return billingAddress.PhoneNumber;

            //concate customer phone number with default country code
            return $"{_twillioSettings.DefaultCountryCode}{billingAddress.PhoneNumber}";
        }

        /// <summary>
        /// Get valid vendor phone number to send SMS using twilio
        /// </summary>
        /// <returns></returns>
        private string GetVendorPhoneNumberWithISD(Vendor vendor)
        {
            if (vendor == null)
                throw new ArgumentNullException(nameof(vendor));

            //get vendor phone
            var vendorPhone = vendor.PhoneNumber;
            if (string.IsNullOrEmpty(vendorPhone))
                return vendorPhone;

            //check if phone already contains isd prefix
            var isdCodeRegex = new Regex(@"^\+[1-9]\d{10,14}$", RegexOptions.IgnoreCase);
            if (isdCodeRegex.IsMatch(vendorPhone))
                return vendorPhone;

            //concate vendor phone number with default country code
            return $"{_twillioSettings.DefaultCountryCode}{vendorPhone}";
        }

        /// <summary>
        /// Get a list of vendors in order (order items)
        /// </summary>
        /// <param name="order">Order</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the vendors
        /// </returns>
        private async Task<IList<Vendor>> GetVendorsInOrderAsync(Order order)
        {
            var pIds = (await _orderService.GetOrderItemsAsync(order.Id)).Select(x => x.ProductId).ToArray();

            return await _vendorService.GetVendorsByProductIdsAsync(pIds);
        }

        #endregion

        /// <summary>
        /// Twillio sms service interface
        /// </summary>
        /// <param name="accountSid"></param>
        /// <param name="authToken"></param>
        /// <param name="body"></param>
        /// <param name="fromPhone"></param>
        /// <param name="toPhone"></param>
        /// <returns></returns>
        public async Task<string> SendSMS(string accountSid, string authToken, string body, string fromPhone, string toPhone, Order order = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(accountSid))
                    throw new NopException("accountSid is not set");

                if (string.IsNullOrWhiteSpace(authToken))
                    throw new NopException("authToken is not set");

                if (string.IsNullOrWhiteSpace(body))
                    throw new NopException("body is not set");

                if (string.IsNullOrWhiteSpace(fromPhone))
                    throw new NopException("fromPhone is not set");

                if (string.IsNullOrWhiteSpace(toPhone))
                    throw new NopException("toPhone is not set");

                //init twilio
                TwilioClient.Init(accountSid, authToken);

                //api call
                var message = await MessageResource.CreateAsync(
                    body: body,
                    from: new Twilio.Types.PhoneNumber(fromPhone),
                    to: new Twilio.Types.PhoneNumber(toPhone)
                );

                return message.Sid;
            }
            catch (ApiException e)
            {
                await _logger.ErrorAsync($"Twilio API Error OrderId: {order?.Id}, {e.Code} - {e.MoreInfo}", e);
                return string.Empty;
            }
            catch (Exception e)
            {
                await _logger.ErrorAsync($"Twilio config Error OrderId: {order?.Id}", e);
                return string.Empty;
            }
        }

        /// <summary>
        /// Send order items delivered customer SMS
        /// </summary>
        /// <param name="order"></param>
        /// <param name="orderItems"></param>
        /// <param name="languageId"></param>
        /// <returns></returns>
        public async Task SendOrderItemsStatusUpdatedCustomerSMSAsync(Order order, IList<OrderItem> orderItems, int languageId, OrderItemStatusActionType orderItemStatusActionType)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            if (orderItems == null)
                throw new ArgumentNullException(nameof(orderItems));

            //check twilio sms enabled
            if (!_twillioSettings.Enabled)
            {
                await AddOrderNoteAsync(order, $"\"Order Items {orderItemStatusActionType}\" SMS is not sent (to customer), because Twilio SMS functionality is not enabled.");
                return;
            }

            var customer = await _customerService.GetCustomerByIdAsync(order.CustomerId);
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            //prepare required param values
            //customer phone
            var customerPhone = await GetCustomerPhoneNumberWithISD(order);

            //tokens
            var commonTokens = new List<Token>();
            await _messageTokenProvider.AddOrderTokensAsync(commonTokens, order, languageId);
            await _messageTokenProvider.AddCustomerTokensAsync(commonTokens, order.CustomerId);

            //body
            string smsBody = string.Empty;
            switch (orderItemStatusActionType)
            {
                case OrderItemStatusActionType.Dispatched:
                    smsBody = _twillioSettings.OrderItemsDispatchedBody;
                    await _messageTokenProvider.AddOrderItemsDispacthedTokensAsync(commonTokens, order, orderItems, languageId);
                    break;
                case OrderItemStatusActionType.PickedUp:
                    smsBody = _twillioSettings.OrderItemsPickedUpBody;
                    await _messageTokenProvider.AddOrderItemsPickupCompletedTokensAsync(commonTokens, order, orderItems, languageId);
                    break;
                case OrderItemStatusActionType.Delivered:
                    smsBody = _twillioSettings.OrderItemsDeliveredBody;
                    await _messageTokenProvider.AddOrderItemsDeliveredTokensAsync(commonTokens, order, orderItems, languageId);
                    break;
                default:
                    break;
            }

            //send SMS
            var messageId = await SendSMS(_twillioSettings.AccountSid, _twillioSettings.AuthToken, smsBody,
                                          _twillioSettings.FromNumber, customerPhone, commonTokens, order);

            //order note
            if (!string.IsNullOrEmpty(messageId))
                await AddOrderNoteAsync(order, $"\"Order Items {orderItemStatusActionType}\" SMS (to customer) has been sent. Twilio message Sid: {messageId}.");
            else
            {
                await AddOrderNoteAsync(order, $"Error while sending \"Order Items {orderItemStatusActionType}\" SMS (to customer), please check error log with title Twilio API Error/Twilio config Error");
                _notificationService.WarningNotification($"Error while sending \"Order Items {orderItemStatusActionType}\" SMS (to customer), please check error log with title Twilio API Error/Twilio config Error");
            }
        }

        /// <summary>
        /// Send order placed customer SMS
        /// </summary>
        /// <param name="order"></param>
        /// <param name="languageId"></param>
        /// <returns></returns>
        public async Task SendOrderPlacedCustomerSMSAsync(Order order, int languageId)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            //check twilio sms enabled
            if (!_twillioSettings.Enabled)
            {
                await AddOrderNoteAsync(order, $"\"Order Placed\" SMS is not sent (to customer), because Twilio SMS functionality is not enabled.");
                return;
            }

            var customer = await _customerService.GetCustomerByIdAsync(order.CustomerId);
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            //prepare required param values
            //customer phone
            var customerPhone = await GetCustomerPhoneNumberWithISD(order);

            //tokens
            var commonTokens = new List<Token>();
            await _messageTokenProvider.AddOrderTokensAsync(commonTokens, order, languageId);
            await _messageTokenProvider.AddCustomerTokensAsync(commonTokens, order.CustomerId);

            //body
            string smsBody = _twillioSettings.OrderPlacedBody;

            //send SMS
            var messageId = await SendSMS(_twillioSettings.AccountSid, _twillioSettings.AuthToken, smsBody,
                                          _twillioSettings.FromNumber, customerPhone, commonTokens, order);

            //order note
            if (!string.IsNullOrEmpty(messageId))
                await AddOrderNoteAsync(order, $"\"Order Placed\" SMS (to customer) has been sent. Twilio message Sid: {messageId}.");
            else
            {
                await AddOrderNoteAsync(order, $"Error while sending \"Order Placed\" SMS (to customer), please check error log with title Twilio API Error/Twilio config Error");
            }
        }


        /// <summary>
        /// Send order placed customer SMS
        /// </summary>
        /// <param name="order"></param>
        /// <param name="languageId"></param>
        /// <returns></returns>
        public async Task SendOrderItemCancelCustomerSMSAsync(Order order, OrderItem orderItem, int languageId)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            //check twilio sms enabled
            if (!_twillioSettings.Enabled)
            {
                await AddOrderNoteAsync(order, $"\"Order Item Cancel\" SMS is not sent (to customer), because Twilio SMS functionality is not enabled.");
                return;
            }

            var customer = await _customerService.GetCustomerByIdAsync(order.CustomerId);
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            //prepare required param values
            //customer phone
            var customerPhone = await GetCustomerPhoneNumberWithISD(order);

            //tokens
            var commonTokens = new List<Token>();
            await _messageTokenProvider.AddOrderItemsCancelVendorTokensAsync(commonTokens, order, orderItem, languageId);
            await _messageTokenProvider.AddOrderTokensAsync(commonTokens, order, languageId);
            await _messageTokenProvider.AddCustomerTokensAsync(commonTokens, order.CustomerId);

            //body
            string smsBody = _twillioSettings.OrderItemsCancelBody;

            //send SMS
            var messageId = await SendSMS(_twillioSettings.AccountSid, _twillioSettings.AuthToken, smsBody,
                                          _twillioSettings.FromNumber, customerPhone, commonTokens, order);

            //order note
            if (!string.IsNullOrEmpty(messageId))
                await AddOrderNoteAsync(order, $"\"Order Item Cancel\" SMS (to customer) has been sent. Twilio message Sid: {messageId}.");
            else
            {
                await AddOrderNoteAsync(order, $"Error while sending \"Order Item Cancel\" SMS (to customer), please check error log with title Twilio API Error/Twilio config Error");
            }
        }

        /// <summary>
        /// Send order placed customer SMS
        /// </summary>
        /// <param name="order"></param>
        /// <param name="languageId"></param>
        /// <returns></returns>
        public async Task SendOrderItemDeliveryDeniedCustomerSMSAsync(Order order, OrderItem orderItem, int languageId)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            //check twilio sms enabled
            if (!_twillioSettings.Enabled)
            {
                await AddOrderNoteAsync(order, $"\"Order Item Delivery Denied\" SMS is not sent (to customer), because Twilio SMS functionality is not enabled.");
                return;
            }

            var customer = await _customerService.GetCustomerByIdAsync(order.CustomerId);
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            //prepare required param values
            //customer phone
            var customerPhone = await GetCustomerPhoneNumberWithISD(order);

            //tokens
            var commonTokens = new List<Token>();
            await _messageTokenProvider.AddOrderItemsCancelVendorTokensAsync(commonTokens, order, orderItem, languageId);
            await _messageTokenProvider.AddOrderTokensAsync(commonTokens, order, languageId);
            await _messageTokenProvider.AddCustomerTokensAsync(commonTokens, order.CustomerId);

            //body
            string smsBody = _twillioSettings.OrderItemsDelivereyDeniedBody;

            //send SMS
            var messageId = await SendSMS(_twillioSettings.AccountSid, _twillioSettings.AuthToken, smsBody,
                                          _twillioSettings.FromNumber, customerPhone, commonTokens, order);

            //order note
            if (!string.IsNullOrEmpty(messageId))
                await AddOrderNoteAsync(order, $"\"Order Item Delivery Denied\" SMS (to customer) has been sent. Twilio message Sid: {messageId}.");
            else
            {
                await AddOrderNoteAsync(order, $"Error while sending \"Order Item Delivery Denied\" SMS (to customer), please check error log with title Twilio API Error/Twilio config Error");
            }
        }


        public async Task SendOrderItemPickupCustomerSMSAsync(Order order, OrderItem orderItem, int languageId)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            //check twilio sms enabled
            if (!_twillioSettings.Enabled)
            {
                await AddOrderNoteAsync(order, $"\"Send email order Item Pickup\" SMS is not sent (to customer), because Twilio SMS functionality is not enabled.");
                return;
            }

            var customer = await _customerService.GetCustomerByIdAsync(order.CustomerId);
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            //prepare required param values
            //customer phone
            var customerPhone = await GetCustomerPhoneNumberWithISD(order);

            //tokens
            var commonTokens = new List<Token>();
            await _messageTokenProvider.AddOrderTokensAsync(commonTokens, order, languageId);
            await _messageTokenProvider.AddCustomerTokensAsync(commonTokens, order.CustomerId);

            //body
            string smsBody = _twillioSettings.OrderItemPickupBody;

            //send SMS
            var messageId = await SendSMS(_twillioSettings.AccountSid, _twillioSettings.AuthToken, smsBody,
                                          _twillioSettings.FromNumber, customerPhone, commonTokens, order);

            //order note
            if (!string.IsNullOrEmpty(messageId))
                await AddOrderNoteAsync(order, $"\"Send email order Item Pickup\" SMS (to customer) has been sent. Twilio message Sid: {messageId}.");
            else
            {
                await AddOrderNoteAsync(order, $"Error while sending \"Send email order Item Pickup\" SMS (to customer), please check error log with title Twilio API Error/Twilio config Error");
            }
        }

        /// <summary>
        /// Send order placed vendor SMS
        /// </summary>
        /// <param name="order"></param>
        /// <param name="languageId"></param>
        /// <returns></returns>
        public async Task SendOrderPlacedVendorSMSAsync(Order order, int languageId)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            //check twilio sms enabled
            if (!_twillioSettings.Enabled)
            {
                await AddOrderNoteAsync(order, $"\"Order Placed\" SMS is not sent (to any vendor), because Twilio SMS functionality is not enabled.");
                return;
            }

            //get all vendors of the order
            var vendors = await GetVendorsInOrderAsync(order);
            foreach (var vendor in vendors)
            {
                //prepare required param values
                //vendor phone
                var vendorPhone = GetVendorPhoneNumberWithISD(vendor);

                //tokens
                var commonTokens = new List<Token>();
                await _messageTokenProvider.AddOrderTokensAsync(commonTokens, order, languageId);
                await _messageTokenProvider.AddCustomerTokensAsync(commonTokens, order.CustomerId);
                await _messageTokenProvider.AddVendorTokensAsync(commonTokens, vendor);

                //body
                string smsBody = _twillioSettings.OrderPlacedVendorBody;

                //send SMS
                var messageId = await SendSMS(_twillioSettings.AccountSid, _twillioSettings.AuthToken, smsBody,
                                              _twillioSettings.FromNumber, vendorPhone, commonTokens, order);

                //order note
                if (!string.IsNullOrEmpty(messageId))
                    await AddOrderNoteAsync(order, $"\"Order Placed\" SMS (to vendor: {vendor.Name}({vendorPhone})) has been sent. Twilio message Sid: {messageId}.");
                else
                {
                    await AddOrderNoteAsync(order, $"Error while sending \"Order Placed\" SMS (to vendor: {vendor.Name}({vendorPhone})), please check error log with title Twilio API Error/Twilio config Error");
                }
            }
        }
    }
}
