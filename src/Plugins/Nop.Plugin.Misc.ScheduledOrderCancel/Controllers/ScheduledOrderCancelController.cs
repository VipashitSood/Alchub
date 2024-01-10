using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Cms;
using Nop.Plugin.Misc.ScheduledOrderCancel.Models;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Misc.ScheduledOrderCancel.Controllers
{
    [AuthorizeAdmin]
    [Area(AreaNames.Admin)]
    [AutoValidateAntiforgeryToken]
    public partial class ScheduledOrderCancelController : BasePluginController
    {
        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly IPermissionService _permissionService;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;
        private readonly IWorkContext _workContext;

        #endregion

        #region Ctor

        public ScheduledOrderCancelController(ILocalizationService localizationService,
            INotificationService notificationService,
            IPermissionService permissionService,
            ISettingService settingService,
            IStoreContext storeContext,
            IWorkContext workContext)

        {
            _localizationService = localizationService;
            _notificationService = notificationService;
            _permissionService = permissionService;
            _settingService = settingService;
            _storeContext = storeContext;
            _workContext = workContext;
        }

        #endregion

        public async Task<IActionResult> Configure()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageWidgets))
                return AccessDeniedView();
            var customer = await _workContext.GetCurrentCustomerAsync();
            var storeId = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var pluginSettings = await _settingService.LoadSettingAsync<ScheduledOrderCancelSettings>(storeId);
            var widgetSettings = await _settingService.LoadSettingAsync<WidgetSettings>(storeId);
            var model = new ConfigurationModel
            {
                Enabled = pluginSettings.Enabled,
                Time=pluginSettings.Time,
                ActiveStoreScopeConfiguration = storeId
            };

            if (storeId > 0)
            {
                model.Enabled_OverrideForStore = await _settingService.SettingExistsAsync(pluginSettings, x => x.Enabled, storeId);
                model.Enabled_OverrideForStore = await _settingService.SettingExistsAsync(pluginSettings, x => x.Time, storeId);
            }

            return View("~/Plugins/Misc.ScheduledOrderCancel/Views/Configure.cshtml", model);
        }

        [HttpPost]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageWidgets))
                return AccessDeniedView();

            if (!ModelState.IsValid)
                return await Configure();

            var storeId = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var pluginSettings = await _settingService.LoadSettingAsync<ScheduledOrderCancelSettings>(storeId);

            pluginSettings.Enabled = model.Enabled;
            pluginSettings.Time = model.Time;

            //save settings
            await _settingService.SaveSettingOverridablePerStoreAsync(pluginSettings, x => x.Enabled, model.Enabled_OverrideForStore, storeId, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(pluginSettings, x => x.Time, model.Time_OverrideForStore, storeId, false);
           
            await _settingService.ClearCacheAsync();
            
            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Plugins.Saved"));

            return await Configure();
        }
    }
}
