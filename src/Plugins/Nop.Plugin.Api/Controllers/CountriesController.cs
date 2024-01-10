using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.Api.Attributes;
using Nop.Plugin.Api.Authorization.Attributes;
using Nop.Plugin.Api.DTO;
using Nop.Plugin.Api.DTO.Errors;
using Nop.Plugin.Api.DTOs.Address;
using Nop.Plugin.Api.JSON.ActionResults;
using Nop.Plugin.Api.JSON.Serializers;
using Nop.Plugin.Api.Models.BaseModels;
using Nop.Plugin.Api.Models.State;
using Nop.Plugin.Api.Services;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Discounts;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Security;
using Nop.Services.Stores;

namespace Nop.Plugin.Api.Controllers
{
    public class CountriesController : BaseApiController
    {
        private readonly IAddressApiService _addressApiService;
        private readonly ICountryService _countryService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly IWorkContext _workContext;

        public CountriesController(
            IJsonFieldsSerializer jsonFieldsSerializer,
            IAclService aclService,
            ICountryService countryService,
            ICustomerService customerService,
            IStoreMappingService storeMappingService,
            IStoreService storeService,
            IStateProvinceService stateProvinceService,
            IWorkContext workContext,
            IDiscountService discountService,
            ICustomerActivityService customerActivityService,
            ILocalizationService localizationService,
            IPictureService pictureService,
            IAddressApiService addressApiService)
            : base(jsonFieldsSerializer, aclService, customerService, storeMappingService, storeService, discountService, customerActivityService, localizationService, pictureService)
        {
            _addressApiService = addressApiService;
            _countryService = countryService;
            _stateProvinceService = stateProvinceService;
            _workContext = workContext;
        }

        /// <summary>
        ///     Receive a list of all Countries
        /// </summary>
        /// <response code="200">OK</response>
        /// <response code="400">Bad Request</response>
        /// <response code="401">Unauthorized</response>
        [HttpGet]
        [Route("/api/countries", Name = "GetCountries")]
        [ProducesResponseType(typeof(CountryListRootObjectResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorsRootObject), (int)HttpStatusCode.BadRequest)]
        [GetRequestsErrorInterceptorActionFilter]
        public async Task<BaseResponseModel> GetCountries([FromQuery] bool? mustAllowBilling = null, [FromQuery] bool? mustAllowShipping = null)
        {
            try
            {
                var countriesDtos = await _addressApiService.GetAllCountriesAsync(mustAllowBilling ?? false, mustAllowShipping ?? false);

                var countriesRootObject = new CountryListRootObjectResponse
                {
                    Countries = countriesDtos
                };

                return SuccessResponse(await _localizationService.GetResourceAsync("Nop.Api.CountryList"), countriesRootObject);
            }
            catch (Exception ex)
            {
                return ErrorResponse(await _localizationService.GetResourceAsync("Nop.Api.CountryList.NoCountryFound"), HttpStatusCode.NotFound);
            }

        }
        [HttpPost]
        [Route("/api/states", Name = "GetStates")]
        [ProducesResponseType(typeof(StateListRootObjectResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorsRootObject), (int)HttpStatusCode.BadRequest)]
        [GetRequestsErrorInterceptorActionFilter]
        public virtual async Task<BaseResponseModel> GetStatesByCountryId(StateModel model)
        {
            try
            {
                if (string.IsNullOrEmpty(model.countryId))
                    throw new ArgumentNullException(nameof(model.countryId));

                var country = await _countryService.GetCountryByIdAsync(Convert.ToInt32(model.countryId));
                var states = (await _stateProvinceService
                    .GetStateProvincesByCountryIdAsync(country?.Id ?? 0, (await _workContext.GetWorkingLanguageAsync()).Id))
                    .ToList();

                var result = new List<StateListResponse>();
                foreach (var state in states)
                    result.Add(new StateListResponse
                    {
                        id = state.Id,
                        name = await _localizationService.GetLocalizedAsync(state, x => x.Name)
                    });
                var stateRootObject = new StateListRootObjectResponse
                {
                    States = result
                };
                return SuccessResponse(await _localizationService.GetResourceAsync("Nop.Api.StateList"), stateRootObject);
            }
            catch (Exception ex)
            {
                return ErrorResponse(await _localizationService.GetResourceAsync("Nop.Api.StateList.NoStateFound"), HttpStatusCode.NotFound);
            }
        }
    }
}
