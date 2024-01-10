using Newtonsoft.Json;

namespace Nop.Plugin.Api.Models.ReturnRequest
{
    public class ReturnRequestModel
    {
        [JsonProperty("order_id")]
        public int OrderId { get; set; }

        [JsonProperty("item_id")]
        public int ItemId { get; set; }

        [JsonProperty("user_id")]
        public int UserId { get; set; }
    }
}
