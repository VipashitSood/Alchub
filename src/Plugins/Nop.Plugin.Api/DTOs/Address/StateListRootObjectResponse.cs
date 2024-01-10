using Newtonsoft.Json;
using Nop.Plugin.Api.DTO;
using System;
using System.Collections.Generic;

namespace Nop.Plugin.Api.DTOs.Address
{
    public class StateListRootObjectResponse 
    {
        public StateListRootObjectResponse()
        {
            States = new List<StateListResponse>();
        }

        [JsonProperty("States")]
        public IList<StateListResponse> States { get; set; }

        public string GetPrimaryPropertyName()
        {
            return "states";
        }

        public Type GetPrimaryPropertyType()
        {
            return typeof(StateListResponse);
        }
    }
}
