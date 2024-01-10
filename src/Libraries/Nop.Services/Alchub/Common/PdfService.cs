using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Tax;
using Nop.Core.Domain.Vendors;
using Nop.Services.Alchub.Slots;

namespace Nop.Services.Common
{
    /// <summary>
    /// Represents a pdf service
    /// </summary>
    public partial class PdfService : IPdfService
    {
        #region methods

        /// <summary>
        /// Print totals
        /// </summary>
        /// <param name="vendorId">Vendor identifier</param>
        /// <param name="lang">Language</param>
        /// <param name="order">Order</param>
        /// <param name="font">Text font</param>
        /// <param name="titleFont">Title font</param>
        /// <param name="doc">PDF document</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        protected virtual async Task PrintTotalsAsync(int vendorId, Language lang, Order order, Font font, Font titleFont, Document doc)
        {
            //vendors cannot see totals
            if (vendorId != 0)
                return;

            //subtotal
            var totalsTable = new PdfPTable(1)
            {
                RunDirection = GetDirection(lang),
                WidthPercentage = 100f
            };
            totalsTable.DefaultCell.Border = Rectangle.NO_BORDER;

            var languageId = lang.Id;

            //order subtotal
            if (order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax &&
                !_taxSettings.ForceTaxExclusionFromOrderSubtotal)
            {
                //including tax

                var orderSubtotalInclTaxInCustomerCurrency =
                    _currencyService.ConvertCurrency(order.OrderSubtotalInclTax, order.CurrencyRate);
                var orderSubtotalInclTaxStr = await _priceFormatter.FormatPriceAsync(orderSubtotalInclTaxInCustomerCurrency, true,
                    order.CustomerCurrencyCode, languageId, true);

                var p = GetPdfCell($"{await _localizationService.GetResourceAsync("PDFInvoice.Sub-Total", languageId)} {orderSubtotalInclTaxStr}", font);
                p.HorizontalAlignment = Element.ALIGN_RIGHT;
                p.Border = Rectangle.NO_BORDER;
                totalsTable.AddCell(p);
            }
            else
            {
                //excluding tax

                var orderSubtotalExclTaxInCustomerCurrency =
                    _currencyService.ConvertCurrency(order.OrderSubtotalExclTax, order.CurrencyRate);
                var orderSubtotalExclTaxStr = await _priceFormatter.FormatPriceAsync(orderSubtotalExclTaxInCustomerCurrency, true,
                    order.CustomerCurrencyCode, languageId, false);

                var p = GetPdfCell($"{await _localizationService.GetResourceAsync("PDFInvoice.Sub-Total", languageId)} {orderSubtotalExclTaxStr}", font);
                p.HorizontalAlignment = Element.ALIGN_RIGHT;
                p.Border = Rectangle.NO_BORDER;
                totalsTable.AddCell(p);
            }

            //discount (applied to order subtotal)
            if (order.OrderSubTotalDiscountExclTax > decimal.Zero)
            {
                //order subtotal
                if (order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax &&
                    !_taxSettings.ForceTaxExclusionFromOrderSubtotal)
                {
                    //including tax

                    var orderSubTotalDiscountInclTaxInCustomerCurrency =
                        _currencyService.ConvertCurrency(order.OrderSubTotalDiscountInclTax, order.CurrencyRate);
                    var orderSubTotalDiscountInCustomerCurrencyStr = await _priceFormatter.FormatPriceAsync(
                        -orderSubTotalDiscountInclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, languageId, true);

                    var p = GetPdfCell($"{await _localizationService.GetResourceAsync("PDFInvoice.Discount", languageId)} {orderSubTotalDiscountInCustomerCurrencyStr}", font);
                    p.HorizontalAlignment = Element.ALIGN_RIGHT;
                    p.Border = Rectangle.NO_BORDER;
                    totalsTable.AddCell(p);
                }
                else
                {
                    //excluding tax

                    var orderSubTotalDiscountExclTaxInCustomerCurrency =
                        _currencyService.ConvertCurrency(order.OrderSubTotalDiscountExclTax, order.CurrencyRate);
                    var orderSubTotalDiscountInCustomerCurrencyStr = await _priceFormatter.FormatPriceAsync(
                        -orderSubTotalDiscountExclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, languageId, false);

                    var p = GetPdfCell($"{await _localizationService.GetResourceAsync("PDFInvoice.Discount", languageId)} {orderSubTotalDiscountInCustomerCurrencyStr}", font);
                    p.HorizontalAlignment = Element.ALIGN_RIGHT;
                    p.Border = Rectangle.NO_BORDER;
                    totalsTable.AddCell(p);
                }
            }

            //shipping
            //++Alchub
            //by default do not show shipping address.
            bool showShipping = false;
            //--Alchub
            if (order.ShippingStatus != ShippingStatus.ShippingNotRequired && showShipping)
            {
                if (order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax)
                {
                    //including tax
                    var orderShippingInclTaxInCustomerCurrency =
                        _currencyService.ConvertCurrency(order.OrderShippingInclTax, order.CurrencyRate);
                    var orderShippingInclTaxStr = await _priceFormatter.FormatShippingPriceAsync(
                        orderShippingInclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, languageId, true);

                    var p = GetPdfCell($"{await _localizationService.GetResourceAsync("PDFInvoice.Shipping", languageId)} {orderShippingInclTaxStr}", font);
                    p.HorizontalAlignment = Element.ALIGN_RIGHT;
                    p.Border = Rectangle.NO_BORDER;
                    totalsTable.AddCell(p);
                }
                else
                {
                    //excluding tax
                    var orderShippingExclTaxInCustomerCurrency =
                        _currencyService.ConvertCurrency(order.OrderShippingExclTax, order.CurrencyRate);
                    var orderShippingExclTaxStr = await _priceFormatter.FormatShippingPriceAsync(
                        orderShippingExclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, languageId, false);

                    var p = GetPdfCell($"{await _localizationService.GetResourceAsync("PDFInvoice.Shipping", languageId)} {orderShippingExclTaxStr}", font);
                    p.HorizontalAlignment = Element.ALIGN_RIGHT;
                    p.Border = Rectangle.NO_BORDER;
                    totalsTable.AddCell(p);
                }
            }

            /*Alchub Start*/
            //delivery fee
            var deliveryFee = _currencyService.ConvertCurrency(order.DeliveryFee, order.CurrencyRate);
            if (deliveryFee > 0)
            {
                var deliveryFeeStr = await _priceFormatter.FormatPriceAsync(deliveryFee, true, order.CustomerCurrencyCode, false, languageId);

                var pDeliveryFee = GetPdfCell($"{string.Format(await _localizationService.GetResourceAsync("Alchub.PDFInvoice.DeliveryFee", languageId), deliveryFeeStr)}", font);
                pDeliveryFee.HorizontalAlignment = Element.ALIGN_RIGHT;
                pDeliveryFee.Border = Rectangle.NO_BORDER;
                totalsTable.AddCell(pDeliveryFee);
            }
            /*Alchub End*/

            //payment fee
            if (order.PaymentMethodAdditionalFeeExclTax > decimal.Zero)
            {
                if (order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax)
                {
                    //including tax
                    var paymentMethodAdditionalFeeInclTaxInCustomerCurrency =
                        _currencyService.ConvertCurrency(order.PaymentMethodAdditionalFeeInclTax, order.CurrencyRate);
                    var paymentMethodAdditionalFeeInclTaxStr = await _priceFormatter.FormatPaymentMethodAdditionalFeeAsync(
                        paymentMethodAdditionalFeeInclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, languageId, true);

                    var p = GetPdfCell($"{await _localizationService.GetResourceAsync("PDFInvoice.PaymentMethodAdditionalFee", languageId)} {paymentMethodAdditionalFeeInclTaxStr}", font);
                    p.HorizontalAlignment = Element.ALIGN_RIGHT;
                    p.Border = Rectangle.NO_BORDER;
                    totalsTable.AddCell(p);
                }
                else
                {
                    //excluding tax
                    var paymentMethodAdditionalFeeExclTaxInCustomerCurrency =
                        _currencyService.ConvertCurrency(order.PaymentMethodAdditionalFeeExclTax, order.CurrencyRate);
                    var paymentMethodAdditionalFeeExclTaxStr = await _priceFormatter.FormatPaymentMethodAdditionalFeeAsync(
                        paymentMethodAdditionalFeeExclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, languageId, false);

                    var p = GetPdfCell($"{await _localizationService.GetResourceAsync("PDFInvoice.PaymentMethodAdditionalFee", languageId)} {paymentMethodAdditionalFeeExclTaxStr}", font);
                    p.HorizontalAlignment = Element.ALIGN_RIGHT;
                    p.Border = Rectangle.NO_BORDER;
                    totalsTable.AddCell(p);
                }
            }

            //tax
            var taxStr = string.Empty;
            var taxRates = new SortedDictionary<decimal, decimal>();
            bool displayTax;
            var displayTaxRates = true;
            if (_taxSettings.HideTaxInOrderSummary && order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax)
            {
                displayTax = false;
            }
            else
            {
                if (order.OrderTax == 0 && _taxSettings.HideZeroTax)
                {
                    displayTax = false;
                    displayTaxRates = false;
                }
                else
                {
                    taxRates = _orderService.ParseTaxRates(order, order.TaxRates);

                    displayTaxRates = _taxSettings.DisplayTaxRates && taxRates.Any();
                    displayTax = !displayTaxRates;

                    var orderTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderTax, order.CurrencyRate);
                    taxStr = await _priceFormatter.FormatPriceAsync(orderTaxInCustomerCurrency, true, order.CustomerCurrencyCode,
                        false, languageId);
                }
            }

