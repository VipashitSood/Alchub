using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Alchub.Domain.Catalog;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Localization;
using Nop.Data;
using Nop.Services.Localization;
using Nop.Services.Security;
using Nop.Services.Stores;

namespace Nop.Services.Alchub.Catalog
{
    public partial class ProductFriendlyUpcService : IProductFriendlyUpcService
    {
        #region Fields

        private readonly IRepository<ProductFriendlyUpcCode> _productFriendlyUpcRepository;
        private readonly IRepository<Product> _productRepository;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IWorkContext _workContext;
        private readonly IAclService _aclService;
        private readonly ILanguageService _languageService;
        private readonly IRepository<LocalizedProperty> _localizedPropertyRepository;
        private readonly IRepository<ProductCategory> _productCategoryRepository;
        private readonly IRepository<ProductManufacturer> _productManufacturerRepository;

        #endregion

        #region Ctor

        public ProductFriendlyUpcService(IRepository<ProductFriendlyUpcCode> productFriendlyUpcRepository,
            IRepository<Product> productRepository,
            IStoreMappingService storeMappingService,
            IWorkContext workContext,
            IAclService aclService,
            ILanguageService languageService,
            IRepository<LocalizedProperty> localizedPropertyRepository,
            IRepository<ProductCategory> productCategoryRepository,
            IRepository<ProductManufacturer> productManufacturerRepository)
        {
            _productFriendlyUpcRepository = productFriendlyUpcRepository;
            _productRepository = productRepository;
            _storeMappingService = storeMappingService;
            _workContext = workContext;
            _aclService = aclService;
            _languageService = languageService;
            _localizedPropertyRepository = localizedPropertyRepository;
            _productCategoryRepository = productCategoryRepository;
            _productManufacturerRepository = productManufacturerRepository;
        }

        #endregion

        /// <summary>
        /// Insert a product friendly upc code
        /// </summary>
        /// <param name="productFriendlyUpcCode"></param>
        /// <returns></returns>
        public virtual async Task InsertProductFriendlyUpcCodeAsync(ProductFriendlyUpcCode productFriendlyUpcCode)
        {
            if (productFriendlyUpcCode == null)
                throw new ArgumentNullException(nameof(productFriendlyUpcCode));

            await _productFriendlyUpcRepository.InsertAsync(productFriendlyUpcCode);
        }

        /// <summary>
        /// Update a product friendly upc code
        /// </summary>
        /// <param name="productFriendlyUpcCode"></param>
        /// <returns></returns>
        public virtual async Task UpdateProductFriendlyUpcCodeAsync(ProductFriendlyUpcCode productFriendlyUpcCode)
        {
            if (productFriendlyUpcCode == null)
                throw new ArgumentNullException(nameof(productFriendlyUpcCode));

            await _productFriendlyUpcRepository.UpdateAsync(productFriendlyUpcCode);
        }

        /// <summary>
        /// Delete a product friendly upc code
        /// </summary>
        /// <param name="productFriendlyUpcCode"></param>
        /// <returns></returns>
        public virtual async Task DeleteProductFriendlyUpcCodeAsync(ProductFriendlyUpcCode productFriendlyUpcCode)
        {
            if (productFriendlyUpcCode == null)
                throw new ArgumentNullException(nameof(productFriendlyUpcCode));

            await _productFriendlyUpcRepository.DeleteAsync(productFriendlyUpcCode);
        }

        /// <summary>
        /// Get a product friendly upc code by identifier
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual async Task<ProductFriendlyUpcCode> GetProductFriendlyUpcCodeByIdAsync(int id)
        {
            return await _productFriendlyUpcRepository.GetByIdAsync(id);
        }

