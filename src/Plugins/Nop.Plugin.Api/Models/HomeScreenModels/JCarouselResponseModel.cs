using Newtonsoft.Json;
using Nop.Plugin.Api.DTOs.JCarousel;

namespace Nop.Plugin.Api.Models.HomeScreenModels
{
    [JsonObject(Title = "jcarousel_info")]
    public class JCarouselResponseModel 
    {
        /// <summary>
        /// Gets or sets the name
        /// </summary>
        [JsonProperty("jcarousel")]
        public JCarouselDto JCarouselDto { get; set; }

        /// <summary>
        /// Gets or sets the is visible
        /// </summary>
        [JsonProperty("is_visible")]
        public bool IsVisible { get; set; }
    }
}
