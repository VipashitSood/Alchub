using Newtonsoft.Json;

namespace Nop.Plugin.Api.DTO.Orders
{
    [JsonObject(Title = "re_order_request")]
    public class ReOrderRequestDto
    {
        [JsonProperty("user_id")]
        public int UserId { get; set; }

        [JsonProperty("order_id")]
        public int OrderId { get; set; }
    }
}
