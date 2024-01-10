using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Alchub.Domain;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Discounts;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using Nop.Data;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Security;
using Nop.Services.Shipping.Date;
using Nop.Services.Stores;

namespace Nop.Services.Catalog
{
    /// <summary>
    /// Product service
    /// </summary>
    public partial class ProductService
    {
        #region Fields

        protected readonly CatalogSettings _catalogSettings;
        protected readonly CommonSettings _commonSettings;
        protected readonly IAclService _aclService;
        protected readonly ICustomerService _customerService;
        protected readonly IDateRangeService _dateRangeService;
        protected readonly ILanguageService _languageService;
        protected readonly ILocalizationService _localizationService;
        protected readonly IProductAttributeParser _productAttributeParser;
        protected readonly IProductAttributeService _productAttributeService;
        protected readonly IRepository<CrossSellProduct> _crossSellProductRepository;
        protected readonly IRepository<DiscountProductMapping> _discountProductMappingRepository;
        protected readonly IRepository<LocalizedProperty> _localizedPropertyRepository;
        protected readonly IRepository<Product> _productRepository;
        protected readonly IRepository<ProductAttributeCombination> _productAttributeCombinationRepository;
        protected readonly IRepository<ProductAttributeMapping> _productAttributeMappingRepository;
        protected readonly IRepository<ProductCategory> _productCategoryRepository;
        protected readonly IRepository<ProductManufacturer> _productManufacturerRepository;
        protected readonly IRepository<ProductPicture> _productPictureRepository;
        protected readonly IRepository<ProductProductTagMapping> _productTagMappingRepository;
        protected readonly IRepository<ProductReview> _productReviewRepository;
        protected readonly IRepository<ProductReviewHelpfulness> _productReviewHelpfulnessRepository;
        protected readonly IRepository<ProductSpecificationAttribute> _productSpecificationAttributeRepository;
        protected readonly IRepository<ProductTag> _productTagRepository;
        protected readonly IRepository<ProductWarehouseInventory> _productWarehouseInventoryRepository;
        protected readonly IRepository<RelatedProduct> _relatedProductRepository;
        protected readonly IRepository<Shipment> _shipmentRepository;
        protected readonly IRepository<StockQuantityHistory> _stockQuantityHistoryRepository;
        protected readonly IRepository<TierPrice> _tierPriceRepository;
        protected readonly IRepository<Warehouse> _warehouseRepository;
        protected readonly IStaticCacheManager _staticCacheManager;
        protected readonly IStoreMappingService _storeMappingService;
        protected readonly IStoreService _storeService;
        protected readonly IWorkContext _workContext;
        protected readonly LocalizationSettings _localizationSettings;

        #endregion

        #region Ctor

        public ProductService(CatalogSettings catalogSettings,
            CommonSettings commonSettings,
            IAclService aclService,
            ICustomerService customerService,
            IDateRangeService dateRangeService,
            ILanguageService languageService,
            ILocalizationService localizationService,
            IProductAttributeParser productAttributeParser,
            IProductAttributeService productAttributeService,
            IRepository<CrossSellProduct> crossSellProductRepository,
            IRepository<DiscountProductMapping> discountProductMappingRepository,
            IRepository<LocalizedProperty> localizedPropertyRepository,
            IRepository<Product> productRepository,
            IRepository<ProductAttributeCombination> productAttributeCombinationRepository,
            IRepository<ProductAttributeMapping> productAttributeMappingRepository,
            IRepository<ProductCategory> productCategoryRepository,
            IRepository<ProductManufacturer> productManufacturerRepository,
            IRepository<ProductPicture> productPictureRepository,
            IRepository<ProductProductTagMapping> productTagMappingRepository,
            IRepository<ProductReview> productReviewRepository,
            IRepository<ProductReviewHelpfulness> productReviewHelpfulnessRepository,
            IRepository<ProductSpecificationAttribute> productSpecificationAttributeRepository,
            IRepository<ProductTag> productTagRepository,
            IRepository<ProductWarehouseInventory> productWarehouseInventoryRepository,
            IRepository<RelatedProduct> relatedProductRepository,
            IRepository<Shipment> shipmentRepository,
            IRepository<StockQuantityHistory> stockQuantityHistoryRepository,
            IRepository<TierPrice> tierPriceRepository,
            IRepository<Warehouse> warehouseRepository,
            IStaticCacheManager staticCacheManager,
            IStoreService storeService,
            IStoreMappingService storeMappingService,
            IWorkContext workContext,
            LocalizationSettings localizationSettings)
        {
            _catalogSettings = catalogSettings;
            _commonSettings = commonSettings;
            _aclService = aclService;
            _customerService = customerService;
            _dateRangeService = dateRangeService;
            _languageService = languageService;
            _localizationService = localizationService;
            _productAttributeParser = productAttributeParser;
            _productAttributeService = productAttributeService;
            _crossSellProductRepository = crossSellProductRepository;
            _discountProductMappingRepository = discountProductMappingRepository;
            _localizedPropertyRepository = localizedPropertyRepository;
            _productRepository = productRepository;
            _productAttributeCombinationRepository = productAttributeCombinationRepository;
            _productAttributeMappingRepository = productAttributeMappingRepository;
            _productCategoryRepository = productCategoryRepository;
            _productManufacturerRepository = productManufacturerRepository;
            _productPictureRepository = productPictureRepository;
            _productTagMappingRepository = productTagMappingRepository;
            _productReviewRepository = productReviewRepository;
            _productReviewHelpfulnessRepository = productReviewHelpfulnessRepository;
            _productSpecificationAttributeRepository = productSpecificationAttributeRepository;
            _productTagRepository = productTagRepository;
            _productWarehouseInventoryRepository = productWarehouseInventoryRepository;
            _relatedProductRepository = relatedProductRepository;
            _shipmentRepository = shipmentRepository;
            _stockQuantityHistoryRepository = stockQuantityHistoryRepository;
            _tierPriceRepository = tierPriceRepository;
            _warehouseRepository = warehouseRepository;
            _staticCacheManager = staticCacheManager;
            _storeMappingService = storeMappingService;
            _storeService = storeService;
            _workContext = workContext;
            _localizationSettings = localizationSettings;
        }

