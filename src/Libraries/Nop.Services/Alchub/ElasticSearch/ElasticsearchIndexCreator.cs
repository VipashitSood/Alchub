using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Nest;
using Newtonsoft.Json;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Logging;
using Nop.Services.Catalog;
using Nop.Services.Logging;
using Nop.Services.Seo;
using Nop.Services.Vendors;

namespace Nop.Services.Alchub.ElasticSearch
{
    public class ElasticsearchIndexCreator : IElasticsearchIndexCreator
    {
        #region Fields
        private readonly IConfiguration _configuration;
        private readonly IProductService _productService;
        private readonly IVendorService _vendorService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IManufacturerService _manufacturerService;
        private readonly ICategoryService _categoryService;
        private readonly ISpecificationAttributeService _specificationAttributeService;
        private readonly ILogger _logger;
        private readonly IElasticSearchProductService _iElasticSearchProductService;
        #endregion

        #region ctor
        public ElasticsearchIndexCreator(IConfiguration configuration,
            IProductService productService,
            IVendorService vendorService,
            IUrlRecordService urlRecordService,
            IManufacturerService manufacturerService,
            ICategoryService categoryService,
            ISpecificationAttributeService specificationAttributeService,
            ILogger logger,
            IElasticSearchProductService iElasticSearchProductService)
        {
            _configuration = configuration;
            _productService = productService;
            _vendorService = vendorService;
            _urlRecordService = urlRecordService;
            _manufacturerService = manufacturerService;
            _categoryService = categoryService;
            _specificationAttributeService = specificationAttributeService;
            _logger = logger;
            _iElasticSearchProductService = iElasticSearchProductService;
        }
        #endregion

        #region method
        private ElasticClient GetElasticClient()
        {
            var elasticsearchSettings = _configuration.GetSection("elasticsearch");

            if (elasticsearchSettings == null)
            {
                _logger.ErrorAsync("Elasticsearch configuration section is missing.");
                return null;
            }

            var elasticSearchUrl = elasticsearchSettings["Url"];
            var elasticSearchIndex = elasticsearchSettings["index"];
            var elasticSearchBasicAuthUser = elasticsearchSettings["BasicAuthUser"];
            var elasticSearchBasicAuthPassword = elasticsearchSettings["BasicAuthPassword"];

            if (string.IsNullOrEmpty(elasticSearchUrl))
            {
                _logger.ErrorAsync("Elasticsearch configuration is missing elastic search URL.");
                return null;
            }

            if (string.IsNullOrEmpty(elasticSearchIndex))
            {
                _logger.ErrorAsync("Elasticsearch configuration is missing elastic search Index.");
                return null;
            }

            var settings = new ConnectionSettings(new Uri(elasticSearchUrl))
                .DefaultIndex(elasticSearchIndex);

            if (string.IsNullOrEmpty(elasticSearchBasicAuthUser))
            {
                _logger.ErrorAsync("Elasticsearch configuration is missing user name.");
                return null;
            }

            if (string.IsNullOrEmpty(elasticSearchBasicAuthPassword))
            {
                _logger.ErrorAsync("Elasticsearch configuration is missing user password.");
                return null;
            }

            if (!string.IsNullOrEmpty(elasticSearchBasicAuthUser) && !string.IsNullOrEmpty(elasticSearchBasicAuthPassword))
                settings = settings.BasicAuthentication(elasticSearchBasicAuthUser, elasticSearchBasicAuthPassword);

            settings = settings.DisableDirectStreaming();

            return new ElasticClient(settings);

        }

