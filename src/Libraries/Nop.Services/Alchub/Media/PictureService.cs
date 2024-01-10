using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Nop.Core;
using Nop.Core.Alchub.Domain;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Media;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Logging;
using Nop.Services.Seo;

namespace Nop.Services.Media
{
    /// <summary>
    /// Picture service
    /// </summary>
    public partial class PictureService : IPictureService
    {
        #region Const
        private const string JPEG = "jpeg";
        private const string JPG = "jpg";
        private const string PNG = "png";
        private const string WEBP = "webp";
        #endregion

        #region Fields

        private readonly IDownloadService _downloadService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly INopFileProvider _fileProvider;
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly IRepository<Picture> _pictureRepository;
        private readonly IRepository<PictureBinary> _pictureBinaryRepository;
        private readonly IRepository<ProductPicture> _productPictureRepository;
        private readonly ISettingService _settingService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IWebHelper _webHelper;
        private readonly MediaSettings _mediaSettings;
        private readonly ILogger _logger;
        private readonly AlchubSettings _alchubSettings;
        private readonly IProductService _productService;

        #endregion

        #region Ctor

        public PictureService(IDownloadService downloadService,
            IHttpContextAccessor httpContextAccessor,
            INopFileProvider fileProvider,
            IProductAttributeParser productAttributeParser,
            IRepository<Picture> pictureRepository,
            IRepository<PictureBinary> pictureBinaryRepository,
            IRepository<ProductPicture> productPictureRepository,
            ISettingService settingService,
            IUrlRecordService urlRecordService,
            IWebHelper webHelper,
            MediaSettings mediaSettings,
            ILogger logger,
            AlchubSettings alchubSettings,
            IProductService productService)
        {
            _downloadService = downloadService;
            _httpContextAccessor = httpContextAccessor;
            _fileProvider = fileProvider;
            _productAttributeParser = productAttributeParser;
            _pictureRepository = pictureRepository;
            _pictureBinaryRepository = pictureBinaryRepository;
            _productPictureRepository = productPictureRepository;
            _settingService = settingService;
            _urlRecordService = urlRecordService;
            _webHelper = webHelper;
            _mediaSettings = mediaSettings;
            _logger = logger;
            _alchubSettings = alchubSettings;
            _productService = productService;
        }

        #endregion

        #region Api call to get image extension

        /// <summary>
        /// Is file exists on cdn server
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        private async Task<bool> IsFileExists(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                return false;

            try
            {
                var client = new HttpClient();
                var request = new HttpRequestMessage(HttpMethod.Get, filePath);
                var response = await client.SendAsync(request);
                if (response.IsSuccessStatusCode)
                    return true;
                else
                    return false;
            }
            catch (Exception ex)
            {
                await _logger.InsertLogAsync(Core.Domain.Logging.LogLevel.Error, $"{"Unable to get file from CDN. error:"}{ex.Message}", ex.InnerException.ToString());
                return false;
            }
        }

        #endregion

        #region Getting picture local path/URL methods

        ///// <summary>
        ///// Get a picture URL
        ///// </summary>
        ///// <param name="picture">Reference instance of Picture</param>
        ///// <param name="targetSize">The target picture size (longest side)</param>
        ///// <param name="showDefaultPicture">A value indicating whether the default picture is shown</param>
        ///// <param name="storeLocation">Store location URL; null to use determine the current store location automatically</param>
        ///// <param name="defaultPictureType">Default picture type</param>
        ///// <returns>
        ///// A task that represents the asynchronous operation
        ///// The task result contains the picture URL
        ///// </returns>
        //public virtual async Task<(string Url, Picture Picture)> GetPictureUrlAsync(Picture picture,
        //    int targetSize = 0,
        //    bool showDefaultPicture = true,
        //    string storeLocation = null,
        //    PictureType defaultPictureType = PictureType.Entity)
        //{
        //    if (picture == null)
        //        return showDefaultPicture ? (await GetDefaultPictureUrlAsync(targetSize, defaultPictureType, storeLocation), null) : (string.Empty, (Picture)null);

        //    byte[] pictureBinary = null;
        //    if (picture.IsNew)
        //    {
        //        await DeletePictureThumbsAsync(picture);
        //        pictureBinary = await LoadPictureBinaryAsync(picture);

        //        if ((pictureBinary?.Length ?? 0) == 0)
        //            return showDefaultPicture ? (await GetDefaultPictureUrlAsync(targetSize, defaultPictureType, storeLocation), picture) : (string.Empty, picture);

        //        //we do not validate picture binary here to ensure that no exception ("Parameter is not valid") will be thrown
        //        picture = await UpdatePictureAsync(picture.Id,
        //            pictureBinary,
        //            picture.MimeType,
        //            picture.SeoFilename,
        //            picture.AltAttribute,
        //            picture.TitleAttribute,
        //            false,
        //            false);
        //    }

        //    var seoFileName = picture.SeoFilename; // = GetPictureSeName(picture.SeoFilename); //just for sure                                                 

        //    //get Upccode
        //    var pictureUpccode = await _productService.GetProductUPCCODEByPictureIdAsync(picture.Id);

        //    var lastPart = await GetFileExtensionFromMimeTypeAsync(picture.MimeType);
        //    string thumbFileName;
        //    string thumbFilePath;

        //    if (targetSize == 0)
        //    {
        //        if (string.IsNullOrEmpty(pictureUpccode))
        //        {
        //            thumbFileName = !string.IsNullOrEmpty(seoFileName)
        //            ? $"{picture.Id:0000000}_{seoFileName}.{lastPart}"
        //            : $"{picture.Id:0000000}.{lastPart}";