        #endregion

        #region Methods

        #region Products

        /// <summary>
        /// Gets a cross-sells
        /// </summary>
        /// <param name="cart">Shopping cart</param>
        /// <param name="numberOfProducts">Number of products to return</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the cross-sells
        /// </returns>
        public virtual async Task<IList<Product>> GetCrossSellProductsByShoppingCartAsync(IList<ShoppingCartItem> cart, int numberOfProducts)
        {
            var result = new List<Product>();

            if (numberOfProducts == 0)
                return result;

            if (cart == null || !cart.Any())
                return result;

            var cartProductIds = new List<int>();
            foreach (var sci in cart)
            {
                var prodId = sci.ProductId;
                if (!cartProductIds.Contains(prodId))
                    cartProductIds.Add(prodId);
            }

            var productIds = cart.Select(sci => sci.MasterProductId).ToArray();
            var crossSells = await GetCrossSellProductsByProductIdsAsync(productIds);
            foreach (var crossSell in crossSells)
            {
                //validate that this product is not added to result yet
                //validate that this product is not in the cart
                if (result.Find(p => p.Id == crossSell.ProductId2) != null || cartProductIds.Contains(crossSell.ProductId2))
                    continue;

                var productToAdd = await GetProductByIdAsync(crossSell.ProductId2);
                //validate product
                if (productToAdd == null || productToAdd.Deleted || !productToAdd.Published)
                    continue;

                //add a product to result
                result.Add(productToAdd);
                if (result.Count >= numberOfProducts)
                    return result;
            }

            return result;
        }

        /// <summary>
        /// Get last inserted master product sku
        /// </summary>
        /// <returns></returns>
        public virtual async Task<string> GetLastInsertedProductSkuAsync()
        {
            var query = from p in _productRepository.Table
                        where p.IsMaster && p.ProductTypeId == (int)ProductType.SimpleProduct && !string.IsNullOrEmpty(p.UPCCode)
                        orderby p.Id descending
                        select p;
            var result = query?.FirstOrDefault()?.UPCCode;
            return result;
        }

        /// <summary>
        /// Get number of product (published and visible) in certain category by geo vendor ids
        /// </summary>
        /// <param name="categoryIds">Category identifiers</param>
        /// <param name="storeId">Store identifier; 0 to load all records</param>
        /// <param name="vendorIds">Vendor identifiers</param>
        /// <param name="isMaster">isMaster false to load all records</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the number of products
        /// </returns>
        public virtual async Task<int> GetNumberOfProductsInCategoryByGeoVendorIdsAsync(IList<int> categoryIds = null, int storeId = 0,
            IList<int> manufacturerIds = null, IList<int> vendorIds = null, decimal? priceMin = null,
            decimal? priceMax = null,
            string keywords = null,
            IList<SpecificationAttributeOption> filteredSpecOptions = null, bool? isMaster = null)
        {
            //validate "categoryIds" parameter
            if (categoryIds != null && categoryIds.Contains(0))
                categoryIds.Remove(0);

            //validate "vendorIds" parameter
            if (vendorIds != null && vendorIds.Contains(0))
                vendorIds.Remove(0);

            var query = _productRepository.Table.Where(p => p.Published && !p.Deleted);

            //apply store mapping constraints
            query = await _storeMappingService.ApplyStoreMapping(query, storeId);
            //apply ACL constraints
            var customer = await _workContext.GetCurrentCustomerAsync();
            var customerRoleIds = await _customerService.GetCustomerRoleIdsAsync(customer);
            query = await _aclService.ApplyAcl(query, customerRoleIds);

            //temp productQuery for price filter
            IQueryable<Product> subProducts = query.Where(p => !p.IsMaster);

            //apply master product filter - along with geoVendors (3-8-22)
            if (isMaster.HasValue)
            {
                //public side request(catalog)
                if (vendorIds is not null)
                {
                    //filter if any vendor available in searched location (filter by searched location) (16-8-22)
                    if (vendorIds.Any())
                    {
                        //vendor available - apply available vendor filter
                        query = await ApplyVendorsGeoRadiusFilter(query, vendorIds);
                    }
                    else
                    {
                        //check wether customer has searched the location
                        //handle - if location has not been searched then show all products
                        if (!string.IsNullOrEmpty(customer.LastSearchedCoordinates))
                            return 0; //location searched and no vendor available
                    }
                }
                //admin side request.
                else
                    query = query.Where(p => p.IsMaster == isMaster.Value);
            }

            //priceing filter - 01-11-22
            if (priceMin != null || priceMax != null)
            {
                IQueryable<Product> tempMasterProductQuery = query;
                query =
                    from p in query
                        //get price (based on simple/grouped product)
                    let price = p.ProductTypeId == (int)ProductType.SimpleProduct ? subProducts.Where(x => x.UPCCode == p.UPCCode).OrderBy(x => x.Price)
                                                                                               .FirstOrDefault().Price :
                                                                                   subProducts.Where(x => x.UPCCode == tempMasterProductQuery.OrderBy(x => x.DisplayOrder).FirstOrDefault(y => y.ParentGroupedProductId == p.Id).UPCCode)
                                                                                               .OrderBy(z => z.Price).FirstOrDefault().Price
                    where
                        (priceMin == null || price >= priceMin) &&
                        (priceMax == null || price <= priceMax)
                    select p;
            }

            //to handle group products
            query = query.Where(p => p.VisibleIndividually);

            if (!string.IsNullOrEmpty(keywords))
            {
                IQueryable<int> productsByKeywords;

                productsByKeywords =
                        from p in _productRepository.Table
                        where p.Name.Contains(keywords)
                        select p.Id;

                query =
                    from p in query
                    join pbk in productsByKeywords on p.Id equals pbk
                    select p;
            }

            //category filtering
            if (categoryIds != null && categoryIds.Any())
            {
                query = from p in query
                        join pc in _productCategoryRepository.Table on p.Id equals pc.ProductId
                        where categoryIds.Contains(pc.CategoryId)
                        select p;
            }

            if (manufacturerIds is not null)
            {
                if (manufacturerIds.Contains(0))
                    manufacturerIds.Remove(0);

                if (manufacturerIds.Any())
                {
                    var productManufacturerQuery =
                        from pm in _productManufacturerRepository.Table
                        where manufacturerIds.Contains(pm.ManufacturerId)
                        group pm by pm.ProductId into pm
                        select new
                        {
                            ProductId = pm.Key,
                            DisplayOrder = pm.First().DisplayOrder
                        };

                    query =
                        from p in query
                        join pm in productManufacturerQuery on p.Id equals pm.ProductId
                        orderby pm.DisplayOrder, p.Name
                        select p;
                }
            }

            if (filteredSpecOptions?.Count > 0)
            {
                var specificationAttributeIds = filteredSpecOptions
                    .Select(sao => sao.SpecificationAttributeId)
                    .Distinct();

                foreach (var specificationAttributeId in specificationAttributeIds)
                {
                    var optionIdsBySpecificationAttribute = filteredSpecOptions
                        .Where(o => o.SpecificationAttributeId == specificationAttributeId)
                        .Select(o => o.Id);

                    var productSpecificationQuery =
                        from psa in _productSpecificationAttributeRepository.Table
                        where psa.AllowFiltering && optionIdsBySpecificationAttribute.Contains(psa.SpecificationAttributeOptionId)
                        select psa;

                    query =
                        from p in query
                        where productSpecificationQuery.Any(pc => pc.ProductId == p.Id)
                        select p;
                }
            }

            //use caching if manufacture, min-max price, spec att & keywords parametor not passed.
            var useCache = manufacturerIds?.Count == 0 && priceMax == null && priceMin == null && string.IsNullOrEmpty(keywords) && filteredSpecOptions?.Count == 0;
            if (useCache)
            {
                var cacheVendorIds = new List<int>();
                if (vendorIds != null && vendorIds.Any())
                    cacheVendorIds.AddRange(vendorIds.OrderBy(x => x));

                var cacheKey = _staticCacheManager
                    .PrepareKeyForDefaultCache(NopCatalogDefaults.CategoryProductsNumberByVendorCacheKey, customerRoleIds, storeId, categoryIds, cacheVendorIds);
                cacheKey.CacheTime = 360;//6h 

                //only distinct products
                return await _staticCacheManager.GetAsync(cacheKey, () => query.Select(p => p.Id).Distinct().Count());
            }

            //removed caching, distinct products using linq
            return query.Select(p => p.Id).Distinct().Count();
        }

