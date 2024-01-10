using Newtonsoft.Json;

namespace Nop.Plugin.Api.Models.Vendors
{
    public class FavoriteToggleRequest
    {
        /// <summary>
        /// toggle on:off
        /// </summary>
        [JsonProperty("toggle")]
        public bool? Toggle { get; set; }

        [JsonProperty("clear_shopping_cart_required")]
        public bool ClearShoppingCartRequired { get; set; }
    }
    
    public class CanSwitchFavoriteToggleOnRequest
    {
        /// <summary>
        /// toggle on:off
        /// </summary>
        [JsonProperty("toggle")]
        public bool? Toggle { get; set; }
    }
}
