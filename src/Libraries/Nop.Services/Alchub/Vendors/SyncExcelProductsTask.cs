using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Nop.Core.Alchub.Domain;
using Nop.Core.Domain.Vendors;
using Nop.Core.Infrastructure;
using Nop.Services.Common;
using Nop.Services.ExportImport;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.ScheduleTasks;
using Nop.Services.Vendors;

namespace Nop.Services.Alchub.Vendors
{
    public partial class SyncExcelProductsTask : IScheduleTask
    {
        #region Fields
        private readonly IVendorService _vendorService;
        private readonly ILogger _logger;
        private readonly ILocalizationService _localizationService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly INopFileProvider _fileProvider;
        private readonly AlchubSettings _alchubSettings;
        private readonly IImportManager _importManager;
        private readonly IWebHostEnvironment _webHostEnvironment;
        #endregion

        #region Ctor
        public SyncExcelProductsTask(IVendorService vendorService,
            ILogger logger,
            ILocalizationService localizationService,
            IGenericAttributeService genericAttributeService,
            INopFileProvider fileProvider,
            AlchubSettings alchubSettings,
            IImportManager importManager,
            IWebHostEnvironment webHostEnvironment)
        {
            _vendorService = vendorService;
            _logger = logger;
            _localizationService = localizationService;
            _genericAttributeService = genericAttributeService;
            _fileProvider = fileProvider;
            _alchubSettings = alchubSettings;
            _importManager = importManager;
            _webHostEnvironment = webHostEnvironment;
        }
        #endregion

        #region Methods

        public virtual async Task ExecuteAsync()
        {
            var ftpPath = _alchubSettings.ExcelFileFTPPath;

            if (string.IsNullOrEmpty(ftpPath))
                throw new Exception("Ftp path can't be null");

            var ftpDirectExists = _fileProvider.DirectoryExists(ftpPath);
            if (!ftpDirectExists)
                throw new Exception("Ftp path not exists");

            var vendors = (await _vendorService.GetAllVendorsAsync())?.ToList();
            foreach (var vendor in vendors)
            {
                try
                {
                    var vendorPath = await _genericAttributeService.GetAttributeAsync<string>(vendor, NopVendorDefaults.ExcelProductFTPPath);
                    if (vendorPath == null)
                    {
                        await _logger.ErrorAsync(string.Format(await _localizationService.GetResourceAsync("Alchub.SyncExcelProducts.VendorPath.Empty"), vendor.Name));
                        continue;
                    }

                    var path = _fileProvider.Combine(ftpPath, vendorPath);
                    var pathExists = _fileProvider.DirectoryExists(path);
                    if (!pathExists)
                    {
                        await _logger.ErrorAsync(string.Format(await _localizationService.
                            GetResourceAsync("Alchub.SyncExcelProducts.VendorProductsExcelFilePath.NotExists"), vendor.Name, path));

                        continue;
                    }

                    var file = _fileProvider.GetFiles(path)?.FirstOrDefault();
                    if (file == null)
                    {
                        await _logger.ErrorAsync(string.Format(await _localizationService.
                            GetResourceAsync("Alchub.SyncExcelProducts.VendorProductsExcelFile.Empty"), vendor.Name, path));

                        continue;
                    }

                    //copy file for internal purpose
                    var targetPath = _fileProvider.Combine(_webHostEnvironment.WebRootPath, "SyncedProductXlsFile");
                    var fileName = vendor.Name.Replace(" ", string.Empty) + "_" + DateTime.UtcNow.ToString("dd-MMM-yy-HH-mm-tt").Replace(" ", string.Empty);
                    FileInfo fileInfo = new FileInfo(file);
                    if (fileInfo.Exists)
                    {
                        if (_fileProvider.DirectoryExists(targetPath))
                        {
                            var filePath = targetPath + "\\" + fileName + fileInfo.Extension;
                            fileInfo.CopyTo(filePath);
                        }
                        else
                        {
                            _fileProvider.CreateDirectory(targetPath);
                            var filePath = targetPath + "\\" + fileName + fileInfo.Extension;
                            fileInfo.CopyTo(filePath);
                        }
                    }

                    //Sync vendor products
                    var isSyncedSuccessfully = await _importManager.SyncVendorProductsFromFtpXlsxAsync(vendor, file);

                    //move the file even if all products are not synced
                    var processedFilesPath = _fileProvider.Combine(_alchubSettings.ExcelFileFTPPath, "ProcessedFiles");
                    if (!_fileProvider.DirectoryExists(processedFilesPath))
                        _fileProvider.CreateDirectory(processedFilesPath);

                    if (_fileProvider.DirectoryExists(processedFilesPath))
                    {
                        var vendorArchivePath = _fileProvider.Combine(processedFilesPath, vendorPath);
                        if (!_fileProvider.DirectoryExists(vendorArchivePath))
                            _fileProvider.CreateDirectory(vendorArchivePath);

                        //move file
                        var filePath = vendorArchivePath + "\\" + fileName + fileInfo.Extension;
                        _fileProvider.FileMove(file, filePath);
                    }
                }
                catch (Exception ex)
                {
                    await _logger.ErrorAsync(string.Format("SyncVendorProduct Error occured: during sync excel products task for vendor: {0}. Error: {1}. ", vendor.Name, ex.Message), ex);
                    continue;
                }
            }
        }

        #endregion
    }
}
