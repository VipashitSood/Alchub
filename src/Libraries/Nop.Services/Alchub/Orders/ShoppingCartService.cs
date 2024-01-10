using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Vendors;
using Nop.Data;
using Nop.Services.Alchub.General;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Shipping;
using Nop.Services.Shipping.Date;
using Nop.Services.Slots;
using Nop.Services.Stores;
using Nop.Services.Vendors;

namespace Nop.Services.Orders
{
    /// <summary>
    /// Shopping cart service
    /// </summary>
    public partial class ShoppingCartService : IShoppingCartService
    {
        #region Fields

        private readonly CatalogSettings _catalogSettings;
        private readonly IAclService _aclService;
        private readonly IActionContextAccessor _actionContextAccessor;
        private readonly ICheckoutAttributeParser _checkoutAttributeParser;
        private readonly ICheckoutAttributeService _checkoutAttributeService;
        private readonly ICurrencyService _currencyService;
        private readonly ICustomerService _customerService;
        private readonly IDateRangeService _dateRangeService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly IProductAttributeService _productAttributeService;
        private readonly IProductService _productService;
        private readonly IRepository<ShoppingCartItem> _sciRepository;
        private readonly IShippingService _shippingService;
        private readonly IStaticCacheManager _staticCacheManager;
        private readonly IStoreContext _storeContext;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IUrlHelperFactory _urlHelperFactory;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IWorkContext _workContext;
        private readonly OrderSettings _orderSettings;
        private readonly ShoppingCartSettings _shoppingCartSettings;
        private readonly IVendorService _vendorService;
        private readonly ISlotService _slotService;
        private readonly ISettingService _settingService;
        private readonly ICategoryService _categoryService;
        private readonly IAlchubGeneralService _alchubGeneralService;
        #endregion Fields

        #region Ctor

        public ShoppingCartService(CatalogSettings catalogSettings,
            IAclService aclService,
            IActionContextAccessor actionContextAccessor,
            ICheckoutAttributeParser checkoutAttributeParser,
            ICheckoutAttributeService checkoutAttributeService,
            ICurrencyService currencyService,
            ICustomerService customerService,
            IDateRangeService dateRangeService,
            IDateTimeHelper dateTimeHelper,
            IGenericAttributeService genericAttributeService,
            ILocalizationService localizationService,
            IPermissionService permissionService,
            IPriceCalculationService priceCalculationService,
            IPriceFormatter priceFormatter,
            IProductAttributeParser productAttributeParser,
            IProductAttributeService productAttributeService,
            IProductService productService,
            IRepository<ShoppingCartItem> sciRepository,
            IShippingService shippingService,
            IStaticCacheManager staticCacheManager,
            IStoreContext storeContext,
            IStoreMappingService storeMappingService,
            IUrlHelperFactory urlHelperFactory,
            IUrlRecordService urlRecordService,
            IWorkContext workContext,
            OrderSettings orderSettings,
            ShoppingCartSettings shoppingCartSettings,
            IVendorService vendorService,
            ISlotService slotService,
            ISettingService settingService,
            ICategoryService categoryService,
            IAlchubGeneralService alchubGeneralService)
        {
            _catalogSettings = catalogSettings;
            _aclService = aclService;
            _actionContextAccessor = actionContextAccessor;
            _checkoutAttributeParser = checkoutAttributeParser;
            _checkoutAttributeService = checkoutAttributeService;
            _currencyService = currencyService;
            _customerService = customerService;
            _dateRangeService = dateRangeService;
            _dateTimeHelper = dateTimeHelper;
            _genericAttributeService = genericAttributeService;
            _localizationService = localizationService;
            _permissionService = permissionService;
            _priceCalculationService = priceCalculationService;
            _priceFormatter = priceFormatter;
            _productAttributeParser = productAttributeParser;
            _productAttributeService = productAttributeService;
            _productService = productService;
            _sciRepository = sciRepository;
            _shippingService = shippingService;
            _staticCacheManager = staticCacheManager;
            _storeContext = storeContext;
            _storeMappingService = storeMappingService;
            _urlHelperFactory = urlHelperFactory;
            _urlRecordService = urlRecordService;
            _workContext = workContext;
            _orderSettings = orderSettings;
            _shoppingCartSettings = shoppingCartSettings;
            _vendorService = vendorService;
            _slotService = slotService;
            _settingService = settingService;
            _categoryService = categoryService;
            _alchubGeneralService = alchubGeneralService;
        }

        #endregion Ctor

        #region Utilities

        /// <summary>
        /// Validates a product for standard properties
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <param name="shoppingCartType">Shopping cart type</param>
        /// <param name="product">Product</param>
        /// <param name="attributesXml">Attributes in XML format</param>
        /// <param name="customerEnteredPrice">Customer entered price</param>
        /// <param name="quantity">Quantity</param>
        /// <param name="shoppingCartItemId">Shopping cart identifier; pass 0 if it's a new item</param>
        /// <param name="storeId">Store identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the warnings
        /// </returns>
        protected virtual async Task<IList<string>> GetStandardWarningsAsync(Customer customer, ShoppingCartType shoppingCartType, Product product,
            string attributesXml, decimal customerEnteredPrice, int quantity, int shoppingCartItemId, int storeId, bool ismigrate = false)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            if (product == null)
                throw new ArgumentNullException(nameof(product));

            var warnings = new List<string>();

            //deleted
            if (product.Deleted)
            {
                warnings.Add(await _localizationService.GetResourceAsync("ShoppingCart.ProductDeleted"));
                return warnings;
            }

            //published
            if (!product.Published)
            {
                warnings.Add(await _localizationService.GetResourceAsync("ShoppingCart.ProductUnpublished"));
            }

            //check for master or group product is Deleted/Unpublished. 17-10-23
            if (shoppingCartType == ShoppingCartType.ShoppingCart)
            {
                var masterProduct = await _alchubGeneralService.GetMasterProductByUpcCodeAsync(product.UPCCode);
                //master deleted
                if (masterProduct == null || product.Deleted)
                {
                    warnings.Add(await _localizationService.GetResourceAsync("Alchub.ShoppingCart.MasterProductDeleted"));
                    return warnings;
                }

                //master published
                if (!masterProduct.Published)
                {
                    warnings.Add(await _localizationService.GetResourceAsync("Alchub.ShoppingCart.MasterProductUnpublished"));
                }

                //check for group product availability
                if (masterProduct.ParentGroupedProductId > 0)
                {
                    var groupProduct = await _productService.GetProductByIdAsync(masterProduct.ParentGroupedProductId);
                    if (groupProduct == null || groupProduct.Deleted)
                    {
                        warnings.Add(await _localizationService.GetResourceAsync("Alchub.ShoppingCart.GroupedProductDeleted"));
                        return warnings;
                    }

                    //master published
                    if (!groupProduct.Published)
                    {
                        warnings.Add(await _localizationService.GetResourceAsync("Alchub.ShoppingCart.GroupedProductUnpublished"));
                    }
                }
            }

