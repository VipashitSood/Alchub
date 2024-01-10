using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core.Domain.Catalog;

namespace Nop.Services.ExportImport
{
    public partial interface IExportManager
    {
        /// <summary>
        /// Export master products to XLSX
        /// </summary>
        /// <param name="products">Products</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task<byte[]> ExportMasterProductsToXlsxAsync(IEnumerable<Product> products);
    }
}
