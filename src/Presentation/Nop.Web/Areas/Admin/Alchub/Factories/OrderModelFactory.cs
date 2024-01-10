using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.DeliveryFees;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Tax;
using Nop.Core.Domain.TipFees;
using Nop.Services.Affiliates;
using Nop.Services.Alchub.Slots;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.DeliveryFees;
using Nop.Services.Directory;
using Nop.Services.Discounts;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Shipping;
using Nop.Services.Stores;
using Nop.Services.Tax;
using Nop.Services.TipFees;
using Nop.Services.Vendors;
using Nop.Web.Areas.Admin.Models.Orders;
using Nop.Web.Areas.Admin.Models.Reports;
using Nop.Web.Framework.Models.Extensions;

namespace Nop.Web.Areas.Admin.Factories
{
    /// <summary>
    /// Represents the order model factory implementation
    /// </summary>
    public partial class OrderModelFactory : IOrderModelFactory
    {
        #region Fields

        private readonly AddressSettings _addressSettings;
        private readonly CatalogSettings _catalogSettings;
        private readonly CurrencySettings _currencySettings;
        private readonly IActionContextAccessor _actionContextAccessor;
        private readonly IAddressModelFactory _addressModelFactory;
        private readonly IAddressService _addressService;
        private readonly IAffiliateService _affiliateService;
        private readonly IBaseAdminModelFactory _baseAdminModelFactory;
        private readonly ICountryService _countryService;
        private readonly ICurrencyService _currencyService;
        private readonly ICustomerService _customerService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IDiscountService _discountService;
        private readonly IDownloadService _downloadService;
        private readonly IEncryptionService _encryptionService;
        private readonly IGiftCardService _giftCardService;
        private readonly ILocalizationService _localizationService;
        private readonly IMeasureService _measureService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IOrderReportService _orderReportService;
        private readonly IOrderService _orderService;
        private readonly IPaymentPluginManager _paymentPluginManager;
        private readonly IPaymentService _paymentService;
        private readonly IPictureService _pictureService;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IProductAttributeService _productAttributeService;
        private readonly IProductService _productService;
        private readonly IReturnRequestService _returnRequestService;
        private readonly IRewardPointService _rewardPointService;
        private readonly IShipmentService _shipmentService;
        private readonly IShippingService _shippingService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly IStoreService _storeService;
        private readonly ITaxService _taxService;
        private readonly IUrlHelperFactory _urlHelperFactory;
        private readonly IVendorService _vendorService;
        private readonly IWorkContext _workContext;
        private readonly MeasureSettings _measureSettings;
        private readonly OrderSettings _orderSettings;
        private readonly ShippingSettings _shippingSettings;
        private readonly IUrlRecordService _urlRecordService;
        private readonly TaxSettings _taxSettings;
        private readonly IDeliveryFeeService _deliveryFeeService;
        private readonly ITipFeeService _tipFeeService;
        private readonly IOrderItemRefundService _orderItemRefundService;
        #endregion

        #region Ctor

