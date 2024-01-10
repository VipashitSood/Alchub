using System;
using System.Threading.Tasks;
using Nop.Services.Alchub.General;
using Nop.Services.Logging;
using Nop.Services.ScheduleTasks;

namespace Nop.Services.Alchub.Catalog
{
    public partial class SyncReArrangeSameNameProductsTask : IScheduleTask
    {
        #region Fields
        private readonly ILogger _logger;
        private readonly IAlchubSameNameProductService _alchubSameNameProductService;
        #endregion

        #region Ctor
        public SyncReArrangeSameNameProductsTask(
            ILogger logger,
            IAlchubSameNameProductService alchubSameNameProductService)
        {

            _logger = logger;
            _alchubSameNameProductService = alchubSameNameProductService;
        }
        #endregion

        #region Utilities

        private async Task RearangeGroupProductsSameNameWise()
        {
            try
            {
                await _alchubSameNameProductService.ReArrangeAllGroupProductsRelatedItemsAsync();
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync(string.Format("SyncSameNameProductsTask Error occured: during sync same name of products getting error : {0}. ", ex.Message), ex);
            }
        }

        private async Task RearangeGroupProductsVariantAndBrandWise()
        {
            await _alchubSameNameProductService.ReArrangeAllGroupProductsAssociatedProductsAsync();
        }

        #endregion

        #region Methods

        public virtual async Task ExecuteAsync()
        {
            bool useNewApproach = true;

            if (useNewApproach)
                await RearangeGroupProductsVariantAndBrandWise();
            else
                await RearangeGroupProductsSameNameWise();
        }

        #endregion
    }
}
