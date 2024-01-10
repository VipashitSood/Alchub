using FluentMigrator.Builders.Create.Table;
using Nop.Core.Domain.DeliveryFees;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Vendors;
using Nop.Data.Extensions;

namespace Nop.Data.Mapping.Builders.DeliveryFees
{
    /// <summary>
    /// Represents a Order Delivery Fee entity builder
    /// </summary>
    public class OrderDeliveryFeeBuilder : NopEntityBuilder<OrderDeliveryFee>
    {
        #region Methods

        /// <summary>
        /// Apply entity configuration
        /// </summary>
        /// <param name="table">Create table expression builder</param>
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(OrderDeliveryFee.OrderId)).AsInt32().ForeignKey<Order>()
                .WithColumn(nameof(OrderDeliveryFee.VendorId)).AsInt32().ForeignKey<Vendor>();
        }

        #endregion
    }
}