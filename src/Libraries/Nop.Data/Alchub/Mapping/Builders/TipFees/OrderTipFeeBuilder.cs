using FluentMigrator.Builders.Create.Table;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.TipFees;
using Nop.Core.Domain.Vendors;
using Nop.Data.Extensions;

namespace Nop.Data.Mapping.Builders.TipFees
{
    /// <summary>
    /// Represents a Order Tip Fee entity builder
    /// </summary>
    public class OrderTipFeeBuilder : NopEntityBuilder<OrderTipFee>
    {
        #region Methods

        /// <summary>
        /// Apply entity configuration
        /// </summary>
        /// <param name="table">Create table expression builder</param>
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(OrderTipFee.OrderId)).AsInt32().ForeignKey<Order>()
                .WithColumn(nameof(OrderTipFee.VendorId)).AsInt32().ForeignKey<Vendor>();
        }

        #endregion
    }
}