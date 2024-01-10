using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Security;
using Nop.Core.Domain.Shipping;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Discounts;
using Nop.Services.Helpers;
using Nop.Services.Html;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Shipping;
using Nop.Services.Slots;
using Nop.Services.Tax;
using Nop.Web.Factories;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Mvc.Filters;
using Nop.Web.Infrastructure.Cache;
using Nop.Web.Models.Media;
using Nop.Web.Models.ShoppingCart;

namespace Nop.Web.Controllers
{
    public partial class ShoppingCartController : BasePublicController
    {
        #region Fields

        private readonly CaptchaSettings _captchaSettings;
        private readonly CustomerSettings _customerSettings;
        private readonly ICheckoutAttributeParser _checkoutAttributeParser;
        private readonly ICheckoutAttributeService _checkoutAttributeService;
        private readonly ICurrencyService _currencyService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly ICustomerService _customerService;
        private readonly IDiscountService _discountService;
        private readonly IDownloadService _downloadService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IGiftCardService _giftCardService;
        private readonly IHtmlFormatter _htmlFormatter;
        private readonly ILocalizationService _localizationService;
        private readonly INopFileProvider _fileProvider;
        private readonly INotificationService _notificationService;
        private readonly IPermissionService _permissionService;
        private readonly IPictureService _pictureService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly IProductAttributeService _productAttributeService;
        private readonly IProductService _productService;
        private readonly IShippingService _shippingService;
        private readonly IShoppingCartModelFactory _shoppingCartModelFactory;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IStaticCacheManager _staticCacheManager;
        private readonly IStoreContext _storeContext;
        private readonly ITaxService _taxService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IWebHelper _webHelper;
        private readonly IWorkContext _workContext;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly MediaSettings _mediaSettings;
        private readonly OrderSettings _orderSettings;
        private readonly ShoppingCartSettings _shoppingCartSettings;
        private readonly ShippingSettings _shippingSettings;
        private readonly ISlotService _slotService;
        private readonly IRepository<ShoppingCartItem> _sciRepository;
        private readonly ISettingService _settingService;
        private readonly ICategoryService _categoryService;
        private readonly IDateTimeHelper _dateTimeHelper;
        #endregion

        #region Ctor

        public ShoppingCartController(CaptchaSettings captchaSettings,
            CustomerSettings customerSettings,
            ICheckoutAttributeParser checkoutAttributeParser,
            ICheckoutAttributeService checkoutAttributeService,
            ICurrencyService currencyService,
            ICustomerActivityService customerActivityService,
            ICustomerService customerService,
            IDiscountService discountService,
            IDownloadService downloadService,
            IGenericAttributeService genericAttributeService,
            IGiftCardService giftCardService,
            IHtmlFormatter htmlFormatter,
            ILocalizationService localizationService,
            INopFileProvider fileProvider,
            INotificationService notificationService,
            IPermissionService permissionService,
            IPictureService pictureService,
            IPriceFormatter priceFormatter,
            IProductAttributeParser productAttributeParser,
            IProductAttributeService productAttributeService,
            IProductService productService,
            IShippingService shippingService,
            IShoppingCartModelFactory shoppingCartModelFactory,
            IShoppingCartService shoppingCartService,
            IStaticCacheManager staticCacheManager,
            IStoreContext storeContext,
            ITaxService taxService,
            IUrlRecordService urlRecordService,
            IWebHelper webHelper,
            IWorkContext workContext,
            IWorkflowMessageService workflowMessageService,
            MediaSettings mediaSettings,
            OrderSettings orderSettings,
            ShoppingCartSettings shoppingCartSettings,
            ShippingSettings shippingSettings,
            ISlotService slotService,
            IRepository<ShoppingCartItem> sciRepository,
            ISettingService settingService,
            ICategoryService categoryService,
            IDateTimeHelper dateTimeHelper)
        {
            _captchaSettings = captchaSettings;
            _customerSettings = customerSettings;
            _checkoutAttributeParser = checkoutAttributeParser;
            _checkoutAttributeService = checkoutAttributeService;
            _currencyService = currencyService;
            _customerActivityService = customerActivityService;
            _customerService = customerService;
            _discountService = discountService;
            _downloadService = downloadService;
            _genericAttributeService = genericAttributeService;
            _giftCardService = giftCardService;
            _htmlFormatter = htmlFormatter;
            _localizationService = localizationService;
            _fileProvider = fileProvider;
            _notificationService = notificationService;
            _permissionService = permissionService;
            _pictureService = pictureService;
            _priceFormatter = priceFormatter;
            _productAttributeParser = productAttributeParser;
            _productAttributeService = productAttributeService;
            _productService = productService;
            _shippingService = shippingService;
            _shoppingCartModelFactory = shoppingCartModelFactory;
            _shoppingCartService = shoppingCartService;
            _staticCacheManager = staticCacheManager;
            _storeContext = storeContext;
            _taxService = taxService;
            _urlRecordService = urlRecordService;
            _webHelper = webHelper;
            _workContext = workContext;
            _workflowMessageService = workflowMessageService;
            _mediaSettings = mediaSettings;
            _orderSettings = orderSettings;
            _shoppingCartSettings = shoppingCartSettings;
            _shippingSettings = shippingSettings;
            _slotService = slotService;
            _sciRepository = sciRepository;
            _settingService = settingService;
            _categoryService = categoryService;
            _dateTimeHelper = dateTimeHelper;
        }

