using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.Payments.StripeConnectRedirect.Models;
using Nop.Plugin.Payments.StripeConnectRedirect.Services;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Payments;
using Nop.Web.Framework.Components;
using Stripe;

namespace Nop.Plugin.Payments.StripeConnectRedirect.Components
{
    [ViewComponent(Name = StripeConnectRedirectPaymentDefaults.StripeVendorConnectViewComponentName)]
    public class StripeVendorConnectViewComponent : NopViewComponent
    {
        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly IStoreContext _storeContext;
        private readonly IStripeConnectRedirectService _stripeConnectRedirectService;
        private readonly IWorkContext _workContext;
        private readonly IPaymentPluginManager _paymentPluginManager;
        private readonly ILogger _logger;
        private readonly StripeConnectRedirectPaymentSettings _stripeConnectRedirectPaymentSettings;

        #endregion Fields

        #region Ctor

        public StripeVendorConnectViewComponent(
            ILocalizationService localizationService,
            IStoreContext storeContext,
            IStripeConnectRedirectService stripeConnectRedirectService,
            IWorkContext workContext,
            IPaymentPluginManager paymentPluginManager,
            ILogger logger,
            StripeConnectRedirectPaymentSettings stripeConnectRedirectPaymentSettings)
        {
            _localizationService = localizationService;
            _storeContext = storeContext;
            _stripeConnectRedirectService = stripeConnectRedirectService;
            _workContext = workContext;
            _paymentPluginManager = paymentPluginManager;
            _logger = logger;
            _stripeConnectRedirectPaymentSettings = stripeConnectRedirectPaymentSettings;
        }

        #endregion Ctor

        #region Methods

        /// <summary>
        /// Invoke the widget view component
        /// </summary>
        /// <param name="additionalData"></param>
        /// <returns>/// A task that represents the asynchronous operation
        /// The task result contains the view component result
        /// </returns>
        public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
        {
            if (!(additionalData is Nop.Web.Areas.Admin.Models.Vendors.VendorModel vendorModel))
                return Content(string.Empty);

            if (vendorModel == null || vendorModel.Id <= 0)
                return Content(string.Empty);

            var customer = await _workContext.GetCurrentCustomerAsync();
            var store = await _storeContext.GetCurrentStoreAsync();
            if (!await _paymentPluginManager.IsPluginActiveAsync(StripeConnectRedirectPaymentDefaults.SystemName, customer, store?.Id ?? 0))
                return Content(string.Empty);

            if (!_stripeConnectRedirectService.IsConfigured(_stripeConnectRedirectPaymentSettings))
                return Content(string.Empty);

            //Stripe Vendor Connect Model
            StripeVendorConnectModel model = new StripeVendorConnectModel
            {
                VendorId = vendorModel.Id
            };

            try
            {
                var stripeVendorConnect = await _stripeConnectRedirectService.GetStripeVendorConnectByVendorIdAsync(vendorModel.Id);
                if (stripeVendorConnect != null && !string.IsNullOrEmpty(stripeVendorConnect.Account))
                {
                    model.IsConnected = true;

                    //If Account exists and Verified
                    if (stripeVendorConnect.IsVerified)
                    {
                        model.IsVerified = true;
                        model.Information = string.Format(await _localizationService.GetResourceAsync("Plugin.Payments.StripeConnectRedirect.VendorConnect.AlreadyConnected"), stripeVendorConnect.Account);
                    }
                    else
                    {
                        StripeConfiguration.ApiKey = _stripeConnectRedirectPaymentSettings.SecretApiKey;

                        var accountService = new AccountService();
                        var account = accountService.Get(stripeVendorConnect.Account);

                        //If Account not verified and errors sent by the Stripe
                        if (account.Requirements.Errors.Count > 0 || !account.ChargesEnabled || !account.PayoutsEnabled || !account.DetailsSubmitted)
                        {
                            model.IsRefreshRequired = true;
                            model.Information = string.Format(await _localizationService.GetResourceAsync("Plugin.Payments.StripeConnectRedirect.VendorConnect.InformationNeeded-UpdateAccount"));
                        }
                        else
                        {
                            stripeVendorConnect.IsVerified = true;
                            await _stripeConnectRedirectService.UpdateStripeVendorConnectAsync(stripeVendorConnect);
                            model.IsVerified = true;
                            model.Information = string.Format(await _localizationService.GetResourceAsync("Plugin.Payments.StripeConnectRedirect.VendorConnect.AlreadyConnected"), stripeVendorConnect.Account);
                        }
                    }
                }
                else
                    model.Information = string.Format(await _localizationService.GetResourceAsync("Plugin.Payments.StripeConnectRedirect.VendorConnect.ConnectInformation"));

            }
            catch (System.Exception ex)
            {
                await _logger.ErrorAsync("Stripe connect error: " + ex.Message, ex);

                model.IsConnected = false;
                model.Error = "Stripe connect error: " + ex.Message;
            }
            return View("~/Plugins/Payments.StripeConnectRedirect/Views/StripeVendorConnect.cshtml", model);
        }

        #endregion Methods
    }
}