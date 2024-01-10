using Newtonsoft.Json;

namespace Nop.Plugin.Api.DTOs.Orders
{
    [JsonObject(Title = "order_return_request")]
    public class OrderReturnRequestDto
    {
        /// <summary>
        /// Gets or sets the value which indicates wether return request is allowed or not.
        /// </summary>
        [JsonProperty("is_return_request_allowed")]
        public bool IsReturnRequestAllowed { get; set; }

        /// <summary>
        /// Gets or sets text string which will be displayed for return request.
        /// </summary>
        [JsonProperty("return_request_message")]
        public string ReturnRequestMessage { get; set; }
    }
}
