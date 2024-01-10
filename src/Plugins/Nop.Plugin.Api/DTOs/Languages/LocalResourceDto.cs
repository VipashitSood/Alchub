using Newtonsoft.Json;
using System.Collections.Generic;

namespace Nop.Plugin.Api.DTO.Languages
{
    [JsonObject(Title = "local_resource")]
    public class LocalResourceDto
    {
        public LocalResourceDto()
        {
            ResourseValues = new Dictionary<string, string>();
        }

        [JsonProperty("language_Id")]
        public int LanguageId { get; set; }

        [JsonProperty("resourse_values")]
        public IDictionary<string, string> ResourseValues { get; set; }
    }
}
