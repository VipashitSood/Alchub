using Newtonsoft.Json;
using Nop.Web.Framework.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Api.Models.Customer
{
    public partial record WishListModel
    {
        [JsonProperty("user_Id")]
        public int UserId { get; set; }

        [JsonProperty("item_Id")]
        public int ItemId { get; set; }
    }
}
