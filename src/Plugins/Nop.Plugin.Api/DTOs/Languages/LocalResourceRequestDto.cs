using Newtonsoft.Json;

namespace Nop.Plugin.Api.DTO.Languages
{
    [JsonObject(Title = "local_resource_request")]
    public class LocalResourceRequestDto
    {
        [JsonProperty("language_Id")]
        public int LanguageId { get; set; }

        [JsonProperty("resource_name_prefix")]
        public string ResourceNamePrefix { get; set; }
    }
}
