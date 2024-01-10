using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Alchub.Domain.Catalog;
using Nop.Services.Alchub.Catalog;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Alchub.Factories;
using Nop.Web.Areas.Admin.Alchub.Models.Catalog;
using Nop.Web.Areas.Admin.Controllers;

namespace Nop.Web.Areas.Admin.Alchub.Controllers
{
    public class ProductFriendlyUpcController : BaseAdminController
    {
        #region Fields

        private readonly IPermissionService _permissionService;
        private readonly IProductFriendlyUpcModelFactory _productFriendlyUpcModelFactory;
        private readonly IWorkContext _workContext;
        private readonly INotificationService _notificationService;
        private readonly ILocalizationService _localizationService;
        private readonly IProductFriendlyUpcService _productFriendlyUpcService;

        #endregion

        #region Ctor

        public ProductFriendlyUpcController(IPermissionService permissionService,
            IProductFriendlyUpcModelFactory productFriendlyUpcModelFactory,
            IWorkContext workContext,
            INotificationService notificationService,
            ILocalizationService localizationService,
            IProductFriendlyUpcService productFriendlyUpcService)
        {
            _permissionService = permissionService;
            _productFriendlyUpcModelFactory = productFriendlyUpcModelFactory;
            _workContext = workContext;
            _notificationService = notificationService;
            _localizationService = localizationService;
            _productFriendlyUpcService = productFriendlyUpcService;
        }

        #endregion

        #region Utilities

        #endregion

        #region Methods

        #region Master Product list

        public virtual IActionResult Index()
        {
            return RedirectToAction("List");
        }

        public virtual async Task<IActionResult> List()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProductFriendlyUpc))
                return AccessDeniedView();

            var currentVendor = await _workContext.GetCurrentVendorAsync();
            if (currentVendor == null)
            {
                _notificationService.WarningNotification(await _localizationService.GetResourceAsync("Alchub.Admin.Catalog.ProductFriendlyUpc.Access.Error"));
                return AccessDeniedView();
            }

            //prepare model
            var model = await _productFriendlyUpcModelFactory.PrepareProductFriendlyUpcSearchModelAsync(new ProductFriendlyUpcSearchModel());

            return View(model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> ProductList(ProductFriendlyUpcSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProductFriendlyUpc))
                return await AccessDeniedDataTablesJson();

            var currentVendor = await _workContext.GetCurrentVendorAsync();
            if (currentVendor == null)
                return await AccessDeniedDataTablesJson();

            //prepare model
            var model = await _productFriendlyUpcModelFactory.PrepareProductFriendlyUpctListModelAsync(searchModel);

            return Json(model);
        }

        /// <summary>
        /// Add/Update/Delete friendly upc (AJAX)
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public virtual async Task<IActionResult> CreateOrUpdateProductFrindlyUpc(ProductFriendlyUpcModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProductFriendlyUpc))
                return ErrorJson("You do not have permission to perform the selected operation");

            //validate
            int vendorId = 0;
            if (model.VendorId == 0)
            {
                var currentVendor = await _workContext.GetCurrentVendorAsync();
                if (currentVendor == null)
                    return ErrorJson("Invalid VendorId");

                vendorId = currentVendor.Id;
            }

            if (model.MasterProductId == 0)
                return ErrorJson("Invalid MasterProductId");

            //remove or insert/update
            bool removeFriendlyUpc = string.IsNullOrEmpty(model.FriendlyUPCCode);
            if (!removeFriendlyUpc)
            {
                if (string.IsNullOrEmpty(model.FriendlyUPCCode))
                    return ErrorJson(await _localizationService.GetResourceAsync("Alchub.Admin.Catalog.ProductFriendlyUpc.Error.FriendlyUPCCode.Empty"));

                //trim
                model.FriendlyUPCCode = model.FriendlyUPCCode.Trim();

                //validate friendly upc already assigned to other product
                var vendorFriendlyUpcs = await _productFriendlyUpcService.GetProductFriendlyUpcCodesAsync(vendorId: vendorId);
                if (vendorFriendlyUpcs != null && vendorFriendlyUpcs.Any())
                {
                    if (vendorFriendlyUpcs.Any(x => x.FriendlyUpcCode.Equals(model.FriendlyUPCCode) && x.MasterProductId != model.MasterProductId))
                        return ErrorJson(await _localizationService.GetResourceAsync("Alchub.Admin.Catalog.ProductFriendlyUpc.Error.FriendlyUPCCode.AlreadyExistWithOtherProduct"));
                }

                //get existing record if present
                var existingFriendlyUpc = await _productFriendlyUpcService.GetVendorProductFriendlyUpcCodeByMasterProductIdAsync(vendorId, model.MasterProductId);
                if (existingFriendlyUpc == null)
                {
                    //insert
                    var productFriendlyUpc = new ProductFriendlyUpcCode()
                    {
                        MasterProductId = model.MasterProductId,
                        VendorId = vendorId,
                        FriendlyUpcCode = model.FriendlyUPCCode
                    };

                    await _productFriendlyUpcService.InsertProductFriendlyUpcCodeAsync(productFriendlyUpc);
                }
                else
                {
                    //update
                    existingFriendlyUpc.FriendlyUpcCode = model.FriendlyUPCCode;
                    await _productFriendlyUpcService.UpdateProductFriendlyUpcCodeAsync(existingFriendlyUpc);
                }
            }
            else
            {
                //delete friendly upc
                //get existing record if present
                var existingFriendlyUpc = await _productFriendlyUpcService.GetVendorProductFriendlyUpcCodeByMasterProductIdAsync(vendorId, model.MasterProductId);
                if (existingFriendlyUpc != null)
                {
                    await _productFriendlyUpcService.DeleteProductFriendlyUpcCodeAsync(existingFriendlyUpc);
                }
            }

            return Json(new { Result = true });
        }

        #endregion

        #endregion
    }
}
