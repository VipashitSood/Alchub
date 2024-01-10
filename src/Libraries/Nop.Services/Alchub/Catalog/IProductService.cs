using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;

namespace Nop.Services.Catalog
{
    /// <summary>
    /// Product service
    /// </summary>
    public partial interface IProductService
    {
        #region Products

        /// <summary>
        /// Get last inserted master product sku
        /// </summary>
        /// <returns></returns>
        Task<string> GetLastInsertedProductSkuAsync();

        /// <summary>
        /// Get number of product (published and visible) in certain category by geo vendor ids
        /// </summary>
        /// <param name="categoryIds">Category identifiers</param>
        /// <param name="storeId">Store identifier; 0 to load all records</param>
        /// <param name="vendorIds">Vendor identifiers</param>
        /// <param name="isMaster">isMaster false to load all records</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the number of products
        /// </returns>
        Task<int> GetNumberOfProductsInCategoryByGeoVendorIdsAsync(IList<int> categoryIds = null, int storeId = 0,
            IList<int> manufacturerIds = null, IList<int> vendorIds = null, decimal? priceMin = null,
            decimal? priceMax = null,
            string keywords = null,
            IList<SpecificationAttributeOption> filteredSpecOptions = null, bool? isMaster = null);

        /// <summary>
        /// Gets all products displayed on the home page
        /// </summary>
        /// <param name="isMasterOnly">A value indicating whether to show only master product records</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the products
        /// </returns>
        Task<IList<Product>> GetAllProductsDisplayedOnHomepageAsync(bool isMasterOnly = false);

        /// <summary>
        /// Search products
        /// </summary>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="categoryIds">Category identifiers</param>
        /// <param name="manufacturerIds">Manufacturer identifiers</param>
        /// <param name="storeId">Store identifier; 0 to load all records</param>
        /// <param name="vendorId">Vendor identifier; 0 to load all records</param>
        /// <param name="warehouseId">Warehouse identifier; 0 to load all records</param>
        /// <param name="productType">Product type; 0 to load all records</param>
        /// <param name="visibleIndividuallyOnly">A values indicating whether to load only products marked as "visible individually"; "false" to load all records; "true" to load "visible individually" only</param>
        /// <param name="excludeFeaturedProducts">A value indicating whether loaded products are marked as featured (relates only to categories and manufacturers); "false" (by default) to load all records; "true" to exclude featured products from results</param>
        /// <param name="priceMin">Minimum price; null to load all records</param>
        /// <param name="priceMax">Maximum price; null to load all records</param>
        /// <param name="productTagId">Product tag identifier; 0 to load all records</param>
        /// <param name="keywords">Keywords</param>
        /// <param name="searchDescriptions">A value indicating whether to search by a specified "keyword" in product descriptions</param>
        /// <param name="searchManufacturerPartNumber">A value indicating whether to search by a specified "keyword" in manufacturer part number</param>
        /// <param name="searchSku">A value indicating whether to search by a specified "keyword" in product SKU</param>
        /// <param name="searchProductTags">A value indicating whether to search by a specified "keyword" in product tags</param>
        /// <param name="languageId">Language identifier (search for text searching)</param>
        /// <param name="filteredSpecOptions">Specification options list to filter products; null to load all records</param>
        /// <param name="orderBy">Order by</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <param name="overridePublished">
        /// <param name="isMaster">A value indicating whether to show only master product records</param>
        /// <param name="geoVendorIds">The list of vendor identifiers who are available according geo location</param>
        /// null - process "Published" property according to "showHidden" parameter
        /// true - load only "Published" products
        /// false - load only "Unpublished" products
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the products
        /// </returns>
        Task<IPagedList<Product>> SearchProductsAsync(
            int pageIndex = 0,
            int pageSize = int.MaxValue,
            IList<int> categoryIds = null,
            IList<int> manufacturerIds = null,
            int storeId = 0,
            int vendorId = 0,
            int warehouseId = 0,
            ProductType? productType = null,
            bool visibleIndividuallyOnly = false,
            bool excludeFeaturedProducts = false,
            decimal? priceMin = null,
            decimal? priceMax = null,
            int productTagId = 0,
            string keywords = null,
            bool searchDescriptions = false,
            bool searchManufacturerPartNumber = true,
            bool searchSku = true,
            bool searchProductTags = false,
            int languageId = 0,
            IList<SpecificationAttributeOption> filteredSpecOptions = null,
            ProductSortingEnum orderBy = ProductSortingEnum.Position,
            bool showHidden = false,
            bool? overridePublished = null,
            bool? isMaster = null,
            string upccode = null,
            IList<int> geoVendorIds = null,
            string size = null,
            Customer customer = null);

