using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Domain.Markup;
using Nop.Services.Catalog;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Markup;
using Nop.Services.Vendors;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Markup;
using Nop.Web.Framework.Models.Extensions;

namespace Nop.Web.Areas.Admin.Factories
{
    /// <summary>
    /// Represents the slot model factory implementation
    /// </summary>
    public partial class CategoryMarkupModelFactory : ICategoryMarkupModelFactory
    {
        #region Fields

        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ICategoryMarkupService _categoryMarkupService;
        private readonly IVendorService _vendorService;
        private readonly IWorkContext _workContext;
        private readonly ICategoryService _categoryService;
        private readonly IProductService _productService;
        private readonly IStoreContext _storeContext;
        private readonly IBaseAdminModelFactory _baseAdminModelFactory;
        private readonly ILocalizationService _localizationService;
        #endregion

        #region Ctor

        public CategoryMarkupModelFactory(IDateTimeHelper dateTimeHelper,
            ICategoryMarkupService categoryMarkupService,
            IVendorService vendorService,
            IWorkContext workContext,
            ICategoryService categoryService,
            IProductService productService,
            IStoreContext storeContext,
            IBaseAdminModelFactory baseAdminModelFactory,
            ILocalizationService localizationService)
        {
            _dateTimeHelper = dateTimeHelper;
            _categoryMarkupService = categoryMarkupService;
            _vendorService = vendorService;
            _workContext = workContext;
            _categoryService = categoryService;
            _productService = productService;
            _storeContext = storeContext;
            _storeContext = storeContext;
            _baseAdminModelFactory = baseAdminModelFactory;
            _localizationService = localizationService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Prepare CategoryMarkup search model
        /// </summary>
        /// <param name="searchModel">CategoryMarkup search model</param>
        /// <returns>CategoryMarkup search model</returns>
        public virtual async Task<CategoryMarkupSearchModel> PrepareCategoryMarkupSearchModel(CategoryMarkupSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            await _baseAdminModelFactory.PrepareCategoriesAsync(searchModel.CategoryList,
               defaultItemText: "All");

            //a vendor should have access only to his products
            searchModel.IsLoggedInAsVendor = await _workContext.GetCurrentVendorAsync() != null;

            searchModel.SetGridPageSize();

            return searchModel;
        }

        /// <summary>
        /// Prepare paged zone list model
        /// </summary>
        /// <param name="searchModel">Zone search model</param>
        /// <returns>Zone list model</returns>
        public virtual async Task<CategoryMarkupListModel> PrepareCategoryMarkupListModel(CategoryMarkupSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get newsletter subscriptions
            var categoryMarkups = await _categoryMarkupService.GetAllCategoryMarkupsAsync(searchModel.CategoryId,0 ,searchModel.VendorId, pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

            ////prepare list model
            var model = await new CategoryMarkupListModel().PrepareToGridAsync(searchModel, categoryMarkups, () =>
            {
                return categoryMarkups.SelectAwait(async markup =>
                {
                    //fill in model values from the entity
                    var category = await _categoryService.GetCategoryByIdAsync(markup.CategoryId);
                    var caetgoryMarkupModel = new CategoryMarkupModel();
                    caetgoryMarkupModel.Id = markup.Id;
                    caetgoryMarkupModel.CategoryName = category != null ? await _categoryService.GetFormattedBreadCrumbAsync(category) : "";
                    caetgoryMarkupModel.Markup = markup.Markup;
                    return caetgoryMarkupModel;
                });
            });
            return model;
        }


        /// <summary>
        /// Prepare zone model
        /// </summary>
        /// <param name="model">Zone model</param>
        /// <param name="zone">Zone</param>
        /// <param name="excludeProperties">Whether to exclude populating of some properties of model</param>
        /// <returns>Zone model</returns>
        public virtual async Task<CategoryMarkupModel> PrepareCategoryMarkupModel(CategoryMarkupModel model, CategoryMarkup categoryMarkup, bool excludeProperties = false)
        {

            await _baseAdminModelFactory.PrepareCategoriesAsync(model.AvailableCategories,
             defaultItemText: await _localizationService.GetResourceAsync("Admin.Catalog.Categories.Fields.Select"));

            return model;
        }

        public virtual async Task PrepareCategoryMarkupCalculationModel(decimal markup = 0 , int categoryId = 0, int vendorId=0)
        {
            if (categoryId > 0)
            {
                var store = await _storeContext.GetCurrentStoreAsync();
                List<int> categoryIds = new List<int>();
                categoryIds.Add(categoryId);
                var products = await _productService.SearchProductsAsync(vendorId: vendorId, categoryIds: categoryIds, overridePublished: true, storeId: store.Id, visibleIndividuallyOnly: true);
                foreach (var product in products)
                {
                    var calPrice = Math.Round(product.Price * markup, 2, MidpointRounding.AwayFromZero) / 100;
                    product.Price += calPrice;
                    //second decimal place should always be 9.
                    var price = string.Format("{0:0.0}", product.Price - (product.Price % 0.1M));
                    if (Convert.ToDecimal(price) > 0)
                        product.Price = Convert.ToDecimal(price + "9");
                    await _productService.UpdateProductAsync(product);
                }
            }
        }

        #endregion
    }
}