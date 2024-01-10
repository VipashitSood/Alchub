using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Nop.Core;
using Nop.Core.Domain.Security;
using Nop.Core.Domain.Stores;
using Nop.Plugin.Api.DTO.Errors;
using Nop.Plugin.Api.JSON.ActionResults;
using Nop.Plugin.Api.JSON.Serializers;
using Nop.Plugin.Api.Models.BaseModels;
using Nop.Services.Customers;
using Nop.Services.Discounts;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Security;
using Nop.Services.Stores;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Nop.Plugin.Api.Controllers
{
    [Authorize(Policy = JwtBearerDefaults.AuthenticationScheme, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ApiController]
    public class BaseApiController : Controller
    {
        protected readonly IAclService _aclService;
        protected readonly ICustomerActivityService _customerActivityService;
        protected readonly ICustomerService _customerService;
        protected readonly IDiscountService _discountService;
        protected readonly IJsonFieldsSerializer _jsonFieldsSerializer;
        protected readonly ILocalizationService _localizationService;
        protected readonly IPictureService _pictureService;
        protected readonly IStoreMappingService _storeMappingService;
        protected readonly IStoreService _storeService;

        public BaseApiController(
            IJsonFieldsSerializer jsonFieldsSerializer,
            IAclService aclService,
            ICustomerService customerService,
            IStoreMappingService storeMappingService,
            IStoreService storeService,
            IDiscountService discountService,
            ICustomerActivityService customerActivityService,
            ILocalizationService localizationService,
            IPictureService pictureService)
        {
            _jsonFieldsSerializer = jsonFieldsSerializer;
            _aclService = aclService;
            _customerService = customerService;
            _storeMappingService = storeMappingService;
            _storeService = storeService;
            _discountService = discountService;
            _customerActivityService = customerActivityService;
            _localizationService = localizationService;
            _pictureService = pictureService;
        }

        protected IActionResult Error(HttpStatusCode statusCode = (HttpStatusCode)422, string propertyKey = "", string errorMessage = "")
        {
            var errors = new Dictionary<string, List<string>>();

            if (!string.IsNullOrEmpty(errorMessage) && !string.IsNullOrEmpty(propertyKey))
            {
                var errorsList = new List<string>
                                 {
                                     errorMessage
                                 };
                errors.Add(propertyKey, errorsList);
            }

            foreach (var item in ModelState)
            {
                var errorMessages = item.Value.Errors.Select(x => x.ErrorMessage);

                var validErrorMessages = new List<string>();

                validErrorMessages.AddRange(errorMessages.Where(message => !string.IsNullOrEmpty(message)));

                if (validErrorMessages.Count > 0)
                {
                    if (errors.ContainsKey(item.Key))
                    {
                        errors[item.Key].AddRange(validErrorMessages);
                    }
                    else
                    {
                        errors.Add(item.Key, validErrorMessages.ToList());
                    }
                }
            }

            var errorsRootObject = new ErrorsRootObject
            {
                Errors = errors
            };

            var errorsJson = _jsonFieldsSerializer.Serialize(errorsRootObject, null);

            return new ErrorActionResult(errorsJson, statusCode);
        }

        protected IActionResult AccessDenied()
        {
            return new StatusCodeResult(Microsoft.AspNetCore.Http.StatusCodes.Status403Forbidden);
        }

        protected async Task UpdateAclRolesAsync<TEntity>(TEntity entity, List<int> passedRoleIds) where TEntity : BaseEntity, IAclSupported
        {
            if (passedRoleIds == null)
            {
                return;
            }

            entity.SubjectToAcl = passedRoleIds.Any();

            var existingAclRecords = await _aclService.GetAclRecordsAsync(entity);
            var allCustomerRoles = await _customerService.GetAllCustomerRolesAsync(true);
            foreach (var customerRole in allCustomerRoles)
            {
                if (passedRoleIds.Contains(customerRole.Id))
                {
                    //new role
                    if (existingAclRecords.Count(acl => acl.CustomerRoleId == customerRole.Id) == 0)
                    {
                        await _aclService.InsertAclRecordAsync(entity, customerRole.Id);
                    }
                }
                else
                {
                    //remove role
                    var aclRecordToDelete = existingAclRecords.FirstOrDefault(acl => acl.CustomerRoleId == customerRole.Id);
                    if (aclRecordToDelete != null)
                    {
                        await _aclService.DeleteAclRecordAsync(aclRecordToDelete);
                    }
                }
            }
        }

        protected async Task UpdateStoreMappingsAsync<TEntity>(TEntity entity, List<int> passedStoreIds) where TEntity : BaseEntity, IStoreMappingSupported
        {
            if (passedStoreIds == null)
            {
                return;
            }

            entity.LimitedToStores = passedStoreIds.Any();

            var existingStoreMappings = await _storeMappingService.GetStoreMappingsAsync(entity);
            var allStores = await _storeService.GetAllStoresAsync();
            foreach (var store in allStores)
            {
                if (passedStoreIds.Contains(store.Id))
                {
                    //new store
                    if (existingStoreMappings.Count(sm => sm.StoreId == store.Id) == 0)
                    {
                        await _storeMappingService.InsertStoreMappingAsync(entity, store.Id);
                    }
                }
                else
                {
                    //remove store
                    var storeMappingToDelete = existingStoreMappings.FirstOrDefault(sm => sm.StoreId == store.Id);
                    if (storeMappingToDelete != null)
                    {
                        await _storeMappingService.DeleteStoreMappingAsync(storeMappingToDelete);
                    }
                }
            }
        }

        /// <summary>
        /// Return response for invalid model
        /// </summary>
        /// <param name="modelState"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        [NonAction]
        public BaseResponseModel InvalidateModelResponse(ModelStateDictionary modelState, string message)
        {
            return new BaseResponseModel
            {
                StatusCode = (int)HttpStatusCode.NoContent,
                Message = message,
                Errors = modelState.Select(e => e.Value.Errors.Select(y => y.ErrorMessage).FirstOrDefault()).ToList(),
                Data = null
            };
        }

        /// <summary>
        /// Return response for error
        /// </summary>
        /// <param name="message"></param>
        /// <param name="stausCode"></param>
        /// <returns></returns>
        [NonAction]
        public BaseResponseModel ErrorResponse(string message = "", HttpStatusCode statusCode = HttpStatusCode.BadRequest, List<string> Errors = null)
        {
            var response = new BaseResponseModel
            {
                StatusCode = (int)statusCode,
                Message = message,
                Errors = Errors
            };

            //response.Errors.Add(message);
            return response;
        }

        /// <summary>
        /// Return response for success
        /// </summary>
        /// <param name="message"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        [NonAction]
        public BaseResponseModel SuccessResponse(string message = "", object data = null, HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            return new BaseResponseModel
            {
                StatusCode = (int)statusCode,
                Message = message,
                Data = data == null ? new List<object>() : data
            };
        }
    }
}
