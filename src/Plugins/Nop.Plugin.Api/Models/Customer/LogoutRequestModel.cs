using Newtonsoft.Json;

namespace Nop.Plugin.Api.Models.Customer
{
    [JsonObject(Title = "Register")]
    public class LogoutRequestModel
    {
        [JsonProperty("customer_id")]
        public int CustomerId { get; set; }
    }
}
