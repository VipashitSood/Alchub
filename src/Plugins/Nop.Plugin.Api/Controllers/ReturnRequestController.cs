using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Orders;
using Nop.Core.Infrastructure;
using Nop.Plugin.Api.Attributes;
using Nop.Plugin.Api.Domain;
using Nop.Plugin.Api.DTO.Customers;
using Nop.Plugin.Api.DTO.Errors;
using Nop.Plugin.Api.DTO.Orders;
using Nop.Plugin.Api.Factories;
using Nop.Plugin.Api.JSON.Serializers;
using Nop.Plugin.Api.Models.BaseModels;
using Nop.Plugin.Api.Models.ReturnRequest;
using Nop.Plugin.Api.Services;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Discounts;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Services.Stores;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Nop.Plugin.Api.Controllers
{
    public class ReturnRequestController : BaseApiController
    {
        #region Fields

        private readonly ICustomerService _customerService;
        private readonly ILocalizationService _localizationService;
        private readonly IReturnRequestModelFactory _returnRequestModelFactory;
        private readonly ICustomerApiService _customerApiService;
        private readonly IOrderService _orderService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly OrderSettings _orderSettings;
        private readonly IDownloadService _downloadService;
        private readonly IReturnRequestService _returnRequestService;
        private readonly IStoreContext _storeContext;
        private readonly ICustomNumberFormatter _customNumberFormatter;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly LocalizationSettings _localizationSettings;
        private readonly INopFileProvider _fileProvider;
        private readonly IWebHelper _webHelper;
        private readonly IWebHostEnvironment _env;
        private readonly IGenericAttributeService _genericAttributeService;

        #endregion

        #region Ctor

        public ReturnRequestController(ICustomerService customerService,
            IOrderService orderService,
            ILocalizationService localizationService,
            IReturnRequestModelFactory returnRequestModelFactory,
            IAclService aclService,
            IDownloadService downloadService,
            IStoreService storeService,
            ICustomerActivityService customerActivityService,
            IDiscountService discountService,
            IJsonFieldsSerializer jsonFieldsSerializer,
            IStoreMappingService storeMappingService,
            IPictureService pictureService,
            IOrderProcessingService orderProcessingService,
            IWorkflowMessageService workflowMessageService,
            ICustomerApiService customerApiService,
            IReturnRequestService returnRequestService,
            ICustomNumberFormatter customNumberFormatter,
            LocalizationSettings localizationSettings,
            IWebHelper webHelper,
            IStoreContext storeContext,
            INopFileProvider fileProvider,
            IGenericAttributeService genericAttributeService,
            IWebHostEnvironment env,
            OrderSettings orderSettings
          ) :
            base(jsonFieldsSerializer,
            aclService,
            customerService,
            storeMappingService,
            storeService,
            discountService,
            customerActivityService,
            localizationService,
            pictureService)
        {
            _customerService = customerService;
            _localizationService = localizationService;
            _returnRequestModelFactory = returnRequestModelFactory;
            _customerApiService = customerApiService;
            _orderService = orderService;
            _orderProcessingService = orderProcessingService;
            _orderSettings = orderSettings;
            _downloadService = downloadService;
            _returnRequestService = returnRequestService;
            _storeContext = storeContext;
            _customNumberFormatter = customNumberFormatter;
            _workflowMessageService = workflowMessageService;
            _localizationSettings = localizationSettings;
            _fileProvider = fileProvider;
            _webHelper = webHelper;
            _env = env;
            _genericAttributeService = genericAttributeService;
        }

        #endregion

        #region Methods

        [HttpGet]
        [Route("/api/return_requests", Name = "returnrequests")]
        [ProducesResponseType(typeof(ProfileDataDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorsRootObject), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        public async Task<BaseResponseModel> ReturnRequests(int UserId)
        {
            if (!await _customerService.IsRegisteredAsync(await _customerApiService.GetCustomerEntityByIdAsync(UserId)))
                return ErrorResponse(await _localizationService.GetResourceAsync("account.customer.alreadyexist"), HttpStatusCode.NotFound);

            var model = await _returnRequestModelFactory.PrepareCustomerReturnRequestsModelAsync(UserId);
            return SuccessResponse(await _localizationService.GetResourceAsync("nop.api.returnrequest.List"), model);
        }

        [HttpGet]
        [Route("/api/return_reason_and_actionlist", Name = "ReturnReasonAndActionList")]
        [ProducesResponseType(typeof(ErrorsRootObject), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        public async Task<BaseResponseModel> ReturnReasonsAndActions([FromQuery] ReturnRequestModel returnModel)
        {
            if (returnModel.UserId == 0)
                return ErrorResponse(await _localizationService.GetResourceAsync("api.User.Invalid.Userid"), HttpStatusCode.NotFound);
            if (returnModel.OrderId == 0)
                return ErrorResponse(await _localizationService.GetResourceAsync("api.User.Invalid.OrderId"), HttpStatusCode.NotFound);
            if (returnModel.ItemId == 0)
                return ErrorResponse(await _localizationService.GetResourceAsync("api.User.Invalid.ItemId"), HttpStatusCode.NotFound);

            var order = await _orderService.GetOrderByIdAsync(returnModel.OrderId);
            var customer = await _customerApiService.GetCustomerEntityByIdAsync(returnModel.UserId);
            if (order == null || order.Deleted || customer.Id != order.CustomerId)
                return ErrorResponse(await _localizationService.GetResourceAsync("nop.api.order.notexist"), HttpStatusCode.NotFound);

           var orderItem = await _orderService.GetOrderItemByIdAsync(returnModel.ItemId);
            if (orderItem == null ||  orderItem.OrderId != order.Id)
                return ErrorResponse(await _localizationService.GetResourceAsync("nop.api.orderItem.notexists"), HttpStatusCode.NotFound);

            if (!await _orderProcessingService.IsReturnRequestAllowedAsync(order))
                return ErrorResponse(await _localizationService.GetResourceAsync("nop.api.order.notfound"), HttpStatusCode.NotFound);

            var model = new SubmitReturnResponseModel();
            model = await _returnRequestModelFactory.PrepareSubmitReturnRequestModelAsync(model, order,orderItem.Id);
            return SuccessResponse(await _localizationService.GetResourceAsync("nop.api.returnreasonandaction.list"), model);
        }

        [HttpPost]
        [Route("/api/add_return_request", Name = "AddReturnRequest")]
        [ProducesResponseType(typeof(ErrorsRootObject), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        public async Task<BaseResponseModel> AddReturnRequest([FromForm] AddReturnRequestModel model)
        {
            var order = await _orderService.GetOrderByIdAsync(model.ItemId);
            var customer = await _customerApiService.GetCustomerEntityByIdAsync(model.UserId);

            if (order == null || order.Deleted || customer.Id != order.CustomerId)
                return ErrorResponse(await _localizationService.GetResourceAsync("nop.api.order.notexist"), HttpStatusCode.NotFound);

            if (!await _orderProcessingService.IsReturnRequestAllowedAsync(order))
                return ErrorResponse(await _localizationService.GetResourceAsync("nop.api.order.notfound"), HttpStatusCode.NotFound);

            var count = 0;

            if (model.ReturnOrderPicUrl != null)
            {
                if (!(model.ReturnOrderPicUrl.FileName.EndsWith(".doc") || !model.ReturnOrderPicUrl.FileName.EndsWith(".pdf") || !model.ReturnOrderPicUrl.FileName.EndsWith(".zip") || !model.ReturnOrderPicUrl.FileName.EndsWith(".jpeg") || !model.ReturnOrderPicUrl.FileName.EndsWith(".jpg") || !model.ReturnOrderPicUrl.FileName.EndsWith(".png") || !model.ReturnOrderPicUrl.FileName.EndsWith(".JFIF")))
                {
                    return ErrorResponse(await _localizationService.GetResourceAsync("Customer.Invalid.File.Extension", 1), HttpStatusCode.NotFound);
                }
            }
            
            //returnable products
            var orderItems = await _orderService.GetOrderItemsAsync(order.Id, isNotReturnable: false);
            foreach (var orderItem in orderItems)
            {
                var quantity = 0; //parse quantity
                if (model.quantity > 0)
                {
                    var rrr = await _returnRequestService.GetReturnRequestReasonByIdAsync(model.ReturnRequestReasonId);
                    var rra = await _returnRequestService.GetReturnRequestActionByIdAsync(model.ReturnRequestActionId);
                    var store = await _storeContext.GetCurrentStoreAsync();

                    var rr = new ReturnRequest
                    {
                        CustomNumber = "",
                        StoreId = store.Id,
                        OrderItemId = orderItem.Id,
                        Quantity = quantity,
                        CustomerId = customer.Id,
                        ReasonForReturn = rrr != null ? await _localizationService.GetLocalizedAsync(rrr, x => x.Name) : "not available",
                        RequestedAction = rra != null ? await _localizationService.GetLocalizedAsync(rra, x => x.Name) : "not available",
                        StaffNotes = string.Empty,
                        ReturnRequestStatus = ReturnRequestStatus.Pending,
                        CreatedOnUtc = DateTime.UtcNow,
                        UpdatedOnUtc = DateTime.UtcNow
                    };

                  
                    await _returnRequestService.InsertReturnRequestAsync(rr);

                    if (model.ReturnOrderPicUrl != null)
                    {
                        string filePath = Path.Combine(_env.ContentRootPath, "wwwroot/images/");

                        //Split the string by character . to get file extension type
                        int lastIndex = model.ReturnOrderPicUrl.FileName.LastIndexOf('.');
                        if (lastIndex + 1 < model.ReturnOrderPicUrl.Length)
                        {
                            string firstPart = model.ReturnOrderPicUrl.FileName.Substring(0, lastIndex);
                            string secondPart = model.ReturnOrderPicUrl.FileName.Substring(lastIndex + 1);
                            string newFileName = $"{firstPart}-{DateTime.UtcNow:yyyyMMdd_hhmmss}." + secondPart;
                            await _genericAttributeService.SaveAttributeAsync(rr, NopCustomerApiDefaults.ReturnOrderPicUrlAttribute, newFileName);
                            using (Stream stream = new FileStream(filePath + newFileName, FileMode.Create))
                            {
                                model.ReturnOrderPicUrl.CopyTo(stream);
                            }
                        }
                    }
                    //set return request custom number
                    rr.CustomNumber = _customNumberFormatter.GenerateReturnRequestCustomNumber(rr);
                    await _customerService.UpdateCustomerAsync(customer);
                    await _returnRequestService.UpdateReturnRequestAsync(rr);

                    //notify store owner
                    await _workflowMessageService.SendNewReturnRequestStoreOwnerNotificationAsync(rr, orderItem, order, _localizationSettings.DefaultAdminLanguageId);
                    //notify customer
                    await _workflowMessageService.SendNewReturnRequestCustomerNotificationAsync(rr, orderItem, order);

                    count++;
                }
            }
          
            if (count > 0)
            return SuccessResponse(await _localizationService.GetResourceAsync("ReturnRequests.Submitted"), model.ItemId);
            else
                return ErrorResponse(await _localizationService.GetResourceAsync("ReturnRequests.NoItemsSubmitted"), HttpStatusCode.NotFound);
        }
       
        #endregion
    }
}
