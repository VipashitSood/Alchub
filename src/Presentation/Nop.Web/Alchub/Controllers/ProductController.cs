using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Security;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Slots;
using Nop.Core.Domain.Vendors;
using Nop.Core.Events;
using Nop.Core.Rss;
using Nop.Services.Alchub.General;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Html;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Slots;
using Nop.Services.Stores;
using Nop.Services.Vendors;
using Nop.Web.Factories;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Mvc.Filters;
using Nop.Web.Models.Catalog;
using Nop.Web.Models.Checkout;
using Nop.Web.Models.Slots;
using Nop.Services.Alchub.Google;
using Nop.Services.DeliveryFees;
using Nop.Services.Helpers;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core.Alchub.Domain;

namespace Nop.Web.Controllers
{
    public partial class ProductController
    {
        #region Fields

        private readonly CaptchaSettings _captchaSettings;
        private readonly CatalogSettings _catalogSettings;
        private readonly IAclService _aclService;
        private readonly ICompareProductsService _compareProductsService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly ICustomerService _customerService;
        private readonly IEventPublisher _eventPublisher;
        private readonly IHtmlFormatter _htmlFormatter;
        private readonly ILocalizationService _localizationService;
        private readonly IOrderService _orderService;
        private readonly IPermissionService _permissionService;
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly IProductModelFactory _productModelFactory;
        private readonly IProductService _productService;
        private readonly IRecentlyViewedProductsService _recentlyViewedProductsService;
        private readonly IReviewTypeService _reviewTypeService;
        private readonly IShoppingCartModelFactory _shoppingCartModelFactory;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IStoreContext _storeContext;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IWebHelper _webHelper;
        private readonly IWorkContext _workContext;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly LocalizationSettings _localizationSettings;
        private readonly ShoppingCartSettings _shoppingCartSettings;
        private readonly ShippingSettings _shippingSettings;
        private readonly IAlchubGeneralService _alchubGeneralService;
        private readonly ISlotService _slotService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ISettingService _settingService;
        private readonly IVendorService _vendorService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly AlchubSettings _alchubSettings;
        #endregion

        #region Ctor

        public ProductController(CaptchaSettings captchaSettings,
            CatalogSettings catalogSettings,
            IAclService aclService,
            ICompareProductsService compareProductsService,
            ICustomerActivityService customerActivityService,
            ICustomerService customerService,
            IEventPublisher eventPublisher,
            IHtmlFormatter htmlFormatter,
            ILocalizationService localizationService,
            IOrderService orderService,
            IPermissionService permissionService,
            IProductAttributeParser productAttributeParser,
            IProductModelFactory productModelFactory,
            IProductService productService,
            IRecentlyViewedProductsService recentlyViewedProductsService,
            IReviewTypeService reviewTypeService,
            IShoppingCartModelFactory shoppingCartModelFactory,
            IShoppingCartService shoppingCartService,
            IStoreContext storeContext,
            IStoreMappingService storeMappingService,
            IUrlRecordService urlRecordService,
            IWebHelper webHelper,
            IWorkContext workContext,
            IWorkflowMessageService workflowMessageService,
            LocalizationSettings localizationSettings,
            ShoppingCartSettings shoppingCartSettings,
            ShippingSettings shippingSettings,
            IAlchubGeneralService alchubGeneralService,
            ISlotService slotService,
            IPriceFormatter priceFormatter,
            IGenericAttributeService genericAttributeService,
            ISettingService settingService,
            IVendorService vendorService,
            IDateTimeHelper dateTimeHelper,
            AlchubSettings alchubSettings)
        {
            _captchaSettings = captchaSettings;
            _catalogSettings = catalogSettings;
            _aclService = aclService;
            _compareProductsService = compareProductsService;
            _customerActivityService = customerActivityService;
            _customerService = customerService;
            _eventPublisher = eventPublisher;
            _htmlFormatter = htmlFormatter;
            _localizationService = localizationService;
            _orderService = orderService;
            _permissionService = permissionService;
            _productAttributeParser = productAttributeParser;
            _productModelFactory = productModelFactory;
            _productService = productService;
            _reviewTypeService = reviewTypeService;
            _recentlyViewedProductsService = recentlyViewedProductsService;
            _shoppingCartModelFactory = shoppingCartModelFactory;
            _shoppingCartService = shoppingCartService;
            _storeContext = storeContext;
            _storeMappingService = storeMappingService;
            _urlRecordService = urlRecordService;
            _webHelper = webHelper;
            _workContext = workContext;
            _workflowMessageService = workflowMessageService;
            _localizationSettings = localizationSettings;
            _shoppingCartSettings = shoppingCartSettings;
            _shippingSettings = shippingSettings;
            _alchubGeneralService = alchubGeneralService;
            _slotService = slotService;
            _priceFormatter = priceFormatter;
            _genericAttributeService = genericAttributeService;
            _settingService = settingService;
            _vendorService = vendorService;
            _dateTimeHelper = dateTimeHelper;
            _alchubSettings = alchubSettings;
        }

