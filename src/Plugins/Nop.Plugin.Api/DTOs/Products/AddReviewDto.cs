using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Api.DTOs.Products
{
    [JsonObject(Title = "AddReview")]
    public class AddReviewDto
    {
        [JsonProperty("message")]
        public string Message { get; set; }
    }
}
