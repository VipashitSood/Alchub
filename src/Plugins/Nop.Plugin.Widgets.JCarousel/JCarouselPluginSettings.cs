using Nop.Core.Configuration;

namespace Nop.Plugin.Widgets.JCarousel
{
    public class JCarouselPluginSettings : ISettings
    {
        public bool Enable { get; set; }
        public int LazyLoadNumberOfProductInCarousel { get; set; }
        public bool EnableViewAll { get; set; }
    }
}
