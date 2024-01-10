using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core.Alchub.Domain.Twillio;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Stores;

namespace Nop.Services.Alchub.Twillio
{
    /// <summary>
    /// Twillio sms service interface
    /// </summary>
    public interface ITwillioService
    {
        /// <summary>
        /// Twillio sms service interface
        /// </summary>
        /// <param name="accountSid"></param>
        /// <param name="authToken"></param>
        /// <param name="body"></param>
        /// <param name="fromPhone"></param>
        /// <param name="toPhone"></param>
        /// <returns></returns>
        Task<string> SendSMS(string accountSid, string authToken, string body, string fromPhone, string toPhone, Order order = null);

        /// <summary>
        /// Send order items delivered customer SMS
        /// </summary>
        /// <param name="order"></param>
        /// <param name="orderItems"></param>
        /// <param name="languageId"></param>
        /// <returns></returns>
        Task SendOrderItemsStatusUpdatedCustomerSMSAsync(Order order, IList<OrderItem> orderItems, int languageId, OrderItemStatusActionType orderItemStatusActionType);

        /// <summary>
        /// Send order placed customer SMS
        /// </summary>
        /// <param name="order"></param>
        /// <param name="languageId"></param>
        /// <returns></returns>
        Task SendOrderPlacedCustomerSMSAsync(Order order, int languageId);


        Task SendOrderItemCancelCustomerSMSAsync(Order order, OrderItem orderItem, int languageId);

        Task SendOrderItemDeliveryDeniedCustomerSMSAsync(Order order, OrderItem orderItems, int languageId);

        Task SendOrderItemPickupCustomerSMSAsync(Order order, OrderItem orderItem, int languageId);

        /// <summary>
        /// Send order placed vendor SMS
        /// </summary>
        /// <param name="order"></param>
        /// <param name="languageId"></param>
        /// <returns></returns>
        Task SendOrderPlacedVendorSMSAsync(Order order, int languageId);
    }
}
