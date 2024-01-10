using Nop.Plugin.Payments.StripeConnectRedirect.Models;
using Nop.Plugin.Payments.StripeConnectRedirect.Services;
using Nop.Web.Framework.Models.Extensions;
using System.Linq;
using System.Threading.Tasks;

namespace Nop.Plugin.Payments.StripeConnectRedirect.Factories
{
    /// <summary>
    /// Represents the stripe connect vendor model factory implementation
    /// </summary>
    public partial class VendorModelFactory : IVendorModelFactory
    {
        #region Fields

        private readonly IStripeConnectRedirectService _stripeConnectRedirectService;

        #endregion

        #region Ctor

        public VendorModelFactory(IStripeConnectRedirectService stripeConnectRedirectService)
        {
            _stripeConnectRedirectService = stripeConnectRedirectService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Prepare paged vendor list model
        /// </summary>
        /// <param name="searchModel">Vendor search model</param>
        /// <returns>Vendor list model</returns>
        public virtual async Task<VendorListModel> PrepareVendorListModelAsync(VendorSearchModel searchModel)
        {
            //get vendors
            var vendors = await _stripeConnectRedirectService.SearchStripeVendorConnectAsync(pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

            //prepare list model
            var model = new VendorListModel().PrepareToGrid(searchModel, vendors, () =>
            {
                //fill in model values from the entity
                return vendors.Select(vendor =>
                {
                    var stripeVendorModel = new StripeVendorModel
                    {
                        Id = vendor.Id,
                        Name = vendor.Name,
                        Email = vendor.Email,
                        Account = vendor.Account,
                        AdminDeliveryCommissionPercentage = vendor.AdminDeliveryCommissionPercentage,
                        AdminPickupCommissionPercentage = vendor.AdminPickupCommissionPercentage,
                        IsVerified = vendor.IsVerified
                    };

                    return stripeVendorModel;
                });
            });

            return model;
        }

        #endregion
    }
}