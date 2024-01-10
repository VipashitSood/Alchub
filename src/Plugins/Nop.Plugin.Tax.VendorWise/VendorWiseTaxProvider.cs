using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Tax;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Plugins;
using Nop.Services.Tax;
using Nop.Services.Vendors;

namespace Nop.Plugin.Tax.VendorWise
{
    /// <summary>
    /// Fixed or by vendor wise tax provider
    /// </summary>
    public class VendorWiseTaxProvider : BasePlugin, ITaxProvider
    {
        #region Fields

        private readonly VendorWiseTaxSettings _countryStateZipSettings;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILocalizationService _localizationService;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly IPaymentService _paymentService;
        private readonly ISettingService _settingService;
        private readonly IStaticCacheManager _staticCacheManager;
        private readonly ITaxCategoryService _taxCategoryService;
        private readonly ITaxService _taxService;
        private readonly IWebHelper _webHelper;
        private readonly TaxSettings _taxSettings;
        private readonly IVendorService _vendorService;
        private readonly IProductService _productService;
        private readonly IStoreContext _storeContext;
        #endregion

        #region Ctor

        public VendorWiseTaxProvider(VendorWiseTaxSettings countryStateZipSettings,
            IGenericAttributeService genericAttributeService,
            IHttpContextAccessor httpContextAccessor,
            ILocalizationService localizationService,
            IOrderTotalCalculationService orderTotalCalculationService,
            IPaymentService paymentService,
            ISettingService settingService,
            IStaticCacheManager staticCacheManager,
            ITaxCategoryService taxCategoryService,
            ITaxService taxService,
            IWebHelper webHelper,
            TaxSettings taxSettings,
            IVendorService vendorService,
            IProductService productService,
            IStoreContext storeContext)
        {
            _countryStateZipSettings = countryStateZipSettings;
            _genericAttributeService = genericAttributeService;
            _httpContextAccessor = httpContextAccessor;
            _localizationService = localizationService;
            _orderTotalCalculationService = orderTotalCalculationService;
            _paymentService = paymentService;
            _settingService = settingService;
            _staticCacheManager = staticCacheManager;
            _taxCategoryService = taxCategoryService;
            _taxService = taxService;
            _webHelper = webHelper;
            _taxSettings = taxSettings;
            _vendorService = vendorService;
            _productService = productService;
            _storeContext = storeContext;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets tax rate
        /// </summary>
        /// <param name="taxRateRequest">Tax rate request</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the ax
        /// </returns>
        public async Task<TaxRateResult> GetTaxRateAsync(TaxRateRequest taxRateRequest)
        {
            var result = new TaxRateResult();

            if (taxRateRequest?.Product != null)
            {
                var storeScope = await _storeContext.GetCurrentStoreAsync();
                var vendorWiseTaxSettings = await _settingService.LoadSettingAsync<VendorWiseTaxSettings>(storeScope.Id);
                if (vendorWiseTaxSettings.Enable)
                {
                    var vendor = await _vendorService.GetVendorByProductIdAsync(taxRateRequest.Product.Id);
                    result.TaxRate = vendor?.OrderTax ?? decimal.Zero;
                }
            }
            
            return result;
        }

        /// <summary>
        /// Gets tax total
        /// </summary>
        /// <param name="taxTotalRequest">Tax total request</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the ax total
        /// </returns>
        public async Task<TaxTotalResult> GetTaxTotalAsync(TaxTotalRequest taxTotalRequest)
        {
            if (_httpContextAccessor.HttpContext.Items.TryGetValue("nop.TaxTotal", out var result)
                && result is (TaxTotalResult taxTotalResult, decimal paymentTax))
            {
                //short-circuit to avoid circular reference when calculating payment method additional fee during the checkout process
                if (!taxTotalRequest.UsePaymentMethodAdditionalFee)
                    return new TaxTotalResult { TaxTotal = taxTotalResult.TaxTotal - paymentTax };

                return taxTotalResult;
            }

            var taxRates = new SortedDictionary<decimal, decimal>();
            var taxTotal = decimal.Zero;

            //order sub total (items + checkout attributes)
            var (_, _, _, _, orderSubTotalTaxRates) = await _orderTotalCalculationService
                .GetShoppingCartSubTotalAsync(taxTotalRequest.ShoppingCart, false);
            var subTotalTaxTotal = decimal.Zero;
            foreach (var kvp in orderSubTotalTaxRates)
            {
                var taxRate = kvp.Key;
                var taxValue = kvp.Value;
                subTotalTaxTotal += taxValue;

                if (taxRate > decimal.Zero && taxValue > decimal.Zero)
                {
                    if (!taxRates.ContainsKey(taxRate))
                        taxRates.Add(taxRate, taxValue);
                    else
                        taxRates[taxRate] = taxRates[taxRate] + taxValue;
                }
            }
            taxTotal += subTotalTaxTotal;

            //shipping
            var shippingTax = decimal.Zero;
            if (_taxSettings.ShippingIsTaxable)
            {
                var (shippingExclTax, _, _) = await _orderTotalCalculationService
                    .GetShoppingCartShippingTotalAsync(taxTotalRequest.ShoppingCart, false);
                var (shippingInclTax, taxRate, _) = await _orderTotalCalculationService
                    .GetShoppingCartShippingTotalAsync(taxTotalRequest.ShoppingCart, true);
                if (shippingExclTax.HasValue && shippingInclTax.HasValue)
                {
                    shippingTax = shippingInclTax.Value - shippingExclTax.Value;
                    if (shippingTax < decimal.Zero)
                        shippingTax = decimal.Zero;

                    if (taxRate > decimal.Zero && shippingTax > decimal.Zero)
                    {
                        if (!taxRates.ContainsKey(taxRate))
                            taxRates.Add(taxRate, shippingTax);
                        else
                            taxRates[taxRate] = taxRates[taxRate] + shippingTax;
                    }
                }
            }
            taxTotal += shippingTax;

            //short-circuit to avoid circular reference when calculating payment method additional fee during the checkout process
            if (!taxTotalRequest.UsePaymentMethodAdditionalFee)
                return new TaxTotalResult { TaxTotal = taxTotal };

            //payment method additional fee
            var paymentMethodAdditionalFeeTax = decimal.Zero;
            if (_taxSettings.PaymentMethodAdditionalFeeIsTaxable)
            {
                var paymentMethodSystemName = taxTotalRequest.Customer != null
                    ? await _genericAttributeService
                        .GetAttributeAsync<string>(taxTotalRequest.Customer, NopCustomerDefaults.SelectedPaymentMethodAttribute, taxTotalRequest.StoreId)
                    : string.Empty;

                var paymentMethodAdditionalFee = await _paymentService
                    .GetAdditionalHandlingFeeAsync(taxTotalRequest.ShoppingCart, paymentMethodSystemName);
                var (paymentMethodAdditionalFeeExclTax, _) = await _taxService
                    .GetPaymentMethodAdditionalFeeAsync(paymentMethodAdditionalFee, false, taxTotalRequest.Customer);
                var (paymentMethodAdditionalFeeInclTax, taxRate) = await _taxService
                    .GetPaymentMethodAdditionalFeeAsync(paymentMethodAdditionalFee, true, taxTotalRequest.Customer);

                paymentMethodAdditionalFeeTax = paymentMethodAdditionalFeeInclTax - paymentMethodAdditionalFeeExclTax;
                if (paymentMethodAdditionalFeeTax < decimal.Zero)
                    paymentMethodAdditionalFeeTax = decimal.Zero;

                if (taxRate > decimal.Zero && paymentMethodAdditionalFeeTax > decimal.Zero)
                {
                    if (!taxRates.ContainsKey(taxRate))
                        taxRates.Add(taxRate, paymentMethodAdditionalFeeTax);
                    else
                        taxRates[taxRate] = taxRates[taxRate] + paymentMethodAdditionalFeeTax;
                }
            }
            taxTotal += paymentMethodAdditionalFeeTax;

            //add at least one tax rate (0%)
            if (!taxRates.Any())
                taxRates.Add(decimal.Zero, decimal.Zero);

            if (taxTotal < decimal.Zero)
                taxTotal = decimal.Zero;

            taxTotalResult = new TaxTotalResult { TaxTotal = taxTotal, TaxRates = taxRates, };

            //store values within the scope of the request to avoid duplicate calculations
            _httpContextAccessor.HttpContext.Items.TryAdd("nop.TaxTotal", (taxTotalResult, paymentMethodAdditionalFeeTax));

            return taxTotalResult;
        }

        /// <summary>
        /// Gets a configuration page URL
        /// </summary>
        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/VendorWiseTax/Configure";
        }

