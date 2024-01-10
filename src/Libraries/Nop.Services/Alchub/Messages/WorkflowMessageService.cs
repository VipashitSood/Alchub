using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core.Domain.Messages;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Vendors;

namespace Nop.Services.Messages
{
    public partial class WorkflowMessageService : IWorkflowMessageService
    {
        #region Import
        /// <summary>
        /// Sends email to vendor for vendor products failed to import
        /// </summary>
        /// <param name="languageId"></param>
        /// <param name="vendor"></param>
        /// <param name="body"></param>
        /// <returns></returns>
        public virtual async Task<IList<int>> SendInvalidProductMessageForVendor(int languageId,
            Vendor vendor, string body)
        {
            var store = await _storeContext.GetCurrentStoreAsync();
            languageId = await EnsureLanguageIsActiveAsync(languageId, store.Id);

            var messageTemplates = await GetActiveMessageTemplatesAsync(MessageTemplateSystemNames.InvalidProductMessageForVendor, store.Id);
            if (!messageTemplates.Any())
                return new List<int>();

            return await messageTemplates.SelectAwait(async messageTemplate =>
            {
                //email account
                var emailAccount = await GetEmailAccountOfMessageTemplateAsync(messageTemplate, languageId);
                //var superAdminName = await _localizationService.GetResourceAsync("Alchub.SuperAdmin.Name");

                var tokens = new List<Token>();
                await _messageTokenProvider.AddStoreTokensAsync(tokens, store, emailAccount);
                await _messageTokenProvider.AddVendorTokensAsync(tokens, vendor);
                //tokens.Add(new Token("SuperAdmin.Name", superAdminName, true));
                tokens.Add(new Token("Body", body, true));

                return await SendNotificationAsync(messageTemplate, emailAccount, languageId, tokens, vendor.Email, vendor.Name,
                    fromEmail: emailAccount?.Email,
                    fromName: emailAccount?.Username,
                    subject: messageTemplate?.Subject,
                    replyToEmailAddress: emailAccount?.Email,
                    replyToName: emailAccount?.Username);
            }).ToListAsync();
        }

        /// <summary>
        /// Sends email to admin for vendor products failed to import
        /// </summary>
        /// <param name="languageId"></param>
        /// <param name="vendor"></param>
        /// <param name="body"></param>
        /// <returns></returns>
        public virtual async Task<IList<int>> SendInvalidProductMessage(int languageId,
            Vendor vendor, string body)
        {
            var store = await _storeContext.GetCurrentStoreAsync();
            languageId = await EnsureLanguageIsActiveAsync(languageId, store.Id);

            var messageTemplates = await GetActiveMessageTemplatesAsync(MessageTemplateSystemNames.InvalidProductMessage, store.Id);
            if (!messageTemplates.Any())
                return new List<int>();

            return await messageTemplates.SelectAwait(async messageTemplate =>
            {
                //email account
                var emailAccount = await GetEmailAccountOfMessageTemplateAsync(messageTemplate, languageId);
                //var superAdminName = await _localizationService.GetResourceAsync("Alchub.SuperAdmin.Name");
                //var superAdminEmailAddress = await _localizationService.GetResourceAsync("Alchub.SuperAdmin.Email");

                var tokens = new List<Token>();
                await _messageTokenProvider.AddStoreTokensAsync(tokens, store, emailAccount);
                await _messageTokenProvider.AddVendorTokensAsync(tokens, vendor);
                tokens.Add(new Token("SuperAdmin.Name", emailAccount.DisplayName, true));
                tokens.Add(new Token("Body", body, true));

                var toEmail = emailAccount.Email;
                var toName = emailAccount.DisplayName;

                //send email to super admin
                return await SendNotificationAsync(messageTemplate, emailAccount, languageId, tokens, toEmail, toName,
                    fromEmail: emailAccount?.Email,
                    fromName: emailAccount?.Username,
                    subject: messageTemplate?.Subject,
                    replyToEmailAddress: emailAccount?.Email,
                    replyToName: emailAccount?.Username);
            }).ToListAsync();

        }

