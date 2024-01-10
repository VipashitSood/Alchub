using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Vendors;
using Nop.Data;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.DeliveryFees;
using Nop.Services.Localization;
using Nop.Services.TipFees;
using Nop.Services.Vendors;

namespace Nop.Services.Orders
{
    /// <summary>
    /// Represents the Stripe Connect Redirect Service
    /// </summary>
    public partial class OrderItemRefundService : IOrderItemRefundService
    {
        #region Fields


        private readonly IRepository<OrderItemRefund> _orderItemRefundRepository;

        #endregion Fields

        #region Ctor

        public OrderItemRefundService(
            IRepository<OrderItemRefund> orderItemRefundRepository)
        {
            _orderItemRefundRepository = orderItemRefundRepository;
        }

        #endregion Ctor


        #region Order Item Refund

        /// <summary>
        /// Insert a order Item Refund
        /// </summary>
        /// <param name="orderItemRefund"></param>
        /// <returns></returns>
        public virtual async Task InsertOrderItemRefundAsync(OrderItemRefund orderItemRefund)
        {
            if (orderItemRefund == null)
                throw new ArgumentNullException(nameof(orderItemRefund));

            await _orderItemRefundRepository.InsertAsync(orderItemRefund);
        }

        /// <summary>
        /// Insert a list of order Item Refund
        /// </summary>
        /// <param name="stripeTransfers"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public virtual async Task InsertOrderItemRefundAsync(IList<OrderItemRefund> orderItemRefunds)
        {
            if (orderItemRefunds == null)
                throw new ArgumentNullException(nameof(orderItemRefunds));

            await _orderItemRefundRepository.InsertAsync(orderItemRefunds);
        }

        /// <summary>
        /// Update a order Item Refund
        /// </summary>
        /// <param name="orderItemRefund"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public virtual async Task UpdateOrderItemRefundAsync(OrderItemRefund orderItemRefund)
        {
            if (orderItemRefund == null)
                throw new ArgumentNullException(nameof(orderItemRefund));

            await _orderItemRefundRepository.UpdateAsync(orderItemRefund);
        }

        /// <summary>
        /// Delete a order Item Refund
        /// </summary>
        /// <param name="orderItemRefund"></param>
        /// <returns></returns>
        public virtual async Task DeleteOrderItemRefundAsync(OrderItemRefund orderItemRefund)
        {
            if (orderItemRefund == null)
                throw new ArgumentNullException(nameof(orderItemRefund));

            await _orderItemRefundRepository.DeleteAsync(orderItemRefund);
        }

        /// <summary>
        /// Get a order Item Refund by Id
        /// </summary>
        /// <param name="orderItemRefundId"></param>
        /// <returns></returns>
        public virtual async Task<OrderItemRefund> GetOrderItemRefundByIdAsync(int orderItemRefundId)
        {
            return await _orderItemRefundRepository.GetByIdAsync(orderItemRefundId);
        }

        /// <summary>
        /// Get order Item Refund By order Item Id
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public virtual async Task<OrderItemRefund> GetOrderItemRefundByOrderItemIdAsync(int orderItemId)
        {
            return await _orderItemRefundRepository.Table.Where(x => x.OrderItemId == orderItemId).FirstOrDefaultAsync();
        }

        /// <summary>
        /// Get order Item Refund By order Item Id
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public virtual async Task<IList<OrderItemRefund>> GetOrderItemRefundByOrderIdAsync(int orderId)
        {
            return await _orderItemRefundRepository.Table.Where(x => x.OrderId == orderId).ToListAsync();
        }

        #endregion Order Item Refund


    }
}