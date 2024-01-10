using Nop.Core.Domain.Orders;
using Nop.Plugin.Forefront.Xero.Domain;
using Nop.Plugin.Forefront.Xero.Services;
using Nop.Services.Events;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Nop.Plugin.Forefront.Xero.Events
{
	public class OrderCancelEventConsumer : IConsumer<OrderCancelledEvent>
	{
        #region Fields

        private readonly IXeroQueueService _xeroQueueService;

        #endregion

        #region Ctor

        public OrderCancelEventConsumer(IXeroQueueService xeroQueueService)
		{
            this._xeroQueueService = xeroQueueService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="eventMessage"></param>
        public async Task HandleEventAsync(OrderCancelledEvent eventMessage)
		{
			var order = eventMessage.Order;
			if (order != null)
			{
				var parentQueueByOrderId = await _xeroQueueService.GetParentQueueByOrderId(order.Id);
				if (parentQueueByOrderId != null)
				{
					var xeroQueue = new XeroQueue()
					{
						OrderId = order.Id,
						ActionType = "Cancel",
						QueuedOn = DateTime.UtcNow,
						ParentId = parentQueueByOrderId.Id,
						Amount = order.OrderTotal,
						SyncAttemptCount = 0
					};

					await _xeroQueueService.InsertQueue(xeroQueue);
					IList<XeroQueue> list = new List<XeroQueue>();
					list.Add(xeroQueue);
                   await _xeroQueueService.CancelInvoice(list);
				}
			}
		}

        #endregion
    }
}