using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Nop.Plugin.Api.DTO.Products;
using Nop.Plugin.Api.DTOs.ShoppingCarts;

namespace Nop.Plugin.Api.DTO.ShoppingCarts
{
    [JsonObject(Title = "add_tip")]
    public class AddTipRequestModel
    {

        [JsonProperty("tip_type_Id")]
        public int TipTypeId { get; set; }

        [JsonProperty("customer_id")]
        public int CustomerId { get; set; }

        [JsonProperty("custom_tip_amount")]
        public decimal CustomTipAmount { get; set; }

    }
}