        //            thumbFilePath = await GetThumbLocalPathAsync(thumbFileName);
        //            if (await GeneratedThumbExistsAsync(thumbFilePath, thumbFileName))
        //                return (await GetThumbUrlAsync(thumbFileName, storeLocation), picture);
        //        }
        //        else
        //        {                   
        //            var cdnlink = _alchubSettings.AlchubPictureCDNURL;
        //            thumbFileName = $"{cdnlink}{pictureUpccode}.{lastPart}";
        //            return (thumbFileName, picture);
        //        }

        //        pictureBinary ??= await LoadPictureBinaryAsync(picture);

        //        //the named mutex helps to avoid creating the same files in different threads,
        //        //and does not decrease performance significantly, because the code is blocked only for the specific file.
        //        //you should be very careful, mutexes cannot be used in with the await operation
        //        //we can't use semaphore here, because it produces PlatformNotSupportedException exception on UNIX based systems
        //        using var mutex = new Mutex(false, thumbFileName);
        //        mutex.WaitOne();
        //        try
        //        {
        //            SaveThumbAsync(thumbFilePath, thumbFileName, picture.MimeType, pictureBinary).Wait();
        //        }
        //        finally
        //        {
        //            mutex.ReleaseMutex();
        //        }
        //    }
        //    else
        //    {
        //        if (string.IsNullOrEmpty(pictureUpccode))
        //        {

        //            thumbFileName = !string.IsNullOrEmpty(seoFileName)
        //            ? $"{picture.Id:0000000}_{seoFileName}_{targetSize}.{lastPart}"
        //            : $"{picture.Id:0000000}_{targetSize}.{lastPart}";

        //            thumbFilePath = await GetThumbLocalPathAsync(thumbFileName);
        //            if (await GeneratedThumbExistsAsync(thumbFilePath, thumbFileName))
        //                return (await GetThumbUrlAsync(thumbFileName, storeLocation), picture);
        //        }
        //        else
        //        {
        //            var cdnlink = _alchubSettings.AlchubPictureCDNURL;
        //            thumbFileName = $"{cdnlink}{pictureUpccode}.{lastPart}";
        //            return (thumbFileName, picture);
        //        }

        //        pictureBinary ??= await LoadPictureBinaryAsync(picture);

        //        //the named mutex helps to avoid creating the same files in different threads,
        //        //and does not decrease performance significantly, because the code is blocked only for the specific file.
        //        //you should be very careful, mutexes cannot be used in with the await operation
        //        //we can't use semaphore here, because it produces PlatformNotSupportedException exception on UNIX based systems
        //        using var mutex = new Mutex(false, thumbFileName);
        //        mutex.WaitOne();
        //        try
        //        {
        //            if (pictureBinary != null)
        //            {
        //                try
        //                {
        //                    using var image = SKBitmap.Decode(pictureBinary);
        //                    var format = GetImageFormatByMimeType(picture.MimeType);
        //                    pictureBinary = ImageResize(image, format, targetSize);
        //                }
        //                catch
        //                {
        //                }
        //            }

        //            SaveThumbAsync(thumbFilePath, thumbFileName, picture.MimeType, pictureBinary).Wait();
        //        }
        //        finally
        //        {
        //            mutex.ReleaseMutex();
        //        }
        //    }

        //    return (await GetThumbUrlAsync(thumbFileName, storeLocation), picture);
        //}

        /// <summary>
        /// Get product picture url.
        /// </summary>
        /// <param name="picture"></param>
        /// <param name="upcCode"></param>
        /// <param name="targetSize"></param>
        /// <param name="showDefaultPicture"></param>
        /// <param name="storeLocation"></param>
        /// <param name="defaultPictureType"></param>
        /// <returns></returns>
        public virtual async Task<string> GetProductPictureUrlAsync(string upcCode,
            int targetSize = 0,
            string storeLocation = null,
            PictureType defaultPictureType = PictureType.Entity)
        {
            if (string.IsNullOrEmpty(upcCode))
                return await GetDefaultPictureUrlAsync(targetSize, defaultPictureType, storeLocation);

            var cdnlink = _alchubSettings.AlchubPictureCDNURL;
            //first try to get picture url from cdn server using all possible extension
            if (!string.IsNullOrEmpty(upcCode))
            {
                //get possible file exts from setting.
                var possibleFileExtensionsSetting = _alchubSettings.AlchubPictureCDNPossibleFileExtensions;
                var possibleFileExtensions = possibleFileExtensionsSetting != null ? possibleFileExtensionsSetting.Split(",")?.ToList() : new List<string>();

                foreach (var posFileExt in possibleFileExtensions)
                {
                    var filePath = $"{cdnlink}{upcCode}.{posFileExt}";
                    //size
                    if (targetSize > 0)
                    {
                        //temp-width
                        if (_alchubSettings.TempAddWidthParam)
                            filePath += $"?w={targetSize}";

                        //temp-height
                        if (_alchubSettings.TempAddHeightParam)
                            filePath += $"&h={targetSize}";

                        //temp-mode
                        if (_alchubSettings.TempAddHeightParam)
                            filePath += $"&mode=stretch";

                        bool fixUrl = (filePath.Contains("&h=") || filePath.Contains("&mode=")) && !filePath.Contains($"{posFileExt}?");
                        if (fixUrl)
                            filePath = filePath.Replace($"{posFileExt}&", $"{posFileExt}?");
                    }

                    if (await IsFileExists(filePath))
                        return filePath;
                }
            }
            return await GetDefaultPictureUrlAsync(targetSize, defaultPictureType, storeLocation);
        }
        #endregion
    }
}