        #endregion

        #region Vendor sorting

        public virtual async Task<IActionResult> VendorSorting(int vendorSortBy, int productId)
        {
            var product = await _productService.GetProductByIdAsync(productId);
            if (product == null)
                return Json(new
                {
                    success = false,
                    message = "No product found with specific id"
                });

            var store = await _storeContext.GetCurrentStoreAsync();
            var currentCustomer = await _workContext.GetCurrentCustomerAsync();
            //get available vendors
            var availableVendors = (await _vendorService.GetAvailableGeoFenceVendorsAsync(currentCustomer, true, product))?.ToList();

            //get product vendors
            var productVendors = (await _productModelFactory.PrepareProductVendorsAsync(product, availableVendors))?.OrderBy(v => v.StartTime)
                                                                                                                   ?.ThenBy(v => v.DistanceValue)
                                                                                                                   ?.ToList();

            var productDetailsModel = new ProductDetailsModel();
            if (productVendors.Any())
            {
                if (vendorSortBy == (int)VendorSort.Cheapest)
                    productDetailsModel.ProductVendors = productVendors.OrderBy(x => x.VendorProductPrice.PriceWithoutDiscount).ToList();
                else if (vendorSortBy == (int)VendorSort.Proximity)
                    productDetailsModel.ProductVendors = productVendors.OrderBy(x => x.DistanceValue).ToList();
                else if (vendorSortBy == (int)VendorSort.Fastest)
                    productDetailsModel.ProductVendors = productVendors.OrderBy(x => x.StartTime).ThenBy(x => x.DistanceValue).ToList();
                else if (vendorSortBy == (int)VendorSort.Recommended)
                {
                    var cart = await _shoppingCartService.GetShoppingCartAsync(currentCustomer, ShoppingCartType.ShoppingCart, store.Id);
                    if (cart.Any())
                    {
                        //cart product vendor ids
                        var cartProductVendorIds = (await _productService.GetProductsByIdsAsync(cart.OrderBy(c => c.CreatedOnUtc)?.
                            Select(x => x.ProductId).ToArray())).Select(x => x.VendorId).ToList();

                        //vendor sorting according to cart product vendor(s).
                        if (cartProductVendorIds.Any())
                        {
                            cartProductVendorIds.AddRange(productVendors.Select(x => x.VendorId));
                            var avIds = cartProductVendorIds.Intersect(availableVendors.Select(av => av.Id).ToList()).ToArray();
                            var sortedVendors = await _vendorService.GetVendorsByIdsAsync(avIds.Distinct().ToArray());
                            productDetailsModel.ProductVendors = await _productModelFactory.PrepareProductVendorsAsync(product, sortedVendors);
                        }
                    }
                    else
                        productDetailsModel.ProductVendors = productVendors;
                }
                else
                    productDetailsModel.ProductVendors = productVendors;
            }
            else
                productDetailsModel.ProductVendors = productVendors;

            //Check with alchub details page setting true only vendor show
            if (productVendors != null && productVendors.Any() && _alchubSettings.AlchubProductDetailPageVendorTakeOne)
            {
                productDetailsModel.ProductVendors = productVendors.Take(1).ToList();
            }
            //prepare vendor sorting options
            productDetailsModel.VendorSortingOptions = await _productModelFactory.PrepareProductDetailsVendorSortingOptions();
            productDetailsModel.VendorSort = vendorSortBy;

            return PartialView("_ProductVendors", productDetailsModel);
        }
        #endregion

