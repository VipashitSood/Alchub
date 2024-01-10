using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Markup;

namespace Nop.Services.Markup
{
    public partial interface ICategoryMarkupService
    {
        Task InsertCategoryMarkupAsync(CategoryMarkup categoryMarkup);

        Task UpdateCategoryMarkupAsync(CategoryMarkup categoryMarkup);

        Task<CategoryMarkup> GetCategoryMarkupIdAsync(int Id);

        Task DeleteCategoryMarkupAsync(IList<CategoryMarkup> categoryMarkup);

        Task DeleteCategoryMarkupAsync(CategoryMarkup categoryMarkup);

        Task<IList<CategoryMarkup>> GetCategoryMarkupByIdsAsync(int[] Ids, int vendorId = 0);

        Task<IPagedList<CategoryMarkup>> GetAllCategoryMarkupsAsync(int vendorId = 0,int categoryId = 0, int storeId = 0,
            int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false, bool? overridePublished = null);

        Task<CategoryMarkup> GetCategoryMarkupAsync(List<int> categoryIds, int vendorId = 0);

    }
}
