using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Slots;
using Nop.Core.Domain.Vendors;
using Nop.Core.Events;
using Nop.Data;
using Nop.Services.Alchub.Slots;
using Nop.Services.Catalog;
using Nop.Services.Common;

namespace Nop.Services.Slots
{
    /// <summary>
    /// Slot service
    /// </summary>
    public class SlotService : ISlotService
    {
        #region Fields

        private readonly IEventPublisher _eventPublisher;
        private readonly IRepository<Slot> _slotRepository;
        private readonly IRepository<Zone> _zoneRepository;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IRepository<CustomerOrderSlot> _customerOrderSlotRepository;
        private readonly IRepository<CustomerProductSlot> _customerProductSlotRepository;
        private readonly IRepository<OrderItem> _orderItemRepository;
        private readonly IRepository<SlotCategory> _slotCategoryRepository;
        private readonly IRepository<Nop.Core.Domain.Catalog.Category> _categoryRepository;
        private readonly IRepository<PickupSlot> _pickupSlotRepository;
        private readonly IRepository<PickupSlotCategory> _pickupSlotCategoryRepository;
        private readonly IStaticCacheManager _staticCacheManager;
        #endregion

        #region Ctor

        public SlotService(IEventPublisher eventPublisher,
            IRepository<Slot> slotRepository,
            IRepository<Zone> zoneRepository,
            IGenericAttributeService genericAttributeService,
            IRepository<CustomerOrderSlot> customerOrderSlotRepository,
            IRepository<CustomerProductSlot> customerProductSlotRepository,
            IRepository<OrderItem> orderItemRepository,
            IRepository<SlotCategory> slotCategoryRepository,
            IRepository<Category> categoryRepository,
            IRepository<PickupSlot> pickupSlotRepository,
            IRepository<PickupSlotCategory> pickupSlotCategoryRepository,
            IStaticCacheManager staticCacheManager)
        {
            _eventPublisher = eventPublisher;
            _slotRepository = slotRepository;
            _zoneRepository = zoneRepository;
            _customerOrderSlotRepository = customerOrderSlotRepository;
            _genericAttributeService = genericAttributeService;
            _customerProductSlotRepository = customerProductSlotRepository;
            _orderItemRepository = orderItemRepository;
            _slotCategoryRepository = slotCategoryRepository;
            _categoryRepository = categoryRepository;
            _pickupSlotRepository = pickupSlotRepository;
            _pickupSlotCategoryRepository = pickupSlotCategoryRepository;
            _staticCacheManager = staticCacheManager;
        }

        #endregion

        #region Delivery Slot Methods

        /// <summary>
        /// Inserts a zone free slot calendar event
        /// </summary>
        /// <param name="zoneFreeSlotCalendarEvent">zoneFreeSlotCalendarEvent</param>
        public async Task InsertZoneFreeSlotCalendarEvent(Slot slot)
        {
            if (slot == null)
            {
                throw new ArgumentNullException(nameof(slot));
            }

            //Zone
            await _slotRepository.InsertAsync(slot);

            //Publish event
            await _eventPublisher.EntityInsertedAsync(slot);
        }

        /// <summary>
        /// Update a zone free slot calendar event
        /// </summary>
        /// <param name="slot">slot</param>
        public async Task Update(Slot slot)
        {
            if (slot == null)
            {
                throw new ArgumentNullException(nameof(slot));
            }
            await _slotRepository.UpdateAsync(slot);

            await _eventPublisher.EntityUpdatedAsync(slot);
        }

        /// <summary>
        /// Remove a zone free slot calendar event
        /// </summary>
        /// <param name="slot">slot</param>
        public async Task Remove(int id)
        {
            if (id == 0)
                throw new ArgumentNullException(nameof(id));
            var obj = await _slotRepository.GetByIdAsync(id);

            await _slotRepository.DeleteAsync(obj);

            await _eventPublisher.EntityDeletedAsync(obj);
        }

        /// <summary>
		///  Get sequence id
		/// </summary>
		/// <param name="sequenceId">sequenceId</param>
		/// <returns>zoneFreeSlotCalendarEvent</returns>
        public async Task Delete(Slot slot)
        {
            if (slot == null)
                throw new ArgumentNullException(nameof(slot));

            await _slotRepository.DeleteAsync(slot);

            await _eventPublisher.EntityDeletedAsync(slot);
        }

        /// <summary>
		///  Get sequence id (changes done by tmotions)
		/// </summary>
		/// <param name="sequenceId">sequenceId</param>
		/// <returns>ZoneFreeSlotCalendarEvents</returns>
		public async Task<IList<Slot>> GetSequenceId(string sequenceId)
        {
            if (sequenceId == null)
                throw new ArgumentNullException(nameof(sequenceId));
            var query = _slotRepository.Table.Where(x => x.Sequence_Id == sequenceId);

            return query.ToList();
        }

        /// <summary>
		///  Get zone slot calendar event
		/// </summary>
		/// <param name="start">Start Date</param>
		/// <param name="end">End Date</param>
		/// <param name="zoneId">The zone identifier</param>
		/// <returns>slot</returns>
        public async Task<IEnumerable<Slot>> GetSlotByZoneAndDate(DateTime start, DateTime end, int zoneId)
        {
            if (zoneId == 0)
                return null;
            var events = await GetFreeSlotByZone(zoneId);

            if (events == null)
                throw new ArgumentNullException(nameof(events));

            DateTime startDate = start.Date;
            DateTime endDate = end.Date;

            var query = from e in events
                        where (e.Start.Date) >= startDate.Date
                        && (e.End.Date) <= endDate.Date
                        select e;

            return query.ToList();
        }

