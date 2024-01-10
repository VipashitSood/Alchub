using Newtonsoft.Json;

namespace Nop.Plugin.Api.Models.Address
{
    public class DeleteAddressModel
    {
        [JsonProperty("addressId")]
        public int addressId { get; set; }
        [JsonProperty("userId")]
        public int userId { get; set; }
    }
}
