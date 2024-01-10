using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Discounts;
using Nop.Services.Catalog;
using Nop.Services.Directory;
using Nop.Services.Discounts;
using Nop.Services.Localization;
using Nop.Services.Seo;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Framework.Extensions;
using Nop.Web.Framework.Factories;
using Nop.Web.Framework.Models.Extensions;

namespace Nop.Web.Areas.Admin.Factories
{
    /// <summary>
    /// Represents the manufacturer model factory implementation
    /// </summary>
    public partial class ManufacturerModelFactory : IManufacturerModelFactory
    {
        #region Methods

        /// <summary>
        /// Prepare paged manufacturer product list model
        /// </summary>
        /// <param name="searchModel">Manufacturer product search model</param>
        /// <param name="manufacturer">Manufacturer</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the manufacturer product list model
        /// </returns>
        public virtual async Task<ManufacturerProductListModel> PrepareManufacturerProductListModelAsync(ManufacturerProductSearchModel searchModel,
            Manufacturer manufacturer)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            if (manufacturer == null)
                throw new ArgumentNullException(nameof(manufacturer));

            //get product manufacturers
            var productManufacturers = await _manufacturerService.GetProductManufacturersByManufacturerIdAsync(showHidden: true,
                manufacturerId: manufacturer.Id,
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize,
                //++Alchub master filter
                isMaster: true);

            //prepare grid model
            var model = await new ManufacturerProductListModel().PrepareToGridAsync(searchModel, productManufacturers, () =>
            {
                return productManufacturers.SelectAwait(async productManufacturer =>
                {
                    //fill in model values from the entity
                    var manufacturerProductModel = productManufacturer.ToModel<ManufacturerProductModel>();

                    //fill in additional values (not existing in the entity)
                    manufacturerProductModel.ProductName = (await _productService.GetProductByIdAsync(productManufacturer.ProductId))?.Name;

                    return manufacturerProductModel;
                });
            });

            return model;
        }

        /// <summary>
        /// Prepare paged product list model to add to the manufacturer
        /// </summary>
        /// <param name="searchModel">Product search model to add to the manufacturer</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the product list model to add to the manufacturer
        /// </returns>
        public virtual async Task<AddProductToManufacturerListModel> PrepareAddProductToManufacturerListModelAsync(AddProductToManufacturerSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get products
            var products = await _productService.SearchProductsAsync(showHidden: true,
                categoryIds: new List<int> { searchModel.SearchCategoryId },
                manufacturerIds: new List<int> { searchModel.SearchManufacturerId },
                storeId: searchModel.SearchStoreId,
                vendorId: searchModel.SearchVendorId,
                productType: searchModel.SearchProductTypeId > 0 ? (ProductType?)searchModel.SearchProductTypeId : null,
                keywords: searchModel.SearchProductName,
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize,
                //++Alchub master filter
                isMaster: true);

            //prepare grid model
            var model = await new AddProductToManufacturerListModel().PrepareToGridAsync(searchModel, products, () =>
            {
                return products.SelectAwait(async product =>
                {
                    var productModel = product.ToModel<ProductModel>();

                    productModel.SeName = await _urlRecordService.GetSeNameAsync(product, 0, true, false);

                    return productModel;
                });
            });

            return model;
        }

        #endregion
    }
}