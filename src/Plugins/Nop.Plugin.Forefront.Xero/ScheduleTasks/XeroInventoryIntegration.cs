using System;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Plugin.Forefront.Xero.Services;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.ScheduleTasks;

namespace Nop.Plugin.Forefront.Xero.ScheduleTasks
{
    public class XeroInventoryIntegration : IScheduleTask
	{
        #region Fields

		private readonly IXeroProductService _xeroProductService;
		private readonly IProductService _productService;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;

        #endregion

        #region Ctor

        public XeroInventoryIntegration(IXeroProductService xeroProductService,
           IProductService productService, ISettingService settingService, IStoreContext storeContext)
		{
            _xeroProductService = xeroProductService;
            _productService = productService;
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
                    if (!string.IsNullOrEmpty(setting.ClientId) && !string.IsNullOrEmpty(setting.ClientSecret))
                    {
                        var productsForInventoryByFixedBatch = await _xeroProductService.GetProductsForInventoryByFixedBatch();
                        if (productsForInventoryByFixedBatch.Count > 0)
                        {
                            var productsByIds =await _productService.GetProductsByIdsAsync(productsForInventoryByFixedBatch.Select(p => p.ProductId).ToArray());
                            productsByIds = productsByIds.Where(p => !string.IsNullOrEmpty(p.UPCCode)).ToList();
                            if (productsByIds.Count > 0)
                            {
                               await _xeroProductService.CreateXeroItem(productsByIds);
                            }
                        }
                    }
                }
			}
			catch (Exception exc)
			{
                
			}
		}

        #endregion
	}
}