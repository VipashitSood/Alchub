using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Elasticsearch.Net;
using Microsoft.Extensions.Configuration;
using Nest;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Services.Catalog;
using Nop.Services.Logging;

namespace Nop.Services.Alchub.ElasticSearch
{
    public class ElasticsearchManagerService : IElasticsearchManagerService
    {
        #region  Fields
        private readonly IConfiguration _configuration;
        private readonly ISpecificationAttributeService _specificationAttributeService;
        private readonly ILogger _logger;
        private readonly IProductService _productService;
        #endregion

        #region ctor
        public ElasticsearchManagerService(IConfiguration configuration,
            ISpecificationAttributeService specificationAttributeService,
            ILogger logger,
            IProductService productService)
        {
            _configuration = configuration;
            _specificationAttributeService = specificationAttributeService;
            _logger = logger;
            _productService = productService;
        }
        #endregion

        #region Utilities

        private async Task<ElasticClient> GetElasticClient()
        {
            var elasticsearchSettings = _configuration.GetSection("elasticsearch");

            if (elasticsearchSettings == null)
            {
                await _logger.ErrorAsync("Elasticsearch configuration section is missing.");
                return null;
            }

            var elasticSearchUrl = elasticsearchSettings["Url"];
            var elasticSearchIndex = elasticsearchSettings["index"];
            var elasticSearchBasicAuthUser = elasticsearchSettings["BasicAuthUser"];
            var elasticSearchBasicAuthPassword = elasticsearchSettings["BasicAuthPassword"];

            if (string.IsNullOrEmpty(elasticSearchUrl))
            {
                await _logger.ErrorAsync("Elasticsearch configuration is missing elastic search URL.");
                return null;
            }

            if (string.IsNullOrEmpty(elasticSearchIndex))
            {
                await _logger.ErrorAsync("Elasticsearch configuration is missing elastic search Index.");
                return null;
            }

            var settings = new ConnectionSettings(new Uri(elasticSearchUrl))
                .DefaultIndex(elasticSearchIndex);

            if (string.IsNullOrEmpty(elasticSearchBasicAuthUser))
            {
                await _logger.ErrorAsync("Elasticsearch configuration is missing user name.");
                return null;
            }

            if (string.IsNullOrEmpty(elasticSearchBasicAuthPassword))
            {
                await _logger.ErrorAsync("Elasticsearch configuration is missing user password.");
                return null;
            }

            if (!string.IsNullOrEmpty(elasticSearchBasicAuthUser) && !string.IsNullOrEmpty(elasticSearchBasicAuthPassword))
                settings = settings.BasicAuthentication(elasticSearchBasicAuthUser, elasticSearchBasicAuthPassword);

            settings = settings.DisableDirectStreaming();

            return new ElasticClient(settings);

        }

        /// <summary>
        /// Replace associated product with group product
        /// </summary>
        /// <param name="sourceProducts"></param>
        /// <returns></returns>
        private async Task<IEnumerable<Product>> ReplaceGroupProductsOnAssociatedProductsInSoruce(IList<Product> sourceProducts)
        {
            if (sourceProducts == null)
                return new List<Product>().AsEnumerable();

            if (!sourceProducts.Any())
                return new List<Product>().AsEnumerable();

            //replace associated product with group product if any. 27-09-23
            var associatedProducts = sourceProducts.Where(p => p.ParentGroupedProductId > 0).ToList();
            var tmpProducts = sourceProducts.ToList();
            foreach (var associatedProduct in associatedProducts)
            {
                //get group product 
                var groupProduct = await _productService.GetProductByIdAsync(associatedProduct.ParentGroupedProductId);
                if (groupProduct != null)
                {
                    //get associated product index
                    var indexOfProduct = sourceProducts.ToList().IndexOf(associatedProduct);
                    if (indexOfProduct != -1)
                    {
                        //replace associated product with group product
                        tmpProducts[indexOfProduct] = groupProduct;
                    }
                }
            }

            return tmpProducts.AsEnumerable();
        }

        #endregion

        #region Elastic Search Method
        /// <summary>
        /// Returns list of master products and total no of master products
        /// </summary>
        /// <param name="keywords"></param>
        /// <param name="pageNumber"></param>
        /// <param name="categoryIds"></param>
        /// <param name="manufacturerIds"></param>
        /// <param name="geoVendorIds"></param>
        /// <param name="filteredSpecOptions"></param>
        /// <param name="priceMin"></param>
        /// <param name="priceMax"></param>
        /// <param name="orderBy"></param>
        /// <returns></returns>

