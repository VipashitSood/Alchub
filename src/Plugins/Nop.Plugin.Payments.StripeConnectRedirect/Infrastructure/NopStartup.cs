using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using Nop.Plugin.Payments.StripeConnectRedirect.Services;
using Nop.Services.Orders;
using Nop.Web.Areas.Admin.Factories;

namespace Nop.Plugin.Payments.StripeConnectRedirect.Infrastructure
{
    public class NopStartup : INopStartup
    {
        /// <summary>
        /// Add and configure any of the middleware
        /// </summary>
        /// <param name="services">Collection of service descriptors</param>
        /// <param name="configuration">Configuration of the application</param>
        public void ConfigureServices(IServiceCollection services, Microsoft.Extensions.Configuration.IConfiguration configuration)
        {
            //Services
            services.AddScoped<IOrderProcessingService, OverrideOrderProcessingService>();
            services.AddScoped<IStripeConnectRedirectService, StripeConnectRedirectService>();
            services.AddScoped<IStripeWorkflowMessageService, StripeWorkflowMessageService>();

            //Factories
            services.AddScoped<Factories.IVendorModelFactory, Factories.VendorModelFactory>();
            services.AddScoped<IOrderModelFactory, Factories.OverridenOrderModelFactory>(); 
        }

        /// <summary>
        /// Configure the using of added middleware
        /// </summary>
        /// <param name="application">Builder for configuring an application's request pipeline</param>
        public void Configure(IApplicationBuilder application)
        {
        }

        /// <summary>
        /// Gets order of this startup configuration implementation
        /// </summary>
        public int Order => int.MaxValue;
    }
}