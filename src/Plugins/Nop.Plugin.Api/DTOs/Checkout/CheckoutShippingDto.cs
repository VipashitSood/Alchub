using Newtonsoft.Json;

namespace Nop.Plugin.Api.DTO.Checkout
{
    [JsonObject(Title = "checkout_shipping")]
    public class CheckoutShippingDto : CheckoutBaseDto
    {
        /// <summary>
        ///  Gets or sets the billing address id
        /// </summary>
        [JsonProperty("shipping_address_id")]
        public int ShippingAddressId { get; set; }
    }
}
