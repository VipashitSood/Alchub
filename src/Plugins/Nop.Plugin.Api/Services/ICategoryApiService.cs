using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Plugin.Api.Infrastructure;

namespace Nop.Plugin.Api.Services
{
    public interface ICategoryApiService
    {
        Category GetCategoryById(int categoryId);

        Task<IPagedList<Category>> GetCategories(
            IList<int> ids = null,
            DateTime? createdAtMin = null, DateTime? createdAtMax = null, DateTime? updatedAtMin = null, DateTime? updatedAtMax = null,
            int limit = int.MaxValue, int page = Constants.Configurations.DEFAULT_PAGE_VALUE,
            int sinceId = Constants.Configurations.DEFAULT_SINCE_ID,
            int? productId = null, bool? publishedStatus = null, int? parentCategoryId = null);

        Task<int> GetCategoriesCountAsync(
            DateTime? createdAtMin = null, DateTime? createdAtMax = null, DateTime? updatedAtMin = null, DateTime? updatedAtMax = null,
            bool? publishedStatus = null, int? productId = null, int? parentCategoryId = null);

        Task<IDictionary<int, IList<Category>>> GetProductCategories(IList<int> productIds);

        IList<Category> GetTopCategories();
    }
}
