using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core.Domain.Directory;
using Nop.Plugin.Api.AutoMapper;
using Nop.Plugin.Api.DTO;

namespace Nop.Plugin.Api.MappingExtensions
{
	public static class CountryDtoMappings
	{
        public static CountryListResponse ToDto(this Country address)
        {
            return address.MapTo<Country, CountryListResponse>();
        }

        public static Country ToEntity(this CountryListResponse addressDto)
        {
            return addressDto.MapTo<CountryListResponse, Country>();
        }
    }
}
