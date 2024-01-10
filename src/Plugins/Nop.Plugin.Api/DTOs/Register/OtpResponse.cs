using Newtonsoft.Json;

namespace Nop.Plugin.Api.DTOs.Register
{
    [JsonObject(Title = "Register")]
    public class OtpResponse
    {
        [JsonProperty("customer_id")]
        public int CustomerId { get; set; }
    }
}
