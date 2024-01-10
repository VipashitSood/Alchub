using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Alchub.Domain.Catalog;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;

namespace Nop.Services.Alchub.Catalog
{
    /// <summary>
    /// Represents a ProductFriendlyUpc interface
    /// </summary>
    public partial interface IProductFriendlyUpcService
    {
        /// <summary>
        /// Insert a product friendly upc code
        /// </summary>
        /// <param name="productFriendlyUpcCode"></param>
        /// <returns></returns>
        Task InsertProductFriendlyUpcCodeAsync(ProductFriendlyUpcCode productFriendlyUpcCode);

        /// <summary>
        /// Update a product friendly upc code
        /// </summary>
        /// <param name="productFriendlyUpcCode"></param>
        /// <returns></returns>
        Task UpdateProductFriendlyUpcCodeAsync(ProductFriendlyUpcCode productFriendlyUpcCode);

        /// <summary>
        /// Delete a product friendly upc code
        /// </summary>
        /// <param name="productFriendlyUpcCode"></param>
        /// <returns></returns>
        Task DeleteProductFriendlyUpcCodeAsync(ProductFriendlyUpcCode productFriendlyUpcCode);

        /// <summary>
        /// Get a product friendly upc code by identifier
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<ProductFriendlyUpcCode> GetProductFriendlyUpcCodeByIdAsync(int id);

        /// <summary>
        /// Get a product friendly upc codes
        /// </summary>
        /// <param name="vendorId"></param>
        /// <param name="masterProductId"></param>
        /// <returns></returns>
        Task<IList<ProductFriendlyUpcCode>> GetProductFriendlyUpcCodesAsync(int vendorId = 0, int masterProductId = 0);

        /// <summary>
        /// Get a vendor product friendly upc code by master product idenifiers
        /// </summary>
        /// <param name="vendorId"></param>
        /// <param name="masterProductId"></param>
        /// <returns></returns>
        Task<ProductFriendlyUpcCode> GetVendorProductFriendlyUpcCodeByMasterProductIdAsync(int vendorId, int masterProductId);

        /// <summary>
        /// Get a vendor product friendly upc record by friendly upc code
        /// </summary>
        /// <param name="vendorId"></param>
        /// <param name="friendlyUpcCode"></param>
        /// <returns></returns>
        Task<ProductFriendlyUpcCode> GetVendorProductFriendlyUpcRecordByFriendlyUpcCodeAsync(int vendorId, string friendlyUpcCode);

        /// <summary>
        /// Get master product by frienly upc code
        /// </summary>
        /// <param name="vendorId"></param>
        /// <param name="friendlyUpcCode"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        Task<Product> GetMasterProductByFriendlyUPCCodeAsync(int vendorId, string friendlyUpcCode);

        /// <summary>
        /// Delete a product friendly upc code mapping
        /// </summary>
        /// <param name="productFriendlyUpcCode"></param>
        /// <returns></returns>
        Task DeleteProductFriendlyUpcCodeMappingsAsync(Product masterProduct);

        /// <summary>
        /// Search master products
        /// </summary>
        /// <param name="vendorId">Vendor identifier; 0 to load all records</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="categoryIds">Category identifiers</param>
        /// <param name="manufacturerIds">Manufacturer identifiers</param>
        /// <param name="storeId">Store identifier; 0 to load all records</param>
        /// <param name="productType">Product type; 0 to load all records</param>
        /// <param name="visibleIndividuallyOnly">A values indicating whether to load only products marked as "visible individually"; "false" to load all records; "true" to load "visible individually" only</param>
        /// <param name="excludeFeaturedProducts">A value indicating whether loaded products are marked as featured (relates only to categories and manufacturers); "false" (by default) to load all records; "true" to exclude featured products from results</param>
        /// <param name="keywords">Keywords</param>
        /// <param name="searchDescriptions">A value indicating whether to search by a specified "keyword" in product descriptions</param>
        /// <param name="searchManufacturerPartNumber">A value indicating whether to search by a specified "keyword" in manufacturer part number</param>
        /// <param name="searchSku">A value indicating whether to search by a specified "keyword" in product SKU</param>
        /// <param name="languageId">Language identifier (search for text searching)</param>
        /// <param name="orderBy">Order by</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <param name="overridePublished">
        /// <param name="isMaster">A value indicating whether to show only master product records</param>
        /// <param name="friendlyUpcCode">friendly upc code</param>
        /// null - process "Published" property according to "showHidden" parameter
        /// true - load only "Published" products
        /// false - load only "Unpublished" products
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the products
        /// </returns>
        Task<IPagedList<Product>> SearchMasterProductsAsync(
            int vendorId,
            int pageIndex = 0,
            int pageSize = int.MaxValue,
            IList<int> categoryIds = null,
            IList<int> manufacturerIds = null,
            int storeId = 0,
            ProductType? productType = null,
            bool visibleIndividuallyOnly = false,
            bool excludeFeaturedProducts = false,
            string keywords = null,
            bool searchDescriptions = false,
            bool searchManufacturerPartNumber = true,
            bool searchSku = true,
            int languageId = 0,
            ProductSortingEnum orderBy = ProductSortingEnum.Position,
            bool showHidden = false,
            bool? overridePublished = null,
            bool? isMaster = null,
            string upccode = null,
            string size = null,
            Customer customer = null,
            string friendlyUpcCode = null);
    }
}