        /// <summary>
		///  Get zone free slot calendar event
		/// </summary>
		/// <param name="start">Start Date</param>
		/// <param name="end">End Date</param>
		/// <param name="zoneId">The zone identifier</param>
		/// <returns>slot</returns>
        public async Task<List<Slot>> GetFreeSlotByZoneAndDate(DateTime start, DateTime end, int zoneId)
        {
            if (zoneId == 0)
                return null;
            var events = await GetFreeSlotByZone(zoneId);

            if (events == null)
                throw new ArgumentNullException(nameof(events));

            DateTime startDate = start.Date;
            DateTime endDate = end.Date;

            var query = from e in events
                        where (e.Start.Date) >= startDate.Date
                        && (e.End.Date) <= endDate.Date
                        && e.IsUnavailable && e.Capacity > 0
                        select e;

            return query.ToList();
        }

        /// <summary>
        /// Gets a zone by zone identifier
        /// </summary>
        /// <param name="zoneId">The zone identifier</param>
        /// <returns>Zone</returns>
        public async Task<IQueryable<Slot>> GetFreeSlotByZone(int zoneId)
        {
            if (zoneId == 0)
                return null;

            //cashing possible
            //var query = (from fs in _slotRepository.Table
            //             join z in _zoneRepository.Table on fs.ZoneId equals z.Id
            //             where z.Id == zoneId
            //             select fs);

            var query = await _slotRepository.GetAllAsync(async query =>
            {
                query = (from fs in _slotRepository.Table
                         join z in _zoneRepository.Table on fs.ZoneId equals z.Id
                         where z.Id == zoneId
                         select fs);

                return query;
            }, cache => cache.PrepareKeyForDefaultCache(SlotDefaults.SlotsByZoneCacheKey,
                zoneId));

            return query?.AsQueryable();
        }

        /// <summary>
		///  Get Same Slot with Same Start and End
		/// </summary>
		/// <param name="start">Start date</param>
		/// <param name="end">end date</param>
		/// <param name="zoneId">The zone identifier</param>
		/// <returns></returns>
		public async Task<IList<Slot>> GetSameSlotwithSameStartEnd(DateTime start, DateTime end, int zoneId)
        {
            if (zoneId == 0)
                return null;

            var query = _slotRepository.Table.Where(x => x.ZoneId == zoneId);
            var result = query.Where(x => (start >= x.Start && start < x.End) || (end > x.Start && end <= x.End) || (start < x.Start && end > x.End));
            return result.ToList();
        }

        /// <summary>
		///  Get UpcomingWeek Start End
		/// </summary>
		/// <param name="start">Start Date</param>
        /// <param name="end">End time</param>
        /// <param name="zoneId">The zone identifier</param>
        /// <returns>ZoneFreeSlotCalendarEvents</returns>
		public async Task<IList<Slot>> GetUpcomingWeekStartEnd(DateTime start, DateTime end, DateTime enddate, int zoneId, RecurringType recurringType = RecurringType.Daily, string weekDays = "")
        {
            if (zoneId == 0)
                return null;

            DateTime startDate = start.Date;
            DateTime endDate = enddate.AddHours(end.Hour).AddMinutes(end.Minute);
            var query = _slotRepository.Table.Where(x => x.ZoneId == zoneId);
            var result = query.Where(x => (start >= x.Start && start < x.End) || (endDate > x.Start && endDate <= x.End) || (start < x.Start && endDate > x.End)).ToList();
            var result1 = result.Where(x => (start.Hour >= x.Start.Hour && start.Hour < x.End.Hour) || (endDate.Hour > x.Start.Hour && endDate.Hour <= x.End.Hour) || (start.Hour < x.Start.Hour && endDate.Hour > x.End.Hour)).ToList();

            var resulDayofWeek = result1;
            if (recurringType == RecurringType.Weekly)
            {
                resulDayofWeek = result1.Where(s => weekDays.Contains(Convert.ToString(s.Start.DayOfWeek))).ToList();
            }
            else if (recurringType == RecurringType.CurrentDay)
            {
                resulDayofWeek = result1.Where(s => s.Start.DayOfWeek == start.DayOfWeek).ToList();
            }

            return resulDayofWeek;
        }

        /// <summary>
        ///  GetSlotById
        /// </summary>
        /// <param name="slotId">slotId</param>
        /// <returns>ZoneFreeSlotCalendarEvents</returns>
        public async Task<Slot> GetSlotById(int slotId)
        {
            if (slotId == 0)
                return null;
            //cashing possible
            return await _slotRepository.GetByIdAsync(slotId, cache => default);
        }


        /// <summary>
        /// Get Booked slot
        /// </summary>
        /// <param name="id"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public async Task<IList<Slot>> GetBookedSlot(int id, DateTime start, DateTime end)
        {
            DateTime startDate = start.Date;
            DateTime endDate = end.Date;
            //prepare input parameters
            //var pId = SqlParameterHelper.GetInt32Parameter("Id", id);
            //var pStartDate = SqlParameterHelper.GetDateTimeParameter("StartDate", startDate.Date);
            //var pEndDate = SqlParameterHelper.GetDateTimeParameter("EndDate", endDate.Date);

            //var bookedslots = _slotRepository.EntityFromSql("BookedSlot",
            //    pId,
            //    pStartDate,
            //    pEndDate).ToList();

            //select[Start],[End],Capacity,COUNT(c.SlotId) AS WeekDays, IsUnavailable, IsRecurring from Slot e
            //left
            // join CustomerOrderSlot c on e.Id = c.SlotId and(Convert(Date, [c].[StartTime]) >= @StartDate and Convert(Date, [c].[EndTime]) <= @EndDate)
            // where Convert(Date, [e].[Start]) >= @StartDate AND Convert(Date, [e].[End]) <= @EndDate AND e.ZoneId = @Id
            //   --and(Convert(Date, [c].[StartTime]) >= @StartDate AND Convert(Date, [c].[EndTime]) <= @EndDate)
            //Group by Start,[End],Capacity,IsUnavailable,IsRecurring

            var query = from e in _slotRepository.Table
                        join c in _customerOrderSlotRepository.Table on e.Id equals c.SlotId
                        where e.Start.Date >= startDate && e.End.Date <= endDate && e.ZoneId == id
                        select e;

            return query.ToList();
        }

        #endregion


        #region Pickup Slot Methods

