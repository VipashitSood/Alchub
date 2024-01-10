using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Widgets.JCarousel.Models.Configuration
{
    public record ConfigurationModel : BaseNopModel
    {
        public int ActiveStoreScopeConfiguration { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.JCarousel.Enabled")]
        public bool Enabled { get; set; }
        public bool Enabled_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.JCarousel.LazyLoadNumberOfProductInCarousel")]
        public int LazyLoadNumberOfProductInCarousel { get; set; }
        public bool Enabled_LazyLoadNumberOfProductInCarousel { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.JCarousel.EnableViewAll")]
        public bool EnableViewAll { get; set; }
        public bool Enabled_EnableViewAll { get; set; }
    }
}
