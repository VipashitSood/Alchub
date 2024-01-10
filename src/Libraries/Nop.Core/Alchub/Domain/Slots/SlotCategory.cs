namespace Nop.Core.Domain.Slots
{
    /// <summary>
    /// Represents a product category mapping
    /// </summary>
    public partial class SlotCategory : BaseEntity
    {
        /// <summary>
        /// Gets or sets the Slot identifier
        /// </summary>
        public int SlotId { get; set; }

        /// <summary>
        /// Gets or sets the category identifier
        /// </summary>
        public int CategoryId { get; set; }

    }
}
