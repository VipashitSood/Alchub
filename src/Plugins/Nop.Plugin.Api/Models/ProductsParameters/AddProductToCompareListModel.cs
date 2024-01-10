using Newtonsoft.Json;

namespace Nop.Plugin.Api.Models.ProductsParameters
{
    public class AddProductToCompareListModel
    {
        [JsonProperty("product_id")]
        public int productId { get; set; }
    }
}