        /// <summary>
        /// Gets all products displayed on the home page
        /// </summary>
        /// <param name="isMasterOnly">A value indicating whether to show only master product records</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the products
        /// </returns>
        public virtual async Task<IList<Product>> GetAllProductsDisplayedOnHomepageAsync(bool isMasterOnly = false)
        {
            var products = await _productRepository.GetAllAsync(query =>
            {
                return from p in query
                       orderby p.DisplayOrder, p.Id
                       where p.Published &&
                             !p.Deleted &&
                             p.ShowOnHomepage &&
                             (isMasterOnly && p.IsMaster)
                       select p;
            }, cache => cache.PrepareKeyForDefaultCache(NopCatalogDefaults.ProductsHomepageCacheKey));

            return products;
        }

        /// <summary>
        /// Search products
        /// </summary>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="categoryIds">Category identifiers</param>
        /// <param name="manufacturerIds">Manufacturer identifiers</param>
        /// <param name="storeId">Store identifier; 0 to load all records</param>
        /// <param name="vendorId">Vendor identifier; 0 to load all records</param>
        /// <param name="warehouseId">Warehouse identifier; 0 to load all records</param>
        /// <param name="productType">Product type; 0 to load all records</param>
        /// <param name="visibleIndividuallyOnly">A values indicating whether to load only products marked as "visible individually"; "false" to load all records; "true" to load "visible individually" only</param>
        /// <param name="excludeFeaturedProducts">A value indicating whether loaded products are marked as featured (relates only to categories and manufacturers); "false" (by default) to load all records; "true" to exclude featured products from results</param>
        /// <param name="priceMin">Minimum price; null to load all records</param>
        /// <param name="priceMax">Maximum price; null to load all records</param>
        /// <param name="productTagId">Product tag identifier; 0 to load all records</param>
        /// <param name="keywords">Keywords</param>
        /// <param name="searchDescriptions">A value indicating whether to search by a specified "keyword" in product descriptions</param>
        /// <param name="searchManufacturerPartNumber">A value indicating whether to search by a specified "keyword" in manufacturer part number</param>
        /// <param name="searchSku">A value indicating whether to search by a specified "keyword" in product SKU</param>
        /// <param name="searchProductTags">A value indicating whether to search by a specified "keyword" in product tags</param>
        /// <param name="languageId">Language identifier (search for text searching)</param>
        /// <param name="filteredSpecOptions">Specification options list to filter products; null to load all records</param>
        /// <param name="orderBy">Order by</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <param name="overridePublished">
        /// <param name="isMaster">A value indicating whether to show only master product records</param>
        /// <param name="geoVendorIds">The list of vendor identifiers who are available according geo location</param>
        /// null - process "Published" property according to "showHidden" parameter
        /// true - load only "Published" products
        /// false - load only "Unpublished" products
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the products
        /// </returns>
        public virtual async Task<IPagedList<Product>> SearchProductsAsync(
            int pageIndex = 0,
            int pageSize = int.MaxValue,
            IList<int> categoryIds = null,
            IList<int> manufacturerIds = null,
            int storeId = 0,
            int vendorId = 0,
            int warehouseId = 0,
            ProductType? productType = null,
            bool visibleIndividuallyOnly = false,
            bool excludeFeaturedProducts = false,
            decimal? priceMin = null,
            decimal? priceMax = null,
            int productTagId = 0,
            string keywords = null,
            bool searchDescriptions = false,
            bool searchManufacturerPartNumber = true,
            bool searchSku = true,
            bool searchProductTags = false,
            int languageId = 0,
            IList<SpecificationAttributeOption> filteredSpecOptions = null,
            ProductSortingEnum orderBy = ProductSortingEnum.Position,
            bool showHidden = false,
            bool? overridePublished = null,
            bool? isMaster = null,
            string upccode = null,
            IList<int> geoVendorIds = null,
            string size = null,
            Customer customer = null)
        {
            //some databases don't support int.MaxValue
            if (pageSize == int.MaxValue)
                pageSize = int.MaxValue - 1;

            var productsQuery = _productRepository.Table;

            if (!showHidden)
                productsQuery = productsQuery.Where(p => p.Published);
            else if (overridePublished.HasValue)
                productsQuery = productsQuery.Where(p => p.Published == overridePublished.Value);

            //apply store mapping constraints
            productsQuery = await _storeMappingService.ApplyStoreMapping(productsQuery, storeId);

            //apply ACL constraints
            if (!showHidden)
            {
                if (customer == null)
                    customer = await _workContext.GetCurrentCustomerAsync();

                productsQuery = await _aclService.ApplyAcl(productsQuery, customer);
            }

            productsQuery =
                from p in productsQuery
                where !p.Deleted &&
                    //(!visibleIndividuallyOnly || p.VisibleIndividually) &&
                    (vendorId == 0 || p.VendorId == vendorId) &&
                    (
                        warehouseId == 0 ||
                        (
                            !p.UseMultipleWarehouses ? p.WarehouseId == warehouseId :
                                _productWarehouseInventoryRepository.Table.Any(pwi => pwi.WarehouseId == warehouseId && pwi.ProductId == p.Id)
                        )
                    ) &&
                    (productType == null || p.ProductTypeId == (int)productType)
                //&&
                //(showHidden ||
                //        DateTime.UtcNow >= (p.AvailableStartDateTimeUtc ?? DateTime.MinValue) &&
                //        DateTime.UtcNow <= (p.AvailableEndDateTimeUtc ?? DateTime.MaxValue)
                //)
                select p;

            //temp productQuery for price filter
            IQueryable<Product> subProducts = productsQuery.Where(p => !p.IsMaster);

            //apply master product filter - along with geoVendors (3-8-22)
            if (isMaster.HasValue)
            {
                //public side request(catalog)
                if (geoVendorIds is not null)
                {
                    //filter if any vendor available in searched location (filter by searched location) (16-8-22)
                    if (geoVendorIds.Any())
                    {
                        //vendor available - apply available vendor filter
                        productsQuery = await ApplyVendorsGeoRadiusFilter(productsQuery, geoVendorIds);
                    }
                    else
                    {
                        //check wether customer has searched the location
                        //handle - if location has not been searched then show all products
                        if (customer == null)
                            customer = await _workContext.GetCurrentCustomerAsync();

                        if (!string.IsNullOrEmpty(customer.LastSearchedCoordinates))
                            return await new List<Product>().AsQueryable().ToPagedListAsync(pageIndex, pageSize); //location searched and no vendor available
                    }
                }
                //admin side request.
                else
                    productsQuery = productsQuery.Where(p => p.IsMaster == isMaster.Value);
            }

            //priceing filter - 01-11-22
            if (priceMin != null || priceMax != null)
            {
                IQueryable<Product> tempMasterProductQuery = productsQuery;
                productsQuery =
                    from p in productsQuery
                        //get price (based on simple/grouped product)
                    let price = p.ProductTypeId == (int)ProductType.SimpleProduct ? subProducts.Where(x => x.UPCCode == p.UPCCode).OrderBy(x => x.Price)
                                                                                               .FirstOrDefault().Price :
                                                                                   subProducts.Where(x => x.UPCCode == tempMasterProductQuery.OrderBy(x => x.DisplayOrder).FirstOrDefault(y => y.ParentGroupedProductId == p.Id).UPCCode)
                                                                                               .OrderBy(z => z.Price).FirstOrDefault().Price
                    where
                        (priceMin == null || price >= priceMin) &&
                        (priceMax == null || price <= priceMax)
                    select p;
            }

            //To handle associated product 
            productsQuery =
               from p in productsQuery
               where (!visibleIndividuallyOnly || p.VisibleIndividually)
               select p;

            //upccode product filter
            if (!string.IsNullOrEmpty(upccode))
            {
                //23-01-23
                //Switching UPC - SKU values 
                productsQuery =
                    from p in productsQuery
                    where p.Sku == upccode.Trim()
                    select p;
            }

            //size product filter
            if (!string.IsNullOrEmpty(size))
            {
                productsQuery =
                    from p in productsQuery
                    where p.Size == size
                    select p;
            }

            if (!string.IsNullOrEmpty(keywords))
            {
                var langs = await _languageService.GetAllLanguagesAsync(showHidden: true);

                //Set a flag which will to points need to search in localized properties. If showHidden doesn't set to true should be at least two published languages.
                var searchLocalizedValue = languageId > 0 && langs.Count >= 2 && (showHidden || langs.Count(l => l.Published) >= 2);

                IQueryable<int> productsByKeywords;

                productsByKeywords =
                        from p in _productRepository.Table
                        where p.Name.Contains(keywords) ||
                            (searchDescriptions &&
                                (p.ShortDescription.Contains(keywords) || p.FullDescription.Contains(keywords))) ||
                            (searchManufacturerPartNumber && p.ManufacturerPartNumber == keywords) ||
                            (searchSku && p.Sku == keywords)
                        select p.Id;

                if (searchLocalizedValue)
                {
                    productsByKeywords = productsByKeywords.Union(
                        from lp in _localizedPropertyRepository.Table
                        let checkName = lp.LocaleKey == nameof(Product.Name) &&
                                        lp.LocaleValue.Contains(keywords)
                        let checkShortDesc = searchDescriptions &&
                                        lp.LocaleKey == nameof(Product.ShortDescription) &&
                                        lp.LocaleValue.Contains(keywords)
                        where
                            lp.LocaleKeyGroup == nameof(Product) && lp.LanguageId == languageId && (checkName || checkShortDesc)

                        select lp.EntityId);
                }

                //search by SKU for ProductAttributeCombination
                if (searchSku)
                {
                    productsByKeywords = productsByKeywords.Union(
                        from pac in _productAttributeCombinationRepository.Table
                        where pac.Sku == keywords
                        select pac.ProductId);
                }

                if (searchProductTags)
                {
                    productsByKeywords = productsByKeywords.Union(
                        from pptm in _productTagMappingRepository.Table
                        join pt in _productTagRepository.Table on pptm.ProductTagId equals pt.Id
                        where pt.Name.Contains(keywords)
                        select pptm.ProductId
                    );

                    if (searchLocalizedValue)
                    {
                        productsByKeywords = productsByKeywords.Union(
                        from pptm in _productTagMappingRepository.Table
                        join lp in _localizedPropertyRepository.Table on pptm.ProductTagId equals lp.EntityId
                        where lp.LocaleKeyGroup == nameof(ProductTag) &&
                              lp.LocaleKey == nameof(ProductTag.Name) &&
                              lp.LocaleValue.Contains(keywords) &&
                              lp.LanguageId == languageId
                        select pptm.ProductId);
                    }
                }

                productsQuery =
                    from p in productsQuery
                    join pbk in productsByKeywords on p.Id equals pbk
                    select p;
            }

            if (categoryIds is not null)
            {
                if (categoryIds.Contains(0))
                    categoryIds.Remove(0);

                if (categoryIds.Any())
                {
                    var productCategoryQuery =
                        from pc in _productCategoryRepository.Table
                        where (!excludeFeaturedProducts || !pc.IsFeaturedProduct) &&
                            categoryIds.Contains(pc.CategoryId)
                        group pc by pc.ProductId into pc
                        select new
                        {
                            ProductId = pc.Key,
                            DisplayOrder = pc.First().DisplayOrder
                        };

                    productsQuery =
                        from p in productsQuery
                        join pc in productCategoryQuery on p.Id equals pc.ProductId
                        orderby pc.DisplayOrder, p.Name
                        select p;
                }
            }

            if (manufacturerIds is not null)
            {
                if (manufacturerIds.Contains(0))
                    manufacturerIds.Remove(0);

                if (manufacturerIds.Any())
                {
                    var productManufacturerQuery =
                        from pm in _productManufacturerRepository.Table
                        where (!excludeFeaturedProducts || !pm.IsFeaturedProduct) &&
                            manufacturerIds.Contains(pm.ManufacturerId)
                        group pm by pm.ProductId into pm
                        select new
                        {
                            ProductId = pm.Key,
                            DisplayOrder = pm.First().DisplayOrder
                        };

                    productsQuery =
                        from p in productsQuery
                        join pm in productManufacturerQuery on p.Id equals pm.ProductId
                        orderby pm.DisplayOrder, p.Name
                        select p;
                }
            }

            if (productTagId > 0)
            {
                productsQuery =
                    from p in productsQuery
                    join ptm in _productTagMappingRepository.Table on p.Id equals ptm.ProductId
                    where ptm.ProductTagId == productTagId
                    select p;
            }

            if (filteredSpecOptions?.Count > 0)
            {
                var specificationAttributeIds = filteredSpecOptions
                    .Select(sao => sao.SpecificationAttributeId)
                    .Distinct();

                foreach (var specificationAttributeId in specificationAttributeIds)
                {
                    var optionIdsBySpecificationAttribute = filteredSpecOptions
                        .Where(o => o.SpecificationAttributeId == specificationAttributeId)
                        .Select(o => o.Id);

                    var productSpecificationQuery =
                        from psa in _productSpecificationAttributeRepository.Table
                        where psa.AllowFiltering && optionIdsBySpecificationAttribute.Contains(psa.SpecificationAttributeOptionId)
                        select psa;

                    productsQuery =
                        from p in productsQuery
                        where productSpecificationQuery.Any(pc => pc.ProductId == p.Id)
                        select p;
                }
            }

            return await productsQuery.OrderBy(_localizedPropertyRepository, await _workContext.GetWorkingLanguageAsync(), orderBy, subProducts).ToPagedListAsync(pageIndex, pageSize);
        }

