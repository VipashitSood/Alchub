using Nop.Core.Alchub.Domain.Common;

namespace Nop.Core.Domain.Common
{
    /// <summary>
    /// Address
    /// </summary>
    public partial class Address : BaseEntity
    {
        /// <summary>
        /// Gets or sets the geo location
        /// </summary>
        public string GeoLocation { get; set; }

        /// <summary>
        /// Gets or sets the geo location coordinates
        /// </summary>
        public string GeoLocationCoordinates { get; set; }

        /// <summary>
        /// Gets or sets the address type identifier
        /// </summary>
        public int AddressTypeId { get; set; }

        #region Custom properties

        /// <summary>
        /// Gets or sets the order status
        /// </summary>
        public AddressType AddressType
        {
            get => (AddressType)AddressTypeId;
            set => AddressTypeId = (int)value;
        }

        #endregion
    }
}