using Nop.Plugin.Widgets.JCarousel.Domain;
using Nop.Plugin.Widgets.JCarousel.Models.Configuration;
using System.Threading.Tasks;

namespace Nop.Plugin.Widgets.JCarousel.Factories
{
    public partial interface IJCarouselModelFactory
    {
        /// <summary>
        /// Prepare JCarousel search model
        /// </summary>
        /// <param name="searchModel">Category search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the JCarousel search model
        /// </returns>
        Task<JCarouselSearchModel> PrepareJCarouselSearchModelAsync(JCarouselSearchModel searchModel);
        /// <summary>
        /// Prepare paged JCarousel list model
        /// </summary>
        /// <param name="searchModel">JCarousel search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the JCarousel list model
        /// </returns>
        Task<JCarouselListModel> PrepareJCarouselListModelAsync(JCarouselSearchModel searchModel);
        /// <summary>
        /// Prepare JCarousel model
        /// </summary>
        /// <param name="model">JCarousel model</param>
        /// <param name="jacrousel">JCarousel</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the JCarousel model
        /// </returns>
        Task<JCarouselModel> PrepareJCarouselModelAsync(JCarouselModel model, JCarouselLog jacrousel);

        /// <summary>
        /// Prepare paged jacrousel product list model
        /// </summary>
        /// <param name="searchModel">JCarousel product search model</param>
        /// <param name="jcarousel">JCarousel</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the jacrousel product list model
        /// </returns>
        Task<JCarouselProductListModel> PrepareJCarouselProductListModelAsync(JCarouselProductSearchModel searchModel, JCarouselLog jcarousel);

        /// <summary>
        /// Prepare product search model to add to the jacrousel
        /// </summary>
        /// <param name="searchModel">Product search model to add to the jacrousel</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the product search model to add to the jacrousel
        /// </returns>
        Task<AddProductToJCarouselSearchModel> PrepareAddProductToJCarouselSearchModelAsync(AddProductToJCarouselSearchModel searchModel);

        /// <summary>
        /// Prepare paged product list model to add to the jacrousel
        /// </summary>
        /// <param name="searchModel">Product search model to add to the jacrousel</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the product list model to add to the jacrousel
        /// </returns>
        Task<AddProductToJCarouselListModel> PrepareAddProductToJCarouselListModelAsync(AddProductToJCarouselSearchModel searchModel);
    }
}
