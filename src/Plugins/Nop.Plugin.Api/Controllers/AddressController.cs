using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.Api.JSON.Serializers;
using Nop.Plugin.Api.Models.BaseModels;
using Nop.Plugin.Api.Services;
using Nop.Services.Customers;
using Nop.Services.Discounts;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Security;
using Nop.Plugin.Api.Infrastructure;
using Nop.Services.Seo;
using Nop.Services.Stores;
using System;
using System.Linq;
using System.Threading.Tasks;
using Nop.Plugin.Api.MappingApiExtensions;
using System.Net;
using Nop.Plugin.Api.DTOs.Address;
using Nop.Plugin.Api.DTO;
using Nop.Plugin.Api.MappingExtensions;
using Nop.Plugin.Api.Attributes;
using Nop.Plugin.Api.DTO.Errors;
using Nop.Plugin.Api.Models.Address;
using Nop.Plugin.Api.Factories;
using Nop.Services.Directory;
using Nop.Core.Domain.Common;
using Nop.Plugin.Api.Authorization.Attributes;
using Nop.Plugin.Api.ModelBinders;
using Nop.Plugin.Api.Delta;
using Nop.Services.Common;
using Nop.Core.Domain.Customers;
using Nop.Services.Messages;
using Nop.Core.Alchub.Domain;

namespace Nop.Plugin.Api.Controllers
{
    public class AddressController : BaseApiController
    {
        #region Fields
        private readonly ICustomerService _customerService;
        private readonly ICustomerApiService _customerApiService;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly IAddressApiService _addressApiService;
        private readonly IAddressModelFactory _addressModelFactory;
        private readonly AddressSettings _addressSettings;
        private readonly IAddressService _addressService;
        private readonly ICountryService _countryService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly CustomerSettings _customerSettings;
        private readonly ICustomerRegistrationService _customerRegistrationService;
        private readonly INotificationService _notificationService;
        #endregion

        #region Ctor
        public AddressController(

            IJsonFieldsSerializer jsonFieldsSerializer,
            IUrlRecordService urlRecordService,
            ICustomerActivityService customerActivityService,
            ILocalizationService localizationService,
            IAddressModelFactory addressModelFactory,
            IPictureService pictureService,
            IStoreMappingService storeMappingService,
            IStoreService storeService,
            IDiscountService discountService,
            IAddressApiService addressApiService,
            ICountryService countryService,
            AddressSettings addressSettings,
            ICustomerApiService customerApiService,
            IStateProvinceService stateProvinceService,
            IAclService aclService,
            IAddressService addressService,
            INotificationService notificationService,
            IStoreContext storeContext,
            IWorkContext workContext,
            ICustomerRegistrationService customerRegistrationService,
            CustomerSettings customerSettings,
            ICustomerService customerService
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
            _addressModelFactory = addressModelFactory;
            _workContext = workContext;
            _addressApiService = addressApiService;
            _storeContext = storeContext;
            _customerService = customerService;
            _countryService = countryService;
            _customerApiService = customerApiService;
            _addressSettings = addressSettings;
            _addressService = addressService;
            _stateProvinceService = stateProvinceService;
            _customerSettings = customerSettings;
            _customerRegistrationService = customerRegistrationService;
            _notificationService = notificationService;
        }
        #endregion

        #region AddNewAddress
        [HttpPost]
        [Route("/api/AddAddress")]
        [ProducesResponseType(typeof(ErrorsRootObject), (int)HttpStatusCode.BadRequest)]
        [GetRequestsErrorInterceptorActionFilter]
        public async Task<BaseResponseModel> AddAddress([FromBody] AddressDto model)
        {
            try
            {
                var customer = await _customerApiService.GetCustomerEntityByIdAsync(model.UserId);
                var newAddress = model;

                var address = _addressApiService.FindAddress((await _customerService.GetAddressesByCustomerIdAsync(customer.Id)).ToList(),
                    newAddress.FirstName, newAddress.LastName, newAddress.PhoneNumber,
                    newAddress.Email,
                    newAddress.Address1, newAddress.City
                    , newAddress.StateProvinceId, newAddress.ZipPostalCode,
                    newAddress.CountryId);

                if (address == null)
                {
                    //address is not found. let's create a new one
                    address = newAddress.ToEntity();
                    // address.CustomAttributes = customAttributes;
                    address.CreatedOnUtc = DateTime.UtcNow;
                    //some validation
                    if (address.CountryId == 0)
                        address.CountryId = null;
                    if (address.StateProvinceId == 0)
                        address.StateProvinceId = null;


                    //address.GeoLocation = model.Address1;
                    address.GeoLocationCoordinates = $"{model.GeoLant?.Trim()}{NopAlchubDefaults.LATLNG_SEPARATOR}{model.GeoLong?.Trim()}";
                    address.AddressTypeId = model.AddressTypeId;
                    await _addressApiService.InsertAddressAsync(address);

                    await _customerService.InsertCustomerAddressAsync(customer, address);
                }

                customer.BillingAddressId = address.Id;

                await _customerService.UpdateCustomerAsync(customer);

                var data = new AddressResponse
                {
                    AddressId = address.Id
                };
                return SuccessResponse(await _localizationService.GetResourceAsync("api.address.saved.successfully"), data);
            }
            catch (Exception ex)
            {
                return ErrorResponse(await _localizationService.GetResourceAsync("api.Address.NotSaved"), HttpStatusCode.NotFound);
            }
        }
        #endregion

