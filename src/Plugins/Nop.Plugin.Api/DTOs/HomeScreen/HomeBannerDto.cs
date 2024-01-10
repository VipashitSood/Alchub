using Newtonsoft.Json;
using Nop.Plugin.Api.DTO.Base;

namespace Nop.Plugin.Api.DTO.HomeScreen
{
    [JsonObject(Title = "homebanner")]
    public class HomeBannerDto : BaseDto
    {
        [JsonProperty("image")]
        public string PictureUrl { get; set; }

        [JsonIgnore]
        public string Text { get; set; }

        [JsonIgnore]
        public string Link { get; set; }

        [JsonIgnore]
        public string AltText { get; set; }
    }
}