        /// <summary>
        /// Inserts a zone free slot calendar event
        /// </summary>
        /// <param name="pickupSlot">pickupSlot</param>
        public async Task InsertPickupSlot(PickupSlot pickupSlot)
        {
            if (pickupSlot == null)
            {
                throw new ArgumentNullException(nameof(pickupSlot));
            }
            //Zone
            await _pickupSlotRepository.InsertAsync(pickupSlot);

            //Publish event
            await _eventPublisher.EntityInsertedAsync(pickupSlot);
        }

        /// <summary>
        /// Update a zone free slot calendar event
        /// </summary>
        /// <param name="slot">slot</param>
        public async Task UpdatePickupSlot(PickupSlot pickupSlot)
        {
            if (pickupSlot == null)
            {
                throw new ArgumentNullException(nameof(pickupSlot));
            }
            await _pickupSlotRepository.UpdateAsync(pickupSlot);

            await _eventPublisher.EntityUpdatedAsync(pickupSlot);
        }

        /// <summary>
        /// Remove a zone free slot calendar event
        /// </summary>
        /// <param name="slot">slot</param>
        public async Task RemovePickupSlot(int id)
        {
            if (id == 0)
                throw new ArgumentNullException(nameof(id));
            var obj = await _pickupSlotRepository.GetByIdAsync(id);

            await _pickupSlotRepository.DeleteAsync(obj);

            await _eventPublisher.EntityDeletedAsync(obj);
        }

        /// <summary>
		///  Get sequence id
		/// </summary>
		/// <param name="sequenceId">sequenceId</param>
		/// <returns>zoneFreeSlotCalendarEvent</returns>
        public async Task DeletePickupSlot(PickupSlot pickupSlot)
        {
            if (pickupSlot == null)
                throw new ArgumentNullException(nameof(pickupSlot));

            await _pickupSlotRepository.DeleteAsync(pickupSlot);

            await _eventPublisher.EntityDeletedAsync(pickupSlot);
        }

        /// <summary>
        /// Deletes a SlotCategory
        /// </summary>
        /// <param name="SlotCategory">SlotCategory</param>
        public async Task DeletePickupSlotIdCategory(int pickupSlotId = 0)
        {
            if (pickupSlotId == null)
                throw new ArgumentNullException(nameof(pickupSlotId));

            var categorySlot = await GetPickupSlotCategoriesBySlotIdAsync(pickupSlotId);
            foreach (var category in categorySlot)
            {
                await _pickupSlotCategoryRepository.DeleteAsync(category);

                //event notification
                await _eventPublisher.EntityDeletedAsync(category);
            }
        }

        /// <summary>
		///  Get sequence id (changes done by tmotions)
		/// </summary>
		/// <param name="sequenceId">sequenceId</param>
		/// <returns>ZoneFreeSlotCalendarEvents</returns>
		public async Task<IList<PickupSlot>> GetPickupSlotSequenceId(string sequenceId)
        {
            if (sequenceId == null)
                throw new ArgumentNullException(nameof(sequenceId));
            var query = _pickupSlotRepository.Table.Where(x => x.Sequence_Id == sequenceId);

            return query.ToList();
        }

        /// <summary>
		///  Get zone slot calendar event
		/// </summary>
		/// <param name="start">Start Date</param>
		/// <param name="end">End Date</param>
		/// <param name="zoneId">The zone identifier</param>
		/// <returns>slot</returns>
        public async Task<IEnumerable<PickupSlot>> GetPickupSlotByZoneAndDate(DateTime start, DateTime end, int zoneId)
        {
            if (zoneId == 0)
                return null;
            var events = await GetFreePickupSlotByZone(zoneId);

            if (events == null)
                throw new ArgumentNullException(nameof(events));

            DateTime startDate = start.Date;
            DateTime endDate = end.Date;

            var query = from e in events
                        where (e.Start.Date) >= startDate.Date
                        && (e.End.Date) <= endDate.Date
                        select e;

            return query.ToList();
        }

        /// <summary>
		///  Get zone free slot calendar event
		/// </summary>
		/// <param name="start">Start Date</param>
		/// <param name="end">End Date</param>
		/// <param name="zoneId">The zone identifier</param>
		/// <returns>slot</returns>
        public async Task<List<PickupSlot>> GetFreePickupSlotByZoneAndDate(DateTime start, DateTime end, int zoneId)
        {
            if (zoneId == 0)
                return null;
            var events = await GetFreePickupSlotByZone(zoneId);

            if (events == null)
                throw new ArgumentNullException(nameof(events));

            DateTime startDate = start.Date;
            DateTime endDate = end.Date;

            var query = from e in events
                        where (e.Start.Date) >= startDate.Date
                        && (e.End.Date) <= endDate.Date
                        && e.IsUnavailable && e.Capacity > 0
                        select e;

            return query.ToList();
        }

        /// <summary>
        /// Gets a zone by zone identifier
        /// </summary>
        /// <param name="zoneId">The zone identifier</param>
        /// <returns>Zone</returns>
        public async Task<IQueryable<PickupSlot>> GetFreePickupSlotByZone(int zoneId)
        {
            if (zoneId == 0)
                return null;

            //cashing possible
            //var query = (from fs in _pickupSlotRepository.Table
            //             join z in _zoneRepository.Table on fs.ZoneId equals z.Id
            //             where z.Id == zoneId
            //             select fs);

            //return query;

            var query = await _pickupSlotRepository.GetAllAsync(async query =>
            {
                query = (from fs in _pickupSlotRepository.Table
                         join z in _zoneRepository.Table on fs.ZoneId equals z.Id
                         where z.Id == zoneId
                         select fs);

                return query;
            }, cache => cache.PrepareKeyForDefaultCache(SlotDefaults.PickupSlotsByZoneCacheKey,
                zoneId));

            return query?.AsQueryable();
        }