        public OrderModelFactory(AddressSettings addressSettings,
            CatalogSettings catalogSettings,
            CurrencySettings currencySettings,
            IActionContextAccessor actionContextAccessor,
            IAddressModelFactory addressModelFactory,
            IAddressService addressService,
            IAffiliateService affiliateService,
            IBaseAdminModelFactory baseAdminModelFactory,
            ICountryService countryService,
            ICurrencyService currencyService,
            ICustomerService customerService,
            IDateTimeHelper dateTimeHelper,
            IDiscountService discountService,
            IDownloadService downloadService,
            IEncryptionService encryptionService,
            IGiftCardService giftCardService,
            ILocalizationService localizationService,
            IMeasureService measureService,
            IOrderProcessingService orderProcessingService,
            IOrderReportService orderReportService,
            IOrderService orderService,
            IPaymentPluginManager paymentPluginManager,
            IPaymentService paymentService,
            IPictureService pictureService,
            IPriceCalculationService priceCalculationService,
            IPriceFormatter priceFormatter,
            IProductAttributeService productAttributeService,
            IProductService productService,
            IReturnRequestService returnRequestService,
            IRewardPointService rewardPointService,
            IShipmentService shipmentService,
            IShippingService shippingService,
            IStateProvinceService stateProvinceService,
            IStoreService storeService,
            ITaxService taxService,
            IUrlHelperFactory urlHelperFactory,
            IVendorService vendorService,
            IWorkContext workContext,
            MeasureSettings measureSettings,
            OrderSettings orderSettings,
            ShippingSettings shippingSettings,
            IUrlRecordService urlRecordService,
            TaxSettings taxSettings,
            IDeliveryFeeService deliveryFeeService,
            ITipFeeService tipFeeService,
            IOrderItemRefundService orderItemRefundService)
        {
            _addressSettings = addressSettings;
            _catalogSettings = catalogSettings;
            _currencySettings = currencySettings;
            _actionContextAccessor = actionContextAccessor;
            _addressModelFactory = addressModelFactory;
            _addressService = addressService;
            _affiliateService = affiliateService;
            _baseAdminModelFactory = baseAdminModelFactory;
            _countryService = countryService;
            _currencyService = currencyService;
            _customerService = customerService;
            _dateTimeHelper = dateTimeHelper;
            _discountService = discountService;
            _downloadService = downloadService;
            _encryptionService = encryptionService;
            _giftCardService = giftCardService;
            _localizationService = localizationService;
            _measureService = measureService;
            _orderProcessingService = orderProcessingService;
            _orderReportService = orderReportService;
            _orderService = orderService;
            _paymentPluginManager = paymentPluginManager;
            _paymentService = paymentService;
            _pictureService = pictureService;
            _priceCalculationService = priceCalculationService;
            _priceFormatter = priceFormatter;
            _productAttributeService = productAttributeService;
            _productService = productService;
            _returnRequestService = returnRequestService;
            _rewardPointService = rewardPointService;
            _shipmentService = shipmentService;
            _shippingService = shippingService;
            _stateProvinceService = stateProvinceService;
            _storeService = storeService;
            _taxService = taxService;
            _urlHelperFactory = urlHelperFactory;
            _vendorService = vendorService;
            _workContext = workContext;
            _measureSettings = measureSettings;
            _orderSettings = orderSettings;
            _shippingSettings = shippingSettings;
            _urlRecordService = urlRecordService;
            _taxSettings = taxSettings;
            _deliveryFeeService = deliveryFeeService;
            _tipFeeService = tipFeeService;
            _orderItemRefundService = orderItemRefundService;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Prepare shipment item model
        /// </summary>
        /// <param name="model">Shipment item model</param>
        /// <param name="orderItem">Order item</param>
        /// <param name="product">Product item</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        protected virtual async Task PrepareShipmentItemModelAsync(ShipmentItemModel model, OrderItem orderItem, Product product)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            if (orderItem is null)
                throw new ArgumentNullException(nameof(orderItem));

            if (product is null)
                throw new ArgumentNullException(nameof(product));

            if (orderItem.ProductId != product.Id)
                throw new ArgumentException($"{nameof(orderItem.ProductId)} != {nameof(product.Id)}");

            //fill in additional values (not existing in the entity)
            model.OrderItemId = orderItem.Id;
            model.ProductId = orderItem.ProductId;
            model.ProductName = await _productService.GetProductItemName(product, orderItem); //++Alchub
            model.Sku = product.UPCCode;//++Alchub
            model.AttributeInfo = orderItem.AttributeDescription;
            model.CustomAttributeInfo = orderItem.CustomAttributesDescription; //++Alchub
            model.ShipSeparately = product.ShipSeparately;
            model.QuantityOrdered = orderItem.Quantity;
            model.QuantityInAllShipments = await _orderService.GetTotalNumberOfItemsInAllShipmentsAsync(orderItem);
            model.QuantityToAdd = await _orderService.GetTotalNumberOfItemsCanBeAddedToShipmentAsync(orderItem);

            var baseWeight = (await _measureService.GetMeasureWeightByIdAsync(_measureSettings.BaseWeightId))?.Name;
            var baseDimension = (await _measureService.GetMeasureDimensionByIdAsync(_measureSettings.BaseDimensionId))?.Name;
            if (orderItem.ItemWeight.HasValue)
                model.ItemWeight = $"{orderItem.ItemWeight:F2} [{baseWeight}]";
            model.ItemDimensions =
                $"{product.Length:F2} x {product.Width:F2} x {product.Height:F2} [{baseDimension}]";

            if (!product.IsRental)
                return;

            var rentalStartDate = orderItem.RentalStartDateUtc.HasValue
                ? _productService.FormatRentalDate(product, orderItem.RentalStartDateUtc.Value) : string.Empty;
            var rentalEndDate = orderItem.RentalEndDateUtc.HasValue
                ? _productService.FormatRentalDate(product, orderItem.RentalEndDateUtc.Value) : string.Empty;
            model.RentalInfo = string.Format(await _localizationService.GetResourceAsync("Order.Rental.FormattedDate"), rentalStartDate, rentalEndDate);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Prepare paged bestseller brief list model
        /// </summary>
        /// <param name="searchModel">Bestseller brief search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the bestseller brief list model
        /// </returns>
        public virtual async Task<BestsellerBriefListModel> PrepareBestsellerBriefListModelAsync(BestsellerBriefSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var vendor = await _workContext.GetCurrentVendorAsync();

            //prepare isMaster value
            var isMasterProductsOnly = false;
            //Note: If admin then he can see master product report.
            if (await _customerService.IsAdminAsync(await _workContext.GetCurrentCustomerAsync()))
                isMasterProductsOnly = true;

            //get bestsellers
            var bestsellers = await _orderReportService.BestSellersReportAsync(showHidden: true,
                vendorId: vendor?.Id ?? 0,
                orderBy: searchModel.OrderBy,
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize,
                isMasterOnly: isMasterProductsOnly);

            //prepare list model
            var model = await new BestsellerBriefListModel().PrepareToGridAsync(searchModel, bestsellers, () =>
            {
                //fill in model values from the entity
                return bestsellers.SelectAwait(async bestseller =>
                {
                    //fill in model values from the entity
                    var bestsellerModel = new BestsellerModel
                    {
                        ProductId = bestseller.ProductId,
                        TotalQuantity = bestseller.TotalQuantity
                    };

                    //fill in additional values (not existing in the entity)
                    bestsellerModel.ProductName = (await _productService.GetProductByIdAsync(bestseller.ProductId))?.Name;
                    bestsellerModel.TotalAmount = await _priceFormatter.FormatPriceAsync(bestseller.TotalAmount, true, false);

                    return bestsellerModel;
                });
            });

            return model;
        }


        //Note: This service has been overriden in stripe connect plugin.
        /// <summary>
        /// Prepare order model totals
        /// </summary>
        /// <param name="model">Order model</param>
        /// <param name="order">Order</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        protected virtual async Task PrepareOrderModelTotalsAsync(OrderModel model, Order order)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            if (order == null)
                throw new ArgumentNullException(nameof(order));

            var primaryStoreCurrency = await _currencyService.GetCurrencyByIdAsync(_currencySettings.PrimaryStoreCurrencyId);
            var languageId = (await _workContext.GetWorkingLanguageAsync()).Id;

            //subtotal
            model.OrderSubtotalInclTax = await _priceFormatter
                .FormatOrderPriceAsync(order.OrderSubtotalInclTax, order.CurrencyRate, order.CustomerCurrencyCode,
                _orderSettings.DisplayCustomerCurrencyOnOrders, primaryStoreCurrency, languageId, true);
            model.OrderSubtotalExclTax = await _priceFormatter
                .FormatOrderPriceAsync(order.OrderSubtotalExclTax, order.CurrencyRate, order.CustomerCurrencyCode,
                _orderSettings.DisplayCustomerCurrencyOnOrders, primaryStoreCurrency, languageId, false);
            model.OrderSubtotalInclTaxValue = order.OrderSubtotalInclTax;
            model.OrderSubtotalExclTaxValue = order.OrderSubtotalExclTax;

            //discount (applied to order subtotal)
            var orderSubtotalDiscountInclTaxStr = await _priceFormatter
                .FormatOrderPriceAsync(order.OrderSubTotalDiscountInclTax, order.CurrencyRate, order.CustomerCurrencyCode,
                _orderSettings.DisplayCustomerCurrencyOnOrders, primaryStoreCurrency, languageId, true);
            var orderSubtotalDiscountExclTaxStr = await _priceFormatter
                .FormatOrderPriceAsync(order.OrderSubTotalDiscountExclTax, order.CurrencyRate, order.CustomerCurrencyCode,
                _orderSettings.DisplayCustomerCurrencyOnOrders, primaryStoreCurrency, languageId, false);
            if (order.OrderSubTotalDiscountInclTax > decimal.Zero)
                model.OrderSubTotalDiscountInclTax = orderSubtotalDiscountInclTaxStr;
            if (order.OrderSubTotalDiscountExclTax > decimal.Zero)
                model.OrderSubTotalDiscountExclTax = orderSubtotalDiscountExclTaxStr;
            model.OrderSubTotalDiscountInclTaxValue = order.OrderSubTotalDiscountInclTax;
            model.OrderSubTotalDiscountExclTaxValue = order.OrderSubTotalDiscountExclTax;

            //shipping
            model.OrderShippingInclTax = await _priceFormatter
                .FormatOrderPriceAsync(order.OrderShippingInclTax, order.CurrencyRate, order.CustomerCurrencyCode,
                _orderSettings.DisplayCustomerCurrencyOnOrders, primaryStoreCurrency, languageId, true,
                _taxSettings.ShippingIsTaxable && _taxSettings.DisplayTaxSuffix);
            model.OrderShippingExclTax = await _priceFormatter
                .FormatOrderPriceAsync(order.OrderShippingExclTax, order.CurrencyRate, order.CustomerCurrencyCode,
                _orderSettings.DisplayCustomerCurrencyOnOrders, primaryStoreCurrency, languageId, false,
                _taxSettings.ShippingIsTaxable && _taxSettings.DisplayTaxSuffix);
            model.OrderShippingInclTaxValue = order.OrderShippingInclTax;
            model.OrderShippingExclTaxValue = order.OrderShippingExclTax;

            //payment method additional fee
            if (order.PaymentMethodAdditionalFeeInclTax > decimal.Zero)
            {
                model.PaymentMethodAdditionalFeeInclTax = await _priceFormatter
                    .FormatOrderPriceAsync(order.PaymentMethodAdditionalFeeInclTax, order.CurrencyRate, order.CustomerCurrencyCode,
                    _orderSettings.DisplayCustomerCurrencyOnOrders, primaryStoreCurrency, languageId, true,
                    _taxSettings.PaymentMethodAdditionalFeeIsTaxable && _taxSettings.DisplayTaxSuffix);
                model.PaymentMethodAdditionalFeeExclTax = await _priceFormatter
                    .FormatOrderPriceAsync(order.PaymentMethodAdditionalFeeExclTax, order.CurrencyRate, order.CustomerCurrencyCode,
                    _orderSettings.DisplayCustomerCurrencyOnOrders, primaryStoreCurrency, languageId, false,
                    _taxSettings.PaymentMethodAdditionalFeeIsTaxable && _taxSettings.DisplayTaxSuffix);
            }

            model.PaymentMethodAdditionalFeeInclTaxValue = order.PaymentMethodAdditionalFeeInclTax;
            model.PaymentMethodAdditionalFeeExclTaxValue = order.PaymentMethodAdditionalFeeExclTax;

            //tax
            model.Tax = await _priceFormatter
                .FormatOrderPriceAsync(order.OrderTax, order.CurrencyRate, order.CustomerCurrencyCode,
                _orderSettings.DisplayCustomerCurrencyOnOrders, primaryStoreCurrency, languageId, null, false);
            var taxRates = _orderService.ParseTaxRates(order, order.TaxRates);
            var displayTaxRates = _taxSettings.DisplayTaxRates && taxRates.Any();
            var displayTax = !displayTaxRates;
            foreach (var tr in taxRates)
            {
                model.TaxRates.Add(new OrderModel.TaxRate
                {
                    Rate = _priceFormatter.FormatTaxRate(tr.Key),
                    Value = await _priceFormatter
                        .FormatOrderPriceAsync(tr.Value, order.CurrencyRate, order.CustomerCurrencyCode,
                        _orderSettings.DisplayCustomerCurrencyOnOrders, primaryStoreCurrency, languageId, null, false)
                });
            }

            model.DisplayTaxRates = displayTaxRates;
            model.DisplayTax = displayTax;
            model.TaxValue = order.OrderTax;
            model.TaxRatesValue = order.TaxRates;

            //service fee
            model.ServiceFee = await _priceFormatter.FormatOrderPriceAsync(order.ServiceFee, order.CurrencyRate, order.CustomerCurrencyCode,
                _orderSettings.DisplayCustomerCurrencyOnOrders, primaryStoreCurrency, languageId, null, false);

            model.SlotFee = await _priceFormatter.FormatOrderPriceAsync(order.SlotFee, order.CurrencyRate, order.CustomerCurrencyCode,
                _orderSettings.DisplayCustomerCurrencyOnOrders, primaryStoreCurrency, languageId, null, false);

            /*Alchub Start*/

            //Delivery Fee
            model.DeliveryFee = await _priceFormatter.FormatOrderPriceAsync(order.DeliveryFee, order.CurrencyRate, order.CustomerCurrencyCode,
                _orderSettings.DisplayCustomerCurrencyOnOrders, primaryStoreCurrency, languageId, null, false);

            var vendorWiseDeliveryFees = await _deliveryFeeService.GetOrderDeliveryFeesByOrderIdAsync(order.Id);

            if (vendorWiseDeliveryFees != null)
            {
                vendorWiseDeliveryFees.ToList().ForEach(async x =>
                {
                    var deliveryFeeValue = _currencyService.ConvertCurrency(x.DeliveryFee, order.CurrencyRate);
                    var vendor = await _vendorService.GetVendorByIdAsync(x.VendorId);
                    model.VendorWiseDeliveryFees.Add(
                        new VendorWiseDeliveryFee
                        {
                            VendorId = x.VendorId,
                            VendorName = vendor != null ? vendor.Name : "Admin",
                            DeliveryFeeValue = deliveryFeeValue,
                            DeliveryFee = await _priceFormatter.FormatPriceAsync(deliveryFeeValue, true, order.CustomerCurrencyCode, false, languageId)
                        });
                });
            }

            //Tip Fee
            model.TipFee = await _priceFormatter.FormatOrderPriceAsync(order.TipFee, order.CurrencyRate, order.CustomerCurrencyCode,
                _orderSettings.DisplayCustomerCurrencyOnOrders, primaryStoreCurrency, languageId, null, false);

            var vendorWiseTipFees = await _tipFeeService.GetVendorWiseOrderTipFeesByOrderIdAsync(order.Id);

            if (vendorWiseTipFees != null)
            {
                vendorWiseTipFees.ToList().ForEach(async x =>
                {
                    var tipFeeValue = _currencyService.ConvertCurrency(x.TipFeeValue, order.CurrencyRate);

                    model.VendorWiseTipFees.Add(
                        new VendorWiseTipFee
                        {
                            VendorId = x.VendorId,
                            VendorName = x.VendorName,
                            TipFeeValue = tipFeeValue,
                            TipFee = await _priceFormatter.FormatPriceAsync(tipFeeValue, true, order.CustomerCurrencyCode, false, languageId)
                        });
                });
            }


            decimal totalRefundAmount = (await _orderItemRefundService.GetOrderItemRefundByOrderIdAsync(order.Id))?.Sum(x => x.TotalAmount) ?? 0;
            model.TotalRefundAmount = await _priceFormatter.FormatPriceAsync(totalRefundAmount, true, false);
            /*Alchub End*/

            //discount
            if (order.OrderDiscount > 0)
            {
                model.OrderTotalDiscount = await _priceFormatter
                    .FormatOrderPriceAsync(-order.OrderDiscount, order.CurrencyRate, order.CustomerCurrencyCode,
                    _orderSettings.DisplayCustomerCurrencyOnOrders, primaryStoreCurrency, languageId, null, false);
            }
            model.OrderTotalDiscountValue = order.OrderDiscount;

            //gift cards
            foreach (var gcuh in await _giftCardService.GetGiftCardUsageHistoryAsync(order))
            {
                model.GiftCards.Add(new OrderModel.GiftCard
                {
                    CouponCode = (await _giftCardService.GetGiftCardByIdAsync(gcuh.GiftCardId)).GiftCardCouponCode,
                    Amount = await _priceFormatter.FormatPriceAsync(-gcuh.UsedValue, true, false)
                });
            }

            //reward points
            if (order.RedeemedRewardPointsEntryId.HasValue && await _rewardPointService.GetRewardPointsHistoryEntryByIdAsync(order.RedeemedRewardPointsEntryId.Value) is RewardPointsHistory redeemedRewardPointsEntry)
            {
                model.RedeemedRewardPoints = -redeemedRewardPointsEntry.Points;
                model.RedeemedRewardPointsAmount =
                    await _priceFormatter.FormatPriceAsync(-redeemedRewardPointsEntry.UsedAmount, true, false);
            }

            //total
            model.OrderTotal = await _priceFormatter
                .FormatOrderPriceAsync(order.OrderTotal, order.CurrencyRate, order.CustomerCurrencyCode,
                _orderSettings.DisplayCustomerCurrencyOnOrders, primaryStoreCurrency, languageId, null, false);
            model.OrderTotalValue = order.OrderTotal;

            //refunded amount
            if (order.RefundedAmount > decimal.Zero)
                model.RefundedAmount = await _priceFormatter.FormatPriceAsync(order.RefundedAmount, true, false);

            //used discounts
            var duh = await _discountService.GetAllDiscountUsageHistoryAsync(orderId: order.Id);
            foreach (var d in duh)
            {
                var discount = await _discountService.GetDiscountByIdAsync(d.DiscountId);

                model.UsedDiscounts.Add(new OrderModel.UsedDiscountModel
                {
                    DiscountId = d.DiscountId,
                    DiscountName = discount.Name
                });
            }

            //profit (hide for vendors)
            if (await _workContext.GetCurrentVendorAsync() != null)
                return;

            var profit = await _orderReportService.ProfitReportAsync(orderId: order.Id);
            model.Profit = await _priceFormatter
                .FormatOrderPriceAsync(profit, order.CurrencyRate, order.CustomerCurrencyCode,
                _orderSettings.DisplayCustomerCurrencyOnOrders, primaryStoreCurrency, languageId, null, false);
        }


