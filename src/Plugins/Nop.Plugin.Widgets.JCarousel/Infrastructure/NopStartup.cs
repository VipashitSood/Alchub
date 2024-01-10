using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using Nop.Plugin.Widgets.JCarousel.Services;

namespace Nop.Plugin.Widgets.JCarousel.Infrastructure
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
            //register services
            services.AddScoped<Factories.IJCarouselModelFactory, Factories.JCarouselModelFactory>();
            services.AddScoped<Factories.IPublicJCarouselModelFactory, Factories.PublicJCarouselModelFactory>();
            services.AddScoped<IJCarouselService, JCarouselService>();

            //register themes support
            services.Configure<RazorViewEngineOptions>(options =>
            {
                options.ViewLocationExpanders.Add(new ThemeableViewEngine());
            });
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
        public int Order => -1;
    }
}
