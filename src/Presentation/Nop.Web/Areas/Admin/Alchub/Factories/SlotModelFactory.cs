using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Domain.Slots;
using Nop.Services.Helpers;
using Nop.Services.Slots;
using Nop.Services.Vendors;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Slots;
using Nop.Web.Framework.Models.Extensions;

namespace Nop.Web.Areas.Admin.Factories
{
    /// <summary>
    /// Represents the slot model factory implementation
    /// </summary>
    public partial class SlotModelFactory : ISlotModelFactory
    {
        #region Fields

        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ISlotService _slotService;
        private readonly IVendorService _vendorService;
        private readonly IWorkContext _workContext;
        private readonly IBaseAdminModelFactory _baseAdminModelFactory;
        #endregion

        #region Ctor

        public SlotModelFactory(IDateTimeHelper dateTimeHelper,
            ISlotService slotService, IVendorService vendorService, IWorkContext workContext, IBaseAdminModelFactory baseAdminModelFactory)
        {
            _dateTimeHelper = dateTimeHelper;
            _slotService = slotService;
            _vendorService = vendorService;
            _workContext = workContext;
            _baseAdminModelFactory = baseAdminModelFactory;
        }

        #endregion

        #region Methods

        #region Delivery Slot
        /// <summary>
        /// Prepare zone search model
        /// </summary>
        /// <param name="searchModel">Zone search model</param>
        /// <returns>Zone search model</returns>
        public virtual async Task<SlotSearchModel> PrepareZoneSearchModel(SlotSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var vendor = await _workContext.GetCurrentVendorAsync();

            await _baseAdminModelFactory.PrepareVendorsAsync(searchModel.VendorList);

            //prepare "Active" filter (0 - all; 1 - Active only; 2 - InActive)
            searchModel.ActiveList.Add(new SelectListItem
            {
                Value = "0",
                Text = "All"
            });
            searchModel.ActiveList.Add(new SelectListItem
            {
                Value = "1",
                Text = "True"
            });
            searchModel.ActiveList.Add(new SelectListItem
            {
                Value = "2",
                Text = "False"
            });

            //a vendor should have access only to his products
            searchModel.IsLoggedInAsVendor = vendor != null ? true : false;

            searchModel.SetGridPageSize();

            if (vendor != null)
            {
                searchModel.SelectedVendorId = vendor.Id;
                searchModel.CreatedBy = 0;
            }
            else
            {
                searchModel.CreatedBy = null;
            }

            var overrideActive = searchModel.SelectedActiveId == 0 ? null : (bool?)(searchModel.SelectedActiveId == 1);
            var zones = await _slotService.GetAllZones(pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize, zoneName: searchModel.ZoneName, isActive: null, vendorId: searchModel.SelectedVendorId,createdBy:searchModel.CreatedBy);
            if (zones.Count > 0)
            {
                searchModel.IsCount = true;
            }



            return searchModel;
        }

        /// <summary>
        /// Prepare paged zone list model
        /// </summary>
        /// <param name="searchModel">Zone search model</param>
        /// <returns>Zone list model</returns>
        public virtual async Task<ZoneListModel> PrepareZoneListModel(SlotSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var overrideActive = searchModel.SelectedActiveId == 0 ? null : (bool?)(searchModel.SelectedActiveId == 1);

            //get newsletter subscriptions
            var zones = await _slotService.GetAllZones(pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize, zoneName: searchModel.ZoneName, isActive: overrideActive, vendorId: searchModel.SelectedVendorId, createdBy:searchModel.CreatedBy);

            //prepare list model
            var model = await new ZoneListModel().PrepareToGridAsync(searchModel, zones, () =>
            {
                return zones.SelectAwait(async zone =>
                {
                    //fill in model values from the entity
                    var zoneModel = new ZoneModel();
                    zoneModel.Id = zone.Id;
                    zoneModel.Name = zone.Name;
                    zoneModel.IsActive = zone.IsActive;
                    zoneModel.CreatedOn = zone.CreatedOnUtc;
                    var vendor = await _vendorService.GetVendorByIdAsync(zone.VendorId);
                    zoneModel.VendorName = vendor != null ? vendor.Name : "Admin";
                    return zoneModel;
                });
            });

            return model;
        }

