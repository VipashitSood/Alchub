using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;

namespace Nop.Plugin.Api.Models.ReturnRequest
{
    public class AddReturnRequestModel
    {
        [JsonProperty("user_id")]
        public int UserId { get; set; }

        [JsonProperty("item_id")]
        public int ItemId { get; set; }

        [JsonProperty("return_reasonid")]
        public int ReturnRequestReasonId { get; set; }

        [JsonProperty("return_actionid")]
        public int ReturnRequestActionId { get; set; }

        [JsonProperty("quantity")]
        public int quantity { get; set; }

        [JsonProperty("returnrequests_uploadedfile")]
        public IFormFile ReturnOrderPicUrl { get; set; }
    }
}
