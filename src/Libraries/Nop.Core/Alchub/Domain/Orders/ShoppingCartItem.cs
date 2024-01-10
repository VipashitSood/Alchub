using System;

namespace Nop.Core.Domain.Orders
{
    /// <summary>
    /// Represents a shopping cart item
    /// </summary>
    public partial class ShoppingCartItem : BaseEntity
    {
        /// <summary>
        /// Gets or sets the SlotId
        /// </summary>
        public int SlotId { get; set; }

        /// <summary>
        /// Gets or sets the SlotPrice
        /// </summary>
        public decimal SlotPrice { get; set; }

        /// <summary> 
        /// Gets or sets the StartTime
        /// </summary>
        public DateTime SlotStartTime { get; set; }

        /// <summary> 
        /// Gets or sets the SlotEndTime
        /// </summary>
        public DateTime SlotEndTime { get; set; }

        /// <summary>
        /// Gets or sets the EndTime
        /// </summary>
        public string SlotTime { get; set; }

        public bool IsPickup { get; set; }

        /// <summary>
        /// Gets or sets the master product identifier
        /// </summary>
        public int MasterProductId { get; set; }

        /// <summary>
        /// Gets or sets the grouped product identifier
        /// </summary>
        public int GroupedProductId { get; set; }

        /// <summary>
        /// Gets or sets the custom attributes xml (for grouped product variant)
        /// </summary>
        public string CustomAttributesXml { get; set; }
    }
}
