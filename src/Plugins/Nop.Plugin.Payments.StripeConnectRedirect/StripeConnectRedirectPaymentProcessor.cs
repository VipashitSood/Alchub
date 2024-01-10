using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Nop.Core;
using Nop.Core.Domain.Cms;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.ScheduleTasks;
using Nop.Plugin.Payments.StripeConnectRedirect.Domain;
using Nop.Plugin.Payments.StripeConnectRedirect.Services;
using Nop.Services.Cms;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Plugins;
using Nop.Services.ScheduleTasks;
using Nop.Web.Framework.Infrastructure;
using Stripe;
using Stripe.Checkout;

namespace Nop.Plugin.Payments.StripeConnectRedirect
{
    /// <summary>
    /// Stripe Connect Redirect payment processor
    /// </summary>
    public class StripeConnectRedirectPaymentProcessor : BasePlugin, IPaymentMethod, IWidgetPlugin
    {
        #region Fields

        private readonly IAddressService _addressService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILocalizationService _localizationService;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly IScheduleTaskService _scheduleTaskService;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;
        private readonly IStripeConnectRedirectService _stripeConnectRedirectService;
        private readonly IWebHelper _webHelper;
        private readonly StripeConnectRedirectPaymentSettings _stripeConnectRedirectPaymentSettings;
        private readonly WidgetSettings _widgetSettings;

        #endregion

        #region Ctor

        public StripeConnectRedirectPaymentProcessor(
            IAddressService addressService,
            IHttpContextAccessor httpContextAccessor,
            ILocalizationService localizationService,
            IOrderTotalCalculationService orderTotalCalculationService,
            IScheduleTaskService scheduleTaskService,
            ISettingService settingService,
            IStripeConnectRedirectService stripeConnectRedirectService,
            IStoreContext storeContext,
            IWebHelper webHelper,
            StripeConnectRedirectPaymentSettings stripeConnectRedirectPaymentSettings,
            WidgetSettings widgetSettings)
        {
            _addressService = addressService;
            _httpContextAccessor = httpContextAccessor;
            _localizationService = localizationService;
            _orderTotalCalculationService = orderTotalCalculationService;
            _scheduleTaskService = scheduleTaskService;
            _settingService = settingService;
            _stripeConnectRedirectService = stripeConnectRedirectService;
            _storeContext = storeContext;
            _webHelper = webHelper;
            _stripeConnectRedirectPaymentSettings = stripeConnectRedirectPaymentSettings;
            _widgetSettings = widgetSettings;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets a configuration page URL
        /// </summary>
        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/StripeConnectRedirect/Configure";
        }

        /// <summary>
        /// Gets a name of a view component for displaying plugin in public store ("payment info" checkout step)
        /// </summary>
        /// <returns>View component name</returns>
        public string GetPublicViewComponentName()
        {
            return StripeConnectRedirectPaymentDefaults.PaymentInfoViewComponentName;
        }

        /// <summary>
        /// Gets widget zones where this widget should be rendered
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the widget zones
        /// </returns>
        public Task<IList<string>> GetWidgetZonesAsync()
        {
            return Task.FromResult<IList<string>>(new List<string> { AdminWidgetZones.VendorDetailsBlock });
        }

        /// <summary>
        /// Gets a name of a view component for displaying widget
        /// </summary>
        /// <param name="widgetZone">Name of the widget zone</param>
        /// <returns>View component name</returns>
        public string GetWidgetViewComponentName(string widgetZone)
        {
            return StripeConnectRedirectPaymentDefaults.StripeVendorConnectViewComponentName;
        }

        /// <summary>
        /// Process a payment
        /// </summary>
        /// <param name="processPaymentRequest">Payment info required for an order processing</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the process payment result
        /// </returns>
        public Task<ProcessPaymentResult> ProcessPaymentAsync(ProcessPaymentRequest processPaymentRequest)
        {
            return Task.FromResult(new ProcessPaymentResult());
        }

