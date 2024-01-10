using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Alchub.Domain;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Vendors;
using Nop.Data;
using Nop.Services.Catalog;
using Nop.Services.Directory;
using Nop.Services.Slots;
using Nop.Services.Tax;

namespace Nop.Services.Alchub.General
{
    /// <summary>
    /// Alchub service
    /// </summary>
    public class AlchubGeneralService : IAlchubGeneralService
    {
        #region Fields

        private readonly IWorkContext _workContext;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly ITaxService _taxService;
        private readonly ICurrencyService _currencyService;
        private readonly IRepository<Product> _productRepository;
        private readonly IStaticCacheManager _staticCacheManager;
        private readonly IRepository<Vendor> _vendorRepository;
        private readonly AlchubSettings _alchubSettings;
        private readonly ISlotService _slotService;

        #endregion

        #region Ctor

        public AlchubGeneralService(IWorkContext workContext,
            IPriceCalculationService priceCalculationService,
            ITaxService taxService,
            ICurrencyService currencyService,
            IRepository<Product> productRepository,
            IStaticCacheManager staticCacheManager,
            IRepository<Vendor> vendorRepository,
            AlchubSettings alchubSettings,
            ISlotService slotService)
        {
            _workContext = workContext;
            _priceCalculationService = priceCalculationService;
            _taxService = taxService;
            _currencyService = currencyService;
            _productRepository = productRepository;
            _staticCacheManager = staticCacheManager;
            _vendorRepository = vendorRepository;
            _alchubSettings = alchubSettings;
            _slotService = slotService;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Get product final price
        /// </summary>
        /// <param name="product"></param>
        /// <returns></returns>
        private async Task<decimal?> GetProductFinalPrice(Product product, Customer customer)
        {
            if (product.CustomerEntersPrice)
                return null;

            if (product.CallForPrice)
                return null;

            if (customer == null)
                customer = await _workContext.GetCurrentCustomerAsync();

            //prices
            var (minPossiblePriceWithoutDiscount, minPossiblePriceWithDiscount, _, _) = await _priceCalculationService.GetFinalPriceAsync(product, customer);
            var (finalPriceWithDiscountBase, _) = await _taxService.GetProductPriceAsync(product, minPossiblePriceWithDiscount);
            var currentCurrency = await _workContext.GetWorkingCurrencyAsync();
            var finalPriceWithDiscount = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(finalPriceWithDiscountBase, currentCurrency);

            return finalPriceWithDiscount;
        }

        #endregion

        #region Methods

        #region Product

        /// <summary>
        /// Gets a master product by upcCode
        /// </summary>
        /// <param name="upcCode">UPCCode</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the product
        /// </returns>
        public async Task<Product> GetMasterProductByUpcCodeAsync(string upcCode)
        {
            if (string.IsNullOrEmpty(upcCode))
                return null;

            upcCode = upcCode.Trim();

            var query = from p in _productRepository.Table
                        orderby p.Id
                        where !p.Deleted &&
                        p.UPCCode == upcCode &&
                        p.IsMaster
                        select p;
            var product = await query.FirstOrDefaultAsync();

            return product;
        }

        /// <summary>
        /// Gets a products by upcCode
        /// </summary>
        /// <param name="upcCode">UPCCode</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the product
        /// </returns>
        public async Task<IList<Product>> GetProductsByUpcCodeAsync(string upcCode, bool excludeMasterProduct = false, bool? showPublished = null)
        {
            if (string.IsNullOrEmpty(upcCode))
                return new List<Product>();

            upcCode = upcCode.Trim();

            var cacheKey = _staticCacheManager.PrepareKeyForDefaultCache(NopCatalogDefaults.SameUpcProductsCacheKey,
                upcCode, excludeMasterProduct);

            var products = await _staticCacheManager.GetAsync(cacheKey, () =>
            {
                var query = from p in _productRepository.Table
                            orderby p.Id
                            where !p.Deleted &&
                            p.UPCCode == upcCode
                            select p;

                if (excludeMasterProduct)
                    query = query.Where(x => !x.IsMaster);

                return Task.FromResult(query.ToList());
            });

            if (showPublished.HasValue)
                products = products.Where(x => x.Published == showPublished.Value).ToList();

            return products.ToList();
        }

        /// <summary>
        /// Gets a products pricerange details
        /// </summary>
        /// <param name="product"></param>
        /// <param name="customer"></param>
        /// <returns>productId, price</returns>
        public async Task<IDictionary<int, decimal>> GetProductPriceRangeAsync(Product product, Customer customer = null)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            if (customer == null)
                customer = await _workContext.GetCurrentCustomerAsync();

            //get upc products
            var upcProducts = await GetProductsByUpcCodeAsync(upcCode: product.UPCCode, excludeMasterProduct: true, showPublished: true);
            if (!upcProducts.Any())
                return null;

            //do not consider gefence vendor if searched location is empty
            bool ignoreGeoVendors = string.IsNullOrEmpty(customer.LastSearchedCoordinates);
            if (!ignoreGeoVendors)
            {
                //get georadius & filtered vendors 
                var availableGefenceVendors = await _workContext.GetAllGeoFenceVendorIdsAsync(customer, true);
                if (!availableGefenceVendors.Any())
                    return null;

                //should be from filtered vendor only.  
                upcProducts = upcProducts.Where(x => availableGefenceVendors.Contains(x.VendorId)).ToList();
            }

            var priceRangeDisc = new Dictionary<int, decimal>();
            foreach (var productItem in upcProducts)
            {
                //apply product details all filter for vendor. (i.e: y miles, out of stok, vendor created slot.) - 23/05/23
                //check for in stock - 24-05-23
                if (productItem.StockQuantity <= 0)
                    continue;

                //check if no slot created for delivery/pickup then do not include vendor - 24-05-23
                var vendor = await _vendorRepository.GetByIdAsync(productItem.VendorId, cache => default);
                if (vendor != null)
                {
                    if (!await _slotService.HasVendorCreatedAnySlot(vendor))
                        continue;
                }

                var finalPrice = await GetProductFinalPrice(productItem, customer);
                if (finalPrice.HasValue)
                    priceRangeDisc.Add(productItem.Id, finalPrice.Value);
            }

            return priceRangeDisc.OrderBy(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
        }

        /// <summary>
        /// Get the product szies
        /// </summary>
        /// <param name="product"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public async Task<IList<string>> GetProductSizesAsync()
        {
            //if (product == null)
            //    throw new ArgumentNullException(nameof(product));

            //get setting value
            var sizesStr = _alchubSettings.ProductSizes;
            //default values
            //if (string.IsNullOrEmpty(sizesStr))
            //    sizesStr = "170mm,200mm,250mm,300mm,500mm";

            var sizes = sizesStr.Split(",", StringSplitOptions.RemoveEmptyEntries)?.ToList() ?? new List<string>();
            return await Task.FromResult(sizes);
        }

        /// <summary>
        /// Get the product containers
        /// </summary>
        /// <param name="product"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public async Task<IList<string>> GetProductContainersAsync(Product product)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            //get setting value
            var containersStr = _alchubSettings.ProductContainers;
            ////default values
            //if (string.IsNullOrEmpty(containersStr))
            //    containersStr = "Container1,Container2,Container3,Container4,Container5";

            var containers = containersStr.Split(",", StringSplitOptions.RemoveEmptyEntries)?.ToList() ?? new List<string>();
            return await Task.FromResult(containers);
        }