        public virtual async Task<Master_products_result> GetMasterProductsListViaElasticSearchAsync(
        string keywords = null,
        int pageNumber = 0,
        int pageSize = int.MaxValue,
        int storeId = 0,
        bool visibleIndividuallyOnly = true,
        bool? isMaster = true,
        IList<int> categoryIds = null,
        IList<int> manufacturerIds = null,
        IList<int> geoVendorIds = null,
        IList<int> selectedVendorIds = null,
        IList<int> filteredSpecOptions = null,
        decimal? priceMin = null,
        decimal? priceMax = null,
        ProductSortingEnum orderBy = ProductSortingEnum.Position,
        int parentCategoryId = 0,
        IList<(int, double)> associateProductIds = null)
        {
            var elasticClient = await GetElasticClient();
            if (elasticClient == null)
            {
                await _logger.ErrorAsync("Elasticsearch client is null.");
                return new Master_products_result();
            }

            var elasticSearchSettings = _configuration.GetSection("elasticsearch");
            var indexName = string.Empty;

            if (elasticSearchSettings != null && !string.IsNullOrEmpty(elasticSearchSettings["index"]))
                indexName = elasticSearchSettings["index"];

            if (associateProductIds == null)
                associateProductIds = new List<(int, double)>();

            var searchTerms = keywords == null
                ? string.Empty
                : keywords.Trim();

            //Get the lists of specifications options ids w.r.t. specification attributes
            List<List<int>> groupedOptionsList = new List<List<int>> { new List<int>() };
            if (filteredSpecOptions != null)
            {
                int[] myArray = filteredSpecOptions.ToArray();

                var specAttributeOptions = _specificationAttributeService.GetSpecificationAttributeOptionsByIdsAsync(myArray).GetAwaiter().GetResult();

                groupedOptionsList = GroupSpecificationOptions(specAttributeOptions);

                static List<List<int>> GroupSpecificationOptions(IList<SpecificationAttributeOption> options)
                {
                    var groupedOptions = options
                        .GroupBy(option => option.SpecificationAttributeId)
                        .Select(group => group.Select(option => option.Id).ToList())
                        .ToList();

                    return groupedOptions;
                }
            }

            SearchDescriptor<Master_products> searchDescriptor;

            searchDescriptor = new SearchDescriptor<Master_products>()
                               .From((pageNumber - 1) * pageSize)
                               .Size(pageSize)
                               .Index(indexName);
            if (geoVendorIds != null && geoVendorIds.Count > 0)
            {
                var specificationTermsMustClauses = GenerateSpecificationTermsMustClauses(groupedOptionsList);

                var shouldClauses = associateProductIds.Select(id => GenerateShouldClause(id.Item1, id.Item2)).ToList();

                searchDescriptor.Query(q => q
                .Bool(b => b
                    .Should(sh => sh
                            .Bool(ba => ba
                                .Must(ms => ms.MultiMatch(mlt => mlt
                                        .Query(searchTerms)
                                        .Fuzziness(Fuzziness.Auto)
                                        .Boost(100)
                                        .Fields(f => f
                                                .Field(x => x.Product.Name, boost: 1000)
                                                .Field(y => y.Product.ShortDescription, boost: 200)
                                                .Field(z => z.Product.FullDescription, boost: 100)
                                                )
                                        ),
                                     ms => ms.MultiMatch(mlt => mlt
                                        .Query(searchTerms)
                                        .Fuzziness(Fuzziness.EditDistance(2))
                                        .PrefixLength(2) // Fuzziness won't apply to substrings with length less than 2
                                        .Fields(f => f
                                                .Field(x => x.Product.Name, boost: 1000)
                                                .Field(y => y.Product.ShortDescription, boost: 200)
                                                .Field(z => z.Product.FullDescription, boost: 100)
                                               )
                                       ),
                                     ms => ms.Term(st => st.Field(bt => bt.Product.VisibleIndividually).Value(true))
                                    )
                                ),
                                 q => q.Bool(bq => bq
                                .Should(
                                     shouldClauses.ToArray()
                                )
                            )
                            ))
                              )
                            .PostFilter(pf => pf
                                .Bool(gf => gf
                                    .Filter(GenerateSpecificationTermsMustClauses(groupedOptionsList, isCategoryAggs: false, isSubCategoryAggs: false, isVendorsAggs: false, isProductVendorCheck: true)
                                    )
                                  )
                               )

                        .Aggregations(a => a
                        //Modified specifications list
                        .Filter("SpecificationAttributes_modified", f => f
                            .Filter(ff => ff
                                .Bool(b => b
                                    .Must(GenerateSpecificationTermsMustClauses(groupedOptionsList, isCategoryAggs: false, isSubCategoryAggs: false, isSpecAttrAggs: false, isModifiedSpecAttrAggs: true)))
                                )
                            .Aggregations(a => a
                                .Nested("Spec_last_one", n => n
                                    .Path(p => p.Specifications)
                                    .Aggregations(aaa => aaa
                                        .Terms("unique_specification_ids_modified", t => t
                                            .Field("specifications.specificationAttribute.id")
                                            .Size(100)
                                            .Aggregations(aaaa => aaaa
                                                .TopHits("top_specification_hits_modified", th => th
                                                    .Size(1)
                                                    .Source(s => s
                                                        .Includes(i => i
                                                            .Fields("specifications.specificationAttribute")
                                                        )
                                                    )
                                                )
                                                .Nested("unique_specification_option_ids_modified", nn => nn
                                                    .Path("specifications.specificationAttributeOptionDetails")
                                                    .Aggregations(aaaaa => aaaaa
                                                        .Terms("unique_option_ids_modified", tt => tt
                                                            .Field("specifications.specificationAttributeOptionDetails.specificationAttributeOption.id")
                                                            .Size(100)
                                                            .Aggregations(aaaaaa => aaaaaa
                                                                .TopHits("top_option_hits_modified", thh => thh
                                                                    .Size(1)
                                                                    .Source(ss => ss
                                                                        .Includes(ii => ii
                                                                            .Fields("specifications.specificationAttributeOptionDetails.specificationAttributeOption")
                                                                        )
                                                                    )
                                                                )
                                                            )
                                                        )
                                                    )
                                                )
                                            )
                                        )
                                    )
                                )
                            )
                            )
                        //specifications list
                        .Filter("SpecificationAttributes", f => f
                            .Filter(ff => ff
                                .Bool(b => b
                                    .Must(GenerateSpecificationTermsMustClauses(groupedOptionsList, isCategoryAggs: false, isSubCategoryAggs: false
                                                        ))))
                        .Aggregations(a => a
                                .Nested("Spec_last_one", n => n
                                    .Path(p => p.Specifications)
                                    .Aggregations(aaa => aaa
                                        .Terms("unique_specification_ids", t => t
                                            .Field("specifications.specificationAttribute.id")
                                            .Size(100)
                                            .Aggregations(aaaa => aaaa
                                                .TopHits("top_specification_hits", th => th
                                                    .Size(1)
                                                    .Source(s => s
                                                        .Includes(i => i
                                                            .Fields("specifications.specificationAttribute")
                                                        )
                                                    )
                                                )
                                                .Nested("unique_specification_option_ids", nn => nn
                                                    .Path("specifications.specificationAttributeOptionDetails")
                                                    .Aggregations(aaaaa => aaaaa
                                                        .Terms("unique_option_ids", tt => tt
                                                            .Field("specifications.specificationAttributeOptionDetails.specificationAttributeOption.id")
                                                            .Size(100)
                                                            .Aggregations(aaaaaa => aaaaaa
                                                                .TopHits("top_option_hits", thh => thh
                                                                    .Size(1)
                                                                    .Source(ss => ss
                                                                        .Includes(ii => ii
                                                                            .Fields("specifications.specificationAttributeOptionDetails.specificationAttributeOption")
                                                                        )
                                                                    )
                                                                )
                                                            )
                                                        )
                                                    )
                                                )
                                            )
                                        )
                                    )
                                )
                            )
                            )
                                //sub category list
                                .Nested("unique_categories_parent_greater_0", n => n
                                    .Path("categories")
                                    .Aggregations(aa => aa
                                        .Filter("filter_parent_greater_0", f => f
                                            .Filter(ff => ff
                                                .Bool(b => b
                                                    .Must(GenerateSpecificationTermsMustClauses(groupedOptionsList, isCategoryAggs: false, isManufacturersAggs: false, isVendorsAggs: false, isSpecAttrAggs: false)
                                                        )))

                                            .Aggregations(aaa => aaa
                                                .Terms("unique_category_ids_subcat", t => t
                                                    .Field("categories.category.id")
                                                    .Size(100)
                                                    .Aggregations(aaaa => aaaa
                                                        .TopHits("top_category_hits_subcat", th => th
                                                            .Source(src => src
                                                                .Includes(i => i
                                                                .Field("categories.category")
                                                                .Field("categories.seName")
                                                                )
                                                            )
                                                            .Size(1)
                                                        ))))
                                        )
                                    )
                                )
                                //Category List
                                .Nested("unique_categories_parent_0", n => n
                                    .Path("categories")
                                    .Aggregations(aa => aa
                                        .Filter("filter_parent_0", f => f
                                            .Filter(ff => ff
                                                .Bool(b => b
                                                    .Must(GenerateSpecificationTermsMustClauses(groupedOptionsList, isSubCategoryAggs: false, isManufacturersAggs: false, isSpecAttrAggs: false)
                                                        )))

                                            .Aggregations(aaa => aaa
                                                .Terms("unique_category_ids", t => t
                                                    .Field("categories.category.id")
                                                    .Size(100)
                                                    .Aggregations(aaaa => aaaa
                                                        .TopHits("top_category_hits", th => th
                                                            .Source(src => src
                                                                .Includes(i => i
                                                                .Field("categories.category")
                                                                .Field("categories.seName")
                                                                )
                                                            )
                                                            .Size(1)
                                                        ))))
                                        )
                                    )
                                )
                                //Manufacturer list
                                .Nested("unique_manufacturers", n => n
                                    .Path("manufacturers")
                                    .Aggregations(aa => aa
                                        .Filter("filter_parent_0", f => f
                                            .Filter(ff => ff
                                                .Bool(b => b
                                                    .Must(GenerateSpecificationTermsMustClauses(groupedOptionsList, isCategoryAggs: false, isSubCategoryAggs: false, isManufacturersAggs: false)
                                                        )))
                                            .Aggregations(aa => aa
                                                .Terms("unique_manufacturer_ids", t => t
                                                    .Field("manufacturers.manufacturer.id")
                                                    .Size(100)
                                                    .Aggregations(aaa => aaa
                                                        .TopHits("top_manufacturer_hits", th => th
                                                            .Source(src => src
                                                                .Includes(i => i
                                                                .Field("manufacturers.manufacturer")
                                                                )
                                                            )
                                                            .Size(1)
                                                        ))))
                                )))
                                //vendors list
                                .Nested("unique_vendors", n => n
                                    .Path("vendors")
                                    .Aggregations(aa => aa
                                            .Filter("filter_parent_0", f => f
                                                .Aggregations(aa => aa
                                                    .Terms("vendor_ids", t => t
                                                    .Field("vendors.vendor.id")
                                                    .Size(100)
                                                        .Aggregations(aaa => aaa
                                                            .TopHits("top_vendor_hits", th => th
                                                            .Source(src => src
                                                                .Includes(i => i
                                                                .Field("vendors.vendor")
                                                                    )
                                                                )
                                                            .Size(1)
                                                    ))))

                                                .Filter(ff => ff
                                                    .Bool(b => b
                                                        .Must(GenerateSpecificationTermsMustClauses(groupedOptionsList, isCategoryAggs: false, isSubCategoryAggs: false, isVendorsAggs: false, isVendorCheck: true)
                                                    )))
                                      )))
                                );
            }
            else
            {
                searchDescriptor.Query(q => q
                    .MatchNone()
                );
            }

            #region Filters

            Func<QueryContainerDescriptor<Master_products>, QueryContainer> GenerateShouldClause(int productId, double boostValue)
            {
                return s => s.Term(t => t.Field("product.id").Value(productId).Boost(boostValue));
            }

            List<Func<QueryContainerDescriptor<Master_products>, QueryContainer>> GenerateSpecificationTermsMustClauses(List<List<int>> groupedOptions,
                bool isCategoryAggs = true,
                bool isSubCategoryAggs = true,
                bool isManufacturersAggs = true,
                bool isVendorsAggs = true,
                bool isSpecAttrAggs = true,
                bool isModifiedSpecAttrAggs = false,
                bool isVendorCheck = false,
                bool isProductVendorCheck = false)
            {
                var boolClauses = new List<Func<QueryContainerDescriptor<Master_products>, QueryContainer>>();

                if (groupedOptions.Count > 1 && isModifiedSpecAttrAggs)
                {
                    // Get the last option id from the list
                    int lastOption = filteredSpecOptions.LastOrDefault();

                    //Get the specification attribute id from last option
                    int specificationAttributeId = _specificationAttributeService.GetSpecificationAttributeIdBySpecificationOptionIdAsync(lastOption).Result;
                    //string key = specificationAttributeId.ToString();

                    IList<int> specAttrKeys = new List<int>();

                    foreach (var item in filteredSpecOptions)
                    {
                        var specAttrId = _specificationAttributeService.GetSpecificationAttributeIdBySpecificationOptionIdAsync(item).Result;
                        if (!specAttrId.Equals(specificationAttributeId))
                            specAttrKeys.Add(item);
                    }

                    int[] myArray = specAttrKeys.ToArray();

                    var modifiedspecAttributeOptions = _specificationAttributeService.GetSpecificationAttributeOptionsByIdsAsync(myArray).GetAwaiter().GetResult();

                    var modifiedgroupedOptionsList = modifiedspecAttributeOptions
                    .GroupBy(option => option.SpecificationAttributeId)
                    .Select(group => group.Select(option => option.Id).ToList())
                    .ToList();

                    foreach (var optionList in modifiedgroupedOptionsList)
                    {
                        boolClauses.Add(q => q
                            .Nested(n => n
                                .Path("specifications.specificationAttributeOptionDetails")
                                .Query(nq => nq
                                    .Bool(b => b
                                        .Must(mu => mu
                                            .Terms(t => t
                                                .Field("specifications.specificationAttributeOptionDetails.specificationAttributeOption.id")
                                                .Terms(optionList)
                                                )
                                            )
                                        )
                                    )
                            )
                        );
                    }
                }

                //Categories check
                if (isCategoryAggs)
                {
                    boolClauses.Add(m => m.Term("categories.category.parentCategoryId", 0));
                    boolClauses.Add(m => m.Term("categories.category.id", parentCategoryId));
                }

                if (isSubCategoryAggs)
                {
                    boolClauses.Add(m => m.Term("categories.category.parentCategoryId", parentCategoryId));
                    boolClauses.Add(m => m.Range(r => r.Field("categories.category.parentCategoryId").GreaterThan(0)));
                }

                if (!isCategoryAggs && !isSubCategoryAggs)
                {
                    boolClauses.Add(m => m.Nested(mn => mn.Path("categories")
                          .Query(q => q.Terms(tem => tem
                                  .Field("categories.category.id")
                                  .Terms(categoryIds)
                              ))));
                }

                //Manufactureres check
                if (isManufacturersAggs)
                {
                    boolClauses.Add(m => m.Nested(mn => mn.Path("manufacturers")
                        .Query(q => q.Terms(tem => tem
                            .Field("manufacturers.manufacturer.id")
                            .Terms(manufacturerIds)
                        ))));
                }

                //Vendors check on vendors filters
                if (isVendorCheck)
                {
                    boolClauses.Add(th => th
                   .Terms(tem => tem
                   .Field("vendors.vendor.id")
                               .Terms(geoVendorIds)));
                    boolClauses.Add(ft => ft.Range(range => range
                        .Field("vendors.stockQuantity")
                        .GreaterThan(0)
                    ));

                    boolClauses.Add(ft => ft.Range(range => range
                        .Field("vendors.price")
                        .GreaterThan(0)
                    ));
                }

                //Vendors check on other filters
                if (isVendorsAggs)
                {
                    boolClauses.Add(m => m.Nested(mn => mn.Path("vendors")
                    .Query(qr => qr
                    .Bool(b => b
                    .Must(ft => ft
                    .Bool(b => b
                    .Filter(nbf => nbf
                         .Terms(t => t
                         .Field("vendors.vendor.id")
                         .Terms(geoVendorIds)
                    ),
                    ft => ft.Range(range => range
                        .Field("vendors.stockQuantity")
                        .GreaterThan(0)
                    ),
                    ft => ft.Range(range => range
                        .Field("vendors.price")
                        .GreaterThan(0)
                    ),
                    ft => priceMax > 0 ? ft.Range(range => range
                                .Field("vendors.price")
                                .GreaterThanOrEquals(Convert.ToDouble(priceMin))
                                .LessThanOrEquals(Convert.ToDouble(priceMax))
                            ) : null
                        )))))));
                }

                //Vendors check on products
                if (isProductVendorCheck)
                {
                    boolClauses.Add(m => m.Nested(mn => mn.Path("vendors")
                        .Query(qr => qr
                            .Bool(b => b
                                .Must(ft => ft
                                    .Bool(b => b
                                        .Filter(nbf => (selectedVendorIds != null && selectedVendorIds.Count > 0) ? nbf
                                            .Terms(t => t
                                                .Field("vendors.vendor.id")
                                                .Terms(selectedVendorIds)
                                            ) : nbf
                                            .Terms(t => t
                                                .Field("vendors.vendor.id")
                                                .Terms(geoVendorIds)
                                            ),
                                            ft => ft.Range(range => range
                                                .Field("vendors.stockQuantity")
                                                .GreaterThan(0)
                                            ),
                                            ft => ft.Range(range => range
                                                .Field("vendors.price")
                                                .GreaterThan(0)
                                            ),
                                            ft => priceMax > 0 ? ft.Range(range => range
                                                .Field("vendors.price")
                                                .GreaterThanOrEquals(Convert.ToDouble(priceMin))
                                                .LessThanOrEquals(Convert.ToDouble(priceMax))
                                            ) : null
                                        )
                                    )
                                )
                            )
                        )
                    ));
                }

                if (isSpecAttrAggs && !isModifiedSpecAttrAggs)
                {

                    foreach (var optionList in groupedOptions)
                    {
                        boolClauses.Add(q => q
                            .Nested(n => n
                                .Path("specifications.specificationAttributeOptionDetails")
                                .Query(nq => nq
                                 .Bool(b => b
                        .Must(mu => mu
                                    .Terms(t => t
                                        .Field("specifications.specificationAttributeOptionDetails.specificationAttributeOption.id")
                                        .Terms(optionList)
                                    )
                                    ))
                                )
                            )
                        );
                    }
                }
                return boolClauses;
            }
            #endregion Filters

            #region Sorting
            if (orderBy == ProductSortingEnum.NameAsc)
                searchDescriptor.Sort(sort => sort.Ascending(x => x.Product.Name.Suffix("raw")));
            else if (orderBy == ProductSortingEnum.NameDesc)
                searchDescriptor.Sort(sort => sort.Descending(x => x.Product.Name.Suffix("raw")));
            else if (orderBy == ProductSortingEnum.PriceAsc)
            {
                searchDescriptor.Sort(sort => sort
                .Field(f => f
                .Field(ff => ff.Vendors.First().Price)
                .Order(SortOrder.Ascending)
                .Nested(n => n
                .Path(p => p.Vendors)
                .Filter(xa => xa
                    .Bool(xd =>
                    {
                        var mustClauses = new List<Func<QueryContainerDescriptor<Master_products>, QueryContainer>>();
                        mustClauses.Add(sa => sa.Range(add => add.Field("vendors.stockQuantity").GreaterThan(0)));
                        mustClauses.Add(sa => sa.Range(add => add.Field("vendors.price").GreaterThan(0)));
                        mustClauses.Add(sa => sa.Terms(add => add.Field("vendors.vendor.id").Terms(geoVendorIds)));

                        if (priceMax > 0)
                        {
                           mustClauses.Add(sa => sa.Range(add => add.Field("vendors.price").GreaterThanOrEquals(Convert.ToDouble(priceMin)).LessThanOrEquals(Convert.ToDouble(priceMax))));
                        }
                        return xd.Must(mustClauses);
                    })
                   )
                  )
                 )
                );
            }
            else if (orderBy == ProductSortingEnum.PriceDesc)
            {
                searchDescriptor.Sort(sort => sort
                    .Script(sc => sc
                        .Type("number")
                        .Script(ss => ss
                            .Source("double minPrice = Double.MAX_VALUE; for (vendor in params._source['vendors']) { if (params.geovendorids.contains(vendor['vendor']['id']) && vendor['price'] > 0 && vendor['stockQuantity'] > 0 && vendor['price'] < minPrice) { minPrice = vendor['price']; } } return minPrice;")
                            .Lang("painless")
                            .Params(p => p.Add("geovendorids", geoVendorIds))
                        )
                        .Order(SortOrder.Descending)
                    )
                );
            }
            else
            {
                //No sorting
            }
            #endregion Sorting

            var searchResponse = await elasticClient.SearchAsync<Master_products>(searchDescriptor);
            var json =  elasticClient.RequestResponseSerializer.SerializeToString(searchDescriptor);

            var model = new Master_products_result();

            #region Vendors List
            List<VendorDetails> distinctVendorNames = new List<VendorDetails>();
            HashSet<string> vendorNames = new HashSet<string>();

            var uniqueVendorsAgg = searchResponse.Aggregations?.Nested("unique_vendors");
            if (uniqueVendorsAgg != null)
            {
                var filterAgg = uniqueVendorsAgg.Filter("filter_parent_0");
                var uniqueVendorIdsAgg = filterAgg.Terms("vendor_ids");

                Dictionary<string, VendorDetails> vendorsDictionary = new Dictionary<string, VendorDetails>();

                foreach (var bucket in uniqueVendorIdsAgg.Buckets)
                {
                    var topHits = bucket.TopHits("top_vendor_hits");
                    var vendorDetails = topHits.Documents<VendorDetails>().FirstOrDefault();

                    if (vendorDetails != null)
                    {
                        var vendorName = vendorDetails.Vendor?.Name;

                        if (!string.IsNullOrEmpty(vendorName))
                        {
                            if (vendorsDictionary.TryGetValue(vendorName, out var existingVendorDetails))
                            {
                                existingVendorDetails.TotalDocuments += bucket.DocCount ?? 0;
                            }
                            else
                            {
                                vendorDetails.TotalDocuments = bucket.DocCount ?? 0;
                                vendorsDictionary.Add(vendorName, vendorDetails);
                            }
                        }
                    }
                }

                model.Vendors = vendorsDictionary.Values.ToList();
            }
            #endregion Vendors list

            #region SpecificationAttributeOptions List

            if (filteredSpecOptions != null && filteredSpecOptions.Count > 0)
            {
                // Initialize the final specification list
                List<SpecificationAttributeDetails> finalSpecifications = new List<SpecificationAttributeDetails>();

                // Get the last option id from the list
                int lastOption = filteredSpecOptions.LastOrDefault();

                //Get the specification attribute id from last option
                int specificationAttributeId = await _specificationAttributeService.GetSpecificationAttributeIdBySpecificationOptionIdAsync(lastOption);
                string key = specificationAttributeId.ToString();

                // Get the aggregations for "SpecificationAttributes_modified"
                var modifieduniqueSpecificationsAgg = searchResponse.Aggregations?.Filter("SpecificationAttributes_modified");
                if (modifieduniqueSpecificationsAgg != null)
                {
                    var specLastOneAgg = modifieduniqueSpecificationsAgg.Nested("Spec_last_one");
                    var uniqueSpecificationIdsAgg = specLastOneAgg.Terms("unique_specification_ids_modified");

                    Dictionary<string, SpecificationAttributeDetails> specificationsDictionary = new Dictionary<string, SpecificationAttributeDetails>();

                    foreach (var specBucket in uniqueSpecificationIdsAgg.Buckets)
                    {
                        if (specBucket.Key == key)
                        {
                            var topSpecHits = specBucket.TopHits("top_specification_hits_modified");
                            var specificationDetails = topSpecHits.Documents<SpecificationAttributeDetails>().FirstOrDefault();

                            if (specificationDetails != null)
                            {
                                var specificationName = specificationDetails.SpecificationAttribute?.Name;

                                if (!string.IsNullOrEmpty(specificationName) && !specificationsDictionary.ContainsKey(specificationName))
                                {
                                    specificationDetails.SpecificationAttributeOptionDetails = new List<SpecificationAttributeOptionDetails>();
                                    specificationsDictionary.Add(specificationName, specificationDetails);
                                }

                                var uniqueOptionIdsAgg = specBucket.Nested("unique_specification_option_ids_modified")?.Terms("unique_option_ids_modified");
                                if (uniqueOptionIdsAgg != null)
                                {
                                    foreach (var optionBucket in uniqueOptionIdsAgg.Buckets)
                                    {
                                        var topOptionHits = optionBucket.TopHits("top_option_hits_modified");
                                        var optionDetails = topOptionHits.Documents<SpecificationAttributeOptionDetails>().FirstOrDefault();

                                        if (optionDetails != null)
                                        {
                                            optionDetails.TotalDocuments = optionBucket.DocCount ?? 0;

                                            if (!string.IsNullOrEmpty(specificationName) && specificationsDictionary.TryGetValue(specificationName, out var specAttributeDetails))
                                            {
                                                specAttributeDetails.SpecificationAttributeOptionDetails.Add(optionDetails);
                                            }
                                        }
                                    }
                                }
                                finalSpecifications.Add(specificationDetails);
                            }
                        }
                    }
                }

                // Get the aggregations for "SpecificationAttributes"
                var uniqueSpecificationsAgg = searchResponse.Aggregations?.Filter("SpecificationAttributes");
                if (uniqueSpecificationsAgg != null)
                {
                    var specLastOneAgg = uniqueSpecificationsAgg.Nested("Spec_last_one");
                    var uniqueSpecificationIdsAgg = specLastOneAgg.Terms("unique_specification_ids");
                    Dictionary<string, SpecificationAttributeDetails> specificationsDictionary = new Dictionary<string, SpecificationAttributeDetails>();
                    foreach (var specBucket in uniqueSpecificationIdsAgg.Buckets)
                    {
                        if (specBucket.Key != key)
                        {
                            var topSpecHits = specBucket.TopHits("top_specification_hits");
                            var specificationDetails = topSpecHits.Documents<SpecificationAttributeDetails>().FirstOrDefault();


                            if (specificationDetails != null)
                            {
                                var specificationName = specificationDetails.SpecificationAttribute?.Name;

                                if (!string.IsNullOrEmpty(specificationName) && !specificationsDictionary.ContainsKey(specificationName))
                                {
                                    specificationDetails.SpecificationAttributeOptionDetails = new List<SpecificationAttributeOptionDetails>();
                                    specificationsDictionary.Add(specificationName, specificationDetails);
                                }

                                var uniqueOptionIdsAgg = specBucket.Nested("unique_specification_option_ids")?.Terms("unique_option_ids");
                                if (uniqueOptionIdsAgg != null)
                                {
                                    foreach (var optionBucket in uniqueOptionIdsAgg.Buckets)
                                    {
                                        var topOptionHits = optionBucket.TopHits("top_option_hits");
                                        var optionDetails = topOptionHits.Documents<SpecificationAttributeOptionDetails>().FirstOrDefault();

                                        if (optionDetails != null)
                                        {
                                            optionDetails.TotalDocuments = optionBucket.DocCount ?? 0;

                                            if (!string.IsNullOrEmpty(specificationName) && specificationsDictionary.TryGetValue(specificationName, out var specAttributeDetails))
                                            {
                                                specAttributeDetails.SpecificationAttributeOptionDetails.Add(optionDetails);
                                            }
                                        }
                                    }
                                }
                                finalSpecifications.Add(specificationDetails);
                            }
                        }
                    }
                }

                // Assign the final specification list to the model
                model.Specifications = finalSpecifications;

            }
            else
            {
                var uniqueSpecificationsAgg = searchResponse.Aggregations?.Filter("SpecificationAttributes");
                if (uniqueSpecificationsAgg != null)
                {
                    var specLastOneAgg = uniqueSpecificationsAgg.Nested("Spec_last_one");
                    var uniqueSpecificationIdsAgg = specLastOneAgg.Terms("unique_specification_ids");

                    Dictionary<string, SpecificationAttributeDetails> specificationsDictionary = new Dictionary<string, SpecificationAttributeDetails>();

                    foreach (var specBucket in uniqueSpecificationIdsAgg.Buckets)
                    {
                        var topSpecHits = specBucket.TopHits("top_specification_hits");
                        var specificationDetails = topSpecHits.Documents<SpecificationAttributeDetails>().FirstOrDefault();

                        if (specificationDetails != null)
                        {
                            var specificationName = specificationDetails.SpecificationAttribute?.Name;

                            if (!string.IsNullOrEmpty(specificationName) && !specificationsDictionary.ContainsKey(specificationName))
                            {
                                specificationDetails.SpecificationAttributeOptionDetails = new List<SpecificationAttributeOptionDetails>();
                                specificationsDictionary.Add(specificationName, specificationDetails);
                            }

                            var uniqueOptionIdsAgg = specBucket.Nested("unique_specification_option_ids")?.Terms("unique_option_ids");
                            if (uniqueOptionIdsAgg != null)
                            {
                                foreach (var optionBucket in uniqueOptionIdsAgg.Buckets)
                                {
                                    var topOptionHits = optionBucket.TopHits("top_option_hits");
                                    var optionDetails = topOptionHits.Documents<SpecificationAttributeOptionDetails>().FirstOrDefault();

                                    if (optionDetails != null)
                                    {
                                        optionDetails.TotalDocuments = optionBucket.DocCount ?? 0;

                                        if (!string.IsNullOrEmpty(specificationName) && specificationsDictionary.TryGetValue(specificationName, out var specAttributeDetails))
                                        {
                                            specAttributeDetails.SpecificationAttributeOptionDetails.Add(optionDetails);
                                        }
                                    }
                                }
                            }
                        }
                    }

                    model.Specifications = specificationsDictionary.Values.ToList();
                }
            }

            #endregion SpecificationAttributeOptions List

            #region Manufacturers List
            var uniqueManufacuterAgg = searchResponse.Aggregations?.Nested("unique_manufacturers");
            if (uniqueManufacuterAgg != null)
            {
                var filterAgg = uniqueManufacuterAgg.Filter("filter_parent_0");
                var uniqueManufacuterIdsAgg = filterAgg.Terms("unique_manufacturer_ids");

                Dictionary<string, ManufacturerDetails> manufacturersDictionary = new Dictionary<string, ManufacturerDetails>();

                foreach (var bucket in uniqueManufacuterIdsAgg.Buckets)
                {
                    var topHits = bucket.TopHits("top_manufacturer_hits");
                    var manufacturerDetails = topHits.Documents<ManufacturerDetails>().FirstOrDefault();

                    if (manufacturerDetails != null)
                    {
                        var manufacturerName = manufacturerDetails.Manufacturer?.Name;

                        if (!string.IsNullOrEmpty(manufacturerName))
                        {
                            if (manufacturersDictionary.TryGetValue(manufacturerName, out var existingManufacturerDetails))
                            {
                                existingManufacturerDetails.TotalDocuments += bucket.DocCount ?? 0;
                            }
                            else
                            {
                                manufacturerDetails.TotalDocuments = bucket.DocCount ?? 0;
                                manufacturersDictionary.Add(manufacturerName, manufacturerDetails);
                            }
                        }
                    }
                }

                model.Manufacturers = manufacturersDictionary.Values.ToList();
            }

            #endregion Manufacturers List

            #region Sub Category List
            if (categoryIds != null && categoryIds.Any())
            {
                var uniqueCategoriesParentGreaterAgg = searchResponse.Aggregations?.Nested("unique_categories_parent_greater_0");
                if (uniqueCategoriesParentGreaterAgg != null)
                {
                    var filterAgg = uniqueCategoriesParentGreaterAgg.Filter("filter_parent_greater_0");
                    var uniqueCategoryIdsAgg = filterAgg?.Terms("unique_category_ids_subcat");

                    if (uniqueCategoryIdsAgg != null)
                    {
                        Dictionary<string, CategoryDetails> categoriesDictionary = new Dictionary<string, CategoryDetails>();

                        foreach (var bucket in uniqueCategoryIdsAgg.Buckets)
                        {
                            var topHits = bucket.TopHits("top_category_hits_subcat");
                            var categoryDetails = topHits.Documents<CategoryDetails>().FirstOrDefault();

                            if (categoryDetails != null)
                            {
                                var categoryName = categoryDetails.Category?.Name;
                                var categorySeoName = categoryDetails.SeName;

                                if (!string.IsNullOrEmpty(categoryName))
                                {
                                    if (categoriesDictionary.TryGetValue(categoryName, out var existingCategory))
                                    {
                                        existingCategory.TotalDocuments += bucket.DocCount ?? 0;
                                        existingCategory.SeName = categorySeoName;
                                    }
                                    else
                                    {
                                        categoryDetails.TotalDocuments = bucket.DocCount ?? 0;
                                        categoryDetails.SeName = categorySeoName;
                                        categoriesDictionary.Add(categoryName, categoryDetails);
                                    }
                                }
                            }
                        }

                        model.SubCategoryList = categoriesDictionary.Values.ToList();
                    }
                }
            }

            #endregion Sub Category List

            #region Parent Category List 
            var uniqueCategoriesParent0Agg = searchResponse.Aggregations?.Nested("unique_categories_parent_0");
            if (uniqueCategoriesParent0Agg != null)
            {
                var filterAggnew = uniqueCategoriesParent0Agg.Filter("filter_parent_0");
                var uniqueCategoryIdsAggnew = filterAggnew?.Terms("unique_category_ids");

                if (uniqueCategoryIdsAggnew != null)
                {
                    Dictionary<string, CategoryDetails> categoriesDictionarynew = new Dictionary<string, CategoryDetails>();

                    foreach (var bucket in uniqueCategoryIdsAggnew.Buckets)
                    {
                        var topHits = bucket.TopHits("top_category_hits");
                        var categoryDetails = topHits.Documents<CategoryDetails>().FirstOrDefault();

                        if (categoryDetails != null)
                        {
                            var categoryName = categoryDetails.Category?.Name;
                            var categorySeoName = categoryDetails.SeName;

                            if (!string.IsNullOrEmpty(categoryName))
                            {
                                if (categoriesDictionarynew.TryGetValue(categoryName, out var existingCategory))
                                {
                                    existingCategory.TotalDocuments += bucket.DocCount ?? 0;
                                    existingCategory.SeName = categorySeoName;
                                }
                                else
                                {
                                    categoryDetails.TotalDocuments = bucket.DocCount ?? 0;
                                    categoryDetails.SeName = categorySeoName;
                                    categoriesDictionarynew.Add(categoryName, categoryDetails);
                                }
                            }
                        }
                    }

                    model.ParentCategoryList = categoriesDictionarynew.Values.ToList();
                }
            }

            #endregion Parent Category List

            model.Master_Products = searchResponse.Documents.ToList();
            model.TotalMasterProducts = Convert.ToInt32(searchResponse.Total);
            return model;

        }

