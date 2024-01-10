using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Tax;
using Nop.Core.Domain.Vendors;
using Nop.Core.Security;
using Nop.Services.Authentication;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.MultiVendors;
using Nop.Services.Stores;
using Nop.Services.Vendors;

namespace Nop.Web.Framework
{
    /// <summary>
    /// Represents work context for web application
    /// </summary>
    public partial class WebWorkContext : IWorkContext
    {
        #region Fields

        private readonly CookieSettings _cookieSettings;
        private readonly CurrencySettings _currencySettings;
        private readonly IAuthenticationService _authenticationService;
        private readonly ICurrencyService _currencyService;
        private readonly ICustomerService _customerService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILanguageService _languageService;
        private readonly IStoreContext _storeContext;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IUserAgentHelper _userAgentHelper;
        private readonly IVendorService _vendorService;
        private readonly IWebHelper _webHelper;
        private readonly LocalizationSettings _localizationSettings;
        private readonly TaxSettings _taxSettings;
        private readonly IMultiVendorService _multiVendorService;
        private Customer _cachedCustomer;
        private Customer _originalCustomerIfImpersonated;
        private Vendor _cachedVendor;
        private Language _cachedLanguage;
        private Currency _cachedCurrency;
        private TaxDisplayType? _cachedTaxDisplayType;
        private List<int> _cachedGeoRadiusVendorIds;
        private List<int> _cachedGeoFenceVendorIds;

        #endregion

        #region Ctor

        public WebWorkContext(CookieSettings cookieSettings,
            CurrencySettings currencySettings,
            IAuthenticationService authenticationService,
            ICurrencyService currencyService,
            ICustomerService customerService,
            IGenericAttributeService genericAttributeService,
            IHttpContextAccessor httpContextAccessor,
            ILanguageService languageService,
            IStoreContext storeContext,
            IStoreMappingService storeMappingService,
            IUserAgentHelper userAgentHelper,
            IVendorService vendorService,
            IWebHelper webHelper,
            LocalizationSettings localizationSettings,
            TaxSettings taxSettings,
            IMultiVendorService multiVendorService)
        {
            _cookieSettings = cookieSettings;
            _currencySettings = currencySettings;
            _authenticationService = authenticationService;
            _currencyService = currencyService;
            _customerService = customerService;
            _genericAttributeService = genericAttributeService;
            _httpContextAccessor = httpContextAccessor;
            _languageService = languageService;
            _storeContext = storeContext;
            _storeMappingService = storeMappingService;
            _userAgentHelper = userAgentHelper;
            _vendorService = vendorService;
            _webHelper = webHelper;
            _localizationSettings = localizationSettings;
            _taxSettings = taxSettings;
            _multiVendorService = multiVendorService;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the vendor idetifiers who are avialble within geo radius of current customers's searched location 
        /// </summary>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task<IList<int>> GetCurrentCustomerGeoRadiusVendorIdsAsync(Customer customer = null, bool applyToggleFilter = true)
        {
            //whether there is a cached value
            if (_cachedGeoRadiusVendorIds != null)
                return _cachedGeoRadiusVendorIds;

            //mostly we'll pass pre customer when we use this service in api
            if (customer == null)
                customer = await GetCurrentCustomerAsync();

            var vendorsIds = await _vendorService.GetAvailableGeoRadiusVendorIdsAsync(customer, applyToggleFilter);

            //cache the found vendorIds
            _cachedGeoRadiusVendorIds = vendorsIds?.ToList();

            return _cachedGeoRadiusVendorIds;
        }

        /// <summary>
        /// Gets the vendor idetifiers who are avialble within geo fence of current customers's searched location 
        /// </summary>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task<IList<int>> GetAllGeoFenceVendorIdsAsync(Customer customer = null, bool applyToggleFilter = true)
        {
            //whether there is a cached value
            if (_cachedGeoFenceVendorIds != null)
                return _cachedGeoFenceVendorIds;

            //mostly we'll pass pre customer when we use this service in api
            if (customer == null)
                customer = await GetCurrentCustomerAsync();

            var vendorsIds = await _vendorService.GetAvailableGeoFenceVendorsAsync(customer: customer, applyToggleFilter: applyToggleFilter);

            //cache the found vendorIds
            _cachedGeoFenceVendorIds = vendorsIds?.Select(x => x.Id)?.ToList();

            return _cachedGeoFenceVendorIds;
        }

        /// <summary>
        /// Gets the current multi vendor (logged-in manager)
        /// </summary>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task<Customer> GetCurrentMultiVendorAsync()
        {
            var customer = await GetCurrentCustomerAsync();
            if (customer == null)
                return null;

            //check vendor availability
            var managerVendorMapping = (await _multiVendorService.GetManagerVendorMappingAsync(multiVendorId: customer.Id))?.FirstOrDefault();
            if (managerVendorMapping == null)
                return null;

            var multiVendor = await _customerService.GetCustomerByIdAsync(managerVendorMapping.MultiVendorId);
            if (multiVendor == null || multiVendor.Deleted || !multiVendor.Active)
                return null;

            //cache the found multi vendor
            _cachedCustomer = multiVendor;

            return _cachedCustomer;
        }

        #endregion
    }
}