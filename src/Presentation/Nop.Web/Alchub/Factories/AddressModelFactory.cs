using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Services.Common;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Web.Models.Common;

namespace Nop.Web.Factories
{
    /// <summary>
    /// Represents the address model factory
    /// </summary>
    public partial class AddressModelFactory : IAddressModelFactory
    {
        #region Methods

        /// <summary>
        /// Prepare address model
        /// </summary>
        /// <param name="model">Address model</param>
        /// <param name="address">Address entity</param>
        /// <param name="excludeProperties">Whether to exclude populating of model properties from the entity</param>
        /// <param name="addressSettings">Address settings</param>
        /// <param name="loadCountries">Countries loading function; pass null if countries do not need to load</param>
        /// <param name="prePopulateWithCustomerFields">Whether to populate model properties with the customer fields (used with the customer entity)</param>
        /// <param name="customer">Customer entity; required if prePopulateWithCustomerFields is true</param>
        /// <param name="overrideAttributesXml">Overridden address attributes in XML format; pass null to use CustomAttributes of the address entity</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task PrepareAddressModelAsync(AddressModel model,
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
                model.CountryName = await _countryService.GetCountryByAddressAsync(address) is Country country ? await _localizationService.GetLocalizedAsync(country, x => x.Name) : null;
                model.StateProvinceId = address.StateProvinceId;
                model.StateProvinceName = await _stateProvinceService.GetStateProvinceByAddressAsync(address) is StateProvince stateProvince ? await _localizationService.GetLocalizedAsync(stateProvince, x => x.Name) : null;
                model.County = address.County;
                model.City = address.City;
                model.Address1 = address.Address1;
                model.Address2 = address.Address2;
                model.ZipPostalCode = address.ZipPostalCode;
                model.PhoneNumber = address.PhoneNumber;
                model.FaxNumber = address.FaxNumber;

                /*Alchub Start*/
                model.GeoLocation = address.GeoLocation;
                model.GeoLocationCoordinates = address.GeoLocationCoordinates;
                model.AddressTypeId = address.AddressTypeId;
                model.AddressType = await _localizationService.GetLocalizedEnumAsync(address.AddressType);
                /*Alchub End*/
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
                model.County = await _genericAttributeService.GetAttributeAsync<string>(customer, NopCustomerDefaults.CountyAttribute);
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
                else
                {
                    model.AvailableCountries.Add(new SelectListItem { Text = await _localizationService.GetResourceAsync("Address.SelectCountry"), Value = "0" });
                }

                foreach (var c in countries)
                {
                    model.AvailableCountries.Add(new SelectListItem
                    {
                        Text = await _localizationService.GetLocalizedAsync(c, x => x.Name),
                        Value = c.Id.ToString(),
                        Selected = c.Id == model.CountryId
                    });
                }

                if (addressSettings.StateProvinceEnabled)
                {
                    var languageId = (await _workContext.GetWorkingLanguageAsync()).Id;
                    var states = (await _stateProvinceService
                        .GetStateProvincesByCountryIdAsync(model.CountryId ?? 0, languageId))
                        .ToList();
                    if (states.Any())
                    {
                        model.AvailableStates.Add(new SelectListItem { Text = await _localizationService.GetResourceAsync("Address.SelectState"), Value = "0" });

                        foreach (var s in states)
                        {
                            model.AvailableStates.Add(new SelectListItem
                            {
                                Text = await _localizationService.GetLocalizedAsync(s, x => x.Name),
                                Value = s.Id.ToString(),
                                Selected = (s.Id == model.StateProvinceId)
                            });
                        }
                    }
                    else
                    {
                        var anyCountrySelected = model.AvailableCountries.Any(x => x.Selected);
                        model.AvailableStates.Add(new SelectListItem
                        {
                            Text = await _localizationService.GetResourceAsync(anyCountrySelected ? "Address.Other" : "Address.SelectState"),
                            Value = "0"
                        });
                    }
                }
            }

            //form fields
            model.CompanyEnabled = addressSettings.CompanyEnabled;
            model.CompanyRequired = addressSettings.CompanyRequired;
            model.StreetAddressEnabled = addressSettings.StreetAddressEnabled;
            model.StreetAddressRequired = addressSettings.StreetAddressRequired;
            model.StreetAddress2Enabled = addressSettings.StreetAddress2Enabled;
            model.StreetAddress2Required = addressSettings.StreetAddress2Required;
            model.ZipPostalCodeEnabled = addressSettings.ZipPostalCodeEnabled;
            model.ZipPostalCodeRequired = addressSettings.ZipPostalCodeRequired;
            model.CityEnabled = addressSettings.CityEnabled;
            model.CityRequired = addressSettings.CityRequired;
            model.CountyEnabled = addressSettings.CountyEnabled;
            model.CountyRequired = addressSettings.CountyRequired;
            model.CountryEnabled = addressSettings.CountryEnabled;
            model.StateProvinceEnabled = addressSettings.StateProvinceEnabled;
            model.PhoneEnabled = addressSettings.PhoneEnabled;
            model.PhoneRequired = addressSettings.PhoneRequired;
            model.FaxEnabled = addressSettings.FaxEnabled;
            model.FaxRequired = addressSettings.FaxRequired;

            //customer attribute services
            if (_addressAttributeService != null && _addressAttributeParser != null)
            {
                await PrepareCustomAddressAttributesAsync(model, address, overrideAttributesXml);
            }
            if (_addressAttributeFormatter != null && address != null)
            {
                model.FormattedCustomAddressAttributes = await _addressAttributeFormatter.FormatAttributesAsync(address.CustomAttributes);
            }
        }

        #endregion
    }
}