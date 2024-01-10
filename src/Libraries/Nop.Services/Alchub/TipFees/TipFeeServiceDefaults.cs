using Nop.Core.Caching;

namespace Nop.Services.TipFees
{
    /// <summary>
    /// Represents default values related to Tip fee services
    /// </summary>
    public static partial class TipFeeServiceDefaults
    {
        #region Caching defaults

        #region Order Delivery Fee

        /// <summary>
        /// Gets a key for caching
        /// </summary>
        public static CacheKey OrderTipFeeByIdCacheKey => new("Nop.ordertipfee.byid.{0}");

        /// <summary>
        /// Gets a key for caching
        /// </summary>
        public static CacheKey OrderTipFeeByOrderIdCacheKey => new("Nop.ordertipfee.byorderid.{0}");

        #endregion Order Delivery Fee

        #endregion Caching defaults
    }
}