using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Cms;
using Nop.Data;
using Nop.Plugin.Widgets.JCarousel;
using Nop.Plugin.Widgets.JCarousel.Domain;
using Nop.Plugin.Widgets.JCarousel.Factories;
using Nop.Plugin.Widgets.JCarousel.Models.Configuration;
using Nop.Plugin.Widgets.JCarousel.Services;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Mvc.Filters;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Nop.Plugin.Widgets.JCorousel.Controller
{
    [Area(AreaNames.Admin)]
    [AuthorizeAdmin]
    [AutoValidateAntiforgeryToken]
    public class JCarouselController : BasePluginController
    {
        #region Fields
        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly IPermissionService _permissionService;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;
        private readonly IStoreService _storeService;
        private readonly IWebHelper _webHelper;
        private readonly IRepository<JCarouselLog> _jCarousalRepository;
        private readonly IJCarouselService _jCarouselService;
        private readonly IJCarouselModelFactory _jCarouselModelFactory;
        private readonly IPublicJCarouselModelFactory _publicjCarouselModelFactory;
        private readonly IProductService _productService;
        private readonly IJCarouselService _jcarouselService;
        private readonly IBaseAdminModelFactory _baseAdminModelFactory;
        private readonly IOrderReportService _orderReportService;
        #endregion

        #region Ctor
        public JCarouselController(ILocalizationService localizationService,
            INotificationService notificationService,
            IPermissionService permissionService,
            ISettingService settingService,
            IStoreContext storeContext,
            IStoreService storeService,
            IWebHelper webHelper,
            IRepository<JCarouselLog> jCarousalRepository,
            IJCarouselService jCarouselService,
            IJCarouselModelFactory jCarouselModelFactory,
            IPublicJCarouselModelFactory publicjCarouselModelFactory,
            IProductService productService,
            IJCarouselService jcarouselService,
            IBaseAdminModelFactory baseAdminModelFactory,
            IOrderReportService orderReportService)
        {
            _localizationService = localizationService;
            _notificationService = notificationService;
            _permissionService = permissionService;
            _settingService = settingService;
            _storeContext = storeContext;
            _storeService = storeService;
            _webHelper = webHelper;
            _jCarousalRepository = jCarousalRepository;
            _jCarouselService = jCarouselService;
            _jCarouselModelFactory = jCarouselModelFactory;
            _publicjCarouselModelFactory = publicjCarouselModelFactory;
            _productService = productService;
            _jcarouselService = jcarouselService;
            _baseAdminModelFactory = baseAdminModelFactory;
            _orderReportService = orderReportService;
        }
        #endregion

        #region Methods

        public async Task<IActionResult> Configure()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageWidgets))
                return AccessDeniedView();
            var storeId = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var settings = await _settingService.LoadSettingAsync<JCarouselPluginSettings>(storeId);
            var widgetSettings = await _settingService.LoadSettingAsync<WidgetSettings>(storeId);
            var model = new ConfigurationModel
            {
                Enabled = settings.Enable,
                ActiveStoreScopeConfiguration = storeId,
                LazyLoadNumberOfProductInCarousel = settings.LazyLoadNumberOfProductInCarousel,
                EnableViewAll = settings.EnableViewAll
            };
            if (storeId > 0)
            {
                model.Enabled_OverrideForStore = await _settingService.SettingExistsAsync(widgetSettings, setting => setting.ActiveWidgetSystemNames, storeId);
                model.Enabled_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.Enable, storeId);
                model.Enabled_LazyLoadNumberOfProductInCarousel = await _settingService.SettingExistsAsync(settings, x => x.LazyLoadNumberOfProductInCarousel, storeId);
                model.Enabled_EnableViewAll = await _settingService.SettingExistsAsync(settings, x => x.EnableViewAll, storeId);
            }
            return View("~/Plugins/Widgets.JCarousel/Views/Admin/Configure/Configure.cshtml", model);
        }

        [HttpPost]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageWidgets))
                return AccessDeniedView();
            if (!ModelState.IsValid)
                return await Configure();
            var storeId = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var settings = await _settingService.LoadSettingAsync<JCarouselPluginSettings>(storeId);
            var widgetSettings = await _settingService.LoadSettingAsync<WidgetSettings>(storeId);

            settings.Enable = model.Enabled;
            settings.LazyLoadNumberOfProductInCarousel = model.LazyLoadNumberOfProductInCarousel;
            settings.EnableViewAll = model.EnableViewAll;

            if (model.Enabled && !widgetSettings.ActiveWidgetSystemNames.Contains(JCarouselDefaults.SystemName))
                widgetSettings.ActiveWidgetSystemNames.Add(JCarouselDefaults.SystemName);
            if (!model.Enabled && widgetSettings.ActiveWidgetSystemNames.Contains(JCarouselDefaults.SystemName))
                widgetSettings.ActiveWidgetSystemNames.Remove(JCarouselDefaults.SystemName);
            await _settingService.SaveSettingOverridablePerStoreAsync(widgetSettings, setting => setting.ActiveWidgetSystemNames, model.Enabled, storeId, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, setting => setting.LazyLoadNumberOfProductInCarousel, model.Enabled_LazyLoadNumberOfProductInCarousel, storeId, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, setting => setting.EnableViewAll, model.Enabled_EnableViewAll, storeId, false);
            await _settingService.ClearCacheAsync();
            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Plugins.Widgets.JCarousel.Saved"));
            return await Configure();
        }
        #endregion

        #region Create / Edit / Delete / List

        public async Task<IActionResult> Create(bool showtour = false)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageWidgets))
                return AccessDeniedView();
            //prepare model
            var model = await _jCarouselModelFactory.PrepareJCarouselModelAsync(new JCarouselModel(), null);
            return View("~/Plugins/Widgets.JCarousel/Views/Admin/Create.cshtml", model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public async Task<IActionResult> Create(JCarouselModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageWidgets))
                return AccessDeniedView();
            if (ModelState.IsValid)
            {
                if (model.SelectedCategoryId == 0 && !model.IsBestSeller)
                {
                    _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Plugins.Widgets.Jcarousel.Categories.Fields.Select"));
                }
                else
                {
                    //To check if jcarousel name already exists
                    var existingName = _jcarouselService.CheckExistingName(model.Name);

                    if (existingName == null)
                    {
                        //fill in entity values from the model
                        var jcarousel = model.ToEntity<JCarouselLog>();
                        jcarousel.CategoryId = model.SelectedCategoryId;
                        //set datasource id as bestSellersProductsByQuantity identifier when IsBestSeller is true.
                        jcarousel.DataSourceTypeId = model.IsBestSeller ? (int)DataSourceType.BestSellersProductsByQuantity : 0;
                        await _jCarouselService.InsertJCarouselAsync(jcarousel);
                        _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Plugins.Widgets.JCarousel.Added"));
                        if (!continueEditing)
                            return RedirectToAction("List");
                        return RedirectToAction("Edit", new { id = jcarousel.Id });
                    }
                    else
                    {
                        _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Plugins.Widgets.JCarousel.Name.AlreadyExists"));
                    }
                }
            }
            //prepare model
            model = await _jCarouselModelFactory.PrepareJCarouselModelAsync(model, null);
            //if we got this far, something failed, redisplay form
            return View("~/Plugins/Widgets.JCarousel/Views/Admin/Create.cshtml", model);
        }

        public virtual async Task<IActionResult> Edit(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageWidgets))
                return AccessDeniedView();

            //try to get a jcarousel with the specified id
            var jcarousel = await _jCarouselService.GetJCarouselByIdAsync(id);
            if (jcarousel == null)
                return RedirectToAction("List");

            //prepare model
            var model = await _jCarouselModelFactory.PrepareJCarouselModelAsync(null, jcarousel);

            return View("~/Plugins/Widgets.JCarousel/Views/Admin/Edit.cshtml", model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> Edit(JCarouselModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageWidgets))
                return AccessDeniedView();
            //try to get a jcarousel with the specified id
            var jcarousel = await _jCarouselService.GetJCarouselByIdAsync(model.Id);
            if (jcarousel == null)
                return RedirectToAction("List");
            //if it is not a bestSeller and not any category is selected, then show error
            if (model.SelectedCategoryId == 0 && !model.IsBestSeller)
            {
                _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Plugins.Widgets.Jcarousel.Categories.Fields.Select"));
                return RedirectToAction("Edit", new { id = jcarousel.Id });
            }
            if (model.IsBestSeller && jcarousel.DataSourceType != DataSourceType.BestSellersProductsByQuantity || jcarousel.CategoryId != model.SelectedCategoryId)
            {
                var existingProductjcarousels = await _jCarouselService.GetProductJCarouselsByJCarouselIdAsync(model.Id);
                foreach (var productjcarouselmap in existingProductjcarousels)
                {
                    //delete the new product jcarousel mapping
                    await _jCarouselService.DeleteProductJCarouselAsync(productjcarouselmap);
                }
            }
            if (jcarousel.Name != model.Name)
            {
                //To check if jcarousel name already exists
                var existingName = _jcarouselService.CheckExistingName(model.Name);
                if (existingName == null)
                {
                    if (ModelState.IsValid)
                    {
                        //fill in entity values from the model
                        jcarousel = model.ToEntity(jcarousel);
                        //set datasource id as bestSellersProductsByQuantity identifier when IsBestSeller is true.
                        jcarousel.DataSourceTypeId = model.IsBestSeller ? (int)DataSourceType.BestSellersProductsByQuantity : 0;
                        if (!model.IsBestSeller)
                            jcarousel.CategoryId = model.SelectedCategoryId;
                        await _jCarouselService.UpdateJCarouselAsync(jcarousel);

                        if (!continueEditing)
                            return RedirectToAction("List");
                        return RedirectToAction("Edit", new { id = jcarousel.Id });
                    }
                    //prepare model
                    model = await _jCarouselModelFactory.PrepareJCarouselModelAsync(model, jcarousel);
                    //if we got this far, something failed, redisplay form
                    return View("~/Plugins/Widgets.JCarousel/Views/Admin/Edit.cshtml", model);
                }
                else
                {
                    _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Plugins.Widgets.JCarousel.Name.AlreadyExists"));
                    return RedirectToAction("Edit", new { id = jcarousel.Id });
                }
            }
            else
            {
                if (ModelState.IsValid)
                {
                    //fill in entity values from the model
                    jcarousel = model.ToEntity(jcarousel);
                    //set datasource id as bestSellersProductsByQuantity identifier when IsBestSeller is true.
                    jcarousel.DataSourceTypeId = model.IsBestSeller ? (int)DataSourceType.BestSellersProductsByQuantity : 0;
                    if (!model.IsBestSeller)
                        jcarousel.CategoryId = model.SelectedCategoryId;
                    await _jCarouselService.UpdateJCarouselAsync(jcarousel);

                    if (!continueEditing)
                        return RedirectToAction("List");
                    return RedirectToAction("Edit", new { id = jcarousel.Id });
                }
                //prepare model
                model = await _jCarouselModelFactory.PrepareJCarouselModelAsync(model, jcarousel);
                //if we got this far, something failed, redisplay form
                return View("~/Plugins/Widgets.JCarousel/Views/Admin/Edit.cshtml", model);
            }
        }

        [HttpPost]
        public virtual async Task<IActionResult> Delete(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageWidgets))
                return AccessDeniedView();
            await _jCarouselService.DeleteProductReferenceAsync(id);
            //try to get a Jcarousel with the specified id
            var jcarousel = await _jCarouselService.GetJCarouselByIdAsync(id);
            if (jcarousel == null)
                return RedirectToAction("List");
            await _jCarouselService.DeleteJCarouselAsync(jcarousel);
            return new NullJsonResult();
        }
        public virtual async Task<IActionResult> List()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageWidgets))
                return AccessDeniedView();
            //prepare model
            var model = await _jCarouselModelFactory.PrepareJCarouselSearchModelAsync(new JCarouselSearchModel());
            return View("~/Plugins/Widgets.JCarousel/Views/Admin/List.cshtml", model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> JCarouselList(JCarouselSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageWidgets))
                return await AccessDeniedDataTablesJson();
            //prepare model
            var model = await _jCarouselModelFactory.PrepareJCarouselListModelAsync(searchModel);
            return Json(model);
        }

        #endregion

        #region Products

        [HttpPost]
        public virtual async Task<IActionResult> ProductList(JCarouselProductSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageWidgets))
                return await AccessDeniedDataTablesJson();
            //try to get a jcarousel with the specified id
            var jcarousel = await _jCarouselService.GetJCarouselByIdAsync(searchModel.JCarouselId)
                ?? throw new ArgumentException("No jcarousel found with the specified id");
            //prepare model
            var model = await _jCarouselModelFactory.PrepareJCarouselProductListModelAsync(searchModel, jcarousel);
            return Json(model);
        }

        public virtual async Task<IActionResult> ProductUpdate(ProductJCarouselMappingModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageWidgets))
                return AccessDeniedView();
            //try to get a product jcarousel with the specified id
            var productJCarousel = await _jCarouselService.GetProductJCarouselByIdAsync(model.Id)
                ?? throw new ArgumentException("No product jcarousel mapping found with the specified id");

            //fill in entity values from the model
            productJCarousel = model.ToEntity(productJCarousel);
            await _jCarouselService.UpdateProductJCarouselAsync(productJCarousel);
            return new NullJsonResult();
        }

        public virtual async Task<IActionResult> ProductDelete(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageWidgets))
                return AccessDeniedView();
            //try to get a product jcarousel with the specified id
            var productJCarousel = await _jCarouselService.GetProductJCarouselByIdAsync(id)
                ?? throw new ArgumentException("No product jcarousel mapping found with the specified id", nameof(id));
            await _jCarouselService.DeleteProductJCarouselAsync(productJCarousel);
            return new NullJsonResult();
        }

        //Product Add popup function
        public virtual async Task<IActionResult> ProductAddPopup(int jcarouselId)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageWidgets))
                return AccessDeniedView();
            var jcarousel = await _jCarouselService.GetJCarouselByIdAsync(jcarouselId);
            //prepare model
            var model = await _jCarouselModelFactory.PrepareAddProductToJCarouselSearchModelAsync(new AddProductToJCarouselSearchModel());
            model.SearchCategoryId = jcarousel.CategoryId;

            return View("~/Plugins/Widgets.JCarousel/Views/Admin/ProductAddPopup.cshtml", model);
        }

        [HttpPost]
        [FormValueRequired("save")]
        public virtual async Task<IActionResult> ProductAddPopup(AddProductToJCarouselModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageWidgets))
                return AccessDeniedView();
            //get selected products
            var selectedProducts = await _productService.GetProductsByIdsAsync(model.SelectedProductIds.ToArray());
            if (selectedProducts.Any())
            {
                var existingProductjcarousels = await _jCarouselService.GetProductJCarouselsByJCarouselIdAsync(model.JCarouselId);
                foreach (var product in selectedProducts)
                {
                    //whether product jcarousels with such parameters already exists
                    if (_jCarouselService.FindProductJCarousel(existingProductjcarousels, product.Id, model.JCarouselId) != null)
                        continue;
                    //insert the new product jcarousel mapping
                    await _jCarouselService.InsertProductJCarouselAsync(new ProductJCarouselMapping
                    {
                        JCarouselId = model.JCarouselId,
                        ProductId = product.Id
                    });
                }
            }
            ViewBag.RefreshPage = true;

            return View("~/Plugins/Widgets.JCarousel/Views/Admin/ProductAddPopup.cshtml", new AddProductToJCarouselSearchModel());
        }

        [HttpPost]
        public virtual async Task<IActionResult> ProductAddPopupList(AddProductToJCarouselSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageWidgets))
                return await AccessDeniedDataTablesJson();
            //prepare model
            var model = await _jCarouselModelFactory.PrepareAddProductToJCarouselListModelAsync(searchModel);
            return Json(model);
        }
        #endregion

    }
}
