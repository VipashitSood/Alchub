using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Messages;
using Nop.Core.Domain.Orders;
using Nop.Core.Events;
using Nop.Services.Affiliates;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Stores;

namespace Nop.Plugin.Payments.StripeConnectRedirect.Services
{
    public partial class StripeWorkflowMessageService : WorkflowMessageService, IStripeWorkflowMessageService
    {
        #region Fields

        private readonly IAddressService _addressService;
        private readonly IEventPublisher _eventPublisher;
        private readonly IMessageTokenProvider _messageTokenProvider;
        private readonly IStoreContext _storeContext;
        private readonly IStoreService _storeService;

        #endregion Fields

        #region Ctor

        public StripeWorkflowMessageService(
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

        #endregion Ctor

        #region Capture/Release

        /// <summary>
        /// Sends a Order payment captured notification to a customer
        /// </summary>
        /// <param name="order"></param>
        /// <param name="amountCaptured"></param>
        /// <param name="amountReleased"></param>
        /// <param name="languageId"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public virtual async Task<IList<int>> SendCapturePaymentCustomerNotificationAsync(Order order, string amountCaptured, string amountReleased, int languageId)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            var store = await _storeService.GetStoreByIdAsync(order.StoreId) ?? await _storeContext.GetCurrentStoreAsync();
            languageId = await EnsureLanguageIsActiveAsync(languageId, store.Id);

            var messageTemplates = await GetActiveMessageTemplatesAsync(MessageTemplateSystemNames.CustomerPaymentCaptureNotification, store.Id);
            if (!messageTemplates.Any())
                return new List<int>();

            //tokens
            var commonTokens = new List<Token>();
            await _messageTokenProvider.AddOrderTokensAsync(commonTokens, order, languageId);
            await _messageTokenProvider.AddCustomerTokensAsync(commonTokens, order.CustomerId);

            //Stripe tokens
            commonTokens.Add(new Token("Stripe.Order.Amount.Captured", amountCaptured));
            commonTokens.Add(new Token("Stripe.Order.Amount.Released", amountReleased));

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
        /// Sends a Order payment released notification to a customer
        /// </summary>
        /// <param name="order"></param>
        /// <param name="amountCaptured"></param>
        /// <param name="amountReleased"></param>
        /// <param name="languageId"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public virtual async Task<IList<int>> SendReleasePaymentCustomerNotificationAsync(Order order, string amountCaptured, string amountReleased, int languageId)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            var store = await _storeService.GetStoreByIdAsync(order.StoreId) ?? await _storeContext.GetCurrentStoreAsync();
            languageId = await EnsureLanguageIsActiveAsync(languageId, store.Id);

            var messageTemplates = await GetActiveMessageTemplatesAsync(MessageTemplateSystemNames.CustomerPaymentReleaseNotification, store.Id);
            if (!messageTemplates.Any())
                return new List<int>();

            //tokens
            var commonTokens = new List<Token>();
            await _messageTokenProvider.AddOrderTokensAsync(commonTokens, order, languageId);
            await _messageTokenProvider.AddCustomerTokensAsync(commonTokens, order.CustomerId);

            //Stripe tokens
            commonTokens.Add(new Token("Stripe.Order.Amount.Captured", amountCaptured));
            commonTokens.Add(new Token("Stripe.Order.Amount.Released", amountReleased));

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

        #endregion Capture/Release

    }
}