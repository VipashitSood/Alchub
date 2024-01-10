using System.Threading.Tasks;
using Nop.Core.Alchub.Domain.Twillio;
using Nop.Core.Domain.Settings;
using Nop.Core.Domain.Vendors;
using Nop.Services;
using Nop.Web.Areas.Admin.Alchub.Models.Settings;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Settings;

namespace Nop.Web.Areas.Admin.Factories
{
    /// <summary>
    /// Represents the setting model factory
    /// </summary>
    public partial class SettingModelFactory : ISettingModelFactory
    {
        #region methods

        /// <summary>
        /// Prepare service fee settings model
        /// </summary>
        /// <param name="model"></param>
        /// <returns>A task that represents the asynchronous operation The task result contains the news settings model</returns>
        public virtual async Task<ServiceFeeSettingsModel> PrepareServiceFeeSettingsModelAsync(ServiceFeeSettingsModel model = null)
        {
            //load settings for a chosen store scope
            var storeId = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var serviceFeeSettings = await _settingService.LoadSettingAsync<ServiceFeeSettings>(storeId);

            //fill in model values from the entity
            model ??= model = new ServiceFeeSettingsModel();

            model.ServiceFeeTypeId = serviceFeeSettings.ServiceFeeTypeId;
            model.ServiceFee = serviceFeeSettings.ServiceFee;
            model.ServiceFeePercentage = serviceFeeSettings.ServiceFeePercentage;
            model.MaximumServiceFee = serviceFeeSettings.MaximumServiceFee;

            //fill in additional values (not existing in the entity)
            model.ActiveStoreScopeConfiguration = storeId;

            if (storeId <= 0)
                return model;

            //fill in overridden values
            model.ServiceFeeTypeId_OverrideForStore = await _settingService.SettingExistsAsync(serviceFeeSettings, x => x.ServiceFeeTypeId, storeId);
            model.ServiceFee_OverrideForStore = await _settingService.SettingExistsAsync(serviceFeeSettings, x => x.ServiceFee, storeId);
            model.ServiceFeePercentage_OverrideForStore = await _settingService.SettingExistsAsync(serviceFeeSettings, x => x.ServiceFeePercentage, storeId);
            model.MaximumServiceFee_OverrideForStore = await _settingService.SettingExistsAsync(serviceFeeSettings, x => x.MaximumServiceFee, storeId);

            return model;
        }

        /// <summary>
        /// Prepare vendor settings model
        /// </summary>
        /// <param name="model">Vendor settings model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the vendor settings model
        /// </returns>
        public virtual async Task<VendorSettingsModel> PrepareVendorSettingsModelAsync(VendorSettingsModel model = null)
        {
            //load settings for a chosen store scope
            var storeId = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var vendorSettings = await _settingService.LoadSettingAsync<VendorSettings>(storeId);

            //fill in model values from the entity
            model ??= vendorSettings.ToSettingsModel<VendorSettingsModel>();

            //fill in additional values (not existing in the entity)
            model.ActiveStoreScopeConfiguration = storeId;

            //alchub
            model.DistanceUnit = (int)vendorSettings.DistanceUnit;
            model.DistanceUnitValues = await vendorSettings.DistanceUnit.ToSelectListAsync();

            //fill in overridden values
            if (storeId > 0)
            {
                model.VendorsBlockItemsToDisplay_OverrideForStore = await _settingService.SettingExistsAsync(vendorSettings, x => x.VendorsBlockItemsToDisplay, storeId);
                model.ShowVendorOnProductDetailsPage_OverrideForStore = await _settingService.SettingExistsAsync(vendorSettings, x => x.ShowVendorOnProductDetailsPage, storeId);
                model.ShowVendorOnOrderDetailsPage_OverrideForStore = await _settingService.SettingExistsAsync(vendorSettings, x => x.ShowVendorOnOrderDetailsPage, storeId);
                model.AllowCustomersToContactVendors_OverrideForStore = await _settingService.SettingExistsAsync(vendorSettings, x => x.AllowCustomersToContactVendors, storeId);
                model.AllowCustomersToApplyForVendorAccount_OverrideForStore = await _settingService.SettingExistsAsync(vendorSettings, x => x.AllowCustomersToApplyForVendorAccount, storeId);
                model.TermsOfServiceEnabled_OverrideForStore = await _settingService.SettingExistsAsync(vendorSettings, x => x.TermsOfServiceEnabled, storeId);
                model.AllowSearchByVendor_OverrideForStore = await _settingService.SettingExistsAsync(vendorSettings, x => x.AllowSearchByVendor, storeId);
                model.AllowVendorsToEditInfo_OverrideForStore = await _settingService.SettingExistsAsync(vendorSettings, x => x.AllowVendorsToEditInfo, storeId);
                model.NotifyStoreOwnerAboutVendorInformationChange_OverrideForStore = await _settingService.SettingExistsAsync(vendorSettings, x => x.NotifyStoreOwnerAboutVendorInformationChange, storeId);
                model.MaximumProductNumber_OverrideForStore = await _settingService.SettingExistsAsync(vendorSettings, x => x.MaximumProductNumber, storeId);
                model.AllowVendorsToImportProducts_OverrideForStore = await _settingService.SettingExistsAsync(vendorSettings, x => x.AllowVendorsToImportProducts, storeId);
                //Alchub custom++
                model.DistanceRadiusValue_OverrideForStore = await _settingService.SettingExistsAsync(vendorSettings, x => x.DistanceRadiusValue, storeId);
                model.DistanceUnit_OverrideForStore = await _settingService.SettingExistsAsync(vendorSettings, x => x.DistanceUnit, storeId);
                model.ShowStoreAddressInFavoriteSection_OverrideForStore = await _settingService.SettingExistsAsync(vendorSettings, x => x.ShowStoreAddressInFavoriteSection, storeId);
            }

            //prepare nested search model
            await _vendorAttributeModelFactory.PrepareVendorAttributeSearchModelAsync(model.VendorAttributeSearchModel);

            return model;
        }