        /// <summary>
        /// Sends email to admin for duplicate product sku
        /// </summary>
        /// <param name="languageId"></param>
        /// <param name="vendor"></param>
        /// <param name="body"></param>
        /// <returns></returns>
        public virtual async Task<IList<int>> SendDuplicateProductSkuMessageForVendor(int languageId,
            Vendor vendor, string body)
        {
            var store = await _storeContext.GetCurrentStoreAsync();
            languageId = await EnsureLanguageIsActiveAsync(languageId, store.Id);

            var messageTemplates = await GetActiveMessageTemplatesAsync(MessageTemplateSystemNames.DuplicateProductSkuMessageForVendor, store.Id);
            if (!messageTemplates.Any())
                return new List<int>();

            return await messageTemplates.SelectAwait(async messageTemplate =>
            {
                //email account
                var emailAccount = await GetEmailAccountOfMessageTemplateAsync(messageTemplate, languageId);

                var tokens = new List<Token>();
                await _messageTokenProvider.AddStoreTokensAsync(tokens, store, emailAccount);
                await _messageTokenProvider.AddVendorTokensAsync(tokens, vendor);
                tokens.Add(new Token("Body", body, true));

                return await SendNotificationAsync(messageTemplate, emailAccount, languageId, tokens, vendor.Email, vendor.Name,
                    fromEmail: emailAccount?.Email,
                    fromName: emailAccount?.Username,
                    subject: messageTemplate?.Subject,
                    replyToEmailAddress: emailAccount?.Email,
                    replyToName: emailAccount?.Username);
            }).ToListAsync();
        }

        /// <summary>
        /// Sends email to admin for master products failed to import
        /// </summary>
        /// <param name="languageId"></param>
        /// <param name="vendor"></param>
        /// <param name="body"></param>
        /// <returns></returns>
        public virtual async Task<IList<int>> SendInvalidProductMessage(int languageId, string body)
        {
            var store = await _storeContext.GetCurrentStoreAsync();
            languageId = await EnsureLanguageIsActiveAsync(languageId, store.Id);

            var messageTemplates = await GetActiveMessageTemplatesAsync(MessageTemplateSystemNames.ProductNotImportedMessage, store.Id);
            if (!messageTemplates.Any())
                return new List<int>();

            return await messageTemplates.SelectAwait(async messageTemplate =>
            {
                //email account
                var emailAccount = await GetEmailAccountOfMessageTemplateAsync(messageTemplate, languageId);
                //var superAdminName = await _localizationService.GetResourceAsync("Alchub.SuperAdmin.Name");
                //var superAdminEmailAddress = await _localizationService.GetResourceAsync("Alchub.SuperAdmin.Email");

                var tokens = new List<Token>();
                await _messageTokenProvider.AddStoreTokensAsync(tokens, store, emailAccount);
                tokens.Add(new Token("SuperAdmin.Name", emailAccount.DisplayName, true));
                tokens.Add(new Token("Body", body, true));

                var toEmail = emailAccount.Email;
                var toName = emailAccount.DisplayName;

                //send email to super admin
                return await SendNotificationAsync(messageTemplate, emailAccount, languageId, tokens, toEmail, toName,
                    fromEmail: emailAccount?.Email,
                    fromName: emailAccount?.Username,
                    subject: messageTemplate?.Subject,
                    replyToEmailAddress: emailAccount?.Email,
                    replyToName: emailAccount?.Username);
            }).ToListAsync();

        }

