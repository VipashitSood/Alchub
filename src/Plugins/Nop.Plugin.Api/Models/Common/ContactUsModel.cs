using Newtonsoft.Json;
using Nop.Plugin.Api.DTO.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Api.Models.Common
{
    public class ContactUsModel : BaseDto
    {
        [JsonProperty("user_id")]
        public int UserId { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("subject")]
        public string Subject { get; set; }

        [JsonProperty("enquiry")]
        public string Enquiry { get; set; }
    }
}
