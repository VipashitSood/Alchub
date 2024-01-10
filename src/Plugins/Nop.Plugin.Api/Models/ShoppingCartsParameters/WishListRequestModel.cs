using Newtonsoft.Json;
using Nop.Core.Domain.Orders;

namespace Nop.Plugin.Api.DTO.ShoppingCarts
{
    [JsonObject(Title = "wishList_request")]
    public class WishListRequestModel
    {
        [JsonProperty("product_id")]
        public int ProductId { get; set; }

        [JsonProperty("customer_id")]
        public int CustomerId { get; set; }

    }
}
