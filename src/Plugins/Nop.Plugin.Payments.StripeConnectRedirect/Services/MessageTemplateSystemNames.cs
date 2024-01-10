namespace Nop.Plugin.Payments.StripeConnectRedirect.Services
{
    /// <summary>
    /// Represents message template system names
    /// </summary>
    public static partial class MessageTemplateSystemNames
    {
        #region Capture/Release

        /// <summary>
        /// Represents system name of notification about payment capture to the customer
        /// </summary>
        public const string CustomerPaymentCaptureNotification = "Customer.Payment.Capture.Notification";

        /// <summary>
        /// Represents system name of notification about payment Release to the customer
        /// </summary>
        public const string CustomerPaymentReleaseNotification = "Customer.Payment.Release.Notification";

        #endregion Capture/Release

    }
}