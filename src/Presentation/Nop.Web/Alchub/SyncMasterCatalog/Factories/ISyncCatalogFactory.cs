using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Web.Alchub.SyncMasterCatalog.Models;

namespace Nop.Web.Alchub.SyncMasterCatalog.Factories
{
    public interface ISyncCatalogFactory
    {
        /// <summary>
        /// Prepare catalog data model
        /// </summary>
        /// <param name="domainUrl"></param>
        /// <param name="fromDateTime"></param>
        /// <returns></returns>
        Task<CatalogDataModel> PrepareCatalogDataModel(string domainUrl, DateTime? fromDateTime);

        /// <summary>
        /// Prepare catalog data model using pagination
        /// </summary>
        /// <param name="domainUrl"></param>
        /// <param name="fromDateTime"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        Task<CatalogDataModel> PrepareCatalogDataModelUsingPagination(string domainUrl, DateTime? fromDateTime, int pageIndex = 0, int pageSize = int.MaxValue);

        /// <summary>
        /// Prepare catalog data model
        /// </summary>
        /// <param name="upcCodeList"></param>
        /// <returns></returns>
        Task<CatalogDataModel> PrepareCatalogDataModel(string[] upcCodeList);
    }
}
