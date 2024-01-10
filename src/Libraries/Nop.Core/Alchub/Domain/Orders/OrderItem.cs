using System;
using Nop.Core.Alchub.Domain.Orders;

namespace Nop.Core.Domain.Orders
{
    /// <summary>
    /// Represents an order item
    /// </summary>
    public partial class OrderItem : BaseEntity
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

        /// <summary>
        /// Gets or sets the EndTime
        /// </summary>
        public bool InPickup { get; set; }

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

        /// <summary>
        /// Gets or sets the custom attributes description (for grouped product variant)
        /// </summary>
        public string CustomAttributesDescription { get; set; }

        /// <summary>
        /// Gets or sets the order item status identifier
        /// </summary>
        public int OrderItemStatusId { get; set; }

        /// <summary>
        /// Gets or sets the Vendor Manage Delivery
        /// </summary>
        public bool VendorManageDelivery { get; set; }

        /// <summary>
        /// Gets or sets the Delivery Fee
        /// </summary>
        public decimal DeliveryFee { get; set; }

        #region Custom properties

        /// <summary>
        /// Gets or sets the order item status
        /// </summary>
        public OrderItemStatus OrderItemStatus
        {
            get => (OrderItemStatus)OrderItemStatusId;
            set => OrderItemStatusId = (int)value;
        }

        #endregion
    }
}
