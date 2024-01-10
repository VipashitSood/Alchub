using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Infrastructure;
using Nop.Plugin.Payments.StripeConnectRedirect.Domain;
using Nop.Plugin.Payments.StripeConnectRedirect.Factories;
using Nop.Plugin.Payments.StripeConnectRedirect.Models;
using Nop.Plugin.Payments.StripeConnectRedirect.Services;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Services.Vendors;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Mvc.Filters;
using Stripe;
using Stripe.Checkout;

namespace Nop.Plugin.Payments.StripeConnectRedirect.Controllers
{
    [AutoValidateAntiforgeryToken]
    public class StripeConnectRedirectController : BasePaymentController
    {
        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly ILogger _logger;
        private readonly INotificationService _notificationService;
        private readonly IOrderService _orderService;
        private readonly IPermissionService _permissionService;
        private readonly IVendorModelFactory _vendorModelFactory;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;
        private readonly IStripeConnectRedirectService _stripeConnectRedirectService;
        private readonly IVendorService _vendorService;
        private readonly IWebHelper _webHelper;
        private readonly StripeConnectRedirectPaymentSettings _stripeConnectRedirectPaymentSettings;

        #endregion Fields

        #region Ctor

        public StripeConnectRedirectController(
            ILocalizationService localizationService,
            ILogger logger,
            INotificationService notificationService,
            IOrderService orderService,
            IPermissionService permissionService,
            IVendorModelFactory vendorModelFactory,
            ISettingService settingService,
            IStoreContext storeContext,
            IStripeConnectRedirectService stripeConnectRedirectService,
            IVendorService vendorService,
            IWebHelper webHelper,
            StripeConnectRedirectPaymentSettings stripeConnectRedirectPaymentSettings)
        {
            _localizationService = localizationService;
            _logger = logger;
            _notificationService = notificationService;
            _orderService = orderService;
            _permissionService = permissionService;
            _vendorModelFactory = vendorModelFactory;
            _settingService = settingService;
            _storeContext = storeContext;
            _stripeConnectRedirectService = stripeConnectRedirectService;
            _vendorService = vendorService;
            _webHelper = webHelper;
            _stripeConnectRedirectPaymentSettings = stripeConnectRedirectPaymentSettings;
        }

        #endregion Ctor

        #region Methods

