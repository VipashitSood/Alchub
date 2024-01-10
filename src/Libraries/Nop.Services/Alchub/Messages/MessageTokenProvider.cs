using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Nop.Core;
using Nop.Core.Domain;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Messages;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Tax;
using Nop.Core.Domain.Vendors;
using Nop.Core.Events;
using Nop.Services.Alchub.Slots;
using Nop.Services.Blogs;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Helpers;
using Nop.Services.Html;
using Nop.Services.Localization;
using Nop.Services.News;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Seo;
using Nop.Services.Shipping;
using Nop.Services.Stores;
using Nop.Services.Vendors;

namespace Nop.Services.Messages
{
    /// <summary>
    /// Represents a message token provider
    /// </summary>
    public partial class MessageTokenProvider : IMessageTokenProvider
    {
        #region Fields

        private readonly CatalogSettings _catalogSettings;
        private readonly CurrencySettings _currencySettings;
        private readonly IActionContextAccessor _actionContextAccessor;
        private readonly IAddressAttributeFormatter _addressAttributeFormatter;
        private readonly IAddressService _addressService;
        private readonly IBlogService _blogService;
        private readonly ICountryService _countryService;
        private readonly ICurrencyService _currencyService;
        private readonly ICustomerAttributeFormatter _customerAttributeFormatter;
        private readonly ICustomerService _customerService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IEventPublisher _eventPublisher;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IGiftCardService _giftCardService;
        private readonly IHtmlFormatter _htmlFormatter;
        private readonly ILanguageService _languageService;
        private readonly ILocalizationService _localizationService;
        private readonly INewsService _newsService;
        private readonly IOrderService _orderService;
        private readonly IPaymentPluginManager _paymentPluginManager;
        private readonly IPaymentService _paymentService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IProductService _productService;
        private readonly IRewardPointService _rewardPointService;
        private readonly IShipmentService _shipmentService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly IStoreContext _storeContext;
        private readonly IStoreService _storeService;
        private readonly IUrlHelperFactory _urlHelperFactory;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IVendorAttributeFormatter _vendorAttributeFormatter;
        private readonly IWorkContext _workContext;
        private readonly MessageTemplatesSettings _templatesSettings;
        private readonly PaymentSettings _paymentSettings;
        private readonly StoreInformationSettings _storeInformationSettings;
        private readonly TaxSettings _taxSettings;
        private readonly IVendorService _vendorService;
        private Dictionary<string, IEnumerable<string>> _allowedTokens;

        #endregion

        #region Ctor

        public MessageTokenProvider(CatalogSettings catalogSettings,
            CurrencySettings currencySettings,
            IActionContextAccessor actionContextAccessor,
            IAddressAttributeFormatter addressAttributeFormatter,
            IAddressService addressService,
            IBlogService blogService,
            ICountryService countryService,
            ICurrencyService currencyService,
            ICustomerAttributeFormatter customerAttributeFormatter,
            ICustomerService customerService,
            IDateTimeHelper dateTimeHelper,
            IEventPublisher eventPublisher,
            IGenericAttributeService genericAttributeService,
            IGiftCardService giftCardService,
            IHtmlFormatter htmlFormatter,
            ILanguageService languageService,
            ILocalizationService localizationService,
            INewsService newsService,
            IOrderService orderService,
            IPaymentPluginManager paymentPluginManager,
            IPaymentService paymentService,
            IPriceFormatter priceFormatter,
            IProductService productService,
            IRewardPointService rewardPointService,
            IShipmentService shipmentService,
            IStateProvinceService stateProvinceService,
            IStoreContext storeContext,
            IStoreService storeService,
            IUrlHelperFactory urlHelperFactory,
            IUrlRecordService urlRecordService,
            IVendorAttributeFormatter vendorAttributeFormatter,
            IWorkContext workContext,
            MessageTemplatesSettings templatesSettings,
            PaymentSettings paymentSettings,
            StoreInformationSettings storeInformationSettings,
            TaxSettings taxSettings,
            IVendorService vendorService)
        {
            _catalogSettings = catalogSettings;
            _currencySettings = currencySettings;
            _actionContextAccessor = actionContextAccessor;
            _addressAttributeFormatter = addressAttributeFormatter;
            _addressService = addressService;
            _blogService = blogService;
            _countryService = countryService;
            _currencyService = currencyService;
            _customerAttributeFormatter = customerAttributeFormatter;
            _customerService = customerService;
            _dateTimeHelper = dateTimeHelper;
            _eventPublisher = eventPublisher;
            _genericAttributeService = genericAttributeService;
            _giftCardService = giftCardService;
            _htmlFormatter = htmlFormatter;
            _languageService = languageService;
            _localizationService = localizationService;
            _newsService = newsService;
            _orderService = orderService;
            _paymentPluginManager = paymentPluginManager;
            _paymentService = paymentService;
            _priceFormatter = priceFormatter;
            _productService = productService;
            _rewardPointService = rewardPointService;
            _shipmentService = shipmentService;
            _stateProvinceService = stateProvinceService;
            _storeContext = storeContext;
            _storeService = storeService;
            _urlHelperFactory = urlHelperFactory;
            _urlRecordService = urlRecordService;
            _vendorAttributeFormatter = vendorAttributeFormatter;
            _workContext = workContext;
            _templatesSettings = templatesSettings;
            _paymentSettings = paymentSettings;
            _storeInformationSettings = storeInformationSettings;
            _taxSettings = taxSettings;
            _vendorService = vendorService;
        }

        #endregion

        #region Utilities

