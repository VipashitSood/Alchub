namespace Nop.Plugin.Widgets.JCarousel
{
    public class JCarouselDefaults
    {
        /// <summary>
        /// Gets the plugin system name
        /// </summary>
        public static string SystemName => "Widgets.JCarousel";

        /// <summary>
        /// Gets the configuration route name
        /// </summary>
        public static string ConfigurationRouteName => "Plugin.Widgets.JCarousel.Configuration";

        /// <summary>
        /// Gets the name of the view component to place a widget into pages
        /// </summary>
        public const string VIEW_COMPONENT = "JCarousel";

        /// <summary>
        /// Gets the name of the view component to place content refernece in head
        /// </summary>
        public const string HEAD_REFERENCE_VIEW_COMPONENT = "JCarouselHeadReference";
    }
}
