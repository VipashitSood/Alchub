using Newtonsoft.Json;
using Nop.Plugin.Api.DTO.Base;
using System.Collections.Generic;

namespace Nop.Plugin.Api.DTO.ShoppingCarts
{
    public class GiftCardBoxDto
    {
        public GiftCardBoxDto()
        {
            GiftCards = new List<GiftCardDto>();
        }

        [JsonProperty("gift_cards")]
        public IList<GiftCardDto> GiftCards { get; set; }

        [JsonProperty("gift_card_total_discount")]
        public string GiftCardTotalDiscount { get; set; }

        [JsonProperty("display")]
        public bool Display { get; set; }
    }

    public class GiftCardDto : BaseDto
    {
        public string CouponCode { get; set; }
        public string Amount { get; set; }
        public string Remaining { get; set; }
    }

    public class ApplyGiftCardRequest
    {
        [JsonProperty("customerId")]
        public int CustomerId { get; set; }

        [JsonProperty("giftcardcouponcode")]
        public string GiftCardCouponCode { get; set; }
    }

    public class ApplyGiftCardResponse
    {
        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("is_applied")]
        public bool IsApplied { get; set; }
    }
}
