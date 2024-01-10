using Microsoft.AspNetCore.Mvc;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Payments.StripeConnectRedirect.Components
{
    [ViewComponent(Name = StripeConnectRedirectPaymentDefaults.PaymentInfoViewComponentName)]
    public class PaymentInfoViewComponent : NopViewComponent
    {
        /// <summary>
        /// Invoke the widget view component
        /// </summary>
        /// <returns></returns>
        public IViewComponentResult Invoke()
        {
            return View("~/Plugins/Payments.StripeConnectRedirect/Views/PaymentInfo.cshtml");
        }
    }
}