        #endregion

        #region Utilities

        protected virtual async Task SaveItemAsync(ShoppingCartItem updatecartitem, List<string> addToCartWarnings, Product product,
 ShoppingCartType cartType, string attributes, decimal customerEnteredPriceConverted, DateTime? rentalStartDate,
 DateTime? rentalEndDate, int quantity, bool ispickup = false, int masterProductId = 0, int groupedProductId = 0)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            var store = await _storeContext.GetCurrentStoreAsync();
            if (updatecartitem == null)
            {
                //add to the cart
                addToCartWarnings.AddRange(await _shoppingCartService.AddToCartAsync(customer,
                    product, cartType, store.Id,
                    attributes, customerEnteredPriceConverted,
                    rentalStartDate, rentalEndDate, quantity, true, ispickup, masterProductId, groupedProductId));
            }
            else
            {
                var cart = await _shoppingCartService.GetShoppingCartAsync(customer, updatecartitem.ShoppingCartType, store.Id);

                var otherCartItemWithSameParameters = await _shoppingCartService.FindShoppingCartItemInTheCartAsync(
                    cart, updatecartitem.ShoppingCartType, product, attributes, customerEnteredPriceConverted,
                    rentalStartDate, rentalEndDate);
                if (otherCartItemWithSameParameters != null &&
                    otherCartItemWithSameParameters.Id == updatecartitem.Id)
                {
                    //ensure it's some other shopping cart item
                    otherCartItemWithSameParameters = null;
                }
                //update existing item
                addToCartWarnings.AddRange(await _shoppingCartService.UpdateShoppingCartItemAsync(customer,
                    updatecartitem.Id, attributes, customerEnteredPriceConverted,
                    rentalStartDate, rentalEndDate, quantity + (otherCartItemWithSameParameters?.Quantity ?? 0), true, ispickup, masterProductId, groupedProductId));
                if (otherCartItemWithSameParameters != null && !addToCartWarnings.Any())
                {
                    //delete the same shopping cart item (the other one)
                    await _shoppingCartService.DeleteShoppingCartItemAsync(otherCartItemWithSameParameters);
                }
            }
        }
        #endregion

        #region Shopping cart

