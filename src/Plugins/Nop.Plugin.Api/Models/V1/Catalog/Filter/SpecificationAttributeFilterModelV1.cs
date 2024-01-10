using Newtonsoft.Json;

namespace Nop.Plugin.Api.Models.V1.Catalog.Filter
{
    public record SpecificationAttributeFilterModelV1 : BaseFilterEntityModel
    {
        /// <summary>
        /// Gets or sets the specification attribute name
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the value indicating whether the value is selected
        /// </summary>
        [JsonProperty("selected")]
        public bool Selected { get; set; }
    }
}
