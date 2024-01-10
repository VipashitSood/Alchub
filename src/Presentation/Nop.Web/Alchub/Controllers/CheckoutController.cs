using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Stores;
using Nop.Core.Http.Extensions;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Shipping;
using Nop.Web.Extensions;
using Nop.Web.Factories;
using Nop.Web.Framework.Controllers;
using Nop.Web.Models.Checkout;

namespace Nop.Web.Controllers
{
    public partial class CheckoutController : BasePublicController
    {
        #region Fields

        private readonly AddressSettings _addressSettings;
        private readonly CustomerSettings _customerSettings;
        private readonly IAddressAttributeParser _addressAttributeParser;
        private readonly IAddressModelFactory _addressModelFactory;
        private readonly IAddressService _addressService;
        private readonly ICheckoutModelFactory _checkoutModelFactory;
        private readonly ICountryService _countryService;
        private readonly ICustomerService _customerService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ILocalizationService _localizationService;
        private readonly ILogger _logger;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IOrderService _orderService;
        private readonly IPaymentPluginManager _paymentPluginManager;
        private readonly IPaymentService _paymentService;
        private readonly IProductService _productService;
        private readonly IShippingService _shippingService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IStoreContext _storeContext;
        private readonly IWebHelper _webHelper;
        private readonly IWorkContext _workContext;
        private readonly OrderSettings _orderSettings;
        private readonly PaymentSettings _paymentSettings;
        private readonly RewardPointsSettings _rewardPointsSettings;
        private readonly ShippingSettings _shippingSettings;
        private readonly INotificationService _notificationService;

        #endregion

        #region Ctor

