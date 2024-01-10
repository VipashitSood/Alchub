using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Data;
using Nop.Plugin.Widgets.JCarousel.Domain;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Services.Alchub.General;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static StackExchange.Redis.Role;
using Nop.Services.Catalog;
using Nop.Plugin.Widgets.JCarousel.Infrastructure.Cache;
using DocumentFormat.OpenXml.Spreadsheet;
using Nop.Plugin.Widgets.JCarousel.Models.Configuration;

namespace Nop.Plugin.Widgets.JCarousel.Services
{
    public partial class JCarouselService : IJCarouselService
    {
        #region Fields
        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<ProductJCarouselMapping> _productJcarouselRepository;
        private readonly IRepository<JCarouselLog> _jCarousalRepository;
        private readonly IRepository<ProductJCarouselMapping> _productjCarousalRepository;
        private readonly IWorkContext _workContext;
        private readonly IAlchubGeneralService _alchubGeneralService;
        private readonly IProductService _productService;
        private readonly IStaticCacheManager _staticCacheManager;
        #endregion

        #region Ctor

        public JCarouselService(
            IRepository<Product> productRepository,
            IRepository<ProductJCarouselMapping> productJcarouselRepository,
            IRepository<JCarouselLog> jCarousalRepository,
            IRepository<ProductJCarouselMapping> productjCarousalRepository,
            IWorkContext workContext,
            IAlchubGeneralService alchubGeneralService,
            IProductService productService,
            IStaticCacheManager staticCacheManager)
        {
            _productRepository = productRepository;
            _productJcarouselRepository = productJcarouselRepository;
            _jCarousalRepository = jCarousalRepository;
            _productjCarousalRepository = productjCarousalRepository;
            _workContext = workContext;
            _alchubGeneralService = alchubGeneralService;
            _productService = productService;
            _staticCacheManager = staticCacheManager;
        }

        #endregion

        #region Utilities

        /// <summary>

        #endregion

        #region Methods
        /// <summary>
        /// Get all Jcarousel Ids
        /// </summary>
        public virtual async Task<IList<JCarouselLog>> GetAllJcarouselIds()
        {
            var rez = await _jCarousalRepository.GetAllAsync(query =>
            {
                return from pm in _jCarousalRepository.Table
                       orderby pm.DisplayOrder, pm.Id
                       select pm;
            }, cache => cache.PrepareKeyForDefaultCache(ModelCacheEventConsumer.CAROUSEL_ALL_KEY));

            return rez;
        }

        /// <summary>
        /// Inserts a Jcarousel
        /// </summary>
        /// <param name="jCarousel">Jcarousel</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task InsertJCarouselAsync(JCarouselLog jCarousel)
        {
            await _jCarousalRepository.InsertAsync(jCarousel);
        }

        /// <summary>
        /// Delete a Jcarousel
        /// </summary>
        /// <param name="jCarousel">Jcarousel</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task DeleteJCarouselAsync(JCarouselLog jCarousel)
        {
            await _jCarousalRepository.DeleteAsync(jCarousel);
        }

        /// <summary>
        /// Gets jcarousel
        /// </summary>
        /// <param name="jCarouselId">Jcarousel identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the product
        /// </returns>
        public virtual async Task<JCarouselLog> GetJCarouselByIdAsync(int jCarouselId)
        {
            return await _jCarousalRepository.GetByIdAsync(jCarouselId, cache => default);
        }

        /// <summary>
        /// Updates a Jcarousel
        /// </summary>
        /// <param name="jCarousel">Jcarousel</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task UpdateJCarouselAsync(JCarouselLog jCarousel)
        {
            await _jCarousalRepository.UpdateAsync(jCarousel);
        }

        /// <summary>
        /// Gets all jcarousels
        /// </summary>
        /// <param name="name">JCarousel name</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the reviews
        /// </returns>
        public virtual async Task<IPagedList<JCarouselLog>> GetAllJCarouselsAsync(string name = null,
            int pageIndex = 0, int pageSize = int.MaxValue,
            bool showHidden = false)
        {
            return await _jCarousalRepository.GetAllPagedAsync(query =>
            {
                if (!string.IsNullOrWhiteSpace(name))
                    query = query.Where(a => a.Name.Contains(name));
                query = query.Distinct().OrderByDescending(a => a.Id);

                return query.OrderBy(a => a.Id);
            }, pageIndex, pageSize);
        }

        /// <summary>
        /// Gets product jcarousel mapping collection
        /// </summary>
        /// <param name="jacrouselId">Jcarousel identifier</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the product a jcarousel mapping collection
        /// </returns>
        public virtual async Task<IPagedList<ProductJCarouselMapping>> GetProductJCarouselsByJCarouselIdAsync(int jacrouselId,
            int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false)
        {
            if (jacrouselId == 0)
                return new PagedList<ProductJCarouselMapping>(new List<ProductJCarouselMapping>(), pageIndex, pageSize);

            var query = from pm in _productJcarouselRepository.Table
                        join p in _productRepository.Table on pm.ProductId equals p.Id
                        where pm.JCarouselId == jacrouselId
                        orderby pm.DisplayOrder, pm.Id
                        select pm;

            return await query.ToPagedListAsync(pageIndex, pageSize);

        }

        /// <summary>
        /// Gets a product jcarousel mapping 
        /// </summary>
        /// <param name="productJcarouselId">Product jcarousel mapping identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the product category mapping
        /// </returns>
        public virtual async Task<ProductJCarouselMapping> GetProductJCarouselByIdAsync(int productJcarouselId)
        {
            return await _productJcarouselRepository.GetByIdAsync(productJcarouselId, cache => default);
        }

