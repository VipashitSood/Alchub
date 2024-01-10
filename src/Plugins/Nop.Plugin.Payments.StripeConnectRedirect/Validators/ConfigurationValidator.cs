using FluentValidation;
using Nop.Plugin.Payments.StripeConnectRedirect.Models;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;

namespace Nop.Plugin.Payments.StripeConnectRedirect.Validators
{
    public partial class ConfigurationValidator : BaseNopValidator<ConfigurationModel>
    {
        public ConfigurationValidator(ILocalizationService localizationService)
        {
            RuleFor(model => model.ClientId)
                .NotEmpty()
                .WithMessageAwait(localizationService.GetResourceAsync("Plugin.Payments.StripeConnectRedirect.Configuration.Validator.ClientId.Required"));

            RuleFor(model => model.PublishableApiKey)
                .NotEmpty()
                .WithMessageAwait(localizationService.GetResourceAsync("Plugin.Payments.StripeConnectRedirect.Configuration.Validator.PublishableApiKey.Required"));

            RuleFor(model => model.SecretApiKey)
                .NotEmpty()
                .WithMessageAwait(localizationService.GetResourceAsync("Plugin.Payments.StripeConnectRedirect.Configuration.Validator.SecretApiKey.Required"));
        }
    }
}