        #region Configure

        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        public async Task<IActionResult> Configure()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePaymentMethods))
                return AccessDeniedView();

            //load settings for Stripe connect redirect payment plugin
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var stripeConnectRedirectPaymentSettings = await _settingService.LoadSettingAsync<StripeConnectRedirectPaymentSettings>(storeScope);

            var configurationModel = new ConfigurationModel()
            {
                AdditionalFee = stripeConnectRedirectPaymentSettings.AdditionalFee,
                AdditionalFeePercentage = stripeConnectRedirectPaymentSettings.AdditionalFeePercentage,
                ClientId = stripeConnectRedirectPaymentSettings.ClientId,
                SecretApiKey = stripeConnectRedirectPaymentSettings.SecretApiKey,
                PublishableApiKey = stripeConnectRedirectPaymentSettings.PublishableApiKey,
                PaymentDescription = stripeConnectRedirectPaymentSettings.PaymentDescription,
                WebhookId = stripeConnectRedirectPaymentSettings.WebhookId,
                SigningSecretKey = stripeConnectRedirectPaymentSettings.SigningSecretKey,
                WebhookUrl = string.Concat(_webHelper.GetStoreLocation(), StripeConnectRedirectPaymentDefaults.WebhookUrl),
                ActiveStoreScopeConfiguration = storeScope
            };

            if (storeScope > 0)
            {
                configurationModel.AdditionalFee_OverrideForStore = await _settingService.SettingExistsAsync<StripeConnectRedirectPaymentSettings, decimal>(stripeConnectRedirectPaymentSettings, (StripeConnectRedirectPaymentSettings x) => x.AdditionalFee, storeScope);
                configurationModel.AdditionalFeePercentage_OverrideForStore = await _settingService.SettingExistsAsync<StripeConnectRedirectPaymentSettings, bool>(stripeConnectRedirectPaymentSettings, (StripeConnectRedirectPaymentSettings x) => x.AdditionalFeePercentage, storeScope);
                configurationModel.ClientId_OverrideForStore = await _settingService.SettingExistsAsync<StripeConnectRedirectPaymentSettings, string>(stripeConnectRedirectPaymentSettings, (StripeConnectRedirectPaymentSettings x) => x.ClientId, storeScope);
                configurationModel.PublishableApiKey_OverrideForStore = await _settingService.SettingExistsAsync<StripeConnectRedirectPaymentSettings, string>(stripeConnectRedirectPaymentSettings, (StripeConnectRedirectPaymentSettings x) => x.PublishableApiKey, storeScope);
                configurationModel.SecretApiKey_OverrideForStore = await _settingService.SettingExistsAsync<StripeConnectRedirectPaymentSettings, string>(stripeConnectRedirectPaymentSettings, (StripeConnectRedirectPaymentSettings x) => x.SecretApiKey, storeScope);
                configurationModel.PaymentDescription_OverrideForStore = await _settingService.SettingExistsAsync<StripeConnectRedirectPaymentSettings, string>(stripeConnectRedirectPaymentSettings, (StripeConnectRedirectPaymentSettings x) => x.PaymentDescription, storeScope);
            }

            return View("~/Plugins/Payments.StripeConnectRedirect/Views/Configure.cshtml", configurationModel);
        }

        [HttpPost]
        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePaymentMethods))
                return AccessDeniedView();

            if (!ModelState.IsValid)
                return await Configure();

            //load settings for Stripe connect redirect payment plugin
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var stripeConnectRedirectPaymentSettings = await _settingService.LoadSettingAsync<StripeConnectRedirectPaymentSettings>(storeScope);

            stripeConnectRedirectPaymentSettings.AdditionalFee = model.AdditionalFee;
            stripeConnectRedirectPaymentSettings.AdditionalFeePercentage = model.AdditionalFeePercentage;
            stripeConnectRedirectPaymentSettings.ClientId = model.ClientId;
            stripeConnectRedirectPaymentSettings.PublishableApiKey = model.PublishableApiKey;
            stripeConnectRedirectPaymentSettings.SecretApiKey = model.SecretApiKey;
            stripeConnectRedirectPaymentSettings.PaymentDescription = model.PaymentDescription;
            stripeConnectRedirectPaymentSettings.WebhookId = model.WebhookId;
            stripeConnectRedirectPaymentSettings.SigningSecretKey = model.SigningSecretKey;

            /* We do not clear cache after each setting update.
             * This behavior can increase performance because cached settings will not be cleared 
             * and loaded from database after each update */
            await _settingService.SaveSettingOverridablePerStoreAsync<StripeConnectRedirectPaymentSettings, decimal>(stripeConnectRedirectPaymentSettings, (StripeConnectRedirectPaymentSettings x) => x.AdditionalFee, model.AdditionalFee_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync<StripeConnectRedirectPaymentSettings, bool>(stripeConnectRedirectPaymentSettings, (StripeConnectRedirectPaymentSettings x) => x.AdditionalFeePercentage, model.AdditionalFeePercentage_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync<StripeConnectRedirectPaymentSettings, string>(stripeConnectRedirectPaymentSettings, (StripeConnectRedirectPaymentSettings x) => x.ClientId, model.ClientId_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync<StripeConnectRedirectPaymentSettings, string>(stripeConnectRedirectPaymentSettings, (StripeConnectRedirectPaymentSettings x) => x.PublishableApiKey, model.PublishableApiKey_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync<StripeConnectRedirectPaymentSettings, string>(stripeConnectRedirectPaymentSettings, (StripeConnectRedirectPaymentSettings x) => x.SecretApiKey, model.SecretApiKey_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync<StripeConnectRedirectPaymentSettings, string>(stripeConnectRedirectPaymentSettings, (StripeConnectRedirectPaymentSettings x) => x.PaymentDescription, model.PaymentDescription_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingAsync<StripeConnectRedirectPaymentSettings, string>(stripeConnectRedirectPaymentSettings, (StripeConnectRedirectPaymentSettings x) => x.WebhookId, clearCache: false);
            await _settingService.SaveSettingAsync<StripeConnectRedirectPaymentSettings, string>(stripeConnectRedirectPaymentSettings, (StripeConnectRedirectPaymentSettings x) => x.SigningSecretKey, clearCache: false);

            //now clear settings cache
            await _settingService.ClearCacheAsync();

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Plugins.Saved"), true);

            return await Configure();
        }

        #endregion Configure

        #region Success/Cancel

        public virtual async Task<IActionResult> Success([FromQuery] string session_id)
        {
            StripeConfiguration.ApiKey = _stripeConnectRedirectPaymentSettings.SecretApiKey;

            var sessionService = new SessionService();
            Session session = await sessionService.GetAsync(session_id);

            if (session == null)
            {
                await _logger.ErrorAsync(String.Format("Stripe Connect Redirect: Session not found for Session Id: {0}", session_id));
                return RedirectToAction("Index", "Home", new { area = string.Empty });
            }

            var stripeOrder = await _stripeConnectRedirectService.GetStripeOrderBySessionIdAsync(session.Id);
            if (stripeOrder == null)
            {
                await _logger.ErrorAsync(String.Format("Stripe Connect Redirect: Stripe Order not found for Session Id: {0}", session_id));
                return RedirectToAction("Index", "Home", new { area = string.Empty });
            }

            var order = await _orderService.GetOrderByIdAsync(stripeOrder.OrderId);
            if (order == null)
            {
                await _logger.ErrorAsync(String.Format("Stripe Connect Redirect: Order not found for Order Id: {0}", stripeOrder.OrderId));
                return RedirectToAction("Index", "Home", new { area = string.Empty });
            }

            stripeOrder.SessionStatus = session.Status;
            stripeOrder.PaymentIntentId = session.PaymentIntentId;
            stripeOrder.PaymentStatus = session.PaymentStatus;
            await _stripeConnectRedirectService.UpdateStripeOrderAsync(stripeOrder);

            return RedirectToRoute("CheckoutCompleted", new { orderId = order.Id });
        }

        public virtual async Task<IActionResult> Cancel()
        {
            return RedirectToAction("Index", "Home", new { area = string.Empty });
        }

        #region Nop API plugin(mobile) success/cancel
        //Note: This 2 method has been mad specifically for mobile app developer, so they can catch event using url pattern. Web part has no relevance to these 2 methods.
        public virtual async Task<IActionResult> SuccessMobileApi([FromQuery] string session_id)
        {
            StripeConfiguration.ApiKey = _stripeConnectRedirectPaymentSettings.SecretApiKey;

            var sessionService = new SessionService();
            Session session = await sessionService.GetAsync(session_id);

            if (session == null)
                await _logger.ErrorAsync(String.Format("Stripe Connect Redirect(Mobile): Session not found for Session Id: {0}", session_id));

            var stripeOrder = await _stripeConnectRedirectService.GetStripeOrderBySessionIdAsync(session.Id);
            if (stripeOrder == null)
                await _logger.ErrorAsync(String.Format("Stripe Connect Redirect(Mobile): Stripe Order not found for Session Id: {0}", session_id));

            var order = await _orderService.GetOrderByIdAsync(stripeOrder.OrderId);
            if (order == null)
                await _logger.ErrorAsync(String.Format("Stripe Connect Redirect(Mobile): Order not found for Order Id: {0}", stripeOrder.OrderId));

            //temparary redirect url, so app developer can catch it using pattern and proceed further. 
            //return RedirectToAction("StripeConnectRedirect", "SuccessMobileEvent", new { area = string.Empty });
            return RedirectToAction("SuccessMobileEvent", "StripeConnectRedirect", new { area = string.Empty });
        }

        public virtual async Task<IActionResult> CancelMobileApi()
        {
            //temparary redirect url, so app developer can catch it using pattern and proceed further. 
            //return RedirectToAction("StripeConnectRedirect", "CancelMobileEvent", new { area = string.Empty });
            return RedirectToAction("CancelMobileEvent", "StripeConnectRedirect", new { area = string.Empty });
        }

        public virtual IActionResult SuccessMobileEvent()
        {
            return Ok();
        }

        public virtual IActionResult CancelMobileEvent()
        {
            return Ok();
        }

        #endregion

        #endregion Success/Cancel

        #region Vendor Connect With Stripe

        [HttpPost]
        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        public async Task<IActionResult> VendorList(VendorSearchModel searchModel)
        {
            //prepare model
            var model = await _vendorModelFactory.PrepareVendorListModelAsync(searchModel);

            return Json(model);
        }

        /// <summary>
        /// Vendor Connect With Stripe Update
        /// </summary>
        /// <param name="model"></param>
        [HttpPost]
        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        public virtual async Task<IActionResult> VendorConnectWithStripeUpdate(StripeVendorModel model)
        {
            var stripeVendorConnect = await _stripeConnectRedirectService.GetStripeVendorConnectByVendorIdAsync(model.Id);
            if (stripeVendorConnect == null)
            {
                StripeVendorConnect newStripeVendorConnect = new StripeVendorConnect
                {
                    VendorId = model.Id,
                    Account = null,
                    AdminDeliveryCommissionPercentage = model.AdminDeliveryCommissionPercentage,
                    AdminPickupCommissionPercentage = model.AdminPickupCommissionPercentage,
                    IsVerified = false,
                    CreatedOnUtc = DateTime.UtcNow
                };
                await _stripeConnectRedirectService.InsertStripeVendorConnectAsync(newStripeVendorConnect);
            }
            else
            {
                stripeVendorConnect.AdminDeliveryCommissionPercentage = model.AdminDeliveryCommissionPercentage;
                stripeVendorConnect.AdminPickupCommissionPercentage = model.AdminPickupCommissionPercentage;
                await _stripeConnectRedirectService.UpdateStripeVendorConnectAsync(stripeVendorConnect);
            }

            return new NullJsonResult();
        }

        protected virtual async Task<string> StripeAccountOnBoardingAsync(int vendorId, string accountId)
        {
            //OnBoarding
            var accountLinkOptions = new AccountLinkCreateOptions
            {
                Account = accountId,
                RefreshUrl = _webHelper.GetStoreLocation() + StripeConnectRedirectPaymentDefaults.RefreshUrl + vendorId,
                ReturnUrl = _webHelper.GetStoreLocation() + StripeConnectRedirectPaymentDefaults.ReturnUrl + vendorId,
                Type = "account_onboarding",
            };

            var accountLinkService = new AccountLinkService();
            var accountLink = await accountLinkService.CreateAsync(accountLinkOptions);

            return accountLink.Url;
        }

        [HttpPost]
        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        public virtual async Task<IActionResult> StripeConnect(int vendorId)
        {
            try
            {
                if (_stripeConnectRedirectService.IsConfigured(_stripeConnectRedirectPaymentSettings))
                {
                    var vendor = await _vendorService.GetVendorByIdAsync(vendorId);
                    if (vendor != null)
                    {

                        StripeConfiguration.ApiKey = _stripeConnectRedirectPaymentSettings.SecretApiKey;

                        var options = new AccountCreateOptions
                        {
                            Type = "express",
                            BusinessType = "individual"
                        };

                        var accountService = new AccountService();

                        var account = accountService.Create(options);

                        if (account != null)
                        {
                            var stripeVendorConnect = await _stripeConnectRedirectService.GetStripeVendorConnectByVendorIdAsync(vendor.Id);
                            if (stripeVendorConnect == null)
                            {
                                StripeVendorConnect newStripeVendorConnect = new StripeVendorConnect
                                {
                                    VendorId = vendor.Id,
                                    Account = account.Id,
                                    AdminDeliveryCommissionPercentage = decimal.Zero,
                                    AdminPickupCommissionPercentage = decimal.Zero,
                                    IsVerified = false,
                                    CreatedOnUtc = DateTime.UtcNow
                                };
                                await _stripeConnectRedirectService.InsertStripeVendorConnectAsync(newStripeVendorConnect);
                            }
                            else
                            {
                                stripeVendorConnect.Account = account.Id;
                                stripeVendorConnect.IsVerified = false;
                                stripeVendorConnect.CreatedOnUtc = DateTime.UtcNow;
                                await _stripeConnectRedirectService.UpdateStripeVendorConnectAsync(stripeVendorConnect);
                            }


                            var url = await StripeAccountOnBoardingAsync(vendorId, account.Id);
                            return Redirect(url);
                        }
                    }

                }
                else
                    _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Plugin.Payments.StripeConnectRedirect.StripeConnect.VendorNotFound"));
            }
            catch (Exception exc)
            {
                await _logger.ErrorAsync(string.Format("Stripe Connect Redirect: Vendor Connect for VendorId {0} failure message {1}", vendorId, exc.InnerException), exception: exc);
                _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Plugin.Payments.StripeConnectRedirect.StripeConnect.Fail"));
            }

            return RedirectToAction("Edit", "Vendor", new { id = vendorId, area = AreaNames.Admin });
        }

        [HttpPost]
        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        public virtual async Task<IActionResult> StripeConnectDashboard(int vendorId)
        {
            try
            {
                if (_stripeConnectRedirectService.IsConfigured(_stripeConnectRedirectPaymentSettings))
                {
                    StripeConfiguration.ApiKey = _stripeConnectRedirectPaymentSettings.SecretApiKey;
                    var vendor = await _vendorService.GetVendorByIdAsync(vendorId);

                    if (vendor != null)
                    {
                        var stripeVendorConnect = await _stripeConnectRedirectService.GetStripeVendorConnectByVendorIdAsync(vendor.Id);
                        if (stripeVendorConnect != null)
                        {
                            var loginLinkService = new LoginLinkService();
                            var accountLink = loginLinkService.Create(stripeVendorConnect.Account);

                            return Redirect(accountLink.Url);
                        }
                        else
                            _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Plugin.Payments.StripeConnectRedirect.StripeConnect.ConnectAccountNotFound"));
                    }
                    else
                        _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Plugin.Payments.StripeConnectRedirect.StripeConnect.VendorNotFound"));
                }
            }
            catch (Exception exc)
            {
                await _logger.ErrorAsync(string.Format("Stripe Connect Redirect: Vendor Connect Dashboard for VendorId {0} failure message {1}", vendorId, exc.InnerException), exception: exc);
                _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Plugin.Payments.StripeConnectRedirect.StripeConnect.Fail"));
            }

            return RedirectToAction("Edit", "Vendor", new { id = vendorId, area = AreaNames.Admin });
        }

        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        public virtual async Task<IActionResult> StripeConnectRefresh(int vendorId)
        {
            try
            {
                if (_stripeConnectRedirectService.IsConfigured(_stripeConnectRedirectPaymentSettings))
                {
                    var vendor = await _vendorService.GetVendorByIdAsync(vendorId);

                    if (vendor != null)
                    {
                        var stripeVendorConnect = await _stripeConnectRedirectService.GetStripeVendorConnectByVendorIdAsync(vendor.Id);
                        if (stripeVendorConnect != null)
                        {
                            var url = await StripeAccountOnBoardingAsync(vendorId, stripeVendorConnect.Account);
                            return Redirect(url);
                        }
                    }
                    else
                        _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Plugin.Payments.StripeConnectRedirect.StripeConnect.ConnectAccountNotFound"));
                }
                else
                    _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Plugin.Payments.StripeConnectRedirect.StripeConnect.VendorNotFound"));
            }
            catch (Exception exc)
            {
                await _logger.ErrorAsync(string.Format("Stripe Connect Redirect: Vendor Connect Refresh for VendorId {0} failure message {1}", vendorId, exc.InnerException), exception: exc);
                _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Plugin.Payments.StripeConnectRedirect.StripeConnect.Fail"));
            }

            //TODO Add notification here that link is expired, try again
            return RedirectToAction("Edit", "Vendor", new { id = vendorId, area = AreaNames.Admin });
        }

        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        public virtual async Task<IActionResult> StripeConnectSuccess(int vendorId)
        {
            try
            {
                if (_stripeConnectRedirectService.IsConfigured(_stripeConnectRedirectPaymentSettings))
                {
                    StripeConfiguration.ApiKey = _stripeConnectRedirectPaymentSettings.SecretApiKey;

                    var vendor = await _vendorService.GetVendorByIdAsync(vendorId);

                    if (vendor != null)
                    {
                        var stripeVendorConnect = await _stripeConnectRedirectService.GetStripeVendorConnectByVendorIdAsync(vendor.Id);
                        if (stripeVendorConnect != null && !string.IsNullOrEmpty(stripeVendorConnect.Account))
                        {
                            var accountService = new AccountService();
                            var account = await accountService.GetAsync(stripeVendorConnect.Account);

                            if (account.Requirements.Errors.Count == 0 && account.ChargesEnabled && account.PayoutsEnabled && account.DetailsSubmitted)
                            {
                                stripeVendorConnect.IsVerified = true;
                                await _stripeConnectRedirectService.UpdateStripeVendorConnectAsync(stripeVendorConnect);

                                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Plugin.Payments.StripeConnectRedirect.StripeConnect.ConnectedSuccessfully"));
                            }
                            else
                                _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Plugin.Payments.StripeConnectRedirect.StripeConnect.NotConnectedSuccessfully"));
                        }
                        else
                            _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Plugin.Payments.StripeConnectRedirect.StripeConnect.ConnectAccountNotFound"));
                    }
                }
                else
                    _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Plugin.Payments.StripeConnectRedirect.StripeConnect.VendorNotFound"));
            }
            catch (Exception exc)
            {
                await _logger.ErrorAsync(string.Format("Stripe Connect Redirect: Vendor Connect Success for VendorId {0} failure message {1}", vendorId, exc.InnerException), exception: exc);
                _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Plugin.Payments.StripeConnectRedirect.StripeConnect.Fail"));
            }

            return RedirectToAction("Edit", "Vendor", new { id = vendorId, area = AreaNames.Admin });
        }

        #endregion Vendor Connect With Stripe

        #region Webhook

        [ActionName("Configure")]
        [FormValueRequired(new string[] { "createwebhook" })]
        [HttpPost]
        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        public virtual async Task<IActionResult> CreateWebHook(ConfigurationModel model)
        {
            if (!_stripeConnectRedirectService.IsConfigured(_stripeConnectRedirectPaymentSettings))
            {
                _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Plugin.Payments.StripeConnectRedirect.StripeConnect.NotConfigured"));
                return await Configure();
            }

            var webhookEndpoint = await CreateWebHook();
            if (webhookEndpoint == null)
            {
                _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Plugin.Payments.StripeConnectRedirect.Webhook.CreateError"), true);
                return await Configure();
            }

            _stripeConnectRedirectPaymentSettings.WebhookId = webhookEndpoint.Id;
            _stripeConnectRedirectPaymentSettings.SigningSecretKey = webhookEndpoint.Secret;

            await _settingService.SaveSettingAsync<StripeConnectRedirectPaymentSettings, string>(_stripeConnectRedirectPaymentSettings, (StripeConnectRedirectPaymentSettings x) => x.WebhookId, clearCache: false);
            await _settingService.SaveSettingAsync<StripeConnectRedirectPaymentSettings, string>(_stripeConnectRedirectPaymentSettings, (StripeConnectRedirectPaymentSettings x) => x.SigningSecretKey, clearCache: false);
            await _settingService.ClearCacheAsync();

            return await Configure();
        }

        protected virtual async Task<WebhookEndpoint> CreateWebHook()
        {
            try
            {
                StripeConfiguration.ApiKey = _stripeConnectRedirectPaymentSettings.SecretApiKey;
                var webhookEndpointService = new WebhookEndpointService();

                if (!string.IsNullOrEmpty(_stripeConnectRedirectPaymentSettings.WebhookId))
                {
                    try
                    {
                        var webhookEndpoint = webhookEndpointService.Get(_stripeConnectRedirectPaymentSettings.WebhookId);
                        if (webhookEndpoint != null)
                        {
                            if (webhookEndpoint.Status != "enabled" || !StripeConnectRedirectPaymentDefaults.WebhookEvents.All(x => webhookEndpoint.EnabledEvents.Any(y => x == y)))
                            {
                                webhookEndpointService.Delete(_stripeConnectRedirectPaymentSettings.WebhookId);
                            }
                            else
                            {
                                return webhookEndpoint;
                            }
                        }
                    }
                    catch { }
                }

                //Store location
                string storeLocation = _webHelper.GetStoreLocation();

                //Create Webhook
                var webhookEndpointCreateOption = new WebhookEndpointCreateOptions
                {
                    Url = string.Concat(storeLocation, StripeConnectRedirectPaymentDefaults.WebhookUrl),
                    EnabledEvents = StripeConnectRedirectPaymentDefaults.WebhookEvents,
                    ApiVersion = StripeConfiguration.ApiVersion
                };

                var newWebhookEndpoint = webhookEndpointService.Create(webhookEndpointCreateOption);

                return newWebhookEndpoint;
            }
            catch (StripeException exc)
            {
                await _logger.ErrorAsync(string.Format("Stripe Connect Redirect: Webhook create failure message {0}", exc.StripeError), exc);
                return null;
            }
        }

        [ActionName("Configure")]
        [FormValueRequired(new string[] { "removewebhook" })]
        [HttpPost]
        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        public async Task<IActionResult> RemoveWebhook(ConfigurationModel model)
        {
            if (await RemoveWebHook())
            {
                _stripeConnectRedirectPaymentSettings.WebhookId = string.Empty;
                _stripeConnectRedirectPaymentSettings.SigningSecretKey = string.Empty;

                await _settingService.SaveSettingAsync<StripeConnectRedirectPaymentSettings, string>(_stripeConnectRedirectPaymentSettings, (StripeConnectRedirectPaymentSettings x) => x.WebhookId, clearCache: false);
                await _settingService.SaveSettingAsync<StripeConnectRedirectPaymentSettings, string>(_stripeConnectRedirectPaymentSettings, (StripeConnectRedirectPaymentSettings x) => x.SigningSecretKey, clearCache: false);
                await _settingService.ClearCacheAsync();
            }

            if (!string.IsNullOrEmpty(_stripeConnectRedirectPaymentSettings.WebhookId))
                _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Plugin.Payments.StripeConnectRedirect.Webhook.RemoveError"), true);

            return await Configure();
        }

        protected virtual async Task<bool> RemoveWebHook()
        {
            try
            {
                StripeConfiguration.ApiKey = _stripeConnectRedirectPaymentSettings.SecretApiKey;
                var webhookEndpointService = new WebhookEndpointService();
                if (!string.IsNullOrEmpty(_stripeConnectRedirectPaymentSettings.WebhookId))
                {
                    WebhookEndpoint webhookEndpoint = webhookEndpointService.Delete(_stripeConnectRedirectPaymentSettings.WebhookId);
                    if (webhookEndpoint == null)
                        return true;

                    return (!webhookEndpoint.Deleted.HasValue ? false : webhookEndpoint.Deleted.Value);
                }

                return true;
            }
            catch (StripeException exc)
            {
                await _logger.ErrorAsync(string.Format("Stripe Connect Redirect: Remove WebHook failure message {0}", exc.StripeError), exception: exc);
                return false;
            }
        }

        [IgnoreAntiforgeryToken]
        [HttpPost]
        public virtual async Task<IActionResult> Webhook()
        {
            try
            {
                StripeConfiguration.ApiKey = _stripeConnectRedirectPaymentSettings.SecretApiKey;
                var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
                var stripeEvent = EventUtility.ConstructEvent(
                    json,
                    Request.Headers["Stripe-Signature"],
                    _stripeConnectRedirectPaymentSettings.SigningSecretKey);

                if (stripeEvent.Type == Events.CheckoutSessionCompleted)
                {
                    Session session = stripeEvent.Data.Object as Session;

                    if (session != null)
                    {
                        var stripeOrder = await _stripeConnectRedirectService.GetStripeOrderBySessionIdAsync(session.Id);
                        if (stripeOrder != null)
                        {
                            stripeOrder.SessionStatus = session.Status;
                            stripeOrder.PaymentIntentId = session.PaymentIntentId;
                            stripeOrder.PaymentStatus = session.PaymentStatus;
                            await _stripeConnectRedirectService.UpdateStripeOrderAsync(stripeOrder);
                        }
                    }
                }
                else if (stripeEvent.Type == Events.PaymentIntentAmountCapturableUpdated)
                {
                    PaymentIntent paymentIntent = stripeEvent.Data.Object as PaymentIntent;

                    if (paymentIntent.Status == "requires_capture")
                        await _stripeConnectRedirectService.StripeOrderPaymentAuthorizedAsync(paymentIntent);

                    await _logger.InformationAsync(string.Format("Stripe Connect Redirect: Webhook PaymentIntentAmountCapturableUpdated Payment Intent Id: {0}, Payment Status: {1}", paymentIntent.Id, paymentIntent.Status));
                }
                else if (stripeEvent.Type == Events.PaymentIntentPaymentFailed)
                {
                    PaymentIntent paymentIntent = stripeEvent.Data.Object as PaymentIntent;

                    await _stripeConnectRedirectService.StripeOrderPaymentFailAsync(paymentIntent);
                    await _logger.InformationAsync(string.Format("Stripe Connect Redirect: Webhook PaymentIntentPaymentFailed Payment Intent Id: {0}, Payment Status: {1}", paymentIntent.Id, paymentIntent.Status));
                }
                else
                {
                    await _logger.InformationAsync(string.Format("Stripe Connect Redirect: Unhandled Webhook event type: {0}", stripeEvent.Type));
                }

                return Ok();
            }
            catch (StripeException stripeException)
            {
                await _logger.ErrorAsync(string.Format("Stripe Connect Redirect: Webhook event catch error: {0}", stripeException.InnerException), exception: stripeException);
                return BadRequest(stripeException);
            }
        }

        #endregion Webhook

        #endregion Methods
    }
}