using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Plugin.Widgets.NivoSlider.Infrastructure.Cache;
using Nop.Plugin.Widgets.NivoSlider.Models;
using Nop.Services.Configuration;
using Nop.Services.Helpers;
using Nop.Services.Media;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Widgets.NivoSlider.Components
{
    public partial class WidgetsNivoSliderViewComponent : NopViewComponent
    {
        private readonly IStoreContext _storeContext;
        private readonly IStaticCacheManager _staticCacheManager;
        private readonly ISettingService _settingService;
        private readonly IPictureService _pictureService;
        private readonly IWebHelper _webHelper;
        private readonly IUserAgentHelper _userAgentHelper;

        public WidgetsNivoSliderViewComponent(IStoreContext storeContext,
            IStaticCacheManager staticCacheManager,
            ISettingService settingService,
            IPictureService pictureService,
            IWebHelper webHelper,
            IUserAgentHelper userAgentHelper)
        {
            _storeContext = storeContext;
            _staticCacheManager = staticCacheManager;
            _settingService = settingService;
            _pictureService = pictureService;
            _webHelper = webHelper;
            _userAgentHelper = userAgentHelper;
        }

        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
        {
            var store = await _storeContext.GetCurrentStoreAsync();
            var nivoSliderSettings = await _settingService.LoadSettingAsync<NivoSliderSettings>(store.Id);
            var isMobileDevice = _userAgentHelper.IsMobileDevice();

            var model = new PublicInfoModel
            {
                Picture1Url = await GetPictureUrlAsync(nivoSliderSettings.Picture1Id),
                Text1 = nivoSliderSettings.Text1,
                Link1 = nivoSliderSettings.Link1,
                AltText1 = nivoSliderSettings.AltText1,

                Picture2Url = await GetPictureUrlAsync(nivoSliderSettings.Picture2Id),
                Text2 = nivoSliderSettings.Text2,
                Link2 = nivoSliderSettings.Link2,
                AltText2 = nivoSliderSettings.AltText2,

                Picture3Url = await GetPictureUrlAsync(nivoSliderSettings.Picture3Id),
                Text3 = nivoSliderSettings.Text3,
                Link3 = nivoSliderSettings.Link3,
                AltText3 = nivoSliderSettings.AltText3,

                Picture4Url = await GetPictureUrlAsync(nivoSliderSettings.Picture4Id),
                Text4 = nivoSliderSettings.Text4,
                Link4 = nivoSliderSettings.Link4,
                AltText4 = nivoSliderSettings.AltText4,

                Picture5Url = await GetPictureUrlAsync(nivoSliderSettings.Picture5Id),
                Text5 = nivoSliderSettings.Text5,
                Link5 = nivoSliderSettings.Link5,
                AltText5 = nivoSliderSettings.AltText5,

                //for mobile
                MobilePicture1Url = await GetPictureUrlAsync(nivoSliderSettings.MobilePicture1Id),
                MobileText1 = nivoSliderSettings.MobileText1,
                MobileLink1 = nivoSliderSettings.MobileLink1,
                MobileAltText1 = nivoSliderSettings.MobileAltText1,

                MobilePicture2Url = await GetPictureUrlAsync(nivoSliderSettings.MobilePicture2Id),
                MobileText2 = nivoSliderSettings.MobileText2,
                MobileLink2 = nivoSliderSettings.MobileLink2,
                MobileAltText2 = nivoSliderSettings.MobileAltText2,

                MobilePicture3Url = await GetPictureUrlAsync(nivoSliderSettings.MobilePicture3Id),
                MobileText3 = nivoSliderSettings.MobileText3,
                MobileLink3 = nivoSliderSettings.MobileLink3,
                MobileAltText3 = nivoSliderSettings.MobileAltText3,

                MobilePicture4Url = await GetPictureUrlAsync(nivoSliderSettings.MobilePicture4Id),
                MobileText4 = nivoSliderSettings.MobileText4,
                MobileLink4 = nivoSliderSettings.MobileLink4,
                MobileAltText4 = nivoSliderSettings.MobileAltText4,

                MobilePicture5Url = await GetPictureUrlAsync(nivoSliderSettings.MobilePicture5Id),
                MobileText5 = nivoSliderSettings.MobileText5,
                MobileLink5 = nivoSliderSettings.MobileLink5,
                MobileAltText5 = nivoSliderSettings.MobileAltText5
            };

            if (isMobileDevice)
            {
                if (string.IsNullOrEmpty(model.MobilePicture1Url) && string.IsNullOrEmpty(model.MobilePicture2Url) &&
                string.IsNullOrEmpty(model.MobilePicture3Url) && string.IsNullOrEmpty(model.MobilePicture4Url) &&
                string.IsNullOrEmpty(model.MobilePicture5Url))
                    //no pictures uploaded
                    return Content("");
            }
            else
            {
                if (string.IsNullOrEmpty(model.Picture1Url) && string.IsNullOrEmpty(model.Picture2Url) &&
                string.IsNullOrEmpty(model.Picture3Url) && string.IsNullOrEmpty(model.Picture4Url) &&
                string.IsNullOrEmpty(model.Picture5Url))
                    //no pictures uploaded
                    return Content("");
            }
            
            return View("~/Plugins/Widgets.NivoSlider/Views/PublicInfo.cshtml", model);
        }

        /// <returns>A task that represents the asynchronous operation</returns>
        protected async Task<string> GetPictureUrlAsync(int pictureId)
        {
            var cacheKey = _staticCacheManager.PrepareKeyForDefaultCache(ModelCacheEventConsumer.PICTURE_URL_MODEL_KEY, 
                pictureId, _webHelper.IsCurrentConnectionSecured() ? Uri.UriSchemeHttps : Uri.UriSchemeHttp);

            return await _staticCacheManager.GetAsync(cacheKey, async () =>
            {
                //little hack here. nulls aren't cacheable so set it to ""
                var url = await _pictureService.GetPictureUrlAsync(pictureId, showDefaultPicture: false) ?? "";
                return url;
            });
        }
    }
}