        /// <summary>
        /// Sends email to vendor for unprocessed products
        /// </summary>
        /// <param name="languageId"></param>
        /// <param name="vendor"></param>
        /// <param name="body"></param>
        /// <returns></returns>
        public virtual async Task<IList<int>> SendUnprocessedProductMessageForVendor(int languageId,
            Vendor vendor, string body)
        {
            var store = await _storeContext.GetCurrentStoreAsync();
            languageId = await EnsureLanguageIsActiveAsync(languageId, store.Id);

            var messageTemplates = await GetActiveMessageTemplatesAsync(MessageTemplateSystemNames.UnprocessedProductMessageForVendor, store.Id);
            if (!messageTemplates.Any())
                return new List<int>();

            return await messageTemplates.SelectAwait(async messageTemplate =>
            {
                //email account
                var emailAccount = await GetEmailAccountOfMessageTemplateAsync(messageTemplate, languageId);

                var tokens = new List<Token>();
                await _messageTokenProvider.AddStoreTokensAsync(tokens, store, emailAccount);
                await _messageTokenProvider.AddVendorTokensAsync(tokens, vendor);
                tokens.Add(new Token("Body", body, true));

                return await SendNotificationAsync(messageTemplate, emailAccount, languageId, tokens, vendor.Email, vendor.Name,
                    fromEmail: emailAccount?.Email,
                    fromName: emailAccount?.Username,
                    subject: messageTemplate?.Subject,
                    replyToEmailAddress: emailAccount?.Email,
                    replyToName: emailAccount?.Username);
            }).ToListAsync();
        }

        /// <summary>
        /// Send unprocessed products email to admin. (Sync vendor product schedule task)
        /// </summary>
        /// <param name="languageId"></param>
        /// <param name="vendor"></param>
        /// <param name="body"></param>
        /// <returns></returns>
        public async Task<IList<int>> SendUnprocessedProductMessageToAdmin(int languageId,
            Vendor vendor, string body)
        {
            var store = await _storeContext.GetCurrentStoreAsync();
            languageId = await EnsureLanguageIsActiveAsync(languageId, store.Id);

            var messageTemplates = await GetActiveMessageTemplatesAsync(MessageTemplateSystemNames.UNPROCESSED_PRODUCT_MESSAGE_TO_ADMIN, store.Id);
            if (!messageTemplates.Any())
                return new List<int>();

            return await messageTemplates.SelectAwait(async messageTemplate =>
            {
                //email account
                var emailAccount = await GetEmailAccountOfMessageTemplateAsync(messageTemplate, languageId);

                var tokens = new List<Token>();
                await _messageTokenProvider.AddStoreTokensAsync(tokens, store, emailAccount);
                await _messageTokenProvider.AddVendorTokensAsync(tokens, vendor);
                tokens.Add(new Token("SuperAdmin.Name", emailAccount.DisplayName, true));
                tokens.Add(new Token("Body", body, true));

                //admin email/name
                var toEmail = emailAccount.Email;
                var toName = emailAccount.DisplayName;

                return await SendNotificationAsync(messageTemplate, emailAccount, languageId, tokens, toEmail, toName,
                    fromEmail: emailAccount?.Email,
                    fromName: emailAccount?.Username,
                    subject: messageTemplate?.Subject,
                    replyToEmailAddress: emailAccount?.Email,
                    replyToName: emailAccount?.Username);
            }).ToListAsync();
        }

        #endregion

        #region Order workflow

