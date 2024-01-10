using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Slots;
using Nop.Core.Domain.Vendors;

namespace Nop.Services.Slots
{
    /// <summary>
    /// Slot interface
    /// </summary>
    public partial interface ISlotService
    {
        /// <summary>
        /// Inserts a zone free slot calendar event
        /// </summary>
        /// <param name="zoneFreeSlotCalendarEvent">zoneFreeSlotCalendarEvent</param>
        Task InsertZoneFreeSlotCalendarEvent(Slot slot);


        Task Update(Slot slot);
        /// <summary>
        /// Remove zone free slot calendar event
        /// </summary>
        /// <param name="id">The zone free slot available Calendar event identifier</param>
        Task Remove(int id);

        Task Delete(Slot slot);

        /// <summary>
        /// Get SequenceId
        /// </summary>
        /// <param name="sequenceId">sequence Id</param>
        /// <returns>ZoneFreeSlotCalendarEvents</returns>
        Task<IList<Slot>> GetSequenceId(string sequenceId);


        Task<IEnumerable<Slot>> GetSlotByZoneAndDate(DateTime start, DateTime end, int zoneId);

        Task<List<Slot>> GetFreeSlotByZoneAndDate(DateTime start, DateTime end, int zoneId);


        Task<IQueryable<Slot>> GetFreeSlotByZone(int zoneId);


        Task<IList<Slot>> GetSameSlotwithSameStartEnd(DateTime start, DateTime end, int zoneId);


        Task<IList<Slot>> GetUpcomingWeekStartEnd(DateTime start, DateTime end, DateTime enddate, int zoneId, RecurringType recurringType = RecurringType.Daily, string weekDays = "");


        Task<Slot> GetSlotById(int slotId);


        Task<IList<Slot>> GetBookedSlot(int id, DateTime start, DateTime end);


        Task InsertCustomerOrderSlot(CustomerOrderSlot customerOrderSlot);


        Task<int> GetCustomerSlotsBySlotIdAndDate(int slotId, DateTime start, DateTime end);


        Task InsertZone(Zone zone);

        Task UpdateZone(Zone zone);

        Task DeleteZone(Zone zone);

        Task<Zone> GetZoneById(int zoneId);

        Task<IPagedList<Zone>> GetAllZones(int pageIndex = 0, int pageSize = int.MaxValue, string zoneName = "", bool? isActive = null, int vendorId = 0, bool isPickup = false,int? createdBy = null);

        Task<Zone> GetZoneByZoneName(string zoneName);

        Task<Zone> CreateAndGetGlobalZone();

        Task<int> ZoneIdByPostCode(string zone);


        Task<Zone> GetVendorZones(bool? isActive = null, int vendorId = 0, bool isPickup = false, int createdBy = 0);

        Task<Zone> GetAdminCreateVendorZones(bool? isActive = null, int vendorId = 0, bool isPickup = false, int createdBy = 0);

        #region PickUp Slot 

        Task InsertPickupSlot(PickupSlot pickupSlot);

        Task UpdatePickupSlot(PickupSlot pickupSlot);

        Task RemovePickupSlot(int id);

        Task DeletePickupSlot(PickupSlot pickupSlot);

        Task DeletePickupSlotIdCategory(int pickupSlotId = 0);

        Task<IList<PickupSlot>> GetPickupSlotSequenceId(string sequenceId);

        Task<IEnumerable<PickupSlot>> GetPickupSlotByZoneAndDate(DateTime start, DateTime end, int zoneId);

        Task<List<PickupSlot>> GetFreePickupSlotByZoneAndDate(DateTime start, DateTime end, int zoneId);

        Task<IQueryable<PickupSlot>> GetFreePickupSlotByZone(int zoneId);

        Task<IList<PickupSlot>> GetSamePickupSlotwithSameStartEnd(DateTime start, DateTime end, int zoneId);

        Task<IList<PickupSlot>> GetPickupSlotUpcomingWeekStartEnd(DateTime start, DateTime end, DateTime enddate, int zoneId, RecurringType recurringType = RecurringType.Daily, string weekDays = "");

        Task<PickupSlot> GetPickupSlotById(int slotId);

        Task<IList<PickupSlot>> GetBookedPickupSlot(int id, DateTime start, DateTime end);
        #endregion

        #region Customer slots


        Task<IList<CustomerProductSlot>> GetCustomerProductSlot(int productId = 0, int customerId = 0, DateTime startDate = default, bool isPickup = false);

        Task<IList<CustomerProductSlot>> GetCustomerProductSlotId(int slotId = 0, int productId = 0, int customerId = 0, DateTime startDate = default, bool isPickup = false);

        Task<CustomerProductSlot> GetCustomerProductSlotDeliveryPickupId(int productId = 0, int customerId = 0, DateTime startDate = default, bool ispickup = false);

        Task<IList<CustomerProductSlot>> GetCustomerCategoryProductSlot(int slotId = 0);

        Task InsertCustomerProductSlot(CustomerProductSlot customerProductSlot);

        Task UpdateCustomerProductSlot(CustomerProductSlot customerProductSlot);

        Task<IList<CustomerProductSlot>> GetCustomerBookSlotId(int slotId = 0);

        Task DeleteCustomerProductSlot(int productId = 0, int customerId = 0);
        Task DeleteCustomerProductSlotById(int Id = 0);

        Task InsertSlotCategory(SlotCategory slotCategory);

        Task UpdateSlotCategory(SlotCategory slotCategory);

        Task DeleteSlotCategory(SlotCategory slotCategory);

        Task DeleteSlotIdCategory(int slotId = 0);

        Task DeleteCustomerProductBySlotId(int SlotId = 0);

        Task<IList<SlotCategory>> GetSlotCategoriesBySlotIdAsync(int slotId);

        SlotCategory FindSlotCategory(IList<SlotCategory> source, int slotId, int categoryId);

        Task<IPagedList<SlotCategory>> GetSlotCategoriesByCategoryIdAsync(int slotId = 0, int categoryId = 0,
            int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false);

        Task<bool> FindSlotCategoryIdExist(int slotId, List<int> categoryId);
        #endregion

        Task<bool> CheckSlotAvailability(int slotId, DateTime start, DateTime end, int capacity, bool fromOrder);

        Task<bool> GetCustomerProductSlotByDate(int productId = 0, int customerId = 0, DateTime startDate = default, string slot = null, bool isPickup = false);

        #region Pickup Slot Category
        Task InsertPickupSlotCategory(PickupSlotCategory pickupSlotCategory);

        Task UpdatePickupSlotCategory(PickupSlotCategory pickupSlotCategory);

        Task DeletePickupSlotCategory(PickupSlotCategory pickupSlotCategory);

        Task<IList<PickupSlotCategory>> GetPickupSlotCategoriesBySlotIdAsync(int pickupSlotId);


        PickupSlotCategory FindPickupSlotCategory(IList<PickupSlotCategory> source, int pickupSlotId, int categoryId);

        Task<IPagedList<PickupSlotCategory>> GetPickupSlotCategoriesByCategoryIdAsync(int pickupSlotId = 0, int categoryId = 0,
           int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false);


        Task<bool> FindPickupSlotCategoryIdExist(int pickupSlotId, List<int> categoryId);
        #endregion

        #region General

        /// <summary>
        /// Get value indicates wether vendor has created any slot or not.
        /// </summary>
        /// <param name="vendor"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        Task<bool> HasVendorCreatedAnySlot(Vendor vendor);

        #endregion
    }
}
