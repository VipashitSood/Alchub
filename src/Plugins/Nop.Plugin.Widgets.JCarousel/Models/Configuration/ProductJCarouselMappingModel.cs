using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Widgets.JCarousel.Models.Configuration
{
    public partial record ProductJCarouselMappingModel : BaseNopEntityModel
    {
        [NopResourceDisplayName("Plugins.Widgets.JCarousel.ProductId")]
        public int ProductId { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.JCarousel.JCarouselId")]
        public int JCarouselId { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.JCarousel.Products.Fields.Displayorder")]
        public int DisplayOrder { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.JCarousel.Products.Fields.Name")]
        public string ProductName { get; set; }

        [NopResourceDisplayName("Plugins.Widgets.JCarousel.Products.Fields.Isfeaturedproduct")]
        public bool IsFeaturedProduct { get; set; }
    }
}
