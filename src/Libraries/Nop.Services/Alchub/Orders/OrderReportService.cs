using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Shipping;
using Nop.Data;
using Nop.Services.Alchub.General;
using Nop.Services.Catalog;
using Nop.Services.Directory;
using Nop.Services.Helpers;
using Nop.Services.Stores;

namespace Nop.Services.Orders
{
    /// <summary>
    /// Order report service
    /// </summary>
    public partial class OrderReportService : IOrderReportService
    {
        #region Fields

        private readonly CurrencySettings _currencySettings;
        private readonly ICurrencyService _currencyService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IRepository<Address> _addressRepository;
        private readonly IRepository<Order> _orderRepository;
        private readonly IRepository<OrderItem> _orderItemRepository;
        private readonly IRepository<OrderNote> _orderNoteRepository;
        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<ProductCategory> _productCategoryRepository;
        private readonly IRepository<ProductManufacturer> _productManufacturerRepository;
        private readonly IRepository<ProductWarehouseInventory> _productWarehouseInventoryRepository;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IWorkContext _workContext;
        private readonly IProductService _productService;
        private readonly IAlchubGeneralService _alchubGeneralService;

        #endregion

        #region Ctor

        public OrderReportService(
            CurrencySettings currencySettings,
            ICurrencyService currencyService,
            IDateTimeHelper dateTimeHelper,
            IPriceFormatter priceFormatter,
            IRepository<Address> addressRepository,
            IRepository<Order> orderRepository,
            IRepository<OrderItem> orderItemRepository,
            IRepository<OrderNote> orderNoteRepository,
            IRepository<Product> productRepository,
            IRepository<ProductCategory> productCategoryRepository,
            IRepository<ProductManufacturer> productManufacturerRepository,
            IRepository<ProductWarehouseInventory> productWarehouseInventoryRepository,
            IStoreMappingService storeMappingService,
            IWorkContext workContext,
            IProductService productService,
            IAlchubGeneralService alchubGeneralService)
        {
            _currencySettings = currencySettings;
            _currencyService = currencyService;
            _dateTimeHelper = dateTimeHelper;
            _priceFormatter = priceFormatter;
            _addressRepository = addressRepository;
            _orderRepository = orderRepository;
            _orderItemRepository = orderItemRepository;
            _orderNoteRepository = orderNoteRepository;
            _productRepository = productRepository;
            _productCategoryRepository = productCategoryRepository;
            _productManufacturerRepository = productManufacturerRepository;
            _productWarehouseInventoryRepository = productWarehouseInventoryRepository;
            _storeMappingService = storeMappingService;
            _workContext = workContext;
            _productService = productService;
            _alchubGeneralService = alchubGeneralService;
        }

        #endregion

        #region Utils

