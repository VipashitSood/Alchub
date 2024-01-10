using System;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Services.Configuration;
using Nop.Services.Logging;
using Nop.Services.ScheduleTasks;
using Stripe;

namespace Nop.Plugin.Payments.StripeConnectRedirect.Services
{
    /// <summary>
    /// Represents a schedule task to synchronize Stripe Connect
    /// </summary>
    public class StripeConnectSyncTask : IScheduleTask
    {
        #region Fields

        private readonly IStripeConnectRedirectService _stripeConnectRedirectService;
        private readonly ILogger _logger;
        private readonly StripeConnectRedirectPaymentSettings _stripeConnectRedirectPaymentSettings;

        #endregion

        #region Ctor

        public StripeConnectSyncTask(
            IStripeConnectRedirectService stripeConnectRedirectService,
            ILogger logger,
            StripeConnectRedirectPaymentSettings stripeConnectRedirectPaymentSettings)
        {
            _stripeConnectRedirectService = stripeConnectRedirectService;
            _logger = logger;
            _stripeConnectRedirectPaymentSettings = stripeConnectRedirectPaymentSettings;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Execute task
        /// </summary>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task ExecuteAsync()
        {
            try
            {
                if (_stripeConnectRedirectService.IsConfigured(_stripeConnectRedirectPaymentSettings))
                {
                    var stripeVendorConnects = await _stripeConnectRedirectService.GetStripeVendorConnectsForVerifyingAsync();
                    if (stripeVendorConnects != null && stripeVendorConnects.Count > 0)
                    {
                        StripeConfiguration.ApiKey = _stripeConnectRedirectPaymentSettings.SecretApiKey;
                        var service = new AccountService();
                        foreach (var stripeVendorConnect in stripeVendorConnects)
                        {
                            var account = service.Get(stripeVendorConnect.Account);

                            if (account.Requirements.Errors.Count == 0 && account.ChargesEnabled && account.PayoutsEnabled && account.DetailsSubmitted)
                            {
                                stripeVendorConnect.IsVerified = true;
                                await _stripeConnectRedirectService.UpdateStripeVendorConnectAsync(stripeVendorConnect);
                            }
                        }
                    }
                }
            }
            catch (Exception exc)
            {
                await _logger.ErrorAsync(string.Format("Stripe Connect Redirect: Vendor Connect Sync failure message {0}", exc.InnerException), exception: exc);
            }
        }

        #endregion
    }
}