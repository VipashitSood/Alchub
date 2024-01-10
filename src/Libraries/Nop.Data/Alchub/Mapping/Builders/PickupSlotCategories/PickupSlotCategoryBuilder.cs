using FluentMigrator.Builders.Create.Table;
using Nop.Core.Domain.Slots;
using Nop.Data.Extensions;

namespace Nop.Data.Mapping.Builders.PickupSlotCategories
{
    /// <summary>
    /// Represents a Delivery Fee entity builder
    /// </summary>
    public class PickupSlotCategoryBuilder : NopEntityBuilder<PickupSlotCategory>
    {
        #region Methods

        /// <summary>
        /// Apply entity configuration
        /// </summary>
        /// <param name="table">Create table expression builder</param>
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(PickupSlotCategory.PickupSlotId)).AsInt32().ForeignKey<PickupSlot>();
        }

        #endregion
    }
}