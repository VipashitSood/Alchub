using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Services.Alchub.Catalog;
using Nop.Services.Catalog;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Web.Areas.Admin.Alchub.Models.Catalog;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Framework.Models.Extensions;

namespace Nop.Web.Areas.Admin.Alchub.Factories
{
    public class ProductFriendlyUpcModelFactory : IProductFriendlyUpcModelFactory
    {
        #region Fields

        private readonly ICategoryService _categoryService;
        private readonly IProductService _productService;
        private readonly IWorkContext _workContext;
        private readonly ILocalizationService _localizationService;
        private readonly IPictureService _pictureService;
        private readonly IProductFriendlyUpcService _productFriendlyUpcService;
        private readonly IBaseAdminModelFactory _baseAdminModelFactory;

        #endregion

        #region Ctor

        public ProductFriendlyUpcModelFactory(ICategoryService categoryService,
            IProductService productService,
            IWorkContext workContext,
            ILocalizationService localizationService,
            IPictureService pictureService,
            IProductFriendlyUpcService productFriendlyUpcService,
            IBaseAdminModelFactory baseAdminModelFactory)
        {
            _categoryService = categoryService;
            _productService = productService;
            _workContext = workContext;
            _localizationService = localizationService;
            _pictureService = pictureService;
            _productFriendlyUpcService = productFriendlyUpcService;
            _baseAdminModelFactory = baseAdminModelFactory;
        }

        #endregion
        /// <summary>
        /// Prepare product friendly upc search model
        /// </summary>
        /// <param name="searchModel">Product search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the product search model
        /// </returns>
        public virtual async Task<ProductFriendlyUpcSearchModel> PrepareProductFriendlyUpcSearchModelAsync(ProductFriendlyUpcSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //prepare available categories
            await _baseAdminModelFactory.PrepareCategoriesAsync(searchModel.AvailableCategories);

            //prepare grid
            searchModel.SetGridPageSize();

            return await Task.FromResult(searchModel);
        }

        /// <summary>
        /// Prepare paged product friendly upc list model
        /// </summary>
        /// <param name="searchModel">Product search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the product list model
        /// </returns>
        public virtual async Task<ProductFriendlyUpcListModel> PrepareProductFriendlyUpctListModelAsync(ProductFriendlyUpcSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var currentVendor = await _workContext.GetCurrentVendorAsync();
            if (currentVendor == null)
                throw new ArgumentNullException(nameof(currentVendor));

            //get parameters to filter comments
            var categoryIds = new List<int> { searchModel.SearchCategoryId };
            bool includeSubCategories = true;
            if (includeSubCategories && searchModel.SearchCategoryId > 0)
            {
                var childCategoryIds = await _categoryService.GetChildCategoryIdsAsync(parentCategoryId: searchModel.SearchCategoryId, showHidden: true);
                categoryIds.AddRange(childCategoryIds);
            }

            //get master products
            var products = await _productFriendlyUpcService.SearchMasterProductsAsync(vendorId: currentVendor.Id, showHidden: true,
                categoryIds: categoryIds,
                productType: ProductType.SimpleProduct, //exclude grouped product (20-12-22)
                keywords: searchModel.SearchProductName,
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize,
                isMaster: true,
                friendlyUpcCode: searchModel.SearchFriendlyUpcCode);

            //prepare list model
            var model = await new ProductFriendlyUpcListModel().PrepareToGridAsync(searchModel, products, () =>
            {
                return products.SelectAwait(async product =>
                {
                    //fill in model values from the entity
                    var productFriendlyUpcModel = new ProductFriendlyUpcModel()
                    {
                        MasterProductId = product.Id,
                        VendorId = currentVendor.Id,
                        ProductName = await _localizationService.GetLocalizedAsync(product, x => x.Name),
                        OriginalUPCCode = product.UPCCode,
                        Sku = product.Sku,
                        Size = product.Size
                    };

                    //picture
                    var defaultProductPicture = (await _pictureService.GetPicturesByProductIdAsync(product.Id, 1)).FirstOrDefault();
                    (productFriendlyUpcModel.PictureThumbnailUrl, _) = await _pictureService.GetPictureUrlAsync(defaultProductPicture, 75);

                    //category name
                    var categoryName = "";
                    var productCategories = (await _categoryService.GetProductCategoriesByProductIdAsync(product.Id)).FirstOrDefault();
                    if (productCategories != null)
                    {
                        categoryName = (await _categoryService.GetCategoryByIdAsync((int)productCategories.CategoryId)).Name;
                    }
                    productFriendlyUpcModel.Category = categoryName != null ? categoryName : "";

                    //fienaldy Upc
                    var vendorFriendlyUpc = await _productFriendlyUpcService.GetVendorProductFriendlyUpcCodeByMasterProductIdAsync(currentVendor.Id, product.Id);
                    if (vendorFriendlyUpc != null)
                    {
                        productFriendlyUpcModel.Id = vendorFriendlyUpc.Id;
                        productFriendlyUpcModel.FriendlyUPCCode = vendorFriendlyUpc.FriendlyUpcCode;
                    }

                    return productFriendlyUpcModel;
                });
            });

            return model;
        }
    }
}
