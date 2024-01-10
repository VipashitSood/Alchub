using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Markup;
using Nop.Core.Events;
using Nop.Data;
using Nop.Services.Stores;

namespace Nop.Services.Markup
{
    public partial class CategoryMarkupService : ICategoryMarkupService
    {
        #region Fields
        private readonly IRepository<CategoryMarkup> _categoryMarkupRepository;
        private readonly IEventPublisher _eventPublisher;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IStoreContext _storeContext;
        #endregion

        #region Ctor
        public CategoryMarkupService(IRepository<CategoryMarkup> categoryMarkupRepository,
            IEventPublisher eventPublisher,
            IStoreMappingService storeMappingService,
             IStoreContext storeContext)
        {
            _categoryMarkupRepository = categoryMarkupRepository;
            _eventPublisher = eventPublisher;
            _storeMappingService = storeMappingService;
            _storeContext = storeContext;
        }
        #endregion

        #region Methods

        public virtual async Task InsertCategoryMarkupAsync(CategoryMarkup categoryMarkup)
        {
            if (categoryMarkup == null)
                throw new ArgumentNullException(nameof(categoryMarkup));

            await _categoryMarkupRepository.InsertAsync(categoryMarkup);

            //event notification
            await _eventPublisher.EntityInsertedAsync(categoryMarkup);
        }

        public virtual async Task UpdateCategoryMarkupAsync(CategoryMarkup categoryMarkup)
        {
            if (categoryMarkup == null)
                throw new ArgumentNullException(nameof(categoryMarkup));

            await _categoryMarkupRepository.UpdateAsync(categoryMarkup);

            //event notification
            await _eventPublisher.EntityUpdatedAsync(categoryMarkup);
        }

        public virtual async Task<CategoryMarkup> GetCategoryMarkupIdAsync(int Id)
        {
            var query = _categoryMarkupRepository.Table;
            if (Id > 0)
			{
                query = query.Where(c => c.Id == Id);
            } 
            return query.FirstOrDefault();
        }
        public virtual async Task DeleteCategoryMarkupAsync(IList<CategoryMarkup> categoryMarkup)
        {
            if (categoryMarkup == null)
                throw new ArgumentNullException(nameof(categoryMarkup));

            foreach (var item in categoryMarkup)
            {
                await _categoryMarkupRepository.DeleteAsync(item);
            }
            //delete product           
        }

        public virtual async Task DeleteCategoryMarkupAsync(CategoryMarkup categoryMarkup)
        {
            if (categoryMarkup == null)
                throw new ArgumentNullException(nameof(categoryMarkup));

            await _categoryMarkupRepository.DeleteAsync(categoryMarkup);

        }
        public virtual async Task<IList<CategoryMarkup>> GetCategoryMarkupByIdsAsync(int[] Ids,int vendorId = 0)
        {
            if (Ids == null || Ids.Length == 0)
                return new List<CategoryMarkup>();


            var query = _categoryMarkupRepository.Table;

            query = from cm in query
                    where Ids.Contains(cm.Id)
                    select cm;

            if (vendorId > 0)
                query = query.Where(x => x.VendorId == vendorId);

            return query.ToList();
        }


        public virtual async Task<IPagedList<CategoryMarkup>> GetAllCategoryMarkupsAsync(int categoryId = 0, int storeId = 0, int vendorId = 0,
            int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false, bool? overridePublished = null)
        {

            var query = _categoryMarkupRepository.Table;

            if (categoryId > 0)
                query = query.Where(c => c.CategoryId == categoryId);

            if (vendorId > 0)
                query = query.Where(c => c.VendorId == vendorId);

            return await query.ToPagedListAsync(pageIndex, pageSize);
        }

        public virtual Task<CategoryMarkup> GetCategoryMarkupAsync(List<int> categoryIds, int vendorId = 0)
        {
            if (!categoryIds.Any())
                return null;

            var query = _categoryMarkupRepository.Table;

            query = from cm in query
                    where categoryIds.Contains(cm.CategoryId)
                    select cm;
            if (vendorId > 0)
                query = query.Where(x => x.VendorId == vendorId)?.OrderByDescending(x => x.UpdatedOnUtc);

            return Task.FromResult(query?.FirstOrDefault());
        }

        #endregion
    }
}