        #region Vendor pricing

        //available even when navigation is not allowed
        [CheckAccessPublicStore(true)]
        //ignore SEO friendly URLs checks
        [CheckLanguageSeoCode(true)]
        public virtual async Task<IActionResult> GetProductPriceByVendorId(string vendorId, string productId)
        {
            //validate
            int.TryParse(vendorId, out var vendorIdInt);
            if (vendorIdInt == 0)
            {
                return Json(new
                {
                    success = false,
                    message = "Invalid vendor id"
                });
            }

            //validate
            int.TryParse(productId, out var productIdInt);
            if (vendorIdInt == 0)
            {
                return Json(new
                {
                    success = false,
                    message = "Invalid product id"
                });
            }

            //verify product
            var product = await _productService.GetProductByIdAsync(productIdInt);
            if (product == null)
            {
                return Json(new
                {
                    success = false,
                    message = "Product not found with specific id"
                });
            }

            //get selected vendor product
            var upcProducts = await _alchubGeneralService.GetProductsByUpcCodeAsync(product.UPCCode, true);
            var vendorProduct = upcProducts?.FirstOrDefault(p => p.VendorId == vendorIdInt);
            if (vendorProduct == null)
            {
                return Json(new
                {
                    success = false,
                    message = "No sub Product aligned along with selected vendor"
                });
            }

            var priceModel = await _productModelFactory.PrepareVendorProductPriceModelAsync(vendorProduct);

            //success
            return Json(new
            {
                success = true,
                message = "haha",
                priceModel = priceModel
            });

            //return Json(priceModel);
        }

        #endregion

        #region Slot Management

        //available even when navigation is not allowed
        [CheckAccessPublicStore(true)]
        //ignore SEO friendly URLs checks
        [CheckLanguageSeoCode(true)]
        public async Task<IActionResult> Slots(string startDate, string endDate, int productId, int vendorId, bool manageDelivery, bool pickAvailable, bool deliveryAvailable)
        {
            if (productId > 0 && vendorId > 0)
            {
                var addDay = await _settingService.GetSettingByKeyAsync<int>("Alchub.Slot.Visible.After.Day", defaultValue: 1);
                var addHour = await _settingService.GetSettingByKeyAsync<int>("Alchub.Slot.Add.Hour", defaultValue: 0);
                var addMintues = await _settingService.GetSettingByKeyAsync<int>("Alchub.Slot.Add.Mintues", defaultValue: 0);
                var dateTime = await _dateTimeHelper.ConvertToUserTimeAsync(DateTime.Now);
                dateTime = dateTime.AddDays(addDay).AddHours(addHour).AddMinutes(addMintues);
                var vendor = await _vendorService.GetVendorByIdAsync(vendorId);
                if (deliveryAvailable)
                {
                    //define manage delivery 
                    if (vendor.ManageDelivery)
                    {
                        var model = await _productModelFactory.PreparepareSlotListModel(0, dateTime.ToString(), dateTime.AddDays(7).ToString(), productId, vendorId);
                        if (model != null)
                        {
                            return PartialView("_BookSlots", model);
                        }
                    }
                    else
                    {
                        var model = await _productModelFactory.PreparepareAdminSlotListModel(0, dateTime.ToString(), dateTime.AddDays(7).ToString(), productId, vendorId, false);
                        if (model != null)
                        {
                            return PartialView("_BookSlots", model);
                        }
                    }
                }
                if (pickAvailable)
                {
                    var model = await _productModelFactory.PrepareparePickupSlotListModel(0, dateTime.ToString(), dateTime.AddDays(7).ToString(), productId, vendorId, true);
                    if (model != null)
                    {
                        return PartialView("_BookSlots", model);
                    }
                }

            }

            return PartialView("_BookSlots", null);
        }

