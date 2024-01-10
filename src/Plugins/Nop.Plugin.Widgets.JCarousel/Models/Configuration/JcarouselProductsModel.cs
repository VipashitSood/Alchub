using Nop.Web.Framework.UI.Paging;
using Nop.Web.Models.Catalog;
using System.Collections.Generic;

namespace Nop.Plugin.Widgets.JCarousel.Models.Configuration
{
    public partial record JcarouselProductsModel : BasePageableModel 
    {
        #region Ctor
        public JcarouselProductsModel()
        {
            Products = new List<ProductOverviewModel>();
        }
        #endregion
        public List<ProductOverviewModel> Products { get; set; }
    }
}
