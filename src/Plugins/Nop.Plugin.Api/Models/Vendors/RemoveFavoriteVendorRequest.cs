using Newtonsoft.Json;

namespace Nop.Plugin.Api.Models.Vendors
{
    public class RemoveFavoriteVendorRequest
    {
        /// <summary>
        /// vendor identifiers
        /// </summary>
        [JsonProperty("vendor_id")]
        public int VendorId { get; set; }
    }
}
