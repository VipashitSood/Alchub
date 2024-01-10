using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Mvc.Routing;

namespace Nop.Plugin.Forefront.Xero.Infrastructure
{
    public class RouteProvider : IRouteProvider
	{
		public int Priority
		{
			get
			{
				return 100;
			}
		}

		public RouteProvider()
		{
		}

		public void RegisterRoutes(IEndpointRouteBuilder routes)
		{
			//MapRouteRouteBuilderExtensions.MapRoute((IRouteBuilder)routes, "Plugin.Forefront.Xero.Configure", "Admin/ForefrontXero/Configure", new { controller = "ForefrontXero", action = "Configure" });
		}
	}
}