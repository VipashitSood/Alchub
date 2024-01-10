using FluentValidation;
using Nop.Services.Localization;
using Nop.Web.Areas.Admin.Models.Slots;
using Nop.Web.Framework.Validators;

namespace Nop.Web.Areas.Admin.Alchub.Validators
{
    /// <summary>
    /// Represents a service fee validator
    /// </summary>
    public partial class ZoneValidator : BaseNopValidator<ZoneModel>
    {
        public ZoneValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.Name).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Admin.Slot.Management.Fields.Name.Required"));
        }
    }
}
