using FluentMigrator;
using Nop.Data.Extensions;
using Nop.Data.Migrations;
using Nop.Plugin.Widgets.JCarousel.Domain;

namespace Nop.Plugin.Widgets.JCarousel.Data
{
    [NopMigration("2022/02/03 09:09:17:6455442", "Widgets.JCarousel base schema", MigrationProcessType.Installation)]
    public class SchemaMigration : AutoReversingMigration
    {
        #region Methods

        /// <summary>
        /// Collect the UP migration expressions
        /// </summary>
        public override void Up()
        {
            Create.TableFor<JCarouselLog>();
            Create.TableFor<ProductJCarouselMapping>();
        }
        #endregion
    }
}
