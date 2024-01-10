using System.Threading.Tasks;
using Nop.Core.Domain.Markup;
using Nop.Core.Domain.Slots;
using Nop.Web.Areas.Admin.Models.Markup;
using Nop.Web.Areas.Admin.Models.Slots;

namespace Nop.Web.Areas.Admin.Factories
{
    /// <summary>
    /// Represents the CategoryMarkup model factory
    /// </summary>
    public partial interface ICategoryMarkupModelFactory
    {
        /// <summary>
        /// Prepare CategoryMarkup search model
        /// </summary>
        /// <param name="searchModel">CategoryMarkup search model</param>
        /// <returns>CategoryMarkup search model</returns>
        Task<CategoryMarkupSearchModel> PrepareCategoryMarkupSearchModel(CategoryMarkupSearchModel searchModel);

        /// <summary>
        /// Prepare paged CategoryMarkup list model
        /// </summary>
        /// <param name="searchModel">CategoryMarkup search model</param>
        /// <returns>CategoryMarkup list model</returns>
        Task<CategoryMarkupListModel> PrepareCategoryMarkupListModel(CategoryMarkupSearchModel searchModel);

        /// <summary>
        ///  PrepareCategoryMarkupModel
        /// </summary>
        /// <param name="model"></param>
        /// <param name="categoryMarkup"></param>
        /// <param name="excludeProperties"></param>
        /// <returns></returns>
        Task<CategoryMarkupModel> PrepareCategoryMarkupModel(CategoryMarkupModel model, CategoryMarkup categoryMarkup, bool excludeProperties = false);


        Task PrepareCategoryMarkupCalculationModel(decimal markup = 0, int categoryId = 0, int vendorId = 0);
    }
}