using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Alchub.Domain;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Plugin.Api.DTO;
using Nop.Plugin.Api.DTOs.Address;
using Nop.Services.Common;
using Nop.Services.Directory;
using Nop.Services.Localization;

namespace Nop.Plugin.Api.Factories
{
    /// <summary>
    /// Represents the address model factory
    /// </summary>
    public partial class AddressModelFactory : IAddressModelFactory
    {
        #region Fields

        private readonly AddressSettings _addressSettings;
        private readonly IAddressAttributeFormatter _addressAttributeFormatter;
        private readonly IAddressAttributeParser _addressAttributeParser;
        private readonly IAddressAttributeService _addressAttributeService;
        private readonly ICountryService _countryService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ILocalizationService _localizationService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly IWorkContext _workContext;

        #endregion

        #region Ctor

        public AddressModelFactory(AddressSettings addressSettings,
            IAddressAttributeFormatter addressAttributeFormatter,
            IAddressAttributeParser addressAttributeParser,
            IAddressAttributeService addressAttributeService,
            ICountryService countryService,
            IGenericAttributeService genericAttributeService,
            ILocalizationService localizationService,
            IStateProvinceService stateProvinceService,
            IWorkContext workContext)
        {
            _addressSettings = addressSettings;
            _addressAttributeFormatter = addressAttributeFormatter;
            _addressAttributeParser = addressAttributeParser;
            _addressAttributeService = addressAttributeService;
            _countryService = countryService;
            _genericAttributeService = genericAttributeService;
            _localizationService = localizationService;
            _stateProvinceService = stateProvinceService;
            _workContext = workContext;
        }

        #endregion

        #region Methods

        public async Task PrepareAddressModelAsync(AddressDto model,
            Address address, bool excludeProperties,
            AddressSettings addressSettings,
            Func<Task<IList<Country>>> loadCountries = null,
            bool prePopulateWithCustomerFields = false,
            Customer customer = null,
            string overrideAttributesXml = "")
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            if (addressSettings == null)
                throw new ArgumentNullException(nameof(addressSettings));

            if (!excludeProperties && address != null)
            {
                model.Id = address.Id;
                model.FirstName = address.FirstName;
                model.LastName = address.LastName;
                model.Email = address.Email;
                model.Company = address.Company;
                model.CountryId = address.CountryId;
                model.StateProvinceId = address.StateProvinceId;
                model.City = address.City;
                model.Address1 = address.Address1;
                model.Address2 = address.Address2;
                model.ZipPostalCode = address.ZipPostalCode;
                model.PhoneNumber = address.PhoneNumber;
                model.FaxNumber = address.FaxNumber;
                //++Alchub
                var pointArr = address.GeoLocationCoordinates?.Split(NopAlchubDefaults.LATLNG_SEPARATOR);
                if (pointArr != null && pointArr.Length == 2)
                {
                    model.GeoLant = pointArr[0];
                    model.GeoLong = pointArr[1];
                }
                model.AddressTypeId = address.AddressTypeId;
                model.AddressType = address.AddressType;
                //--Alchub
            }

            if (address == null && prePopulateWithCustomerFields)
            {
                if (customer == null)
                    throw new Exception("Customer cannot be null when prepopulating an address");
                model.Email = customer.Email;
                model.FirstName = await _genericAttributeService.GetAttributeAsync<string>(customer, NopCustomerDefaults.FirstNameAttribute);
                model.LastName = await _genericAttributeService.GetAttributeAsync<string>(customer, NopCustomerDefaults.LastNameAttribute);
                model.Company = await _genericAttributeService.GetAttributeAsync<string>(customer, NopCustomerDefaults.CompanyAttribute);
                model.Address1 = await _genericAttributeService.GetAttributeAsync<string>(customer, NopCustomerDefaults.StreetAddressAttribute);
                model.Address2 = await _genericAttributeService.GetAttributeAsync<string>(customer, NopCustomerDefaults.StreetAddress2Attribute);
                model.ZipPostalCode = await _genericAttributeService.GetAttributeAsync<string>(customer, NopCustomerDefaults.ZipPostalCodeAttribute);
                model.City = await _genericAttributeService.GetAttributeAsync<string>(customer, NopCustomerDefaults.CityAttribute);
                model.PhoneNumber = await _genericAttributeService.GetAttributeAsync<string>(customer, NopCustomerDefaults.PhoneAttribute);
                model.FaxNumber = await _genericAttributeService.GetAttributeAsync<string>(customer, NopCustomerDefaults.FaxAttribute);
            }
            //countries and states
            if (addressSettings.CountryEnabled && loadCountries != null)
            {
                var countries = await loadCountries();

                if (_addressSettings.PreselectCountryIfOnlyOne && countries.Count == 1)
                {
                    model.CountryId = countries[0].Id;
                }

                if (addressSettings.StateProvinceEnabled)
                {
                    var languageId = (await _workContext.GetWorkingLanguageAsync()).Id;
                    var states = (await _stateProvinceService
                        .GetStateProvincesByCountryIdAsync(model.CountryId ?? 0, languageId))
                        .ToList();
                }
            }
        }

        #endregion
    }
}