        /// <summary>
        /// Post process payment (used by payment gateways that require redirecting to a third-party URL)
        /// </summary>
        /// <param name="postProcessPaymentRequest">Payment info required for an order processing</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task PostProcessPaymentAsync(PostProcessPaymentRequest postProcessPaymentRequest)
        {
            //post process payment & get redirect url.
            var url = await _stripeConnectRedirectService.PostProcessPaymentAndGetRedirectUrl(postProcessPaymentRequest.Order);

            _httpContextAccessor.HttpContext.Response.Redirect(url);
        }

        /// <summary>
        /// Returns a value indicating whether payment method should be hidden during checkout
        /// </summary>
        /// <param name="cart">Shopping cart</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the rue - hide; false - display.
        /// </returns>
        public async Task<bool> HidePaymentMethodAsync(IList<ShoppingCartItem> cart)
        {
            if (_stripeConnectRedirectService.IsConfigured(_stripeConnectRedirectPaymentSettings))
                return false;

            return true;
        }

        /// <summary>
        /// Gets additional handling fee
        /// </summary>
        /// <param name="cart">Shopping cart</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the additional handling fee
        /// </returns>
        public async Task<decimal> GetAdditionalHandlingFeeAsync(IList<ShoppingCartItem> cart)
        {
            return await _orderTotalCalculationService.CalculatePaymentAdditionalFeeAsync(cart,
                _stripeConnectRedirectPaymentSettings.AdditionalFee, _stripeConnectRedirectPaymentSettings.AdditionalFeePercentage);
        }

        /// <summary>
        /// Captures payment
        /// </summary>
        /// <param name="capturePaymentRequest">Capture payment request</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the capture payment result
        /// </returns>
        public Task<CapturePaymentResult> CaptureAsync(CapturePaymentRequest capturePaymentRequest)
        {
            return Task.FromResult(new CapturePaymentResult { Errors = new[] { "Capture method not supported" } });
        }

        /// <summary>
        /// Refunds a payment
        /// </summary>
        /// <param name="refundPaymentRequest">Request</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the result
        /// </returns>
        public Task<RefundPaymentResult> RefundAsync(RefundPaymentRequest refundPaymentRequest)
        {
            return Task.FromResult(new RefundPaymentResult { Errors = new[] { "Refund method not supported" } });
        }

        /// <summary>
        /// Voids a payment
        /// </summary>
        /// <param name="voidPaymentRequest">Request</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the result
        /// </returns>
        public Task<VoidPaymentResult> VoidAsync(VoidPaymentRequest voidPaymentRequest)
        {
            return Task.FromResult(new VoidPaymentResult { Errors = new[] { "Void method not supported" } });
        }

        /// <summary>
        /// Process recurring payment
        /// </summary>
        /// <param name="processPaymentRequest">Payment info required for an order processing</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the process payment result
        /// </returns>
        public Task<ProcessPaymentResult> ProcessRecurringPaymentAsync(ProcessPaymentRequest processPaymentRequest)
        {
            return Task.FromResult(new ProcessPaymentResult { Errors = new[] { "Recurring payment not supported" } });
        }

        /// <summary>
        /// Cancels a recurring payment
        /// </summary>
        /// <param name="cancelPaymentRequest">Request</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the result
        /// </returns>
        public Task<CancelRecurringPaymentResult> CancelRecurringPaymentAsync(CancelRecurringPaymentRequest cancelPaymentRequest)
        {
            return Task.FromResult(new CancelRecurringPaymentResult { Errors = new[] { "Recurring payment not supported" } });
        }

        /// <summary>
        /// Gets a value indicating whether customers can complete a payment after order is placed but not completed (for redirection payment methods)
        /// </summary>
        /// <param name="order">Order</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the result
        /// </returns>
        public Task<bool> CanRePostProcessPaymentAsync(Order order)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            return Task.FromResult(false);
        }

        /// <summary>
        /// Validate payment form
        /// </summary>
        /// <param name="form">The parsed form values</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the list of validating errors
        /// </returns>
        public Task<IList<string>> ValidatePaymentFormAsync(IFormCollection form)
        {
            return Task.FromResult<IList<string>>(new List<string>());
        }

