using Newtonsoft.Json;

namespace Nop.Plugin.Api.DTO.Checkout
{
    [JsonObject(Title = "checkout_order_confirm")]
    public class CheckoutOrderConfirmDto : CheckoutBaseDto
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

        ///// <summary>
        /////  Gets or sets the billing address id
        ///// </summary>
        //[JsonProperty("shipping_address_id")]
        //public int? ShippingAddressId { get; set; }

        //[JsonProperty("shipping_method_name")]
        //public string ShippingMethodName { get; set; }

        //[JsonProperty("shipping_rate_computation_method_system_name")]
        //public string ShippingRateComputationMethodSystemName { get; set; }

        [JsonProperty("payment_method_system_name")]
        public string PaymentMethodSystemName { get; set; }
    }
}