        /// <summary>
        /// Returns list of first 10 products that matches the query
        /// </summary>
        /// <param name="keyword">Keyword to be searched</param>
        /// <returns></returns>
        public virtual async Task<List<Master_products>> GetSearchAutoCompleteProductsAsync(string keyword, int pageSize = int.MaxValue, bool visibleIndividuallyOnly = false,
           bool isMaster = true, int languageId = 0, IList<int> geoVendorIds = null, IList<(int, double)> associationProductIds = null)
        {
            if (associationProductIds == null)
                associationProductIds = new List<(int, double)>();

            var elasticClient = await GetElasticClient();
            if (elasticClient == null)
            {
                await _logger.ErrorAsync("Elasticsearch client is null.");
                return new List<Master_products>();
            }
            var elasticSearchSettings = _configuration.GetSection("elasticsearch");
            var indexName = string.Empty;

            if (elasticSearchSettings != null && !string.IsNullOrEmpty(elasticSearchSettings["index"]))
                indexName = elasticSearchSettings["index"];

            SearchDescriptor<Master_products> searchDescriptor;
            searchDescriptor = new SearchDescriptor<Master_products>()
                                   .Size(pageSize)
                                   .Index(indexName)
                                   .TrackTotalHits(true);

            var shouldClauses = associationProductIds.Select(id => GenerateShouldClause(id.Item1, id.Item2)).ToList();

            Func<QueryContainerDescriptor<Master_products>, QueryContainer> GenerateShouldClause(int productId, double boostValue)
            {
                return s => s.Term(t => t.Field("product.id").Value(productId).Boost(boostValue));
            }

            if (geoVendorIds != null && geoVendorIds.Count > 0)
            {
                searchDescriptor.Query(q => q
                .Bool(ba => ba
                .Should(sh => sh
                                       .Bool(ba => ba
                                           .Must(ms => ms.MultiMatch(mlt => mlt
                                        .Query(keyword)
                                        .Fuzziness(Fuzziness.Auto)
                                        .Boost(100)
                                        .Fields(f => f
                                                .Field(x => x.Product.Name, boost: 1000)
                                                .Field(y => y.Product.ShortDescription, boost: 200)
                                                .Field(z => z.Product.FullDescription, boost: 100)
                                                )
                                        ),
                                     ms => ms.MultiMatch(mlt => mlt
                                        .Query(keyword)
                                        .Fuzziness(Fuzziness.EditDistance(2))
                                        .PrefixLength(2) // Fuzziness won't apply to substrings with length less than 2
                                        .Fields(f => f
                                                .Field(x => x.Product.Name, boost: 1000)
                                                .Field(y => y.Product.ShortDescription, boost: 200)
                                                .Field(z => z.Product.FullDescription, boost: 100)
                                               )
                                       ),
                                                ms => ms
                                                .Term(st => st
                                                .Field(bt => bt.Product.VisibleIndividually).Value(true)
                                                ),
                                                m => m.Nested(mn => mn.Path("vendors")
                                                    .Query(qr => qr
                                                    .Bool(b => b
                                                    .Must(ft => ft
                                                    .Bool(b => b
                                                    .Filter(th => th
                                                    .Terms(tem => tem
                                                    .Field("vendors.vendor.id")
                                                                .Terms(geoVendorIds)),
                                                    ft => ft.Range(range => range
                                                        .Field("vendors.stockQuantity")
                                                        .GreaterThan(0)
                                                    ),
                                                    ft => ft.Range(range => range
                                                        .Field("vendors.price")
                                                        .GreaterThan(0)
                                                    )
                                                        ))))))
                                           )
                                   ),
                                    q => q.Bool(bq => bq
                                .Should(
                                     shouldClauses.ToArray()
                                )
                            )
                                       )
                )
                               );
            }
            else
            {
                searchDescriptor.Query(q => q
                        .MatchNone()
                    );
            }

            var json =  elasticClient.RequestResponseSerializer.SerializeToString(searchDescriptor);
            var searchResponseajax = await elasticClient.SearchAsync<Master_products>(searchDescriptor);
            
            var productsSource = searchResponseajax.Documents.ToList();

            foreach (var source in productsSource)
            {
                //associated?
                if (source?.Product?.ParentGroupedProductId > 0)
                {
                    //replace group products on associated product in source.
                    var groupProduct = (await ReplaceGroupProductsOnAssociatedProductsInSoruce(new List<Product> { source.Product }))?.FirstOrDefault();

                    //replace if not null
                    source.Product = groupProduct ?? source.Product;
                }
            }

            return productsSource;
        }

