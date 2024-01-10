using System;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Catalog;

namespace Nop.Services.Catalog
{
    /// <summary>
    /// Extended Category service interface
    /// </summary>
    public partial interface ICategoryService
    {
        /// <summary>
        /// Gets all categories - for sync catalog data
        /// </summary>
        /// <param name="storeId">Store identifier; 0 if you want to get all records</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <param name="createdOrUpdatedFromUtc">Created or Updated From date time utc</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the categories
        /// </returns>
        Task<IPagedList<Category>> SearchCategoriesAsync(int storeId = 0, bool showHidden = false, DateTime? createdOrUpdatedFromUtc = null,
            int pageIndex = 0, int pageSize = int.MaxValue);
    }
}