        /// <summary>
        /// Gets products which marked as new
        /// </summary>
        /// <param name="storeId">Store identifier; 0 if you want to get all records</param>
        /// <param name="geoVendorIds">Available geo radius vendor identifiers</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the list of new products
        /// </returns>
        public virtual async Task<IList<Product>> GetProductsMarkedAsNewAsync(int storeId = 0, IList<int> geoVendorIds = null)
        {
            return await _productRepository.GetAllAsync(async query =>
            {
                //apply store mapping constraints
                query = await _storeMappingService.ApplyStoreMapping(query, storeId);

                //apply ACL constraints
                var customer = await _workContext.GetCurrentCustomerAsync();
                query = await _aclService.ApplyAcl(query, customer);

                query = from p in query
                        where p.Published && p.VisibleIndividually && p.MarkAsNew && !p.Deleted &&
                            DateTime.UtcNow >= (p.MarkAsNewStartDateTimeUtc ?? DateTime.MinValue) &&
                            DateTime.UtcNow <= (p.MarkAsNewEndDateTimeUtc ?? DateTime.MaxValue)
                        orderby p.CreatedOnUtc descending
                        select p;

                //filter by avialble georadius vendor
                if (geoVendorIds is not null)
                {
                    //filter if any vendor available in searched location
                    if (geoVendorIds.Any())
                    {
                        //vendor available
                        //get master product if at least one available.
                        var avaialbleProducts = query.Where(p => geoVendorIds.Contains(p.VendorId));
                        var masterProducts = query.Where(p => p.IsMaster == true); //master by default
                        masterProducts = masterProducts.Where(mp => avaialbleProducts.Select(ap => ap.UPCCode).Contains(mp.UPCCode));
                        //assign to query
                        query = masterProducts;
                    }
                    else
                    {
                        //check wether customer has searched the location
                        //handle - if location has not been searched then show all products
                        if (!string.IsNullOrEmpty(customer.LastSearchedCoordinates))
                            return new List<Product>().AsQueryable(); //location searched and no vendor available
                    }
                }

                return query.Take(_catalogSettings.NewProductsNumber);
            });
        }

