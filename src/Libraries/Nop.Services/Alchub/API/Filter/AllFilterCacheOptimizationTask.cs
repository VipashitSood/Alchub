using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Services.Catalog;
using Nop.Services.Logging;
using Nop.Services.ScheduleTasks;
using Nop.Services.Vendors;

namespace Nop.Services.Alchub.API.Filter
{
    /// <summary>
    /// Represent api all filter ache optimization schedule task
    /// </summary>
    public class AllFilterCacheOptimizationTask : IScheduleTask
    {
        #region Fields

        private readonly IApiFilterOptimzationService _apiFilterOptimzationService;
        private readonly ILogger _logger;
        private readonly IStoreContext _storeContext;
        private readonly ICategoryService _categoryService;
        private readonly IVendorService _vendorService;

        #endregion

        #region Ctor

        public AllFilterCacheOptimizationTask(IApiFilterOptimzationService apiFilterOptimzationService,
            ILogger logger,
            IStoreContext storeContext,
            ICategoryService categoryService,
            IVendorService vendorService)
        {
            _apiFilterOptimzationService = apiFilterOptimzationService;
            _logger = logger;
            _storeContext = storeContext;
            _categoryService = categoryService;
            _vendorService = vendorService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Executes a task
        /// </summary>
        public virtual async Task ExecuteAsync()
        {
            try
            {
                var currentStore = await _storeContext.GetCurrentStoreAsync();
                var allCategories = (await _categoryService.GetAllCategoriesAsync(currentStore.Id)).ToList();
                //root categories
                var rootCategories = allCategories.Where(c => c.ParentCategoryId == 0).OrderBy(c => c.DisplayOrder).ToList();
                var allVendors = (await _vendorService.GetAllVendorsAsync())?.ToList();

                //prepare availble vendor combination are wise.
                var vendorCombinations = new List<VendorCombination>();

                //set all vendor cobination (i.e: when location is not searched)
                var allVendorIds = allVendors.Select(v => v.Id).OrderBy(id => id).ToList();
                if (!vendorCombinations.Select(x => x.AvailbleVendorIds).Any(x => x.SequenceEqual(allVendorIds)))
                    vendorCombinations.Add(new VendorCombination
                    {
                        AvailbleVendorIds = allVendorIds
                    });

                foreach (var venor in allVendors)
                {
                    if (string.IsNullOrEmpty(venor.GeoLocationCoordinates))
                        continue;

                    //get other availble vendors in this at this vendor location
                    var areaAvailbleVendors = (await _vendorService.GetAvailableGeoRadiusVendorIdsAsync(venor.GeoLocationCoordinates, allVendors))?.ToList();

                    //make sure this vendor is availble
                    if (!areaAvailbleVendors.Contains(venor.Id))
                        areaAvailbleVendors.Add(venor.Id);

                    //order by id
                    areaAvailbleVendors = areaAvailbleVendors?.OrderBy(x => x)?.ToList();

                    //var a = vendorCombinations.Select(x => x.AvailbleVendorIds).Any(x => x.SequenceEqual(areaAvailbleVendors));
                    //add only if same vendor combination not added already.
                    if (!vendorCombinations.Select(x => x.AvailbleVendorIds).Any(x => x.SequenceEqual(areaAvailbleVendors)))
                        vendorCombinations.Add(new VendorCombination
                        {
                            AvailbleVendorIds = areaAvailbleVendors
                        });
                }

                //prepare caching for all category + vendor combination
                foreach (var category in rootCategories)
                    foreach (var vendorCombination in vendorCombinations)
                        //create caching for each combination if not already created.
                        await _apiFilterOptimzationService.CreatAllFilterCache(category.Id, null, vendorCombination?.AvailbleVendorIds?.ToList());
            }
            catch (Exception exc)
            {
                await _logger.ErrorAsync($"Error while task all filter cache optimization run. {exc.Message}", exc);
            }
        }

        private class VendorCombination
        {
            public VendorCombination()
            {
                AvailbleVendorIds = new List<int>();
            }
            public IList<int> AvailbleVendorIds { get; set; }
        }

        #endregion
    }
}
