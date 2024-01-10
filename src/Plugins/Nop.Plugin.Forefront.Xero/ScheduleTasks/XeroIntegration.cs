using System;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Plugin.Forefront.Xero.Services;
using Nop.Services.Configuration;
using Nop.Services.ScheduleTasks;

namespace Nop.Plugin.Forefront.Xero.ScheduleTasks
{
    public class XeroIntegration : IScheduleTask
	{
        #region Fields

        private readonly IXeroQueueService _xeroQueueService;
		private readonly IXeroProductService _xeroProductService;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;

        #endregion

        #region Ctor

        public XeroIntegration(IXeroQueueService xeroQueueService, IXeroProductService xeroProductService,
            ISettingService settingService, IStoreContext storeContext)
		{
			_xeroQueueService = xeroQueueService;
			_xeroProductService = xeroProductService;
            _settingService = settingService;
            _storeContext = storeContext;
		}

        #endregion

        #region Methods

        /// <summary>
        /// 
        /// </summary>
		public async Task ExecuteAsync()
        {
            try
            {
                int activeStoreScopeConfiguration = await _storeContext.GetActiveStoreScopeConfigurationAsync();
                var setting = await _settingService.LoadSettingAsync<ForeFrontXeroSetting>(activeStoreScopeConfiguration);

                if (setting.Enable)
                {
                    if (!string.IsNullOrWhiteSpace(setting.ClientId) && !string.IsNullOrWhiteSpace(setting.ClientSecret))
                    {
                        var queueForInvoice =await _xeroQueueService.GetQueueForInvoice();

                        if (queueForInvoice.Count > 0)
                        {
                           await _xeroQueueService.CreateXeroInvoice(queueForInvoice);
                        }

                        if (setting.IsInventorySyncEnabled)
                        {
                           await _xeroProductService.ManageProductQuantity();
                        }

                        var queueForPayment = await _xeroQueueService.GetQueueForPayment();
                        if (queueForPayment.Count > 0)
                        {
                          await  _xeroQueueService.XeroPayment(queueForPayment);
                        }

                        var queueForCancelOrder = await _xeroQueueService.GetQueueForCancelOrder();
                        if (queueForCancelOrder.Count > 0)
                        {
                           await _xeroQueueService.CancelInvoice(queueForCancelOrder);
                        }

                       await _xeroQueueService.ManagePayment();
                    }
                }
            }
            catch (Exception )
            {
            }
		}

        #endregion
    }
}