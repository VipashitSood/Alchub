using Newtonsoft.Json;
using Nop.Plugin.Api.Attributes;
using Nop.Plugin.Api.DTO.Base;
using Nop.Plugin.Api.DTO.Images;
using System.Collections.Generic;

namespace Nop.Plugin.Api.DTO.Products
{
    [JsonObject(Title = "product")]
    public class FeaturedProductDto : BaseDto
    {

        /// <summary>
        /// Gets or sets the name
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the price
        /// </summary>
        [JsonProperty("price")]
        public decimal? Price { get; set; }

        [ImageCollectionValidation]
        [JsonProperty("images")]
        public List<ImageMappingDto> Images { get; set; }

        [JsonIgnore]
        public List<int> AssociatedProductIds { get; set; }

        /// <summary>
        ///     Gets or sets a list of ids of products that are required to be added to the cart by this product
        /// </summary>
        [JsonIgnore]
        public IList<int> RequiredProductIds { get; set; }

    }
}
