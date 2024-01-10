using System;
using System.Threading.Tasks;
using Nop.Core.Domain.Catalog;
using Nop.Core.Events;
using Nop.Services.Alchub.Catalog;
using Nop.Services.Alchub.ElasticSearch;
using Nop.Services.Events;
using Nop.Services.Logging;

namespace Nop.Services.Alchub
{
    /// <summary>
    /// Represents alchub event consumer
    /// </summary>
    public class AlchubEventConsumer :
        IConsumer<EntityDeletedEvent<Product>>
    {
        #region Fields

        private readonly IProductFriendlyUpcService _productFriendlyUpcService;
        private readonly IElasticsearchIndexCreator _elasticsearchIndexCreator;
        private readonly ILogger _logger;

        #endregion

        #region Ctor

        public AlchubEventConsumer(IProductFriendlyUpcService productFriendlyUpcService,
            ILogger logger,
            IElasticsearchIndexCreator elasticsearchIndexCreator)
        {
            _productFriendlyUpcService = productFriendlyUpcService;
            _logger = logger;
            _elasticsearchIndexCreator = elasticsearchIndexCreator;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Handle product deleted event
        /// </summary>
        /// <param name="eventMessage">Event message</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task HandleEventAsync(EntityDeletedEvent<Product> eventMessage)
        {
            if (eventMessage.Entity == null)
                return;

            #region Friendly upc event handle

            //delete mapping records
            if (eventMessage.Entity.IsMaster)
                await _productFriendlyUpcService.DeleteProductFriendlyUpcCodeMappingsAsync(eventMessage.Entity);

            #endregion

            #region Elastic search event handle

            try
            {
                var deleteResponse = await _elasticsearchIndexCreator.DeleteProductDocumentAsync(eventMessage.Entity.Id);

                // Handle document deletion failure
                if (!deleteResponse)
                {
                    await _logger.ErrorAsync("Handle deleted product on elastic",
                        new Exception($"Failed to delete product with ID {eventMessage.Entity.Id}"));
                }
            }
            catch (Exception ex)
            {
                // Handled exception
                await _logger.ErrorAsync("An error occurred while deleting the product document", ex);
            }

            #endregion
        }

        #endregion
    }
}