        /// <summary>
		///  Get Same Slot with Same Start and End
		/// </summary>
		/// <param name="start">Start date</param>
		/// <param name="end">end date</param>
		/// <param name="zoneId">The zone identifier</param>
		/// <returns></returns>
		public async Task<IList<PickupSlot>> GetSamePickupSlotwithSameStartEnd(DateTime start, DateTime end, int zoneId)
        {
            if (zoneId == 0)
                return null;

            var query = _pickupSlotRepository.Table.Where(x => x.ZoneId == zoneId);
            var result = query.Where(x => (start >= x.Start && start < x.End) || (end > x.Start && end <= x.End) || (start < x.Start && end > x.End));
            return result.ToList();
        }

        /// <summary>
		///  Get UpcomingWeek Start End
		/// </summary>
		/// <param name="start">Start Date</param>
        /// <param name="end">End time</param>
        /// <param name="zoneId">The zone identifier</param>
        /// <returns>ZoneFreeSlotCalendarEvents</returns>
		public async Task<IList<PickupSlot>> GetPickupSlotUpcomingWeekStartEnd(DateTime start, DateTime end, DateTime enddate, int zoneId, RecurringType recurringType = RecurringType.Daily, string weekDays = "")
        {
            if (zoneId == 0)
                return null;

            DateTime startDate = start.Date;
            DateTime endDate = enddate.AddHours(end.Hour).AddMinutes(end.Minute);
            var query = _pickupSlotRepository.Table.Where(x => x.ZoneId == zoneId);
            var result = query.Where(x => (start >= x.Start && start < x.End) || (endDate > x.Start && endDate <= x.End) || (start < x.Start && endDate > x.End)).ToList();
            var result1 = result.Where(x => (start.Hour >= x.Start.Hour && start.Hour < x.End.Hour) || (endDate.Hour > x.Start.Hour && endDate.Hour <= x.End.Hour) || (start.Hour < x.Start.Hour && endDate.Hour > x.End.Hour)).ToList();

            var resulDayofWeek = result1;
            if (recurringType == RecurringType.Weekly)
            {
                resulDayofWeek = result1.Where(s => weekDays.Contains(Convert.ToString(s.Start.DayOfWeek))).ToList();
            }
            else if (recurringType == RecurringType.CurrentDay)
            {
                resulDayofWeek = result1.Where(s => s.Start.DayOfWeek == start.DayOfWeek).ToList();
            }

            return resulDayofWeek;
        }

        /// <summary>
        ///  GetSlotById
        /// </summary>
        /// <param name="slotId">slotId</param>
        /// <returns>ZoneFreeSlotCalendarEvents</returns>
        public async Task<PickupSlot> GetPickupSlotById(int slotId)
        {
            if (slotId == 0)
                return null;
            return await _pickupSlotRepository.GetByIdAsync(slotId, cache => default);
        }


        /// <summary>
        /// Get Booked slot
        /// </summary>
        /// <param name="id"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public async Task<IList<PickupSlot>> GetBookedPickupSlot(int id, DateTime start, DateTime end)
        {
            DateTime startDate = start.Date;
            DateTime endDate = end.Date;

            var query = from e in _pickupSlotRepository.Table
                        join c in _customerOrderSlotRepository.Table on e.Id equals c.SlotId
                        where e.Start.Date >= startDate && e.End.Date <= endDate && e.ZoneId == id
                        select e;

            return query.ToList();
        }

        #endregion

        #region Customer Order slot
        /// <summary>
        /// Inserts a customer order slot
        /// </summary>
        /// <param name="customerOrderSlot">customerOrderSlot</param>
        public async Task InsertCustomerOrderSlot(CustomerOrderSlot customerOrderSlot)
        {
            if (customerOrderSlot == null)
            {
                throw new ArgumentNullException(nameof(customerOrderSlot));
            }

            //customerOrderSlot
            await _customerOrderSlotRepository.InsertAsync(customerOrderSlot);

            //Publish event
            await _eventPublisher.EntityInsertedAsync(customerOrderSlot);
        }

        /// <summary>
        /// Gets a customer order slot
        /// <summary>
        /// <param name="slotId">slotId</param>
        /// <param name="start">start</param>
        /// <param name="end">end</param>
        public async Task<int> GetCustomerSlotsBySlotIdAndDate(int slotId, DateTime start, DateTime end)
        {
            if (slotId < 0)
            {
                throw new ArgumentNullException(nameof(slotId));
            }
            //cashing possible
            //return _orderItemRepository.Table.Count(x => x.SlotId == slotId);

            var cacheKey = _staticCacheManager
                .PrepareKeyForDefaultCache(SlotDefaults.SlotsNumberInOrderItemCacheKey, slotId);

            return await _staticCacheManager.GetAsync(cacheKey, () => _orderItemRepository.Table.Count(x => x.SlotId == slotId));
        }

        #endregion

        #region Zone Methods
        /// <summary>
        /// Inserts a zone
        /// </summary>
        /// <param name="zone">zone</param>
        public async Task InsertZone(Zone zone)
        {
            if (zone == null)
            {
                throw new ArgumentNullException(nameof(zone));
            }

            //Zone
            await _zoneRepository.InsertAsync(zone);

            //Publish event
            await _eventPublisher.EntityInsertedAsync(zone);
        }

        /// <summary>
        /// Updates a zone
        /// </summary>
        /// <param name="zone">Zone</param>
        public async Task UpdateZone(Zone zone)
        {
            if (zone == null)
            {
                throw new ArgumentNullException(nameof(zone));
            }

            //Zone
            await _zoneRepository.UpdateAsync(zone);

            //Publish event
            await _eventPublisher.EntityUpdatedAsync(zone);
        }

        /// <summary>
        /// Deletes a zone
        /// </summary>
        /// <param name="zone">Zone</param>
        public async Task DeleteZone(Zone zone)
        {
            if (zone == null)
                throw new ArgumentNullException(nameof(zone));

            await _zoneRepository.DeleteAsync(zone);

            //event notification
            await _eventPublisher.EntityDeletedAsync(zone);
        }

        /// <summary>
        /// Gets a zone by zone identifier
        /// </summary>
        /// <param name="zoneId">The zone identifier</param>
        /// <returns>Zone</returns>
        public async Task<Zone> GetZoneById(int zoneId)
        {
            if (zoneId == 0)
                return null;

            var query = await _zoneRepository.GetByIdAsync(zoneId, cache => default);

            return query;
        }

