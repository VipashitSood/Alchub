using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Catalog;

namespace Nop.Services.Alchub.ElasticSearch
{
    public partial interface IElasticsearchManagerService
    {
        /// <summary>
        /// Returns list of master products and total no of master products
        /// </summary>
        /// <param name="keywords"></param>
        /// <param name="pageNumber"></param>
        /// <param name="categoryIds"></param>
        /// <param name="manufacturerIds"></param>
        /// <param name="geoVendorIds"></param>
        /// <param name="filteredSpecOptions"></param>
        /// <param name="priceMin"></param>
        /// <param name="priceMax"></param>
        /// <param name="orderBy"></param>
        /// <returns></returns>
        Task<Master_products_result> GetMasterProductsListViaElasticSearchAsync(
           string keywords = null,
           int pageNumber = 0,
           int pageSize = int.MaxValue,
           int storeId = 0,
           bool visibleIndividuallyOnly = false,
           bool? isMaster = true,
           IList<int> categoryIds = null,
           IList<int> manufacturerIds = null,
           IList<int> geoVendorIds = null,
           IList<int> selectedVendorIds = null,
           IList<int> filteredSpecOptions = null,
           decimal? priceMin = null,
           decimal? priceMax = null,
           ProductSortingEnum orderBy = ProductSortingEnum.Position,
           int parentCategoryId = 0,
           IList<(int, double)> associateProductIds = null);

        /// <summary>
        /// Returns list of first 10 products that matches the query
        /// </summary>
        /// <param name="keyword">Keyword to be searched</param>
        /// <returns></returns>
        Task<List<Master_products>> GetSearchAutoCompleteProductsAsync(string keyword, int pageSize = int.MaxValue, bool visibleIndividuallyOnly = false,
           bool isMaster = true, int languageId = 0, IList<int> geoVendorIds = null, IList<(int, double)> associationProductIds = null);

        /// <summary>
        /// Returns list of master products and total no of master products
        /// </summary>
        /// <param name="keywords"></param>
        /// <param name="pageIndex"></param>
        /// <param name="categoryIds"></param>
        /// <param name="manufacturerIds"></param>
        /// <param name="geoVendorIds"></param>
        /// <param name="filteredSpecOptions"></param>
        /// <param name="priceMin"></param>
        /// <param name="priceMax"></param>
        /// <param name="orderBy"></param>
        /// <returns></returns>
        Task<(IPagedList<Product> pagedProducts, Master_products_result elasticResponse)> SearchProducts(
            string keywords = null,
           int pageNumber = 0,
           int pageSize = int.MaxValue,
           int storeId = 0,
           bool visibleIndividuallyOnly = false,
           bool? isMaster = true,
           IList<int> categoryIds = null,
           IList<int> manufacturerIds = null,
           IList<int> geoVendorIds = null,
           IList<int> selectedVendorIds = null,
           IList<int> filteredSpecOptions = null,
           decimal? priceMin = null,
           decimal? priceMax = null,
           ProductSortingEnum orderBy = ProductSortingEnum.Position,
           int parentCategoryId = 0,
           IList<(int, double)> associateProductIds = null);

        /// <summary>
        /// Search product using elastic
        /// </summary>
        /// <param name="keyword"></param>
        /// <param name="pageSize"></param>
        /// <param name="visibleIndividuallyOnly"></param>
        /// <param name="isMaster"></param>
        /// <param name="languageId"></param>
        /// <param name="geoVendorIds"></param>
        /// <returns></returns>
        Task<IPagedList<Product>> SearchAutoCompleteProductsAsync(
           string keyword,
           int pageSize = int.MaxValue,
           bool visibleIndividuallyOnly = false,
           bool isMaster = true,
           int languageId = 0,
           IList<int> geoVendorIds = null,
           IList<(int, double)> associationProductIds = null);

        Task<List<(int, double)>> GetAssociateProductIdsListViaElasticSearchAsync(
          string keyword,
          int pageSize = int.MaxValue,
          bool visibleIndividuallyOnly = false,
          bool isMaster = true,
          int languageId = 0,
          IList<int> geoVendorIds = null);

        Task<List<(int, double)>> GetAssociateProductsListViaElasticSearchAsync(
           string keywords = null,
           int storeId = 0,
           bool visibleIndividuallyOnly = false,
           bool? isMaster = true,
           IList<int> categoryIds = null,
           IList<int> manufacturerIds = null,
           IList<int> geoVendorIds = null,
           IList<int> selectedVendorIds = null,
           IList<int> filteredSpecOptions = null,
           decimal? priceMin = null,
           decimal? priceMax = null,
           int parentCategoryId = 0);
    }
}
