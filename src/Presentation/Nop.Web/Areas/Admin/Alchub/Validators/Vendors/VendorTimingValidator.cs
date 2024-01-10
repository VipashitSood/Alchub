using System;
using FluentValidation;
using Nop.Data.Mapping;
using Nop.Services.Localization;
using Nop.Web.Areas.Admin.Models.Vendors;
using Nop.Web.Framework.Validators;
namespace Nop.Web.Areas.Admin.Alchub.Validators.Vendors
{
    public partial class VendorTimingValidator : BaseNopValidator<VendorTimingModel>
    {
        public VendorTimingValidator(ILocalizationService localizationService, IMappingEntityAccessor mappingEntityAccessor)
        {
            RuleFor(x => x.VendorId)
             .GreaterThan(0)
             .WithMessageAwait(localizationService.GetResourceAsync("Alchub.Admin.VendorsTiming.Fields.VendorId.GreaterThanZero"));

            RuleFor(x => x.DayId)
             .GreaterThanOrEqualTo(0)
             .WithMessageAwait(localizationService.GetResourceAsync("Alchub.Admin.VendorsTiming.Fields.DayId.GreaterThanOrEqualToZero"));

            RuleFor(x => x.OpenTimeStr)
            .NotEmpty()
            .WithMessageAwait(localizationService.GetResourceAsync("Alchub.Admin.VendorsTiming.Fields.OpenTimeStr.NotEmpty"))
            .When(x => !x.DayOff);

            RuleFor(x => x.CloseTimeStr)
            .NotEmpty()
            .WithMessageAwait(localizationService.GetResourceAsync("Alchub.Admin.VendorsTiming.Fields.CloseTimeStr.NotEmpty"))
            .When(x => !x.DayOff);

            RuleFor(x => x.OpenTimeStr).Must((x, context) =>
            {
                if (!string.IsNullOrEmpty(x.OpenTimeStr) && !string.IsNullOrEmpty(x.CloseTimeStr))
                {
                    //convert time from stirng
                    DateTime.TryParse(x.OpenTimeStr, out var openTime);
                    DateTime.TryParse(x.CloseTimeStr, out var closeTime);

                    return openTime.TimeOfDay < closeTime.TimeOfDay;
                }

                return true;
            }).WithMessageAwait(localizationService.GetResourceAsync("Alchub.Admin.VendorsTiming.Fields.OpenTimeUtc.Must.GreaterThanCloseTime"))
            .When(x => !x.DayOff);

            RuleFor(x => x.CloseTimeStr).Must((x, context) =>
            {
                if (!string.IsNullOrEmpty(x.OpenTimeStr) && !string.IsNullOrEmpty(x.CloseTimeStr))
                {
                    //convert time from stirng
                    DateTime.TryParse(x.OpenTimeStr, out var openTime);
                    DateTime.TryParse(x.CloseTimeStr, out var closeTime);

                    return closeTime.TimeOfDay > openTime.TimeOfDay;
                }

                return true;
            }).WithMessageAwait(localizationService.GetResourceAsync("Alchub.Admin.VendorsTiming.Fields.CloseTimeUtc.Must.LessThanOpenTime"))
            .When(x => !x.DayOff);
        }
    }
}
