using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Forums;
using Nop.Core.Domain.Gdpr;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Security;
using Nop.Core.Domain.Tax;
using Nop.Core.Domain.Vendors;
using Nop.Services.Authentication.External;
using Nop.Services.Authentication.MultiFactor;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Gdpr;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Seo;
using Nop.Services.Stores;
using Nop.Web.Models.Common;
using Nop.Web.Models.Customer;

namespace Nop.Web.Factories
{
    /// <summary>
    /// Represents the customer model factory
    /// </summary>
    public partial class CustomerModelFactory : ICustomerModelFactory
    {
        #region Methods

        /// <summary>
        /// Prepare the customer navigation model
        /// </summary>
        /// <param name="selectedTabId">Identifier of the selected tab</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the customer navigation model
        /// </returns>
        public virtual async Task<CustomerNavigationModel> PrepareCustomerNavigationModelAsync(int selectedTabId = 0)
        {
            var model = new CustomerNavigationModel();

            model.CustomerNavigationItems.Add(new CustomerNavigationItemModel
            {
                RouteName = "CustomerInfo",
                Title = await _localizationService.GetResourceAsync("Account.CustomerInfo"),
                Tab = (int)CustomerNavigationEnum.Info,
                ItemClass = "customer-info"
            });

            //++Alchub
            model.CustomerNavigationItems.Add(new CustomerNavigationItemModel
            {
                RouteName = "CustomerFavorite",
                Title = await _localizationService.GetResourceAsync("Account.Favorite"),
                Tab = (int)CustomerNavigationEnum.Wishlist,
                ItemClass = "customer-wishlist"
            });

            model.CustomerNavigationItems.Add(new CustomerNavigationItemModel
            {
                RouteName = "CustomerAddresses",
                Title = await _localizationService.GetResourceAsync("Account.CustomerAddresses"),
                Tab = (int)CustomerNavigationEnum.Addresses,
                ItemClass = "customer-addresses"
            });

            model.CustomerNavigationItems.Add(new CustomerNavigationItemModel
            {
                RouteName = "CustomerOrders",
                Title = await _localizationService.GetResourceAsync("Account.CustomerOrders"),
                Tab = (int)CustomerNavigationEnum.Orders,
                ItemClass = "customer-orders"
            });

            var store = await _storeContext.GetCurrentStoreAsync();
            var customer = await _workContext.GetCurrentCustomerAsync();

            if (_orderSettings.ReturnRequestsEnabled &&
                (await _returnRequestService.SearchReturnRequestsAsync(store.Id,
                    customer.Id, pageIndex: 0, pageSize: 1)).Any())
            {
                model.CustomerNavigationItems.Add(new CustomerNavigationItemModel
                {
                    RouteName = "CustomerReturnRequests",
                    Title = await _localizationService.GetResourceAsync("Account.CustomerReturnRequests"),
                    Tab = (int)CustomerNavigationEnum.ReturnRequests,
                    ItemClass = "return-requests"
                });
            }

            if (!_customerSettings.HideDownloadableProductsTab)
            {
                model.CustomerNavigationItems.Add(new CustomerNavigationItemModel
                {
                    RouteName = "CustomerDownloadableProducts",
                    Title = await _localizationService.GetResourceAsync("Account.DownloadableProducts"),
                    Tab = (int)CustomerNavigationEnum.DownloadableProducts,
                    ItemClass = "downloadable-products"
                });
            }

            if (!_customerSettings.HideBackInStockSubscriptionsTab)
            {
                model.CustomerNavigationItems.Add(new CustomerNavigationItemModel
                {
                    RouteName = "CustomerBackInStockSubscriptions",
                    Title = await _localizationService.GetResourceAsync("Account.BackInStockSubscriptions"),
                    Tab = (int)CustomerNavigationEnum.BackInStockSubscriptions,
                    ItemClass = "back-in-stock-subscriptions"
                });
            }

            if (_rewardPointsSettings.Enabled)
            {
                model.CustomerNavigationItems.Add(new CustomerNavigationItemModel
                {
                    RouteName = "CustomerRewardPoints",
                    Title = await _localizationService.GetResourceAsync("Account.RewardPoints"),
                    Tab = (int)CustomerNavigationEnum.RewardPoints,
                    ItemClass = "reward-points"
                });
            }

            model.CustomerNavigationItems.Add(new CustomerNavigationItemModel
            {
                RouteName = "CustomerChangePassword",
                Title = await _localizationService.GetResourceAsync("Account.ChangePassword"),
                Tab = (int)CustomerNavigationEnum.ChangePassword,
                ItemClass = "change-password"
            });

            if (_customerSettings.AllowCustomersToUploadAvatars)
            {
                model.CustomerNavigationItems.Add(new CustomerNavigationItemModel
                {
                    RouteName = "CustomerAvatar",
                    Title = await _localizationService.GetResourceAsync("Account.Avatar"),
                    Tab = (int)CustomerNavigationEnum.Avatar,
                    ItemClass = "customer-avatar"
                });
            }

            if (_forumSettings.ForumsEnabled && _forumSettings.AllowCustomersToManageSubscriptions)
            {
                model.CustomerNavigationItems.Add(new CustomerNavigationItemModel
                {
                    RouteName = "CustomerForumSubscriptions",
                    Title = await _localizationService.GetResourceAsync("Account.ForumSubscriptions"),
                    Tab = (int)CustomerNavigationEnum.ForumSubscriptions,
                    ItemClass = "forum-subscriptions"
                });
            }
            if (_catalogSettings.ShowProductReviewsTabOnAccountPage)
            {
                model.CustomerNavigationItems.Add(new CustomerNavigationItemModel
                {
                    RouteName = "CustomerProductReviews",
                    Title = await _localizationService.GetResourceAsync("Account.CustomerProductReviews"),
                    Tab = (int)CustomerNavigationEnum.ProductReviews,
                    ItemClass = "customer-reviews"
                });
            }
            if (_vendorSettings.AllowVendorsToEditInfo && await _workContext.GetCurrentVendorAsync() != null)
            {
                model.CustomerNavigationItems.Add(new CustomerNavigationItemModel
                {
                    RouteName = "CustomerVendorInfo",
                    Title = await _localizationService.GetResourceAsync("Account.VendorInfo"),
                    Tab = (int)CustomerNavigationEnum.VendorInfo,
                    ItemClass = "customer-vendor-info"
                });
            }
            if (_gdprSettings.GdprEnabled)
            {
                model.CustomerNavigationItems.Add(new CustomerNavigationItemModel
                {
                    RouteName = "GdprTools",
                    Title = await _localizationService.GetResourceAsync("Account.Gdpr"),
                    Tab = (int)CustomerNavigationEnum.GdprTools,
                    ItemClass = "customer-gdpr"
                });
            }

            if (_captchaSettings.Enabled && _customerSettings.AllowCustomersToCheckGiftCardBalance)
            {
                model.CustomerNavigationItems.Add(new CustomerNavigationItemModel
                {
                    RouteName = "CheckGiftCardBalance",
                    Title = await _localizationService.GetResourceAsync("CheckGiftCardBalance"),
                    Tab = (int)CustomerNavigationEnum.CheckGiftCardBalance,
                    ItemClass = "customer-check-gift-card-balance"
                });
            }

            if (await _multiFactorAuthenticationPluginManager.HasActivePluginsAsync())
            {
                model.CustomerNavigationItems.Add(new CustomerNavigationItemModel
                {
                    RouteName = "MultiFactorAuthenticationSettings",
                    Title = await _localizationService.GetResourceAsync("PageTitle.MultiFactorAuthentication"),
                    Tab = (int)CustomerNavigationEnum.MultiFactorAuthentication,
                    ItemClass = "customer-multiFactor-authentication"
                });
            }

            model.SelectedTab = selectedTabId;

            return model;
        }

        #endregion
    }
}