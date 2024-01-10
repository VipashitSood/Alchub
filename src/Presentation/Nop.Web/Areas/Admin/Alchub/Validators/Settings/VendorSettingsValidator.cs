using FluentValidation;
using Nop.Services.Localization;
using Nop.Web.Areas.Admin.Models.Settings;
using Nop.Web.Framework.Validators;

namespace Nop.Web.Areas.Admin.Alchub.Validators.Settings
{
    public partial class VendorSettingsValidator : BaseNopValidator<VendorSettingsModel>
    {
        public VendorSettingsValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.DistanceRadiusValue)
                .GreaterThanOrEqualTo(0)
                .WithMessageAwait(localizationService.GetResourceAsync("Alchub.Admin.Configuration.Settings.Vendor.DistanceRadiusValue.GreaterThanOrEqualZero"));
        }
    }
}
