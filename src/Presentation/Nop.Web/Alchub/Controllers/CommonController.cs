using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Alchub.Domain;
using Nop.Core.Domain;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Security;
using Nop.Core.Domain.Tax;
using Nop.Core.Domain.Vendors;
using Nop.Services.Alchub.Google;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Html;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Vendors;
using Nop.Web.Factories;
using Nop.Web.Framework.Mvc.Filters;
using Nop.Web.Framework.Themes;
using Nop.Web.Models.Common;

namespace Nop.Web.Controllers
{
    public partial class CommonController
    {
        #region Fields

        private readonly CaptchaSettings _captchaSettings;
        private readonly CommonSettings _commonSettings;
        private readonly ICommonModelFactory _commonModelFactory;
        private readonly ICurrencyService _currencyService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IHtmlFormatter _htmlFormatter;
        private readonly ILanguageService _languageService;
        private readonly ILocalizationService _localizationService;
        private readonly IStoreContext _storeContext;
        private readonly IThemeContext _themeContext;
        private readonly IVendorService _vendorService;
        private readonly IWorkContext _workContext;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly LocalizationSettings _localizationSettings;
        private readonly SitemapSettings _sitemapSettings;
        private readonly SitemapXmlSettings _sitemapXmlSettings;
        private readonly StoreInformationSettings _storeInformationSettings;
        private readonly VendorSettings _vendorSettings;
        private readonly ICustomerService _customerService;
        private readonly IGeoService _geoService;

        #endregion

        #region Ctor

        public CommonController(CaptchaSettings captchaSettings,
            CommonSettings commonSettings,
            ICommonModelFactory commonModelFactory,
            ICurrencyService currencyService,
            ICustomerActivityService customerActivityService,
            IGenericAttributeService genericAttributeService,
            IHtmlFormatter htmlFormatter,
            ILanguageService languageService,
            ILocalizationService localizationService,
            IStoreContext storeContext,
            IThemeContext themeContext,
            IVendorService vendorService,
            IWorkContext workContext,
            IWorkflowMessageService workflowMessageService,
            LocalizationSettings localizationSettings,
            SitemapSettings sitemapSettings,
            SitemapXmlSettings sitemapXmlSettings,
            StoreInformationSettings storeInformationSettings,
            VendorSettings vendorSettings,
            ICustomerService customerService,
            IGeoService geoService)
        {
            _captchaSettings = captchaSettings;
            _commonSettings = commonSettings;
            _commonModelFactory = commonModelFactory;
            _currencyService = currencyService;
            _customerActivityService = customerActivityService;
            _genericAttributeService = genericAttributeService;
            _htmlFormatter = htmlFormatter;
            _languageService = languageService;
            _localizationService = localizationService;
            _storeContext = storeContext;
            _themeContext = themeContext;
            _vendorService = vendorService;
            _workContext = workContext;
            _workflowMessageService = workflowMessageService;
            _localizationSettings = localizationSettings;
            _sitemapSettings = sitemapSettings;
            _sitemapXmlSettings = sitemapXmlSettings;
            _storeInformationSettings = storeInformationSettings;
            _vendorSettings = vendorSettings;
            _customerService = customerService;
            _geoService = geoService;
        }

        #endregion

        #region Methods

        //available even when a store is closed
        [CheckAccessClosedStore(true)]
        public virtual async Task<IActionResult> SetCustomerSearchedCoordinates(string latitude, string longitude, string searchedText, bool clearLocation)
        {
            string result = "";
            var success = false;
            try
            {
                //current customer
                var customer = await _workContext.GetCurrentCustomerAsync();
                if (clearLocation)
                {
                    //clear location
                    customer.LastSearchedCoordinates = null;
                    customer.LastSearchedText = null;
                    //update customer
                    await _customerService.UpdateCustomerAsync(customer);
                    
                    success = true;
                    result = "Location cleaed!";
                }
                else
                {
                    if (string.IsNullOrEmpty(latitude) || string.IsNullOrEmpty(longitude))
                    {
                        result = "Coordinates latlng is missing!";
                    }
                    else
                    {
                        //save location
                        customer.LastSearchedCoordinates = $"{latitude}{NopAlchubDefaults.LATLNG_SEPARATOR}{longitude}";
                        customer.LastSearchedText = searchedText?.Trim();
                        //update customer
                        await _customerService.UpdateCustomerAsync(customer);

                        success = true;
                        result = "Location coordinates saved!";
                    }
                }
            }
            catch (Exception ex)
            {
                result = ex.ToString();
                success = false;
            }

            return Json(new
            {
                Success = success,
                Result = result,
            });
        }

        #endregion
    }
}