using Newtonsoft.Json;
using Nop.Plugin.Api.DTO;
using Nop.Plugin.Api.DTOs.Address;
using System;
using System.Collections.Generic;

namespace Nop.Plugin.Api.Models.Address
{
    public class CustomerAddressListModel : ISerializableObject
    {
        public CustomerAddressListModel()
        {
            Addresses = new List<AddressDto>();
        }
        [JsonProperty("Address")]
        public IList<AddressDto> Addresses { get; set; }

        [JsonProperty("pageIndex")]
        public int PageIndex { get; set; }

        [JsonProperty("pageSize")]
        public int PageSize { get; set; }

        [JsonProperty("total_records")]
        public int TotalRecords { get; set; }
        public string GetPrimaryPropertyName()
        {
            return "Address";
        }

        public Type GetPrimaryPropertyType()
        {
            return typeof(AddressDto);
        }
    }
}
