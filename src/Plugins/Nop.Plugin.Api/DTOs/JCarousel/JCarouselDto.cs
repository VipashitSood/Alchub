using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Nop.Plugin.Api.DTO.Base;
using Nop.Plugin.Api.DTO.Products;

namespace Nop.Plugin.Api.DTOs.JCarousel
{
    [JsonObject(Title = "jcarousel")]
    public class JCarouselDto : BaseDto
    {
        public JCarouselDto()
        {
            Products = new List<ProductListDto>();
        }
        /// <summary>
        /// Gets or sets the name
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the display order
        /// </summary>
        [JsonProperty("display_order")]
        public int DisplayOrder { get; set; }

        /// <summary>
        /// Gets or sets the category identidier
        /// </summary>
        [JsonProperty("category_id")]
        public int CategoryId { get; set; }

        /// <summary>
        /// Gets or sets the parent category identidier
        /// </summary>
        [JsonProperty("parent_category_id")]
        public int ParentCategoryId { get; set; }

        /// <summary>
        /// Gets or sets the parent category name
        /// </summary>
        [JsonProperty("parent_category_name")]
        public string ParentCategoryName { get; set; }

        /// <summary>
        /// Gets or sets the products
        /// </summary>
        [JsonProperty("products")]
        public IList<ProductListDto> Products { get; set; }
    }

    //[JsonObject(Title = "jcarousel_product")]
    //public class JCarouselProductDto : BaseDto
    //{
    //    /// <summary>
    //    /// Gets or sets the name
    //    /// </summary>
    //    [JsonProperty("name")]
    //    public string Name { get; set; }

    //    /// <summary>
    //    /// Gets or sets the price
    //    /// </summary>
    //    [JsonProperty("price")]
    //    public decimal? Price { get; set; }
    //}
}