        /// <summary>
        /// Gets the zone list
        /// </summary>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>Zones</returns>
        public virtual async Task<IPagedList<Zone>> GetAllZones(int pageIndex = 0, int pageSize = int.MaxValue, string zoneName = "", bool? isActive = null, int vendorId = 0, bool isPickup = false, int? createdBy = null)
        {
            //do not filter by customer role
            var query = _zoneRepository.Table;

            if (!string.IsNullOrEmpty(zoneName))
                query = query.Where(nls => nls.Name.Contains(zoneName));

            if (isActive != null)
                query = query.Where(n => n.IsActive == isActive);

            if (vendorId > 0)
                query = query.Where(n => n.VendorId == vendorId);

            if (isPickup != null)
                query = query.Where(n => n.IsPickUp == isPickup);

            if (createdBy != null)
                query = query.Where(n => n.CreatedBy == createdBy);

            return await query.ToPagedListAsync(pageIndex, pageSize);
        }

        /// <summary>
        /// Get a zone by zone name
        /// </summary>
        /// <param name="zoneName"></param>
        /// <returns></returns>
        public async Task<Zone> GetZoneByZoneName(string zoneName)
        {
            if (string.IsNullOrEmpty(zoneName))
                return null;

            return _zoneRepository.Table.Where(x => x.Name.ToLower().Trim() == zoneName.ToLower().Trim() && (x.IsPickUp != null)).FirstOrDefault();
        }


        /// <summary>
        /// Create and Gets the zone
        /// </summary>
        /// <returns>Zones</returns>
        public async Task<Zone> CreateAndGetGlobalZone()
        {
            var zone = _zoneRepository.Table.FirstOrDefault();

            if (zone != null)
            {
                return zone;
            }

            var newZone = new Zone()
            {
                IsActive = true
            };

            await _zoneRepository.InsertAsync(newZone);

            return newZone;
        }

        /// <summary>
        /// Gets a zoneid by postcode
        /// </summary>
        /// <param name="zone">The zone identifier</param>
        /// <returns>ZoneId</returns>
        public async Task<int> ZoneIdByPostCode(string zone)
        {
            if (string.IsNullOrEmpty(zone))
                return 0;
            return _zoneRepository.Table.Where(z => z.IsActive).Select(z => z.Id).FirstOrDefault();

        }


        public virtual async Task<Zone> GetVendorZones(bool? isActive = null, int vendorId = 0, bool isPickup = false, int createdBy = 0)
        {
            ////do not filter by customer role
            //var query = _zoneRepository.Table;

            //if (isActive != null)
            //    query = query.Where(n => n.IsActive == isActive && n.VendorId == vendorId && n.IsPickUp == isPickup && n.CreatedBy == createdBy);

            //return query.FirstOrDefault();

            var cacheKey = _staticCacheManager.PrepareKeyForDefaultCache(SlotDefaults.ZonesByVendorCacheKey,
                vendorId, isActive, isPickup, createdBy);

            var result = await _staticCacheManager.GetAsync(cacheKey, async () =>
            {
                //do not filter by customer role
                var query = _zoneRepository.Table;

                if (isActive != null)
                    query = query.Where(n => n.IsActive == isActive && n.VendorId == vendorId && n.IsPickUp == isPickup && n.CreatedBy == createdBy);

                return query.FirstOrDefault();
            });

            return result;
        }

        public virtual async Task<Zone> GetAdminCreateVendorZones(bool? isActive = null, int vendorId = 0, bool isPickup = false, int createdBy = 0)
        {

            //do not filter by customer role
            var query = _zoneRepository.Table;

            if (isActive != null)
                query = query.Where(n => n.IsActive == isActive && n.VendorId == vendorId && n.CreatedBy == 1 && n.IsPickUp == isPickup && n.CreatedBy == createdBy);

            return query.FirstOrDefault();
        }


        public virtual async Task<IList<CustomerProductSlot>> GetCustomerProductSlot(int productId = 0, int customerId = 0, DateTime startDate = default, bool isPickup = false)
        {
            ////do not filter by CustomerProductSlot
            //var query = _customerProductSlotRepository.Table;

            ////partial cashing possible
            //if (productId > 0 && customerId > 0 && startDate != null)
            //    query = query.Where(n => n.ProductId == productId && n.CustomerId == customerId && n.StartTime > startDate && n.IsPickup == isPickup);

            //return query.ToList();

            var cacheKey = _staticCacheManager.PrepareKeyForDefaultCache(SlotDefaults.CustomerProductSlotbyCustomerCacheKey,
               customerId, productId, isPickup);

            var result = await _staticCacheManager.GetAsync(cacheKey, async () =>
            {
                var query = _customerProductSlotRepository.Table;
                if (productId > 0 && customerId > 0)
                    query = query.Where(n => n.ProductId == productId && n.CustomerId == customerId && n.IsPickup == isPickup);

                return query.ToList();
            });

            if (startDate != null)
                result = result.Where(n => n.StartTime > startDate).ToList();

            return result;
        }

        public virtual async Task<IList<CustomerProductSlot>> GetCustomerProductSlotId(int slotId = 0, int productId = 0, int customerId = 0, DateTime startDate = default, bool isPickup = false)
        {
            //do not filter by CustomerProductSlot
            var query = _customerProductSlotRepository.Table;

            if (productId > 0 && customerId > 0 && startDate != null)
                query = query.Where(n => n.ProductId == productId && n.SlotId == slotId && n.CustomerId == customerId && n.IsPickup == isPickup);

            return query.ToList();
        }

        public virtual async Task<CustomerProductSlot> GetCustomerProductSlotDeliveryPickupId(int productId = 0, int customerId = 0, DateTime startDate = default, bool ispickup = false)
        {
            //do not filter by CustomerProductSlot
            var query = _customerProductSlotRepository.Table;

            if (productId > 0 && customerId > 0 && startDate != null)
                query = query.Where(n => n.ProductId == productId && n.CustomerId == customerId && n.StartTime > startDate && n.IsPickup == ispickup);

            if (query.Any(x => x.IsSelected))
            {
                return query.Where(x => x.IsSelected).OrderByDescending(x => x.LastUpdated).FirstOrDefault();
            }

            return query.OrderByDescending(x => x.LastUpdated).FirstOrDefault();
        }

