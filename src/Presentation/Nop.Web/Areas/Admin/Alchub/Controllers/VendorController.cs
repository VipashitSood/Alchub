using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Nop.Core;
using Nop.Core.Alchub.Domain;
using Nop.Core.Alchub.Domain.Vendors;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Vendors;
using Nop.Core.Infrastructure;
using Nop.Services.Alchub.Vendors;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Vendors;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Vendors;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Web.Areas.Admin.Controllers
{
    public partial class VendorController : BaseAdminController
    {
        #region Fields

        private readonly IAddressAttributeParser _addressAttributeParser;
        private readonly IAddressService _addressService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly ICustomerService _customerService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ILocalizationService _localizationService;
        private readonly ILocalizedEntityService _localizedEntityService;
        private readonly INotificationService _notificationService;
        private readonly IPermissionService _permissionService;
        private readonly IPictureService _pictureService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IVendorAttributeParser _vendorAttributeParser;
        private readonly IVendorAttributeService _vendorAttributeService;
        private readonly IVendorModelFactory _vendorModelFactory;
        private readonly IVendorService _vendorService;
        private readonly IWorkContext _workContext;
        private readonly INopFileProvider _fileProvider;
        private readonly AlchubSettings _alchubSettings;
        private readonly IProductService _productService;
        private readonly IVendorTimingService _vendorTimingService;

        #endregion

        #region Ctor

        public VendorController(IAddressAttributeParser addressAttributeParser,
            IAddressService addressService,
            ICustomerActivityService customerActivityService,
            ICustomerService customerService,
            IGenericAttributeService genericAttributeService,
            ILocalizationService localizationService,
            ILocalizedEntityService localizedEntityService,
            INotificationService notificationService,
            IPermissionService permissionService,
            IPictureService pictureService,
            IUrlRecordService urlRecordService,
            IVendorAttributeParser vendorAttributeParser,
            IVendorAttributeService vendorAttributeService,
            IVendorModelFactory vendorModelFactory,
            IVendorService vendorService,
            IWorkContext workContext,
            INopFileProvider fileProvider,
            AlchubSettings alchubSettings,
            IProductService productService,
            IVendorTimingService vendorTimingService)
        {
            _addressAttributeParser = addressAttributeParser;
            _addressService = addressService;
            _customerActivityService = customerActivityService;
            _customerService = customerService;
            _genericAttributeService = genericAttributeService;
            _localizationService = localizationService;
            _localizedEntityService = localizedEntityService;
            _notificationService = notificationService;
            _permissionService = permissionService;
            _pictureService = pictureService;
            _urlRecordService = urlRecordService;
            _vendorAttributeParser = vendorAttributeParser;
            _vendorAttributeService = vendorAttributeService;
            _vendorModelFactory = vendorModelFactory;
            _vendorService = vendorService;
            _workContext = workContext;
            _fileProvider = fileProvider;
            _alchubSettings = alchubSettings;
            _productService = productService;
            _vendorTimingService = vendorTimingService;
        }

        #endregion

        #region Utilities 

        private async Task CreateVendorProductsExcelPath(Vendor vendor)
        {
            try
            {
                //ftp path for vendors
                var ftpPath = _alchubSettings.ExcelFileFTPPath;
                if (ftpPath == null)
                {
                    _notificationService.ErrorNotification("Ftp path is empty.");
                    return;
                }

                //check directory exists or not
                var ftpDirectExists = _fileProvider.DirectoryExists(ftpPath);
                if (!ftpDirectExists)
                {
                    _notificationService.ErrorNotification(string.Format("Ftp path doesn't exists. Path: {0} ", ftpPath));
                    return;
                }

                //vendor products excel path
                var vendorFtpFilePath = await _genericAttributeService.GetAttributeAsync<string>(vendor, NopVendorDefaults.ExcelProductFTPPath);
                if (!string.IsNullOrEmpty(vendorFtpFilePath))
                {
                    //if vendor file path is not created on ftp create it.
                    var existingPath = _fileProvider.Combine(ftpPath, vendorFtpFilePath);
                    if (!_fileProvider.DirectoryExists(existingPath))
                        _fileProvider.CreateDirectory(existingPath);

                    return;
                }

                //generate unique vendor products excel path name
                vendorFtpFilePath = vendor.Name.Replace(" ", string.Empty) + "_" + CommonHelper.GenerateRandomDigitCode(8);
                await _genericAttributeService.SaveAttributeAsync(vendor, NopVendorDefaults.ExcelProductFTPPath, vendorFtpFilePath);

                //create path for this vendor
                var path = _fileProvider.Combine(ftpPath, vendorFtpFilePath);
                _fileProvider.CreateDirectory(path);

                var vendorNote = new VendorNote()
                {
                    VendorId = vendor.Id,
                    Note = $"Vendor products excel file path : {vendorFtpFilePath} has been created for vendor : {vendor.Name}",
                    CreatedOnUtc = DateTime.UtcNow
                };
                await _vendorService.InsertVendorNoteAsync(vendorNote);
            }
            catch (Exception ex)
            {
                _notificationService.ErrorNotification(ex.Message);
                return;
            }
        }
        #endregion

        #region Vendors

        public virtual async Task<IActionResult> Create()
        {
            //++Alchub
            //a vendor does not have access to this functionality
            if (await _workContext.GetCurrentVendorAsync() != null)
                return AccessDeniedView();

            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageVendors) ||
                !await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessVendorCreate))
                return AccessDeniedView();

            //prepare model
            var model = await _vendorModelFactory.PrepareVendorModelAsync(new VendorModel(), null);

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [FormValueRequired("save", "save-continue")]
        public virtual async Task<IActionResult> Create(VendorModel model, bool continueEditing, IFormCollection form)
        {
            //++Alchub
            //a vendor does not have access to this functionality
            if (await _workContext.GetCurrentVendorAsync() != null)
                return AccessDeniedView();

            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageVendors) ||
                !await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessVendorCreate))
                return AccessDeniedView();

            //parse vendor attributes
            var vendorAttributesXml = await ParseVendorAttributesAsync(form);
            (await _vendorAttributeParser.GetAttributeWarningsAsync(vendorAttributesXml)).ToList()
                .ForEach(warning => ModelState.AddModelError(string.Empty, warning));

            if (ModelState.IsValid)
            {
                var vendor = model.ToEntity<Vendor>();
                await _vendorService.InsertVendorAsync(vendor);

                //activity log
                await _customerActivityService.InsertActivityAsync("AddNewVendor",
                    string.Format(await _localizationService.GetResourceAsync("ActivityLog.AddNewVendor"), vendor.Id), vendor);

                //search engine name
                model.SeName = await _urlRecordService.ValidateSeNameAsync(vendor, model.SeName, vendor.Name, true);
                await _urlRecordService.SaveSlugAsync(vendor, model.SeName, 0);

                //address
                var address = model.Address.ToEntity<Address>();
                address.CreatedOnUtc = DateTime.UtcNow;

                //some validation
                if (address.CountryId == 0)
                    address.CountryId = null;
                if (address.StateProvinceId == 0)
                    address.StateProvinceId = null;
                await _addressService.InsertAddressAsync(address);
                vendor.AddressId = address.Id;
                await _vendorService.UpdateVendorAsync(vendor);

                //vendor attributes
                await _genericAttributeService.SaveAttributeAsync(vendor, NopVendorDefaults.VendorAttributes, vendorAttributesXml);

                //locales
                await UpdateLocalesAsync(vendor, model);

                //update picture seo file name
                await UpdatePictureSeoNamesAsync(vendor);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Vendors.Added"));

                if (!continueEditing)
                    return RedirectToAction("List");

                return RedirectToAction("Edit", new { id = vendor.Id });
            }

            //prepare model
            model = await _vendorModelFactory.PrepareVendorModelAsync(model, null, true);

            //if we got this far, something failed, redisplay form
            return View(model);
        }

        public virtual async Task<IActionResult> Edit(int id)
        {
            //++Alchub
            //a vendor does have access to this personal info functionality
            var currentVendor = await _workContext.GetCurrentVendorAsync();
            if (currentVendor != null)
            {
                //vendor personal edit?
                if (currentVendor.Id != id)
                    return AccessDeniedView();
            }

            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageVendors))
                return AccessDeniedView();

            //try to get a vendor with the specified id
            var vendor = await _vendorService.GetVendorByIdAsync(id);
            if (vendor == null || vendor.Deleted)
                return RedirectToAction("List");

            //create vendor products excel path
            await CreateVendorProductsExcelPath(vendor);

            //prepare model
            var model = await _vendorModelFactory.PrepareVendorModelAsync(null, vendor);

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> Edit(VendorModel model, bool continueEditing, IFormCollection form)
        {
            //++Alchub
            //a vendor does have access to this personal info functionality
            var currentVendor = await _workContext.GetCurrentVendorAsync();
            if (currentVendor != null)
            {
                //vendor personal edit?
                if (currentVendor.Id != model.Id)
                    return AccessDeniedView();
            }

            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageVendors))
                return AccessDeniedView();

            //try to get a vendor with the specified id
            var vendor = await _vendorService.GetVendorByIdAsync(model.Id);
            if (vendor == null || vendor.Deleted)
                return RedirectToAction("List");

            //parse vendor attributes
            var vendorAttributesXml = await ParseVendorAttributesAsync(form);
            (await _vendorAttributeParser.GetAttributeWarningsAsync(vendorAttributesXml)).ToList()
                .ForEach(warning => ModelState.AddModelError(string.Empty, warning));

            //custom address attributes
            var customAttributes = await _addressAttributeParser.ParseCustomAddressAttributesAsync(form);
            var customAttributeWarnings = await _addressAttributeParser.GetAttributeWarningsAsync(customAttributes);
            foreach (var error in customAttributeWarnings)
            {
                ModelState.AddModelError(string.Empty, error);
            }

            if (ModelState.IsValid)
            {
                var prevPictureId = vendor.PictureId;
                vendor = model.ToEntity(vendor);
                await _vendorService.UpdateVendorAsync(vendor);

                //vendor attributes
                await _genericAttributeService.SaveAttributeAsync(vendor, NopVendorDefaults.VendorAttributes, vendorAttributesXml);

                //activity log
                await _customerActivityService.InsertActivityAsync("EditVendor",
                    string.Format(await _localizationService.GetResourceAsync("ActivityLog.EditVendor"), vendor.Id), vendor);

                //search engine name
                model.SeName = await _urlRecordService.ValidateSeNameAsync(vendor, model.SeName, vendor.Name, true);
                await _urlRecordService.SaveSlugAsync(vendor, model.SeName, 0);

                //address
                var address = await _addressService.GetAddressByIdAsync(vendor.AddressId);
                if (address == null)
                {
                    address = model.Address.ToEntity<Address>();
                    address.CustomAttributes = customAttributes;
                    address.CreatedOnUtc = DateTime.UtcNow;

                    //some validation
                    if (address.CountryId == 0)
                        address.CountryId = null;
                    if (address.StateProvinceId == 0)
                        address.StateProvinceId = null;

                    await _addressService.InsertAddressAsync(address);
                    vendor.AddressId = address.Id;
                    await _vendorService.UpdateVendorAsync(vendor);
                }
                else
                {
                    address = model.Address.ToEntity(address);
                    address.CustomAttributes = customAttributes;

                    //some validation
                    if (address.CountryId == 0)
                        address.CountryId = null;
                    if (address.StateProvinceId == 0)
                        address.StateProvinceId = null;

                    await _addressService.UpdateAddressAsync(address);
                }

                //locales
                await UpdateLocalesAsync(vendor, model);

                //delete an old picture (if deleted or updated)
                if (prevPictureId > 0 && prevPictureId != vendor.PictureId)
                {
                    var prevPicture = await _pictureService.GetPictureByIdAsync(prevPictureId);
                    if (prevPicture != null)
                        await _pictureService.DeletePictureAsync(prevPicture);
                }
                //update picture seo file name
                await UpdatePictureSeoNamesAsync(vendor);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Vendors.Updated"));

                if (!continueEditing)
                    return RedirectToAction("List");

                return RedirectToAction("Edit", new { id = vendor.Id });
            }

            //prepare model
            model = await _vendorModelFactory.PrepareVendorModelAsync(model, vendor, true);

            //if we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> Delete(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageVendors))
                return AccessDeniedView();

            //try to get a vendor with the specified id
            var vendor = await _vendorService.GetVendorByIdAsync(id);
            if (vendor == null)
                return RedirectToAction("List");

            //clear associated customer references
            var associatedCustomers = await _customerService.GetAllCustomersAsync(vendorId: vendor.Id);
            foreach (var customer in associatedCustomers)
            {
                customer.VendorId = 0;
                await _customerService.UpdateCustomerAsync(customer);
            }

            //++Alchub

            //clear vendor products references
            var vendorProducts = await _productService.SearchProductsAsync(vendorId: vendor.Id);
            if (vendorProducts != null && vendorProducts.Any())
                await _productService.DeleteProductsAsync(vendorProducts.ToList());

            //--Alchub

            //delete a vendor
            await _vendorService.DeleteVendorAsync(vendor);

            //activity log
            await _customerActivityService.InsertActivityAsync("DeleteVendor",
                string.Format(await _localizationService.GetResourceAsync("ActivityLog.DeleteVendor"), vendor.Id), vendor);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Vendors.Deleted"));

            return RedirectToAction("List");
        }

        #endregion

        #region Vendor Timing
        [HttpPost]
        public virtual async Task<IActionResult> VendorTimingList(VendorTimingSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageVendors))
                return await AccessDeniedDataTablesJson();

            //try to get a vendor with the specified id
            var vendor = await _vendorService.GetVendorByIdAsync(searchModel.VendorId)
                ?? throw new ArgumentException("No vendor found with the specified id");

            //prepare model
            var model = await _vendorModelFactory.PrepareVendorTimingListModel(searchModel, vendor);

            return Json(model);
        }

        public virtual async Task<IActionResult> VendorTimingCreateOrEdit(int vendorId, int dayId)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageVendors))
                return AccessDeniedView();

            //try to get a vendor with the specified id
            var vendor = await _vendorService.GetVendorByIdAsync(vendorId);
            if (vendor == null)
                return ErrorJson("Vendor cannot be loaded");

            //get vendor timing by 
            var vendorTiming = await _vendorTimingService.GetVendorTimingByVendorIdAsync(vendor.Id, dayId);

            //prepare vendor
            var model = await _vendorModelFactory.PrepareVendorTimingModelAsync(null, vendor, vendorTiming, dayId);
            return View(model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> VendorTimingCreateOrEdit(int vendorId, VendorTimingModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageVendors))
                return AccessDeniedView();

            //try to get a vendor with the specified id
            var vendor = await _vendorService.GetVendorByIdAsync(vendorId);
            if (vendor == null)
                return ErrorJson("Vendor cannot be loaded");

            //get vendor timing by vendor id & day id
            var vendorTiming = await _vendorTimingService.GetVendorTimingByVendorIdAsync(vendorId, model.DayId);

            //convert time from stirng
            DateTime.TryParse(model.OpenTimeStr, out var openTime);
            DateTime.TryParse(model.CloseTimeStr, out var closeTime);

            model.OpenTimeUtc = openTime.TimeOfDay;
            model.CloseTimeUtc = closeTime.TimeOfDay;

            if (ModelState.IsValid)
            {
                //insert vendor timing if null, otherwise insert it.
                if (vendorTiming == null)
                {
                    vendorTiming = new VendorTiming
                    {
                        OpenTimeUtc = !string.IsNullOrEmpty(model.OpenTimeStr) ? openTime : null,
                        CloseTimeUtc = !string.IsNullOrEmpty(model.CloseTimeStr) ? closeTime : null,
                        DayOff = model.DayOff,
                        DayId = model.DayId,
                        VendorId = model.VendorId
                    };

                    await _vendorTimingService.InsertVendorTimingAsync(vendorTiming);
                }
                else
                {
                    vendorTiming.OpenTimeUtc = !string.IsNullOrEmpty(model.OpenTimeStr) ? openTime : null;
                    vendorTiming.CloseTimeUtc = !string.IsNullOrEmpty(model.CloseTimeStr) ? closeTime : null;
                    vendorTiming.DayOff = model.DayOff;

                    await _vendorTimingService.UpdateVendorTimingAsync(vendorTiming);
                }

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Vendor.Timing.Updated"));

                ViewBag.RefreshPage = true;
                ViewBag.ClosePage = true;
            }

            //prepare model
            model = await _vendorModelFactory.PrepareVendorTimingModelAsync(model, vendor, vendorTiming, model.DayId, true);

            //if we got this far, something failed, redisplay form
            return View(model);
        }

        #endregion
    }
}