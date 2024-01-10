using Newtonsoft.Json;

namespace Nop.Plugin.Api.DTO.Checkout
{
    public class CheckoutBaseDto
    {
        /// <summary>
        ///  Gets or sets the customer id
        /// </summary>
        [JsonProperty("customer_id")]
        public int CustomerId { get; set; }
    }
}
