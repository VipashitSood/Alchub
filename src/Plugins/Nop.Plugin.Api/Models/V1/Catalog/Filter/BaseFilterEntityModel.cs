using Newtonsoft.Json;

namespace Nop.Plugin.Api.Models.V1.Catalog.Filter
{
    public record BaseFilterEntityModel
    {
        /// <summary>
        /// Gets or sets the entity identifiers
        /// </summary>
        [JsonProperty("id")]
        public int Id { get; set; }
    }
}
