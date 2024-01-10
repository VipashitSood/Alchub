using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.MultiVendor;
using Nop.Core.Domain.Customers;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.MultiVendors;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Models.Customers;
using Nop.Web.Areas.Admin.Models.Vendors;
using System.Collections.Generic;
using Nop.Web.Areas.Admin.Alchub.Factories;

namespace Nop.Web.Areas.Admin.Controllers
{
    public class MultiVendorController : BaseAdminController
    {
        #region Fields

        private readonly IMultiVendorModelFactory _multiVendorModelFactory;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly ICustomerService _customerService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly IPermissionService _permissionService;
        private readonly IMultiVendorService _multiVendorService;
        private readonly IWorkContext _workContext;
        private readonly IWebHelper _webHelper;

        #endregion

        #region Ctor

        public MultiVendorController(IMultiVendorModelFactory multiVendorModelFactory,
            ICustomerActivityService customerActivityService,
            ICustomerService customerService,
            IGenericAttributeService genericAttributeService,
            ILocalizationService localizationService,
            INotificationService notificationService,
            IPermissionService permissionService,
            IMultiVendorService multiVendorService,
            IWorkContext workContext,
            IWebHelper webHelper)
        {
            _multiVendorModelFactory = multiVendorModelFactory;
            _customerActivityService = customerActivityService;
            _customerService = customerService;
            _genericAttributeService = genericAttributeService;
            _localizationService = localizationService;
            _notificationService = notificationService;
            _permissionService = permissionService;
            _multiVendorService = multiVendorService;
            _workContext = workContext;
            _webHelper = webHelper;
        }

        #endregion

        public virtual async Task<IActionResult> List()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            //prepare model
            var model = await _multiVendorModelFactory.PrepareMultiVendorSearchModelAsync(new CustomerSearchModel());

            return View(model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> List(CustomerSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCustomers))
                return await AccessDeniedDataTablesJson();

            //get only multi vendors by multi vendor role
            var multiVendorRole = await _customerService.GetCustomerRoleBySystemNameAsync(NopCustomerDefaults.MultiVendorsRole);
            searchModel.SelectedCustomerRoleIds.Add(multiVendorRole.Id);

            //prepare model
            var model = await _multiVendorModelFactory.PrepareMultiVendorListModelAsync(searchModel);
            return Json(model);
        }

