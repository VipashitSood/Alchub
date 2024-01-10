using Nop.Core.Alchub.Domain.Vendors;

namespace Nop.Core.Domain.Vendors
{
    /// <summary>
    /// Represents a extended vendor
    /// </summary>
    public partial class Vendor
    {
        /// <summary>
        /// Gets or sets a value indicating whether the vendor manages the delivery or not
        /// </summary>
        public bool ManageDelivery { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the pick available or not
        /// </summary>
        public bool PickAvailable { get; set; }

        /// <summary>
        /// Gets or sets a value indicating minimum order amount
        /// </summary>
        public decimal MinimumOrderAmount { get; set; }

        /// <summary>
        /// Gets or sets the geo location coordinates
        /// </summary>
        public string GeoLocationCoordinates { get; set; }

        /// <summary>
        /// Gets or sets the geo fencing coordinates
        /// </summary>
        public string GeoFencingCoordinates { get; set; }

        /// <summary>
        /// Gets or sets a value indicating minimum order amount
        /// </summary>
        public decimal OrderTax { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the DeliveryAvailable / Manage Admin
        /// </summary>
        public bool DeliveryAvailable { get; set; }

        /// <summary>
        /// Gets or sets the phone number
        /// </summary>
        public string PhoneNumber { get; set; }

        /// <summary>
        /// Gets or sets the vendor address
        /// </summary>
        public string PickupAddress { get; set; }

        /// <summary>
        /// Gets or sets the geo fence shape type identifiers.
        /// </summary>
        public int GeoFenceShapeTypeId { get; set; }

        /// <summary>
        /// Gets or sets a total distance value to draw radius polygon when GeoFenceShapeType type is set to Radius.
        /// </summary>
        public decimal RadiusDistance { get; set; }

        /// <summary>
        /// Gets or sets the geo fence shape type
        /// </summary>
        public GeoFenceShapeType GeoFenceShapeType
        {
            get => (GeoFenceShapeType)GeoFenceShapeTypeId;
            set => GeoFenceShapeTypeId = (int)value;
        }
    }
}
