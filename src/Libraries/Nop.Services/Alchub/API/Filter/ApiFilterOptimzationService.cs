using System.Collections.Generic;
using System.Threading.Tasks;

namespace Nop.Services.Alchub.API.Filter
{
    /// <summary>
    /// Represent Nop.API filter optimization service
    /// Note: We are decalring service here and further we'll override it in Nop.API pluign.
    /// </summary>
    public partial class ApiFilterOptimzationService : IApiFilterOptimzationService
    {
        /// <summary>
        /// Create all filter api cache. This service will be overrided in nop.api plugin
        /// Note: We'll use this service in scheduler to create all filter api response data cache, to optimize mobile user interation.
        /// </summary>
        /// <param name="categoryId"></param>
        /// <param name="orderbyId"></param>
        /// <param name="availableVendorIds"></param>
        /// <returns></returns>
        public virtual Task CreatAllFilterCache(int categoryId, int? orderbyId, List<int> availableVendorIds)
        {
            //Nothing here. Overrided in nop.api plugin
            return Task.CompletedTask;
        }
    }
}
