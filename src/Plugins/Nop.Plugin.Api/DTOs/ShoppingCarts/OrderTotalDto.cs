using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using Nop.Core.Domain.DeliveryFees;
using Nop.Core.Domain.TipFees;
using Nop.Plugin.Api.DTO.Base;
using Nop.Plugin.Api.DTOs.ShoppingCarts;
using System.Collections.Generic;

namespace Nop.Plugin.Api.DTO.ShoppingCarts
{
    [JsonObject(Title = "order_total")]
    public class OrderTotalDto
    {
        public OrderTotalDto()
        {
            GiftCards = new List<GiftCardDto>();
            VendorWiseDeliveryFees = new List<VendorWiseDeliveryFeeDto>();
            VendorWiseTipFees = new List<VendorWiseTipFeeDto>();
            AvailableTipTypes = new List<SelectListItem>();
            SlotWiseFee = new List<SlotWiseFeeDto>();
        }

        [JsonProperty("sub_total")]
        public string SubTotal { get; set; }

        [JsonProperty("sub_total_discount")]
        public string SubTotalDiscount { get; set; }

        [JsonProperty("shipping")]
        public string Shipping { get; set; }

        [JsonProperty("tax")]
        public string Tax { get; set; }

        [JsonProperty("order_total_discount")]
        public string OrderTotalDiscount { get; set; }

        [JsonProperty("order_total")]
        public string OrderTotal { get; set; }

        [JsonProperty("gift_cards")]
        public IList<GiftCardDto> GiftCards { get; set; }

        [JsonProperty("gift_card_total_discount")]
        public string GiftCardTotalDiscount { get; set; }

        [JsonProperty("is_shipping_required")]
        public bool ShippingRequired { get; set; }

        [JsonProperty("service_fee")]
        public string ServiceFee { get; set; }

        [JsonProperty("slot_fee")]
        public string SlotFee { get; set; }

        [JsonProperty("slot_wise_fees_list")]
        public IList<SlotWiseFeeDto> SlotWiseFee { get; set; }

        [JsonProperty("delivery_fee")]
        public string DeliveryFee { get; set; }

        [JsonProperty("vendor_wise_delivery_fees")]
        public IList<VendorWiseDeliveryFeeDto> VendorWiseDeliveryFees { get; set; }

        [JsonProperty("tip_fee")]
        public string TipFee { get; set; }

        [JsonProperty("vendor_wise_tip_fees")]
        public IList<VendorWiseTipFeeDto> VendorWiseTipFees { get; set; }

        [JsonProperty("tip_type_id")]
        public int TipTypeId { get; set; }

        [JsonProperty("custom_tip_amount")]
        public decimal CustomTipAmount { get; set; }

        [JsonProperty("available_tip_types")]
        public IList<SelectListItem> AvailableTipTypes { get; set; }
    }
}
