using Nop.Core.Caching;

namespace Nop.Services.DeliveryFees
{
    /// <summary>
    /// Represents default values related to delivery fee services
    /// </summary>
    public static partial class DeliveryFeeServiceDefaults
    {
        #region Caching defaults

        #region Delivery Fee

        /// <summary>
        /// Gets a key for caching
        /// </summary>
        public static CacheKey DeliveryFeeByIdCacheKey => new("Nop.deliveryfee.byid.{0}");

        /// <summary>
        /// Gets a key for caching
        /// </summary>
        public static CacheKey DeliveryFeeByVendorIdCacheKey => new("Nop.deliveryfee.byvendorid.{0}");

        #endregion Delivery Fee

        #region Order Delivery Fee

        /// <summary>
        /// Gets a key for caching
        /// </summary>
        public static CacheKey OrderDeliveryFeeByIdCacheKey => new("Nop.orderdeliveryfee.byid.{0}");

        /// <summary>
        /// Gets a key for caching
        /// </summary>
        public static CacheKey OrderDeliveryFeeByOrderIdCacheKey => new("Nop.orderdeliveryfee.byorderid.{0}");

        #endregion Order Delivery Fee

        #region Delivery Fee Calculations

        /// <summary>
        /// Gets a key for caching
        /// </summary>
        public static CacheKey DistanceBetweenOriginAndDestinationCacheKey => new("Nop.distance.origin.destination.{0}.{1}");

        #endregion Delivery Fee Calculations

        #endregion Caching defaults

    }
}