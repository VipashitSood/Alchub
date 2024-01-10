using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.Tax.VendorWise.Models;
using Nop.Services.Configuration;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Tax.VendorWise.Controllers
{
    [AuthorizeAdmin]
    [Area(AreaNames.Admin)]
    [AutoValidateAntiforgeryToken]
    public class VendorWiseTaxController : BasePluginController
    {
        #region Fields

        private readonly VendorWiseTaxSettings _vendorWiseTaxSettings;
        private readonly IPermissionService _permissionService;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;
        #endregion

        #region Ctor

        public VendorWiseTaxController(VendorWiseTaxSettings vendorWiseTaxSettings,
            IPermissionService permissionService,
            ISettingService settingService,
            IStoreContext storeContext)

        {
            _vendorWiseTaxSettings = vendorWiseTaxSettings;
            _permissionService = permissionService;
            _settingService = settingService;
            _storeContext = storeContext;
        }

        #endregion

        #region Methods

        public async Task<IActionResult> Configure()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageTaxSettings))
                return AccessDeniedView();

            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var vendorWiseTaxSettings = await _settingService.LoadSettingAsync<VendorWiseTaxSettings>(storeScope);
            var model = new ConfigurationModel()
            {
                Enabled = vendorWiseTaxSettings.Enable,
              
            };

            if (storeScope > 0)
            {
                model.Enabled = await _settingService.SettingExistsAsync(vendorWiseTaxSettings, x => x.Enable, storeScope);
            }

         
            return View("~/Plugins/Tax.VendorWise/Views/Configure.cshtml", model);
        }

        [HttpPost]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            //save settings
            _vendorWiseTaxSettings.Enable = model.Enabled;
            await _settingService.SaveSettingAsync(_vendorWiseTaxSettings);

            return View("~/Plugins/Tax.VendorWise/Views/Configure.cshtml", model);
        }

        #endregion
    }
}