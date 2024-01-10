using Nop.Core.Caching;

namespace Nop.Web.Infrastructure.Cache
{
    public static partial class NopModelCacheDefaults
    {
        /// <summary>
        /// Key for Home Manufacturers Model caching
        /// </summary>
        /// <remarks>
        /// {0} : current store ID
        /// </remarks>
        public static CacheKey HomeManufacturersModelKey => new("Nop.alchub.home.manufacturers-{0}", HomeManufacturersPrefixCacheKey);
        public static string HomeManufacturersPrefixCacheKey => "Nop.alchub.home.manufacturers";
    }
}
