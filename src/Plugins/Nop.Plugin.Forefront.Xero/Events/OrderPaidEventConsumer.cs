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
	public class OrderPaidEventConsumer : IConsumer<OrderPaidEvent>
	{
        #region Fields

        private readonly IPluginService _pluginService;
		private readonly IXeroQueueService _xeroQueueService;
		private readonly ILogger _logger;

        #endregion

        #region Ctor

        public OrderPaidEventConsumer(IPluginService pluginService, IXeroQueueService xeroQueueService, ILogger logger)
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
        public async Task HandleEventAsync(OrderPaidEvent eventMessage)
		{
			if (await _pluginService.GetPluginDescriptorBySystemNameAsync<IPlugin>("Forefront.Xero", LoadPluginsMode.InstalledOnly) != null)
			{
				var order = eventMessage.Order;
				if (order != null)
				{
					try
					{
						if (order.OrderTotal != decimal.Zero)
						{
							var parentQueueByOrderId = await _xeroQueueService.GetParentQueueByOrderId(order.Id);
							if (parentQueueByOrderId != null)
							{
								bool? isPaid = parentQueueByOrderId.IsPaid;
								if ((isPaid.GetValueOrDefault() ? !isPaid.HasValue : true))
								{
									var xeroQueue = new XeroQueue()
									{
										OrderId = order.Id,
										ActionType = "Payment",
										QueuedOn = DateTime.UtcNow,
										ParentId = parentQueueByOrderId.Id,
										Amount = order.OrderTotal,
										SyncAttemptCount = 0
									};

									await _xeroQueueService.InsertQueue(xeroQueue);
									IList<XeroQueue> list = new List<XeroQueue>();
									list.Add(xeroQueue);
                                    await _xeroQueueService.XeroPayment(list);
								}
							}
						}
					}
					catch (Exception ex)
					{
					 await	_logger.InsertLogAsync(LogLevel.Error, string.Concat("Xero Queue Payment Error ", ex.InnerException), string.Concat(ex.Message, "stacktrace", ex.StackTrace), null);
					}
				}
			}
		}

        #endregion
    }
}