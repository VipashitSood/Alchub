using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Models.Dispatch;

namespace Nop.Web.Areas.Admin.Factories
{
    public partial interface IDispatchModelFactory
    {
        /// <summary>
        ///  Prepare Order Items according to slot time 
        /// </summary>
        /// <param name="searchModel">searchModel</param>
        /// <returns></returns>
        Task<IList<Dispatch>> PrepareOrderItemsSlotTimeListModelAsync(DispatchSearchModel searchModel);
    }
}
