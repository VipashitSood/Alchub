using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Nop.Plugin.Api.DTO.ShoppingCarts
{
    public class ShoppingCartItemsRootObject : ISerializableObject
    {
        public ShoppingCartItemsRootObject()
        {
            ShoppingCartVendors = new List<ShoppingCartVendorDto>();
            CrossSellsProductIds = new List<int>();
            Warnings = new List<string>();
        }

        [JsonProperty("shopping_cart_vendors")]
        public IList<ShoppingCartVendorDto> ShoppingCartVendors { get; set; }

        public string GetPrimaryPropertyName()
        {
            return "shopping_carts";
        }

        public Type GetPrimaryPropertyType()
        {
            return typeof(ShoppingCartItemDto);
        }

        public partial record ShoppingCartVendorDto
        {
            public ShoppingCartVendorDto()
            {

                ShoppingCartItems = new List<ShoppingCartItemDto>();
            }

            /// <summary>
            /// Get or set the Vendor Id
            /// </summary>
            public int Id { get; set; }

            /// <summary>
            /// Get or set the Vendor Name
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Get or set the Warnings list
            /// </summary>
            public string Warnings { get; set; }

            [JsonProperty("shopping_carts")]
            public IList<ShoppingCartItemDto> ShoppingCartItems { get; set; }
        }


        [JsonProperty("count")]
        public int count { get; set; }
        public IList<int> CrossSellsProductIds { get; set; }

        public IList<string> Warnings { get; set; }

        public bool CanCheckout { get; set; }
    }

    public class WishlistItemsRootObject : ISerializableObject
    {
        public WishlistItemsRootObject()
        {
            WishlistItems = new List<ShoppingCartItemDto>();
        }

        [JsonProperty("wishlist_items")]
        public IList<ShoppingCartItemDto> WishlistItems { get; set; }

        public string GetPrimaryPropertyName()
        {
            return "wishlist_items";
        }

        public Type GetPrimaryPropertyType()
        {
            return typeof(ShoppingCartItemDto);
        }
    }
}
