using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.Api.Attributes;
using Nop.Plugin.Api.Authorization.Attributes;
using Nop.Plugin.Api.DTO.Categories;
using Nop.Plugin.Api.DTO.Errors;
using Nop.Plugin.Api.DTO.HomeScreen;
using Nop.Plugin.Api.DTOs.HomeScreen.V1;
using Nop.Plugin.Api.Helpers;
using Nop.Plugin.Api.JSON.Serializers;
using Nop.Plugin.Api.Models.BaseModels;
using Nop.Plugin.Api.Models.HomeScreenModels;
using Nop.Plugin.Widgets.JCarousel.Services;
using Nop.Services.Authentication;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Discounts;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Security;
using Nop.Services.Stores;

namespace Nop.Plugin.Api.Controllers
{
    public class HomeScreenController : BaseApiController
    {
        private readonly ICategoryService _categoryService;
        private readonly IDTOHelper _dtoHelper;
        private readonly IAuthenticationService _authenticationService;
        private readonly IStoreContext _storeContext;
        private readonly IJCarouselService _jCarouselService;

        public HomeScreenController(
            IJsonFieldsSerializer jsonFieldsSerializer,
            ICustomerActivityService customerActivityService,
            ILocalizationService localizationService,
            IPictureService pictureService,
            IStoreMappingService storeMappingService,
            IStoreService storeService,
            IDiscountService discountService,
            IAclService aclService,
            ICustomerService customerService,
            ICategoryService categoryService,
            IDTOHelper dtoHelper,
            IAuthenticationService authenticationService,
            IStoreContext storeContext,
            Widgets.JCarousel.Services.IJCarouselService jCarouselService) : base(
                jsonFieldsSerializer,
                aclService,
                customerService,
                storeMappingService,
                storeService,
                discountService,
                customerActivityService,
                localizationService,
                pictureService)
        {
            _categoryService = categoryService;
            _dtoHelper = dtoHelper;
            _authenticationService = authenticationService;
            _storeContext = storeContext;
            _jCarouselService = jCarouselService;
        }

