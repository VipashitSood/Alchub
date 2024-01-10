using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Nop.Plugin.Api.DTO.Products;
using Nop.Plugin.Api.DTOs.ShoppingCarts;

namespace Nop.Plugin.Api.DTO.ShoppingCarts
{
    [JsonObject(Title = "add_cart")]
    public class AddCartRequestModel 
    {

        [JsonProperty("quantity")]
        public int Quantity { get; set; }

        [JsonProperty("shopping_cart_type", Required = Required.Always)]
        public ShoppingCartType ShoppingCartType { get; set; }

        [JsonProperty("product_id")]
        public int ProductId { get; set; }

        [JsonProperty("customer_id")]
        public int CustomerId { get; set; }

        [JsonProperty("master_product_id")]
        public int MasterProductId { get; set; }

        [JsonProperty("grouped_product_id")]
        public int GroupedProductId { get; set; }

        [JsonProperty("is_pickup")]
        public bool Ispickup { get; set; }
    }
}