        /// <summary>
        /// Sends a orderItems delivered notification to a customer
        /// </summary>
        /// <param name="order">order</param>
        /// <param name="orderItems">order items</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the queued email identifier
        /// </returns>
        public async Task<IList<int>> SendOrderItemsDeliveredCustomerNotificationAsync(Order order, IList<OrderItem> orderItems, int languageId)
        {
            if (orderItems == null)
                throw new ArgumentNullException(nameof(orderItems));

            if (order == null)
                throw new ArgumentNullException(nameof(order));

            var store = await _storeService.GetStoreByIdAsync(order.StoreId) ?? await _storeContext.GetCurrentStoreAsync();
            languageId = await EnsureLanguageIsActiveAsync(languageId, store.Id);

            var messageTemplates = await GetActiveMessageTemplatesAsync(MessageTemplateSystemNames.OrderItemsDeliveredCustomerNotification, store.Id);
            if (!messageTemplates.Any())
                return new List<int>();

            //tokens
            var commonTokens = new List<Token>();
            await _messageTokenProvider.AddOrderItemsDeliveredTokensAsync(commonTokens, order, orderItems, languageId);
            await _messageTokenProvider.AddOrderTokensAsync(commonTokens, order, languageId);
            await _messageTokenProvider.AddCustomerTokensAsync(commonTokens, order.CustomerId);

            return await messageTemplates.SelectAwait(async messageTemplate =>
            {
                //email account
                var emailAccount = await GetEmailAccountOfMessageTemplateAsync(messageTemplate, languageId);

                var tokens = new List<Token>(commonTokens);
                await _messageTokenProvider.AddStoreTokensAsync(tokens, store, emailAccount);

                //event notification
                await _eventPublisher.MessageTokensAddedAsync(messageTemplate, tokens);

                var billingAddress = await _addressService.GetAddressByIdAsync(order.BillingAddressId);

                var toEmail = billingAddress.Email;
                var toName = $"{billingAddress.FirstName} {billingAddress.LastName}";

                return await SendNotificationAsync(messageTemplate, emailAccount, languageId, tokens, toEmail, toName);
            }).ToListAsync();
        }

        /// <summary>
        /// Sends a orderItems delivered notification to a customer
        /// </summary>
        /// <param name="order">order</param>
        /// <param name="orderItems">order items</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the queued email identifier
        /// </returns>
        public async Task<IList<int>> SendOrderItemsPickupCompletedCustomerNotificationAsync(Order order, IList<OrderItem> orderItems, int languageId)
        {
            if (orderItems == null)
                throw new ArgumentNullException(nameof(orderItems));

            if (order == null)
                throw new ArgumentNullException(nameof(order));

            var store = await _storeService.GetStoreByIdAsync(order.StoreId) ?? await _storeContext.GetCurrentStoreAsync();
            languageId = await EnsureLanguageIsActiveAsync(languageId, store.Id);

            var messageTemplates = await GetActiveMessageTemplatesAsync(MessageTemplateSystemNames.OrderItemsPickupCompletedCustomerNotification, store.Id);
            if (!messageTemplates.Any())
                return new List<int>();

            //tokens
            var commonTokens = new List<Token>();
            await _messageTokenProvider.AddOrderItemsPickupCompletedTokensAsync(commonTokens, order, orderItems, languageId);
            await _messageTokenProvider.AddOrderTokensAsync(commonTokens, order, languageId);
            await _messageTokenProvider.AddCustomerTokensAsync(commonTokens, order.CustomerId);

            return await messageTemplates.SelectAwait(async messageTemplate =>
            {
                //email account
                var emailAccount = await GetEmailAccountOfMessageTemplateAsync(messageTemplate, languageId);

                var tokens = new List<Token>(commonTokens);
                await _messageTokenProvider.AddStoreTokensAsync(tokens, store, emailAccount);

                //event notification
                await _eventPublisher.MessageTokensAddedAsync(messageTemplate, tokens);

                var billingAddress = await _addressService.GetAddressByIdAsync(order.BillingAddressId);

                var toEmail = billingAddress.Email;
                var toName = $"{billingAddress.FirstName} {billingAddress.LastName}";

                return await SendNotificationAsync(messageTemplate, emailAccount, languageId, tokens, toEmail, toName);
            }).ToListAsync();
        }

