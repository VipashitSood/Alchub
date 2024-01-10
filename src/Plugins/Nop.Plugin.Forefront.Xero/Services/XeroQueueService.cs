using Newtonsoft.Json;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Tax;
using Nop.Data;
using Nop.Plugin.Forefront.Xero.Domain;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Discounts;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xero.NetStandard.OAuth2.Api;
using Xero.NetStandard.OAuth2.Model;
using Xero.NetStandard.OAuth2.Model.Accounting;

namespace Nop.Plugin.Forefront.Xero.Services
{
	public class XeroQueueService : IXeroQueueService
    {
        #region Fields

        private readonly IRepository<XeroQueue> _queueRepository;
        private readonly IOrderService _orderService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly ILogger _logger;
        private readonly IWorkContext _workContext;
        private readonly ISettingService _settingService;
        private readonly IRepository<XeroAccounting> _xeroAccountingRepository;
        private readonly ICheckoutAttributeParser _checkoutAttributeParser;
        private readonly TaxSettings _taxSettings;
        private readonly ICurrencyService _currencyService;
        private readonly ICustomerService _customerService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IXeroAccessRefreshTokenService _xeroAccessRefreshTokenService;
        private readonly IStoreContext _storeContext;
        private readonly IAddressService _addressService;
        private readonly ICountryService _countryService;
        private readonly IProductService _productService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly IDiscountService _discountService;
        private readonly IRewardPointService _rewardPointService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly LocalizationSettings _localizationSettings;
        private readonly ILanguageService _languageService;
        private readonly IXeroProductService _xeroProductService;
        #endregion

        #region Ctor

