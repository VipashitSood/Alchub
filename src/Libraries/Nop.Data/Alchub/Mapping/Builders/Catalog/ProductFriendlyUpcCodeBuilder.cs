using FluentMigrator.Builders.Create.Table;
using Nop.Core.Alchub.Domain.Catalog;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Vendors;
using Nop.Data.Extensions;
using Nop.Data.Mapping.Builders;

namespace Nop.Data.Alchub.Mapping.Builders.Catalog
{
    /// <summary>
    /// Represents a product friendly upc entity builder
    /// </summary>
    public class ProductFriendlyUpcCodeBuilder : NopEntityBuilder<ProductFriendlyUpcCode>
    {
        #region Methods

        /// <summary>
        /// Apply entity configuration
        /// </summary>
        /// <param name="table">Create table expression builder</param>
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(ProductFriendlyUpcCode.MasterProductId)).AsInt32().ForeignKey<Product>()
                .WithColumn(nameof(ProductFriendlyUpcCode.VendorId)).AsInt32().ForeignKey<Vendor>();
        }

        #endregion
    }
}