        /// <summary>
        /// Sends a orderItems dispatched notification to a customer
        /// </summary>
        /// <param name="order">order</param>
        /// <param name="orderItems">order items</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the queued email identifier
        /// </returns>
        public async Task<IList<int>> SendOrderItemsDispachedCustomerNotificationAsync(Order order, IList<OrderItem> orderItems, int languageId)
        {
            if (orderItems == null)
                throw new ArgumentNullException(nameof(orderItems));

            if (order == null)
                throw new ArgumentNullException(nameof(order));

            var store = await _storeService.GetStoreByIdAsync(order.StoreId) ?? await _storeContext.GetCurrentStoreAsync();
            languageId = await EnsureLanguageIsActiveAsync(languageId, store.Id);

            var messageTemplates = await GetActiveMessageTemplatesAsync(MessageTemplateSystemNames.OrderItemsDispatchedCustomerNotification, store.Id);
            if (!messageTemplates.Any())
                return new List<int>();

            //tokens
            var commonTokens = new List<Token>();
            await _messageTokenProvider.AddOrderItemsDispacthedTokensAsync(commonTokens, order, orderItems, languageId);
            await _messageTokenProvider.AddOrderTokensAsync(commonTokens, order, languageId);
            await _messageTokenProvider.AddCustomerTokensAsync(commonTokens, order.CustomerId);

            return await messageTemplates.SelectAwait(async messageTemplate =>
            {
                //email account
                var emailAccount = await GetEmailAccountOfMessageTemplateAsync(messageTemplate, languageId);

                var tokens = new List<Token>(commonTokens);
                await _messageTokenProvider.AddStoreTokensAsync(tokens, store, emailAccount);

                //event notification
                await _eventPublisher.MessageTokensAddedAsync(messageTemplate, tokens);

                var billingAddress = await _addressService.GetAddressByIdAsync(order.BillingAddressId);

                var toEmail = billingAddress.Email;
                var toName = $"{billingAddress.FirstName} {billingAddress.LastName}";

                return await SendNotificationAsync(messageTemplate, emailAccount, languageId, tokens, toEmail, toName);
            }).ToListAsync();
        }

        #endregion

        #region Order Item Refund

        public async Task<IList<int>> SendOrderItemsCancelCustomerNotificationAsync(Order order, OrderItem orderItems, decimal orderItemRefundAmount, int languageId)
        {
            if (orderItems == null)
                throw new ArgumentNullException(nameof(orderItems));

            if (order == null)
                throw new ArgumentNullException(nameof(order));

            var store = await _storeService.GetStoreByIdAsync(order.StoreId) ?? await _storeContext.GetCurrentStoreAsync();
            languageId = await EnsureLanguageIsActiveAsync(languageId, store.Id);

            var messageTemplates = await GetActiveMessageTemplatesAsync(MessageTemplateSystemNames.OrderItemCancelNotificationsCustomer, store.Id);
            if (!messageTemplates.Any())
                return new List<int>();

            //tokens
            var commonTokens = new List<Token>();
            await _messageTokenProvider.AddOrderItemsCancelCustomerTokensAsync(commonTokens, order, orderItems, orderItemRefundAmount, languageId);
            await _messageTokenProvider.AddOrderTokensAsync(commonTokens, order, languageId);
            await _messageTokenProvider.AddCustomerTokensAsync(commonTokens, order.CustomerId);

            return await messageTemplates.SelectAwait(async messageTemplate =>
            {
                //email account
                var emailAccount = await GetEmailAccountOfMessageTemplateAsync(messageTemplate, languageId);

                var tokens = new List<Token>(commonTokens);
                await _messageTokenProvider.AddStoreTokensAsync(tokens, store, emailAccount);

                //event notification
                await _eventPublisher.MessageTokensAddedAsync(messageTemplate, tokens);

                var billingAddress = await _addressService.GetAddressByIdAsync(order.BillingAddressId);

                var toEmail = billingAddress.Email;
                var toName = $"{billingAddress.FirstName} {billingAddress.LastName}";

                return await SendNotificationAsync(messageTemplate, emailAccount, languageId, tokens, toEmail, toName);
            }).ToListAsync();
        }

