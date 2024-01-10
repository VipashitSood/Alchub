using Newtonsoft.Json;

namespace Nop.Plugin.Api.DTO.Checkout
{
    [JsonObject(Title = "checkout_billing")]
    public class CheckoutBillingDto : CheckoutBaseDto
    {
        /// <summary>
        ///  Gets or sets the billing address id
        /// </summary>
        [JsonProperty("billing_address_id")]
        public int BillingAddressId { get; set; }

        /// <summary>
        ///  Gets or sets ship to same address
        /// </summary>
        [JsonProperty("ship_to_same_address")]
        public bool ShipToSameAddress { get; set; }
    }
}
