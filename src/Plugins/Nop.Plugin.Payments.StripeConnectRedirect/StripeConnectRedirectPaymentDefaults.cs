using System.Collections.Generic;
using Stripe;

namespace Nop.Plugin.Payments.StripeConnectRedirect
{
    /// <summary>
    /// Represents Defaults of the Stripe Connect Redirect payment plugin
    /// </summary>
    public static partial class StripeConnectRedirectPaymentDefaults
    {
        /// <summary>
        /// Gets the plugin system name
        /// </summary>
        public static string SystemName => "Payments.StripeConnectRedirect";

        /// <summary>
        /// Webhook Url
        /// </summary>
        public const string WebhookUrl = "StripeConnectRedirect/Webhook";

        /// <summary>
        /// Name of the view component to display payment Info widget in public store
        /// </summary>
        public const string PaymentInfoViewComponentName = "StripeConnectRedirect.PaymentInfo.ViewComponent";

        /// <summary>
        /// Name of the view component to display Stripe Vendor Connect widget in public store
        /// </summary>
        public const string StripeVendorConnectViewComponentName = "StripeConnectRedirect.StripeVendorConnect.ViewComponent";

        /// <summary>
        /// Gets a name of the StripeConnectSync schedule task
        /// </summary>
        public static string StripeConnectSyncTaskName => "StripeConnectSyncTask";

        /// <summary>
        /// Gets a type of the StripeConnectSync schedule task
        /// </summary>
        public static string StripeConnectSyncTask => "Nop.Plugin.Payments.StripeConnectRedirect.Services.StripeConnectSyncTask";

        /// <summary>
        /// Gets or sets a Refresh Url
        /// </summary>
        public static string RefreshUrl => "stripeconnectrefresh/";

        /// <summary>
        /// Gets or sets a Return Url
        /// </summary>
        public static string ReturnUrl => "stripeconnectsuccess/";

        /// <summary>
        /// Gets or sets a Dashboard Url 
        /// </summary>
        public static string DashboardUrl => "StripeConnectRedirectDashboardUrl";

        /// <summary>
        /// Gets webhook events
        /// </summary>
        public static List<string> WebhookEvents => new()
        {
            Events.CheckoutSessionCompleted,
            Events.PaymentIntentAmountCapturableUpdated,
            Events.PaymentIntentPaymentFailed
        };

        /// <summary>
        /// Gets or sets a Payment Paid Status
        /// </summary>
        public static string PaymentPaidStatus => "paid";

        /// <summary>
        /// Gets or sets a Payment Unpaid Status
        /// </summary>
        public static string PaymentUnpaidStatus => "unpaid";

        /// <summary>
        /// Zero Decimal
        /// </summary>
        public readonly static string[] ZeroDecimal = new string[] { "BIF", "CLP", "DJF", "GNF", "JPY", "KMF", "KRW", "MGA", "PYG", "RWF", "UGX", "VND", "VUV", "XAF", "XOF", "XPF" };

    }
}