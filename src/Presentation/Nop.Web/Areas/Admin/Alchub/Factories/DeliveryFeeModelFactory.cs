using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core.Domain.DeliveryFees;
using Nop.Services.DeliveryFees;
using Nop.Services.Localization;
using Nop.Services.Vendors;
using Nop.Web.Areas.Admin.Models.DeliveryFees;

namespace Nop.Web.Areas.Admin.Factories
{
    /// <summary>
    /// Represents the Delivery Fee factory implementation
    /// </summary>
    public partial class DeliveryFeeModelFactory : IDeliveryFeeModelFactory
    {
        #region Fields

        private readonly IBaseAdminModelFactory _baseAdminModelFactory;
        private readonly IDeliveryFeeService _deliveryFeeService;
        private readonly ILocalizationService _localizationService;
        private readonly IVendorService _vendorService;

        #endregion

        #region Ctor

        public DeliveryFeeModelFactory(
            IBaseAdminModelFactory baseAdminModelFactory,
            IDeliveryFeeService deliveryFeeService,
            ILocalizationService localizationService,
            IVendorService vendorService)
        {
            _baseAdminModelFactory = baseAdminModelFactory;
            _deliveryFeeService = deliveryFeeService;
            _localizationService = localizationService;
            _vendorService = vendorService;
        }

        #endregion

        #region Methods

        #region Delivery Fee

        /// <summary>
        /// Prepare Delivery Fee Model Properties
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public virtual async Task<DeliveryFeeModel> PrepareDeliveryFeeModelPropertiesAsync(DeliveryFeeModel model = null)
        {
            if (model == null)
                return model;

            model.AvailableDeliveryFeeTypes =
                (from DeliveryFeeType e in Enum.GetValues(typeof(DeliveryFeeType))
                 select new SelectListItem { Value = ((int)e).ToString(), Text = e.ToString() }).ToList();

            if (model.IsAdmin)
            {
                //prepare available vendors
                await _baseAdminModelFactory.PrepareVendorsAsync(model.AvailableVendors,
                    defaultItemText: await _localizationService.GetResourceAsync("Admin.Catalog.Products.Fields.Vendor.None"));
            }
            else
            {
                var vendor = await _vendorService.GetVendorByIdAsync(model.VendorId);

                if (vendor != null)
                    model.AvailableVendors.Add(new SelectListItem { Text = vendor.Name, Value = vendor.Id.ToString(), Selected = true });
            }

            return model;
        }

        /// <summary>
        /// Prepare Delivery Fee Model
        /// </summary>
        /// <param name="vendorId"></param>
        /// <returns></returns>
        public virtual async Task<DeliveryFeeModel> PrepareDeliveryFeeModelAsync(
           DeliveryFeeModel model = null,
           int vendorId = 0,
           bool isAdmin = false)
        {
            if (model == null)
            {
                model = new DeliveryFeeModel
                {
                    VendorId = vendorId,
                    IsAdmin = isAdmin
                };
            }

            if (model.VendorId > 0)
            {
                var deliveryFee = await _deliveryFeeService.GetDeliveryFeeByVendorIdAsync(vendorId: vendorId);

                if (deliveryFee != null)
                {
                    model.Id = deliveryFee.Id;
                    model.DeliveryFeeTypeId = deliveryFee.DeliveryFeeTypeId;
                    model.FixedFee = deliveryFee.FixedFee;
                    model.DynamicBaseFee = deliveryFee.DynamicBaseFee;
                    model.DynamicBaseDistance = deliveryFee.DynamicBaseDistance;
                    model.DynamicExtraFee = deliveryFee.DynamicExtraFee;
                    model.DynamicMaximumFee = deliveryFee.DynamicMaximumFee;
                }
            }

            //Prepare Delivery Fee Model Properties
            model = await PrepareDeliveryFeeModelPropertiesAsync(model);

            return model;
        }

        #endregion Delivery Fee

        #endregion Methods
    }
}