using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Messages;
using Nop.Core.Domain.Orders;
using Nop.Core.Events;
using Nop.Plugin.Misc.ScheduledOrderCancel.Domain;
using Nop.Services.Affiliates;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Stores;

namespace Nop.Plugin.Misc.ScheduledOrderCancel.Services
{
    public class CustomWorkflowMessageService : WorkflowMessageService
    {
        #region Fields

        private readonly IAddressService _addressService;
        private readonly IEventPublisher _eventPublisher;
        private readonly IMessageTokenProvider _messageTokenProvider;
        private readonly IStoreContext _storeContext;
        private readonly IStoreService _storeService;

        #endregion

        public CustomWorkflowMessageService(
            CommonSettings commonSettings,
            EmailAccountSettings emailAccountSettings,
            IAddressService addressService,
            IAffiliateService affiliateService,
            ICustomerService customerService,
            IEmailAccountService emailAccountService,
            IEventPublisher eventPublisher,
            ILanguageService languageService,
            ILocalizationService localizationService,
            IMessageTemplateService messageTemplateService,
            IMessageTokenProvider messageTokenProvider,
            IOrderService orderService,
            IProductService productService,
            IQueuedEmailService queuedEmailService,
            IStoreContext storeContext,
            IStoreService storeService,
            ITokenizer tokenizer) : base(commonSettings,
                                         emailAccountSettings,
                                         addressService,
                                         affiliateService,
                                         customerService,
                                         emailAccountService,
                                         eventPublisher,
                                         languageService,
                                         localizationService,
                                         messageTemplateService,
                                         messageTokenProvider,
                                         orderService,
                                         productService,
                                         queuedEmailService,
                                         storeContext,
                                         storeService,
                                         tokenizer)
        {
            _addressService = addressService;
            _eventPublisher = eventPublisher;
            _messageTokenProvider = messageTokenProvider;
            _storeContext = storeContext;
            _storeService = storeService;
        }

        #region Methods

        /// <summary>
        /// Sends an order cancelled notification to a customer
        /// </summary>
        /// <param name="order">Order instance</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the queued email identifier
        /// </returns>
        public virtual async Task<IList<int>> SendOrderCancelledCustomerCustomNotificationAsync(Order order, int languageId)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            var store = await _storeService.GetStoreByIdAsync(order.StoreId) ?? await _storeContext.GetCurrentStoreAsync();
            languageId = await EnsureLanguageIsActiveAsync(languageId, store.Id);

            var messageTemplates = await GetActiveMessageTemplatesAsync(MessageTemplateSystemName.OrderCancelledCustomerCustomNotification, store.Id);
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
