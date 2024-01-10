using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Payments.StripeConnectRedirect.Domain;
using Stripe;

namespace Nop.Plugin.Payments.StripeConnectRedirect.Services
{
    /// <summary>
    /// Represents the Stripe Connect Redirect Service Interface
    /// </summary>
    public partial interface IStripeConnectRedirectService
    {
        #region Methods

        /// <summary>
        /// IsConfigured
        /// </summary>
        /// <param name="settings"></param>
        /// <returns></returns>
        bool IsConfigured(StripeConnectRedirectPaymentSettings settings);

        #region Stripe Vendor Connect

        /// <summary>
        /// Insert a Stripe Vendor Connect
        /// </summary>
        /// <param name="stripeVendorConnect"></param>
        Task InsertStripeVendorConnectAsync(StripeVendorConnect stripeVendorConnect);

        /// <summary>
        /// Update a Stripe Vendor Connect
        /// </summary>
        /// <param name="stripeVendorConnect"></param>
        Task UpdateStripeVendorConnectAsync(StripeVendorConnect stripeVendorConnect);

        /// <summary>
        /// Delete a Stripe Vendor Connect
        /// </summary>
        /// <param name="stripeVendorConnect"></param>
        Task DeleteStripeVendorConnectAsync(StripeVendorConnect stripeVendorConnect);

        /// <summary>
        /// Search Stripe Vendor Connect
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        Task<IPagedList<StripeVendor>> SearchStripeVendorConnectAsync(
           int pageIndex = 0,
           int pageSize = int.MaxValue);

        /// <summary>
        /// Get Stripe Vendor Connect By Vendor Id
        /// </summary>
        /// <param name="vendorId">Vendor identifier</param>
        /// <returns></returns>
        Task<StripeVendorConnect> GetStripeVendorConnectByVendorIdAsync(int vendorId);

        /// <summary>
        /// Get Stripe Vendor Connects For Verifying
        /// </summary>
        /// <returns></returns>
        Task<IList<StripeVendorConnect>> GetStripeVendorConnectsForVerifyingAsync();

        #endregion Stripe Vendor Connect

        #region Vendor Connect With Stripe Order Item Mapping

        ///// <summary>
        ///// Insert a vendor connect with stripe order item mapping
        ///// </summary>
        ///// <param name="vendorConnectWithStripeOrderItemMapping">Vendor Connect With Stripe Order item mapping</param>
        //Task InsertVendorConnectWithStripeOrderItemMappingAsync(VendorConnectWithStripeOrderItemMapping vendorConnectWithStripeOrderItemMapping);

        ///// <summary>
        ///// Update a vendor connect with stripe order item mapping
        ///// </summary>
        ///// <param name="vendorConnectWithStripeOrderItemMapping">Vendor Connect With Stripe Order item mapping</param>
        //Task UpdateVendorConnectWithStripeOrderItemMappingAsync(VendorConnectWithStripeOrderItemMapping vendorConnectWithStripeOrderItemMapping);

        ///// <summary>
        ///// Gets a vendor connect with stripe order item mapping by shopping cart item id
        ///// </summary>
        ///// <param name="shoppingCartItemId"></param>
        ///// <returns>Vendor Connect With Stripe Order item mapping</returns>
        //Task<VendorConnectWithStripeOrderItemMapping> GetVendorConnectWithStripeOrderItemMappingByShoppingCartItemIdAsync(int shoppingCartItemId);

        ///// <summary>
        ///// Gets a vendor connect with stripe order item mapping by order id
        ///// </summary>
        ///// <param name="orderId"></param>
        ///// <returns>Vendor Connect With Stripe List</returns>
        //Task<List<VendorConnectWithStripeOrderItemMapping>> GetVendorConnectWithStripeOrderItemMappingByOrderIdAsync(int orderId);

        ///// <summary>
        ///// Gets a vendor connect with stripe order item mapping by order and order item id
        ///// </summary>
        ///// <param name="orderId"></param>
        ///// <param name="orderItemId"></param>
        ///// <returns>Vendor Connect With Stripe</returns>
        //Task<VendorConnectWithStripeOrderItemMapping> GetVendorConnectWithStripeOrderItemMappingByOrderAndOrderItemIdAsync(int orderId, int orderItemId);

        #endregion

        #region Stripe Order

        /// <summary>
        /// Insert a Stripe Order
        /// </summary>
        /// <param name="stripeOrder">Stripe Order</param>
        Task InsertStripeOrderAsync(StripeOrder stripeOrder);

