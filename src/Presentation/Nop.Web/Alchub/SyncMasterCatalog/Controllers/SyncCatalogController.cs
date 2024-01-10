using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Alchub.Domain;
using Nop.Core.Domain.Logging;
using Nop.Services.Logging;
using Nop.Web.Alchub.SyncMasterCatalog.Factories;
using Nop.Web.Alchub.SyncMasterCatalog.Models;

namespace Nop.Web.Alchub.SyncMasterCatalog.Controllers
{
    /// <summary>
    /// Represent sync catalog controller
    /// </summary>
    public class SyncCatalogController : SyncCatalogBaseApiController
    {
        #region Fields

        private readonly ILogger _logger;
        private readonly ISyncCatalogFactory _syncCatalogFactory;
        private readonly AlchubSettings _alchubSettings;

        #endregion

        #region Ctor

        public SyncCatalogController(ILogger logger,
            ISyncCatalogFactory syncCatalogFactory,
            AlchubSettings alchubSettings)
        {
            _logger = logger;
            _syncCatalogFactory = syncCatalogFactory;
            _alchubSettings = alchubSettings;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Log serverd data information
        /// </summary>
        /// <param name="catalodData"></param>
        /// <param name="domainUrl"></param>
        /// <param name="fromDateTime"></param>
        /// <returns></returns>
        private async Task LogServedInformation(CatalogDataModel catalodData, DateTime processStartTime, string domainUrl, DateTime? fromDateTime = null)
        {
            var message = $"Sync Catalog API served data to \"{domainUrl}\"";
            var fullMessage = new StringBuilder();

            //start - end timeing
            TimeSpan timeSpan = DateTime.Now.Subtract(processStartTime);
            var totalTime = string.Format("{0:D2}h:{1:D2}m:{2:D2}s:{3:D3}ms",
                timeSpan.Hours,
                timeSpan.Minutes,
                timeSpan.Seconds,
                timeSpan.Milliseconds);

            fullMessage.AppendLine($"Start at: {processStartTime.ToString()}, End at: {DateTime.Now.ToString()}, Total run time: {totalTime}\n");
            if (fromDateTime.HasValue)
            {
                //prepare all data which was served.
                if (catalodData.Categories.Any())
                {
                    var list = new List<string>();
                    foreach (var categoryModel in catalodData.Categories)
                        list.Add($"{categoryModel.Category.Id}--{categoryModel.Category.Name}");
                    fullMessage.AppendLine($"Categories: ({catalodData.Categories.Count()}) \n{string.Join(", ", list)}");
                }
                if (catalodData.Manufacturers.Any())
                {
                    fullMessage.AppendLine();
                    var list = new List<string>();
                    foreach (var manufactureModel in catalodData.Manufacturers)
                        list.Add($"{manufactureModel.Manufacturer.Id}--{manufactureModel.Manufacturer.Name}");
                    fullMessage.AppendLine($"Manufactures: ({catalodData.Manufacturers.Count()}) \n{string.Join(", ", list)}");
                }
                if (catalodData.SpecificationAttributes.Any())
                {
                    fullMessage.AppendLine();
                    var list = new List<string>();
                    foreach (var spec in catalodData.SpecificationAttributes)
                        list.Add($"{spec.Id}--{spec.Name}");
                    fullMessage.AppendLine($"Specifications: ({catalodData.SpecificationAttributes.Count()}) \n{string.Join(", ", list)}");
                }
                if (catalodData.MasterProducts.Any())
                {
                    fullMessage.AppendLine();
                    var list = new List<string>();
                    foreach (var product in catalodData.MasterProducts)
                        list.Add($"{product.Id}--{product.Name}");
                    fullMessage.AppendLine($"Products: ({catalodData.MasterProducts.Count()}) \n{string.Join(", ", list)}");
                }
            }
            else
            {
                //prepare data count only.
                fullMessage.AppendLine($"Total Categories: {catalodData.Categories.Count()}");
                fullMessage.AppendLine($"Total Manufactures: {catalodData.Manufacturers.Count()}");
                fullMessage.AppendLine($"Total Specification att: {catalodData.SpecificationAttributes.Count()}");
                fullMessage.AppendLine($"Total Products: {catalodData.MasterProducts.Count()}");
            }

            await _logger.InsertLogAsync(LogLevel.Information, message, fullMessage?.ToString());
        }

        #endregion

        #region Methods

        /// <summary>
        /// Test get request
        /// </summary>
        /// <param name="test"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("/sync-catalog-api/index", Name = "SyncCatalogAPI_Test")]
        public SyncCatalogBaseResponseModel SyncCatalogAPI_Test([FromQuery] string test)
        {
            if (test == null)
                return ErrorResponse("test Param missing", System.Net.HttpStatusCode.BadRequest);

            return SuccessResponse("Ok", data: test);
        }

        /// <summary>
        /// Get catalog all data
        /// </summary>
        /// <param name="domainUrl"></param>
        /// <param name="fromDateTimeUtc"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("/sync-catalog-api/catalog", Name = "SyncCatalogAPI_GetCatalog")]
        public async Task<SyncCatalogBaseResponseModel> GetCatalog([FromQuery] string domainUrl, string fromDateTimeUtc)
        {
            if (string.IsNullOrEmpty(domainUrl))
                return ErrorResponse("invalid domainUrl");

            //assign process start time
            var processStartTime = DateTime.Now;

            //prepare required param
            bool loadCatalogAllData = false;
            if (string.IsNullOrEmpty(fromDateTimeUtc))
                loadCatalogAllData = true;

            //validate fromdate
            DateTime? fromDateTime = null;
            if (!loadCatalogAllData)
            {
                //validate passed date in correct format.
                if (!DateTime.TryParse(fromDateTimeUtc, out var covertedFromDateTime))
                {
                    return ErrorResponse("Invalid fromDateTimeUtc string");
                }
                //assign date after convert.
                fromDateTime = covertedFromDateTime;
            }

            try
            {
                //prepare data
                var catalodData = await _syncCatalogFactory.PrepareCatalogDataModel(domainUrl, fromDateTime);

                //log information 
                if (_alchubSettings.LogServedDataInformation)
                    await LogServedInformation(catalodData, processStartTime, domainUrl, fromDateTime);

                //success
                return SuccessResponse("Catalog data", data: catalodData);
            }
            catch (Exception ex)
            {
                //log the error with domain name & send error response.
                var message = $"Sync catalog error, requested domain:{domainUrl}";
                await _logger.ErrorAsync(message, ex);
                return ErrorResponse(message, HttpStatusCode.InternalServerError, new List<string>() { ex.ToString() });
            }
        }

        /// <summary>
        /// Get catalog all data using pagination
        /// </summary>
        /// <param name="domainUrl"></param>
        /// <param name="fromDateTimeUtc"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("/sync-catalog-api/catalog/products_using_pagination", Name = "SyncCatalogAPI_GetCatalog_Using_Pagination")]
        public async Task<SyncCatalogBaseResponseModel> GetCatalog([FromQuery] string domainUrl, string fromDateTimeUtc, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            if (string.IsNullOrEmpty(domainUrl))
                return ErrorResponse("invalid domainUrl");

            //assign process start time
            var processStartTime = DateTime.Now;

            //prepare required param
            bool loadCatalogAllData = false;
            if (string.IsNullOrEmpty(fromDateTimeUtc))
                loadCatalogAllData = true;

            //validate fromdate
            DateTime? fromDateTime = null;
            if (!loadCatalogAllData)
            {
                //validate passed date in correct format.
                if (!DateTime.TryParse(fromDateTimeUtc, out var covertedFromDateTime))
                {
                    return ErrorResponse("Invalid fromDateTimeUtc string");
                }
                //assign date after convert.
                fromDateTime = covertedFromDateTime;
            }

            try
            {
                //prepare data
                var catalodData = await _syncCatalogFactory.PrepareCatalogDataModelUsingPagination(domainUrl, fromDateTime, pageIndex, pageSize);

                //log information 
                if (_alchubSettings.LogServedDataInformation)
                    await LogServedInformation(catalodData, processStartTime, domainUrl, fromDateTime);

                //success
                return SuccessResponse("Catalog data", data: catalodData);
            }
            catch (Exception ex)
            {
                //log the error with domain name & send error response.
                var message = $"Sync catalog error, requested domain:{domainUrl}";
                await _logger.ErrorAsync(message, ex);
                return ErrorResponse(message, HttpStatusCode.InternalServerError, new List<string>() { ex.ToString() });
            }
        }

        /// <summary>
        /// Get master product data
        /// </summary>
        /// <param name="domainUrl"></param>
        /// <param name="productUpcs"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("/sync-catalog-api/catalog/products_by_upcs", Name = "SyncCatalogAPI_GetCatalogByProductUpcs")]
        public async Task<SyncCatalogBaseResponseModel> GetMasterProductsByUpcList([FromQuery] string domainUrl, [FromBody] string upcs)
        {
            if (string.IsNullOrEmpty(domainUrl))
                return ErrorResponse("invalid domainUrl");

            if (string.IsNullOrEmpty(upcs))
                return ErrorResponse("invalid productUpcs");

            //assign process start time
            var processStartTime = DateTime.Now;

            try
            {
                var productUpcList = upcs.Split(",");
                //prepare catalog data models
                var catalogData = await _syncCatalogFactory.PrepareCatalogDataModel(productUpcList);

                //log information 
                if (_alchubSettings.LogServedDataInformation)
                    await LogServedInformation(catalogData, processStartTime, domainUrl);

                //success
                return SuccessResponse("Catalog data", data: catalogData);
            }
            catch (Exception ex)
            {
                //log the error with domain name & send error response.
                var message = $"Sync catalog error, requested domain:{domainUrl}";
                await _logger.ErrorAsync(message, ex);
                return ErrorResponse(message, HttpStatusCode.InternalServerError, new List<string>() { ex.ToString() });
            }
        }

        #endregion
    }
}