            ////we can add only simple products
            //if (product.ProductType != ProductType.SimpleProduct)
            //{
            //    warnings.Add("This is not simple product");
            //}

            //ACL
            if (!await _aclService.AuthorizeAsync(product, customer))
            {
                warnings.Add(await _localizationService.GetResourceAsync("ShoppingCart.ProductUnpublished"));
            }

            //Store mapping
            if (!await _storeMappingService.AuthorizeAsync(product, storeId))
            {
                warnings.Add(await _localizationService.GetResourceAsync("ShoppingCart.ProductUnpublished"));
            }

            //disabled "add to cart" button
            if (shoppingCartType == ShoppingCartType.ShoppingCart && product.DisableBuyButton)
            {
                warnings.Add(await _localizationService.GetResourceAsync("ShoppingCart.BuyingDisabled"));
            }

            //disabled "add to wishlist" button
            if (shoppingCartType == ShoppingCartType.Wishlist && product.DisableWishlistButton)
            {
                warnings.Add(await _localizationService.GetResourceAsync("ShoppingCart.WishlistDisabled"));
            }

            var cartforSlotCheck = await GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, storeId);
            var cartitem = cartforSlotCheck.FirstOrDefault(item => item.ProductId == product.Id);
            if (cartitem != null)
            {
                //check whether slot remove or delete from database
                bool isExist = false;
                if (cartitem.IsPickup)
                {
                    var slotExist = await _slotService.GetPickupSlotById(cartitem.SlotId);
                    isExist = slotExist != null ? true : false;
                }
                else if (!cartitem.IsPickup)
                {
                    var slotExist = await _slotService.GetSlotById(cartitem.SlotId);
                    isExist = slotExist != null ? true : false;
                }
                if (isExist)
                {
                    var availableSlotDateTime = DateTime.SpecifyKind(cartitem.SlotStartTime, DateTimeKind.Utc);
                    var datetime = await _dateTimeHelper.ConvertToUserTimeAsync(DateTime.Now);
                    if (availableSlotDateTime < datetime)
                    {
                        warnings.Add(await _localizationService.GetResourceAsync("Alchub.ShoppingCart.SlotTime.Expiry"));
                    }
                }
                else
                {
                    warnings.Add(await _localizationService.GetResourceAsync("Alchub.ShoppingCart.Slot.Remove"));
                }
            }

            //++Alchub

            //Deliverable for shopping cart item
            if (!ismigrate)
            {
                if (shoppingCartType == ShoppingCartType.ShoppingCart)
                {
                    var (deliverableWarning, deliverable) = await IsItemDeliverable(product, customer, cartitem);
                    if (!deliverable)
                        warnings.Add(deliverableWarning);
                }
            }
            //--Alchub

            //call for price
            if (shoppingCartType == ShoppingCartType.ShoppingCart && product.CallForPrice &&
                    //also check whether the current user is impersonated
                    (!_orderSettings.AllowAdminsToBuyCallForPriceProducts || _workContext.OriginalCustomerIfImpersonated == null))
            {
                warnings.Add(await _localizationService.GetResourceAsync("Products.CallForPrice"));
            }

            //customer entered price
            if (product.CustomerEntersPrice)
            {
                if (customerEnteredPrice < product.MinimumCustomerEnteredPrice ||
                    customerEnteredPrice > product.MaximumCustomerEnteredPrice)
                {
                    var currentCurrency = await _workContext.GetWorkingCurrencyAsync();
                    var minimumCustomerEnteredPrice = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(product.MinimumCustomerEnteredPrice, currentCurrency);
                    var maximumCustomerEnteredPrice = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(product.MaximumCustomerEnteredPrice, currentCurrency);
                    warnings.Add(string.Format(await _localizationService.GetResourceAsync("ShoppingCart.CustomerEnteredPrice.RangeError"),
                        await _priceFormatter.FormatPriceAsync(minimumCustomerEnteredPrice, false, false),
                        await _priceFormatter.FormatPriceAsync(maximumCustomerEnteredPrice, false, false)));
                }
            }

            //quantity validation
            var hasQtyWarnings = false;
            if (quantity < product.OrderMinimumQuantity)
            {
                warnings.Add(string.Format(await _localizationService.GetResourceAsync("ShoppingCart.MinimumQuantity"), product.OrderMinimumQuantity));
                hasQtyWarnings = true;
            }

            if (quantity > product.OrderMaximumQuantity)
            {
                warnings.Add(string.Format(await _localizationService.GetResourceAsync("ShoppingCart.MaximumQuantity"), product.OrderMaximumQuantity));
                hasQtyWarnings = true;
            }

            var allowedQuantities = _productService.ParseAllowedQuantities(product);
            if (allowedQuantities.Length > 0 && !allowedQuantities.Contains(quantity))
            {
                warnings.Add(string.Format(await _localizationService.GetResourceAsync("ShoppingCart.AllowedQuantities"), string.Join(", ", allowedQuantities)));
            }

