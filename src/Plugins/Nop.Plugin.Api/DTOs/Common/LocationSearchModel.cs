using Newtonsoft.Json;

namespace Nop.Plugin.Api.DTOs.Common
{
    [JsonObject(Title = "LocationSearch")]
    public class LocationSearchModel
    {
        [JsonProperty("customer_id")]
        public int CustomerId { get; set; }

        [JsonProperty("latitude")]
        public string Latitude { get; set; }

        [JsonProperty("longitude")]
        public string Longitude { get; set; }

        [JsonProperty("searched_text")]
        public string SearchedText { get; set; }

        [JsonProperty("clear_location")]
        public bool ClearLocation { get; set; }

        [JsonProperty("address_type")]
        public string AddressType { get; set; }
    }
}
