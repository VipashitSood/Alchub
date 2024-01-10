using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Vendors;

namespace Nop.Services.Alchub.General
{
    /// <summary>
    /// Alchub service
    /// </summary>
    public interface IAlchubGeneralService
    {
        #region Products

        /// <summary>
        /// Gets a master product by upcCode
        /// </summary>
        /// <param name="upcCode">UPCCode</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the product
        /// </returns>
        Task<Product> GetMasterProductByUpcCodeAsync(string upcCode);

        /// <summary>
        /// Gets a products by upcCode
        /// </summary>
        /// <param name="upcCode">UPCCode</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the product
        /// </returns>
        Task<IList<Product>> GetProductsByUpcCodeAsync(string upcCode, bool excludeMasterProduct = false, bool? showPublished = null);

        /// <summary>
        /// Gets a products pricerange details
        /// </summary>
        /// <param name="product"></param>
        /// <param name="customer"></param>
        /// <returns></returns>
        Task<IDictionary<int, decimal>> GetProductPriceRangeAsync(Product product, Customer customer = null);

        /// <summary>
        /// Get the product szies
        /// </summary>
        /// <param name="product"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        Task<IList<string>> GetProductSizesAsync();

        /// <summary>
        /// Get the product containers
        /// </summary>
        /// <param name="product"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        Task<IList<string>> GetProductContainersAsync(Product product);

        /// <summary>
        /// Gets the default variant of the grouped product
        /// </summary>
        /// <param name="associatedProducts"></param>
        /// <returns>product</returns>
        Task<Product> GetGroupedProductDefaultVariantAsync(IList<Product> associatedProducts);

        #endregion

        #region Vendor

        /// <summary>
        /// Gets a vendor by master product identifier
        /// </summary>
        /// <param name="masterProduct">Product</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the vendor
        /// </returns>
        Task<IList<Vendor>> GetVendorByMasterProductIdAsync(Product masterProduct);

        /// <summary>
        /// Gets a vendors by master product identifiers
        /// </summary>
        /// <param name="masterProducts">List of product</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the vendors
        /// </returns>
        Task<IList<Vendor>> GetVendorsByMasterProductIdsAsync(IList<Product> masterProducts);

        #endregion
    }
}