        public CheckoutController(AddressSettings addressSettings,
            CustomerSettings customerSettings,
            IAddressAttributeParser addressAttributeParser,
            IAddressModelFactory addressModelFactory,
            IAddressService addressService,
            ICheckoutModelFactory checkoutModelFactory,
            ICountryService countryService,
            ICustomerService customerService,
            IGenericAttributeService genericAttributeService,
            ILocalizationService localizationService,
            ILogger logger,
            IOrderProcessingService orderProcessingService,
            IOrderService orderService,
            IPaymentPluginManager paymentPluginManager,
            IPaymentService paymentService,
            IProductService productService,
            IShippingService shippingService,
            IShoppingCartService shoppingCartService,
            IStoreContext storeContext,
            IWebHelper webHelper,
            IWorkContext workContext,
            OrderSettings orderSettings,
            PaymentSettings paymentSettings,
            RewardPointsSettings rewardPointsSettings,
            ShippingSettings shippingSettings,
            INotificationService notificationService)
        {
            _addressSettings = addressSettings;
            _customerSettings = customerSettings;
            _addressAttributeParser = addressAttributeParser;
            _addressModelFactory = addressModelFactory;
            _addressService = addressService;
            _checkoutModelFactory = checkoutModelFactory;
            _countryService = countryService;
            _customerService = customerService;
            _genericAttributeService = genericAttributeService;
            _localizationService = localizationService;
            _logger = logger;
            _orderProcessingService = orderProcessingService;
            _orderService = orderService;
            _paymentPluginManager = paymentPluginManager;
            _paymentService = paymentService;
            _productService = productService;
            _shippingService = shippingService;
            _shoppingCartService = shoppingCartService;
            _storeContext = storeContext;
            _webHelper = webHelper;
            _workContext = workContext;
            _orderSettings = orderSettings;
            _paymentSettings = paymentSettings;
            _rewardPointsSettings = rewardPointsSettings;
            _shippingSettings = shippingSettings;
            _notificationService = notificationService;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Save Customer GeoCoordinates
        /// </summary>
        /// <param name="customer"></param>
        /// <returns></returns>
        public virtual async Task<string> SaveCustomerGeoCoordinatesAsync(Customer customer)
        {
            var billingAddress = await _addressService.GetAddressByIdAsync(customer.BillingAddressId ?? 0);
            customer.LastSearchedCoordinates = billingAddress?.GeoLocationCoordinates;
            customer.LastSearchedText = billingAddress?.Address1;
            await _customerService.UpdateCustomerAsync(customer);

            return customer.LastSearchedText;
        }

        /// <summary>
        /// check dose shopping cart contains any level of errors 
        /// </summary>
        /// <param name="cart"></param>
        /// <returns></returns>
        private async Task<bool> ShoppingCartHasAnyError(IList<ShoppingCartItem> cart, Customer customer, Store store)
        {
            //validate the entire shopping cart
            var warnings = await _shoppingCartService.GetShoppingCartWarningsAsync(cart, "", false);
            if (warnings.Any())
                return true;

            //validate individual cart items
            foreach (var sci in cart)
            {
                var product = await _productService.GetProductByIdAsync(sci.ProductId);

                var sciWarnings = await _shoppingCartService.GetShoppingCartItemWarningsAsync(customer,
                    sci.ShoppingCartType, product, store.Id, sci.AttributesXml,
                    sci.CustomerEnteredPrice, sci.RentalStartDateUtc, sci.RentalEndDateUtc, sci.Quantity, false, sci.Id);
                if (sciWarnings.Any())
                    return true;
            }

            //vendor minimum amount
            var vendorMinimumOrderAmountWarnings = await _shoppingCartService.GetVendorMinimumOrderAmountWarningsAsync(cart);
            if (vendorMinimumOrderAmountWarnings.Any())
                return true;

            return false;
        }

        #endregion Utilities

        #region Methods (common)

        public virtual async Task<IActionResult> Index()
        {
            //validation
            if (_orderSettings.CheckoutDisabled)
                return RedirectToRoute("ShoppingCart");

            var customer = await _workContext.GetCurrentCustomerAsync();
            var store = await _storeContext.GetCurrentStoreAsync();
            var cart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, store.Id);

            if (!cart.Any())
                return RedirectToRoute("ShoppingCart");

            var cartProductIds = cart.Select(ci => ci.ProductId).ToArray();
            var downloadableProductsRequireRegistration =
                _customerSettings.RequireRegistrationForDownloadableProducts && await _productService.HasAnyDownloadableProductAsync(cartProductIds);

            if (await _customerService.IsGuestAsync(customer) && (!_orderSettings.AnonymousCheckoutAllowed || downloadableProductsRequireRegistration))
                return Challenge();

            //if we have only "button" payment methods available (displayed on the shopping cart page, not during checkout),
            //then we should allow standard checkout
            //all payment methods (do not filter by country here as it could be not specified yet)
            var paymentMethods = await (await _paymentPluginManager
                .LoadActivePluginsAsync(customer, store.Id))
                .WhereAwait(async pm => !await pm.HidePaymentMethodAsync(cart)).ToListAsync();
            //payment methods displayed during checkout (not with "Button" type)
            var nonButtonPaymentMethods = paymentMethods
                .Where(pm => pm.PaymentMethodType != PaymentMethodType.Button)
                .ToList();
            //"button" payment methods(*displayed on the shopping cart page)
            var buttonPaymentMethods = paymentMethods
                .Where(pm => pm.PaymentMethodType == PaymentMethodType.Button)
                .ToList();
            if (!nonButtonPaymentMethods.Any() && buttonPaymentMethods.Any())
                return RedirectToRoute("ShoppingCart");

            //reset checkout data
            await _customerService.ResetCheckoutDataAsync(customer, store.Id);

            //validation (cart)
            var checkoutAttributesXml = await _genericAttributeService.GetAttributeAsync<string>(customer,
                NopCustomerDefaults.CheckoutAttributes, store.Id);
            var scWarnings = await _shoppingCartService.GetShoppingCartWarningsAsync(cart, checkoutAttributesXml, true);
            if (scWarnings.Any())
                return RedirectToRoute("ShoppingCart");
            //validation (each shopping cart item)
            foreach (var sci in cart)
            {
                var product = await _productService.GetProductByIdAsync(sci.ProductId);

                var sciWarnings = await _shoppingCartService.GetShoppingCartItemWarningsAsync(customer,
                    sci.ShoppingCartType,
                    product,
                    sci.StoreId,
                    sci.AttributesXml,
                    sci.CustomerEnteredPrice,
                    sci.RentalStartDateUtc,
                    sci.RentalEndDateUtc,
                    sci.Quantity,
                    false,
                    sci.Id);
                if (sciWarnings.Any())
                    return RedirectToRoute("ShoppingCart");
            }

            /*Alchub Start*/
            var vendorMinimumOrderAmountWarnings = await _shoppingCartService.GetVendorMinimumOrderAmountWarningsAsync(cart);
            if (vendorMinimumOrderAmountWarnings.Any())
                return RedirectToRoute("ShoppingCart");
            /*Alchub End*/

            if (_orderSettings.OnePageCheckoutEnabled)
                return RedirectToRoute("CheckoutOnePage");

            return RedirectToRoute("CheckoutBillingAddress");
        }

        #endregion Methods (common)

        #region Methods (multistep checkout)

