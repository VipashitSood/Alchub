using System;
using FluentValidation;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Plugin.Api.Models.Customer;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;

namespace Nop.Plugin.Api.Validators
{
    public class ProfileDataValidatorModel : BaseNopValidator<ProfileDataModel>
    {
        public ProfileDataValidatorModel(ILocalizationService localizationService,
            IStateProvinceService stateProvinceService,
            CustomerSettings customerSettings)
        {
            RuleFor(x => x.Email).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Account.Fields.Email.Required"));
            RuleFor(x => x.Email).EmailAddress().WithMessageAwait(localizationService.GetResourceAsync("Common.WrongEmail"));
            if (customerSettings.FirstNameEnabled && customerSettings.FirstNameRequired)
            {
                RuleFor(x => x.FirstName).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Account.Fields.FirstName.Required"));
            }
            if (customerSettings.LastNameEnabled && customerSettings.LastNameRequired)
            {
                RuleFor(x => x.LastName).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Account.Fields.LastName.Required"));
            }
            if (customerSettings.DateOfBirthEnabled && customerSettings.DateOfBirthRequired)
            {
                //entered?
                RuleFor(x => x.DateOfBirth).Must((x, context) =>
                {
                    //var dateOfBirth = x.ParseDateOfBirth();
                    if (!x.DateOfBirth.HasValue)
                        return false;

                    return true;
                }).WithMessageAwait(localizationService.GetResourceAsync("Account.Fields.DateOfBirth.Required"));

                //minimum age
                RuleFor(x => x.DateOfBirth).Must((x, context) =>
                {
                    //var dateOfBirth = x.ParseDateOfBirth();
                    if (x.DateOfBirth.HasValue && customerSettings.DateOfBirthMinimumAge.HasValue &&
                        CommonHelper.GetDifferenceInYears(x.DateOfBirth.Value, DateTime.Today) <
                        customerSettings.DateOfBirthMinimumAge.Value)
                        return false;

                    return true;
                }).WithMessageAwait(localizationService.GetResourceAsync("Account.ProfileData.Fields.DateOfBirth.MinimumAge"), customerSettings.DateOfBirthMinimumAge);
            }
            if (customerSettings.PhoneRequired && customerSettings.PhoneEnabled)
            {
                RuleFor(x => x.PhoneNumber).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Account.Fields.Phone.Required"));
            }
            if (customerSettings.PhoneEnabled)
            {
                RuleFor(x => x.PhoneNumber).IsPhoneNumber(customerSettings).WithMessageAwait(localizationService.GetResourceAsync("Account.Fields.Phone.NotValid"));
            }
        }
    }
}