        /// <summary>
        /// Show Add To Cart Confirm Box
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="form"></param>
        /// <returns></returns>
        [HttpPost]
        public virtual async Task<IActionResult> ShowAddToCartConfirmBoxAsync(int productId, bool isPickup, IFormCollection form)
        {
            try
            {
                var product = await _productService.GetProductByIdAsync(productId);
                if (product == null)
                    return Json(new { showAddToCartConfirmBox = false });

                var dateTime = await _dateTimeHelper.ConvertToUserTimeAsync(DateTime.Now);
                var customer = await _workContext.GetCurrentCustomerAsync();
                var customerProductSlot = await _slotService.GetCustomerProductSlotDeliveryPickupId(productId, customer.Id, dateTime, ispickup: isPickup);
                List<bool> alertList = new List<bool>();
                if (customerProductSlot == null)
                {
                    return Json(new { showAddToCartConfirmBox = false });
                }
                else
                {
                    if (customerProductSlot.IsPickup)
                    {
                        return Json(new { showAddToCartConfirmBox = false });
                    }
                    else
                    {
                        var cart = await _shoppingCartService.GetShoppingCartAsync(customer: customer, shoppingCartType: ShoppingCartType.ShoppingCart);
                        if (cart != null && cart.Count > 0)
                        {
                            foreach (var sci in cart)
                            {
                                var scproduct = await _productService.GetProductByIdAsync(sci.ProductId);
                                if (scproduct != null && scproduct.VendorId == product.VendorId && scproduct.Id != productId && (sci.SlotTime != customerProductSlot.EndTime || sci.SlotStartTime != customerProductSlot.StartTime))
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
                                return Json(new { showAddToCartConfirmBox = false });
                            }
                            else
                            {
                                return Json(new { showAddToCartConfirmBox = true });
                            }
                        }
                    }
                }

                return Json(new { showAddToCartConfirmBox = false });
            }
            catch (Exception) { return Json(new { showAddToCartConfirmBox = false }); }
        }

        /// <summary>
        /// Show Add To Cart Confirm Box for purchase product from multi vendor
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="form"></param>
        /// <returns></returns>
        [HttpPost]
        public virtual async Task<IActionResult> ShowAddToCartConfirmBoxForMultiVendorsSelect(int masterProductId, int productId, IFormCollection form)
        {
            try
            {

                var productToBeAddToCart = await _productService.GetProductByIdAsync(productId);
                if (productToBeAddToCart == null)
                    return Json(new { showAddToCartConfirmBox = false });

                var customer = await _workContext.GetCurrentCustomerAsync();
                var cart = await _shoppingCartService.GetShoppingCartAsync(customer: customer, shoppingCartType: ShoppingCartType.ShoppingCart);
                if (cart == null)
                    return Json(new { showAddToCartConfirmBox = false });

                var vendorIds = new List<int>();
                foreach (var sci in cart)
                {
                    //get subproducts of the master product
                    var scproduct = await _productService.GetProductByIdAsync(sci.ProductId);
                    if (scproduct != null)
                        vendorIds.Add(scproduct.VendorId);
                }

                //check vendorId exists or not for sc product, if exists display popUp
                if (vendorIds.Any() && !vendorIds.Contains(productToBeAddToCart.VendorId))
                    return Json(new { showMultiVendorConfirmBox = true });

                return Json(new { showAddToCartConfirmBox = false });
            }
            catch (Exception) { return Json(new { showAddToCartConfirmBox = false }); }
        }

        //add product to cart using AJAX
        //currently we use this method on the product details pages
        [HttpPost]
        public virtual async Task<IActionResult> AddProductToCart_Details(int productId, int shoppingCartTypeId, int masterProductId, int groupedProductId,
            int quantity, bool ispickup, IFormCollection form)
        {
            var product = await _productService.GetProductByIdAsync(productId);
            if (product == null)
            {
                return Json(new
                {
                    redirect = Url.RouteUrl("Homepage")
                });
            }

            //we can add only simple products
            if (product.ProductType != ProductType.SimpleProduct)
            {
                return Json(new
                {
                    success = false,
                    message = "Only simple products could be added to the cart"
                });
            }

            //alchub++ master product validation
            //Note: groupedProductId can be 0, so do not validate
            if (masterProductId == 0)
            {
                return Json(new
                {
                    success = false,
                    message = "Invalid Master product identifier"
                });
            }

            //update existing shopping cart item
            var updatecartitemid = 0;
            foreach (var formKey in form.Keys)
                if (formKey.Equals($"addtocart_{productId}.UpdatedShoppingCartItemId", StringComparison.InvariantCultureIgnoreCase))
                {
                    _ = int.TryParse(form[formKey], out updatecartitemid);
                    break;
                }

            ShoppingCartItem updatecartitem = null;
            if (_shoppingCartSettings.AllowCartItemEditing && updatecartitemid > 0)
            {
                var store = await _storeContext.GetCurrentStoreAsync();
                //search with the same cart type as specified
                var cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), (ShoppingCartType)shoppingCartTypeId, store.Id);