        public async Task CreateElasticsearchIndex()
        {
            // To get data from db using service
            //var products = await _productService.GetProductsFromDatabaseAsync();
            int page = 1;
            long noOfProducts = 10;
            long totalCount = noOfProducts;
            var allProducts = await _iElasticSearchProductService.SyncAllProducts(page, noOfProducts);
            var productTotalCounts = allProducts.TotalCount;

            if (productTotalCounts > 0)
            {
                totalCount = productTotalCounts;
            }

            try
            {
                // Create the first index if there are products to index
                await IndexElasticsearchRecord(allProducts);

                for (var i = page + 1; i <= totalCount / noOfProducts; i++)
                {
                    allProducts = await _iElasticSearchProductService.SyncAllProducts(i, noOfProducts);

                    // Create the new index if there are products to index
                    await IndexElasticsearchRecord(allProducts);
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions here (e.g., log the exception).
                await _logger.ErrorAsync($"An error occurred while creating Elasticsearch index: {ex.Message}");
            }
        }
        /// <summary>
        /// For index products
        /// </summary>
        /// <param name="allProducts"></param>
        /// <returns></returns>
        private async Task IndexElasticsearchRecord(Master_products_result allProducts)
        {
            var elasticSearchSettings = _configuration.GetSection("elasticsearch");
            var indexName = string.Empty;
            if (elasticSearchSettings != null && !string.IsNullOrEmpty(elasticSearchSettings["index"]))
                indexName = elasticSearchSettings["index"];

            try
            {
                var elasticClient = GetElasticClient();

                if (elasticClient == null)
                {
                    // Handle the case where GetElasticClient() returns null, log an error, and exit.
                    await _logger.ErrorAsync("Elasticsearch client is null.");
                    return;
                }

                if (allProducts.Master_Products.Any())
                {
                    // Check if the "default index already exists
                    var indexExists = elasticClient.Indices.Exists(indexName).Exists;

                    if (!indexExists)
                    {
                        try
                        {
                            await CreateIndexAsync(indexName);
                            await _logger.InformationAsync($"Index {indexName} created successfully.");
                        }
                        catch (Exception createIndexEx)
                        {
                            await _logger.ErrorAsync($"An error occurred during index creation: {createIndexEx.Message}");
                        }
                    }

                    await IndexProductAsync(indexName, allProducts.Master_Products);
                }
                else
                    await _logger.InformationAsync("No products to index, skipping index creation.");
            }
            catch (Exception ex)
            {
                // Handle exceptions here (e.g., log the exception).
                await _logger.ErrorAsync($"An error occurred: {ex.Message}");
            }
        }

        [Obsolete]
        public async Task CreateIndexAsync(string newIndexName)
        {
            try
            {
                var elasticClient = GetElasticClient();

                if (elasticClient == null)
                {
                    await _logger.ErrorAsync("Elasticsearch client is null.");
                    return;
                }

                var createIndexResponse = await elasticClient.Indices.CreateAsync(newIndexName, index => index
                .Settings(s => s
                    .Analysis(a => a
                        .Analyzers(an => an
                            .Custom("custom_keyword_analyzer", u => u
                                .Tokenizer("keyword")
                                .Filters("lowercase")
                            )
                        )
                    )
                )
                .Mappings(m => m
                    .Map<Master_products>(x => x
                        .AutoMap()
                        .Properties(p => p
                            .Nested<CategoryDetails>(nested => nested
                                .Name(n => n.Categories)
                                .AutoMap()
                            )
                            .Nested<ManufacturerDetails>(nested => nested
                                .Name(n => n.Manufacturers)
                                .AutoMap()
                            )
                            .Nested<SpecificationAttributeDetails>(nested => nested
                                .Name(n => n.Specifications)
                                .AutoMap()
                                .Properties(innerSpecProps => innerSpecProps
                                    .Nested<SpecificationAttributeOptionDetails>(nestedSpecOption => nestedSpecOption
                                        .Name(n => n.SpecificationAttributeOptionDetails)
                                        .AutoMap()
                                        .Properties(innerSpecOptionProps => innerSpecOptionProps
                                            .Number(nnn => nnn
                                                .Name(nnnn => nnnn.TotalDocuments)
                                                .Type(NumberType.Long)
                                            )
                                        )
                                    )
                                )
                            )
                            .Nested<VendorDetails>(nested => nested
                                .Name(n => n.Vendors)
                                .AutoMap()
                            )
                            .Number(n => n
                                .Name(p => p.Id)
                                .Type(NumberType.Integer)
                            )
                            .Text(t => t
                                .Name(p => p.SeName)
                            )
                            .Object<Product>(o => o
                                .Name(p => p.Product)
                                .AutoMap()
                                .Properties(innerProductProps => innerProductProps
                                    .Text(t => t
                                        .Name(p => p.Name)
                                        .Fields(f => f
                                            .Text(t1 => t1
                                                .Name("raw")
                                                .Analyzer("custom_keyword_analyzer")
                                                .Fielddata(true)
                                            )
                                        )
                                    )
                                    .Number(n => n
                                        .Name(p => p.Id)
                                    )
                                )
                            )
                        )
                    )
                )
                );

                // Handle index creation failure
                if (!createIndexResponse.IsValid)
                    await _logger.ErrorAsync($"Failed to create index: {createIndexResponse.ServerError?.Error?.Reason}");
                else
                    await _logger.InformationAsync($"Index {newIndexName} created successfully.");
            }
            catch (Exception ex)
            {
                // Handle exceptions here (e.g., log the exception).
                await _logger.ErrorAsync($"An error occurred while creating Elasticsearch index: {ex.Message}");
            }
        }


        /// <summary>
        /// Prepare products in the format needs to be indexed
        /// </summary>
        /// <param name="products">List of products</param>
        /// <returns></returns>
        IList<Master_products> CreateVendorProducts(IList<Product> products)
        {
            if (products == null || !products.Any())
                // No products are found
                return new List<Master_products>();
            var masterProducts = new List<Master_products>();
            var distinctUPCCodes = products.Select(p => p.UPCCode).Distinct();
            foreach (var upcCode in distinctUPCCodes)
            {
                try
                {
                    var productGroup = products.Where(p => p.UPCCode == upcCode).ToList();
                    var product = productGroup.Where(p => p.IsMaster == true).FirstOrDefault();
                    if (product != null)
                    {
                        var masterProduct = new Master_products();
                        masterProduct.Id = product.Id;
                        masterProduct.Product = product;
                        masterProduct.SeName = _urlRecordService.GetSeNameAsync(product).Result ?? string.Empty;

                        //Categories mapping
                        var productcategoriesmapping = _categoryService.GetProductCategoriesByProductIdAsync(product.Id).GetAwaiter().GetResult();
                        if (productcategoriesmapping != null)
                        {
                            var categoryIds = productcategoriesmapping.Select(c => c.CategoryId).ToArray();
                            var productCategories = _categoryService.GetCategoriesByIdsAsync(categoryIds).GetAwaiter().GetResult();

                            //Categories list
                            masterProduct.Categories = productCategories.Select(c => new CategoryDetails
                            {
                                Category = c,
                                SeName = _urlRecordService.GetSeNameAsync(c).Result ?? string.Empty
                            }).ToList();
                        }

                        //Manufacturers mapping
                        var productmanufacturersmapping = _manufacturerService.GetProductManufacturersByProductIdAsync(product.Id).GetAwaiter().GetResult();
                        if (productmanufacturersmapping != null)
                        {
                            var manufacturerIds = productmanufacturersmapping.Select(c => c.ManufacturerId).ToArray();
                            var productmanufactureres = _manufacturerService.GetManufacturersByIdsAsync(manufacturerIds).GetAwaiter().GetResult();

                            //Manufacturers list
                            masterProduct.Manufacturers = productmanufactureres.Select(c => new ManufacturerDetails
                            {
                                Manufacturer = c
                            }).ToList();
                        }

                        //Specification Attributes mapping
                        var productspecificationsmapping = _specificationAttributeService.GetProductSpecificationAttributesAsync(product.Id).GetAwaiter().GetResult();
                        if (productspecificationsmapping != null)
                        {

                            var specificationIds = productspecificationsmapping.Select(c => c.SpecificationAttributeOptionId).ToArray();
                            var productSpecifications = _specificationAttributeService.GetSpecificationAttributeOptionsByIdsAsync(specificationIds).GetAwaiter().GetResult();

                            var specificationAttributeList = new List<SpecificationAttribute>();

                            //Get the list of Specification Attributes
                            foreach (var item in productSpecifications)
                            {
                                var specificationAttribute = _specificationAttributeService.GetSpecificationAttributeByIdAsync(item.SpecificationAttributeId).Result;
                                specificationAttributeList.Add(specificationAttribute);
                            }
                            specificationAttributeList = specificationAttributeList.DistinctBy(c => c.Id).ToList();

                            // Create a list to hold SpecificationAttributeDetails
                            var specificationAttributeDetailsList = new List<SpecificationAttributeDetails>();

                            foreach (var specificationAttribute in specificationAttributeList)
                            {
                                var specificationAttributeDetail = new SpecificationAttributeDetails();
                                specificationAttributeDetail.SpecificationAttribute = specificationAttribute;

                                var specificationAttributeOptionList = productSpecifications
                                    .Where(c => c.SpecificationAttributeId == specificationAttribute.Id)
                                    .Select(specOption => new SpecificationAttributeOptionDetails
                                    {
                                        SpecificationAttributeOption = specOption
                                    })
                                    .ToList();

                                specificationAttributeDetail.SpecificationAttributeOptionDetails = specificationAttributeOptionList;

                                specificationAttributeDetailsList.Add(specificationAttributeDetail);
                            }

                            masterProduct.Specifications = specificationAttributeDetailsList;
                        }

                        //Vendors list
                        masterProduct.Vendors = productGroup.Select(p =>
                        {
                            if (p.VendorId > 0)
                            {
                                var vendor = _vendorService.GetVendorByIdAsync(p.VendorId).GetAwaiter().GetResult();
                                if (vendor != null)
                                {
                                    return new VendorDetails
                                    {
                                        Vendor = vendor,
                                        StockQuantity = p.StockQuantity,
                                        Price = p.Price
                                    };
                                }
                            }
                            return null;
                        }).Where(v => v != null).ToList();

                        masterProducts.Add(masterProduct);
                    }
                }
                catch (Exception exc)
                {
                    _logger.ErrorAsync($"Error while preparing products list. {exc.Message}", exc);
                    throw;
                }

            }
            return masterProducts;
        }

        /// <summary>
        /// Index product in elastic index
        /// </summary>
        /// <param name="productModels"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task IndexProductAsync(string indexName, IList<Master_products> productModels)
        {
            int successfullyIndexedCount = 0;

            var elasticClient = GetElasticClient();
            if (elasticClient == null)
                return;

            foreach (var product in productModels)
            {
                try
                {
                    // Use the Update API with upsert to handle both insert and update
                    var updateResponse = await elasticClient.UpdateAsync<Master_products, object>(
                        product.Id,
                        u => u.Index(indexName)
                              .Doc(product)
                              .DocAsUpsert());

                    if (updateResponse.IsValid)
                    {
                        if (updateResponse.Result == Result.Created || updateResponse.Result == Result.Updated)
                            successfullyIndexedCount++;
                        else
                            await _logger.WarningAsync($"Product {product.Id} update result: {updateResponse.Result}");
                    }
                    else
                        await _logger.ErrorAsync($"Failed to index/update product {product.Id}: {updateResponse.ServerError}");
                }
                catch (Exception ex)
                {
                    await _logger.ErrorAsync($"Error indexing product {product.Id}: {ex.Message}", ex);
                }
            }

            if (successfullyIndexedCount > 0)
            {
                await _logger.InformationAsync($"{successfullyIndexedCount} products indexed/updated successfully.");
            }
        }

        public async Task<bool> DeleteProductDocumentAsync(int productId)
        {
            var elasticClient = GetElasticClient();
            if (elasticClient == null)
                return false;

            var elasticSearchSettings = _configuration.GetSection("elasticsearch");
            var indexName = string.Empty;
            if (elasticSearchSettings != null && !string.IsNullOrEmpty(elasticSearchSettings["index"]))
                indexName = elasticSearchSettings["index"];

            var response = await elasticClient.DeleteAsync<Master_products>(productId, u => u.Index(indexName));

            return response.IsValid;
        }

        #endregion
    }

}
