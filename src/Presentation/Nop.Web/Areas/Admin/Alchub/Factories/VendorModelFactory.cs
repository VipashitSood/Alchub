using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Alchub.Domain;
using Nop.Core.Alchub.Domain.Vendors;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Vendors;
using Nop.Services.Alchub.Vendors;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Vendors;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Vendors;
using Nop.Web.Framework.Factories;
using Nop.Web.Framework.Models.Extensions;

namespace Nop.Web.Areas.Admin.Factories
{
    /// <summary>
    /// Represents the vendor model factory implementation
    /// </summary>
    public partial class VendorModelFactory : IVendorModelFactory
    {
        #region Fields

        private readonly CurrencySettings _currencySettings;
        private readonly ICurrencyService _currencyService;
        private readonly IAddressModelFactory _addressModelFactory;
        private readonly IAddressService _addressService;
        private readonly ICustomerService _customerService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ILocalizationService _localizationService;
        private readonly ILocalizedModelFactory _localizedModelFactory;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IVendorAttributeParser _vendorAttributeParser;
        private readonly IVendorAttributeService _vendorAttributeService;
        private readonly IVendorService _vendorService;
        private readonly VendorSettings _vendorSettings;
        private readonly IWorkContext _workContext;
        private readonly AlchubSettings _alchubSettings;
        private readonly IVendorTimingService _vendorTimingService;
        private readonly IPermissionService _permissionService;
        #endregion

        #region Ctor

        public VendorModelFactory(CurrencySettings currencySettings,
            ICurrencyService currencyService,
            IAddressModelFactory addressModelFactory,
            IAddressService addressService,
            ICustomerService customerService,
            IDateTimeHelper dateTimeHelper,
            IGenericAttributeService genericAttributeService,
            ILocalizationService localizationService,
            ILocalizedModelFactory localizedModelFactory,
            IUrlRecordService urlRecordService,
            IVendorAttributeParser vendorAttributeParser,
            IVendorAttributeService vendorAttributeService,
            IVendorService vendorService,
            VendorSettings vendorSettings,
            IWorkContext workContext,
            AlchubSettings alchubSettings,
            IVendorTimingService vendorTimingService,
             IPermissionService permissionService)
        {
            _currencySettings = currencySettings;
            _currencyService = currencyService;
            _addressModelFactory = addressModelFactory;
            _addressService = addressService;
            _customerService = customerService;
            _dateTimeHelper = dateTimeHelper;
            _genericAttributeService = genericAttributeService;
            _localizationService = localizationService;
            _localizedModelFactory = localizedModelFactory;
            _urlRecordService = urlRecordService;
            _vendorAttributeParser = vendorAttributeParser;
            _vendorAttributeService = vendorAttributeService;
            _vendorService = vendorService;
            _vendorSettings = vendorSettings;
            _workContext = workContext;
            _alchubSettings = alchubSettings;
            _vendorTimingService = vendorTimingService;
            _permissionService = permissionService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Prepare vendor search model
        /// </summary>
        /// <param name="searchModel">Vendor search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the vendor search model
        /// </returns>
        public virtual async Task<VendorSearchModel> PrepareVendorSearchModelAsync(VendorSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //prepare page parameters
            searchModel.SetGridPageSize();

            //vendor create access
            searchModel.AllowVendorCreate = (await _workContext.GetCurrentVendorAsync() == null) && await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessVendorCreate);

            return searchModel;
        }