        /// <summary>
        /// Update a Stripe Order
        /// </summary>
        /// <param name="stripeOrder">Stripe Order</param>
        Task UpdateStripeOrderAsync(StripeOrder stripeOrder);

        /// <summary>
        /// Delete a Stripe Order
        /// </summary>
        /// <param name="stripeOrder">Stripe Order</param>
        Task DeleteStripeOrderAsync(StripeOrder stripeOrder);

        /// <summary>
        /// Get Stripe Order By Id
        /// </summary>
        /// <param name="stripeOrderId"></param>
        /// <returns></returns>
        Task<StripeOrder> GetStripeOrderByIdAsync(int stripeOrderId);

        /// <summary>
        /// Get Stripe Order By Session Id
        /// </summary>
        /// <param name="sessionId"></param>
        /// <returns></returns>
        Task<StripeOrder> GetStripeOrderBySessionIdAsync(string sessionId);

        /// <summary>
        /// Get Stripe Order By Payment Intent Id
        /// </summary>
        /// <param name="paymentIntentId"></param>
        /// <returns></returns>
        Task<StripeOrder> GetStripeOrderByPaymentIntentIdAsync(string paymentIntentId);

        /// <summary>
        ///Get Stripe Order By Order Id
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        Task<StripeOrder> GetStripeOrderByOrderIdAsync(int orderId);

        #endregion

        #region Stripe Transfer

        /// <summary>
        /// Insert a Stripe Transfer
        /// </summary>
        /// <param name="stripeTransfer"></param>
        /// <returns></returns>
        Task InsertStripeTransferAsync(StripeTransfer stripeTransfer);

        /// <summary>
        /// Insert a list of Stripe Transfers
        /// </summary>
        /// <param name="stripeTransfers"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        Task InsertStripeTransfersAsync(IList<StripeTransfer> stripeTransfers);

        /// <summary>
        /// Update a Stripe Transfer
        /// </summary>
        /// <param name="stripeTransfer"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        Task UpdateStripeTransferAsync(StripeTransfer stripeTransfer);

        /// <summary>
        /// Delete a Stripe Transfer
        /// </summary>
        /// <param name="stripeTransfer"></param>
        /// <returns></returns>
        Task DeleteStripeTransferAsync(StripeTransfer stripeTransfer);

        /// <summary>
        /// a Stripe Transfer by Id
        /// </summary>
        /// <param name="stripeTransferId"></param>
        /// <returns></returns>
        Task<StripeTransfer> GetStripeTransferByIdAsync(int stripeTransferId);

        /// <summary>
        /// Get Stripe Transfers By Order Id
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        Task<IList<StripeTransfer>> GetStripeTransfersByOrderIdAsync(int orderId);

        /// <summary>
        /// Get Stripe Transfers By Order Id and Vendor Id
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="vendorId"></param>
        /// <returns></returns>
        Task<IList<StripeTransfer>> GetStripeTransfersByOrderIdVendorIdAsync(int orderId, int vendorId);

        #endregion Stripe Transfer

        #region Order

        // <summary>
        /// Stripe Order Payment Authorized
        /// </summary>
        /// <param name="paymentIntent"></param>
        /// <returns></returns>
        Task StripeOrderPaymentAuthorizedAsync(PaymentIntent paymentIntent);

        /// <summary>
        /// Stripe Order Payment Fail
        /// </summary>
        /// <param name="paymentIntent"></param>
        /// <returns></returns>
        Task StripeOrderPaymentFailAsync(PaymentIntent paymentIntent);

        /// <summary>
        /// Save Order Transfers Async
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        Task SaveOrderTransfersAsync(Order order);

        #endregion Order

        #region Capture/Release Payment

        /// <summary>
        /// /Capture the Payment
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        Task<bool> CapturePaymentAsync(int orderId);

        /// <summary>
        /// Release the Payment
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        Task<bool> ReleasePaymentAsync(int orderId);

        #endregion Capture/Release Payment

        #region Post process payment

        /// <summary>
        /// post process payment & get payment redirect url
        /// </summary>
        /// <param name="order"></param>
        /// <param name="requestFromApi"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        Task<string> PostProcessPaymentAndGetRedirectUrl(Order order, bool requestFromApi = false);

        #endregion Post process payment

        #region Misc

        /// <summary>
        /// Get order credit amounts (admin/vendor)
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        Task<IDictionary<int, decimal>> GetOrderCreditAmounts(Order order);

        #endregion

        #endregion Methods
    }
}