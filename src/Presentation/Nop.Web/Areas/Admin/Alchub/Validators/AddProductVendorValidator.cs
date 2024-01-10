using System.Linq;
using FluentValidation;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;
using Nop.Web.Models.Common;
using static Nop.Web.Areas.Admin.Models.Catalog.ProductSearchModel;

namespace Nop.Web.Validators.Common
{
    public partial class AddProductVendorValidator : BaseNopValidator<AddProductVendor>
    {
        public AddProductVendorValidator(ILocalizationService localizationService,
            IStateProvinceService stateProvinceService,
            AddressSettings addressSettings,
            CustomerSettings customerSettings)
        {
            
            RuleFor(x => x.Price)
                .NotEmpty()
                .WithMessageAwait(localizationService.GetResourceAsync("Alchub.Fields.Price.Required"));
            RuleFor(x => x.Stock)
                .NotEmpty()
                .WithMessageAwait(localizationService.GetResourceAsync("Alchub.Fields.Stock.Required"));
            #region Alchub cutom

          
            #endregion
        }
    }
}