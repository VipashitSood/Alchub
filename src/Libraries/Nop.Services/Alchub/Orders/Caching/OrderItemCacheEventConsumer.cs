using System.Threading.Tasks;
using Nop.Core.Domain.Orders;
using Nop.Services.Alchub.Slots;
using Nop.Services.Caching;

namespace Nop.Services.Orders.Caching
{
    /// <summary>
    /// Represents an order item cache event consumer
    /// </summary>
    public partial class OrderItemCacheEventConsumer : CacheEventConsumer<OrderItem>
    {
        /// <summary>
        /// Clear cache data
        /// </summary>
        /// <param name="entity">Entity</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        protected override async Task ClearCacheAsync(OrderItem entity)
        {
            //remove patent when orderitem gets insert/update/delete
            await RemoveByPrefixAsync(SlotDefaults.SlotsNumberInOrderItemCacheKeyPrefix);
        }
    }
}
