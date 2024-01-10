using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Plugin.Api.DTO;
using Nop.Plugin.Api.Helpers;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;

namespace Nop.Plugin.Api.Validators
{
    //public class AddressDtoValidator : BaseDtoValidator<AddressDto>
    //{
    //    #region Constructors

    //    public AddressDtoValidator(IHttpContextAccessor httpContextAccessor, IJsonHelper jsonHelper, Dictionary<string, object> requestJsonDictionary) :
    //        base(httpContextAccessor, jsonHelper, requestJsonDictionary)
    //    {
    //        SetFirstNameRule();
    //        SetLastNameRule();
    //        SetEmailRule();
    //        SetUserIdRule();
    //        SetAddress1Rule();
    //        SetCityRule();
    //        SetZipPostalCodeRule();
    //        SetCountryIdRule();
    //        SetPhoneNumberRule();
    //    }

    //    #endregion

    //    #region Private Methods

    //    private void SetAddress1Rule()
    //    {
    //        SetNotNullOrEmptyCreateOrUpdateRule(a => a.Address1, "address1 required", "address1");
    //    }

    //    private void SetCityRule()
    //    {
    //        SetNotNullOrEmptyCreateOrUpdateRule(a => a.City, "city required", "city");
    //    }

    //    private void SetCountryIdRule()
    //    {
    //        SetGreaterThanZeroCreateOrUpdateRule(a => a.CountryId, "country_id required", "country_id");
    //    }

    //    private void SetEmailRule()
    //    {
    //        SetNotNullOrEmptyCreateOrUpdateRule(a => a.Email, "email required", "email");
    //    }
    //    private void SetUserIdRule()
    //    {
    //        SetNotNullOrEmptyCreateOrUpdateRule(a => a.UserId.ToString(), "userid required", "userid");
    //    }

    //    private void SetFirstNameRule()
    //    {
    //        SetNotNullOrEmptyCreateOrUpdateRule(a => a.FirstName, "first_name required", "first_name");
    //    }

    //    private void SetLastNameRule()
    //    {
    //        SetNotNullOrEmptyCreateOrUpdateRule(a => a.LastName, "last_name required", "last_name");
    //    }

    //    private void SetPhoneNumberRule()
    //    {
    //        SetNotNullOrEmptyCreateOrUpdateRule(a => a.PhoneNumber, "phone_number required", "phone_number");
    //    }

    //    private void SetZipPostalCodeRule()
    //    {
    //        SetNotNullOrEmptyCreateOrUpdateRule(a => a.ZipPostalCode, "zip_postal_code required", "zip_postal_code");
    //    }

    //    #endregion
    //}

    public class AddressDtoValidator : BaseNopValidator<AddressDto>
    {
        public AddressDtoValidator(ILocalizationService localizationService,
            IStateProvinceService stateProvinceService,
            AddressSettings addressSettings,
            CustomerSettings customerSettings)
        {
            RuleFor(x => x.FirstName)
                .NotEmpty()
                .WithMessageAwait(localizationService.GetResourceAsync("Address.Fields.FirstName.Required"));
            RuleFor(x => x.LastName)
                .NotEmpty()
                .WithMessageAwait(localizationService.GetResourceAsync("Address.Fields.LastName.Required"));
            RuleFor(x => x.Email)
                .NotEmpty()
                .WithMessageAwait(localizationService.GetResourceAsync("Address.Fields.Email.Required"));
            RuleFor(x => x.Email)
                .EmailAddress()
                .WithMessageAwait(localizationService.GetResourceAsync("Common.WrongEmail"));
            if (addressSettings.CountryEnabled)
            {
                RuleFor(x => x.CountryId)
                    .NotNull()
                    .WithMessageAwait(localizationService.GetResourceAsync("Address.Fields.Country.Required"));
                RuleFor(x => x.CountryId)
                    .NotEqual(0)
                    .WithMessageAwait(localizationService.GetResourceAsync("Address.Fields.Country.Required"));
            }
            if (addressSettings.CountryEnabled && addressSettings.StateProvinceEnabled)
            {
                RuleFor(x => x.StateProvinceId).MustAwait(async (x, context) =>
                {
                    //does selected country has states?
                    var countryId = x.CountryId ?? 0;
                    var hasStates = (await stateProvinceService.GetStateProvincesByCountryIdAsync(countryId)).Any();

                    if (hasStates)
                    {
                        //if yes, then ensure that state is selected
                        if (!x.StateProvinceId.HasValue || x.StateProvinceId.Value == 0)
                            return false;
                    }

                    return true;
                }).WithMessageAwait(localizationService.GetResourceAsync("Address.Fields.StateProvince.Required"));
            }
            if (addressSettings.CompanyRequired && addressSettings.CompanyEnabled)
            {
                RuleFor(x => x.Company).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Account.Fields.Company.Required"));
            }
            if (addressSettings.StreetAddressRequired && addressSettings.StreetAddressEnabled)
            {
                RuleFor(x => x.Address1).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Account.Fields.StreetAddress.Required"));
            }
            if (addressSettings.StreetAddress2Required && addressSettings.StreetAddress2Enabled)
            {
                RuleFor(x => x.Address2).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Account.Fields.StreetAddress2.Required"));
            }
            if (addressSettings.ZipPostalCodeRequired && addressSettings.ZipPostalCodeEnabled)
            {
                RuleFor(x => x.ZipPostalCode).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Account.Fields.ZipPostalCode.Required"));
            }
            if (addressSettings.CityRequired && addressSettings.CityEnabled)
            {
                RuleFor(x => x.City).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Account.Fields.City.Required"));
            }
            if (addressSettings.PhoneRequired && addressSettings.PhoneEnabled)
            {
                RuleFor(x => x.PhoneNumber).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Account.Fields.Phone.Required"));
            }
            if (addressSettings.PhoneEnabled)
            {
                RuleFor(x => x.PhoneNumber).IsPhoneNumber(customerSettings).WithMessageAwait(localizationService.GetResourceAsync("Account.Fields.Phone.NotValid"));
            }
            if (addressSettings.FaxRequired && addressSettings.FaxEnabled)
            {
                RuleFor(x => x.FaxNumber).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Account.Fields.Fax.Required"));
            }

            #region Alchub cutom

            //RuleFor(x => x.GeoLocation).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Alchub.Account.Fields.GeoLocation.Required"));
            RuleFor(x => x.GeoLant).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Alchub.Account.Fields.GeoLocationCoordinates.Required.SelectFromSuggetion"));
            RuleFor(x => x.GeoLong).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Alchub.Account.Fields.GeoLocationCoordinates.Required.SelectFromSuggetion"));

            RuleFor(x => x.AddressTypeId)
               .GreaterThan(0)
               .WithMessageAwait(localizationService.GetResourceAsync("Alchub.Account.Fields.AddressTypeId.GreaterThanZero"));

            #endregion
        }
    }
}