        /// <summary>
        /// Gets products which marked as new
        /// </summary>
        /// <param name="storeId">Store identifier; 0 if you want to get all records</param>
        /// <param name="geoVendorIds">Available geo radius vendor identifiers</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the list of new products
        /// </returns>
        Task<IList<Product>> GetProductsMarkedAsNewAsync(int storeId = 0, IList<int> geoVendorIds = null);

        /// <summary>
        /// Gets associated products
        /// </summary>
        /// <param name="parentGroupedProductId">Parent product identifier (used with grouped products)</param>
        /// <param name="storeId">Store identifier; 0 to load all records</param>
        /// <param name="vendorId">Vendor identifier; 0 to load all records</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <param name="geoVendorIds">The list of vendor identifiers who are available according geo location</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the products
        /// </returns>
        Task<IList<Product>> GetAssociatedProductsAsync(int parentGroupedProductId,
            int storeId = 0, int vendorId = 0, bool showHidden = false, IList<int> geoVendorIds = null);

        /// <summary>
        /// Apply vendors georadius to the passed query
        /// </summary>
        /// <param name="productsQuery">Query to filter</param>
        /// <param name="geoVendorIds">geoVendorIds</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the filtered query
        /// </returns>
        Task<IQueryable<Product>> ApplyVendorsGeoRadiusFilter(IQueryable<Product> productsQuery, IList<int> geoVendorIds);

        #endregion

        #region Alchub

        /// <summary>
        /// Get master products by upc code array.
        /// </summary>
        /// <param name="upcCodeArray"></param>
        /// <returns></returns>
        Task<IList<Product>> GetMasterProductsByUPCCodeListAsync(string[] upcCodeArray);

        Task<bool> IsProductUPCCodeExist(string upcCode = "");

        /// <summary>
        /// Get reformated product name.
        /// </summary>
        /// <param name="product"></param>
        /// <param name="sci"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        Task<string> GetProductItemName(Product product, ShoppingCartItem sci);

        /// <summary>
        /// Get reformated product name.
        /// </summary>
        /// <param name="product"></param>
        /// <param name="orderItem"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        Task<string> GetProductItemName(Product product, OrderItem orderItem);

        /// <summary>
        /// Get product average price by master product upc code
        /// </summary>
        /// <param name="upcCode"></param>
        /// <returns></returns>
        Task<decimal> GetProductAveragePriceByUPCCode(string upcCode);

        /// <summary>
        /// Search master product - for sync catalog system
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="showHidden"></param>
        /// <param name="createdOrUpdatedFromUtc"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        Task<IPagedList<Product>> SearchMasterProductsAsync(
            int storeId = 0,
            bool showHidden = false,
            DateTime? createdOrUpdatedFromUtc = null,
            int pageIndex = 0,
            int pageSize = int.MaxValue);
        /// <summary>
        /// Get all undeleted products(specifically for Scheduler to update images) 
        /// </summary>
        /// <returns></returns>
        Task<IList<Product>> GetAllProductsAsync();
        #endregion

        #region Related products

        /// <summary>
        /// Gets related products by product identifier
        /// </summary>
        /// <param name="productId1">The first product identifier</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <param name="isMaster">A value indicating whether to show only master product records</param>
        /// <param name="geoVendorIds">The list of vendor identifiers who are available according geo location</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the related products
        /// </returns>
        Task<IList<RelatedProduct>> GetRelatedProductsByProductId1Async(int productId1, bool showHidden = false, bool? isMaster = null, IList<int> geoVendorIds = null);

        /// <summary>
        /// Gets master products by UPCCode array
        /// </summary>
        /// <param name="upcArray">Upc array</param>
        /// <param name="vendorId">Vendor ID; 0 to load all records</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the products
        /// </returns>
        Task<IList<Product>> GetMasterProductsByUPCCodeAsync(string[] upcArray, int vendorId = 0);

        Task<IList<Product>> GetMasterProductsBySKUCodeAsync(string[] upcArray, int vendorId = 0);

        #endregion

        /// <summary>
        /// Gets a vendor product
        /// </summary>
        /// <param name="vendorId"></param>
        /// <param name="storeId"></param>
        /// <param name="upcCode"></param>
        /// <param name="sku"></param>
        /// <returns></returns>
        Task<Product> GetVendorProduct(int vendorId, int storeId = 0, string upcCode = null, string sku = null);

        #region Helpers

        /// <summary>
        /// Auto generate master Product SKU
        /// </summary>
        /// <returns></returns>
        Task<string> GenerateMasterProductSKU();

        #endregion

        #region Elastic Search
        /// <summary>
        /// To get all the data of the product table
        /// </summary>
        Task<IList<Product>> GetProductsFromDatabaseAsync();
        #endregion
    }
}