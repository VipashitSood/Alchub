using Newtonsoft.Json;
using System.Collections.Generic;

namespace Nop.Plugin.Api.DTO.Checkout
{
    public class CheckoutShippingMethodDto
    {
        //response
        public CheckoutShippingMethodDto()
        {
            ShippingMethods = new List<ShippingMethodDto>();
            Warnings = new List<string>();
        }

        [JsonProperty("shipping_methods")]
        public IList<ShippingMethodDto> ShippingMethods { get; set; }

        [JsonProperty("warnings")]
        public IList<string> Warnings { get; set; }

        #region Nested classes

        public partial class ShippingMethodDto
        {
            [JsonProperty("shipping_rate_computation_method_system_name")]
            public string ShippingRateComputationMethodSystemName { get; set; }

            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("description")]
            public string Description { get; set; }

            [JsonProperty("fee")]
            public string Fee { get; set; }

            [JsonProperty("rate")]
            public decimal Rate { get; set; }

            [JsonProperty("display_order")]
            public int DisplayOrder { get; set; }
        }

        #endregion
    }

    //request
    public class CheckoutShippingMethodRequest : CheckoutBaseDto
    {
        [JsonProperty("shipping_address_id")]
        public int? ShippingAddressId { get; set; }     
    }

    //save shipping method request
    public class CheckoutSaveShippingMethodRequest : CheckoutBaseDto
    {
        [JsonProperty("shipping_method_name")]
        public string ShippingMethodName { get; set; }

        [JsonProperty("shipping_rate_computation_method_system_name")]
        public string ShippingRateComputationMethodSystemName { get; set; }
    }
}
