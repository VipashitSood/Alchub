using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Tax;
using Nop.Services.Catalog;
using Nop.Services.Directory;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Orders;
using Nop.Services.Seo;
using Nop.Web.Models.Order;

namespace Nop.Web.Factories
{
    /// <summary>
    /// Represents the return request model factory
    /// </summary>
    public partial class ReturnRequestModelFactory
    {
        #region Methods

        /// <summary>
        /// Prepares the order item models for return request by specified order.
        /// </summary>
        /// <param name="order">Order</param>
        /// <returns>
        /// The <see cref="Task"/> containing the <see cref="IList{SubmitReturnRequestModel.OrderItemModel}"/>
        /// </returns>
        protected virtual async Task<IList<SubmitReturnRequestModel.OrderItemModel>> PrepareSubmitReturnRequestOrderItemModelsAsync(Order order)
        {
            if (order is null)
                throw new ArgumentNullException(nameof(order));

            var models = new List<SubmitReturnRequestModel.OrderItemModel>();

            var returnRequestAvailability = await _returnRequestService.GetReturnRequestAvailabilityAsync(order.Id);
            if (returnRequestAvailability?.IsAllowed == true)
            {
                foreach (var returnableOrderItem in returnRequestAvailability.ReturnableOrderItems)
                {
                    if (returnableOrderItem.AvailableQuantityForReturn == 0)
                        continue;

                    var orderItem = returnableOrderItem.OrderItem;
                    var product = await _productService.GetProductByIdAsync(orderItem.ProductId);
                    var masterProduct = orderItem.GroupedProductId > 0 ? await _productService.GetProductByIdAsync(orderItem.GroupedProductId) :
                                                               await _productService.GetProductByIdAsync(orderItem.MasterProductId);
                    if (masterProduct == null)
                        masterProduct = product;

                    var model = new SubmitReturnRequestModel.OrderItemModel
                    {
                        Id = orderItem.Id,
                        ProductId = product.Id,
                        ProductName = await _productService.GetProductItemName(product, orderItem), //++Alchub
                        ProductSeName = await _urlRecordService.GetSeNameAsync(masterProduct), //++Alchub
                        AttributeInfo = orderItem.AttributeDescription,
                        Quantity = returnableOrderItem.AvailableQuantityForReturn,
                        CustomAttributeInfo = orderItem.CustomAttributesDescription
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