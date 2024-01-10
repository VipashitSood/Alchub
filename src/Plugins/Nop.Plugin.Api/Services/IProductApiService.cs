using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Plugin.Api.DTOs.Products;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Nop.Plugin.Api.Services
{
    public interface IProductApiService
    {
        IList<Product> GetProducts(
            IList<int> ids = null,
            DateTime? createdAtMin = null, DateTime? createdAtMax = null, DateTime? updatedAtMin = null, DateTime? updatedAtMax = null,
            int? limit = null, int? page = null,
            int? sinceId = null,
            int? categoryId = null, string vendorName = null, bool? publishedStatus = null, IList<string> manufacturerPartNumbers = null, bool? isDownload = null);

        Task<int> GetProductsCountAsync(
            DateTime? createdAtMin = null, DateTime? createdAtMax = null,
            DateTime? updatedAtMin = null, DateTime? updatedAtMax = null, bool? publishedStatus = null,
            string vendorName = null, int? categoryId = null, IList<string> manufacturerPartNumbers = null, bool? isDownload = null);

        Product GetProductById(int productId);

        Product GetProductByIdNoTracking(int productId);

        IList<Product> GetfeaturedProducts();

        IList<Product> GetDealsOfTheDayProducts();

        /// <summary>
        /// Get product list / Search Product
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="productType"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="keywords"></param>
        /// <returns></returns>
        Task<IPagedList<Product>> GetProductsList(int userId, int storeId = 0, IList<int> categoryIds = null, int productType = 0, int? pageIndex = null, int? pageSize = null, string keywords = null, decimal? priceMin = null, decimal? priceMax = null, ProductSortingEnum? orderBy = null, IList<int> manufacturerIds = null, IList<SpecificationAttributeOption> filteredSpecOptions = null, IList<int> vendorIds = null);

        /// <summary>
        /// Get products by productIds 
        /// </summary>
        /// <param name="productIds"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="storeId"></param>
        /// <returns></returns>
        Task<IPagedList<Product>> GetProductByIdsAsync(
          IList<int> productIds,
          int? pageIndex = 0,
          int? pageSize = int.MaxValue,
          int storeId = 0);

        Task<AllReviewDto> GetAllReview(int productId = 0);

        Task<decimal> GetProductsPriceMaxAsync(int? categoryId = null);

        Task<decimal> GetProductsPriceMinAsync(int? categoryId = null);

        #region Compare products

        /// <summary>
        /// Adds a product to a "compare products" list (for API)
        /// </summary>
        /// <param name="productId">Product identifier</param>
        /// <param name="customer">Customer</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task AddProductToCompareListAsync(int productId, Customer customer);

        /// <summary>
        /// Gets a "compare products" list
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the "Compare products" list
        /// </returns>
        Task<IList<Product>> GetComparedProductsAsync(Customer customer);

        /// <summary>
        /// Removes a product from a "compare products" list
        /// </summary>
        /// <param name="productId">Product identifier</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task RemoveProductFromCompareListAsync(int productId, Customer customer);

        #endregion

        #region
        Task<IList<CrossSellProduct>> GetCrossSellProductsByProductIdsAsync(int[] productIds, bool showHidden = false);
        #endregion
    }
}