        /// <summary>
        /// Get a product friendly upc codes
        /// </summary>
        /// <param name="vendorId"></param>
        /// <param name="masterProductId"></param>
        /// <returns></returns>
        public virtual async Task<IList<ProductFriendlyUpcCode>> GetProductFriendlyUpcCodesAsync(int vendorId = 0, int masterProductId = 0)
        {
            var query = _productFriendlyUpcRepository.Table;

            if (vendorId > 0)
                query = query.Where(q => q.VendorId == vendorId);

            if (masterProductId > 0)
                query = query.Where(q => q.MasterProductId == masterProductId);

            return await query.ToListAsync();
        }

        /// <summary>
        /// Get a vendor product friendly upc code by master product idenifiers
        /// </summary>
        /// <param name="vendorId"></param>
        /// <param name="masterProductId"></param>
        /// <returns></returns>
        public virtual async Task<ProductFriendlyUpcCode> GetVendorProductFriendlyUpcCodeByMasterProductIdAsync(int vendorId, int masterProductId)
        {
            return (await GetProductFriendlyUpcCodesAsync(vendorId, masterProductId)).FirstOrDefault();
        }

        /// <summary>
        /// Get a vendor product friendly upc record by friendly upc code
        /// </summary>
        /// <param name="vendorId"></param>
        /// <param name="friendlyUpcCode"></param>
        /// <returns></returns>
        public virtual async Task<ProductFriendlyUpcCode> GetVendorProductFriendlyUpcRecordByFriendlyUpcCodeAsync(int vendorId, string friendlyUpcCode)
        {
            if (vendorId <= 0)
                throw new ArgumentOutOfRangeException(nameof(vendorId));

            if (string.IsNullOrEmpty(friendlyUpcCode))
                throw new ArgumentNullException(nameof(friendlyUpcCode));

            var query = _productFriendlyUpcRepository.Table;

            //trim space
            friendlyUpcCode = friendlyUpcCode.Trim();
            query = query.Where(q => q.VendorId == vendorId && q.FriendlyUpcCode == friendlyUpcCode);

            return await query.FirstOrDefaultAsync();
        }

        /// <summary>
        /// Get master product by frienly upc code
        /// </summary>
        /// <param name="vendorId"></param>
        /// <param name="friendlyUpcCode"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public async Task<Product> GetMasterProductByFriendlyUPCCodeAsync(int vendorId, string friendlyUpcCode)
        {
            if (vendorId <= 0)
                throw new ArgumentOutOfRangeException(nameof(vendorId));

            if (friendlyUpcCode == null)
                throw new ArgumentNullException(nameof(friendlyUpcCode));

            //trim space
            friendlyUpcCode = friendlyUpcCode.Trim();

            var masterProductQuery = from p in _productRepository.Table
                                     join fpu in _productFriendlyUpcRepository.Table on p.Id equals fpu.MasterProductId
                                     where
                                     !p.Deleted && p.IsMaster && !string.IsNullOrEmpty(p.UPCCode) && //product filter
                                     fpu.VendorId == vendorId && fpu.FriendlyUpcCode.Equals(friendlyUpcCode) //freindly upc filter
                                     select p;

            return await masterProductQuery.FirstOrDefaultAsync();
        }

        /// <summary>
        /// Delete a product friendly upc code mapping
        /// </summary>
        /// <param name="productFriendlyUpcCode"></param>
        /// <returns></returns>
        public virtual async Task DeleteProductFriendlyUpcCodeMappingsAsync(Product masterProduct)
        {
            if (masterProduct == null)
                return;

            //get all records
            var mappingRecords = await GetProductFriendlyUpcCodesAsync(masterProductId: masterProduct.Id);
            foreach (var mappingRecord in mappingRecords)
            {
                await _productFriendlyUpcRepository.DeleteAsync(mappingRecord);
            }
        }

