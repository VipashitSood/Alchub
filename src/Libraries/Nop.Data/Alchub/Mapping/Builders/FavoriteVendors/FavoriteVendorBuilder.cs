using FluentMigrator.Builders.Create.Table;
using Nop.Core.Alchub.Domain.Vendors;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Vendors;
using Nop.Data.Extensions;

namespace Nop.Data.Mapping.Builders.FavoriteVendors
{
    /// <summary>
    /// Represents a favorite vendor entity builder
    /// </summary>
    public class FavoriteVendorBuilder : NopEntityBuilder<FavoriteVendor>
    {
        #region Methods

        /// <summary>
        /// Apply entity configuration
        /// </summary>
        /// <param name="table">Create table expression builder</param>
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(FavoriteVendor.VendorId)).AsInt32().ForeignKey<Vendor>()
                .WithColumn(nameof(FavoriteVendor.CustomerId)).AsInt32().ForeignKey<Customer>();
        }

        #endregion
    }
}