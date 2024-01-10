using Newtonsoft.Json;

namespace Nop.Plugin.Api.DTOs.Vendors
{
    public class CanSwitchFavoriteToggleOnDto
    {
        //[JsonProperty("can_switch_toggle_on")]
        //public bool CanSwitchToggleOn { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("clear_shopping_cart_required")]
        public bool ClearShoppingCartRequired { get; set; }
    }
}
