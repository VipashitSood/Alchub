using Newtonsoft.Json;

namespace Nop.Plugin.Api.DTOs.Register
{
    [JsonObject(Title = "Register")]
    public class ForgetPasswordResponse
    {
        [JsonProperty("OTP")]
        public int OTP { get; set; }

        [JsonProperty("customer_id")]
        public int CustomerId { get; set; }

        [JsonProperty("mobile_number")]
        public string MobileNumber { get; set; }
    }
}