        /// <summary>
        /// Search order items
        /// </summary>
        /// <param name="storeId">Store identifier (orders placed in a specific store); 0 to load all records</param>
        /// <param name="vendorId">Vendor identifier; 0 to load all records</param>
        /// <param name="categoryId">Category identifier; 0 to load all records</param>
        /// <param name="manufacturerId">Manufacturer identifier; 0 to load all records</param>
        /// <param name="createdFromUtc">Order created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Order created date to (UTC); null to load all records</param>
        /// <param name="os">Order status; null to load all records</param>
        /// <param name="ps">Order payment status; null to load all records</param>
        /// <param name="ss">Shipping status; null to load all records</param>
        /// <param name="billingCountryId">Billing country identifier; 0 to load all records</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <param name="isMasterOnly">A value indicating whether to show only master product records</param>
        /// <returns>Result query</returns>
        private IQueryable<OrderItem> SearchOrderItems(
            int categoryId = 0,
            int manufacturerId = 0,
            int storeId = 0,
            int vendorId = 0,
            DateTime? createdFromUtc = null,
            DateTime? createdToUtc = null,
            OrderStatus? os = null,
            PaymentStatus? ps = null,
            ShippingStatus? ss = null,
            int billingCountryId = 0,
            bool showHidden = false,
            bool isMasterOnly = false)
        {
            int? orderStatusId = null;
            if (os.HasValue)
                orderStatusId = (int)os.Value;

            int? paymentStatusId = null;
            if (ps.HasValue)
                paymentStatusId = (int)ps.Value;

            int? shippingStatusId = null;
            if (ss.HasValue)
                shippingStatusId = (int)ss.Value;

            var orderItems = from orderItem in _orderItemRepository.Table
                             join o in _orderRepository.Table on orderItem.OrderId equals o.Id
                             join p in _productRepository.Table on orderItem.ProductId equals p.Id
                             join oba in _addressRepository.Table on o.BillingAddressId equals oba.Id
                             where (storeId == 0 || storeId == o.StoreId) &&
                                 (!createdFromUtc.HasValue || createdFromUtc.Value <= o.CreatedOnUtc) &&
                                 (!createdToUtc.HasValue || createdToUtc.Value >= o.CreatedOnUtc) &&
                                 (!orderStatusId.HasValue || orderStatusId == o.OrderStatusId) &&
                                 (!paymentStatusId.HasValue || paymentStatusId == o.PaymentStatusId) &&
                                 (!shippingStatusId.HasValue || shippingStatusId == o.ShippingStatusId) &&
                                 !o.Deleted && !p.Deleted &&
                                 (vendorId == 0 || p.VendorId == vendorId) &&
                                 (billingCountryId == 0 || oba.CountryId == billingCountryId) &&
                                 (showHidden || p.Published)
                             //(isMasterOnly && p.IsMaster)
                             select orderItem;

            if (categoryId > 0)
            {
                orderItems = from orderItem in orderItems
                             join p in _productRepository.Table on orderItem.ProductId equals p.Id
                             join pc in _productCategoryRepository.Table on p.Id equals pc.ProductId
                             into p_pc
                             from pc in p_pc.DefaultIfEmpty()
                             where pc.CategoryId == categoryId
                             select orderItem;
            }

            if (manufacturerId > 0)
            {
                orderItems = from orderItem in orderItems
                             join p in _productRepository.Table on orderItem.ProductId equals p.Id
                             join pm in _productManufacturerRepository.Table on p.Id equals pm.ProductId
                             into p_pm
                             from pm in p_pm.DefaultIfEmpty()
                             where pm.ManufacturerId == manufacturerId
                             select orderItem;
            }

            return orderItems;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Get best sellers report
        /// </summary>
        /// <param name="storeId">Store identifier (orders placed in a specific store); 0 to load all records</param>
        /// <param name="vendorId">Vendor identifier; 0 to load all records</param>
        /// <param name="categoryId">Category identifier; 0 to load all records</param>
        /// <param name="manufacturerId">Manufacturer identifier; 0 to load all records</param>
        /// <param name="createdFromUtc">Order created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Order created date to (UTC); null to load all records</param>
        /// <param name="os">Order status; null to load all records</param>
        /// <param name="ps">Order payment status; null to load all records</param>
        /// <param name="ss">Shipping status; null to load all records</param>
        /// <param name="billingCountryId">Billing country identifier; 0 to load all records</param>
        /// <param name="orderBy">1 - order by quantity, 2 - order by total amount</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <param name="isMasterOnly">A value indicating whether to show only master product records</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the result
        /// </returns>
        public virtual async Task<IPagedList<BestsellersReportLine>> BestSellersReportAsync(
            int categoryId = 0,
            int manufacturerId = 0,
            int storeId = 0,
            int vendorId = 0,
            DateTime? createdFromUtc = null,
            DateTime? createdToUtc = null,
            OrderStatus? os = null,
            PaymentStatus? ps = null,
            ShippingStatus? ss = null,
            int billingCountryId = 0,
            OrderByEnum orderBy = OrderByEnum.OrderByQuantity,
            int pageIndex = 0,
            int pageSize = int.MaxValue,
            bool showHidden = false,
            bool isMasterOnly = false)
        {

            var bestSellers = SearchOrderItems(categoryId, manufacturerId, storeId, vendorId, createdFromUtc, createdToUtc, os, ps, ss, billingCountryId, showHidden, isMasterOnly);

            if (isMasterOnly)
            {
                //alchub custom bestseller products (master products)
                var bsReport =
                    //group by products
                    from orderItem in bestSellers
                    group orderItem by orderItem.ProductId into g
                    select new BestsellersReportLine
                    {
                        ProductId = g.Key,
                        TotalAmount = g.Sum(x => x.PriceExclTax),
                        TotalQuantity = g.Sum(x => x.Quantity)
                    };

                //filter
                var finalbsReport = new List<BestsellersReportLine>();
                //group by upc code
                var upcGroupedItems = await bsReport.GroupByAwait(async c => (await _productService.GetProductByIdAsync(c.ProductId))?.UPCCode ?? $"<Null>{c.ProductId}")
                .ToDictionaryAsync(c => c.Key, c => c.FirstAsync());
                foreach (var item in upcGroupedItems)
                {
                    //ignore with '<Null>' pattern - performance enhance
                    if (item.Key.StartsWith("<Null>"))
                        continue;

                    //get master product
                    var masterProduct = await _alchubGeneralService.GetMasterProductByUpcCodeAsync(item.Key);
                    if (masterProduct == null)
                        continue;

                    //upc products 
                    var upcProducts = await _alchubGeneralService.GetProductsByUpcCodeAsync(item.Key);
                    if (!upcProducts.Any())
                        continue;

                    //upc products bestseller items
                    var finalProducts = from product in upcProducts
                                        join oi in bestSellers on product.Id equals oi.ProductId
                                        select oi;

                    //report by upc gruoped order item
                    var masterReport = new BestsellersReportLine
                    {
                        ProductId = masterProduct.Id,
                        TotalAmount = finalProducts.Sum(x => x.PriceExclTax),
                        TotalQuantity = finalProducts.Sum(x => x.Quantity)
                    };

                    finalbsReport.Add(masterReport);
                }

                var finalbsReportQueryable = orderBy switch
                {
                    OrderByEnum.OrderByQuantity => finalbsReport.OrderByDescending(x => x.TotalQuantity),
                    OrderByEnum.OrderByTotalAmount => finalbsReport.OrderByDescending(x => x.TotalAmount),
                    _ => throw new ArgumentException("Wrong orderBy parameter", nameof(orderBy)),
                };

                var result = await finalbsReportQueryable.AsQueryable().ToPagedListAsync(pageIndex, pageSize);

                return result;
            }
            else
            {
                //nop default
                var bsReport =
                    //group by products
                    from orderItem in bestSellers
                    group orderItem by orderItem.ProductId into g
                    select new BestsellersReportLine
                    {
                        ProductId = g.Key,
                        TotalAmount = g.Sum(x => x.PriceExclTax),
                        TotalQuantity = g.Sum(x => x.Quantity)
                    };

                bsReport = orderBy switch
                {
                    OrderByEnum.OrderByQuantity => bsReport.OrderByDescending(x => x.TotalQuantity),
                    OrderByEnum.OrderByTotalAmount => bsReport.OrderByDescending(x => x.TotalAmount),
                    _ => throw new ArgumentException("Wrong orderBy parameter", nameof(orderBy)),
                };

                var result = await bsReport.ToPagedListAsync(pageIndex, pageSize);

                return result;
            }
        }

        /// <summary>
        /// Get order average report
        /// </summary>
        /// <param name="storeId">Store identifier; pass 0 to ignore this parameter</param>
        /// <param name="vendorId">Vendor identifier; pass 0 to ignore this parameter</param>
        /// <param name="productId">Product identifier which was purchased in an order; 0 to load all orders</param>
        /// <param name="warehouseId">Warehouse identifier; pass 0 to ignore this parameter</param>
        /// <param name="billingCountryId">Billing country identifier; 0 to load all orders</param>
        /// <param name="orderId">Order identifier; pass 0 to ignore this parameter</param>
        /// <param name="paymentMethodSystemName">Payment method system name; null to load all records</param>
        /// <param name="osIds">Order status identifiers</param>
        /// <param name="psIds">Payment status identifiers</param>
        /// <param name="ssIds">Shipping status identifiers</param>
        /// <param name="startTimeUtc">Start date</param>
        /// <param name="endTimeUtc">End date</param>
        /// <param name="billingPhone">Billing phone. Leave empty to load all records.</param>
        /// <param name="billingEmail">Billing email. Leave empty to load all records.</param>
        /// <param name="billingLastName">Billing last name. Leave empty to load all records.</param>
        /// <param name="orderNotes">Search in order notes. Leave empty to load all records.</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the result
        /// </returns>
        public virtual async Task<OrderAverageReportLine> GetOrderAverageReportLineAsync(int storeId = 0,
            int vendorId = 0, int productId = 0, int warehouseId = 0, int billingCountryId = 0,
            int orderId = 0, string paymentMethodSystemName = null,
            List<int> osIds = null, List<int> psIds = null, List<int> ssIds = null,
            DateTime? startTimeUtc = null, DateTime? endTimeUtc = null,
            string billingPhone = null, string billingEmail = null, string billingLastName = "", string orderNotes = null)
        {
            var query = _orderRepository.Table;

            query = query.Where(o => !o.Deleted);
            if (storeId > 0)
                query = query.Where(o => o.StoreId == storeId);
            if (orderId > 0)
                query = query.Where(o => o.Id == orderId);

            if (vendorId > 0)
                query = from o in query
                        join oi in _orderItemRepository.Table on o.Id equals oi.OrderId
                        join p in _productRepository.Table on oi.ProductId equals p.Id
                        where p.VendorId == vendorId
                        select o;

            if (productId > 0)
                query = from o in query
                        join oi in _orderItemRepository.Table on o.Id equals oi.OrderId
                        where oi.ProductId == productId
                        select o;

            if (warehouseId > 0)
            {
                var manageStockInventoryMethodId = (int)ManageInventoryMethod.ManageStock;

                query = from o in query
                        join oi in _orderItemRepository.Table on o.Id equals oi.OrderId
                        join p in _productRepository.Table on oi.ProductId equals p.Id
                        join pwi in _productWarehouseInventoryRepository.Table on p.Id equals pwi.ProductId
                        where
                            //"Use multiple warehouses" enabled
                            //we search in each warehouse
                            (p.ManageInventoryMethodId == manageStockInventoryMethodId && p.UseMultipleWarehouses && pwi.WarehouseId == warehouseId) ||
                            //"Use multiple warehouses" disabled
                            //we use standard "warehouse" property
                            ((p.ManageInventoryMethodId != manageStockInventoryMethodId || !p.UseMultipleWarehouses) && p.WarehouseId == warehouseId)
                        select o;
            }

            query = from o in query
                    join oba in _addressRepository.Table on o.BillingAddressId equals oba.Id
                    where
                        (billingCountryId <= 0 || (oba.CountryId == billingCountryId)) &&
                        (string.IsNullOrEmpty(billingPhone) || (!string.IsNullOrEmpty(oba.PhoneNumber) && oba.PhoneNumber.Contains(billingPhone))) &&
                        (string.IsNullOrEmpty(billingEmail) || (!string.IsNullOrEmpty(oba.Email) && oba.Email.Contains(billingEmail))) &&
                        (string.IsNullOrEmpty(billingLastName) || (!string.IsNullOrEmpty(oba.LastName) && oba.LastName.Contains(billingLastName)))
                    select o;

            if (!string.IsNullOrEmpty(paymentMethodSystemName))
                query = query.Where(o => o.PaymentMethodSystemName == paymentMethodSystemName);

            if (osIds != null && osIds.Any())
                query = query.Where(o => osIds.Contains(o.OrderStatusId));

            if (psIds != null && psIds.Any())
                query = query.Where(o => psIds.Contains(o.PaymentStatusId));

            if (ssIds != null && ssIds.Any())
                query = query.Where(o => ssIds.Contains(o.ShippingStatusId));

            if (startTimeUtc.HasValue)
                query = query.Where(o => startTimeUtc.Value <= o.CreatedOnUtc);

            if (endTimeUtc.HasValue)
                query = query.Where(o => endTimeUtc.Value >= o.CreatedOnUtc);

            if (!string.IsNullOrEmpty(orderNotes))
                query = from o in query
                        join n in _orderNoteRepository.Table on o.Id equals n.OrderId
                        where n.Note.Contains(orderNotes)
                        select o;

            var item = await (from oq in query
                              group oq by 1
                into result
                              select new
                              {
                                  OrderCount = result.Count(),
                                  OrderShippingExclTaxSum = result.Sum(o => o.OrderShippingExclTax),
                                  OrderPaymentFeeExclTaxSum = result.Sum(o => o.PaymentMethodAdditionalFeeExclTax),
                                  OrderTaxSum = result.Sum(o => o.OrderTax),
                                  OrderServiceFee = result.Sum(o => o.ServiceFee),
                                  OrderSlotFee = result.Sum(o => o.SlotFee),
                                  OrderTotalSum = result.Sum(o => o.OrderTotal),
                                  OrederRefundedAmountSum = result.Sum(o => o.RefundedAmount),
                              }).Select(r => new OrderAverageReportLine
                              {
                                  CountOrders = r.OrderCount,
                                  SumShippingExclTax = r.OrderShippingExclTaxSum,
                                  OrderPaymentFeeExclTaxSum = r.OrderPaymentFeeExclTaxSum,
                                  SumTax = r.OrderTaxSum,
                                  OrderServiceFee = r.OrderServiceFee,
                                  OrderSlotFee = r.OrderSlotFee,
                                  SumOrders = r.OrderTotalSum,
                                  SumRefundedAmount = r.OrederRefundedAmountSum
                              })
                .FirstOrDefaultAsync();

            item ??= new OrderAverageReportLine
            {
                CountOrders = 0,
                SumShippingExclTax = decimal.Zero,
                OrderPaymentFeeExclTaxSum = decimal.Zero,
                SumTax = decimal.Zero,
                OrderServiceFee = decimal.Zero,
                OrderSlotFee = decimal.Zero,
                SumOrders = decimal.Zero
            };
            return item;
        }

        /// <summary>
        /// Get profit report
        /// </summary>
        /// <param name="storeId">Store identifier; pass 0 to ignore this parameter</param>
        /// <param name="vendorId">Vendor identifier; pass 0 to ignore this parameter</param>
        /// <param name="productId">Product identifier which was purchased in an order; 0 to load all orders</param>
        /// <param name="warehouseId">Warehouse identifier; pass 0 to ignore this parameter</param>
        /// <param name="orderId">Order identifier; pass 0 to ignore this parameter</param>
        /// <param name="billingCountryId">Billing country identifier; 0 to load all orders</param>
        /// <param name="paymentMethodSystemName">Payment method system name; null to load all records</param>
        /// <param name="startTimeUtc">Start date</param>
        /// <param name="endTimeUtc">End date</param>
        /// <param name="osIds">Order status identifiers; null to load all records</param>
        /// <param name="psIds">Payment status identifiers; null to load all records</param>
        /// <param name="ssIds">Shipping status identifiers; null to load all records</param>
        /// <param name="billingPhone">Billing phone. Leave empty to load all records.</param>
        /// <param name="billingEmail">Billing email. Leave empty to load all records.</param>
        /// <param name="billingLastName">Billing last name. Leave empty to load all records.</param>
        /// <param name="orderNotes">Search in order notes. Leave empty to load all records.</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the result
        /// </returns>
        public virtual async Task<decimal> ProfitReportAsync(int storeId = 0, int vendorId = 0, int productId = 0,
            int warehouseId = 0, int billingCountryId = 0, int orderId = 0, string paymentMethodSystemName = null,
            List<int> osIds = null, List<int> psIds = null, List<int> ssIds = null,
            DateTime? startTimeUtc = null, DateTime? endTimeUtc = null,
            string billingPhone = null, string billingEmail = null, string billingLastName = "", string orderNotes = null)
        {
            var dontSearchPhone = string.IsNullOrEmpty(billingPhone);
            var dontSearchEmail = string.IsNullOrEmpty(billingEmail);
            var dontSearchLastName = string.IsNullOrEmpty(billingLastName);
            var dontSearchOrderNotes = string.IsNullOrEmpty(orderNotes);
            var dontSearchPaymentMethods = string.IsNullOrEmpty(paymentMethodSystemName);

            var orders = _orderRepository.Table;
            if (osIds != null && osIds.Any())
                orders = orders.Where(o => osIds.Contains(o.OrderStatusId));
            if (psIds != null && psIds.Any())
                orders = orders.Where(o => psIds.Contains(o.PaymentStatusId));
            if (ssIds != null && ssIds.Any())
                orders = orders.Where(o => ssIds.Contains(o.ShippingStatusId));

            var manageStockInventoryMethodId = (int)ManageInventoryMethod.ManageStock;

            var query = from orderItem in _orderItemRepository.Table
                        join o in orders on orderItem.OrderId equals o.Id
                        join p in _productRepository.Table on orderItem.ProductId equals p.Id
                        join oba in _addressRepository.Table on o.BillingAddressId equals oba.Id
                        where (storeId == 0 || storeId == o.StoreId) &&
                              (orderId == 0 || orderId == o.Id) &&
                              (billingCountryId == 0 || (oba.CountryId == billingCountryId)) &&
                              (dontSearchPaymentMethods || paymentMethodSystemName == o.PaymentMethodSystemName) &&
                              (!startTimeUtc.HasValue || startTimeUtc.Value <= o.CreatedOnUtc) &&
                              (!endTimeUtc.HasValue || endTimeUtc.Value >= o.CreatedOnUtc) &&
                              !o.Deleted &&
                              (vendorId == 0 || p.VendorId == vendorId) &&
                              (productId == 0 || orderItem.ProductId == productId) &&
                              (warehouseId == 0 ||
                                  //"Use multiple warehouses" enabled
                                  //we search in each warehouse
                                  p.ManageInventoryMethodId == manageStockInventoryMethodId &&
                                  p.UseMultipleWarehouses &&
                                  _productWarehouseInventoryRepository.Table.Any(pwi =>
                                      pwi.ProductId == orderItem.ProductId && pwi.WarehouseId == warehouseId)
                                  ||
                                  //"Use multiple warehouses" disabled
                                  //we use standard "warehouse" property
                                  (p.ManageInventoryMethodId != manageStockInventoryMethodId ||
                                   !p.UseMultipleWarehouses) &&
                                  p.WarehouseId == warehouseId) &&
                              //we do not ignore deleted products when calculating order reports
                              //(!p.Deleted)
                              (dontSearchPhone || (!string.IsNullOrEmpty(oba.PhoneNumber) &&
                                                   oba.PhoneNumber.Contains(billingPhone))) &&
                              (dontSearchEmail || (!string.IsNullOrEmpty(oba.Email) && oba.Email.Contains(billingEmail))) &&
                              (dontSearchLastName ||
                               (!string.IsNullOrEmpty(oba.LastName) && oba.LastName.Contains(billingLastName))) &&
                              (dontSearchOrderNotes || _orderNoteRepository.Table.Any(oNote =>
                                   oNote.OrderId == o.Id && oNote.Note.Contains(orderNotes)))
                        select orderItem;

            var productCost = Convert.ToDecimal(await query.SumAsync(orderItem => (decimal?)orderItem.OriginalProductCost * orderItem.Quantity));

            var reportSummary = await GetOrderAverageReportLineAsync(
                storeId,
                vendorId,
                productId,
                warehouseId,
                billingCountryId,
                orderId,
                paymentMethodSystemName,
                osIds,
                psIds,
                ssIds,
                startTimeUtc,
                endTimeUtc,
                billingPhone,
                billingEmail,
                billingLastName,
                orderNotes);

            var profit = reportSummary.SumOrders
                         - reportSummary.SumShippingExclTax
                         - reportSummary.OrderPaymentFeeExclTaxSum
                         - reportSummary.SumTax
                         - reportSummary.SumRefundedAmount
                         - reportSummary.OrderServiceFee
                         - reportSummary.OrderSlotFee
                         - productCost;
            return profit;
        }

        #endregion
    }
}