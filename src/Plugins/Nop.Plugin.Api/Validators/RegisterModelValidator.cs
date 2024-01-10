using System;
using FluentValidation;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Plugin.Api.Models.Customer;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;

namespace Nop.Plugin.Api.Validators
{
    public class RegisterModelValidator : BaseNopValidator<RegisterModel>
    {
        public RegisterModelValidator(ILocalizationService localizationService,
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

            //Password rule
            RuleFor(x => x.Password).IsPassword(localizationService, customerSettings);

            if (customerSettings.DateOfBirthEnabled && customerSettings.DateOfBirthRequired)
            {
                //entered?
                RuleFor(x => x.DateOfBirth).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Account.Fields.DateOfBirth.Required"));

                //minimum age
                RuleFor(x => x.DateOfBirth).Must((x, context) =>
                {
                    var dateOfBirth = Convert.ToDateTime(x.DateOfBirth);
                    if (customerSettings.DateOfBirthMinimumAge.HasValue &&
                        CommonHelper.GetDifferenceInYears(dateOfBirth, DateTime.Today) <
                        customerSettings.DateOfBirthMinimumAge.Value)
                        return false;

                    return true;
                }).WithMessageAwait(localizationService.GetResourceAsync("Account.Fields.DateOfBirth.MinimumAge"), customerSettings.DateOfBirthMinimumAge);
            }
            if (customerSettings.PhoneEnabled)
            {
                RuleFor(x => x.Phone).IsPhoneNumber(customerSettings).WithMessageAwait(localizationService.GetResourceAsync("Account.Fields.Phone.NotValid"));
            }
        }
    }
}
