using FluentMigrator;
using Nop.Data.Extensions;
using Nop.Data.Migrations;
using Nop.Plugin.Forefront.Xero.Domain;

namespace Nop.Plugin.Forefront.Xero.Data
{
	
    [NopMigration("2020/07/22 09:09:17:6455442", "Forefront.Xero base schema", MigrationProcessType.Installation)]
    public class SchemaMigration : AutoReversingMigration
    {
       

        #region Methods

        /// <summary>
        /// Collect the UP migration expressions
        /// </summary>
        public override void Up()
        {
            Create.TableFor<XeroAccessRefreshToken>();
            Create.TableFor<XeroAccounting>();
            Create.TableFor<XeroProduct>();
            Create.TableFor<XeroQueue>();
        }

        #endregion
    }
}