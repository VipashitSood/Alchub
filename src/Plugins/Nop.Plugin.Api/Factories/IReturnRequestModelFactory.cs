using System.Threading.Tasks;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Api.DTO.Orders;

namespace Nop.Plugin.Api.Factories
{
    /// <summary>
    /// Represents the interface of the return request model factory
    /// </summary>
    public partial interface IReturnRequestModelFactory
    {

        /// <summary>
        /// Prepare the customer return requests model
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the customer return requests model
        /// </returns>
        Task<CustomerReturnRequestsModel> PrepareCustomerReturnRequestsModelAsync(int UserId);

        Task<SubmitReturnResponseModel> PrepareSubmitReturnRequestModelAsync(SubmitReturnResponseModel model, Order order ,int orderitemid);
    }
}