        #region Addresses
        [HttpGet]
        [Route("/api/MyAddressList")]
        [AuthorizePermission("PublicStoreAllowNavigation")]
        [ProducesResponseType(typeof(CustomerAddressListModel), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ErrorsRootObject), (int)HttpStatusCode.BadRequest)]
        [GetRequestsErrorInterceptorActionFilter]
        public async Task<BaseResponseModel> MyAddressList([FromQuery] AddressListModel model)
        {
            if (model.PageSize < Constants.Configurations.MIN_LIMIT || model.PageSize > Constants.Configurations.MAX_LIMIT)
                return ErrorResponse("invalid limit parameter", HttpStatusCode.BadRequest);
            if (model.PageSize < Constants.Configurations.DEFAULT_PAGE_VALUE)
                return ErrorResponse("invalid page parameter", HttpStatusCode.BadRequest);
            var customer = await _customerApiService.GetCustomerEntityByIdAsync(model.Userid);
            //get customer addresses
            var allAddresses = await _customerApiService.GetAddressesByCustomerIdAsync(customer.Id, model.PageIndex, model.PageSize);
            var store = await _storeContext.GetCurrentStoreAsync();

            var modelAddress = new CustomerAddressListModel();
            foreach (var address in allAddresses)
            {
                var addressModel = new AddressDto();
                await _addressModelFactory.PrepareAddressModelAsync(addressModel,
                    address: address,
                    excludeProperties: false,
                    addressSettings: _addressSettings, null
                    );
                modelAddress.Addresses.Add(addressModel);
            }
            var customerAddressListObject = new CustomerAddressListModel
            {
                Addresses = modelAddress.Addresses,
                PageIndex = allAddresses.PageIndex + 1,
                PageSize = allAddresses.PageSize,
                TotalRecords = allAddresses.TotalCount
            };
            return SuccessResponse(await _localizationService.GetResourceAsync("Nop.Api.CountryList"), customerAddressListObject);
        }

