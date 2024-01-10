using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Core.Domain.Common;
using Nop.Plugin.Api.DTO;

namespace Nop.Plugin.Api.Services
{
	public interface IAddressApiService
	{
		Task<IList<AddressDto>> GetAddressesByCustomerIdAsync(int customerId);
		Task<AddressDto> GetCustomerAddressAsync(int customerId, int addressId);
		Task<IList<CountryListResponse>> GetAllCountriesAsync(bool mustAllowBilling = false, bool mustAllowShipping = false);
        //Task<IList<StateProvinceModel>> GetStatesByCountryIdAsync(string countryId, bool addSelectStateItem);
        Task<AddressDto> GetAddressByIdAsync(int addressId);
		Address FindAddress(List<Address> addresses, string firstName, string lastName, string phoneNumber, string email, string address1, string city, int? stateProvinceId, string zipPostalCode, int? countryId);

        /// <summary>
        /// Inserts an address
        /// </summary>
        /// <param name="address">Address</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task InsertAddressAsync(Address address);

        /// <summary>
        /// Updates the address
        /// </summary>
        /// <param name="address">Address</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task UpdateAddressAsync(Address address);
    }
}