        /// <summary>
        /// Get payment information
        /// </summary>
        /// <param name="form">The parsed form values</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the payment info holder
        /// </returns>
        public Task<ProcessPaymentRequest> GetPaymentInfoAsync(IFormCollection form)
        {
            return Task.FromResult(new ProcessPaymentRequest());
        }

        /// <summary>
        /// Install the plugin
        /// </summary>
        /// <returns>A task that represents the asynchronous operation</returns>
        public override async Task InstallAsync()
        {
            //settings
            await _settingService.SaveSettingAsync(new StripeConnectRedirectPaymentSettings()
            {
                PaymentDescription = _storeContext.GetCurrentStore().Name
            });

            //Install StripeConnectSync schedule task
            if (await _scheduleTaskService.GetTaskByTypeAsync(StripeConnectRedirectPaymentDefaults.StripeConnectSyncTask) == null)
            {
                await _scheduleTaskService.InsertTaskAsync(new ScheduleTask
                {
                    Enabled = true,
                    LastEnabledUtc = DateTime.UtcNow,
                    Seconds = 5 * 60,
                    Name = StripeConnectRedirectPaymentDefaults.StripeConnectSyncTaskName,
                    Type = StripeConnectRedirectPaymentDefaults.StripeConnectSyncTask,
                    StopOnError = false
                });
            }

            //mark Widget as active
            if (!_widgetSettings.ActiveWidgetSystemNames.Contains(StripeConnectRedirectPaymentDefaults.SystemName))
            {
                _widgetSettings.ActiveWidgetSystemNames.Add(StripeConnectRedirectPaymentDefaults.SystemName);
                await _settingService.SaveSettingAsync(_widgetSettings);
            }

            //locales
            await _localizationService.AddOrUpdateLocaleResourceAsync(new Dictionary<string, string>
            {
                ["Plugin.Payments.StripeConnectRedirect.Configuration.Fields.TransactMode"] = "Transaction mode",
                ["Plugin.Payments.StripeConnectRedirect.Configuration.Fields.TransactMode.Hint"] = "Choose transaction mode",
                ["Plugin.Payments.StripeConnectRedirect.Configuration.Fields.AdditionalFee"] = "Additional fee",
                ["Plugin.Payments.StripeConnectRedirect.Configuration.Fields.AdditionalFee.Hint"] = "Enter additional fee to charge your customers.",
                ["Plugin.Payments.StripeConnectRedirect.Configuration.Fields.AdditionalFeePercentage"] = "Additional fee. Use percentage",
                ["Plugin.Payments.StripeConnectRedirect.Configuration.Fields.AdditionalFeePercentage.Hint"] = "Determines whether to apply a percentage additional fee to the order total. If not enabled, a fixed value is used.",
                ["Plugin.Payments.StripeConnectRedirect.Configuration.Fields.ClientId"] = "Client Id",
                ["Plugin.Payments.StripeConnectRedirect.Configuration.Fields.ClientId.Hint"] = "Enter Client Id",
                ["Plugin.Payments.StripeConnectRedirect.Configuration.Fields.PublishableApiKey"] = "Publishable key",
                ["Plugin.Payments.StripeConnectRedirect.Configuration.Fields.PublishableApiKey.Hint"] = "Enter Publishable key.",
                ["Plugin.Payments.StripeConnectRedirect.Configuration.Fields.SecretApiKey"] = "Secret key",
                ["Plugin.Payments.StripeConnectRedirect.Configuration.Fields.SecretApiKey.Hint"] = "Enter Secret key",
                ["Plugin.Payments.StripeConnectRedirect.Configuration.Fields.PaymentDescription"] = "Statement descriptor",
                ["Plugin.Payments.StripeConnectRedirect.Configuration.Fields.PaymentDescription.Hint"] = "A name that the browser shows the customer in the payment interface.",
                ["Plugin.Payments.StripeConnectRedirect.Configuration.Fields.WebhookUrl"] = "Webhook Url",
                ["Plugin.Payments.StripeConnectRedirect.Configuration.Fields.WebhookUrl.Hint"] = "Configure Webhook Url to Stripe Account.",
                ["Plugin.Payments.StripeConnectRedirect.Configuration.Fields.WebhookId"] = "Webhook Id",
                ["Plugin.Payments.StripeConnectRedirect.Configuration.Fields.WebhookId.Hint"] = "Stripe Webhook Id.",
                ["Plugin.Payments.StripeConnectRedirect.Configuration.Fields.SigningSecretKey"] = "Webhook Signing Secret",
                ["Plugin.Payments.StripeConnectRedirect.Configuration.Fields.SigningSecretKey.Hint"] = "Specify webhook 'Signing Secret' from webhook detail's page.",
                ["Plugin.Payments.StripeConnectRedirect.Configuration.Validator.ClientId.Required"] = "Client Id is required.",
                ["Plugin.Payments.StripeConnectRedirect.Configuration.Validator.PublishableApiKey.Required"] = "Publishable Key is required.",
                ["Plugin.Payments.StripeConnectRedirect.Configuration.Validator.SecretApiKey.Required"] = "Secret Key is required.",
                ["Plugin.Payments.StripeConnectRedirect.PaymentMethodDescription"] = "You will be redirected to Stripe site to complete the payment.",
                ["Plugin.Payments.StripeConnectRedirect.PaymentInfo.RedirectionTip"] = "You will be redirected to Stripe site to complete the payment.",
                ["Plugin.Payments.StripeConnectRedirect.Configure.ConfigurePage"] = "Configure",
                ["Plugin.Payments.StripeConnectRedirect.Configure.VendorPage"] = "Vendors",
                ["Plugin.Payments.StripeConnectRedirect.Configure.Information.Heading"] = "Information",
                ["Plugin.Payments.StripeConnectRedirect.Configure.Information.Instructions"] = "\r\n                <div class=\"callout bg-gray-light no-margin\">\r\n                    <p>For plugin configuration follow these steps:</p>\r\n                    <ul style=\"list-style-type:none;\">\r\n                        <li>\r\n                            1. Sign up for a <a href=\"https://dashboard.stripe.com/register\" target=\"_blank\">Stripe Account</a>\r\n                        </li>\r\n                        <li>\r\n                            2. Upon stripe account approval an email will be sent to you with your Virtual Terminal credentials where you can confirm your email address.\r\n                        </li>\r\n                        <li>\r\n                            3. Steps to obtain each:\r\n                            <ul style=\"list-style-type:none;\">\r\n                                <li>a. Sign into the Virtual Terminal with the login credentials that you received.</li>\r\n                                <li>b. To obtain your Keys, navigate to the Developers tab and select the API Key link.</li>\r\n                                <li>c. Select appropriate API keys – Publishable key and Secret key.</li>\r\n                            </ul>\r\n                        </li>\r\n                        <li>\r\n                            4. Fill in the remaining fields and save to complete the configuration\r\n                        </li>\r\n                        <li>\r\n                            5. Steps to Webhook for Recurring Billing Features:\r\n                            <ul style=\"list-style-type:none;\">\r\n                                <li>i. To configure your Webhook, navigate Developers tab and select the Webhooks link to the stripe webhook settings block.</li>\r\n                                <li>ii. Click Create Webhook button to create webhook on your stripe account.</li>\r\n                                <li>iii. Navigate to your stripe account Developers tab and select the Webhooks that we created from your store.</li>\r\n                                <li>iv. Where you can check selected below events from the list of events in Webhook details block.</li>\r\n                                    <ul style=\"list-style-type:none;\">\r\n                                        <li>- payment_intent.succeeded </li>\r\n                                        <li>- payment_intent.payment_failed </li>\r\n                                        <li>- charge.refunded </li>\r\n                                        <li>- customer.subscription.created </li>\r\n                                        <li>- customer.subscription.deleted </li>\r\n                                    </ul>\r\n                                <li>v. For field Webhook Signing Secret, After Webhook details block see the Signing secret block, Here click on 'Click to reveal' to retrive signing secret.</li>\r\n                            </ul>\r\n                        </li>\r\n                        <li>\r\n                            6. Save to complete the webhook configuration\r\n                        </li>\r\n                    </ul>\r\n                    <br /><b>Notes: </b>\r\n                    <ul style=\"list-style-type:none;\">\r\n                        <li>\r\n                            - <em>However, The payment form must be generated only on a webpage that uses HTTPS.</em>\r\n                        </li>\r\n                        <li>\r\n                            - <em>Some of the settings below are mandatory, if any is missing, the settings are invalid and the payment method is not shown to the customer on the checkout page.</em>\r\n                        </li>\r\n                        <li>\r\n                            - <em>The Stripe platform supports multiple currency; ensure that you have correctly configured the exchange rate from your primary store currency to the stripe supported currency. For more please contact to nopCommerceplus at '<b>info@nopCommerceplus.com</b>'.</em>\r\n                        </li>\r\n                    </ul>\r\n                </div>   \r\n            <br />",
                ["Plugin.Payments.StripeConnectRedirect.Configure.Common.Heading"] = "Common",
                ["Plugin.Payments.StripeConnectRedirect.Configure.Settings.Heading"] = "Settings",
                ["Plugin.Payments.StripeConnectRedirect.Configure.Settings.PaymentDescription.Note"] = "Must be at most 22 characters (including white-space)",
                ["Plugin.Payments.StripeConnectRedirect.Configure.Webhook.Heading"] = "Webhook",
                ["Plugin.Payments.StripeConnectRedirect.Configure.Webhook.Create.Button"] = "Create Webhook",
                ["Plugin.Payments.StripeConnectRedirect.Configure.Webhook.Remove.Button"] = "Remove Webhook",
                ["Plugin.Payments.StripeConnectRedirect.Configure.Fields.Vendor.Name"] = "Name",
                ["Plugin.Payments.StripeConnectRedirect.Configure.Fields.Vendor.Email"] = "Email",
                ["Plugin.Payments.StripeConnectRedirect.Configure.Fields.Vendor.IsActive"] = "Active",
                ["Plugin.Payments.StripeConnectRedirect.Configure.Fields.Vendor.Account"] = "Stripe Connected Account",
                ["Plugin.Payments.StripeConnectRedirect.Configure.Fields.Vendor.IsVerified"] = "Is Verified",
                ["Plugin.Payments.StripeConnectRedirect.Configure.Fields.Vendor.AdminDeliveryCommissionPercentage"] = "Admin Delivery Commission Percentage",
                ["Plugin.Payments.StripeConnectRedirect.Configure.Fields.Vendor.AdminPickupCommissionPercentage"] = "Admin Pickup Commission Percentage",
                ["Plugin.Payments.StripeConnectRedirect.Configure.Vendor.Account.NotLinkedYet"] = "Not linked yet",
                ["Plugin.Payments.StripeConnectRedirect.Webhook.CreateError"] = "Webhook is not created (see details in the log)",
                ["Plugin.Payments.StripeConnectRedirect.Webhook.RemoveError"] = "Webhook is not removed (see details in the log)",
                ["Plugin.Payments.StripeConnectRedirect.ProductIdPrefix"] = "Alchub-",
                ["Plugin.Payments.StripeConnectRedirect.ProductNamePrefix"] = "Alchub order number-",
                ["Plugin.Payments.StripeConnectRedirect.StripeConnect.VendorNotFound"] = "Vendor not found.",
                ["Plugin.Payments.StripeConnectRedirect.StripeConnect.Fail"] = "Vendor Connect failed (see details in the log)",
                ["Plugin.Payments.StripeConnectRedirect.StripeConnect.ConnectAccountNotFound"] = "Vendor connect account not found.",
                ["Plugin.Payments.StripeConnectRedirect.StripeConnect.ConnectedSuccessfully"] = "Account connected successfully.",
                ["Plugin.Payments.StripeConnectRedirect.StripeConnect.NotConnectedSuccessfully"] = "Account not connected successfully.",
                ["Plugin.Payments.StripeConnectRedirect.VendorConnect.InformationNeeded-UpdateAccount"] = "Your stripe connected account needs more information. Please Click \"Update Information\" button to complete the process.",
                ["Plugin.Payments.StripeConnectRedirect.VendorConnect.ConnectInformation"] = "Click \"Connect\" button, you will be redirected to Stripe website to connect with your merchant account.",
                ["Plugin.Payments.StripeConnectRedirect.VendorConnect.AlreadyConnected"] = "You are already connected with Stripe Connect account and your account id is {0}",
                ["Plugin.Payments.StripeConnectRedirect.StripeConnect.CardTitle"] = "Stripe Connect",
                ["Plugin.Payments.StripeConnectRedirect.StripeConnectDashboard.Button"] = "View Dashboard",
                ["Plugin.Payments.StripeConnectRedirect.StripeConnectRefresh.Button"] = "Update Information",
                ["Plugin.Payments.StripeConnectRedirect.StripeConnect.Button"] = "Connect",
                ["Plugin.Payments.StripeConnectRedirect.StripeConnect.NotConfigured"] = "Please Configure Stripe first.",
            });

            await base.InstallAsync();
        }

