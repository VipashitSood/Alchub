using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Events;
using Nop.Services.Affiliates;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Plugin.Api.Domain.Messages;
using Nop.Core.Domain.Messages;
using Nop.Core.Domain.Orders;
using Nop.Services.Stores;

namespace Nop.Plugin.Api.Services
{
    public class WorkflowMessageService : IWorkflowMessageService
    {
        #region Fields

        private readonly CommonSettings _commonSettings;
        private readonly EmailAccountSettings _emailAccountSettings;
        private readonly IAddressService _addressService;
        private readonly IAffiliateService _affiliateService;
        private readonly ICustomerService _customerService;
        private readonly IEmailAccountService _emailAccountService;
        private readonly IEventPublisher _eventPublisher;
        private readonly ILanguageService _languageService;
        private readonly ILocalizationService _localizationService;
        private readonly IMessageTemplateService _messageTemplateService;
        private readonly IMessageTokenProvider _messageTokenProvider;
        private readonly IOrderService _orderService;
        private readonly IQueuedEmailService _queuedEmailService;
        private readonly IStoreContext _storeContext;
        private readonly ITokenizer _tokenizer;
        private readonly IStoreService _storeService;

        #endregion

        #region Ctor

        public WorkflowMessageService(CommonSettings commonSettings,
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
            IQueuedEmailService queuedEmailService,
            IStoreContext storeContext,
            IStoreService storeService,
            ITokenizer tokenizer)
        {
            _commonSettings = commonSettings;
            _emailAccountSettings = emailAccountSettings;
            _addressService = addressService;
            _affiliateService = affiliateService;
            _customerService = customerService;
            _emailAccountService = emailAccountService;
            _eventPublisher = eventPublisher;
            _languageService = languageService;
            _localizationService = localizationService;
            _messageTemplateService = messageTemplateService;
            _messageTokenProvider = messageTokenProvider;
            _orderService = orderService;
            _queuedEmailService = queuedEmailService;
            _storeContext = storeContext;
            _tokenizer = tokenizer;
            _storeService = storeService;
        }

        #endregion
        public virtual async Task<IList<int>> SendCustomerPasswordRecoveryMessageAsync(Customer customer,string otp, int languageId)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            var store = await _storeContext.GetCurrentStoreAsync();
            languageId = await EnsureLanguageIsActiveAsync(languageId, store.Id);

            var messageTemplates = await GetActiveMessageTemplatesAsync(MessageApiTemplateSystemNames.CustomerPasswordRecoveryOTPMessage, store.Id);
            if (!messageTemplates.Any())
                return new List<int>();

            //tokens
            var commonTokens = new List<Token>();
            await _messageTokenProvider.AddCustomerTokensAsync(commonTokens, customer);

            //add otp
           commonTokens.Add(new Token("Customer.PasswordRecoveryOTP", otp, true));

