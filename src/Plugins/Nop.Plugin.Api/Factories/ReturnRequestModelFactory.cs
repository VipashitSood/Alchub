using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Tax;
using Nop.Plugin.Api.Domain;
using Nop.Plugin.Api.DTO.Orders;
using Nop.Plugin.Api.Services;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Directory;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Orders;
using Nop.Services.Seo;

namespace Nop.Plugin.Api.Factories
{
    /// <summary>
    /// Represents the return request model factory
    /// </summary>
    public partial class ReturnRequestModelFactory : IReturnRequestModelFactory
    {
        #region Fields

        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IDownloadService _downloadService;
        private readonly ILocalizationService _localizationService;
        private readonly IOrderService _orderService;
        private readonly IProductService _productService;
        private readonly IReturnRequestService _returnRequestService;
        private readonly IStoreContext _storeContext;
        private readonly IUrlRecordService _urlRecordService;
        private readonly ICustomerApiService _customerApiService;
        private readonly OrderSettings _orderSettings;
        private readonly IWorkContext _workContext;
        private readonly ICurrencyService _currencyService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IWebHelper _webHelper;
        private readonly IGenericAttributeService _genericAttributeService;

        #endregion

        #region Ctor

        public ReturnRequestModelFactory(
            IDateTimeHelper dateTimeHelper,
            IDownloadService downloadService,
            ILocalizationService localizationService,
            IOrderService orderService,
            ICustomerApiService customerApiService,
            IProductService productService,
            IReturnRequestService returnRequestService,
            IStoreContext storeContext,
            IUrlRecordService urlRecordService,
            ICurrencyService currencyService,
            IPriceFormatter priceFormatter,
            IGenericAttributeService genericAttributeService,
            IWebHelper webHelper,
            IWorkContext workContext,
             OrderSettings orderSettings
            )
        {
            _dateTimeHelper = dateTimeHelper;
            _downloadService = downloadService;
            _localizationService = localizationService;
            _orderService = orderService;
            _productService = productService;
            _returnRequestService = returnRequestService;
            _storeContext = storeContext;
            _urlRecordService = urlRecordService;
            _customerApiService = customerApiService;
            _orderSettings = orderSettings;
            _currencyService = currencyService;
            _priceFormatter = priceFormatter;
            _workContext = workContext;
            _webHelper = webHelper;
            _genericAttributeService = genericAttributeService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Prepare the submit return request model
        /// </summary>
        /// <param name="model">Submit return request model</param>
        /// <param name="order">Order</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the submit return request model
        /// </returns>
        public virtual async Task<SubmitReturnResponseModel> PrepareSubmitReturnRequestModelAsync(SubmitReturnResponseModel model,
            Order order ,int orderItemId)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            if (model == null)
                throw new ArgumentNullException(nameof(model));

            model.OrderId = order.Id;
            model.AllowFiles = _orderSettings.ReturnRequestsAllowFiles;
            model.CustomOrderNumber = order.CustomOrderNumber;

            //return reasons
            model.AvailableReturnReasons = await (await _returnRequestService.GetAllReturnRequestReasonsAsync())
                .SelectAwait(async rrr => new SubmitReturnResponseModel.ReturnRequestReasonModel
                {
                    Id = rrr.Id,
                    Name = await _localizationService.GetLocalizedAsync(rrr, x => x.Name)
                }).ToListAsync();

            //return actions
            model.AvailableReturnActions = await (await _returnRequestService.GetAllReturnRequestActionsAsync())
                .SelectAwait(async rra => new SubmitReturnResponseModel.ReturnRequestActionModel
                {
                    Id = rra.Id,
                    Name = await _localizationService.GetLocalizedAsync(rra, x => x.Name)
                })
                .ToListAsync();

            //returnable products
            model.Items = await PrepareSubmitReturnRequestOrderItemModelsAsync(order);
            if (model.Items.Any())
            {
                model.Items = model.Items.Where(i => i.Id == orderItemId).ToList();
            }

            return model;
        }
        /// <summary>
        /// Prepare the customer return requests model
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the customer return requests model
        /// </returns>
        public virtual async Task<CustomerReturnRequestsModel> PrepareCustomerReturnRequestsModelAsync(int UserId)
        {
            var model = new CustomerReturnRequestsModel();
            var store = await _storeContext.GetCurrentStoreAsync();
            var customer = await _customerApiService.GetCustomerEntityByIdAsync(UserId);
            var returnRequests = await _returnRequestService.SearchReturnRequestsAsync(store.Id, customer.Id);

          

            foreach (var returnRequest in returnRequests)
            {
                var orderItem = await _orderService.GetOrderItemByIdAsync(returnRequest.OrderItemId);
                if (orderItem != null)
                {
                    var product = await _productService.GetProductByIdAsync(orderItem.ProductId);
                    var filePath = "";
                    var download = await _downloadService.GetDownloadByIdAsync(returnRequest.UploadedFileId);
                    var ReturnOrderPicUrl = await _genericAttributeService.GetAttributeAsync<string>(returnRequest, NopCustomerApiDefaults.ReturnOrderPicUrlAttribute);
                    var storeUrl = _webHelper.GetStoreLocation();
                    if (ReturnOrderPicUrl != null)
                    {
                        filePath = Path.Combine(storeUrl, "images/") + ReturnOrderPicUrl;
                    }
                    
                    var itemModel = new CustomerReturnRequestsModel.ReturnRequestModel
                    {
                        Id = returnRequest.Id,
                        CustomNumber = returnRequest.CustomNumber,
                        ReturnRequestStatus = await _localizationService.GetLocalizedEnumAsync(returnRequest.ReturnRequestStatus),
                        ProductId = product.Id,
                        ProductName = await _localizationService.GetLocalizedAsync(product, x => x.Name),
                        ProductSeName = await _urlRecordService.GetSeNameAsync(product),
                        Quantity = returnRequest.Quantity,
                        ReturnAction = returnRequest.RequestedAction,
                        ReturnReason = returnRequest.ReasonForReturn,
                        Comments = returnRequest.CustomerComments,
                        UploadedFileGuid = download?.DownloadGuid ?? Guid.Empty,
                        DownloadUrl = filePath != null? filePath:string.Empty,
                        CreatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(returnRequest.CreatedOnUtc, DateTimeKind.Utc),
                    };
                    model.Items.Add(itemModel);
                }
            }

            return model;
        }
        /// <summary>
        /// Prepares the order item models for return request by specified order.
        /// </summary>
        /// <param name="order">Order</param>
        /// <returns>
        /// The <see cref="Task"/> containing the <see cref="IList{SubmitReturnResponseModel.OrderItemModel}"/>
        /// </returns>
        public virtual async Task<IList<SubmitReturnResponseModel.OrderItemModel>> PrepareSubmitReturnRequestOrderItemModelsAsync(Order order)
        {
            if (order is null)
                throw new ArgumentNullException(nameof(order));

            var models = new List<SubmitReturnResponseModel.OrderItemModel>();

            var returnRequestAvailability = await _returnRequestService.GetReturnRequestAvailabilityAsync(order.Id);
            if (returnRequestAvailability?.IsAllowed == true)
            {
                foreach (var returnableOrderItem in returnRequestAvailability.ReturnableOrderItems)
                {
                    if (returnableOrderItem.AvailableQuantityForReturn == 0)
                        continue;

                    var orderItem = returnableOrderItem.OrderItem;
                    var product = await _productService.GetProductByIdAsync(orderItem.ProductId);

                    var model = new SubmitReturnResponseModel.OrderItemModel
                    {
                        Id = orderItem.Id,
                        ProductId = product.Id,
                        ProductName = await _localizationService.GetLocalizedAsync(product, x => x.Name),
                        ProductSeName = await _urlRecordService.GetSeNameAsync(product),
                        AttributeInfo = orderItem.AttributeDescription,
                        Quantity = returnableOrderItem.AvailableQuantityForReturn
                    };

                    var languageId = (await _workContext.GetWorkingLanguageAsync()).Id;

                    //unit price
                    if (order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax)
                    {
                        //including tax
                        var unitPriceInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(orderItem.UnitPriceInclTax, order.CurrencyRate);
                        model.UnitPrice = await _priceFormatter.FormatPriceAsync(unitPriceInclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, languageId, true);
                    }
                    else
                    {
                        //excluding tax
                        var unitPriceExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(orderItem.UnitPriceExclTax, order.CurrencyRate);
                        model.UnitPrice = await _priceFormatter.FormatPriceAsync(unitPriceExclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, languageId, false);
                    }

                    models.Add(model);
                }
            }

            return models;
        }
        #endregion
    }
}