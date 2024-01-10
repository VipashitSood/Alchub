using Nop.Plugin.Payments.StripeConnectRedirect.Models;
using System.Threading.Tasks;

namespace Nop.Plugin.Payments.StripeConnectRedirect.Factories
{
    /// <summary>
    /// Represents the stripe connect vendor model factory
    /// </summary>
    public partial interface IVendorModelFactory
    {
        /// <summary>
        /// Prepare paged vendor list model
        /// </summary>
        /// <param name="searchModel">Vendor search model</param>
        /// <returns>Vendor list model</returns>
        Task<VendorListModel> PrepareVendorListModelAsync(VendorSearchModel searchModel);
    }
}