        public virtual async Task<IList<CustomerProductSlot>> GetCustomerCategoryProductSlot(int slotId = 0)
        {
            //do not filter by CustomerProductSlot
            var query = _customerProductSlotRepository.Table;

            if (slotId > 0)
                query = query.Where(n => n.SlotId == slotId);

            return query.ToList();
        }


        public async Task InsertCustomerProductSlot(CustomerProductSlot customerProductSlot)
        {
            if (customerProductSlot == null)
            {
                throw new ArgumentNullException(nameof(customerProductSlot));
            }

            //Zone
            await _customerProductSlotRepository.InsertAsync(customerProductSlot);

            //Publish event
            await _eventPublisher.EntityInsertedAsync(customerProductSlot);
        }

        /// <summary>
        /// Update a zone free slot calendar event
        /// </summary>
        /// <param name="slot">slot</param>
        public async Task UpdateCustomerProductSlot(CustomerProductSlot customerProductSlot)
        {
            if (customerProductSlot == null)
            {
                throw new ArgumentNullException(nameof(customerProductSlot));
            }
            await _customerProductSlotRepository.UpdateAsync(customerProductSlot);

            await _eventPublisher.EntityUpdatedAsync(customerProductSlot);
        }

        public virtual async Task<IList<CustomerProductSlot>> GetCustomerBookSlotId(int slotId = 0)
        {
            ////do not filter by CustomerProductSlot
            //var query = _customerProductSlotRepository.Table;

            ////cashing possible
            //if (slotId > 0)
            //    query = query.Where(n => n.SlotId == slotId);

            //return query.ToList();

            var cacheKey = _staticCacheManager.PrepareKeyForDefaultCache(SlotDefaults.CustomerProductSlotbySlotCacheKey,
               slotId);

            var result = await _staticCacheManager.GetAsync(cacheKey, async () =>
            {
                //do not filter by CustomerProductSlot
                var query = _customerProductSlotRepository.Table;

                if (slotId > 0)
                    query = query.Where(n => n.SlotId == slotId);

                return query.ToList();
            });

            return result;
        }


        public async Task DeleteCustomerProductSlot(int productId = 0, int customerId = 0)
        {
            //do not filter by CustomerProductSlot
            var query = _customerProductSlotRepository.Table;
            query = query.Where(n => n.ProductId == productId && n.CustomerId == customerId && n.StartTime < DateTime.Now);
            foreach (var item in query)
            {
                await _customerProductSlotRepository.DeleteAsync(item);
            }
        }

        public async Task DeleteCustomerProductSlotById(int Id = 0)
        {
            //do not filter by CustomerProductSlot
            var query = _customerProductSlotRepository.Table;
            query = query.Where(n => n.Id == Id);
            foreach (var item in query)
            {
                await _customerProductSlotRepository.DeleteAsync(item);
            }
        }

        public async Task DeleteCustomerProductBySlotId(int SlotId = 0)
        {
            //do not filter by CustomerProductSlot
            var query = _customerProductSlotRepository.Table;
            query = query.Where(n => n.SlotId == SlotId);
            foreach (var item in query)
            {
                await _customerProductSlotRepository.DeleteAsync(item);
            }
        }

        public virtual async Task<bool> GetCustomerProductSlotByDate(int productId = 0, int customerId = 0, DateTime startDate = default, string slot = null, bool isPickup = false)
        {
            //do not filter by CustomerProductSlot
            var isDateExist = false;

            var query = _customerProductSlotRepository.Table;

            if (productId > 0 && customerId > 0 && startDate != null)
                query = query.Where(n => n.ProductId == productId && n.CustomerId == customerId && n.IsPickup == isPickup && n.StartTime.Date == startDate.Date && n.EndTime == slot);

            if (query.Count() > 0)
            {
                isDateExist = true;
            }
            return isDateExist;
        }
        #endregion

        #region SlotCategory
        /// <summary>
        /// Inserts a SlotCategory
        /// </summary>
        /// <param name="SlotCategory">SlotCategory</param>
        public async Task InsertSlotCategory(SlotCategory slotCategory)
        {
            if (slotCategory == null)
            {
                throw new ArgumentNullException(nameof(slotCategory));
            }

            //Zone
            await _slotCategoryRepository.InsertAsync(slotCategory);

            //Publish event
            await _eventPublisher.EntityInsertedAsync(slotCategory);
        }

        /// <summary>
        /// Updates a SlotCategory
        /// </summary>
        /// <param name="SlotCategory">SlotCategory</param>
        public async Task UpdateSlotCategory(SlotCategory slotCategory)
        {
            if (slotCategory == null)
            {
                throw new ArgumentNullException(nameof(slotCategory));
            }

            //Zone
            await _slotCategoryRepository.UpdateAsync(slotCategory);

            //Publish event
            await _eventPublisher.EntityUpdatedAsync(slotCategory);
        }

        /// <summary>
        /// Deletes a SlotCategory
        /// </summary>
        /// <param name="SlotCategory">SlotCategory</param>
        public async Task DeleteSlotCategory(SlotCategory slotCategory)
        {
            if (slotCategory == null)
                throw new ArgumentNullException(nameof(slotCategory));

            await _slotCategoryRepository.DeleteAsync(slotCategory);

            //event notification
            await _eventPublisher.EntityDeletedAsync(slotCategory);
        }

        /// <summary>
        /// Deletes a SlotCategory
        /// </summary>
        /// <param name="SlotCategory">SlotCategory</param>
        public async Task DeleteSlotIdCategory(int slotId = 0)
        {
            if (slotId == null)
                throw new ArgumentNullException(nameof(slotId));

            var categorySlot = await GetSlotCategoriesBySlotIdAsync(slotId);
            foreach (var category in categorySlot)
            {
                await _slotCategoryRepository.DeleteAsync(category);

                //event notification
                await _eventPublisher.EntityDeletedAsync(category);
            }
        }