        /// <summary>
        /// Returns list of master products and total no of master products
        /// </summary>
        /// <param name="keywords"></param>
        /// <param name="pageIndex"></param>
        /// <param name="categoryIds"></param>
        /// <param name="manufacturerIds"></param>
        /// <param name="geoVendorIds"></param>
        /// <param name="filteredSpecOptions"></param>
        /// <param name="priceMin"></param>
        /// <param name="priceMax"></param>
        /// <param name="orderBy"></param>
        /// <returns></returns>
        public virtual async Task<(IPagedList<Product> pagedProducts, Master_products_result elasticResponse)> SearchProducts(
           string keywords = null,
           int pageNumber = 0,
           int pageSize = int.MaxValue,
           int storeId = 0,
           bool visibleIndividuallyOnly = false,
           bool? isMaster = true,
           IList<int> categoryIds = null,
           IList<int> manufacturerIds = null,
           IList<int> geoVendorIds = null,
           IList<int> selectedVendorIds = null,
           IList<int> filteredSpecOptions = null,
           decimal? priceMin = null,
           decimal? priceMax = null,
           ProductSortingEnum orderBy = ProductSortingEnum.Position,
           int parentCategoryId = 0,
           IList<(int, double)> associateProductIds = null)
        {
            //get elastic response 
            var elasticResult = await GetMasterProductsListViaElasticSearchAsync(keywords,
                pageNumber,
                pageSize,
                storeId,
                visibleIndividuallyOnly,
                isMaster,
                categoryIds,
                manufacturerIds,
                geoVendorIds,
                selectedVendorIds,
                filteredSpecOptions,
                priceMin,
                priceMax,
                orderBy,
                parentCategoryId,
                associateProductIds);

            //take maste products to prepare paged list type.
            var products = elasticResult?.Master_Products?.Select(r => r.Product)?.ToList();

            //replace group products on associated product in source.
            products = (await ReplaceGroupProductsOnAssociatedProductsInSoruce(products)).ToList();

            //paged list
            var pagedList = new PagedList<Product>(products, pageNumber - 1, pageSize, elasticResult.TotalMasterProducts);

            return (pagedList, elasticResult);
        }

