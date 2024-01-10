using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Domain.Markup;
using Nop.Services.Catalog;
using Nop.Services.Localization;
using Nop.Services.Markup;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Models.Markup;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Web.Areas.Admin.Controllers
{
    public partial class CategoryMarkupController : BaseAdminController
    {
        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly IPermissionService _permissionService;
        private readonly ICategoryMarkupModelFactory _categoryMarkupModelFactory;
        private readonly ICategoryMarkupService _categoryMarkupService;
        private readonly IWorkContext _workContext;
        private readonly ICategoryService _categoryService;
        private readonly IBaseAdminModelFactory _baseAdminModelFactory;

        #endregion

        #region Ctor

        public CategoryMarkupController(
            ILocalizationService localizationService,
            INotificationService notificationService,
            IPermissionService permissionService,
            ICategoryMarkupModelFactory categoryMarkupModelFactory,
            ICategoryMarkupService categoryMarkupService,
            IWorkContext workContext, ICategoryService categoryService, IBaseAdminModelFactory baseAdminModelFactory
            )
        {
            _localizationService = localizationService;
            _notificationService = notificationService;
            _permissionService = permissionService;
            _categoryMarkupModelFactory = categoryMarkupModelFactory;
            _categoryMarkupService = categoryMarkupService;
            _workContext = workContext;
            _categoryService = categoryService;
            _baseAdminModelFactory = baseAdminModelFactory;
        }

        #endregion

        #region Methods


        public async Task<IActionResult> Index()
        {
            return RedirectToAction("List");
        }

        public async Task<IActionResult> List()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCategoryMarkups))
                return AccessDeniedView();

            //prepare model
            var model = await _categoryMarkupModelFactory.PrepareCategoryMarkupSearchModel(new CategoryMarkupSearchModel());

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> List(CategoryMarkupSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCategoryMarkups))
                return AccessDeniedView();

            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var vendor = await _workContext.GetCurrentVendorAsync();

            if (vendor != null)
            {
                searchModel.VendorId = vendor.Id;
            }

            var model = await _categoryMarkupModelFactory.PrepareCategoryMarkupListModel(searchModel);

            return Json(model);
        }

        public async Task<IActionResult> Create()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCategoryMarkups))
                return AccessDeniedView();

            //prepare model
            var model = await _categoryMarkupModelFactory.PrepareCategoryMarkupModel(new CategoryMarkupModel(), null);

            return PartialView(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public async Task<IActionResult> Create(CategoryMarkupModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCategoryMarkups))
                return AccessDeniedView();


            var vendor = await _workContext.GetCurrentVendorAsync();
            var vendorId= vendor != null ? vendor.Id : 0;
            if (model.SelectedCategoryId == 0)
            {
                _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Alchub.CategoryMarkup.Category.Select"));
                model = await _categoryMarkupModelFactory.PrepareCategoryMarkupModel(model, null);
                return View(model);
            }
            var categoryMarkups = await _categoryMarkupService.GetAllCategoryMarkupsAsync(model.SelectedCategoryId, 0, vendorId);
            if (categoryMarkups.Count > 0)
            {
                _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Alchub.CategoryMarkup.Category.Already.Exist"));
                model = await _categoryMarkupModelFactory.PrepareCategoryMarkupModel(model, null);
                return View(model);
            }

            CategoryMarkup categoryMarkup = new CategoryMarkup();
            if (model != null)
            {
                categoryMarkup.CreatedOnUtc = DateTime.UtcNow;
                categoryMarkup.UpdatedOnUtc = DateTime.UtcNow;
                categoryMarkup.CategoryId = model.SelectedCategoryId;
                categoryMarkup.Markup = model.Markup;
                categoryMarkup.VendorId = vendor != null ? vendor.Id : 0;
                await _categoryMarkupService.InsertCategoryMarkupAsync(categoryMarkup);
                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.CategoryMarkup.Created"));
                await _categoryMarkupModelFactory.PrepareCategoryMarkupCalculationModel(categoryMarkup.Markup, categoryMarkup.CategoryId, categoryMarkup.VendorId);
            }
            if (!continueEditing)
                return RedirectToAction("List");

            return RedirectToAction("Edit", new { id = categoryMarkup.Id });
        }

        public async Task<IActionResult> Edit(int Id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCategoryMarkups))
                return AccessDeniedView();

            CategoryMarkupModel model = new CategoryMarkupModel();

            if (Id > 0)
            {
                var categoryMarkup = await _categoryMarkupService.GetCategoryMarkupIdAsync(Id);
                if (categoryMarkup != null) 
                {
                    //fill entity from model
                    model.Id = categoryMarkup.Id;
                    model.SelectedCategoryId = categoryMarkup.CategoryId;
                    model.Markup = categoryMarkup.Markup;
                }
            }

            await _baseAdminModelFactory.PrepareCategoriesAsync(model.AvailableCategories,
              defaultItemText: await _localizationService.GetResourceAsync(await _localizationService.GetResourceAsync("Admin.Catalog.Categories.Fields.Select")));

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public async Task<IActionResult> Edit(CategoryMarkupModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCategoryMarkups))
                return AccessDeniedView();


            if (model.SelectedCategoryId == 0)
            {
                _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Alchub.CategoryMarkup.Category.Select"));
                model = await _categoryMarkupModelFactory.PrepareCategoryMarkupModel(model, null);
                return View(model);
            }

            var categoryMarkup = await _categoryMarkupService.GetCategoryMarkupIdAsync(model.Id);
            if (categoryMarkup != null)
            {
                //fill entity from model
                categoryMarkup.Id = model.Id;
                categoryMarkup.CategoryId = model.SelectedCategoryId;
                categoryMarkup.Markup = model.Markup;
                categoryMarkup.UpdatedOnUtc = DateTime.UtcNow;
                await _categoryMarkupService.UpdateCategoryMarkupAsync(categoryMarkup);
                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.CategoryMarkup.Updated"));
                await _categoryMarkupModelFactory.PrepareCategoryMarkupCalculationModel(categoryMarkup.Markup, categoryMarkup.CategoryId, categoryMarkup.VendorId);
            }

            if (!continueEditing)
                return RedirectToAction("List");

            return RedirectToAction("Edit", new { id = categoryMarkup.Id });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCategoryMarkups))
                return AccessDeniedView();

            if (id > 0)
            {
                var categoryMarkup = await _categoryMarkupService.GetCategoryMarkupIdAsync(id);
                if (categoryMarkup != null)
                {
                    await _categoryMarkupService.DeleteCategoryMarkupAsync(categoryMarkup);
                }
            }
            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.CategoryMarkup.Deleted"));

            return RedirectToAction("List");
        }


        [HttpPost]
        public virtual async Task<IActionResult> DeleteSelected(ICollection<int> selectedIds)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCategoryMarkups))
                return AccessDeniedView();

            if (selectedIds == null || selectedIds.Count == 0)
                return NoContent();

            var vendor = await _workContext.GetCurrentVendorAsync();
            var vendorId = vendor != null ? vendor.Id : 0;

            await _categoryMarkupService.DeleteCategoryMarkupAsync(await _categoryMarkupService.GetCategoryMarkupByIdsAsync(selectedIds.ToArray(), vendorId));
            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.CategoryMarkup.Deleted"));
            return Json(new { Result = true });
        }

        #endregion
    }
}