        protected virtual async Task<string> ProductListToHtmlTableAsync(Order order, int languageId, int vendorId)
        {
            var language = await _languageService.GetLanguageByIdAsync(languageId);

            var sb = new StringBuilder();
            sb.AppendLine("<table border=\"0\" style=\"width:100%;\">");

            sb.AppendLine($"<tr style=\"background-color:{_templatesSettings.Color1};text-align:center;\">");
            sb.AppendLine($"<th>{await _localizationService.GetResourceAsync("Messages.Order.Product(s).Name", languageId)}</th>");
            sb.AppendLine($"<th>{await _localizationService.GetResourceAsync("Messages.Order.Product(s).Price", languageId)}</th>");
            sb.AppendLine($"<th>{await _localizationService.GetResourceAsync("Messages.Order.Product(s).Quantity", languageId)}</th>");
            sb.AppendLine($"<th>{await _localizationService.GetResourceAsync("Messages.Order.Product(s).SlotDateTime", languageId)}</th>");
            sb.AppendLine($"<th>{await _localizationService.GetResourceAsync("Messages.Order.Product(s).Total", languageId)}</th>");
            sb.AppendLine("</tr>");

            var table = await _orderService.GetOrderItemsAsync(order.Id, vendorId: vendorId);
            for (var i = 0; i <= table.Count - 1; i++)
            {
                var orderItem = table[i];

                var product = await _productService.GetProductByIdAsync(orderItem.ProductId);

                if (product == null)
                    continue;

                sb.AppendLine($"<tr style=\"background-color: {_templatesSettings.Color2};text-align: center;\">");
                //product name
                var productName = await _productService.GetProductItemName(product, orderItem); //++Alchub;

                sb.AppendLine("<td style=\"padding: 0.6em 0.4em;text-align: left;\">" + WebUtility.HtmlEncode(productName));

                //add download link
                if (await _orderService.IsDownloadAllowedAsync(orderItem))
                {
                    var downloadUrl = await RouteUrlAsync(order.StoreId, "GetDownload", new { orderItemId = orderItem.OrderItemGuid });
                    var downloadLink = $"<a class=\"link\" href=\"{downloadUrl}\">{await _localizationService.GetResourceAsync("Messages.Order.Product(s).Download", languageId)}</a>";
                    sb.AppendLine("<br />");
                    sb.AppendLine(downloadLink);
                }
                //add download link
                if (await _orderService.IsLicenseDownloadAllowedAsync(orderItem))
                {
                    var licenseUrl = await RouteUrlAsync(order.StoreId, "GetLicense", new { orderItemId = orderItem.OrderItemGuid });
                    var licenseLink = $"<a class=\"link\" href=\"{licenseUrl}\">{await _localizationService.GetResourceAsync("Messages.Order.Product(s).License", languageId)}</a>";
                    sb.AppendLine("<br />");
                    sb.AppendLine(licenseLink);
                }
                //attributes
                if (!string.IsNullOrEmpty(orderItem.AttributeDescription))
                {
                    sb.AppendLine("<br />");
                    sb.AppendLine(orderItem.AttributeDescription);
                }
                //custom attributes
                if (!string.IsNullOrEmpty(orderItem.CustomAttributesDescription))
                {
                    sb.AppendLine("<br />");
                    sb.AppendLine(orderItem.CustomAttributesDescription);
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
                    sb.AppendLine("<br />");
                    sb.AppendLine(rentalInfo);
                }
                //SKU
                if (_catalogSettings.ShowSkuOnProductDetailsPage)
                {
                    //var sku = await _productService.FormatSkuAsync(product, orderItem.AttributesXml);
                    var sku = product.UPCCode;
                    if (!string.IsNullOrEmpty(sku))
                    {
                        sb.AppendLine("<br />");
                        sb.AppendLine(string.Format(await _localizationService.GetResourceAsync("Messages.Order.Product(s).SKU", languageId), WebUtility.HtmlEncode(sku)));
                    }
                }
                //++Alchub++
                //Vendor name
                if (product.VendorId > 0)
                {
                    var vendorName = (await _vendorService.GetVendorByIdAsync(product.VendorId))?.Name;
                    if (!string.IsNullOrEmpty(vendorName))
                    {
                        sb.AppendLine("<br />");
                        sb.AppendLine(string.Format(await _localizationService.GetResourceAsync("Alchub.Messages.Order.Product(s).Vendor", languageId), WebUtility.HtmlEncode(vendorName)));
                    }
                }
                //--Alchub--

                sb.AppendLine("</td>");

                string unitPriceStr;
                if (order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax)
                {
                    //including tax
                    var unitPriceInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(orderItem.UnitPriceInclTax, order.CurrencyRate);
                    unitPriceStr = await _priceFormatter.FormatPriceAsync(unitPriceInclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, languageId, true);
                }
                else
                {
                    //excluding tax
                    var unitPriceExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(orderItem.UnitPriceExclTax, order.CurrencyRate);
                    unitPriceStr = await _priceFormatter.FormatPriceAsync(unitPriceExclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, languageId, false);
                }

                sb.AppendLine($"<td style=\"padding: 0.6em 0.4em;text-align: right;\">{unitPriceStr}</td>");

                sb.AppendLine($"<td style=\"padding: 0.6em 0.4em;text-align: center;\">{orderItem.Quantity}</td>");

                /*Alchub Start*/
                sb.AppendLine($"<td style=\"padding: 0.6em 0.4em;text-align: center;\">{(orderItem.InPickup ? await _localizationService.GetResourceAsync("Alchub.ShoppingCart.InPickup.Text") : await _localizationService.GetResourceAsync("Alchub.ShoppingCart.Delivery.Text")) + orderItem.SlotStartTime.ToString("MM/dd/yyyy") + " " + SlotHelper.ConvertTo12hoursSlotTime(orderItem.SlotTime)}</td>");
                /*Alchub End*/

                string priceStr;
                if (order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax)
                {
                    //including tax
                    var priceInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(orderItem.PriceInclTax, order.CurrencyRate);
                    priceStr = await _priceFormatter.FormatPriceAsync(priceInclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, languageId, true);
                }
                else
                {
                    //excluding tax
                    var priceExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(orderItem.PriceExclTax, order.CurrencyRate);
                    priceStr = await _priceFormatter.FormatPriceAsync(priceExclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, languageId, false);
                }

                sb.AppendLine($"<td style=\"padding: 0.6em 0.4em;text-align: right;\">{priceStr}</td>");

                sb.AppendLine("</tr>");
            }

            if (vendorId == 0)
            {
                //we render checkout attributes and totals only for store owners (hide for vendors)

                if (!string.IsNullOrEmpty(order.CheckoutAttributeDescription))
                {
                    sb.AppendLine("<tr><td style=\"text-align:right;\" colspan=\"1\">&nbsp;</td><td colspan=\"3\" style=\"text-align:right\">");
                    sb.AppendLine(order.CheckoutAttributeDescription);
                    sb.AppendLine("</td></tr>");
                }

                //totals
                await WriteTotalsAsync(order, language, sb);
            }

            sb.AppendLine("</table>");
            var result = sb.ToString();
            return result;
        }

        /// <summary>
        /// Convert a collection to a HTML table
        /// </summary>
        /// <param name="shipment">Shipment</param>
        /// <param name="languageId">Language identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the hTML table of products
        /// </returns>
        protected virtual async Task<string> ProductListToHtmlTableAsync(Shipment shipment, int languageId)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<table border=\"0\" style=\"width:100%;\">");

            sb.AppendLine($"<tr style=\"background-color:{_templatesSettings.Color1};text-align:center;\">");
            sb.AppendLine($"<th>{await _localizationService.GetResourceAsync("Messages.Order.Product(s).Name", languageId)}</th>");
            sb.AppendLine($"<th>{await _localizationService.GetResourceAsync("Messages.Order.Product(s).Quantity", languageId)}</th>");
            sb.AppendLine("</tr>");

            var table = await _shipmentService.GetShipmentItemsByShipmentIdAsync(shipment.Id);
            for (var i = 0; i <= table.Count - 1; i++)
            {
                var si = table[i];
                var orderItem = await _orderService.GetOrderItemByIdAsync(si.OrderItemId);

                if (orderItem == null)
                    continue;

                var product = await _productService.GetProductByIdAsync(orderItem.ProductId);

                if (product == null)
                    continue;

                sb.AppendLine($"<tr style=\"background-color: {_templatesSettings.Color2};text-align: center;\">");
                //product name
                var productName = await _productService.GetProductItemName(product, orderItem); //++Alchub;

                sb.AppendLine("<td style=\"padding: 0.6em 0.4em;text-align: left;\">" + WebUtility.HtmlEncode(productName));

                //attributes
                if (!string.IsNullOrEmpty(orderItem.AttributeDescription))
                {
                    sb.AppendLine("<br />");
                    sb.AppendLine(orderItem.AttributeDescription);
                }
                //custom attributes ++Alchub
                if (!string.IsNullOrEmpty(orderItem.CustomAttributesDescription))
                {
                    sb.AppendLine("<br />");
                    sb.AppendLine(orderItem.CustomAttributesDescription);
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
                    sb.AppendLine("<br />");
                    sb.AppendLine(rentalInfo);
                }

                //SKU
                if (_catalogSettings.ShowSkuOnProductDetailsPage)
                {
                    //var sku = await _productService.FormatSkuAsync(product, orderItem.AttributesXml);
                    var sku = product.UPCCode;
                    if (!string.IsNullOrEmpty(sku))
                    {
                        sb.AppendLine("<br />");
                        sb.AppendLine(string.Format(await _localizationService.GetResourceAsync("Messages.Order.Product(s).SKU", languageId), WebUtility.HtmlEncode(sku)));
                    }
                }

                sb.AppendLine("</td>");

                sb.AppendLine($"<td style=\"padding: 0.6em 0.4em;text-align: center;\">{si.Quantity}</td>");

                sb.AppendLine("</tr>");
            }

