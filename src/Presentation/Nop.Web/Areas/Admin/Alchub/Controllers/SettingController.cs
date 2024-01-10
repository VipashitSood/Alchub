using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Alchub.Domain.Google;
using Nop.Core.Alchub.Domain.ServiceFee;
using Nop.Core.Alchub.Domain.Twillio;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Settings;
using Nop.Core.Domain.Vendors;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Alchub.Models.Settings;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Settings;

namespace Nop.Web.Areas.Admin.Controllers
{
    public partial class SettingController : BaseAdminController
    {
        #region methods

        #region ServiceFee

        public virtual async Task<IActionResult> ServiceFee()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            //prepare model
            var model = await _settingModelFactory.PrepareServiceFeeSettingsModelAsync();

            return View(model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> ServiceFee(ServiceFeeSettingsModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                //load settings for a chosen store scope
                var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
                var serviceFeeSettings = await _settingService.LoadSettingAsync<ServiceFeeSettings>(storeScope);

                //prepare settings
                serviceFeeSettings.ServiceFeeTypeId = model.ServiceFeeTypeId;
                serviceFeeSettings.ServiceFee = model.ServiceFee;
                serviceFeeSettings.ServiceFeePercentage = model.ServiceFeePercentage;
                serviceFeeSettings.MaximumServiceFee = model.MaximumServiceFee;

                //we do not clear cache after each setting update.
                //this behavior can increase performance because cached settings will not be cleared 
                //and loaded from database after each update
                if (serviceFeeSettings.ServiceFeeTypeId == (int)ServiceFeeType.Percentage)
                {
                    await _settingService.SaveSettingOverridablePerStoreAsync(serviceFeeSettings, x => x.ServiceFeeTypeId, model.ServiceFeeTypeId_OverrideForStore, storeScope, false);
                    await _settingService.SaveSettingOverridablePerStoreAsync(serviceFeeSettings, x => x.ServiceFeePercentage, model.ServiceFeePercentage_OverrideForStore, storeScope, false);
                    await _settingService.SaveSettingOverridablePerStoreAsync(serviceFeeSettings, x => x.MaximumServiceFee, model.MaximumServiceFee_OverrideForStore, storeScope, false);
                }
                else
                {
                    await _settingService.SaveSettingOverridablePerStoreAsync(serviceFeeSettings, x => x.ServiceFeeTypeId, model.ServiceFeeTypeId_OverrideForStore, storeScope, false);
                    await _settingService.SaveSettingOverridablePerStoreAsync(serviceFeeSettings, x => x.ServiceFee, model.ServiceFee_OverrideForStore, storeScope, false);
                }

                //now clear settings cache
                await _settingService.ClearCacheAsync();

                //activity log
                await _customerActivityService.InsertActivityAsync("EditSettings", await _localizationService.GetResourceAsync("ActivityLog.EditSettings"));

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Configuration.Updated"));

                return RedirectToAction("ServiceFee");
            }

            //prepare model
            model = await _settingModelFactory.PrepareServiceFeeSettingsModelAsync(model);

            //if we got this far, something failed, redisplay form
            return View(model);
        }

        #endregion

        #region Vendor

