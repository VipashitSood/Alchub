using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Customers;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Web.Controllers
{
    public partial class CustomerController : BasePublicController
    {

        public virtual async Task<IActionResult> LogoutImpersonatedVendor()
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            if (_workContext.OriginalCustomerIfImpersonated != null)
            {
                //activity log
                await _customerActivityService.InsertActivityAsync(_workContext.OriginalCustomerIfImpersonated, "Impersonation.Finished",
                    string.Format(await _localizationService.GetResourceAsync("ActivityLog.Impersonation.Finished.StoreOwner"),
                        customer.Email, customer.Id),
                    customer);

                await _customerActivityService.InsertActivityAsync("Impersonation.Finished",
                    string.Format(await _localizationService.GetResourceAsync("ActivityLog.Impersonation.Finished.Customer"),
                        _workContext.OriginalCustomerIfImpersonated.Email, _workContext.OriginalCustomerIfImpersonated.Id),
                    _workContext.OriginalCustomerIfImpersonated);

                //logout impersonated vendor
                await _genericAttributeService
                    .SaveAttributeAsync<int?>(_workContext.OriginalCustomerIfImpersonated, NopCustomerDefaults.ImpersonatedCustomerIdAttribute, null);

                //redirect back to vendor list page (admin area)
                return RedirectToAction("MultiVendors", "MultiVendor", new { area = AreaNames.Admin });
            }

            return RedirectToRoute("Homepage");
        }
    }
}
