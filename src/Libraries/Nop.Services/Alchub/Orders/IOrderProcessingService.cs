using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using Nop.Services.Payments;

namespace Nop.Services.Orders
{
    /// <summary>
    /// Order processing service interface
    /// </summary>
    public partial interface IOrderProcessingService
    {
        Task OrderStatusCancelledChangedAsync(Order order);

        Task OrderStatusCompleteChangedAsync(Order order);
        Task OrderItemDeliveredAsync(Order order, OrderItem orderItem);
        Task OrderItemCancelOrderAsync(Order order, OrderItem item);

        Task OrderItemDeliveryDeniedOrderAsync(Order order, OrderItem orderItem, string addOrderNoteMessage);

        /// <summary>
        /// Marks order as authorized
        /// </summary>
        /// <param name="order">Order</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task MarkAsAuthorizedAsync(Order order, bool performPaidAction = false);

        /// <summary>
        /// Marks order as paid
        /// </summary>
        /// <param name="order">Order</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task MarkOrderAsPaidAsync(Order order, bool performPaidAction = true);
    }
}
