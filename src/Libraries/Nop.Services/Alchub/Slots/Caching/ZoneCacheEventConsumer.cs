using System.Threading.Tasks;
using Nop.Core.Caching;
using Nop.Core.Domain.Slots;
using Nop.Core.Domain.Vendors;
using Nop.Services.Caching;

namespace Nop.Services.Alchub.Slots.Caching
{
    /// <summary>
    /// Represents a zone cache event consumer
    /// </summary>
    public partial class ZoneCacheEventConsumer : CacheEventConsumer<Zone>
    {
        /// <summary>
        /// Clear cache data
        /// </summary>
        /// <param name="entity">Entity</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        protected override async Task ClearCacheAsync(Zone entity)
        {
            await RemoveByPrefixAsync(SlotDefaults.ZonesByVendorPrefix);
            await RemoveByPrefixAsync(SlotDefaults.SlotsByZoneCacheKeyPrefix);
            await RemoveByPrefixAsync(SlotDefaults.PickupSlotsByZoneCacheKeyPrefix);
            //mapping
            await RemoveByPrefixAsync(SlotDefaults.SlotCategorybySlotCacheKeyPrefix);
            //remove all vendor cache
            await RemoveByPrefixAsync(NopEntityCacheDefaults<Vendor>.AllPrefix);
        }
    }
}
