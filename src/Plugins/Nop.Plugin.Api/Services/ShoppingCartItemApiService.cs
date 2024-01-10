using System;
using System.Collections.Generic;
using System.Linq;
using Nop.Core;
using Nop.Data;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Api.DataStructures;
using Nop.Plugin.Api.Infrastructure;
using System.Threading.Tasks;

namespace Nop.Plugin.Api.Services
{
    public class ShoppingCartItemApiService : IShoppingCartItemApiService
    {
        private readonly IRepository<ShoppingCartItem> _shoppingCartItemsRepository;
        private readonly IStoreContext _storeContext;

        public ShoppingCartItemApiService(IRepository<ShoppingCartItem> shoppingCartItemsRepository, IStoreContext storeContext)
        {
            _shoppingCartItemsRepository = shoppingCartItemsRepository;
            _storeContext = storeContext;
        }

        public async Task<IPagedList<ShoppingCartItem>> GetShoppingCartItems(
            int? customerId = null, DateTime? createdAtMin = null, DateTime? createdAtMax = null, int productId = 0,
            DateTime? updatedAtMin = null, DateTime? updatedAtMax = null, int? limit = null,
            int? page = null, ShoppingCartType? shoppingCartType = null)
        {
            var pageIndex = ((page ?? Constants.Configurations.DEFAULT_PAGE_VALUE) - 1);
            if (pageIndex <= 0)
                pageIndex = 1;

            var query = await (GetShoppingCartItemsQuery(customerId, createdAtMin, createdAtMax, productId,
                                                  updatedAtMin, updatedAtMax, shoppingCartType)).ToListAsync();

            return new PagedList<ShoppingCartItem>(query.ToList(), pageIndex - 1, limit ?? Constants.Configurations.DEFAULT_LIMIT);
        }

        public Task<ShoppingCartItem> GetShoppingCartItemAsync(int id)
        {
            return _shoppingCartItemsRepository.GetByIdAsync(id);
        }

        private IQueryable<ShoppingCartItem> GetShoppingCartItemsQuery(
            int? customerId = null, DateTime? createdAtMin = null, DateTime? createdAtMax = null, int productId = 0,
            DateTime? updatedAtMin = null, DateTime? updatedAtMax = null, ShoppingCartType? shoppingCartType = null)
        {
            var query = _shoppingCartItemsRepository.Table;

            if (customerId != null)
            {
                query = query.Where(shoppingCartItem => shoppingCartItem.CustomerId == customerId);
            }

            if (createdAtMin != null)
            {
                query = query.Where(c => c.CreatedOnUtc > createdAtMin.Value);
            }

            if (createdAtMax != null)
            {
                query = query.Where(c => c.CreatedOnUtc < createdAtMax.Value);
            }

            if (updatedAtMin != null)
            {
                query = query.Where(c => c.UpdatedOnUtc > updatedAtMin.Value);
            }

            if (updatedAtMax != null)
            {
                query = query.Where(c => c.UpdatedOnUtc < updatedAtMax.Value);
            }

            if (shoppingCartType != null)
            {
                query = query.Where(c => c.ShoppingCartTypeId == (int)shoppingCartType.Value);
            }

            if (productId > 0)
            {
                query = query.Where(c => c.ProductId == productId);
            }

            // items for the current store only
            var currentStoreId = _storeContext.GetCurrentStore().Id;
            query = query.Where(c => c.StoreId == currentStoreId);

            query = query.OrderBy(shoppingCartItem => shoppingCartItem.Id);

            return query;
        }

        /// <summary>
        /// Marks affiliate as deleted 
        /// </summary>
        /// <param name="affiliate">Affiliate</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task DeleteShoppingCartItemsAsync(ShoppingCartItem shoppingCartItem)
        {
            await _shoppingCartItemsRepository.DeleteAsync(shoppingCartItem);
        }
    }
}
