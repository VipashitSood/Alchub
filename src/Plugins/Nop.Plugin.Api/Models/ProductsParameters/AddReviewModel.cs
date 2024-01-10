using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Api.Models.ProductsParameters
{
    public class AddReviewModel
    {
        [JsonProperty("user_id")]
        public int UserId { get; set; }

        [JsonProperty("product_id")]
        public int ProductId { get; set; }

        [JsonProperty("store_id")]
        public int StoreId { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("rating")]
        public int Rating { get; set; }
    }
}
