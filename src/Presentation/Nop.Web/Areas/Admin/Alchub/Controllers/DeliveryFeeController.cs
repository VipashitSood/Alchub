using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.DeliveryFees;
using Nop.Services.Customers;
using Nop.Services.DeliveryFees;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Controllers;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Models.DeliveryFees;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Web.Areas.Admin.Alchub.Controllers
{
    public partial class DeliveryFeeController : BaseAdminController
    {
        #region Fields

        private readonly ICustomerService _customerService;
        private readonly IDeliveryFeeModelFactory _deliveryFeeModelFactory;
        private readonly IDeliveryFeeService _deliveryFeeService;
        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly IPermissionService _permissionService;
        private readonly IWorkContext _workContext;

        #endregion Fields

        #region Ctor

        public DeliveryFeeController(
            ICustomerService customerService,
            IDeliveryFeeModelFactory deliveryFeeModelFactory,
            IDeliveryFeeService deliveryFeeService,
            ILocalizationService localizationService,
            INotificationService notificationService,
            IPermissionService permissionService,
            IWorkContext workContext)
        {
            _customerService = customerService;
            _deliveryFeeModelFactory = deliveryFeeModelFactory;
            _deliveryFeeService = deliveryFeeService;
            _localizationService = localizationService;
            _notificationService = notificationService;
            _permissionService = permissionService;
            _workContext = workContext;
        }

        #endregion Ctor

        #region Methods

        public virtual async Task<IActionResult> DeliveryFeeAsync()
        {
            var customer = await _workContext.GetCurrentCustomerAsync();

            if (!await _customerService.IsRegisteredAsync(customer))
                return Challenge();

            var accessAllowed = await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageDeliveryFee, customer);
            if (!accessAllowed)
                return AccessDeniedView();

            var currentVendor = await _workContext.GetCurrentVendorAsync();

            //make admin permission based on access controll and current vendor. 21-09-23
            bool hasFullAccess = accessAllowed && currentVendor == null;

            var model = await _deliveryFeeModelFactory.PrepareDeliveryFeeModelAsync(
                model: null,
                vendorId: currentVendor?.Id ?? 0,
                isAdmin: hasFullAccess);

            return View(model);
        }

        [HttpPost]
        [ValidateCaptcha]
        public virtual async Task<IActionResult> DeliveryFeeAsync(DeliveryFeeModel model)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();

            if (!await _customerService.IsRegisteredAsync(customer))
                return Challenge();

            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageDeliveryFee, customer))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                if (model.Id == 0)  //Insert
                {
                    var deliveryFee = new DeliveryFee
                    {
                        VendorId = model.VendorId,
                        DeliveryFeeTypeId = model.DeliveryFeeTypeId,
                        FixedFee = model.FixedFee,
                        DynamicBaseFee = model.DynamicBaseFee,
                        DynamicBaseDistance = model.DynamicBaseDistance,
                        DynamicExtraFee = model.DynamicExtraFee,
                        DynamicMaximumFee = model.DynamicMaximumFee,
                        CreatedOnUtc = DateTime.UtcNow
                    };

                    await _deliveryFeeService.InsertDeliveryFeeAsync(deliveryFee);

                    _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Alchub.Admin.DeliveryFee.Successfully.Inserted"));
                }
                else   //Update
                {
                    var deliveryFee = await _deliveryFeeService.GetDeliveryFeeByIdAsync(model.Id);

                    if (deliveryFee == null)
                    {
                        _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Alchub.Admin.DeliveryFee.NotFound"));

                        //Prepare Delivery Fee Model Properties
                        model = await _deliveryFeeModelFactory.PrepareDeliveryFeeModelPropertiesAsync(model);
                        return View(model);
                    }

                    deliveryFee.DeliveryFeeTypeId = model.DeliveryFeeTypeId;
                    deliveryFee.FixedFee = model.FixedFee;
                    deliveryFee.DynamicBaseFee = model.DynamicBaseFee;
                    deliveryFee.DynamicBaseDistance = model.DynamicBaseDistance;
                    deliveryFee.DynamicExtraFee = model.DynamicExtraFee;
                    deliveryFee.DynamicMaximumFee = model.DynamicMaximumFee;
                    deliveryFee.UpdatedOnUtc = DateTime.UtcNow;

                    await _deliveryFeeService.UpdateDeliveryFeeAsync(deliveryFee);

                    _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Alchub.Admin.DeliveryFee.Successfully.Updated"));
                }

                //Prepare Delivery Fee Model Properties
                model = await _deliveryFeeModelFactory.PrepareDeliveryFeeModelPropertiesAsync(model);

                return View(model);
            }

            //If we got this far, something failed, redisplay form
            model = await _deliveryFeeModelFactory.PrepareDeliveryFeeModelPropertiesAsync(model);
            return View(model);
        }

        [HttpPost]
        [ValidateCaptcha]
        public virtual async Task<IActionResult> ChangeDeliveryFeeVendorAsync(int vendorId = 0)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();

            if (!await _customerService.IsRegisteredAsync(customer))
                return Challenge();

            var accessAllowed = await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageDeliveryFee, customer);
            if (!accessAllowed)
                return AccessDeniedView();

            var currentVendor = await _workContext.GetCurrentVendorAsync();

            //make admin permission based on access controll and current vendor. 21-09-23
            bool hasFullAccess = accessAllowed && currentVendor == null;

            var model = await _deliveryFeeModelFactory.PrepareDeliveryFeeModelAsync(
                model: null,
                vendorId: vendorId,
                isAdmin: hasFullAccess);

            return Json(new { html = await RenderPartialViewToStringAsync("_CreateOrUpdate.DeliveryFee", model) });
        }

        #endregion Methods
    }
}