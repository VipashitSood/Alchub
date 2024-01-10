using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core.Domain.Catalog;

namespace Nop.Services.Alchub.General
{
    /// <summary>
    /// Alchub service
    /// </summary>
    public interface IAlchubSameNameProductService
    {
        #region Products

        /// <summary>
        ///  Gets all master with same name
        /// </summary>
        /// <returns></returns>
        Task<IList<Product>> GetAllMasterProductSameByNameAsync();

        /// <summary>
        /// Same name of products to create group product
        /// </summary>
        /// <param name="products"></param>
        /// <returns></returns>
        Task<bool> SameProductNameCreateGroupProductsAsync(IList<Product> products);

        /// <summary>
        ///  Get All group Products
        /// </summary>
        /// <returns></returns>
        Task<bool> ReArrangeAllGroupProductsRelatedItemsAsync();

        #endregion

        #region Assemble group product

        /// <summary>
        /// Assemble group produts, which has same Variant specification attribute & has same manufacturer(brand)
        /// </summary>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task<bool> AssembleGroupProductsAsync();

        /// <summary>
        /// Rearrange sizes and containers of related products within all group products
        /// </summary>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task<bool> ReArrangeAllGroupProductsAssociatedProductsAsync();

        /// <summary>
        /// Get grouped products variants disctionary.
        /// </summary>
        /// <param name="groupedProducts"></param>
        /// <param name="manufacturerId"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        Task<IDictionary<Product, string>> GetGroupedProductVariants(Product groupedProducts, int manufacturerId);

        #endregion
    }
}
