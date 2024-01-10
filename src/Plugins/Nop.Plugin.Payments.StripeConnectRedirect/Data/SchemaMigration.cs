using FluentMigrator;
using Nop.Data;
using Nop.Data.Extensions;
using Nop.Data.Mapping;
using Nop.Data.Migrations;
using Nop.Plugin.Payments.StripeConnectRedirect.Domain;

namespace Nop.Plugin.Payments.StripeConnectRedirect.Data
{
    [NopMigration("2020/12/15 02:09:17:6455445", "Payments.StripeConnectRedirect base schema", MigrationProcessType.Installation)]
    public class SchemaMigration : MigrationBase
    {
        public override void Up()
        {
            if (!DataSettingsManager.IsDatabaseInstalled())
                return;

            //Create table
            var vendorConnectStripe = NameCompatibilityManager.GetTableName(typeof(StripeVendorConnect));
            if (!Schema.Table(vendorConnectStripe).Exists())
            {
                Create.TableFor<StripeVendorConnect>();
            }

            var stripeOrder = NameCompatibilityManager.GetTableName(typeof(StripeOrder));
            if (!Schema.Table(stripeOrder).Exists())
            {
                Create.TableFor<StripeOrder>();
            }

            var vendorConnectPayment = NameCompatibilityManager.GetTableName(typeof(StripeTransfer));
            if (!Schema.Table(vendorConnectPayment).Exists())
            {
                Create.TableFor<StripeTransfer>();
            }
        }

        public override void Down()
        {
            //Nothing
        }
    }
}