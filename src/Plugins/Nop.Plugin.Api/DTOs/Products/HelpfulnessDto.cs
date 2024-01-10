using Newtonsoft.Json;
using Nop.Plugin.Api.DTO.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Api.DTOs.Products
{
    [JsonObject(Title = "LikeDislike")]
    public class HelpfulnessDto
    {
        [JsonProperty("likecount")]
        public int LikeCount { get; set; }

        [JsonProperty("dislikecount")]
        public int DislikeCount { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }
    }
}
