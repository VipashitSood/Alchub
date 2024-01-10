using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Api.DTOs.Products
{
    [JsonObject(Title = "EamilAFriend")]
    public class ProductEmailAFriendDto
    {
        [JsonProperty("message")]
        public string Message { get; set; }
    }
}
