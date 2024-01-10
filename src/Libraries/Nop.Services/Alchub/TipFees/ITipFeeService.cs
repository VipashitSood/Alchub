using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.TipFees;

namespace Nop.Services.TipFees
{
    /// <summary>
    /// Tip Fee Service interface
    /// </summary>
    public partial interface ITipFeeService
    {
        #region Methods

        #region Order Tip Fee

        /// <summary>
        /// Inserts a Order Tip Fee
        /// </summary>
        /// <param name="orderTipFee"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        Task InsertOrderTipFeeAsync(OrderTipFee orderTipFee);

        /// <summary>
        /// Inserts a list of Order Tip Fee
        /// </summary>
        /// <param name="orderTipFees"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        Task InsertOrderTipFeesAsync(IList<OrderTipFee> orderTipFees);

        /// <summary>
        /// Updates a Order Tip Fee
        /// </summary>
        /// <param name="orderTipFee"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        Task UpdateOrderTipFeeAsync(OrderTipFee orderTipFee);

        /// <summary>
        /// Deletes a Order Tip Fee
        /// </summary>
        /// <param name="orderTipFee"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        Task DeleteOrderTipFeeAsync(OrderTipFee orderTipFee);

        /// <summary>
        /// Get Order Tip Fee By Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<OrderTipFee> GetOrderTipFeeByIdAsync(int id = 0);

        /// <summary>
        /// Get Order Tip Fees By Order Id
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        Task<IList<OrderTipFee>> GetOrderTipFeesByOrderIdAsync(int orderId = 0);

        /// <summary>
        /// Get Vendor Wise Order Tip Fees By Order Id
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        Task<IList<VendorWiseTipFee>> GetVendorWiseOrderTipFeesByOrderIdAsync(int orderId = 0);

        #endregion Order Tip Fee

        #region Tip Fee Calculations

        /// <summary>
        /// Get Total Tip Fee
        /// </summary>
        /// <param name="sciSubTotal"></param>
        /// <param name="orderSubtotal"></param>
        /// <returns></returns>
        Task<decimal> GetTotalTipFeeAsync(decimal sciSubTotal, decimal orderSubtotal);

        /// <summary>
        /// Get Vendor Wise Tip Fee
        /// </summary>
        /// <param name="cart"></param>
        /// <returns></returns>
        Task<IList<VendorWiseTipFee>> GetVendorWiseTipFeeAsync(IList<ShoppingCartItem> cart, decimal orderSubtotal);

        /// <summary>
        /// Get customer Tip Fee Details
        /// </summary>
        /// <returns></returns>
        Task<(int, decimal)> GetCustomerTipFeeDetailsAsync();

        /// <summary>
        /// Remove Customer Tip Fee Details
        /// </summary>
        /// <returns></returns>
        Task RemoveCustomerTipFeeDetailsAsync();

        #endregion Tip Fee Calculations

        #endregion Methods
    }
}