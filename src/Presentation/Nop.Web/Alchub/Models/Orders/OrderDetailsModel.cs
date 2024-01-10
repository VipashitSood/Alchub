using System;
using System.Collections.Generic;
using Nop.Core.Domain.DeliveryFees;
using Nop.Core.Domain.TipFees;
using Nop.Web.Framework.Models;
using Nop.Web.Models.Common;

namespace Nop.Web.Models.Order
{
    /// <summary>
    /// Represents a order details model
    /// </summary>
    public partial record OrderDetailsModel : BaseNopEntityModel
    {
        public OrderDetailsModel()
        {
            TaxRates = new List<TaxRate>();
            GiftCards = new List<GiftCard>();
            Items = new List<OrderItemModel>();
            OrderNotes = new List<OrderNote>();
            Shipments = new List<ShipmentBriefModel>();

            BillingAddress = new AddressModel();
            ShippingAddress = new AddressModel();
            PickupAddress = new AddressModel();

            CustomValues = new Dictionary<string, object>();
            VendorWiseDeliveryFees = new List<VendorWiseDeliveryFee>();
            VendorWiseTipFees = new List<VendorWiseTipFee>();
            VendorPickupAddresses = new List<VendorPickupAddressModel>();
        }

        /// <summary>
        /// Gets or sets service fee
        /// </summary>
        public string ServiceFee { get; set; }

        /// <summary>
        /// Gets or sets Delivery fee
        /// </summary>
        public string DeliveryFee { get; set; }

        /// <summary>
        /// Gets or sets vendor wise delivery fee list
        /// </summary>
        public IList<VendorWiseDeliveryFee> VendorWiseDeliveryFees { get; set; }

        /// <summary>
        /// Gets or sets Tip fee
        /// </summary>
        public string TipFee { get; set; }

        /// <summary>
        /// Gets or sets vendor wise Tip fee list
        /// </summary>
        public IList<VendorWiseTipFee> VendorWiseTipFees { get; set; }

        /// <summary>
        /// Gets or sets service fee
        /// </summary>
        public string SlotFee { get; set; }

        /// <summary>
        /// Gets or sets TotalRefundAmount
        /// </summary>
        public string TotalRefundAmount { get; set; }

        /// <summary>
        /// Pickup addresses
        /// </summary>
        public IList<VendorPickupAddressModel> VendorPickupAddresses { get; set; }

        public partial record OrderItemModel : BaseNopEntityModel
        {

            /// <summary>
            /// Gets or sets the SlotPrice
            /// </summary>
            public decimal SlotPrice { get; set; }

            /// <summary> 
            /// Gets or sets the StartTime
            /// </summary>
            public string SlotStartTime { get; set; }

            /// <summary>
            /// Gets or sets the EndTime
            /// </summary>
            public string SlotTime { get; set; }

            public bool InPickup { get; set; }

            /// <summary>
            /// Gets or sets the custom attributes xml (for grouped product variant)
            /// </summary>
            public string CustomAttributeInfo { get; set; }

            /// <summary> 
            /// Gets or sets the OrderItemStatus
            /// </summary>
            public string OrderItemStatus { get; set; }

            /// <summary>
            /// Gets or sets TrackingUrl
            /// </summary>
            public string TrackingUrl { get; set; }
        }

        public partial class VendorPickupAddressModel
        {
            public int VendorId { get; set; }
            public string VendorName { get; set; }
            public string PickupAddress { get; set; }
        }
    }
}