        public XeroQueueService(IRepository<XeroQueue> queueRepository,
            IOrderService orderService, IOrderProcessingService orderProcessingService, ILogger logger,
            IWorkContext workContext,
            IXeroProductService xeroProductService, ISettingService settingService,
            IRepository<XeroAccounting> xeroAccountingRepository,
            ICheckoutAttributeParser checkoutAttributeParser,
            TaxSettings taxSettings, ICurrencyService currencyService,
            ICustomerService customerService,
            IGenericAttributeService genericAttributeService,
            IXeroAccessRefreshTokenService xeroAccessRefreshTokenService,
            IStoreContext storeContext,
            IAddressService addressService,
            ICountryService countryService,
            IProductService productService,
            IStateProvinceService stateProvinceService,
            IDiscountService discountService,
            IRewardPointService rewardPointService,
            IPriceFormatter priceFormatter,
            LocalizationSettings localizationSettings,
            ILanguageService languageService)
        {
            _queueRepository = queueRepository;
            _orderService = orderService;
            _orderProcessingService = orderProcessingService;
            _logger = logger;
            _workContext = workContext;
            _settingService = settingService;
            _xeroAccountingRepository = xeroAccountingRepository;
            _checkoutAttributeParser = checkoutAttributeParser;
            _taxSettings = taxSettings;
            _currencyService = currencyService;
            _customerService = customerService;
            _genericAttributeService = genericAttributeService;
            _xeroAccessRefreshTokenService = xeroAccessRefreshTokenService;
            _storeContext = storeContext;
            _addressService = addressService;
            _countryService = countryService;
            _productService = productService;
            _stateProvinceService = stateProvinceService;
            _discountService = discountService;
            _rewardPointService = rewardPointService;
            _priceFormatter = priceFormatter;
            _localizationSettings = localizationSettings;
            _languageService = languageService;
            _xeroProductService = xeroProductService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public virtual async Task<IPagedList<XeroQueue>> AllXeroQueue(int pageIndex = 0, int pageSize = 2147483647)
        {
            var query = (from o in _queueRepository.Table
                orderby o.QueuedOn descending
                select o).ToList();

            return  new  PagedList<XeroQueue>(query, pageIndex, pageSize);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="xeroQueue"></param>
		public async Task CancelInvoice(IList<XeroQueue> xeroQueue)
        {
            if (xeroQueue.Count > 0)
            {
                try
                {
                    var record = await _xeroAccessRefreshTokenService.CheckRecordExist();
                    if (record != null)
                    {
                        IAccountingApi accountingApi = new AccountingApi();
                        foreach (var queue in xeroQueue)
                        {
                            if (queue.OrderId != 0)
                            {
                                var parentQueueByOrderId = await GetParentQueueByOrderId(queue.OrderId);
                                if (parentQueueByOrderId != null)
                                {
                                    if (parentQueueByOrderId.XeroId != null)
                                    {
                                        var invoiceApi = accountingApi.GetInvoiceAsync(record.AccessToken, record.Tenants[0].TenantId.ToString(), Guid.Parse(parentQueueByOrderId.XeroId)).GetAwaiter();
                                        var invoice = invoiceApi.GetResult()._Invoices.FirstOrDefault(p => p.InvoiceID == Guid.Parse(parentQueueByOrderId.XeroId));
                                        if (invoice != null)
                                        {
                                            invoice.Status = Invoice.StatusEnum.VOIDED;

                                            var updateInvoiceApi = accountingApi.UpdateInvoiceAsync(record.AccessToken, record.Tenants[0].TenantId.ToString(), Guid.Parse(parentQueueByOrderId.XeroId), invoiceApi.GetResult()).GetAwaiter();
                                            var invoices = updateInvoiceApi.GetResult()._Invoices;
                                            var result = invoices.FirstOrDefault(p => p.InvoiceID == invoice.InvoiceID);

                                            if (result != null)
                                            {
                                                result.Warnings = new List<ValidationError>();
                                                result.ValidationErrors = new List<ValidationError>();

                                                if (result.Warnings.Count > 0)
                                                {
                                                    queue.ResponseMessages = JsonConvert.SerializeObject(result.Warnings);
                                                }
                                                else if (result.ValidationErrors.Count > 0)
                                                {
                                                    queue.ResponseMessages = JsonConvert.SerializeObject(result.ValidationErrors);
                                                }
                                                else
                                                {
                                                    queue.IsSuccess = true;
                                                    queue.IsPaid = false;
                                                    queue.ResponseData = JsonConvert.SerializeObject(result);
                                                    queue.XeroId = result.InvoiceID.ToString();
                                                }

                                                queue.SyncAttemptCount = queue.SyncAttemptCount + 1;
                                                queue.SyncAttemptOn = DateTime.UtcNow;
                                               await UpdateQueue(queue);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception )
                {

                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="xeroQueue"></param>
		public virtual async Task CreateXeroInvoice(IList<XeroQueue> xeroQueueList)
        {
            var record = await _xeroAccessRefreshTokenService.CheckRecordExist();
            if (record != null)
            {
                IAccountingApi accountingApi = new AccountingApi();

                double? num;
                if (xeroQueueList.Count > 0)
                {
                    int activeStoreScopeConfiguration = await _storeContext.GetActiveStoreScopeConfigurationAsync();
                    var setting = await _settingService.LoadSettingAsync<ForeFrontXeroSetting>(activeStoreScopeConfiguration);

                    int batchSize = 0;
                    batchSize = setting.BatchSize <= 0 ? 100 : setting.BatchSize;

                    decimal number = new decimal();
                    number = xeroQueueList.Count > batchSize ? xeroQueueList.Count / batchSize : new decimal();

                    var currencyList = new List<Currency>();
                    var currenciesApi = accountingApi.GetCurrenciesAsync(record.AccessToken, record.Tenants[0].TenantId.ToString()).GetAwaiter();
                    var currencies = currenciesApi.GetResult();
                    currencyList.AddRange(currencies._Currencies);

                    var taxRateList = new List<TaxRate>();
                    var taxRatesApi = accountingApi.GetTaxRatesAsync(record.AccessToken, record.Tenants[0].TenantId.ToString()).GetAwaiter();
                    var taxRates = taxRatesApi.GetResult();
                    taxRateList.AddRange(taxRates._TaxRates);

                    string defaultCode, standardTaxCategoryCode, zeroTaxCategoryCode, shippingCode, discountCode, paymentMethodAdditonalFee, rewardPoints, empty = string.Empty;
                    defaultCode = "200";
                    standardTaxCategoryCode = string.IsNullOrEmpty(setting.SalesAccountCode) ? defaultCode : setting.SalesAccountCode;
                    zeroTaxCategoryCode = string.IsNullOrEmpty(setting.NonVatAccountCode) ? defaultCode : setting.NonVatAccountCode;
                    shippingCode = string.IsNullOrEmpty(setting.PostageAccountCode) ? defaultCode : setting.PostageAccountCode;
                    discountCode = string.IsNullOrEmpty(setting.DiscountAmountAccountCode) ? defaultCode : setting.DiscountAmountAccountCode;
                    paymentMethodAdditonalFee = string.IsNullOrEmpty(setting.PaymentMethodAdditonalFeeAccountCode) ? defaultCode : setting.PaymentMethodAdditonalFeeAccountCode;
                    rewardPoints = string.IsNullOrEmpty(setting.RewardPointsAccountCode) ? defaultCode : setting.RewardPointsAccountCode;

                    for (int i = 0; i <= Math.Round(number); i++)
                    {
                        var queueList = await GetQueueForInvoice(batchSize);
                        var invoiceList = new List<Invoice>();

                        foreach (var xeroQueue in queueList)
                        {
                            if (xeroQueue.OrderId != 0)
                            {
                                var order = await _orderService.GetOrderByIdAsync(xeroQueue.OrderId);
                                if (order != null)
                                {
                                    var invoice = new Invoice();
                                    var contact = new Contact();
                                    var customer = await _customerService.GetCustomerByIdAsync(order.CustomerId);
                                    var billingAddress = await _addressService.GetAddressByIdAsync(order.BillingAddressId);
                                    var billingCountry = await _countryService.GetCountryByAddressAsync(billingAddress);
                                    if (customer.Username != null)
                                    {
                                        contact.Name = await _customerService.GetCustomerFullNameAsync(customer);

                                        if (await _genericAttributeService.GetAttributeAsync<string>(customer, NopCustomerDefaults.PhoneAttribute, 0) != null)
                                        {
                                            contact.ContactNumber =await _genericAttributeService.GetAttributeAsync<string>(customer, NopCustomerDefaults.PhoneAttribute, 0);
                                        }

                                        contact.FirstName =await _genericAttributeService.GetAttributeAsync<string>(customer, NopCustomerDefaults.FirstNameAttribute, 0);
                                        contact.LastName =await _genericAttributeService.GetAttributeAsync<string>(customer, NopCustomerDefaults.LastNameAttribute, 0);
                                        contact.EmailAddress = (customer.Email != null ? customer.Email : "test@gmail.com");
                                    }
                                    else if (billingAddress == null)
                                    {
                                        contact.EmailAddress = "guest@gmail.com";
                                    }
                                    else
                                    {
                                        contact.Name = string.Concat(billingAddress.FirstName, " ", billingAddress.LastName);

                                        if (billingAddress.PhoneNumber != null)
                                        {
                                            contact.ContactNumber = billingAddress.PhoneNumber;
                                        }

                                        contact.FirstName = billingAddress.FirstName;
                                        contact.LastName = billingAddress.LastName;
                                        contact.EmailAddress = (billingAddress.Email != null ? billingAddress.Email : "test@gmail.com");
                                    }

                                    contact.IsCustomer = true;
                                    invoice.Contact = contact;
                                    var shippingAddress =await _addressService.GetAddressByIdAsync(order.ShippingAddressId.Value);
                                    var shippingCountry =await _countryService.GetCountryByAddressAsync(shippingAddress);
                                    if ((billingAddress == null ? true : shippingAddress == null))
                                    {
                                        //_logger.InsertLog(40, "Xero Invoice Integration ", string.Concat("shipping address and billing address require to create this invoice ", orderById.Id), null);
                                    }
                                    else
                                    {
                                        var addressList = new List<Address>();

                                        var billingAddresses = new Address()
                                        {
                                            AddressType = Address.AddressTypeEnum.POBOX,
                                            AddressLine1 = (billingAddress.Address1 == null ? "" : billingAddress.Address1),
                                            AddressLine2 = (billingAddress.Address2 == null ? "" : billingAddress.Address2),
                                            City = (billingAddress.City == null ? "" : billingAddress.City)
                                        };
                                        var biliingStateProvinceService =await _stateProvinceService.GetStateProvinceByAddressAsync(billingAddress);
                                        if (biliingStateProvinceService == null)
                                        {
                                            billingAddresses.Region = "";
                                        }
                                        else
                                        {
                                            billingAddresses.Region = (biliingStateProvinceService.Name == null ? "" : biliingStateProvinceService.Name);
                                        }

                                        billingAddresses.PostalCode = (billingAddress.ZipPostalCode == null ? "" : billingAddress.ZipPostalCode);

                                        if (billingCountry == null)
                                        {
                                            billingAddresses.Country = "";
                                        }
                                        else
                                        {
                                            billingAddresses.Country = (billingCountry.Name == null ? "" : billingCountry.Name);
                                        }

                                        addressList.Add(billingAddresses);

                                        var shippingAddresses = new Address()
                                        {
                                            AddressType = Address.AddressTypeEnum.STREET,
                                            AddressLine1 = (shippingAddress.Address1 == null ? "" : shippingAddress.Address1),
                                            AddressLine2 = (shippingAddress.Address2 == null ? "" : shippingAddress.Address2),
                                            City = (shippingAddress.City == null ? "" : shippingAddress.City)
                                        };
                                        var shippingStateProvinceService =await _stateProvinceService.GetStateProvinceByAddressAsync(shippingAddress);
                                        if (shippingStateProvinceService == null)
                                        {
                                            shippingAddresses.Region = "";
                                        }
                                        else
                                        {
                                            shippingAddresses.Region = (shippingStateProvinceService.Name == null ? "" : shippingStateProvinceService.Name);
                                        }

                                        shippingAddresses.PostalCode = (shippingAddress.ZipPostalCode == null ? "" : shippingAddress.ZipPostalCode);

                                        if (shippingCountry == null)
                                        {
                                            shippingAddresses.Country = "";
                                        }
                                        else
                                        {
                                            shippingAddresses.Country = (shippingCountry.Name == null ? "" : shippingCountry.Name);
                                        }

                                        addressList.Add(shippingAddresses);
                                        invoice.Contact.Addresses = addressList;
                                    }

                                    var lineItemList = new List<LineItem>();
                                    string common = string.Empty;
                                    var orderItems = await _orderService.GetOrderItemsAsync(order.Id);
                                    foreach (var orderItem in orderItems)
                                    {
                                        var product = await _productService.GetProductByIdAsync(orderItem.ProductId);
                                        IList<Nop.Core.Domain.Catalog.Product> products = new List<Nop.Core.Domain.Catalog.Product>();
                                        products.Add(product);
                                        await _xeroProductService.CreateXeroItemOrderTime(products);
                                        empty = product.TaxCategoryId > 0 ? product.TaxCategoryId == 1 ? standardTaxCategoryCode : zeroTaxCategoryCode : zeroTaxCategoryCode;

                                        decimal unitPriceInclTax = orderItem.UnitPriceInclTax - orderItem.UnitPriceExclTax;
                                        var lineItem = new LineItem()
                                        {
                                            Description = product.Name.ToString(),
                                            Quantity = orderItem.Quantity
                                        };

                                        if (_taxSettings.TaxDisplayType != 0)
                                        {
                                            lineItem.UnitAmount = (decimal?)(float)(orderItem.UnitPriceExclTax + (orderItem.DiscountAmountExclTax / orderItem.Quantity));
                                            lineItem.TaxAmount = (decimal?)((float)unitPriceInclTax * orderItem.Quantity);
                                        }
                                        else
                                        {
                                            lineItem.UnitAmount = (decimal?)(float)(orderItem.UnitPriceInclTax + (orderItem.DiscountAmountInclTax / orderItem.Quantity));
                                            lineItem.TaxAmount = (decimal?)(float)(unitPriceInclTax * orderItem.Quantity);
                                        }

                                        if (lineItem.TaxAmount > 0)
                                        {
                                            lineItem.AccountCode = standardTaxCategoryCode;
                                        }
                                        else
                                        {
                                            lineItem.AccountCode = zeroTaxCategoryCode;
                                        }

                                        lineItem.TaxType = "OUTPUT";
                                        lineItem.ItemCode = product.UPCCode;

                                        var discountUsageHistory = await _discountService.GetAllDiscountUsageHistoryAsync(orderId: order.Id);
                                        var lineItemListNew = new List<LineItem>();

                                        foreach (var discountHistory in discountUsageHistory)
                                        {
                                            var discount = await _discountService.GetDiscountByIdAsync(discountHistory.DiscountId);
                                            if ((int)discount.DiscountType == 2)
                                            {
                                               if (!discount.UsePercentage)
                                                  {
                                                        var lineItem1 = new LineItem()
                                                        {
                                                            Description = string.Concat("Fixed discount on ", product.Name),
                                                            UnitAmount = (decimal?)(float)-discount.DiscountAmount,
                                                            LineAmount = (decimal?)((float)-discount.DiscountAmount * orderItem.Quantity),
                                                            Quantity = orderItem.Quantity,
                                                            AccountCode = empty
                                                        };

                                                        num = new double();
                                                        lineItem1.TaxAmount = (decimal?)num;
                                                        lineItemListNew.Add(lineItem1);
                                                    }
                                                    else
                                                    {
                                                        lineItem.DiscountRate = (decimal?)Convert.ToDouble(discount.DiscountPercentage);
                                                    }
                                                
                                            }

                                            if ((int)discount.DiscountType == 5)
                                            {
                                                if (!discount.UsePercentage)
                                                {
                                                    var lineItem2 = new LineItem()
                                                    {
                                                        Description = string.Concat("Fixed discount on ", product.Name),
                                                        UnitAmount = (decimal?)(float)(-discount.DiscountAmount),
                                                        LineAmount = (decimal?)(float)(-discount.DiscountAmount * orderItem.Quantity),
                                                        Quantity = orderItem.Quantity,
                                                        AccountCode = empty
                                                    };

                                                    num = new double();
                                                    lineItem2.TaxAmount = (decimal?)num;
                                                    lineItemListNew.Add(lineItem2);
                                                }
                                                else
                                                {
                                                    lineItem.DiscountRate = (decimal?)Convert.ToDouble(discount.DiscountPercentage);
                                                }
                                            }

                                            if ((int)discount.DiscountType == 6)
                                            {
                                                if (!discount.UsePercentage)
                                                {
                                                    var lineItem3 = new LineItem()
                                                    {
                                                        Description = string.Concat("Fixed discount on ", product.Name),
                                                        UnitAmount = (decimal?)(float)-discount.DiscountAmount,
                                                        LineAmount = (decimal?)(float)-discount.DiscountAmount * orderItem.Quantity,
                                                        AccountCode = empty
                                                    };

                                                    num = new double();
                                                    lineItem3.TaxAmount = (decimal?)num;
                                                    lineItemListNew.Add(lineItem3);
                                                }
                                                else
                                                {
                                                    lineItem.DiscountRate = (decimal?)Convert.ToDouble(discount.DiscountPercentage);
                                                }
                                            }
                                        }

                                        lineItemList.Add(lineItem);

                                        foreach (var line in lineItemListNew)
                                        {
                                            lineItemList.Add(line);
                                        }
                                    }

                                    var checkoutAttributeList = _checkoutAttributeParser.ParseCheckoutAttributeValues(order.CheckoutAttributesXml);
                                    string str = null;
                                    decimal priceAdjustment = new decimal();
                                    var attributeValues = _checkoutAttributeParser.ParseCheckoutAttributeValues(order.CheckoutAttributesXml);
                                    foreach (var attributeValue in await attributeValues.SelectMany(x => x.values).ToListAsync())
                                    {
                                        priceAdjustment += attributeValue.PriceAdjustment;
                                        if (priceAdjustment > decimal.Zero)
                                        {
                                            str = string.Concat(str, attributeValue.Name, ",");
                                        }
                                    }
                                    decimal orderShippingInclTax = order.OrderShippingInclTax;
                                    if (priceAdjustment > decimal.Zero)
                                    {
                                        var lineItem4 = new LineItem()
                                        {
                                            Description = string.Concat("Checkout Attribute (", str, ")"),
                                            UnitAmount = (decimal?)(float)priceAdjustment,
                                            LineAmount = (decimal?)(float)priceAdjustment,
                                            AccountCode = zeroTaxCategoryCode
                                        };

                                        num = new double();
                                        lineItem4.TaxAmount = (decimal?)num;
                                        lineItemList.Add(lineItem4);
                                    }

                                    if (orderShippingInclTax > decimal.Zero)
                                    {
                                        var lineItem5 = new LineItem()
                                        {
                                            Description = "Shipping Charge",
                                            UnitAmount = (decimal?)(float)orderShippingInclTax,
                                            TaxAmount = (decimal?)(float)((orderShippingInclTax) - (orderShippingInclTax - orderShippingInclTax / (100 + 20) * 20)),
                                            AccountCode = shippingCode
                                        };

                                        lineItemList.Add(lineItem5);
                                    }

                                    decimal orderDiscount = order.OrderDiscount + order.OrderSubTotalDiscountInclTax;
                                    if (orderDiscount > decimal.Zero)
                                    {
                                        var lineItem6 = new LineItem()
                                        {
                                            Description = "Discount Amount",
                                            UnitAmount = (decimal?)(float)-orderDiscount,
                                            LineAmount = (decimal?)(float)-orderDiscount,
                                            AccountCode = discountCode
                                        };

                                        num = new double();
                                        lineItem6.TaxAmount = (decimal?)num;
                                        lineItemList.Add(lineItem6);
                                    }

                                    if (order.PaymentMethodAdditionalFeeInclTax > decimal.Zero)
                                    {
                                        var lineItem7 = new LineItem()
                                        {
                                            Description = "Payment method additional fee",
                                            UnitAmount = (decimal?)(float)order.PaymentMethodAdditionalFeeInclTax,
                                            LineAmount = (decimal?)(float)order.PaymentMethodAdditionalFeeInclTax,
                                            AccountCode = paymentMethodAdditonalFee,
                                            TaxAmount = (decimal?)(float)((order.PaymentMethodAdditionalFeeInclTax - order.PaymentMethodAdditionalFeeExclTax))
                                        };

                                        lineItemList.Add(lineItem7);
                                    }

                                    if (order.RedeemedRewardPointsEntryId != null)
                                    {
                                        var redeemedRewardPointsEntry = await _rewardPointService.GetRewardPointsHistoryEntryByIdAsync(order.RedeemedRewardPointsEntryId.Value);
                                        decimal amount = -redeemedRewardPointsEntry.Points;
                                        var language = await _languageService.GetLanguageByIdAsync(_localizationSettings.DefaultAdminLanguageId);
                                        string points =await _priceFormatter.FormatPriceAsync(-(_currencyService.ConvertCurrency(redeemedRewardPointsEntry.UsedAmount, order.CurrencyRate)), true, order.CustomerCurrencyCode, false, language.Id);
                                        decimal point = Convert.ToDecimal(points);
                                        var lineItem8 = new LineItem()
                                        {
                                            Description = "Reward Points",
                                            Quantity = (decimal?)(float)point,
                                            UnitAmount = (decimal?)(float)-(amount / point),
                                            LineAmount = (decimal?)(float)-amount,
                                            AccountCode = rewardPoints
                                        };

                                        num = new double();
                                        lineItem8.TaxAmount = (decimal?)num ;
                                        lineItemList.Add(lineItem8);
                                    }

                                    if (order.ServiceFee > decimal.Zero)
                                    {
                                        var lineItem9 = new LineItem()
                                        {
                                            Description = "Service Fee",
                                            UnitAmount = (decimal?)(float)order.ServiceFee,
                                            LineAmount = (decimal?)(float)order.ServiceFee,
                                            AccountCode = shippingCode
                                        };

                                        lineItemList.Add(lineItem9);
                                    }
                                    if (order.DeliveryFee > decimal.Zero)
                                    {
                                        var lineItem10 = new LineItem()
                                        {
                                            Description = "Delivery Fee",
                                            UnitAmount = (decimal?)(float)order.DeliveryFee,
                                            LineAmount = (decimal?)(float)order.DeliveryFee,
                                            AccountCode = "200"
                                        };

                                        lineItemList.Add(lineItem10);
                                    }
                                    if (order.TipFee > decimal.Zero)
                                    {
                                        var lineItem11 = new LineItem()
                                        {
                                            Description = "Tip Fee",
                                            UnitAmount = (decimal?)(float)order.TipFee,
                                            LineAmount = (decimal?)(float)order.TipFee,
                                            AccountCode = "200"
                                        };

                                        lineItemList.Add(lineItem11);
                                    }
                                    if (order.SlotFee > decimal.Zero)
                                    {
                                        var lineItem12 = new LineItem()
                                        {
                                            Description = "Slot Fee",
                                            UnitAmount = (decimal?)(float)order.SlotFee,
                                            LineAmount = (decimal?)(float)order.SlotFee,
                                            AccountCode = "200"
                                        };

                                        lineItemList.Add(lineItem12);
                                    }

                                    invoice.LineItems = lineItemList;
                                    invoice.Type = Invoice.TypeEnum.ACCREC;
                                    invoice.Status = Invoice.StatusEnum.AUTHORISED;
                                    invoice.Date = order.CreatedOnUtc;
                                    invoice.DueDate = DateTime.UtcNow;

                                    if (_taxSettings.TaxDisplayType != 0)
                                    {
                                        invoice.LineAmountTypes = LineAmountTypes.Exclusive;
                                    }
                                    else
                                    {
                                        invoice.LineAmountTypes = LineAmountTypes.Inclusive;
                                    }

                                    invoice.Reference = xeroQueue.OrderId.ToString();
                                    var cyCode = await _workContext.GetWorkingCurrencyAsync();
                                    var currencyCode = (CurrencyCode)Enum.Parse(typeof(CurrencyCode), cyCode.CurrencyCode);
                                    if (currencyList.FirstOrDefault(p => p.Code == currencyCode) == null)
                                    {
                                        var currency = new Currency()
                                        {
                                            Code = currencyCode
                                        };

                                        var currencyApi = accountingApi.CreateCurrencyAsync(record.AccessToken, record.Tenants[0].TenantId.ToString(), currency).GetAwaiter();
                                        currencyList.Add(currency);
                                    }

                                    invoice.CurrencyRate = cyCode.Rate;
                                    invoice.CurrencyCode = currencyCode;
                                    invoiceList.Add(invoice);
                                }
                            }
                        }
                        try
                        {
                            if (invoiceList.Count > 0)
                            {
                                var invoices = new Invoices();
                                invoices._Invoices = new List<Invoice>();
                                invoices._Invoices.AddRange(invoiceList);
                                var postInvoices = accountingApi.CreateInvoicesAsync(record.AccessToken, record.Tenants[0].TenantId.ToString(), invoices, false).GetAwaiter();

                                foreach (var result in postInvoices.GetResult()._Invoices)
                                {
                                    var queueByOrderId = await GetQueueByOrderId(int.Parse(result.Reference));
                                    result.Warnings = new List<ValidationError>();
                                    result.ValidationErrors = new List<ValidationError>();

                                    if (result.Warnings.Count > 0)
                                    {
                                        queueByOrderId.ResponseMessages = JsonConvert.SerializeObject(result.Warnings);
                                    }
                                    else if (result.ValidationErrors.Count > 0)
                                    {
                                        queueByOrderId.ResponseMessages = JsonConvert.SerializeObject(result.ValidationErrors);
                                    }
                                    else
                                    {
                                        queueByOrderId.IsSuccess = true;
                                        queueByOrderId.IsPaid = false;
                                        queueByOrderId.ResponseData = JsonConvert.SerializeObject(result);
                                        queueByOrderId.XeroId = result.InvoiceID.ToString();
                                    }

                                    queueByOrderId.SyncAttemptCount = queueByOrderId.SyncAttemptCount + 1;
                                    queueByOrderId.SyncAttemptOn = DateTime.UtcNow;
                                   await UpdateQueue(queueByOrderId);
                                }
                            }
                        }
                        catch (Exception )
                        {

                        }
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="XeroAccounting"></param>
		public virtual async Task DeleteAccountMap(XeroAccounting xeroAccounting)
        {
            if (xeroAccounting == null)
            {
                throw new ArgumentNullException("XeroAccounting");
            }

           await _xeroAccountingRepository.DeleteAsync(xeroAccounting);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="queue"></param>
		public virtual async Task DeleteQueue(XeroQueue queue)
        {
            if (queue == null)
            {
                throw new ArgumentNullException("XeroQueue");
            }

          await  _queueRepository.DeleteAsync(queue);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
		public virtual async Task <IList<XeroQueue>> GetAllQueue()
        {
            var list = (from q in _queueRepository.Table
                select q).ToList();

            return list;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
		public virtual async Task<IList<XeroQueue>> GetAllXeroQueueByOrderIdAndXeroId()
        {
            var list = (from q in _queueRepository.Table
                where q.OrderId != 0 && q.XeroId != null
                select q).ToList();

            return list;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
		public async Task<XeroQueue> GetParentQueueByOrderId(int orderId)
        {
            if (orderId == 0)
            {
                throw new ArgumentNullException("orderId");
            }

            var xeroQueue = (
                from q in _queueRepository.Table
                where q.OrderId == orderId && q.ActionType == "Invoice"
                select q).FirstOrDefault();

            return xeroQueue;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
		public async Task<XeroQueue> GetPaymentQueueByOrderId(int orderId)
        {
            if (orderId == 0)
            {
                throw new ArgumentNullException("orderId");
            }

            var xeroQueue = (from q in _queueRepository.Table
                where q.OrderId == orderId && q.ActionType == "Payment"
                select q).FirstOrDefault();

            return xeroQueue;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
		public async Task<XeroQueue> GetQueueById(int id)
        {
            var xeroQueue = (from i in _queueRepository.Table
               where i.Id == id
               select i).FirstOrDefault();

            return xeroQueue;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public async Task<XeroQueue> GetQueueByOrderId(int orderId)
        {
            if (orderId == 0)
            {
                throw new ArgumentNullException("orderId");
            }

            var xeroQueue = (from q in _queueRepository.Table
                where q.OrderId == orderId
                select q).FirstOrDefault();

            return xeroQueue;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<IList<XeroQueue>> GetQueueForCancelOrder()
        {
            var list = (from q in _queueRepository.Table
                where !q.IsSuccess && q.ActionType == "Cancel" && q.SyncAttemptCount < 3
                select q).ToList();

            return list;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public async Task<IList<XeroQueue>> GetQueueForInvoice(int num = 0)
        {
            var list = new List<XeroQueue>();
            if (num == 0)
            {
                list = (from q in _queueRepository.Table
                            where !q.IsSuccess && q.ActionType == "Invoice" && q.SyncAttemptCount < 3
                            select q).ToList();
            }
            else
            {
                list = (from q in _queueRepository.Table
                        where !q.IsSuccess && q.ActionType == "Invoice" && q.SyncAttemptCount < 3
                        select q).Take(num).ToList();
            }

            return list;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public async Task<IList<XeroQueue>> GetQueueForPayment(int num = 0)
        {
            var list = new List<XeroQueue>();
            if (num == 0)
            {
                list = (from q in _queueRepository.Table
                            where !q.IsSuccess && q.ActionType == "Payment" && q.SyncAttemptCount < 3
                            select q).ToList();
            }
            else
            {
                list = (from q in _queueRepository.Table
                        where !q.IsSuccess && q.ActionType == "Payment" && q.SyncAttemptCount < 3
                        select q).Take(num).ToList();
            }

            return list;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<XeroQueue> GetUnPaidInvoice(string invoiceId)
        {
            var query = (from q in _queueRepository.Table
                where q.IsPaid == false && q.XeroId == invoiceId
                         select q).FirstOrDefault();

            return query;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="systemName"></param>
        /// <returns></returns>
        public async Task<XeroAccounting> GetXeroAccountByPaymentMethodSystemName(string systemName)
        {
            var xeroAccounting = (from a in _xeroAccountingRepository.Table
              where a.NopPaymentMethod == systemName
              select a).FirstOrDefault();

            return xeroAccounting;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="XeroAccounting"></param>
        public async Task InsertAccountMap(XeroAccounting xeroAccounting)
        {
            if (xeroAccounting == null)
            {
                throw new ArgumentNullException("XeroAccounting");
            }

           await  _xeroAccountingRepository.InsertAsync(xeroAccounting);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="xeroQueue"></param>
        public async Task InsertQueue(XeroQueue xeroQueue)
        {
            if (xeroQueue == null)
            {
                throw new ArgumentNullException("XeroQueue");
            }

           await _queueRepository.InsertAsync(xeroQueue);
        }

        /// <summary>
        /// 
        /// </summary>
        public async Task ManagePayment()
        {
            Guid guid;
            try
            {
                var record = await _xeroAccessRefreshTokenService.CheckRecordExist();
                if (record != null)
                {
                    IAccountingApi accountingApi = new AccountingApi();

                    int num = 1;
                    while (num <= 10)
                    {
                        var utcNow = DateTime.UtcNow;
                        var invoicesApi = accountingApi.GetInvoicesAsync(record.AccessToken, record.Tenants[0].TenantId.ToString(), ifModifiedSince: utcNow.AddDays(-2), page: num).GetAwaiter();
                        IList<Invoice> invoices = invoicesApi.GetResult()._Invoices;
                        invoices = invoices.Where(i => i.Status == Invoice.StatusEnum.PAID).ToList();

                        if (invoices.Count <= 0)
                        {
                            break;
                        }
                        else
                        {
                            foreach (var invoice in invoices)
                            {
                                if (invoice.Status == Invoice.StatusEnum.PAID)
                                {
                                    var order = new Order();
                                    if (string.IsNullOrEmpty(invoice.Reference))
                                    {
                                        var xeroQueue = await GetUnPaidInvoice(invoice.InvoiceID.Value.ToString());
                                        if (xeroQueue == null)
                                        {
                                            guid = invoice.InvoiceID.Value;
                                        }
                                        else
                                        {
                                            order = await _orderService.GetOrderByIdAsync(xeroQueue.OrderId);
                                        }
                                    }
                                    else
                                    {
                                        order = await _orderService.GetOrderByIdAsync(int.Parse(invoice.Reference));
                                    }

                                    if (order == null)
                                    {
                                        guid = invoice.InvoiceID.Value;
                                    }
                                    else if (order.PaymentStatusId == 30)
                                    {
                                        //_logger.InsertLog(20, string.Concat("Order is already Paid orderId:", order.Id), "", null);
                                    }
                                    else
                                    {
                                        
                                        var orderNotes =await _orderService.GetOrderNotesByOrderIdAsync(order.Id);
                                        var orderNote = new OrderNote();
                                        orderNote.Note = "Payment is done via Xero Accounting";
                                        orderNote.DisplayToCustomer = false;
                                        orderNote.CreatedOnUtc = DateTime.UtcNow;
                                        orderNotes.Add(orderNote);

                                        var queueByOrderId = await GetQueueByOrderId(order.Id);
                                        if (queueByOrderId != null)
                                        {
                                            queueByOrderId.IsPaid = true;
                                            UpdateQueue(queueByOrderId);
                                        }

                                        var paymentQueueByOrderId = await GetPaymentQueueByOrderId(order.Id);
                                        if (paymentQueueByOrderId != null)
                                        {
                                            paymentQueueByOrderId.IsSuccess = true;
                                            paymentQueueByOrderId.ResponseData = "Payment is done via Xero Accounting";
                                            paymentQueueByOrderId.XeroId = "Paid using Xero";
                                            paymentQueueByOrderId.SyncAttemptCount = paymentQueueByOrderId.SyncAttemptCount + 1;
                                            paymentQueueByOrderId.SyncAttemptOn = DateTime.UtcNow;
                                            UpdateQueue(paymentQueueByOrderId);
                                        }

                                       await _orderProcessingService.MarkOrderAsPaidAsync(order);
                                    }
                                }
                            }

                            num++;
                        }
                    }
                }
            }
            catch (Exception )
            {

            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="XeroAccounting"></param>
        public async Task UpdateAccountMap(XeroAccounting xeroAccounting)
        {
            if (xeroAccounting == null)
            {
                throw new ArgumentNullException("XeroAccounting");
            }

          await  _xeroAccountingRepository.UpdateAsync(xeroAccounting);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="queue"></param>
        public async Task UpdateQueue(XeroQueue queue)
        {
            if (queue == null)
            {
                throw new ArgumentNullException("XeroQueue");
            }

            await _queueRepository.UpdateAsync(queue);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="xeroQueue"></param>
        public async Task XeroPayment(IList<XeroQueue> xeroQueueList)
        {
            var record = await _xeroAccessRefreshTokenService.CheckRecordExist();
            if (record != null)
            {
                IAccountingApi accountingApi = new AccountingApi();
                Guid guid;
                if (xeroQueueList.Count > 0)
                {
                    int activeStoreScopeConfiguration = await _storeContext.GetActiveStoreScopeConfigurationAsync();
                    var setting = await _settingService.LoadSettingAsync<ForeFrontXeroSetting>(activeStoreScopeConfiguration);

                    int num = 0;
                    num = setting.BatchSize <= 0 ? 100 : setting.BatchSize;

                    decimal number = new decimal();
                    number = xeroQueueList.Count > num ? xeroQueueList.Count / num : new decimal();

                    for (int i = 0; i <= Math.Round(number); i++)
                    {
                        var paymentList = new List<Payment>();
                        foreach (var xeroQueue in await GetQueueForPayment(num))
                        {
                            if (xeroQueue.OrderId != 0)
                            {
                                var parentQueueByOrderId = await GetParentQueueByOrderId(xeroQueue.OrderId);
                                if (parentQueueByOrderId != null)
                                {
                                    if (parentQueueByOrderId.IsSuccess)
                                    {
                                        var order =  await _orderService.GetOrderByIdAsync(xeroQueue.OrderId);
                                        var currency = await _workContext.GetWorkingCurrencyAsync();
                                        if (order != null)
                                        {
                                            var payment = new Payment()
                                            {
                                                PaymentType = Payment.PaymentTypeEnum.ACCRECPAYMENT,
                                                Date = DateTime.UtcNow,
                                                CurrencyRate = currency.Rate,
                                                Amount =order.OrderTotal
                                            };

                                            int orderId = order.Id;
                                            payment.Reference = string.Concat(orderId.ToString(), " , ", order.PaymentMethodSystemName);
                                            var invoice = new Invoice()
                                            {
                                                InvoiceID = Guid.Parse(parentQueueByOrderId.XeroId)
                                            };

                                            payment.Invoice = invoice;
                                            var account = new Account();
                                            string empty = string.Empty;
                                            if (order.PaymentMethodSystemName == null)
                                            {
                                                var accountsApi = accountingApi.GetAccountsAsync(record.AccessToken, record.Tenants[0].id.ToString()).GetAwaiter();
                                                IList<Account> result = accountsApi.GetResult()._Accounts;
                                                guid = result.FirstOrDefault(a => a.Code == "880").AccountID.Value;
                                                empty = guid.ToString();
                                            }
                                            else
                                            {
                                                var xeroAccountByPaymentMethodSystemName = await GetXeroAccountByPaymentMethodSystemName(order.PaymentMethodSystemName);
                                                if ((xeroAccountByPaymentMethodSystemName == null ? true : xeroAccountByPaymentMethodSystemName.XeroAccountId == "0"))
                                                {
                                                    var accountsApi = accountingApi.GetAccountsAsync(record.AccessToken, record.Tenants[0].id.ToString()).GetAwaiter();
                                                    IList<Account> enumerable = accountsApi.GetResult()._Accounts;
                                                    guid = enumerable.FirstOrDefault(a => a.Code == "880").AccountID.Value;
                                                    empty = guid.ToString();
                                                }
                                                else
                                                {
                                                    empty = xeroAccountByPaymentMethodSystemName.XeroAccountId.Trim();
                                                }
                                            }

                                            account.AccountID = Guid.Parse(empty);
                                            payment.Account = account;
                                            paymentList.Add(payment);
                                        }
                                    }
                                }
                            }
                        }
                        try
                        {
                            if (paymentList.Count > 0)
                            {
                                var payments = new Payments();
                                payments._Payments = new List<Payment>();
                                payments._Payments.AddRange(paymentList);

                                var createPaymentApi = accountingApi.CreatePaymentsAsync(record.AccessToken, record.Tenants[0].TenantId.ToString(), payments).GetAwaiter();
                                IList<Payment> result = createPaymentApi.GetResult()._Payments;

                                foreach (var payment in result)
                                {
                                    var parentQueue = await GetParentQueueByOrderId(int.Parse(payment.Invoice.Reference));
                                    var paymentQueueByOrderId = await GetPaymentQueueByOrderId(int.Parse(payment.Invoice.Reference));
                                    payment.ValidationErrors = new List<ValidationError>();
                                    if (payment.ValidationErrors.Count > 0)
                                    {
                                        paymentQueueByOrderId.ResponseMessages = JsonConvert.SerializeObject(payment.ValidationErrors);
                                    }
                                    else
                                    {
                                        paymentQueueByOrderId.IsSuccess = true;
                                        paymentQueueByOrderId.ResponseData = JsonConvert.SerializeObject(result);
                                        paymentQueueByOrderId.XeroId = payment.PaymentID.ToString();
                                        parentQueue.IsPaid = true;
                                       await UpdateQueue(parentQueue);
                                    }

                                    paymentQueueByOrderId.SyncAttemptCount = paymentQueueByOrderId.SyncAttemptCount + 1;
                                    paymentQueueByOrderId.SyncAttemptOn = DateTime.UtcNow;
                                    await UpdateQueue(paymentQueueByOrderId);
                                }
                            }
                        }
                        catch (Exception)
                        {

                        }
                    }
                }
            }
        }
    }

    #endregion
}