            return await messageTemplates.SelectAwait(async messageTemplate =>
            {
                //email account
                var emailAccount = await GetEmailAccountOfMessageTemplateAsync(messageTemplate, languageId);

                var tokens = new List<Token>(commonTokens);
                await _messageTokenProvider.AddStoreTokensAsync(tokens, store, emailAccount);


                //event notification
                await _eventPublisher.MessageTokensAddedAsync(messageTemplate, tokens);

                var toEmail = customer.Email;
                var toName = await _customerService.GetCustomerFullNameAsync(customer);

                return await SendNotificationAsync(messageTemplate, emailAccount, languageId, tokens, toEmail, toName);
            }).ToListAsync();
        }
        protected virtual async Task<int> EnsureLanguageIsActiveAsync(int languageId, int storeId)
        {
            //load language by specified ID
            var language = await _languageService.GetLanguageByIdAsync(languageId);

            if (language == null || !language.Published)
            {
                //load any language from the specified store
                language = (await _languageService.GetAllLanguagesAsync(storeId: storeId)).FirstOrDefault();
            }

            if (language == null || !language.Published)
            {
                //load any language
                language = (await _languageService.GetAllLanguagesAsync()).FirstOrDefault();
            }

            if (language == null)
                throw new Exception("No active language could be loaded");

            return language.Id;
        }
        protected virtual async Task<IList<MessageTemplate>> GetActiveMessageTemplatesAsync(string messageTemplateName, int storeId)
        {
            //get message templates by the name
            var messageTemplates = await _messageTemplateService.GetMessageTemplatesByNameAsync(messageTemplateName, storeId);

            //no template found
            if (!messageTemplates?.Any() ?? true)
                return new List<MessageTemplate>();

            //filter active templates
            messageTemplates = messageTemplates.Where(messageTemplate => messageTemplate.IsActive).ToList();

            return messageTemplates;
        }
        protected virtual async Task<EmailAccount> GetEmailAccountOfMessageTemplateAsync(MessageTemplate messageTemplate, int languageId)
        {
            var emailAccountId = await _localizationService.GetLocalizedAsync(messageTemplate, mt => mt.EmailAccountId, languageId);
            //some 0 validation (for localizable "Email account" dropdownlist which saves 0 if "Standard" value is chosen)
            if (emailAccountId == 0)
                emailAccountId = messageTemplate.EmailAccountId;

            var emailAccount = (await _emailAccountService.GetEmailAccountByIdAsync(emailAccountId) ?? await _emailAccountService.GetEmailAccountByIdAsync(_emailAccountSettings.DefaultEmailAccountId)) ??
                               (await _emailAccountService.GetAllEmailAccountsAsync()).FirstOrDefault();
            return emailAccount;
        }

        public virtual async Task<int> SendNotificationAsync(MessageTemplate messageTemplate,
        EmailAccount emailAccount, int languageId, IList<Token> tokens,
        string toEmailAddress, string toName,
        string attachmentFilePath = null, string attachmentFileName = null,
        string replyToEmailAddress = null, string replyToName = null,
        string fromEmail = null, string fromName = null, string subject = null)
        {
            if (messageTemplate == null)
                throw new ArgumentNullException(nameof(messageTemplate));

            if (emailAccount == null)
                throw new ArgumentNullException(nameof(emailAccount));

            //retrieve localized message template data
            var bcc = await _localizationService.GetLocalizedAsync(messageTemplate, mt => mt.BccEmailAddresses, languageId);
            if (string.IsNullOrEmpty(subject))
                subject = await _localizationService.GetLocalizedAsync(messageTemplate, mt => mt.Subject, languageId);
            var body = await _localizationService.GetLocalizedAsync(messageTemplate, mt => mt.Body, languageId);

            //Replace subject and body tokens 
            var subjectReplaced = _tokenizer.Replace(subject, tokens, false);
            var bodyReplaced = _tokenizer.Replace(body, tokens, true);

            //limit name length
            toName = CommonHelper.EnsureMaximumLength(toName, 300);

            var email = new QueuedEmail
            {
                Priority = QueuedEmailPriority.High,
                From = !string.IsNullOrEmpty(fromEmail) ? fromEmail : emailAccount.Email,
                FromName = !string.IsNullOrEmpty(fromName) ? fromName : emailAccount.DisplayName,
                To = toEmailAddress,
                ToName = toName,
                ReplyTo = replyToEmailAddress,
                ReplyToName = replyToName,
                CC = string.Empty,
                Bcc = bcc,
                Subject = subjectReplaced,
                Body = bodyReplaced,
                AttachmentFilePath = attachmentFilePath,
                AttachmentFileName = attachmentFileName,
                AttachedDownloadId = messageTemplate.AttachedDownloadId,
                CreatedOnUtc = DateTime.UtcNow,
                EmailAccountId = emailAccount.Id,
                DontSendBeforeDateUtc = !messageTemplate.DelayBeforeSend.HasValue ? null
                    : (DateTime?)(DateTime.UtcNow + TimeSpan.FromHours(messageTemplate.DelayPeriod.ToHours(messageTemplate.DelayBeforeSend.Value)))
            };

            await _queuedEmailService.InsertQueuedEmailAsync(email);
            return email.Id;
        }

