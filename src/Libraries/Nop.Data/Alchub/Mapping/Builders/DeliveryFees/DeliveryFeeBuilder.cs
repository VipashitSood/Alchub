using FluentMigrator.Builders.Create.Table;
using Nop.Core.Domain.DeliveryFees;
using Nop.Core.Domain.Vendors;
using Nop.Data.Extensions;

namespace Nop.Data.Mapping.Builders.DeliveryFees
{
    /// <summary>
    /// Represents a Delivery Fee entity builder
    /// </summary>
    public class DeliveryFeeBuilder : NopEntityBuilder<DeliveryFee>
    {
        #region Methods

        /// <summary>
        /// Apply entity configuration
        /// </summary>
        /// <param name="table">Create table expression builder</param>
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(DeliveryFee.VendorId)).AsInt32().ForeignKey<Vendor>();
        }

        #endregion
    }
}