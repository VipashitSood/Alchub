using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using LinqToDB.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Vendors;
using Nop.Data;
using Nop.Plugin.Api.Attributes;
using Nop.Plugin.Api.Delta;
using Nop.Plugin.Api.DTO.Errors;
using Nop.Plugin.Api.DTO.ShoppingCarts;
using Nop.Plugin.Api.Factories;
using Nop.Plugin.Api.Helpers;
using Nop.Plugin.Api.Infrastructure;
using Nop.Plugin.Api.JSON.ActionResults;
using Nop.Plugin.Api.JSON.Serializers;
using Nop.Plugin.Api.ModelBinders;
using Nop.Plugin.Api.Models.BaseModels;
using Nop.Plugin.Api.Models.ShoppingCartsParameters;
using Nop.Plugin.Api.Services;
using Nop.Services.Authentication;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Discounts;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Services.Slots;
using Nop.Services.Stores;
using Nop.Services.Vendors;

namespace Nop.Plugin.Api.Controllers
{
    public class ShoppingCartItemsController : BaseApiController
    {
        private readonly IDTOHelper _dtoHelper;
        private readonly IFactory<ShoppingCartItem> _factory;
        private readonly IProductAttributeConverter _productAttributeConverter;
        private readonly IProductService _productService;
        private readonly IShoppingCartItemApiService _shoppingCartItemApiService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IStoreContext _storeContext;
        private readonly IPermissionService _permissionService;
        private readonly IAuthenticationService _authenticationService;
        private readonly IVendorService _vendorService;
        private readonly ISettingService _settingService;
        private readonly ICategoryService _categoryService;
        private readonly ISlotService _slotService;
        private readonly IRepository<ShoppingCartItem> _sciRepository;
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IProductApiService _productApiService;
        private readonly IDateTimeHelper _dateTimeHelper;

        public ShoppingCartItemsController(
            IShoppingCartItemApiService shoppingCartItemApiService,
            IJsonFieldsSerializer jsonFieldsSerializer,
            IAclService aclService,
            ICustomerService customerService,
            IStoreMappingService storeMappingService,
            IStoreService storeService,
            IDiscountService discountService,
            ICustomerActivityService customerActivityService,
            ILocalizationService localizationService,
            IShoppingCartService shoppingCartService,
            IProductService productService,
            IFactory<ShoppingCartItem> factory,
            IPictureService pictureService,
            IProductAttributeConverter productAttributeConverter,
            IDTOHelper dtoHelper,
            IStoreContext storeContext,
            IPermissionService permissionService,
            IAuthenticationService authenticationService,
            IVendorService vendorService,
            ISettingService settingService,
            ICategoryService categoryService,
            ISlotService slotService,
            IRepository<ShoppingCartItem> sciRepository,
            IProductAttributeParser productAttributeParser,
            IPriceFormatter priceFormatter,
            IProductApiService productApiService,
            IDateTimeHelper dateTimeHelper
            )
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
            _shoppingCartItemApiService = shoppingCartItemApiService;
            _shoppingCartService = shoppingCartService;
            _productService = productService;
            _factory = factory;
            _productAttributeConverter = productAttributeConverter;
            _dtoHelper = dtoHelper;
            _storeContext = storeContext;
            _permissionService = permissionService;
            _authenticationService = authenticationService;
            _vendorService = vendorService;
            _settingService = settingService;
            _vendorService = vendorService;
            _categoryService = categoryService;
            _slotService = slotService;
            _sciRepository = sciRepository;
            _productAttributeParser = productAttributeParser;
            _priceFormatter = priceFormatter;
            _productApiService = productApiService;
            _dateTimeHelper = dateTimeHelper;
        }

