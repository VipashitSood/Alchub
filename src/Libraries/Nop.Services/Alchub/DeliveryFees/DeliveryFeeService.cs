using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Alchub.Domain;
using Nop.Core.Caching;
using Nop.Core.Domain.DeliveryFees;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Vendors;
using Nop.Data;
using Nop.Services.Catalog;
using Nop.Services.Localization;
using Nop.Services.Vendors;

namespace Nop.Services.DeliveryFees
{
    /// <summary>
    /// Delivery Fee service
    /// </summary>
    public class DeliveryFeeService : IDeliveryFeeService
    {
        #region Fields

        private readonly AlchubSettings _alchubSettings;
        private readonly IRepository<DeliveryFee> _deliveryFeeRepository;
        private readonly IRepository<OrderDeliveryFee> _orderDeliveryFeeRepository;
        protected readonly IStaticCacheManager _staticCacheManager;
        private readonly IVendorService _vendorService;
        private readonly IRepository<Vendor> _vendorRepository;
        private readonly IWorkContext _workContext;
        private readonly IProductService _productService;
        private readonly ILocalizationService _localizationService;

        #endregion Fields

        #region Ctor

        public DeliveryFeeService(
            AlchubSettings alchubSettings,
            IRepository<DeliveryFee> deliveryFeeRepository,
            IRepository<OrderDeliveryFee> orderDeliveryFeeRepository,
            IStaticCacheManager staticCacheManager,
            IVendorService vendorService,
            IRepository<Vendor> vendorRepository,
            IWorkContext workContext,
            IProductService productService,
            ILocalizationService localizationService)
        {
            _alchubSettings = alchubSettings;
            _deliveryFeeRepository = deliveryFeeRepository;
            _orderDeliveryFeeRepository = orderDeliveryFeeRepository;
            _staticCacheManager = staticCacheManager;
            _vendorService = vendorService;
            _vendorRepository = vendorRepository;
            _workContext = workContext;
            _productService = productService;
            _localizationService = localizationService;
        }

        #endregion Ctor

        #region Methods

        #region Delivery Fee

        /// <summary>
        /// Inserts a Delivery Fee
        /// </summary>
        /// <param name="deliveryFee"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public virtual async Task InsertDeliveryFeeAsync(DeliveryFee deliveryFee)
        {
            if (deliveryFee == null)
                throw new ArgumentNullException(nameof(deliveryFee));

            //Insert
            await _deliveryFeeRepository.InsertAsync(deliveryFee);
        }

        /// <summary>
        /// Updates a Delivery Fee
        /// </summary>
        /// <param name="deliveryFee"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public virtual async Task UpdateDeliveryFeeAsync(DeliveryFee deliveryFee)
        {
            if (deliveryFee == null)
                throw new ArgumentNullException(nameof(deliveryFee));

            //Update
            await _deliveryFeeRepository.UpdateAsync(deliveryFee);
        }

        /// <summary>
        /// Deletes a Delivery Fee
        /// </summary>
        /// <param name="deliveryFee"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public virtual async Task DeleteDeliveryFeeAsync(DeliveryFee deliveryFee)
        {
            if (deliveryFee == null)
                throw new ArgumentNullException(nameof(deliveryFee));

            //Delete
            await _deliveryFeeRepository.DeleteAsync(deliveryFee);
        }

        /// <summary>
        /// Get Delivery Fee By Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual async Task<DeliveryFee> GetDeliveryFeeByIdAsync(int id = 0)
        {
            if (id == 0)
                return null;

            var cacheKey = _staticCacheManager.PrepareKeyForDefaultCache(DeliveryFeeServiceDefaults.DeliveryFeeByIdCacheKey, id);

            return await _staticCacheManager.GetAsync(cacheKey, () => _deliveryFeeRepository.GetByIdAsync(id));
        }