        /// <summary>
        /// Gets associated products
        /// </summary>
        /// <param name="parentGroupedProductId">Parent product identifier (used with grouped products)</param>
        /// <param name="storeId">Store identifier; 0 to load all records</param>
        /// <param name="vendorId">Vendor identifier; 0 to load all records</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <param name="geoVendorIds">The list of vendor identifiers who are available according geo location</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the products
        /// </returns>
        public virtual async Task<IList<Product>> GetAssociatedProductsAsync(int parentGroupedProductId,
            int storeId = 0, int vendorId = 0, bool showHidden = false, IList<int> geoVendorIds = null)
        {
            var query = _productRepository.Table;
            query = query.Where(x => x.ParentGroupedProductId == parentGroupedProductId);
            if (!showHidden)
            {
                query = query.Where(x => x.Published);

                ////available dates
                //query = query.Where(p =>
                //    (!p.AvailableStartDateTimeUtc.HasValue || p.AvailableStartDateTimeUtc.Value < DateTime.UtcNow) &&
                //    (!p.AvailableEndDateTimeUtc.HasValue || p.AvailableEndDateTimeUtc.Value > DateTime.UtcNow));
            }
            //vendor filtering
            if (vendorId > 0)
            {
                query = query.Where(p => p.VendorId == vendorId);
            }

            query = query.Where(x => !x.Deleted);
            query = query.OrderBy(x => x.DisplayOrder).ThenBy(x => x.Id);

            //filter by geovendors
            if (geoVendorIds is not null)
            {
                //filter if any vendor available in searched location (filter by searched location)
                if (geoVendorIds.Any())
                {
                    //all variant upc codes
                    var upcCodes = query.Select(x => x.UPCCode);

                    //get all upc products of all variant
                    var upcProductQuery = from p in _productRepository.Table
                                          orderby p.Id
                                          where !p.Deleted &&
                                          p.Published &&
                                          upcCodes.Contains(p.UPCCode) &&
                                          !p.IsMaster &&
                                          p.StockQuantity > 0
                                          select p;

                    //vendor available
                    //check available vendor in upc product
                    var availableAssociatedProducts = upcProductQuery.Where(p => geoVendorIds.Contains(p.VendorId));
                    query = query.Where(mp => availableAssociatedProducts.Select(ap => ap.UPCCode).Contains(mp.UPCCode));
                }
                else
                {
                    //check wether customer has searched the location
                    var customer = await _workContext.GetCurrentCustomerAsync();
                    if (!string.IsNullOrEmpty(customer.LastSearchedCoordinates))
                        return new List<Product>(); //location searched and no vendor available
                }
            }

            var products = await query.ToListAsync();

            //ACL mapping
            if (!showHidden)
                products = await products.WhereAwait(async x => await _aclService.AuthorizeAsync(x)).ToListAsync();

            //Store mapping
            if (!showHidden && storeId > 0)
                products = await products.WhereAwait(async x => await _storeMappingService.AuthorizeAsync(x, storeId)).ToListAsync();

            return products;
        }

