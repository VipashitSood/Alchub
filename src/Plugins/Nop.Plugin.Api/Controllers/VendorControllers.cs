using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Alchub.Domain.Vendors;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Api.Attributes;
using Nop.Plugin.Api.Authorization.Attributes;
using Nop.Plugin.Api.DTO.Errors;
using Nop.Plugin.Api.DTOs.Vendors;
using Nop.Plugin.Api.Factories;
using Nop.Plugin.Api.JSON.Serializers;
using Nop.Plugin.Api.Models;
using Nop.Plugin.Api.Models.BaseModels;
using Nop.Plugin.Api.Models.Vendors;
using Nop.Services.Alchub.Vendors;
using Nop.Services.Authentication;
using Nop.Services.Customers;
using Nop.Services.Discounts;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Services.Vendors;

namespace Nop.Plugin.Api.Controllers
{
    public class VendorController : BaseApiController
    {
        #region Fields
        private readonly IVendorModelFactory _vendorModelFactory;
        private readonly IAuthenticationService _authenticationService;
        private readonly IVendorService _vendorService;
        private readonly IFavoriteVendorService _favoriteVendorService;
        private readonly IStoreContext _storeContext;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IWorkContext _workContext;

        #endregion

        #region Ctor
        public VendorController(
            IVendorModelFactory vendorModelFactory,
            IAuthenticationService authenticationService,
            IVendorService vendorService,
            IFavoriteVendorService favoriteVendorService,
            ICustomerService customerService,
            IJsonFieldsSerializer jsonFieldsSerializer,
            ICustomerActivityService customerActivityService,
            ILocalizationService localizationService,
            IAclService aclService,
            IStoreMappingService storeMappingService,
            IStoreService storeService,
            IDiscountService discountService,
            IPictureService pictureService,
            IStoreContext storeContext,
            IShoppingCartService shoppingCartService,
            IWorkContext workContext)
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
            _vendorModelFactory = vendorModelFactory;
            _authenticationService = authenticationService;
            _vendorService = vendorService;
            _favoriteVendorService = favoriteVendorService;
            _storeContext = storeContext;
            _shoppingCartService = shoppingCartService;
            _workContext = workContext;
        }

        #endregion

        #region Favorite vendor

