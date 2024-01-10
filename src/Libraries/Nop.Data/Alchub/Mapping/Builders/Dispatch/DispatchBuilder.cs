using FluentMigrator.Builders.Create.Table;
using Nop.Core.Alchub.Domain.Vendors;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Vendors;
using Nop.Data.Extensions;

namespace Nop.Data.Mapping.Builders
{
    /// <summary>
    /// Represents a dispatch entity builder
    /// </summary>
    public class DispatchBuilder : NopEntityBuilder<Dispatch>
    {
        #region Methods

        /// <summary>
        /// Apply entity configuration
        /// </summary>
        /// <param name="table">Create table expression builder</param>
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(Dispatch.ExtrnalDeliveryId)).AsString(400).NotNullable()
                .WithColumn(nameof(Dispatch.CreatedOnUtc)).AsDateTime().NotNullable()
                .WithColumn(nameof(Dispatch.OrderNumber)).AsInt32().ForeignKey<Order>()
                .WithColumn(nameof(Dispatch.VendorId)).AsInt32().ForeignKey<Vendor>();
        }

        #endregion
    }
}