            if (displayTax)
            {
                var p = GetPdfCell($"{await _localizationService.GetResourceAsync("PDFInvoice.Tax", languageId)} {taxStr}", font);
                p.HorizontalAlignment = Element.ALIGN_RIGHT;
                p.Border = Rectangle.NO_BORDER;
                totalsTable.AddCell(p);
            }

            if (displayTaxRates)
            {
                foreach (var item in taxRates)
                {
                    var taxRate = string.Format(await _localizationService.GetResourceAsync("PDFInvoice.TaxRate", languageId),
                        _priceFormatter.FormatTaxRate(item.Key));
                    var taxValue = await _priceFormatter.FormatPriceAsync(
                        _currencyService.ConvertCurrency(item.Value, order.CurrencyRate), true, order.CustomerCurrencyCode,
                        false, languageId);

                    var p = GetPdfCell($"{taxRate} {taxValue}", font);
                    p.HorizontalAlignment = Element.ALIGN_RIGHT;
                    p.Border = Rectangle.NO_BORDER;
                    totalsTable.AddCell(p);
                }
            }

            /*Alchub Start*/
            //service fee
            if (order.ServiceFee > 0)
            {
                var serviceFee = string.Format(await _localizationService.GetResourceAsync("PDFInvoice.ServiceFee", languageId),
                            await _priceFormatter.FormatPriceAsync(_currencyService.ConvertCurrency(order.ServiceFee, order.CurrencyRate)));
                var pdfCell = GetPdfCell($"{serviceFee}", font);
                pdfCell.HorizontalAlignment = Element.ALIGN_RIGHT;
                pdfCell.Border = Rectangle.NO_BORDER;
                totalsTable.AddCell(pdfCell);
            }

            //slot fee
            if (order.SlotFee > 0)
            {
                var slotFee = string.Format(await _localizationService.GetResourceAsync("PDFInvoice.SlotFee", languageId),
                        await _priceFormatter.FormatPriceAsync(_currencyService.ConvertCurrency(order.SlotFee, order.CurrencyRate)));
                var pdfslotCell = GetPdfCell($"{slotFee}", font);
                pdfslotCell.HorizontalAlignment = Element.ALIGN_RIGHT;
                pdfslotCell.Border = Rectangle.NO_BORDER;
                totalsTable.AddCell(pdfslotCell);
            }

            //Tip fee
            if (order.TipFee > 0)
            {
                var tipFee = _currencyService.ConvertCurrency(order.TipFee, order.CurrencyRate);
                var tipFeeStr = await _priceFormatter.FormatPriceAsync(tipFee, true, order.CustomerCurrencyCode, false, languageId);

                var pTipFee = GetPdfCell($"{string.Format(await _localizationService.GetResourceAsync("Alchub.PDFInvoice.TipFee", languageId), tipFeeStr)}", font);
                pTipFee.HorizontalAlignment = Element.ALIGN_RIGHT;
                pTipFee.Border = Rectangle.NO_BORDER;
                totalsTable.AddCell(pTipFee);
            }
            /*Alchub End*/

            //discount (applied to order total)
            if (order.OrderDiscount > decimal.Zero)
            {
                var orderDiscountInCustomerCurrency =
                    _currencyService.ConvertCurrency(order.OrderDiscount, order.CurrencyRate);
                var orderDiscountInCustomerCurrencyStr = await _priceFormatter.FormatPriceAsync(-orderDiscountInCustomerCurrency,
                    true, order.CustomerCurrencyCode, false, languageId);

                var p = GetPdfCell($"{await _localizationService.GetResourceAsync("PDFInvoice.Discount", languageId)} {orderDiscountInCustomerCurrencyStr}", font);
                p.HorizontalAlignment = Element.ALIGN_RIGHT;
                p.Border = Rectangle.NO_BORDER;
                totalsTable.AddCell(p);
            }

            //gift cards
            foreach (var gcuh in await _giftCardService.GetGiftCardUsageHistoryAsync(order))
            {
                var gcTitle = string.Format(await _localizationService.GetResourceAsync("PDFInvoice.GiftCardInfo", languageId),
                    (await _giftCardService.GetGiftCardByIdAsync(gcuh.GiftCardId))?.GiftCardCouponCode);
                var gcAmountStr = await _priceFormatter.FormatPriceAsync(
                    -_currencyService.ConvertCurrency(gcuh.UsedValue, order.CurrencyRate), true,
                    order.CustomerCurrencyCode, false, languageId);

                var p = GetPdfCell($"{gcTitle} {gcAmountStr}", font);
                p.HorizontalAlignment = Element.ALIGN_RIGHT;
                p.Border = Rectangle.NO_BORDER;
                totalsTable.AddCell(p);
            }

