using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core.Domain.Orders;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Orders;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Orders;
using Nop.Web.Framework.Factories;
using Nop.Web.Framework.Models.Extensions;

namespace Nop.Web.Areas.Admin.Factories
{
    /// <summary>
    /// Represents the return request model factory implementation
    /// </summary>
    public partial class ReturnRequestModelFactory
    {
        #region Methods

        /// <summary>
        /// Prepare return request model
        /// </summary>
        /// <param name="model">Return request model</param>
        /// <param name="returnRequest">Return request</param>
        /// <param name="excludeProperties">Whether to exclude populating of some properties of model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the return request model
        /// </returns>
        public virtual async Task<ReturnRequestModel> PrepareReturnRequestModelAsync(ReturnRequestModel model,
            ReturnRequest returnRequest, bool excludeProperties = false)
        {
            if (returnRequest == null)
                return model;

            //fill in model values from the entity
            model ??= returnRequest.ToModel<ReturnRequestModel>();

            var customer = await _customerService.GetCustomerByIdAsync(returnRequest.CustomerId);

            model.CreatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(returnRequest.CreatedOnUtc, DateTimeKind.Utc);

            model.CustomerInfo = await _customerService.IsRegisteredAsync(customer)
                ? customer.Email : await _localizationService.GetResourceAsync("Admin.Customers.Guest");
            model.UploadedFileGuid = (await _downloadService.GetDownloadByIdAsync(returnRequest.UploadedFileId))?.DownloadGuid ?? Guid.Empty;
            model.ReturnRequestStatusStr = await _localizationService.GetLocalizedEnumAsync(returnRequest.ReturnRequestStatus);
            var orderItem = await _orderService.GetOrderItemByIdAsync(returnRequest.OrderItemId);
            if (orderItem != null)
            {
                var order = await _orderService.GetOrderByIdAsync(orderItem.OrderId);
                var product = await _productService.GetProductByIdAsync(orderItem.ProductId);

                model.ProductId = product.Id;
                model.ProductName = await _productService.GetProductItemName(product, orderItem); //++Alchub
                model.OrderId = order.Id;
                model.AttributeInfo = orderItem.AttributeDescription;
                model.CustomAttributeInfo = orderItem.CustomAttributesDescription; //++Alchub custom
                model.CustomOrderNumber = order.CustomOrderNumber;
            }

            if (excludeProperties)
                return model;

            model.ReasonForReturn = returnRequest.ReasonForReturn;
            model.RequestedAction = returnRequest.RequestedAction;
            model.CustomerComments = returnRequest.CustomerComments;
            model.StaffNotes = returnRequest.StaffNotes;
            model.ReturnRequestStatusId = returnRequest.ReturnRequestStatusId;

            return model;
        }

        #endregion
    }
}