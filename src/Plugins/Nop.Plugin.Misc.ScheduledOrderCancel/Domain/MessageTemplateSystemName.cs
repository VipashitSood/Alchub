namespace Nop.Plugin.Misc.ScheduledOrderCancel.Domain
{
    /// <summary>
    /// Represents message template system names
    /// </summary>
    public static partial class MessageTemplateSystemName
    {
        #region Order

        /// <summary>
        /// Represents system name of notification customer about cancelled order due to payment failed
        /// </summary>
        public const string OrderCancelledCustomerCustomNotification = "OrderCancelledPaymentFailed.CustomerCustomNotification";

        #endregion

    }
}