using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Catalog;

namespace Nop.Services.Catalog
{
    /// <summary>
    /// Extended Category service
    /// </summary>
    public partial class CategoryService : ICategoryService
    {
        #region Methods

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
        public virtual async Task<IPagedList<Category>> SearchCategoriesAsync(int storeId = 0, bool showHidden = false, DateTime? createdOrUpdatedFromUtc = null,
            int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var unsortedCategories = await _categoryRepository.GetAllAsync(async query =>
            {
                if (!showHidden)
                    query = query.Where(c => c.Published);

                //apply store mapping constraints
                if (storeId > 0)
                    query = await _storeMappingService.ApplyStoreMapping(query, storeId);

                //exclude deleted
                //query = query.Where(c => !c.Deleted);

                //from date time
                if (createdOrUpdatedFromUtc.HasValue)
                    query = query.Where(c => c.CreatedOnUtc > createdOrUpdatedFromUtc || c.UpdatedOnUtc > createdOrUpdatedFromUtc);

                return query.OrderBy(c => c.ParentCategoryId).ThenBy(c => c.DisplayOrder).ThenBy(c => c.Id);
            });

            //sort categories
            var sortedCategories = await SortCategoriesForTreeAsync(unsortedCategories);

            //paging
            return new PagedList<Category>(sortedCategories, pageIndex, pageSize);
        }

        /// <summary>
        /// Delete category
        /// </summary>
        /// <param name="category">Category</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task DeleteCategoryAsync(Category category)
        {
            //++Alchub

            //set updateOnUtc to include product in sync cataLOG system.
            category.UpdatedOnUtc = DateTime.UtcNow;

            //--Alchub

            await _categoryRepository.DeleteAsync(category);

            //reset a "Parent category" property of all child subcategories
            var subcategories = await GetAllCategoriesByParentCategoryIdAsync(category.Id, true);
            foreach (var subcategory in subcategories)
            {
                subcategory.ParentCategoryId = 0;
                await UpdateCategoryAsync(subcategory);
            }
        }

        #endregion
    }
}