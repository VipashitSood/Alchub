using System.Threading.Tasks;
using Nop.Web.Areas.Admin.Alchub.Models.Catalog;

namespace Nop.Web.Areas.Admin.Alchub.Factories
{
    /// <summary>
    /// Represents the product friendly upc model factory
    /// </summary>
    public partial interface IProductFriendlyUpcModelFactory
    {
        /// <summary>
        /// Prepare product friendly upc search model
        /// </summary>
        /// <param name="searchModel">Product search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the product search model
        /// </returns>
        Task<ProductFriendlyUpcSearchModel> PrepareProductFriendlyUpcSearchModelAsync(ProductFriendlyUpcSearchModel searchModel);

        /// <summary>
        /// Prepare paged product friendly upc list model
        /// </summary>
        /// <param name="searchModel">Product search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the product list model
        /// </returns>
        Task<ProductFriendlyUpcListModel> PrepareProductFriendlyUpctListModelAsync(ProductFriendlyUpcSearchModel searchModel);

        ///// <summary>
        ///// Prepare product friendly upc model
        ///// </summary>
        ///// <param name="model"></param>
        ///// <param name="product"></param>
        ///// <returns>
        ///// A task that represents the asynchronous operation
        ///// The task result contains the product list model
        ///// </returns>
        //Task<ProductFriendlyUpcModel> PrepareProductFriendlyUpcModelAsync(ProductFriendlyUpcModel model, Product product);
    }
}
