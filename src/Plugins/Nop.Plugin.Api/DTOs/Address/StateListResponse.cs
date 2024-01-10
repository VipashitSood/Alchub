using Newtonsoft.Json;
using Nop.Plugin.Api.DTO.Base;

namespace Nop.Plugin.Api.DTOs.Address
{
    [JsonObject(Title = "state")]
    
        public partial record StateListResponse 
        {
            public int id { get; set; }
            public string name { get; set; }
        }

}
