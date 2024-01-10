using Nop.Core.Domain.Logging;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Forefront.Xero.Domain;
using Nop.Plugin.Forefront.Xero.Services;
using Nop.Services.Events;
using Nop.Services.Logging;
using Nop.Services.Plugins;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Nop.Plugin.Forefront.Xero.Events
{
	public class OrderPlacedEventConsumer : IConsumer<OrderPlacedEvent>
    {
        #region Fields

        private readonly IPluginService _pluginService;
        private readonly IXeroQueueService _xeroQueueService;
        private readonly ILogger _logger;

        #endregion

        #region Ctor

        public OrderPlacedEventConsumer(IPluginService pluginService, IXeroQueueService xeroQueueService, ILogger logger)
        {
            this._pluginService = pluginService;
            this._xeroQueueService = xeroQueueService;
            this._logger = logger;
        }

        #endregion

        #region Methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="eventMessage"></param>
        public async Task HandleEventAsync(OrderPlacedEvent eventMessage)
        {
            if (await _pluginService.GetPluginDescriptorBySystemNameAsync<IPlugin>("Forefront.Xero") != null)
            {
                var order = eventMessage.Order;
                if (order != null)
                {
                    try
                    {
                        var xeroQueue = new XeroQueue()
                        {
                            OrderId = order.Id,
                            ActionType = "Invoice",
                            QueuedOn = DateTime.UtcNow,
                            Amount = order.OrderTotal,
                            SyncAttemptCount = 0
                        };

                      await  _xeroQueueService.InsertQueue(xeroQueue);
                        IList<XeroQueue> list = new List<XeroQueue>();
                        if (xeroQueue != null)
                        {
                            list.Add(xeroQueue);
                        await _xeroQueueService.CreateXeroInvoice(list);
                        }
                    }
                    catch (Exception ex)
                    {
                      await  _logger.InsertLogAsync(LogLevel.Error, string.Concat("Xero Queue Invoice Error ", ex.InnerException), string.Concat(ex.Message, "stacktrace", ex.StackTrace), null);
                    }
                }
            }
        }

        #endregion
    }
}