        /// <summary>
        /// Apply vendors georadius to the passed query
        /// </summary>
        /// <param name="productsQuery">Query to filter</param>
        /// <param name="geoVendorIds">geoVendorIds</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the filtered query
        /// </returns>
        public virtual async Task<IQueryable<Product>> ApplyVendorsGeoRadiusFilter(IQueryable<Product> productsQuery, IList<int> geoVendorIds)
        {
            if (productsQuery is null)
                throw new ArgumentNullException(nameof(productsQuery));

            if (geoVendorIds is null)
                return productsQuery;

            //vendor available
            //get master product if at least one subproduct(vendor product) available.
            var avaialbleProducts = productsQuery.Where(p => geoVendorIds.Contains(p.VendorId) && !p.IsMaster && p.StockQuantity > 0); //available vendor products
            var masterProducts = productsQuery.Where(p => p.IsMaster == true && p.ProductTypeId != (int)ProductType.GroupedProduct);
            masterProducts = masterProducts.Where(mp => avaialbleProducts.Select(ap => ap.UPCCode).Contains(mp.UPCCode));

            //available group products 
            var groupProducts = from gp in productsQuery.Where(p => p.ProductTypeId == (int)ProductType.GroupedProduct)
                                from mp in masterProducts
                                where gp.Id == mp.ParentGroupedProductId &&
                                mp.ParentGroupedProductId != 0
                                select gp.Id;

            //add groupproduct along with master product
            var finalProductIds = masterProducts.Select(x => x.Id).Union(groupProducts);

            //assign to query
            productsQuery =
                from p in productsQuery
                join fpi in finalProductIds on p.Id equals fpi
                select p;

            return await Task.FromResult(productsQuery);
        }

        /// <summary>
        /// Delete a product
        /// </summary>
        /// <param name="product">Product</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task DeleteProductAsync(Product product)
        {
            //++Alchub

            //set updateOnUtc to include product in sync cataLOG system.
            product.UpdatedOnUtc = DateTime.UtcNow;

            //--Alchub

            await _productRepository.DeleteAsync(product);
        }

        /// <summary>
        /// Delete products
        /// </summary>
        /// <param name="products">Products</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task DeleteProductsAsync(IList<Product> products)
        {
            //++Alchub

            foreach (var product in products)
            {
                //set updateOnUtc to include product in sync cataLOG system.
                product.UpdatedOnUtc = DateTime.UtcNow;
            }

            //--Alchub

            await _productRepository.DeleteAsync(products);
        }

        #endregion

        #region Related products

