using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Api.Attributes;
using Nop.Plugin.Api.DTO.Errors;
using Nop.Plugin.Api.DTO.ShoppingCarts;
using Nop.Plugin.Api.Helpers;
using Nop.Plugin.Api.JSON.Serializers;
using Nop.Plugin.Api.Models.BaseModels;
using Nop.Services.Catalog;
using Nop.Services.Common;
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Nop.Plugin.Api.Controllers
{
    public class ShoppingCartController : BaseApiController
    {
        private readonly IDTOHelper _dtoHelper;
        private readonly IStoreContext _storeContext;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly ShoppingCartSettings _shoppingCartSettings;
        private readonly IGiftCardService _giftCardService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IProductService _productService;
        private readonly ISlotService _slotService;
        private readonly IDateTimeHelper _dateTimeHelper;

        #region Fields

        #endregion

        #region Ctor

        public ShoppingCartController(
            IJsonFieldsSerializer jsonFieldsSerializer,
            IAclService aclService,
            ICustomerService customerService,
            IStoreMappingService storeMappingService,
            IStoreService storeService,
            IDiscountService discountService,
            ICustomerActivityService customerActivityService,
            ILocalizationService localizationService,
            IPictureService pictureService,
            IDTOHelper dtoHelper,
            IStoreContext storeContext,
            IShoppingCartService shoppingCartService,
            ShoppingCartSettings shoppingCartSettings,
            IGiftCardService giftCardService,
            IGenericAttributeService genericAttributeService,
            IProductService productService,
            ISlotService slotService,
            IDateTimeHelper dateTimeHelper)
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
            _dtoHelper = dtoHelper;
            _storeContext = storeContext;
            _shoppingCartService = shoppingCartService;
            _shoppingCartSettings = shoppingCartSettings;
            _giftCardService = giftCardService;
            _genericAttributeService = genericAttributeService;
            _productService = productService;
            _slotService = slotService;
            _dateTimeHelper = dateTimeHelper;
        }

        #endregion

        /// <summary>
        ///  Recive order total summary
        /// </summary>
        /// <response code="200">OK</response>
        /// <response code="400">Bad Request</response>
        /// <response code="401">Unauthorized</response>
        [HttpGet]
        [Route("/api/shopping_cart/order_totals", Name = "GetOrderTotals")]
        [ProducesResponseType(typeof(OrderTotalDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ErrorsRootObject), (int)HttpStatusCode.BadRequest)]
        [GetRequestsErrorInterceptorActionFilter]
        public async Task<BaseResponseModel> GetOrderTotals([FromQuery] int customerId)
        {
            if (customerId <= 0)
                return ErrorResponse("invalid customerId parameter", HttpStatusCode.BadRequest);

            //get customer
            var customer = await _customerService.GetCustomerByIdAsync(customerId);
            if (customer == null)
                return ErrorResponse("customer not found!", HttpStatusCode.NotFound);

            var orderTotals = await _dtoHelper.PrepareOrderTotalsDtoAsync(customer, await _storeContext.GetCurrentStoreAsync());

            return SuccessResponse(await _localizationService.GetResourceAsync("Nop.Api.ShoppingCart.OrderTotals"), orderTotals);
        }

        /// <summary>
        ///  Recive applied coupen code
        /// </summary>
        /// <response code="200">OK</response>
        /// <response code="400">Bad Request</response>
        /// <response code="401">Unauthorized</response>
        [HttpGet]
        [Route("/api/shopping_cart/discount_code", Name = "GetAppliedDiscountCodes")]
        [ProducesResponseType(typeof(DiscountBoxDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ErrorsRootObject), (int)HttpStatusCode.BadRequest)]
        [GetRequestsErrorInterceptorActionFilter]
        public async Task<BaseResponseModel> GetAppliedDiscountCode([FromQuery] int customerId)
        {
            if (customerId <= 0)
                return ErrorResponse("invalid customerId parameter", HttpStatusCode.BadRequest);

            //get customer
            var customer = await _customerService.GetCustomerByIdAsync(customerId);
            if (customer == null)
                return ErrorResponse("customer not found!", HttpStatusCode.NotFound);

            //prepare model
            var model = await _dtoHelper.PrepareDiscountBoxDtoAsync(customer);

            return SuccessResponse(await _localizationService.GetResourceAsync("Nop.Api.ShoppingCart.AppliedDiscount"), model);
        }

        /// <summary>
        ///  Apply discount coupone code
        /// </summary>
        /// <response code="200">OK</response>
        /// <response code="400">Bad Request</response>
        /// <response code="401">Unauthorized</response>
        [HttpPost]
        [Route("/api/shopping_cart/discount_code", Name = "ApplyDiscountCode")]
        [ProducesResponseType(typeof(ApplyDiscountCodeResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ErrorsRootObject), (int)HttpStatusCode.BadRequest)]
        [GetRequestsErrorInterceptorActionFilter]
        public async Task<BaseResponseModel> ApplyDiscountCode([FromBody] ApplyDiscountCodeRequest request)
        {
            if (string.IsNullOrEmpty(request.DiscountCouponCode))
                return ErrorResponse("invalid discountcouponcode parameter", HttpStatusCode.BadRequest);

            if (request.CustomerId <= 0)
                return ErrorResponse("invalid customerId parameter", HttpStatusCode.BadRequest);

            //get customer
            var customer = await _customerService.GetCustomerByIdAsync(request.CustomerId);
            if (customer == null)
                return ErrorResponse("customer not found!", HttpStatusCode.NotFound);
            var discountCouponCodes = await _customerService.ParseAppliedDiscountCouponCodesAsync(customer);

            //trim
            request.DiscountCouponCode = request.DiscountCouponCode.Trim();

            //cart
            var store = await _storeContext.GetCurrentStoreAsync();
            var cart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, store.Id);
            var model = new ApplyDiscountCodeResponse();
            if (discountCouponCodes.Any())
            {
                model.Messages.Add(await _localizationService.GetResourceAsync("ShoppingCart.DiscountCouponCode.OnlyOneCouponCodeCanApply"));
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(request.DiscountCouponCode))
                {
                    //we find even hidden records here. this way we can display a user-friendly message if it's expired
                    var discounts = (await _discountService.GetAllDiscountsAsync(couponCode: request.DiscountCouponCode, showHidden: true))
                        .Where(d => d.RequiresCouponCode)
                        .ToList();
                    if (discounts.Any())
                    {
                        var userErrors = new List<string>();
                        var anyValidDiscount = await discounts.AnyAwaitAsync(async discount =>
                        {
                            var validationResult = await _discountService.ValidateDiscountAsync(discount, customer, new[] { request.DiscountCouponCode });
                            userErrors.AddRange(validationResult.Errors);

                            return validationResult.IsValid;
                        });

                        if (anyValidDiscount)
                        {
                            //valid
                            await _customerService.ApplyDiscountCouponCodeAsync(customer, request.DiscountCouponCode);
                            model.Messages.Add(await _localizationService.GetResourceAsync("ShoppingCart.DiscountCouponCode.Applied"));
                            model.IsApplied = true;
                        }
                        else
                        {
                            if (userErrors.Any())
                                //some user errors
                                model.Messages = userErrors;
                            else
                                //general error text
                                model.Messages.Add(await _localizationService.GetResourceAsync("ShoppingCart.DiscountCouponCode.WrongDiscount"));
                        }
                    }
                    else
                        //discount cannot be found
                        model.Messages.Add(await _localizationService.GetResourceAsync("ShoppingCart.DiscountCouponCode.CannotBeFound"));
                }
                else
                    //empty coupon code
                    model.Messages.Add(await _localizationService.GetResourceAsync("ShoppingCart.DiscountCouponCode.Empty"));
            }

            return SuccessResponse(!model.Messages.Any() ? await _localizationService.GetResourceAsync("Nop.Api.ShoppingCart.DiscountBox") : model.Messages.FirstOrDefault(), model);
        }

        /// <summary>
        ///  Apply discount coupone code
        /// </summary>
        /// <response code="200">OK</response>
        /// <response code="400">Bad Request</response>
        /// <response code="401">Unauthorized</response>
        [HttpDelete]
        [Route("/api/shopping_cart/discount_code", Name = "RemoveDiscountCode")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ErrorsRootObject), (int)HttpStatusCode.BadRequest)]
        [GetRequestsErrorInterceptorActionFilter]
        public async Task<BaseResponseModel> RemoveDiscountCode([FromQuery] int customerId, [FromQuery] int discountId)
        {
            if (discountId <= 0)
                return ErrorResponse("invalid discountId parameter", HttpStatusCode.BadRequest);

            if (customerId <= 0)
                return ErrorResponse("invalid customerId parameter", HttpStatusCode.BadRequest);

            //get discount
            var discount = await _discountService.GetDiscountByIdAsync(discountId);
            if (discount == null)
                return ErrorResponse("discount not found with specific id!", HttpStatusCode.NotFound);

            //get customer
            var customer = await _customerService.GetCustomerByIdAsync(customerId);
            if (customer == null)
                return ErrorResponse("customer not found!", HttpStatusCode.NotFound);

            //get applied coupon codes
            var existingCouponCodes = await _customerService.ParseAppliedDiscountCouponCodesAsync(customer);
            if (!existingCouponCodes.Any(x => x.Equals(discount.CouponCode)))
                return ErrorResponse(await _localizationService.GetResourceAsync("Nop.Api.ShoppingCart.DiscountCoupon.NotAppliedToShoppingCart"), HttpStatusCode.BadRequest);

            //remove discount
            await _customerService.RemoveDiscountCouponCodeAsync(customer, discount.CouponCode);

            return SuccessResponse(await _localizationService.GetResourceAsync("Nop.Api.ShoppingCart.DiscountCode.Removed"), await _localizationService.GetResourceAsync("Nop.Api.ShoppingCart.DiscountCode.Removed"));
        }

        /// <summary>
        ///  Recive value which indicates whether to show gift card box or not.
        /// </summary>
        /// <response code="200">OK</response>
        /// <response code="400">Bad Request</response>
        /// <response code="401">Unauthorized</response>
        [HttpGet]
        [Route("/api/shopping_cart/giftcard_code", Name = "GetAppliedGiftCards")]
        [ProducesResponseType(typeof(GiftCardBoxDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ErrorsRootObject), (int)HttpStatusCode.BadRequest)]
        [GetRequestsErrorInterceptorActionFilter]
        public async Task<BaseResponseModel> GetDisplayGiftCard([FromQuery] int customerId)
        {
            if (customerId <= 0)
                return ErrorResponse("invalid customerId parameter", HttpStatusCode.BadRequest);

            //get customer
            var customer = await _customerService.GetCustomerByIdAsync(customerId);
            if (customer == null)
                return ErrorResponse("customer not found!", HttpStatusCode.NotFound);

            //prepare model
            var model = await _dtoHelper.PrepareGiftCardBoxDto(customer, await _storeContext.GetCurrentStoreAsync());

            return SuccessResponse(await _localizationService.GetResourceAsync("Nop.Api.ShoppingCart.GifCardBox"), model);
        }

        /// <summary>
        ///  Apply gift card code
        /// </summary>
        /// <response code="200">OK</response>
        /// <response code="400">Bad Request</response>
        /// <response code="401">Unauthorized</response>
        [HttpPost]
        [Route("/api/shopping_cart/giftcard_code", Name = "ApplyGiftCard")]
        [ProducesResponseType(typeof(ApplyGiftCardResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ErrorsRootObject), (int)HttpStatusCode.BadRequest)]
        [GetRequestsErrorInterceptorActionFilter]
        public async Task<BaseResponseModel> ApplyGiftCardCode([FromBody] ApplyGiftCardRequest request)
        {
            if (string.IsNullOrEmpty(request.GiftCardCouponCode))
                return ErrorResponse("invalid GiftCardCouponCode parameter", HttpStatusCode.BadRequest);

            if (request.CustomerId <= 0)
                return ErrorResponse("invalid customerId parameter", HttpStatusCode.BadRequest);

            //get customer
            var customer = await _customerService.GetCustomerByIdAsync(request.CustomerId);
            if (customer == null)
                return ErrorResponse("customer not found!", HttpStatusCode.NotFound);

            //cart
            var store = await _storeContext.GetCurrentStoreAsync();
            var cart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, store.Id);

            var model = new ApplyGiftCardResponse();
            if (!await _shoppingCartService.ShoppingCartIsRecurringAsync(cart))
            {
                if (!string.IsNullOrWhiteSpace(request.GiftCardCouponCode))
                {
                    var giftCard = (await _giftCardService.GetAllGiftCardsAsync(giftCardCouponCode: request.GiftCardCouponCode)).FirstOrDefault();
                    var isGiftCardValid = giftCard != null && await _giftCardService.IsGiftCardValidAsync(giftCard);
                    if (isGiftCardValid)
                    {
                        await _customerService.ApplyGiftCardCouponCodeAsync(customer, request.GiftCardCouponCode);
                        model.Message = await _localizationService.GetResourceAsync("ShoppingCart.GiftCardCouponCode.Applied");
                        model.IsApplied = true;
                    }
                    else
                    {
                        model.Message = await _localizationService.GetResourceAsync("ShoppingCart.GiftCardCouponCode.WrongGiftCard");
                        model.IsApplied = false;
                    }
                }
                else
                {
                    model.Message = await _localizationService.GetResourceAsync("ShoppingCart.GiftCardCouponCode.WrongGiftCard");
                    model.IsApplied = false;
                }
            }
            else
            {
                model.Message = await _localizationService.GetResourceAsync("ShoppingCart.GiftCardCouponCode.DontWorkWithAutoshipProducts");
                model.IsApplied = false;
            }

            return SuccessResponse(model.Message, model);
        }

        /// <summary>
        ///  Apply discount coupone code
        /// </summary>
        /// <response code="200">OK</response>
        /// <response code="400">Bad Request</response>
        /// <response code="401">Unauthorized</response>
        [HttpDelete]
        [Route("/api/shopping_cart/giftcard_code", Name = "RemoveGiftCardCode")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ErrorsRootObject), (int)HttpStatusCode.BadRequest)]
        [GetRequestsErrorInterceptorActionFilter]
        public async Task<BaseResponseModel> RemoveGiftCardCode([FromQuery] int customerId, [FromQuery] int giftCardId)
        {
            if (giftCardId <= 0)
                return ErrorResponse("invalid giftcardId parameter", HttpStatusCode.BadRequest);

            if (customerId <= 0)
                return ErrorResponse("invalid customerId parameter", HttpStatusCode.BadRequest);

            //get customer
            var customer = await _customerService.GetCustomerByIdAsync(customerId);
            if (customer == null)
                return ErrorResponse("customer not found!", HttpStatusCode.NotFound);

            //get gift card identifier
            var giftcCard = await _giftCardService.GetGiftCardByIdAsync(giftCardId);
            if (giftcCard is null)
                return ErrorResponse("giftcard not found with specific id!", HttpStatusCode.BadRequest);

            //get applied coupon codes
            var existingCouponCodes = await _customerService.ParseAppliedGiftCardCouponCodesAsync(customer);
            if (!existingCouponCodes.Any(x => x.Equals(giftcCard.GiftCardCouponCode)))
                return ErrorResponse(await _localizationService.GetResourceAsync("Nop.Api.ShoppingCart.GiftCard.NotAppliedToShoppingCart"), HttpStatusCode.BadRequest);

            //remove gifcard
            await _customerService.RemoveGiftCardCouponCodeAsync(customer, giftcCard.GiftCardCouponCode);

            return SuccessResponse(await _localizationService.GetResourceAsync("Nop.Api.ShoppingCart.GiftCardCode.Removed"), await _localizationService.GetResourceAsync("Nop.Api.ShoppingCart.GiftCardCode.Removed"));
        }


        #region ApplyTip

        [HttpPost]
        [Route("/api/shopping_cart/apply_Tip", Name = "ApplyTip")]
        public async Task<BaseResponseModel> ApplyTipAsync([FromBody] AddTipRequestModel model)
        {
            var customer = await _customerService.GetCustomerByIdAsync(model.CustomerId);

            if (customer == null)
                return ErrorResponse("customer not found", HttpStatusCode.NotFound);

            try
            {
                await _genericAttributeService.SaveAttributeAsync<int?>(customer, NopCustomerDefaults.TipTypeIdAttribute, model.TipTypeId);
                await _genericAttributeService.SaveAttributeAsync<decimal?>(customer, NopCustomerDefaults.CustomTipAmountAttribute, model.TipTypeId == 0 ? model.CustomTipAmount : null);
                return SuccessResponse(await _localizationService.GetResourceAsync("Nop.Api.ShoppingCart.Apply.Tip.success"));
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex.Message, HttpStatusCode.BadRequest);
            }
        }

        #endregion

        #region Alert Two Messge Shopping Cart Add Items

        /// <summary>
        /// Show Add To Cart Confirm Box
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="form"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("/api/shopping_cart/show_add_To_cart_confirmBox", Name = "show_add_To_cart_confirmBox")]
        public async Task<BaseResponseModel> ShowAddToCartConfirmBoxAsync([FromBody] AlertRequestModel model)
        {
            try
            {
                var product = await _productService.GetProductByIdAsync(model.ProductId);
                if (product == null)
                    return ErrorResponse("No product found", HttpStatusCode.BadRequest);

                var customer = await _customerService.GetCustomerByIdAsync(model.CustomerId);
                if (customer == null)
                    return ErrorResponse("customer not found", HttpStatusCode.NotFound);

                List<bool> alertList = new List<bool>();
                bool pickup = false;
                if (model.IsPickup)
                {
                    pickup = true;
                    return SuccessResponse(await _localizationService.GetResourceAsync("Nop.Api.Show.Alert.Multi.Slot"), false);
                }
                var dateTime = await _dateTimeHelper.ConvertToUserTimeAsync(DateTime.Now);
                var customerProductSlot = await _slotService.GetCustomerProductSlotDeliveryPickupId(model.ProductId, customer.Id, dateTime, ispickup: pickup);
                if (customerProductSlot == null)
                {
                    return SuccessResponse(await _localizationService.GetResourceAsync("Nop.Api.Show.Alert.Multi.Slot"), false);
                }
                else
                {
                    if (customerProductSlot.IsPickup)
                    {
                        return SuccessResponse(await _localizationService.GetResourceAsync("Nop.Api.Show.Alert.Multi.Slot"), false);
                    }
                    else
                    {
                        var cart = await _shoppingCartService.GetShoppingCartAsync(customer: customer, shoppingCartType: ShoppingCartType.ShoppingCart);
                        if (cart != null && cart.Count > 0)
                        {
                            foreach (var sci in cart)
                            {
                                var scproduct = await _productService.GetProductByIdAsync(sci.ProductId);
                                if (scproduct != null && scproduct.VendorId == product.VendorId && scproduct.Id != model.ProductId && (sci.SlotTime != customerProductSlot.EndTime || sci.SlotStartTime != customerProductSlot.StartTime))
                                {
                                    alertList.Add(true);
                                }
                                else
                                {
                                    alertList.Add(false);
                                }

                            }
                            if (alertList.Any(x => x == false))
                            {
                                return SuccessResponse(await _localizationService.GetResourceAsync("Nop.Api.Show.Alert.MultiSlot"), false);
                            }
                            else
                            {
                                return SuccessResponse(await _localizationService.GetResourceAsync("Nop.Api.Show.Alert.MultiSlot"), true);
                            }
                        }
                    }
                }
                return SuccessResponse(await _localizationService.GetResourceAsync("Nop.Api.Show.Alert.Multi.Slot"), false);
            }
            catch (Exception ex) { return ErrorResponse(ex.Message, HttpStatusCode.BadRequest); }
        }

        /// <summary>
        /// Show Add To Cart Confirm Box for purchase product from multi vendor
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="form"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("/api/shopping_cart/show_add_to_cart_multi_vendors", Name = "show_add_to_cart_multi_vendors")]
        public async Task<BaseResponseModel> ShowAddToCartConfirmBoxForMultiVendorsSelect([FromBody] AlertRequestModel model)
        {
            try
            {

                var product = await _productService.GetProductByIdAsync(model.ProductId);
                if (product == null)
                    return ErrorResponse("No product found", HttpStatusCode.BadRequest);

                var customer = await _customerService.GetCustomerByIdAsync(model.CustomerId);

                if (customer == null)
                    return ErrorResponse("customer not found", HttpStatusCode.NotFound);

                var cart = await _shoppingCartService.GetShoppingCartAsync(customer: customer, shoppingCartType: ShoppingCartType.ShoppingCart);
                if (cart == null)
                    return SuccessResponse(await _localizationService.GetResourceAsync("Nop.Api.Show.Alert.Mutli.Vendor"), false);

                var vendorIds = new List<int>();
                foreach (var sci in cart)
                {
                    //get subproducts of the master product
                    var scproduct = await _productService.GetProductByIdAsync(sci.ProductId);
                    if (scproduct != null)
                        vendorIds.Add(scproduct.VendorId);
                }
                //check vendorId exists or not for sc product, if exists display popUp
                if (vendorIds.Any() && !vendorIds.Contains(product.VendorId))
                    return SuccessResponse(await _localizationService.GetResourceAsync("Nop.Api.Show.Alert.Mutli.Vendor"), true);

                return SuccessResponse(await _localizationService.GetResourceAsync("Nop.Api.Show.Alert.Mutli.Vendor"), false);
            }
            catch (Exception ex) { return ErrorResponse(ex.Message, HttpStatusCode.BadRequest); }
        }

        #endregion

        [HttpPost]
        [Route("/api/shopping_cart/delete_wish_list_item", Name = "delete_wish_list_item")]
        public async Task<BaseResponseModel> DeleteWishListItem([FromBody] WishListRequestModel model)
        {
            try
            {
                var store = await _storeContext.GetCurrentStoreAsync();
                var customer = await _customerService.GetCustomerByIdAsync(model.CustomerId);
                if (customer == null)
                {
                    return ErrorResponse("customer not found", HttpStatusCode.NotFound);
                }
                var product = await _productService.GetProductByIdAsync(model.ProductId);
                if (product == null)
                {
                    return ErrorResponse("item not found", HttpStatusCode.NotFound);
                }

                var shoppingCart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.Wishlist, store.Id);
                var cart = shoppingCart.Where(x => x.ShoppingCartTypeId == (int)ShoppingCartType.Wishlist && x.ProductId == product.Id).ToList();

                var itemToDelete = await _shoppingCartService.FindShoppingCartItemInTheCartAsync(
                    cart, ShoppingCartType.Wishlist, product, null, decimal.Zero, null, null);

                if (itemToDelete != null)
                {
                    await _shoppingCartService.DeleteShoppingCartItemAsync(itemToDelete);
                }
                return SuccessResponse(await _localizationService.GetResourceAsync("Nop.Api.Wishlist.Products.delete.success"), true);
            }
            catch (Exception ex) { return ErrorResponse(ex.Message, HttpStatusCode.BadRequest); }
        }
    }
}
