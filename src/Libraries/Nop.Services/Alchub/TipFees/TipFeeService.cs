using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.TipFees;
using Nop.Core.Domain.Vendors;
using Nop.Data;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Localization;
using Nop.Services.Orders;
using Nop.Services.Vendors;

namespace Nop.Services.TipFees
{
    /// <summary>
    /// Tip Fee service
    /// </summary>
    public class TipFeeService : ITipFeeService
    {
        #region Fields

        private readonly IRepository<OrderTipFee> _orderTipFeeRepository;
        private readonly IStaticCacheManager _staticCacheManager;
        private readonly IVendorService _vendorService;
        private readonly IRepository<Vendor> _vendorRepository;
        private readonly IWorkContext _workContext;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IProductService _productService;
        private readonly ILocalizationService _localizationService;

        #endregion Fields

        #region Ctor

        public TipFeeService(
            IRepository<OrderTipFee> orderTipFeeRepository,
            IStaticCacheManager staticCacheManager,
            IVendorService vendorService,
            IRepository<Vendor> vendorRepository,
            IWorkContext workContext,
            IGenericAttributeService genericAttributeService,
            IShoppingCartService shoppingCartService,
            IProductService productService,
            ILocalizationService localizationService)
        {
            _orderTipFeeRepository = orderTipFeeRepository;
            _staticCacheManager = staticCacheManager;
            _vendorService = vendorService;
            _vendorRepository = vendorRepository;
            _workContext = workContext;
            _genericAttributeService = genericAttributeService;
            _shoppingCartService = shoppingCartService;
            _productService = productService;
            _localizationService = localizationService;
        }

        #endregion Ctor

        #region Methods

        #region Order Tip Fee

        /// <summary>
        /// Inserts a Order Tip Fee
        /// </summary>
        /// <param name="orderTipFee"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public virtual async Task InsertOrderTipFeeAsync(OrderTipFee orderTipFee)
        {
            if (orderTipFee == null)
                throw new ArgumentNullException(nameof(orderTipFee));

            //Insert
            await _orderTipFeeRepository.InsertAsync(orderTipFee);
        }

        /// <summary>
        /// Inserts a list of Order Tip Fee
        /// </summary>
        /// <param name="orderTipFees"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public virtual async Task InsertOrderTipFeesAsync(IList<OrderTipFee> orderTipFees)
        {
            if (orderTipFees == null)
                throw new ArgumentNullException(nameof(orderTipFees));

            //Insert
            await _orderTipFeeRepository.InsertAsync(orderTipFees);
        }

        /// <summary>
        /// Updates a Order Tip Fee
        /// </summary>
        /// <param name="orderTipFee"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public virtual async Task UpdateOrderTipFeeAsync(OrderTipFee orderTipFee)
        {
            if (orderTipFee == null)
                throw new ArgumentNullException(nameof(orderTipFee));

            //Update
            await _orderTipFeeRepository.UpdateAsync(orderTipFee);
        }

        /// <summary>
        /// Deletes a Order Tip Fee
        /// </summary>
        /// <param name="orderTipFee"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public virtual async Task DeleteOrderTipFeeAsync(OrderTipFee orderTipFee)
        {
            if (orderTipFee == null)
                throw new ArgumentNullException(nameof(orderTipFee));

            //Delete
            await _orderTipFeeRepository.DeleteAsync(orderTipFee);
        }

        /// <summary>
        /// Get Order Tip Fee By Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual async Task<OrderTipFee> GetOrderTipFeeByIdAsync(int id = 0)
        {
            if (id == 0)
                return null;

            var cacheKey = _staticCacheManager.PrepareKeyForDefaultCache(TipFeeServiceDefaults.OrderTipFeeByIdCacheKey, id);

            return await _staticCacheManager.GetAsync(cacheKey, () => _orderTipFeeRepository.GetByIdAsync(id));
        }

        /// <summary>
        /// Get Order Tip Fees By Order Id
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public virtual async Task<IList<OrderTipFee>> GetOrderTipFeesByOrderIdAsync(int orderId = 0)
        {
            if (orderId == 0)
                return null;

            var cacheKey = _staticCacheManager.PrepareKeyForDefaultCache(TipFeeServiceDefaults.OrderTipFeeByOrderIdCacheKey, orderId);

            var query = _orderTipFeeRepository.Table.Where(x => x.OrderId == orderId);

            return await _staticCacheManager.GetAsync(cacheKey, () => query.ToListAsync());
        }