        /// <summary>
        /// Get Delivery Fee By Vendor Id
        /// </summary>
        /// <param name="vendorId"></param>
        /// <returns></returns>
        public virtual async Task<DeliveryFee> GetDeliveryFeeByVendorIdAsync(int vendorId = 0)
        {
            if (vendorId == 0)
                return null;

            var cacheKey = _staticCacheManager.PrepareKeyForDefaultCache(DeliveryFeeServiceDefaults.DeliveryFeeByVendorIdCacheKey, vendorId);

            var query = _deliveryFeeRepository.Table.Where(x => x.VendorId == vendorId);

            return await _staticCacheManager.GetAsync(cacheKey, () => query.FirstOrDefaultAsync());
        }

        #endregion Delivery Fee

        #region Order Delivery Fee

        /// <summary>
        /// Inserts a Order Delivery Fee
        /// </summary>
        /// <param name="orderDeliveryFee"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public virtual async Task InsertOrderDeliveryFeeAsync(OrderDeliveryFee orderDeliveryFee)
        {
            if (orderDeliveryFee == null)
                throw new ArgumentNullException(nameof(orderDeliveryFee));

            //Insert
            await _orderDeliveryFeeRepository.InsertAsync(orderDeliveryFee);
        }

        /// <summary>
        /// Inserts a list of Order Delivery Fee
        /// </summary>
        /// <param name="orderDeliveryFees"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public virtual async Task InsertOrderDeliveryFeesAsync(IList<OrderDeliveryFee> orderDeliveryFees)
        {
            if (orderDeliveryFees == null)
                throw new ArgumentNullException(nameof(orderDeliveryFees));

            //Insert
            await _orderDeliveryFeeRepository.InsertAsync(orderDeliveryFees);
        }

        /// <summary>
        /// Updates a Order Delivery Fee
        /// </summary>
        /// <param name="orderDeliveryFee"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public virtual async Task UpdateOrderDeliveryFeeAsync(OrderDeliveryFee orderDeliveryFee)
        {
            if (orderDeliveryFee == null)
                throw new ArgumentNullException(nameof(orderDeliveryFee));

            //Update
            await _orderDeliveryFeeRepository.UpdateAsync(orderDeliveryFee);
        }

        /// <summary>
        /// Deletes a Order Delivery Fee
        /// </summary>
        /// <param name="orderDeliveryFee"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public virtual async Task DeleteOrderDeliveryFeeAsync(OrderDeliveryFee orderDeliveryFee)
        {
            if (orderDeliveryFee == null)
                throw new ArgumentNullException(nameof(orderDeliveryFee));

            //Delete
            await _orderDeliveryFeeRepository.DeleteAsync(orderDeliveryFee);
        }

        /// <summary>
        /// Get Order Delivery Fee By Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual async Task<OrderDeliveryFee> GetOrderDeliveryFeeByIdAsync(int id = 0)
        {
            if (id == 0)
                return null;

            var cacheKey = _staticCacheManager.PrepareKeyForDefaultCache(DeliveryFeeServiceDefaults.OrderDeliveryFeeByIdCacheKey, id);

            return await _staticCacheManager.GetAsync(cacheKey, () => _orderDeliveryFeeRepository.GetByIdAsync(id));
        }

        /// <summary>
        /// Get Order Delivery Fees By Order Id
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public virtual async Task<IList<OrderDeliveryFee>> GetOrderDeliveryFeesByOrderIdAsync(int orderId = 0)
        {
            if (orderId == 0)
                return null;

            var cacheKey = _staticCacheManager.PrepareKeyForDefaultCache(DeliveryFeeServiceDefaults.OrderDeliveryFeeByOrderIdCacheKey, orderId);

            var query = _orderDeliveryFeeRepository.Table.Where(x => x.OrderId == orderId);

            return await _staticCacheManager.GetAsync(cacheKey, () => query.ToListAsync());
        }

