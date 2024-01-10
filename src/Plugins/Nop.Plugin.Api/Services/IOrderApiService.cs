using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Shipping;
using Nop.Plugin.Api.Infrastructure;

namespace Nop.Plugin.Api.Services
{
    public interface IOrderApiService
    {
        IList<Order> GetOrdersByCustomerId(int customerId);

        Task<IPagedList<Order>> GetOrders(
            IList<int> ids = null, DateTime? createdAtMin = null, DateTime? createdAtMax = null,
            int limit = Constants.Configurations.DEFAULT_LIMIT, int page = Constants.Configurations.DEFAULT_PAGE_VALUE,
            int sinceId = Constants.Configurations.DEFAULT_SINCE_ID, OrderStatus? status = null, PaymentStatus? paymentStatus = null,
            ShippingStatus? shippingStatus = null, int? customerId = null, int? storeId = null);

        Order GetOrderById(int orderId);

        int GetOrdersCount(
            DateTime? createdAtMin = null, DateTime? createdAtMax = null, OrderStatus? status = null,
            PaymentStatus? paymentStatus = null, ShippingStatus? shippingStatus = null,
            int? customerId = null, int? storeId = null);
    }
}
