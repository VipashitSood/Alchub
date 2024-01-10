using System;
using System.Threading.Tasks;
using Nop.Services.Alchub.General;
using Nop.Services.Logging;
using Nop.Services.ScheduleTasks;

namespace Nop.Services.Alchub.Catalog
{
    public partial class SyncSameNameProductsTask : IScheduleTask
    {
        #region Fields
        private readonly ILogger _logger;
        private readonly IAlchubSameNameProductService _alchubSameNameProductService;
        #endregion

        #region Ctor
        public SyncSameNameProductsTask(
            ILogger logger,
            IAlchubSameNameProductService alchubSameNameProductService)
        {

            _logger = logger;
            _alchubSameNameProductService = alchubSameNameProductService;
        }
        #endregion

        #region Utilities

        private async Task CreateGroupProductSameNameWise()
        {
            try
            {
                var products = await _alchubSameNameProductService.GetAllMasterProductSameByNameAsync();
                await _alchubSameNameProductService.SameProductNameCreateGroupProductsAsync(products);
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync(string.Format("SyncSameNameProductsTask Error occured: during sync same name of products getting error : {0}. ", ex.Message), ex);
            }
        }

        private async Task CreateGroupProductVariantAndBrandWise()
        {
            _ = await _alchubSameNameProductService.AssembleGroupProductsAsync();
        }

        #endregion

        #region Methods

        public virtual async Task ExecuteAsync()
        {
            bool useNewApproach = true;

            if (useNewApproach)
                await CreateGroupProductVariantAndBrandWise();
            else
                await CreateGroupProductSameNameWise();
        }

        #endregion
    }
}