        /// <summary>
        /// Gets the default variant of the grouped product
        /// </summary>
        /// <param name="associatedProducts"></param>
        /// <returns>product</returns>
        public async Task<Product> GetGroupedProductDefaultVariantAsync(IList<Product> associatedProducts)
        {
            if (associatedProducts == null || !associatedProducts.Any())
                return null;

            //associated product will already filtered by geovendor now, so first product will be default.
            return await Task.FromResult(associatedProducts.FirstOrDefault());
        }

        #endregion

        #region Vendor

        /// <summary>
        /// Gets a vendor by master product identifier
        /// </summary>
        /// <param name="masterProduct">Product</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the vendor
        /// </returns>
        public virtual async Task<IList<Vendor>> GetVendorByMasterProductIdAsync(Product masterProduct)
        {
            if (masterProduct == null)
                new List<Vendor>();

            //get upc matching products
            var upcProducts = await GetProductsByUpcCodeAsync(upcCode: masterProduct.UPCCode, excludeMasterProduct: true, showPublished: true);
            if (!upcProducts.Any())
                return new List<Vendor>();

            return await (from v in _vendorRepository.Table
                          join p in _productRepository.Table on v.Id equals p.VendorId
                          where upcProducts.Select(x => x.Id).Contains(p.Id) && !v.Deleted && v.Active
                          select v).Distinct().ToListAsync();
        }

        /// <summary>
        /// Gets a vendors by master product identifiers
        /// </summary>
        /// <param name="masterProducts">List of product</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the vendors
        /// </returns>
        public virtual async Task<IList<Vendor>> GetVendorsByMasterProductIdsAsync(IList<Product> masterProducts)
        {
            if (masterProducts is null)
                throw new ArgumentNullException(nameof(masterProducts));

            var allProductIds = new List<int>();
            //prepare master product's upc prodcuts collection.
            foreach (var masterProduct in masterProducts)
            {
                //get upc matching products
                var upcProducts = await GetProductsByUpcCodeAsync(upcCode: masterProduct.UPCCode, excludeMasterProduct: true, showPublished: true);
                if (!upcProducts.Any())
                    continue;

                allProductIds.AddRange(upcProducts.Select(x => x.Id));
            }

            //disctict to make sure
            allProductIds = allProductIds.Distinct().ToList();

            return await (from v in _vendorRepository.Table
                          join p in _productRepository.Table on v.Id equals p.VendorId
                          where allProductIds.Contains(p.Id) && !v.Deleted && v.Active
                          select v).Distinct().ToListAsync();
        }

        #endregion

        #endregion
    }
}
