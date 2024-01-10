using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Widgets.JCarousel.Components
{
    /// <summary>
    /// Represents the view component to place a widget into pages
    /// </summary>
    [ViewComponent(Name = JCarouselDefaults.HEAD_REFERENCE_VIEW_COMPONENT)]
    public class JCarouselHeadReferenceViewComponent : NopViewComponent
    {
        #region Methods

        /// <summary>
        /// Invoke view component
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the view component result
        /// </returns>
        public async Task<IViewComponentResult> InvokeAsync()
        {
            //view will have custom js & css reference.
            return await Task.FromResult(View());
        }

        #endregion
    }
}