        /// <summary>
        /// Updates the product jcarousel mapping 
        /// </summary>
        /// <param name="productJcarousel">>Product jcarousel mapping</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task UpdateProductJCarouselAsync(ProductJCarouselMapping productJcarousel)
        {
            await _productJcarouselRepository.UpdateAsync(productJcarousel);
        }

        /// <summary>
        /// Deletes the product jcarousel mapping 
        /// </summary>
        /// <param name="productJcarousel">>Product jcarousel mapping</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task DeleteProductJCarouselAsync(ProductJCarouselMapping productJcarousel)
        {
            await _productJcarouselRepository.DeleteAsync(productJcarousel);
        }

        /// <summary>
        /// Returns a ProductJcarousel that has the specified values
        /// </summary>
        /// <param name="source">Source</param>
        /// <param name="productId">Product identifier</param>
        /// <param name="jcarouselId">Jcarousel identifier</param>
        /// <returns>A ProductJcarousel that has the specified values; otherwise null</returns>
        public virtual ProductJCarouselMapping FindProductJCarousel(IList<ProductJCarouselMapping> source, int productId, int jcarouselId)
        {
            foreach (var productRecipe in source)
                if (productRecipe.ProductId == productId && productRecipe.JCarouselId == jcarouselId)
                    return productRecipe;

            return null;
        }

        /// <summary>
        /// Insert the product jcarousel mapping 
        /// </summary>
        /// <param name="productJcarousel">>Product jcarousel mapping</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task InsertProductJCarouselAsync(ProductJCarouselMapping productJcarousel)
        {
            await _productJcarouselRepository.InsertAsync(productJcarousel);
        }
        #endregion

        /// <summary>
        /// Get Data source of products with Jcarousel identifier 
        /// </summary>
        /// <param name="jCarouselId">Jcarousel identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the result
        /// </returns>
        public IList<int> GetAllDatasource(int jCarouselId)
        {
            var query = from pm in _jCarousalRepository.Table
                        where pm.Id == jCarouselId
                        select pm.DataSourceTypeId;

            return query.ToList();
        }

        /// <summary>
        /// Gets Product ids by jcarousel identifier
        /// </summary>
        /// <param name="jacrouselId">Jcarousel identifier</param>
        /// <param name="recordsToReturn">Number of records to return. 0 if you want to get all items</param>
        /// <param name="geoVendorIds">Available geo radius vendor identifiers</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the pictures
        /// </returns>
        public virtual async Task<IList<Product>> GetProductByJcarouselIdAsync(int jacrouselId, IList<int> geoVendorIds = null, int takeNumberOfProducts = 0)
        {
            if (geoVendorIds != null)
                geoVendorIds = geoVendorIds.OrderBy(x => x).ToList();

            var cacheKey = _staticCacheManager.PrepareKeyForDefaultCache(ModelCacheEventConsumer.ProductsByCarauselCacheKey, jacrouselId, geoVendorIds, takeNumberOfProducts);
            return await _staticCacheManager.GetAsync(cacheKey, async () =>
            {
                if (jacrouselId == 0)
                    return new List<Product>();

                var productsQuery = from p in _productRepository.Table
                                    where !p.Deleted && p.Published
                                    select p;

                //filter by avialble georadius vendor
                if (geoVendorIds is not null)
                {
                    //filter if any vendor available in searched location
                    if (geoVendorIds.Any())
                    {
                        //vendor available - apply available vendor filter
                        productsQuery = await _productService.ApplyVendorsGeoRadiusFilter(productsQuery, geoVendorIds);
                    }
                    else
                    {
                        //check wether customer has searched the location
                        //handle - if location has not been searched then show all products
                        var customer = await _workContext.GetCurrentCustomerAsync();
                        if (!string.IsNullOrEmpty(customer.LastSearchedCoordinates))
                            return new List<Product>(); //location searched and no vendor available
                    }
                }

                //carousel product filter
                productsQuery = from p in productsQuery
                                join pp in _productjCarousalRepository.Table on p.Id equals pp.ProductId
                                orderby pp.DisplayOrder, pp.Id
                                where pp.JCarouselId == jacrouselId
                                select p;
                if (takeNumberOfProducts > 0)
                    productsQuery = productsQuery.Take(takeNumberOfProducts);
                //return await productsQuery?.Select(x => x.Id)?.ToListAsync();
                return await productsQuery?.ToListAsync();
            });
        }

        /// <summary>
        /// Gets a Jcarousel
        /// </summary>
        /// <param name="jcarouselId">Jcarousel identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the category
        /// </returns>
        public virtual async Task<JCarouselLog> GetJcarouselByIdAsync(int jcarouselId)
        {
            return await _jCarousalRepository.GetByIdAsync(jcarouselId, cache => default);
        }

        /// <summary>
        /// Check and delete the products mapping with Jcarousel identifier
        /// </summary>
        /// <param name="jcarouselId">Jcarousel identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the result
        /// </returns>
        public async Task DeleteProductReferenceAsync(int jcarouselId)
        {

            var query = from p in _productJcarouselRepository.Table
                        where p.JCarouselId == jcarouselId
                        select p;
            var products = query.ToList();
            foreach (var product in products)
            {
                await _productJcarouselRepository.DeleteAsync(product);
            }
        }

        /// <summary>
        /// Check if the Jcaousel name already exists
        /// </summary>
        /// <param name="name">Jcarousel name to be checked</param>
        public virtual string CheckExistingName(string name)
        {
            var jcarouselNames = from sci in _jCarousalRepository.Table
                                 where sci.Name == name
                                 select sci.Name;
            var emailList = jcarouselNames.FirstOrDefault();
            return emailList;
        }
    }
}
