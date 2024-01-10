using FluentMigrator.Builders.Create.Table;
using Nop.Core.Domain.Slots;
using Nop.Data.Extensions;

namespace Nop.Data.Mapping.Builders.SlotCategories
{
    /// <summary>
    /// Represents a Delivery Fee entity builder
    /// </summary>
    public class SlotCategoryBuilder : NopEntityBuilder<SlotCategory>
    {
        #region Methods

        /// <summary>
        /// Apply entity configuration
        /// </summary>
        /// <param name="table">Create table expression builder</param>
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            table
                .WithColumn(nameof(SlotCategory.SlotId)).AsInt32().ForeignKey<Slot>();
        }

        #endregion
    }
}