        /// <summary>
        /// Search product using elastic
        /// </summary>
        /// <param name="keyword"></param>
        /// <param name="pageSize"></param>
        /// <param name="visibleIndividuallyOnly"></param>
        /// <param name="isMaster"></param>
        /// <param name="languageId"></param>
        /// <param name="geoVendorIds"></param>
        /// <returns></returns>
        public virtual async Task<IPagedList<Product>> SearchAutoCompleteProductsAsync(
           string keyword,
           int pageSize = int.MaxValue,
           bool visibleIndividuallyOnly = false,
           bool isMaster = true,
           int languageId = 0,
           IList<int> geoVendorIds = null,
           IList<(int, double)> associationProductIds = null)
        {
            var elasticResult = await GetSearchAutoCompleteProductsAsync(
                keyword,
                pageSize,
                visibleIndividuallyOnly,
                isMaster,
                languageId,
                geoVendorIds,
                associationProductIds);

            //take maste products to prepare paged list type.
            var products = elasticResult?.Select(r => r.Product)?.ToList();

            //replace group products on associated product in source.
            products = (await ReplaceGroupProductsOnAssociatedProductsInSoruce(products)).ToList();

            //paged list
            var pagedList = new PagedList<Product>(products, 0, pageSize);

            return pagedList;
        }