        public virtual async Task<IActionResult> SelectBillingAddress(int addressId, bool shipToSameAddress = false)
        {
            //validation
            if (_orderSettings.CheckoutDisabled)
                return RedirectToRoute("ShoppingCart");

            var customer = await _workContext.GetCurrentCustomerAsync();
            var address = await _customerService.GetCustomerAddressAsync(customer.Id, addressId);

            if (address == null)
                return RedirectToRoute("CheckoutBillingAddress");

            customer.BillingAddressId = address.Id;
            await _customerService.UpdateCustomerAsync(customer);

            /*Alchub Start*/
            await SaveCustomerGeoCoordinatesAsync(customer);
            /*Alchub End*/

            var store = await _storeContext.GetCurrentStoreAsync();
            var cart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, store.Id);

            //ship to the same address?
            //by default Shipping is available if the country is not specified
            var shippingAllowed = !_addressSettings.CountryEnabled || ((await _countryService.GetCountryByAddressAsync(address))?.AllowsShipping ?? false);
            if (_shippingSettings.ShipToSameAddress && shipToSameAddress && await _shoppingCartService.ShoppingCartRequiresShippingAsync(cart) && shippingAllowed)
            {
                customer.ShippingAddressId = customer.BillingAddressId;
                await _customerService.UpdateCustomerAsync(customer);
                //reset selected shipping method (in case if "pick up in store" was selected)
                await _genericAttributeService.SaveAttributeAsync<ShippingOption>(customer, NopCustomerDefaults.SelectedShippingOptionAttribute, null, store.Id);
                await _genericAttributeService.SaveAttributeAsync<PickupPoint>(customer, NopCustomerDefaults.SelectedPickupPointAttribute, null, store.Id);
                //limitation - "Ship to the same address" doesn't properly work in "pick up in store only" case (when no shipping plugins are available) 

                return RedirectToRoute("CheckoutShippingMethod");
            }

            return RedirectToRoute("CheckoutShippingAddress");
        }

        [HttpPost, ActionName("BillingAddress")]
        [FormValueRequired("nextstep")]
        public virtual async Task<IActionResult> NewBillingAddress(CheckoutBillingAddressModel model, IFormCollection form)
        {
            //validation
            if (_orderSettings.CheckoutDisabled)
                return RedirectToRoute("ShoppingCart");

            var customer = await _workContext.GetCurrentCustomerAsync();
            var store = await _storeContext.GetCurrentStoreAsync();
            var cart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, store.Id);

            if (!cart.Any())
                return RedirectToRoute("ShoppingCart");

            if (_orderSettings.OnePageCheckoutEnabled)
                return RedirectToRoute("CheckoutOnePage");

            if (await _customerService.IsGuestAsync(customer) && !_orderSettings.AnonymousCheckoutAllowed)
                return Challenge();

            //custom address attributes
            var customAttributes = await _addressAttributeParser.ParseCustomAddressAttributesAsync(form);
            var customAttributeWarnings = await _addressAttributeParser.GetAttributeWarningsAsync(customAttributes);
            foreach (var error in customAttributeWarnings)
            {
                ModelState.AddModelError("", error);
            }

            var newAddress = model.BillingNewAddress;

            if (ModelState.IsValid)
            {
                //try to find an address with the same values (don't duplicate records)
                var address = _addressService.FindAddress((await _customerService.GetAddressesByCustomerIdAsync(customer.Id)).ToList(),
                    newAddress.FirstName, newAddress.LastName, newAddress.PhoneNumber,
                    newAddress.Email, newAddress.FaxNumber, newAddress.Company,
                    newAddress.Address1, newAddress.Address2, newAddress.City,
                    newAddress.County, newAddress.StateProvinceId, newAddress.ZipPostalCode,
                    newAddress.CountryId, customAttributes);

                if (address == null)
                {
                    //address is not found. let's create a new one
                    address = newAddress.ToEntity();
                    address.CustomAttributes = customAttributes;
                    address.CreatedOnUtc = DateTime.UtcNow;

                    //some validation
                    if (address.CountryId == 0)
                        address.CountryId = null;
                    if (address.StateProvinceId == 0)
                        address.StateProvinceId = null;

                    await _addressService.InsertAddressAsync(address);

                    await _customerService.InsertCustomerAddressAsync(customer, address);
                }

                customer.BillingAddressId = address.Id;
                await _customerService.UpdateCustomerAsync(customer);

                /*Alchub Start*/
                await SaveCustomerGeoCoordinatesAsync(customer);
                /*Alchub End*/

                //ship to the same address?
                if (_shippingSettings.ShipToSameAddress && model.ShipToSameAddress && await _shoppingCartService.ShoppingCartRequiresShippingAsync(cart))
                {
                    customer.ShippingAddressId = customer.BillingAddressId;
                    await _customerService.UpdateCustomerAsync(customer);

                    //reset selected shipping method (in case if "pick up in store" was selected)
                    await _genericAttributeService.SaveAttributeAsync<ShippingOption>(customer, NopCustomerDefaults.SelectedShippingOptionAttribute, null, store.Id);
                    await _genericAttributeService.SaveAttributeAsync<PickupPoint>(customer, NopCustomerDefaults.SelectedPickupPointAttribute, null, store.Id);

                    //limitation - "Ship to the same address" doesn't properly work in "pick up in store only" case (when no shipping plugins are available) 

                    return RedirectToRoute("CheckoutShippingMethod");
                }

                return RedirectToRoute("CheckoutShippingAddress");
            }

