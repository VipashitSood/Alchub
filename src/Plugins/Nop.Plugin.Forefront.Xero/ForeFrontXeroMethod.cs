using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.ScheduleTasks;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Services.ScheduleTasks;

namespace Nop.Plugin.Forefront.Xero
{
    public class ForeFrontXeroMethod : BasePlugin, IMiscPlugin, IPlugin
    {
        #region Fields

        private readonly IWebHelper _webHelper;
        private readonly IScheduleTaskService _scheduleTaskService;
        private readonly ForeFrontXeroSetting _tmotionsXeroSetting;
        private readonly ISettingService _settingService;
        private readonly INopFileProvider _nopfileProvider;
        private readonly ILocalizationService _localizationService;
        private readonly INopDataProvider _nopDataProvider;
        private readonly IStoreContext _storeContext;

        #endregion

        #region Ctor

        public ForeFrontXeroMethod(IWebHelper webHelper, 
            IScheduleTaskService scheduleTaskService,
            ForeFrontXeroSetting tmotionsXeroSetting,
            ISettingService settingService,
            INopFileProvider nopfileProvider,
            ILocalizationService localizationService,
            INopDataProvider nopDataProvider,
            IStoreContext storeContext)
        {
            _webHelper = webHelper;
            _scheduleTaskService = scheduleTaskService;
            _tmotionsXeroSetting = tmotionsXeroSetting;
            _settingService = settingService;
            _nopfileProvider = nopfileProvider;
            _localizationService = localizationService;
            _nopDataProvider = nopDataProvider;
            _storeContext = storeContext;
        }

        #endregion