        /// <summary>
        ///     Receive a list of items to show on home screen
        /// </summary>
        /// <response code="200">OK</response>
        /// <response code="400">Bad Request</response>
        /// <response code="401">Unauthorized</response>
        [HttpGet]
        [Route("/api/homescreen", Name = "GetHomeScreen")]
        [AuthorizePermission("PublicStoreAllowNavigation")]
        [ProducesResponseType(typeof(HomeScreenRootObject), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorsRootObject), (int)HttpStatusCode.BadRequest)]
        [GetRequestsErrorInterceptorActionFilter]
        public async Task<BaseResponseModel> GetHomeScreen()
        {

            try
            {
                //current customer
                var customer = await _authenticationService.GetAuthenticatedCustomerAsync();
                if (customer is null)
                    return ErrorResponse("Unauthorized! Customer not found", HttpStatusCode.Unauthorized);

                //var allCategories = _categoryApiService.GetTopCategories();
                var homepageCategories = (await _categoryService.GetAllCategoriesDisplayedOnHomepageAsync())?.ToList();

                //show top level category on home page (4-01-23)
                var store = await _storeContext.GetCurrentStoreAsync();
                var topCategories = (await _categoryService.GetAllCategoriesAsync(storeId: store.Id))?.Where(c => c.ParentCategoryId == 0)?.ToList();

                //show root + top level category on home page (24-01-23)
                homepageCategories.AddRange(topCategories);
                homepageCategories = homepageCategories.DistinctBy(c => c.Id).ToList();

                IList<TopCategoryDto> homeCategoriesAsDtos = await homepageCategories.OrderBy(c => c.DisplayOrder).SelectAwait(async category => await _dtoHelper.PrepareTopCategoryDTOAsync(category)).ToListAsync();
                if (homeCategoriesAsDtos == null)
                    return ErrorResponse(await _localizationService.GetResourceAsync("Nop.Api.TopCategories.NotFound"), HttpStatusCode.NotFound);

                IList<HomeBannerDto> homeBannerAsDtos = await _dtoHelper.PrepareHomeBannerDtoAsync();
                if (homeBannerAsDtos == null)
                    return ErrorResponse(await _localizationService.GetResourceAsync("Nop.Api.HomeBanner.NotFound"), HttpStatusCode.NotFound);

                //carousel
                var carouselDtos = await _dtoHelper.PrepareJCarouselDTOsAsync(customer);

                var homeScreenRootObject = new HomeScreenRootObject
                {
                    HomePageCategories = homeCategoriesAsDtos,
                    HomeBanners = homeBannerAsDtos,
                    JCarousels = carouselDtos
                };

                return SuccessResponse(await _localizationService.GetResourceAsync("Nop.Api.HomeScreenRootObject"), homeScreenRootObject);
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Receive a homepagr categories, banner & carousel details on home screen
        /// </summary>
        /// <response code="200">OK</response>
        /// <response code="400">Bad Request</response>
        /// <response code="401">Unauthorized</response>
        [HttpGet]
        [Route("/api/v1/homescreen", Name = "GetHomeScreen_V1")]
        [AuthorizePermission("PublicStoreAllowNavigation")]
        [ProducesResponseType(typeof(HomeScreenRootObjectV1), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorsRootObject), (int)HttpStatusCode.BadRequest)]
        [GetRequestsErrorInterceptorActionFilter]
        public async Task<BaseResponseModel> GetHomeScreen_V1()
        {
            try
            {
                //current customer
                var customer = await _authenticationService.GetAuthenticatedCustomerAsync();
                if (customer is null)
                    return ErrorResponse("Unauthorized! Customer not found", HttpStatusCode.Unauthorized);

                //var allCategories = _categoryApiService.GetTopCategories();
                var homepageCategories = (await _categoryService.GetAllCategoriesDisplayedOnHomepageAsync())?.ToList();

                //show top level category on home page (4-01-23)
                var store = await _storeContext.GetCurrentStoreAsync();
                var topCategories = (await _categoryService.GetAllCategoriesAsync(storeId: store.Id))?.Where(c => c.ParentCategoryId == 0)?.ToList();

                //show root + top level category on home page (24-01-23)
                homepageCategories.AddRange(topCategories);
                homepageCategories = homepageCategories.DistinctBy(c => c.Id).ToList();

                //prepare home categories dtos
                var homeCategoriesAsDtos = await homepageCategories.OrderBy(c => c.DisplayOrder).SelectAwait(async category => await _dtoHelper.PrepareTopCategoryDTOAsync(category)).ToListAsync();

                //prepare home banner dtos
                var homeBannerAsDtos = await _dtoHelper.PrepareHomeBannerDtoAsync();

                //carousel
                var jCarouselInfoDtos = await _dtoHelper.PrepareJCarouselInfoDTOsAsync(customer);

                var homeScreenRootObject = new HomeScreenRootObjectV1
                {
                    HomePageCategories = homeCategoriesAsDtos,
                    HomeBanners = homeBannerAsDtos,
                    JCarousels = jCarouselInfoDtos
                };

                return SuccessResponse(await _localizationService.GetResourceAsync("Nop.Api.HomeScreenRootObject"), homeScreenRootObject);
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Receive a homepagr categories, banner & carousel details on home screen
        /// </summary>
        /// <response code="200">OK</response>
        /// <response code="400">Bad Request</response>
        /// <response code="401">Unauthorized</response>
        [HttpGet]
        [Route("/api/v1/jcarousel", Name = "GetJCarousel_V1")]
        [AuthorizePermission("PublicStoreAllowNavigation")]
        [ProducesResponseType(typeof(CategoriesRootObject), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorsRootObject), (int)HttpStatusCode.BadRequest)]
        [GetRequestsErrorInterceptorActionFilter]
        public async Task<BaseResponseModel> GetJCarousel_V1([FromQuery] int jcarouselId)
        {
            try
            {
                //current customer
                var customer = await _authenticationService.GetAuthenticatedCustomerAsync();
                if (customer is null)
                    return ErrorResponse("Unauthorized! Customer not found", HttpStatusCode.Unauthorized);

                //get carsouel record
                var jcarousel = await _jCarouselService.GetJCarouselByIdAsync(jcarouselId);
                if (jcarousel is null)
                    return ErrorResponse("JCarousel record not found with specific id", HttpStatusCode.Unauthorized);

                //carousel
                var jCarouselDto = await _dtoHelper.PrepareJCarouselDTOAsync(customer, jcarousel);

                //response model
                var responseModel = new JCarouselResponseModel
                {
                    JCarouselDto = jCarouselDto,
                    IsVisible = jCarouselDto != null
                };

                return SuccessResponse("jcarousel", responseModel);
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex.Message, HttpStatusCode.BadRequest);
            }
        }
    }
}