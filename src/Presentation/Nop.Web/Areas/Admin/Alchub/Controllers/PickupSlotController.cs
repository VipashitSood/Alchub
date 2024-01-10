using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Calendar;
using Nop.Core.Domain.Slots;
using Nop.Services.Catalog;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Services.Slots;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Models.Slots;

namespace Nop.Web.Areas.Admin.Controllers
{
    public partial class PickupSlotController : BaseAdminController
    {
        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly IPermissionService _permissionService;
        private readonly ISlotModelFactory _slotModelFactory;
        private readonly ISlotService _slotService;
        private readonly IWorkContext _workContext;
        private readonly IBaseAdminModelFactory _baseAdminModelFactory;
        private readonly ICategoryService _categoryService;
        #endregion

        #region Ctor

        public PickupSlotController(
            ILocalizationService localizationService,
            INotificationService notificationService,
            IPermissionService permissionService,
            ISlotModelFactory slotModelFactory,
            ISlotService slotService,
            IWorkContext workContext,
            IBaseAdminModelFactory baseAdminModelFactory,
            ICategoryService categoryService
            )
        {
            _localizationService = localizationService;
            _notificationService = notificationService;
            _permissionService = permissionService;
            _slotModelFactory = slotModelFactory;
            _slotService = slotService;
            _workContext = workContext;
            _baseAdminModelFactory = baseAdminModelFactory;
            _categoryService = categoryService;
        }

        #endregion

        #region Methods

        #region Zone
        public async Task<IActionResult> Index()
        {
            return RedirectToAction("List");
        }

