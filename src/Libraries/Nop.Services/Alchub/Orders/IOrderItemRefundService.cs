using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core.Domain.Orders;

namespace Nop.Services.Orders
{
    /// <summary>
    /// Represents the order Item Refund
    /// </summary>
    public partial interface IOrderItemRefundService
    {

        #region Stripe Transfer

        /// <summary>
        /// Insert a order Item Refund
        /// </summary>
        /// <param name="stripeTransfer"></param>
        /// <returns></returns>
        Task InsertOrderItemRefundAsync(OrderItemRefund orderItemRefund);

        /// <summary>
        /// Insert a list of order Item Refund
        /// </summary>
        /// <param name="stripeTransfers"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        Task InsertOrderItemRefundAsync(IList<OrderItemRefund> orderItemRefunds);

        /// <summary>
        /// Update a order Item Refund
        /// </summary>
        /// <param name="stripeTransfer"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        Task UpdateOrderItemRefundAsync(OrderItemRefund orderItemRefund);

        /// <summary>
        /// Delete a order Item Refund
        /// </summary>
        /// <param name="stripeTransfer"></param>
        /// <returns></returns>
        Task DeleteOrderItemRefundAsync(OrderItemRefund orderItemRefund);

        /// <summary>
        /// a order Item Refund by Id
        /// </summary>
        /// <param name="stripeTransferId"></param>
        /// <returns></returns>
        Task<OrderItemRefund> GetOrderItemRefundByIdAsync(int orderItemRefundId);

        /// <summary>
        /// Get order Item Refund By Order Id
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        Task<OrderItemRefund> GetOrderItemRefundByOrderItemIdAsync(int orderItemId);


        Task<IList<OrderItemRefund>> GetOrderItemRefundByOrderIdAsync(int orderId);
        #endregion Stripe Transfer

    }
}