            //reward points
            if (order.RedeemedRewardPointsEntryId.HasValue && await _rewardPointService.GetRewardPointsHistoryEntryByIdAsync(order.RedeemedRewardPointsEntryId.Value) is RewardPointsHistory redeemedRewardPointsEntry)
            {
                var rpTitle = string.Format(await _localizationService.GetResourceAsync("PDFInvoice.RewardPoints", languageId),
                    -redeemedRewardPointsEntry.Points);
                var rpAmount = await _priceFormatter.FormatPriceAsync(
                    -_currencyService.ConvertCurrency(redeemedRewardPointsEntry.UsedAmount, order.CurrencyRate),
                    true, order.CustomerCurrencyCode, false, languageId);

                var p = GetPdfCell($"{rpTitle} {rpAmount}", font);
                p.HorizontalAlignment = Element.ALIGN_RIGHT;
                p.Border = Rectangle.NO_BORDER;
                totalsTable.AddCell(p);
            }

            //order total
            var orderTotalInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderTotal, order.CurrencyRate);
            var orderTotalStr = await _priceFormatter.FormatPriceAsync(orderTotalInCustomerCurrency, true, order.CustomerCurrencyCode, false, languageId);

            var pTotal = GetPdfCell($"{await _localizationService.GetResourceAsync("PDFInvoice.OrderTotal", languageId)} {orderTotalStr}", titleFont);
            pTotal.HorizontalAlignment = Element.ALIGN_RIGHT;
            pTotal.Border = Rectangle.NO_BORDER;
            totalsTable.AddCell(pTotal);