        /// <summary>
        /// Get Vendor Wise Order Delivery Fees By Order Id
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public virtual async Task<IList<VendorWiseDeliveryFee>> GetVendorWiseOrderDeliveryFeesByOrderIdAsync(int orderId = 0)
        {
            if (orderId == 0)
                return null;

            var cacheKey = _staticCacheManager.PrepareKeyForDefaultCache(DeliveryFeeServiceDefaults.OrderDeliveryFeeByOrderIdCacheKey, orderId);


            var query = from o in _orderDeliveryFeeRepository.Table
                        join v in _vendorRepository.Table on o.VendorId equals v.Id
                        where o.OrderId == orderId
                        select new VendorWiseDeliveryFee
                        {
                            Id = o.Id,
                            VendorId = o.VendorId,
                            VendorName = v.Name,
                            DeliveryFeeValue = o.DeliveryFee
                        };

            return await _staticCacheManager.GetAsync(cacheKey, () => query.ToListAsync());
        }

        #endregion Order Delivery Fee

        #region Delivery Fee Calculations

        public virtual async Task<decimal> GetDistanceAsync(string origin, string destination)
        {
            if (string.IsNullOrEmpty(origin) || string.IsNullOrEmpty(destination))
                return decimal.Zero;

            var cacheKey = _staticCacheManager.PrepareKeyForDefaultCache(DeliveryFeeServiceDefaults.DistanceBetweenOriginAndDestinationCacheKey, origin, destination);

            return await _staticCacheManager.GetAsync(cacheKey, async () =>
            {
                decimal result = decimal.Zero;
                string url = @"https://maps.googleapis.com/maps/api/distancematrix/xml?origins=" +
                    origin + "&destinations=" + destination + "&sensor=false&key=" + _alchubSettings.GoogleApiKey;

                //Get distance from Google API
                HttpClient client = new HttpClient();
                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                DataSet ds = new DataSet();
                ds.ReadXml(new StringReader(responseBody));
                if (ds.Tables.Count > 0)
                {
                    if (ds.Tables["element"].Rows[0]["status"].ToString() == "OK")
                    {
                        string distanceString = ds.Tables["distance"].Rows[0]["value"].ToString();

                        if (Decimal.TryParse(distanceString, out result))
                            return result;
                    }
                }

                return result;
            });
        }

        public partial class VendorWiseDeliveryFeeSlot
        {
            public int VendorId { get; set; }

            public int SlotId { get; set; }

            public Vendor Vendor { get; set; }

            public string SlotTime { get; set; }

            public int Count { get; set; }
        }

        public virtual async Task<decimal> GetVendorDeliveryFeeAsync(Vendor vendor, string currentCustomerLastSearchedCoordinates)
        {
            decimal fee = decimal.Zero;

            var deliveryFee = await GetDeliveryFeeByVendorIdAsync(vendor.Id);

            if (deliveryFee != null)
            {
                if (deliveryFee.DeliveryFeeType == DeliveryFeeType.Fixed)
                {
                    fee = deliveryFee.FixedFee;
                }
                else if (deliveryFee.DeliveryFeeType == DeliveryFeeType.Dynamic)
                {
                    decimal distanceInMeters = await GetDistanceAsync(
                        origin: vendor.GeoLocationCoordinates,
                        destination: currentCustomerLastSearchedCoordinates);

                    //Convert Distance from Meter to Miles
                    decimal distance = Math.Round(distanceInMeters / Convert.ToDecimal(1609.34), 1);

                    fee = deliveryFee.DynamicBaseFee;

                    if (distance - deliveryFee.DynamicBaseDistance > 0)
                        fee += Math.Ceiling(distance - deliveryFee.DynamicBaseDistance) * deliveryFee.DynamicExtraFee;

                    if (fee > deliveryFee.DynamicMaximumFee)
                        fee = deliveryFee.DynamicMaximumFee;
                }
            }

            return fee;
        }

