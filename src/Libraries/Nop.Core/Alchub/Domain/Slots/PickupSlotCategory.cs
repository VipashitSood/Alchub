namespace Nop.Core.Domain.Slots
{
    /// <summary>
    /// Represents a product category mapping
    /// </summary>
    public partial class PickupSlotCategory : BaseEntity
    {
        /// <summary>
        /// Gets or sets the PickupSlot identifier
        /// </summary>
        public int PickupSlotId { get; set; }

        /// <summary>
        /// Gets or sets the category identifier
        /// </summary>
        public int CategoryId { get; set; }

    }
}
