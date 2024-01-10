using Newtonsoft.Json;
using System.Collections.Generic;

namespace Nop.Plugin.Api.DTO.ShoppingCarts
{
    public class ShoppingCartItemsListObject : ShoppingCartItemsRootObject
    {
        [JsonProperty("pageIndex")]
        public int PageIndex { get; set; }

        [JsonProperty("pageSize")]
        public int PageSize { get; set; }

        [JsonProperty("total_records")]
        public int TotalRecords { get; set; }
    }

    public class WishlistItemsListObject : WishlistItemsRootObject
    {
        [JsonProperty("pageIndex")]
        public int PageIndex { get; set; }

        [JsonProperty("pageSize")]
        public int PageSize { get; set; }

        [JsonProperty("total_records")]
        public int TotalRecords { get; set; }
    }
}