        #region Methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        protected virtual void ExecuteSqlFile(string path)
        {
            var list = new List<string>();
            var fileStream = File.OpenRead(path);

            try
            {
                var streamReader = new StreamReader(fileStream);
                try
                {
                    while (true)
                    {
                        string str = ReadNextStatementFromStream(streamReader);
                        if (str == null)
                        {
                            break;
                        }

                        list.Add(str);
                    }
                }
                finally
                {
                    if (streamReader != null)
                    {
                        streamReader.Dispose();
                    }
                }
            }
            finally
            {
                if (fileStream != null)
                {
                    fileStream.Dispose();
                }
            }

            foreach (string str2 in list)
            {
                if ((string.IsNullOrEmpty(str2) ? false : !string.IsNullOrWhiteSpace(str2)))
                {
                    //int? nullable = null;
                    _nopDataProvider.ExecuteNonQueryAsync(str2);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
		public override string GetConfigurationPageUrl()
        {
            bool? nullable = null;
            string str = string.Format("{0}Admin/ForefrontXero/MiscConfigure", _webHelper.GetStoreLocation(nullable));
            return str;
        }

        /// <summary>
        /// 
        /// </summary>
		public override async Task InstallAsync()
        {
            int activeStoreScopeConfiguration =  _storeContext.GetCurrentStore().Id;
            await _settingService.SaveSettingAsync(new ForeFrontXeroSetting()
            {
                Enable = true,
                IsDropTable = true,
                IsInventorySyncEnabled = true,
                BatchSize = 100,
                AuthorizedURL = "https://login.xero.com/identity/connect/authorize",
                AuthorizedResponseType = "code",
                AuthorizedState = "123",
                TokenURL = "https://identity.xero.com/connect/token",
                TokenGrantType = "code",
                Scope = "openid profile email files accounting.transactions accounting.transactions.read accounting.reports.read accounting.journals.read accounting.settings accounting.settings.read accounting.contacts accounting.contacts.read accounting.attachments accounting.attachments.read offline_access",
                ConnectionsURL = "https://api.xero.com/connections"
            }, activeStoreScopeConfiguration);


            if (await _scheduleTaskService.GetTaskByTypeAsync("Nop.Plugin.Forefront.Xero.ScheduleTasks.XeroIntegration") == null)
            {
                var scheduleTaskXeroIntegration = new ScheduleTask();
                scheduleTaskXeroIntegration.Name = "Xero Integration";
                scheduleTaskXeroIntegration.Type = "Nop.Plugin.Forefront.Xero.ScheduleTasks.XeroIntegration";
                scheduleTaskXeroIntegration.Seconds = 600;
                scheduleTaskXeroIntegration.Enabled = true;
                scheduleTaskXeroIntegration.LastEnabledUtc = DateTime.UtcNow;
                await _scheduleTaskService.InsertTaskAsync(scheduleTaskXeroIntegration);
            }

            if (await _scheduleTaskService.GetTaskByTypeAsync("Nop.Plugin.Forefront.Xero.ScheduleTasks.XeroInventoryIntegration") == null)
            {
                var scheduleTaskXeroInventoryIntegration = new ScheduleTask();
                scheduleTaskXeroInventoryIntegration.Name = "Xero Inventory Integration";
                scheduleTaskXeroInventoryIntegration.Seconds = 600;
                scheduleTaskXeroInventoryIntegration.Type = "Nop.Plugin.Forefront.Xero.ScheduleTasks.XeroInventoryIntegration";
                scheduleTaskXeroInventoryIntegration.Enabled = true;
                scheduleTaskXeroInventoryIntegration.LastEnabledUtc = DateTime.UtcNow;
                await _scheduleTaskService.InsertTaskAsync(scheduleTaskXeroInventoryIntegration);
            }

           
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugin.Forefront.Xero.Fields.Tmotions.Xero", "Tmotions-Xero", null);
             await _localizationService.AddOrUpdateLocaleResourceAsync("Plugin.Forefront.Xero.Fields.Enable", "Enable", null);
             await _localizationService.AddOrUpdateLocaleResourceAsync("Plugin.Forefront.Xero.Fields.Enable.Hint", "Check to enable.", null);
             await _localizationService.AddOrUpdateLocaleResourceAsync("Plugin.Forefront.Xero.Fields.IsInventorySyncEnabled.Hint", "Check to IsInventorySyncEnabled.", null);
             await _localizationService.AddOrUpdateLocaleResourceAsync("Plugin.Forefront.Xero.Fields.IsInventorySyncEnabled", "Is Quantity Sync Enabled", null);
             await _localizationService.AddOrUpdateLocaleResourceAsync("Plugin.Forefront.Xero.Fields.PostingBatchSize", "Batch size", null);
             await _localizationService.AddOrUpdateLocaleResourceAsync("Plugin.Forefront.Xero.Fields.PostingBatchSize.Hint", "Please enter batch size", null);
             await _localizationService.AddOrUpdateLocaleResourceAsync("Plugin.Forefront.Xero.Info", "Configuration", null);
             await _localizationService.AddOrUpdateLocaleResourceAsync("Plugin.Forefront.Xero.XeroQueue", "Xero Queue", null);
             await _localizationService.AddOrUpdateLocaleResourceAsync("Plugin.Forefront.Xero.Field.OrderId", "Order", null);
             await _localizationService.AddOrUpdateLocaleResourceAsync("Plugin.Forefront.Xero.Field.QueuedOn", "Queued On", null);
             await _localizationService.AddOrUpdateLocaleResourceAsync("Plugin.Forefront.Xero.Field.SyncAttemptOn", "Sync Attempt", null);
             await _localizationService.AddOrUpdateLocaleResourceAsync("Plugin.Forefront.Xero.Field.Amount", "Amount", null);
             await _localizationService.AddOrUpdateLocaleResourceAsync("Plugin.Forefront.Xero.Field.IsSuccess", "Status", null);
             await _localizationService.AddOrUpdateLocaleResourceAsync("Plugin.Forefront.Xero.Field.ResponseMessages", "Messages", null);
             await _localizationService.AddOrUpdateLocaleResourceAsync("Plugin.Forefront.Xero.accountliability", "Payment Accounts Map", null);
             await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.Plugins.Account.saved", "Payment Accounts updated successfully", null);
             await _localizationService.AddOrUpdateLocaleResourceAsync("Plugin.Forefront.Xero.Fields.IsDropTable", "Drop Table", null);
             await _localizationService.AddOrUpdateLocaleResourceAsync("Plugin.Forefront.Xero.Fields.IsDropTable.Hint", "Please check to drop table while uninstalling plugin", null);
             await _localizationService.AddOrUpdateLocaleResourceAsync("Plugin.Forefront.Xero.Fields.RestrictToCount", "Restrict To Count", null);
             await _localizationService.AddOrUpdateLocaleResourceAsync("Plugin.Forefront.Xero.Fields.RestrictToCount.Hint", "No. of Api call made on Today", null);
             await _localizationService.AddOrUpdateLocaleResourceAsync("Plugin.Forefront.Xero.Field.ActionType", "Action Type", null);
             await _localizationService.AddOrUpdateLocaleResourceAsync("Plugin.Forefront.Xero.XeroProduct", "Xero Product", null);
             await _localizationService.AddOrUpdateLocaleResourceAsync("Plugin.Forefront.Xero.Product.Fields.Id", "Id", null);
             await _localizationService.AddOrUpdateLocaleResourceAsync("Plugin.Forefront.Xero.Product.Fields.ProductId", "Product Id", null);
             await _localizationService.AddOrUpdateLocaleResourceAsync("Plugin.Forefront.Xero.Product.Fields.XeroStatus", "Product Status", null);
             await _localizationService.AddOrUpdateLocaleResourceAsync("Plugin.Forefront.Xero.Product.Fields.IsDeleted", "Is Deleted", null);
             await _localizationService.AddOrUpdateLocaleResourceAsync("Plugin.Forefront.Xero.Product.Fields.ActionType", "Action Type", null);
             await _localizationService.AddOrUpdateLocaleResourceAsync("Plugin.Forefront.Xero.Title", "Xero Accounting", null);
             await _localizationService.AddOrUpdateLocaleResourceAsync("Plugin.Forefront.Xero.DomainMismatch", "Your domain is missmatched with this license keys", null);
             await _localizationService.AddOrUpdateLocaleResourceAsync("Plugin.Forefront.Xero.LicenseExpire", "Your Xero plugin is expire. Please contact to nopCommerceplus at 'info@nopCommerceplus.com'", null);
             await _localizationService.AddOrUpdateLocaleResourceAsync("Plugin.Forefront.Xero.InvalidLicense", "Invalid license keys. Please check your license keys.", null);
             await _localizationService.AddOrUpdateLocaleResourceAsync("Plugin.Forefront.Xero.TrialMode", "Your plugin is in trial mode", null);
             await _localizationService.AddOrUpdateLocaleResourceAsync("Plugin.Forefront.Xero.Fields.ClientId", "Client Id", null);
             await _localizationService.AddOrUpdateLocaleResourceAsync("Plugin.Forefront.Xero.Fields.ClientId.Hint", "Please enter Client Id", null);
             await _localizationService.AddOrUpdateLocaleResourceAsync("Plugin.Forefront.Xero.Fields.ClientSecret", "Client Secret", null);
             await _localizationService.AddOrUpdateLocaleResourceAsync("Plugin.Forefront.Xero.Fields.ClientSecret.Hint", "Please enter Client Secret key", null);
             await _localizationService.AddOrUpdateLocaleResourceAsync("Plugin.Forefront.Xero.Fields.CallbackURL", "Callback URL", null);
             await _localizationService.AddOrUpdateLocaleResourceAsync("Plugin.Forefront.Xero.Fields.CallbackURL.Hint", "Please enter Callback URL", null);
             await _localizationService.AddOrUpdateLocaleResourceAsync("Plugin.Forefront.Xero.Fields.ConnectToXero", "Connect", null);
             await _localizationService.AddOrUpdateLocaleResourceAsync("Plugin.Forefront.Xero.Fields.ConnectToXero.Hint", "Please make connection with Xero", null);
             await _localizationService.AddOrUpdateLocaleResourceAsync("Plugin.Forefront.Xero.Fields.SalesAccountCode", "Sales Account Code", null);
             await _localizationService.AddOrUpdateLocaleResourceAsync("Plugin.Forefront.Xero.Fields.SalesAccountCode.Hint", "Please enter sales Account Code", null);
             await _localizationService.AddOrUpdateLocaleResourceAsync("Plugin.Forefront.Xero.Fields.NonVatAccountCode", "Non Vat Account Code", null);
             await _localizationService.AddOrUpdateLocaleResourceAsync("Plugin.Forefront.Xero.Fields.NonVatAccountCode.Hint", "Please enter Non Vat Account Code", null);
             await _localizationService.AddOrUpdateLocaleResourceAsync("Plugin.Forefront.Xero.Fields.PurchaseAccountCode", "Purchase Account Code", null);
             await _localizationService.AddOrUpdateLocaleResourceAsync("Plugin.Forefront.Xero.Fields.PurchaseAccountCode.Hint", "Please enter Purchase Account Code", null);
             await _localizationService.AddOrUpdateLocaleResourceAsync("Plugin.Forefront.Xero.Fields.PostageAccountCode", "Postage Account Code", null);
             await _localizationService.AddOrUpdateLocaleResourceAsync("Plugin.Forefront.Xero.Fields.PostageAccountCode.Hint", "Please enter Postage Account Code", null);
             await _localizationService.AddOrUpdateLocaleResourceAsync("Plugin.Forefront.Xero.Fields.DiscountAmountAccountCode", "Discount Amount Account Code", null);
             await _localizationService.AddOrUpdateLocaleResourceAsync("Plugin.Forefront.Xero.Fields.DiscountAmountAccountCode.Hint", "Please enter Discount Amount Account Code", null);
             await _localizationService.AddOrUpdateLocaleResourceAsync("Plugin.Forefront.Xero.Fields.RewardPointsAccountCode", "Reward Points Account Code", null);
             await _localizationService.AddOrUpdateLocaleResourceAsync("Plugin.Forefront.Xero.Fields.RewardPointsAccountCode.Hint", "Please enter Reward Points Account Code", null);
             await _localizationService.AddOrUpdateLocaleResourceAsync("Plugin.Forefront.Xero.Fields.BatchSize", "Xero Integration Data Import BatchSize", null);
             await _localizationService.AddOrUpdateLocaleResourceAsync("Plugin.Forefront.Xero.Fields.BatchSize.Hint", "Xero Integration Batch Size(int) which contains the value of batch size while Sending and Receiving data.", null);
             await _localizationService.AddOrUpdateLocaleResourceAsync("Plugin.Forefront.Xero.Fields.AuthorizedURL", "Xero Authorize URL");
             await _localizationService.AddOrUpdateLocaleResourceAsync("Plugin.Forefront.Xero.Fields.AuthorizedURL.Hint", "Please enter Xero Authorize URL");
             await _localizationService.AddOrUpdateLocaleResourceAsync("Plugin.Forefront.Xero.Fields.AuthorizedResponseType", "Response Type", null);
             await _localizationService.AddOrUpdateLocaleResourceAsync("Plugin.Forefront.Xero.Fields.AuthorizedResponseType.Hint", "Please enter response type", null);
             await _localizationService.AddOrUpdateLocaleResourceAsync("Plugin.Forefront.Xero.Fields.AuthorizedState", "State", null);
             await _localizationService.AddOrUpdateLocaleResourceAsync("Plugin.Forefront.Xero.Fields.AuthorizedState.Hint", "Please enter state", null);
             await _localizationService.AddOrUpdateLocaleResourceAsync("Plugin.Forefront.Xero.Fields.TokenURL", "Token URL", null);
             await _localizationService.AddOrUpdateLocaleResourceAsync("Plugin.Forefront.Xero.Fields.TokenURL.Hint", "Please enter token URL", null);
             await _localizationService.AddOrUpdateLocaleResourceAsync("Plugin.Forefront.Xero.Fields.TokenGrantType", "Grant Type", null);
             await _localizationService.AddOrUpdateLocaleResourceAsync("Plugin.Forefront.Xero.Fields.TokenGrantType.Hint", "Please enter grant type", null);
             await _localizationService.AddOrUpdateLocaleResourceAsync("Plugin.Forefront.Xero.Fields.Scope", "Scope", null);
             await _localizationService.AddOrUpdateLocaleResourceAsync("Plugin.Forefront.Xero.Fields.Scope.Hint", "Please enter scope", null);
             await _localizationService.AddOrUpdateLocaleResourceAsync("Plugin.Forefront.Xero.Fields.ConnectionsURL", "Connections URL", null);
             await _localizationService.AddOrUpdateLocaleResourceAsync("Plugin.Forefront.Xero.Fields.ConnectionsURL.Hint", "Please enter Connections URL", null);
             await _localizationService.AddOrUpdateLocaleResourceAsync("Plugin.Forefront.Xero.Fields.PaymentMethodAdditonalFeeAccountCode", "Payment Method Additonal Fee Account Code", null);
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugin.Forefront.Xero.Fields.PaymentMethodAdditonalFeeAccountCode.Hint", "Please enter Payment Method Additonal Fee Account Code", null);

            ExecuteSqlFile(_nopfileProvider.MapPath("~/Plugins/Forefront.Xero/Scripts/XeroTables_Install.sql"));

            await base.InstallAsync();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
		protected virtual string ReadNextStatementFromStream(StreamReader reader)
        {
            string str;
            var stringBuilder = new StringBuilder();

            while (true)
            {
                string str1 = reader.ReadLine();
                if (str1 == null)
                {
                    if (stringBuilder.Length <= 0)
                    {
                        str = null;
                        break;
                    }
                    else
                    {
                        str = stringBuilder.ToString();
                        break;
                    }
                }
                else if (str1.TrimEnd(new char[0]).ToUpper() != "GO")
                {
                    stringBuilder.Append(string.Concat(str1, Environment.NewLine));
                }
                else
                {
                    str = stringBuilder.ToString();
                    break;
                }
            }

            return str;
        }

        /// <summary>
        /// 
        /// </summary>
        public override async Task UninstallAsync()
        {
           
            await _localizationService.DeleteLocaleResourcesAsync("Plugin.Forefront.Xero.Fields.Tmotions.Xero");
            await _localizationService.DeleteLocaleResourcesAsync("Plugin.Forefront.Xero.Fields.Enable");
            await _localizationService.DeleteLocaleResourcesAsync("Plugin.Forefront.Xero.Fields.Enable.Hint");
            await _localizationService.DeleteLocaleResourcesAsync("Plugin.Forefront.Xero.Fields.IsInventorySyncEnabled.Hint");
            await _localizationService.DeleteLocaleResourcesAsync("Plugin.Forefront.Xero.Fields.IsInventorySyncEnabled");
            await _localizationService.DeleteLocaleResourcesAsync("Plugin.Forefront.Xero.Fields.ClientId");
            await _localizationService.DeleteLocaleResourcesAsync("Plugin.Forefront.Xero.Fields.AccountCode");
            await _localizationService.DeleteLocaleResourcesAsync("Plugin.Forefront.Xero.Fields.AccountCode.Hint");
            await _localizationService.DeleteLocaleResourcesAsync("Plugin.Forefront.Xero.Fields.NonVatAccountCode");
            await _localizationService.DeleteLocaleResourcesAsync("Plugin.Forefront.Xero.Fields.NonVatAccountCode.Hint");
            await _localizationService.DeleteLocaleResourcesAsync("Plugin.Forefront.Xero.Fields.PostageAccountCode");
            await _localizationService.DeleteLocaleResourcesAsync("Plugin.Forefront.Xero.Fields.PostageAccountCode.Hint");
            await _localizationService.DeleteLocaleResourcesAsync("Plugin.Forefront.Xero.Fields.ClientSecret");
            await _localizationService.DeleteLocaleResourcesAsync("Plugin.Forefront.Xero.Fields.CallbackURL");
            await _localizationService.DeleteLocaleResourcesAsync("Plugin.Forefront.Xero.Info");
            await _localizationService.DeleteLocaleResourcesAsync("Plugin.Forefront.Xero.XeroQueue");
            await _localizationService.DeleteLocaleResourcesAsync("Plugin.Forefront.Xero.Field.OrderId");
            await _localizationService.DeleteLocaleResourcesAsync("Plugin.Forefront.Xero.Field.QueuedOn");
            await _localizationService.DeleteLocaleResourcesAsync("Plugin.Forefront.Xero.Field.SyncAttemptOn");
            await _localizationService.DeleteLocaleResourcesAsync("Plugin.Forefront.Xero.Field.Amount");
            await _localizationService.DeleteLocaleResourcesAsync("Plugin.Forefront.Xero.Fields.PostingBatchSize");
            await _localizationService.DeleteLocaleResourcesAsync("Plugin.Forefront.Xero.Fields.PostingBatchSize.Hint");
            await _localizationService.DeleteLocaleResourcesAsync("Plugin.Forefront.Xero.Fields.ClientId.Hint");
            await _localizationService.DeleteLocaleResourcesAsync("Plugin.Forefront.Xero.Fields.ClientSecret.Hint");
            await _localizationService.DeleteLocaleResourcesAsync("Plugin.Forefront.Xero.Fields.CallbackURL.Hint");
            await _localizationService.DeleteLocaleResourcesAsync("Plugin.Forefront.Xero.Fields.ConnectToXero");
            await _localizationService.DeleteLocaleResourcesAsync("Plugin.Forefront.Xero.Fields.ConnectToXero.Hint");
            await _localizationService.DeleteLocaleResourcesAsync("Plugin.Forefront.Xero.Field.IsSuccess");
            await _localizationService.DeleteLocaleResourcesAsync("Plugin.Forefront.Xero.Field.ResponseMessages");
            await _localizationService.DeleteLocaleResourcesAsync("Plugin.Forefront.Xero.accountliability");
            await _localizationService.DeleteLocaleResourcesAsync("Admin.Plugins.Account.saved");
            await _localizationService.DeleteLocaleResourcesAsync("Plugin.Forefront.Xero.Fields.IsDropTable");
            await _localizationService.DeleteLocaleResourcesAsync("Plugin.Forefront.Xero.Fields.IsDropTable.Hint");
            await _localizationService.DeleteLocaleResourcesAsync("Plugin.Forefront.Xero.Fields.BatchSize");
            await _localizationService.DeleteLocaleResourcesAsync("Plugin.Forefront.Xero.Fields.BatchSize.Hint");
            await _localizationService.DeleteLocaleResourcesAsync("Plugin.Forefront.Xero.Fields.RestrictToCount");
            await _localizationService.DeleteLocaleResourcesAsync("Plugin.Forefront.Xero.Fields.RestrictToCount.Hint");
            await _localizationService.DeleteLocaleResourcesAsync("Plugin.Forefront.Xero.Field.ActionType");
            await _localizationService.DeleteLocaleResourcesAsync("Plugin.Forefront.Xero.XeroProduct");
            await _localizationService.DeleteLocaleResourcesAsync("Plugin.Forefront.Xero.Product.Fields.Id");
            await _localizationService.DeleteLocaleResourcesAsync("Plugin.Forefront.Xero.Product.Fields.ProductId");
            await _localizationService.DeleteLocaleResourcesAsync("Plugin.Forefront.Xero.Product.Fields.XeroStatus");
            await _localizationService.DeleteLocaleResourcesAsync("Plugin.Forefront.Xero.Product.Fields.IsDeleted");
            await _localizationService.DeleteLocaleResourcesAsync("Plugin.Forefront.Xero.Product.Fields.ActionType");
            await _localizationService.DeleteLocaleResourcesAsync("Plugin.Forefront.Xero.Title");
            await _localizationService.DeleteLocaleResourcesAsync("Plugin.Forefront.Xero.DomainMismatch");
            await _localizationService.DeleteLocaleResourcesAsync("Plugin.Forefront.Xero.LicenseExpire");
            await _localizationService.DeleteLocaleResourcesAsync("Plugin.Forefront.Xero.InvalidLicense");
            await _localizationService.DeleteLocaleResourcesAsync("Plugin.Forefront.Xero.TrialMode");
            await _localizationService.DeleteLocaleResourcesAsync("Plugin.Forefront.Xero.Fields.DiscountAmountAccountCode");
            await _localizationService.DeleteLocaleResourcesAsync("Plugin.Forefront.Xero.Fields.DiscountAmountAccountCode.Hint");
            await _localizationService.DeleteLocaleResourcesAsync("Plugin.Forefront.Xero.Fields.PaymentMethodAdditonalFeeAccountCode");
            await _localizationService.DeleteLocaleResourcesAsync("Plugin.Forefront.Xero.Fields.PaymentMethodAdditonalFeeAccountCode.Hint");
            await _localizationService.DeleteLocaleResourcesAsync("Plugin.Forefront.Xero.Fields.RewardPointsAccountCode");
            await _localizationService.DeleteLocaleResourcesAsync("Plugin.Forefront.Xero.Fields.RewardPointsAccountCode.Hint");
            await _localizationService.DeleteLocaleResourcesAsync("Plugin.Forefront.Xero.Fields.AuthorizeURL");
            await _localizationService.DeleteLocaleResourcesAsync("Plugin.Forefront.Xero.Fields.AuthorizeURL.Hint");
            await _localizationService.DeleteLocaleResourcesAsync("Plugin.Forefront.Xero.Fields.AuthorizedResponseType");
            await _localizationService.DeleteLocaleResourcesAsync("Plugin.Forefront.Xero.Fields.AuthorizedResponseType.Hint");
            await _localizationService.DeleteLocaleResourcesAsync("Plugin.Forefront.Xero.Fields.AuthorizedState");
            await _localizationService.DeleteLocaleResourcesAsync("Plugin.Forefront.Xero.Fields.AuthorizedState.Hint");
            await _localizationService.DeleteLocaleResourcesAsync("Plugin.Forefront.Xero.Fields.TokenURL");
            await _localizationService.DeleteLocaleResourcesAsync("Plugin.Forefront.Xero.Fields.TokenURL.Hint");
            await _localizationService.DeleteLocaleResourcesAsync("Plugin.Forefront.Xero.Fields.TokenGrantType");
            await _localizationService.DeleteLocaleResourcesAsync("Plugin.Forefront.Xero.Fields.TokenGrantType.Hint");
            await _localizationService.DeleteLocaleResourcesAsync("Plugin.Forefront.Xero.Fields.Scope");
            await _localizationService.DeleteLocaleResourcesAsync("Plugin.Forefront.Xero.Fields.Scope.Hint");
            await _localizationService.DeleteLocaleResourcesAsync("Plugin.Forefront.Xero.Fields.ConnectionsURL");
            await _localizationService.DeleteLocaleResourcesAsync("Plugin.Forefront.Xero.Fields.ConnectionsURL.Hint");

            var taskByType =  await _scheduleTaskService.GetTaskByTypeAsync("Nop.Plugin.Forefront.Xero.ScheduleTasks.XeroIntegration");
            if (taskByType != null)
            {
                await _scheduleTaskService.DeleteTaskAsync(taskByType);
            }

            var scheduleTask = await _scheduleTaskService.GetTaskByTypeAsync("Nop.Plugin.Forefront.Xero.ScheduleTasks.XeroIntegration");
            if (scheduleTask != null)
            {
                await _scheduleTaskService.DeleteTaskAsync(scheduleTask);
            }

            await _settingService.DeleteSettingAsync<ForeFrontXeroSetting>();

            await base.UninstallAsync();
        }

        #endregion
    }
}