        public virtual async Task<IList<RelatedProduct>> GetRelatedProductsByProductId1Async(int productId, bool showHidden = false, bool? isMaster = null, IList<int> geoVendorIds = null)
        {
            var query = from rp in _relatedProductRepository.Table
                        join p in _productRepository.Table on rp.ProductId2 equals p.Id
                        where rp.ProductId1 == productId &&
                        !p.Deleted &&
                        (showHidden || p.Published) &&
                        (!isMaster.HasValue || p.IsMaster == isMaster.Value)
                        orderby rp.DisplayOrder, rp.Id
                        select rp;

            //product filter - along with geoVendors (3-8-22)
            //Note: not passed geovendor from public side, will pass and manage caching if required. so following code want be executed as of now. 
            //filter wont be applied if no vendor passed(meaning, location has been not searched) (10-8-22)
            if (isMaster.HasValue && geoVendorIds is not null)
            {
                //have to pass 0 vendor if no vendor available. 
                if (geoVendorIds.Any())
                {
                    query = from rp in query
                            join p in _productRepository.Table on rp.ProductId2 equals p.Id
                            where geoVendorIds.Contains(p.VendorId)
                            select rp;
                }
                else
                {
                    //check wether customer has searched the location
                    //handle - if location has not been searched then show all products
                    var customer = await _workContext.GetCurrentCustomerAsync();
                    if (!string.IsNullOrEmpty(customer.LastSearchedCoordinates))
                        return new List<RelatedProduct>(); //location searched and no vendor available
                }
            }

            var relatedProducts = await _staticCacheManager.GetAsync(_staticCacheManager.PrepareKeyForDefaultCache(NopCatalogDefaults.RelatedProductsCacheKey, productId, showHidden, isMaster), async () => await query.ToListAsync());

            return relatedProducts;
        }

        /// <summary>
        /// Gets master products by UPCCode array
        /// </summary>
        /// <param name="upcArray">Upc array</param>
        /// <param name="vendorId">Vendor ID; 0 to load all records</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the products
        /// </returns>

        #endregion

        #region Alchub

        /// <summary>
        /// Get master products by upc code array.
        /// </summary>
        /// <param name="upcCodeArray"></param>
        /// <returns></returns>
        public async Task<IList<Product>> GetMasterProductsByUPCCodeListAsync(string[] upcCodeArray)
        {
            if (upcCodeArray == null)
                throw new ArgumentNullException(nameof(upcCodeArray));

            var query = _productRepository.Table;
            query = query.Where(p => p.IsMaster && upcCodeArray.Contains(p.Sku)); //upccode is sku.

            return await query.ToListAsync();
        }

        public async Task<IList<Product>> GetMasterProductsByUPCCodeAsync(string[] upcArray, int vendorId = 0)
        {
            if (upcArray == null)
                throw new ArgumentNullException(nameof(upcArray));

            var query = _productRepository.Table;
            query = query.Where(p => !p.Deleted && p.IsMaster && upcArray.Contains(p.UPCCode));

            if (vendorId != 0)
                query = query.Where(p => p.VendorId == vendorId);

            return await query.ToListAsync();
        }

        public async Task<IList<Product>> GetMasterProductsBySKUCodeAsync(string[] upcArray, int vendorId = 0)
        {
            if (upcArray == null)
                throw new ArgumentNullException(nameof(upcArray));

            if (!upcArray.Any())
                return new List<Product>();

            //if upc code is 11 digit
            var upcArrayWith11digit = upcArray.Where(x => x.Length == NopAlchubDefaults.PRODUCT_UPC_11_DIGIT);

            //if upc code is not 11 digit
            var upcArrayWithout11digit = upcArray.Where(x => x.Length != NopAlchubDefaults.PRODUCT_UPC_11_DIGIT);

            var query = _productRepository.Table;
            query = query.Where(p => !p.Deleted && p.IsMaster && p.UPCCode != null);

            query = query.Where(p => upcArrayWith11digit.Contains(p.Sku.Substring(0, Math.Min(p.Sku.Length, NopAlchubDefaults.PRODUCT_UPC_11_DIGIT))) || upcArrayWithout11digit.Contains(p.Sku));

            return await query.ToListAsync();
        }

        public virtual async Task<bool> IsProductUPCCodeExist(string upcCode = "")
        {
            bool isProductUPCCode = false;
            var products = _productRepository.Table.Where(o => o.UPCCode == upcCode && !o.Deleted);
            if (products.ToList().Count > 0)
            {
                isProductUPCCode = true;
            }
            return isProductUPCCode;
        }

        /// <summary>
        /// Get reformated product name.
        /// </summary>
        /// <param name="product"></param>
        /// <param name="sci"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public async Task<string> GetProductItemName(Product product, ShoppingCartItem sci)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            if (sci == null)
                throw new ArgumentNullException(nameof(sci));

            return await GetProductItemName(product, sci.MasterProductId, sci.GroupedProductId);
        }

        /// <summary>
        /// Get reformated product name.
        /// </summary>
        /// <param name="product"></param>
        /// <param name="orderItem"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public async Task<string> GetProductItemName(Product product, OrderItem orderItem)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            if (orderItem == null)
                throw new ArgumentNullException(nameof(orderItem));