        /// <summary>
        /// Install plugin
        /// </summary>
        /// <returns>A task that represents the asynchronous operation</returns>
        public override async Task InstallAsync()
        {
            //settings
            await _settingService.SaveSettingAsync(new VendorWiseTaxSettings
            {
                Enable = true
            });

            //locales
            await _localizationService.AddOrUpdateLocaleResourceAsync(new Dictionary<string, string>
            {
                ["Plugins.Tax.VendorWiseTax.Fixed"] = "Fixed rate",
                ["Plugins.Tax.VendorWiseTax.Tax.Categories.Manage"] = "Manage tax categories",
                ["Plugins.Tax.VendorWiseTax.TaxCategoriesCanNotLoaded"] = "No tax categories can be loaded. You may manage tax categories by <a href='{0}'>this link</a>",
                ["Plugins.Tax.VendorWiseTax.TaxByCountryStateZip"] = "By Country",
                ["Plugins.Tax.VendorWiseTax.Fields.TaxCategoryName"] = "Tax category",
                ["Plugins.Tax.VendorWiseTax.Fields.Rate"] = "Rate",
                ["Plugins.Tax.VendorWiseTax.Fields.Store"] = "Store",
                ["Plugins.Tax.VendorWiseTax.Fields.Store.Hint"] = "If an asterisk is selected, then this shipping rate will apply to all stores.",
                ["Plugins.Tax.VendorWiseTax.Fields.Country"] = "Country",
                ["Plugins.Tax.VendorWiseTax.Fields.Country.Hint"] = "The country.",
                ["Plugins.Tax.VendorWiseTax.Fields.StateProvince"] = "State / province",
                ["Plugins.Tax.VendorWiseTax.Fields.StateProvince.Hint"] = "If an asterisk is selected, then this tax rate will apply to all customers from the given country, regardless of the state.",
                ["Plugins.Tax.VendorWiseTax.Fields.Zip"] = "Zip",
                ["Plugins.Tax.VendorWiseTax.Fields.Zip.Hint"] = "Zip / postal code. If zip is empty, then this tax rate will apply to all customers from the given country or state, regardless of the zip code.",
                ["Plugins.Tax.VendorWiseTax.Fields.TaxCategory"] = "Tax category",
                ["Plugins.Tax.VendorWiseTax.Fields.TaxCategory.Hint"] = "The tax category.",
                ["Plugins.Tax.VendorWiseTax.Fields.Percentage"] = "Percentage",
                ["Plugins.Tax.VendorWiseTax.Fields.Percentage.Hint"] = "The tax rate.",
                ["Plugins.Tax.VendorWiseTax.AddRecord"] = "Add tax rate",
                ["Plugins.Tax.VendorWiseTax.AddRecordTitle"] = "New tax rate"
            });

            await base.InstallAsync();
        }

        /// <summary>
        /// Uninstall plugin
        /// </summary>
        /// <returns>A task that represents the asynchronous operation</returns>
        public override async Task UninstallAsync()
        {
            //settings
            await _settingService.DeleteSettingAsync<VendorWiseTaxSettings>();

            //fixed rates
            var fixedRates = await (await _taxCategoryService.GetAllTaxCategoriesAsync())
                .SelectAwait(async taxCategory => await _settingService.GetSettingAsync(string.Format(VendorWiseTaxDefaults.FixedRateSettingsKey, taxCategory.Id)))
                .Where(setting => setting != null).ToListAsync();
            await _settingService.DeleteSettingsAsync(fixedRates);

            //locales
            await _localizationService.DeleteLocaleResourcesAsync("Plugins.Tax.VendorWiseTax");

            await base.UninstallAsync();
        }


        protected virtual decimal CalculatePrice(decimal price, decimal percent, bool increase)
        {
            if (percent == decimal.Zero)
                return price;

            decimal result;
            if (increase)
                result = price * (1 + percent / 100);
            else
                result = price - price / (100 + percent) * percent;

            return result;
        }
        #endregion
    }
}