using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc.Routing;

namespace Nop.Plugin.Payments.StripeConnectRedirect.Infrastructure
{
    public partial class RouteProvider : IRouteProvider
    {
        /// <summary>
        /// Register routes
        /// </summary>
        /// <param name="endpointRouteBuilder">Route builder</param>
        public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
        {
            //add route to the webhook handler
            endpointRouteBuilder.MapControllerRoute("Plugin.Payments.StripeConnectRedirect.Configure", "Admin/StripeConnectRedirect/Configure",
               new { controller = "StripeConnectRedirect", action = "Configure" });

            endpointRouteBuilder.MapControllerRoute("Plugin.Payments.StripeConnectRedirect.Webhook", StripeConnectRedirectPaymentDefaults.WebhookUrl,
              new { controller = "StripeConnectRedirect", action = "Webhook" });

            endpointRouteBuilder.MapControllerRoute(name: "Plugin.Payments.StripeConnectRedirect.StripeConnect",
                pattern: $"stripeconnect/{{vendorId:min(0)}}",
                defaults: new { controller = "StripeConnectRedirect", action = "StripeConnect", area = AreaNames.Admin });

            endpointRouteBuilder.MapControllerRoute(name: "Plugin.Payments.StripeConnectRedirect.StripeConnectDashboard",
                pattern: $"stripeconnectdashboard/{{vendorId:min(0)}}",
                defaults: new { controller = "StripeConnectRedirect", action = "StripeConnectDashboard", area = AreaNames.Admin });

            endpointRouteBuilder.MapControllerRoute("Plugin.Payments.StripeConnectRedirect.StripeConnectRefresh", StripeConnectRedirectPaymentDefaults.RefreshUrl + "{vendorId}",
                new { controller = "StripeConnectRedirect", action = "StripeConnectRefresh", area = AreaNames.Admin });

            endpointRouteBuilder.MapControllerRoute("Plugin.Payments.StripeConnectRedirect.StripeConnectSuccess", StripeConnectRedirectPaymentDefaults.ReturnUrl + "{vendorId}",
                new { controller = "StripeConnectRedirect", action = "StripeConnectSuccess", area = AreaNames.Admin });
        }

        /// <summary>
        /// Gets a priority of route provider
        /// </summary>
        public int Priority => int.MaxValue;
    }
}