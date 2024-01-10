using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Widgets.JCarousel.Models.Configuration;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Widgets.JCarousel.Components
{
    public class JCarouselSliderViewComponent : NopViewComponent
    {
        /// <summary>
        /// To load the view of the slider using jcarouselModel
        /// </summary>
        /// <param name="jcarouselModel"></param>
        /// <returns></returns>
        public IViewComponentResult Invoke(JCarouselModel jcarouselModel)
        {
            return View("LoadSliderData", jcarouselModel);
        }
    }
}