        public virtual async Task<IActionResult> MultiVendors()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageMultiVendors))
                return AccessDeniedView();

            //prepare model
            var model = await _multiVendorModelFactory.PrepareMultiVendorSearchModelAsync(new CustomerSearchModel());

            return View(model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> MultiVendors(CustomerSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageMultiVendors))
                return await AccessDeniedDataTablesJson();

            //prepare model
            var model = await _multiVendorModelFactory.PrepareMultiVendorAssosiatedVendorListModelAsync(searchModel);

            return Json(model);
        }

        public virtual async Task<IActionResult> Edit(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            //try to get a customer with the specified id
            var customer = await _customerService.GetCustomerByIdAsync(id);
            if (customer == null || customer.Deleted)
                return RedirectToAction("List");

            //prepare model
            var model = await _multiVendorModelFactory.PrepareMultiVendorModel(null, customer);

            return View(model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> Edit(MultiVendorModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            //try to get a customer with the specified id
            var customer = await _customerService.GetCustomerByIdAsync(model.Id);
            if (customer == null || customer.Deleted)
                return RedirectToAction("List");

            //ensure that a customer in the multi vendors role has a vendor account associated.
            if (await _customerService.IsMultiVendorAsync(customer) && !model.SelectedVendorIds.Any())
            {
                _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Customers.Customers.AtleastOneVendorMustBeSelected"));

                //prepare model
                model = await _multiVendorModelFactory.PrepareMultiVendorModel(model, customer);
                model.SelectedVendorIds = new List<int>();

                //if we got this far, something failed, redisplay form
                return View(model);
            }

            var vendorRole = await _customerService.GetCustomerRoleBySystemNameAsync(NopCustomerDefaults.VendorsRoleName);
            var roleIds = new List<int> { vendorRole.Id };
            var customersWithVendorRole = await _customerService.GetAllCustomersAsync(customerRoleIds: roleIds.ToArray());

            if (ModelState.IsValid)
            {
                try
                {
                    var currentVendorIds = await _multiVendorService.GetVendorIdsByMultiVendorAsync(customer.Id);
                    //vendors
                    foreach (var vendor in customersWithVendorRole)
                    {
                        if (model.SelectedVendorIds.Contains(vendor.Id))
                        {
                            //new vendor
                            if (currentVendorIds.All(id => id != vendor.Id))
                                await _multiVendorService.InsertManagerVendorMappingAsync(new ManagerVendorMapping { VendorId = vendor.Id, MultiVendorId = customer.Id });
                        }
                        else
                        {
                            //remove vendor
                            if (currentVendorIds.Any(id => id == vendor.Id))
                                await _multiVendorService.RemoveManagerVendorMappingAsync(customer, vendor.Id);
                        }
                    }
                    _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Customer.MultiVendor.Updated"));

                    return RedirectToAction("Edit", new { id = customer.Id });
                }
                catch (Exception exc)
                {
                    _notificationService.ErrorNotification(exc.Message);
                }
            }

            //prepare model
            model = await _multiVendorModelFactory.PrepareMultiVendorModel(model, customer);

            //if we got this far, something failed, redisplay form
            return View(model);
        }

        public virtual async Task<IActionResult> Impersonate(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageMultiVendors))
                return AccessDeniedView();

            //try to get a customer with the specified id
            var vendorAssosiatedCustomer = await _customerService.GetCustomerByIdAsync(id);
            if (vendorAssosiatedCustomer == null)
                return RedirectToAction("MultiVendors");

            if (vendorAssosiatedCustomer == null)
            {
                _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.MultiVendors.Vendors.NoAssosiatedVendorFound"));
                return RedirectToAction("MultiVendors");
            }

            if (!vendorAssosiatedCustomer.Active)
            {
                _notificationService.WarningNotification(await _localizationService.GetResourceAsync("Admin.MultiVendors.Vendors.Impersonate.Inactive"));
                return RedirectToAction("MultiVendors");
            }

            //ensure that a non-admin user cannot impersonate as an administrator
            //otherwise, that user can simply impersonate as an administrator and gain additional administrative privileges
            var currentCustomer = await _workContext.GetCurrentCustomerAsync();
            if (!await _customerService.IsMultiVendorAsync(currentCustomer) && await _customerService.IsVendorAsync(vendorAssosiatedCustomer))
            {
                _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.MultiVendors.Vendors.NonMultiVendorNotImpersonateAsMultiVendorError"));
                return RedirectToAction("MultiVendors");
            }

            //activity log
            await _customerActivityService.InsertActivityAsync("Impersonation.Started",
                string.Format(await _localizationService.GetResourceAsync("ActivityLog.Impersonation.Started.StoreOwner"), vendorAssosiatedCustomer.Email, vendorAssosiatedCustomer.Id), vendorAssosiatedCustomer);
            await _customerActivityService.InsertActivityAsync(vendorAssosiatedCustomer, "Impersonation.Started",
                string.Format(await _localizationService.GetResourceAsync("ActivityLog.Impersonation.Started.Customer"), currentCustomer.Email, currentCustomer.Id), currentCustomer);

            //ensure login is not required
            vendorAssosiatedCustomer.RequireReLogin = false;
            await _customerService.UpdateCustomerAsync(vendorAssosiatedCustomer);
            await _genericAttributeService.SaveAttributeAsync<int?>(currentCustomer, NopCustomerDefaults.ImpersonatedCustomerIdAttribute, vendorAssosiatedCustomer.Id);

            //redirect to admin side after impersonated
            var adminUrl = _webHelper.GetStoreLocation(true);
            adminUrl += "Admin";

            return Redirect(adminUrl);
        }
    }
}
