using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Vendors;

namespace Nop.Services.Messages
{
    public partial interface IWorkflowMessageService
    {
        /// <summary>
        /// Sends email to vendor for vendor products failed to import
        /// </summary>
        /// <param name="languageId"></param>
        /// <param name="vendor"></param>
        /// <param name="body"></param>
        /// <returns></returns>
        Task<IList<int>> SendInvalidProductMessage(int languageId, Vendor vendor, string body);

        /// <summary>
        /// Sends email to admin for vendor products failed to import
        /// </summary>
        /// <param name="languageId"></param>
        /// <param name="vendor"></param>
        /// <param name="body"></param>
        /// <returns></returns>
        Task<IList<int>> SendInvalidProductMessageForVendor(int languageId, Vendor vendor, string body);

        /// <summary>
        /// Sends email to admin for duplicate product sku
        /// </summary>
        /// <param name="languageId"></param>
        /// <param name="vendor"></param>
        /// <param name="body"></param>
        /// <returns></returns>
        Task<IList<int>> SendDuplicateProductSkuMessageForVendor(int languageId,
            Vendor vendor, string body);

        /// <summary>
        /// Sends email to admin for master products failed to import
        /// </summary>
        /// <param name="languageId"></param>
        /// <param name="vendor"></param>
        /// <param name="body"></param>
        /// <returns></returns>
        Task<IList<int>> SendInvalidProductMessage(int languageId, string body);

        Task<IList<int>> SendUnprocessedProductMessageForVendor(int languageId,
            Vendor vendor, string body);

        /// <summary>
        /// Send unprocessed products email to admin. (Sync vendor product schedule task)
        /// </summary>
        /// <param name="languageId"></param>
        /// <param name="vendor"></param>
        /// <param name="body"></param>
        /// <returns></returns>
        Task<IList<int>> SendUnprocessedProductMessageToAdmin(int languageId,
            Vendor vendor, string body);

        /// <summary>
        /// Sends a orderItems delivered notification to a customer
        /// </summary>
        /// <param name="order">order</param>
        /// <param name="orderItems">order items</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the queued email identifier
        /// </returns>
        Task<IList<int>> SendOrderItemsDeliveredCustomerNotificationAsync(Order order, IList<OrderItem> orderItems, int languageId);

        /// <summary>
        /// Sends a orderItems delivered notification to a customer
        /// </summary>
        /// <param name="order">order</param>
        /// <param name="orderItems">order items</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the queued email identifier
        /// </returns>
        Task<IList<int>> SendOrderItemsPickupCompletedCustomerNotificationAsync(Order order, IList<OrderItem> orderItems, int languageId);

        /// <summary>
        /// Sends a orderItems dispatched notification to a customer
        /// </summary>
        /// <param name="order">order</param>
        /// <param name="orderItems">order items</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the queued email identifier
        /// </returns>
        Task<IList<int>> SendOrderItemsDispachedCustomerNotificationAsync(Order order, IList<OrderItem> orderItems, int languageId);


        Task<IList<int>> SendOrderItemsCancelCustomerNotificationAsync(Order order, OrderItem orderItems, decimal orderItemRefundAmount, int languageId);

        Task<IList<int>> SendOrderItemsCancelVendorNotificationAsync(Order order, OrderItem orderItems, Vendor vendor, int languageId);

        Task<IList<int>> SendOrderItemsDeliveryDeniedCustomerNotificationAsync(Order order, IList<OrderItem> orderItems, int languageId);

        Task<IList<int>> SendOrderItemsDeliveryDeniedVendorNotificationAsync(Order order, IList<OrderItem> orderItems, Vendor vendor, int languageId);

        Task<IList<int>> SendPickupOrderItemCustomerNotificationAsync(Order order, OrderItem orderItems, int languageId);
    }
}
