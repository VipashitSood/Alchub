using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Nop.Plugin.Api.DTO;

namespace Nop.Plugin.Api.DTOs.Vendors
{
    public class VendorsRootObjectDto : ISerializableObject
    {
        public VendorsRootObjectDto()
        {
            Vendors = new List<VendorDto>();
        }

        [JsonProperty("vendors")]
        public IList<VendorDto> Vendors { get; set; }

        public string GetPrimaryPropertyName()
        {
            return "vendors";
        }

        public Type GetPrimaryPropertyType()
        {
            return typeof(VendorDto);
        }
    }
}
