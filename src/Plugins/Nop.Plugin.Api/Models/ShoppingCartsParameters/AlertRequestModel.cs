using Newtonsoft.Json;

namespace Nop.Plugin.Api.DTO.ShoppingCarts
{
    [JsonObject(Title = "alert_request")]
    public class AlertRequestModel
    {

        [JsonProperty("product_id")]
        public int ProductId { get; set; }

        [JsonProperty("customer_id")]
        public int CustomerId { get; set; }

        [JsonProperty("masterproduct_id")]
        public int MasterProductId { get; set; }

        [JsonProperty("is_pickup")]
        public bool IsPickup { get; set; }

    }
}
