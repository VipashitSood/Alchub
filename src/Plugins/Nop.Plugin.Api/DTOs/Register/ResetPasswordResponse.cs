using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Api.DTOs.Register
{
    [JsonObject(Title = "Register")]
    public class ResetPasswordResponse
    {
        [JsonProperty("customer_id")]
        public int CustomerId { get; set; }
    }
}
