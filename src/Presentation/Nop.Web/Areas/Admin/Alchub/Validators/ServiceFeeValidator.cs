using FluentValidation;
using Nop.Core.Alchub.Domain.ServiceFee;
using Nop.Services.Localization;
using Nop.Web.Areas.Admin.Models.Settings;
using Nop.Web.Framework.Validators;

namespace Nop.Web.Areas.Admin.Alchub.Validators
{
    /// <summary>
    /// Represents a service fee validator
    /// </summary>
    public partial class ServiceFeeValidator : BaseNopValidator<ServiceFeeSettingsModel>
    {
        public ServiceFeeValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.ServiceFee)
                .GreaterThanOrEqualTo(0)
                .WithMessageAwait(localizationService.GetResourceAsync("Admin.Setting.ServiceFee.Fields.ServiceFee.ShouldBeGreaterThanZero"))
                .When(x => x.ServiceFeeTypeId == (int)ServiceFeeType.Flat);

            RuleFor(x => x.ServiceFeePercentage)
                .GreaterThanOrEqualTo(0)
                .WithMessageAwait(localizationService.GetResourceAsync("Admin.Setting.ServiceFee.Fields.ServiceFeePercentage.ShouldBeGreaterThanZero"))
                .When(x => x.ServiceFeeTypeId == (int)ServiceFeeType.Percentage);

            RuleFor(x => x.MaximumServiceFee)
            .GreaterThanOrEqualTo(0)
            .WithMessageAwait(localizationService.GetResourceAsync("Admin.Setting.ServiceFee.Fields.MaximumServiceFee.ShouldBeGreaterThanZero"))
            .When(x => x.ServiceFeeTypeId == (int)ServiceFeeType.Percentage);
        }
    }
}
