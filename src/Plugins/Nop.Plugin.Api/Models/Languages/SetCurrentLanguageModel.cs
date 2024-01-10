using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Api.Models.Languages
{
    public class SetCurrentLanguageModel
    {
        [JsonProperty("id")]
        public int Id { get; set; }
    }
}
