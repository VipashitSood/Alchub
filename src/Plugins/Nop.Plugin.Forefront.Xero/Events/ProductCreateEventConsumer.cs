using System.Threading.Tasks;
using Nop.Core.Domain.Catalog;
using Nop.Core.Events;
using Nop.Plugin.Forefront.Xero.Services;
using Nop.Services.Events;
using Nop.Services.Plugins;

namespace Nop.Plugin.Forefront.Xero.Events
{
	public class ProductCreateEventConsumer : IConsumer<EntityInsertedEvent<Product>>
	{
        #region Fields

        private readonly IXeroProductService _xeroProductService;
		private readonly IPluginService _pluginService;

        #endregion

        #region Ctor

        public ProductCreateEventConsumer(IXeroProductService xeroProductService, IPluginService pluginService)
		{
			this._xeroProductService = xeroProductService;
            this._pluginService = pluginService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="eventMessage"></param>
        public async Task HandleEventAsync(EntityInsertedEvent<Product> eventMessage)
		{
			if (await _pluginService.GetPluginDescriptorBySystemNameAsync<IPlugin>("Forefront.Xero") != null)
			{
				if (eventMessage.Entity != null)
				{
					if ((eventMessage.Entity.Deleted || !eventMessage.Entity.Published ? false : eventMessage.Entity.VisibleIndividually))
					{
						await _xeroProductService.InsertXeroProductByProductId(eventMessage.Entity.Id, "Create");
					}
					else
					{
						await _xeroProductService.ChangeXeroProductStatus(eventMessage.Entity.Id, true, "Create", false);
					}
				}
			}
		}

        #endregion
    }
}