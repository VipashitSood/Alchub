using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Api.DTOs.Common
{
    [JsonObject(Title = "ContactUs")]
    public class ContactUsDto
    {
        [JsonProperty("message")]
        public string Message { get; set; }
    }
}
