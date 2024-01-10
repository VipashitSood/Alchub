using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.Api.Attributes;
using Nop.Plugin.Api.DTO.Errors;
using Nop.Plugin.Api.DTO.Languages;
using Nop.Plugin.Api.Helpers;
using Nop.Plugin.Api.JSON.ActionResults;
using Nop.Plugin.Api.JSON.Serializers;
using Nop.Plugin.Api.Models.BaseModels;
using Nop.Plugin.Api.Models.Languages;
using Nop.Plugin.Api.Services;
using Nop.Services.Authentication;
using Nop.Services.Customers;
using Nop.Services.Discounts;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Security;
using Nop.Services.Stores;

namespace Nop.Plugin.Api.Controllers
{
    public class LanguagesController : BaseApiController
    {
        private readonly IDTOHelper _dtoHelper;
        private readonly ICustomerApiService _customerApiService;
        private readonly IAuthenticationService _authenticationService;
        private readonly ILanguageService _languageService;

        public LanguagesController(
            IJsonFieldsSerializer jsonFieldsSerializer,
            IAclService aclService,
            ICustomerService customerService,
            IStoreMappingService storeMappingService,
            IStoreService storeService,
            IDiscountService discountService,
            ICustomerActivityService customerActivityService,
            ILocalizationService localizationService,
            IPictureService pictureService,
            ILanguageService languageService,
            IDTOHelper dtoHelper,
            ICustomerApiService customerApiService,
            IAuthenticationService authenticationService)
            : base(jsonFieldsSerializer,
                   aclService,
                   customerService,
                   storeMappingService,
                   storeService,
                   discountService,
                   customerActivityService,
                   localizationService,
                   pictureService)
        {
            _languageService = languageService;
            _dtoHelper = dtoHelper;
            _customerApiService = customerApiService;
            _authenticationService = authenticationService;
        }

        /// <summary>
        ///     Retrieve all languages
        /// </summary>
        /// <param name="fields">Fields from the language you want your json to contain</param>
        /// <response code="200">OK</response>
        /// <response code="401">Unauthorized</response>
        [HttpGet]
        [Route("/api/languages", Name = "GetAllLanguages")]
        [ProducesResponseType(typeof(LanguagesRootObject), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ErrorsRootObject), (int)HttpStatusCode.BadRequest)]
        [GetRequestsErrorInterceptorActionFilter]
        public async Task<BaseResponseModel> GetAllLanguages([FromQuery] int? storeId = null, [FromQuery] string fields = null)
        {
            try
            {
                // no permissions required
                var allLanguages = await _languageService.GetAllLanguagesAsync(storeId: storeId ?? 0);

                IList<LanguageDto> languagesAsDto = await allLanguages.SelectAwait(async language => await _dtoHelper.PrepareLanguageDtoAsync(language)).ToListAsync();

                var languagesRootObject = new LanguagesRootObject
                {
                    Languages = languagesAsDto
                };
                return SuccessResponse(await _localizationService.GetResourceAsync("Nop.Api.LanguageList"), languagesRootObject);
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex.Message, HttpStatusCode.BadRequest);
            }
        }

        [HttpGet]
        [Route("/api/languages/current", Name = "GetCurrentLanguage")]
        [ProducesResponseType(typeof(LanguageDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(void), (int)HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ErrorsRootObject), (int)HttpStatusCode.Unauthorized)]
        public async Task<BaseResponseModel> GetCurrentLanguage()
        {
            try
            {
                var customer = await _authenticationService.GetAuthenticatedCustomerAsync();
                if (customer is null)
                    return ErrorResponse("Customer not found", HttpStatusCode.BadRequest);
                // no permissions required
                var language = await _customerApiService.GetCustomerLanguageAsync(customer);
                if (language is null)
                    return ErrorResponse("Language not found", HttpStatusCode.BadRequest);
                var languageDto = await _dtoHelper.PrepareLanguageDtoAsync(language);
                return SuccessResponse(await _localizationService.GetResourceAsync("Nop.Api.CurrentLanguage"), languageDto);
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex.Message, HttpStatusCode.BadRequest);
            }
        }

        [HttpPost]
        [Route("/api/languages/current", Name = "SetCurrentLanguage")]
        [ProducesResponseType(typeof(void), (int)HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ErrorsRootObject), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ErrorsRootObject), (int)HttpStatusCode.Unauthorized)]
        public async Task<BaseResponseModel> SetCurrentLanguage([FromBody] SetCurrentLanguageModel model)
        {
            try
            {
                var customer = await _authenticationService.GetAuthenticatedCustomerAsync();
                if (customer is null)
                    return ErrorResponse("Customer not found", HttpStatusCode.BadRequest);
                // no permissions required
                var language = await _languageService.GetLanguageByIdAsync(model.Id);
                if (language is null)
                    return ErrorResponse("Language not found", HttpStatusCode.BadRequest);
                await _customerApiService.SetCustomerLanguageAsync(customer, language);
                return SuccessResponse(await _localizationService.GetResourceAsync("Nop.Api.LanguageIsUpdated"), model);
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex.Message, HttpStatusCode.BadRequest);
            }
        }

        #region Resources

        [HttpGet]
        [Route("/api/languages/resources", Name = "GetLanguageResources")]
        [ProducesResponseType(typeof(LocalResourceDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(void), (int)HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ErrorsRootObject), (int)HttpStatusCode.Unauthorized)]
        public async Task<BaseResponseModel> GetLanguageResources([FromQuery] LocalResourceRequestDto requestModel)
        {
            try
            {
                if (string.IsNullOrEmpty(requestModel.ResourceNamePrefix))
                    return ErrorResponse("ResourceNamePrefix cannot be empty", HttpStatusCode.BadRequest);

                //try to get a language with the specified id
                var language = await _languageService.GetLanguageByIdAsync(requestModel.LanguageId);
                if (language == null)
                    return ErrorResponse("Language not found", HttpStatusCode.NotFound);

                //prepare model
                var localResourcesDto = await _dtoHelper.PrepareLocalResourceDtosAsync(language, requestModel.ResourceNamePrefix);

                //success
                return SuccessResponse(await _localizationService.GetResourceAsync("Nop.Api.Language.ResourceValues"), localResourcesDto);
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex.Message, HttpStatusCode.BadRequest);
            }
        }

        #endregion
    }
}