            var validateOutOfStock = shoppingCartType == ShoppingCartType.ShoppingCart || !_shoppingCartSettings.AllowOutOfStockItemsToBeAddedToWishlist;
            if (validateOutOfStock && !hasQtyWarnings)
            {
                switch (product.ManageInventoryMethod)
                {
                    case ManageInventoryMethod.DontManageStock:
                        //do nothing
                        break;
                    case ManageInventoryMethod.ManageStock:
                        if (product.BackorderMode == BackorderMode.NoBackorders)
                        {
                            var maximumQuantityCanBeAdded = await _productService.GetTotalStockQuantityAsync(product);

                            warnings.AddRange(await GetQuantityProductWarningsAsync(product, quantity, maximumQuantityCanBeAdded));

                            if (warnings.Any())
                                return warnings;

                            //validate product quantity with non combinable product attributes
                            var productAttributeMappings = await _productAttributeService.GetProductAttributeMappingsByProductIdAsync(product.Id);
                            if (productAttributeMappings?.Any() == true)
                            {
                                var onlyCombinableAttributes = productAttributeMappings.All(mapping => !mapping.IsNonCombinable());
                                if (!onlyCombinableAttributes)
                                {
                                    var cart = await GetShoppingCartAsync(customer, shoppingCartType, storeId);
                                    var totalAddedQuantity = cart
                                        .Where(item => item.ProductId == product.Id && item.Id != shoppingCartItemId)
                                        .Sum(product => product.Quantity);

                                    totalAddedQuantity += quantity;

                                    //counting a product into bundles
                                    foreach (var bundle in cart.Where(x => x.Id != shoppingCartItemId && !string.IsNullOrEmpty(x.AttributesXml)))
                                    {
                                        var attributeValues = await _productAttributeParser.ParseProductAttributeValuesAsync(bundle.AttributesXml);
                                        foreach (var attributeValue in attributeValues)
                                        {
                                            if (attributeValue.AttributeValueType == AttributeValueType.AssociatedToProduct && attributeValue.AssociatedProductId == product.Id)
                                                totalAddedQuantity += bundle.Quantity * attributeValue.Quantity;
                                        }
                                    }

                                    warnings.AddRange(await GetQuantityProductWarningsAsync(product, totalAddedQuantity, maximumQuantityCanBeAdded));
                                }
                            }

                            if (warnings.Any())
                                return warnings;

                            //validate product quantity and product quantity into bundles
                            if (string.IsNullOrEmpty(attributesXml))
                            {
                                var cart = await GetShoppingCartAsync(customer, shoppingCartType, storeId);
                                var totalQuantityInCart = cart.Where(item => item.ProductId == product.Id && item.Id != shoppingCartItemId && string.IsNullOrEmpty(item.AttributesXml))
                                    .Sum(product => product.Quantity);

                                totalQuantityInCart += quantity;

                                foreach (var bundle in cart.Where(x => x.Id != shoppingCartItemId && !string.IsNullOrEmpty(x.AttributesXml)))
                                {
                                    var attributeValues = await _productAttributeParser.ParseProductAttributeValuesAsync(bundle.AttributesXml);
                                    foreach (var attributeValue in attributeValues)
                                    {
                                        if (attributeValue.AttributeValueType == AttributeValueType.AssociatedToProduct && attributeValue.AssociatedProductId == product.Id)
                                            totalQuantityInCart += bundle.Quantity * attributeValue.Quantity;
                                    }
                                }

                                warnings.AddRange(await GetQuantityProductWarningsAsync(product, totalQuantityInCart, maximumQuantityCanBeAdded));
                            }
                        }

                        break;
                    case ManageInventoryMethod.ManageStockByAttributes:
                        var combination = await _productAttributeParser.FindProductAttributeCombinationAsync(product, attributesXml);
                        if (combination != null)
                        {
                            //combination exists
                            //let's check stock level
                            if (!combination.AllowOutOfStockOrders)
                                warnings.AddRange(await GetQuantityProductWarningsAsync(product, quantity, combination.StockQuantity));
                        }
                        else
                        {
                            //combination doesn't exist
                            if (product.AllowAddingOnlyExistingAttributeCombinations)
                            {
                                //maybe, is it better  to display something like "No such product/combination" message?
                                var productAvailabilityRange = await _dateRangeService.GetProductAvailabilityRangeByIdAsync(product.ProductAvailabilityRangeId);
                                var warning = productAvailabilityRange == null ? await _localizationService.GetResourceAsync("ShoppingCart.OutOfStock")
                                    : string.Format(await _localizationService.GetResourceAsync("ShoppingCart.AvailabilityRange"),
                                        await _localizationService.GetLocalizedAsync(productAvailabilityRange, range => range.Name));
                                warnings.Add(warning);
                            }
                        }

                        break;
                    default:
                        break;
                }
            }

            //availability dates
            var availableStartDateError = false;
            if (product.AvailableStartDateTimeUtc.HasValue)
            {
                var availableStartDateTime = DateTime.SpecifyKind(product.AvailableStartDateTimeUtc.Value, DateTimeKind.Utc);
                if (availableStartDateTime.CompareTo(DateTime.UtcNow) > 0)
                {
                    warnings.Add(await _localizationService.GetResourceAsync("ShoppingCart.NotAvailable"));
                    availableStartDateError = true;
                }
            }

            if (!product.AvailableEndDateTimeUtc.HasValue || availableStartDateError)
                return warnings;

            var availableEndDateTime = DateTime.SpecifyKind(product.AvailableEndDateTimeUtc.Value, DateTimeKind.Utc);
            if (availableEndDateTime.CompareTo(DateTime.UtcNow) < 0)
            {
                warnings.Add(await _localizationService.GetResourceAsync("ShoppingCart.NotAvailable"));
            }

            return warnings;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Get Vendor Minimum Order Amount Warnings
        /// </summary>
        /// <param name="cart"></param>
        /// <returns></returns>
        public virtual async Task<IList<string>> GetVendorMinimumOrderAmountWarningsAsync(IList<ShoppingCartItem> cart)
        {
            var warnings = new List<string>();

            if (cart == null)
                return warnings;

            var vendors = await _vendorService.GetVendorsByProductIdsAsync(cart.Select(x => x.ProductId).ToArray());

            if (vendors != null && vendors.Count > 0)
            {
                foreach (var vendor in vendors)
                {
                    warnings.AddRange(await GetVendorMinimumOrderAmountWarningsAsync(cart, vendor));
                }
            }

            return warnings;
        }

