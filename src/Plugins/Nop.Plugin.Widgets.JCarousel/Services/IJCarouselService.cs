using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Plugin.Widgets.JCarousel.Domain;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Nop.Plugin.Widgets.JCarousel.Services
{
    /// <summary>
    /// Jcarousel service interface
    /// </summary>
    public partial interface IJCarouselService
    {
        /// <summary>
        /// Get all Jcarousel Ids
        /// </summary>
        Task<IList<JCarouselLog>> GetAllJcarouselIds();

        /// <summary>
        /// Inserts a Jcarousel
        /// </summary>
        /// <param name="jCarousel">Jcarousel</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task InsertJCarouselAsync(JCarouselLog jCarousel);

        /// <summary>
        /// Delete a Jcarousel
        /// </summary>
        /// <param name="jCarousel">Jcarousel</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task DeleteJCarouselAsync(JCarouselLog jCarousel);

        /// <summary>
        /// Gets jcarousel
        /// </summary>
        /// <param name="jCarouselId">Jcarousel identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the product
        /// </returns>
        Task<JCarouselLog> GetJCarouselByIdAsync(int jCarouselId);

        /// <summary>
        /// Updates a Jcarousel
        /// </summary>
        /// <param name="jCarousel">Jcarousel</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task UpdateJCarouselAsync(JCarouselLog jCarousel);

        /// <summary>
        /// Gets all jcarousels
        /// </summary>
        /// <param name="name">JCarousel name</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the reviews
        /// </returns>
        Task<IPagedList<JCarouselLog>> GetAllJCarouselsAsync(string name = null,
                int pageIndex = 0, int pageSize = int.MaxValue,
                bool showHidden = false);

        /// <summary>
        /// Gets product jcarousel mapping collection
        /// </summary>
        /// <param name="jacrouselId">Jcarousel identifier</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the product a jcarousel mapping collection
        /// </returns>
        Task<IPagedList<ProductJCarouselMapping>> GetProductJCarouselsByJCarouselIdAsync(int jacrouselId,
            int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false);

        /// <summary>
        /// Gets a product jcarousel mapping 
        /// </summary>
        /// <param name="productJcarouselId">Product jcarousel mapping identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the product category mapping
        /// </returns>
        Task<ProductJCarouselMapping> GetProductJCarouselByIdAsync(int productJcarouselId);

        /// <summary>
        /// Updates the product jcarousel mapping 
        /// </summary>
        /// <param name="productJcarousel">>Product jcarousel mapping</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task UpdateProductJCarouselAsync(ProductJCarouselMapping productJcarousel);

        /// <summary>
        /// Deletes the product jcarousel mapping 
        /// </summary>
        /// <param name="productJcarousel">>Product jcarousel mapping</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task DeleteProductJCarouselAsync(ProductJCarouselMapping productJcarousel);

        /// <summary>
        /// Returns a ProductJcarousel that has the specified values
        /// </summary>
        /// <param name="source">Source</param>
        /// <param name="productId">Product identifier</param>
        /// <param name="jcarouselId">Jcarousel identifier</param>
        /// <returns>A ProductJcarousel that has the specified values; otherwise null</returns>
        ProductJCarouselMapping FindProductJCarousel(IList<ProductJCarouselMapping> source, int productId, int jcarouselId);

        /// <summary>
        /// Insert the product jcarousel mapping 
        /// </summary>
        /// <param name="productJcarousel">>Product jcarousel mapping</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task InsertProductJCarouselAsync(ProductJCarouselMapping productJcarousel);

        /// <summary>
        /// Get Data source of products with Jcarousel identifier 
        /// </summary>
        /// <param name="jCarouselId">Jcarousel identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the result
        /// </returns>
        IList<int> GetAllDatasource(int jCarouselId);

        /// <summary>
        /// Gets Product ids by jcarousel identifier
        /// </summary>
        /// <param name="jacrouselId">Jcarousel identifier</param>
        /// <param name="recordsToReturn">Number of records to return. 0 if you want to get all items</param>
        /// <param name="geoVendorIds">Available geo radius vendor identifiers</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the pictures
        /// </returns>
        Task<IList<Product>> GetProductByJcarouselIdAsync(int jacrouselId, IList<int> geoVendorIds = null, int takeNumberOfProducts=0);

        /// <summary>
        /// Gets a Jcarousel
        /// </summary>
        /// <param name="jcarouselId">Jcarousel identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the category
        /// </returns>
        Task<JCarouselLog> GetJcarouselByIdAsync(int jcarouselId);

        /// <summary>
        /// Check and delete the products mapping with Jcarousel identifier
        /// </summary>
        /// <param name="jcarouselId">Jcarousel identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the result
        /// </returns>
        Task DeleteProductReferenceAsync(int jcarouselId);

        /// <summary>
        /// Check if the Jcaousel name already exists
        /// </summary>
        /// <param name="name">Jcarousel name to be checked</param>
        string CheckExistingName(string name);
    }
}
