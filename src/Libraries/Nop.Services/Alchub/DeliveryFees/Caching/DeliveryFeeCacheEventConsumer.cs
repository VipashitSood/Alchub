using System.Threading.Tasks;
using Nop.Core.Domain.DeliveryFees;
using Nop.Services.Caching;

namespace Nop.Services.DeliveryFees.Caching
{
    /// <summary>
    /// Represents a Delivery Fee cache event consumer
    /// </summary>
    public partial class DeliveryFeeCacheEventConsumer : CacheEventConsumer<DeliveryFee>
    {
        #region Methods

        /// <summary>
        /// Clear cache data
        /// </summary>
        /// <param name="entity">Entity</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        protected override async Task ClearCacheAsync(DeliveryFee entity)
        {
            await RemoveAsync(DeliveryFeeServiceDefaults.DeliveryFeeByIdCacheKey, entity.Id);
            await RemoveAsync(DeliveryFeeServiceDefaults.DeliveryFeeByVendorIdCacheKey, entity.VendorId);
        }

        #endregion Methods
    }
}