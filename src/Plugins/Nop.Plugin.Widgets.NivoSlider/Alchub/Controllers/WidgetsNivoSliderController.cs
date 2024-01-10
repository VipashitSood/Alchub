using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.Widgets.NivoSlider.Models;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Widgets.NivoSlider.Controllers
{
    [AuthorizeAdmin]
    public partial class WidgetsNivoSliderController : BasePluginController
    {
        public async Task<IActionResult> Configure()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageWidgets))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var nivoSliderSettings = await _settingService.LoadSettingAsync<NivoSliderSettings>(storeScope);
            var model = new ConfigurationModel
            {
                Picture1Id = nivoSliderSettings.Picture1Id,
                Text1 = nivoSliderSettings.Text1,
                Link1 = nivoSliderSettings.Link1,
                AltText1 = nivoSliderSettings.AltText1,
                Picture2Id = nivoSliderSettings.Picture2Id,
                Text2 = nivoSliderSettings.Text2,
                Link2 = nivoSliderSettings.Link2,
                AltText2 = nivoSliderSettings.AltText2,
                Picture3Id = nivoSliderSettings.Picture3Id,
                Text3 = nivoSliderSettings.Text3,
                Link3 = nivoSliderSettings.Link3,
                AltText3 = nivoSliderSettings.AltText3,
                Picture4Id = nivoSliderSettings.Picture4Id,
                Text4 = nivoSliderSettings.Text4,
                Link4 = nivoSliderSettings.Link4,
                AltText4 = nivoSliderSettings.AltText4,
                Picture5Id = nivoSliderSettings.Picture5Id,
                Text5 = nivoSliderSettings.Text5,
                Link5 = nivoSliderSettings.Link5,
                AltText5 = nivoSliderSettings.AltText5,

                //for mobile
                MobilePicture1Id = nivoSliderSettings.MobilePicture1Id,
                MobileText1 = nivoSliderSettings.MobileText1,
                MobileLink1 = nivoSliderSettings.MobileLink1,
                MobileAltText1 = nivoSliderSettings.MobileAltText1,
                MobilePicture2Id = nivoSliderSettings.MobilePicture2Id,
                MobileText2 = nivoSliderSettings.MobileText2,
                MobileLink2 = nivoSliderSettings.MobileLink2,
                MobileAltText2 = nivoSliderSettings.MobileAltText2,
                MobilePicture3Id = nivoSliderSettings.MobilePicture3Id,
                MobileText3 = nivoSliderSettings.MobileText3,
                MobileLink3 = nivoSliderSettings.MobileLink3,
                MobileAltText3 = nivoSliderSettings.MobileAltText3,
                MobilePicture4Id = nivoSliderSettings.MobilePicture4Id,
                MobileText4 = nivoSliderSettings.MobileText4,
                MobileLink4 = nivoSliderSettings.MobileLink4,
                MobileAltText4 = nivoSliderSettings.MobileAltText4,
                MobilePicture5Id = nivoSliderSettings.MobilePicture5Id,
                MobileText5 = nivoSliderSettings.MobileText5,
                MobileLink5 = nivoSliderSettings.MobileLink5,
                MobileAltText5 = nivoSliderSettings.MobileAltText5,
                ActiveStoreScopeConfiguration = storeScope
            };

            if (storeScope > 0)
            {
                model.Picture1Id_OverrideForStore = await _settingService.SettingExistsAsync(nivoSliderSettings, x => x.Picture1Id, storeScope);
                model.Text1_OverrideForStore = await _settingService.SettingExistsAsync(nivoSliderSettings, x => x.Text1, storeScope);
                model.Link1_OverrideForStore = await _settingService.SettingExistsAsync(nivoSliderSettings, x => x.Link1, storeScope);
                model.AltText1_OverrideForStore = await _settingService.SettingExistsAsync(nivoSliderSettings, x => x.AltText1, storeScope);
                model.Picture2Id_OverrideForStore = await _settingService.SettingExistsAsync(nivoSliderSettings, x => x.Picture2Id, storeScope);
                model.Text2_OverrideForStore = await _settingService.SettingExistsAsync(nivoSliderSettings, x => x.Text2, storeScope);
                model.Link2_OverrideForStore = await _settingService.SettingExistsAsync(nivoSliderSettings, x => x.Link2, storeScope);
                model.AltText2_OverrideForStore = await _settingService.SettingExistsAsync(nivoSliderSettings, x => x.AltText2, storeScope);
                model.Picture3Id_OverrideForStore = await _settingService.SettingExistsAsync(nivoSliderSettings, x => x.Picture3Id, storeScope);
                model.Text3_OverrideForStore = await _settingService.SettingExistsAsync(nivoSliderSettings, x => x.Text3, storeScope);
                model.Link3_OverrideForStore = await _settingService.SettingExistsAsync(nivoSliderSettings, x => x.Link3, storeScope);
                model.AltText3_OverrideForStore = await _settingService.SettingExistsAsync(nivoSliderSettings, x => x.AltText3, storeScope);
                model.Picture4Id_OverrideForStore = await _settingService.SettingExistsAsync(nivoSliderSettings, x => x.Picture4Id, storeScope);
                model.Text4_OverrideForStore = await _settingService.SettingExistsAsync(nivoSliderSettings, x => x.Text4, storeScope);
                model.Link4_OverrideForStore = await _settingService.SettingExistsAsync(nivoSliderSettings, x => x.Link4, storeScope);
                model.AltText4_OverrideForStore = await _settingService.SettingExistsAsync(nivoSliderSettings, x => x.AltText4, storeScope);
                model.Picture5Id_OverrideForStore = await _settingService.SettingExistsAsync(nivoSliderSettings, x => x.Picture5Id, storeScope);
                model.Text5_OverrideForStore = await _settingService.SettingExistsAsync(nivoSliderSettings, x => x.Text5, storeScope);
                model.Link5_OverrideForStore = await _settingService.SettingExistsAsync(nivoSliderSettings, x => x.Link5, storeScope);
                model.AltText5_OverrideForStore = await _settingService.SettingExistsAsync(nivoSliderSettings, x => x.AltText5, storeScope);

                //for mobile
                model.MobilePicture1Id_OverrideForStore = await _settingService.SettingExistsAsync(nivoSliderSettings, x => x.MobilePicture1Id, storeScope);
                model.MobileText1_OverrideForStore = await _settingService.SettingExistsAsync(nivoSliderSettings, x => x.MobileText1, storeScope);
                model.MobileLink1_OverrideForStore = await _settingService.SettingExistsAsync(nivoSliderSettings, x => x.MobileLink1, storeScope);
                model.MobileAltText1_OverrideForStore = await _settingService.SettingExistsAsync(nivoSliderSettings, x => x.MobileAltText1, storeScope);
                model.MobilePicture2Id_OverrideForStore = await _settingService.SettingExistsAsync(nivoSliderSettings, x => x.MobilePicture2Id, storeScope);
                model.MobileText2_OverrideForStore = await _settingService.SettingExistsAsync(nivoSliderSettings, x => x.MobileText2, storeScope);
                model.MobileLink2_OverrideForStore = await _settingService.SettingExistsAsync(nivoSliderSettings, x => x.MobileLink2, storeScope);
                model.MobileAltText2_OverrideForStore = await _settingService.SettingExistsAsync(nivoSliderSettings, x => x.MobileAltText2, storeScope);
                model.MobilePicture3Id_OverrideForStore = await _settingService.SettingExistsAsync(nivoSliderSettings, x => x.MobilePicture3Id, storeScope);
                model.MobileText3_OverrideForStore = await _settingService.SettingExistsAsync(nivoSliderSettings, x => x.MobileText3, storeScope);
                model.MobileLink3_OverrideForStore = await _settingService.SettingExistsAsync(nivoSliderSettings, x => x.MobileLink3, storeScope);
                model.MobileAltText3_OverrideForStore = await _settingService.SettingExistsAsync(nivoSliderSettings, x => x.MobileAltText3, storeScope);
                model.MobilePicture4Id_OverrideForStore = await _settingService.SettingExistsAsync(nivoSliderSettings, x => x.MobilePicture4Id, storeScope);
                model.MobileText4_OverrideForStore = await _settingService.SettingExistsAsync(nivoSliderSettings, x => x.MobileText4, storeScope);
                model.MobileLink4_OverrideForStore = await _settingService.SettingExistsAsync(nivoSliderSettings, x => x.MobileLink4, storeScope);
                model.MobileAltText4_OverrideForStore = await _settingService.SettingExistsAsync(nivoSliderSettings, x => x.MobileAltText4, storeScope);
                model.MobilePicture5Id_OverrideForStore = await _settingService.SettingExistsAsync(nivoSliderSettings, x => x.MobilePicture5Id, storeScope);
                model.MobileText5_OverrideForStore = await _settingService.SettingExistsAsync(nivoSliderSettings, x => x.MobileText5, storeScope);
                model.MobileLink5_OverrideForStore = await _settingService.SettingExistsAsync(nivoSliderSettings, x => x.MobileLink5, storeScope);
                model.MobileAltText5_OverrideForStore = await _settingService.SettingExistsAsync(nivoSliderSettings, x => x.MobileAltText5, storeScope);
            }

            return View("~/Plugins/Widgets.NivoSlider/Views/Configure.cshtml", model);
        }

        [HttpPost]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageWidgets))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var nivoSliderSettings = await _settingService.LoadSettingAsync<NivoSliderSettings>(storeScope);

            //get previous picture identifiers
            var previousPictureIds = new[] 
            {
                nivoSliderSettings.Picture1Id,
                nivoSliderSettings.Picture2Id,
                nivoSliderSettings.Picture3Id,
                nivoSliderSettings.Picture4Id,
                nivoSliderSettings.Picture5Id
            };

            //get previous mobile picture identifiers
            var previousMobilePictureIds = new[]
            {
                nivoSliderSettings.MobilePicture1Id,
                nivoSliderSettings.MobilePicture2Id,
                nivoSliderSettings.MobilePicture3Id,
                nivoSliderSettings.MobilePicture4Id,
                nivoSliderSettings.MobilePicture5Id
            };

            nivoSliderSettings.Picture1Id = model.Picture1Id;
            nivoSliderSettings.Text1 = model.Text1;
            nivoSliderSettings.Link1 = model.Link1;
            nivoSliderSettings.AltText1 = model.AltText1;
            nivoSliderSettings.Picture2Id = model.Picture2Id;
            nivoSliderSettings.Text2 = model.Text2;
            nivoSliderSettings.Link2 = model.Link2;
            nivoSliderSettings.AltText2 = model.AltText2;
            nivoSliderSettings.Picture3Id = model.Picture3Id;
            nivoSliderSettings.Text3 = model.Text3;
            nivoSliderSettings.Link3 = model.Link3;
            nivoSliderSettings.AltText3 = model.AltText3;
            nivoSliderSettings.Picture4Id = model.Picture4Id;
            nivoSliderSettings.Text4 = model.Text4;
            nivoSliderSettings.Link4 = model.Link4;
            nivoSliderSettings.AltText4 = model.AltText4;
            nivoSliderSettings.Picture5Id = model.Picture5Id;
            nivoSliderSettings.Text5 = model.Text5;
            nivoSliderSettings.Link5 = model.Link5;
            nivoSliderSettings.AltText5 = model.AltText5;

            //for mobile
            nivoSliderSettings.MobilePicture1Id = model.MobilePicture1Id;
            nivoSliderSettings.MobileText1 = model.MobileText1;
            nivoSliderSettings.MobileLink1 = model.MobileLink1;
            nivoSliderSettings.MobileAltText1 = model.MobileAltText1;
            nivoSliderSettings.MobilePicture2Id = model.MobilePicture2Id;
            nivoSliderSettings.MobileText2 = model.MobileText2;
            nivoSliderSettings.MobileLink2 = model.MobileLink2;
            nivoSliderSettings.MobileAltText2 = model.MobileAltText2;
            nivoSliderSettings.MobilePicture3Id = model.MobilePicture3Id;
            nivoSliderSettings.MobileText3 = model.MobileText3;
            nivoSliderSettings.MobileLink3 = model.MobileLink3;
            nivoSliderSettings.MobileAltText3 = model.MobileAltText3;
            nivoSliderSettings.MobilePicture4Id = model.MobilePicture4Id;
            nivoSliderSettings.MobileText4 = model.MobileText4;
            nivoSliderSettings.MobileLink4 = model.MobileLink4;
            nivoSliderSettings.MobileAltText4 = model.MobileAltText4;
            nivoSliderSettings.MobilePicture5Id = model.MobilePicture5Id;
            nivoSliderSettings.MobileText5 = model.MobileText5;
            nivoSliderSettings.MobileLink5 = model.MobileLink5;
            nivoSliderSettings.MobileAltText5 = model.MobileAltText5;

            /* We do not clear cache after each setting update.
             * This behavior can increase performance because cached settings will not be cleared 
             * and loaded from database after each update */
            await _settingService.SaveSettingOverridablePerStoreAsync(nivoSliderSettings, x => x.Picture1Id, model.Picture1Id_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(nivoSliderSettings, x => x.Text1, model.Text1_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(nivoSliderSettings, x => x.Link1, model.Link1_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(nivoSliderSettings, x => x.AltText1, model.AltText1_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(nivoSliderSettings, x => x.Picture2Id, model.Picture2Id_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(nivoSliderSettings, x => x.Text2, model.Text2_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(nivoSliderSettings, x => x.Link2, model.Link2_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(nivoSliderSettings, x => x.AltText2, model.AltText2_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(nivoSliderSettings, x => x.Picture3Id, model.Picture3Id_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(nivoSliderSettings, x => x.Text3, model.Text3_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(nivoSliderSettings, x => x.Link3, model.Link3_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(nivoSliderSettings, x => x.AltText3, model.AltText3_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(nivoSliderSettings, x => x.Picture4Id, model.Picture4Id_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(nivoSliderSettings, x => x.Text4, model.Text4_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(nivoSliderSettings, x => x.Link4, model.Link4_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(nivoSliderSettings, x => x.AltText4, model.AltText4_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(nivoSliderSettings, x => x.Picture5Id, model.Picture5Id_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(nivoSliderSettings, x => x.Text5, model.Text5_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(nivoSliderSettings, x => x.Link5, model.Link5_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(nivoSliderSettings, x => x.AltText5, model.AltText5_OverrideForStore, storeScope, false);

            await _settingService.SaveSettingOverridablePerStoreAsync(nivoSliderSettings, x => x.MobilePicture1Id, model.MobilePicture1Id_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(nivoSliderSettings, x => x.MobileText1, model.MobileText1_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(nivoSliderSettings, x => x.MobileLink1, model.MobileLink1_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(nivoSliderSettings, x => x.MobileAltText1, model.MobileAltText1_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(nivoSliderSettings, x => x.MobilePicture2Id, model.MobilePicture2Id_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(nivoSliderSettings, x => x.MobileText2, model.MobileText2_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(nivoSliderSettings, x => x.MobileLink2, model.MobileLink2_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(nivoSliderSettings, x => x.MobileAltText2, model.MobileAltText2_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(nivoSliderSettings, x => x.MobilePicture3Id, model.MobilePicture3Id_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(nivoSliderSettings, x => x.MobileText3, model.MobileText3_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(nivoSliderSettings, x => x.MobileLink3, model.MobileLink3_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(nivoSliderSettings, x => x.MobileAltText3, model.MobileAltText3_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(nivoSliderSettings, x => x.MobilePicture4Id, model.MobilePicture4Id_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(nivoSliderSettings, x => x.MobileText4, model.MobileText4_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(nivoSliderSettings, x => x.MobileLink4, model.MobileLink4_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(nivoSliderSettings, x => x.MobileAltText4, model.MobileAltText4_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(nivoSliderSettings, x => x.MobilePicture5Id, model.MobilePicture5Id_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(nivoSliderSettings, x => x.MobileText5, model.MobileText5_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(nivoSliderSettings, x => x.MobileLink5, model.MobileLink5_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(nivoSliderSettings, x => x.MobileAltText5, model.MobileAltText5_OverrideForStore, storeScope, false);
            //now clear settings cache
            await _settingService.ClearCacheAsync();
            
            //get current picture identifiers
            var currentPictureIds = new[]
            {
                nivoSliderSettings.Picture1Id,
                nivoSliderSettings.Picture2Id,
                nivoSliderSettings.Picture3Id,
                nivoSliderSettings.Picture4Id,
                nivoSliderSettings.Picture5Id
            };

            //get current mobile picture identifiers
            var currentMobilePictureIds = new[]
            {
                nivoSliderSettings.MobilePicture1Id,
                nivoSliderSettings.MobilePicture2Id,
                nivoSliderSettings.MobilePicture3Id,
                nivoSliderSettings.MobilePicture4Id,
                nivoSliderSettings.MobilePicture5Id
            };

            //delete an old picture (if deleted or updated)
            foreach (var pictureId in previousPictureIds.Except(currentPictureIds))
            { 
                var previousPicture = await _pictureService.GetPictureByIdAsync(pictureId);
                if (previousPicture != null)
                    await _pictureService.DeletePictureAsync(previousPicture);
            }

            //delete an old mobile picture (if deleted or updated)
            foreach (var pictureId in previousMobilePictureIds.Except(currentMobilePictureIds))
            {
                var previousPicture = await _pictureService.GetPictureByIdAsync(pictureId);
                if (previousPicture != null)
                    await _pictureService.DeletePictureAsync(previousPicture);
            }

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Plugins.Saved"));
            
            return await Configure();
        }
    }
}