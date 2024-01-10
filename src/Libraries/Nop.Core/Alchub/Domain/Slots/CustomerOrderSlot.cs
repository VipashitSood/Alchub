using System;

namespace Nop.Core.Domain.Slots
{
    /// <summary>
    /// Represents Zone entity
    /// </summary>
    public partial class CustomerOrderSlot : BaseEntity
    {
        /// <summary> 
        /// Gets or sets the StartTime
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// Gets or sets the EndTime
        /// </summary>
        public DateTime EndTime { get; set; }

        /// <summary>
        /// Gets or sets OrderId
        /// </summary>
        public int OrderId { get; set; }

        /// <summary>
        /// Gets or sets the SlotId
        /// </summary>
        public int SlotId { get; set; }

        /// <summary>
        /// Gets or sets the Price
        /// </summary>
        public decimal Price { get; set; }

    }
}
