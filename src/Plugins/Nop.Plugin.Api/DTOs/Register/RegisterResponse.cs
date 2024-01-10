using Newtonsoft.Json;

namespace Nop.Plugin.Api.RegisterResponses
{
    [JsonObject(Title = "Register")]
    public class RegisterResponse
    {
        [JsonProperty("customer_id")]
        public int CustomerId { get; set; }

        //[JsonProperty("OTP")]
        //public int OTP { get; set; }

        [JsonProperty("AccessToken")]
        public string AccessToken { get; set; }

        [JsonProperty("otp_required_on_registration")]
        public bool OtpRequiredOnRegistration { get; set; }

        /// <summary>
        /// User registration type
        /// </summary>
        [JsonProperty("user_registration_type_id")]
        public int UserRegistrationTypeId { get; set; }
    }
}