        [HttpPost]
        public virtual async Task<IActionResult> Vendor(VendorSettingsModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                //load settings for a chosen store scope
                var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
                var vendorSettings = await _settingService.LoadSettingAsync<VendorSettings>(storeScope);
                vendorSettings = model.ToSettings(vendorSettings);

                //alchub custom
                vendorSettings.DistanceUnit = (DistanceUnit)model.DistanceUnit;

                //we do not clear cache after each setting update.
                //this behavior can increase performance because cached settings will not be cleared 
                //and loaded from database after each update
                await _settingService.SaveSettingOverridablePerStoreAsync(vendorSettings, x => x.VendorsBlockItemsToDisplay, model.VendorsBlockItemsToDisplay_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(vendorSettings, x => x.ShowVendorOnProductDetailsPage, model.ShowVendorOnProductDetailsPage_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(vendorSettings, x => x.ShowVendorOnOrderDetailsPage, model.ShowVendorOnOrderDetailsPage_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(vendorSettings, x => x.AllowCustomersToContactVendors, model.AllowCustomersToContactVendors_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(vendorSettings, x => x.AllowCustomersToApplyForVendorAccount, model.AllowCustomersToApplyForVendorAccount_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(vendorSettings, x => x.TermsOfServiceEnabled, model.TermsOfServiceEnabled_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(vendorSettings, x => x.AllowSearchByVendor, model.AllowSearchByVendor_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(vendorSettings, x => x.AllowVendorsToEditInfo, model.AllowVendorsToEditInfo_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(vendorSettings, x => x.NotifyStoreOwnerAboutVendorInformationChange, model.NotifyStoreOwnerAboutVendorInformationChange_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(vendorSettings, x => x.MaximumProductNumber, model.MaximumProductNumber_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(vendorSettings, x => x.AllowVendorsToImportProducts, model.AllowVendorsToImportProducts_OverrideForStore, storeScope, false);
                //Alchub custom++
                await _settingService.SaveSettingOverridablePerStoreAsync(vendorSettings, x => x.DistanceRadiusValue, model.DistanceRadiusValue_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(vendorSettings, x => x.DistanceUnit, model.DistanceUnit_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(vendorSettings, x => x.ShowStoreAddressInFavoriteSection, model.ShowStoreAddressInFavoriteSection_OverrideForStore, storeScope, false);

                //now clear settings cache
                await _settingService.ClearCacheAsync();

                //activity log
                await _customerActivityService.InsertActivityAsync("EditSettings", await _localizationService.GetResourceAsync("ActivityLog.EditSettings"));

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Configuration.Updated"));

                return RedirectToAction("Vendor");
            }

            //prepare model
            model = await _settingModelFactory.PrepareVendorSettingsModelAsync(model);

            //if we got this far, something failed, redisplay form
            return View(model);
        }

        #endregion

        #region Twillio

        public virtual async Task<IActionResult> Twillio()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageTwillioSettings))
                return AccessDeniedView();

            //prepare model
            var model = await _settingModelFactory.PrepareTwillioSettingsModelAsync();

            return View(model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> Twillio(TwillioSettingsModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageTwillioSettings))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                //load settings for a chosen store scope
                var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
                var twillioSettings = await _settingService.LoadSettingAsync<TwillioSettings>(storeScope);
                twillioSettings = model.ToSettings(twillioSettings);

                //we do not clear cache after each setting update.
                //this behavior can increase performance because cached settings will not be cleared 
                //and loaded from database after each update
                await _settingService.SaveSettingOverridablePerStoreAsync(twillioSettings, x => x.Enabled, model.Enabled_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(twillioSettings, x => x.AccountSid, model.AccountSid_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(twillioSettings, x => x.AuthToken, model.AuthToken_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(twillioSettings, x => x.FromNumber, model.FromNumber_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(twillioSettings, x => x.DefaultCountryCode, model.DefaultCountryCode_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(twillioSettings, x => x.OrderPlacedBody, model.OrderPlacedBody_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(twillioSettings, x => x.OrderItemsDispatchedBody, model.OrderItemsDispatchedBody_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(twillioSettings, x => x.OrderItemsPickedUpBody, model.OrderItemsPickedUpBody_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(twillioSettings, x => x.OrderItemsDeliveredBody, model.OrderItemsDeliveredBody_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(twillioSettings, x => x.OrderItemsCancelBody, model.OrderItemsCancelBody_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(twillioSettings, x => x.OrderItemsDelivereyDeniedBody, model.OrderItemsDelivereyDeniedBody_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(twillioSettings, x => x.OrderItemPickupBody, model.OrderItemsPickedUpBody_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(twillioSettings, x => x.OrderPlacedVendorBody, model.OrderPlacedVendorBody_OverrideForStore, storeScope, false);
                //now clear settings cache
                await _settingService.ClearCacheAsync();

                //activity log
                await _customerActivityService.InsertActivityAsync("EditSettings", await _localizationService.GetResourceAsync("ActivityLog.EditSettings"));

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Configuration.Updated"));

                return RedirectToAction("Twillio");
            }

            //prepare model
            model = await _settingModelFactory.PrepareTwillioSettingsModelAsync(model);

            //if we got this far, something failed, redisplay form
            return View(model);
        }

        #endregion

        #region Catalog

        [HttpPost]
        public virtual async Task<IActionResult> Catalog(CatalogSettingsModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                //load settings for a chosen store scope
                var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
                var catalogSettings = await _settingService.LoadSettingAsync<CatalogSettings>(storeScope);
                catalogSettings = model.ToSettings(catalogSettings);

                //we do not clear cache after each setting update.
                //this behavior can increase performance because cached settings will not be cleared 
                //and loaded from database after each update
                await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.AllowViewUnpublishedProductPage, model.AllowViewUnpublishedProductPage_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.DisplayDiscontinuedMessageForUnpublishedProducts, model.DisplayDiscontinuedMessageForUnpublishedProducts_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.ShowSkuOnProductDetailsPage, model.ShowSkuOnProductDetailsPage_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.ShowSkuOnCatalogPages, model.ShowSkuOnCatalogPages_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.ShowManufacturerPartNumber, model.ShowManufacturerPartNumber_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.ShowGtin, model.ShowGtin_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.ShowFreeShippingNotification, model.ShowFreeShippingNotification_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.AllowProductSorting, model.AllowProductSorting_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.AllowProductViewModeChanging, model.AllowProductViewModeChanging_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.DefaultViewMode, model.DefaultViewMode_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.ShowProductsFromSubcategories, model.ShowProductsFromSubcategories_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.ShowCategoryProductNumber, model.ShowCategoryProductNumber_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.ShowCategoryProductNumberIncludingSubcategories, model.ShowCategoryProductNumberIncludingSubcategories_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.CategoryBreadcrumbEnabled, model.CategoryBreadcrumbEnabled_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.ShowShareButton, model.ShowShareButton_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.PageShareCode, model.PageShareCode_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.ProductReviewsMustBeApproved, model.ProductReviewsMustBeApproved_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.OneReviewPerProductFromCustomer, model.OneReviewPerProductFromCustomer, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.AllowAnonymousUsersToReviewProduct, model.AllowAnonymousUsersToReviewProduct_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.ProductReviewPossibleOnlyAfterPurchasing, model.ProductReviewPossibleOnlyAfterPurchasing_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.NotifyStoreOwnerAboutNewProductReviews, model.NotifyStoreOwnerAboutNewProductReviews_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.NotifyCustomerAboutProductReviewReply, model.NotifyCustomerAboutProductReviewReply_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.EmailAFriendEnabled, model.EmailAFriendEnabled_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.AllowAnonymousUsersToEmailAFriend, model.AllowAnonymousUsersToEmailAFriend_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.RecentlyViewedProductsNumber, model.RecentlyViewedProductsNumber_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.RecentlyViewedProductsEnabled, model.RecentlyViewedProductsEnabled_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.NewProductsNumber, model.NewProductsNumber_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.NewProductsEnabled, model.NewProductsEnabled_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.CompareProductsEnabled, model.CompareProductsEnabled_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.ShowBestsellersOnHomepage, model.ShowBestsellersOnHomepage_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.NumberOfBestsellersOnHomepage, model.NumberOfBestsellersOnHomepage_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.SearchPageProductsPerPage, model.SearchPageProductsPerPage_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.SearchPageAllowCustomersToSelectPageSize, model.SearchPageAllowCustomersToSelectPageSize_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.SearchPagePageSizeOptions, model.SearchPagePageSizeOptions_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.SearchPagePriceRangeFiltering, model.SearchPagePriceRangeFiltering_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.SearchPagePriceFrom, model.SearchPagePriceFrom_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.SearchPagePriceTo, model.SearchPagePriceTo_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.SearchPageManuallyPriceRange, model.SearchPageManuallyPriceRange_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.ProductSearchAutoCompleteEnabled, model.ProductSearchAutoCompleteEnabled_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.ProductSearchEnabled, model.ProductSearchEnabled_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.ProductSearchAutoCompleteNumberOfProducts, model.ProductSearchAutoCompleteNumberOfProducts_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.ShowProductImagesInSearchAutoComplete, model.ShowProductImagesInSearchAutoComplete_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.ShowLinkToAllResultInSearchAutoComplete, model.ShowLinkToAllResultInSearchAutoComplete_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.ProductSearchTermMinimumLength, model.ProductSearchTermMinimumLength_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.ProductsAlsoPurchasedEnabled, model.ProductsAlsoPurchasedEnabled_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.ProductsAlsoPurchasedNumber, model.ProductsAlsoPurchasedNumber_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.NumberOfProductTags, model.NumberOfProductTags_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.ProductsByTagPageSize, model.ProductsByTagPageSize_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.ProductsByTagAllowCustomersToSelectPageSize, model.ProductsByTagAllowCustomersToSelectPageSize_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.ProductsByTagPageSizeOptions, model.ProductsByTagPageSizeOptions_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.ProductsByTagPriceRangeFiltering, model.ProductsByTagPriceRangeFiltering_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.ProductsByTagPriceFrom, model.ProductsByTagPriceFrom_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.ProductsByTagPriceTo, model.ProductsByTagPriceTo_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.ProductsByTagManuallyPriceRange, model.ProductsByTagManuallyPriceRange_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.IncludeShortDescriptionInCompareProducts, model.IncludeShortDescriptionInCompareProducts_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.IncludeFullDescriptionInCompareProducts, model.IncludeFullDescriptionInCompareProducts_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.ManufacturersBlockItemsToDisplay, model.ManufacturersBlockItemsToDisplay_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.DisplayTaxShippingInfoFooter, model.DisplayTaxShippingInfoFooter_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.DisplayTaxShippingInfoProductDetailsPage, model.DisplayTaxShippingInfoProductDetailsPage_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.DisplayTaxShippingInfoProductBoxes, model.DisplayTaxShippingInfoProductBoxes_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.DisplayTaxShippingInfoShoppingCart, model.DisplayTaxShippingInfoShoppingCart_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.DisplayTaxShippingInfoWishlist, model.DisplayTaxShippingInfoWishlist_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.DisplayTaxShippingInfoOrderDetailsPage, model.DisplayTaxShippingInfoOrderDetailsPage_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.ShowProductReviewsPerStore, model.ShowProductReviewsPerStore_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.ShowProductReviewsTabOnAccountPage, model.ShowProductReviewsOnAccountPage_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.ProductReviewsPageSizeOnAccountPage, model.ProductReviewsPageSizeOnAccountPage_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.ProductReviewsSortByCreatedDateAscending, model.ProductReviewsSortByCreatedDateAscending_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.ExportImportProductAttributes, model.ExportImportProductAttributes_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.ExportImportProductSpecificationAttributes, model.ExportImportProductSpecificationAttributes_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.ExportImportProductCategoryBreadcrumb, model.ExportImportProductCategoryBreadcrumb_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.ExportImportCategoriesUsingCategoryName, model.ExportImportCategoriesUsingCategoryName_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.ExportImportAllowDownloadImages, model.ExportImportAllowDownloadImages_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.ExportImportSplitProductsFile, model.ExportImportSplitProductsFile_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.RemoveRequiredProducts, model.RemoveRequiredProducts_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.ExportImportRelatedEntitiesByName, model.ExportImportRelatedEntitiesByName_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.ExportImportProductUseLimitedToStores, model.ExportImportProductUseLimitedToStores_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.DisplayDatePreOrderAvailability, model.DisplayDatePreOrderAvailability_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.UseAjaxCatalogProductsLoading, model.UseAjaxCatalogProductsLoading_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.EnableManufacturerFiltering, model.EnableManufacturerFiltering_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.EnablePriceRangeFiltering, model.EnablePriceRangeFiltering_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.EnableSpecificationAttributeFiltering, model.EnableSpecificationAttributeFiltering_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.AttributeValueOutOfStockDisplayType, model.AttributeValueOutOfStockDisplayType_OverrideForStore, storeScope, false);
                //++Alchub
                await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.ShowFastestSlotOnCatalogPage, model.ShowFastestSlotOnCatalogPage_OverrideForStore, storeScope, false);
                //filter price range options
                await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.FilterPriceRangeOption1, model.FilterPriceRangeOption1_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.FilterPriceRangeOption2, model.FilterPriceRangeOption2_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.FilterPriceRangeOption3, model.FilterPriceRangeOption3_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(catalogSettings, x => x.NumberOfManufacturersOnHomepage, model.NumberOfManufacturersOnHomepage_OverrideForStore, storeScope, false);
                //--Alchub

                //now settings not overridable per store
                await _settingService.SaveSettingAsync(catalogSettings, x => x.IgnoreDiscounts, 0, false);
                await _settingService.SaveSettingAsync(catalogSettings, x => x.IgnoreFeaturedProducts, 0, false);
                await _settingService.SaveSettingAsync(catalogSettings, x => x.IgnoreAcl, 0, false);
                await _settingService.SaveSettingAsync(catalogSettings, x => x.IgnoreStoreLimitations, 0, false);
                await _settingService.SaveSettingAsync(catalogSettings, x => x.CacheProductPrices, 0, false);

                //now clear settings cache
                await _settingService.ClearCacheAsync();

                //activity log
                await _customerActivityService.InsertActivityAsync("EditSettings", await _localizationService.GetResourceAsync("ActivityLog.EditSettings"));

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Configuration.Updated"));

                return RedirectToAction("Catalog");
            }

            //prepare model
            model = await _settingModelFactory.PrepareCatalogSettingsModelAsync(model);

            //if we got this far, something failed, redisplay form
            return View(model);
        }

        #endregion

        #endregion
    }
}