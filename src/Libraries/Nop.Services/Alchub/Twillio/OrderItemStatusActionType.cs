namespace Nop.Services.Alchub.Twillio
{
    /// <summary>
    /// Represents an order item status action type enumeration
    /// </summary>
    public enum OrderItemStatusActionType
    {
        /// <summary>
        /// Dispatched
        /// </summary>
        Dispatched = 10,

        /// <summary>
        /// Dispatch
        /// </summary>
        PickedUp = 20,

        /// <summary>
        /// Complete
        /// </summary>
        Delivered = 30,
    }
}
