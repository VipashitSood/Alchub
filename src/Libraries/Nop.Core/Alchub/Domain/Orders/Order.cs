namespace Nop.Core.Domain.Orders
{
    /// <summary>
    /// Represents an order entity
    /// </summary>
    public partial class Order : BaseEntity
    {
        /// <summary>
        /// Gets or sets service fee
        /// </summary>
        public decimal ServiceFee { get; set; }

        public decimal SlotFee { get; set; }

        /// <summary>
        /// Gets or sets Delivery fee
        /// </summary>
        public decimal DeliveryFee { get; set; }

        /// <summary>
        /// Gets or sets Tip fee
        /// </summary>
        public decimal TipFee { get; set; }
    }
}