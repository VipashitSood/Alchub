using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Messages;
using Nop.Core.Domain.ScheduleTasks;
using Nop.Plugin.Misc.ScheduledOrderCancel.Domain;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Plugins;
using Nop.Services.ScheduleTasks;

namespace Nop.Plugin.Misc.ScheduledOrderCancel
{
    /// <summary>
    /// PLugin
    /// </summary>
    public class ScheduledOrderCancelPlugin : BasePlugin, IMiscPlugin
    {
        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly ISettingService _settingService;
        private readonly IWebHelper _webHelper;
        private readonly IScheduleTaskService _scheduleTaskService;
        private readonly IMessageTemplateService _messageTemplateService;
        private readonly IEmailAccountService _emailAccountService;

        #endregion

        #region Ctor

        public ScheduledOrderCancelPlugin(ILocalizationService localizationService,
            ISettingService settingService,
            IWebHelper webHelper,
            IScheduleTaskService scheduleTaskService,
            IMessageTemplateService messageTemplateService,
            IEmailAccountService emailAccountService)
        {
            _localizationService = localizationService;
            _settingService = settingService;
            _webHelper = webHelper;
            _scheduleTaskService = scheduleTaskService;
            _messageTemplateService = messageTemplateService;
            _emailAccountService = emailAccountService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets a configuration page URL
        /// </summary>
        public override string GetConfigurationPageUrl()
        {
            return _webHelper.GetStoreLocation() + "Admin/ScheduledOrderCancel/Configure";
        }

        /// <summary>
        /// Install plugin
        /// </summary>
        /// <returns>A task that represents the asynchronous operation</returns>
        public override async Task InstallAsync()
        {
            //settings
            var settings = new ScheduledOrderCancelSettings
            {
                Enabled = false,
                Time = 10
            };

            //install synchronization task
            if (await _scheduleTaskService.GetTaskByTypeAsync(ScheduledOrderCancelDefaults.ScheduledOrderCancelTask) == null)
            {
                await _scheduleTaskService.InsertTaskAsync(new ScheduleTask
                {
                    Enabled = true,
                    LastEnabledUtc = DateTime.UtcNow,
                    Seconds = ScheduledOrderCancelDefaults.DefaultSynchronizationPeriod * 10 * 60,
                    Name = ScheduledOrderCancelDefaults.ScheduledOrderCancelTaskName,
                    Type = ScheduledOrderCancelDefaults.ScheduledOrderCancelTask,
                });
                await _settingService.SaveSettingAsync(settings);
            }

            await _localizationService.AddOrUpdateLocaleResourceAsync(new Dictionary<string, string>
            {
                ["Plugins.Misc.ScheduledOrderCancel.Fields.Enabled"] = "Enabled",
                ["Plugins.Misc.ScheduledOrderCancel.Fields.Enabled.hint"] = "Enable plugin functionality",
                ["Plugins.Misc.ScheduledOrderCancel.Fields.Time"] = "Time in minutes",
                ["Plugins.Misc.ScheduledOrderCancel.Fields.Time.hint"] = "Specify the time in minutes, after how long once the order gets placed, it is still in a pending state, then It'll get canceled. If specified 0 then It will skip the order cancelation.",
                ["plugins.Misc.ScheduledOrderCancel"] = "Configuration",
                ["Admin.Common.Save"] = "Save"
            });

            //add message template
            var messageTemplates = await _messageTemplateService.GetMessageTemplatesByNameAsync(MessageTemplateSystemName.OrderCancelledCustomerCustomNotification);
            if (messageTemplates == null || !messageTemplates.Any())
            {
                var customOrderCancelMessageTemplate = new MessageTemplate
                {
                    Name = MessageTemplateSystemName.OrderCancelledCustomerCustomNotification,
                    Subject = "%Store.Name%. Your order cancelled due to payment failure.",
                    Body = $"<p>{Environment.NewLine}Hello %Order.CustomerFullName%, {Environment.NewLine}<br /><br />{Environment.NewLine}Your order has been canceled, due to payment failed to process.{Environment.NewLine}<br />{Environment.NewLine}Order Number: %Order.OrderNumber%{Environment.NewLine}<br />{Environment.NewLine}Order Details: <a href=\"%Order.OrderURLForCustomer%\" target=\"_blank\">%Order.OrderURLForCustomer%</a>{Environment.NewLine}<br />{Environment.NewLine}Date Ordered: %Order.CreatedOn%{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}Address{Environment.NewLine}<br />{Environment.NewLine}%Order.BillingFirstName% %Order.BillingLastName%{Environment.NewLine}<br />{Environment.NewLine}%Order.BillingAddress1%{Environment.NewLine}<br />{Environment.NewLine}%Order.BillingAddress2%{Environment.NewLine}<br />{Environment.NewLine}%Order.BillingCity% %Order.BillingZipPostalCode%{Environment.NewLine}<br />{Environment.NewLine}%Order.BillingStateProvince% %Order.BillingCountry%{Environment.NewLine}Type: %Order.BillingAddressType%{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}%Order.Product(s)%{Environment.NewLine}</p>{Environment.NewLine}",
                    IsActive = true,
                    EmailAccountId = (await _emailAccountService.GetAllEmailAccountsAsync())?.FirstOrDefault()?.Id ?? 0
                };

                await _messageTemplateService.InsertMessageTemplateAsync(customOrderCancelMessageTemplate);
            }

            await base.InstallAsync();
        }

        /// <summary>
        /// Uninstall plugin
        /// </summary>
        /// <returns>A task that represents the asynchronous operation</returns>
        public override async Task UninstallAsync()
        {
            //settings
            await _settingService.DeleteSettingAsync<ScheduledOrderCancelSettings>();

            //schedule task
            var task = await _scheduleTaskService.GetTaskByTypeAsync(ScheduledOrderCancelDefaults.ScheduledOrderCancelTask);
            if (task != null)
                await _scheduleTaskService.DeleteTaskAsync(task);

            //locales
            await _localizationService.DeleteLocaleResourcesAsync("Plugins.Misc.ScheduledOrderCancel");

            await base.UninstallAsync();

        }

        #endregion
    }
}