        public async Task<IActionResult> List()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePickupSlots))
                return AccessDeniedView();

            //prepare model
            var model = await _slotModelFactory.PreparePickupZoneSearchModel(new PickupSlotSearchModel());

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ZoneList(PickupSlotSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePickupSlots))
                return AccessDeniedView();

            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var vendor = await _workContext.GetCurrentVendorAsync();

            if (vendor != null)
            {
                searchModel.SelectedVendorId = vendor.Id;
            }

            searchModel.IsLoggedInAsVendor = await _workContext.GetCurrentVendorAsync() != null;

            var model = await _slotModelFactory.PreparePickupZoneListModel(searchModel);

            return Json(model);
        }

        public async Task<IActionResult> Create()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePickupSlots))
                return AccessDeniedView();

            //prepare model
            var model = await _slotModelFactory.PreparePickupZoneModel(new ZoneModel(), null);

            return PartialView(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(ZoneModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePickupSlots))
                return AccessDeniedView();

            Zone zone = new Zone();

            if (ModelState.IsValid)
            {
                if (model != null)
                {
                    var vendor = await _workContext.GetCurrentVendorAsync();
                    var existingZone = await _slotService.GetZoneByZoneName(model.Name);
                    if (existingZone != null)
                    {
                        _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Alchub.Already.Created.Slot.Name"));
                        return RedirectToAction("List");
                    }
                    else
                    {
                        zone.CreatedOnUtc = DateTime.UtcNow;
                        zone.Name = model.Name;
                        zone.IsActive = model.IsActive;
                        zone.IsPickUp = true;
                        zone.VendorId = vendor != null ? vendor.Id : 0;
                        await _slotService.InsertZone(zone);
                    }
                }
                else
                {
                    model = await _slotModelFactory.PreparePickupZoneModel(model, null);
                    //if we got this far, something failed, redisplay form
                    return View(model);
                }
            }
            return RedirectToAction("List");
        }

        [HttpPost]
        public async Task<IActionResult> ZoneUpdate(ZoneModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePickupSlots))
                return AccessDeniedView();

            if (model != null)
            {
                if (string.IsNullOrEmpty(model.Name))
                {
                    return Json(false);
                }
                var zone = await _slotService.GetZoneById(model.Id);
                if (zone != null)
                {
                    //fill entity from model
                    zone.Id = model.Id;
                    zone.Name = model.Name;
                    zone.IsActive = model.IsActive;
                    zone.IsPickUp = true;
                    await _slotService.UpdateZone(zone);
                }
            }
            return Json(model);
        }

        [HttpPost]
        public async Task<IActionResult> ZoneDelete(ZoneModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePickupSlots))
                return AccessDeniedView();

            if (model != null)
            {
                var zone = await _slotService.GetZoneById(model.Id);
                if (zone != null)
                {
                    await _slotService.DeleteZone(zone);
                }
            }
            return Json(model);
        }

        #endregion

        #region Delivery Slots Availability

        public async Task<IActionResult> AvailableCalendar(int id)
        {
            if (id == 0)
                return RedirectToAction("List");

            var zone = await _slotService.GetZoneById(id);
            if (zone != null)
            {
                return View(zone);
            }
            return View(null);
        }

        [HttpGet]
        public async Task<JsonResult> GetAvailableCalendarEvents(string start, string end, int zoneId)
        {
            try
            {
                var startDate = DateTime.Parse(start);
                var endDate = DateTime.Parse(end);
                var events = await _slotService.GetPickupSlotByZoneAndDate(startDate, endDate, zoneId);
                if (events.ToList() != null)
                {
                    return Json(events.Select(x => new CalendarEventDto(
                                x.Id,
                                x.Start,
                                (x.End.TimeOfDay.Hours == 23 && x.End.TimeOfDay.Minutes == 59) ? x.End.AddMinutes(1) : x.End,//handel 12am
                                "",
                                x.IsUnavailable ? "#2E9F05" : "#ababab",// "color"
                                "",
                                0,
                                x.IsRecurring,
                                x.RecurringType,
                                x.WeekDays,
                                x.Sequence_Id,
                                x.Capacity,
                                x.Price,
                                x.IsUnavailable,
                                x.Name)));
                }
                return Json(false);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        [HttpGet]
        public async Task<ActionResult> AddAvailableCalendarEvents(string start, string end, int id, int zoneId, bool isrecurring, RecurringType recurringtype, string weekdays, string sequenceIds, int capacity, decimal price, bool isunavailable, string name) /*AddAvailableCalendarEvents start ,end, id, sectorid, upcomingweek ,sequenceid */
        {
            try
            {
                var zone = await _slotService.GetZoneById(zoneId);
                var workingLanguage = await _workContext.GetWorkingLanguageAsync();
                var startDate = DateTime.ParseExact(start, "dd/MM/yyyy HH:mm:ss", new CultureInfo(workingLanguage.LanguageCulture));
                var endDate = DateTime.ParseExact(end, "dd/MM/yyyy HH:mm:ss", new CultureInfo(workingLanguage.LanguageCulture));
                var eventDto = new PickupSlotModel
                {
                    Start = startDate,
                    End = (endDate.TimeOfDay.Hours == 23 && endDate.TimeOfDay.Minutes == 59) ? endDate.AddMinutes(1) : endDate,//handel 12am
                    IsRecurring = isrecurring,
                    RecurringType = recurringtype,
                    WeekDays = weekdays,
                    Sequence_Id = sequenceIds,
                    Zone = new Zone() { Id = zoneId, Name = zone != null ? zone.Name : name },
                    Price = price,
                    Capacity = capacity,
                    Day = Convert.ToString(startDate.DayOfWeek),
                    EndDate = DateTime.UtcNow.AddYears(1),
                    IsUnavailable = true,
                    Name = zone != null ? zone.Name : name,
                    Id = id
                };

                //prepare model categories
                await _baseAdminModelFactory.PrepareCategoriesAsync(eventDto.AvailableCategories, false);

                if (id > 0)
                {
                    eventDto.SelectedCategoryIds = (await _slotService.GetPickupSlotCategoriesBySlotIdAsync(id))
                                .Select(productCategory => productCategory.CategoryId).ToList();
                }

                return PartialView(eventDto);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        [HttpPost]
        public async Task<ActionResult> AddAvailableCalendarEvents(PickupSlotModel model)
        {
            var errors = new List<string>();
            if (ModelState.TryGetValue("Capacity", out var capacity))
            {
                string capacityAttemptedValue = capacity.AttemptedValue.ToString();
                if (string.IsNullOrEmpty(capacityAttemptedValue))
                {
                    errors.Add(await _localizationService.GetResourceAsync("Alchub.Slot.Capacity.Required"));
                }
            }
            if (ModelState.TryGetValue("Price", out var price))
            {
                string priceAttemptedValue = price.AttemptedValue.ToString();
                if (string.IsNullOrEmpty(priceAttemptedValue))
                {
                    errors.Add(await _localizationService.GetResourceAsync("Alchub.Slot.Price.Required"));
                }
            }
            if (model.IsRecurring && model.RecurringType == RecurringType.Weekly && string.IsNullOrEmpty(model.WeekDays))
            {
                errors.Add(await _localizationService.GetResourceAsync("Alchub.Slot.Weekly.SelectWeek.Required"));
            }
            if (model.Start.Date < DateTime.UtcNow.Date)
            {
                errors.Add(await _localizationService.GetResourceAsync("Alchub.Slot.Create.Date.Greater.Than"));
            }
            if (model.Start.Date > model.EndDate)
            {
                errors.Add(await _localizationService.GetResourceAsync("Alchub.Slot.End.Date.Greater.Than.Start"));
            }

            if (errors != null && errors.Count > 0)
            {
                return Json(new { result = false, message = string.Join("; ", errors) });
            }

            if (ModelState.IsValid)
            {
                if (model.IsRecurring == false && model.Id == 0 && model.IsUnavailable == false)
                {
                    var sameSlotCount = await _slotService.GetSamePickupSlotwithSameStartEnd(model.Start, model.End, model.Zone.Id);
                    if (sameSlotCount.Count > 0)
                    {
                        string msgdate = string.Join("; ", sameSlotCount.Select(x => x.Start.Date.ToString("MM/dd/yyyy") + "_" + x.Start.Hour + ":" + x.Start.Minute + x.Start.Second + "-" + x.End.Hour + ":" + x.End.Minute + x.End.Second));
                        string ids = string.Join(", ", sameSlotCount.Select(x => x.Id));
                        string categoris = model.SelectedCategoryIds != null ? string.Join(", ", model.SelectedCategoryIds) : "";
                        return Json(new { result = true, value = msgdate, Id = ids, start = model.Start, end = model.End, enddate = model.EndDate, zoneid = model.Zone.Id, isrecurring = model.IsRecurring, recurringtype = model.RecurringType, weekdays = model.WeekDays, capacity = model.Capacity, price = model.Price, selectedcategories = categoris });
                    }
                }
                else if (model.IsRecurring == true && model.Id == 0 && model.IsUnavailable == false)
                {
                    var zonecount = await _slotService.GetPickupSlotUpcomingWeekStartEnd(model.Start, model.End, model.EndDate, model.Zone.Id, model.RecurringType, model.WeekDays);
                    if (zonecount.Count > 0)
                    {
                        string msgdate = string.Join("; ", zonecount.Select(x => x.Start.Date.ToString("MM/dd/yyyy") + "_" + x.Start.Hour + ":" + x.Start.Minute + x.Start.Second + "-" + x.End.Hour + ":" + x.End.Minute + x.End.Second));
                        string ids = string.Join(", ", zonecount.Select(x => x.Id));
                        string categoris = model.SelectedCategoryIds != null ? string.Join(", ", model.SelectedCategoryIds) : "";
                        return Json(new { result = true, value = msgdate, Id = ids, start = model.Start, end = model.End, enddate = model.EndDate, zoneid = model.Zone.Id, isrecurring = model.IsRecurring, recurringtype = model.RecurringType, weekdays = model.WeekDays, capacity = model.Capacity, price = model.Price, selectedcategories = categoris });
                    }
                }
                else if (model.IsRecurring == false && model.Id == 0 && model.IsUnavailable == true)
                {
                    var sameSlotCount = await _slotService.GetSamePickupSlotwithSameStartEnd(model.Start, model.End, model.Zone.Id);
                    if (sameSlotCount.Count > 0)
                    {
                        string msgdate = string.Join("; ", sameSlotCount.Select(x => x.Start.Date.ToString("MM/dd/yyyy") + "_" + x.Start.Hour + ":" + x.Start.Minute + x.Start.Second + "-" + x.End.Hour + ":" + x.End.Minute + x.End.Second));
                        string ids = string.Join(", ", sameSlotCount.Select(x => x.Id));
                        string categoris = model.SelectedCategoryIds != null ? string.Join(", ", model.SelectedCategoryIds) : "";
                        return Json(new { result = true, value = msgdate, Id = ids, start = model.Start, end = model.End, enddate = model.EndDate, zoneid = model.Zone.Id, isrecurring = model.IsRecurring, recurringtype = model.RecurringType, weekdays = model.WeekDays, capacity = model.Capacity, price = model.Price, selectedcategories = categoris });
                    }
                }
                else if (model.IsRecurring == true && model.Id == 0 && model.IsUnavailable == true)
                {
                    var zonecount = await _slotService.GetPickupSlotUpcomingWeekStartEnd(model.Start, model.End, model.EndDate, model.Zone.Id, model.RecurringType, model.WeekDays);
                    if (zonecount.Count > 0)
                    {
                        string msgdate = string.Join("; ", zonecount.Select(x => x.Start.Date.ToString("MM/dd/yyyy") + "_" + x.Start.Hour + ":" + x.Start.Minute + x.Start.Second + "-" + x.End.Hour + ":" + x.End.Minute + x.End.Second));
                        string ids = string.Join(", ", zonecount.Select(x => x.Id));
                        string categoris = model.SelectedCategoryIds != null ? string.Join(", ", model.SelectedCategoryIds) : "";
                        return Json(new { result = true, value = msgdate, Id = ids, start = model.Start, end = model.End, enddate = model.EndDate, zoneid = model.Zone.Id, isrecurring = model.IsRecurring, recurringtype = model.RecurringType, weekdays = model.WeekDays, capacity = model.Capacity, price = model.Price, selectedcategories = categoris });
                    }
                }
                try
                {
                    bool isAddSlotforSelectedDay = true; //created slot for respective day which admin is selected from admoin section
                    var freeslot = new PickupSlot();
                    var zone = await _slotService.GetZoneById(model.Zone.Id);
                    if (zone != null)
                    {
                        freeslot.Zone = zone;
                    }
                    // create guid id for sequenceid
                    model.Sequence_Id = Guid.NewGuid().ToString("N");
                    freeslot.Sequence_Id = model.Sequence_Id;
                    freeslot.Capacity = model.Capacity;
                    freeslot.Price = model.Price;
                    freeslot.ZoneId = model.Zone.Id;
                    freeslot.IsRecurring = model.IsRecurring;
                    freeslot.RecurringType = model.RecurringType;
                    freeslot.WeekDays = model.WeekDays;
                    freeslot.IsUnavailable = model.IsUnavailable;
                    freeslot.Name = zone.Name;

                    //handle 24hrs slot. - 03-05-23
                    //if user select slot timing till 12am, then end date will have next day. 
                    if (model.Start.Day != model.End.Day)
                    {
                        model.End = model.End.AddMinutes(-1); //=11:59pm
                    }

                    //check IsRecurring 
                    if (model.IsRecurring == false)
                    {
                        // create guid id for sequenceid
                        freeslot.Start = model.Start;
                        freeslot.End = model.End;
                        await _slotService.InsertPickupSlot(freeslot);
                        await SaveCategoryMappingsAsync(freeslot, model);
                    }
                    else
                    {
                        if (model.RecurringType == RecurringType.Daily)
                        {

                            DateTime Date = DateTime.Now;
                            DateTime startDate = Convert.ToDateTime(model.Start);
                            DateTime endDate = Convert.ToDateTime(model.End);
                            while (startDate <= model.EndDate) //As we are creating allowed entries slot Given date
                            {
                                if (!isAddSlotforSelectedDay)
                                {
                                    startDate = startDate.AddDays(1);
                                    endDate = endDate.AddDays(1);
                                }
                                if (startDate.Date <= model.EndDate)
                                {
                                    freeslot.Start = startDate;
                                    freeslot.End = endDate;
                                    await _slotService.InsertPickupSlot(freeslot);
                                    isAddSlotforSelectedDay = false;
                                    await SaveCategoryMappingsAsync(freeslot, model);
                                }
                            }
                        }
                        else if (model.RecurringType == RecurringType.Weekly)
                        {
                            var day = DateTime.Now.DayOfWeek;
                            var weekDays = model.WeekDays;
                            weekDays = weekDays.Trim().TrimStart(',');

                            string[] values = weekDays.Split(',');
                            for (int j = 0; j < values.Length; j++)
                            {
                                values[j] = values[j].Trim();

                                // Get the indicated date.
                                DateTime Date = DateTime.Now;

                                DateTime startDateForWeekDays = Convert.ToDateTime(model.Start);
                                DateTime endDateForWeekDays = Convert.ToDateTime(model.End);
                                // Get the number of days between today's
                                // day of the week .
                                Enum.TryParse<DayOfWeek>(values[j], out day);
                                int num_days = day - startDateForWeekDays.DayOfWeek;
                                if (num_days < 0)
                                    num_days += 7;

                                // Add the needed number of days.
                                startDateForWeekDays = startDateForWeekDays.AddDays(num_days);
                                endDateForWeekDays = endDateForWeekDays.AddDays(num_days);
                                isAddSlotforSelectedDay = true;
                                DateTime startDate = startDateForWeekDays;
                                DateTime endDate = endDateForWeekDays;
                                while (startDate <= model.EndDate) //As we are creating allowed entries slot Given date
                                {
                                    if (!isAddSlotforSelectedDay)
                                    {
                                        startDate = startDate.AddDays(7 * 1);
                                        endDate = endDate.AddDays(7 * 1);
                                    }
                                    if (startDate.Date <= model.EndDate)
                                    {
                                        freeslot.Start = startDate;
                                        freeslot.End = endDate;
                                        await _slotService.InsertPickupSlot(freeslot);
                                        isAddSlotforSelectedDay = false;
                                        await SaveCategoryMappingsAsync(freeslot, model);
                                    }
                                }
                            }
                        }
                        else if (model.RecurringType == RecurringType.CurrentDay)
                        {
                            DateTime Date = DateTime.Now;
                            DateTime startDate = Convert.ToDateTime(model.Start);
                            DateTime endDate = Convert.ToDateTime(model.End);
                            while (startDate <= model.EndDate) //As we are creating allowed entries slot Given date
                            {
                                if (!isAddSlotforSelectedDay)
                                {
                                    startDate = startDate.AddDays(7 * 1);
                                    endDate = endDate.AddDays(7 * 1);
                                }
                                if (startDate.Date <= model.EndDate)
                                {
                                    freeslot.Start = startDate;
                                    freeslot.End = endDate;
                                    await _slotService.InsertPickupSlot(freeslot);
                                    isAddSlotforSelectedDay = false;
                                    await SaveCategoryMappingsAsync(freeslot, model);
                                }
                            }
                        }
                    }
                    return Json(true);
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
            }
            string errormsg = string.Join("; ", ModelState.Values
                                        .SelectMany(x => x.Errors)
                                        .Select(x => x.ErrorMessage));

            return Json(new { result = false, message = errormsg });
        }

        //overlapping calendar event remove
        public async Task<JsonResult> RemoveEvents(string data, string start, string end, string enddate, int zoneId, bool isrecurring, RecurringType recurringtype, string weekdays, int capacity, decimal price, string selectedcategories)
        {
            if (data != null)
            {
                List<string> Ids = data.Split(',').ToList<string>();
                foreach (var id in Ids)
                {
                    int calendarId = Convert.ToInt32(id);
                    await _slotService.DeletePickupSlotIdCategory(calendarId);
                    await _slotService.RemovePickupSlot(calendarId);
                    await _slotService.DeleteCustomerProductBySlotId(calendarId);
                }
                List<string> categoriesIds = !string.IsNullOrEmpty(selectedcategories) ? selectedcategories.Split(',').ToList<string>() : null;
                PickupSlotModel model = new PickupSlotModel();
                IList<int> newCatgeoryList = new List<int>();
                if (categoriesIds != null)
                {
                    foreach (var categoriesId in categoriesIds)
                    {
                        newCatgeoryList.Add(Convert.ToInt32(categoriesId));
                    }
                }
                model.SelectedCategoryIds = newCatgeoryList;
                try
                {
                    bool isAddSlotforSelectedDay = true;
                    var freeslot = new PickupSlot();
                    var zone = await _slotService.GetZoneById(zoneId);
                    if (zone != null)
                    {
                        freeslot.Zone = zone;
                    }
                    // create guid id for sequenceid
                    freeslot.Sequence_Id = Guid.NewGuid().ToString("N");
                    freeslot.Capacity = capacity;
                    freeslot.Price = price;
                    freeslot.ZoneId = zoneId;
                    freeslot.IsRecurring = isrecurring;
                    freeslot.IsUnavailable = true;
                    freeslot.RecurringType = recurringtype;
                    freeslot.WeekDays = weekdays;

                    var workinngLanguage = await _workContext.GetWorkingLanguageAsync();
                    DateTime newenddate = DateTime.ParseExact(enddate, "dd/MM/yyyy HH:mm:ss", new CultureInfo(workinngLanguage.LanguageCulture));

                    //check upcomingweek 
                    if (isrecurring == false)
                    {
                        // create guid id for sequenceid
                        freeslot.Start = DateTime.ParseExact(start, "dd/MM/yyyy HH:mm:ss", new CultureInfo(workinngLanguage.LanguageCulture));
                        freeslot.End = DateTime.ParseExact(end, "dd/MM/yyyy HH:mm:ss", new CultureInfo(workinngLanguage.LanguageCulture));
                        await _slotService.InsertPickupSlot(freeslot);
                        await SaveCategoryMappingsAsync(freeslot, model);
                    }
                    else
                    {
                        if (recurringtype == RecurringType.Daily)
                        {
                            DateTime Date = DateTime.Now;
                            DateTime startDate = DateTime.ParseExact(start, "dd/MM/yyyy HH:mm:ss", new CultureInfo(workinngLanguage.LanguageCulture));
                            DateTime endDate = DateTime.ParseExact(end, "dd/MM/yyyy HH:mm:ss", new CultureInfo(workinngLanguage.LanguageCulture));
                            while (startDate <= newenddate) //As we are creating allowed entries slot Given date
                            {
                                if (!isAddSlotforSelectedDay)
                                {
                                    startDate = startDate.AddDays(1);
                                    endDate = endDate.AddDays(1);
                                }
                                if (startDate.Date <= newenddate)
                                {
                                    freeslot.Start = startDate;
                                    freeslot.End = endDate;
                                    await _slotService.InsertPickupSlot(freeslot);
                                    isAddSlotforSelectedDay = false;
                                    await SaveCategoryMappingsAsync(freeslot, model);
                                }
                            }
                        }
                        else if (recurringtype == RecurringType.Weekly)
                        {
                            var day = DateTime.Now.DayOfWeek;
                            var weekDays = weekdays;
                            weekDays = weekDays.Trim().TrimStart(',');

                            string[] values = weekDays.Split(',');
                            for (int j = 0; j < values.Length; j++)
                            {
                                values[j] = values[j].Trim();

                                // Get the indicated date.
                                DateTime Date = DateTime.Now;

                                DateTime startDateForWeekDays = DateTime.ParseExact(start, "dd/MM/yyyy HH:mm:ss", new CultureInfo(workinngLanguage.LanguageCulture));
                                DateTime endDateForWeekDays = DateTime.ParseExact(end, "dd/MM/yyyy HH:mm:ss", new CultureInfo(workinngLanguage.LanguageCulture));

                                // Get the number of days between today's
                                // day of the week .
                                Enum.TryParse<DayOfWeek>(values[j], out day);
                                int num_days = day - startDateForWeekDays.DayOfWeek;
                                if (num_days < 0)
                                    num_days += 7;

                                // Add the needed number of days.
                                startDateForWeekDays = startDateForWeekDays.AddDays(num_days);
                                endDateForWeekDays = endDateForWeekDays.AddDays(num_days);

                                isAddSlotforSelectedDay = true;
                                DateTime startDate = startDateForWeekDays;
                                DateTime endDate = endDateForWeekDays;
                                while (startDate <= newenddate) //As we are creating allowed entries slot Given date
                                {
                                    if (!isAddSlotforSelectedDay)
                                    {
                                        startDate = startDate.AddDays(7 * 1);
                                        endDate = endDate.AddDays(7 * 1);
                                    }
                                    if (startDate.Date <= newenddate)
                                    {
                                        freeslot.Start = startDate;
                                        freeslot.End = endDate;
                                        await _slotService.InsertPickupSlot(freeslot);
                                        isAddSlotforSelectedDay = false;
                                        await SaveCategoryMappingsAsync(freeslot, model);
                                    }
                                }
                            }
                        }
                        else if (recurringtype == RecurringType.CurrentDay)
                        {
                            DateTime Date = DateTime.Now;
                            DateTime startDate = DateTime.ParseExact(start, "dd/MM/yyyy HH:mm:ss", new CultureInfo(workinngLanguage.LanguageCulture));
                            DateTime endDate = DateTime.ParseExact(end, "dd/MM/yyyy HH:mm:ss", new CultureInfo(workinngLanguage.LanguageCulture));
                            while (startDate <= newenddate) //As we are creating allowed entries slot Given date
                            {
                                if (!isAddSlotforSelectedDay)
                                {
                                    startDate = startDate.AddDays(7 * 1);
                                    endDate = endDate.AddDays(7 * 1);
                                }
                                if (startDate <= newenddate)
                                {
                                    freeslot.Start = startDate;
                                    freeslot.End = endDate;
                                    await _slotService.InsertPickupSlot(freeslot);
                                    isAddSlotforSelectedDay = false;
                                    await SaveCategoryMappingsAsync(freeslot, model);
                                }
                            }
                        }
                    }
                    return Json(true);
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
            }
            return Json(true);
        }
        //allowed calendar event remove  (changes done by tmotions)
        [HttpPost]
        public async Task<JsonResult> RemoveAvailableCalendarEvents(PickupSlotModel model)
        {

            if (model.Id <= 0)
            {
                throw new Exception("id is missing");
            }
            if (model != null)
            {
                // Remove slot
                await _slotService.DeletePickupSlotIdCategory(model.Id);
                await _slotService.RemovePickupSlot(model.Id);
                await _slotService.DeleteCustomerProductBySlotId(model.Id);
            }
            return Json(true);
        }

        [HttpPost]
        public async Task<JsonResult> RemoveAllAvailableCalendarEvents(PickupSlotModel model)
        {

            if (model.Id <= 0)
            {
                throw new Exception("id is missing");
            }
            // Get all sectorsequenceIds
            var sectorFreeSlotAllowedCalendarEventList = await _slotService.GetPickupSlotSequenceId(model.Sequence_Id);
            foreach (var slot in sectorFreeSlotAllowedCalendarEventList)
            {

                await _slotService.DeletePickupSlotIdCategory(slot.Id);
                await _slotService.DeletePickupSlot(slot);
                await _slotService.DeleteCustomerProductBySlotId(slot.Id);
            }
            return Json(true);
        }

        [HttpPost]
        public async Task<JsonResult> UpdateAvailableCalendarEvents(PickupSlotModel model)
        {

            if (model.Id <= 0)
            {
                throw new Exception("id is missing");
            }
            try
            {
                var errors = new List<string>();
                if (ModelState.TryGetValue("Capacity", out var capacity))
                {
                    string capacityAttemptedValue = capacity.AttemptedValue.ToString();
                    if (string.IsNullOrEmpty(capacityAttemptedValue))
                    {
                        errors.Add(await _localizationService.GetResourceAsync("Alchub.Slot.Capacity.Required"));
                    }
                }
                if (ModelState.TryGetValue("Price", out var price))
                {
                    string priceAttemptedValue = price.AttemptedValue.ToString();
                    if (string.IsNullOrEmpty(priceAttemptedValue))
                    {
                        errors.Add(await _localizationService.GetResourceAsync("Alchub.Slot.Price.Required"));
                    }
                }

                if (errors != null && errors.Count > 0)
                {
                    return Json(new { result = false, message = string.Join("; ", errors) });
                }

                var zoneslot = await _slotService.GetPickupSlotById(model.Id);
                if (zoneslot != null)
                {
                    zoneslot.IsUnavailable = model.IsUnavailable;
                    zoneslot.Name = model.Name;
                    zoneslot.Capacity = model.Capacity;
                    zoneslot.Price = model.Price;
                    if (model.IsUnavailable == true)
                    {
                        zoneslot.IsRecurring = false;
                    }
                    await _slotService.UpdatePickupSlot(zoneslot);
                    await UpdateCategoryMappingsAsync(zoneslot, model);
                }
                return Json(true);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        #endregion

        #region PickupSlot category Mapping 
        protected virtual async Task SaveCategoryMappingsAsync(PickupSlot slot, PickupSlotModel model)
        {
            var existingProductCategories = await _slotService.GetPickupSlotCategoriesBySlotIdAsync(slot.Id);

            //delete categories

            foreach (var existingProductCategory in existingProductCategories)
                if (!model.SelectedCategoryIds.Contains(existingProductCategory.CategoryId))
                    await _slotService.DeletePickupSlotCategory(existingProductCategory);


            if (model.SelectedCategoryIds != null)
            {
                //add categories
                foreach (var categoryId in model.SelectedCategoryIds)
                {
                    if (_slotService.FindPickupSlotCategory(existingProductCategories, slot.Id, categoryId) == null)
                    {
                        //find next display order
                        var existingCategoryMapping = await _slotService.GetPickupSlotCategoriesByCategoryIdAsync(slot.Id, categoryId, showHidden: true);
                        if (existingCategoryMapping.Any())
                            continue;

                        await _slotService.InsertPickupSlotCategory(new PickupSlotCategory
                        {
                            PickupSlotId = slot.Id,
                            CategoryId = categoryId
                        });
                    }
                }
            }
        }

        protected virtual async Task UpdateCategoryMappingsAsync(PickupSlot slot, PickupSlotModel model)
        {
            var existingProductCategories = await _slotService.GetPickupSlotCategoriesBySlotIdAsync(slot.Id);

            //delete categories

            foreach (var existingProductCategory in existingProductCategories)
                if (!model.SelectedCategoryIds.Contains(existingProductCategory.CategoryId))
                {
                    await _slotService.DeletePickupSlotCategory(existingProductCategory);
                    await DeleteProductPickupSlotExcludeCategoriesAsync(slot);
                }


            if (model.SelectedCategoryIds != null)
            {
                //add categories
                foreach (var categoryId in model.SelectedCategoryIds)
                {
                    if (_slotService.FindPickupSlotCategory(existingProductCategories, slot.Id, categoryId) == null)
                    {
                        //find next display order
                        var existingCategoryMapping = await _slotService.GetPickupSlotCategoriesByCategoryIdAsync(slot.Id, categoryId, showHidden: true);
                        if (existingCategoryMapping.Any())
                            continue;

                        await _slotService.InsertPickupSlotCategory(new PickupSlotCategory
                        {
                            PickupSlotId = slot.Id,
                            CategoryId = categoryId
                        });
                        await DeleteProductPickupSlotExcludeCategoriesAsync(slot);
                    }
                }
            }
        }

        protected virtual async Task DeleteProductPickupSlotExcludeCategoriesAsync(PickupSlot slot)
        {
            var customerProductSlotList = await _slotService.GetCustomerCategoryProductSlot(slot.Id);
            foreach (var item in customerProductSlotList)
            {
                await _slotService.DeleteCustomerProductSlotById(item.Id);
            }
        }
        #endregion

        #endregion
    }
}