            doc.Add(totalsTable);
        }


        protected virtual async Task PrintProductsAsync(int vendorId, Language lang, Font titleFont, Document doc, Order order, Font font, Font attributesFont)
        {
            var productsHeader = new PdfPTable(1)
            {
                RunDirection = GetDirection(lang),
                WidthPercentage = 100f
            };
            var cellProducts = await GetPdfCellAsync("PDFInvoice.Product(s)", lang, titleFont);
            cellProducts.Border = Rectangle.NO_BORDER;
            productsHeader.AddCell(cellProducts);
            doc.Add(productsHeader);
            doc.Add(new Paragraph(" "));

            //a vendor should have access only to products
            var orderItems = await _orderService.GetOrderItemsAsync(order.Id, vendorId: vendorId);

            var count = 5 + (_catalogSettings.ShowSkuOnProductDetailsPage ? 1 : 0)
                        + (_vendorSettings.ShowVendorOnOrderDetailsPage ? 1 : 0);

            var productsTable = new PdfPTable(count)
            {
                RunDirection = GetDirection(lang),
                WidthPercentage = 100f
            };

            var widths = new Dictionary<int, int[]>
            {
                { 4, new[] { 50, 20, 10, 20 } },
                { 5, new[] { 45, 15, 15, 10, 15 } },
                { 6, new[] { 40, 13, 13, 12, 10, 12 } },
                { 7, new[] { 30, 13, 13, 12, 10, 12,10 } }
            };

            productsTable.SetWidths(lang.Rtl ? widths[count].Reverse().ToArray() : widths[count]);

            //product name
            var cellProductItem = await GetPdfCellAsync("PDFInvoice.ProductName", lang, font);
            cellProductItem.BackgroundColor = BaseColor.LightGray;
            cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
            productsTable.AddCell(cellProductItem);

            //SKU
            if (_catalogSettings.ShowSkuOnProductDetailsPage)
            {
                cellProductItem = await GetPdfCellAsync("PDFInvoice.SKU", lang, font);
                cellProductItem.BackgroundColor = BaseColor.LightGray;
                cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
                productsTable.AddCell(cellProductItem);
            }

            //Vendor name
            if (_vendorSettings.ShowVendorOnOrderDetailsPage)
            {
                cellProductItem = await GetPdfCellAsync("PDFInvoice.VendorName", lang, font);
                cellProductItem.BackgroundColor = BaseColor.LightGray;
                cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
                productsTable.AddCell(cellProductItem);
            }

            //price
            cellProductItem = await GetPdfCellAsync("PDFInvoice.ProductPrice", lang, font);
            cellProductItem.BackgroundColor = BaseColor.LightGray;
            cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
            productsTable.AddCell(cellProductItem);

            //qty
            cellProductItem = await GetPdfCellAsync("PDFInvoice.ProductQuantity", lang, font);
            cellProductItem.BackgroundColor = BaseColor.LightGray;
            cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
            productsTable.AddCell(cellProductItem);

            //Alchub Start
            //ProductSlotDateTime
            cellProductItem = await GetPdfCellAsync("PDFInvoice.ProductSlotDateTime", lang, font);
            cellProductItem.BackgroundColor = BaseColor.LightGray;
            cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
            productsTable.AddCell(cellProductItem);
            //Alchub End

            //ProductTotal
            cellProductItem = await GetPdfCellAsync("PDFInvoice.ProductTotal", lang, font);
            cellProductItem.BackgroundColor = BaseColor.LightGray;
            cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
            productsTable.AddCell(cellProductItem);

            var vendors = _vendorSettings.ShowVendorOnOrderDetailsPage ? await _vendorService.GetVendorsByProductIdsAsync(orderItems.Select(item => item.ProductId).ToArray()) : new List<Vendor>();

            foreach (var orderItem in orderItems)
            {
                var product = await _productService.GetProductByIdAsync(orderItem.ProductId);

                var pAttribTable = new PdfPTable(1) { RunDirection = GetDirection(lang) };
                pAttribTable.DefaultCell.Border = Rectangle.NO_BORDER;

                //product name
                var name = await _productService.GetProductItemName(product, orderItem); //++Alchub;
                pAttribTable.AddCell(new Paragraph(name, font));
                cellProductItem.AddElement(new Paragraph(name, font));
                //attributes
                if (!string.IsNullOrEmpty(orderItem.AttributeDescription))
                {
                    var attributesParagraph =
                        new Paragraph(_htmlFormatter.ConvertHtmlToPlainText(orderItem.AttributeDescription, true, true),
                            attributesFont);
                    pAttribTable.AddCell(attributesParagraph);
                }
                //custom attributes
                if (!string.IsNullOrEmpty(orderItem.CustomAttributesDescription))
                {
                    var customAttributesParagraph =
                        new Paragraph(_htmlFormatter.ConvertHtmlToPlainText(orderItem.CustomAttributesDescription, true, true),
                            attributesFont);
                    pAttribTable.AddCell(customAttributesParagraph);
                }

                //rental info
                if (product.IsRental)
                {
                    var rentalStartDate = orderItem.RentalStartDateUtc.HasValue
                        ? _productService.FormatRentalDate(product, orderItem.RentalStartDateUtc.Value)
                        : string.Empty;
                    var rentalEndDate = orderItem.RentalEndDateUtc.HasValue
                        ? _productService.FormatRentalDate(product, orderItem.RentalEndDateUtc.Value)
                        : string.Empty;
                    var rentalInfo = string.Format(await _localizationService.GetResourceAsync("Order.Rental.FormattedDate"),
                        rentalStartDate, rentalEndDate);

                    var rentalInfoParagraph = new Paragraph(rentalInfo, attributesFont);
                    pAttribTable.AddCell(rentalInfoParagraph);
                }

                productsTable.AddCell(pAttribTable);

                //SKU
                if (_catalogSettings.ShowSkuOnProductDetailsPage)
                {
                    //var sku = await _productService.FormatSkuAsync(product, orderItem.AttributesXml);
                    var sku = product.UPCCode;
                    cellProductItem = GetPdfCell(sku ?? string.Empty, font);
                    cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
                    productsTable.AddCell(cellProductItem);
                }

                //Vendor name
                if (_vendorSettings.ShowVendorOnOrderDetailsPage)
                {
                    var vendorName = vendors.FirstOrDefault(v => v.Id == product.VendorId)?.Name ?? string.Empty;
                    cellProductItem = GetPdfCell(vendorName, font);
                    cellProductItem.HorizontalAlignment = Element.ALIGN_CENTER;
                    productsTable.AddCell(cellProductItem);
                }

                //price
                string unitPrice;
                if (order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax)
                {
                    //including tax
                    var unitPriceInclTaxInCustomerCurrency =
                        _currencyService.ConvertCurrency(orderItem.UnitPriceInclTax, order.CurrencyRate);
                    unitPrice = await _priceFormatter.FormatPriceAsync(unitPriceInclTaxInCustomerCurrency, true,
                        order.CustomerCurrencyCode, lang.Id, true);
                }
                else
                {
                    //excluding tax
                    var unitPriceExclTaxInCustomerCurrency =
                        _currencyService.ConvertCurrency(orderItem.UnitPriceExclTax, order.CurrencyRate);
                    unitPrice = await _priceFormatter.FormatPriceAsync(unitPriceExclTaxInCustomerCurrency, true,
                        order.CustomerCurrencyCode, lang.Id, false);
                }

                cellProductItem = GetPdfCell(unitPrice, font);
                cellProductItem.HorizontalAlignment = Element.ALIGN_LEFT;
                productsTable.AddCell(cellProductItem);

                //qty
                cellProductItem = GetPdfCell(orderItem.Quantity, font);
                cellProductItem.HorizontalAlignment = Element.ALIGN_LEFT;
                productsTable.AddCell(cellProductItem);

                //Alchub Start
                //SlotTime
                cellProductItem = GetPdfCell(((orderItem.InPickup ? await _localizationService.GetResourceAsync("Alchub.ShoppingCart.InPickup.Text") : await _localizationService.GetResourceAsync("Alchub.ShoppingCart.Delivery.Text")) + " " + orderItem.SlotStartTime.ToString("MM/dd/yyyy") + " " + SlotHelper.ConvertTo12hoursSlotTime(orderItem.SlotTime)), font);
                cellProductItem.HorizontalAlignment = Element.ALIGN_LEFT;
                productsTable.AddCell(cellProductItem);
                //Alchub End

                //total
                string subTotal;
                if (order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax)
                {
                    //including tax
                    var priceInclTaxInCustomerCurrency =
                        _currencyService.ConvertCurrency(orderItem.PriceInclTax, order.CurrencyRate);
                    subTotal = await _priceFormatter.FormatPriceAsync(priceInclTaxInCustomerCurrency, true, order.CustomerCurrencyCode,
                        lang.Id, true);
                }
                else
                {
                    //excluding tax
                    var priceExclTaxInCustomerCurrency =
                        _currencyService.ConvertCurrency(orderItem.PriceExclTax, order.CurrencyRate);
                    subTotal = await _priceFormatter.FormatPriceAsync(priceExclTaxInCustomerCurrency, true, order.CustomerCurrencyCode,
                        lang.Id, false);
                }

                cellProductItem = GetPdfCell(subTotal, font);
                cellProductItem.HorizontalAlignment = Element.ALIGN_LEFT;
                productsTable.AddCell(cellProductItem);
            }

            doc.Add(productsTable);
        }

        /// <summary>
        /// Print addresses
        /// </summary>
        /// <param name="vendorId">Vendor identifier</param>
        /// <param name="lang">Language</param>
        /// <param name="titleFont">Title font</param>
        /// <param name="order">Order</param>
        /// <param name="font">Text font</param>
        /// <param name="doc">Document</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        protected virtual async Task PrintAddressesAsync(int vendorId, Language lang, Font titleFont, Order order, Font font, Document doc)
        {
            var addressTable = new PdfPTable(2) { RunDirection = GetDirection(lang) };
            addressTable.DefaultCell.Border = Rectangle.NO_BORDER;
            addressTable.WidthPercentage = 100f;
            addressTable.SetWidths(new[] { 50, 50 });

            //billing info
            await PrintBillingInfoAsync(vendorId, lang, titleFont, order, font, addressTable);

            //shipping info
            //await PrintShippingInfoAsync(lang, order, titleFont, font, addressTable);

            //++Alchub
            //vendor pickup address info
            await PrintVendorPickupAddressInfoAsync(vendorId, lang, titleFont, order, font, addressTable);

            doc.Add(addressTable);
            doc.Add(new Paragraph(" "));
        }

        /// <summary>
        /// Print shipping info
        /// </summary>
        /// <param name="lang">Language</param>
        /// <param name="order">Order</param>
        /// <param name="titleFont">Title font</param>
        /// <param name="font">Text font</param>
        /// <param name="addressTable">PDF table for address</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        protected virtual async Task PrintShippingInfoAsync(Language lang, Order order, Font titleFont, Font font, PdfPTable addressTable)
        {
            var shippingAddressPdf = new PdfPTable(1)
            {
                RunDirection = GetDirection(lang)
            };
            shippingAddressPdf.DefaultCell.Border = Rectangle.NO_BORDER;

            //++Alchub
            //by default do not show shipping address.
            bool showShipping = false;
            //--Alchub

            if (order.ShippingStatus != ShippingStatus.ShippingNotRequired && showShipping)
            {
                //cell = new PdfPCell();
                //cell.Border = Rectangle.NO_BORDER;
                const string indent = "   ";

                if (!order.PickupInStore)
                {
                    if (order.ShippingAddressId == null || await _addressService.GetAddressByIdAsync(order.ShippingAddressId.Value) is not Address shippingAddress)
                        throw new NopException($"Shipping is required, but address is not available. Order ID = {order.Id}");

                    shippingAddressPdf.AddCell(await GetParagraphAsync("PDFInvoice.ShippingInformation", lang, titleFont));
                    if (!string.IsNullOrEmpty(shippingAddress.Company))
                        shippingAddressPdf.AddCell(await GetParagraphAsync("PDFInvoice.Company", indent, lang, font, shippingAddress.Company));
                    shippingAddressPdf.AddCell(await GetParagraphAsync("PDFInvoice.Name", indent, lang, font, shippingAddress.FirstName + " " + shippingAddress.LastName));
                    if (_addressSettings.PhoneEnabled)
                        shippingAddressPdf.AddCell(await GetParagraphAsync("PDFInvoice.Phone", indent, lang, font, shippingAddress.PhoneNumber));
                    if (_addressSettings.FaxEnabled && !string.IsNullOrEmpty(shippingAddress.FaxNumber))
                        shippingAddressPdf.AddCell(await GetParagraphAsync("PDFInvoice.Fax", indent, lang, font, shippingAddress.FaxNumber));
                    if (_addressSettings.StreetAddressEnabled)
                        shippingAddressPdf.AddCell(await GetParagraphAsync("Alchub.PDFInvoice.Address", indent, lang, font, shippingAddress.Address1));
                    if (_addressSettings.StreetAddress2Enabled && !string.IsNullOrEmpty(shippingAddress.Address2))
                        shippingAddressPdf.AddCell(await GetParagraphAsync("Alchub.PDFInvoice.Address2", indent, lang, font, shippingAddress.Address2));
                    //if (_addressSettings.CityEnabled || _addressSettings.StateProvinceEnabled ||
                    //    _addressSettings.CountyEnabled || _addressSettings.ZipPostalCodeEnabled)
                    //{
                    //    var addressLine = $"{indent}{shippingAddress.City}, " +
                    //        $"{(!string.IsNullOrEmpty(shippingAddress.County) ? $"{shippingAddress.County}, " : string.Empty)}" +
                    //        $"{(await _stateProvinceService.GetStateProvinceByAddressAsync(shippingAddress) is StateProvince stateProvince ? await _localizationService.GetLocalizedAsync(stateProvince, x => x.Name, lang.Id) : string.Empty)} " +
                    //        $"{shippingAddress.ZipPostalCode}";
                    //    shippingAddressPdf.AddCell(new Paragraph(addressLine, font));
                    //}

                    if (_addressSettings.CountryEnabled && await _countryService.GetCountryByAddressAsync(shippingAddress) is Country country)
                    {
                        shippingAddressPdf.AddCell(
                            new Paragraph(indent + await _localizationService.GetLocalizedAsync(country, x => x.Name, lang.Id), font));
                    }
                    //++Alchub
                    shippingAddressPdf.AddCell(await GetParagraphAsync("Alchub.PDFInvoice.Address.Fields.AddressType", indent, lang, font, shippingAddress.AddressType.ToString()));
                    //--Alchub
                    //custom attributes
                    var customShippingAddressAttributes = await _addressAttributeFormatter
                        .FormatAttributesAsync(shippingAddress.CustomAttributes, $"<br />{indent}");
                    if (!string.IsNullOrEmpty(customShippingAddressAttributes))
                    {
                        var text = _htmlFormatter.ConvertHtmlToPlainText(customShippingAddressAttributes, true, true);
                        shippingAddressPdf.AddCell(new Paragraph(indent + text, font));
                    }

                    shippingAddressPdf.AddCell(new Paragraph(" "));
                }
                else if (order.PickupAddressId.HasValue && await _addressService.GetAddressByIdAsync(order.PickupAddressId.Value) is Address pickupAddress)
                {
                    shippingAddressPdf.AddCell(await GetParagraphAsync("PDFInvoice.Pickup", lang, titleFont));

                    if (!string.IsNullOrEmpty(pickupAddress.Address1))
                        shippingAddressPdf.AddCell(new Paragraph(
                            $"{indent}{string.Format(await _localizationService.GetResourceAsync("PDFInvoice.Address", lang.Id), pickupAddress.Address1)}",
                            font));

                    if (!string.IsNullOrEmpty(pickupAddress.City))
                        shippingAddressPdf.AddCell(new Paragraph($"{indent}{pickupAddress.City}", font));

                    if (!string.IsNullOrEmpty(pickupAddress.County))
                        shippingAddressPdf.AddCell(new Paragraph($"{indent}{pickupAddress.County}", font));

                    if (await _countryService.GetCountryByAddressAsync(pickupAddress) is Country country)
                        shippingAddressPdf.AddCell(
                            new Paragraph($"{indent}{await _localizationService.GetLocalizedAsync(country, x => x.Name, lang.Id)}", font));

                    if (!string.IsNullOrEmpty(pickupAddress.ZipPostalCode))
                        shippingAddressPdf.AddCell(new Paragraph($"{indent}{pickupAddress.ZipPostalCode}", font));

                    shippingAddressPdf.AddCell(new Paragraph(" "));
                }

                shippingAddressPdf.AddCell(await GetParagraphAsync("PDFInvoice.ShippingMethod", indent, lang, font, order.ShippingMethod));
                shippingAddressPdf.AddCell(new Paragraph());

                addressTable.AddCell(shippingAddressPdf);
            }
            else
            {
                shippingAddressPdf.AddCell(new Paragraph());
                addressTable.AddCell(shippingAddressPdf);
            }
        }

        /// <summary>
        /// Print billing info
        /// </summary>
        /// <param name="vendorId">Vendor identifier</param>
        /// <param name="lang">Language</param>
        /// <param name="titleFont">Title font</param>
        /// <param name="order">Order</param>
        /// <param name="font">Text font</param>
        /// <param name="addressTable">Address PDF table</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        protected virtual async Task PrintBillingInfoAsync(int vendorId, Language lang, Font titleFont, Order order, Font font, PdfPTable addressTable)
        {
            const string indent = "   ";
            var billingAddressPdf = new PdfPTable(1) { RunDirection = GetDirection(lang) };
            billingAddressPdf.DefaultCell.Border = Rectangle.NO_BORDER;
            billingAddressPdf.DefaultCell.HorizontalAlignment = Element.ALIGN_LEFT;

            billingAddressPdf.AddCell(await GetParagraphAsync("PDFInvoice.BillingInformation", lang, titleFont));

            var billingAddress = await _addressService.GetAddressByIdAsync(order.BillingAddressId);

            if (_addressSettings.CompanyEnabled && !string.IsNullOrEmpty(billingAddress.Company))
                billingAddressPdf.AddCell(await GetParagraphAsync("PDFInvoice.Company", indent, lang, font, billingAddress.Company));

            billingAddressPdf.AddCell(await GetParagraphAsync("PDFInvoice.Name", indent, lang, font, billingAddress.FirstName + " " + billingAddress.LastName));

            if (_addressSettings.PhoneEnabled)
                billingAddressPdf.AddCell(await GetParagraphAsync("PDFInvoice.Phone", indent, lang, font, billingAddress.PhoneNumber));

            if (_addressSettings.FaxEnabled && !string.IsNullOrEmpty(billingAddress.FaxNumber))
                billingAddressPdf.AddCell(await GetParagraphAsync("PDFInvoice.Fax", indent, lang, font, billingAddress.FaxNumber));

            if (_addressSettings.StreetAddressEnabled)
                billingAddressPdf.AddCell(await GetParagraphAsync("Alchub.PDFInvoice.Address", indent, lang, font, billingAddress.Address1));

            if (_addressSettings.StreetAddress2Enabled && !string.IsNullOrEmpty(billingAddress.Address2))
                billingAddressPdf.AddCell(await GetParagraphAsync("Alchub.PDFInvoice.Address2", indent, lang, font, billingAddress.Address2));

            //if (_addressSettings.CityEnabled || _addressSettings.StateProvinceEnabled ||
            //    _addressSettings.CountyEnabled || _addressSettings.ZipPostalCodeEnabled)
            //{
            //    var addressLine = $"{indent}{billingAddress.City}, " +
            //        $"{(!string.IsNullOrEmpty(billingAddress.County) ? $"{billingAddress.County}, " : string.Empty)}" +
            //        $"{(await _stateProvinceService.GetStateProvinceByAddressAsync(billingAddress) is StateProvince stateProvince ? await _localizationService.GetLocalizedAsync(stateProvince, x => x.Name, lang.Id) : string.Empty)} " +
            //        $"{billingAddress.ZipPostalCode}";
            //    billingAddressPdf.AddCell(new Paragraph(addressLine, font));
            //}

            //++Alchub
            billingAddressPdf.AddCell(await GetParagraphAsync("Alchub.PDFInvoice.Address.Fields.AddressType", indent, lang, font, billingAddress.AddressType.ToString()));
            //--Alchub

            if (_addressSettings.CountryEnabled && await _countryService.GetCountryByAddressAsync(billingAddress) is Country country)
                billingAddressPdf.AddCell(new Paragraph(indent + await _localizationService.GetLocalizedAsync(country, x => x.Name, lang.Id), font));

            //VAT number
            if (!string.IsNullOrEmpty(order.VatNumber))
                billingAddressPdf.AddCell(await GetParagraphAsync("PDFInvoice.VATNumber", indent, lang, font, order.VatNumber));

            //custom attributes
            var customBillingAddressAttributes = await _addressAttributeFormatter
                .FormatAttributesAsync(billingAddress.CustomAttributes, $"<br />{indent}");
            if (!string.IsNullOrEmpty(customBillingAddressAttributes))
            {
                var text = _htmlFormatter.ConvertHtmlToPlainText(customBillingAddressAttributes, true, true);
                billingAddressPdf.AddCell(new Paragraph(indent + text, font));
            }

            //vendors payment details
            if (vendorId == 0)
            {
                //payment method
                var paymentMethod = await _paymentPluginManager.LoadPluginBySystemNameAsync(order.PaymentMethodSystemName);
                var paymentMethodStr = paymentMethod != null
                    ? await _localizationService.GetLocalizedFriendlyNameAsync(paymentMethod, lang.Id)
                    : order.PaymentMethodSystemName;
                if (!string.IsNullOrEmpty(paymentMethodStr))
                {
                    billingAddressPdf.AddCell(new Paragraph(" "));
                    billingAddressPdf.AddCell(await GetParagraphAsync("PDFInvoice.PaymentMethod", indent, lang, font, paymentMethodStr));
                    billingAddressPdf.AddCell(new Paragraph());
                }

                //custom values
                var customValues = _paymentService.DeserializeCustomValues(order);
                if (customValues != null)
                {
                    foreach (var item in customValues)
                    {
                        billingAddressPdf.AddCell(new Paragraph(" "));
                        billingAddressPdf.AddCell(new Paragraph(indent + item.Key + ": " + item.Value, font));
                        billingAddressPdf.AddCell(new Paragraph());
                    }
                }
            }

            addressTable.AddCell(billingAddressPdf);
        }

        /// <summary>
        /// Print vendor pickup address info
        /// </summary>
        /// <param name="vendorId">Vendor identifier</param>
        /// <param name="lang">Language</param>
        /// <param name="titleFont">Title font</param>
        /// <param name="order">Order</param>
        /// <param name="font">Text font</param>
        /// <param name="addressTable">Address PDF table</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        protected virtual async Task PrintVendorPickupAddressInfoAsync(int vendorId, Language lang, Font titleFont, Order order, Font font, PdfPTable addressTable)
        {
            var vendorPickupAddressPdf = new PdfPTable(1)
            {
                RunDirection = GetDirection(lang)
            };
            vendorPickupAddressPdf.DefaultCell.Border = Rectangle.NO_BORDER;

            var orderItems = await _orderService.GetOrderItemsAsync(order.Id);
            if (orderItems.Any(x => x.InPickup))
            {
                const string indent = "   ";

                vendorPickupAddressPdf.AddCell(await GetParagraphAsync("Alchub.PDFInvoice.VendorPickupAddressInformation", lang, titleFont));
                //print pickup details for pickup vendors
                foreach (var orderItem in orderItems)
                {
                    if (!orderItem.InPickup)
                        continue;

                    var product = await _productService.GetProductByIdAsync(orderItem.ProductId);
                    if (product == null)
                        continue;

                    //vendor
                    var vendor = await _vendorService.GetVendorByIdAsync(product.VendorId);
                    if (vendor == null)
                        continue;

                    //vendor name
                    vendorPickupAddressPdf.AddCell(await GetParagraphAsync("Alchub.PDFInvoice.VendorPickupAddress.Common", indent, lang, font, vendor.Name));

                    //pickup address
                    vendorPickupAddressPdf.AddCell(await GetParagraphAsync("Alchub.PDFInvoice.VendorPickupAddress.Common", indent, lang, font, vendor.PickupAddress?.Replace(", USA", "")));//remove USA from store name - 14-06-23

                    vendorPickupAddressPdf.AddCell(new Paragraph(" "));
                    vendorPickupAddressPdf.AddCell(new Paragraph());
                }

                addressTable.AddCell(vendorPickupAddressPdf);
            }
            else
            {
                vendorPickupAddressPdf.AddCell(new Paragraph());
                addressTable.AddCell(vendorPickupAddressPdf);
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Print packaging slips to PDF
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="shipments">Shipments</param>
        /// <param name="languageId">Language identifier; 0 to use a language used when placing an order</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task PrintPackagingSlipsToPdfAsync(Stream stream, IList<Shipment> shipments, int languageId = 0)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (shipments == null)
                throw new ArgumentNullException(nameof(shipments));

            var pageSize = PageSize.A4;

            if (_pdfSettings.LetterPageSizeEnabled)
            {
                pageSize = PageSize.Letter;
            }

            var doc = new Document(pageSize);
            PdfWriter.GetInstance(doc, stream);
            doc.Open();

            //fonts
            var titleFont = GetFont();
            titleFont.SetStyle(Font.BOLD);
            titleFont.Color = BaseColor.Black;
            var font = GetFont();
            var attributesFont = GetFont();
            attributesFont.SetStyle(Font.ITALIC);

            var shipmentCount = shipments.Count;
            var shipmentNum = 0;

            foreach (var shipment in shipments)
            {
                var order = await _orderService.GetOrderByIdAsync(shipment.OrderId);

                var lang = await _languageService.GetLanguageByIdAsync(languageId == 0 ? order.CustomerLanguageId : languageId);
                if (lang == null || !lang.Published)
                    lang = await _workContext.GetWorkingLanguageAsync();

                var addressTable = new PdfPTable(1);
                if (lang.Rtl)
                    addressTable.RunDirection = PdfWriter.RUN_DIRECTION_RTL;
                addressTable.DefaultCell.Border = Rectangle.NO_BORDER;
                addressTable.WidthPercentage = 100f;

                addressTable.AddCell(await GetParagraphAsync("PDFPackagingSlip.Shipment", lang, titleFont, shipment.Id));
                addressTable.AddCell(await GetParagraphAsync("PDFPackagingSlip.Order", lang, titleFont, order.CustomOrderNumber));

                if (!order.PickupInStore)
                {
                    if (order.ShippingAddressId == null || await _addressService.GetAddressByIdAsync(order.ShippingAddressId.Value) is not Address shippingAddress)
                        throw new NopException($"Shipping is required, but address is not available. Order ID = {order.Id}");

                    if (_addressSettings.CompanyEnabled && !string.IsNullOrEmpty(shippingAddress.Company))
                        addressTable.AddCell(await GetParagraphAsync("PDFPackagingSlip.Company", lang, font, shippingAddress.Company));

                    addressTable.AddCell(await GetParagraphAsync("PDFPackagingSlip.Name", lang, font, shippingAddress.FirstName + " " + shippingAddress.LastName));
                    if (_addressSettings.PhoneEnabled)
                        addressTable.AddCell(await GetParagraphAsync("PDFPackagingSlip.Phone", lang, font, shippingAddress.PhoneNumber));
                    if (_addressSettings.StreetAddressEnabled)
                        addressTable.AddCell(await GetParagraphAsync("PDFPackagingSlip.Address", lang, font, shippingAddress.Address1));

                    if (_addressSettings.StreetAddress2Enabled && !string.IsNullOrEmpty(shippingAddress.Address2))
                        addressTable.AddCell(await GetParagraphAsync("PDFPackagingSlip.Address2", lang, font, shippingAddress.Address2));

                    if (_addressSettings.CityEnabled || _addressSettings.StateProvinceEnabled ||
                        _addressSettings.CountyEnabled || _addressSettings.ZipPostalCodeEnabled)
                    {
                        var addressLine = $"{shippingAddress.City}, " +
                            $"{(!string.IsNullOrEmpty(shippingAddress.County) ? $"{shippingAddress.County}, " : string.Empty)}" +
                            $"{(await _stateProvinceService.GetStateProvinceByAddressAsync(shippingAddress) is StateProvince stateProvince ? await _localizationService.GetLocalizedAsync(stateProvince, x => x.Name, lang.Id) : string.Empty)} " +
                            $"{shippingAddress.ZipPostalCode}";
                        addressTable.AddCell(new Paragraph(addressLine, font));
                    }

                    if (_addressSettings.CountryEnabled && await _countryService.GetCountryByAddressAsync(shippingAddress) is Country country)
                        addressTable.AddCell(new Paragraph(await _localizationService.GetLocalizedAsync(country, x => x.Name, lang.Id), font));

                    //custom attributes
                    var customShippingAddressAttributes = await _addressAttributeFormatter.FormatAttributesAsync(shippingAddress.CustomAttributes);
                    if (!string.IsNullOrEmpty(customShippingAddressAttributes))
                    {
                        addressTable.AddCell(new Paragraph(_htmlFormatter.ConvertHtmlToPlainText(customShippingAddressAttributes, true, true), font));
                    }
                }
                else
                    if (order.PickupAddressId.HasValue && await _addressService.GetAddressByIdAsync(order.PickupAddressId.Value) is Address pickupAddress)
                {
                    addressTable.AddCell(new Paragraph(await _localizationService.GetResourceAsync("PDFInvoice.Pickup", lang.Id), titleFont));

                    if (!string.IsNullOrEmpty(pickupAddress.Address1))
                        addressTable.AddCell(new Paragraph($"   {string.Format(await _localizationService.GetResourceAsync("PDFInvoice.Address", lang.Id), pickupAddress.Address1)}", font));

                    if (!string.IsNullOrEmpty(pickupAddress.City))
                        addressTable.AddCell(new Paragraph($"   {pickupAddress.City}", font));

                    if (!string.IsNullOrEmpty(pickupAddress.County))
                        addressTable.AddCell(new Paragraph($"   {pickupAddress.County}", font));

                    if (await _countryService.GetCountryByAddressAsync(pickupAddress) is Country country)
                        addressTable.AddCell(new Paragraph($"   {await _localizationService.GetLocalizedAsync(country, x => x.Name, lang.Id)}", font));

                    if (!string.IsNullOrEmpty(pickupAddress.ZipPostalCode))
                        addressTable.AddCell(new Paragraph($"   {pickupAddress.ZipPostalCode}", font));

                    addressTable.AddCell(new Paragraph(" "));
                }

                addressTable.AddCell(new Paragraph(" "));

                addressTable.AddCell(await GetParagraphAsync("PDFPackagingSlip.ShippingMethod", lang, font, order.ShippingMethod));
                addressTable.AddCell(new Paragraph(" "));
                doc.Add(addressTable);

                var productsTable = new PdfPTable(3) { WidthPercentage = 100f };
                if (lang.Rtl)
                {
                    productsTable.RunDirection = PdfWriter.RUN_DIRECTION_RTL;
                    productsTable.SetWidths(new[] { 20, 20, 60 });
                }
                else
                {
                    productsTable.SetWidths(new[] { 60, 20, 20 });
                }

                //product name
                var cell = await GetPdfCellAsync("PDFPackagingSlip.ProductName", lang, font);
                cell.BackgroundColor = BaseColor.LightGray;
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                productsTable.AddCell(cell);

                //SKU
                cell = await GetPdfCellAsync("PDFPackagingSlip.SKU", lang, font);
                cell.BackgroundColor = BaseColor.LightGray;
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                productsTable.AddCell(cell);

                //qty
                cell = await GetPdfCellAsync("PDFPackagingSlip.QTY", lang, font);
                cell.BackgroundColor = BaseColor.LightGray;
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                productsTable.AddCell(cell);

                foreach (var si in await _shipmentService.GetShipmentItemsByShipmentIdAsync(shipment.Id))
                {
                    var productAttribTable = new PdfPTable(1);
                    if (lang.Rtl)
                        productAttribTable.RunDirection = PdfWriter.RUN_DIRECTION_RTL;
                    productAttribTable.DefaultCell.Border = Rectangle.NO_BORDER;

                    //product name
                    var orderItem = await _orderService.GetOrderItemByIdAsync(si.OrderItemId);
                    if (orderItem == null)
                        continue;

                    var product = await _productService.GetProductByIdAsync(orderItem.ProductId);

                    var name = await _productService.GetProductItemName(product, orderItem); //++Alchub;
                    productAttribTable.AddCell(new Paragraph(name, font));
                    //attributes
                    if (!string.IsNullOrEmpty(orderItem.AttributeDescription))
                    {
                        var attributesParagraph = new Paragraph(_htmlFormatter.ConvertHtmlToPlainText(orderItem.AttributeDescription, true, true), attributesFont);
                        productAttribTable.AddCell(attributesParagraph);
                    }
                    //custom attributes ++Alchub
                    if (!string.IsNullOrEmpty(orderItem.CustomAttributesDescription))
                    {
                        var attributesParagraph = new Paragraph(_htmlFormatter.ConvertHtmlToPlainText(orderItem.CustomAttributesDescription, true, true), attributesFont);
                        productAttribTable.AddCell(attributesParagraph);
                    }

                    //rental info
                    if (product.IsRental)
                    {
                        var rentalStartDate = orderItem.RentalStartDateUtc.HasValue
                            ? _productService.FormatRentalDate(product, orderItem.RentalStartDateUtc.Value) : string.Empty;
                        var rentalEndDate = orderItem.RentalEndDateUtc.HasValue
                            ? _productService.FormatRentalDate(product, orderItem.RentalEndDateUtc.Value) : string.Empty;
                        var rentalInfo = string.Format(await _localizationService.GetResourceAsync("Order.Rental.FormattedDate"),
                            rentalStartDate, rentalEndDate);

                        var rentalInfoParagraph = new Paragraph(rentalInfo, attributesFont);
                        productAttribTable.AddCell(rentalInfoParagraph);
                    }

                    productsTable.AddCell(productAttribTable);

                    //SKU
                    //var sku = await _productService.FormatSkuAsync(product, orderItem.AttributesXml);
                    //Note:(10-12-22) Now UPCCODE is SKU
                    var sku = product.UPCCode;
                    cell = GetPdfCell(sku ?? string.Empty, font);
                    cell.HorizontalAlignment = Element.ALIGN_CENTER;
                    productsTable.AddCell(cell);

                    //qty
                    cell = GetPdfCell(si.Quantity, font);
                    cell.HorizontalAlignment = Element.ALIGN_CENTER;
                    productsTable.AddCell(cell);
                }

                doc.Add(productsTable);

                shipmentNum++;
                if (shipmentNum < shipmentCount)
                    doc.NewPage();
            }

            doc.Close();
        }

        /// <summary>
        /// Print products to PDF
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="products">Products</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task PrintProductsToPdfAsync(Stream stream, IList<Product> products)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (products == null)
                throw new ArgumentNullException(nameof(products));

            var lang = await _workContext.GetWorkingLanguageAsync();

            var pageSize = PageSize.A4;

            if (_pdfSettings.LetterPageSizeEnabled)
            {
                pageSize = PageSize.Letter;
            }

            var doc = new Document(pageSize);
            PdfWriter.GetInstance(doc, stream);
            doc.Open();

            //fonts
            var titleFont = GetFont();
            titleFont.SetStyle(Font.BOLD);
            titleFont.Color = BaseColor.Black;
            var font = GetFont();

            var productNumber = 1;
            var prodCount = products.Count;

            foreach (var product in products)
            {
                var productName = await _localizationService.GetLocalizedAsync(product, x => x.Name, lang.Id);
                var productDescription = await _localizationService.GetLocalizedAsync(product, x => x.FullDescription, lang.Id);

                var productTable = new PdfPTable(1) { WidthPercentage = 100f };
                productTable.DefaultCell.Border = Rectangle.NO_BORDER;
                if (lang.Rtl)
                {
                    productTable.RunDirection = PdfWriter.RUN_DIRECTION_RTL;
                }

                productTable.AddCell(new Paragraph($"{productNumber}. {productName}", titleFont));
                productTable.AddCell(new Paragraph(" "));
                productTable.AddCell(new Paragraph(_htmlFormatter.StripTags(_htmlFormatter.ConvertHtmlToPlainText(productDescription, decode: true)), font));
                productTable.AddCell(new Paragraph(" "));

                if (product.ProductType == ProductType.SimpleProduct)
                {
                    //simple product
                    //render its properties such as price, weight, etc
                    var priceStr = $"{product.Price:0.00} {(await _currencyService.GetCurrencyByIdAsync(_currencySettings.PrimaryStoreCurrencyId)).CurrencyCode}";
                    if (product.IsRental)
                        priceStr = await _priceFormatter.FormatRentalProductPeriodAsync(product, priceStr);
                    productTable.AddCell(new Paragraph($"{await _localizationService.GetResourceAsync("PDFProductCatalog.Price", lang.Id)}: {priceStr}", font));
                    //Note:(10-12-22) Now UPCCODE is SKU
                    productTable.AddCell(new Paragraph($"{await _localizationService.GetResourceAsync("PDFProductCatalog.SKU", lang.Id)}: {product.UPCCode}", font));

                    if (product.IsShipEnabled && product.Weight > decimal.Zero)
                        productTable.AddCell(new Paragraph($"{await _localizationService.GetResourceAsync("PDFProductCatalog.Weight", lang.Id)}: {product.Weight:0.00} {(await _measureService.GetMeasureWeightByIdAsync(_measureSettings.BaseWeightId)).Name}", font));

                    if (product.ManageInventoryMethod == ManageInventoryMethod.ManageStock)
                        productTable.AddCell(new Paragraph($"{await _localizationService.GetResourceAsync("PDFProductCatalog.StockQuantity", lang.Id)}: {await _productService.GetTotalStockQuantityAsync(product)}", font));

                    productTable.AddCell(new Paragraph(" "));
                }

                var pictures = await _pictureService.GetPicturesByProductIdAsync(product.Id);
                if (pictures.Any())
                {
                    var table = new PdfPTable(2) { WidthPercentage = 100f };
                    if (lang.Rtl)
                        table.RunDirection = PdfWriter.RUN_DIRECTION_RTL;

                    foreach (var pic in pictures)
                    {
                        var picBinary = await _pictureService.LoadPictureBinaryAsync(pic);
                        if (picBinary == null || picBinary.Length <= 0)
                            continue;

                        var pictureLocalPath = await _pictureService.GetThumbLocalPathAsync(pic, 200, false);
                        var cell = new PdfPCell(Image.GetInstance(pictureLocalPath))
                        {
                            HorizontalAlignment = Element.ALIGN_LEFT,
                            Border = Rectangle.NO_BORDER
                        };
                        table.AddCell(cell);
                    }

                    if (pictures.Count % 2 > 0)
                    {
                        var cell = new PdfPCell(new Phrase(" "))
                        {
                            Border = Rectangle.NO_BORDER
                        };
                        table.AddCell(cell);
                    }

                    productTable.AddCell(table);
                    productTable.AddCell(new Paragraph(" "));
                }

                if (product.ProductType == ProductType.GroupedProduct)
                {
                    //grouped product. render its associated products
                    var pvNum = 1;
                    foreach (var associatedProduct in await _productService.GetAssociatedProductsAsync(product.Id, showHidden: true))
                    {
                        productTable.AddCell(new Paragraph($"{productNumber}-{pvNum}. {await _localizationService.GetLocalizedAsync(associatedProduct, x => x.Name, lang.Id)}", font));
                        productTable.AddCell(new Paragraph(" "));

                        //uncomment to render associated product description
                        //string apDescription = associated_localizationService.GetLocalized(product, x => x.ShortDescription, lang.Id);
                        //if (!string.IsNullOrEmpty(apDescription))
                        //{
                        //    productTable.AddCell(new Paragraph(_htmlHelper.StripTags(_htmlHelper.ConvertHtmlToPlainText(apDescription)), font));
                        //    productTable.AddCell(new Paragraph(" "));
                        //}

                        //uncomment to render associated product picture
                        //var apPicture = _pictureService.GetPicturesByProductId(associatedProduct.Id).FirstOrDefault();
                        //if (apPicture != null)
                        //{
                        //    var picBinary = _pictureService.LoadPictureBinary(apPicture);
                        //    if (picBinary != null && picBinary.Length > 0)
                        //    {
                        //        var pictureLocalPath = _pictureService.GetThumbLocalPath(apPicture, 200, false);
                        //        productTable.AddCell(Image.GetInstance(pictureLocalPath));
                        //    }
                        //}

                        productTable.AddCell(new Paragraph($"{await _localizationService.GetResourceAsync("PDFProductCatalog.Price", lang.Id)}: {associatedProduct.Price:0.00} {(await _currencyService.GetCurrencyByIdAsync(_currencySettings.PrimaryStoreCurrencyId)).CurrencyCode}", font));
                        //Note:(10-12-22) Now UPCCODE is SKU
                        productTable.AddCell(new Paragraph($"{await _localizationService.GetResourceAsync("PDFProductCatalog.SKU", lang.Id)}: {associatedProduct.UPCCode}", font));

                        if (associatedProduct.IsShipEnabled && associatedProduct.Weight > decimal.Zero)
                            productTable.AddCell(new Paragraph($"{await _localizationService.GetResourceAsync("PDFProductCatalog.Weight", lang.Id)}: {associatedProduct.Weight:0.00} {(await _measureService.GetMeasureWeightByIdAsync(_measureSettings.BaseWeightId)).Name}", font));

                        if (associatedProduct.ManageInventoryMethod == ManageInventoryMethod.ManageStock)
                            productTable.AddCell(new Paragraph($"{await _localizationService.GetResourceAsync("PDFProductCatalog.StockQuantity", lang.Id)}: {await _productService.GetTotalStockQuantityAsync(associatedProduct)}", font));

                        productTable.AddCell(new Paragraph(" "));

                        pvNum++;
                    }
                }

                doc.Add(productTable);

                productNumber++;

                if (productNumber <= prodCount)
                {
                    doc.NewPage();
                }
            }

            doc.Close();
        }

        #endregion
    }
}
