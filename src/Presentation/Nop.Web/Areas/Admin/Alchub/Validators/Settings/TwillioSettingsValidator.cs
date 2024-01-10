using System.Text.RegularExpressions;
using FluentValidation;
using Nop.Services.Localization;
using Nop.Web.Areas.Admin.Alchub.Models.Settings;
using Nop.Web.Framework.Validators;

namespace Nop.Web.Areas.Admin.Alchub.Validators.Settings
{
    public partial class TwillioSettingsValidator : BaseNopValidator<TwillioSettingsModel>
    {
        public TwillioSettingsValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.AccountSid).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Alchub.Admin.Configuration.Settings.TwillioSettings.Fields.AccountSid.Required"))
                .When(x => x.Enabled);

            RuleFor(x => x.AuthToken).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Alchub.Admin.Configuration.Settings.TwillioSettings.Fields.AuthToken.Required"))
                .When(x => x.Enabled);

            //phone
            RuleFor(x => x.FromNumber).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Alchub.Admin.Configuration.Settings.TwillioSettings.Fields.FromNumber.Required"))
                .When(x => x.Enabled);
            RuleFor(x => x.FromNumber)
                .Must(x =>
                {
                    if (string.IsNullOrEmpty(x))
                        return false;

                    var phoneRegex = new Regex(@"^\+[1-9]\d{10,14}$", RegexOptions.IgnoreCase);
                    return phoneRegex.IsMatch(x);
                })
                .WithMessageAwait(localizationService.GetResourceAsync("Alchub.Admin.Configuration.Settings.TwillioSettings.Fields.FromNumber.Invalid"))
                .When(x => x.Enabled);

            //default country code
            RuleFor(x => x.DefaultCountryCode).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Alchub.Admin.Configuration.Settings.TwillioSettings.Fields.DefaultCountryCode.Required"))
                .When(x => x.Enabled);
            RuleFor(x => x.DefaultCountryCode)
                .Must(x =>
                {
                    if (string.IsNullOrEmpty(x))
                        return false;

                    var isdCodeRegex = new Regex(@"^\+[1-9]\d{0,2}$", RegexOptions.IgnoreCase);
                    return isdCodeRegex.IsMatch(x);
                })
                .WithMessageAwait(localizationService.GetResourceAsync("Alchub.Admin.Configuration.Settings.TwillioSettings.Fields.DefaultCountryCode.Invalid"))
                .When(x => x.Enabled);

            //sms body templates
            RuleFor(x => x.OrderPlacedBody).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Alchub.Admin.Configuration.Settings.TwillioSettings.Fields.OrderPlacedBody.Required"))
                .When(x => x.Enabled);
            RuleFor(x => x.OrderItemsDispatchedBody).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Alchub.Admin.Configuration.Settings.TwillioSettings.Fields.OrderItemsDispatchedBody.Required"))
                .When(x => x.Enabled);
            RuleFor(x => x.OrderItemsPickedUpBody).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Alchub.Admin.Configuration.Settings.TwillioSettings.Fields.OrderItemsPickedUpBody.Required"))
                .When(x => x.Enabled);
            RuleFor(x => x.OrderItemsDeliveredBody).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Alchub.Admin.Configuration.Settings.TwillioSettings.Fields.OrderItemsDeliveredBody.Required"))
                .When(x => x.Enabled);
            RuleFor(x => x.OrderPlacedVendorBody).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Alchub.Admin.Configuration.Settings.TwillioSettings.Fields.OrderPlacedVendorBody.Required"))
                .When(x => x.Enabled);
        }
    }
}
