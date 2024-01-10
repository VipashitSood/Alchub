using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Alchub.Domain.ServiceFee;
using Nop.Core.Domain.Settings;
using Nop.Services.Configuration;

namespace Nop.Services.Alchub.ServiceFee
{
    /// <summary>
    /// Represents a service fee manager service
    /// </summary>
    public partial class ServiceFeeManager : IServiceFeeManager
    {
        #region fields
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;
        #endregion

        #region ctor
        public ServiceFeeManager(ISettingService settingService,
            IStoreContext storeContext)
        {
            _settingService = settingService;
            _storeContext = storeContext;
        }
        #endregion

        #region methods

        /// <summary>
        /// Gets service fee
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the order model
        /// </returns>
        public async Task<decimal> GetServiceFeeAsync(decimal subtotal)
        {
            var storeId = (await _storeContext.GetCurrentStoreAsync()).Id;
            //service fee settings
            var serviceFeeSettings = await _settingService.LoadSettingAsync<ServiceFeeSettings>(storeId);
            decimal serviceFee;

            //check service fee type 
            if (serviceFeeSettings.ServiceFeeTypeId == (int)ServiceFeeType.Percentage)
            {
                 serviceFee = (subtotal * serviceFeeSettings.ServiceFeePercentage) / 100;

                //check if service fee exceeds maximum service fee, consider maximum service fee as service fee.
                if (serviceFee > serviceFeeSettings.MaximumServiceFee)
                    serviceFee = serviceFeeSettings.MaximumServiceFee;
            }
            else
                serviceFee = serviceFeeSettings.ServiceFee;

            return serviceFee;
        }
        #endregion
    }
}
