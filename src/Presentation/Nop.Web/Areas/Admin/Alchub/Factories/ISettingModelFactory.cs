using System.Threading.Tasks;
using Nop.Web.Areas.Admin.Alchub.Models.Settings;
using Nop.Web.Areas.Admin.Models.Settings;

namespace Nop.Web.Areas.Admin.Factories
{
    /// <summary>
    /// Represents the setting model factory
    /// </summary>
    public partial interface ISettingModelFactory
    {
        /// <summary>
        /// Prepare service fee settings model
        /// </summary>
        /// <param name="model"></param>
        /// <returns>A task that represents the asynchronous operation The task result contains the news settings model</returns>
        Task<ServiceFeeSettingsModel> PrepareServiceFeeSettingsModelAsync(ServiceFeeSettingsModel model = null);

        /// <summary>
        /// Prepare twillio settings model
        /// </summary>
        /// <param name="model">Twillio settings model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the vendor settings model
        /// </returns>
        Task<TwillioSettingsModel> PrepareTwillioSettingsModelAsync(TwillioSettingsModel model = null);
    }
}