        public async Task<IList<int>> SendOrderItemsCancelVendorNotificationAsync(Order order, OrderItem orderItems, Vendor vendor, int languageId)
        {
            if (orderItems == null)
                throw new ArgumentNullException(nameof(orderItems));

            if (order == null)
                throw new ArgumentNullException(nameof(order));

            var store = await _storeService.GetStoreByIdAsync(order.StoreId) ?? await _storeContext.GetCurrentStoreAsync();
            languageId = await EnsureLanguageIsActiveAsync(languageId, store.Id);

            var messageTemplates = await GetActiveMessageTemplatesAsync(MessageTemplateSystemNames.OrderItemCancelNotificationsVendor, store.Id);
            if (!messageTemplates.Any())
                return new List<int>();

            //tokens
            var commonTokens = new List<Token>();
            await _messageTokenProvider.AddOrderItemsCancelVendorTokensAsync(commonTokens, order, orderItems, languageId);
            await _messageTokenProvider.AddCustomerTokensAsync(commonTokens, order.CustomerId);
            await _messageTokenProvider.AddVendorTokensAsync(commonTokens, vendor);

            return await messageTemplates.SelectAwait(async messageTemplate =>
            {
                //email account
                var emailAccount = await GetEmailAccountOfMessageTemplateAsync(messageTemplate, languageId);

                var tokens = new List<Token>(commonTokens);
                await _messageTokenProvider.AddStoreTokensAsync(tokens, store, emailAccount);

                //event notification
                await _eventPublisher.MessageTokensAddedAsync(messageTemplate, tokens);

                //var billingAddress = await _addressService.GetAddressByIdAsync(order.BillingAddressId);

                var toEmail = vendor.Email;
                var toName = $"{vendor.Name}";

                return await SendNotificationAsync(messageTemplate, emailAccount, languageId, tokens, toEmail, toName);
            }).ToListAsync();
        }



        public async Task<IList<int>> SendOrderItemsDeliveryDeniedCustomerNotificationAsync(Order order, IList<OrderItem> orderItems, int languageId)
        {
            if (orderItems == null)
                throw new ArgumentNullException(nameof(orderItems));

            if (order == null)
                throw new ArgumentNullException(nameof(order));

            var store = await _storeService.GetStoreByIdAsync(order.StoreId) ?? await _storeContext.GetCurrentStoreAsync();
            languageId = await EnsureLanguageIsActiveAsync(languageId, store.Id);

            var messageTemplates = await GetActiveMessageTemplatesAsync(MessageTemplateSystemNames.OrderItemDeliveryDeniedNotificationsCustomer, store.Id);
            if (!messageTemplates.Any())
                return new List<int>();

            //tokens
            var commonTokens = new List<Token>();
            await _messageTokenProvider.AddOrderItemsDeliveryDeniedCustomerTokensAsync(commonTokens, order, orderItems, languageId);
            await _messageTokenProvider.AddOrderTokensAsync(commonTokens, order, languageId);
            await _messageTokenProvider.AddCustomerTokensAsync(commonTokens, order.CustomerId);

            return await messageTemplates.SelectAwait(async messageTemplate =>
            {
                //email account
                var emailAccount = await GetEmailAccountOfMessageTemplateAsync(messageTemplate, languageId);

                var tokens = new List<Token>(commonTokens);
                await _messageTokenProvider.AddStoreTokensAsync(tokens, store, emailAccount);

                //event notification
                await _eventPublisher.MessageTokensAddedAsync(messageTemplate, tokens);

                var billingAddress = await _addressService.GetAddressByIdAsync(order.BillingAddressId);

                var toEmail = billingAddress.Email;
                var toName = $"{billingAddress.FirstName} {billingAddress.LastName}";

                return await SendNotificationAsync(messageTemplate, emailAccount, languageId, tokens, toEmail, toName);
            }).ToListAsync();
        }

