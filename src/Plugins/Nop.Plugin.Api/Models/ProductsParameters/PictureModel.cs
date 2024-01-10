using Newtonsoft.Json;

namespace Nop.Plugin.Api.Models.ProductsParameters
{
    public partial record PictureModel
    {
        [JsonProperty("image_url")]
        public string ImageUrl { get; set; }
        [JsonIgnore]
        public string ThumbImageUrl { get; set; }
        [JsonProperty("fullsize_image_url")]
        public string FullSizeImageUrl { get; set; }
        [JsonIgnore]
        public string Title { get; set; }
        [JsonIgnore]
        public string AlternateText { get; set; }
    }
}