        /// <summary>
        /// Gets a product category mapping collection
        /// </summary>
        /// <param name="productId">Product identifier</param>
        /// <param name="storeId">Store identifier (used in multi-store environment). "showHidden" parameter should also be "true"</param>
        /// <param name="showHidden"> A value indicating whether to show hidden records</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the product category mapping collection
        /// </returns>
        public virtual async Task<IList<SlotCategory>> GetSlotCategoriesBySlotIdAsync(int slotId)
        {
            if (slotId == 0)
                return new List<SlotCategory>();

            var query = _slotCategoryRepository.Table;
            var categoriesQuery = _categoryRepository.Table.Where(c => c.Published);
            query = query.Where(pc => categoriesQuery.Any(c => c.Id == pc.CategoryId));
            return query.Where(pc => pc.SlotId == slotId).ToList();
        }

        /// <summary>
        /// Returns a SlotCategory that has the specified values
        /// </summary>
        /// <param name="source">Source</param>
        /// <param name="slotId">Slot identifier</param>
        /// <param name="categoryId">Category identifier</param>
        /// <returns>A ProductCategory that has the specified values; otherwise null</returns>
        public virtual SlotCategory FindSlotCategory(IList<SlotCategory> source, int slotId, int categoryId)
        {
            foreach (var productCategory in source)
                if (productCategory.SlotId == slotId && productCategory.CategoryId == categoryId)
                    return productCategory;

            return null;
        }

        public virtual async Task<IPagedList<SlotCategory>> GetSlotCategoriesByCategoryIdAsync(int slotId = 0, int categoryId = 0,
           int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false)
        {
            if (categoryId == 0)
                return new PagedList<SlotCategory>(new List<SlotCategory>(), pageIndex, pageSize);

            var query = from pc in _slotCategoryRepository.Table
                        join p in _slotRepository.Table on pc.SlotId equals p.Id
                        where pc.CategoryId == categoryId && pc.SlotId == slotId
                        orderby pc.Id
                        select pc;

            if (!showHidden)
            {
                var categoriesQuery = _slotRepository.Table.Where(c => c.IsUnavailable);

                query = query.Where(pc => categoriesQuery.Any(c => c.Id == pc.CategoryId));
            }

            return await query.ToPagedListAsync(pageIndex, pageSize);
        }

        /// <summary>
        /// Returns a SlotCategory that has the specified values
        /// </summary>
        /// <param name="source">Source</param>
        /// <param name="slotId">Slot identifier</param>
        /// <param name="categoryId">Category identifier</param>
        /// <returns>A ProductCategory that has the specified values; otherwise null</returns>
        public virtual async Task<bool> FindSlotCategoryIdExist(int slotId, List<int> categoryId)
        {
            //bool IsExist = false;
            ////cashing possible
            //var query = _slotCategoryRepository.Table;
            //query = query.Where(x => categoryId.Contains(x.CategoryId) && x.SlotId == slotId);
            //if (query.ToList().Count > 0)
            //{
            //    IsExist = true;
            //}
            //return IsExist;

            var cacheKey = _staticCacheManager.PrepareKeyForDefaultCache(SlotDefaults.SlotCategorybySlotCacheKey,
               slotId);

            var result = await _staticCacheManager.GetAsync(cacheKey, async () =>
            {
                var query = _slotCategoryRepository.Table;
                query = query.Where(x => x.SlotId == slotId);

                return query.ToList();
            });

            //filter by cateory ids
            result = result.Where(x => categoryId.Contains(x.CategoryId)).ToList();
            bool isExist = false;
            if (result.Count > 0)
            {
                isExist = true;
            }
            return isExist;
        }
        #endregion

        #region checkaviability
        public async Task<bool> CheckSlotAvailability(int slotId, DateTime start, DateTime end, int capacity, bool fromOrder)
        {
            bool result = true;
            try
            {
                int bookedCustomerSlot = await GetCustomerSlotsBySlotIdAndDate(slotId, start, end);
                if (bookedCustomerSlot < capacity)
                {
                    //Check temporary booked slots
                    var tempBookedSlots = await GetCustomerBookSlotId(slotId);
                    if (tempBookedSlots.Count > 0)
                    {
                        int count = 0;
                        foreach (var item in tempBookedSlots)
                        {
                            var time = item.EndTime;
                            DateTime st = Convert.ToDateTime(time);
                            DateTime tempStartTime = new DateTime(start.Year, start.Month, start.Day, st.Hour, st.Minute, st.Second);

                            DateTime et = Convert.ToDateTime(time);
                            DateTime tempEndTime = new DateTime(end.Year, end.Month, end.Day, et.Hour, et.Minute, et.Second);
                            if (slotId == item.SlotId && start == tempStartTime && end == tempEndTime)
                            {
                                count++;
                            }

                        }
                        if (bookedCustomerSlot + count >= capacity)
                            result = false;
                        else
                            result = true;
                    }
                }
                else
                {
                    result = false;
                }
                return result;
            }
            catch (Exception exc)
            {
                return result;
            }

        }
        #endregion


        #region Pickup Slot Category

        /// <summary>
        /// Inserts a SlotCategory
        /// </summary>
        /// <param name="SlotCategory">SlotCategory</param>
        public async Task InsertPickupSlotCategory(PickupSlotCategory pickupSlotCategory)
        {
            if (pickupSlotCategory == null)
            {
                throw new ArgumentNullException(nameof(pickupSlotCategory));
            }

            //Zone
            await _pickupSlotCategoryRepository.InsertAsync(pickupSlotCategory);

            //Publish event
            await _eventPublisher.EntityInsertedAsync(pickupSlotCategory);
        }