        public async Task<IList<int>> SendOrderItemsDeliveryDeniedVendorNotificationAsync(Order order, IList<OrderItem> orderItems, Vendor vendor, int languageId)
        {
            if (orderItems == null)
                throw new ArgumentNullException(nameof(orderItems));

            if (order == null)
                throw new ArgumentNullException(nameof(order));

            var store = await _storeService.GetStoreByIdAsync(order.StoreId) ?? await _storeContext.GetCurrentStoreAsync();
            languageId = await EnsureLanguageIsActiveAsync(languageId, store.Id);

            var messageTemplates = await GetActiveMessageTemplatesAsync(MessageTemplateSystemNames.OrderItemDeliveryDeniedNotificationsVendor, store.Id);
            if (!messageTemplates.Any())
                return new List<int>();

            //tokens
            var commonTokens = new List<Token>();
            await _messageTokenProvider.AddOrderItemsDeliveryDeniedVendorTokensAsync(commonTokens, order, orderItems, languageId);
            await _messageTokenProvider.AddOrderTokensAsync(commonTokens, order, languageId);
            await _messageTokenProvider.AddCustomerTokensAsync(commonTokens, order.CustomerId);
            await _messageTokenProvider.AddVendorTokensAsync(commonTokens, vendor);

            return await messageTemplates.SelectAwait(async messageTemplate =>
            {
                //email account
                var emailAccount = await GetEmailAccountOfMessageTemplateAsync(messageTemplate, languageId);

                var tokens = new List<Token>(commonTokens);
                await _messageTokenProvider.AddStoreTokensAsync(tokens, store, emailAccount);

                //event notification
                await _eventPublisher.MessageTokensAddedAsync(messageTemplate, tokens);

                //var billingAddress = await _addressService.GetAddressByIdAsync(order.BillingAddressId);

                var toEmail = vendor.Email;
                var toName = $"{vendor.Name}";

                return await SendNotificationAsync(messageTemplate, emailAccount, languageId, tokens, toEmail, toName);
            }).ToListAsync();
        }


        public async Task<IList<int>> SendPickupOrderItemCustomerNotificationAsync(Order order, OrderItem orderItems, int languageId)
        {
            if (orderItems == null)
                throw new ArgumentNullException(nameof(orderItems));

            if (order == null)
                throw new ArgumentNullException(nameof(order));

            var store = await _storeService.GetStoreByIdAsync(order.StoreId) ?? await _storeContext.GetCurrentStoreAsync();
            languageId = await EnsureLanguageIsActiveAsync(languageId, store.Id);

            var messageTemplates = await GetActiveMessageTemplatesAsync(MessageTemplateSystemNames.OrderItemPickupNotificationsCustomer, store.Id);
            if (!messageTemplates.Any())
                return new List<int>();

            //tokens
            var commonTokens = new List<Token>();
            await _messageTokenProvider.AddOrderTokensAsync(commonTokens, order, languageId);
            await _messageTokenProvider.AddCustomerTokensAsync(commonTokens, order.CustomerId);

            return await messageTemplates.SelectAwait(async messageTemplate =>
            {
                //email account
                var emailAccount = await GetEmailAccountOfMessageTemplateAsync(messageTemplate, languageId);

                var tokens = new List<Token>(commonTokens);
                await _messageTokenProvider.AddStoreTokensAsync(tokens, store, emailAccount);

                //event notification
                await _eventPublisher.MessageTokensAddedAsync(messageTemplate, tokens);

                var billingAddress = await _addressService.GetAddressByIdAsync(order.BillingAddressId);

                var toEmail = billingAddress.Email;
                var toName = $"{billingAddress.FirstName} {billingAddress.LastName}";

                return await SendNotificationAsync(messageTemplate, emailAccount, languageId, tokens, toEmail, toName);
            }).ToListAsync();
        }
        #endregion
    }
}