        [HttpGet]
        [Route("/api/ViewAddress")]
        [AuthorizePermission("PublicStoreAllowNavigation")]
        [ProducesResponseType(typeof(CustomerAddressEditModel), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ErrorsRootObject), (int)HttpStatusCode.BadRequest)]
        [GetRequestsErrorInterceptorActionFilter]
        public async Task<BaseResponseModel> ViewAddress([FromQuery] ViewAddressModel model)
        {
            var customer = await _customerApiService.GetCustomerEntityByIdAsync(model.userId);
            //var customer = await _workContext.GetCurrentCustomerAsync();
            if (!await _customerService.IsRegisteredAsync(customer))

                return ErrorResponse(await _localizationService.GetResourceAsync("api.User.NotFound"), HttpStatusCode.NotFound);

            //find address (ensure that it belongs to the current customer)
            var address = await _customerService.GetCustomerAddressAsync(customer.Id, model.addressId);
            if (address == null)
                //address is not found
                return ErrorResponse(await _localizationService.GetResourceAsync("api.Address.NotFound"), HttpStatusCode.NotFound);
            //var country = await _countryService.GetCountryByIdAsync(Convert.ToInt32(address.CountryId));
            //var state = await _stateProvinceService.GetStateProvinceByIdAsync(Convert.ToInt32(address.StateProvinceId));
            //modelAddress.Address.CountryName = country.Name;
            //modelAddress.Address.StateProvinceName = state.Name;
            var modelAddress = new CustomerAddressEditModel();
            await _addressModelFactory.PrepareAddressModelAsync(modelAddress.Address,
                address: address,
                excludeProperties: false,
                addressSettings: _addressSettings, null);

            return SuccessResponse(await _localizationService.GetResourceAsync("api.Address.View"), modelAddress);
        }

        [HttpPost]
        [Route("/api/UpdateAddress")]
        [AuthorizePermission("PublicStoreAllowNavigation")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ErrorsRootObject), (int)HttpStatusCode.BadRequest)]
        [GetRequestsErrorInterceptorActionFilter]
        public async Task<BaseResponseModel> UpdateAddress([FromBody] AddressDto model)
        {
            try
            {
                var customer = await _customerApiService.GetCustomerEntityByIdAsync(model.UserId);
                if (!await _customerService.IsRegisteredAsync(customer))
                    return ErrorResponse(await _localizationService.GetResourceAsync("api.User.NotFound"), HttpStatusCode.NotFound);

                //find address (ensure that it belongs to the current customer)
                var address = await _customerService.GetCustomerAddressAsync(customer.Id, model.Id);
                if (address == null)
                    //address is not found
                    return ErrorResponse(await _localizationService.GetResourceAsync("api.Address.NotFound"), HttpStatusCode.NotFound);

                var data = new AddressResponse
                {
                    AddressId = address.Id
                };

                if (ModelState.IsValid)
                {
                    address = model.ToEntity(address);
                    //address.GeoLocation = model.Address1;
                    address.GeoLocationCoordinates = $"{model.GeoLant?.Trim()}{NopAlchubDefaults.LATLNG_SEPARATOR}{model.GeoLong?.Trim()}";
                    address.AddressTypeId = model.AddressTypeId;
                    await _addressApiService.UpdateAddressAsync(address);
                    return SuccessResponse(await _localizationService.GetResourceAsync("api.Address.Updated.successfully"), data);
                }
                return ErrorResponse(await _localizationService.GetResourceAsync("api.Address.NotFound"), HttpStatusCode.NotFound);
            }
            catch (Exception ex)
            {
                return ErrorResponse(await _localizationService.GetResourceAsync("api.Address.NotFound"), HttpStatusCode.NotFound);
            }

        }

        [HttpPost]
        [Route("/api/AddressDelete")]
        [AuthorizePermission("PublicStoreAllowNavigation")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ErrorsRootObject), (int)HttpStatusCode.BadRequest)]
        [GetRequestsErrorInterceptorActionFilter]
        public async Task<BaseResponseModel> AddressDelete([FromBody] DeleteAddressModel model)
        {
            try
            {
                if (model.userId == 0)
                    return ErrorResponse(await _localizationService.GetResourceAsync("api.User.Invalid.Userid"), HttpStatusCode.NotFound);
                if (model.addressId == 0)
                    return ErrorResponse(await _localizationService.GetResourceAsync("api.User.Invalid.addressId"), HttpStatusCode.NotFound);

                var customer = await _customerApiService.GetCustomerEntityByIdAsync(model.userId);
                if (!await _customerService.IsRegisteredAsync(customer))
                    return ErrorResponse(await _localizationService.GetResourceAsync("api.User.NotFound"), HttpStatusCode.NotFound);

                //find address (ensure that it belongs to the current customer)
                var address = await _customerService.GetCustomerAddressAsync(customer.Id, model.addressId);
                if (address != null)
                {
                    await _customerService.RemoveCustomerAddressAsync(customer, address);
                    await _customerService.UpdateCustomerAsync(customer);
                    //now delete the address record
                    await _addressService.DeleteAddressAsync(address);
                }

                //after Delete get customer addresses
                var allAddresses = await _customerService.GetAddressesByCustomerIdAsync(customer.Id);
                var store = await _storeContext.GetCurrentStoreAsync();

                var modelAddress = new CustomerAddressListModel();
                foreach (var addres in allAddresses)
                {
                    var addressModel = new AddressDto();
                    await _addressModelFactory.PrepareAddressModelAsync(addressModel,
                        address: addres,
                        excludeProperties: false,
                        addressSettings: _addressSettings, null
                        );
                    modelAddress.Addresses.Add(addressModel);
                }
                var customerAddressListObject = new CustomerAddressListModel
                {
                    Addresses = modelAddress.Addresses
                };

                return SuccessResponse(await _localizationService.GetResourceAsync("api.Address.Delete.successfully"), customerAddressListObject);

            }
            catch (Exception ex)
            {
                return ErrorResponse(await _localizationService.GetResourceAsync("api.Address.NotFound"), HttpStatusCode.NotFound);
            }

        }

        #endregion
    }
}