                updatecartitem = cart.FirstOrDefault(x => x.Id == updatecartitemid);
                //not found? let's ignore it. in this case we'll add a new item
                //if (updatecartitem == null)
                //{
                //    return Json(new
                //    {
                //        success = false,
                //        message = "No shopping cart item found to update"
                //    });
                //}
                //is it this product?
                if (updatecartitem != null && product.Id != updatecartitem.ProductId)
                {
                    return Json(new
                    {
                        success = false,
                        message = "This product does not match a passed shopping cart item identifier"
                    });
                }
            }

            var addToCartWarnings = new List<string>();

            //customer entered price
            var customerEnteredPriceConverted = await _productAttributeParser.ParseCustomerEnteredPriceAsync(product, form);

            //entered quantity
            //var quantity = _productAttributeParser.ParseEnteredQuantity(product, form);
            quantity = quantity == 0 ? 1 : quantity;

            //product and gift card attributes
            var attributes = await _productAttributeParser.ParseProductAttributesAsync(product, form, addToCartWarnings);

            //rental attributes
            _productAttributeParser.ParseRentalDates(product, form, out var rentalStartDate, out var rentalEndDate);

            var cartType = updatecartitem == null ? (ShoppingCartType)shoppingCartTypeId :
                //if the item to update is found, then we ignore the specified "shoppingCartTypeId" parameter
                updatecartitem.ShoppingCartType;

            await SaveItemAsync(updatecartitem, addToCartWarnings, product, cartType, attributes, customerEnteredPriceConverted, rentalStartDate, rentalEndDate, quantity, ispickup, masterProductId, groupedProductId);

            //return result
            return await GetProductToCartDetailsAsync(addToCartWarnings, cartType, product);
        }

        [HttpPost, ActionName("Cart")]
        [FormValueRequired("applytip")]
        public virtual async Task<IActionResult> ApplyTipAsync(int tipTypeId, decimal customTipAmount)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            var store = await _storeContext.GetCurrentStoreAsync();
            var cart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, store.Id);

            await _genericAttributeService.SaveAttributeAsync<int?>(customer, NopCustomerDefaults.TipTypeIdAttribute, tipTypeId);
            await _genericAttributeService.SaveAttributeAsync<decimal?>(customer, NopCustomerDefaults.CustomTipAmountAttribute, tipTypeId == 0 ? customTipAmount : null);

            var model = new ShoppingCartModel();

            model = await _shoppingCartModelFactory.PrepareShoppingCartModelAsync(model, cart);

            return View(model);
        }

        [HttpPost, ActionName("Cart")]
        [FormValueRequired("applydiscountcouponcode")]
        public virtual async Task<IActionResult> ApplyDiscountCoupon(string discountcouponcode, IFormCollection form)
        {
            //trim
            if (discountcouponcode != null)
                discountcouponcode = discountcouponcode.Trim();

            //cart
            var customer = await _workContext.GetCurrentCustomerAsync();
            var store = await _storeContext.GetCurrentStoreAsync();
            var cart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, store.Id);
            var discountCouponCodes = await _customerService.ParseAppliedDiscountCouponCodesAsync(customer);

            //parse and save checkout attributes
            await ParseAndSaveCheckoutAttributesAsync(cart, form);

            var model = new ShoppingCartModel();
            if (discountCouponCodes.Any())
            {
                model.DiscountBox.Messages.Add(await _localizationService.GetResourceAsync("ShoppingCart.DiscountCouponCode.OnlyOneCouponCodeCanApply"));
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(discountcouponcode))
                {
                    //we find even hidden records here. this way we can display a user-friendly message if it's expired
                    var discounts = (await _discountService.GetAllDiscountsAsync(couponCode: discountcouponcode, showHidden: true))
                        .Where(d => d.RequiresCouponCode)
                        .ToList();
                    if (discounts.Any())
                    {
                        var userErrors = new List<string>();
                        var anyValidDiscount = await discounts.AnyAwaitAsync(async discount =>
                        {
                            var validationResult = await _discountService.ValidateDiscountAsync(discount, customer, new[] { discountcouponcode });
                            userErrors.AddRange(validationResult.Errors);

                            return validationResult.IsValid;
                        });

                        if (anyValidDiscount)
                        {
                            //valid
                            await _customerService.ApplyDiscountCouponCodeAsync(customer, discountcouponcode);
                            model.DiscountBox.Messages.Add(await _localizationService.GetResourceAsync("ShoppingCart.DiscountCouponCode.Applied"));
                            model.DiscountBox.IsApplied = true;
                        }
                        else
                        {
                            if (userErrors.Any())
                                //some user errors
                                model.DiscountBox.Messages = userErrors;
                            else
                                //general error text
                                model.DiscountBox.Messages.Add(await _localizationService.GetResourceAsync("ShoppingCart.DiscountCouponCode.WrongDiscount"));
                        }
                    }
                    else
                        //discount cannot be found
                        model.DiscountBox.Messages.Add(await _localizationService.GetResourceAsync("ShoppingCart.DiscountCouponCode.CannotBeFound"));
                }
                else
                    //empty coupon code
                    model.DiscountBox.Messages.Add(await _localizationService.GetResourceAsync("ShoppingCart.DiscountCouponCode.Empty"));
            }
            model = await _shoppingCartModelFactory.PrepareShoppingCartModelAsync(model, cart);

            return View(model);
        }


        [HttpPost]
        public virtual async Task<IActionResult> AddProductToCart_Catalog(int productId, int shoppingCartTypeId, int quantity, bool forceredirection = false)
        {
            var cartType = (ShoppingCartType)shoppingCartTypeId;

            var product = await _productService.GetProductByIdAsync(productId);
            if (product == null)
                //no product found
                return Json(new
                {
                    success = false,
                    message = "No product found with the specified ID"
                });


            //allow a product to be added to the cart when all attributes are with "read-only checkboxes" type
            var productAttributes = await _productAttributeService.GetProductAttributeMappingsByProductIdAsync(product.Id);
            //creating XML for "read-only checkboxes" attributes
            var attXml = await productAttributes.AggregateAwaitAsync(string.Empty, async (attributesXml, attribute) =>
            {
                var attributeValues = await _productAttributeService.GetProductAttributeValuesAsync(attribute.Id);
                foreach (var selectedAttributeId in attributeValues
                    .Where(v => v.IsPreSelected)
                    .Select(v => v.Id)
                    .ToList())
                {
                    attributesXml = _productAttributeParser.AddProductAttribute(attributesXml,
                        attribute, selectedAttributeId.ToString());
                }

                return attributesXml;
            });

            //get standard warnings without attribute validations
            //first, try to find existing shopping cart item
            var customer = await _workContext.GetCurrentCustomerAsync();
            var store = await _storeContext.GetCurrentStoreAsync();
            var cart = await _shoppingCartService.GetShoppingCartAsync(customer, cartType, store.Id);
            var shoppingCartItem = await _shoppingCartService.FindShoppingCartItemInTheCartAsync(cart, cartType, product);
            //if we already have the same product in the cart, then use the total quantity to validate
            var quantityToValidate = shoppingCartItem != null ? shoppingCartItem.Quantity + quantity : quantity;
            var addToCartWarnings = await _shoppingCartService
                .GetShoppingCartItemWarningsAsync(customer, cartType,
                product, store.Id, string.Empty,
                decimal.Zero, null, null, quantityToValidate, false, shoppingCartItem?.Id ?? 0, true, false, false, false);
            if (addToCartWarnings.Any())
            {
                //cannot be added to the cart
                //let's display standard warnings
                return Json(new
                {
                    success = false,
                    message = addToCartWarnings.ToArray()
                });
            }

            //now let's try adding product to the cart (now including product attribute validation, etc)
            addToCartWarnings = await _shoppingCartService.AddToCartAsync(customer: customer,
                product: product,
                shoppingCartType: cartType,
                storeId: store.Id,
                attributesXml: attXml,
                quantity: quantity);
            if (addToCartWarnings.Any())
            {
                ////cannot be added to the cart
                ////but we do not display attribute and gift card warnings here. let's do it on the product details page
                //return Json(new
                //{
                //    redirect = Url.RouteUrl("Product", new { SeName = await _urlRecordService.GetSeNameAsync(product) })
                //});
            }

            //added to the cart/wishlist
            switch (cartType)
            {
                case ShoppingCartType.Wishlist:
                    {
                        //activity log
                        await _customerActivityService.InsertActivityAsync("PublicStore.AddToWishlist",
                            string.Format(await _localizationService.GetResourceAsync("ActivityLog.PublicStore.AddToWishlist"), product.Name), product);

                        if (_shoppingCartSettings.DisplayWishlistAfterAddingProduct || forceredirection)
                        {
                            //redirect to the wishlist page
                            return Json(new
                            {
                                redirect = Url.RouteUrl("CustomerFavorite")
                            });
                        }

                        //display notification message and update appropriate blocks
                        var shoppingCarts = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.Wishlist, store.Id);

                        var updatetopwishlistsectionhtml = string.Format(await _localizationService.GetResourceAsync("Wishlist.HeaderQuantity"),
                            shoppingCarts.Sum(item => item.Quantity));
                        return Json(new
                        {
                            success = true,
                            message = string.Format(await _localizationService.GetResourceAsync("Products.ProductHasBeenAddedToTheWishlist.Link"), Url.RouteUrl("CustomerFavorite")),
                            updatetopwishlistsectionhtml,
                            updateWishlistHeartProductId = product.Id
                        });
                    }

                case ShoppingCartType.ShoppingCart:
                default:
                    {
                        //activity log
                        await _customerActivityService.InsertActivityAsync("PublicStore.AddToShoppingCart",
                            string.Format(await _localizationService.GetResourceAsync("ActivityLog.PublicStore.AddToShoppingCart"), product.Name), product);

                        if (_shoppingCartSettings.DisplayCartAfterAddingProduct || forceredirection)
                        {
                            //redirect to the shopping cart page
                            return Json(new
                            {
                                redirect = Url.RouteUrl("ShoppingCart")
                            });
                        }

                        //display notification message and update appropriate blocks
                        var shoppingCarts = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, store.Id);

                        var updatetopcartsectionhtml = string.Format(await _localizationService.GetResourceAsync("ShoppingCart.HeaderQuantity"),
                            shoppingCarts.Sum(item => item.Quantity));

                        var updateflyoutcartsectionhtml = _shoppingCartSettings.MiniShoppingCartEnabled
                            ? await RenderViewComponentToStringAsync("FlyoutShoppingCart")
                            : string.Empty;

                        return Json(new
                        {
                            success = true,
                            message = string.Format(await _localizationService.GetResourceAsync("Products.ProductHasBeenAddedToTheCart.Link"), Url.RouteUrl("ShoppingCart")),
                            updatetopcartsectionhtml,
                            updateflyoutcartsectionhtml
                        });
                    }
            }
        }

        [HttpPost]
        public virtual async Task<IActionResult> DeleteProductFromCart_Details(int productId, int shoppingCartTypeId, IFormCollection form)
        {
            bool status = false;
            var store = await _storeContext.GetCurrentStoreAsync();
            var customer = await _workContext.GetCurrentCustomerAsync();
            var product = await _productService.GetProductByIdAsync(productId);
            if (product == null)
            {
                status = false;
                return Json(new
                {
                    status
                });
            }

            try
            {
                //allow a product to be added to the cart when all attributes are with "read-only checkboxes" type
                var productAttributes = await _productAttributeService.GetProductAttributeMappingsByProductIdAsync(product.Id);
                if (productAttributes.Any(pam => pam.AttributeControlType != AttributeControlType.ReadonlyCheckboxes))
                {
                    //product has some attributes. let a customer see them
                    return Json(new
                    {
                        redirect = Url.RouteUrl("Product", new { SeName = await _urlRecordService.GetSeNameAsync(product) })
                    });
                }
                var attXml = await productAttributes.AggregateAwaitAsync(string.Empty, async (attributesXml, attribute) =>
                {
                    var attributeValues = await _productAttributeService.GetProductAttributeValuesAsync(attribute.Id);
                    foreach (var selectedAttributeId in attributeValues
                        .Where(v => v.IsPreSelected)
                        .Select(v => v.Id)
                        .ToList())
                    {
                        attributesXml = _productAttributeParser.AddProductAttribute(attributesXml,
                            attribute, selectedAttributeId.ToString());
                    }

                    return attributesXml;
                });

                var shoppingCart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.Wishlist, store.Id);
                var cart = shoppingCart.Where(x => x.ShoppingCartTypeId == shoppingCartTypeId && x.ProductId == productId).ToList();

                var itemToDelete = await _shoppingCartService.FindShoppingCartItemInTheCartAsync(
                    cart, ShoppingCartType.Wishlist, product, attXml, decimal.Zero, null, null);

                if (itemToDelete != null)
                {
                    await _shoppingCartService.DeleteShoppingCartItemAsync(itemToDelete);
                }

                status = true;


            }
            catch (Exception ex)
            {
                status = false;
                return Json(new
                {
                    status,
                    message = ex.Message
                });
            }

            var shoppingCarts = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.Wishlist, store.Id);
            var updateTopWishlistSectionHtml = string.Format(
                          await _localizationService.GetResourceAsync("Wishlist.HeaderQuantity"),
                          shoppingCarts.Sum(item => item.Quantity));

            return Json(new
            {
                status,
                message = string.Format((await _localizationService.GetResourceAsync("Alchub.Product.Removed.From.Wishlist.Link")), (await _productService.GetProductByIdAsync(productId)).Name, Url.RouteUrl("CustomerFavorite")),
                updateWishlistHeartProductId = product.Id,
                updatetopwishlistsectionhtml = updateTopWishlistSectionHtml,
            });
        }

        public virtual async Task<IActionResult> RemoveItemFromFlyoutCart(int itemId)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.EnableShoppingCart))
                return RedirectToRoute("Homepage");

            var store = await _storeContext.GetCurrentStoreAsync();
            var customer = await _workContext.GetCurrentCustomerAsync();
            var cartItem = (await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart,
                store.Id))?.Where(x => x.Id == itemId)?.FirstOrDefault();

            if (cartItem != null)
                await _shoppingCartService.UpdateShoppingCartItemAsync(customer,
                    cartItem.Id, cartItem.AttributesXml, cartItem.CustomerEnteredPrice,
                    cartItem.RentalStartDateUtc, cartItem.RentalEndDateUtc, 0, true);

            var updateFlyoutCartSectionHtml = _shoppingCartSettings.MiniShoppingCartEnabled
                            ? await RenderViewComponentToStringAsync("FlyoutShoppingCart")
                            : string.Empty;

            var itemsQuantity = (await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, store.Id))
                    .Sum(item => item.Quantity);

            return Json(new
            {
                success = true,
                itemsQuantity = itemsQuantity,
                updateflyoutcartsectionhtml = updateFlyoutCartSectionHtml
            });
        }

        [HttpPost]
        public virtual async Task<IActionResult> UpdateItemQuantity(int productId, int shoppingCartTypeId, int quantity, bool manuallyChange = false)
        {
            var cartType = (ShoppingCartType)shoppingCartTypeId;
            var product = await _productService.GetProductByIdAsync(productId);
            if (product == null)
                //no product found
                return Json(new
                {
                    success = false,
                    message = "No product found with the specified ID"
                });

            //get standard warnings without attribute validations
            //first, try to find existing shopping cart item
            var customer = await _workContext.GetCurrentCustomerAsync();
            var store = await _storeContext.GetCurrentStoreAsync();
            var cart = await _shoppingCartService.GetShoppingCartAsync(customer, cartType, store.Id);
            var shoppingCartItem = await _shoppingCartService.FindShoppingCartItemInTheCartAsync(cart, cartType, product);

            if (shoppingCartItem == null)
                return Json(new
                {
                    success = false,
                    message = "No cart item found"
                });

            try
            {
                //update item quantity(+/-)
                var newQuantity = 0;
                if (manuallyChange)
                    newQuantity = quantity;
                else
                    newQuantity = shoppingCartItem.Quantity + quantity;

                shoppingCartItem.Quantity = newQuantity;
                shoppingCartItem.UpdatedOnUtc = DateTime.UtcNow;

                //update quantity if greater than zero, otherwise delete item from cart
                if (shoppingCartItem.Quantity > 0)
                    await _sciRepository.UpdateAsync(shoppingCartItem);
                else
                    await _shoppingCartService.DeleteShoppingCartItemAsync(shoppingCartItem);
            }
            catch (Exception e)
            {
                return Json(new
                {
                    success = false,
                    message = e.Message
                });
            }

            var shoppingCarts = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, store.Id);
            var updatetopcartsectionhtml = string.Format(await _localizationService.GetResourceAsync("ShoppingCart.HeaderQuantity"),
                shoppingCarts.Sum(item => item.Quantity));
            var updateflyoutcartsectionhtml = _shoppingCartSettings.MiniShoppingCartEnabled
                ? await RenderViewComponentToStringAsync("FlyoutShoppingCart")
                : string.Empty;

            var updateOrderSummaryHtml = await RenderViewComponentToStringAsync("OrderSummary",
                new { isEditable = true });
            var itemSubtotal = (await _shoppingCartService.GetSubTotalAsync(shoppingCartItem, true)).subTotal;

            return Json(new
            {
                success = true,
                updateOrderSummaryHtml,
                updatetopcartsectionhtml,
                updateflyoutcartsectionhtml
            });
        }


        [HttpPost, ActionName("Cart")]
        [FormValueRequired("updatecart")]
        public virtual async Task<IActionResult> UpdateCart(IFormCollection form)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.EnableShoppingCart))
                return RedirectToRoute("Homepage");

            var customer = await _workContext.GetCurrentCustomerAsync();
            var store = await _storeContext.GetCurrentStoreAsync();
            var cart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, store.Id);

            foreach (var item in cart)
            {
                await _shoppingCartService.DeleteShoppingCartItemAsync(item);
            }
            await _localizationService.GetResourceAsync("Alchub.Removed.All.Product.ShoppingCart");
            return RedirectToRoute("ShoppingCart");
        }

        #region Wishlist

        [HttpPost, ActionName("Wishlist")]
        [FormValueRequired("updatecart")]
        public virtual async Task<IActionResult> UpdateWishlist(IFormCollection form)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.EnableWishlist))
                return RedirectToRoute("Homepage");

            var customer = await _workContext.GetCurrentCustomerAsync();
            var store = await _storeContext.GetCurrentStoreAsync();
            var cart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.Wishlist, store.Id);

            var allIdsToRemove = form.ContainsKey("removefromcart")
                ? form["removefromcart"].ToString().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(int.Parse)
                .ToList()
                : new List<int>();

            //current warnings <cart item identifier, warnings>
            var innerWarnings = new Dictionary<int, IList<string>>();
            foreach (var sci in cart)
            {
                var remove = allIdsToRemove.Contains(sci.Id);
                if (remove)
                    await _shoppingCartService.DeleteShoppingCartItemAsync(sci);
                else
                {
                    foreach (var formKey in form.Keys)
                        if (formKey.Equals($"itemquantity{sci.Id}", StringComparison.InvariantCultureIgnoreCase))
                        {
                            if (int.TryParse(form[formKey], out var newQuantity))
                            {
                                var currSciWarnings = await _shoppingCartService.UpdateShoppingCartItemAsync(customer,
                                    sci.Id, sci.AttributesXml, sci.CustomerEnteredPrice,
                                    sci.RentalStartDateUtc, sci.RentalEndDateUtc,
                                    newQuantity, true);
                                innerWarnings.Add(sci.Id, currSciWarnings);
                            }

                            break;
                        }
                }
            }

            //updated wishlist
            cart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.Wishlist, store.Id);
            var model = new WishlistModel();
            model = await _shoppingCartModelFactory.PrepareWishlistModelAsync(model, cart);
            //update current warnings
            foreach (var kvp in innerWarnings)
            {
                //kvp = <cart item identifier, warnings>
                var sciId = kvp.Key;
                var warnings = kvp.Value;
                //find model
                var sciModel = model.Items.FirstOrDefault(x => x.Id == sciId);
                if (sciModel != null)
                    foreach (var w in warnings)
                        if (!sciModel.Warnings.Contains(w))
                            sciModel.Warnings.Add(w);
            }

            //++Alchub

            return RedirectToRoute("CustomerFavorite", new { customerGuid = customer.CustomerGuid });

            //--Alchub
            //return View(model);
        }

        #endregion

        #endregion
    }
}