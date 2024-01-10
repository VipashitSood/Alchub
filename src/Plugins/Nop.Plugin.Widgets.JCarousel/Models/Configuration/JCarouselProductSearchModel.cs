using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Widgets.JCarousel.Models.Configuration
{
    public partial record JCarouselProductSearchModel : BaseSearchModel
    {
        #region Properties
        [NopResourceDisplayName("Plugins.Widgets.JCarousel.JCarouselId")]
        public int JCarouselId { get; set; }

        #endregion
    }
}
