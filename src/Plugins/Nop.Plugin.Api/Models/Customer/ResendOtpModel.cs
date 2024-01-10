using Newtonsoft.Json;

namespace Nop.Plugin.Api.Models.Customer
{
    public class ResendOtpModel
    {
        [JsonProperty("Userid")]
        public int Userid { get; set; }
    }
}
