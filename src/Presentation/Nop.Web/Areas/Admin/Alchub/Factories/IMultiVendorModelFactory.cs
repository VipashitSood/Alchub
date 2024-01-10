using System.Threading.Tasks;
using Nop.Core.Domain.Customers;
using Nop.Web.Areas.Admin.Models.Customers;
using Nop.Web.Areas.Admin.Models.Vendors;

namespace Nop.Web.Areas.Admin.Alchub.Factories
{
    public partial interface IMultiVendorModelFactory
    {
        /// <summary>
        /// Prepare multi vendor search model
        /// </summary>
        /// <param name="searchModel">Customer search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the multi vendor search model
        /// </returns>
        Task<CustomerSearchModel> PrepareMultiVendorSearchModelAsync(CustomerSearchModel searchModel);

        /// <summary>
        /// Prepare paged multi vendor list model
        /// </summary>
        /// <param name="searchModel">Customer search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the multi vendor list model
        /// </returns>
        Task<CustomerListModel> PrepareMultiVendorListModelAsync(CustomerSearchModel searchModel);

        /// <summary>
        /// Prepare paged multi vendor assosiated vendor list model
        /// </summary>
        /// <param name="searchModel">Customer search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the vendor list model
        /// </returns>
        Task<CustomerListModel> PrepareMultiVendorAssosiatedVendorListModelAsync(CustomerSearchModel searchModel);


        /// <summary>
        /// Prepare Multi vendor model
        /// </summary>
        /// <param name="multiVendorModel"></param>
        /// <param name="customer"></param>
        /// <returns></returns>
        Task<MultiVendorModel> PrepareMultiVendorModel(MultiVendorModel multiVendorModel, Customer customer);
    }
}
