using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Services.Orders;
using Nop.Web.Areas.Admin.Models.Dispatch;

namespace Nop.Web.Areas.Admin.Factories
{
    /// <summary>
    /// Represents the dispatch model factory implementation
    /// </summary>
    public partial class DispatchModelFactory : IDispatchModelFactory
    {
        #region Fields
        private readonly IWorkContext _workContext;
        private readonly IOrderDispatchService _orderDispatchService;
        #endregion

        #region Ctor

        public DispatchModelFactory(IWorkContext workContext,
             IOrderDispatchService orderDispatchService)
        {
            _workContext = workContext;
            _orderDispatchService = orderDispatchService;
        }
        #endregion

        #region methods

        /// <summary>
        /// Prepare dispatch list model
        /// </summary>
        /// <param name="searchModel">Dispatch brief search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the dispatch list model
        /// </returns>
        public virtual async Task<IList<Dispatch>> PrepareOrderItemsSlotTimeListModelAsync(DispatchSearchModel searchModel)
        {

            var currentVendor = await _workContext.GetCurrentVendorAsync();
            if (currentVendor != null)
                searchModel.VendorId = currentVendor.Id;

            var model = await _orderDispatchService.GetOrderItemVendorTimeSlotsListAysc(searchModel.VendorId);
            return model;
        }
        #endregion
    }
}