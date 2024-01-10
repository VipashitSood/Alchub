namespace Nop.Core.Alchub.Domain.Orders
{
    /// <summary>
    /// Represents an order item status enumeration
    /// </summary>
    public enum OrderItemStatus
    {
        /// <summary>
        /// Pending
        /// </summary>
        Pending = 10,

        /// <summary>
        /// Dispatch
        /// </summary>
        Dispatch = 20,

        /// <summary>
        /// Complete
        /// </summary>
        Delivered = 30,

        /// <summary>
        /// Cancelled
        /// </summary>
        DeliveryDenied = 40,

        /// <summary>
        /// DeliveryDenied
        /// </summary>
        Cancelled = 50
    }
}
