using System;

namespace Nop.Core.Domain.Orders
{
    /// <summary>
    /// Represents an order entity
    /// </summary>
    public partial class Dispatch : BaseEntity
    {

        /// <summary>
        /// Gets or sets External Delivery Id
        /// </summary>
        public string ExtrnalDeliveryId { get; set; }
        /// <summary>
        /// Gets or sets Order Item Id
        /// </summary>
        public int OrderItemId { get; set; }
        /// <summary>
        /// Gets or sets Order Number
        /// </summary>
        public int OrderNumber { get; set; }

        /// <summary>
        /// Gets or sets Vendor Id
        /// </summary>
        public int VendorId { get; set; }

        /// <summary>
        /// Gets or sets Vendor Name
        /// </summary>
        public string VendorName { get; set; }

        /// <summary>
        /// Gets or sets Time Slot
        /// </summary>
        public string TimeSlot { get; set; }


        /// <summary>
        /// Gets or sets Fee
        /// </summary>
        public decimal Fee { get; set; }

        /// <summary>
        /// Gets or sets Tip
        /// </summary>
        public decimal Tip { get; set; }

        /// <summary>
        /// Gets or sets Delivery Status
        /// </summary>
        public string DeliveryStatus { get; set; }

        /// <summary>
        /// Gets or sets Tracking Url
        /// </summary>
        public string TrackingUrl { get; set; }

        /// <summary>
        /// Gets or sets Dasher Name
        /// </summary>
        public string DasherName { get; set; }

        /// <summary>
        /// Gets or sets Dash Phone Number
        /// </summary>
        public string DashPhoneNumber { get; set; }

        /// <summary>
        /// Gets or sets Dash Vehicle Number
        /// </summary>
        public string DashVehicleNumber { get; set; }

        /// <summary>
        /// Gets or sets Customer Signature
        /// </summary>
        public string CustomerSignature { get; set; }

        /// <summary>
        /// Gets or sets the date and time when entity was created
        /// </summary>
        public DateTime CreatedOnUtc { get; set; }
    }
}