        /// <summary>
        /// Prepare zone model
        /// </summary>
        /// <param name="model">Zone model</param>
        /// <param name="zone">Zone</param>
        /// <param name="excludeProperties">Whether to exclude populating of some properties of model</param>
        /// <returns>Zone model</returns>
        public virtual async Task<ZoneModel> PrepareZoneModel(ZoneModel model, Zone zone, bool excludeProperties = false)
        {
            if (zone != null)
            {
                //fill in model values from the entity
                model ??= zone.ToModel<ZoneModel>();
                model.Name = zone.Name;
                model.CreatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(zone.CreatedOnUtc, DateTimeKind.Utc);

            }
            if (zone == null)
            {
                model.IsActive = true;
            }

            //prepare available vendors
            var availableVendorItems = await _vendorService.GetAllVendorsAsync();
            model.VendorList.Add(new SelectListItem { Text = "All", Value = "0" });
            foreach (var vendorItem in availableVendorItems)
            {
                SelectListItem item = new SelectListItem();
                item.Text = vendorItem.Name;
                item.Value = Convert.ToString(vendorItem.Id);
                model.VendorList.Add(item);
            }
            model.IsLoggedInAsVendor = await _workContext.GetCurrentVendorAsync() != null;
            return model;
        }

        #endregion

        #region Pickup Slot
        /// <summary>
        /// Prepare zone search model
        /// </summary>
        /// <param name="searchModel">Zone search model</param>
        /// <returns>Zone search model</returns>
        public virtual async Task<PickupSlotSearchModel> PreparePickupZoneSearchModel(PickupSlotSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var vendor = await _workContext.GetCurrentVendorAsync();

            await _baseAdminModelFactory.PrepareVendorsAsync(searchModel.VendorList);

            //prepare "Active" filter (0 - all; 1 - Active only; 2 - InActive)
            searchModel.ActiveList.Add(new SelectListItem
            {
                Value = "0",
                Text = "All"
            });
            searchModel.ActiveList.Add(new SelectListItem
            {
                Value = "1",
                Text = "True"
            });
            searchModel.ActiveList.Add(new SelectListItem
            {
                Value = "2",
                Text = "False"
            });

            //a vendor should have access only to his products
            searchModel.IsLoggedInAsVendor = vendor != null ? true : false;

            searchModel.SetGridPageSize();

            if (vendor != null)
            {
                searchModel.SelectedVendorId = vendor.Id;
            }

            var overrideActive = searchModel.SelectedActiveId == 0 ? null : (bool?)(searchModel.SelectedActiveId == 1);
            var zones = await _slotService.GetAllZones(pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize, zoneName: searchModel.ZoneName, isActive: null, vendorId: searchModel.SelectedVendorId,isPickup:true);
            if (zones.Count > 0)
            {
                searchModel.IsCount = true;
            }



            return searchModel;
        }

        /// <summary>
        /// Prepare paged zone list model
        /// </summary>
        /// <param name="searchModel">Zone search model</param>
        /// <returns>Zone list model</returns>
        public virtual async Task<ZoneListModel> PreparePickupZoneListModel(PickupSlotSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var overrideActive = searchModel.SelectedActiveId == 0 ? null : (bool?)(searchModel.SelectedActiveId == 1);

            //get newsletter subscriptions
            var zones = await _slotService.GetAllZones(pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize, zoneName: searchModel.ZoneName, isActive: overrideActive, vendorId: searchModel.SelectedVendorId, isPickup: true);

            //prepare list model
            var model = await new ZoneListModel().PrepareToGridAsync(searchModel, zones, () =>
            {
                return zones.SelectAwait(async zone =>
                {
                    //fill in model values from the entity
                    var zoneModel = new ZoneModel();
                    zoneModel.Id = zone.Id;
                    zoneModel.Name = zone.Name;
                    var vendor = await _vendorService.GetVendorByIdAsync(zone.VendorId);
                    zoneModel.VendorName = vendor != null ? vendor.Name : "Admin";
                    zoneModel.IsActive = zone.IsActive;
                    zoneModel.CreatedOn = zone.CreatedOnUtc;

                    return zoneModel;
                });
            });

            return model;
        }

        /// <summary>
        /// Prepare zone model
        /// </summary>
        /// <param name="model">Zone model</param>
        /// <param name="zone">Zone</param>
        /// <param name="excludeProperties">Whether to exclude populating of some properties of model</param>
        /// <returns>Zone model</returns>
        public virtual async Task<ZoneModel> PreparePickupZoneModel(ZoneModel model, Zone zone, bool excludeProperties = false)
        {
            if (zone != null)
            {
                //fill in model values from the entity
                model ??= zone.ToModel<ZoneModel>();
                model.Name = zone.Name;
                model.CreatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(zone.CreatedOnUtc, DateTimeKind.Utc);

            }
            if (zone == null)
            {
                model.IsActive = true;
            }
            return model;
        }
        #endregion

        #endregion
    }
}