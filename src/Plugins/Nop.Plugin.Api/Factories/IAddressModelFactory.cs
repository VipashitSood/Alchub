using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Plugin.Api.DTO;
using Nop.Plugin.Api.DTOs.Address;

namespace Nop.Plugin.Api.Factories
{
    /// <summary>
    /// Represents the interface of the address model factory
    /// </summary>
    public partial interface IAddressModelFactory
    {
        Task PrepareAddressModelAsync(AddressDto address1, Address address, bool excludeProperties,
            AddressSettings addressSettings,
            Func<Task<IList<Country>>> loadCountries = null,
            bool prePopulateWithCustomerFields = false,
            Customer customer = null,
            string overrideAttributesXml = "");
    }
}
