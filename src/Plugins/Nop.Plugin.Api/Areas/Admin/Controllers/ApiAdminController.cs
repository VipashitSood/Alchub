using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.Api.Areas.Admin.Models;
using Nop.Plugin.Api.Domain;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nop.Plugin.Api.Areas.Admin.Controllers
{
    [AuthorizeAdmin]
    [Area(AreaNames.Admin)]
    public class ApiAdminController : BasePluginController
    {
        private readonly ICustomerActivityService _customerActivityService;
        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;

        public ApiAdminController(
            IStoreContext storeContext,
            ISettingService settingService,
            ICustomerActivityService customerActivityService,
            ILocalizationService localizationService,
            INotificationService notificationService)
        {
            _storeContext = storeContext;
            _settingService = settingService;
            _customerActivityService = customerActivityService;
            _localizationService = localizationService;
            _notificationService = notificationService;
        }

        [HttpGet]
        public async Task<IActionResult> Settings()
        {
            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var apiSettings = await _settingService.LoadSettingAsync<ApiSettings>(storeScope);
            var model = new ConfigurationModel()
            {
                EnableApi = apiSettings.EnableApi,
                TokenExpiryInDays = apiSettings.TokenExpiryInDays,
                OtpRequiredOnRegistration = apiSettings.OtpRequiredOnRegistration,
                Restricted_PaymentMethodsSystemNames = string.Join(",", apiSettings?.Restricted_PaymentMethodsSystemNames), //paymentmethods
                ActiveStoreScopeConfiguration = storeScope
            };

            if (storeScope > 0)
            {
                model.EnableApi_OverrideForStore = await _settingService.SettingExistsAsync(apiSettings, x => x.EnableApi, storeScope);
                model.TokenExpiryInDays_OverrideForStore = await _settingService.SettingExistsAsync(apiSettings, x => x.TokenExpiryInDays, storeScope);
                model.Restricted_PaymentMethodsSystemNames_OverrideForStore = await _settingService.SettingExistsAsync(apiSettings, x => string.Join(",", x.Restricted_PaymentMethodsSystemNames), storeScope);
                model.OtpRequiredOnRegistration_OverrideForStore = await _settingService.SettingExistsAsync(apiSettings, x => x.OtpRequiredOnRegistration, storeScope);
            }

            return View($"~/Plugins/Nop.Plugin.Api/Areas/Admin/Views/ApiAdmin/Settings.cshtml", model);
        }

        [HttpPost]
        public async Task<IActionResult> Settings(ConfigurationModel model)
        {
            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var apiSettings = await _settingService.LoadSettingAsync<ApiSettings>(storeScope);
            //var apiSettings = model.ToEntity();

            apiSettings.EnableApi = model.EnableApi;
            apiSettings.TokenExpiryInDays = model.TokenExpiryInDays;
            apiSettings.OtpRequiredOnRegistration = model.OtpRequiredOnRegistration;
            apiSettings.Restricted_PaymentMethodsSystemNames = model.Restricted_PaymentMethodsSystemNames?.Split(',')?.ToList() ?? new List<string>();

            /* We do not clear cache after each setting update.
            * This behavior can increase performance because cached settings will not be cleared 
            * and loaded from database after each update */

            await _settingService.SaveSettingOverridablePerStoreAsync(apiSettings, x => x.EnableApi, model.EnableApi_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(apiSettings, x => x.TokenExpiryInDays, model.TokenExpiryInDays_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(apiSettings, x => x.Restricted_PaymentMethodsSystemNames, model.Restricted_PaymentMethodsSystemNames_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(apiSettings, x => x.OtpRequiredOnRegistration, model.OtpRequiredOnRegistration_OverrideForStore, storeScope, false);

            //now clear settings cache
            await _settingService.ClearCacheAsync();

            await _customerActivityService.InsertActivityAsync("EditApiSettings", "Edit Api Settings");

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Plugins.Saved"));

            return View($"~/Plugins/Nop.Plugin.Api/Areas/Admin/Views/ApiAdmin/Settings.cshtml", model);
        }
    }
}
