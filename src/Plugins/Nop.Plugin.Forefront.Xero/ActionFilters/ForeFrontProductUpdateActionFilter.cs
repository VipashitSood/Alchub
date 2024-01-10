using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Nop.Plugin.Forefront.Xero.Services;
using Nop.Services.Catalog;
using Nop.Services.Plugins;
using System;
using System.Threading.Tasks;

namespace Nop.Plugin.Forefront.Xero.ActionFilters
{
    public  class ForeFrontProductUpdateActionFilter : ActionFilterAttribute
    {
        #region Fields

        private readonly IProductService _productService;
        private readonly IXeroProductService _xeroProductService;
        private readonly IPluginService _pluginService;

        #endregion

        #region Ctor

        public ForeFrontProductUpdateActionFilter(IProductService productService,
            IXeroProductService xeroProductService,
            IPluginService pluginService)
        {
            this._productService = productService;
            this._xeroProductService = xeroProductService;
            this._pluginService = pluginService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filterContext"></param>
        public async Task OnActionExecuted(ActionExecutedContext filterContext)
        {
            string actionName;
            string controllerName;
            var actionDescriptor = filterContext.ActionDescriptor as ControllerActionDescriptor;

            if (actionDescriptor != null)
            {
                actionName = actionDescriptor.ActionName;
                controllerName = actionDescriptor.ControllerName;
            }
            else
            {
                actionName = null;
                controllerName = null;
            }

            bool flag = false;
            int num = 0;
            foreach (var parameter in actionDescriptor.Parameters)
            {
                if (parameter.Name == "id")
                {
                    num++;
                }
            }

            if (num == 0)
            {
                flag = true;
            }

            if ((string.IsNullOrEmpty(actionName) ? false : !string.IsNullOrEmpty(controllerName)))
            {
                if ((await _pluginService.GetPluginDescriptorBySystemNameAsync<IPlugin>("Forefront.Xero") == null || !(controllerName == "Product") ? false : actionName == "Edit") & flag)
                {
                    object item = filterContext.RouteData.Values["Id"];
                    var productById = await  _productService.GetProductByIdAsync(Convert.ToInt32(item));
                    if (productById != null)
                    {
                        if ((productById.Deleted || !productById.Published ? true : !productById.VisibleIndividually))
                        {
                            if (await _xeroProductService.GetXeroProductByProductId(productById.Id) != null)
                            {
                                await _xeroProductService.ChangeXeroProductStatus(productById.Id, true, "Update", false);
                            }
                            else
                            {
                                await _xeroProductService.InsertXeroProductByProductId(productById.Id, "Create");
                            }
                        }
                        else if (await _xeroProductService.GetXeroProductByProductId(productById.Id) != null)
                        {
                            await _xeroProductService.ChangeXeroProductStatus(productById.Id, false, "Update", false);
                        }
                        else
                        {
                            await _xeroProductService.InsertXeroProductByProductId(productById.Id, "Create");
                        }
                    }
                }
            }
        }
        #endregion
    }
}