        public async Task<IActionResult> BookSlot(int slotId, bool isBook, int blockId, string timeSlot, int productId, string startDate, string endDate, int vendorId, bool isPickup = false)
        {
            //Selected insert into table
            if (slotId < 0)
                throw new Exception("Invalid slot id");
            var customer = await _workContext.GetCurrentCustomerAsync();
            var store = _storeContext.GetCurrentStore();
            var addDay = await _settingService.GetSettingByKeyAsync<int>("Alchub.Slot.Visible.After.Day", defaultValue: 1);
            var addHour = await _settingService.GetSettingByKeyAsync<int>("Alchub.Slot.Add.Hour", defaultValue: 0);
            var addMintues = await _settingService.GetSettingByKeyAsync<int>("Alchub.Slot.Add.Mintues", defaultValue: 0);
            var dateTime = await _dateTimeHelper.ConvertToUserTimeAsync(DateTime.Now);
            dateTime = dateTime.AddDays(addDay).AddHours(addHour).AddMinutes(addMintues);
            decimal price = 0;
            var vendor = await _vendorService.GetVendorByIdAsync(vendorId);
            var cutomerProductSlotList = (await _slotService.GetCustomerProductSlot(productId, customer.Id, dateTime, isPickup)).Where(x => x.IsSelected).ToList();
            if (cutomerProductSlotList.Count == 0)
            {
                await _slotService.DeleteCustomerProductSlot(productId, customer.Id);
                CustomerProductSlot customerProductSlot = new CustomerProductSlot();
                customerProductSlot.SlotId = slotId;
                customerProductSlot.BlockId = blockId;
                customerProductSlot.StartTime = Convert.ToDateTime(startDate);
                customerProductSlot.EndDateTime = Convert.ToDateTime(endDate);
                customerProductSlot.EndTime = timeSlot;
                customerProductSlot.Price = price;
                customerProductSlot.CustomerId = customer.Id;
                customerProductSlot.ProductId = productId;
                customerProductSlot.IsPickup = isPickup;
                customerProductSlot.IsSelected = true;
                customerProductSlot.LastUpdated = DateTime.UtcNow;
                await _slotService.InsertCustomerProductSlot(customerProductSlot);
            }
            else
            {
                foreach (var cutomerProductSlot in cutomerProductSlotList)
                {
                    if (cutomerProductSlot.IsSelected == true)
                    {
                        cutomerProductSlot.Id = cutomerProductSlot.Id;
                        cutomerProductSlot.StartTime = Convert.ToDateTime(startDate);
                        cutomerProductSlot.EndDateTime = Convert.ToDateTime(endDate);
                        cutomerProductSlot.EndTime = timeSlot;
                        cutomerProductSlot.SlotId = slotId;
                        cutomerProductSlot.BlockId = blockId;
                        cutomerProductSlot.Price = price;
                        cutomerProductSlot.IsPickup = isPickup;
                        cutomerProductSlot.IsSelected = true;
                        cutomerProductSlot.LastUpdated = DateTime.UtcNow;
                        await _slotService.UpdateCustomerProductSlot(cutomerProductSlot);
                    }
                }
            }
            if (isPickup)
            {
                var pickup = await _slotService.GetPickupSlotById(slotId);
                price = pickup != null ? pickup.Price : 0;
            }
            else
            {
                var slot = await _slotService.GetSlotById(slotId);
                price = slot != null ? slot.Price : 0;
            }

            if (!isPickup)
            {
                if (vendor.ManageDelivery)
                {
                    var model = await _productModelFactory.PreparepareSlotListModel(slotId, dateTime.ToString(), dateTime.AddDays(7).ToString(), productId, vendorId, isPickup);
                    return PartialView("_BookSlots", model);
                }
                else
                {
                    if (vendor.DeliveryAvailable)
                    {
                        var model = await _productModelFactory.PreparepareAdminSlotListModel(0, dateTime.ToString(), dateTime.AddDays(7).ToString(), productId, vendorId, false);
                        return PartialView("_BookSlots", model);
                    }
                }
            }
            if (isPickup)
            {
                var model = await _productModelFactory.PrepareparePickupSlotListModel(slotId, dateTime.ToString(), dateTime.AddDays(7).ToString(), productId, vendorId, isPickup);
                return PartialView("_BookSlots", model);

            }
            return PartialView("_BookSlots", null);
        }

