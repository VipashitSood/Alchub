using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Core.Caching;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Data;
using Nop.Plugin.Api.DTO;
using Nop.Plugin.Api.MappingExtensions;
using Nop.Services.Customers;
using Nop.Services.Directory;

namespace Nop.Plugin.Api.Services
{
	public class AddressApiService : IAddressApiService
	{
        private readonly IStaticCacheManager _cacheManager;
		private readonly ICountryService _countryService;
		private readonly IRepository<Address> _addressRepository;
        private readonly IRepository<CustomerAddressMapping> _customerAddressMappingRepository;

        public AddressApiService(
            IRepository<Address> addressRepository,
            IRepository<CustomerAddressMapping> customerAddressMappingRepository,
            IStaticCacheManager staticCacheManager,
            ICountryService countryService)
		{
            _addressRepository = addressRepository;
            _customerAddressMappingRepository = customerAddressMappingRepository;
            _cacheManager = staticCacheManager;
			_countryService = countryService;
		}

        /// <summary>
        /// Gets a list of addresses mapped to customer
        /// </summary>
        /// <param name="customerId">Customer identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the result
        /// </returns>
        public async Task<IList<AddressDto>> GetAddressesByCustomerIdAsync(int customerId)
        {
            var query = from address in _addressRepository.Table
                        join cam in _customerAddressMappingRepository.Table on address.Id equals cam.AddressId
                        where cam.CustomerId == customerId
                        orderby address.Id descending
                        select address;

            var key = _cacheManager.PrepareKeyForShortTermCache(NopCustomerServicesDefaults.CustomerAddressesCacheKey, customerId);

            var addresses = await _cacheManager.GetAsync(key, async () => await query.ToListAsync());
            return addresses.Select(a => a.ToDto()).ToList();
        }

		/// <summary>
		/// Gets a address mapped to customer
		/// </summary>
		/// <param name="customerId">Customer identifier</param>
		/// <param name="addressId">Address identifier</param>
		/// <returns>
		/// A task that represents the asynchronous operation
		/// The task result contains the result
		/// </returns>
		public async Task<AddressDto> GetCustomerAddressAsync(int customerId, int addressId)
        {
            var query = from address in _addressRepository.Table
                        join cam in _customerAddressMappingRepository.Table on address.Id equals cam.AddressId
                        where cam.CustomerId == customerId && address.Id == addressId
                        select address;

            var key = _cacheManager.PrepareKeyForShortTermCache(NopCustomerServicesDefaults.CustomerAddressCacheKey, customerId, addressId);

            var addressEntity = await _cacheManager.GetAsync(key, async () => await query.FirstOrDefaultAsync());
            return addressEntity?.ToDto();
        }

		public async Task<IList<CountryListResponse>> GetAllCountriesAsync(bool mustAllowBilling = false, bool mustAllowShipping = false)
		{
            IEnumerable<Country> countries = await _countryService.GetAllCountriesAsync();
            if (mustAllowBilling)
                countries = countries.Where(c => c.AllowsBilling);
            if (mustAllowShipping)
                countries = countries.Where(c => c.AllowsShipping);
            return countries.Select(c => c.ToDto()).ToList();
        }

		public async Task<AddressDto> GetAddressByIdAsync(int addressId)
		{
            var query = from address in _addressRepository.Table
                        where address.Id == addressId
                        select address;
            var addressEntity = await query.FirstOrDefaultAsync();
            return addressEntity?.ToDto();
        }

        public Address FindAddress(List<Address> addresses, string firstName, string lastName, string phoneNumber, string email, string address1, string city, int? stateProvinceId, string zipPostalCode, int? countryId)
        {
            return addresses.Find(a => ((string.IsNullOrEmpty(a.FirstName) && string.IsNullOrEmpty(firstName)) || a.FirstName == firstName) &&
          ((string.IsNullOrEmpty(a.LastName) && string.IsNullOrEmpty(lastName)) || a.LastName == lastName) &&
          ((string.IsNullOrEmpty(a.PhoneNumber) && string.IsNullOrEmpty(phoneNumber)) || a.PhoneNumber == phoneNumber) &&
          ((string.IsNullOrEmpty(a.Email) && string.IsNullOrEmpty(email)) || a.Email == email) &&
          ((string.IsNullOrEmpty(a.Address1) && string.IsNullOrEmpty(address1)) || a.Address1 == address1) &&
          ((string.IsNullOrEmpty(a.City) && string.IsNullOrEmpty(city)) || a.City == city) &&
          ((a.StateProvinceId == null && (stateProvinceId == null || stateProvinceId == 0)) || (a.StateProvinceId != null && a.StateProvinceId == stateProvinceId)) &&
          ((string.IsNullOrEmpty(a.ZipPostalCode) && string.IsNullOrEmpty(zipPostalCode)) || a.ZipPostalCode == zipPostalCode) &&
          ((a.CountryId == null && countryId == null) || (a.CountryId != null && a.CountryId == countryId)));
        
        }
     
        /// <summary>
        /// Inserts an address
        /// </summary>
        /// <param name="address">Address</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task InsertAddressAsync(Address address)
        {
            if (address == null)
                throw new ArgumentNullException(nameof(address));

            address.CreatedOnUtc = DateTime.UtcNow;

            //some validation
            if (address.CountryId == 0)
                address.CountryId = null;
            if (address.StateProvinceId == 0)
                address.StateProvinceId = null;

            await _addressRepository.InsertAsync(address);
        }

        /// <summary>
        /// Updates the address
        /// </summary>
        /// <param name="address">Address</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task UpdateAddressAsync(Address address)
        {
            if (address == null)
                throw new ArgumentNullException(nameof(address));

            //some validation
            if (address.CountryId == 0)
                address.CountryId = null;
            if (address.StateProvinceId == 0)
                address.StateProvinceId = null;

            await _addressRepository.UpdateAsync(address);
        }

    }
}
