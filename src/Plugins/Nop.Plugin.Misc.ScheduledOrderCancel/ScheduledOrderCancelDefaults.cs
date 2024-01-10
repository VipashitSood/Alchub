namespace Nop.Plugin.Misc.ScheduledOrderCancel
{
    /// <summary>
    /// Represents plugin constants
    /// </summary>
    public class ScheduledOrderCancelDefaults
    {
        /// <summary>
        /// Gets the Status provider system name
        /// </summary>
        public static string SystemName => "Misc.ScheduledOrderCancel";

        /// <summary>
        /// Gets a name of the ScheduledOrderCancel schedule task
        /// </summary>
        public static string ScheduledOrderCancelTaskName => "Cancel Pending Orders(Scheduled Order Cancelation plugin)";

        /// <summary>
        /// Gets a type of the ScheduledOrderCancel schedule task
        /// </summary>
        public static string ScheduledOrderCancelTask => "Nop.Plugin.Misc.ScheduledOrderCancel.Services.ScheduledOrderCancelTask";

        /// <summary>
        /// Gets a default synchronization period in hours
        /// </summary>
        public static int DefaultSynchronizationPeriod => 12;

        /// <summary>
        /// Gets a default synchronization limit of Lists
        /// </summary>
        public static int DefaultSynchronizationListsLimit => 50;

        /// <summary>
        /// Gets the configuration route name
        /// </summary>
        public static string ConfigurationRouteName => "Plugin.Misc.ScheduledOrderCancel.Configure";
    }
}