        /// <summary>
        ///     Receive a list of all shopping cart items of all customers if the customerId is NOT specified. Otherwise, return all shopping cart items of a customer.
        /// </summary>
        /// <response code="200">OK</response>
        /// <response code="400">Bad Request</response>
        /// <response code="401">Unauthorized</response>
        [HttpGet]
        [Route("/api/shopping_cart_items", Name = "GetShoppingCartItems")]
        [ProducesResponseType(typeof(ShoppingCartItemsRootObject), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ErrorsRootObject), (int)HttpStatusCode.BadRequest)]
        [GetRequestsErrorInterceptorActionFilter]
        public async Task<BaseResponseModel> GetShoppingCartItems([FromQuery] ShoppingCartItemsParametersModel parameters)
        {
            if (parameters.Limit.HasValue && (parameters.Limit < Constants.Configurations.MIN_LIMIT || parameters.Limit > Constants.Configurations.MAX_LIMIT))
                return ErrorResponse("invalid limit parameter", HttpStatusCode.BadRequest);

            if (parameters.Page.HasValue && (parameters.Page < Constants.Configurations.DEFAULT_PAGE_VALUE))
                return ErrorResponse("invalid page parameter", HttpStatusCode.BadRequest);

            ShoppingCartType? shoppingCartType = null;

            if (parameters.ShoppingCartType.HasValue && Enum.IsDefined(parameters.ShoppingCartType.Value))
            {
                shoppingCartType = (ShoppingCartType)parameters.ShoppingCartType.Value;
            }

            if (!await CheckPermissions(parameters.CustomerId, shoppingCartType))
                return ErrorResponse("Access Denied!", HttpStatusCode.Forbidden);

            //get shopping cart items
            var shoppingCartItems = await _shoppingCartItemApiService.GetShoppingCartItems(parameters.CustomerId,
                                                                                           parameters.CreatedAtMin,
                                                                                           parameters.CreatedAtMax,
                                                                                           0,
                                                                                           parameters.UpdatedAtMin,
                                                                                           parameters.UpdatedAtMax,
                                                                                           parameters.Limit,
                                                                                           parameters.Page,
                                                                                           shoppingCartType);
            //prepare dto model
            var shoppingCartItemsDtos = await shoppingCartItems
                                        .SelectAwait(async shoppingCartItem => await _dtoHelper.PrepareShoppingCartItemDTOAsync(shoppingCartItem))
                                        .ToListAsync();



            var shoppingCartsRootObject = new ShoppingCartItemsListObject
            {
                PageIndex = shoppingCartItems.PageIndex + 1,
                PageSize = shoppingCartItems.PageSize,
                TotalRecords = shoppingCartItems.TotalCount,
            };

            /*Alchub Start*/
            var vendors = await _vendorService.GetVendorsByProductIdsAsync(shoppingCartItems.Select(x => x.ProductId).ToArray());

            if (vendors != null)
            {
                foreach (var vendor in vendors)
                {
                    shoppingCartsRootObject.ShoppingCartVendors.Add(new ShoppingCartItemsRootObject.ShoppingCartVendorDto
                    {
                        Id = vendor.Id,
                        Name = string.Format(await _localizationService.GetResourceAsync("Alchub.Vendor.MinimumOrderSubtotal.Vendor.Name"), vendor.Name),
                        Warnings = await GetVendorMinimumOrderAmountWarningsAsync(shoppingCartItems, vendor),
                        ShoppingCartItems = shoppingCartItemsDtos.Where(x => x.VendorId == vendor.Id).ToList(),
                    });
                }
                shoppingCartsRootObject.ShoppingCartVendors = shoppingCartsRootObject.ShoppingCartVendors?.OrderBy(x => x.Name).ToList();
            }
            shoppingCartsRootObject.CrossSellsProductIds = await GetCrossSellProducts(shoppingCartItems);
            /*Alchub End*/

            var message = await _localizationService.GetResourceAsync("Nop.Api.ShoppingCartItems");
            if (shoppingCartType != null && shoppingCartType == ShoppingCartType.Wishlist)
                message = await _localizationService.GetResourceAsync("Nop.Api.WishlistItems");

            return SuccessResponse(message, shoppingCartsRootObject);
        }

        /// <summary>
        ///  Receive a list of all shopping cart items of all customers if the customerId is NOT specified. Otherwise, return all shopping cart items of a customer.
        /// </summary>
        /// <response code="200">OK</response>
        /// <response code="400">Bad Request</response>
        /// <response code="401">Unauthorized</response>
        [HttpGet]
        [Route("/api/wishlist_items", Name = "API_GetWishListItems")]
        [ProducesResponseType(typeof(ShoppingCartItemsRootObject), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ErrorsRootObject), (int)HttpStatusCode.BadRequest)]
        [GetRequestsErrorInterceptorActionFilter]
        public async Task<BaseResponseModel> GetWishListItems([FromQuery] WishlistItemsParametersModel parameters)
        {
            if (parameters.Limit.HasValue && (parameters.Limit < Constants.Configurations.MIN_LIMIT || parameters.Limit > Constants.Configurations.MAX_LIMIT))
                return ErrorResponse("invalid limit parameter", HttpStatusCode.BadRequest);

            if (parameters.Page.HasValue && (parameters.Page < Constants.Configurations.DEFAULT_PAGE_VALUE))
                return ErrorResponse("invalid page parameter", HttpStatusCode.BadRequest);

            var shoppingCartType = ShoppingCartType.Wishlist;
            if (!await CheckPermissions(parameters.CustomerId, shoppingCartType))
                return ErrorResponse("Access Denied!", HttpStatusCode.Forbidden);

            //get shopping cart items
            var shoppingCartItems = await _shoppingCartItemApiService.GetShoppingCartItems(customerId: parameters.CustomerId,
                                                                                           limit: parameters.Limit,
                                                                                           page: parameters.Page,
                                                                                           shoppingCartType: shoppingCartType);
            //prepare dto model
            var shoppingCartItemsDtos = await shoppingCartItems
                                        .SelectAwait(async shoppingCartItem => await _dtoHelper.PrepareShoppingCartItemDTOAsync(shoppingCartItem))
                                        .ToListAsync();

            var wishlistItemsRootObject = new WishlistItemsListObject
            {
                PageIndex = shoppingCartItems.PageIndex + 1,
                PageSize = shoppingCartItems.PageSize,
                TotalRecords = shoppingCartItems.TotalCount,
                WishlistItems = shoppingCartItemsDtos
            };

            return SuccessResponse(await _localizationService.GetResourceAsync("Nop.Api.WishlistItems"), wishlistItemsRootObject);
        }

        [HttpGet]
        [Route("/api/shopping_cart_items/me", Name = "GetCurrentShoppingCart")]
        [ProducesResponseType(typeof(ShoppingCartItemsRootObject), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        public async Task<BaseResponseModel> GetCurrentShoppingCart()
        {
            var customer = await _authenticationService.GetAuthenticatedCustomerAsync();

            if (customer is null)
                return ErrorResponse("Customer not found", HttpStatusCode.Unauthorized);

            var shoppingCartType = ShoppingCartType.ShoppingCart;

            if (!await CheckPermissions(customer.Id, shoppingCartType))
                return ErrorResponse("Access Denied!", HttpStatusCode.Forbidden);

            // load current shopping cart and return it as result of request
            var shoppingCartsRootObject = await LoadCurrentShoppingCartItems(shoppingCartType, customer);

            return SuccessResponse(await _localizationService.GetResourceAsync("Nop.Api.ShoppingCartItems"), shoppingCartsRootObject);
        }

        [HttpPost]
        [Route("/api/shopping_cart_items", Name = "CreateShoppingCartItem")]
        [ProducesResponseType(typeof(ShoppingCartItemsRootObject), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ErrorsRootObject), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        public async Task<BaseResponseModel> CreateShoppingCartItem(
            [FromBody] AddCartRequestModel addCartItemDelta)
        {

            // We know that the product id and customer id will be provided because they are required by the validator.
            // TODO: validate
            var product = await _productService.GetProductByIdAsync(addCartItemDelta.ProductId);

            if (product == null)
                return ErrorResponse("Product not found", HttpStatusCode.NotFound);

            var customer = await _customerService.GetCustomerByIdAsync(addCartItemDelta.CustomerId);

            if (customer == null)
                return ErrorResponse("Customer not found", HttpStatusCode.NotFound);

            if (!await CheckPermissions(addCartItemDelta.CustomerId, (ShoppingCartType)addCartItemDelta.ShoppingCartType))
                return ErrorResponse("Access Denied!", HttpStatusCode.Forbidden);

            var currentStoreId = _storeContext.GetCurrentStore().Id;

            //Alchub Start
            IList<string> warnings = new List<string>();

            warnings = await _shoppingCartService.AddToCartAsync(customer, product, (ShoppingCartType)addCartItemDelta.ShoppingCartType, currentStoreId, null, 0M,
                                                          null, null,
                                                          addCartItemDelta.Quantity,ispickup:addCartItemDelta.Ispickup,masterProductId:addCartItemDelta.MasterProductId,groupedProductId:addCartItemDelta.GroupedProductId);
            if (warnings.Count > 0)
            {
                var errorMessage = string.Join(",", warnings.ToList());
                return ErrorResponse(errorMessage, HttpStatusCode.BadRequest, warnings.ToList());
            }

            // load current shopping cart and return it as result of request
            var shoppingCartsRootObject = await LoadCurrentShoppingCartItems((ShoppingCartType)addCartItemDelta.ShoppingCartType, customer);

            var message = await _localizationService.GetResourceAsync("Nop.Api.ShoppingCartItem.added.success");
            if ((ShoppingCartType)addCartItemDelta.ShoppingCartType == ShoppingCartType.Wishlist)
                message = await _localizationService.GetResourceAsync("Nop.Api.WishlistCartItem.added.success");

            return SuccessResponse(message, shoppingCartsRootObject);
        }

        [HttpPost]
        [Route("/api/shopping_cart_items/batch", Name = "BatchCreateShoppingCartItems")]
        [ProducesResponseType(typeof(ShoppingCartItemsRootObject), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ErrorsRootObject), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> BatchCreateShoppingCartItems(
            [FromBody]
            [ModelBinder(typeof(JsonModelBinder<ShoppingCartItemsCreateParametersModel>))]
            Delta<ShoppingCartItemsCreateParametersModel> parameters)
        {
            var customer = await _customerService.GetCustomerByIdAsync(parameters.Dto.CustomerId);

            if (customer == null)
            {
                return Error(HttpStatusCode.NotFound, "customer", "not found");
            }

            if (!await CheckPermissions(parameters.Dto.CustomerId, (ShoppingCartType)parameters.Dto.ShoppingCartType))
            {
                return AccessDenied();
            }

            List<string> allWarnings = new();

            // add all items to cart
            foreach (var shoppingCartItemDto in parameters.Dto.Items)
            {
                Product product = null;
                if (shoppingCartItemDto.ProductId.HasValue)
                {
                    product = await _productService.GetProductByIdAsync(shoppingCartItemDto.ProductId.Value);
                }

                if (product == null)
                {
                    return Error(HttpStatusCode.NotFound, "product", "not found");
                }

                if (!product.IsRental)
                {
                    shoppingCartItemDto.RentalStartDateUtc = null;
                    shoppingCartItemDto.RentalEndDateUtc = null;
                }

                var attributesXml = await _productAttributeConverter.ConvertToXmlAsync(shoppingCartItemDto.Attributes, product.Id);

                var currentStoreId = _storeContext.GetCurrentStore().Id;

                var warnings = await _shoppingCartService.AddToCartAsync(customer, product, (ShoppingCartType)parameters.Dto.ShoppingCartType, currentStoreId, attributesXml, 0M,
                                                              shoppingCartItemDto.RentalStartDateUtc, shoppingCartItemDto.RentalEndDateUtc,
                                                              shoppingCartItemDto.Quantity ?? 1);
                allWarnings.AddRange(warnings);
            }

            // report warnings if any
            if (allWarnings.Count > 0)
            {
                foreach (var warning in allWarnings)
                {
                    ModelState.AddModelError("shopping cart item", warning);
                }

                return Error(HttpStatusCode.BadRequest);
            }

            // load current shopping cart and return it as result of request
            var shoppingCartsRootObject = await LoadCurrentShoppingCartItems((ShoppingCartType)parameters.Dto.ShoppingCartType, customer);

            var json = _jsonFieldsSerializer.Serialize(shoppingCartsRootObject, string.Empty);

            return new RawJsonActionResult(json);
        }

        [HttpPut]
        [Route("/api/shopping_cart_items/{id}", Name = "UpdateShoppingCartItem")]
        [ProducesResponseType(typeof(ShoppingCartItemsRootObject), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ErrorsRootObject), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        public async Task<BaseResponseModel> UpdateShoppingCartItem(
            [FromBody]
            [ModelBinder(typeof(JsonModelBinder<ShoppingCartItemDto>))]
            Delta<ShoppingCartItemDto> shoppingCartItemDelta) // NOTE: id parameter is missing intentionally to fix the generation of swagger json
        {
            // We kno that the id will be valid integer because the validation for this happens in the validator which is executed by the model binder.
            var shoppingCartItemForUpdate = await _shoppingCartItemApiService.GetShoppingCartItemAsync(shoppingCartItemDelta.Dto.Id);

            if (shoppingCartItemForUpdate == null)
                return ErrorResponse("shopping_cart_item not found", HttpStatusCode.NotFound);

            if (!await CheckPermissions(shoppingCartItemForUpdate.CustomerId, shoppingCartItemForUpdate.ShoppingCartType))
                return ErrorResponse("Access Denied!", HttpStatusCode.Forbidden);

            shoppingCartItemDelta.Merge(shoppingCartItemForUpdate);

            if (!(await _productService.GetProductByIdAsync(shoppingCartItemForUpdate.ProductId)).IsRental)
            {
                shoppingCartItemForUpdate.RentalStartDateUtc = null;
                shoppingCartItemForUpdate.RentalEndDateUtc = null;
            }

            if (shoppingCartItemDelta.Dto.Attributes != null)
            {
                shoppingCartItemForUpdate.AttributesXml = await _productAttributeConverter.ConvertToXmlAsync(shoppingCartItemDelta.Dto.Attributes, shoppingCartItemForUpdate.ProductId);
            }

            var customer = await _customerService.GetCustomerByIdAsync(shoppingCartItemForUpdate.CustomerId);
            // The update time is set in the service.
            var warnings = await _shoppingCartService.UpdateShoppingCartItemAsync(customer, shoppingCartItemForUpdate.Id,
                                                                       shoppingCartItemForUpdate.AttributesXml, shoppingCartItemForUpdate.CustomerEnteredPrice,
                                                                       shoppingCartItemForUpdate.RentalStartDateUtc, shoppingCartItemForUpdate.RentalEndDateUtc,
                                                                       shoppingCartItemForUpdate.Quantity);

            if (warnings.Count > 0)
            {
                var errorMessage = string.Join(",", warnings.ToList());
                return ErrorResponse(errorMessage, HttpStatusCode.BadRequest, warnings.ToList());
            }


            shoppingCartItemForUpdate = await _shoppingCartItemApiService.GetShoppingCartItemAsync(shoppingCartItemForUpdate.Id);

            // Preparing the result dto of the new product category mapping
            var updatedShoppingCart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart);

            var shoppingCartsRootObject = new ShoppingCartItemsRootObject();

            //prepare dto model
            var shoppingCartItemsDtos = await updatedShoppingCart
                                        .SelectAwait(async shoppingCartItem => await _dtoHelper.PrepareShoppingCartItemDTOAsync(shoppingCartItem))
                                        .ToListAsync();

            /*Alchub Start*/
            var vendors = await _vendorService.GetVendorsByProductIdsAsync(updatedShoppingCart.Select(x => x.ProductId).ToArray());

            if (vendors != null)
            {
                foreach (var vendor in vendors)
                {
                    shoppingCartsRootObject.ShoppingCartVendors.Add(new ShoppingCartItemsRootObject.ShoppingCartVendorDto
                    {
                        Id = vendor.Id,
                        Name = string.Format(await _localizationService.GetResourceAsync("Alchub.Vendor.MinimumOrderSubtotal.Vendor.Name"), vendor.Name),
                        Warnings = await GetVendorMinimumOrderAmountWarningsAsync(updatedShoppingCart, vendor),
                        ShoppingCartItems = shoppingCartItemsDtos.Where(x => x.VendorId == vendor.Id).ToList(),
                    });
                }
                shoppingCartsRootObject.ShoppingCartVendors = shoppingCartsRootObject.ShoppingCartVendors?.OrderBy(x => x.Name).ToList();
            }
            shoppingCartsRootObject.count = (int)shoppingCartItemsDtos.Sum(x => x.Quantity);
            shoppingCartsRootObject.CrossSellsProductIds = await GetCrossSellProducts(updatedShoppingCart);
            /*Alchub End*/

            return SuccessResponse(await _localizationService.GetResourceAsync("Nop.Api.ShoppingCart.Updated.success"), shoppingCartsRootObject);
        }

        [HttpDelete]
        [Route("/api/shopping_cart_items/{id}", Name = "DeleteShoppingCartItem")]
        [ProducesResponseType(typeof(void), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ErrorsRootObject), (int)HttpStatusCode.BadRequest)]
        [GetRequestsErrorInterceptorActionFilter]
        public async Task<BaseResponseModel> DeleteShoppingCartItem([FromRoute] int id)
        {
            if (id <= 0)
                return ErrorResponse("invalid id", HttpStatusCode.BadRequest);

            var shoppingCartItemForDelete = await _shoppingCartItemApiService.GetShoppingCartItemAsync(id);

            if (shoppingCartItemForDelete == null)
                return ErrorResponse("shopping_cart_item not found", HttpStatusCode.NotFound);

            if (!await CheckPermissions(shoppingCartItemForDelete.CustomerId, shoppingCartItemForDelete.ShoppingCartType))
                return ErrorResponse("Access Denied!", HttpStatusCode.Forbidden);

            await _shoppingCartService.DeleteShoppingCartItemAsync(shoppingCartItemForDelete);

            //activity log
            await _customerActivityService.InsertActivityAsync("DeleteShoppingCartItem", await _localizationService.GetResourceAsync("ActivityLog.DeleteShoppingCartItem"), shoppingCartItemForDelete);

            return SuccessResponse(await _localizationService.GetResourceAsync("Nop.Api.ShoppingCart.Product.delete.success"), await _localizationService.GetResourceAsync("Nop.Api.ShoppingCart.Product.delete.success"));
        }

        [HttpDelete]
        [Route("/api/shopping_cart_items", Name = "DeleteShoppingCartItems")]
        [ProducesResponseType(typeof(void), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ErrorsRootObject), (int)HttpStatusCode.BadRequest)]
        [GetRequestsErrorInterceptorActionFilter]
        public async Task<BaseResponseModel> DeleteShoppingCartItems([FromQuery] ShoppingCartItemsDeleteParametersModel parameters)
        {
            ShoppingCartType? shoppingCartType = null;

            if (parameters.ShoppingCartType.HasValue && Enum.IsDefined(parameters.ShoppingCartType.Value))
            {
                shoppingCartType = (ShoppingCartType)parameters.ShoppingCartType.Value;
            }

            int customerId;
            if (parameters.CustomerId.HasValue)
            {
                customerId = parameters.CustomerId.Value;
            }
            else
            {
                var currentCustomer = await _authenticationService.GetAuthenticatedCustomerAsync();
                if (currentCustomer is null)
                    return ErrorResponse("Unauthorized!", HttpStatusCode.Unauthorized);

                customerId = currentCustomer.Id;
            }

            //if (!await CheckPermissions(customerId, shoppingCartType))
            //    return ErrorResponse("Access Denied!", HttpStatusCode.Forbidden);

            if (parameters.Ids is { Count: > 0 })
            {
                foreach (int id in parameters.Ids)
                {
                    var item = await _shoppingCartItemApiService.GetShoppingCartItemAsync(id);
                    if (item is not null)
                    {
                        await deleteShoppingCartItem(item);
                    }
                }
            }
            else
            {
                var shoppingCartItemsForDelete = await _shoppingCartItemApiService.GetShoppingCartItems(customerId: customerId, shoppingCartType: shoppingCartType);
                foreach (var item in shoppingCartItemsForDelete)
                {
                    await deleteShoppingCartItem(item);
                }
            }

            async Task deleteShoppingCartItem(ShoppingCartItem item)
            {
                await _shoppingCartService.DeleteShoppingCartItemAsync(item);
                //activity log
                await _customerActivityService.InsertActivityAsync("DeleteShoppingCartItem", await _localizationService.GetResourceAsync("ActivityLog.DeleteShoppingCartItem"), item);
            }

            return SuccessResponse(await _localizationService.GetResourceAsync("Nop.Api.ShoppingCart.Products.delete.success"), await _localizationService.GetResourceAsync("Nop.Api.ShoppingCart.Products.delete.success"));
        }

        #region Private methods

        private async Task<bool> CheckPermissions(int? customerId, ShoppingCartType? shoppingCartType)
        {
            var currentCustomer = await _authenticationService.GetAuthenticatedCustomerAsync();
            if (currentCustomer is null) // authenticated, but does not exist in db
                return false;
            if (customerId.HasValue && currentCustomer.Id == customerId)
            {
                // if I want to handle my own shopping cart, check only public store permission
                switch (shoppingCartType)
                {
                    case ShoppingCartType.ShoppingCart:
                        return await _permissionService.AuthorizeAsync(StandardPermissionProvider.EnableShoppingCart, currentCustomer);
                    case ShoppingCartType.Wishlist:
                        return await _permissionService.AuthorizeAsync(StandardPermissionProvider.EnableWishlist, currentCustomer);
                    default:
                        return await _permissionService.AuthorizeAsync(StandardPermissionProvider.EnableShoppingCart, currentCustomer)
                            && await _permissionService.AuthorizeAsync(StandardPermissionProvider.EnableWishlist, currentCustomer);
                }
            }
            // if I want to handle other customer's shopping carts, check admin permission
            return await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCurrentCarts, currentCustomer);
        }

        private async Task<ShoppingCartItemsRootObject> LoadCurrentShoppingCartItems(ShoppingCartType shoppingCartType, Core.Domain.Customers.Customer customer)
        {
            var updatedShoppingCart = await _shoppingCartService.GetShoppingCartAsync(customer, shoppingCartType);

            var shoppingCartsRootObject = new ShoppingCartItemsRootObject();

            //prepare dto model
            var shoppingCartItemsDtos = await updatedShoppingCart
                                        .SelectAwait(async shoppingCartItem => await _dtoHelper.PrepareShoppingCartItemDTOAsync(shoppingCartItem))
                                        .ToListAsync();

            /*Alchub Start*/
            var vendors = await _vendorService.GetVendorsByProductIdsAsync(updatedShoppingCart.Select(x => x.ProductId).ToArray());

            if (vendors != null)
            {
                foreach (var vendor in vendors)
                {
                    shoppingCartsRootObject.ShoppingCartVendors.Add(new ShoppingCartItemsRootObject.ShoppingCartVendorDto
                    {
                        Id = vendor.Id,
                        Name = vendor.Name,
                        Warnings = await GetVendorMinimumOrderAmountWarningsAsync(updatedShoppingCart, vendor),
                        ShoppingCartItems = shoppingCartItemsDtos.Where(x => x.VendorId == vendor.Id).ToList(),
                    });
                }
                shoppingCartsRootObject.ShoppingCartVendors = shoppingCartsRootObject.ShoppingCartVendors?.OrderBy(x => x.Name).ToList();
            }
            shoppingCartsRootObject.count = (int)shoppingCartItemsDtos.Sum(x => x.Quantity);
            shoppingCartsRootObject.CrossSellsProductIds = await GetCrossSellProducts(updatedShoppingCart);

            //cart warnings
            var cartWarnings = await _shoppingCartService.GetShoppingCartWarningsAsync(updatedShoppingCart, "", false);
            foreach (var warning in cartWarnings)
                shoppingCartsRootObject.Warnings.Add(warning);

            shoppingCartsRootObject.CanCheckout = true;
            if (shoppingCartsRootObject.Warnings.Any())
            {
                shoppingCartsRootObject.CanCheckout = false;
            }
            if (shoppingCartsRootObject.ShoppingCartVendors.Any(x => !string.IsNullOrEmpty(x.Warnings)))
            {
                shoppingCartsRootObject.CanCheckout = false;
            }
            if (shoppingCartsRootObject.ShoppingCartVendors.Any(x => x.ShoppingCartItems.Any(x => x.Warnings.Any())))
            {
                shoppingCartsRootObject.CanCheckout = false;
            }
            /*Alchub End*/

                return shoppingCartsRootObject;
        }

        /// <summary>
        /// Get Vendor Minimum Order Amount Warnings
        /// </summary>
        /// <param name="cart"></param>
        /// <param name="vendor"></param>
        /// <returns></returns>
        protected virtual async Task<string> GetVendorMinimumOrderAmountWarningsAsync(IList<ShoppingCartItem> cart, Vendor vendor)
        {
            string warnings = "";
            decimal vendorSubtotal = decimal.Zero;
            bool isVendorProductAvailable = false;

            if (cart == null)
                return warnings;

            if (vendor == null)
                return warnings;

            if (cart.Any(x => !x.IsPickup))
            {
                foreach (var shoppingCartItem in cart.Where(x => !x.IsPickup).ToList())
                {
                    var product = await _productService.GetProductByIdAsync(shoppingCartItem.ProductId);

                    if (product.VendorId == vendor.Id)
                    {
                        isVendorProductAvailable = true;
                        vendorSubtotal += (await _shoppingCartService.GetSubTotalAsync(shoppingCartItem, true)).subTotal;
                    }
                }

                if (isVendorProductAvailable && vendorSubtotal < vendor.MinimumOrderAmount)
                {
                    warnings = string.Format(await _localizationService.GetResourceAsync("Nop.Vendor.MinimumOrderSubtotal.First.Warning.Message") + " " + await _priceFormatter.FormatPriceAsync(vendor.MinimumOrderAmount - vendorSubtotal, true, false) + " " + await _localizationService.GetResourceAsync("Nop.Vendor.MinimumOrderSubtotal.Second.Warning.Message") + " " + await _priceFormatter.FormatPriceAsync(vendor.MinimumOrderAmount, true, false));
                }
            }
            return warnings;
        }


        protected virtual async Task<IList<int>> GetCrossSellProducts(IList<ShoppingCartItem> cart)
        {
            var crossellproductsIds = new List<int>();

            var cartProductIds = new List<int>();
            foreach (var sci in cart)
            {
                var prodId = sci.ProductId;
                if (!cartProductIds.Contains(prodId))
                    cartProductIds.Add(prodId);
            }
            var productIds = cart.Select(sci => sci.MasterProductId).ToArray();
            var crossSells = await _productApiService.GetCrossSellProductsByProductIdsAsync(productIds);
            foreach (var crossSell in crossSells)
            {
                crossellproductsIds.Add(crossSell.ProductId2);
            }
            return crossellproductsIds.Distinct().ToList();
        }

        #endregion
    }
}
