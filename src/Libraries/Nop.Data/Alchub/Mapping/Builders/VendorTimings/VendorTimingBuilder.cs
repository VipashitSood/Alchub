using FluentMigrator.Builders.Create.Table;
using Nop.Core.Alchub.Domain.Vendors;
using Nop.Core.Domain.Vendors;
using Nop.Data.Extensions;

namespace Nop.Data.Mapping.Builders.VendorTimings
{
    /// <summary>
    /// Represents a Vendor Timing entity builder
    /// </summary>
    public class VendorTimingBuilder : NopEntityBuilder<VendorTiming>
    {
        #region Methods

        /// <summary>
        /// Apply entity configuration
        /// </summary>
        /// <param name="table">Create table expression builder</param>
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(VendorTiming.VendorId)).AsInt32().ForeignKey<Vendor>();
        }

        #endregion
    }
}