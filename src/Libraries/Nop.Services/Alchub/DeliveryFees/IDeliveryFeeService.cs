using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core.Domain.DeliveryFees;
using Nop.Core.Domain.Orders;

namespace Nop.Services.DeliveryFees
{
    /// <summary>
    /// Delivery Fee Service interface
    /// </summary>
    public partial interface IDeliveryFeeService
    {
        #region Methods

        #region Delivery Fee

        /// <summary>
        /// Inserts a Delivery Fee
        /// </summary>
        /// <param name="deliveryFee"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        Task InsertDeliveryFeeAsync(DeliveryFee deliveryFee);

        /// <summary>
        /// Updates a Delivery Fee
        /// </summary>
        /// <param name="deliveryFee"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        Task UpdateDeliveryFeeAsync(DeliveryFee deliveryFee);

        /// <summary>
        /// Deletes a Delivery Fee
        /// </summary>
        /// <param name="deliveryFee"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        Task DeleteDeliveryFeeAsync(DeliveryFee deliveryFee);

        /// <summary>
        /// Get Delivery Fee By Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<DeliveryFee> GetDeliveryFeeByIdAsync(int id = 0);

        /// <summary>
        /// Get Delivery Fee By Vendor Id
        /// </summary>
        /// <param name="vendorId"></param>
        /// <returns></returns>
        Task<DeliveryFee> GetDeliveryFeeByVendorIdAsync(int vendorId = 0);

        #endregion Delivery Fee

        #region Order Delivery Fee

        /// <summary>
        /// Inserts a Order Delivery Fee
        /// </summary>
        /// <param name="orderDeliveryFee"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        Task InsertOrderDeliveryFeeAsync(OrderDeliveryFee orderDeliveryFee);

        /// <summary>
        /// Inserts a list of Order Delivery Fee
        /// </summary>
        /// <param name="orderDeliveryFees"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        Task InsertOrderDeliveryFeesAsync(IList<OrderDeliveryFee> orderDeliveryFees);

        /// <summary>
        /// Updates a Order Delivery Fee
        /// </summary>
        /// <param name="orderDeliveryFee"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        Task UpdateOrderDeliveryFeeAsync(OrderDeliveryFee orderDeliveryFee);

        /// <summary>
        /// Deletes a Order Delivery Fee
        /// </summary>
        /// <param name="orderDeliveryFee"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        Task DeleteOrderDeliveryFeeAsync(OrderDeliveryFee orderDeliveryFee);

        /// <summary>
        /// Get Order Delivery Fee By Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<OrderDeliveryFee> GetOrderDeliveryFeeByIdAsync(int id = 0);

        /// <summary>
        /// Get Order Delivery Fees By Order Id
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        Task<IList<OrderDeliveryFee>> GetOrderDeliveryFeesByOrderIdAsync(int orderId = 0);

        /// <summary>
        /// Get Vendor Wise Order Delivery Fees By Order Id
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        Task<IList<VendorWiseDeliveryFee>> GetVendorWiseOrderDeliveryFeesByOrderIdAsync(int orderId = 0);

        #endregion Order Delivery Fee

        #region Delivery Fee Calculations

        Task<decimal> GetDistanceAsync(string origin, string destination);

        /// <summary>
        /// Get Vendor Wise Delivery Fee
        /// </summary>
        /// <param name="cart"></param>
        /// <returns></returns>
        Task<IList<VendorWiseDeliveryFee>> GetVendorWiseDeliveryFeeAsync(IList<ShoppingCartItem> cart);

        /// <summary>
        /// Get OrderItem delivery fee
        /// </summary>
        /// <param name="shoppingCartItem"></param>
        /// <returns></returns>
        Task<decimal> GetOrderItemDeliveryFeeAsync(ShoppingCartItem shoppingCartItem);

        #endregion Delivery Fee Calculations

        #endregion Methods
    }
}