using System.Threading.Tasks;
using Nop.Core.Domain.TipFees;
using Nop.Services.Caching;

namespace Nop.Services.TipFees.Caching
{
    /// <summary>
    /// Represents a Order Tip Fee cache event consumer
    /// </summary>
    public partial class OrderTipFeeCacheEventConsumer : CacheEventConsumer<OrderTipFee>
    {
        #region Methods

        /// <summary>
        /// Clear cache data
        /// </summary>
        /// <param name="entity">Entity</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        protected override async Task ClearCacheAsync(OrderTipFee entity)
        {
            await RemoveAsync(TipFeeServiceDefaults.OrderTipFeeByIdCacheKey, entity.Id);
            await RemoveAsync(TipFeeServiceDefaults.OrderTipFeeByOrderIdCacheKey, entity.OrderId);
        }

        #endregion
    }
}