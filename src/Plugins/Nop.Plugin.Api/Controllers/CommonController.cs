using LinqToDB.Common;
using Microsoft.AspNetCore.Mvc;
using MySqlX.XDevAPI.Common;
using Nop.Core;
using Nop.Core.Alchub.Domain;
using Nop.Core.Alchub.Domain.Common;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Api.Attributes;
using Nop.Plugin.Api.Authorization.Attributes;
using Nop.Plugin.Api.DTO.Checkout;
using Nop.Plugin.Api.DTO.Errors;
using Nop.Plugin.Api.DTOs.Common;
using Nop.Plugin.Api.JSON.Serializers;
using Nop.Plugin.Api.Models.BaseModels;
using Nop.Plugin.Api.Models.Common;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Discounts;
using Nop.Services.Html;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Security;
using Nop.Services.Stores;
using System;
using System.Net;
using System.Threading.Tasks;

namespace Nop.Plugin.Api.Controllers
{
    public class CommonController : BaseApiController
    {
        #region Fields

        private readonly CommonSettings _commonSettings;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly IHtmlFormatter _htmlFormatter;
        private readonly ILocalizationService _localizationService;
        private readonly IWorkContext _workContext;
        private readonly Nop.Services.Messages.IWorkflowMessageService _defaultWorkflowMessageService;
        private readonly IGenericAttributeService _genericAttributeService;

        #endregion

        #region Ctor

        public CommonController(CommonSettings commonSettings,
            IJsonFieldsSerializer jsonFieldsSerializer,
            ICustomerActivityService customerActivityService,
            IStoreMappingService storeMappingService,
            IStoreService storeService,
            IDiscountService discountService,
            IPictureService pictureService,
            IHtmlFormatter htmlFormatter,
            ILocalizationService localizationService,
            IAclService aclService,
            ICustomerService customerService,
            IWorkContext workContext,
            IGenericAttributeService genericAttributeService,
            Nop.Services.Messages.IWorkflowMessageService defaultWorkflowMessageService) : base(jsonFieldsSerializer, aclService, customerService, storeMappingService, storeService, discountService, customerActivityService,
                                             localizationService, pictureService)
        {
            _commonSettings = commonSettings;
            _customerActivityService = customerActivityService;
            _htmlFormatter = htmlFormatter;
            _localizationService = localizationService;
            _workContext = workContext;
            _defaultWorkflowMessageService = defaultWorkflowMessageService;
            _genericAttributeService = genericAttributeService;
        }

        #endregion

        #region Methods

        /// <summary>
        ///    ContactUs
        /// </summary>
        /// <response code="200">OK</response>
        /// <response code="400">Bad Request</response>
        /// <response code="401">Unauthorized</response>
        [HttpPost]
        [Route("/api/common/contactus", Name = "API_ContactUs")]
        [AuthorizePermission("PublicStoreAllowNavigation")]
        [ProducesResponseType(typeof(ContactUsDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ErrorsRootObject), 422)]
        [GetRequestsErrorInterceptorActionFilter]
        public virtual async Task<BaseResponseModel> ContactUsSend([FromBody] ContactUsModel model)
        {
            if (model.UserId == 0)
            {
                return ErrorResponse(await _localizationService.GetResourceAsync("Nop.Api.InvalidUserId"));
            }
            try
            {
                if (ModelState.IsValid)
                {
                    var subject = _commonSettings.SubjectFieldOnContactUsForm ? model.Subject : null;
                    var body = _htmlFormatter.FormatText(model.Enquiry, false, true, false, false, false, false);

                    await _defaultWorkflowMessageService.SendContactUsMessageAsync((await _workContext.GetWorkingLanguageAsync()).Id,
                        model.Email.Trim(), model.Name, subject, model.Enquiry);

                    //activity log
                    await _customerActivityService.InsertActivityAsync("PublicStore.ContactUs",
                        await _localizationService.GetResourceAsync("ActivityLog.PublicStore.ContactUs"));

                    ContactUsDto contactUsDto = new ContactUsDto();
                    contactUsDto.Message = await _localizationService.GetResourceAsync("Nop.Api.YourEnquiryHasBeenSentSuccessfully");

                    return SuccessResponse(await _localizationService.GetResourceAsync("Nop.Api.YourEnquiryHasBeenSentSuccessfully"), contactUsDto);
                }
                return ErrorResponse("Request model is not valid", HttpStatusCode.BadRequest);
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex.Message, HttpStatusCode.BadRequest);
            }

        }

        [HttpPost]
        [Route("/api/common/set_customer_searched_coordinates", Name = "API_SetCustomerSearchedCoordinates")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ErrorsRootObject), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ErrorsRootObject), 422)]
        public async Task<BaseResponseModel> SetCustomerSearchedCoordinates([FromBody] LocationSearchModel locationSearchModel)
        {
            try
            {
                //validations
                if (locationSearchModel.CustomerId <= 0)
                    return ErrorResponse("Invalid customer id", HttpStatusCode.BadRequest);

                var customer = await _customerService.GetCustomerByIdAsync(locationSearchModel.CustomerId);
                if (customer == null)
                    return ErrorResponse("customer not found!", HttpStatusCode.NotFound);

                if (locationSearchModel.ClearLocation)
                {
                    //clear location
                    customer.LastSearchedCoordinates = null;
                    customer.LastSearchedText = null;
                    //update customer
                    await _customerService.UpdateCustomerAsync(customer);

                    //success
                    return SuccessResponse(await _localizationService.GetResourceAsync("Nop.Api.Customer.Location.Cleared"), "");
                }
                else
                {
                    if (string.IsNullOrEmpty(locationSearchModel.Latitude) || string.IsNullOrEmpty(locationSearchModel.Longitude))
                    {
                        return ErrorResponse("Coordinates latlng is missing!", HttpStatusCode.BadRequest);
                    }
                    else
                    {
                        //save location
                        customer.LastSearchedCoordinates = $"{locationSearchModel.Latitude}{NopAlchubDefaults.LATLNG_SEPARATOR}{locationSearchModel.Longitude}";
                        customer.LastSearchedText = locationSearchModel.SearchedText?.Trim();
                        //update customer
                        await _customerService.UpdateCustomerAsync(customer);

                        if (locationSearchModel.AddressType != null)
                        {
                            var addressType = string.Empty;
                            switch (locationSearchModel.AddressType)
                            {
                                case "Home":
                                    addressType = locationSearchModel.AddressType;
                                    break;
                                case "Work":
                                    addressType = locationSearchModel.AddressType;
                                    break;
                                case "Other":
                                    addressType = locationSearchModel.AddressType;
                                    break;
                                default:
                                    return ErrorResponse("Invalid address type", HttpStatusCode.BadRequest);
                            }

                            await _genericAttributeService.SaveAttributeAsync(customer, NopCustomerDefaults.LastSearchedAddressType, addressType);
                        }
                    }


                    //success
                    return SuccessResponse(await _localizationService.GetResourceAsync("Nop.Api.Customer.Location.Saved"), "");
                    }
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex.Message, HttpStatusCode.BadRequest);
            }
        }

        #endregion
    }
}