        /// <summary>
        /// Prepare vendor model
        /// </summary>
        /// <param name="model">Vendor model</param>
        /// <param name="vendor">Vendor</param>
        /// <param name="excludeProperties">Whether to exclude populating of some properties of model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the vendor model
        /// </returns>
        public virtual async Task<VendorModel> PrepareVendorModelAsync(VendorModel model, Vendor vendor, bool excludeProperties = false)
        {
            Func<VendorLocalizedModel, int, Task> localizedModelConfiguration = null;

            if (vendor != null)
            {
                //fill in model values from the entity
                if (model == null)
                {
                    model = vendor.ToModel<VendorModel>();
                    model.SeName = await _urlRecordService.GetSeNameAsync(vendor, 0, true, false);
                }

                //define localized model configuration action
                localizedModelConfiguration = async (locale, languageId) =>
                {
                    locale.Name = await _localizationService.GetLocalizedAsync(vendor, entity => entity.Name, languageId, false, false);
                    locale.Description = await _localizationService.GetLocalizedAsync(vendor, entity => entity.Description, languageId, false, false);
                    locale.MetaKeywords = await _localizationService.GetLocalizedAsync(vendor, entity => entity.MetaKeywords, languageId, false, false);
                    locale.MetaDescription = await _localizationService.GetLocalizedAsync(vendor, entity => entity.MetaDescription, languageId, false, false);
                    locale.MetaTitle = await _localizationService.GetLocalizedAsync(vendor, entity => entity.MetaTitle, languageId, false, false);
                    locale.SeName = await _urlRecordService.GetSeNameAsync(vendor, languageId, false, false);
                };

                //prepare associated customers
                await PrepareAssociatedCustomerModelsAsync(model.AssociatedCustomers, vendor);

                //prepare nested search models
                PrepareVendorNoteSearchModel(model.VendorNoteSearchModel, vendor);

                //prepare vendor timing search model
                PrepareVendorTimingSearchModel(model.VendorTimingSearchModel, vendor);

                //ftp file path
                var excelProductFtpPath = await _genericAttributeService.GetAttributeAsync<string>(vendor, NopVendorDefaults.ExcelProductFTPPath);
                model.FtpFilePath = excelProductFtpPath;
            }

            //set default values for the new model
            if (vendor == null)
            {
                model.PageSize = 6;
                model.Active = true;
                model.AllowCustomersToSelectPageSize = true;
                model.PageSizeOptions = _vendorSettings.DefaultVendorPageSizeOptions;
                model.PriceRangeFiltering = true;
                model.ManuallyPriceRange = true;
                model.PriceFrom = NopCatalogDefaults.DefaultPriceRangeFrom;
                model.PriceTo = NopCatalogDefaults.DefaultPriceRangeTo;
            }

            model.PrimaryStoreCurrencyCode = (await _currencyService.GetCurrencyByIdAsync(_currencySettings.PrimaryStoreCurrencyId)).CurrencyCode;

            //prepare localized models
            if (!excludeProperties)
                model.Locales = await _localizedModelFactory.PrepareLocalizedModelsAsync(localizedModelConfiguration);

            //prepare model vendor attributes
            await PrepareVendorAttributeModelsAsync(model.VendorAttributes, vendor);

            //prepare address model
            var address = await _addressService.GetAddressByIdAsync(vendor?.AddressId ?? 0);
            if (!excludeProperties && address != null)
                model.Address = address.ToModel(model.Address);
            await _addressModelFactory.PrepareAddressModelAsync(model.Address, address);

            return model;
        }