        #endregion Elastic Search Method

        public virtual async Task<List<(int, double)>> GetAssociateProductsListViaElasticSearchAsync(
        string keywords = null,
        int storeId = 0,
        bool visibleIndividuallyOnly = false,
        bool? isMaster = true,
        IList<int> categoryIds = null,
        IList<int> manufacturerIds = null,
        IList<int> geoVendorIds = null,
        IList<int> selectedVendorIds = null,
        IList<int> filteredSpecOptions = null,
        decimal? priceMin = null,
        decimal? priceMax = null,
        int parentCategoryId = 0)
        {
            var indexName = _configuration.GetSection("elasticsearch")["index"];
            var searchTerms = keywords == null
                ? string.Empty
                : keywords.Trim();

            //Get the lists of specifications options ids w.r.t. specification attributes
            List<List<int>> groupedOptionsList = new List<List<int>> { new List<int>() };
            if (filteredSpecOptions != null)
            {
                int[] myArray = filteredSpecOptions.ToArray();

                var specAttributeOptions = _specificationAttributeService.GetSpecificationAttributeOptionsByIdsAsync(myArray).GetAwaiter().GetResult();

                groupedOptionsList = GroupSpecificationOptions(specAttributeOptions);

                static List<List<int>> GroupSpecificationOptions(IList<SpecificationAttributeOption> options)
                {
                    var groupedOptions = options
                        .GroupBy(option => option.SpecificationAttributeId)
                        .Select(group => group.Select(option => option.Id).ToList())
                        .ToList();

                    return groupedOptions;
                }
            }

            SearchDescriptor<Master_products> searchDescriptor;

            searchDescriptor = new SearchDescriptor<Master_products>()
                               .Index(indexName);
            if (geoVendorIds != null && geoVendorIds.Count > 0)
            {
                var specificationTermsMustClauses = GenerateSpecificationTermsMustClauses(groupedOptionsList);


                searchDescriptor.Query(q => q
                                .Bool(ba => ba
                                    .Must(ms => ms.MultiMatch(mlt => mlt
                                            .Query(searchTerms)
                                            .Fuzziness(Fuzziness.Auto)
                                            .Boost(100)
                                            .Fields(f => f
                                                    .Field(x => x.Product.Name, boost: 1000)
                                                    .Field(y => y.Product.ShortDescription, boost: 200)
                                                    .Field(z => z.Product.FullDescription, boost: 100)
                                                    )
                                            ),
                                         ms => ms.MultiMatch(mlt => mlt
                                            .Query(searchTerms)
                                            .Fuzziness(Fuzziness.EditDistance(2))
                                            .PrefixLength(2) // Fuzziness won't apply to substrings with length less than 2
                                            .Fields(f => f
                                                    .Field(x => x.Product.Name, boost: 1000)
                                                    .Field(y => y.Product.ShortDescription, boost: 200)
                                                    .Field(z => z.Product.FullDescription, boost: 100)
                                                   )
                                           ),
                                         ms => ms.Term(st => st.Field(bt => bt.Product.VisibleIndividually).Value(false))
                                        )
                                    )
                              )
                            .PostFilter(pf => pf
                                .Bool(gf => gf
                                    .Filter(GenerateSpecificationTermsMustClauses(groupedOptionsList, isCategoryAggs: false, isSubCategoryAggs: false)
                                    )
                                  )
                               )
                        //Association product list
                        .Aggregations(a => a

                        //Association product list
                        .Filter("distinct_parentGroupedProductIdDetail", f => f
                            .Filter(ff => ff
                                .Bool(b => b
                                    .Must(GenerateSpecificationTermsMustClauses(groupedOptionsList, isCategoryAggs: false, isSubCategoryAggs: false, isAssociatedAggs: true)
                              )))
                            .Aggregations(aa => aa
                                .Terms("parentGroupedProductId_terms", ta => ta
                                    .Field("product.parentGroupedProductId")
                                    .Size(10000)
                                    .Order(o => o
                                        .Descending("max_score_product")
                                     )
                                    .Aggregations(aaa => aaa
                                        .TopHits("product_details", th => th
                                        .Size(1)
                                        .Sort(s => s
                                            .Descending("_score")
                                        )
                                            .Source(sf => sf
                                                .Includes(i => i
                                                    .Field("product.id")
                                                    .Field("_score")
                                                )
                                            )
                                        )
                                         .Max("max_score_product", m => m
                                            .Script("_score")
                                        )
                                    )
                                )
                            )
                        )
                        );
            }
            else
            {
                searchDescriptor.Query(q => q
                    .MatchNone()
                );
            }


            List<Func<QueryContainerDescriptor<Master_products>, QueryContainer>> GenerateSpecificationTermsMustClauses(List<List<int>> groupedOptions,
                bool isCategoryAggs = true,
                bool isSubCategoryAggs = true,
                bool isManufacturersAggs = true,
                bool isVendorsAggs = true,
                bool isSpecAttrAggs = true,
                bool isModifiedSpecAttrAggs = false,
                bool isAssociatedAggs = false)
            {
                var boolClauses = new List<Func<QueryContainerDescriptor<Master_products>, QueryContainer>>();


                if (groupedOptions.Count > 1 && isModifiedSpecAttrAggs)
                {
                    // Get the last option id from the list
                    int lastOption = filteredSpecOptions.LastOrDefault();

                    //Get the specification attribute id from last option
                    int specificationAttributeId = _specificationAttributeService.GetSpecificationAttributeIdBySpecificationOptionIdAsync(lastOption).Result;
                    //string key = specificationAttributeId.ToString();

                    IList<int> specAttrKeys = new List<int>();

                    foreach (var item in filteredSpecOptions)
                    {
                        var specAttrId = _specificationAttributeService.GetSpecificationAttributeIdBySpecificationOptionIdAsync(item).Result;
                        if (!specAttrId.Equals(specificationAttributeId))
                            specAttrKeys.Add(item);
                    }

                    int[] myArray = specAttrKeys.ToArray();

                    var modifiedspecAttributeOptions = _specificationAttributeService.GetSpecificationAttributeOptionsByIdsAsync(myArray).GetAwaiter().GetResult();

                    var modifiedgroupedOptionsList = modifiedspecAttributeOptions
                    .GroupBy(option => option.SpecificationAttributeId)
                    .Select(group => group.Select(option => option.Id).ToList())
                    .ToList();

                    foreach (var optionList in modifiedgroupedOptionsList)
                    {
                        boolClauses.Add(q => q
                            .Nested(n => n
                                .Path("specifications.specificationAttributeOptionDetails")
                                .Query(nq => nq
                                    .Bool(b => b
                                        .Must(mu => mu
                                            .Terms(t => t
                                                .Field("specifications.specificationAttributeOptionDetails.specificationAttributeOption.id")
                                                .Terms(optionList)
                                                )
                                            )
                                        )
                                    )
                            )
                        );
                    }
                }

                //Associated Products check
                if (isAssociatedAggs)
                {
                    boolClauses.Add(m => m.Range(r => r.Field("product.parentGroupedProductId").GreaterThan(0)));

                }

                //Categories check
                if (isCategoryAggs)
                {
                    boolClauses.Add(m => m.Term("categories.category.parentCategoryId", 0));
                }

                if (isSubCategoryAggs)
                {
                    boolClauses.Add(m => m.Term("categories.category.parentCategoryId", parentCategoryId));
                    boolClauses.Add(m => m.Range(r => r.Field("categories.category.parentCategoryId").GreaterThan(0)));
                }

                if (!isCategoryAggs && !isSubCategoryAggs)
                {
                    boolClauses.Add(m => m.Nested(mn => mn.Path("categories")
                          .Query(q => q.Terms(tem => tem
                                  .Field("categories.category.id")
                                  .Terms(categoryIds)
                              ))));
                }

                if (isManufacturersAggs)
                {
                    //Manufactureres check
                    boolClauses.Add(m => m.Nested(mn => mn.Path("manufacturers")
                        .Query(q => q.Terms(tem => tem
                            .Field("manufacturers.manufacturer.id")
                            .Terms(manufacturerIds)
                        ))));
                }

                if (isVendorsAggs)
                {
                    //vendors check
                    boolClauses.Add(m => m.Nested(mn => mn.Path("vendors")
                .Query(qr => qr
                .Bool(b => b
                .Must(ft => ft
                .Bool(b => b
                .Filter(th => th
                .Terms(tem => tem
                .Field("vendors.vendor.id")
                            .Terms(geoVendorIds)),
                ft => ft.Range(range => range
                    .Field("vendors.stockQuantity")
                    .GreaterThan(0)
                ),
                ft => ft.Range(range => range
                    .Field("vendors.price")
                    .GreaterThan(0)
                ),
                ft => priceMax > 0 ? ft.Range(range => range
                            .Field("vendors.price")
                            .GreaterThanOrEquals(Convert.ToDouble(priceMin))
                            .LessThanOrEquals(Convert.ToDouble(priceMax))
                        ) : null
                    )))))));


                }

                if (isSpecAttrAggs && !isModifiedSpecAttrAggs)
                {

                    foreach (var optionList in groupedOptions)
                    {
                        boolClauses.Add(q => q
                            .Nested(n => n
                                .Path("specifications.specificationAttributeOptionDetails")
                                .Query(nq => nq
                                 .Bool(b => b
                        .Must(mu => mu
                                    .Terms(t => t
                                        .Field("specifications.specificationAttributeOptionDetails.specificationAttributeOption.id")
                                        .Terms(optionList)
                                    )
                                    ))
                                )
                            )
                        );
                    }
                }
                return boolClauses;
            }

            //var searchResponse = await GetElasticClient().SearchAsync<Master_products>(searchDescriptor);
            //var json = GetElasticClient().RequestResponseSerializer.SerializeToString(searchDescriptor);

            //var productIds = new List<int>();
            ////Get association products ids list
            //var buckets = searchResponse.Aggregations.Filter("distinct_parentGroupedProductIdDetail").Terms("parentGroupedProductId_terms").Buckets;

            //foreach (var bucket in buckets)
            //{
            //    var topHits = bucket.TopHits("product_details");
            //    var associatedProductDetails = topHits.Documents<Master_products>().FirstOrDefault();
            //    var productId = associatedProductDetails.Product.Id;
            //    productIds.Add(productId);
            //}

            //return productIds;
            var elasticClient = await GetElasticClient();
            if (elasticClient == null)
            {
                await _logger.ErrorAsync("Elasticsearch client is null.");
                return new List<(int, double)>();
            }
            var searchResponse = await elasticClient.SearchAsync<Master_products>(searchDescriptor);
            var json = elasticClient.RequestResponseSerializer.SerializeToString(searchDescriptor);

            if (searchResponse == null)
            {
                //await _logger.ErrorAsync("Search response is null");
                return new List<(int, double)>();
            }

            if (searchResponse.Aggregations == null)
            {
                //await _logger.ErrorAsync("Aggregations in search response is null");
                return new List<(int, double)>();
            }

            var filterAggregation = searchResponse.Aggregations.Filter("distinct_parentGroupedProductIdDetail");

            if (filterAggregation == null)
            {
                //await _logger.ErrorAsync("Filter aggregation is null");
                return new List<(int, double)>();
            }

            var termsAggregation = filterAggregation.Terms("parentGroupedProductId_terms");

            if (termsAggregation == null)
            {
                //await _logger.ErrorAsync("Terms aggregation is null");
                return new List<(int, double)>();
            }

            if (termsAggregation?.Buckets == null || !termsAggregation.Buckets.Any())
            {
                //await _logger.ErrorAsync("No buckets found in terms aggregation");
                return new List<(int, double)>();
            }

            var productIdsWithScores = searchResponse.Aggregations
                .Filter("distinct_parentGroupedProductIdDetail")
                .Terms("parentGroupedProductId_terms")
                .Buckets
                .Select(bucket =>
                {
                    var topHits = bucket.TopHits("product_details");
                    if (topHits == null)
                    {
                        //_logger.ErrorAsync($"Top hits for bucket {bucket.Key} is null");
                        return (0, 0.0);
                    }

                    var documents = topHits.Documents<Master_products>();
                    if (documents == null || !documents.Any())
                    {
                        //_logger.ErrorAsync($"No documents found in top hits for bucket {bucket.Key}");
                        return (0, 0.0);
                    }

                    var associatedProductDetails = topHits.Documents<Master_products>().FirstOrDefault();
                    var productId = associatedProductDetails?.Product?.Id ?? 0;
                    var score = topHits.MaxScore ?? 0.0;

                    return (productId, score);
                })
                 .Where(p => p.Item1 != 0)
                .ToList();

            return productIdsWithScores;


        }