        //available even when navigation is not allowed
        [CheckAccessPublicStore(true)]
        //ignore SEO friendly URLs checks
        [CheckLanguageSeoCode(true)]
        public async Task<IActionResult> PickupSlots(string startDate, string endDate, int productId, int vendorId)
        {
            var addDay = await _settingService.GetSettingByKeyAsync<int>("Alchub.Slot.Visible.After.Day", defaultValue: 1);
            var addHour = await _settingService.GetSettingByKeyAsync<int>("Alchub.Slot.Add.Hour", defaultValue: 0);
            var addMintues = await _settingService.GetSettingByKeyAsync<int>("Alchub.Slot.Add.Mintues", defaultValue: 0);
            var dateTime = await _dateTimeHelper.ConvertToUserTimeAsync(DateTime.Now);
            dateTime = dateTime.AddDays(addDay).AddHours(addHour).AddMinutes(addMintues);
            var model = await _productModelFactory.PrepareparePickupSlotListModel(0, dateTime.ToString(), dateTime.AddDays(7).ToString(), productId, vendorId, true);
            if (model != null)
            {
                return PartialView("_BookSlots", model);
            }
            return PartialView("_BookSlots", null);
        }

        /// <summary>
        /// Product fastest slot (AJAX)
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        public virtual async Task<IActionResult> GetProductFastestSlot(int productId)
        {
            try
            {
                //validation
                if (productId == 0)
                    throw new Exception("Invalid productId");

                //get product
                var product = await _productService.GetProductByIdAsync(productId) ??
                    throw new Exception("Product not found with specific productId");

                //prepare fastest slot string
                var fastestSlot = await _productModelFactory.PrepareProductFastestSlotAsync(product, await _workContext.GetCurrentCustomerAsync());

                return Json(new
                {
                    success = true,
                    update_fastest_slot_section = new
                    {
                        name = $"fastest_slot_section_{productId}",
                        html = await RenderPartialViewToStringAsync("_ProductFastestSlot", fastestSlot)
                    }
                });
            }
            catch (Exception exc)
            {
                return Json(new { error = 1, success = false, message = exc.Message });
            }
        }

        #endregion

        #region Grouped Product

        [HttpPost]
        public virtual async Task<IActionResult> GroupedProductSelectVariant(int groupedProductId, int variantId)
        {
            try
            {
                //validation
                if (groupedProductId == 0)
                    throw new Exception("Invalid groupedProductId");

                if (variantId == 0)
                    throw new Exception("Invalid variantId");

                //get product
                var product = await _productService.GetProductByIdAsync(variantId) ??
                    throw new Exception("Product not found with specific variantId");

                //prepare model
                var model = await _productModelFactory.PrepareProductDetailsModelAsync(product, null, false);

                return Json(new
                {
                    update_picture_section = new
                    {
                        name = "product_details_picture_section",
                        html = await RenderPartialViewToStringAsync("_ProductDetailsPictures", model)
                    },
                    update_vendors_section = new
                    {
                        name = "product_detail_vendors",
                        html = await RenderPartialViewToStringAsync("_ProductVendors", model)
                    },
                    update_specification_section = new
                    {
                        name = "product_detail_specification_att",
                        html = await RenderPartialViewToStringAsync("_ProductSpecifications", model.ProductSpecificationModel)
                    },
                    update_descriptions_section = new
                    {
                        name = "product_detail_descriptions",
                        html = await RenderPartialViewToStringAsync("_ProductDescriptions", model)
                    }
                });
            }
            catch (Exception exc)
            {
                return Json(new { error = 1, message = exc.Message });
            }
        }

