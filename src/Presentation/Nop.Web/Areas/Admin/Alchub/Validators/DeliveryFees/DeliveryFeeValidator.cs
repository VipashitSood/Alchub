using FluentValidation;
using Nop.Core.Domain.DeliveryFees;
using Nop.Services.Localization;
using Nop.Web.Areas.Admin.Models.DeliveryFees;
using Nop.Web.Framework.Validators;

namespace Nop.Web.Areas.Admin.Alchub.Validators
{
    /// <summary>
    /// Represents a Delivery Fee validator
    /// </summary>
    public partial class DeliveryFeeValidator : BaseNopValidator<DeliveryFeeModel>
    {
        public DeliveryFeeValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.VendorId)
                .GreaterThan(0)
                .WithMessageAwait(localizationService.GetResourceAsync("Alchub.Admin.Validator.DeliveryFee.Fields.VendorId.Required"));

            RuleFor(x => x.FixedFee)
                .GreaterThanOrEqualTo(0)
                .WithMessageAwait(localizationService.GetResourceAsync("Alchub.Admin.Validator.DeliveryFee.Fields.FixedFee.ShouldBeGreaterThanOrEqualToZero"))
                .When(x => x.DeliveryFeeTypeId == (int)DeliveryFeeType.Fixed);

            RuleFor(x => x.DynamicBaseFee)
            .GreaterThanOrEqualTo(0)
            .WithMessageAwait(localizationService.GetResourceAsync("Alchub.Admin.Validator.DeliveryFee.Fields.DynamicBaseFee.ShouldBeGreaterThanOrEqualToZero"))
            .When(x => x.DeliveryFeeTypeId == (int)DeliveryFeeType.Dynamic);

            RuleFor(x => x.DynamicBaseDistance)
            .GreaterThanOrEqualTo(0)
            .WithMessageAwait(localizationService.GetResourceAsync("Alchub.Admin.Validator.DeliveryFee.Fields.DynamicBaseDistance.ShouldBeGreaterThanOrEqualToZero"))
            .When(x => x.DeliveryFeeTypeId == (int)DeliveryFeeType.Dynamic);

            RuleFor(x => x.DynamicExtraFee)
            .GreaterThanOrEqualTo(0)
            .WithMessageAwait(localizationService.GetResourceAsync("Alchub.Admin.Validator.DeliveryFee.Fields.DynamicExtraFee.ShouldBeGreaterThanOrEqualToZero"))
            .When(x => x.DeliveryFeeTypeId == (int)DeliveryFeeType.Dynamic);

            RuleFor(x => x.DynamicMaximumFee)
            .GreaterThanOrEqualTo(0)
            .WithMessageAwait(localizationService.GetResourceAsync("Alchub.Admin.Validator.DeliveryFee.Fields.DynamicMaximumFee.ShouldBeGreaterThanOrEqualToZero"))
            .When(x => x.DeliveryFeeTypeId == (int)DeliveryFeeType.Dynamic);
        }
    }
}