            return await GetProductItemName(product, orderItem.MasterProductId, orderItem.GroupedProductId);
        }

        /// <summary>
        /// Formate product name
        /// </summary>
        /// <param name="product"></param>
        /// <param name="masterProductId"></param>
        /// <param name="groupedProductId"></param>
        /// <returns></returns>
        private async Task<string> GetProductItemName(Product product, int masterProductId, int groupedProductId)
        {
            //default
            var itemName = await _localizationService.GetLocalizedAsync(product, x => x.Name);
            //master product
            var masterProduct = await GetProductByIdAsync(masterProductId);
            if (groupedProductId == 0)
            {
                if (masterProduct != null)
                    itemName = await _localizationService.GetLocalizedAsync(masterProduct, x => x.Name);
            }
            else
            {
                //grouped product
                var groupedProduct = await GetProductByIdAsync(groupedProductId);
                if (groupedProduct != null)
                {
                    itemName = await _localizationService.GetLocalizedAsync(groupedProduct, x => x.Name);
                    //if (masterProduct != null)
                    //    itemName += $"({masterProduct.Size}, {masterProduct.Container})";
                }
            }

            return itemName;
        }

        /// <summary>
        /// Get average price by master product upc code -- 09/05/23
        /// </summary>
        /// <param name="upcCode"></param>
        /// <returns></returns>
        public virtual Task<decimal> GetProductAveragePriceByUPCCode(string upcCode)
        {
            if (string.IsNullOrEmpty(upcCode))
                return Task.FromResult(decimal.Zero);

            //get all published vendor products by master product upc code where stock quantity is greater than 0.
            var query = from p in _productRepository.Table
                        where p.Published && !p.Deleted && !p.IsMaster
                        && p.UPCCode == upcCode && p.StockQuantity > 0
                        select p;

            if (!query.Any())
                return Task.FromResult(decimal.Zero);

            //get average product price from products
            var averagePrice = query.Average(p => p.Price);
            averagePrice = Math.Round(averagePrice, 2);

            return Task.FromResult(averagePrice);
        }

        /// <summary>
        /// Search master product - for sync catalog system
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="showHidden"></param>
        /// <param name="createdOrUpdatedFromUtc"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public virtual async Task<IPagedList<Product>> SearchMasterProductsAsync(
            int storeId = 0,
            bool showHidden = false,
            DateTime? createdOrUpdatedFromUtc = null,
            int pageIndex = 0,
            int pageSize = int.MaxValue)
        {
            return await _productRepository.GetAllPagedAsync(async query =>
            {
                if (!showHidden)
                    query = query.Where(p => p.Published);

                //apply store mapping constraints
                query = await _storeMappingService.ApplyStoreMapping(query, storeId);

                //exclude deleted
                //query = query.Where(p => !p.Deleted);

                //only master
                query = query.Where(p => p.IsMaster);

                //from date time
                if (createdOrUpdatedFromUtc.HasValue)
                    query = query.Where(p => p.CreatedOnUtc > createdOrUpdatedFromUtc || p.UpdatedOnUtc > createdOrUpdatedFromUtc);

                return query.OrderBy(m => m.DisplayOrder).ThenBy(m => m.Id);
            }, pageIndex, pageSize);
        }

        /// <summary>
        /// Get all undeleted products(specifically for Scheduler to update images)
        /// </summary>
        /// <returns></returns>
        public async Task<IList<Product>> GetAllProductsAsync()
        {
            var query = _productRepository.Table;
            query = query.Where(p => !p.Deleted);
            return await query.ToListAsync();
        }
        #endregion

        /// <summary>
        /// Gets a vendor product
        /// </summary>
        /// <param name="vendorId"></param>
        /// <param name="storeId"></param>
        /// <param name="upcCode"></param>
        /// <param name="sku"></param>
        /// <returns></returns>
        public virtual async Task<Product> GetVendorProduct(int vendorId, int storeId = 0, string upcCode = null, string sku = null)
        {
            if (vendorId == 0)
                throw new ArgumentNullException(nameof(vendorId));

            var query = from p in _productRepository.Table
                        where p.VendorId == vendorId && p.Published
                        && !p.Deleted
                        select p;

            query = await _storeMappingService.ApplyStoreMapping(query, storeId);

            if (!string.IsNullOrEmpty(upcCode))
                query = query.Where(p => p.UPCCode == upcCode);

            if (!string.IsNullOrEmpty(sku))
                query = query.Where(p => p.Sku == sku);

            return query?.FirstOrDefault();
        }

        #region Helpers

        /// <summary>
        /// Auto generate master Product SKU
        /// </summary>
        /// <returns></returns>
        public async Task<string> GenerateMasterProductSKU()
        {
            //get last inserted master product sku
            var lastMasterProductSKU = await GetLastInsertedProductSkuAsync();
            if (!string.IsNullOrEmpty(lastMasterProductSKU) && !string.IsNullOrWhiteSpace(lastMasterProductSKU))
            {
                //split with pattern 
                var skuStr = lastMasterProductSKU.Split(NopAlchubDefaults.PRODUCT_SKU_PATTERN);
                if (skuStr.Length == 2)
                {
                    int.TryParse(skuStr[1], out var number);

                    //incriment by 1
                    number += 1;

                    //prepare padded number string. //{AH000000}
                    var numberStr = number.ToString();
                    if (numberStr.Length <= 6)
                        numberStr = numberStr.PadLeft(6, '0');
                    else
                        numberStr = numberStr.PadLeft(numberStr.Length, '0');

                    return $"{NopAlchubDefaults.PRODUCT_SKU_PATTERN}{numberStr}";
                }
            }

            //if we reach this far, meaning its first product or issue with last product sku pattern.
            return $"{NopAlchubDefaults.PRODUCT_SKU_PATTERN}{1.ToString().PadLeft(6, '0')}"; //{AH000000}
        }

        #endregion

        /// <summary>
        /// Inserts a product
        /// </summary>
        /// <param name="product">Product</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task InsertProductAsync(Product product)
        {
            //++Alchub

            //validate sku duplication 
            //Note:(17-04-23)
            if (product.IsMaster)
            {
                var query = from p in _productRepository.Table
                            where p.IsMaster && p.ProductTypeId == (int)ProductType.SimpleProduct
                            && !string.IsNullOrEmpty(p.UPCCode) && p.UPCCode == product.UPCCode //sku is UPC
                            select p;

                //duplicate record check
                if (query.Any())
                    throw new ArgumentException($"SKU cannot be duplicated, existing master product productId: {query?.FirstOrDefault()?.Id}");
            }

            //--Alchub

            await _productRepository.InsertAsync(product);
        }

        #endregion

        #region  Get Picture 

        /// <summary>
        /// Get product upccode
        /// </summary>
        /// <param name="pictureId"></param>
        /// <returns></returns>
        public async Task<string> GetProductUPCCODEByPictureIdAsync(int pictureId)
        {
            var cacheKey = _staticCacheManager.PrepareKeyForDefaultCache(NopCatalogDefaults.UpccodeProductPictureCacheKey, pictureId);
            var upccode = await _staticCacheManager.GetAsync<string>(cacheKey, () =>
            {
                var query = (from pp in _productPictureRepository.Table
                             join p in _productRepository.Table on pp.ProductId equals p.Id
                             where pp.PictureId == pictureId
                             select p.Sku).FirstOrDefault();
                return query;

            });
            return upccode;
        }

        #endregion  Get Picture 

        #region Elastic Search
        /// <summary>
        /// To get all the data of the product table
        /// </summary>
        public virtual async Task<IList<Product>> GetProductsFromDatabaseAsync()
        {

            var query = from p in _productRepository.Table
                        where !p.Deleted
                        select p;
            var productList = await query.ToListAsync();

            return productList;
        }
        #endregion
    }
}
