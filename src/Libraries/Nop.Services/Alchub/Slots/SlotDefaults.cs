using Nop.Core.Caching;

namespace Nop.Services.Alchub.Slots
{
    /// <summary>
    /// Represents default values related to slot services
    /// </summary>
    public static partial class SlotDefaults
    {
        #region Caching defaults

        #region Zone

        /// <summary>
        /// Gets a key for zone by vendor
        /// </summary>
        /// <remarks>
        /// {0} : vendor ID
        /// {1} : is active?
        /// {2} : is pickup?
        /// {3} : created by
        /// </remarks>
        public static CacheKey ZonesByVendorCacheKey => new("Alchub.Nop.slot.zone.byvendor.{0}-{1}-{2}-{3}", ZonesByVendorPrefix);

        /// <summary>
        /// Gets a key pattern to clear cache
        /// </summary>
        public static string ZonesByVendorPrefix => "Alchub.Nop.slot.zone.byvendor.";

        #endregion

        #region Slot

        /// <summary>
        /// Gets a key for slots by zone
        /// </summary>
        /// <remarks>
        /// {0} : zone ID
        /// </remarks>
        public static CacheKey SlotsByZoneCacheKey => new("Alchub.Nop.slot.byzone.{0}", ZonesByVendorPrefix);

         /// <summary>
        /// Gets a key for has vendor created slot
        /// </summary>
        /// <remarks>
        /// {0} : vendor ID
        /// </remarks>
        public static CacheKey HasVendorCreatedSlotCacheKey => new("Alchub.Nop.slot.byzone.HasVendorCreatedSlot.{0}", SlotsByZoneCacheKeyPrefix);

        /// <summary>
        /// Gets a key pattern to clear cache
        /// </summary>
        public static string SlotsByZoneCacheKeyPrefix => "Alchub.Nop.slot.byzone.";

        /// <summary>
        /// Gets a key for pickup slots by zone
        /// </summary>
        /// <remarks>
        /// {0} : zone ID
        /// </remarks>
        public static CacheKey PickupSlotsByZoneCacheKey => new("Alchub.Nop.pickup.slot.byzone.{0}", PickupSlotsByZoneCacheKeyPrefix);

        /// <summary>
        /// Gets a key pattern to clear cache
        /// </summary>
        public static string PickupSlotsByZoneCacheKeyPrefix => "Alchub.Nop.pickup.slot.byzone.";

        /// <summary>
        /// Key for caching slot by in orderitem
        /// </summary>
        /// <remarks>
        /// {0} : slot ID
        /// </remarks>
        public static CacheKey SlotsNumberInOrderItemCacheKey => new("Alchub.Nop.slots.in.orderitem.{0}", SlotsNumberInOrderItemCacheKeyPrefix);

        /// <summary>
        /// Gets a key pattern to clear cache
        /// </summary>
        public static string SlotsNumberInOrderItemCacheKeyPrefix => "Alchub.Nop.slots.in.orderitem.";

        #endregion

        #region CustomerProductSlot

        /// <summary>
        /// Key for caching customer product slot by slot id
        /// </summary>
        /// <remarks>
        /// {0} : slot ID
        /// </remarks>
        public static CacheKey CustomerProductSlotbySlotCacheKey => new("Alchub.Nop.slots.customerproductslot.byslot.{0}", CustomerProductSlotbySlotCacheKeyPrefix);

        /// <summary>
        /// Gets a key pattern to clear cache
        /// </summary>
        public static string CustomerProductSlotbySlotCacheKeyPrefix => "Alchub.Nop.slots.customerproductslot.byslot.";

        /// <summary>
        /// Key for caching customer product slot by customer, product, pickup options
        /// </summary>
        /// <remarks>
        /// {0} : customer ID
        /// {1} : product ID
        /// {2} : is pickup
        /// </remarks>
        public static CacheKey CustomerProductSlotbyCustomerCacheKey => new("Alchub.Nop.slots.customerproductslot.bycustomer.{0}-{1}-{2}", CustomerProductSlotbySlotCacheKeyPrefix);

        /// <summary>
        /// Gets a key pattern to clear cache
        /// </summary>
        public static string CustomerProductSlotbyCustomerCacheKeyPrefix => "Alchub.Nop.slots.customerproductslot.bycustomer.{0}";

        #endregion

        #region Slot Category

        /// <summary>
        /// Key for caching slot category by slot id
        /// </summary>
        /// <remarks>
        /// {0} : slot ID
        /// </remarks>
        public static CacheKey SlotCategorybySlotCacheKey => new("Alchub.Nop.slots.slotcategory.byslot.{0}", SlotCategorybySlotCacheKeyPrefix);

        /// <summary>
        /// Gets a key pattern to clear cache
        /// </summary>
        public static string SlotCategorybySlotCacheKeyPrefix => "Alchub.Nop.slots.slotcategory.byslot.";

        #endregion

        #endregion
    }
}
