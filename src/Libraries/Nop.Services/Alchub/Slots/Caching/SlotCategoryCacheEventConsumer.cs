using System.Threading.Tasks;
using Nop.Core.Domain.Slots;
using Nop.Services.Caching;

namespace Nop.Services.Alchub.Slots.Caching
{
    /// <summary>
    /// Represents a SlotCategory cache event consumer
    /// </summary>
    public partial class SlotCategoryCacheEventConsumer : CacheEventConsumer<SlotCategory>
    {
        /// <summary>
        /// Clear cache data
        /// </summary>
        /// <param name="entity">Entity</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        protected override async Task ClearCacheAsync(SlotCategory entity)
        {
            await RemoveByPrefixAsync(SlotDefaults.SlotCategorybySlotCacheKeyPrefix);
        }
    }
}