        /// <summary>
        /// Get Vendor Wise Order Tip Fees By Order Id
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public virtual async Task<IList<VendorWiseTipFee>> GetVendorWiseOrderTipFeesByOrderIdAsync(int orderId = 0)
        {
            if (orderId == 0)
                return null;

            var adminName = await _localizationService.GetResourceAsync("Alchub.TipFee.Admin");

            var cacheKey = _staticCacheManager.PrepareKeyForDefaultCache(TipFeeServiceDefaults.OrderTipFeeByOrderIdCacheKey, orderId);

            var query = from o in _orderTipFeeRepository.Table
                        join v in _vendorRepository.Table on o.VendorId equals v.Id into x
                        from v in x.DefaultIfEmpty()
                        where o.OrderId == orderId
                        select new VendorWiseTipFee
                        {
                            Id = o.Id,
                            VendorId = o.VendorId,
                            VendorName = v != null ? v.Name : adminName,
                            TipFeeValue = o.TipFee
                        };

            return await _staticCacheManager.GetAsync(cacheKey, () => query.ToListAsync());
        }

        #endregion Order Tip Fee

        #region Tip Fee Calculations

        /// <summary>
        /// Get Total Tip Fee
        /// </summary>
        /// <param name="sciSubTotal"></param>
        /// <param name="orderSubtotal"></param>
        /// <returns></returns>
        public virtual async Task<decimal> GetTotalTipFeeAsync(decimal sciSubTotal, decimal orderSubtotal)
        {
            var customerTipFeeDetails = await GetCustomerTipFeeDetailsAsync();

            if (customerTipFeeDetails.Item1 == 0)
                return ((sciSubTotal / orderSubtotal) * customerTipFeeDetails.Item2);

            else
                return (sciSubTotal * customerTipFeeDetails.Item1) / 100;
        }

        /// <summary>
        /// Get Vendor Wise Tip Fee
        /// </summary>
        /// <param name="cart"></param>
        /// <returns></returns>
        public virtual async Task<IList<VendorWiseTipFee>> GetVendorWiseTipFeeAsync(IList<ShoppingCartItem> cart, decimal orderSubtotal)
        {
            var result = new List<VendorWiseTipFee>();
            if (cart == null)
                return result;

            decimal adminTipFee = decimal.Zero;

            var vendors = await _vendorService.GetVendorsByProductIdsAsync(cart.Select(x => x.ProductId).ToArray());

            if (vendors != null && vendors.Count > 0)
            {
                vendors.ToList().ForEach(vendor =>
                {
                    result.Add(new VendorWiseTipFee { VendorId = vendor.Id, VendorName = vendor.Name, TipFeeValue = decimal.Zero });
                });
            }

            result = result.OrderBy(x => x.VendorName).ToList();

            foreach (var shoppingCartItem in cart)
            {
                var sciSubTotal = (await _shoppingCartService.GetSubTotalAsync(shoppingCartItem, true)).subTotal;
                var sciTipFee = await GetTotalTipFeeAsync(sciSubTotal, orderSubtotal);

                var product = await _productService.GetProductByIdAsync(shoppingCartItem.ProductId);
                var vendor = await _vendorService.GetVendorByIdAsync(product.VendorId);

                if (vendor != null && vendor.ManageDelivery)
                    result.Where(x => x.VendorId == product.VendorId).ToList().ForEach(x => { x.TipFeeValue += sciTipFee; });
                else
                    adminTipFee += sciTipFee;
            }

            if (adminTipFee > decimal.Zero)
            {
                result.Add(new VendorWiseTipFee
                {
                    VendorId = 0,
                    VendorName = await _localizationService.GetResourceAsync("Alchub.TipFee.Admin"),
                    TipFeeValue = adminTipFee
                });
            }

            return result;
        }

        /// <summary>
        /// Get customer Tip Fee Details
        /// </summary>
        /// <returns></returns>
        public virtual async Task<(int, decimal)> GetCustomerTipFeeDetailsAsync()
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            var tipTypeId = await _genericAttributeService.GetAttributeAsync<int?>(customer, NopCustomerDefaults.TipTypeIdAttribute) ?? 10;
            var customTipAmount = await _genericAttributeService.GetAttributeAsync<decimal?>(customer, NopCustomerDefaults.CustomTipAmountAttribute) ?? decimal.Zero;

            return (tipTypeId, customTipAmount);
        }

        /// <summary>
        /// Remove Customer Tip Fee Details
        /// </summary>
        /// <returns></returns>
        public virtual async Task RemoveCustomerTipFeeDetailsAsync()
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            await _genericAttributeService.SaveAttributeAsync<int?>(customer, NopCustomerDefaults.TipTypeIdAttribute, null);
            await _genericAttributeService.SaveAttributeAsync<decimal?>(customer, NopCustomerDefaults.CustomTipAmountAttribute, null);
        }

        #endregion Tip Fee Calculations

        #endregion Methods
    }
}