using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core.Domain.Orders;

namespace Nop.Plugin.Payments.StripeConnectRedirect.Services
{
    public partial interface IStripeWorkflowMessageService
    {
        #region Capture/Release

        /// <summary>
        /// Sends a Order payment captured notification to a customer
        /// </summary>
        /// <param name="order"></param>
        /// <param name="amountCaptured"></param>
        /// <param name="amountReleased"></param>
        /// <param name="languageId"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        Task<IList<int>> SendCapturePaymentCustomerNotificationAsync(Order order, string amountCaptured, string amountReleased, int languageId);

        /// <summary>
        /// Sends a Order payment released notification to a customer
        /// </summary>
        /// <param name="order"></param>
        /// <param name="amountCaptured"></param>
        /// <param name="amountReleased"></param>
        /// <param name="languageId"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        Task<IList<int>> SendReleasePaymentCustomerNotificationAsync(Order order, string amountCaptured, string amountReleased, int languageId);

        #endregion Capture/Release
    }
}