        #region Twillio

        /// <summary>
        /// Prepare twillio settings model
        /// </summary>
        /// <param name="model">Twillio settings model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the vendor settings model
        /// </returns>
        public async Task<TwillioSettingsModel> PrepareTwillioSettingsModelAsync(TwillioSettingsModel model = null)
        {
            //load settings for a chosen store scope
            var storeId = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var twillioSettings = await _settingService.LoadSettingAsync<TwillioSettings>(storeId);

            //fill in model values from the entity
            model ??= twillioSettings.ToSettingsModel<TwillioSettingsModel>();

            //fill in additional values (not existing in the entity)
            model.ActiveStoreScopeConfiguration = storeId;

            //fill in overridden values
            if (storeId > 0)
            {
                model.Enabled_OverrideForStore = await _settingService.SettingExistsAsync(twillioSettings, x => x.Enabled, storeId);
                model.AccountSid_OverrideForStore = await _settingService.SettingExistsAsync(twillioSettings, x => x.AccountSid, storeId);
                model.AuthToken_OverrideForStore = await _settingService.SettingExistsAsync(twillioSettings, x => x.AuthToken, storeId);
                model.FromNumber_OverrideForStore = await _settingService.SettingExistsAsync(twillioSettings, x => x.FromNumber, storeId);
                model.DefaultCountryCode_OverrideForStore = await _settingService.SettingExistsAsync(twillioSettings, x => x.DefaultCountryCode, storeId);
                model.OrderPlacedBody_OverrideForStore = await _settingService.SettingExistsAsync(twillioSettings, x => x.OrderPlacedBody, storeId);
                model.OrderItemsDispatchedBody_OverrideForStore = await _settingService.SettingExistsAsync(twillioSettings, x => x.OrderItemsDispatchedBody, storeId);
                model.OrderItemsPickedUpBody_OverrideForStore = await _settingService.SettingExistsAsync(twillioSettings, x => x.OrderItemsPickedUpBody, storeId);
                model.OrderItemsDeliveredBody_OverrideForStore = await _settingService.SettingExistsAsync(twillioSettings, x => x.OrderItemsDeliveredBody, storeId);
                model.OrderItemsCancelBody_OverrideForStore = await _settingService.SettingExistsAsync(twillioSettings, x => x.OrderItemsCancelBody, storeId);
                model.OrderItemsDelivereyDeniedBody_OverrideForStore = await _settingService.SettingExistsAsync(twillioSettings, x => x.OrderItemsDelivereyDeniedBody, storeId);
                model.OrderItemsPickedUpBody_OverrideForStore = await _settingService.SettingExistsAsync(twillioSettings, x => x.OrderItemPickupBody, storeId);
                model.OrderPlacedVendorBody_OverrideForStore = await _settingService.SettingExistsAsync(twillioSettings, x => x.OrderPlacedVendorBody, storeId);
            }

            return model;
        }

        #endregion

        #endregion
    }
}
