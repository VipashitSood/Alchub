using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Stores;
using Nop.Core.Infrastructure;
using Nop.Plugin.Api.DTO.Checkout;
using Nop.Plugin.Api.DTO.Errors;
using Nop.Plugin.Api.DTO.Orders;
using Nop.Plugin.Api.Helpers;
using Nop.Plugin.Api.JSON.Serializers;
using Nop.Plugin.Api.Models.BaseModels;
using Nop.Plugin.Api.Services;
using Nop.Plugin.Payments.StripeConnectRedirect.Services;
using Nop.Services.Authentication;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Discounts;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Security;
using Nop.Services.Shipping;
using Nop.Services.Stores;

namespace Nop.Plugin.Api.Controllers
{
    public class CheckoutController : BaseApiController
    {
        #region Fields

        private readonly IDTOHelper _dtoHelper;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IPaymentService _paymentService;
        private readonly IPermissionService _permissionService;
        private readonly IAuthenticationService _authenticationService;
        private readonly AddressSettings _addressSettings;
        private readonly ShippingSettings _shippingSettings;
        private readonly ICountryService _countryService;
        private readonly IPaymentPluginManager _paymentPluginManager;
        private readonly IOrderApiService _orderApiService;
        private readonly IStripeConnectRedirectService _stripeConnectRedirectService;
        private readonly IShippingService _shippingService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IStoreContext _storeContext;

        // We resolve the order settings this way because of the tests.
        // The auto mocking does not support concreate types as dependencies. It supports only interfaces.
        private OrderSettings _orderSettings;

        #endregion

        #region Ctor

        public CheckoutController(
            IJsonFieldsSerializer jsonFieldsSerializer,
            IAclService aclService,
            ICustomerService customerService,
            IStoreMappingService storeMappingService,
            IStoreService storeService,
            IDiscountService discountService,
            ICustomerActivityService customerActivityService,
            ILocalizationService localizationService,
            IOrderProcessingService orderProcessingService,
            IShoppingCartService shoppingCartService,
            IGenericAttributeService genericAttributeService,
            IStoreContext storeContext,
            IShippingService shippingService,
            IPictureService pictureService,
            IDTOHelper dtoHelper,
            IPaymentService paymentService,
            IPermissionService permissionService,
            IAuthenticationService authenticationService,
            AddressSettings addressSettings,
            ShippingSettings shippingSettings,
            ICountryService countryService,
            IPaymentPluginManager paymentPluginManager,
            IOrderApiService orderApiService,
            IStripeConnectRedirectService stripeConnectRedirectService)
            : base(jsonFieldsSerializer, aclService, customerService, storeMappingService,
                   storeService, discountService, customerActivityService, localizationService, pictureService)
        {
            _orderProcessingService = orderProcessingService;
            _shoppingCartService = shoppingCartService;
            _genericAttributeService = genericAttributeService;
            _storeContext = storeContext;
            _shippingService = shippingService;
            _dtoHelper = dtoHelper;
            _paymentService = paymentService;
            _permissionService = permissionService;
            _authenticationService = authenticationService;
            _addressSettings = addressSettings;
            _shippingSettings = shippingSettings;
            _countryService = countryService;
            _paymentPluginManager = paymentPluginManager;
            _orderApiService = orderApiService;
            _stripeConnectRedirectService = stripeConnectRedirectService;
        }

        private OrderSettings OrderSettings => _orderSettings ?? (_orderSettings = EngineContext.Current.Resolve<OrderSettings>());

        #endregion

