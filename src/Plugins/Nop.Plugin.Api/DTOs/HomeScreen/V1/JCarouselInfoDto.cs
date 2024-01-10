using Newtonsoft.Json;
using Nop.Plugin.Api.DTO.Base;

namespace Nop.Plugin.Api.DTOs.HomeScreen.V1
{
    [JsonObject(Title = "jcarousel_info")]
    public class JCarouselInfoDto : BaseDto
    {
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
        /// Gets or sets the DataSourceTypeId
        /// </summary>
        [JsonProperty("data_source_type_id")]
        public int DataSourceTypeId { get; set; }
    }
}
