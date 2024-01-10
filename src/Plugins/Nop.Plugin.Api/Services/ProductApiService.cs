using Microsoft.Extensions.Logging;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Vendors;
using Nop.Data;
using Nop.Plugin.Api.DataStructures;
using Nop.Plugin.Api.Domain;
using Nop.Plugin.Api.DTOs.Products;
using Nop.Plugin.Api.Infrastructure;
using Nop.Services.Authentication;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Helpers;
using Nop.Services.Stores;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nop.Plugin.Api.Services
{
    public class ProductApiService : IProductApiService
    {
        private readonly IRepository<ProductCategory> _productCategoryMappingRepository;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly CatalogSettings _catalogSettings;
        private readonly IRepository<Product> _productRepository;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IProductService _productService;
        private readonly IRepository<Vendor> _vendorRepository;
        private readonly ICustomerApiService _customerApiService;
        private readonly ICustomerService _customerService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IWorkContext _workContext;
        protected readonly IRepository<CrossSellProduct> _crossSellProductRepository;
        private readonly IAuthenticationService _authenticationService;
        private readonly Nop.Services.Logging.ILogger _logger;

        public ProductApiService(
            IRepository<Product> productRepository,
            IDateTimeHelper dateTimeHelper,
            IRepository<ProductCategory> productCategoryMappingRepository,
            CatalogSettings catalogSettings,
            IRepository<Vendor> vendorRepository,
            IStoreMappingService storeMappingService,
            IProductService productService,
            ICustomerService customerService,
            IGenericAttributeService genericAttributeService,
            IWorkContext workContext,
            IRepository<CrossSellProduct> crossSellProductRepository,
            IAuthenticationService authenticationService,
            Nop.Services.Logging.ILogger logger)
        {
            _productRepository = productRepository;
            _productCategoryMappingRepository = productCategoryMappingRepository;
            _catalogSettings = catalogSettings;
            _vendorRepository = vendorRepository;
            _storeMappingService = storeMappingService;
            _productService = productService;
            _dateTimeHelper = dateTimeHelper;
            _customerService = customerService;
            _genericAttributeService = genericAttributeService;
            _workContext = workContext;
            _crossSellProductRepository = crossSellProductRepository;
            _authenticationService = authenticationService;
            _logger = logger;
        }

        public IList<Product> GetProducts(
            IList<int> ids = null,
            DateTime? createdAtMin = null, DateTime? createdAtMax = null, DateTime? updatedAtMin = null, DateTime? updatedAtMax = null,
            int? limit = null, int? page = null,
            int? sinceId = null,
            int? categoryId = null, string vendorName = null, bool? publishedStatus = null, IList<string> manufacturerPartNumbers = null, bool? isDownload = null)
        {
            var query = GetProductsQuery(createdAtMin, createdAtMax, updatedAtMin, updatedAtMax, vendorName, publishedStatus, ids, categoryId, manufacturerPartNumbers, isDownload);

            if (sinceId > 0)
            {
                query = query.Where(c => c.Id > sinceId);
            }

            return new ApiList<Product>(query, (page ?? Constants.Configurations.DEFAULT_PAGE_VALUE) - 1, (limit ?? Constants.Configurations.DEFAULT_LIMIT));
        }

        public async Task<int> GetProductsCountAsync(
            DateTime? createdAtMin = null, DateTime? createdAtMax = null,
            DateTime? updatedAtMin = null, DateTime? updatedAtMax = null, bool? publishedStatus = null, string vendorName = null,
            int? categoryId = null, IList<string> manufacturerPartNumbers = null, bool? isDownload = null)
        {
            var query = GetProductsQuery(createdAtMin, createdAtMax, updatedAtMin, updatedAtMax, vendorName,
                                         publishedStatus, ids: null, categoryId, manufacturerPartNumbers, isDownload);

            return await query.WhereAwait(async p => await _storeMappingService.AuthorizeAsync(p)).CountAsync();
        }

        public Product GetProductById(int productId)
        {
            if (productId == 0)
            {
                return null;
            }

            return _productRepository.Table.FirstOrDefault(product => product.Id == productId && !product.Deleted);
        }

        public Product GetProductByIdNoTracking(int productId)
        {
            if (productId == 0)
            {
                return null;
            }

            return _productRepository.Table.FirstOrDefault(product => product.Id == productId && !product.Deleted);
        }

        private IQueryable<Product> GetProductsQuery(
            DateTime? createdAtMin = null, DateTime? createdAtMax = null,
            DateTime? updatedAtMin = null, DateTime? updatedAtMax = null, string vendorName = null,
            bool? publishedStatus = null, IList<int> ids = null, int? categoryId = null, IList<string> manufacturerPartNumbers = null, bool? isDownload = null)

        {
            var query = _productRepository.Table;

            if (ids != null && ids.Count > 0)
            {
                query = query.Where(p => ids.Contains(p.Id));
            }

            if (manufacturerPartNumbers != null && manufacturerPartNumbers.Count > 0)
            {
                query = query.Where(p => manufacturerPartNumbers.Contains(p.ManufacturerPartNumber));
            }

            if (publishedStatus != null)
            {
                query = query.Where(p => p.Published == publishedStatus.Value);
            }

            if (isDownload != null)
            {
                query = query.Where(p => p.IsDownload == isDownload.Value);
            }

            // always return products that are not deleted!!!
            query = query.Where(p => !p.Deleted);

            if (createdAtMin != null)
            {
                query = query.Where(p => p.CreatedOnUtc > createdAtMin.Value);
            }

            if (createdAtMax != null)
            {
                query = query.Where(p => p.CreatedOnUtc < createdAtMax.Value);
            }

            if (updatedAtMin != null)
            {
                query = query.Where(p => p.UpdatedOnUtc > updatedAtMin.Value);
            }

            if (updatedAtMax != null)
            {
                query = query.Where(p => p.UpdatedOnUtc < updatedAtMax.Value);
            }

            if (!string.IsNullOrEmpty(vendorName))
            {
                query = from vendor in _vendorRepository.Table
                        join product in _productRepository.Table on vendor.Id equals product.VendorId
                        where vendor.Name == vendorName && !vendor.Deleted && vendor.Active
                        select product;
            }

            if (categoryId != null)
            {
                var categoryMappingsForProduct = from productCategoryMapping in _productCategoryMappingRepository.Table
                                                 where productCategoryMapping.CategoryId == categoryId
                                                 select productCategoryMapping;

                query = from product in query
                        join productCategoryMapping in categoryMappingsForProduct on product.Id equals productCategoryMapping.ProductId
                        select product;
            }

            query = query.OrderBy(product => product.Id);

            return query;
        }

        public IList<Product> GetfeaturedProducts()
        {
            var query = _productRepository.Table.Where(c => c.ShowOnHomepage == true);

            return query.ToList();
        }

        public IList<Product> GetDealsOfTheDayProducts()
        {
            var query = _productRepository.Table.Where(c => c.Price > 0 && c.OldPrice > 0);

            return query.ToList();
        }

        private IQueryable<Product> GetProductsQuery(string productType, string vendorName = null, int? categoryId = null)

        {
            var query = _productRepository.Table;

            if (productType == ProductType.SimpleProduct.ToString())
            {
                query = query.Where(p => p.ProductTypeId == (int)ProductType.SimpleProduct);
            }

            if (productType == ProductType.GroupedProduct.ToString())
            {
                query = query.Where(p => p.ProductTypeId == (int)ProductType.GroupedProduct);
            }

            // always return products that are not deleted!!!
            query = query.Where(p => !p.Deleted);

            if (!string.IsNullOrEmpty(vendorName))
            {
                query = from vendor in _vendorRepository.Table
                        join product in _productRepository.Table on vendor.Id equals product.VendorId
                        where vendor.Name == vendorName && !vendor.Deleted && vendor.Active
                        select product;
            }

            if (categoryId != null)
            {
                var categoryMappingsForProduct = from productCategoryMapping in _productCategoryMappingRepository.Table
                                                 where productCategoryMapping.CategoryId == categoryId
                                                 select productCategoryMapping;

                query = from product in query
                        join productCategoryMapping in categoryMappingsForProduct on product.Id equals productCategoryMapping.ProductId
                        select product;
            }

            query = query.OrderBy(product => product.Id);

            return query;
        }

        /// <summary>
        /// Get product list / Search Product
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="productType"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="keywords"></param>
        /// <returns></returns>
        public async Task<IPagedList<Product>> GetProductsList(int userId, int storeId = 0, IList<int> categoryIds = null, int productType = 0, int? pageIndex = null, int? pageSize = null, string keywords = null, decimal? priceMin = null, decimal? priceMax = null, ProductSortingEnum? orderBy = null, IList<int> manufacturerIds = null, IList<SpecificationAttributeOption> filteredSpecOptions = null, IList<int> vendorIds = null)
        {
            //use nop search service.
            var page = ((pageIndex ?? Constants.Configurations.DEFAULT_PAGE_VALUE) - 1);
            if (page < 0)
                page = 0;

            //api customer
            var customer = await _authenticationService.GetAuthenticatedCustomerAsync();
            //prepare available vendors
            var availableVendorIds = new List<int>();
            if (vendorIds != null && vendorIds.Any())
                availableVendorIds = vendorIds.ToList();
            else
                availableVendorIds = (await _workContext.GetCurrentCustomerGeoRadiusVendorIdsAsync(customer))?.ToList();

            var products = await _productService.SearchProductsAsync(pageIndex: page,
            pageSize: pageSize ?? Constants.Configurations.DEFAULT_LIMIT,
            storeId: storeId,
            visibleIndividuallyOnly: true,
            categoryIds: categoryIds,
            productType: productType > 0 ? (ProductType?)productType : null,
            keywords: keywords,
            priceMax: priceMax,
            priceMin: priceMin,
            orderBy: orderBy > 0 ? (ProductSortingEnum)orderBy : ProductSortingEnum.Position,
            manufacturerIds: manufacturerIds,
            filteredSpecOptions: filteredSpecOptions,
            //master products only filter along with geoVendorIds
            isMaster: true,
            geoVendorIds: availableVendorIds,
            customer: customer,
            //exclude other search exept product name
            searchDescriptions: false,
            searchManufacturerPartNumber: false,
            searchSku: false,
            searchProductTags: false,
            languageId: 0);

            return products;
        }

        public virtual async Task<IPagedList<Product>> GetProductByIdsAsync(
        IList<int> productIds,
        int? pageIndex = 0,
        int? pageSize = int.MaxValue,
        int storeId = 0)
        {
            var page = ((pageIndex ?? Constants.Configurations.DEFAULT_PAGE_VALUE) - 1);
            if (page <= 0)
                page = 1;

            var products = await _productService.GetProductsByIdsAsync(productIds.ToArray());

            var pagedProducts = new PagedList<Product>(products, page - 1, pageSize ?? Constants.Configurations.DEFAULT_LIMIT);

            return pagedProducts;
        }

        public async Task<AllReviewDto> GetAllReview(int productId = 0)
        {
            var isApproved = !_catalogSettings.ProductReviewsMustBeApproved;
            var productReview = await _productService.GetAllProductReviewsAsync(approved: isApproved, productId: productId);
            var totalCount = productReview.Count();
            var allReviewsDto = new AllReviewDto();
            allReviewsDto.TotalRating = totalCount;

            var reviewCount = productReview.Count(x => x.ReviewText != null);
            allReviewsDto.TotalReview = reviewCount;

            var ratingCount1 = productReview.Count();
            allReviewsDto.RatingCount = ratingCount1;

            var ratingSum = productReview.Sum(x => x.Rating);

            var fivestarCount = productReview.Count(x => x.Rating == 5);
            var fourstarCount = productReview.Count(x => x.Rating == 4);
            var threestarCount = productReview.Count(x => x.Rating == 3);
            var twostarCount = productReview.Count(x => x.Rating == 2);
            var onestarCount = productReview.Count(x => x.Rating == 1);

            allReviewsDto.FiveStar = fivestarCount;
            allReviewsDto.FourStar = fourstarCount;
            allReviewsDto.ThreeStar = threestarCount;
            allReviewsDto.TwoStar = twostarCount;
            allReviewsDto.OneStar = onestarCount;

            foreach (var review in productReview)
            {
                var customer = await _customerService.GetCustomerByIdAsync(review.CustomerId);
                var fullName = await _customerService.GetCustomerFullNameAsync(customer);
                var reviewModel = new ReviewModel();
                reviewModel.ReviewId = review.Id;
                reviewModel.ReviewByName = fullName;
                reviewModel.ReviewText = review.ReviewText;
                reviewModel.Rating = review.Rating;
                reviewModel.ReviewTitle = review.Title;
                reviewModel.ReviewDate = review.CreatedOnUtc.ToString("MM/dd/yyyy HH:mm tt").Replace('-', '/');
                reviewModel.LikeCount = review.HelpfulYesTotal;
                reviewModel.DislikeCount = review.HelpfulNoTotal;

                allReviewsDto.ReviewList.Add(reviewModel);
            }

            //Rating Avg
            decimal ratingAvg = 0;
            if (ratingSum != null && ratingSum != 0)
                ratingAvg = ((decimal)ratingSum / (decimal)allReviewsDto.TotalRating);
            ratingAvg = Math.Round(ratingAvg, 1);
            allReviewsDto.RatingAvg = ratingAvg;

            return allReviewsDto;
        }

        public async Task<decimal> GetProductsPriceMaxAsync(int? categoryId = null)
        {
            decimal price = 0;
            var query = _productRepository.Table;
            if (categoryId != null)
            {
                var categoryMappingsForProduct = from productCategoryMapping in _productCategoryMappingRepository.Table
                                                 where productCategoryMapping.CategoryId == categoryId
                                                 select productCategoryMapping;

                query = from product in query
                        join productCategoryMapping in categoryMappingsForProduct on product.Id equals productCategoryMapping.ProductId
                        select product;
            }
            price = query.OrderByDescending(x => x.Price).Select(x => x.Price).FirstOrDefault();
            return price;
        }

        public async Task<decimal> GetProductsPriceMinAsync(int? categoryId = null)
        {
            decimal price = 0;
            var query = _productRepository.Table;
            if (categoryId != null)
            {
                var categoryMappingsForProduct = from productCategoryMapping in _productCategoryMappingRepository.Table
                                                 where productCategoryMapping.CategoryId == categoryId
                                                 select productCategoryMapping;

                query = from product in query
                        join productCategoryMapping in categoryMappingsForProduct on product.Id equals productCategoryMapping.ProductId
                        select product;
            }
            price = query.OrderBy(x => x.Price).Select(x => x.Price).FirstOrDefault();
            return price;
        }

        #region Compare products

        /// <summary>
        /// Adds a product to a "compare products" list (for API)
        /// </summary>
        /// <param name="productId">Product identifier</param>
        /// <param name="customer">Customer</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task AddProductToCompareListAsync(int productId, Customer customer)
        {
            //get list of compared product identifiers
            var comparedProductIds = await GetComparedProductIds(customer);

            //whether product identifier to add already exist
            if (!comparedProductIds.Contains(productId))
                comparedProductIds.Insert(0, productId);

            await AddCompareProductsAttribute(comparedProductIds, customer);
        }

        /// <summary>
        /// Gets a "compare products" list
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the "Compare products" list
        /// </returns>
        public virtual async Task<IList<Product>> GetComparedProductsAsync(Customer customer)
        {
            //get list of compared product identifiers
            var productIds = await GetComparedProductIds(customer);

            //return list of product
            return (await _productService.GetProductsByIdsAsync(productIds.ToArray()))
                .Where(product => product.Published && !product.Deleted).ToList();
        }

        /// <summary>
        /// Removes a product from a "compare products" list
        /// </summary>
        /// <param name="productId">Product identifier</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task RemoveProductFromCompareListAsync(int productId, Customer customer)
        {
            if (customer == null)
                return;

            //get list of compared product identifiers
            var comparedProductIds = await GetComparedProductIds(customer);

            //whether product identifier to remove exists
            if (!comparedProductIds.Contains(productId))
                return;

            //it exists, so remove it from list
            comparedProductIds.Remove(productId);

            await AddCompareProductsAttribute(comparedProductIds, customer);
        }

        /// <summary>
        /// Add cookie value for the compared products
        /// </summary>
        /// <param name="comparedProductIds">Collection of compared products identifiers</param>
        protected async Task AddCompareProductsAttribute(IEnumerable<int> comparedProductIds, Customer customer)
        {
            //add/update productIds
            await _genericAttributeService.SaveAttributeAsync(customer, NopCustomerApiDefaults.ComparedProductsAttribute, comparedProductIds);
        }

        /// <summary>
        /// Get a list of identifier of compared products (for API)
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <returns>List of identifier</returns>
        protected async Task<List<int>> GetComparedProductIds(Customer customer)
        {
            if (customer == null)
                return new List<int>();

            //try to get productsids value from generic attributes
            var productIds = await _genericAttributeService.GetAttributeAsync<List<int>>(customer, NopCustomerApiDefaults.ComparedProductsAttribute);
            if (productIds == null || !productIds.Any())
                return new List<int>();

            //return list of int product identifiers
            return productIds.Distinct().ToList();
        }

        #endregion

        #region Crossell Products
        /// <summary>
        /// Gets cross-sell products by product identifier
        /// </summary>
        /// <param name="productIds">The first product identifiers</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the cross-sell products
        /// </returns>
        public virtual async Task<IList<CrossSellProduct>> GetCrossSellProductsByProductIdsAsync(int[] productIds, bool showHidden = false)
        {
            if (productIds == null || productIds.Length == 0)
                return new List<CrossSellProduct>();

            var query = from csp in _crossSellProductRepository.Table
                        join p in _productRepository.Table on csp.ProductId2 equals p.Id
                        where productIds.Contains(csp.ProductId1) &&
                              !p.Deleted &&
                              (showHidden || p.Published)
                        orderby csp.Id
                        select csp;
            var crossSellProducts = await query.ToListAsync();

            return crossSellProducts;
        }
        #endregion
    }
}