        /// <summary>
        /// Search master products
        /// </summary>
        /// <param name="vendorId">Vendor identifier; 0 to load all records</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="categoryIds">Category identifiers</param>
        /// <param name="manufacturerIds">Manufacturer identifiers</param>
        /// <param name="storeId">Store identifier; 0 to load all records</param>
        /// <param name="productType">Product type; 0 to load all records</param>
        /// <param name="visibleIndividuallyOnly">A values indicating whether to load only products marked as "visible individually"; "false" to load all records; "true" to load "visible individually" only</param>
        /// <param name="excludeFeaturedProducts">A value indicating whether loaded products are marked as featured (relates only to categories and manufacturers); "false" (by default) to load all records; "true" to exclude featured products from results</param>
        /// <param name="keywords">Keywords</param>
        /// <param name="searchDescriptions">A value indicating whether to search by a specified "keyword" in product descriptions</param>
        /// <param name="searchManufacturerPartNumber">A value indicating whether to search by a specified "keyword" in manufacturer part number</param>
        /// <param name="searchSku">A value indicating whether to search by a specified "keyword" in product SKU</param>
        /// <param name="languageId">Language identifier (search for text searching)</param>
        /// <param name="orderBy">Order by</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <param name="overridePublished">
        /// <param name="isMaster">A value indicating whether to show only master product records</param>
        /// <param name="friendlyUpcCode">friendly upc code</param>
        /// null - process "Published" property according to "showHidden" parameter
        /// true - load only "Published" products
        /// false - load only "Unpublished" products
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the products
        /// </returns>
        public virtual async Task<IPagedList<Product>> SearchMasterProductsAsync(
            int vendorId,
            int pageIndex = 0,
            int pageSize = int.MaxValue,
            IList<int> categoryIds = null,
            IList<int> manufacturerIds = null,
            int storeId = 0,
            ProductType? productType = null,
            bool visibleIndividuallyOnly = false,
            bool excludeFeaturedProducts = false,
            string keywords = null,
            bool searchDescriptions = false,
            bool searchManufacturerPartNumber = true,
            bool searchSku = true,
            int languageId = 0,
            ProductSortingEnum orderBy = ProductSortingEnum.Position,
            bool showHidden = false,
            bool? overridePublished = null,
            bool? isMaster = null,
            string upccode = null,
            string size = null,
            Customer customer = null,
            string friendlyUpcCode = null)
        {
            if (vendorId <= 0)
                throw new ArgumentOutOfRangeException(nameof(vendorId));

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
                   (productType == null || p.ProductTypeId == (int)productType)
               select p;

            //apply master product filter - along with geoVendors (3-8-22)
            if (isMaster.HasValue)
            {
                productsQuery = productsQuery.Where(p => p.IsMaster == isMaster.Value);
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
                        where (isMaster.HasValue || p.IsMaster == isMaster.Value) &&
                            p.Name.Contains(keywords) ||
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

            //friendly upc code
            var friendlyUpcQuery = _productFriendlyUpcRepository.Table;
            friendlyUpcQuery = friendlyUpcQuery.Where(f => f.VendorId == vendorId);
            if (!string.IsNullOrEmpty(friendlyUpcCode))
            {
                var productsByFriendlyUpc = from p in _productRepository.Table
                                            join fpu in friendlyUpcQuery on p.Id equals fpu.MasterProductId
                                            where
                                            !p.Deleted && p.IsMaster && !string.IsNullOrEmpty(p.UPCCode) && //product filter
                                            fpu.FriendlyUpcCode.Contains(friendlyUpcCode) //freindly upc filter
                                            select p.Id;

                productsQuery =
                   from p in productsQuery
                   join pbf in productsByFriendlyUpc on p.Id equals pbf
                   orderby p.DisplayOrder, p.Id //order by friendly upc
                   select p;
            }
            else
            {
                productsQuery = from p in productsQuery
                                join fpu in friendlyUpcQuery on p.Id equals fpu.MasterProductId into fpuGroup
                                from fpu in fpuGroup.DefaultIfEmpty()
                                orderby fpu.Id descending, p.DisplayOrder, p.Id
                                select p;
            }

            return await productsQuery.ToPagedListAsync(pageIndex, pageSize);
        }
    }
}