        public virtual async Task<List<(int, double)>> GetAssociateProductIdsListViaElasticSearchAsync(
        string keyword,
        int pageSize = int.MaxValue,
        bool visibleIndividuallyOnly = false,
        bool isMaster = true,
        int languageId = 0,
        IList<int> geoVendorIds = null)
        {
            var indexName = _configuration.GetSection("elasticsearch")["index"];
            var searchTerms = keyword == null ? string.Empty : keyword.Trim();

            SearchDescriptor<Master_products> searchDescriptor;

            searchDescriptor = new SearchDescriptor<Master_products>().Index(indexName);

            if (geoVendorIds != null && geoVendorIds.Count > 0)
            {

                searchDescriptor.Query(q => q
                                .Bool(ba => ba
                                    .Must(ms => ms.MultiMatch(mlt => mlt
                                            .Query(searchTerms)
                                            .Fuzziness(Fuzziness.Auto)
                                            .Boost(100)
                                            .Fields(f => f
                                                    .Field(x => x.Product.Name, boost: 1000)
                                                    .Field(y => y.Product.ShortDescription, boost: 200)
                                                    .Field(z => z.Product.FullDescription, boost: 100)
                                                    )
                                            ),
                                         ms => ms.MultiMatch(mlt => mlt
                                            .Query(searchTerms)
                                            .Fuzziness(Fuzziness.EditDistance(2))
                                            .PrefixLength(2) // Fuzziness won't apply to substrings with length less than 2
                                            .Fields(f => f
                                                    .Field(x => x.Product.Name, boost: 1000)
                                                    .Field(y => y.Product.ShortDescription, boost: 200)
                                                    .Field(z => z.Product.FullDescription, boost: 100)
                                                   )
                                           ),
                                         ms => ms.Term(st => st.Field(bt => bt.Product.VisibleIndividually).Value(false)),
                                         mu => mu
                                        .Nested(n => n
                                            .Path("vendors")
                                            .Query(nq => nq
                                                .Bool(nb => nb
                                                    .Must(nbm => nbm
                                                        .Bool(nbb => nbb
                                                            .Filter(nbf => nbf
                                                                .Range(r => r
                                                                    .Field("vendors.stockQuantity")
                                                                    .GreaterThan(0)
                                                                ),
                                                                nbf => nbf
                                                                .Range(r => r
                                                                    .Field("vendors.price")
                                                                    .GreaterThan(0)
                                                                ),
                                                                nbf => nbf
                                                                .Terms(t => t
                                                                    .Field("vendors.vendor.id")
                                                                    .Terms(geoVendorIds)
                                                                )
                                                            )
                                                        )
                                                    )
                                                )
                                            )
                                        )
                                        )
                                    )
                              )
                            //Association product list
                            .Aggregations(aa => aa
                                .Terms("parentGroupedProductId_terms", ta => ta
                                    .Field("product.parentGroupedProductId")
                                    .Size(10000)
                                    .Order(o => o
                                        .Descending("max_score_product")
                                     )
                                    .Aggregations(aaa => aaa
                                        .TopHits("product_details", th => th
                                        .Size(1)
                                        .Sort(s => s
                                            .Descending("_score")
                                        )
                                            .Source(sf => sf
                                                .Includes(i => i
                                                    .Field("product.id")
                                                    .Field("_score")
                                                )
                                            )
                                        )
                                         .Max("max_score_product", m => m
                                            .Script("_score")
                                        )
                            )
                        )
                        );
            }
            else
            {
                searchDescriptor.Query(q => q.MatchNone());
            }

            var elasticClient = await GetElasticClient();
            if (elasticClient == null)
            {
                await _logger.ErrorAsync("Elasticsearch client is null.");
                return new List<(int, double)>();
            }

            var searchResponse = await elasticClient.SearchAsync<Master_products>(searchDescriptor);
            var json = elasticClient.RequestResponseSerializer.SerializeToString(searchDescriptor);

            if (searchResponse?.Aggregations == null)
            {
                //await _logger.ErrorAsync("Search response or aggregations is null for autocomplete");
                return new List<(int, double)>();
            }

            var termsAggregation = searchResponse.Aggregations.Terms("parentGroupedProductId_terms");

            if (termsAggregation == null)
            {
                ///await _logger.ErrorAsync("Terms aggregation is null");
                return new List<(int, double)>();
            }

            if (termsAggregation?.Buckets == null || !termsAggregation.Buckets.Any())
            {
                //await _logger.ErrorAsync("No buckets found in terms aggregation for autocomplete");
                return new List<(int, double)>();
            }

            var productIdsWithScores = termsAggregation
                .Buckets
                .Select(bucket =>
                {
                    var topHits = bucket.TopHits("product_details");
                    if (topHits?.Documents<Master_products>() == null || !topHits.Documents<Master_products>().Any())
                    {
                       // _logger.ErrorAsync($"No documents found in top hits for bucket {bucket.Key} in autocomplete");
                        return (0, 0.0);
                    }

                    var associatedProductDetails = topHits.Documents<Master_products>().FirstOrDefault();
                    var productId = associatedProductDetails?.Product?.Id ?? 0;
                    var score = topHits.MaxScore ?? 0.0;

                    return (productId, score);
                })
                .Where(p => p.Item1 != 0)
                .ToList();

            return productIdsWithScores;
        }

    }

}
