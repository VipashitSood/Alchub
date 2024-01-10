using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Api.Models.ProductsParameters
{
    public class HelpfulnessModel
    {
        [JsonProperty("userid")]
        public int UserId { get; set; }

        [JsonProperty("productreviewid")]
        public int ProductReviewId { get; set; }

        [JsonProperty("washelpfulness")]
        public bool WasHelpfulness { get; set; }
    }
}
