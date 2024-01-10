using System.Text.RegularExpressions;
using FluentValidation;
using Nop.Core.Alchub.Domain.Vendors;
using Nop.Core.Domain.Vendors;
using Nop.Data.Mapping;
using Nop.Services.Localization;
using Nop.Services.Seo;
using Nop.Web.Areas.Admin.Models.Vendors;
using Nop.Web.Framework.Validators;

namespace Nop.Web.Areas.Admin.Validators.Vendors
{
    public partial class VendorValidator : BaseNopValidator<VendorModel>
    {
        public VendorValidator(ILocalizationService localizationService, IMappingEntityAccessor mappingEntityAccessor)
        {
            RuleFor(x => x.Name).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Admin.Vendors.Fields.Name.Required"));

            RuleFor(x => x.Email).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Admin.Vendors.Fields.Email.Required"));
            RuleFor(x => x.Email).EmailAddress().WithMessageAwait(localizationService.GetResourceAsync("Admin.Common.WrongEmail"));
            RuleFor(x => x.PageSizeOptions).Must(ValidatorUtilities.PageSizeOptionsValidator).WithMessageAwait(localizationService.GetResourceAsync("Admin.Vendors.Fields.PageSizeOptions.ShouldHaveUniqueItems"));
            RuleFor(x => x.PageSize).Must((x, context) =>
            {
                if (!x.AllowCustomersToSelectPageSize && x.PageSize <= 0)
                    return false;

                return true;
            }).WithMessageAwait(localizationService.GetResourceAsync("Admin.Vendors.Fields.PageSize.Positive"));
            RuleFor(x => x.SeName).Length(0, NopSeoDefaults.SearchEngineNameLength)
                .WithMessageAwait(localizationService.GetResourceAsync("Admin.SEO.SeName.MaxLengthValidation"), NopSeoDefaults.SearchEngineNameLength);

            RuleFor(x => x.PriceFrom)
                .GreaterThanOrEqualTo(0)
                .WithMessageAwait(localizationService.GetResourceAsync("Admin.Vendors.Fields.PriceFrom.GreaterThanOrEqualZero"))
                .When(x => x.PriceRangeFiltering && x.ManuallyPriceRange);

            RuleFor(x => x.PriceTo)
                .GreaterThan(x => x.PriceFrom > decimal.Zero ? x.PriceFrom : decimal.Zero)
                .WithMessage(x => string.Format(localizationService.GetResourceAsync("Admin.Vendors.Fields.PriceTo.GreaterThanZeroOrPriceFrom").Result, x.PriceFrom > decimal.Zero ? x.PriceFrom : decimal.Zero))
                .When(x => x.PriceRangeFiltering && x.ManuallyPriceRange);

            /*Alchub Start*/
            RuleFor(x => x.MinimumOrderAmount)
                .GreaterThanOrEqualTo(0)
                .WithMessageAwait(localizationService.GetResourceAsync("Alchub.Admin.Vendors.Fields.MinimumOrderAmount.GreaterThanOrEqualZero"));

            RuleFor(x => x.OrderTax)
               .GreaterThanOrEqualTo(0)
               .WithMessageAwait(localizationService.GetResourceAsync("Alchub.Admin.Vendors.Fields.OrderTax.GreaterThanOrEqualZero"));

            RuleFor(x => x.PhoneNumber).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Alchub.Admin.Vendors.Fields.PhoneNumber.Required"));
            RuleFor(x => x.PhoneNumber)
                .Must(x =>
                {
                    if (string.IsNullOrEmpty(x))
                        return false;

                    var phoneRegex = new Regex(@"^\d{10,14}$", RegexOptions.IgnoreCase);
                    return phoneRegex.IsMatch(x);
                })
                .WithMessageAwait(localizationService.GetResourceAsync("Alchub.Admin.Vendors.Fields.PhoneNumber.NotValid"));

            RuleFor(x => x.PickupAddress)
                .NotEmpty()
                .WithMessageAwait(localizationService.GetResourceAsync("Admin.Vendors.Fields.PickupAddress.Required"))
                .When(x => x.PickAvailable);

            //Geofence CR: 19-05-23
            RuleFor(x => x.RadiusDistance)
                .GreaterThan(decimal.Zero)
                .WithMessageAwait(localizationService.GetResourceAsync("Alchub.Admin.Vendors.Fields.RadiusDistance.GreaterThanZero"))
                .When(x => x.GeoFenceShapeTypeId == (int)GeoFenceShapeType.Radius);

            /*Alchub End*/

            SetDatabaseValidationRules<Vendor>(mappingEntityAccessor);
        }
    }
}