            //If we got this far, something failed, redisplay form
            model = await _checkoutModelFactory.PrepareBillingAddressModelAsync(cart,
                selectedCountryId: newAddress.CountryId,
                overrideAttributesXml: customAttributes);
            return View(model);
        }

        public virtual async Task<IActionResult> SelectShippingAddress(int addressId)
        {
            //validation
            if (_orderSettings.CheckoutDisabled)
                return RedirectToRoute("ShoppingCart");

            var customer = await _workContext.GetCurrentCustomerAsync();
            var address = await _customerService.GetCustomerAddressAsync(customer.Id, addressId);

            if (address == null)
                return RedirectToRoute("CheckoutShippingAddress");

            customer.ShippingAddressId = address.Id;
            await _customerService.UpdateCustomerAsync(customer);

            if (_shippingSettings.AllowPickupInStore)
            {
                var store = await _storeContext.GetCurrentStoreAsync();
                //set value indicating that "pick up in store" option has not been chosen
                await _genericAttributeService.SaveAttributeAsync<PickupPoint>(customer, NopCustomerDefaults.SelectedPickupPointAttribute, null, store.Id);
            }

            /*Alchub Start*/
            await SaveCustomerGeoCoordinatesAsync(customer);
            /*Alchub End*/

            return RedirectToRoute("CheckoutShippingMethod");
        }

        [HttpPost, ActionName("ShippingAddress")]
        [FormValueRequired("nextstep")]
        public virtual async Task<IActionResult> NewShippingAddress(CheckoutShippingAddressModel model, IFormCollection form)
        {
            //validation
            if (_orderSettings.CheckoutDisabled)
                return RedirectToRoute("ShoppingCart");

            var customer = await _workContext.GetCurrentCustomerAsync();
            var store = await _storeContext.GetCurrentStoreAsync();
            var cart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, store.Id);

            if (!cart.Any())
                return RedirectToRoute("ShoppingCart");

            if (_orderSettings.OnePageCheckoutEnabled)
                return RedirectToRoute("CheckoutOnePage");

            if (await _customerService.IsGuestAsync(customer) && !_orderSettings.AnonymousCheckoutAllowed)
                return Challenge();

            if (!await _shoppingCartService.ShoppingCartRequiresShippingAsync(cart))
                return RedirectToRoute("CheckoutShippingMethod");

            //pickup point
            if (_shippingSettings.AllowPickupInStore && !_orderSettings.DisplayPickupInStoreOnShippingMethodPage)
            {
                var pickupInStore = ParsePickupInStore(form);
                if (pickupInStore)
                {
                    var pickupOption = await ParsePickupOptionAsync(form);
                    await SavePickupOptionAsync(pickupOption);

                    return RedirectToRoute("CheckoutPaymentMethod");
                }

                //set value indicating that "pick up in store" option has not been chosen
                await _genericAttributeService.SaveAttributeAsync<PickupPoint>(customer, NopCustomerDefaults.SelectedPickupPointAttribute, null, store.Id);
            }

            //custom address attributes
            var customAttributes = await _addressAttributeParser.ParseCustomAddressAttributesAsync(form);
            var customAttributeWarnings = await _addressAttributeParser.GetAttributeWarningsAsync(customAttributes);
            foreach (var error in customAttributeWarnings)
            {
                ModelState.AddModelError("", error);
            }

            var newAddress = model.ShippingNewAddress;

            if (ModelState.IsValid)
            {
                //try to find an address with the same values (don't duplicate records)
                var address = _addressService.FindAddress((await _customerService.GetAddressesByCustomerIdAsync(customer.Id)).ToList(),
                    newAddress.FirstName, newAddress.LastName, newAddress.PhoneNumber,
                    newAddress.Email, newAddress.FaxNumber, newAddress.Company,
                    newAddress.Address1, newAddress.Address2, newAddress.City,
                    newAddress.County, newAddress.StateProvinceId, newAddress.ZipPostalCode,
                    newAddress.CountryId, customAttributes);

                if (address == null)
                {
                    address = newAddress.ToEntity();
                    address.CustomAttributes = customAttributes;
                    address.CreatedOnUtc = DateTime.UtcNow;
                    //some validation
                    if (address.CountryId == 0)
                        address.CountryId = null;
                    if (address.StateProvinceId == 0)
                        address.StateProvinceId = null;

                    await _addressService.InsertAddressAsync(address);

                    await _customerService.InsertCustomerAddressAsync(customer, address);

                }

                customer.ShippingAddressId = address.Id;
                await _customerService.UpdateCustomerAsync(customer);

                /*Alchub Start*/
                await SaveCustomerGeoCoordinatesAsync(customer);
                /*Alchub End*/

                return RedirectToRoute("CheckoutShippingMethod");
            }

            //If we got this far, something failed, redisplay form
            model = await _checkoutModelFactory.PrepareShippingAddressModelAsync(cart,
                selectedCountryId: newAddress.CountryId,
                overrideAttributesXml: customAttributes);
            return View(model);
        }

        public virtual async Task<IActionResult> ShippingMethod()
        {
            //validation
            if (_orderSettings.CheckoutDisabled)
                return RedirectToRoute("ShoppingCart");

            var customer = await _workContext.GetCurrentCustomerAsync();
            var store = await _storeContext.GetCurrentStoreAsync();
            var cart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, store.Id);

            if (!cart.Any())
                return RedirectToRoute("ShoppingCart");

            if (_orderSettings.OnePageCheckoutEnabled)
                return RedirectToRoute("CheckoutOnePage");

            //++Alchub

            //check shopping cart have error? then redirect to address selection page.
            if (await ShoppingCartHasAnyError(cart, customer, store))
            {
                _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Alchub.Checkout.ShoppingCartHasError"));
                return RedirectToRoute("CheckoutBillingAddress");
            }

            //--Alchub

            if (await _customerService.IsGuestAsync(customer) && !_orderSettings.AnonymousCheckoutAllowed)
                return Challenge();

            if (!await _shoppingCartService.ShoppingCartRequiresShippingAsync(cart))
            {
                await _genericAttributeService.SaveAttributeAsync<ShippingOption>(customer, NopCustomerDefaults.SelectedShippingOptionAttribute, null, store.Id);
                return RedirectToRoute("CheckoutPaymentMethod");
            }

            //check if pickup point is selected on the shipping address step
            if (!_orderSettings.DisplayPickupInStoreOnShippingMethodPage)
            {
                var selectedPickUpPoint = await _genericAttributeService
                    .GetAttributeAsync<PickupPoint>(customer, NopCustomerDefaults.SelectedPickupPointAttribute, store.Id);
                if (selectedPickUpPoint != null)
                    return RedirectToRoute("CheckoutPaymentMethod");
            }

            //model
            var model = await _checkoutModelFactory.PrepareShippingMethodModelAsync(cart, await _customerService.GetCustomerShippingAddressAsync(customer));

            if (_shippingSettings.BypassShippingMethodSelectionIfOnlyOne &&
                model.ShippingMethods.Count == 1)
            {
                //if we have only one shipping method, then a customer doesn't have to choose a shipping method
                await _genericAttributeService.SaveAttributeAsync(customer,
                    NopCustomerDefaults.SelectedShippingOptionAttribute,
                    model.ShippingMethods.First().ShippingOption,
                    store.Id);

                return RedirectToRoute("CheckoutPaymentMethod");
            }
            else
            {
                //++Alchuv

                //still by pass shipping step by selecting ground shipping by default.
                var shippingMethod = model?.ShippingMethods?.FirstOrDefault(x => x.ShippingRateComputationMethodSystemName.Equals("Shipping.FixedByWeightByTotal"));
                if (shippingMethod != null)
                {
                    await _genericAttributeService.SaveAttributeAsync(customer,
                    NopCustomerDefaults.SelectedShippingOptionAttribute,
                    shippingMethod.ShippingOption,
                    store.Id);

                    return RedirectToRoute("CheckoutPaymentMethod");
                }

                //--Alchub
            }

            return View(model);
        }

        public virtual async Task<IActionResult> PaymentMethod()
        {
            //validation
            if (_orderSettings.CheckoutDisabled)
                return RedirectToRoute("ShoppingCart");

            var customer = await _workContext.GetCurrentCustomerAsync();
            var store = await _storeContext.GetCurrentStoreAsync();
            var cart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, store.Id);

            if (!cart.Any())
                return RedirectToRoute("ShoppingCart");

            if (_orderSettings.OnePageCheckoutEnabled)
                return RedirectToRoute("CheckoutOnePage");

            if (await _customerService.IsGuestAsync(customer) && !_orderSettings.AnonymousCheckoutAllowed)
                return Challenge();

            //++Alchub

            //check shopping cart have error? then redirect to address selection page.
            if (await ShoppingCartHasAnyError(cart, customer, store))
            {
                _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Alchub.Checkout.ShoppingCartHasError"));
                return RedirectToRoute("CheckoutBillingAddress");
            }

            //--Alchub

            //Check whether payment workflow is required
            //we ignore reward points during cart total calculation
            var isPaymentWorkflowRequired = await _orderProcessingService.IsPaymentWorkflowRequiredAsync(cart, false);
            if (!isPaymentWorkflowRequired)
            {
                await _genericAttributeService.SaveAttributeAsync<string>(customer,
                    NopCustomerDefaults.SelectedPaymentMethodAttribute, null, store.Id);
                return RedirectToRoute("CheckoutPaymentInfo");
            }

            //filter by country
            var filterByCountryId = 0;
            if (_addressSettings.CountryEnabled)
            {
                filterByCountryId = (await _customerService.GetCustomerBillingAddressAsync(customer))?.CountryId ?? 0;
            }

            //model
            var paymentMethodModel = await _checkoutModelFactory.PreparePaymentMethodModelAsync(cart, filterByCountryId);

            //if (_paymentSettings.BypassPaymentMethodSelectionIfOnlyOne &&
            //    paymentMethodModel.PaymentMethods.Count == 1 && !paymentMethodModel.DisplayRewardPoints)
            //{
            //    //if we have only one payment method and reward points are disabled or the current customer doesn't have any reward points
            //    //so customer doesn't have to choose a payment method

            //    await _genericAttributeService.SaveAttributeAsync(customer,
            //        NopCustomerDefaults.SelectedPaymentMethodAttribute,
            //        paymentMethodModel.PaymentMethods[0].PaymentMethodSystemName,
            //        store.Id);
            //    return RedirectToRoute("CheckoutPaymentInfo");
            //}

            //return View(paymentMethodModel);

            //++Alchuv

            //Bypass payment step by selecting stripe payment by default.
            var stripePaymentMethod = paymentMethodModel?.PaymentMethods?.FirstOrDefault(x => x.PaymentMethodSystemName.Equals("Payments.StripeConnectRedirect"));
            if (stripePaymentMethod == null)
            {
                //throw new NopException("Stripe payment method is not avilable! please contact support team.");
                await _logger.ErrorAsync("Checkout error: Stripe payment method is not avilable! please contact support team.", null, await _workContext.GetCurrentCustomerAsync());
                _notificationService.ErrorNotification("Stripe payment method is not avilable! please contact support team.");

                return RedirectToRoute("CheckoutBillingAddress");
            }

            //save payment method in generic
            await _genericAttributeService.SaveAttributeAsync(customer,
                                                              NopCustomerDefaults.SelectedPaymentMethodAttribute,
                                                              stripePaymentMethod.PaymentMethodSystemName,
                                                              store.Id);

            return RedirectToRoute("CheckoutPaymentInfo");

            //--Alchub
        }

        public virtual async Task<IActionResult> PaymentInfo()
        {
            //validation
            if (_orderSettings.CheckoutDisabled)
                return RedirectToRoute("ShoppingCart");

            var customer = await _workContext.GetCurrentCustomerAsync();
            var store = await _storeContext.GetCurrentStoreAsync();
            var cart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, store.Id);

            if (!cart.Any())
                return RedirectToRoute("ShoppingCart");

            if (_orderSettings.OnePageCheckoutEnabled)
                return RedirectToRoute("CheckoutOnePage");

            if (await _customerService.IsGuestAsync(customer) && !_orderSettings.AnonymousCheckoutAllowed)
                return Challenge();

            //Check whether payment workflow is required
            var isPaymentWorkflowRequired = await _orderProcessingService.IsPaymentWorkflowRequiredAsync(cart);
            if (!isPaymentWorkflowRequired)
            {
                return RedirectToRoute("CheckoutConfirm");
            }

            ////load payment method
            //var paymentMethodSystemName = await _genericAttributeService.GetAttributeAsync<string>(customer,
            //    NopCustomerDefaults.SelectedPaymentMethodAttribute, store.Id);
            //var paymentMethod = await _paymentPluginManager
            //    .LoadPluginBySystemNameAsync(paymentMethodSystemName, customer, store.Id);
            //if (paymentMethod == null)
            //    return RedirectToRoute("CheckoutPaymentMethod");

            ////Check whether payment info should be skipped
            //if (paymentMethod.SkipPaymentInfo ||
            //    (paymentMethod.PaymentMethodType == PaymentMethodType.Redirection && _paymentSettings.SkipPaymentInfoStepForRedirectionPaymentMethods))
            //{
            //    //skip payment info page
            //    var paymentInfo = new ProcessPaymentRequest();

            //    //session save
            //    HttpContext.Session.Set("OrderPaymentInfo", paymentInfo);

            //    return RedirectToRoute("CheckoutConfirm");
            //}

            ////model
            //var model = await _checkoutModelFactory.PreparePaymentInfoModelAsync(paymentMethod);
            //return View(model);

            //++Alchub

            //By default skip payment info page
            var paymentInfo = new ProcessPaymentRequest();

            //session save
            HttpContext.Session.Set("OrderPaymentInfo", paymentInfo);

            return RedirectToRoute("CheckoutConfirm");

            //--Alchub
        }

        public virtual async Task<IActionResult> Confirm()
        {
            //validation
            if (_orderSettings.CheckoutDisabled)
                return RedirectToRoute("ShoppingCart");

            var customer = await _workContext.GetCurrentCustomerAsync();
            var store = await _storeContext.GetCurrentStoreAsync();
            var cart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, store.Id);

            if (!cart.Any())
                return RedirectToRoute("ShoppingCart");

            if (_orderSettings.OnePageCheckoutEnabled)
                return RedirectToRoute("CheckoutOnePage");

            if (await _customerService.IsGuestAsync(customer) && !_orderSettings.AnonymousCheckoutAllowed)
                return Challenge();

            //++Alchub

            //check shopping cart have error? then redirect to address selection page.
            if (await ShoppingCartHasAnyError(cart, customer, store))
            {
                _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Alchub.Checkout.ShoppingCartHasError"));
                return RedirectToRoute("CheckoutBillingAddress");
            }

            //--Alchub

            //model
            var model = await _checkoutModelFactory.PrepareConfirmOrderModelAsync(cart);
            return View(model);
        }

        #endregion

        #region Methods (one page checkout)

        protected virtual async Task<JsonResult> OpcLoadStepAfterShippingAddress(IList<ShoppingCartItem> cart)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            var shippingMethodModel = await _checkoutModelFactory.PrepareShippingMethodModelAsync(cart, await _customerService.GetCustomerShippingAddressAsync(customer));
            if (_shippingSettings.BypassShippingMethodSelectionIfOnlyOne &&
                shippingMethodModel.ShippingMethods.Count == 1)
            {
                var store = await _storeContext.GetCurrentStoreAsync();
                //if we have only one shipping method, then a customer doesn't have to choose a shipping method
                await _genericAttributeService.SaveAttributeAsync(customer,
                    NopCustomerDefaults.SelectedShippingOptionAttribute,
                    shippingMethodModel.ShippingMethods.First().ShippingOption,
                    store.Id);

                //load next step
                return await OpcLoadStepAfterShippingMethod(cart);
            }

            /*Alchub Start*/
            var lastSearchedText = await SaveCustomerGeoCoordinatesAsync(customer);
            /*Alchub End*/

            return Json(new
            {
                update_section = new UpdateSectionJsonModel
                {
                    name = "shipping-method",
                    html = await RenderPartialViewToStringAsync("OpcShippingMethods", shippingMethodModel)
                },
                goto_section = "shipping_method"
                /*Alchub Start*/
                ,
                lastSearchedText = lastSearchedText
                /*Alchub End*/
            });
        }

        protected virtual async Task<JsonResult> OpcLoadStepAfterShippingMethod(IList<ShoppingCartItem> cart)
        {
            //Check whether payment workflow is required
            //we ignore reward points during cart total calculation
            var customer = await _workContext.GetCurrentCustomerAsync();
            var store = await _storeContext.GetCurrentStoreAsync();

            /*Alchub Start*/
            var lastSearchedText = await SaveCustomerGeoCoordinatesAsync(customer);
            /*Alchub End*/

            var isPaymentWorkflowRequired = await _orderProcessingService.IsPaymentWorkflowRequiredAsync(cart, false);
            if (isPaymentWorkflowRequired)
            {
                //filter by country
                var filterByCountryId = 0;
                if (_addressSettings.CountryEnabled)
                {
                    filterByCountryId = (await _customerService.GetCustomerBillingAddressAsync(customer))?.CountryId ?? 0;
                }

                //payment is required
                var paymentMethodModel = await _checkoutModelFactory.PreparePaymentMethodModelAsync(cart, filterByCountryId);

                if (_paymentSettings.BypassPaymentMethodSelectionIfOnlyOne &&
                    paymentMethodModel.PaymentMethods.Count == 1 && !paymentMethodModel.DisplayRewardPoints)
                {
                    //if we have only one payment method and reward points are disabled or the current customer doesn't have any reward points
                    //so customer doesn't have to choose a payment method

                    var selectedPaymentMethodSystemName = paymentMethodModel.PaymentMethods[0].PaymentMethodSystemName;
                    await _genericAttributeService.SaveAttributeAsync(customer,
                        NopCustomerDefaults.SelectedPaymentMethodAttribute,
                        selectedPaymentMethodSystemName, store.Id);

                    var paymentMethodInst = await _paymentPluginManager
                        .LoadPluginBySystemNameAsync(selectedPaymentMethodSystemName, customer, store.Id);
                    if (!_paymentPluginManager.IsPluginActive(paymentMethodInst))
                        throw new Exception("Selected payment method can't be parsed");

                    return await OpcLoadStepAfterPaymentMethod(paymentMethodInst, cart);
                }

                //customer have to choose a payment method
                return Json(new
                {
                    update_section = new UpdateSectionJsonModel
                    {
                        name = "payment-method",
                        html = await RenderPartialViewToStringAsync("OpcPaymentMethods", paymentMethodModel)
                    },
                    goto_section = "payment_method"
                    /*Alchub Start*/
                    ,
                    lastSearchedText = lastSearchedText
                    /*Alchub End*/
                });
            }

            //payment is not required
            await _genericAttributeService.SaveAttributeAsync<string>(customer,
                NopCustomerDefaults.SelectedPaymentMethodAttribute, null, store.Id);

            var confirmOrderModel = await _checkoutModelFactory.PrepareConfirmOrderModelAsync(cart);
            return Json(new
            {
                update_section = new UpdateSectionJsonModel
                {
                    name = "confirm-order",
                    html = await RenderPartialViewToStringAsync("OpcConfirmOrder", confirmOrderModel)
                },
                goto_section = "confirm_order"
                /*Alchub Start*/
                ,
                lastSearchedText = lastSearchedText
                /*Alchub End*/
            });
        }

        protected virtual async Task<JsonResult> OpcLoadStepAfterPaymentMethod(IPaymentMethod paymentMethod, IList<ShoppingCartItem> cart)
        {
            /*Alchub Start*/
            var lastSearchedText = await SaveCustomerGeoCoordinatesAsync(await _workContext.GetCurrentCustomerAsync());
            /*Alchub End*/

            if (paymentMethod.SkipPaymentInfo ||
                (paymentMethod.PaymentMethodType == PaymentMethodType.Redirection && _paymentSettings.SkipPaymentInfoStepForRedirectionPaymentMethods))
            {
                //skip payment info page
                var paymentInfo = new ProcessPaymentRequest();

                //session save
                HttpContext.Session.Set("OrderPaymentInfo", paymentInfo);

                var confirmOrderModel = await _checkoutModelFactory.PrepareConfirmOrderModelAsync(cart);
                return Json(new
                {
                    update_section = new UpdateSectionJsonModel
                    {
                        name = "confirm-order",
                        html = await RenderPartialViewToStringAsync("OpcConfirmOrder", confirmOrderModel)
                    },
                    goto_section = "confirm_order"
                    /*Alchub Start*/
                    ,
                    lastSearchedText = lastSearchedText
                    /*Alchub End*/
                });
            }

            //return payment info page
            var paymenInfoModel = await _checkoutModelFactory.PreparePaymentInfoModelAsync(paymentMethod);
            return Json(new
            {
                update_section = new UpdateSectionJsonModel
                {
                    name = "payment-info",
                    html = await RenderPartialViewToStringAsync("OpcPaymentInfo", paymenInfoModel)
                },
                goto_section = "payment_info"
                /*Alchub Start*/
                ,
                lastSearchedText = lastSearchedText
                /*Alchub End*/
            });
        }

        #endregion
    }
}