        /// <summary>
        /// Get Vendor Wise Delivery Fee
        /// </summary>
        /// <param name="cart"></param>
        /// <returns></returns>
        public virtual async Task<IList<VendorWiseDeliveryFee>> GetVendorWiseDeliveryFeeAsync(IList<ShoppingCartItem> cart)
        {
            var result = new List<VendorWiseDeliveryFee>();
            if (cart == null)
                return result;

            decimal adminDeliveryFee = decimal.Zero;
            var currentCustomerLastSearchedCoordinates = (await _workContext.GetCurrentCustomerAsync()).LastSearchedCoordinates;

            var vendors = new List<VendorWiseDeliveryFeeSlot>();

            foreach (var shoppingCartItem in cart)
            {
                if (!shoppingCartItem.IsPickup)
                {
                    var product = await _productService.GetProductByIdAsync(shoppingCartItem.ProductId);
                    var productVendor = await _vendorService.GetVendorByIdAsync(product?.VendorId ?? 0);

                    var vendorId = productVendor?.Id ?? 0;
                    if (vendorId > 0)
                    {
                        //var deliveryFeeSlot = vendors.Where(x => x.VendorId == vendorId && x.SlotId == shoppingCartItem.SlotId).FirstOrDefault();
                        var deliveryFeeSlot = vendors.Where(x => x.VendorId == vendorId && x.SlotId == shoppingCartItem.SlotId && x.SlotTime == shoppingCartItem.SlotTime).FirstOrDefault();
                        if (deliveryFeeSlot != null)
                        {
                            deliveryFeeSlot.Count += 1;
                        }
                        else
                        {
                            vendors.Add(new VendorWiseDeliveryFeeSlot
                            {
                                VendorId = vendorId,
                                SlotId = shoppingCartItem.SlotId,
                                SlotTime= shoppingCartItem.SlotTime,
                                Vendor = productVendor,
                                Count = 1
                            });
                        }
                    }
                }
            }

            if (vendors != null)
            {
                foreach (var vendor in vendors.Where(x => x.VendorId > 0).Select(x => x.Vendor).Distinct())
                {
                    decimal fee = await GetVendorDeliveryFeeAsync(vendor, currentCustomerLastSearchedCoordinates)
                        * vendors.Where(x => x.VendorId == vendor.Id).Count();

                    if (vendor.ManageDelivery)
                        result.Add(new VendorWiseDeliveryFee { VendorId = vendor.Id, VendorName = vendor.Name, DeliveryFeeValue = fee });
                    else
                        adminDeliveryFee += fee;
                }
            }

            result = result.OrderBy(x => x.VendorName).ToList();

            if (adminDeliveryFee > decimal.Zero)
                result.Add(new VendorWiseDeliveryFee { VendorId = 0, VendorName = await _localizationService.GetResourceAsync("Alchub.TipFee.Admin"), DeliveryFeeValue = adminDeliveryFee });

            return result;
        }

        /// <summary>
        /// Get OrerItem delivery fee
        /// </summary>
        /// <param name="shoppingCartItem"></param>
        /// <returns></returns>
        public virtual async Task<decimal> GetOrderItemDeliveryFeeAsync(ShoppingCartItem shoppingCartItem)
        {
            var deliveryFee = decimal.Zero;
            if (shoppingCartItem == null)
                return deliveryFee;

            var currentCustomerLastSearchedCoordinates = (await _workContext.GetCurrentCustomerAsync()).LastSearchedCoordinates;

            if (!shoppingCartItem.IsPickup)
            {
                var product = await _productService.GetProductByIdAsync(shoppingCartItem.ProductId);
                var productVendor = await _vendorService.GetVendorByIdAsync(product?.VendorId ?? 0);

                if ((productVendor?.Id ?? 0) > 0)
                    deliveryFee = await GetVendorDeliveryFeeAsync(productVendor, currentCustomerLastSearchedCoordinates);
            }

            return deliveryFee;
        }

        #endregion Delivery Fee Calculations

        #endregion Methods
    }
}