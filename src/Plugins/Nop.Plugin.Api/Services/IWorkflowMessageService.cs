using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Nop.Plugin.Api.Services
{
    public partial interface IWorkflowMessageService
    {
        Task<IList<int>> SendCustomerPasswordRecoveryMessageAsync(Customer customer,string otp, int languageId);

        #region Return requests

        /// <summary>
        /// Sends 'New Return Request' message to a store owner
        /// </summary>
        /// <param name="returnRequest">Return request</param>
        /// <param name="orderItem">Order item</param>
        /// <param name="order">Order</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the queued email identifier
        /// </returns>
        Task<IList<int>> SendNewReturnRequestStoreOwnerNotificationAsync(ReturnRequest returnRequest, OrderItem orderItem, Order order, int languageId);

        /// <summary>
        /// Sends 'New Return Request' message to a customer
        /// </summary>
        /// <param name="returnRequest">Return request</param>
        /// <param name="orderItem">Order item</param>
        /// <param name="order">Order</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the queued email identifier
        /// </returns>
        Task<IList<int>> SendNewReturnRequestCustomerNotificationAsync(ReturnRequest returnRequest, OrderItem orderItem, Order order);

        /// <summary>
        /// Sends 'Return Request status changed' message to a customer
        /// </summary>
        /// <param name="returnRequest">Return request</param>
        /// <param name="orderItem">Order item</param>
        /// <param name="order">Order</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the queued email identifier
        /// </returns>
        Task<IList<int>> SendReturnRequestStatusChangedCustomerNotificationAsync(ReturnRequest returnRequest, OrderItem orderItem, Order order);

        #endregion
    }
}