        /// <summary>
        /// Prepare order item models
        /// </summary>
        /// <param name="models">List of order item models</param>
        /// <param name="order">Order</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        protected virtual async Task PrepareOrderItemModelsAsync(IList<OrderItemModel> models, Order order)
        {
            if (models == null)
                throw new ArgumentNullException(nameof(models));

            if (order == null)
                throw new ArgumentNullException(nameof(order));

            var primaryStoreCurrency = await _currencyService.GetCurrencyByIdAsync(_currencySettings.PrimaryStoreCurrencyId);

            //get order items
            var vendor = await _workContext.GetCurrentVendorAsync();
            var orderItems = await _orderService.GetOrderItemsAsync(order.Id, vendorId: vendor?.Id ?? 0);

            foreach (var orderItem in orderItems)
            {
                var product = await _productService.GetProductByIdAsync(orderItem.ProductId);

                //fill in model values from the entity
                var orderItemModel = new OrderItemModel
                {
                    Id = orderItem.Id,
                    ProductId = orderItem.ProductId,
                    ProductName = await _productService.GetProductItemName(product, orderItem), //++Alchub
                    Quantity = orderItem.Quantity,
                    IsDownload = product.IsDownload,
                    DownloadCount = orderItem.DownloadCount,
                    DownloadActivationType = product.DownloadActivationType,
                    IsDownloadActivated = orderItem.IsDownloadActivated,
                    UnitPriceInclTaxValue = orderItem.UnitPriceInclTax,
                    UnitPriceExclTaxValue = orderItem.UnitPriceExclTax,
                    DiscountInclTaxValue = orderItem.DiscountAmountInclTax,
                    DiscountExclTaxValue = orderItem.DiscountAmountExclTax,
                    SubTotalInclTaxValue = orderItem.PriceInclTax,
                    SubTotalExclTaxValue = orderItem.PriceExclTax,
                    AttributeInfo = orderItem.AttributeDescription,
                    CustomAttributeInfo = orderItem.CustomAttributesDescription
                };

                //fill in additional values (not existing in the entity)
                var productVendor = await _vendorService.GetVendorByIdAsync(product.VendorId);
                //orderItemModel.Sku = await _productService.FormatSkuAsync(product, orderItem.AttributesXml);
                orderItemModel.Sku = product.UPCCode;//++Alchub //Note:(10-12-22) Now UPCCODE is SKU
                orderItemModel.VendorName = productVendor != null ? productVendor.Name : "";

                //Alchub Slot 
                orderItemModel.SlotId = orderItem.SlotId;
                orderItemModel.SlotPrice = orderItem.SlotPrice;
                orderItemModel.SlotStartTime = orderItem.SlotStartTime != null ? orderItem.SlotStartTime.ToString("MM/dd/yyyy") : "";
                orderItemModel.SlotTime = orderItem.SlotTime != null ? SlotHelper.ConvertTo12hoursSlotTime(orderItem.SlotTime) : "";
                orderItemModel.InPickup = orderItem.InPickup;
                orderItemModel.OrderItemStatusId = orderItem.OrderItemStatusId;
                orderItemModel.OrderItemStatus = await _localizationService.GetLocalizedEnumAsync(orderItem.OrderItemStatus);
                orderItemModel.IsVendorManageDelivery = productVendor != null ? productVendor.ManageDelivery : false;

                //picture
                //++Alchub masterproduct picture.
                var masterProduct = await _productService.GetProductByIdAsync(orderItem.MasterProductId);
                if (masterProduct == null)
                    masterProduct = product;
                var orderItemPicture = await _pictureService.GetProductPictureAsync(masterProduct, orderItem.AttributesXml);
                //(orderItemModel.PictureThumbnailUrl, _) = await _pictureService.GetPictureUrlAsync(orderItemPicture, 75);
                //orderItemModel.PictureThumbnailUrl = await _pictureService.GetProductPictureUrlAsync(product.Sku, 75);
                orderItemModel.PictureThumbnailUrl = product.ImageUrl;

                //license file
                if (orderItem.LicenseDownloadId.HasValue)
                {
                    orderItemModel.LicenseDownloadGuid = (await _downloadService
                        .GetDownloadByIdAsync(orderItem.LicenseDownloadId.Value))?.DownloadGuid ?? Guid.Empty;
                }

                var languageId = (await _workContext.GetWorkingLanguageAsync()).Id;

                //unit price
                orderItemModel.UnitPriceInclTax = await _priceFormatter
                    .FormatOrderPriceAsync(orderItem.UnitPriceInclTax, order.CurrencyRate, order.CustomerCurrencyCode,
                    _orderSettings.DisplayCustomerCurrencyOnOrders, primaryStoreCurrency, languageId, true, true);
                orderItemModel.UnitPriceExclTax = await _priceFormatter
                    .FormatOrderPriceAsync(orderItem.UnitPriceExclTax, order.CurrencyRate, order.CustomerCurrencyCode,
                    _orderSettings.DisplayCustomerCurrencyOnOrders, primaryStoreCurrency, languageId, false, true);

                //discounts
                orderItemModel.DiscountInclTax = await _priceFormatter
                    .FormatOrderPriceAsync(orderItem.DiscountAmountInclTax, order.CurrencyRate, order.CustomerCurrencyCode,
                    _orderSettings.DisplayCustomerCurrencyOnOrders, primaryStoreCurrency, languageId, true, true);
                orderItemModel.DiscountExclTax = await _priceFormatter
                    .FormatOrderPriceAsync(orderItem.DiscountAmountExclTax, order.CurrencyRate, order.CustomerCurrencyCode,
                    _orderSettings.DisplayCustomerCurrencyOnOrders, primaryStoreCurrency, languageId, false, true);

                //subtotal
                orderItemModel.SubTotalInclTax = await _priceFormatter
                    .FormatOrderPriceAsync(orderItem.PriceInclTax, order.CurrencyRate, order.CustomerCurrencyCode,
                    _orderSettings.DisplayCustomerCurrencyOnOrders, primaryStoreCurrency, languageId, true, true);
                orderItemModel.SubTotalExclTax = await _priceFormatter
                    .FormatOrderPriceAsync(orderItem.PriceExclTax, order.CurrencyRate, order.CustomerCurrencyCode,
                    _orderSettings.DisplayCustomerCurrencyOnOrders, primaryStoreCurrency, languageId, false, true);

                //recurring info
                if (product.IsRecurring)
                {
                    orderItemModel.RecurringInfo = string.Format(await _localizationService.GetResourceAsync("Admin.Orders.Products.RecurringPeriod"),
                        product.RecurringCycleLength, await _localizationService.GetLocalizedEnumAsync(product.RecurringCyclePeriod));
                }

                //rental info
                if (product.IsRental)
                {
                    var rentalStartDate = orderItem.RentalStartDateUtc.HasValue
                        ? _productService.FormatRentalDate(product, orderItem.RentalStartDateUtc.Value) : string.Empty;
                    var rentalEndDate = orderItem.RentalEndDateUtc.HasValue
                        ? _productService.FormatRentalDate(product, orderItem.RentalEndDateUtc.Value) : string.Empty;
                    orderItemModel.RentalInfo = string.Format(await _localizationService.GetResourceAsync("Order.Rental.FormattedDate"),
                        rentalStartDate, rentalEndDate);
                }

                //prepare return request models
                await PrepareReturnRequestBriefModelsAsync(orderItemModel.ReturnRequests, orderItem);

                //gift card identifiers
                orderItemModel.PurchasedGiftCardIds = (await _giftCardService
                    .GetGiftCardsByPurchasedWithOrderItemIdAsync(orderItem.Id)).Select(card => card.Id).ToList();

                models.Add(orderItemModel);
            }
        }
        #endregion
    }
}