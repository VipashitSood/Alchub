using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentMigrator;
using Nito.AsyncEx.Synchronous;
using Nop.Core;
using Nop.Core.Alchub.Domain;
using Nop.Core.Alchub.Domain.Catalog;
using Nop.Core.Alchub.Domain.Google;
using Nop.Core.Alchub.Domain.Orders;
using Nop.Core.Alchub.Domain.Twillio;
using Nop.Core.Alchub.Domain.Vendors;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Configuration;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.DeliveryFees;
using Nop.Core.Domain.Markup;
using Nop.Core.Domain.Messages;
using Nop.Core.Domain.MultiVendor;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.ScheduleTasks;
using Nop.Core.Domain.Security;
using Nop.Core.Domain.Seo;
using Nop.Core.Domain.Slots;
using Nop.Core.Domain.Topics;
using Nop.Core.Domain.Vendors;
using Nop.Data.Extensions;
using Nop.Data.Mapping;

namespace Nop.Data.Migrations.UpgradeAlchub
{
    [NopMigration("2023/11/22 17:04:22", "4.50.0", UpdateMigrationType.Data, MigrationProcessType.Update)]
    public class DataMigration : Migration
    {
        private readonly INopDataProvider _dataProvider;
        private readonly IRepository<EmailAccount> _emailAccountRepository;

        public DataMigration(INopDataProvider dataProvider,
            IRepository<EmailAccount> emailAccountRepository)
        {
            _dataProvider = dataProvider;
            _emailAccountRepository = emailAccountRepository;
        }

