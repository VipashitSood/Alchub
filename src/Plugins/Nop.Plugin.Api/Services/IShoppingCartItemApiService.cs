using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Api.Infrastructure;

namespace Nop.Plugin.Api.Services
{
    public interface IShoppingCartItemApiService
    {
        Task<IPagedList<ShoppingCartItem>> GetShoppingCartItems(
            int? customerId = null, DateTime? createdAtMin = null, DateTime? createdAtMax = null, int productId = 0,
            DateTime? updatedAtMin = null, DateTime? updatedAtMax = null, int? limit = null,
            int? page = null, ShoppingCartType? shoppingCartType = null);

        Task<ShoppingCartItem> GetShoppingCartItemAsync(int id);

        Task DeleteShoppingCartItemsAsync(ShoppingCartItem shoppingCartItem);
    }
}
