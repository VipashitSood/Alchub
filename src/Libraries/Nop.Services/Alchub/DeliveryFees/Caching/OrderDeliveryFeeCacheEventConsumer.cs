using System.Threading.Tasks;
using Nop.Core.Domain.DeliveryFees;
using Nop.Services.Caching;

namespace Nop.Services.DeliveryFees.Caching
{
    /// <summary>
    /// Represents a Order Delivery Fee cache event consumer
    /// </summary>
    public partial class OrderDeliveryFeeCacheEventConsumer : CacheEventConsumer<OrderDeliveryFee>
    {
        #region Methods

        /// <summary>
        /// Clear cache data
        /// </summary>
        /// <param name="entity">Entity</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        protected override async Task ClearCacheAsync(OrderDeliveryFee entity)
        {
            await RemoveAsync(DeliveryFeeServiceDefaults.OrderDeliveryFeeByIdCacheKey, entity.Id);
            await RemoveAsync(DeliveryFeeServiceDefaults.OrderDeliveryFeeByOrderIdCacheKey, entity.OrderId);
        }

        #endregion
    }
}