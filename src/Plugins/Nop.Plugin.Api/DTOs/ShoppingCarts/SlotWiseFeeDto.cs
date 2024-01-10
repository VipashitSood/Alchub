using Nop.Plugin.Api.DTO.Base;

namespace Nop.Plugin.Api.DTOs.ShoppingCarts
{
    public partial class SlotWiseFeeDto
    {

        /// <summary>
        /// Gets or sets SlotName
        /// </summary>
        public string VendorName { get; set; }

        /// <summary>
        /// Gets or sets SlotFee
        /// </summary>
        public string SlotFee { get; set; }

        public string SlotDateTime { get; set; }
    }
}