        [HttpGet]
        [Route("/api/favorite_vendors", Name = "API_FavoriteVendors")]
        [AuthorizePermission("PublicStoreAllowNavigation")]
        [ProducesResponseType(typeof(VendorsRootObjectDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorsRootObject), (int)HttpStatusCode.BadRequest)]
        [GetRequestsErrorInterceptorActionFilter]
        public async Task<BaseResponseModel> FavoriteVendors()
        {
            var customer = await _authenticationService.GetAuthenticatedCustomerAsync();
            if (customer is null)
                return ErrorResponse("Unauthorized! Customer not found", HttpStatusCode.Unauthorized);

            try
            {
                var model = new FavoriteVendorModel();
                model = await _vendorModelFactory.PrepareFavoriteVendorModelAsync(model, customer);

                return SuccessResponse(await _localizationService.GetResourceAsync("Nop.Api.FavoriteVendors"), model);
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex.Message, HttpStatusCode.BadRequest);
            }
        }

        [HttpPost]
        [Route("/api/favorite_vendors", Name = "API_AddIntoFavoriteVendor")]
        [AuthorizePermission("PublicStoreAllowNavigation")]
        [ProducesResponseType(typeof(VendorsRootObjectDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorsRootObject), (int)HttpStatusCode.BadRequest)]
        [GetRequestsErrorInterceptorActionFilter]
        public async Task<BaseResponseModel> AddIntoFavoriteVendor([FromBody] AddFavotiteVendorRequest requestModel)
        {
            if (requestModel.VendorId <= 0)
                return ErrorResponse("Invalid vendorId parameter", HttpStatusCode.BadRequest);

            //get vendor
            var vendor = await _vendorService.GetVendorByIdAsync(requestModel.VendorId);
            if (vendor == null || vendor.Deleted || !vendor.Active)
                return ErrorResponse("Vendor not found", HttpStatusCode.BadRequest);

            //get customer
            var customer = await _authenticationService.GetAuthenticatedCustomerAsync();
            if (customer == null)
                return ErrorResponse("Unauthorized! Customer not found", HttpStatusCode.Unauthorized);

            try
            {
                //get favorite vendor by identifier and customer identifier
                var favoriteVendor = await _favoriteVendorService.GetFavoriteVendorByVendorIdAsync(vendor.Id, customer.Id);
                if (favoriteVendor != null)
                    return ErrorResponse("Favorite vendor already exists", HttpStatusCode.BadRequest);

                favoriteVendor = new Nop.Core.Alchub.Domain.Vendors.FavoriteVendor()
                {
                    CustomerId = customer.Id,
                    VendorId = vendor.Id
                };

                //add this vendor into customer favorite vendor list
                await _favoriteVendorService.InsertFavoriteVendorAsync(favoriteVendor);

                //prepare favorite vendor list with updated details
                var model = new FavoriteVendorModel();
                model = await _vendorModelFactory.PrepareFavoriteVendorModelAsync(model, customer);

                return SuccessResponse(await _localizationService.GetResourceAsync("Nop.Api.FavoriteVendors"), model);
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex.Message, HttpStatusCode.BadRequest);
            }
        }

        [HttpDelete]
        [Route("/api/favorite_vendors/{vendorId}", Name = "API_RemoveFromFavoriteVendor")]
        [AuthorizePermission("PublicStoreAllowNavigation")]
        [ProducesResponseType(typeof(VendorsRootObjectDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorsRootObject), (int)HttpStatusCode.BadRequest)]
        [GetRequestsErrorInterceptorActionFilter]
        public async Task<BaseResponseModel> RemoveFromFavoriteVendor([FromRoute] int vendorId)
        {
            if (vendorId <= 0)
                return ErrorResponse("Invalid vendorId parameter", HttpStatusCode.BadRequest);

            //get vendor
            var vendor = await _vendorService.GetVendorByIdAsync(vendorId);
            if (vendor == null || vendor.Deleted || !vendor.Active)
                return ErrorResponse("Vendor not found", HttpStatusCode.BadRequest);

            //get customer
            var customer = await _authenticationService.GetAuthenticatedCustomerAsync();
            if (customer == null)
                return ErrorResponse("Unauthorized! Customer not found", HttpStatusCode.Unauthorized);

            try
            {
                //get favorite vendor by identifier and customer identifier
                var favoriteVendor = await _favoriteVendorService.GetFavoriteVendorByVendorIdAsync(vendor.Id, customer.Id);

                //delete this vendor from customer favorite list
                if (favoriteVendor != null)
                    await _favoriteVendorService.DeleteFavoriteVendorAsync(favoriteVendor);

                //check if no favorite vendor remain, then also trun off the toggle 
                var favoriteVendors = await _favoriteVendorService.GetFavoriteVendorByCustomerIdAsync(customer.Id);
                if (!favoriteVendors.Any())
                {
                    //update IsFavorite toggle for customer
                    customer.IsFavoriteToggleOn = false;
                    await _customerService.UpdateCustomerAsync(customer);
                }

                //prepare favorite vendor list with updated details
                var model = new FavoriteVendorModel();
                model = await _vendorModelFactory.PrepareFavoriteVendorModelAsync(model, customer);

                return SuccessResponse(await _localizationService.GetResourceAsync("Nop.Api.FavoriteVendors"), model);
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex.Message, HttpStatusCode.BadRequest);
            }
        }

        [HttpPost]
        [Route("/api/favorite_vendors/can_switch_favorite_toggle_on", Name = "API_CanSwitchFavoriteToggleOn")]
        [AuthorizePermission("PublicStoreAllowNavigation")]
        [ProducesResponseType(typeof(CanSwitchFavoriteToggleOnDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorsRootObject), (int)HttpStatusCode.BadRequest)]
        [GetRequestsErrorInterceptorActionFilter]
        public async Task<BaseResponseModel> CanSwitchFavoriteToggleOn([FromBody] CanSwitchFavoriteToggleOnRequest reuestModel)
        {
            if (!reuestModel.Toggle.HasValue)
                return ErrorResponse("toggle parameter cannot empty!", HttpStatusCode.BadRequest);

            //get customer
            var customer = await _authenticationService.GetAuthenticatedCustomerAsync();
            if (customer == null)
                return ErrorResponse("Unauthorized! Customer not found", HttpStatusCode.Unauthorized);

            try
            {

                //if toggle false, then do not proceed
                if (reuestModel.Toggle.Value == false)
                {
                    var siwtchModel = new CanSwitchFavoriteToggleOnDto() { Message = "", ClearShoppingCartRequired = false };
                    return SuccessResponse(await _localizationService.GetResourceAsync("Nop.Api.FavoriteVendors.CanSwitchFavoriteToggleOn"), siwtchModel);
                }

                //cart 
                var store = await _storeContext.GetCurrentStoreAsync();
                var cart = await _shoppingCartService.GetShoppingCartAsync(customer, Core.Domain.Orders.ShoppingCartType.ShoppingCart, store.Id);
                if (!cart.Any())
                {
                    var siwtchModel = new CanSwitchFavoriteToggleOnDto() { Message = "", ClearShoppingCartRequired = false };
                    return SuccessResponse(await _localizationService.GetResourceAsync("Nop.Api.FavoriteVendors.CanSwitchFavoriteToggleOn"), siwtchModel);
                }

                //check switch to toggle ON possible 
                var favoriteVendors = await _favoriteVendorService.GetFavoriteVendorByCustomerIdAsync(customer.Id);
                if (favoriteVendors == null || !favoriteVendors.Any())
                {
                    var warnings = new List<string>() { await _localizationService.GetResourceAsync("Nop.Api.FavoriteVendors.Toggle.On.Error") };
                    var errorMessage = string.Join(",", warnings.ToList());
                    return ErrorResponse(errorMessage, HttpStatusCode.BadRequest, warnings.ToList());
                }

                var pIds = cart.Select(x => x.ProductId).ToArray();
                var shoppingCartVendors = await _vendorService.GetVendorsByProductIdsAsync(pIds);
                var shoppingCartHasVendorExeptFavorite = false;

                foreach (var vendor in shoppingCartVendors)
                {
                    if (!favoriteVendors.Any(x => x.VendorId == vendor.Id))
                    {
                        shoppingCartHasVendorExeptFavorite = true;
                        break;
                    }
                }

                //check shopping cart has any other vendor then favorite
                if (shoppingCartHasVendorExeptFavorite)
                {
                    var siwtchModel = new CanSwitchFavoriteToggleOnDto()
                    {
                        Message = await _localizationService.GetResourceAsync("Alchub.FavoriteVendor.Toggle.CanSwitchToggleOn.ShoppingCartVendor.Error"),
                        ClearShoppingCartRequired = true
                    };
                    return SuccessResponse(await _localizationService.GetResourceAsync("Nop.Api.FavoriteVendors.CanSwitchFavoriteToggleOn"), siwtchModel);
                }

                //here means success
                var model = new CanSwitchFavoriteToggleOnDto() { Message = "", ClearShoppingCartRequired = false };
                return SuccessResponse(await _localizationService.GetResourceAsync("Nop.Api.FavoriteVendors.CanSwitchFavoriteToggleOn"), model);
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex.Message, HttpStatusCode.BadRequest);
            }
        }

        [HttpPost]
        [Route("/api/favorite_vendors/favorite_toggle", Name = "API_FavoriteToggle")]
        [AuthorizePermission("PublicStoreAllowNavigation")]
        [ProducesResponseType(typeof(VendorsRootObjectDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorsRootObject), (int)HttpStatusCode.BadRequest)]
        [GetRequestsErrorInterceptorActionFilter]
        public async Task<BaseResponseModel> FavoriteToggle([FromBody] FavoriteToggleRequest reuestModel)
        {
            if (!reuestModel.Toggle.HasValue)
                return ErrorResponse("toggle parameter cannot empty!", HttpStatusCode.BadRequest);

            //get customer
            var customer = await _authenticationService.GetAuthenticatedCustomerAsync();
            if (customer == null)
                return ErrorResponse("Unauthorized! Customer not found", HttpStatusCode.Unauthorized);

            try
            {
                //check switch to toggle ON possible 
                if (reuestModel.Toggle.Value)
                {
                    var favoriteVendors = await _favoriteVendorService.GetFavoriteVendorByCustomerIdAsync(customer.Id);
                    if (favoriteVendors == null || !favoriteVendors.Any())
                    {
                        var warnings = new List<string>() { await _localizationService.GetResourceAsync("Nop.Api.FavoriteVendors.Toggle.On.Error") };
                        var errorMessage = string.Join(",", warnings.ToList());
                        return ErrorResponse(errorMessage, HttpStatusCode.BadRequest, warnings.ToList());
                    }
                }

                //set toggle value for customer
                customer.IsFavoriteToggleOn = reuestModel.Toggle.Value;
                await _customerService.UpdateCustomerAsync(customer);

                //clear shoppingcart
                if (reuestModel.ClearShoppingCartRequired == true)
                {
                    var store = await _storeContext.GetCurrentStoreAsync();
                    var cart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, store.Id);

                    foreach (var item in cart)
                    {
                        await _shoppingCartService.DeleteShoppingCartItemAsync(item);
                    }
                }

                //prepare favorite vendor list with updated details
                var model = new FavoriteVendorModel();
                model = await _vendorModelFactory.PrepareFavoriteVendorModelAsync(model, customer);

                return SuccessResponse(await _localizationService.GetResourceAsync("Nop.Api.FavoriteVendors"), model);
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex.Message, HttpStatusCode.BadRequest);
            }
        }

        #endregion

        #region Geo

        [HttpGet]
        [Route("/api/vendor/available_vendors", Name = "API_AvailableVendors")]
        [AuthorizePermission("PublicStoreAllowNavigation")]
        [ProducesResponseType(typeof(VendorsRootObjectDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorsRootObject), (int)HttpStatusCode.BadRequest)]
        [GetRequestsErrorInterceptorActionFilter]
        public async Task<BaseResponseModel> AvailableVendors()
        {
            var customer = await _authenticationService.GetAuthenticatedCustomerAsync();
            if (customer is null)
                return ErrorResponse("Unauthorized! Customer not found", HttpStatusCode.Unauthorized);

            try
            {
                //get available vendors
                var availableVendorsIds = await _workContext.GetCurrentCustomerGeoRadiusVendorIdsAsync(customer);
                var availableVendors = await _vendorService.GetVendorsByIdsAsync(availableVendorsIds?.ToArray());

                var model = new
                {
                    availableVendorsIds = availableVendorsIds,
                    availableVendors = availableVendors
                };

                return SuccessResponse("Available Vendors (Accoridng searched location)", model);
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex.Message, HttpStatusCode.BadRequest);
            }
        }

        #endregion
    }
}
