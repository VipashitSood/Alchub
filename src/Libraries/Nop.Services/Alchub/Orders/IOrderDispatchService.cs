using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Vendors;

namespace Nop.Services.Orders
{
    /// <summary>
    /// Order report service interface
    /// </summary>
    public partial interface IOrderDispatchService
    {

        /// <summary>
        /// Insert Dispatch 
        /// </summary>
        /// <param name="dispatch">Dispatch</param>
        /// <returns></returns>
        Task InsertDispatchAsync(Dispatch dispatch);

        /// <summary>
        /// Update Dispatch
        /// </summary>
        /// <param name="dispatch">Dispatch</param>
        /// <returns></returns>
        Task UpdateDispatchAsync(Dispatch dispatch);

        /// <summary>
        /// Delete multiple list of dispatch
        /// </summary>
        /// <param name="dispatch">Dispatch</param>
        /// <returns></returns>
        Task DeleteDispatchAsync(IList<Dispatch> dispatch);

        /// <summary>
        ///  Delete Dispatch
        /// </summary>
        /// <param name="dispatch">Dispatch</param>
        /// <returns></returns>
        Task DeleteDispatchAsync(Dispatch dispatch);

        /// <summary>
        ///  Get all order Item Vendor according to time slots
        /// </summary>
        /// <param name="vendorId">vendor Id</param>
        /// <returns></returns>
        Task<IList<Dispatch>> GetOrderItemVendorTimeSlotsListAysc(int vendorId = 0);

        /// <summary>
        /// Get All Dispacth External Delevery Id
        /// </summary>
        /// <returns></returns>
        Task<IList<Dispatch>> GetDispatchOrderOneWeekAsync();

        /// <summary>
        ///  Get Dispatch OrderItem TrackingUrl
        /// </summary>
        /// <param name="orderItemId">orderItemId</param>
        /// <returns></returns>
        Task<string> GetDispatchOrderItemIdTrackingUrlAsync(int orderItemId);

        /// <summary>
        ///  Get Quotes Request Door Dash
        /// </summary>
        /// <param name="customer">customer</param>
        /// <param name="vendor">vendor</param>
        /// <param name="order">order</param>
        /// <param name="orderItem">orderItem</param>
        /// <returns> Return Request Get Door Dash Quotes details like Fees</returns>
        Task<HttpResponseMessage> GetDoorDashQuotesAsync(Customer customer, Vendor vendor, Order order, OrderItem orderItem);

        /// <summary>
        ///  Accept Quote send request to door dash
        /// </summary>
        /// <param name="externalDeliveryId">externalDeliveryId</param>
        /// <param name="tipAmount">tipAmount</param>
        /// <param name="dropOffInstruction">dropOffInstruction</param>
        /// <returns>Return get tracking url Dasher details</returns>
        Task<HttpResponseMessage> AcceptDoorDashQuotesAsync(Vendor vendor, string externalDeliveryId, int tipAmount, string dropOffInstruction);


        /// <summary>
        ///  Get status of delivery from Door dash
        /// </summary>
        /// <param name="externalDeliveryId">ExternalDeliveryId</param>
        /// <returns>Return delivery status and Dasher details</returns>
        Task<HttpResponseMessage> GetDoorDashDeliveryAsync(string externalDeliveryId = "");

    }
}
