using Newtonsoft.Json;
using Nop.Plugin.Api.DTO.Base;

namespace Nop.Plugin.Api.DTO.HomeScreen
{
    [JsonObject(Title = "homescreen")]
    public class TopCategoryDto : BaseDto
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("display_order")]
        public int DisplayOrder { get; set; }

        [JsonProperty("image")]
        public string Image { get; set; }

        [JsonProperty("parent_category_id")]
        public int ParentCategoryId { get; set; }

        [JsonProperty("parent_category_name")]
        public string ParentCategoryName { get; set; }
    }
}
