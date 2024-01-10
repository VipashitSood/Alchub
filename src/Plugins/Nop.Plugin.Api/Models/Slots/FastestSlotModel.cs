using Newtonsoft.Json;

namespace Nop.Plugin.Api.Models.Slots
{
    /// <summary>
    /// Represents a fastest slot model
    /// </summary>
    public partial record FastestSlotModel
    {
        [JsonProperty("product_id")]
        public int ProductId { get; set; }

        [JsonProperty("fastest_slot_timing")]
        public string FastestSlotTiming { get; set; }
    }
}