        /// <summary>
        /// Collect the UP migration expressions
        /// </summary>
        public override void Up()
        {
            #region Product

            //add column in product table
            var productTableName = NameCompatibilityManager.GetTableName(typeof(Product));
            var isMasterColumnName = "IsMaster";

            //add column isMaster
            if (!Schema.Table(productTableName).Column(isMasterColumnName).Exists())
            {
                Alter.Table(productTableName)
                    .AddColumn(isMasterColumnName).AsBoolean().NotNullable().SetExistingRowsTo(0);
            }

            //add column UPCCode
            var upcCodeColumnName = "UPCCode";

            if (!Schema.Table(productTableName).Column(upcCodeColumnName).Exists())
            {
                Alter.Table(productTableName)
                    .AddColumn(upcCodeColumnName).AsString();
            }

            //22-8-22
            var sizeColumnName = "Size";
            //add column Size
            if (!Schema.Table(productTableName).Column(sizeColumnName).Exists())
            {
                Alter.Table(productTableName)
                    .AddColumn(sizeColumnName).AsString().Nullable();
            }

            var containerColumnName = "Container";
            //add column Container
            if (!Schema.Table(productTableName).Column(containerColumnName).Exists())
            {
                Alter.Table(productTableName)
                    .AddColumn(containerColumnName).AsString().Nullable();
            }

            //add column overridePrice
            var overridePriceColumnName = "OverridePrice";
            if (!Schema.Table(productTableName).Column(overridePriceColumnName).Exists())
            {
                Alter.Table(productTableName)
                    .AddColumn(overridePriceColumnName).AsBoolean().SetExistingRowsTo(0);
            }

            //add column overrideStock
            var overrideStockColumnName = "OverrideStock";
            if (!Schema.Table(productTableName).Column(overrideStockColumnName).Exists())
            {
                Alter.Table(productTableName)
                    .AddColumn(overrideStockColumnName).AsBoolean().SetExistingRowsTo(0);
            }

            //add column overridePrice
            var overrideNegativeStockColumnName = "OverrideNegativeStock";
            if (!Schema.Table(productTableName).Column(overrideNegativeStockColumnName).Exists())
            {
                Alter.Table(productTableName)
                    .AddColumn(overrideNegativeStockColumnName).AsBoolean().SetExistingRowsTo(0);
            }

            //add column IsAlcohol
            var isAlcoholColumnName = "IsAlcohol";
            if (!Schema.Table(productTableName).Column(isAlcoholColumnName).Exists())
            {
                Alter.Table(productTableName)
                    .AddColumn(isAlcoholColumnName).AsBoolean().NotNullable().SetExistingRowsTo(1);
            }

            //set product size & container settings defaults (23-8-22)
            //size
            var productSizeSettingName = $"{nameof(AlchubSettings)}.{nameof(AlchubSettings.ProductSizes)}";
            if (!_dataProvider.GetTable<Setting>().Any(s => s.StoreId == 0 && string.Compare(s.Name, productSizeSettingName, true) == 0))
            {
                var productSizeSetting = _dataProvider.InsertEntity(
                    new Setting
                    {
                        Name = productSizeSettingName,
                        Value = "170ml,200ml,250ml,350ml,500ml",
                        StoreId = 0
                    });
            }
            //container
            var productContainerSettingName = $"{nameof(AlchubSettings)}.{nameof(AlchubSettings.ProductContainers)}";
            if (!_dataProvider.GetTable<Setting>().Any(s => s.StoreId == 0 && string.Compare(s.Name, productContainerSettingName, true) == 0))
            {
                var productContainerSetting = _dataProvider.InsertEntity(
                    new Setting
                    {
                        Name = productContainerSettingName,
                        Value = "Container1,Container2,Container3,Container4,Container5",
                        StoreId = 0
                    });
            }

            //is product sku is editable setting
            var isProductSkuEditableSettingName = $"{nameof(AlchubSettings)}.{nameof(AlchubSettings.IsProductSkuEditable)}";
            if (!_dataProvider.GetTable<Setting>().Any(s => s.StoreId == 0 && string.Compare(s.Name, isProductSkuEditableSettingName, true) == 0))
            {
                var productContainerSetting = _dataProvider.InsertEntity(
                    new Setting
                    {
                        Name = isProductSkuEditableSettingName,
                        Value = false.ToString(),
                        StoreId = 0
                    });
            }

            //Add bulk import/export permission
            if (!_dataProvider.GetTable<PermissionRecord>().Any(pr => string.Compare(pr.SystemName, "AccessMasterBulkImportExport", StringComparison.InvariantCultureIgnoreCase) == 0))
            {
                var accessBulkImportExportPermission = _dataProvider.InsertEntity(
                    new PermissionRecord
                    {
                        Name = "Admin area. Access Master bulk import/export",
                        SystemName = "AccessMasterBulkImportExport",
                        Category = "Catalog"
                    }
                );

                //add it to the Admin role by default
                var adminRole = _dataProvider
                    .GetTable<CustomerRole>()
                    .FirstOrDefault(x => x.IsSystemRole && x.SystemName == NopCustomerDefaults.AdministratorsRoleName);

                if (accessBulkImportExportPermission != null && adminRole != null)
                {
                    _dataProvider.InsertEntity(
                        new PermissionRecordCustomerRoleMapping
                        {
                            CustomerRoleId = adminRole.Id,
                            PermissionRecordId = accessBulkImportExportPermission.Id
                        }
                    );
                }
            }

            //access vendor product actions ACL 20-04-23
            var accessVendorProductActionSystemName = "AccessVendorProductActions";
            if (!_dataProvider.GetTable<PermissionRecord>().Any(pr => string.Compare(pr.SystemName, accessVendorProductActionSystemName, StringComparison.InvariantCultureIgnoreCase) == 0))
            {
                var accessVendorProductActionsPermission = _dataProvider.InsertEntity(
                    new PermissionRecord
                    {
                        Name = "Admin area. Access Vendor Product Actions",
                        SystemName = accessVendorProductActionSystemName,
                        Category = "Catalog"
                    });

                //add it to the Admin role by default
                var adminRole = _dataProvider
                    .GetTable<CustomerRole>()
                    .FirstOrDefault(x => x.IsSystemRole && x.SystemName == NopCustomerDefaults.AdministratorsRoleName);

                if (adminRole != null)
                {
                    _dataProvider.InsertEntity(
                        new PermissionRecordCustomerRoleMapping
                        {
                            CustomerRoleId = adminRole.Id,
                            PermissionRecordId = accessVendorProductActionsPermission.Id
                        });
                }

                //add it to the Vendor role by default
                var vendorRole = _dataProvider
                    .GetTable<CustomerRole>()
                    .FirstOrDefault(x => x.IsSystemRole && x.SystemName == NopCustomerDefaults.VendorsRoleName);

                if (vendorRole != null)
                {
                    _dataProvider.InsertEntity(
                        new PermissionRecordCustomerRoleMapping
                        {
                            CustomerRoleId = vendorRole.Id,
                            PermissionRecordId = accessVendorProductActionsPermission.Id
                        });
                }
            }

            //add column ImageUrl
            var imageUrlColumnName = "ImageUrl";
            if (!Schema.Table(productTableName).Column(imageUrlColumnName).Exists())
            {
                Alter.Table(productTableName)
                    .AddColumn(imageUrlColumnName).AsString().Nullable();
            }
            #endregion

            #region Order
            //add column in order table
            var orderTableName = NameCompatibilityManager.GetTableName(typeof(Order));
            var serviceFeeColumnName = "ServiceFee";

            //add column service fee
            if (!Schema.Table(orderTableName).Column(serviceFeeColumnName).Exists())
            {
                Alter.Table(orderTableName)
                    .AddColumn(serviceFeeColumnName).AsDecimal(18, 4).NotNullable().SetExistingRowsTo(0);
            }

            //add column slot fee
            var slotFeeColumnName = "SlotFee";
            if (!Schema.Table(orderTableName).Column(slotFeeColumnName).Exists())
            {
                Alter.Table(orderTableName)
                    .AddColumn(slotFeeColumnName).AsDecimal(18, 4).NotNullable().SetExistingRowsTo(0);
            }

            #endregion

            #region OrderItem
            //add column in order table
            var orderItemTableName = NameCompatibilityManager.GetTableName(typeof(OrderItem));
            var soltIdOrderItemColumnName = "SlotId";

            //add column SlotId
            if (!Schema.Table(orderItemTableName).Column(soltIdOrderItemColumnName).Exists())
            {
                Alter.Table(orderItemTableName)
                    .AddColumn(soltIdOrderItemColumnName).AsInt64().NotNullable().SetExistingRowsTo(0);
            }

            var slotPriceOrderItemColumnName = "SlotPrice";

            //add column SlotPrice
            if (!Schema.Table(orderItemTableName).Column(slotPriceOrderItemColumnName).Exists())
            {
                Alter.Table(orderItemTableName)
                    .AddColumn(slotPriceOrderItemColumnName).AsDecimal(18, 4).NotNullable().SetExistingRowsTo(0);
            }

            var slotStartColumnName = "SlotStartTime";

            //add column SlotStartTime
            if (!Schema.Table(orderItemTableName).Column(slotStartColumnName).Exists())
            {
                Alter.Table(orderItemTableName)
                    .AddColumn(slotStartColumnName).AsDateTime2().NotNullable().SetExistingRowsTo(0);
            }

            var slotEndTimeColumnName = "SlotEndTime";

            //add column SlotEndTime
            if (!Schema.Table(orderItemTableName).Column(slotEndTimeColumnName).Exists())
            {
                Alter.Table(orderItemTableName)
                    .AddColumn(slotEndTimeColumnName).AsDateTime2().NotNullable().SetExistingRowsTo(0);
            }


            var slotTimeOderItemColumnName = "SlotTime";

            //add column SlotTime
            if (!Schema.Table(orderItemTableName).Column(slotTimeOderItemColumnName).Exists())
            {
                Alter.Table(orderItemTableName)
                    .AddColumn(slotTimeOderItemColumnName).AsString().NotNullable().SetExistingRowsTo(0);
            }

            var inPickupOderItemColumnName = "InPickup";

            //add column SlotTime
            if (!Schema.Table(orderItemTableName).Column(inPickupOderItemColumnName).Exists())
            {
                Alter.Table(orderItemTableName)
                    .AddColumn(inPickupOderItemColumnName).AsBoolean().NotNullable().SetExistingRowsTo(0);
            }

            //masterProductId
            var masterProductIdColumnName = "MasterProductId";
            if (!Schema.Table(orderItemTableName).Column(masterProductIdColumnName).Exists())
            {
                Alter.Table(orderItemTableName)
                    .AddColumn(masterProductIdColumnName).AsInt32().NotNullable().SetExistingRowsTo(0);
            }

            //groupedProductId
            var groupedProductIdColumnName = "GroupedProductId";
            if (!Schema.Table(orderItemTableName).Column(groupedProductIdColumnName).Exists())
            {
                Alter.Table(orderItemTableName)
                    .AddColumn(groupedProductIdColumnName).AsInt32().NotNullable().SetExistingRowsTo(0);
            }

            //customAttributesXml
            var customAttributesXmlColumnName = "CustomAttributesXml";
            if (!Schema.Table(orderItemTableName).Column(customAttributesXmlColumnName).Exists())
            {
                Alter.Table(orderItemTableName)
                    .AddColumn(customAttributesXmlColumnName).AsString(int.MaxValue).Nullable();
            }

            //customAttributesDescription
            var customAttributesDescriptionColumnName = "CustomAttributesDescription";
            if (!Schema.Table(orderItemTableName).Column(customAttributesDescriptionColumnName).Exists())
            {
                Alter.Table(orderItemTableName)
                    .AddColumn(customAttributesDescriptionColumnName).AsString(int.MaxValue).Nullable();
            }

            //add column orderItemStatuId
            var orderItemStatusIdColumnName = "OrderItemStatusId";
            if (!Schema.Table(orderItemTableName).Column(orderItemStatusIdColumnName).Exists())
            {
                Alter.Table(orderItemTableName)
                    .AddColumn(orderItemStatusIdColumnName).AsInt32().NotNullable().SetExistingRowsTo((int)OrderItemStatus.Pending);
            }

            //add column VendorManageDelivery
            var orderItemVendorManageDeliveryColumnName = "VendorManageDelivery";
            if (!Schema.Table(orderItemTableName).Column(orderItemVendorManageDeliveryColumnName).Exists())
            {
                Alter.Table(orderItemTableName)
                    .AddColumn(orderItemVendorManageDeliveryColumnName).AsBoolean().NotNullable().SetExistingRowsTo(0);
            }

            //add column VendorManageDelivery
            var orderItemDeliveryFeeColumnName = "DeliveryFee";
            if (!Schema.Table(orderItemTableName).Column(orderItemDeliveryFeeColumnName).Exists())
            {
                Alter.Table(orderItemTableName)
                    .AddColumn(orderItemDeliveryFeeColumnName).AsDecimal(18, 4).NotNullable().SetExistingRowsTo(0);
            }

            // add message template (orderItems delivered)
            if (!_dataProvider.GetTable<MessageTemplate>().Any(pr => string.Compare(pr.Name, MessageTemplateSystemNames.OrderItemsDeliveredCustomerNotification, true) == 0))
            {
                var messageTemplate = _dataProvider.InsertEntity(
                    new MessageTemplate
                    {
                        Name = MessageTemplateSystemNames.OrderItemsDeliveredCustomerNotification,
                        Subject = "Your order from %Store.Name% has been %if (!%Order.IsCompletelyDelivered%) partially endif%delivered.",
                        Body = $"<p>{Environment.NewLine}<a href=\"%Store.URL%\"> %Store.Name%</a>{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}Hello %Order.CustomerFullName%!,{Environment.NewLine}<br />{Environment.NewLine}Good news! Your order has been %if (!%Order.IsCompletelyDelivered%) partially endif%delivered.{Environment.NewLine}<br />{Environment.NewLine}Order Number: %Order.OrderNumber%{Environment.NewLine}<br />{Environment.NewLine}Order Details: <a href=\"%Order.OrderURLForCustomer%\" target=\"_blank\">%Order.OrderURLForCustomer%</a>{Environment.NewLine}<br />{Environment.NewLine}Date Ordered: %Order.CreatedOn%{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}Address{Environment.NewLine}<br />{Environment.NewLine}%Order.BillingFirstName% %Order.BillingLastName%{Environment.NewLine}<br />{Environment.NewLine}%Order.BillingAddress1%{Environment.NewLine}<br />{Environment.NewLine}%Order.BillingAddress2%{Environment.NewLine}<br />{Environment.NewLine}Type: %Order.BillingAddressType%{Environment.NewLine}<br />{Environment.NewLine}%Order.BillingCity% %Order.BillingZipPostalCode%{Environment.NewLine}<br />{Environment.NewLine}%Order.BillingStateProvince% %Order.BillingCountry%{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}Delivered Products:{Environment.NewLine}<br />{Environment.NewLine}%OrderItem.Product(s)%{Environment.NewLine}</p>{Environment.NewLine}",
                        IsActive = true,
                        EmailAccountId = _dataProvider.GetTable<EmailAccount>().FirstOrDefault()?.Id ?? 0
                    }
                );
            }
            // add message template (orderItems dispacthed)
            if (!_dataProvider.GetTable<MessageTemplate>().Any(pr => string.Compare(pr.Name, MessageTemplateSystemNames.OrderItemsDispatchedCustomerNotification, true) == 0))
            {
                var messageTemplate = _dataProvider.InsertEntity(
                    new MessageTemplate
                    {
                        Name = MessageTemplateSystemNames.OrderItemsDispatchedCustomerNotification,
                        Subject = "Products of your order from %Store.Name% has been dispatched.",
                        Body = $"<p>{Environment.NewLine}<a href=\"%Store.URL%\"> %Store.Name%</a>{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}Hello %Order.CustomerFullName%!,{Environment.NewLine}<br />{Environment.NewLine}Good news! Products of your order has been dispatched.{Environment.NewLine}<br />{Environment.NewLine}Order Number: %Order.OrderNumber%{Environment.NewLine}<br />{Environment.NewLine}Order Details: <a href=\"%Order.OrderURLForCustomer%\" target=\"_blank\">%Order.OrderURLForCustomer%</a>{Environment.NewLine}<br />{Environment.NewLine}Date Ordered: %Order.CreatedOn%{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}Address{Environment.NewLine}<br />{Environment.NewLine}%Order.BillingFirstName% %Order.BillingLastName%{Environment.NewLine}<br />{Environment.NewLine}%Order.BillingAddress1%{Environment.NewLine}<br />{Environment.NewLine}%Order.BillingAddress2%{Environment.NewLine}<br />{Environment.NewLine}Type: %Order.BillingAddressType%{Environment.NewLine}<br />{Environment.NewLine}%Order.BillingCity% %Order.BillingZipPostalCode%{Environment.NewLine}<br />{Environment.NewLine}%Order.BillingStateProvince% %Order.BillingCountry%{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}Delivered Products:{Environment.NewLine}<br />{Environment.NewLine}%OrderItem.Product(s)%{Environment.NewLine}</p>{Environment.NewLine}",
                        IsActive = true,
                        EmailAccountId = _dataProvider.GetTable<EmailAccount>().FirstOrDefault()?.Id ?? 0
                    }
                );
            }
            // add message template (orderItems pickeu completed)
            if (!_dataProvider.GetTable<MessageTemplate>().Any(pr => string.Compare(pr.Name, MessageTemplateSystemNames.OrderItemsPickupCompletedCustomerNotification, true) == 0))
            {
                var messageTemplate = _dataProvider.InsertEntity(
                    new MessageTemplate
                    {
                        Name = MessageTemplateSystemNames.OrderItemsPickupCompletedCustomerNotification,
                        Subject = "Pickup completed for your order from %Store.Name%.",
                        Body = $"<p>{Environment.NewLine}<a href=\"%Store.URL%\"> %Store.Name%</a>{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}Hello %Order.CustomerFullName%!,{Environment.NewLine}<br />{Environment.NewLine}Good news! Pickup completed for your order.{Environment.NewLine}<br />{Environment.NewLine}Order Number: %Order.OrderNumber%{Environment.NewLine}<br />{Environment.NewLine}Order Details: <a href=\"%Order.OrderURLForCustomer%\" target=\"_blank\">%Order.OrderURLForCustomer%</a>{Environment.NewLine}<br />{Environment.NewLine}Date Ordered: %Order.CreatedOn%{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}Address{Environment.NewLine}<br />{Environment.NewLine}%Order.BillingFirstName% %Order.BillingLastName%{Environment.NewLine}<br />{Environment.NewLine}%Order.BillingAddress1%{Environment.NewLine}<br />{Environment.NewLine}%Order.BillingAddress2%{Environment.NewLine}<br />{Environment.NewLine}Type: %Order.BillingAddressType%{Environment.NewLine}<br />{Environment.NewLine}%Order.BillingCity% %Order.BillingZipPostalCode%{Environment.NewLine}<br />{Environment.NewLine}%Order.BillingStateProvince% %Order.BillingCountry%{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}Delivered Products:{Environment.NewLine}<br />{Environment.NewLine}%OrderItem.Product(s)%{Environment.NewLine}</p>{Environment.NewLine}",
                        IsActive = true,
                        EmailAccountId = _dataProvider.GetTable<EmailAccount>().FirstOrDefault()?.Id ?? 0
                    }
                );
            }

            #endregion

            //Add permission
            if (!_dataProvider.GetTable<PermissionRecord>().Any(pr => string.Compare(pr.SystemName, "ManageServiceFee", StringComparison.InvariantCultureIgnoreCase) == 0))
            {
                //service fee permission record
                var serviceFeePermission = _dataProvider.InsertEntity(
                    new PermissionRecord
                    {
                        Name = "Admin area. Manage Service Fee",
                        SystemName = "ManageServiceFee",
                        Category = "Order"
                    });

                //add it to the Admin role by default
                var adminRole = _dataProvider
                    .GetTable<CustomerRole>()
                    .FirstOrDefault(x => x.IsSystemRole && x.SystemName == NopCustomerDefaults.AdministratorsRoleName);

                _dataProvider.InsertEntity(
                  new PermissionRecordCustomerRoleMapping
                  {
                      CustomerRoleId = adminRole.Id,
                      PermissionRecordId = serviceFeePermission.Id
                  });
            }

            //Add permission slot management
            if (!_dataProvider.GetTable<PermissionRecord>().Any(pr => string.Compare(pr.SystemName, "ManageSlots", StringComparison.InvariantCultureIgnoreCase) == 0))
            {
                var multiFactorAuthenticationPermission = _dataProvider.InsertEntity(
                    new PermissionRecord
                    {
                        Name = "Admin area. Manage Slots",
                        SystemName = "ManageSlots",
                        Category = "Configuration"
                    }
                );

                //add it to the Admin role by default
                var adminRole = _dataProvider
                    .GetTable<CustomerRole>()
                    .FirstOrDefault(x => x.IsSystemRole && x.SystemName == NopCustomerDefaults.AdministratorsRoleName);

                _dataProvider.InsertEntity(
                    new PermissionRecordCustomerRoleMapping
                    {
                        CustomerRoleId = adminRole.Id,
                        PermissionRecordId = multiFactorAuthenticationPermission.Id
                    }
                );

                var vendorRole = _dataProvider
                   .GetTable<CustomerRole>()
                   .FirstOrDefault(x => x.IsSystemRole && x.SystemName == NopCustomerDefaults.VendorsRoleName);

                _dataProvider.InsertEntity(
                    new PermissionRecordCustomerRoleMapping
                    {
                        CustomerRoleId = vendorRole.Id,
                        PermissionRecordId = multiFactorAuthenticationPermission.Id
                    }
                );
            }

            if (!_dataProvider.GetTable<PermissionRecord>().Any(pr => string.Compare(pr.SystemName, "ManageMasterProducts", StringComparison.InvariantCultureIgnoreCase) == 0))
            {
                var multiFactorAuthenticationPermission = _dataProvider.InsertEntity(
                    new PermissionRecord
                    {
                        Name = "Admin area. Manage Master Products",
                        SystemName = "ManageMasterProducts",
                        Category = "Catalog"
                    });

                //add it to the Admin role by default
                var adminRole = _dataProvider
                    .GetTable<CustomerRole>()
                    .FirstOrDefault(x => x.IsSystemRole && x.SystemName == NopCustomerDefaults.AdministratorsRoleName);

                _dataProvider.InsertEntity(
                    new PermissionRecordCustomerRoleMapping
                    {
                        CustomerRoleId = adminRole.Id,
                        PermissionRecordId = multiFactorAuthenticationPermission.Id
                    });
            }


            //Add Manage Dispatch
            if (!_dataProvider.GetTable<PermissionRecord>().Any(pr => string.Compare(pr.SystemName, "ManageDispatch", StringComparison.InvariantCultureIgnoreCase) == 0))
            {
                var multiFactorAuthenticationPermission = _dataProvider.InsertEntity(
                    new PermissionRecord
                    {
                        Name = "Admin area. Manage Dispatch",
                        SystemName = "ManageDispatch",
                        Category = "Sales"
                    }
                );

                //add it to the Admin role by default
                var adminRole = _dataProvider
                    .GetTable<CustomerRole>()
                    .FirstOrDefault(x => x.IsSystemRole && x.SystemName == NopCustomerDefaults.AdministratorsRoleName);

                _dataProvider.InsertEntity(
                    new PermissionRecordCustomerRoleMapping
                    {
                        CustomerRoleId = adminRole.Id,
                        PermissionRecordId = multiFactorAuthenticationPermission.Id
                    }
                );

                var vendorRole = _dataProvider
                   .GetTable<CustomerRole>()
                   .FirstOrDefault(x => x.IsSystemRole && x.SystemName == NopCustomerDefaults.VendorsRoleName);

                _dataProvider.InsertEntity(
                    new PermissionRecordCustomerRoleMapping
                    {
                        CustomerRoleId = vendorRole.Id,
                        PermissionRecordId = multiFactorAuthenticationPermission.Id
                    }
                );
            }

            #region ImportMessageTemplates

            //Add message template
            if (!_dataProvider.GetTable<MessageTemplate>().Any(mt => string.Compare(mt.Name, "InvalidProduct.Message", StringComparison.InvariantCultureIgnoreCase) == 0))
            {
                var eaGeneral = _emailAccountRepository.Table.FirstOrDefault();
                if (eaGeneral == null)
                    throw new Exception("Default email account cannot be loaded");

                _dataProvider.InsertEntity(
                new MessageTemplate
                {
                    Name = MessageTemplateSystemNames.InvalidProductMessage,
                    Subject = "Invalid product",
                    Body = $"<p>Dear %SuperAdmin.Name%, <br/> We would like to tell you that this Vendor %Vendor.Name% has tried to upload the products which are not present in the Master Sheet of Products.Kindly review all the below mentioned products :</p> %Body%<p>Regards,<br/>Alchub</p>",
                    IsActive = true,
                    EmailAccountId = eaGeneral.Id
                });
            }

            if (!_dataProvider.GetTable<MessageTemplate>().Any(mt => string.Compare(mt.Name, "InvalidProduct.Message.Vendor", StringComparison.InvariantCultureIgnoreCase) == 0))
            {
                var eaGeneral = _emailAccountRepository.Table.FirstOrDefault();
                if (eaGeneral == null)
                    throw new Exception("Default email account cannot be loaded");

                _dataProvider.InsertEntity(
                new MessageTemplate
                {
                    Name = MessageTemplateSystemNames.InvalidProductMessageForVendor,
                    Subject = "Invalid product",
                    Body = $"<p>Dear %Vendor.Name%, <br/> We would like to tell you that 'No any master products present in the database with these UPCCode'.</p> %Body%<p>Regards,<br/>Alchub</p>",
                    IsActive = true,
                    EmailAccountId = eaGeneral.Id
                });
            }

            //Add message template
            if (!_dataProvider.GetTable<MessageTemplate>().Any(mt => string.Compare(mt.Name, "Product.NotImported.Message", StringComparison.InvariantCultureIgnoreCase) == 0))
            {
                var eaGeneral = _emailAccountRepository.Table.FirstOrDefault();
                if (eaGeneral == null)
                    throw new Exception("Default email account cannot be loaded");

                _dataProvider.InsertEntity(
                new MessageTemplate
                {
                    Name = MessageTemplateSystemNames.ProductNotImportedMessage,
                    Subject = "Product not imported",
                    Body = $"<p>Dear %SuperAdmin.Name%, <br/> We would like to tell you that these product(s) failed to import, kindly review all the below mentioned products and reason of failure :</p> %Body%<p>Regards,<br/>Alchub</p>",
                    IsActive = true,
                    EmailAccountId = eaGeneral.Id
                });
            }

            //Add message template for duplicate sku for vendor
            if (!_dataProvider.GetTable<MessageTemplate>().Any(mt => string.Compare(mt.Name, "Duplicate.Product.Sku.Message.Vendor", StringComparison.InvariantCultureIgnoreCase) == 0))
            {
                var eaGeneral = _emailAccountRepository.Table.FirstOrDefault();
                if (eaGeneral == null)
                    throw new Exception("Default email account cannot be loaded");

                _dataProvider.InsertEntity(
                new MessageTemplate
                {
                    Name = MessageTemplateSystemNames.DuplicateProductSkuMessageForVendor,
                    Subject = "Duplicate product sku",
                    Body = $"<p>Dear %Vendor.Name%, <br/> We would like to tell you that 'these sku(s) are duplicate', so these products couldn't be uploaded.</p> %Body%<p>Regards,<br/>Alchub</p>",
                    IsActive = true,
                    EmailAccountId = eaGeneral.Id
                });
            }

            //Add message template for unprocessed product for vendor
            if (!_dataProvider.GetTable<MessageTemplate>().Any(mt => string.Compare(mt.Name, "UnprocessedProduct.Message.Vendor", StringComparison.InvariantCultureIgnoreCase) == 0))
            {
                var eaGeneral = _emailAccountRepository.Table.FirstOrDefault();
                if (eaGeneral == null)
                    throw new Exception("Default email account cannot be loaded");

                _dataProvider.InsertEntity(
                new MessageTemplate
                {
                    Name = MessageTemplateSystemNames.UnprocessedProductMessageForVendor,
                    Subject = "Unprocessed product",
                    Body = $"<p>Dear %Vendor.Name%, <br/> We would like to tell you that 'These product(s) couldn't be processed due to specific reason'.</p> %Body%<p>Regards,<br/>Alchub</p>",
                    IsActive = true,
                    EmailAccountId = eaGeneral.Id
                });
            }

            //Add message template for unprocessed product for admin
            if (!_dataProvider.GetTable<MessageTemplate>().Any(mt => string.Compare(mt.Name, MessageTemplateSystemNames.UNPROCESSED_PRODUCT_MESSAGE_TO_ADMIN, StringComparison.InvariantCultureIgnoreCase) == 0))
            {
                var eaGeneral = _emailAccountRepository.Table.FirstOrDefault();
                if (eaGeneral == null)
                    throw new Exception("Default email account cannot be loaded");

                _dataProvider.InsertEntity(
                new MessageTemplate
                {
                    Name = MessageTemplateSystemNames.UNPROCESSED_PRODUCT_MESSAGE_TO_ADMIN,
                    Subject = "Unprocessed vendor's product(s)",
                    Body = $"<p>Dear %SuperAdmin.Name%, <br/> We would like to inform you that during the Sync vendor schedule process, this '%Vendor.Name%' vendor's product(s) couldn't be processed due to a specific reason.</p> %Body%<p>Regards,<br/>Alchub</p>",
                    IsActive = true,
                    EmailAccountId = eaGeneral.Id
                });
            }

            #endregion

            //Add permission slot management
            if (!_dataProvider.GetTable<PermissionRecord>().Any(pr => string.Compare(pr.SystemName, "ManageCategoryMarkups", StringComparison.InvariantCultureIgnoreCase) == 0))
            {
                var multiFactorAuthenticationPermission = _dataProvider.InsertEntity(
                    new PermissionRecord
                    {
                        Name = "Admin area. Manage CategoryMarkup",
                        SystemName = "ManageCategoryMarkups",
                        Category = "Catalog"
                    }
                );

                //add it to the Admin role by default
                var adminRole = _dataProvider
                    .GetTable<CustomerRole>()
                    .FirstOrDefault(x => x.IsSystemRole && x.SystemName == NopCustomerDefaults.AdministratorsRoleName);

                _dataProvider.InsertEntity(
                    new PermissionRecordCustomerRoleMapping
                    {
                        CustomerRoleId = adminRole.Id,
                        PermissionRecordId = multiFactorAuthenticationPermission.Id
                    }
                );

                //add it to the Admin role by default
                var vendorRole = _dataProvider
                    .GetTable<CustomerRole>()
                    .FirstOrDefault(x => x.IsSystemRole && x.SystemName == NopCustomerDefaults.VendorsRoleName);

                _dataProvider.InsertEntity(
                    new PermissionRecordCustomerRoleMapping
                    {
                        CustomerRoleId = vendorRole.Id,
                        PermissionRecordId = multiFactorAuthenticationPermission.Id
                    }
                );
            }

            if (!_dataProvider.GetTable<CustomerRole>().Any(cr => string.Compare(cr.SystemName, "MultiVendors", StringComparison.InvariantCultureIgnoreCase) == 0))
            {
                _dataProvider.InsertEntity(
                new CustomerRole
                {
                    Name = "Multi Vendor",
                    SystemName = "MultiVendors",
                    Active = true,
                });
            }

            if (!_dataProvider.GetTable<PermissionRecord>().Any(pr => string.Compare(pr.SystemName, "ManageMultiVendors", StringComparison.InvariantCultureIgnoreCase) == 0))
            {
                var multiFactorAuthenticationPermission = _dataProvider.InsertEntity(
                    new PermissionRecord
                    {
                        Name = "Admin area. Multi Vendors",
                        SystemName = "ManageMultiVendors",
                        Category = "Customers"
                    });

                var multiVendorRole = _dataProvider
                    .GetTable<CustomerRole>()
                    .FirstOrDefault(x => x.SystemName == NopCustomerDefaults.MultiVendorsRole);

                _dataProvider.InsertEntity(
                    new PermissionRecordCustomerRoleMapping
                    {
                        CustomerRoleId = multiVendorRole.Id,
                        PermissionRecordId = multiFactorAuthenticationPermission.Id
                    });
            }

            #region Vendor

            //add column in vendor table
            var vendorTableName = NameCompatibilityManager.GetTableName(typeof(Vendor));

            //Registation additional fields
            //add column manageDeliverys
            var manageDeliveryColumnName = "ManageDelivery";
            if (!Schema.Table(vendorTableName).Column(manageDeliveryColumnName).Exists())
            {
                Alter.Table(vendorTableName)
                    .AddColumn(manageDeliveryColumnName).AsBoolean().NotNullable().SetExistingRowsTo(0);
            }

            //add column pickAvailable
            var pickAvailableColumnName = "PickAvailable";
            if (!Schema.Table(vendorTableName).Column(pickAvailableColumnName).Exists())
            {
                Alter.Table(vendorTableName)
                    .AddColumn(pickAvailableColumnName).AsBoolean().NotNullable().SetExistingRowsTo(0);
            }

            //add column noAvailable
            var deliveryAvailableColumnName = "DeliveryAvailable";
            if (!Schema.Table(vendorTableName).Column(deliveryAvailableColumnName).Exists())
            {
                Alter.Table(vendorTableName)
                    .AddColumn(deliveryAvailableColumnName).AsBoolean().NotNullable().SetExistingRowsTo(0);
            }

            //GeoCoordinates manage fields.
            //add column geoLocationCoordinates
            var geoLocationCoordinatesColumnName = "GeoLocationCoordinates";
            if (!Schema.Table(vendorTableName).Column(geoLocationCoordinatesColumnName).Exists())
            {
                Alter.Table(vendorTableName)
                    .AddColumn(geoLocationCoordinatesColumnName).AsString(int.MaxValue).Nullable();
            }
            //add column geoFencingCoordinates
            var geoFencingCoordinatesColumnName = "GeoFencingCoordinates";
            if (!Schema.Table(vendorTableName).Column(geoFencingCoordinatesColumnName).Exists())
            {
                Alter.Table(vendorTableName)
                    .AddColumn(geoFencingCoordinatesColumnName).AsString(int.MaxValue).Nullable();
            }

            //add column MinimumOrderAmount
            var minimumOrderAmountColumnName = "MinimumOrderAmount";
            if (!Schema.Table(vendorTableName).Column(minimumOrderAmountColumnName).Exists())
            {
                Alter.Table(vendorTableName)
                    .AddColumn(minimumOrderAmountColumnName).AsDecimal(18, 4).NotNullable().SetExistingRowsTo(0);
            }

            //add column OrderTax
            var orderTaxColumnName = "OrderTax";
            if (!Schema.Table(vendorTableName).Column(orderTaxColumnName).Exists())
            {
                Alter.Table(vendorTableName)
                    .AddColumn(orderTaxColumnName).AsDecimal(18, 4).NotNullable().SetExistingRowsTo(0);
            }

            //add column vendor phone number 
            var vendorPhoneColumnName = "PhoneNumber";
            if (!Schema.Table(vendorTableName).Column(vendorPhoneColumnName).Exists())
            {
                Alter.Table(vendorTableName)
                    .AddColumn(vendorPhoneColumnName).AsString(int.MaxValue).Nullable();
            }

            //set vendor settings defaults (2-8-22)
            //distanceRadiusSetting
            var distanceRadiusSettingName = $"{nameof(VendorSettings)}.{nameof(VendorSettings.DistanceRadiusValue)}";
            if (!_dataProvider.GetTable<Setting>().Any(s => s.StoreId == 0 && string.Compare(s.Name, distanceRadiusSettingName, true) == 0))
            {
                var distanceRadiusSetting = _dataProvider.InsertEntity(
                    new Setting
                    {
                        Name = distanceRadiusSettingName,
                        Value = "60",
                        StoreId = 0
                    });
            }
            //distanceUnitSetting
            var distanceUnitSettingName = $"{nameof(VendorSettings)}.{nameof(VendorSettings.DistanceUnit)}";
            if (!_dataProvider.GetTable<Setting>().Any(s => s.StoreId == 0 && string.Compare(s.Name, distanceUnitSettingName, true) == 0))
            {
                var distanceUnitSetting = _dataProvider.InsertEntity(
                    new Setting
                    {
                        Name = distanceUnitSettingName,
                        Value = DistanceUnit.Mile.ToString(),
                        StoreId = 0
                    });
            }

            //vendor products excel ftp path
            var excelFilePath = $"{nameof(AlchubSettings)}.{nameof(AlchubSettings.ExcelFileFTPPath)}";
            if (!_dataProvider.GetTable<Setting>().Any(s => s.StoreId == 0 && string.Compare(s.Name, excelFilePath, true) == 0))
            {
                var excelFilePathSetting = _dataProvider.InsertEntity(
                    new Setting
                    {
                        Name = excelFilePath,
                        Value = @"D:/Alchub/Vendors/",
                        StoreId = 0
                    });
            }

            //Admin side, add manage vendor permission by default
            var manageVendorPermission = _dataProvider.GetTable<PermissionRecord>().FirstOrDefault(pr => pr.SystemName.Equals("ManageVendors"));
            if (manageVendorPermission != null)
            {
                //add it to the vendor role by default
                var vendorRole = _dataProvider
                    .GetTable<CustomerRole>()
                    .FirstOrDefault(x => x.IsSystemRole && x.SystemName == NopCustomerDefaults.VendorsRoleName);

                //permission record & role mapping
                var mapping = _dataProvider.GetTable<PermissionRecordCustomerRoleMapping>()
                                           .FirstOrDefault(x => x.CustomerRoleId == vendorRole.Id &&
                                           x.PermissionRecordId == manageVendorPermission.Id);
                if (mapping == null)
                {
                    //add if not exist
                    _dataProvider.InsertEntity(
                    new PermissionRecordCustomerRoleMapping
                    {
                        CustomerRoleId = vendorRole.Id,
                        PermissionRecordId = manageVendorPermission.Id
                    });
                }
            }

            //add column pickup address 21-03-23
            var vendorPickupAddressColumnName = "PickupAddress";
            if (!Schema.Table(vendorTableName).Column(vendorPickupAddressColumnName).Exists())
            {
                Alter.Table(vendorTableName)
                    .AddColumn(vendorPickupAddressColumnName).AsString(int.MaxValue).Nullable();
            }

            //add column GeoFenceShapeTypeId & RadiusDistance - 18-05-23
            var geoFenceShapeTypeIdColumnName = nameof(Vendor.GeoFenceShapeTypeId);
            if (!Schema.Table(vendorTableName).Column(geoFenceShapeTypeIdColumnName).Exists())
            {
                Alter.Table(vendorTableName)
                    .AddColumn(geoFenceShapeTypeIdColumnName).AsInt32().NotNullable().SetExistingRowsTo((int)GeoFenceShapeType.Manual);
            }
            var radiusDistanceColumnName = nameof(Vendor.RadiusDistance);
            if (!Schema.Table(vendorTableName).Column(radiusDistanceColumnName).Exists())
            {
                Alter.Table(vendorTableName)
                    .AddColumn(radiusDistanceColumnName).AsDecimal(18, 4).NotNullable().SetExistingRowsTo(0);
            }

            //access vendor create ACL 20-04-23
            var accessVendorCreateSystemName = "AccessVendorCreate";
            if (!_dataProvider.GetTable<PermissionRecord>().Any(pr => string.Compare(pr.SystemName, accessVendorCreateSystemName, StringComparison.InvariantCultureIgnoreCase) == 0))
            {
                var accessVendorCreatePermission = _dataProvider.InsertEntity(
                    new PermissionRecord
                    {
                        Name = "Admin area. Access Vendor Create",
                        SystemName = accessVendorCreateSystemName,
                        Category = "Customers"
                    });

                //add it to the Admin role by default
                var adminRole = _dataProvider
                    .GetTable<CustomerRole>()
                    .FirstOrDefault(x => x.IsSystemRole && x.SystemName == NopCustomerDefaults.AdministratorsRoleName);

                _dataProvider.InsertEntity(
                    new PermissionRecordCustomerRoleMapping
                    {
                        CustomerRoleId = adminRole.Id,
                        PermissionRecordId = accessVendorCreatePermission.Id
                    });
            }

            //product friendly upc ACL 28-06-23
            var aclProductFriendlyUpcSystemName = "ManageProductFriendlyUpc";
            if (!_dataProvider.GetTable<PermissionRecord>().Any(pr => string.Compare(pr.SystemName, aclProductFriendlyUpcSystemName, StringComparison.InvariantCultureIgnoreCase) == 0))
            {
                var alcProductFriendlyUpcPermission = _dataProvider.InsertEntity(
                    new PermissionRecord
                    {
                        Name = "Admin area. Manage Product friendly upc",
                        SystemName = aclProductFriendlyUpcSystemName,
                        Category = "Catalog"
                    });

                //add it to the Admin role by default
                var vendorRole = _dataProvider
                    .GetTable<CustomerRole>()
                    .FirstOrDefault(x => x.IsSystemRole && x.SystemName == NopCustomerDefaults.VendorsRoleName);
                if (vendorRole != null)
                {
                    _dataProvider.InsertEntity(
                        new PermissionRecordCustomerRoleMapping
                        {
                            CustomerRoleId = vendorRole.Id,
                            PermissionRecordId = alcProductFriendlyUpcPermission.Id
                        });
                }
            }

            #region Favorite vendor

            //set favorite vendor settings defaults (05-05-23)
            //ShowStoreAddressInFavoriteSection
            var showStoreAddressInFavoriteSectionSettingName = $"{nameof(VendorSettings)}.{nameof(VendorSettings.ShowStoreAddressInFavoriteSection)}";
            if (!_dataProvider.GetTable<Setting>().Any(s => s.StoreId == 0 && string.Compare(s.Name, showStoreAddressInFavoriteSectionSettingName, true) == 0))
            {
                var showStoreAddressInFavoriteSectionSetting = _dataProvider.InsertEntity(
                    new Setting
                    {
                        Name = showStoreAddressInFavoriteSectionSettingName,
                        Value = "True",
                        StoreId = 0
                    });
            }

            #endregion

            #endregion

            #region Customer

            //add column in customer table
            var customerTableName = NameCompatibilityManager.GetTableName(typeof(Customer));

            //GeoCoordinates manage fields.
            //add column lastSearchedCoordinates
            var lastSearchedCoordinatesColumnName = "LastSearchedCoordinates";
            if (!Schema.Table(customerTableName).Column(lastSearchedCoordinatesColumnName).Exists())
            {
                Alter.Table(customerTableName)
                    .AddColumn(lastSearchedCoordinatesColumnName).AsString(int.MaxValue).Nullable();
            }

            //add column LastSearchedText
            var lastSearchedTextColumnName = "LastSearchedText";
            if (!Schema.Table(customerTableName).Column(lastSearchedTextColumnName).Exists())
            {
                Alter.Table(customerTableName)
                    .AddColumn(lastSearchedTextColumnName).AsString(int.MaxValue).Nullable();
            }

            //add column LastSearchedText
            var isFavoriteToggleOnColumnName = "IsFavoriteToggleOn";
            if (!Schema.Table(customerTableName).Column(isFavoriteToggleOnColumnName).Exists())
            {
                Alter.Table(customerTableName)
                    .AddColumn(isFavoriteToggleOnColumnName).AsBoolean().NotNullable().SetExistingRowsTo(false);
            }

            #endregion

            #region shoppingcart table
            //add column in order table
            var shoppingcartTableName = NameCompatibilityManager.GetTableName(typeof(ShoppingCartItem));
            var slotIdColumnName = "SlotId";

            //add column service fee
            if (!Schema.Table(shoppingcartTableName).Column(slotIdColumnName).Exists())
            {
                Alter.Table(shoppingcartTableName)
                    .AddColumn(slotIdColumnName).AsInt64().NotNullable().SetExistingRowsTo(0);
            }


            var slotPriceColumnName = "SlotPrice";

            //add column service fee
            if (!Schema.Table(shoppingcartTableName).Column(slotPriceColumnName).Exists())
            {
                Alter.Table(shoppingcartTableName)
                    .AddColumn(slotPriceColumnName).AsDecimal(18, 4).NotNullable().SetExistingRowsTo(0);
            }



            var slotStartTimeColumnName = "SlotStartTime";

            //add column service fee
            if (!Schema.Table(shoppingcartTableName).Column(slotStartTimeColumnName).Exists())
            {
                Alter.Table(shoppingcartTableName)
                    .AddColumn(slotStartTimeColumnName).AsDateTime2().NotNullable().SetExistingRowsTo(0);
            }

            var slotEndTimeShoppingcartColumnName = "SlotEndTime";

            //add column service fee
            if (!Schema.Table(shoppingcartTableName).Column(slotEndTimeShoppingcartColumnName).Exists())
            {
                Alter.Table(shoppingcartTableName)
                    .AddColumn(slotEndTimeShoppingcartColumnName).AsDateTime2().NotNullable().SetExistingRowsTo(0);
            }


            var slotTimeColumnName = "SlotTime";

            //add column service fee
            if (!Schema.Table(shoppingcartTableName).Column(slotTimeColumnName).Exists())
            {
                Alter.Table(shoppingcartTableName)
                    .AddColumn(slotTimeColumnName).AsString().NotNullable().SetExistingRowsTo(0);
            }

            //masterProductId
            if (!Schema.Table(shoppingcartTableName).Column(masterProductIdColumnName).Exists())
            {
                Alter.Table(shoppingcartTableName)
                    .AddColumn(masterProductIdColumnName).AsInt32().NotNullable().SetExistingRowsTo(0);
            }

            //groupedProductId
            if (!Schema.Table(shoppingcartTableName).Column(groupedProductIdColumnName).Exists())
            {
                Alter.Table(shoppingcartTableName)
                    .AddColumn(groupedProductIdColumnName).AsInt32().NotNullable().SetExistingRowsTo(0);
            }

            //customAttributesXml
            if (!Schema.Table(shoppingcartTableName).Column(customAttributesXmlColumnName).Exists())
            {
                Alter.Table(shoppingcartTableName)
                    .AddColumn(customAttributesXmlColumnName).AsString(int.MaxValue).Nullable();
            }

            #endregion

            #region CustomerProductSlot

            var customerProductSlotTableName = NameCompatibilityManager.GetTableName(typeof(CustomerProductSlot));
            //Add column in CustomerProductSlot
            var customerProductSlotEndDateTimeColumnName = "EndDateTime";

            //add column EndDateTime
            if (!Schema.Table(customerProductSlotTableName).Column(customerProductSlotEndDateTimeColumnName).Exists())
            {
                Alter.Table(customerProductSlotTableName)
                    .AddColumn(customerProductSlotEndDateTimeColumnName).AsDateTime2().NotNullable().SetExistingRowsTo(0);
            }

            //Add column in CustomerProductSlot
            var customerProductSlotIsSelectedColumnName = "IsSelected";
            //add column EndDateTime
            if (!Schema.Table(customerProductSlotTableName).Column(customerProductSlotIsSelectedColumnName).Exists())
            {
                Alter.Table(customerProductSlotTableName)
                    .AddColumn(customerProductSlotIsSelectedColumnName).AsBoolean().NotNullable().SetExistingRowsTo(0);
            }
            #endregion

            #region Delivery fee

            //add column in order table
            var orderDeliveryFeeColumnName = "DeliveryFee";

            //add column Delivery Fee
            if (!Schema.Table(orderTableName).Column(orderDeliveryFeeColumnName).Exists())
            {
                Alter.Table(orderTableName)
                    .AddColumn(orderDeliveryFeeColumnName).AsDecimal(18, 4).NotNullable().SetExistingRowsTo(0);
            }

            //add column in address table
            var addressTableName = NameCompatibilityManager.GetTableName(typeof(Address));
            var addressGeoLocationColumnName = "GeoLocation";
            var addressGeoLocationCoordinatesColumnName = "GeoLocationCoordinates";
            var addressTypeIdColumnName = "AddressTypeId";

            //add column GeoLocation
            if (!Schema.Table(addressTableName).Column(addressGeoLocationColumnName).Exists())
            {
                Alter.Table(addressTableName)
                    .AddColumn(addressGeoLocationColumnName).AsString();
            }

            //add column GeoLocationCoordinates
            if (!Schema.Table(addressTableName).Column(addressGeoLocationCoordinatesColumnName).Exists())
            {
                Alter.Table(addressTableName)
                    .AddColumn(addressGeoLocationCoordinatesColumnName).AsString();
            }

            //add column addressTypeId
            if (!Schema.Table(addressTableName).Column(addressTypeIdColumnName).Exists())
            {
                Alter.Table(addressTableName)
                    .AddColumn(addressTypeIdColumnName).AsInt32().NotNullable().SetExistingRowsTo(10);
            }

            //Add permission
            if (!_dataProvider.GetTable<PermissionRecord>().Any(pr => string.Compare(pr.SystemName, "ManageDeliveryFee", StringComparison.InvariantCultureIgnoreCase) == 0))
            {
                //Delivery Fee permission record
                var deliveryFeePermission = _dataProvider.InsertEntity(
                    new PermissionRecord
                    {
                        Name = "Admin area. Manage DeliveryFee",
                        SystemName = "ManageDeliveryFee",
                        Category = "Configuration"
                    });

                //add it to the Admin role by default
                var adminRole = _dataProvider
                    .GetTable<CustomerRole>()
                    .FirstOrDefault(x => x.IsSystemRole && x.SystemName == NopCustomerDefaults.AdministratorsRoleName);

                _dataProvider.InsertEntity(
                    new PermissionRecordCustomerRoleMapping
                    {
                        CustomerRoleId = adminRole.Id,
                        PermissionRecordId = deliveryFeePermission.Id
                    }
                );

                //add it to the Vendor role by default
                var vendorRole = _dataProvider
                    .GetTable<CustomerRole>()
                    .FirstOrDefault(x => x.IsSystemRole && x.SystemName == NopCustomerDefaults.VendorsRoleName);

                _dataProvider.InsertEntity(
                    new PermissionRecordCustomerRoleMapping
                    {
                        CustomerRoleId = vendorRole.Id,
                        PermissionRecordId = deliveryFeePermission.Id
                    }
                );
            }

            #endregion Delivery fee

            #region Twillio

            //Add twillio permission
            if (!_dataProvider.GetTable<PermissionRecord>().Any(pr => string.Compare(pr.SystemName, "ManageTwillioSettings", StringComparison.InvariantCultureIgnoreCase) == 0))
            {
                var manageTwillioSettingsPermission = _dataProvider.InsertEntity(
                    new PermissionRecord
                    {
                        Name = "Admin area. Manage Twillio Settings",
                        SystemName = "ManageTwillioSettings",
                        Category = "Configuration"
                    }
                );

                //add it to the Admin role by default
                var adminRole = _dataProvider
                    .GetTable<CustomerRole>()
                    .FirstOrDefault(x => x.IsSystemRole && x.SystemName == NopCustomerDefaults.AdministratorsRoleName);

                _dataProvider.InsertEntity(
                    new PermissionRecordCustomerRoleMapping
                    {
                        CustomerRoleId = adminRole.Id,
                        PermissionRecordId = manageTwillioSettingsPermission.Id
                    }
                );
            }

            //Twilio sms default templates
            //order placed sms template
            var orderPlacedBodySettingName = $"{nameof(TwillioSettings)}.{nameof(TwillioSettings.OrderPlacedBody)}";
            if (!_dataProvider.GetTable<Setting>().Any(s => s.StoreId == 0 && string.Compare(s.Name, orderPlacedBodySettingName, true) == 0))
            {
                var orderPlacedBodySetting = _dataProvider.InsertEntity(
                    new Setting
                    {
                        Name = orderPlacedBodySettingName,
                        Value = $"Order placed: Hi %Order.CustomerFullName%, Your order #%Order.OrderId% at %Store.URL% amounting %Order.OrderTotal% is placed. Click here for more details %Order.OrderURLForCustomer%",
                        StoreId = 0
                    });
            }

            //order dispacthed sms template
            var orderDispatchedBodySettingName = $"{nameof(TwillioSettings)}.{nameof(TwillioSettings.OrderItemsDispatchedBody)}";
            if (!_dataProvider.GetTable<Setting>().Any(s => s.StoreId == 0 && string.Compare(s.Name, orderDispatchedBodySettingName, true) == 0))
            {
                var orderDispatchedBodySetting = _dataProvider.InsertEntity(
                    new Setting
                    {
                        Name = orderDispatchedBodySettingName,
                        Value = $"Dispatched: %OrderItem.Product(s)Name% with order ID #%Order.OrderId% has been dispatched and is out for delivery.",
                        StoreId = 0
                    });
            }

            //order pickup completed sms template
            var orderPickedUpBodySettingName = $"{nameof(TwillioSettings)}.{nameof(TwillioSettings.OrderItemsPickedUpBody)}";
            if (!_dataProvider.GetTable<Setting>().Any(s => s.StoreId == 0 && string.Compare(s.Name, orderPickedUpBodySettingName, true) == 0))
            {
                var orderPickedUpBodySetting = _dataProvider.InsertEntity(
                    new Setting
                    {
                        Name = orderPickedUpBodySettingName,
                        Value = $"Picked Up: %OrderItem.Product(s)Name% with order ID #%Order.OrderId% has been successfully picked up.",
                        StoreId = 0
                    });
            }

            //order delivered sms template
            var orderDeliveredBodySettingName = $"{nameof(TwillioSettings)}.{nameof(TwillioSettings.OrderItemsDeliveredBody)}";
            if (!_dataProvider.GetTable<Setting>().Any(s => s.StoreId == 0 && string.Compare(s.Name, orderDeliveredBodySettingName, true) == 0))
            {
                var orderDeliveredBodySetting = _dataProvider.InsertEntity(
                    new Setting
                    {
                        Name = orderDeliveredBodySettingName,
                        Value = $"Delivered: %OrderItem.Product(s)Name% with order ID #%Order.OrderId% has been successfully delivered.",
                        StoreId = 0
                    });
            }

            //order placed vendor sms template
            var orderPlacedVendorBodySettingName = $"{nameof(TwillioSettings)}.{nameof(TwillioSettings.OrderPlacedVendorBody)}";
            if (!_dataProvider.GetTable<Setting>().Any(s => s.StoreId == 0 && string.Compare(s.Name, orderPlacedVendorBodySettingName, true) == 0))
            {
                var orderPlacedVendorBodySetting = _dataProvider.InsertEntity(
                    new Setting
                    {
                        Name = orderPlacedVendorBodySettingName,
                        Value = $"Order received: Hi %Vendor.Name%, %Store.Name% customer %Order.CustomerFullName% (%Order.CustomerEmail%) has just placed an order. Order Number: %Order.OrderNumber%",
                        StoreId = 0
                    });
            }

            #endregion

            #region Catalog

            //enable fastest slot on product listing
            var enableFastestSlotSettingName = $"{nameof(CatalogSettings)}.{nameof(CatalogSettings.ShowFastestSlotOnCatalogPage)}";
            if (!_dataProvider.GetTable<Setting>().Any(s => s.StoreId == 0 && string.Compare(s.Name, enableFastestSlotSettingName, true) == 0))
            {
                var enableFastestSlotSetting = _dataProvider.InsertEntity(
                    new Setting
                    {
                        Name = enableFastestSlotSettingName,
                        Value = "True",
                        StoreId = 0
                    });
            }

            //number of manufacturers on homepage
            var numberOfManufacturersOnHomeSettingName = $"{nameof(CatalogSettings)}.{nameof(CatalogSettings.NumberOfManufacturersOnHomepage)}";
            if (!_dataProvider.GetTable<Setting>().Any(s => s.StoreId == 0 && string.Compare(s.Name, numberOfManufacturersOnHomeSettingName, true) == 0))
            {
                var numberOfManufacturersOnHomeSetting = _dataProvider.InsertEntity(
                    new Setting
                    {
                        Name = numberOfManufacturersOnHomeSettingName,
                        Value = "10",
                        StoreId = 0
                    });
            }

            #endregion

            #region Order Item Refund

            var orderItemRefundTableName = NameCompatibilityManager.GetTableName(typeof(OrderItemRefund));
            var adminCommissionColumnName = "AdminCommission";
            //Add column DeliverySubtotalExclTax
            if (!Schema.Table(orderItemRefundTableName).Column(adminCommissionColumnName).Exists())
            {
                Alter.Table(orderItemRefundTableName)
                    .AddColumn(adminCommissionColumnName).AsDecimal(18, 4).NotNullable().SetExistingRowsTo(0);
            }

            //order pickup completed sms template
            var orderItemsCancelCustomerBodySettingName = $"{nameof(TwillioSettings)}.{nameof(TwillioSettings.OrderItemsCancelBody)}";
            if (!_dataProvider.GetTable<Setting>().Any(s => s.StoreId == 0 && string.Compare(s.Name, orderItemsCancelCustomerBodySettingName, true) == 0))
            {
                var orderPickedUpBodySetting = _dataProvider.InsertEntity(
                    new Setting
                    {
                        Name = "OrderItemCancelSMS.CustomerNotification",
                        Value = $"Your order placed for order number #%Order.OrderNumber% , %OrderItem.ProductName% has been cancelled and the refund procedure has been initiated.",
                        StoreId = 0
                    });
            }

            //order delivered sms template
            var orderItemsDeliveryDeniedBodySettingName = $"{nameof(TwillioSettings)}.{nameof(TwillioSettings.OrderItemsDelivereyDeniedBody)}";
            if (!_dataProvider.GetTable<Setting>().Any(s => s.StoreId == 0 && string.Compare(s.Name, orderItemsDeliveryDeniedBodySettingName, true) == 0))
            {
                var orderDeliveredBodySetting = _dataProvider.InsertEntity(
                    new Setting
                    {
                        Name = "OrderItemDelivereyDeniedSMS.CustomerNotification",
                        Value = $"The ordernumber #%Order.OrderNumber% for %OrderItem.Product(s)% could not be delivered as you were unable to fulfil the delivery conditions. Thus, your order is being cancelled.",
                        StoreId = 0
                    });
            }

            //order Item Pickup sms template
            var orderItempickupBodySettingName = $"{nameof(TwillioSettings)}.{nameof(TwillioSettings.OrderItemPickupBody)}";
            if (!_dataProvider.GetTable<Setting>().Any(s => s.StoreId == 0 && string.Compare(s.Name, orderItempickupBodySettingName, true) == 0))
            {
                var orderItempickupBodySetting = _dataProvider.InsertEntity(
                    new Setting
                    {
                        Name = "OrderItemPickupSMS.CustomerNotification",
                        Value = $"Hi, %Customer.FullName%, We waited for you to pickup the order  but unfortunately you were not able to pickup so we are cancelling the order.",
                        StoreId = 0
                    });
            }

            // add message template (orderItems dispacthed)
            if (!_dataProvider.GetTable<MessageTemplate>().Any(pr => string.Compare(pr.Name, MessageTemplateSystemNames.OrderItemPickupNotificationsCustomer, true) == 0))
            {
                var messageTemplate = _dataProvider.InsertEntity(
                    new MessageTemplate
                    {
                        Name = MessageTemplateSystemNames.OrderItemPickupNotificationsCustomer,
                        Subject = "Products of your order from %Store.Name%.",
                        Body = $"Hi, %Customer.FullName%, {Environment.NewLine}<br /> {Environment.NewLine}<br /> We waited for you to pickup the order  but unfortunately you were not able to pickup so we are cancelling the order.",
                        IsActive = true,
                        EmailAccountId = _dataProvider.GetTable<EmailAccount>().FirstOrDefault()?.Id ?? 0
                    }
                );
            }

            // add message template (orderItems cancel )
            if (!_dataProvider.GetTable<MessageTemplate>().Any(pr => string.Compare(pr.Name, MessageTemplateSystemNames.OrderItemCancelNotificationsCustomer, true) == 0))
            {
                var messageTemplate = _dataProvider.InsertEntity(
                    new MessageTemplate
                    {
                        Name = MessageTemplateSystemNames.OrderItemCancelNotificationsCustomer,
                        Subject = "Product of your order from %Store.Name% has been cancel.",
                        Body = $"<p>{Environment.NewLine}Hi, {Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}Your cancellation request for the Order Number: %Order.OrderNumber%,{Environment.NewLine}<br />{Environment.NewLine}for %OrderItem.ProductName% has been submitted successfully{Environment.NewLine}<br />{Environment.NewLine}{Environment.NewLine}<br />{Environment.NewLine}and the payment refund procedure for %OrderItem.RefundAmount% has been initiated.{Environment.NewLine}<br />",
                        IsActive = true,
                        EmailAccountId = _dataProvider.GetTable<EmailAccount>().FirstOrDefault()?.Id ?? 0
                    }
                );
            }

            // add message template (orderItems dispacthed)
            if (!_dataProvider.GetTable<MessageTemplate>().Any(pr => string.Compare(pr.Name, MessageTemplateSystemNames.OrderItemCancelNotificationsVendor, true) == 0))
            {
                var messageTemplate = _dataProvider.InsertEntity(
                    new MessageTemplate
                    {
                        Name = MessageTemplateSystemNames.OrderItemCancelNotificationsVendor,
                        Subject = "Product of your order from %Store.Name% has been cancel.",
                        Body = $"<p>{Environment.NewLine}Hi, {Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}The order placed for Order Number: %Order.OrderNumber%,{Environment.NewLine}<br />{Environment.NewLine}for %OrderItem.ProductName% has been cancelled.{Environment.NewLine}<br />{Environment.NewLine}{Environment.NewLine}<br />",
                        IsActive = true,
                        EmailAccountId = _dataProvider.GetTable<EmailAccount>().FirstOrDefault()?.Id ?? 0
                    }
                );
            }

            // add message template (orderItems dispacthed)
            if (!_dataProvider.GetTable<MessageTemplate>().Any(pr => string.Compare(pr.Name, MessageTemplateSystemNames.OrderItemDeliveryDeniedNotificationsCustomer, true) == 0))
            {
                var messageTemplate = _dataProvider.InsertEntity(
                    new MessageTemplate
                    {
                        Name = MessageTemplateSystemNames.OrderItemDeliveryDeniedNotificationsCustomer,
                        Subject = "Product of your order from %Store.Name% has been delivery denied.",
                        Body = $"<p>{Environment.NewLine}Hi,</a>{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}your order Number: %Order.OrderNumber%,{Environment.NewLine}<br />{Environment.NewLine}for %OrderItem.ProductName% could not be delivered because the recipient was unable to fulfil the delivery conditions. Thus, the cancellation request is being generated against the same.{Environment.NewLine}<br />{Environment.NewLine}{Environment.NewLine}<br />",
                        IsActive = true,
                        EmailAccountId = _dataProvider.GetTable<EmailAccount>().FirstOrDefault()?.Id ?? 0
                    }
                );
            }

            // add message template (orderItems dispacthed)
            if (!_dataProvider.GetTable<MessageTemplate>().Any(pr => string.Compare(pr.Name, MessageTemplateSystemNames.OrderItemDeliveryDeniedNotificationsVendor, true) == 0))
            {
                var messageTemplate = _dataProvider.InsertEntity(
                    new MessageTemplate
                    {
                        Name = MessageTemplateSystemNames.OrderItemDeliveryDeniedNotificationsVendor,
                        Subject = "Products of your order from %Store.Name% has been delivery denied.",
                        Body = $"<p>{Environment.NewLine}Hi,</a>{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}The order Number: %Order.OrderNumber%,{Environment.NewLine}<br />{Environment.NewLine}for the %OrderItem.ProductName% could not be delivered as the customer could not fulfil the delivery conditions. Thus, the cancellation request is being generated against the same.{Environment.NewLine}<br />{Environment.NewLine}{Environment.NewLine}<br />",
                        IsActive = true,
                        EmailAccountId = _dataProvider.GetTable<EmailAccount>().FirstOrDefault()?.Id ?? 0
                    }
                );
            }
            #endregion

            #region Topic page

            var addNewProductVendorInstructTopicSysName = NopAlchubDefaults.TOPIC_ADD_NEW_PRODUCT_VENDOR_SYS_NAME;
            if (!_dataProvider.GetTable<Topic>().Any(s => string.Compare(s.SystemName, addNewProductVendorInstructTopicSysName, true) == 0))
            {
                var defaultTopicTemplate =
               _dataProvider.GetTable<TopicTemplate>().FirstOrDefault(tt => tt.Name == "Default template");

                var addNewProductVendorInstructTopic = _dataProvider.InsertEntity(
                    new Topic
                    {
                        SystemName = addNewProductVendorInstructTopicSysName,
                        IncludeInSitemap = false,
                        IsPasswordProtected = false,
                        IncludeInFooterColumn1 = false,
                        DisplayOrder = 50,
                        Published = true,
                        Title = "Add new product Vednor instruction",
                        Body =
                        "<p>Put your &quot;Add new product Vednor instruction&quot; information here. You can edit this in the admin site.</p>",
                        TopicTemplateId = defaultTopicTemplate?.Id ?? 1
                    });

                //search engine names
                var addProductUrlRecord = InsertInstallationDataAsync(new UrlRecord
                {
                    EntityId = addNewProductVendorInstructTopic.Id,
                    EntityName = nameof(Topic),
                    LanguageId = 0,
                    IsActive = true,
                    Slug = ValidateSeNameAsync(addNewProductVendorInstructTopic, !string.IsNullOrEmpty(addNewProductVendorInstructTopic.Title) ? addNewProductVendorInstructTopic.Title : addNewProductVendorInstructTopic.SystemName)
                });
            }

            #endregion

            #region Common

            //set andoid and ios app link settings defaults (20-02-23)
            var androidAppLinkSettingName = $"{nameof(AlchubSettings)}.{nameof(AlchubSettings.AndroidAppLink)}";
            if (!_dataProvider.GetTable<Setting>().Any(s => s.StoreId == 0 && string.Compare(s.Name, androidAppLinkSettingName, true) == 0))
            {
                _dataProvider.InsertEntity(
                new Setting
                {
                    Name = androidAppLinkSettingName,
                    Value = "https://play.google.com/store/apps/details?id=com.alchub.llc",
                    StoreId = 0
                });
            }
            var iosAppLinkSettingName = $"{nameof(AlchubSettings)}.{nameof(AlchubSettings.IOSAppLink)}";
            if (!_dataProvider.GetTable<Setting>().Any(s => s.StoreId == 0 && string.Compare(s.Name, iosAppLinkSettingName, true) == 0))
            {
                _dataProvider.InsertEntity(
                new Setting
                {
                    Name = iosAppLinkSettingName,
                    Value = "https://apps.apple.com/us/app/alchub-alcohol-delivery/id1660914907",
                    StoreId = 0
                });
            }

            #endregion

            #region Scheduler task

            //Add schedule task for sync vendor products
            if (!_dataProvider.GetTable<ScheduleTask>().Any(st => string.Compare(st.Type, "Nop.Services.Alchub.Vendors.SyncExcelProductsTask", StringComparison.InvariantCultureIgnoreCase) == 0))
            {
                _dataProvider.InsertEntity(
                 new ScheduleTask
                 {
                     Enabled = true,
                     LastEnabledUtc = DateTime.UtcNow,
                     Seconds = 5 * 60,
                     Name = "Sync Vendor Products",
                     Type = "Nop.Services.Alchub.Vendors.SyncExcelProductsTask",
                     StopOnError = false
                 });
            }

            //Add schedule task for api all filter cache optimization
            if (!_dataProvider.GetTable<ScheduleTask>().Any(st => string.Compare(st.Type, NopAlchubDefaults.API_ALL_FILTER_CACHE_OPTIMIZATION_SCHEDULE_TASK_TYPE, StringComparison.InvariantCultureIgnoreCase) == 0))
            {
                //schedule task record
                _dataProvider.InsertEntity(
                 new ScheduleTask
                 {
                     Enabled = true,
                     LastEnabledUtc = DateTime.UtcNow,
                     Seconds = (2 * 60) * 60, //2h
                     Name = NopAlchubDefaults.API_ALL_FILTER_CACHE_OPTIMIZATION_SCHEDULE_TASK_NAME,
                     Type = NopAlchubDefaults.API_ALL_FILTER_CACHE_OPTIMIZATION_SCHEDULE_TASK_TYPE,
                     StopOnError = false
                 });
            }

            if (!_dataProvider.GetTable<ScheduleTask>().Any(st => string.Compare(st.Type, "Nop.Services.Alchub.Catalog.SyncSameNameProductsTask", StringComparison.InvariantCultureIgnoreCase) == 0))
            {
                _dataProvider.InsertEntity(
                 new ScheduleTask
                 {
                     Enabled = false,
                     LastEnabledUtc = DateTime.UtcNow,
                     Seconds = 5 * 60,
                     Name = "Sync Same Product Name Create Group Product",
                     Type = "Nop.Services.Alchub.Catalog.SyncSameNameProductsTask",
                     StopOnError = false
                 });
            }

            // For Product Image Sync
            if (!_dataProvider.GetTable<ScheduleTask>().Any(st => string.Compare(st.Type, "Nop.Services.Alchub.Catalog.SyncProductsImagesTask", StringComparison.InvariantCultureIgnoreCase) == 0))
            {
                _dataProvider.InsertEntity(
                 new ScheduleTask
                 {
                     Enabled = false,
                     LastEnabledUtc = DateTime.UtcNow,
                     Seconds = (24 * 60 * 60) * 7, // for week in seconds
                     Name = "Sync Product Images",
                     Type = "Nop.Services.Alchub.Catalog.SyncProductsImagesTask",
                     StopOnError = false
                 });
            }

            if (!_dataProvider.GetTable<ScheduleTask>().Any(st => string.Compare(st.Type, "Nop.Services.Alchub.Catalog.SyncReArrangeSameNameProductsTask", StringComparison.InvariantCultureIgnoreCase) == 0))
            {
                _dataProvider.InsertEntity(
                 new ScheduleTask
                 {
                     Enabled = false,
                     LastEnabledUtc = DateTime.UtcNow,
                     Seconds = 5 * 60,
                     Name = "Sync Re Arrange Size and Containers Same Products Name",
                     Type = "Nop.Services.Alchub.Catalog.SyncReArrangeSameNameProductsTask",
                     StopOnError = false
                 });
            }

            // For Product in Elastic index
            if (!_dataProvider.GetTable<ScheduleTask>().Any(st => string.Compare(st.Type, "Nop.Services.Alchub.ElasticSearch.SyncProductsInElasticTask", StringComparison.InvariantCultureIgnoreCase) == 0))
            {
                _dataProvider.InsertEntity(
                 new ScheduleTask
                 {
                     Enabled = false,
                     LastEnabledUtc = DateTime.UtcNow,
                     Seconds = (24 * 60 * 60), // for week in seconds
                     Name = "Sync Product in Elastic Index",
                     Type = "Nop.Services.Alchub.ElasticSearch.SyncProductsInElasticTask",
                     StopOnError = false
                 });
            }
            #endregion

            //Note: Plz add any new table migration in bloved region. also if there's any new column need to be added in new created table, that will also gose after create table code.
            #region Schema migration

            #region Zone

            //create zone table if not exist
            if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(Zone))).Exists())
                Create.TableFor<Zone>();

            var zoneTableName = NameCompatibilityManager.GetTableName(typeof(Zone));
            var soltIdzoneTableName = "CreatedBy";
            //add column CreatedBy
            if (!Schema.Table(zoneTableName).Column(soltIdzoneTableName).Exists())
            {
                Alter.Table(zoneTableName)
                    .AddColumn(soltIdzoneTableName).AsInt64().NotNullable().SetExistingRowsTo(0);
            }

            #endregion

            //create slot table if not exist
            if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(SlotCategory))).Exists())
                Create.TableFor<SlotCategory>();

            //create slot table if not exist
            if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(Slot))).Exists())
                Create.TableFor<Slot>();

            //create CustomerOrderSlot table if not exist
            if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(CustomerOrderSlot))).Exists())
                Create.TableFor<CustomerOrderSlot>();

            //create CategoryMarkup table if not exist
            if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(CategoryMarkup))).Exists())
                Create.TableFor<CategoryMarkup>();

            //create CustomerProductSlot table if not exist
            if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(CustomerProductSlot))).Exists())
                Create.TableFor<CustomerProductSlot>();

            //create DeliveryFee table if not exist
            if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(DeliveryFee))).Exists())
                Create.TableFor<DeliveryFee>();

            //create OrderDeliveryFee table if not exist
            if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(OrderDeliveryFee))).Exists())
                Create.TableFor<OrderDeliveryFee>();

            //create ManagerVendorMapping table if not exist
            if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(ManagerVendorMapping))).Exists())
                Create.TableFor<ManagerVendorMapping>();

            //create OrderItemRefund table if not exist
            if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(OrderItemRefund))).Exists())
                Create.TableFor<OrderItemRefund>();

            //create VendorTiming table if not exist
            if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(VendorTiming))).Exists())
                Create.TableFor<VendorTiming>();

            //create FavoriteVendor table if not exist
            if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(FavoriteVendor))).Exists())
                Create.TableFor<FavoriteVendor>();

            //create ProductFriendlyUpcCode table if not exist
            if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(ProductFriendlyUpcCode))).Exists())
                Create.TableFor<ProductFriendlyUpcCode>();

            #endregion

            #region Door Dash Dispatch

            //create dispatch table if not exist
            if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(Dispatch))).Exists())
                Create.TableFor<Dispatch>();


            //add settings 

            //Door dash developer Id
            var alchubDoorDashDeveloperId = $"{nameof(AlchubSettings)}.{nameof(AlchubSettings.AlchubDoorDashDeveloperId)}";
            if (!_dataProvider.GetTable<Setting>().Any(s => s.StoreId == 0 && string.Compare(s.Name, alchubDoorDashDeveloperId, true) == 0))
            {
                var alchubDoorDashDeveloper = _dataProvider.InsertEntity(
                    new Setting
                    {
                        Name = alchubDoorDashDeveloperId,
                        Value = "1cf4877c-df87-4277-a77a-c2bc512e1473",
                        StoreId = 0
                    });
            }
            //alchub Alchub Door Dash Key Id
            var alchubDoorDashKeyId = $"{nameof(AlchubSettings)}.{nameof(AlchubSettings.AlchubDoorDashKeyId)}";
            if (!_dataProvider.GetTable<Setting>().Any(s => s.StoreId == 0 && string.Compare(s.Name, alchubDoorDashKeyId, true) == 0))
            {
                var alchubDoorDashKey = _dataProvider.InsertEntity(
                    new Setting
                    {
                        Name = alchubDoorDashKeyId,
                        Value = "aae1c270-67e5-42c1-a3fe-b0795cf6d4b4",
                        StoreId = 0
                    });
            }

            //Door Dash Signing Secret
            var alchubDoorDashSigningSecret = $"{nameof(AlchubSettings)}.{nameof(AlchubSettings.AlchubDoorDashSigningSecret)}";
            if (!_dataProvider.GetTable<Setting>().Any(s => s.StoreId == 0 && string.Compare(s.Name, alchubDoorDashSigningSecret, true) == 0))
            {
                var alchubDoorDashSigningKey = _dataProvider.InsertEntity(
                    new Setting
                    {
                        Name = alchubDoorDashSigningSecret,
                        Value = "k4-ruAzlEyh5KKrri7bUOvmeE-VzJ8nLDrNfAOlQqOk",
                        StoreId = 0
                    });
            }
            //Alchub Door Dash External Delivery Add Seconds
            var alchubDoorDashExternalDeliveryAddSeconds = $"{nameof(AlchubSettings)}.{nameof(AlchubSettings.AlchubDoorDashExternalDeliveryAddSeconds)}";
            if (!_dataProvider.GetTable<Setting>().Any(s => s.StoreId == 0 && string.Compare(s.Name, alchubDoorDashExternalDeliveryAddSeconds, true) == 0))
            {
                var alchubDoorDashExternalDeliverySeconds = _dataProvider.InsertEntity(
                    new Setting
                    {
                        Name = alchubDoorDashExternalDeliveryAddSeconds,
                        Value = "60",
                        StoreId = 0
                    });
            }
            #endregion

            #region VedorAttributes

            //add doorDashPickupExternalBusinessId
            var doorDashPickupExternalBusinessId = AlchubVendorDefaults.DoorDashExternalBusinessId;
            if (!_dataProvider.GetTable<VendorAttribute>().Any(s => string.Compare(s.Name, doorDashPickupExternalBusinessId, true) == 0))
            {
                var pickupExternalBusinessId = _dataProvider.InsertEntity(
                    new VendorAttribute
                    {
                        Name = doorDashPickupExternalBusinessId,
                        IsRequired = false,
                        AttributeControlType = AttributeControlType.TextBox,
                        AttributeControlTypeId = (int)AttributeControlType.TextBox,
                        DisplayOrder = 1,
                    });
            }

            //add doorDashPickupExternalBusinessId
            var doorDashPickupExternalStoreId = AlchubVendorDefaults.DoorDashExternalStoreId;
            if (!_dataProvider.GetTable<VendorAttribute>().Any(s => string.Compare(s.Name, doorDashPickupExternalStoreId, true) == 0))
            {
                var pickupExternalStoreId = _dataProvider.InsertEntity(
                    new VendorAttribute
                    {
                        Name = doorDashPickupExternalStoreId,
                        IsRequired = false,
                        AttributeControlType = AttributeControlType.TextBox,
                        AttributeControlTypeId = (int)AttributeControlType.TextBox,
                        DisplayOrder = 2,
                    });
            }

            #endregion

            #region Custom SQL commands

            //create SP and Functions
            //Note: Commenting as we'll execure upToDate SP script manually.
            //var task = _dataProvider.ExecuteSqlFileScriptsOnDatabase();
            //task.WaitAndUnwrapException();

            #endregion

        }

        public override void Down()
        {
            //add the downgrade logic if necessary 
        }

        #region Utilities

        /// <returns>A task that represents the asynchronous operation</returns>
        private T InsertInstallationDataAsync<T>(T entity) where T : BaseEntity
        {
            return _dataProvider.InsertEntity(entity);
        }

        /// <returns>A task that represents the asynchronous operation</returns>
        private string ValidateSeNameAsync<T>(T entity, string seName) where T : BaseEntity
        {
            //duplicate of ValidateSeName method of \Nop.Services\Seo\UrlRecordService.cs (we cannot inject it here)
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            //validation
            var okChars = "abcdefghijklmnopqrstuvwxyz1234567890 _-";
            seName = seName.Trim().ToLowerInvariant();

            var sb = new StringBuilder();
            foreach (var c in seName.ToCharArray())
            {
                var c2 = c.ToString();
                if (okChars.Contains(c2))
                    sb.Append(c2);
            }

            seName = sb.ToString();
            seName = seName.Replace(" ", "-");
            while (seName.Contains("--"))
                seName = seName.Replace("--", "-");
            while (seName.Contains("__"))
                seName = seName.Replace("__", "_");

            //max length
            seName = CommonHelper.EnsureMaximumLength(seName, 200);

            //ensure this sename is not reserved yet
            var i = 2;
            var tempSeName = seName;
            while (true)
            {
                //check whether such slug already exists (and that is not the current entity)

                var query = from ur in _dataProvider.GetTable<UrlRecord>()
                            where tempSeName != null && ur.Slug == tempSeName
                            select ur;
                var urlRecord = query.FirstOrDefault();

                var entityName = entity.GetType().Name;
                var reserved = urlRecord != null && !(urlRecord.EntityId == entity.Id && urlRecord.EntityName.Equals(entityName, StringComparison.InvariantCultureIgnoreCase));
                if (!reserved)
                    break;

                tempSeName = $"{seName}-{i}";
                i++;
            }

            seName = tempSeName;

            return seName;
        }

        #endregion
    }
}