        #region Return requests

        /// <summary>
        /// Sends 'New Return Request' message to a store owner
        /// </summary>
        /// <param name="returnRequest">Return request</param>
        /// <param name="orderItem">Order item</param>
        /// <param name="order">Order</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the queued email identifier
        /// </returns>
        public virtual async Task<IList<int>> SendNewReturnRequestStoreOwnerNotificationAsync(ReturnRequest returnRequest, OrderItem orderItem, Order order, int languageId)
        {
            if (returnRequest == null)
                throw new ArgumentNullException(nameof(returnRequest));

            if (orderItem == null)
                throw new ArgumentNullException(nameof(orderItem));

            if (order == null)
                throw new ArgumentNullException(nameof(order));

            var store = await _storeService.GetStoreByIdAsync(order.StoreId) ?? await _storeContext.GetCurrentStoreAsync();
            languageId = await EnsureLanguageIsActiveAsync(languageId, store.Id);

            var messageTemplates = await GetActiveMessageTemplatesAsync(MessageTemplateSystemNames.NewReturnRequestStoreOwnerNotification, store.Id);
            if (!messageTemplates.Any())
                return new List<int>();

            //tokens
            var commonTokens = new List<Token>();
            await _messageTokenProvider.AddOrderTokensAsync(commonTokens, order, languageId);
            await _messageTokenProvider.AddCustomerTokensAsync(commonTokens, returnRequest.CustomerId);
            await _messageTokenProvider.AddReturnRequestTokensAsync(commonTokens, returnRequest, orderItem, languageId);

            return await messageTemplates.SelectAwait(async messageTemplate =>
            {
                //email account
                var emailAccount = await GetEmailAccountOfMessageTemplateAsync(messageTemplate, languageId);

                var tokens = new List<Token>(commonTokens);
                await _messageTokenProvider.AddStoreTokensAsync(tokens, store, emailAccount);

                //event notification
                await _eventPublisher.MessageTokensAddedAsync(messageTemplate, tokens);

                var toEmail = emailAccount.Email;
                var toName = emailAccount.DisplayName;

                return await SendNotificationAsync(messageTemplate, emailAccount, languageId, tokens, toEmail, toName);
            }).ToListAsync();
        }

        /// <summary>
        /// Sends 'New Return Request' message to a customer
        /// </summary>
        /// <param name="returnRequest">Return request</param>
        /// <param name="orderItem">Order item</param>
        /// <param name="order">Order</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the queued email identifier
        /// </returns>
        public virtual async Task<IList<int>> SendNewReturnRequestCustomerNotificationAsync(ReturnRequest returnRequest, OrderItem orderItem, Order order)
        {
            if (returnRequest == null)
                throw new ArgumentNullException(nameof(returnRequest));

            if (orderItem == null)
                throw new ArgumentNullException(nameof(orderItem));

            if (order == null)
                throw new ArgumentNullException(nameof(order));

            var store = await _storeService.GetStoreByIdAsync(order.StoreId) ?? await _storeContext.GetCurrentStoreAsync();
            var languageId = await EnsureLanguageIsActiveAsync(order.CustomerLanguageId, store.Id);

            var messageTemplates = await GetActiveMessageTemplatesAsync(MessageTemplateSystemNames.NewReturnRequestCustomerNotification, store.Id);
            if (!messageTemplates.Any())
                return new List<int>();

            var customer = await _customerService.GetCustomerByIdAsync(returnRequest.CustomerId);

            //tokens
            var commonTokens = new List<Token>();
            await _messageTokenProvider.AddOrderTokensAsync(commonTokens, order, languageId);
            await _messageTokenProvider.AddCustomerTokensAsync(commonTokens, customer);
            await _messageTokenProvider.AddReturnRequestTokensAsync(commonTokens, returnRequest, orderItem, languageId);

            return await messageTemplates.SelectAwait(async messageTemplate =>
            {
                //email account
                var emailAccount = await GetEmailAccountOfMessageTemplateAsync(messageTemplate, languageId);

                var tokens = new List<Token>(commonTokens);
                await _messageTokenProvider.AddStoreTokensAsync(tokens, store, emailAccount);

                //event notification
                await _eventPublisher.MessageTokensAddedAsync(messageTemplate, tokens);

                var billingAddress = await _addressService.GetAddressByIdAsync(order.BillingAddressId);

                var toEmail = (await _customerService.IsGuestAsync(customer))
                    ? billingAddress.Email
                    : customer.Email;
                var toName = (await _customerService.IsGuestAsync(customer))
                    ? billingAddress.FirstName
                    : await _customerService.GetCustomerFullNameAsync(customer);

                return await SendNotificationAsync(messageTemplate, emailAccount, languageId, tokens, toEmail, toName);
            }).ToListAsync();
        }

