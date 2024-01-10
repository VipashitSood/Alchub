using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.Widgets.JCarousel.Factories;
using Nop.Plugin.Widgets.JCarousel.Services;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Widgets.JCarousel.Components
{
    /// <summary>
    /// Represents the view component to place a widget into pages
    /// </summary>
    [ViewComponent(Name = JCarouselDefaults.VIEW_COMPONENT)]
    public class JCarouselViewComponent : NopViewComponent
    {
        #region Fields

        private readonly IJCarouselService _jCarouselService;
        private readonly IPublicJCarouselModelFactory _publicjCarouselModelFactory;
        private readonly IWorkContext _workContext;

        #endregion

        #region Ctor

        public JCarouselViewComponent(IJCarouselService jCarouselService,
            IPublicJCarouselModelFactory publicjCarouselModelFactory,
            IWorkContext workContext)
        {
            _jCarouselService = jCarouselService;
            _publicjCarouselModelFactory = publicjCarouselModelFactory;
            _workContext = workContext;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Invoke view component
        /// </summary>
        /// <param name="widgetZone">Widget zone name</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the view component result
        /// </returns>
        public async Task<IViewComponentResult> InvokeAsync(string widgetZone)
        {
            //List of jacrousel ids
            var jcarousels = await _jCarouselService.GetAllJcarouselIds();
            var jCarouselIds = jcarousels.Select(j => j.Id).ToList();

            return View(jCarouselIds);
        }
        #endregion
    }
}

