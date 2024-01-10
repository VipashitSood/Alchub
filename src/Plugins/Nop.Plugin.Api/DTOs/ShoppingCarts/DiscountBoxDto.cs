using Newtonsoft.Json;
using Nop.Plugin.Api.DTO.Base;
using System.Collections.Generic;

namespace Nop.Plugin.Api.DTO.ShoppingCarts
{
    public class DiscountBoxDto
    {
        public DiscountBoxDto()
        {
            AppliedDiscountsWithCodes = new List<DiscountInfoDto>();
        }

        [JsonProperty("applied_discount_with_codes")]
        public List<DiscountInfoDto> AppliedDiscountsWithCodes { get; set; }

        [JsonProperty("display")]
        public bool Display { get; set; }

        public class DiscountInfoDto : BaseDto
        {
            [JsonProperty("coupon_code")]
            public string CouponCode { get; set; }
        }
    }

    public class ApplyDiscountCodeResponse
    {
        public ApplyDiscountCodeResponse()
        {
            Messages = new List<string>();
        }

        [JsonProperty("messages")]
        public List<string> Messages { get; set; }

        [JsonProperty("is_applied")]
        public bool IsApplied { get; set; }
    }

    public class ApplyDiscountCodeRequest
    {
        [JsonProperty("customerId")]
        public int CustomerId { get; set; }

        [JsonProperty("discountcouponcode")]
        public string DiscountCouponCode { get; set; }
    }
}
