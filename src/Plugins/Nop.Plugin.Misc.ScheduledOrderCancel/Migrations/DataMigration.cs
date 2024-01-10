using System;
using System.Linq;
using FluentMigrator;
using Nop.Core.Domain.Messages;
using Nop.Data;
using Nop.Data.Migrations;
using Nop.Plugin.Misc.ScheduledOrderCancel.Domain;

namespace Nop.Plugin.Misc.ScheduledOrderCancel.Migrations
{
    [NopMigration("2022-12-09 12:00:00", "Misc.ScheduledOrderCancel add records", MigrationProcessType.Update)]
    public class PluginMigration : Migration
    {
        private readonly INopDataProvider _dataProvider;

        public PluginMigration(INopDataProvider dataProvider)
        {
            _dataProvider = dataProvider;
        }

        public override void Up()
        {
            //// add message template (order cancel )
            //if (!_dataProvider.GetTable<MessageTemplate>().Any(pr => string.Compare(pr.Name, MessageTemplateSystemName.OrderCancelledCustomerCustomNotification, true) == 0))
            //{
            //    var messageTemplate = _dataProvider.InsertEntity(
            //        new MessageTemplate
            //        {
            //            Name = MessageTemplateSystemName.OrderCancelledCustomerCustomNotification,
            //            Subject = "%Store.Name%. Your order cancelled due to payment failure.",
            //            Body = $"<p>{Environment.NewLine}Hello %Order.CustomerFullName%, {Environment.NewLine}<br /><br />{Environment.NewLine}Your order has been canceled, due to payment failed to process.{Environment.NewLine}<br />{Environment.NewLine}Order Number: %Order.OrderNumber%{Environment.NewLine}<br />{Environment.NewLine}Order Details: <a href=\"%Order.OrderURLForCustomer%\" target=\"_blank\">%Order.OrderURLForCustomer%</a>{Environment.NewLine}<br />{Environment.NewLine}Date Ordered: %Order.CreatedOn%{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}Address{Environment.NewLine}<br />{Environment.NewLine}%Order.BillingFirstName% %Order.BillingLastName%{Environment.NewLine}<br />{Environment.NewLine}%Order.BillingAddress1%{Environment.NewLine}<br />{Environment.NewLine}%Order.BillingAddress2%{Environment.NewLine}<br />{Environment.NewLine}%Order.BillingCity% %Order.BillingZipPostalCode%{Environment.NewLine}<br />{Environment.NewLine}%Order.BillingStateProvince% %Order.BillingCountry%{Environment.NewLine}Type: %Order.BillingAddressType%{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}%Order.Product(s)%{Environment.NewLine}</p>{Environment.NewLine}",
            //            IsActive = true,
            //            EmailAccountId = _dataProvider.GetTable<EmailAccount>().FirstOrDefault()?.Id ?? 0
            //        }
            //    );
            //}
        }

        public override void Down()
        {
            //add the downgrade logic if necessary 
        }
    }
}
