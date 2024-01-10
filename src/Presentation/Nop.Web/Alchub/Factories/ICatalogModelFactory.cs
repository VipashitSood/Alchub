using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml.Linq;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Vendors;
using Nop.Web.Alchub.Models.Catalog;
using Nop.Web.Models.Catalog;

namespace Nop.Web.Factories
{
    public partial interface ICatalogModelFactory
    {
        #region Searching

        /// <summary>
        /// Prepares the search products model
        /// </summary>
        /// <param name="model">Search model</param>
        /// <param name="command">Model to get the catalog products</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the search products model
        /// </returns>
        Task<CatalogProductsModel> PrepareSearchProductsModelAsync(SearchModel searchModel, CatalogProductsCommand command);

        #endregion

        Task<CatalogProductsModel> PrepareCustomSearchProductsModelAsync(SearchModel searchModel, CatalogProductsCommand command);

        #region Manufacturers

        /// <summary>
        /// Prepare manufacturers models
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the list of manufacturer models
        /// </returns>
        Task<List<ManufacturerModel>> PrepareManufacturersModelsAsync(int pageIndex = 0, int pageSize = int.MaxValue);

        #endregion

        #region Elastic Search
        /// <summary>
        /// Search product Via Elastic search
        /// </summary>
        /// <param name="keyword"></param>
        /// <returns>List of Product search overview model</returns>
        //Task<List<ProductSearchOverviewModel>> MasterProductsAjaxAsync(string keyword);
        Task<List<ProductSearchOverviewModel>> PrepareProductSearchOverviewModelsElasticAsync(string keyword, int pageSize = int.MaxValue, bool visibleIndividuallyOnly = false,
           bool isMaster = true, int languageId = 0, IList<int> geoVendorIds = null);

        /// <summary>
        /// Prepare search model
        /// </summary>
        /// <param name="model">Search model</param>
        /// <param name="command">Model to get the catalog products</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the search model
        /// </returns>
        Task<SearchModel> PrepareElasticSearchModelAsync(SearchModel model, CatalogProductsCommand command);

        /// <summary>
        /// Prepare catalog product model
        /// </summary>
        /// <param name="searchModel"></param>
        /// <param name="command"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        Task<CatalogProductsModel> PrepareElasticCatalogModelAsync(SearchModel searchModel, CatalogProductsCommand command);

        #endregion
    }
}