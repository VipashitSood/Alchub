using Newtonsoft.Json;
using Nop.Plugin.Api.DTO.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Api.Models.ProductsParameters
{
    public class ProductEmailAFriend : BaseDto
    {
        [JsonProperty("user_id")]
        public int UserId { get; set; }

        [JsonProperty("product_id")]
        public int ProductId { get; set; }

        [JsonProperty("friendemailaddress")] 
        public string FriendEmailAddress { get; set; }

        [JsonProperty("youremailaddress")]
        public string YourEmailAddress { get; set; }

        [JsonProperty("perosnalmessage")]
        public string PersonalMessage { get; set; }
    }
}