        [HttpPost]
        [Route("/api/checkout/set_billing_address", Name = "CheckoutSetBillingAddress")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ErrorsRootObject), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ErrorsRootObject), 422)]
        public async Task<BaseResponseModel> CheckoutSetBillingAddress([FromBody] CheckoutBillingDto CheckoutBillingRequest)
        {
            try
            {
                //validation
                if (OrderSettings.CheckoutDisabled)
                    throw new Exception(await _localizationService.GetResourceAsync("Checkout.Disabled"));

                if (CheckoutBillingRequest.BillingAddressId <= 0)
                    return ErrorResponse("Invalide billing address id", HttpStatusCode.BadRequest);

                if (CheckoutBillingRequest.CustomerId <= 0)
                    return ErrorResponse("Invalide customer id", HttpStatusCode.BadRequest);

                var customer = await _customerService.GetCustomerByIdAsync(CheckoutBillingRequest.CustomerId);
                if (customer == null)
                    return ErrorResponse("customer not found!", HttpStatusCode.NotFound);

                if (await _customerService.IsGuestAsync(customer) && !_orderSettings.AnonymousCheckoutAllowed)
                    return ErrorResponse("Anonymous checkout is not allowed", HttpStatusCode.BadRequest);

                var store = await _storeContext.GetCurrentStoreAsync();
                //shopping cart
                var cart = await _shoppingCartService.GetShoppingCartAsync(customer, Nop.Core.Domain.Orders.ShoppingCartType.ShoppingCart, store.Id);
                if (!cart.Any())
                    return ErrorResponse("Your shopping cart is empty", HttpStatusCode.NotFound);

                //existing address
                var address = await _customerService.GetCustomerAddressAsync(customer.Id, CheckoutBillingRequest.BillingAddressId)
                    ?? throw new Exception(await _localizationService.GetResourceAsync("Checkout.Address.NotFound"));

                //save 
                customer.BillingAddressId = address.Id;
                await _customerService.UpdateCustomerAsync(customer);

                if (await _shoppingCartService.ShoppingCartRequiresShippingAsync(cart))
                {
                    //shipping is required
                    //var address = await _customerService.GetCustomerBillingAddressAsync(customer);

                    //by default Shipping is available if the country is not specified
                    //var shippingAllowed = !_addressSettings.CountryEnabled || ((await _countryService.GetCountryByAddressAsync(address))?.AllowsShipping ?? false);

                    //++Alchub
                    //Defautl ship to same address
                    CheckoutBillingRequest.ShipToSameAddress = true;
                    if (_shippingSettings.ShipToSameAddress && CheckoutBillingRequest.ShipToSameAddress)
                    {
                        //ship to the same address
                        customer.ShippingAddressId = address.Id;
                        await _customerService.UpdateCustomerAsync(customer);
                        //reset selected shipping method (in case if "pick up in store" was selected)
                        await _genericAttributeService.SaveAttributeAsync<ShippingOption>(customer, NopCustomerDefaults.SelectedShippingOptionAttribute, null, store.Id);
                        await _genericAttributeService.SaveAttributeAsync<PickupPoint>(customer, NopCustomerDefaults.SelectedPickupPointAttribute, null, store.Id);

                        //update customer coordinates.
                        await SaveCustomerGeoCoordinatesAsync(customer, address);

                        //bypass shipping step:
                        //get available shipping methods
                        var shippingMethods = await _dtoHelper.PrepareCheckoutShippingMethodDtoAsync(cart, address, customer, store);
                        if (_shippingSettings.BypassShippingMethodSelectionIfOnlyOne &&
                            shippingMethods?.ShippingMethods?.Count == 1)
                        {
                            //if we have only one shipping method, then a customer doesn't have to choose a shipping method
                            var shippingMethod = shippingMethods.ShippingMethods.First();

                            //set shipping option
                            await SetShippingOptionAsync(customer, store, cart, shippingMethod.Name, shippingMethod.ShippingRateComputationMethodSystemName);
                        }
                        else
                        {
                            //still by pass shipping step by selecting ground shipping by default.
                            var shippingMethod = shippingMethods?.ShippingMethods?.FirstOrDefault(x => x.ShippingRateComputationMethodSystemName.Equals("Shipping.FixedByWeightByTotal"));
                            if (shippingMethod != null)
                                //set shipping option
                                await SetShippingOptionAsync(customer, store, cart, shippingMethod.Name, shippingMethod.ShippingRateComputationMethodSystemName);
                        }
                        //--Alchub

                        //success
                        return SuccessResponse(await _localizationService.GetResourceAsync("Nop.Api.Checkout.BillingAddress.Saved") + " & Shipping is set to same as Billing!",
                                               await _localizationService.GetResourceAsync("Nop.Api.Checkout.BillingAddress.Saved") + " & Shipping is set to same as Billing!");
                    }

                    //do not ship to the same address (SaveShipping API needs to be called!)
                }

                //shipping is not required
                await _genericAttributeService.SaveAttributeAsync<ShippingOption>(customer, NopCustomerDefaults.SelectedShippingOptionAttribute, null, store.Id);

                //success
                return SuccessResponse(await _localizationService.GetResourceAsync("Nop.Api.Checkout.BillingAddress.Saved"),
                                       await _localizationService.GetResourceAsync("Nop.Api.Checkout.BillingAddress.Saved"));
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex.Message, HttpStatusCode.BadRequest);
            }
        }

        [HttpPost]
        [Route("/api/checkout/set_shipping_address", Name = "CheckoutSetShippingAddress")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ErrorsRootObject), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ErrorsRootObject), 422)]
        public async Task<BaseResponseModel> CheckoutSetShippingAddress([FromBody] CheckoutShippingDto CheckoutShippingRequest)
        {
            try
            {
                //validation
                if (OrderSettings.CheckoutDisabled)
                    throw new Exception(await _localizationService.GetResourceAsync("Checkout.Disabled"));

                if (CheckoutShippingRequest.ShippingAddressId <= 0)
                    return ErrorResponse("Invalide billing address id", HttpStatusCode.BadRequest);

                if (CheckoutShippingRequest.CustomerId <= 0)
                    return ErrorResponse("Invalide customer id", HttpStatusCode.BadRequest);

                var customer = await _customerService.GetCustomerByIdAsync(CheckoutShippingRequest.CustomerId);
                if (customer == null)
                    return ErrorResponse("customer not found!", HttpStatusCode.NotFound);

                if (await _customerService.IsGuestAsync(customer) && !_orderSettings.AnonymousCheckoutAllowed)
                    return ErrorResponse("Anonymous checkout is not allowed", HttpStatusCode.BadRequest);

                var store = await _storeContext.GetCurrentStoreAsync();
                //shopping cart
                var cart = await _shoppingCartService.GetShoppingCartAsync(customer, Nop.Core.Domain.Orders.ShoppingCartType.ShoppingCart, store.Id);
                if (!cart.Any())
                    return ErrorResponse("Your shopping cart is empty", HttpStatusCode.NotFound);

                if (!await _shoppingCartService.ShoppingCartRequiresShippingAsync(cart))
                    throw new Exception("Shipping is not required");

                //existing address
                var address = await _customerService.GetCustomerAddressAsync(customer.Id, CheckoutShippingRequest.ShippingAddressId)
                    ?? throw new Exception(await _localizationService.GetResourceAsync("Checkout.Address.NotFound"));

                customer.ShippingAddressId = address.Id;
                await _customerService.UpdateCustomerAsync(customer);

                //success
                return SuccessResponse(await _localizationService.GetResourceAsync("Nop.Api.Checkout.ShippingAddress.Saved"),
                                       await _localizationService.GetResourceAsync("Nop.Api.Checkout.ShippingAddress.Saved"));
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex.Message, HttpStatusCode.BadRequest);
            }
        }

        [HttpGet]
        [Route("/api/checkout/shipping_methods", Name = "CheckoutGetShippingMethods")]
        [ProducesResponseType(typeof(CheckoutShippingMethodDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ErrorsRootObject), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ErrorsRootObject), 422)]
        public async Task<BaseResponseModel> CheckoutGetShippingMethods([FromQuery] CheckoutShippingMethodRequest checkoutShippingMethodRequest)
        {
            try
            {
                //validation
                if (OrderSettings.CheckoutDisabled)
                    return ErrorResponse("Checkout was disabled!", HttpStatusCode.Forbidden);

                if (checkoutShippingMethodRequest.CustomerId <= 0)
                    return ErrorResponse("Invalide customer id", HttpStatusCode.BadRequest);

                var customer = await _customerService.GetCustomerByIdAsync(checkoutShippingMethodRequest.CustomerId);
                if (customer == null)
                    return ErrorResponse("customer not found!", HttpStatusCode.NotFound);

                var store = await _storeContext.GetCurrentStoreAsync();
                var cart = await _shoppingCartService.GetShoppingCartAsync(customer, Nop.Core.Domain.Orders.ShoppingCartType.ShoppingCart, store.Id);
                var shippingAddress = await _customerService.GetCustomerAddressAsync(customer.Id, checkoutShippingMethodRequest?.ShippingAddressId ?? 0);

                //get available shipping methods
                var shippingMethods = await _dtoHelper.PrepareCheckoutShippingMethodDtoAsync(cart, shippingAddress, customer, store);

                //success
                return SuccessResponse(await _localizationService.GetResourceAsync("Nop.Api.Checkout.ShippingMethods"), shippingMethods);
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex.Message, HttpStatusCode.BadRequest);
            }
        }

        [HttpPost]
        [Route("/api/checkout/set_shipping_method", Name = "CheckoutSetShippingMethod")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ErrorsRootObject), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ErrorsRootObject), 422)]
        public async Task<BaseResponseModel> CheckoutSetShippingMethod([FromBody] CheckoutSaveShippingMethodRequest checkoutSaveShippingMethodRequest)
        {
            try
            {
                //validation
                if (OrderSettings.CheckoutDisabled)
                    throw new Exception(await _localizationService.GetResourceAsync("Checkout.Disabled"));

                if (checkoutSaveShippingMethodRequest.CustomerId <= 0)
                    return ErrorResponse("Invalide customer id", HttpStatusCode.BadRequest);

                if (string.IsNullOrEmpty(checkoutSaveShippingMethodRequest.ShippingMethodName))
                    return ErrorResponse("selected shipping method name cannot be empty", HttpStatusCode.BadRequest);

                if (string.IsNullOrEmpty(checkoutSaveShippingMethodRequest.ShippingRateComputationMethodSystemName))
                    return ErrorResponse("selected shipping_rate_computation_method_system_name cannot be empty", HttpStatusCode.BadRequest);

                var customer = await _customerService.GetCustomerByIdAsync(checkoutSaveShippingMethodRequest.CustomerId);
                if (customer == null)
                    return ErrorResponse("customer not found!", HttpStatusCode.NotFound);

                var store = await _storeContext.GetCurrentStoreAsync();
                var cart = await _shoppingCartService.GetShoppingCartAsync(customer, Nop.Core.Domain.Orders.ShoppingCartType.ShoppingCart, store.Id);
                if (!cart.Any())
                    throw new Exception("Your cart is empty");

                if (await _customerService.IsGuestAsync(customer) && !_orderSettings.AnonymousCheckoutAllowed)
                    throw new Exception("Anonymous checkout is not allowed");

                if (!await _shoppingCartService.ShoppingCartRequiresShippingAsync(cart))
                    throw new Exception("Shipping is not required");

                //set shipping option
                await SetShippingOptionAsync(customer, store, cart, checkoutSaveShippingMethodRequest.ShippingMethodName, checkoutSaveShippingMethodRequest.ShippingRateComputationMethodSystemName);

                //success
                return SuccessResponse(await _localizationService.GetResourceAsync("Nop.Api.Checkout.ShippingMethod.Selected.Saved"),
                                       await _localizationService.GetResourceAsync("Nop.Api.Checkout.ShippingMethod.Selected.Saved"));
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex.Message, HttpStatusCode.BadRequest);
            }
        }

        [HttpGet]
        [Route("/api/checkout/payment_methods", Name = "CheckoutGetPaymentMethods")]
        [ProducesResponseType(typeof(CheckoutPaymentMethodDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ErrorsRootObject), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ErrorsRootObject), 422)]
        public async Task<BaseResponseModel> CheckoutGetPaymentMethods([FromQuery] CheckoutPaymentMethodRequest checkoutPaymentMethodRequest)
        {
            try
            {
                //validation
                if (OrderSettings.CheckoutDisabled)
                    return ErrorResponse("Checkout was disabled!", HttpStatusCode.Forbidden);

                if (checkoutPaymentMethodRequest.CustomerId <= 0)
                    return ErrorResponse("Invalide customer id", HttpStatusCode.BadRequest);

                var customer = await _customerService.GetCustomerByIdAsync(checkoutPaymentMethodRequest.CustomerId);
                if (customer == null)
                    return ErrorResponse("customer not found!", HttpStatusCode.NotFound);

                if (await _customerService.IsGuestAsync(customer) && !_orderSettings.AnonymousCheckoutAllowed)
                    throw new Exception("Anonymous checkout is not allowed");

                var store = await _storeContext.GetCurrentStoreAsync();
                var cart = await _shoppingCartService.GetShoppingCartAsync(customer, Nop.Core.Domain.Orders.ShoppingCartType.ShoppingCart, store.Id);


                //Check whether payment workflow is required
                var isPaymentWorkflowRequired = await _orderProcessingService.IsPaymentWorkflowRequiredAsync(cart, false);
                if (!isPaymentWorkflowRequired)
                {
                    await _genericAttributeService.SaveAttributeAsync<string>(customer,
                        NopCustomerDefaults.SelectedPaymentMethodAttribute, null, store.Id);

                    var model = new CheckoutPaymentMethodDto()
                    {
                        IsPaymentWorkflowRequired = false
                    };

                    //success with no payment methods
                    return SuccessResponse(await _localizationService.GetResourceAsync("Nop.Api.Checkout.PaymentMethods.PaymentWorkflowNotRequired"), model);
                }

                //filter by country
                var filterByCountryId = 0;
                if (_addressSettings.CountryEnabled)
                {
                    filterByCountryId = (await _customerService.GetCustomerBillingAddressAsync(customer))?.CountryId ?? 0;
                }

                //prepare dto model
                var paymentMethods = await _dtoHelper.PrepareCheckoutPaymentMethodDtoAsync(cart, filterByCountryId, customer, store);

                //no payment method available.
                if (!paymentMethods.PaymentMethods.Any())
                    return ErrorResponse(await _localizationService.GetResourceAsync("Nop.Api.Checkout.PaymentMethods.Error.NoMethodAvailable"), HttpStatusCode.NotFound);

                //success
                return SuccessResponse(await _localizationService.GetResourceAsync("Nop.Api.Checkout.PaymentMethods"), paymentMethods);
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex.Message, HttpStatusCode.BadRequest);
            }
        }

        [HttpPost]
        [Route("/api/checkout/set_payment_method", Name = "CheckoutSetPaymentMethod")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ErrorsRootObject), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ErrorsRootObject), 422)]
        public async Task<BaseResponseModel> CheckoutSetPaymentMethod([FromBody] CheckoutSavePaymentMethodRequest checkoutSavePaymentMethodRequest)
        {
            try
            {
                //validations
                if (OrderSettings.CheckoutDisabled)
                    throw new Exception(await _localizationService.GetResourceAsync("Checkout.Disabled"));

                if (checkoutSavePaymentMethodRequest.CustomerId <= 0)
                    return ErrorResponse("Invalide customer id", HttpStatusCode.BadRequest);

                if (string.IsNullOrEmpty(checkoutSavePaymentMethodRequest.PaymentMethodSystemName))
                    return ErrorResponse("selected payment method system name cannot be empty", HttpStatusCode.BadRequest);

                var customer = await _customerService.GetCustomerByIdAsync(checkoutSavePaymentMethodRequest.CustomerId);
                if (customer == null)
                    return ErrorResponse("customer not found!", HttpStatusCode.NotFound);

                var store = await _storeContext.GetCurrentStoreAsync();
                var cart = await _shoppingCartService.GetShoppingCartAsync(customer, Nop.Core.Domain.Orders.ShoppingCartType.ShoppingCart, store.Id);
                if (!cart.Any())
                    throw new Exception("Your cart is empty");

                if (await _customerService.IsGuestAsync(customer) && !_orderSettings.AnonymousCheckoutAllowed)
                    throw new Exception("Anonymous checkout is not allowed");

                //Check whether payment workflow is required
                var isPaymentWorkflowRequired = await _orderProcessingService.IsPaymentWorkflowRequiredAsync(cart);

                //set payment option
                await SetPaymentOptionAsync(customer, store, cart, checkoutSavePaymentMethodRequest.PaymentMethodSystemName, isPaymentWorkflowRequired);

                if (!isPaymentWorkflowRequired)
                    //success with no payment methods
                    return SuccessResponse(await _localizationService.GetResourceAsync("Nop.Api.Checkout.PaymentMethods.PaymentWorkflowNotRequired"), string.Empty);

                //success
                return SuccessResponse(await _localizationService.GetResourceAsync("Nop.Api.Checkout.PaymentMethod.Selected.Saved"),
                                       await _localizationService.GetResourceAsync("Nop.Api.Checkout.PaymentMethod.Selected.Saved"));
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex.Message, HttpStatusCode.BadRequest);
            }
        }

        [HttpPost]
        [Route("/api/orders/place_order", Name = "CheckoutPlaceOrder")]
        [ProducesResponseType(typeof(OrderDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ErrorsRootObject), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ErrorsRootObject), 422)]
        public async Task<BaseResponseModel> CheckoutPlaceOrder([FromBody] CheckoutOrderConfirmDto checkoutOrderConfirmDto)
        {
            try
            {
                //validations
                if (OrderSettings.CheckoutDisabled)
                    throw new Exception(await _localizationService.GetResourceAsync("Checkout.Disabled"));

                if (checkoutOrderConfirmDto.CustomerId <= 0)
                    return ErrorResponse("Invalide customer id", HttpStatusCode.BadRequest);

                var customer = await _customerService.GetCustomerByIdAsync(checkoutOrderConfirmDto.CustomerId);
                if (customer == null)
                    return ErrorResponse("customer not found!", HttpStatusCode.NotFound);

                var store = await _storeContext.GetCurrentStoreAsync();
                var cart = await _shoppingCartService.GetShoppingCartAsync(customer, Nop.Core.Domain.Orders.ShoppingCartType.ShoppingCart, store.Id);
                if (!cart.Any())
                    throw new Exception("Your cart is empty");

                if (await _customerService.IsGuestAsync(customer) && !_orderSettings.AnonymousCheckoutAllowed)
                    throw new Exception("Anonymous checkout is not allowed");

                if (!await CheckPermissions(checkoutOrderConfirmDto.CustomerId))
                {
                    // TODO: check _orderSettings.AnonymousCheckoutAllowed if IsGuest
                    return ErrorResponse("Access Denied!", HttpStatusCode.Forbidden);
                }

                //validate required inputs

                //bliing
                if (checkoutOrderConfirmDto.BillingAddressId <= 0)
                    return ErrorResponse("Invalide billing address id", HttpStatusCode.BadRequest);
                customer.BillingAddressId = checkoutOrderConfirmDto.BillingAddressId;

                //shipping
                if (await _shoppingCartService.ShoppingCartRequiresShippingAsync(cart))
                {
                    //default bill to ship
                    checkoutOrderConfirmDto.ShipToSameAddress = true;
                    if (checkoutOrderConfirmDto.ShipToSameAddress)
                        customer.ShippingAddressId = checkoutOrderConfirmDto.BillingAddressId;
                }

                //update billing/shipping addresses
                await _customerService.UpdateCustomerAsync(customer);

                //bypass shipping step:
                //existing address
                var billingAddress = await _customerService.GetCustomerAddressAsync(customer.Id, checkoutOrderConfirmDto.BillingAddressId)
                    ?? throw new Exception(await _localizationService.GetResourceAsync("Checkout.Address.NotFound"));

                //update customer coordinates.
                await SaveCustomerGeoCoordinatesAsync(customer, billingAddress);

                //get available shipping methods
                var shippingMethods = await _dtoHelper.PrepareCheckoutShippingMethodDtoAsync(cart, billingAddress, customer, store);
                if (_shippingSettings.BypassShippingMethodSelectionIfOnlyOne &&
                    shippingMethods?.ShippingMethods?.Count == 1)
                {
                    //if we have only one shipping method, then a customer doesn't have to choose a shipping method
                    var shippingMethod = shippingMethods.ShippingMethods.First();

                    //set shipping option
                    await SetShippingOptionAsync(customer, store, cart, shippingMethod.Name, shippingMethod.ShippingRateComputationMethodSystemName);
                }
                else
                {
                    //still by pass shipping step by selecting ground shipping by default.
                    var shippingMethod = shippingMethods?.ShippingMethods?.FirstOrDefault(x => x.ShippingRateComputationMethodSystemName.Equals("Shipping.FixedByWeightByTotal"));
                    if (shippingMethod != null)
                        //set shipping option
                        await SetShippingOptionAsync(customer, store, cart, shippingMethod.Name, shippingMethod.ShippingRateComputationMethodSystemName);
                }

                //set payment option
                var isPaymentWorkflowRequired = await _orderProcessingService.IsPaymentWorkflowRequiredAsync(cart);
                await SetPaymentOptionAsync(customer, store, cart, checkoutOrderConfirmDto.PaymentMethodSystemName, isPaymentWorkflowRequired);

                //(Note: Currently we only support checkout/money order (COD) payment method. No redirection method supported)
                //payment request
                var processPaymentRequest = new ProcessPaymentRequest();
                processPaymentRequest.OrderGuid = Guid.NewGuid();
                processPaymentRequest.StoreId = store.Id;
                processPaymentRequest.CustomerId = customer.Id;
                processPaymentRequest.PaymentMethodSystemName = await _genericAttributeService.GetAttributeAsync<string>(customer,
                    NopCustomerDefaults.SelectedPaymentMethodAttribute, store.Id);

                //place order
                var placeOrderResult = await _orderProcessingService.PlaceOrderAsync(processPaymentRequest);
                if (placeOrderResult.Success)
                {
                    var postProcessPaymentRequest = new PostProcessPaymentRequest
                    {
                        Order = placeOrderResult.PlacedOrder
                    };

                    //await _paymentService.PostProcessPaymentAsync(postProcessPaymentRequest);

                    //prepare order dto
                    var placedOrderDto = await _dtoHelper.PrepareOrderDTOAsync(placeOrderResult.PlacedOrder);

                    //success
                    return SuccessResponse(await _localizationService.GetResourceAsync("Nop.Api.Checkout.OrderPlace.Success"), placedOrderDto);
                }

                //error
                return ErrorResponse("Order place error", HttpStatusCode.BadRequest, placeOrderResult.Errors.ToList());
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex.Message, HttpStatusCode.BadRequest);
            }
        }

        #region Stripe payment 

        [HttpGet]
        [Route("/api/checkout/stripe_redirect_url", Name = "CheckoutGetStripeRedirectUrl")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ErrorsRootObject), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ErrorsRootObject), 422)]
        public async Task<BaseResponseModel> CheckoutGetStripeRedirectUrl([FromQuery] int orderId)
        {
            var customer = await _authenticationService.GetAuthenticatedCustomerAsync();
            if (customer is null)
                return ErrorResponse("Unauthorized! Customer not found", HttpStatusCode.Unauthorized);

            //get order
            var order = _orderApiService.GetOrderById(orderId);
            if (order is null)
                return ErrorResponse("order not found", HttpStatusCode.NotFound);

            //check order permission
            if (order.CustomerId != customer.Id)
                return ErrorResponse("Unauthorized! Order customer is different", HttpStatusCode.Unauthorized);

            try
            {
                //stripe: get redirect url (stripe plugin service call)
                var redirectUrl = await _stripeConnectRedirectService.PostProcessPaymentAndGetRedirectUrl(order, true);

                //success
                return SuccessResponse(await _localizationService.GetResourceAsync("Nop.Api.Checkout.Stripe.PaymentUrl"), redirectUrl);
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex.Message, HttpStatusCode.BadRequest);
            }
        }

        #endregion

        #region Private methods

        private async Task<bool> CheckPermissions(int? customerId)
        {
            var currentCustomer = await _authenticationService.GetAuthenticatedCustomerAsync();
            if (currentCustomer is null) // authenticated, but does not exist in db
                return false;
            if (customerId.HasValue && currentCustomer.Id == customerId)
            {
                // if I want to handle my own orders, check only public store permission
                return await _permissionService.AuthorizeAsync(StandardPermissionProvider.EnableShoppingCart, currentCustomer);
            }
            // if I want to handle other customer's orders, check admin permission
            return await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageOrders, currentCustomer);
        }

        private async Task SetShippingOptionAsync(Customer customer, Store store, IList<ShoppingCartItem> cart, string shippingMethodName, string shippingRateComputationMethodSystemName)
        {
            if (await _shoppingCartService.ShoppingCartRequiresShippingAsync(cart))
            {
                if (customer == null)
                    throw new ArgumentNullException(nameof(customer));

                if (store == null)
                    throw new ArgumentNullException(nameof(store));

                if (string.IsNullOrEmpty(shippingMethodName))
                    throw new Exception("shippingMethodName canot be empty");

                if (string.IsNullOrEmpty(shippingRateComputationMethodSystemName))
                    throw new Exception("shippingRateComputationMethodSystemName canot be empty");

                if (!cart.Any())
                    throw new Exception("Your cart is empty");

                //find it
                //performance optimization. try cache first
                var shippingOptions = await _genericAttributeService.GetAttributeAsync<List<ShippingOption>>(customer,
                    NopCustomerDefaults.OfferedShippingOptionsAttribute, store.Id);
                if (shippingOptions == null || !shippingOptions.Any())
                {
                    //not found? let's load them using shipping service
                    shippingOptions = (await _shippingService.GetShippingOptionsAsync(cart, await _customerService.GetCustomerShippingAddressAsync(customer),
                        customer, shippingRateComputationMethodSystemName.Trim(), store.Id)).ShippingOptions.ToList();
                }
                else
                {
                    //loaded cached results. let's filter result by a chosen shipping rate computation method
                    shippingOptions = shippingOptions.Where(so => so.ShippingRateComputationMethodSystemName.Equals(shippingRateComputationMethodSystemName.Trim(), StringComparison.InvariantCultureIgnoreCase))
                        .ToList();
                }

                var shippingOption = shippingOptions
                    .Find(so => !string.IsNullOrEmpty(so.Name) && so.Name.Equals(shippingMethodName.Trim(), StringComparison.InvariantCultureIgnoreCase));
                if (shippingOption == null)
                    throw new Exception("Selected shipping method can't be loaded");

                //save
                await _genericAttributeService.SaveAttributeAsync(customer, NopCustomerDefaults.SelectedShippingOptionAttribute, shippingOption, store.Id);
            }

        }

        private async Task SetPaymentOptionAsync(Customer customer, Store store, IList<ShoppingCartItem> cart, string paymentMethodSystemName, bool isPaymentWorkflowRequired)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            if (store == null)
                throw new ArgumentNullException(nameof(store));

            if (string.IsNullOrEmpty(paymentMethodSystemName))
                throw new Exception("paymentMethodSystemName canot be empty");

            if (!cart.Any())
                throw new Exception("Your cart is empty");

            //Check whether payment workflow is required
            if (!isPaymentWorkflowRequired)
            {
                //payment is not required
                await _genericAttributeService.SaveAttributeAsync<string>(customer,
                    NopCustomerDefaults.SelectedPaymentMethodAttribute, null, store.Id);
            }

            var paymentMethodInst = await _paymentPluginManager
                .LoadPluginBySystemNameAsync(paymentMethodSystemName, customer, store.Id);
            if (!_paymentPluginManager.IsPluginActive(paymentMethodInst))
                throw new Exception("Selected payment method can't be parsed");

            //save
            await _genericAttributeService.SaveAttributeAsync(customer,
                NopCustomerDefaults.SelectedPaymentMethodAttribute, paymentMethodSystemName, store.Id);
        }

        /// <summary>
        /// Save Customer GeoCoordinates
        /// </summary>
        /// <param name="customer"></param>
        /// <returns></returns>
        private async Task<string> SaveCustomerGeoCoordinatesAsync(Customer customer, Address address)
        {
            customer.LastSearchedCoordinates = address?.GeoLocationCoordinates;
            customer.LastSearchedText = address?.Address1;
            await _customerService.UpdateCustomerAsync(customer);

            return customer.LastSearchedText;
        }

        #endregion
    }
}