        #endregion

        #region Product reviews

        [HttpPost]
        [ValidateCaptcha]
        public virtual async Task<IActionResult> AddProductReviews(int productId, string title, string reviewText, int reviewRating, bool captchaValid)
        {
            var product = await _productService.GetProductByIdAsync(productId);
            var currentStore = await _storeContext.GetCurrentStoreAsync();

            if (product == null || product.Deleted || !product.Published || !product.AllowCustomerReviews ||
                !await _productService.CanAddReviewAsync(product.Id, _catalogSettings.ShowProductReviewsPerStore ? currentStore.Id : 0))
                return PartialView("_ProductReviews", null);

            //model 
            var model = new ProductReviewsModel();
            model.AddProductReview.Title = title;
            model.AddProductReview.Rating = reviewRating;
            model.AddProductReview.ReviewText = reviewText;
            model.ProductId = productId;
            model.ProductName = product.Name;

            //validate CAPTCHA
            if (_captchaSettings.Enabled && _captchaSettings.ShowOnProductReviewPage && !captchaValid)
            {
                ModelState.AddModelError("", await _localizationService.GetResourceAsync("Common.WrongCaptchaMessage"));
            }

            await ValidateProductReviewAvailabilityAsync(product);

            if (!ModelState.IsValid)
                return PartialView("_ProductReviews", null);

            //save review
            var rating = model.AddProductReview.Rating;
            if (rating < 1 || rating > 5)
                rating = _catalogSettings.DefaultProductRatingValue;
            var isApproved = !_catalogSettings.ProductReviewsMustBeApproved;
            var customer = await _workContext.GetCurrentCustomerAsync();

            var productReview = new ProductReview
            {
                ProductId = product.Id,
                CustomerId = customer.Id,
                Title = model.AddProductReview.Title,
                ReviewText = model.AddProductReview.ReviewText,
                Rating = rating,
                HelpfulYesTotal = 0,
                HelpfulNoTotal = 0,
                IsApproved = isApproved,
                CreatedOnUtc = DateTime.UtcNow,
                StoreId = currentStore.Id,
            };

            await _productService.InsertProductReviewAsync(productReview);

            //add product review and review type mapping                
            foreach (var additionalReview in model.AddAdditionalProductReviewList)
            {
                var additionalProductReview = new ProductReviewReviewTypeMapping
                {
                    ProductReviewId = productReview.Id,
                    ReviewTypeId = additionalReview.ReviewTypeId,
                    Rating = additionalReview.Rating
                };

                await _reviewTypeService.InsertProductReviewReviewTypeMappingsAsync(additionalProductReview);
            }

            //update product totals
            await _productService.UpdateProductReviewTotalsAsync(product);

            //notify store owner
            if (_catalogSettings.NotifyStoreOwnerAboutNewProductReviews)
                await _workflowMessageService.SendProductReviewNotificationMessageAsync(productReview, _localizationSettings.DefaultAdminLanguageId);

            //activity log
            await _customerActivityService.InsertActivityAsync("PublicStore.AddProductReview",
                string.Format(await _localizationService.GetResourceAsync("ActivityLog.PublicStore.AddProductReview"), product.Name), product);

            //raise event
            if (productReview.IsApproved)
                await _eventPublisher.PublishAsync(new ProductReviewApprovedEvent(productReview));

            model = await _productModelFactory.PrepareProductReviewsModelAsync(model, product);
            model.AddProductReview.Title = null;
            model.AddProductReview.ReviewText = null;

            model.AddProductReview.SuccessfullyAdded = true;
            if (!isApproved)
                model.AddProductReview.Result = await _localizationService.GetResourceAsync("Reviews.SeeAfterApproving");
            else
                model.AddProductReview.Result = await _localizationService.GetResourceAsync("Reviews.SuccessfullyAdded");

            if (model != null)
            {
                return PartialView("_ProductReviews", model);
            }
            return PartialView("_ProductReviews", null);
        }

