using System.IO;
using System.Threading.Tasks;
using Nop.Core.Domain.Vendors;

namespace Nop.Services.ExportImport
{
    public partial interface IImportManager
    {
        /// <summary>
        /// Imports products for vendor
        /// </summary>
        /// <param name="stream"></param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task VendorImportProductsFromXlsxAsync(Stream stream);

        /// <summary>
        /// Import master products from XLSX file
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task ImportMasterProductsFromXlsxAsync(Stream stream);

        /// <summary>
        /// Sync products from vendor ftp xlsx file
        /// </summary>
        /// <param name="file"></param>
        /// <param name="vendor"></param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task<bool> SyncVendorProductsFromFtpXlsxAsync(Vendor vendor, string file);
    }
}
