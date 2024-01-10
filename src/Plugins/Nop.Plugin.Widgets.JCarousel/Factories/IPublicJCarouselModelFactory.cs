using Nop.Core.Domain.Customers;
using Nop.Plugin.Widgets.JCarousel.Domain;
using Nop.Plugin.Widgets.JCarousel.Models.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Nop.Plugin.Widgets.JCarousel.Factories
{
    public partial interface IPublicJCarouselModelFactory
    {
        /// <summary>
        /// Prepare Jcarousel public view model
        /// </summary>
        /// <param name="jcarousels">List of Jcarousels</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task<IList<JCarouselModel>> PrepareJcarouselOverviewModelsAsync(IEnumerable<JCarouselLog> jcarousels, Customer customer);

        /// <summary>
        /// Prepare data of the current carousel
        /// </summary>
        /// <param name="jcarousel"></param>
        /// <param name="customer"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        Task<JCarouselModel> PrepareJcarouselSliderDataModelAsync(JCarouselLog jcarousel, Customer customer);
    }
}
