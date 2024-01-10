using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using System.Collections.Generic;

namespace Nop.Plugin.Widgets.JCarousel.Models.Configuration
{
    public partial record AddProductToJCarouselModel : BaseNopModel
    {
        #region Ctor

        public AddProductToJCarouselModel()
        {
            SelectedProductIds = new List<int>();
        }
        #endregion

        #region Properties

        [NopResourceDisplayName("Plugins.Widgets.JCarousel.JCarouselId")]
        public int JCarouselId { get; set; }

        public IList<int> SelectedProductIds { get; set; }

        #endregion
    }
}