        /// <summary>
        /// Sends 'Return Request status changed' message to a customer
        /// </summary>
        /// <param name="returnRequest">Return request</param>
        /// <param name="orderItem">Order item</param>
        /// <param name="order">Order</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the queued email identifier
        /// </returns>
        public virtual async Task<IList<int>> SendReturnRequestStatusChangedCustomerNotificationAsync(ReturnRequest returnRequest, OrderItem orderItem, Order order)
        {
            if (returnRequest == null)
                throw new ArgumentNullException(nameof(returnRequest));

            if (orderItem == null)
                throw new ArgumentNullException(nameof(orderItem));

            if (order == null)
                throw new ArgumentNullException(nameof(order));

            var store = await _storeService.GetStoreByIdAsync(order.StoreId) ?? await _storeContext.GetCurrentStoreAsync();
            var languageId = await EnsureLanguageIsActiveAsync(order.CustomerLanguageId, store.Id);

            var messageTemplates = await GetActiveMessageTemplatesAsync(MessageTemplateSystemNames.ReturnRequestStatusChangedCustomerNotification, store.Id);
            if (!messageTemplates.Any())
                return new List<int>();

            var customer = await _customerService.GetCustomerByIdAsync(returnRequest.CustomerId);

            //tokens
            var commonTokens = new List<Token>();
            await _messageTokenProvider.AddOrderTokensAsync(commonTokens, order, languageId);
            await _messageTokenProvider.AddCustomerTokensAsync(commonTokens, customer);
            await _messageTokenProvider.AddReturnRequestTokensAsync(commonTokens, returnRequest, orderItem, languageId);

            return await messageTemplates.SelectAwait(async messageTemplate =>
            {
                //email account
                var emailAccount = await GetEmailAccountOfMessageTemplateAsync(messageTemplate, languageId);

                var tokens = new List<Token>(commonTokens);
                await _messageTokenProvider.AddStoreTokensAsync(tokens, store, emailAccount);

                //event notification
                await _eventPublisher.MessageTokensAddedAsync(messageTemplate, tokens);

                var billingAddress = await _addressService.GetAddressByIdAsync(order.BillingAddressId);

                var toEmail = (await _customerService.IsGuestAsync(customer))
                    ? billingAddress.Email
                    : customer.Email;
                var toName = (await _customerService.IsGuestAsync(customer))
                    ? billingAddress.FirstName
                    : await _customerService.GetCustomerFullNameAsync(customer);

                return await SendNotificationAsync(messageTemplate, emailAccount, languageId, tokens, toEmail, toName);
            }).ToListAsync();
        }

        #endregion

    }
}
