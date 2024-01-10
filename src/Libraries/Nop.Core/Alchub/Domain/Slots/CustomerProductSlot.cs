using System;

namespace Nop.Core.Domain.Slots
{
    /// <summary>
    /// Represents Zone entity
    /// </summary>
    public partial class CustomerProductSlot : BaseEntity
    {
        /// <summary> 
        /// Gets or sets the StartTime
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary> 
        /// Gets or sets the StartTime
        /// </summary>
        public DateTime EndDateTime { get; set; }

        /// <summary>
        /// Gets or sets the EndTime
        /// </summary>
        public string EndTime { get; set; }

        /// <summary>
        /// Gets or sets OrderId
        /// </summary>
        public int CustomerId { get; set; }

        /// <summary>
        /// Gets or sets OrderId
        /// </summary>
        public int ProductId { get; set; }

        /// <summary>
        /// Gets or sets the SlotId
        /// </summary>
        public int SlotId { get; set; }

        /// <summary>
        /// Gets or sets the SlotId
        /// </summary>
        public int BlockId { get; set; }

        /// <summary>
        /// Gets or sets the Price
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// Gets or sets the IsPickup
        /// </summary>
        public bool IsPickup { get; set; }

        /// <summary>
        /// Gets or sets the IsSelected
        /// </summary>
        public bool IsSelected { get; set; }

        /// <summary>
        /// Gets or sets the LastUpdated
        /// </summary>
        public DateTime LastUpdated { get; set; }
    }
}
