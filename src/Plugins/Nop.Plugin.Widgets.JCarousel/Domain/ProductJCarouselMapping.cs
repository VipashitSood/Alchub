using Nop.Core;

namespace Nop.Plugin.Widgets.JCarousel.Domain
{
    public partial class ProductJCarouselMapping : BaseEntity
    {
        public int ProductId { get; set; }
        public int JCarouselId { get; set; }
        public int DisplayOrder { get; set; }
    }
}