        /// <summary>
        /// Get Vendor Minimum Order Amount Warnings
        /// </summary>
        /// <param name="cart"></param>
        /// <param name="vendor"></param>
        /// <returns></returns>
        public virtual async Task<IList<string>> GetVendorMinimumOrderAmountWarningsAsync(IList<ShoppingCartItem> cart, Vendor vendor)
        {
            var warnings = new List<string>();
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
                        vendorSubtotal += (await GetSubTotalAsync(shoppingCartItem, true)).subTotal;
                    }
                }

                if (isVendorProductAvailable && vendorSubtotal < vendor.MinimumOrderAmount)
                {
                    warnings.Add(string.Format(await _localizationService.GetResourceAsync("Alchub.Vendor.MinimumOrderSubtotal.Warning.Message"),
                        await _priceFormatter.FormatPriceAsync(vendor.MinimumOrderAmount - vendorSubtotal, true, false),
                        await _priceFormatter.FormatPriceAsync(vendor.MinimumOrderAmount, true, false),
                        vendor.Id));
                }
            }

            return warnings;
        }

        /// <summary>
        /// Migrate shopping cart
        /// </summary>
        /// <param name="fromCustomer">From customer</param>
        /// <param name="toCustomer">To customer</param>
        /// <param name="includeCouponCodes">A value indicating whether to coupon codes (discount and gift card) should be also re-applied</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task MigrateShoppingCartAsync(Customer fromCustomer, Customer toCustomer, bool includeCouponCodes)
        {
            if (fromCustomer == null)
                throw new ArgumentNullException(nameof(fromCustomer));
            if (toCustomer == null)
                throw new ArgumentNullException(nameof(toCustomer));

            if (fromCustomer.Id == toCustomer.Id)
                return; //the same customer

            //shopping cart items
            var fromCart = await GetShoppingCartAsync(fromCustomer);

            for (var i = 0; i < fromCart.Count; i++)
            {
                var sci = fromCart[i];
                var product = await _productService.GetProductByIdAsync(sci.ProductId);

                await AddToCartAsync(toCustomer, product, sci.ShoppingCartType, sci.StoreId,
                    sci.AttributesXml, sci.CustomerEnteredPrice,
                    sci.RentalStartDateUtc, sci.RentalEndDateUtc, sci.Quantity, false, sci.IsPickup, sci.MasterProductId, sci.GroupedProductId, ismigrate: true);

                //Alchub Start
                var cart = await GetShoppingCartAsync(toCustomer);
                var shoppingcart = cart.FirstOrDefault(x => x.ProductId == product.Id && x.ShoppingCartType == sci.ShoppingCartType);
                if (shoppingcart != null)
                {
                    shoppingcart.SlotPrice = sci.SlotPrice;
                    shoppingcart.SlotStartTime = sci.SlotStartTime;
                    shoppingcart.SlotEndTime = sci.SlotEndTime;
                    shoppingcart.SlotTime = sci.SlotTime;
                    shoppingcart.SlotId = sci.SlotId;
                    shoppingcart.IsPickup = sci.IsPickup;
                    //master & grouped product fields
                    shoppingcart.MasterProductId = sci.MasterProductId;
                    shoppingcart.GroupedProductId = sci.GroupedProductId;
                    shoppingcart.CustomAttributesXml = sci.CustomAttributesXml;
                    await _sciRepository.UpdateAsync(shoppingcart);
                }
                //AlchubEnd

            }

            for (var i = 0; i < fromCart.Count; i++)
            {
                var sci = fromCart[i];
                await DeleteShoppingCartItemAsync(sci);
            }

            //copy discount and gift card coupon codes
            if (includeCouponCodes)
            {
                //discount
                foreach (var code in await _customerService.ParseAppliedDiscountCouponCodesAsync(fromCustomer))
                    await _customerService.ApplyDiscountCouponCodeAsync(toCustomer, code);

                //gift card
                foreach (var code in await _customerService.ParseAppliedGiftCardCouponCodesAsync(fromCustomer))
                    await _customerService.ApplyGiftCardCouponCodeAsync(toCustomer, code);

                //save customer
                await _customerService.UpdateCustomerAsync(toCustomer);
            }

            //move selected checkout attributes
            var store = await _storeContext.GetCurrentStoreAsync();
            var checkoutAttributesXml = await _genericAttributeService.GetAttributeAsync<string>(fromCustomer, NopCustomerDefaults.CheckoutAttributes, store.Id);
            await _genericAttributeService.SaveAttributeAsync(toCustomer, NopCustomerDefaults.CheckoutAttributes, checkoutAttributesXml, store.Id);

            //++Alchub

            //migrate searched location
            toCustomer.LastSearchedCoordinates = fromCustomer.LastSearchedCoordinates;
            toCustomer.LastSearchedText = fromCustomer.LastSearchedText;

            //save customer
            await _customerService.UpdateCustomerAsync(toCustomer);

            //--Alchub
        }

        /// <summary>
        /// Validates whether this shopping cart is valid
        /// </summary>
        /// <param name="shoppingCart">Shopping cart</param>
        /// <param name="checkoutAttributesXml">Checkout attributes in XML format</param>
        /// <param name="validateCheckoutAttributes">A value indicating whether to validate checkout attributes</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the warnings
        /// </returns>
        public virtual async Task<IList<string>> GetShoppingCartWarningsAsync(IList<ShoppingCartItem> shoppingCart,
            string checkoutAttributesXml, bool validateCheckoutAttributes)
        {
            var warnings = new List<string>();

            if (shoppingCart.Count > _shoppingCartSettings.MaximumShoppingCartItems)
                warnings.Add(string.Format(await _localizationService.GetResourceAsync("ShoppingCart.MaximumShoppingCartItems"), _shoppingCartSettings.MaximumShoppingCartItems));

            var hasStandartProducts = false;
            var hasRecurringProducts = false;

            foreach (var sci in shoppingCart)
            {
                var product = await _productService.GetProductByIdAsync(sci.ProductId);
                if (product == null)
                {
                    warnings.Add(string.Format(await _localizationService.GetResourceAsync("ShoppingCart.CannotLoadProduct"), sci.ProductId));
                    return warnings;
                }

                if (product.IsRecurring)
                    hasRecurringProducts = true;
                else
                    hasStandartProducts = true;
            }

            //don't mix standard and recurring products
            if (hasStandartProducts && hasRecurringProducts)
                warnings.Add(await _localizationService.GetResourceAsync("ShoppingCart.CannotMixStandardAndAutoshipProducts"));

            //recurring cart validation
            if (hasRecurringProducts)
            {
                var cyclesError = (await GetRecurringCycleInfoAsync(shoppingCart)).error;
                if (!string.IsNullOrEmpty(cyclesError))
                {
                    warnings.Add(cyclesError);
                    return warnings;
                }
            }

            //++Alchub

            //location searched warning.
            //var customer = await _workContext.GetCurrentCustomerAsync();
            //if (string.IsNullOrEmpty(customer.LastSearchedCoordinates))
            //    warnings.Add(await _localizationService.GetResourceAsync("Alchub.ShoppingCart.Warning.EnterDeliveryLocation"));

            //--Alchub

            //validate checkout attributes
            if (!validateCheckoutAttributes)
                return warnings;

            //selected attributes
            var attributes1 = await _checkoutAttributeParser.ParseCheckoutAttributesAsync(checkoutAttributesXml);

            //existing checkout attributes
            var excludeShippableAttributes = !await ShoppingCartRequiresShippingAsync(shoppingCart);
            var store = await _storeContext.GetCurrentStoreAsync();
            var attributes2 = await _checkoutAttributeService.GetAllCheckoutAttributesAsync(store.Id, excludeShippableAttributes);

            //validate conditional attributes only (if specified)
            attributes2 = await attributes2.WhereAwait(async x =>
            {
                var conditionMet = await _checkoutAttributeParser.IsConditionMetAsync(x, checkoutAttributesXml);
                return !conditionMet.HasValue || conditionMet.Value;
            }).ToListAsync();

            foreach (var a2 in attributes2)
            {
                if (!a2.IsRequired)
                    continue;

                var found = false;
                //selected checkout attributes
                foreach (var a1 in attributes1)
                {
                    if (a1.Id != a2.Id)
                        continue;

                    var attributeValuesStr = _checkoutAttributeParser.ParseValues(checkoutAttributesXml, a1.Id);
                    foreach (var str1 in attributeValuesStr)
                        if (!string.IsNullOrEmpty(str1.Trim()))
                        {
                            found = true;
                            break;
                        }
                }

                if (found)
                    continue;

                //if not found
                warnings.Add(!string.IsNullOrEmpty(await _localizationService.GetLocalizedAsync(a2, a => a.TextPrompt))
                    ? await _localizationService.GetLocalizedAsync(a2, a => a.TextPrompt)
                    : string.Format(await _localizationService.GetResourceAsync("ShoppingCart.SelectAttribute"),
                        await _localizationService.GetLocalizedAsync(a2, a => a.Name)));
            }

            //now validation rules

            //minimum length
            foreach (var ca in attributes2)
            {
                string enteredText;
                int enteredTextLength;

                if (ca.ValidationMinLength.HasValue)
                {
                    if (ca.AttributeControlType == AttributeControlType.TextBox ||
                        ca.AttributeControlType == AttributeControlType.MultilineTextbox)
                    {
                        enteredText = _checkoutAttributeParser.ParseValues(checkoutAttributesXml, ca.Id).FirstOrDefault();
                        enteredTextLength = string.IsNullOrEmpty(enteredText) ? 0 : enteredText.Length;

                        if (ca.ValidationMinLength.Value > enteredTextLength)
                        {
                            warnings.Add(string.Format(await _localizationService.GetResourceAsync("ShoppingCart.TextboxMinimumLength"), await _localizationService.GetLocalizedAsync(ca, a => a.Name), ca.ValidationMinLength.Value));
                        }
                    }
                }

                //maximum length
                if (!ca.ValidationMaxLength.HasValue)
                    continue;

                if (ca.AttributeControlType != AttributeControlType.TextBox && ca.AttributeControlType != AttributeControlType.MultilineTextbox)
                    continue;

                enteredText = _checkoutAttributeParser.ParseValues(checkoutAttributesXml, ca.Id).FirstOrDefault();
                enteredTextLength = string.IsNullOrEmpty(enteredText) ? 0 : enteredText.Length;

                if (ca.ValidationMaxLength.Value < enteredTextLength)
                {
                    warnings.Add(string.Format(await _localizationService.GetResourceAsync("ShoppingCart.TextboxMaximumLength"), await _localizationService.GetLocalizedAsync(ca, a => a.Name), ca.ValidationMaxLength.Value));
                }
            }

            return warnings;
        }

        /// <summary>
        /// Check whether shopping cart item is deliverable or not. 
        /// </summary>
        /// <param name="product"></param>
        /// <returns></returns>
        public virtual async Task<(string warning, bool deliverable)> IsItemDeliverable(Product product, Customer customer, ShoppingCartItem shoppingCartItem = null)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            string error = string.Empty;

            //location searched warning.
            if (string.IsNullOrEmpty(customer.LastSearchedCoordinates))
                return (await _localizationService.GetResourceAsync("Alchub.ShoppingCartItem.Warning.MissingDeliveryLocation"), false);

            //get available vendors
            var includePickupByDefualt = shoppingCartItem != null ? shoppingCartItem.IsPickup : true; //(15-02-23: Check according pickup/delivery item)
            var availableVendors = await _vendorService.GetAvailableGeoFenceVendorsAsync(customer, includePickupByDefualt, product);
            if (availableVendors.Any(x => x.Id == product.VendorId))
                return ("", true);

            return (await _localizationService.GetResourceAsync("Alchub.ShoppingCart.Warning.Product.NotDeliverable"), false);
        }


        /// <summary>
        /// Add a product to shopping cart
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <param name="product">Product</param>
        /// <param name="shoppingCartType">Shopping cart type</param>
        /// <param name="storeId">Store identifier</param>
        /// <param name="attributesXml">Attributes in XML format</param>
        /// <param name="customerEnteredPrice">The price enter by a customer</param>
        /// <param name="rentalStartDate">Rental start date</param>
        /// <param name="rentalEndDate">Rental end date</param>
        /// <param name="quantity">Quantity</param>
        /// <param name="addRequiredProducts">Whether to add required products</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the warnings
        /// </returns>
        public virtual async Task<IList<string>> AddToCartAsync(Customer customer, Product product,
            ShoppingCartType shoppingCartType, int storeId, string attributesXml = null,
            decimal customerEnteredPrice = decimal.Zero,
            DateTime? rentalStartDate = null, DateTime? rentalEndDate = null,
            int quantity = 1, bool addRequiredProducts = true, bool ispickup = false, int masterProductId = 0, int groupedProductId = 0, bool ismigrate = false)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            if (product == null)
                throw new ArgumentNullException(nameof(product));

            var warnings = new List<string>();
            if (shoppingCartType == ShoppingCartType.ShoppingCart && !await _permissionService.AuthorizeAsync(StandardPermissionProvider.EnableShoppingCart, customer))
            {
                warnings.Add("Shopping cart is disabled");
                return warnings;
            }

            if (shoppingCartType == ShoppingCartType.Wishlist && !await _permissionService.AuthorizeAsync(StandardPermissionProvider.EnableWishlist, customer))
            {
                warnings.Add("Wishlist is disabled");
                return warnings;
            }

            if (customer.IsSearchEngineAccount())
            {
                warnings.Add("Search engine can't add to cart");
                return warnings;
            }

            if (quantity <= 0)
            {
                warnings.Add(await _localizationService.GetResourceAsync("ShoppingCart.QuantityShouldPositive"));
                return warnings;
            }

            //reset checkout info
            await _customerService.ResetCheckoutDataAsync(customer, storeId);

            var cart = await GetShoppingCartAsync(customer, shoppingCartType, storeId);

            var shoppingCartItem = await FindShoppingCartItemInTheCartAsync(cart,
                shoppingCartType, product, attributesXml, customerEnteredPrice,
                rentalStartDate, rentalEndDate);

            if (shoppingCartItem != null)
            {
                //update existing shopping cart item
                var newQuantity = shoppingCartItem.Quantity + quantity;
                warnings.AddRange(await GetShoppingCartItemWarningsAsync(customer, shoppingCartType, product,
                    storeId, attributesXml,
                    customerEnteredPrice, rentalStartDate, rentalEndDate,
                    newQuantity, addRequiredProducts, shoppingCartItem.Id, ismigrate: ismigrate));

                if (warnings.Any())
                    return warnings;

                shoppingCartItem.AttributesXml = attributesXml;
                shoppingCartItem.Quantity = newQuantity;
                shoppingCartItem.UpdatedOnUtc = DateTime.UtcNow;

                await _sciRepository.UpdateAsync(shoppingCartItem);
                if (shoppingCartType == ShoppingCartType.ShoppingCart)
                {
                    await UpdateSlotIncartAsync(customer, product, shoppingCartItem.ShoppingCartType, shoppingCartItem.StoreId, shoppingCartItem.Id, warnings, ispickup, masterProductId, groupedProductId);
                }
            }
            else
            {
                //new shopping cart item
                warnings.AddRange(await GetShoppingCartItemWarningsAsync(customer, shoppingCartType, product,
                    storeId, attributesXml, customerEnteredPrice,
                    rentalStartDate, rentalEndDate,
                    quantity, addRequiredProducts, ismigrate: ismigrate));

                if (warnings.Any())
                    return warnings;

                //maximum items validation
                switch (shoppingCartType)
                {
                    case ShoppingCartType.ShoppingCart:
                        if (cart.Count >= _shoppingCartSettings.MaximumShoppingCartItems)
                        {
                            warnings.Add(string.Format(await _localizationService.GetResourceAsync("ShoppingCart.MaximumShoppingCartItems"), _shoppingCartSettings.MaximumShoppingCartItems));
                            return warnings;
                        }

                        break;
                    case ShoppingCartType.Wishlist:
                        if (cart.Count >= _shoppingCartSettings.MaximumWishlistItems)
                        {
                            warnings.Add(string.Format(await _localizationService.GetResourceAsync("ShoppingCart.MaximumWishlistItems"), _shoppingCartSettings.MaximumWishlistItems));
                            return warnings;
                        }

                        break;
                    default:
                        break;
                }

                var now = DateTime.UtcNow;
                shoppingCartItem = new ShoppingCartItem
                {
                    ShoppingCartType = shoppingCartType,
                    StoreId = storeId,
                    ProductId = product.Id,
                    AttributesXml = attributesXml,
                    CustomerEnteredPrice = customerEnteredPrice,
                    Quantity = quantity,
                    RentalStartDateUtc = rentalStartDate,
                    RentalEndDateUtc = rentalEndDate,
                    CreatedOnUtc = now,
                    UpdatedOnUtc = now,
                    CustomerId = customer.Id,
                };

                await _sciRepository.InsertAsync(shoppingCartItem);


                //Alchub Start
                if (shoppingCartType == ShoppingCartType.ShoppingCart)
                {
                    if (!ismigrate)
                    {
                        await AddSlotIncartAsync(customer, shoppingCartItem.Id, product, shoppingCartType, storeId, warnings, ispickup, masterProductId, groupedProductId);
                    }
                }
                //Alchub End

                //updated "HasShoppingCartItems" property used for performance optimization
                customer.HasShoppingCartItems = !IsCustomerShoppingCartEmpty(customer);

                await _customerService.UpdateCustomerAsync(customer);

            }


            return warnings;
        }

        public virtual async Task<IList<string>> UpdateShoppingCartItemAsync(Customer customer,
           int shoppingCartItemId, string attributesXml,
           decimal customerEnteredPrice,
           DateTime? rentalStartDate = null, DateTime? rentalEndDate = null,
           int quantity = 1, bool resetCheckoutData = true, bool ispickup = false, int masterProductId = 0, int groupedProductId = 0)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            var warnings = new List<string>();

            var shoppingCartItem = await _sciRepository.GetByIdAsync(shoppingCartItemId, cache => default);

            if (shoppingCartItem == null || shoppingCartItem.CustomerId != customer.Id)
                return warnings;

            if (resetCheckoutData)
            {
                //reset checkout data
                await _customerService.ResetCheckoutDataAsync(customer, shoppingCartItem.StoreId);
            }

            var product = await _productService.GetProductByIdAsync(shoppingCartItem.ProductId);

            if (quantity > 0)
            {
                //check warnings
                warnings.AddRange(await GetShoppingCartItemWarningsAsync(customer, shoppingCartItem.ShoppingCartType,
                    product, shoppingCartItem.StoreId,
                    attributesXml, customerEnteredPrice,
                    rentalStartDate, rentalEndDate, quantity, false, shoppingCartItemId));
                if (warnings.Any())
                    return warnings;

                //if everything is OK, then update a shopping cart item
                shoppingCartItem.Quantity = quantity;
                shoppingCartItem.AttributesXml = attributesXml;
                shoppingCartItem.CustomerEnteredPrice = customerEnteredPrice;
                shoppingCartItem.RentalStartDateUtc = rentalStartDate;
                shoppingCartItem.RentalEndDateUtc = rentalEndDate;
                shoppingCartItem.UpdatedOnUtc = DateTime.UtcNow;

                await _sciRepository.UpdateAsync(shoppingCartItem);
                await _customerService.UpdateCustomerAsync(customer);

                //Alchub Start
                await UpdateSlotIncartAsync(customer, product, shoppingCartItem.ShoppingCartType, shoppingCartItem.StoreId, shoppingCartItemId, warnings, ispickup, masterProductId, groupedProductId);
                //Alchub End
            }
            else
            {
                //check warnings for required products
                warnings.AddRange(await GetRequiredProductWarningsAsync(customer, shoppingCartItem.ShoppingCartType,
                    product, shoppingCartItem.StoreId, quantity, false, shoppingCartItemId));
                if (warnings.Any())
                    return warnings;

                //delete a shopping cart item
                await DeleteShoppingCartItemAsync(shoppingCartItem, resetCheckoutData, true);
            }
            return warnings;
        }


        public virtual async Task<IList<string>> GetShoppingCartItemWarningsAsync(Customer customer, ShoppingCartType shoppingCartType,
    Product product, int storeId,
    string attributesXml, decimal customerEnteredPrice,
    DateTime? rentalStartDate = null, DateTime? rentalEndDate = null,
    int quantity = 1, bool addRequiredProducts = true, int shoppingCartItemId = 0,
    bool getStandardWarnings = true, bool getAttributesWarnings = true,
    bool getGiftCardWarnings = true, bool getRequiredProductWarnings = true,
    bool getRentalWarnings = true, bool ismigrate = false)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            var warnings = new List<string>();

            //standard properties
            if (getStandardWarnings)
                warnings.AddRange(await GetStandardWarningsAsync(customer, shoppingCartType, product, attributesXml, customerEnteredPrice, quantity, shoppingCartItemId, storeId, ismigrate: ismigrate));

            //selected attributes
            if (getAttributesWarnings)
                warnings.AddRange(await GetShoppingCartItemAttributeWarningsAsync(customer, shoppingCartType, product, quantity, attributesXml, false, false, shoppingCartItemId));

            //gift cards
            if (getGiftCardWarnings)
                warnings.AddRange(await GetShoppingCartItemGiftCardWarningsAsync(shoppingCartType, product, attributesXml));

            //required products
            if (getRequiredProductWarnings)
                warnings.AddRange(await GetRequiredProductWarningsAsync(customer, shoppingCartType, product, storeId, quantity, addRequiredProducts, shoppingCartItemId));

            //rental products
            if (getRentalWarnings)
                warnings.AddRange(await GetRentalProductWarningsAsync(product, rentalStartDate, rentalEndDate));

            return warnings;
        }
        #endregion


        #region Alchub Slot

        public virtual async Task<IList<string>> AddSlotIncartAsync(Customer customer, int shoppingCartItemId, Product product,
            ShoppingCartType shoppingCartType, int storeId, List<string> warnings, bool ispickup = false, int masterProductId = 0, int groupedProductId = 0)
        {

            // Add Slot warning
            var addDay = await _settingService.GetSettingByKeyAsync<int>("Alchub.Slot.Visible.After.Day", defaultValue: 1);
            var addHour = await _settingService.GetSettingByKeyAsync<int>("Alchub.Slot.Add.Hour", defaultValue: 0);
            var addMintues = await _settingService.GetSettingByKeyAsync<int>("Alchub.Slot.Add.Mintues", defaultValue: 0);
            var dateTime = await _dateTimeHelper.ConvertToUserTimeAsync(DateTime.UtcNow);
            //dateTime = dateTime.AddDays(addDay).AddHours(addHour).AddMinutes(addMintues);
            var customerProductSlot = await _slotService.GetCustomerProductSlotDeliveryPickupId(product.Id, customer.Id, dateTime, ispickup);
            var currentcart = await GetShoppingCartAsync(customer, shoppingCartType, storeId);
            if (customerProductSlot == null)
            {
                warnings.Add(await _localizationService.GetResourceAsync("Alchub.Product.Slot.Not.Available.AddCart"));
                var itemToDelete = await _sciRepository.GetByIdAsync(shoppingCartItemId, cache => default);
                if (itemToDelete != null)
                {
                    await DeleteShoppingCartItemAsync(itemToDelete);
                    return warnings;
                }
            }
            else
            {
                if (customerProductSlot.IsPickup)
                {
                    var slotSelect = await _slotService.GetPickupSlotById(customerProductSlot.SlotId);
                    if (slotSelect == null)
                    {
                        warnings.Add(await _localizationService.GetResourceAsync("Alchub.Product.Slot.Not.Available.AddCart"));
                        var itemToDelete = await _sciRepository.GetByIdAsync(shoppingCartItemId, cache => default);
                        if (itemToDelete != null)
                        {
                            await DeleteShoppingCartItemAsync(itemToDelete);
                            return warnings;
                        }
                    }
                    else
                    {
                        var productCategories = (await _categoryService.GetProductCategoriesByProductIdAsync(product.Id)).Select(x => x.CategoryId).ToList();
                        var isUnavailability = await _slotService.CheckSlotAvailability(slotSelect.Id, slotSelect.Start, slotSelect.End, slotSelect.Capacity, false);
                        if (!(isUnavailability) || await _slotService.FindSlotCategoryIdExist(slotSelect.Id, productCategories))
                        {
                            warnings.Add(await _localizationService.GetResourceAsync("Alchub.Product.Slot.Not.Available.AddCart"));
                            var itemToDelete = await _sciRepository.GetByIdAsync(shoppingCartItemId, cache => default);
                            if (itemToDelete != null)
                            {
                                await DeleteShoppingCartItemAsync(itemToDelete);
                                return warnings;
                            }
                        }
                    }

                }
                else
                {
                    var slotSelect = await _slotService.GetSlotById(customerProductSlot.SlotId);
                    if (slotSelect == null)
                    {
                        warnings.Add(await _localizationService.GetResourceAsync("Alchub.Product.Slot.Not.Available.AddCart"));
                        var itemToDelete = await _sciRepository.GetByIdAsync(shoppingCartItemId, cache => default);
                        if (itemToDelete != null)
                        {
                            await DeleteShoppingCartItemAsync(itemToDelete);
                            return warnings;
                        }
                    }
                    else
                    {
                        var productCategories = (await _categoryService.GetProductCategoriesByProductIdAsync(product.Id)).Select(x => x.CategoryId).ToList();
                        var isUnavailability = await _slotService.CheckSlotAvailability(slotSelect.Id, slotSelect.Start, slotSelect.End, slotSelect.Capacity, false);
                        if (!(isUnavailability) || await _slotService.FindSlotCategoryIdExist(slotSelect.Id, productCategories))
                        {
                            warnings.Add(await _localizationService.GetResourceAsync("Alchub.Product.Slot.Not.Available.AddCart"));
                            var itemToDelete = await _sciRepository.GetByIdAsync(shoppingCartItemId, cache => default);
                            if (itemToDelete != null)
                            {
                                await DeleteShoppingCartItemAsync(itemToDelete);
                                return warnings;
                            }
                        }
                    }

                }

                var store = await _storeContext.GetCurrentStoreAsync();
                if (customerProductSlot != null)
                {
                    decimal price = 0;
                    if (customerProductSlot.IsPickup)
                    {
                        var slot = await _slotService.GetPickupSlotById(customerProductSlot.SlotId);
                        price = slot != null ? slot.Price : decimal.Zero;
                    }
                    else
                    {
                        var slot = await _slotService.GetSlotById(customerProductSlot.SlotId);
                        price = slot != null ? slot.Price : decimal.Zero;
                    }
                    //var shoppingcart = currentcart.FirstOrDefault(x => x.ProductId == customerProductSlot.ProductId);
                    var shoppingcart = await _sciRepository.GetByIdAsync(shoppingCartItemId, cache => default);
                    //product alchub custom attributes (for grouped product selected variant)
                    var masterProduct = await _productService.GetProductByIdAsync(masterProductId);
                    var customAttributes = await _productAttributeParser.ParseCustomProductAttributesAsync(masterProduct, groupedProductId);
                    if (shoppingcart != null)
                    {
                        shoppingcart.SlotPrice = price;
                        shoppingcart.SlotStartTime = customerProductSlot.StartTime;
                        shoppingcart.SlotEndTime = customerProductSlot.EndDateTime;
                        shoppingcart.SlotTime = customerProductSlot.EndTime;
                        shoppingcart.SlotId = customerProductSlot.SlotId;
                        shoppingcart.IsPickup = customerProductSlot.IsPickup;
                        //master & grouped product fields
                        shoppingcart.MasterProductId = masterProductId;
                        shoppingcart.GroupedProductId = groupedProductId;
                        shoppingcart.CustomAttributesXml = customAttributes;
                        await _sciRepository.UpdateAsync(shoppingcart);
                    }
                }
            }

            return warnings;
        }


        public virtual async Task<IList<string>> UpdateSlotIncartAsync(Customer customer, Product product,
            ShoppingCartType shoppingCartType, int storeId, int shoppingCartItemId, List<string> warnings, bool ispickup = false, int masterProductId = 0, int groupedProductId = 0)
        {

            //search with the same cart type as specified
            // Add Slot warning
            var addDay = await _settingService.GetSettingByKeyAsync<int>("Alchub.Slot.Visible.After.Day", defaultValue: 1);
            var addHour = await _settingService.GetSettingByKeyAsync<int>("Alchub.Slot.Add.Hour", defaultValue: 0);
            var addMintues = await _settingService.GetSettingByKeyAsync<int>("Alchub.Slot.Add.Mintues", defaultValue: 0);
            var dateTime = await _dateTimeHelper.ConvertToUserTimeAsync(DateTime.UtcNow);
            //dateTime = dateTime.AddDays(addDay).AddHours(addHour).AddMinutes(addMintues);
            var customerProductSlot = await _slotService.GetCustomerProductSlotDeliveryPickupId(product.Id, customer.Id, dateTime, ispickup);
            if (customerProductSlot == null)
            {
                warnings.Add(await _localizationService.GetResourceAsync("Alchub.Product.Slot.Not.Available.AddCart"));
                var itemToDelete = await _sciRepository.GetByIdAsync(shoppingCartItemId, cache => default);
                if (itemToDelete != null)
                {
                    await DeleteShoppingCartItemAsync(itemToDelete);
                    return warnings;
                }
            }
            else
            {
                if (customerProductSlot.IsPickup)
                {
                    var slotSelect = await _slotService.GetPickupSlotById(customerProductSlot.SlotId);
                    if (slotSelect == null)
                    {
                        warnings.Add(await _localizationService.GetResourceAsync("Alchub.Product.Slot.Not.Available.AddCart"));
                        var itemToDelete = await _sciRepository.GetByIdAsync(shoppingCartItemId, cache => default);
                        if (itemToDelete != null)
                        {
                            await DeleteShoppingCartItemAsync(itemToDelete);
                            return warnings;
                        }
                    }
                    else
                    {
                        var productCategories = (await _categoryService.GetProductCategoriesByProductIdAsync(product.Id)).Select(x => x.CategoryId).ToList();
                        var isUnavailability = await _slotService.CheckSlotAvailability(slotSelect.Id, slotSelect.Start, slotSelect.End, slotSelect.Capacity, false);
                        if (!(isUnavailability) || await _slotService.FindSlotCategoryIdExist(slotSelect.Id, productCategories))
                        {
                            warnings.Add(await _localizationService.GetResourceAsync("Alchub.Product.Slot.Not.Available.AddCart"));
                            var itemToDelete = await _sciRepository.GetByIdAsync(shoppingCartItemId, cache => default);
                            if (itemToDelete != null)
                            {
                                await DeleteShoppingCartItemAsync(itemToDelete);
                                return warnings;
                            }
                        }
                    }

                }
                else
                {
                    var slotSelect = await _slotService.GetSlotById(customerProductSlot.SlotId);
                    if (slotSelect == null)
                    {
                        warnings.Add(await _localizationService.GetResourceAsync("Alchub.Product.Slot.Not.Available.AddCart"));
                        var itemToDelete = await _sciRepository.GetByIdAsync(shoppingCartItemId, cache => default);
                        if (itemToDelete != null)
                        {
                            await DeleteShoppingCartItemAsync(itemToDelete);
                            return warnings;
                        }
                    }
                    else
                    {
                        var productCategories = (await _categoryService.GetProductCategoriesByProductIdAsync(product.Id)).Select(x => x.CategoryId).ToList();
                        var isUnavailability = await _slotService.CheckSlotAvailability(slotSelect.Id, slotSelect.Start, slotSelect.End, slotSelect.Capacity, false);
                        if (!(isUnavailability) || await _slotService.FindSlotCategoryIdExist(slotSelect.Id, productCategories))
                        {
                            warnings.Add(await _localizationService.GetResourceAsync("Alchub.Product.Slot.Not.Available.AddCart"));
                            var itemToDelete = await _sciRepository.GetByIdAsync(shoppingCartItemId, cache => default);
                            if (itemToDelete != null)
                            {
                                await DeleteShoppingCartItemAsync(itemToDelete);
                                return warnings;
                            }
                        }
                    }

                }

                var store = await _storeContext.GetCurrentStoreAsync();
                if (customerProductSlot != null)
                {
                    decimal price = 0;
                    if (customerProductSlot.IsPickup)
                    {
                        var slot = await _slotService.GetPickupSlotById(customerProductSlot.SlotId);
                        price = slot != null ? slot.Price : decimal.Zero;
                    }
                    else
                    {
                        var slot = await _slotService.GetSlotById(customerProductSlot.SlotId);
                        price = slot != null ? slot.Price : decimal.Zero;
                    }
                    //product alchub custom attributes (for grouped product selected variant)
                    var masterProduct = await _productService.GetProductByIdAsync(masterProductId);
                    var customAttributes = await _productAttributeParser.ParseCustomProductAttributesAsync(masterProduct, groupedProductId);
                    var shoppingCartItem = await _sciRepository.GetByIdAsync(shoppingCartItemId, cache => default);
                    if (shoppingCartItem != null)
                    {
                        shoppingCartItem.SlotPrice = price;
                        shoppingCartItem.SlotStartTime = customerProductSlot.StartTime;
                        shoppingCartItem.SlotEndTime = customerProductSlot.EndDateTime;
                        shoppingCartItem.SlotTime = customerProductSlot.EndTime;
                        shoppingCartItem.SlotId = customerProductSlot.SlotId;
                        shoppingCartItem.IsPickup = customerProductSlot.IsPickup;
                        //master & grouped product fields
                        shoppingCartItem.MasterProductId = masterProductId;
                        shoppingCartItem.GroupedProductId = groupedProductId;
                        shoppingCartItem.CustomAttributesXml = customAttributes;
                        await _sciRepository.UpdateAsync(shoppingCartItem);
                    }
                }
            }

            return warnings;
        }

        #endregion
    }
}