            sb.AppendLine("</table>");
            var result = sb.ToString();
            return result;
        }

        /// <summary>
        /// Convert a collection to a HTML table
        /// </summary>
        /// <param name="orderItems">orderItems</param>
        /// <param name="languageId">Language identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the hTML table of products
        /// </returns>
        protected virtual async Task<string> ProductListToHtmlTableAsync(Order order, IList<OrderItem> orderItems, int languageId)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<table border=\"0\" style=\"width:100%;\">");

            sb.AppendLine($"<tr style=\"background-color:{_templatesSettings.Color1};text-align:center;\">");
            sb.AppendLine($"<th>{await _localizationService.GetResourceAsync("Messages.Order.Product(s).Name", languageId)}</th>");
            sb.AppendLine($"<th>{await _localizationService.GetResourceAsync("Messages.Order.Product(s).Price", languageId)}</th>");
            sb.AppendLine($"<th>{await _localizationService.GetResourceAsync("Messages.Order.Product(s).Quantity", languageId)}</th>");
            sb.AppendLine($"<th>{await _localizationService.GetResourceAsync("Messages.Order.Product(s).SlotDateTime", languageId)}</th>");
            sb.AppendLine($"<th>{await _localizationService.GetResourceAsync("Messages.Order.Product(s).Total", languageId)}</th>");
            sb.AppendLine("</tr>");

            var table = orderItems;
            for (var i = 0; i <= table.Count - 1; i++)
            {
                var orderItem = table[i];
                if (orderItem == null)
                    continue;

                var product = await _productService.GetProductByIdAsync(orderItem.ProductId);

                if (product == null)
                    continue;

                sb.AppendLine($"<tr style=\"background-color: {_templatesSettings.Color2};text-align: center;\">");
                //product name
                var productName = await _productService.GetProductItemName(product, orderItem); //++Alchub;

                sb.AppendLine("<td style=\"padding: 0.6em 0.4em;text-align: left;\">" + WebUtility.HtmlEncode(productName));

                //attributes
                if (!string.IsNullOrEmpty(orderItem.AttributeDescription))
                {
                    sb.AppendLine("<br />");
                    sb.AppendLine(orderItem.AttributeDescription);
                }
                //custom attributes ++Alchub
                if (!string.IsNullOrEmpty(orderItem.CustomAttributesDescription))
                {
                    sb.AppendLine("<br />");
                    sb.AppendLine(orderItem.CustomAttributesDescription);
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
                    sb.AppendLine("<br />");
                    sb.AppendLine(rentalInfo);
                }

                //SKU
                if (_catalogSettings.ShowSkuOnProductDetailsPage)
                {
                    //var sku = await _productService.FormatSkuAsync(product, orderItem.AttributesXml);
                    var sku = product.UPCCode;
                    if (!string.IsNullOrEmpty(sku))
                    {
                        sb.AppendLine("<br />");
                        sb.AppendLine(string.Format(await _localizationService.GetResourceAsync("Messages.Order.Product(s).SKU", languageId), WebUtility.HtmlEncode(sku)));
                    }
                }

                sb.AppendLine("</td>");

                string unitPriceStr;
                if (order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax)
                {
                    //including tax
                    var unitPriceInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(orderItem.UnitPriceInclTax, order.CurrencyRate);
                    unitPriceStr = await _priceFormatter.FormatPriceAsync(unitPriceInclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, languageId, true);
                }
                else
                {
                    //excluding tax
                    var unitPriceExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(orderItem.UnitPriceExclTax, order.CurrencyRate);
                    unitPriceStr = await _priceFormatter.FormatPriceAsync(unitPriceExclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, languageId, false);
                }

                sb.AppendLine($"<td style=\"padding: 0.6em 0.4em;text-align: right;\">{unitPriceStr}</td>");

                sb.AppendLine($"<td style=\"padding: 0.6em 0.4em;text-align: center;\">{orderItem.Quantity}</td>");

                /*Alchub Start*/
                sb.AppendLine($"<td style=\"padding: 0.6em 0.4em;text-align: center;\">{(orderItem.InPickup ? await _localizationService.GetResourceAsync("Alchub.ShoppingCart.InPickup.Text") : await _localizationService.GetResourceAsync("Alchub.ShoppingCart.Delivery.Text")) + orderItem.SlotStartTime.ToString("MM/dd/yyyy") + " " + SlotHelper.ConvertTo12hoursSlotTime(orderItem.SlotTime)}</td>");
                /*Alchub End*/

                string priceStr;
                if (order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax)
                {
                    //including tax
                    var priceInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(orderItem.PriceInclTax, order.CurrencyRate);
                    priceStr = await _priceFormatter.FormatPriceAsync(priceInclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, languageId, true);
                }
                else
                {
                    //excluding tax
                    var priceExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(orderItem.PriceExclTax, order.CurrencyRate);
                    priceStr = await _priceFormatter.FormatPriceAsync(priceExclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, languageId, false);
                }

                sb.AppendLine($"<td style=\"padding: 0.6em 0.4em;text-align: right;\">{priceStr}</td>");

                sb.AppendLine("</tr>");
            }

            sb.AppendLine("</table>");
            var result = sb.ToString();
            return result;
        }

        /// <summary>
        /// Write order totals
        /// </summary>
        /// <param name="order">Order</param>
        /// <param name="language">Language</param>
        /// <param name="sb">StringBuilder</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        protected virtual async Task WriteTotalsAsync(Order order, Language language, StringBuilder sb)
        {
            //subtotal
            string cusSubTotal;
            var displaySubTotalDiscount = false;
            var cusSubTotalDiscount = string.Empty;
            var languageId = language.Id;
            if (order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax && !_taxSettings.ForceTaxExclusionFromOrderSubtotal)
            {
                //including tax

                //subtotal
                var orderSubtotalInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderSubtotalInclTax, order.CurrencyRate);
                cusSubTotal = await _priceFormatter.FormatPriceAsync(orderSubtotalInclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, languageId, true);
                //discount (applied to order subtotal)
                var orderSubTotalDiscountInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderSubTotalDiscountInclTax, order.CurrencyRate);
                if (orderSubTotalDiscountInclTaxInCustomerCurrency > decimal.Zero)
                {
                    cusSubTotalDiscount = await _priceFormatter.FormatPriceAsync(-orderSubTotalDiscountInclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, languageId, true);
                    displaySubTotalDiscount = true;
                }
            }
            else
            {
                //excluding tax

                //subtotal
                var orderSubtotalExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderSubtotalExclTax, order.CurrencyRate);
                cusSubTotal = await _priceFormatter.FormatPriceAsync(orderSubtotalExclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, languageId, false);
                //discount (applied to order subtotal)
                var orderSubTotalDiscountExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderSubTotalDiscountExclTax, order.CurrencyRate);
                if (orderSubTotalDiscountExclTaxInCustomerCurrency > decimal.Zero)
                {
                    cusSubTotalDiscount = await _priceFormatter.FormatPriceAsync(-orderSubTotalDiscountExclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, languageId, false);
                    displaySubTotalDiscount = true;
                }
            }

            //shipping, payment method fee
            string cusShipTotal;
            string cusPaymentMethodAdditionalFee;
            var taxRates = new SortedDictionary<decimal, decimal>();
            var cusTaxTotal = string.Empty;
            var cusDiscount = string.Empty;
            if (order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax)
            {
                //including tax

                //shipping
                var orderShippingInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderShippingInclTax, order.CurrencyRate);
                cusShipTotal = await _priceFormatter.FormatShippingPriceAsync(orderShippingInclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, languageId, true);
                //payment method additional fee
                var paymentMethodAdditionalFeeInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.PaymentMethodAdditionalFeeInclTax, order.CurrencyRate);
                cusPaymentMethodAdditionalFee = await _priceFormatter.FormatPaymentMethodAdditionalFeeAsync(paymentMethodAdditionalFeeInclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, languageId, true);
            }
            else
            {
                //excluding tax

                //shipping
                var orderShippingExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderShippingExclTax, order.CurrencyRate);
                cusShipTotal = await _priceFormatter.FormatShippingPriceAsync(orderShippingExclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, languageId, false);
                //payment method additional fee
                var paymentMethodAdditionalFeeExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.PaymentMethodAdditionalFeeExclTax, order.CurrencyRate);
                cusPaymentMethodAdditionalFee = await _priceFormatter.FormatPaymentMethodAdditionalFeeAsync(paymentMethodAdditionalFeeExclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, languageId, false);
            }

            //shipping
            //++Alchub
            //by default do not show shipping address.
            bool showShipping = false;
            //--Alchub
            var displayShipping = order.ShippingStatus != ShippingStatus.ShippingNotRequired && showShipping;

            //payment method fee
            var displayPaymentMethodFee = order.PaymentMethodAdditionalFeeExclTax > decimal.Zero;

            //tax
            bool displayTax;
            bool displayTaxRates;
            if (_taxSettings.HideTaxInOrderSummary && order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax)
            {
                displayTax = false;
                displayTaxRates = false;
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
                    taxRates = new SortedDictionary<decimal, decimal>();
                    foreach (var tr in _orderService.ParseTaxRates(order, order.TaxRates))
                        taxRates.Add(tr.Key, _currencyService.ConvertCurrency(tr.Value, order.CurrencyRate));

                    displayTaxRates = _taxSettings.DisplayTaxRates && taxRates.Any();
                    displayTax = !displayTaxRates;

                    var orderTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderTax, order.CurrencyRate);
                    var taxStr = await _priceFormatter.FormatPriceAsync(orderTaxInCustomerCurrency, true, order.CustomerCurrencyCode,
                        false, languageId);
                    cusTaxTotal = taxStr;
                }
            }

            //discount
            var displayDiscount = false;
            if (order.OrderDiscount > decimal.Zero)
            {
                var orderDiscountInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderDiscount, order.CurrencyRate);
                cusDiscount = await _priceFormatter.FormatPriceAsync(-orderDiscountInCustomerCurrency, true, order.CustomerCurrencyCode, false, languageId);
                displayDiscount = true;
            }

            //total
            var orderTotalInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderTotal, order.CurrencyRate);
            var cusTotal = await _priceFormatter.FormatPriceAsync(orderTotalInCustomerCurrency, true, order.CustomerCurrencyCode, false, languageId);

            //common colspan for order totals
            var colSpan = 3;

            //subtotal
            sb.AppendLine($"<tr style=\"text-align:right;\"><td>&nbsp;</td><td colspan=\"{colSpan}\" style=\"background-color: {_templatesSettings.Color3};padding:0.6em 0.4 em;\"><strong>{await _localizationService.GetResourceAsync("Messages.Order.SubTotal", languageId)}</strong></td> <td style=\"background-color: {_templatesSettings.Color3};padding:0.6em 0.4 em;\"><strong>{cusSubTotal}</strong></td></tr>");

            //discount (applied to order subtotal)
            if (displaySubTotalDiscount)
            {
                sb.AppendLine($"<tr style=\"text-align:right;\"><td>&nbsp;</td><td colspan=\"{colSpan}\" style=\"background-color: {_templatesSettings.Color3};padding:0.6em 0.4 em;\"><strong>{await _localizationService.GetResourceAsync("Messages.Order.SubTotalDiscount", languageId)}</strong></td> <td style=\"background-color: {_templatesSettings.Color3};padding:0.6em 0.4 em;\"><strong>{cusSubTotalDiscount}</strong></td></tr>");
            }

            //shipping
            if (displayShipping)
            {
                sb.AppendLine($"<tr style=\"text-align:right;\"><td>&nbsp;</td><td colspan=\"{colSpan}\" style=\"background-color: {_templatesSettings.Color3};padding:0.6em 0.4 em;\"><strong>{await _localizationService.GetResourceAsync("Messages.Order.Shipping", languageId)}</strong></td> <td style=\"background-color: {_templatesSettings.Color3};padding:0.6em 0.4 em;\"><strong>{cusShipTotal}</strong></td></tr>");
            }

            /*Alchub Start*/
            //delivery fee
            var deliveryFeeInCustomerCurrency = _currencyService.ConvertCurrency(order.DeliveryFee, order.CurrencyRate);
            if (deliveryFeeInCustomerCurrency > 0)
            {
                var deliveryFeeTotal = await _priceFormatter.FormatPriceAsync(deliveryFeeInCustomerCurrency, true, order.CustomerCurrencyCode, false, languageId);
                sb.AppendLine($"<tr style=\"text-align:right;\"><td>&nbsp;</td><td colspan=\"{colSpan}\" style=\"background-color: {_templatesSettings.Color3};padding:0.6em 0.4 em;\"><strong>{await _localizationService.GetResourceAsync("Alchub.Messages.Order.DeliveryFee", languageId)}</strong></td> <td style=\"background-color: {_templatesSettings.Color3};padding:0.6em 0.4 em;\"><strong>{deliveryFeeTotal}</strong></td></tr>");
            }
            /*Alchub End*/

            //payment method fee
            if (displayPaymentMethodFee)
            {
                var paymentMethodFeeTitle = await _localizationService.GetResourceAsync("Messages.Order.PaymentMethodAdditionalFee", languageId);
                sb.AppendLine($"<tr style=\"text-align:right;\"><td>&nbsp;</td><td colspan=\"{colSpan}\" style=\"background-color: {_templatesSettings.Color3};padding:0.6em 0.4 em;\"><strong>{paymentMethodFeeTitle}</strong></td> <td style=\"background-color: {_templatesSettings.Color3};padding:0.6em 0.4 em;\"><strong>{cusPaymentMethodAdditionalFee}</strong></td></tr>");
            }

            //tax
            if (displayTax)
            {
                sb.AppendLine($"<tr style=\"text-align:right;\"><td>&nbsp;</td><td colspan=\"{colSpan}\" style=\"background-color: {_templatesSettings.Color3};padding:0.6em 0.4 em;\"><strong>{await _localizationService.GetResourceAsync("Messages.Order.Tax", languageId)}</strong></td> <td style=\"background-color: {_templatesSettings.Color3};padding:0.6em 0.4 em;\"><strong>{cusTaxTotal}</strong></td></tr>");
            }

            if (displayTaxRates)
            {
                foreach (var item in taxRates)
                {
                    var taxRate = string.Format(await _localizationService.GetResourceAsync("Messages.Order.TaxRateLine"),
                        _priceFormatter.FormatTaxRate(item.Key));
                    var taxValue = await _priceFormatter.FormatPriceAsync(item.Value, true, order.CustomerCurrencyCode, false, languageId);
                    sb.AppendLine($"<tr style=\"text-align:right;\"><td>&nbsp;</td><td colspan=\"{colSpan}\" style=\"background-color: {_templatesSettings.Color3};padding:0.6em 0.4 em;\"><strong>{taxRate}</strong></td> <td style=\"background-color: {_templatesSettings.Color3};padding:0.6em 0.4 em;\"><strong>{taxValue}</strong></td></tr>");
                }
            }

            //service fee
            if (order.ServiceFee > 0)
            {
                var serviceFee = await _priceFormatter.FormatPriceAsync(order.ServiceFee);
                sb.AppendLine($"<tr style=\"text-align:right;\"><td>&nbsp;</td><td colspan=\"{colSpan}\" style=\"background-color: {_templatesSettings.Color3};padding:0.6em 0.4 em;\"><strong>{await _localizationService.GetResourceAsync("Messages.Order.ServiceFee", languageId)}</strong></td> <td style=\"background-color: {_templatesSettings.Color3};padding:0.6em 0.4 em;\"><strong>{serviceFee}</strong></td></tr>");
            }

            //slot fee
            if (order.SlotFee > 0)
            {
                var slotFee = await _priceFormatter.FormatPriceAsync(order.SlotFee);
                sb.AppendLine($"<tr style=\"text-align:right;\"><td>&nbsp;</td><td colspan=\"{colSpan}\" style=\"background-color: {_templatesSettings.Color3};padding:0.6em 0.4 em;\"><strong>{await _localizationService.GetResourceAsync("Messages.Order.SlotFee", languageId)}</strong></td> <td style=\"background-color: {_templatesSettings.Color3};padding:0.6em 0.4 em;\"><strong>{slotFee}</strong></td></tr>");
            }

            /*Alchub Start*/
            //delivery fee
            var tipFeeInCustomerCurrency = _currencyService.ConvertCurrency(order.TipFee, order.CurrencyRate);
            if (tipFeeInCustomerCurrency > 0)
            {
                var tipFeeTotal = await _priceFormatter.FormatPriceAsync(tipFeeInCustomerCurrency, true, order.CustomerCurrencyCode, false, languageId);
                sb.AppendLine($"<tr style=\"text-align:right;\"><td>&nbsp;</td><td colspan=\"{colSpan}\" style=\"background-color: {_templatesSettings.Color3};padding:0.6em 0.4 em;\"><strong>{await _localizationService.GetResourceAsync("Alchub.Messages.Order.TipFee", languageId)}</strong></td> <td style=\"background-color: {_templatesSettings.Color3};padding:0.6em 0.4 em;\"><strong>{tipFeeTotal}</strong></td></tr>");
            }
            /*Alchub End*/

            //discount
            if (displayDiscount)
            {
                sb.AppendLine($"<tr style=\"text-align:right;\"><td>&nbsp;</td><td colspan=\"{colSpan}\" style=\"background-color: {_templatesSettings.Color3};padding:0.6em 0.4 em;\"><strong>{await _localizationService.GetResourceAsync("Messages.Order.TotalDiscount", languageId)}</strong></td> <td style=\"background-color: {_templatesSettings.Color3};padding:0.6em 0.4 em;\"><strong>{cusDiscount}</strong></td></tr>");
            }

            //gift cards
            foreach (var gcuh in await _giftCardService.GetGiftCardUsageHistoryAsync(order))
            {
                var giftCardText = string.Format(await _localizationService.GetResourceAsync("Messages.Order.GiftCardInfo", languageId),
                    WebUtility.HtmlEncode((await _giftCardService.GetGiftCardByIdAsync(gcuh.GiftCardId))?.GiftCardCouponCode));
                var giftCardAmount = await _priceFormatter.FormatPriceAsync(-_currencyService.ConvertCurrency(gcuh.UsedValue, order.CurrencyRate), true, order.CustomerCurrencyCode,
                    false, languageId);
                sb.AppendLine($"<tr style=\"text-align:right;\"><td>&nbsp;</td><td colspan=\"{colSpan}\" style=\"background-color: {_templatesSettings.Color3};padding:0.6em 0.4 em;\"><strong>{giftCardText}</strong></td> <td style=\"background-color: {_templatesSettings.Color3};padding:0.6em 0.4 em;\"><strong>{giftCardAmount}</strong></td></tr>");
            }

            //reward points
            if (order.RedeemedRewardPointsEntryId.HasValue && await _rewardPointService.GetRewardPointsHistoryEntryByIdAsync(order.RedeemedRewardPointsEntryId.Value) is RewardPointsHistory redeemedRewardPointsEntry)
            {
                var rpTitle = string.Format(await _localizationService.GetResourceAsync("Messages.Order.RewardPoints", languageId),
                    -redeemedRewardPointsEntry.Points);
                var rpAmount = await _priceFormatter.FormatPriceAsync(-_currencyService.ConvertCurrency(redeemedRewardPointsEntry.UsedAmount, order.CurrencyRate), true,
                    order.CustomerCurrencyCode, false, languageId);
                sb.AppendLine($"<tr style=\"text-align:right;\"><td>&nbsp;</td><td colspan=\"{colSpan}\" style=\"background-color: {_templatesSettings.Color3};padding:0.6em 0.4 em;\"><strong>{rpTitle}</strong></td> <td style=\"background-color: {_templatesSettings.Color3};padding:0.6em 0.4 em;\"><strong>{rpAmount}</strong></td></tr>");
            }

            //total
            sb.AppendLine($"<tr style=\"text-align:right;\"><td>&nbsp;</td><td colspan=\"{colSpan}\" style=\"background-color: {_templatesSettings.Color3};padding:0.6em 0.4 em;\"><strong>{await _localizationService.GetResourceAsync("Messages.Order.OrderTotal", languageId)}</strong></td> <td style=\"background-color: {_templatesSettings.Color3};padding:0.6em 0.4 em;\"><strong>{cusTotal}</strong></td></tr>");
        }

        /// <summary>
        /// Prepare products name string for SMS
        /// </summary>
        /// <param name="orderItems"></param>
        /// <returns></returns>
        private async Task<string> PrepareProductNamesStr(IList<OrderItem> orderItems)
        {
            var sb = new StringBuilder();
            var firstProductName = await _productService.GetProductItemName((await _productService.GetProductByIdAsync(orderItems.FirstOrDefault().ProductId)), orderItems.FirstOrDefault());
            firstProductName = CommonHelper.EnsureMaximumLength(firstProductName, 20, "...");
            sb.Append(firstProductName);
            if (orderItems.Count > 1)
            {
                var moreItemsCount = orderItems.Count - 1;
                sb.Append($" +{moreItemsCount} item");
                if (moreItemsCount > 1)
                    sb.Append("s");
            }

            return sb.ToString();
        }

        /// <summary>
        /// Order pickup addresses
        /// </summary>
        /// <param name="order"></param>
        /// <param name="languageId"></param>
        /// <param name="vendorId"></param>
        /// <param name="orderItems"></param>
        /// <returns></returns>
        protected virtual async Task<string> OrderPickupAddressToHtmlAsync(Order order, int languageId, int vendorId, IList<OrderItem> orderItems)
        {
            var sb = new StringBuilder();
            var orderVendorsIds = new List<int>();

            for (var i = 0; i <= orderItems.Count - 1; i++)
            {
                var orderItem = orderItems[i];

                //only add pickup 
                if (!orderItem.InPickup)
                    continue;

                var product = await _productService.GetProductByIdAsync(orderItem.ProductId);

                if (product == null)
                    continue;

                //vendor
                var vendor = await _vendorService.GetVendorByIdAsync(product.VendorId);
                if (vendor == null)
                    continue;

                //check already added
                if (orderVendorsIds.Contains(product.VendorId))
                    continue;

                orderVendorsIds.Add(product.VendorId);

                sb.AppendLine($"<b>{vendor.Name}:</b> ");
                sb.AppendLine("<br />");
                sb.AppendLine($"{vendor.PickupAddress?.Replace(", USA", "")}");//remove USA from store name - 14-06-23
                sb.AppendLine("<br />");
                sb.AppendLine("<br />");
            }

            var result = sb.ToString();
            return result;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Add order tokens
        /// </summary>
        /// <param name="tokens">List of already added tokens</param>
        /// <param name="order"></param>
        /// <param name="languageId">Language identifier</param>
        /// <param name="vendorId">Vendor identifier</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task AddOrderTokensAsync(IList<Token> tokens, Order order, int languageId, int vendorId = 0)
        {
            //lambda expression for choosing correct order address
            async Task<Address> orderAddress(Order o) => await _addressService.GetAddressByIdAsync((o.PickupInStore ? o.PickupAddressId : o.ShippingAddressId) ?? 0);

            var billingAddress = await _addressService.GetAddressByIdAsync(order.BillingAddressId);

            tokens.Add(new Token("Order.OrderId", order.Id));
            tokens.Add(new Token("Order.OrderNumber", order.CustomOrderNumber));

            tokens.Add(new Token("Order.CustomerFullName", $"{billingAddress.FirstName} {billingAddress.LastName}"));
            tokens.Add(new Token("Order.CustomerEmail", billingAddress.Email));

            tokens.Add(new Token("Order.BillingFirstName", billingAddress.FirstName));
            tokens.Add(new Token("Order.BillingLastName", billingAddress.LastName));
            tokens.Add(new Token("Order.BillingPhoneNumber", billingAddress.PhoneNumber));
            tokens.Add(new Token("Order.BillingEmail", billingAddress.Email));
            tokens.Add(new Token("Order.BillingFaxNumber", billingAddress.FaxNumber));
            tokens.Add(new Token("Order.BillingCompany", billingAddress.Company));
            tokens.Add(new Token("Order.BillingAddress1", billingAddress.Address1));
            tokens.Add(new Token("Order.BillingAddress2", billingAddress.Address2));
            tokens.Add(new Token("Order.BillingCity", billingAddress.City));
            tokens.Add(new Token("Order.BillingCounty", billingAddress.County));
            tokens.Add(new Token("Order.BillingStateProvince", await _stateProvinceService.GetStateProvinceByAddressAsync(billingAddress) is StateProvince billingStateProvince ? await _localizationService.GetLocalizedAsync(billingStateProvince, x => x.Name) : string.Empty));
            tokens.Add(new Token("Order.BillingZipPostalCode", billingAddress.ZipPostalCode));
            tokens.Add(new Token("Order.BillingCountry", await _countryService.GetCountryByAddressAsync(billingAddress) is Country billingCountry ? await _localizationService.GetLocalizedAsync(billingCountry, x => x.Name) : string.Empty));
            //++Alchub
            tokens.Add(new Token("Order.BillingAddressType", billingAddress.AddressType.ToString()));
            //--Alchub
            tokens.Add(new Token("Order.BillingCustomAttributes", await _addressAttributeFormatter.FormatAttributesAsync(billingAddress.CustomAttributes), true));

            //++Alchub
            //by default do not show shipping address.
            bool showShipping = false;
            tokens.Add(new Token("Order.Shippable", showShipping));
            //--Alchub
            tokens.Add(new Token("Order.ShippingMethod", order.ShippingMethod));
            tokens.Add(new Token("Order.PickupInStore", order.PickupInStore));
            tokens.Add(new Token("Order.ShippingFirstName", (await orderAddress(order))?.FirstName ?? string.Empty));
            tokens.Add(new Token("Order.ShippingLastName", (await orderAddress(order))?.LastName ?? string.Empty));
            tokens.Add(new Token("Order.ShippingPhoneNumber", (await orderAddress(order))?.PhoneNumber ?? string.Empty));
            tokens.Add(new Token("Order.ShippingEmail", (await orderAddress(order))?.Email ?? string.Empty));
            tokens.Add(new Token("Order.ShippingFaxNumber", (await orderAddress(order))?.FaxNumber ?? string.Empty));
            tokens.Add(new Token("Order.ShippingCompany", (await orderAddress(order))?.Company ?? string.Empty));
            tokens.Add(new Token("Order.ShippingAddress1", (await orderAddress(order))?.Address1 ?? string.Empty));
            tokens.Add(new Token("Order.ShippingAddress2", (await orderAddress(order))?.Address2 ?? string.Empty));
            tokens.Add(new Token("Order.ShippingCity", (await orderAddress(order))?.City ?? string.Empty));
            tokens.Add(new Token("Order.ShippingCounty", (await orderAddress(order))?.County ?? string.Empty));
            tokens.Add(new Token("Order.ShippingStateProvince", await _stateProvinceService.GetStateProvinceByAddressAsync(await orderAddress(order)) is StateProvince shippingStateProvince ? await _localizationService.GetLocalizedAsync(shippingStateProvince, x => x.Name) : string.Empty));
            tokens.Add(new Token("Order.ShippingZipPostalCode", (await orderAddress(order))?.ZipPostalCode ?? string.Empty));
            tokens.Add(new Token("Order.ShippingCountry", await _countryService.GetCountryByAddressAsync(await orderAddress(order)) is Country orderCountry ? await _localizationService.GetLocalizedAsync(orderCountry, x => x.Name) : string.Empty));
            //++Alchub
            tokens.Add(new Token("Order.ShippingAddressType", (await orderAddress(order))?.AddressType.ToString() ?? string.Empty));
            //--Alchub
            tokens.Add(new Token("Order.ShippingCustomAttributes", await _addressAttributeFormatter.FormatAttributesAsync((await orderAddress(order))?.CustomAttributes ?? string.Empty), true));
            tokens.Add(new Token("Order.IsCompletelyShipped", !order.PickupInStore && order.ShippingStatus == ShippingStatus.Shipped));
            tokens.Add(new Token("Order.IsCompletelyReadyForPickup", order.PickupInStore && !await _orderService.HasItemsToAddToShipmentAsync(order) && !await _orderService.HasItemsToReadyForPickupAsync(order)));
            tokens.Add(new Token("Order.IsCompletelyDelivered", order.ShippingStatus == ShippingStatus.Delivered));

            //pickup address
            var orderItems = await _orderService.GetOrderItemsAsync(order.Id, vendorId: vendorId);
            //add token pickup included
            tokens.Add(new Token("Order.PickupIncluded", orderItems.Any(oi => oi.InPickup)));
            tokens.Add(new Token("Order.PickupAddress", await OrderPickupAddressToHtmlAsync(order, languageId, vendorId, orderItems), true));

            var paymentMethod = await _paymentPluginManager.LoadPluginBySystemNameAsync(order.PaymentMethodSystemName);
            var paymentMethodName = paymentMethod != null ? await _localizationService.GetLocalizedFriendlyNameAsync(paymentMethod, (await _workContext.GetWorkingLanguageAsync()).Id) : order.PaymentMethodSystemName;
            tokens.Add(new Token("Order.PaymentMethod", paymentMethodName));
            tokens.Add(new Token("Order.VatNumber", order.VatNumber));
            var sbCustomValues = new StringBuilder();
            var customValues = _paymentService.DeserializeCustomValues(order);
            if (customValues != null)
            {
                foreach (var item in customValues)
                {
                    sbCustomValues.AppendFormat("{0}: {1}", WebUtility.HtmlEncode(item.Key), WebUtility.HtmlEncode(item.Value != null ? item.Value.ToString() : string.Empty));
                    sbCustomValues.Append("<br />");
                }
            }

            tokens.Add(new Token("Order.CustomValues", sbCustomValues.ToString(), true));

            tokens.Add(new Token("Order.Product(s)", await ProductListToHtmlTableAsync(order, languageId, vendorId), true));

            var language = await _languageService.GetLanguageByIdAsync(languageId);
            if (language != null && !string.IsNullOrEmpty(language.LanguageCulture))
            {
                var customer = await _customerService.GetCustomerByIdAsync(order.CustomerId);
                var createdOn = _dateTimeHelper.ConvertToUserTime(order.CreatedOnUtc, TimeZoneInfo.Utc, await _dateTimeHelper.GetCustomerTimeZoneAsync(customer));
                tokens.Add(new Token("Order.CreatedOn", createdOn.ToString("D", new CultureInfo(language.LanguageCulture))));
            }
            else
            {
                tokens.Add(new Token("Order.CreatedOn", order.CreatedOnUtc.ToString("D")));
            }

            var orderUrl = await RouteUrlAsync(order.StoreId, "OrderDetails", new { orderId = order.Id });
            tokens.Add(new Token("Order.OrderURLForCustomer", orderUrl, true));

            //total
            var orderTotalInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderTotal, order.CurrencyRate);
            var cusTotal = await _priceFormatter.FormatPriceAsync(orderTotalInCustomerCurrency, true, order.CustomerCurrencyCode, false, languageId);
            tokens.Add(new Token("Order.OrderTotal", cusTotal));

            //event notification
            await _eventPublisher.EntityTokensAddedAsync(order, tokens);
        }

        /// <summary>
        /// Add order item delivered tokens
        /// </summary>
        /// <param name="tokens">List of already added tokens</param>
        /// <param name="order">order</param>
        /// <param name="orderItems">orderItems</param>
        /// <param name="languageId">Language identifier</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task AddOrderItemsDeliveredTokensAsync(IList<Token> tokens, Order order, IList<OrderItem> orderItems, int languageId)
        {
            tokens.Add(new Token("OrderItem.Product(s)", await ProductListToHtmlTableAsync(order, orderItems, languageId), true));
            //products name
            tokens.Add(new Token("OrderItem.Product(s)Name", await PrepareProductNamesStr(orderItems)));
        }

        /// <summary>
        /// Add order item pickup completed tokens
        /// </summary>
        /// <param name="tokens">List of already added tokens</param>
        /// <param name="order">order</param>
        /// <param name="orderItems">orderItems</param>
        /// <param name="languageId">Language identifier</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task AddOrderItemsPickupCompletedTokensAsync(IList<Token> tokens, Order order, IList<OrderItem> orderItems, int languageId)
        {
            tokens.Add(new Token("OrderItem.Product(s)", await ProductListToHtmlTableAsync(order, orderItems, languageId), true));
            //products name
            tokens.Add(new Token("OrderItem.Product(s)Name", await PrepareProductNamesStr(orderItems)));
        }

        /// <summary>
        /// Add order item dispacthed tokens
        /// </summary>
        /// <param name="tokens">List of already added tokens</param>
        /// <param name="order">order</param>
        /// <param name="orderItems">orderItems</param>
        /// <param name="languageId">Language identifier</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task AddOrderItemsDispacthedTokensAsync(IList<Token> tokens, Order order, IList<OrderItem> orderItems, int languageId)
        {
            tokens.Add(new Token("OrderItem.Product(s)", await ProductListToHtmlTableAsync(order, orderItems, languageId), true));
            //products name
            tokens.Add(new Token("OrderItem.Product(s)Name", await PrepareProductNamesStr(orderItems)));
        }

        /// <summary>
        /// Add order item dispacthed tokens
        /// </summary>
        /// <param name="tokens">List of already added tokens</param>
        /// <param name="order">order</param>
        /// <param name="orderItems">orderItems</param>
        /// <param name="languageId">Language identifier</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task AddOrderItemsCancelCustomerTokensAsync(IList<Token> tokens, Order order, OrderItem orderItems, decimal orderItemRefundAmount, int languageId)
        {
            tokens.Add(new Token("Order.OrderNumber", order.Id.ToString()));

            var firstProductName = (await _productService.GetProductByIdAsync(orderItems.ProductId)).Name;
            //products name
            tokens.Add(new Token("OrderItem.ProductName", firstProductName.ToString()));
            //formate amount
            var amountRefundStr = await _priceFormatter.FormatPriceAsync(orderItemRefundAmount);
            tokens.Add(new Token("OrderItem.RefundAmount", amountRefundStr));
        }

        /// <summary>
        /// Add order item dispacthed tokens
        /// </summary>
        /// <param name="tokens">List of already added tokens</param>
        /// <param name="order">order</param>
        /// <param name="orderItems">orderItems</param>
        /// <param name="languageId">Language identifier</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task AddOrderItemsCancelVendorTokensAsync(IList<Token> tokens, Order order, OrderItem orderItems, int languageId)
        {
            tokens.Add(new Token("Order.OrderNumber", order.Id.ToString()));

            var firstProductName = (await _productService.GetProductByIdAsync(orderItems.ProductId)).Name;
            //products name
            tokens.Add(new Token("OrderItem.ProductName", firstProductName.ToString()));
        }

        /// <summary>
        /// Add order item dispacthed tokens
        /// </summary>
        /// <param name="tokens">List of already added tokens</param>
        /// <param name="order">order</param>
        /// <param name="orderItems">orderItems</param>
        /// <param name="languageId">Language identifier</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task AddOrderItemsDeliveryDeniedCustomerTokensAsync(IList<Token> tokens, Order order, IList<OrderItem> orderItems, int languageId)
        {
            tokens.Add(new Token("OrderItem.Product(s)", await ProductListToHtmlTableAsync(order, orderItems, languageId), true));
            //products name
            tokens.Add(new Token("OrderItem.Product(s)Name", await PrepareProductNamesStr(orderItems)));
        }

        /// <summary>
        /// Add order item dispacthed tokens
        /// </summary>
        /// <param name="tokens">List of already added tokens</param>
        /// <param name="order">order</param>
        /// <param name="orderItems">orderItems</param>
        /// <param name="languageId">Language identifier</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task AddOrderItemsDeliveryDeniedVendorTokensAsync(IList<Token> tokens, Order order, IList<OrderItem> orderItems, int languageId)
        {
            tokens.Add(new Token("OrderItem.Product(s)", await ProductListToHtmlTableAsync(order, orderItems, languageId), true));
            //products name
            tokens.Add(new Token("OrderItem.Product(s)Name", await PrepareProductNamesStr(orderItems)));
        }

        /// <summary>
        /// Get token groups of message template
        /// </summary>
        /// <param name="messageTemplate">Message template</param>
        /// <returns>Collection of token group names</returns>
        public virtual IEnumerable<string> GetTokenGroups(MessageTemplate messageTemplate)
        {
            //groups depend on which tokens are added at the appropriate methods in IWorkflowMessageService
            return messageTemplate.Name switch
            {
                MessageTemplateSystemNames.CustomerRegisteredNotification or
                MessageTemplateSystemNames.CustomerWelcomeMessage or
                MessageTemplateSystemNames.CustomerEmailValidationMessage or
                MessageTemplateSystemNames.CustomerEmailRevalidationMessage or
                MessageTemplateSystemNames.CustomerPasswordRecoveryMessage => new[] { TokenGroupNames.StoreTokens, TokenGroupNames.CustomerTokens },

                MessageTemplateSystemNames.OrderPlacedVendorNotification or
                MessageTemplateSystemNames.OrderPlacedStoreOwnerNotification or
                MessageTemplateSystemNames.OrderPlacedAffiliateNotification or
                MessageTemplateSystemNames.OrderPaidStoreOwnerNotification or
                MessageTemplateSystemNames.OrderPaidCustomerNotification or
                MessageTemplateSystemNames.OrderPaidVendorNotification or
                MessageTemplateSystemNames.OrderPaidAffiliateNotification or
                MessageTemplateSystemNames.OrderPlacedCustomerNotification or
                MessageTemplateSystemNames.OrderCompletedCustomerNotification or
                MessageTemplateSystemNames.OrderCancelledCustomerNotification => new[] { TokenGroupNames.StoreTokens, TokenGroupNames.OrderTokens, TokenGroupNames.CustomerTokens },

                MessageTemplateSystemNames.ShipmentSentCustomerNotification or
                MessageTemplateSystemNames.ShipmentReadyForPickupCustomerNotification or
                MessageTemplateSystemNames.ShipmentDeliveredCustomerNotification => new[] { TokenGroupNames.StoreTokens, TokenGroupNames.ShipmentTokens, TokenGroupNames.OrderTokens, TokenGroupNames.CustomerTokens },

                //++Alchub
                MessageTemplateSystemNames.OrderItemsDeliveredCustomerNotification => new[] { TokenGroupNames.StoreTokens, TokenGroupNames.OrderTokens, TokenGroupNames.CustomerTokens },
                //--Alchub

                MessageTemplateSystemNames.OrderRefundedStoreOwnerNotification or
                MessageTemplateSystemNames.OrderRefundedCustomerNotification => new[] { TokenGroupNames.StoreTokens, TokenGroupNames.OrderTokens, TokenGroupNames.RefundedOrderTokens, TokenGroupNames.CustomerTokens },

                MessageTemplateSystemNames.NewOrderNoteAddedCustomerNotification => new[] { TokenGroupNames.StoreTokens, TokenGroupNames.OrderNoteTokens, TokenGroupNames.OrderTokens, TokenGroupNames.CustomerTokens },

                MessageTemplateSystemNames.RecurringPaymentCancelledStoreOwnerNotification or
                MessageTemplateSystemNames.RecurringPaymentCancelledCustomerNotification or
                MessageTemplateSystemNames.RecurringPaymentFailedCustomerNotification => new[] { TokenGroupNames.StoreTokens, TokenGroupNames.OrderTokens, TokenGroupNames.CustomerTokens, TokenGroupNames.RecurringPaymentTokens },

                MessageTemplateSystemNames.NewsletterSubscriptionActivationMessage or
                MessageTemplateSystemNames.NewsletterSubscriptionDeactivationMessage => new[] { TokenGroupNames.StoreTokens, TokenGroupNames.SubscriptionTokens },

                MessageTemplateSystemNames.EmailAFriendMessage => new[] { TokenGroupNames.StoreTokens, TokenGroupNames.CustomerTokens, TokenGroupNames.ProductTokens, TokenGroupNames.EmailAFriendTokens },
                MessageTemplateSystemNames.WishlistToFriendMessage => new[] { TokenGroupNames.StoreTokens, TokenGroupNames.CustomerTokens, TokenGroupNames.WishlistToFriendTokens },

                MessageTemplateSystemNames.NewReturnRequestStoreOwnerNotification or
                MessageTemplateSystemNames.NewReturnRequestCustomerNotification or
                MessageTemplateSystemNames.ReturnRequestStatusChangedCustomerNotification => new[] { TokenGroupNames.StoreTokens, TokenGroupNames.OrderTokens, TokenGroupNames.CustomerTokens, TokenGroupNames.ReturnRequestTokens },

                MessageTemplateSystemNames.NewForumTopicMessage => new[] { TokenGroupNames.StoreTokens, TokenGroupNames.ForumTopicTokens, TokenGroupNames.ForumTokens, TokenGroupNames.CustomerTokens },
                MessageTemplateSystemNames.NewForumPostMessage => new[] { TokenGroupNames.StoreTokens, TokenGroupNames.ForumPostTokens, TokenGroupNames.ForumTopicTokens, TokenGroupNames.ForumTokens, TokenGroupNames.CustomerTokens },
                MessageTemplateSystemNames.PrivateMessageNotification => new[] { TokenGroupNames.StoreTokens, TokenGroupNames.PrivateMessageTokens, TokenGroupNames.CustomerTokens },
                MessageTemplateSystemNames.NewVendorAccountApplyStoreOwnerNotification => new[] { TokenGroupNames.StoreTokens, TokenGroupNames.CustomerTokens, TokenGroupNames.VendorTokens },
                MessageTemplateSystemNames.VendorInformationChangeNotification => new[] { TokenGroupNames.StoreTokens, TokenGroupNames.VendorTokens },
                MessageTemplateSystemNames.GiftCardNotification => new[] { TokenGroupNames.StoreTokens, TokenGroupNames.GiftCardTokens },

                MessageTemplateSystemNames.ProductReviewStoreOwnerNotification or
                MessageTemplateSystemNames.ProductReviewReplyCustomerNotification => new[] { TokenGroupNames.StoreTokens, TokenGroupNames.ProductReviewTokens, TokenGroupNames.CustomerTokens },

                MessageTemplateSystemNames.QuantityBelowStoreOwnerNotification => new[] { TokenGroupNames.StoreTokens, TokenGroupNames.ProductTokens },
                MessageTemplateSystemNames.QuantityBelowAttributeCombinationStoreOwnerNotification => new[] { TokenGroupNames.StoreTokens, TokenGroupNames.ProductTokens, TokenGroupNames.AttributeCombinationTokens },
                MessageTemplateSystemNames.NewVatSubmittedStoreOwnerNotification => new[] { TokenGroupNames.StoreTokens, TokenGroupNames.CustomerTokens, TokenGroupNames.VatValidation },
                MessageTemplateSystemNames.BlogCommentNotification => new[] { TokenGroupNames.StoreTokens, TokenGroupNames.BlogCommentTokens, TokenGroupNames.CustomerTokens },
                MessageTemplateSystemNames.NewsCommentNotification => new[] { TokenGroupNames.StoreTokens, TokenGroupNames.NewsCommentTokens, TokenGroupNames.CustomerTokens },
                MessageTemplateSystemNames.BackInStockNotification => new[] { TokenGroupNames.StoreTokens, TokenGroupNames.CustomerTokens, TokenGroupNames.ProductBackInStockTokens },
                MessageTemplateSystemNames.ContactUsMessage => new[] { TokenGroupNames.StoreTokens, TokenGroupNames.ContactUs },
                MessageTemplateSystemNames.ContactVendorMessage => new[] { TokenGroupNames.StoreTokens, TokenGroupNames.ContactVendor },
                _ => Array.Empty<string>(),
            };
        }

        #endregion
    }
}
