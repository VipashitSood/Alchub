using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Plugin.Api.Helpers;
using Nop.Services.Alchub.API.Filter;

namespace Nop.Plugin.Api.Services.Filter
{
    /// <summary>
    /// Represent overriden api filter optimization service
    /// </summary>
    public partial class OverridenApiFilterOptimzationService : ApiFilterOptimzationService
    {
        #region Fields

        private readonly IDTOHelper _dTOHelper;

        #endregion

        #region Ctor

        public OverridenApiFilterOptimzationService(IDTOHelper dTOHelper)
        {
            _dTOHelper = dTOHelper;
        }

        #endregion

        /// <summary>
        /// Create all filter api cache. This service will be overrided in nop.api plugin
        /// Note: We'll use this service in scheduler to create all filter api response data cache, to optimize mobile user interation.
        /// </summary>
        /// <param name="categoryId"></param>
        /// <param name="orderbyId"></param>
        /// <param name="availableVendorIds"></param>
        /// <returns></returns>
        public override async Task CreatAllFilterCache(int categoryId, int? orderbyId, List<int> availableVendorIds)
        {
            if (categoryId <= 0)
                return;


            //invoke dto hlper service to prepare chache according availble param.
            //We'll do nothing with data, this servrive call purpose is to create cache in memory.
            _ = await _dTOHelper.PrepareAllFilters(categoryId, orderbyId, availableVendorIds);
        }
    }
}
