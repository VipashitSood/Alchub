using Newtonsoft.Json;
using System.Collections.Generic;

namespace Nop.Plugin.Api.Models.ProductsParameters
{
    public class CompareProductsModel
    {
        public CompareProductsModel()
        {
            Products = new List<ProductOverviewModel>();
        }
        [JsonProperty("Products")]
        public IList<ProductOverviewModel> Products { get; set; }

        [JsonProperty("include_short_description_in_compare_products")]
        public bool IncludeShortDescriptionInCompareProducts { get; set; }

        [JsonProperty("include_full_description_in_compare_products")]
        public bool IncludeFullDescriptionInCompareProducts { get; set; }
    }
}