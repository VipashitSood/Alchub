using System.Threading.Tasks;
using Nop.Core.Domain.Slots;
using Nop.Web.Areas.Admin.Models.Slots;

namespace Nop.Web.Areas.Admin.Factories
{
    /// <summary>
    /// Represents the slot model factory
    /// </summary>
    public partial interface ISlotModelFactory
    {
        /// <summary>
        /// Prepare zone search model
        /// </summary>
        /// <param name="searchModel">Zone search model</param>
        /// <returns>Zone search model</returns>
        Task<SlotSearchModel> PrepareZoneSearchModel(SlotSearchModel searchModel);

        /// <summary>
        /// Prepare paged zone list model
        /// </summary>
        /// <param name="searchModel">Zone search model</param>
        /// <returns>Zone list model</returns>
        Task<ZoneListModel> PrepareZoneListModel(SlotSearchModel searchModel);

        /// <summary>
        /// Prepare zone model
        /// </summary>
        /// <param name="model">Zone model</param>
        /// <param name="zone">Zone</param>
        /// <param name="excludeProperties">Whether to exclude populating of some properties of model</param>
        /// <returns>Zone model</returns>
        Task<ZoneModel> PrepareZoneModel(ZoneModel model, Zone zone, bool excludeProperties = false);

        #region Pickup Slot

        Task<PickupSlotSearchModel> PreparePickupZoneSearchModel(PickupSlotSearchModel searchModel);

        Task<ZoneListModel> PreparePickupZoneListModel(PickupSlotSearchModel searchModel);

        Task<ZoneModel> PreparePickupZoneModel(ZoneModel model, Zone zone, bool excludeProperties = false);

        #endregion
    }
}