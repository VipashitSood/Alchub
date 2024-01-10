using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Nop.Plugin.Api.DTO
{
	public class CountryListRootObjectResponse : ISerializableObject
    {
        public CountryListRootObjectResponse()
        {
            Countries = new List<CountryListResponse>();
        }

        [JsonProperty("countries")]
        public IList<CountryListResponse> Countries { get; set; }

        public string GetPrimaryPropertyName()
        {
            return "countries";
        }

        public Type GetPrimaryPropertyType()
        {
            return typeof(CountryListResponse);
        }
    }
}
