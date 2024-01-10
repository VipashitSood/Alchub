using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core.Domain.Orders;

namespace Nop.Services.Messages
{
    /// <summary>
    /// Message token provider
    /// </summary>
    public partial interface IMessageTokenProvider
    {
        /// <summary>
        /// Add shipment tokens
        /// </summary>
        /// <param name="tokens">List of already added tokens</param>
        /// <param name="order">order</param>
        /// <param name="orderItems">orderItems</param>
        /// <param name="languageId">Language identifier</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task AddOrderItemsDeliveredTokensAsync(IList<Token> tokens, Order order, IList<OrderItem> orderItems, int languageId);

        /// <summary>
        /// Add order item pickup completed tokens
        /// </summary>
        /// <param name="tokens">List of already added tokens</param>
        /// <param name="order">order</param>
        /// <param name="orderItems">orderItems</param>
        /// <param name="languageId">Language identifier</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task AddOrderItemsPickupCompletedTokensAsync(IList<Token> tokens, Order order, IList<OrderItem> orderItems, int languageId);

        /// <summary>
        /// Add order item dispacthed tokens
        /// </summary>
        /// <param name="tokens">List of already added tokens</param>
        /// <param name="order">order</param>
        /// <param name="orderItems">orderItems</param>
        /// <param name="languageId">Language identifier</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task AddOrderItemsDispacthedTokensAsync(IList<Token> tokens, Order order, IList<OrderItem> orderItems, int languageId);


        #region Order Item Cancel Email

        Task AddOrderItemsCancelCustomerTokensAsync(IList<Token> tokens, Order order, OrderItem orderItems, decimal orderItemRefundAmount, int languageId);

        Task AddOrderItemsCancelVendorTokensAsync(IList<Token> tokens, Order order, OrderItem orderItems, int languageId);

        Task AddOrderItemsDeliveryDeniedCustomerTokensAsync(IList<Token> tokens, Order order, IList<OrderItem> orderItems, int languageId);

        Task AddOrderItemsDeliveryDeniedVendorTokensAsync(IList<Token> tokens, Order order, IList<OrderItem> orderItems, int languageId);
        #endregion
    }
}