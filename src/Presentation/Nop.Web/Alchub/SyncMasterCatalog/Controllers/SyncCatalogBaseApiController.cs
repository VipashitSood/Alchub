using System.Collections.Generic;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Nop.Web.Alchub.SyncMasterCatalog.Authorization;
using Nop.Web.Alchub.SyncMasterCatalog.Models;

namespace Nop.Web.Alchub.SyncMasterCatalog.Controllers
{
    /// <summary>
    /// Represent sync catalog base api controller
    /// </summary>
    [AthorizeSyncCatalog]
    public class SyncCatalogBaseApiController : Controller
    {
        /// <summary>
        /// Access denied for unothrized request
        /// </summary>
        /// <returns></returns>
        protected IActionResult AccessDenied()
        {
            return new StatusCodeResult(Microsoft.AspNetCore.Http.StatusCodes.Status403Forbidden);
        }

        /// <summary>
        /// Return response for error
        /// </summary>
        /// <param name="message"></param>
        /// <param name="statusCode"></param>
        /// <param name="errors"></param>
        /// <returns></returns>
        [NonAction]
        public SyncCatalogBaseResponseModel ErrorResponse(string message = "", HttpStatusCode statusCode = HttpStatusCode.BadRequest, List<string> errors = null)
        {
            var response = new SyncCatalogBaseResponseModel
            {
                StatusCode = (int)statusCode,
                Message = message,
                Errors = errors
            };

            //response.Errors.Add(message);
            return response;
        }

        /// <summary>
        /// Return response for success
        /// </summary>
        /// <param name="message"></param>
        /// <param name="data"></param>
        /// <param name="statusCode"></param>
        /// <returns></returns>
        [NonAction]
        public SyncCatalogBaseResponseModel SuccessResponse(string message = "", HttpStatusCode statusCode = HttpStatusCode.OK, object data = null)
        {
            return new SyncCatalogBaseResponseModel
            {
                StatusCode = (int)statusCode,
                Message = message,
                Data = data == null ? new List<object>() : data
            };
        }
    }
}