        /// <summary>
        /// Prepare paged vendor list model
        /// </summary>
        /// <param name="searchModel">Vendor search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the vendor list model
        /// </returns>
        public virtual async Task<VendorListModel> PrepareVendorListModelAsync(VendorSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //vendor can she his details only
            var currentVendor = await _workContext.GetCurrentVendorAsync();
            if (currentVendor != null)
                //fix: new vendor not showing in list because of pagination. - 12-06-23
                //add email in search, as all the vendors will have unique email
                searchModel.SearchEmail = currentVendor.Email;

            //get vendors
            var vendors = await _vendorService.GetAllVendorsAsync(showHidden: true,
                name: searchModel.SearchName,
                email: searchModel.SearchEmail,
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

            //++Alchub
            if (currentVendor != null)
                vendors = vendors?.Where(v => v.Id == currentVendor.Id)?.ToList()?.ToPagedList(searchModel);

            //prepare list model
            var model = await new VendorListModel().PrepareToGridAsync(searchModel, vendors, () =>
            {
                //fill in model values from the entity
                return vendors.SelectAwait(async vendor =>
                {
                    var vendorModel = vendor.ToModel<VendorModel>();

                    vendorModel.SeName = await _urlRecordService.GetSeNameAsync(vendor, 0, true, false);

                    return vendorModel;
                });
            });

            return model;
        }

        #endregion

        #region VendorTiming

        /// <summary>
        /// Prepare a vendor timing model
        /// </summary>
        /// <param name="model"></param>
        /// <param name="vendor"></param>
        /// <param name="vendorTiming"></param>
        /// <param name="dayId"></param>
        /// <param name="excludeProperties"></param>
        /// <returns></returns>
        public virtual async Task<VendorTimingModel> PrepareVendorTimingModelAsync(VendorTimingModel model,
            Vendor vendor, VendorTiming vendorTiming, int dayId, bool excludeProperties = false)
        {
            if (vendor == null)
                throw new ArgumentNullException(nameof(vendor));

            model ??= new VendorTimingModel();

            if (vendorTiming != null)
            {
                //whether to fill in some of properties
                if (!excludeProperties)
                {
                    model.Id = vendorTiming.Id;
                    model.OpenTimeUtc = vendorTiming.OpenTimeUtc.HasValue ? vendorTiming.OpenTimeUtc.Value.TimeOfDay : null;
                    model.OpenTimeStr = vendorTiming.OpenTimeUtc.HasValue ? vendorTiming.OpenTimeUtc.Value.ToString("hh:mm tt") : "";
                    model.CloseTimeUtc = vendorTiming.CloseTimeUtc.HasValue ? vendorTiming.CloseTimeUtc.Value.TimeOfDay : null;
                    model.CloseTimeStr = vendorTiming.CloseTimeUtc.HasValue ? vendorTiming.CloseTimeUtc.Value.ToString("hh:mm tt") : "";
                    model.DayOff = vendorTiming.DayOff;
                }
            }

            model.DayId = dayId;
            model.Day = Enum.GetName(typeof(DayofWeek), dayId);

            return model;
        }

        /// <summary>
        /// Prepare vendor timing search model
        /// </summary>
        /// <param name="searchModel">Vendor tiing search model</param>
        /// <param name="vendor">Vendor</param>
        /// <returns>Vendor timing search model</returns>
        protected virtual VendorTimingSearchModel PrepareVendorTimingSearchModel(VendorTimingSearchModel searchModel, Vendor vendor)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            if (vendor == null)
                throw new ArgumentNullException(nameof(vendor));

            searchModel.VendorId = vendor.Id;

            //prepare page parameters
            searchModel.SetGridPageSize();

            return searchModel;
        }

        /// <summary>
        /// Prepare vendor timing list model
        /// </summary>
        /// <param name="searchModel"></param>
        /// <param name="vendor"></param>
        /// <returns></returns>
        public virtual async Task<VendorTimingListModel> PrepareVendorTimingListModel(VendorTimingSearchModel searchModel, Vendor vendor)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            if (vendor == null)
                throw new ArgumentNullException(nameof(vendor));

            //get days
            var days = Enum.GetValues(typeof(DayofWeek)).Cast<DayofWeek>().ToList();
            var daysToPageList = days.ToPagedList(searchModel);

            //prepare list model
            var model = await new VendorTimingListModel().PrepareToGridAsync(searchModel, daysToPageList, () =>
            {
                //fill in model values from the entity
                return days.SelectAwait(async day =>
                {
                    //fill in model values from the entity        
                    var vendorTimingModel = new VendorTimingModel
                    {
                        Day = await _localizationService.GetLocalizedEnumAsync(day),
                        VendorId = vendor.Id,
                        DayId = (int)day,
                    };

                    //get vendor timing by vendor id and day id
                    var vendorTiming = await _vendorTimingService.GetVendorTimingByVendorIdAsync(vendor.Id, (int)day);
                    if (vendorTiming != null)
                    {
                        vendorTimingModel.Id = vendorTiming.Id;
                        vendorTimingModel.OpenTimeStr = vendorTiming.OpenTimeUtc.HasValue ? vendorTiming.OpenTimeUtc.Value.ToString("hh:mm tt") : "";
                        vendorTimingModel.CloseTimeStr = vendorTiming.CloseTimeUtc.HasValue ? vendorTiming.CloseTimeUtc.Value.ToString("hh:mm tt") : "";
                        vendorTimingModel.DayOff = vendorTiming.DayOff;
                    }

                    return vendorTimingModel;
                });
            });

            return model;
        }
        #endregion
    }
}