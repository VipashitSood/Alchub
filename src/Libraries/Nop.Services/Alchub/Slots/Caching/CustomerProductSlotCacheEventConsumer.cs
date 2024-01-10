using System.Threading.Tasks;
using Nop.Core.Domain.Slots;
using Nop.Services.Caching;

namespace Nop.Services.Alchub.Slots.Caching
{
    /// <summary>
    /// Represents a CustomerProductSlot cache event consumer
    /// </summary>
    public partial class CustomerProductSlotCacheEventConsumer : CacheEventConsumer<CustomerProductSlot>
    {
        /// <summary>
        /// Clear cache data
        /// </summary>
        /// <param name="entity">Entity</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        protected override async Task ClearCacheAsync(CustomerProductSlot entity)
        {
            await RemoveByPrefixAsync(SlotDefaults.CustomerProductSlotbySlotCacheKeyPrefix);
            await RemoveByPrefixAsync(SlotDefaults.CustomerProductSlotbyCustomerCacheKeyPrefix);
        }
    }
}
