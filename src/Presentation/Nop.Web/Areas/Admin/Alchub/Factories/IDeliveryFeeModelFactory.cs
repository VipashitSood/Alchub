using System.Threading.Tasks;
using Nop.Web.Areas.Admin.Models.DeliveryFees;

namespace Nop.Web.Areas.Admin.Factories
{
    /// <summary>
    /// Represents the Delivery Fee model factory
    /// </summary>
    public partial interface IDeliveryFeeModelFactory
    {
        #region Methods

        #region Delivery Fee

        /// <summary>
        /// Prepare Delivery Fee Model Properties
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<DeliveryFeeModel> PrepareDeliveryFeeModelPropertiesAsync(DeliveryFeeModel model = null);

        /// <summary>
        /// Prepare Delivery Fee Model
        /// </summary>
        /// <param name="vendorId"></param>
        /// <returns></returns>
        Task<DeliveryFeeModel> PrepareDeliveryFeeModelAsync(
           DeliveryFeeModel model = null,
           int vendorId = 0,
           bool isAdmin = false);

        #endregion Delivery Fee

        #endregion Methods
    }
}