        /// <summary>
        /// Uninstall the plugin
        /// </summary>
        /// <returns>A task that represents the asynchronous operation</returns>
        public override async Task UninstallAsync()
        {
            //Uninstall StripeConnectSync schedule task
            var task = await _scheduleTaskService.GetTaskByTypeAsync(StripeConnectRedirectPaymentDefaults.StripeConnectSyncTask);
            if (task != null)
                await _scheduleTaskService.DeleteTaskAsync(task);

            //settings
            await _settingService.DeleteSettingAsync<StripeConnectRedirectPaymentSettings>();

            if (_widgetSettings.ActiveWidgetSystemNames.Contains(StripeConnectRedirectPaymentDefaults.SystemName))
            {
                _widgetSettings.ActiveWidgetSystemNames.Remove(StripeConnectRedirectPaymentDefaults.SystemName);
                await _settingService.SaveSettingAsync(_widgetSettings);
            }

            //locales
            await _localizationService.DeleteLocaleResourcesAsync("Plugin.Payments.StripeConnectRedirect");

            await base.UninstallAsync();
        }

        /// <summary>
        /// Gets a payment method description that will be displayed on checkout pages in the public store
        /// </summary>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task<string> GetPaymentMethodDescriptionAsync()
        {
            return await _localizationService.GetResourceAsync("Plugin.Payments.StripeConnectRedirect.PaymentMethodDescription");
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets a value indicating whether to hide this plugin on the widget list page in the admin area
        /// </summary>
        public bool HideInWidgetList => false;

        /// <summary>
        /// Gets a value indicating whether capture is supported
        /// </summary>
        public bool SupportCapture => true;

        /// <summary>
        /// Gets a value indicating whether partial refund is supported
        /// </summary>
        public bool SupportPartiallyRefund => false;

        /// <summary>
        /// Gets a value indicating whether refund is supported
        /// </summary>
        public bool SupportRefund => false;

        /// <summary>
        /// Gets a value indicating whether void is supported
        /// </summary>
        public bool SupportVoid => false;

        /// <summary>
        /// Gets a recurring payment type of payment method
        /// </summary>
        public RecurringPaymentType RecurringPaymentType => RecurringPaymentType.NotSupported;

        /// <summary>
        /// Gets a payment method type
        /// </summary>
        public PaymentMethodType PaymentMethodType => PaymentMethodType.Redirection;

        /// <summary>
        /// Gets a value indicating whether we should display a payment information page for this plugin
        /// </summary>
        public bool SkipPaymentInfo => false;

        #endregion
    }
}