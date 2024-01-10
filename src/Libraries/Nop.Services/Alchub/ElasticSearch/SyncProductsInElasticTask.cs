using System;
using System.Threading.Tasks;
using Nop.Services.Logging;
using Nop.Services.ScheduleTasks;

namespace Nop.Services.Alchub.ElasticSearch
{
    public class SyncProductsInElasticTask : IScheduleTask
    {
        #region Fields
        private readonly ILogger _logger;
        private readonly IElasticsearchIndexCreator _elasticSearchIndexCreator;
        #endregion

        #region Ctor
        public SyncProductsInElasticTask(
            ILogger logger,
            IElasticsearchIndexCreator elasticSearchManager)
        {
            _logger = logger;
            _elasticSearchIndexCreator = elasticSearchManager;
        }
        #endregion
        public virtual async Task ExecuteAsync()
        {
            try
            {
                await _elasticSearchIndexCreator.CreateElasticsearchIndex();
            }
            catch (Exception exc)
            {
                await _logger.ErrorAsync($"Error while task to index products. {exc.Message}", exc);
                throw;
            }
        }
    }
}
