using Newtonsoft.Json;

namespace Nop.Plugin.Api.DTOs.Address
{
    [JsonObject(Title = "Address")]
    public class AddressResponse
    {
        [JsonProperty("AddressId")]
        public int AddressId { get; set; }
    }
}