        #endregion

        #region Product details page

        public virtual async Task<IActionResult> ProductDetails(int productId, int updatecartitemid = 0)
        {
            var product = await _productService.GetProductByIdAsync(productId);
            if (product == null || product.Deleted)
                return InvokeHttp404();

            //++Alchub
            //get master product & redirect to it.
            if (!product.IsMaster)
            {
                var masterProduct = await _alchubGeneralService.GetMasterProductByUpcCodeAsync(product.UPCCode);
                if (masterProduct != null && !masterProduct.Deleted)
                    product = masterProduct;
            }

            //--Alchub

            var notAvailable =
                //published?
                (!product.Published && !_catalogSettings.AllowViewUnpublishedProductPage) ||
                //ACL (access control list) 
                !await _aclService.AuthorizeAsync(product) ||
                //Store mapping
                !await _storeMappingService.AuthorizeAsync(product) ||
                //availability dates
                !_productService.ProductIsAvailable(product);
            //Check whether the current user has a "Manage products" permission (usually a store owner)
            //We should allows him (her) to use "Preview" functionality
            var hasAdminAccess = await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessAdminPanel) && await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts);
            if (notAvailable && !hasAdminAccess)
                return InvokeHttp404();

            //visible individually?
            if (!product.VisibleIndividually)
            {
                //is this one an associated products?
                var parentGroupedProduct = await _productService.GetProductByIdAsync(product.ParentGroupedProductId);
                if (parentGroupedProduct == null)
                    return RedirectToRoute("Homepage");

                return RedirectToRoutePermanent("Product", new { SeName = await _urlRecordService.GetSeNameAsync(parentGroupedProduct) });
            }

            //update existing shopping cart or wishlist  item?
            ShoppingCartItem updatecartitem = null;
            if (_shoppingCartSettings.AllowCartItemEditing && updatecartitemid > 0)
            {
                var store = await _storeContext.GetCurrentStoreAsync();
                var cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), storeId: store.Id);
                updatecartitem = cart.FirstOrDefault(x => x.Id == updatecartitemid);
                //not found?
                if (updatecartitem == null)
                {
                    return RedirectToRoute("Product", new { SeName = await _urlRecordService.GetSeNameAsync(product) });
                }
                //is it this product?
                if (product.Id != updatecartitem.ProductId)
                {
                    return RedirectToRoute("Product", new { SeName = await _urlRecordService.GetSeNameAsync(product) });
                }
            }

            //save as recently viewed
            await _recentlyViewedProductsService.AddProductToRecentlyViewedListAsync(product.Id);

            //display "edit" (manage) link
            if (await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessAdminPanel) &&
                await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
            {
                //a vendor should have access only to his products
                var currentVendor = await _workContext.GetCurrentVendorAsync();
                if (currentVendor == null || currentVendor.Id == product.VendorId)
                {
                    //++Alchub: add MasterProduct controler name instead of Product
                    DisplayEditLink(Url.Action("Edit", "MasterProduct", new { id = product.Id, area = AreaNames.Admin }));
                }
            }

            //activity log
            await _customerActivityService.InsertActivityAsync("PublicStore.ViewProduct",
                string.Format(await _localizationService.GetResourceAsync("ActivityLog.PublicStore.ViewProduct"), product.Name), product);

            //model
            var model = await _productModelFactory.PrepareProductDetailsModelAsync(product, updatecartitem, false);

            //template
            var productTemplateViewPath = await _productModelFactory.PrepareProductTemplateViewPathAsync(product);

            return View(productTemplateViewPath, model);
        }

        #endregion
    }
}