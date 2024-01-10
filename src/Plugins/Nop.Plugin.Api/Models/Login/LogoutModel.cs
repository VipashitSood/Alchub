using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Api.Models.Login
{
    public class LogoutModel
    {
        [JsonProperty("user_id")]
        public int UserId { get; set; }
    }
}