        /// <summary>
        /// Updates a SlotCategory
        /// </summary>
        /// <param name="SlotCategory">SlotCategory</param>
        public async Task UpdatePickupSlotCategory(PickupSlotCategory pickupSlotCategory)
        {
            if (pickupSlotCategory == null)
            {
                throw new ArgumentNullException(nameof(pickupSlotCategory));
            }

            //Zone
            await _pickupSlotCategoryRepository.UpdateAsync(pickupSlotCategory);

            //Publish event
            await _eventPublisher.EntityUpdatedAsync(pickupSlotCategory);
        }

        /// <summary>
        /// Deletes a SlotCategory
        /// </summary>
        /// <param name="SlotCategory">SlotCategory</param>
        public async Task DeletePickupSlotCategory(PickupSlotCategory pickupSlotCategory)
        {
            if (pickupSlotCategory == null)
                throw new ArgumentNullException(nameof(pickupSlotCategory));

            await _pickupSlotCategoryRepository.DeleteAsync(pickupSlotCategory);

            //event notification
            await _eventPublisher.EntityDeletedAsync(pickupSlotCategory);
        }

        /// <summary>
        /// Gets a product category mapping collection
        /// </summary>
        /// <param name="productId">Product identifier</param>
        /// <param name="storeId">Store identifier (used in multi-store environment). "showHidden" parameter should also be "true"</param>
        /// <param name="showHidden"> A value indicating whether to show hidden records</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the product category mapping collection
        /// </returns>
        public virtual async Task<IList<PickupSlotCategory>> GetPickupSlotCategoriesBySlotIdAsync(int pickupSlotId)
        {
            if (pickupSlotId == 0)
                return new List<PickupSlotCategory>();

            var query = _pickupSlotCategoryRepository.Table;
            var categoriesQuery = _categoryRepository.Table.Where(c => c.Published);
            query = query.Where(pc => categoriesQuery.Any(c => c.Id == pc.CategoryId));
            return query.Where(pc => pc.PickupSlotId == pickupSlotId).ToList();
        }

        /// <summary>
        /// Returns a SlotCategory that has the specified values
        /// </summary>
        /// <param name="source">Source</param>
        /// <param name="slotId">Slot identifier</param>
        /// <param name="categoryId">Category identifier</param>
        /// <returns>A ProductCategory that has the specified values; otherwise null</returns>
        public virtual PickupSlotCategory FindPickupSlotCategory(IList<PickupSlotCategory> source, int pickupSlotId, int categoryId)
        {
            foreach (var productCategory in source)
                if (productCategory.PickupSlotId == pickupSlotId && productCategory.CategoryId == categoryId)
                    return productCategory;

            return null;
        }

        public virtual async Task<IPagedList<PickupSlotCategory>> GetPickupSlotCategoriesByCategoryIdAsync(int pickupSlotId = 0, int categoryId = 0,
           int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false)
        {
            if (categoryId == 0)
                return new PagedList<PickupSlotCategory>(new List<PickupSlotCategory>(), pageIndex, pageSize);

            var query = from pc in _pickupSlotCategoryRepository.Table
                        join p in _pickupSlotRepository.Table on pc.PickupSlotId equals p.Id
                        where pc.CategoryId == categoryId && pc.PickupSlotId == pickupSlotId
                        orderby pc.Id
                        select pc;

            if (!showHidden)
            {
                var categoriesQuery = _pickupSlotRepository.Table.Where(c => c.IsUnavailable);

                query = query.Where(pc => categoriesQuery.Any(c => c.Id == pc.CategoryId));
            }

            return await query.ToPagedListAsync(pageIndex, pageSize);
        }

        /// <summary>
        /// Returns a SlotCategory that has the specified values
        /// </summary>
        /// <param name="source">Source</param>
        /// <param name="pickupSlotId">Slot identifier</param>
        /// <param name="categoryId">Category identifier</param>
        /// <returns>A ProductCategory that has the specified values; otherwise null</returns>
        public virtual async Task<bool> FindPickupSlotCategoryIdExist(int pickupSlotId, List<int> categoryId)
        {
            bool IsExist = false;
            var query = _pickupSlotCategoryRepository.Table;
            query = query.Where(x => categoryId.Contains(x.CategoryId) && x.PickupSlotId == pickupSlotId);
            if (query.ToList().Count > 0)
            {
                IsExist = true;
            }
            return IsExist;
        }

        #endregion

        #region General

        /// <summary>
        /// Get value indicates wether vendor has created any slot or not.
        /// </summary>
        /// <param name="vendor"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public virtual async Task<bool> HasVendorCreatedAnySlot(Vendor vendor)
        {
            if (vendor == null)
                throw new ArgumentNullException(nameof(vendor));

            var cacheKey = _staticCacheManager.PrepareKeyForDefaultCache(SlotDefaults.HasVendorCreatedSlotCacheKey, vendor.Id);
            var result = await _staticCacheManager.GetAsync(cacheKey, async () =>
            {
                //check if delivery and pick both disbale - then do not check for slot - 06-07-23
                if (!vendor.DeliveryAvailable && !vendor.PickAvailable)
                    return false;

                //check if no slot created for delivery/pickup - 23-12-22
                bool isAnySlotCreated = false;

                //if DeliveryAvailable then only check for delivery slot
                if (vendor.DeliveryAvailable)
                {
                    if (vendor.ManageDelivery)
                    {
                        //get vendor slots
                        var zone = await GetVendorZones(true, vendor.Id, isPickup: false, createdBy: 0);
                        isAnySlotCreated = zone != null;
                    }
                    else
                    {
                        //get slot created by vendor
                        var zone = await GetAdminCreateVendorZones(true, vendor.Id, false, createdBy: 1);
                        if (zone == null)
                        {
                            zone = await GetVendorZones(true, 0, false);
                        }
                        isAnySlotCreated = zone != null;
                    }
                }

                //no delivery slot created? check for pickup
                if (!isAnySlotCreated)
                {
                    if (vendor.PickAvailable)
                    {
                        var zone = await GetVendorZones(true, vendor.Id, isPickup: true);
                        isAnySlotCreated = zone != null;
                    }
                }

                return isAnySlotCreated;
            });

            return result;
        }

        #endregion
    }
}