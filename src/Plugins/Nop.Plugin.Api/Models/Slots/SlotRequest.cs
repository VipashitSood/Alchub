using Newtonsoft.Json;

namespace Nop.Plugin.Api.Models.Slots
{
    public class SlotRequest
    {
        [JsonProperty("slot_id")]
        public int SlotId { get; set; }

        [JsonProperty("block_id")]
        public int BlockId { get; set; }

        [JsonProperty("slot_time")]
        public string SlotTime { get; set; }

        [JsonProperty("product_id")]
        public int productId { get; set; }

        [JsonProperty("start_date")]
        public string startDate { get; set; }

        [JsonProperty("end_date")]
        public string endDate { get; set; }

        [JsonProperty("vendor_id")]
        public int vendorId { get; set; }

        [JsonProperty("customer_id")]
        public int customerId { get; set; }

        [JsonProperty("is_pickup")]
        public bool isPickup { get; set; }
    }
}
