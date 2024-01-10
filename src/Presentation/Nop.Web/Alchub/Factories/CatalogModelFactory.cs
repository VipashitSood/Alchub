using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Blogs;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Forums;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Vendors;
using Nop.Core.Events;
using Nop.Services.Alchub.ElasticSearch;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Seo;
using Nop.Services.Topics;
using Nop.Services.Vendors;
using Nop.Web.Alchub.Models.Catalog;
using Nop.Web.Framework.Events;
using Nop.Web.Infrastructure.Cache;
using Nop.Web.Models.Catalog;
using Nop.Web.Models.Media;

namespace Nop.Web.Factories
{
    public partial class CatalogModelFactory : ICatalogModelFactory
    {
        #region Fields

        private readonly BlogSettings _blogSettings;
        private readonly CatalogSettings _catalogSettings;
        private readonly DisplayDefaultMenuItemSettings _displayDefaultMenuItemSettings;
        private readonly ForumSettings _forumSettings;
        private readonly IActionContextAccessor _actionContextAccessor;
        private readonly ICategoryService _categoryService;
        private readonly ICategoryTemplateService _categoryTemplateService;
        private readonly ICurrencyService _currencyService;
        private readonly ICustomerService _customerService;
        private readonly IEventPublisher _eventPublisher;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILocalizationService _localizationService;
        private readonly IManufacturerService _manufacturerService;
        private readonly IManufacturerTemplateService _manufacturerTemplateService;
        private readonly IPictureService _pictureService;
        private readonly IProductModelFactory _productModelFactory;
        private readonly IProductService _productService;
        private readonly IProductTagService _productTagService;
        private readonly ISearchTermService _searchTermService;
        private readonly ISpecificationAttributeService _specificationAttributeService;
        private readonly IStaticCacheManager _staticCacheManager;
        private readonly IStoreContext _storeContext;
        private readonly ITopicService _topicService;
        private readonly IUrlHelperFactory _urlHelperFactory;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IVendorService _vendorService;
        private readonly IWebHelper _webHelper;
        private readonly IWorkContext _workContext;
        private readonly MediaSettings _mediaSettings;
        private readonly VendorSettings _vendorSettings;

        private readonly IElasticsearchManagerService _elasticsearchManager;
        private readonly IPriceFormatter _priceFormatter;
        #endregion

        #region Ctor

        public CatalogModelFactory(BlogSettings blogSettings,
            CatalogSettings catalogSettings,
            DisplayDefaultMenuItemSettings displayDefaultMenuItemSettings,
            ForumSettings forumSettings,
            IActionContextAccessor actionContextAccessor,
            ICategoryService categoryService,
            ICategoryTemplateService categoryTemplateService,
            ICurrencyService currencyService,
            ICustomerService customerService,
            IEventPublisher eventPublisher,
            IHttpContextAccessor httpContextAccessor,
            ILocalizationService localizationService,
            IManufacturerService manufacturerService,
            IManufacturerTemplateService manufacturerTemplateService,
            IPictureService pictureService,
            IProductModelFactory productModelFactory,
            IProductService productService,
            IProductTagService productTagService,
            ISearchTermService searchTermService,
            ISpecificationAttributeService specificationAttributeService,
            IStaticCacheManager staticCacheManager,
            IStoreContext storeContext,
            ITopicService topicService,
            IUrlHelperFactory urlHelperFactory,
            IUrlRecordService urlRecordService,
            IVendorService vendorService,
            IWebHelper webHelper,
            IWorkContext workContext,
            MediaSettings mediaSettings,
            VendorSettings vendorSettings,

            IElasticsearchManagerService elasticsearchManager,
            IPriceFormatter priceFormatter)
        {
            _blogSettings = blogSettings;
            _catalogSettings = catalogSettings;
            _displayDefaultMenuItemSettings = displayDefaultMenuItemSettings;
            _forumSettings = forumSettings;
            _actionContextAccessor = actionContextAccessor;
            _categoryService = categoryService;
            _categoryTemplateService = categoryTemplateService;
            _currencyService = currencyService;
            _customerService = customerService;
            _eventPublisher = eventPublisher;
            _httpContextAccessor = httpContextAccessor;
            _localizationService = localizationService;
            _manufacturerService = manufacturerService;
            _manufacturerTemplateService = manufacturerTemplateService;
            _pictureService = pictureService;
            _productModelFactory = productModelFactory;
            _productService = productService;
            _productTagService = productTagService;
            _searchTermService = searchTermService;
            _specificationAttributeService = specificationAttributeService;
            _staticCacheManager = staticCacheManager;
            _storeContext = storeContext;
            _topicService = topicService;
            _urlHelperFactory = urlHelperFactory;
            _urlRecordService = urlRecordService;
            _vendorService = vendorService;
            _webHelper = webHelper;
            _workContext = workContext;
            _mediaSettings = mediaSettings;
            _vendorSettings = vendorSettings;

            _elasticsearchManager = elasticsearchManager;
            _priceFormatter = priceFormatter;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Prepares the category filter model
        /// </summary>
        /// <param name="selectedManufacturers">The selected manufacturers to filter the products</param>
        /// <param name="availableManufacturers">The available manufacturers to filter the products</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the specification filter model
        /// </returns>
        protected virtual async Task<CategoryFilterModel> PrepareCategoryFilterModel(IList<int> selectedCategoriesIds, IList<Category> availableCategories,
            IList<int> manufacturerIds, IList<int> vendorIds, decimal? priceMin = null,
            decimal? priceMax = null,
            string keywords = null,
            IList<SpecificationAttributeOption> filteredSpecOptions = null)
        {
            var model = new CategoryFilterModel();

            if (availableCategories?.Any() == true)
            {
                model.Enabled = true;

                var workingLanguage = await _workContext.GetWorkingLanguageAsync();
                var numberOfProducts = new int?();
                var currentStore = await _storeContext.GetCurrentStoreAsync();

                foreach (var category in availableCategories)
                {
                    if (category.ParentCategoryId == 0)
                    {
                        if (_catalogSettings.ShowCategoryProductNumber)
                        {
                            var categoryIds = new List<int> { category.Id };
                            //include subcategories
                            if (_catalogSettings.ShowCategoryProductNumberIncludingSubcategories)
                                categoryIds.AddRange(
                                    await _categoryService.GetChildCategoryIdsAsync(category.Id, currentStore.Id));

                            numberOfProducts = await _productService.GetNumberOfProductsInCategoryByGeoVendorIdsAsync(categoryIds, currentStore.Id,
                                manufacturerIds, vendorIds, priceMin, priceMax, keywords, filteredSpecOptions, true);
                        }

                        model.Categories.Add(new CategoryModel
                        {
                            Id = category.Id,
                            Name = await _localizationService
                                .GetLocalizedAsync(category, x => x.Name, workingLanguage.Id),
                            IsSelected = selectedCategoriesIds?
                                .Any(categoryId => categoryId == category.Id) == true,
                            NumberOfProducts = numberOfProducts,
                            DisplayOrder = category.DisplayOrder
                        });
                    }
                }

                foreach (var item in model.Categories.Where(x => x.IsSelected == true))
                {
                    var subCategorys = await _categoryService.GetAllCategoriesByParentCategoryIdAsync(item.Id);
                    foreach (var subCategory in subCategorys)
                    {
                        int? subCategoryProductCount = new int?();
                        if (_catalogSettings.ShowCategoryProductNumber)
                        {
                            var categoryIds = new List<int> { subCategory.Id };
                            //include subcategories
                            if (_catalogSettings.ShowCategoryProductNumberIncludingSubcategories)
                                categoryIds.AddRange(
                                    await _categoryService.GetChildCategoryIdsAsync(subCategory.Id, currentStore.Id));

                            subCategoryProductCount = await _productService.GetNumberOfProductsInCategoryByGeoVendorIdsAsync(categoryIds, currentStore.Id,
                                 manufacturerIds, vendorIds, priceMin, priceMax, keywords, filteredSpecOptions, true);
                        }

                        model.SubCategories.Add(new CategoryModel
                        {
                            Id = subCategory.Id,
                            Name = await _localizationService
                                .GetLocalizedAsync(subCategory, x => x.Name, workingLanguage.Id),
                            NumberOfProducts = subCategoryProductCount,
                            ParentCategryId = item.Id,
                            DisplayOrder = subCategory.DisplayOrder
                        });
                    }
                }

                foreach (var item in model.SubCategories)
                {
                    var childSubCategories = await _categoryService.GetAllCategoriesByParentCategoryIdAsync(item.Id);
                    foreach (var subCategory in childSubCategories)
                    {
                        int? childCategoryProductCount = new int?();
                        if (_catalogSettings.ShowCategoryProductNumber)
                        {
                            var categoryIds = new List<int> { subCategory.Id };
                            //include subcategories
                            if (_catalogSettings.ShowCategoryProductNumberIncludingSubcategories)
                                categoryIds.AddRange(
                                    await _categoryService.GetChildCategoryIdsAsync(subCategory.Id, currentStore.Id));

                            childCategoryProductCount = await _productService.GetNumberOfProductsInCategoryByGeoVendorIdsAsync(categoryIds, currentStore.Id,
                                      manufacturerIds, vendorIds, priceMin, priceMax, keywords, filteredSpecOptions, true);
                        }

                        model.ChildCategories.Add(new CategoryModel
                        {
                            Id = subCategory.Id,
                            Name = await _localizationService
                                .GetLocalizedAsync(subCategory, x => x.Name, workingLanguage.Id),
                            NumberOfProducts = childCategoryProductCount,
                            ParentCategryId = item.Id,
                            DisplayOrder = subCategory.DisplayOrder
                        });
                    }
                }
            }

            return model;
        }

        protected virtual async Task<CategoryFilterModel> PrepareCategoryFilterModel(IList<int> selectedCategoriesIds, IList<Category> availableCategories)
        {
            var model = new CategoryFilterModel();

            if (availableCategories?.Any() == true)
            {
                model.Enabled = true;

                var workingLanguage = await _workContext.GetWorkingLanguageAsync();

                foreach (var category in availableCategories)
                {
                    if (category.ParentCategoryId == 0)
                    {
                        model.Categories.Add(new CategoryModel
                        {
                            Id = category.Id,
                            Name = await _localizationService
                                .GetLocalizedAsync(category, x => x.Name, workingLanguage.Id),
                            IsSelected = selectedCategoriesIds?
                                .Any(categoryId => categoryId == category.Id) == true,
                        });
                    }
                }

            }

            return model;
        }
        #endregion

        #region Categories

        /// <summary>
        /// Prepares the category products model
        /// </summary>
        /// <param name="category">Category</param>
        /// <param name="command">Model to get the catalog products</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the category products model
        /// </returns>
        public virtual async Task<CatalogProductsModel> PrepareCategoryProductsModelAsync(Category category, CatalogProductsCommand command)
        {
            if (category == null)
                throw new ArgumentNullException(nameof(category));

            if (command == null)
                throw new ArgumentNullException(nameof(command));

            var model = new CatalogProductsModel
            {
                UseAjaxLoading = _catalogSettings.UseAjaxCatalogProductsLoading
            };

            var currentStore = await _storeContext.GetCurrentStoreAsync();

            //sorting
            await PrepareSortingOptionsAsync(model, command);
            //view mode
            await PrepareViewModesAsync(model, command);
            //page size
            await PreparePageSizeOptionsAsync(model, command, category.AllowCustomersToSelectPageSize,
                category.PageSizeOptions, category.PageSize);
            var categoryIds = new List<int> { category.Id };

            //include subcategories
            if (_catalogSettings.ShowProductsFromSubcategories)
            {
                categoryIds.AddRange(await _categoryService.GetChildCategoryIdsAsync(category.Id, currentStore.Id));
            }

            //price range
            PriceRangeModel selectedPriceRange = null;
            if (_catalogSettings.EnablePriceRangeFiltering && category.PriceRangeFiltering)
            {
                selectedPriceRange = await GetConvertedPriceRangeAsync(command);

                PriceRangeModel availablePriceRange = null;
                if (!category.ManuallyPriceRange)
                {
                    async Task<decimal?> getProductPriceAsync(ProductSortingEnum orderBy)
                    {
                        var products = await _productService.SearchProductsAsync(0, 1,
                            categoryIds: categoryIds,
                            storeId: currentStore.Id,
                            visibleIndividuallyOnly: true,
                            excludeFeaturedProducts: !_catalogSettings.IgnoreFeaturedProducts && !_catalogSettings.IncludeFeaturedProductsInNormalLists,
                            orderBy: orderBy);

                        return products?.FirstOrDefault()?.Price ?? 0;
                    }

                    availablePriceRange = new PriceRangeModel
                    {
                        From = await getProductPriceAsync(ProductSortingEnum.PriceAsc),
                        To = await getProductPriceAsync(ProductSortingEnum.PriceDesc)
                    };
                }
                else
                {
                    availablePriceRange = new PriceRangeModel
                    {
                        From = category.PriceFrom,
                        To = category.PriceTo
                    };
                }

                model.PriceRangeFilter = await PreparePriceRangeFilterAsync(selectedPriceRange, availablePriceRange);
            }
            var parentCategoryIds = await _categoryService.GetAllCategoriesByParentCategoryIdAsync(category.Id);

            //products
            var searchTerm = await model.CatalogProductsCommand.SearchTermFilter.GetSearchTerm(_webHelper);

            // get category Ids
            var categoryIdArray = await model.CatalogProductsCommand.SearchTermFilter.GetCategoryIds(_webHelper);

            // get sub category ids
            var subCategoryIdArray = await model.CatalogProductsCommand.SearchTermFilter.GetSubCategoryId(_webHelper);

            //get vendor ids
            var vendorIdArray = await model.CatalogProductsCommand.SearchTermFilter.GetVendorIds(_webHelper);

            if (!string.IsNullOrEmpty(categoryIdArray))
            {
                categoryIds.AddRange(categoryIdArray.Split(',').Select(int.Parse).ToList());
            }

            if (!string.IsNullOrEmpty(categoryIdArray) && !string.IsNullOrEmpty(subCategoryIdArray))
            {
                categoryIds.AddRange(subCategoryIdArray.Split(',').Select(int.Parse).ToList());
            }
            else
            {
                //include subcategories
                if (_catalogSettings.ShowProductsFromSubcategories)
                {
                    var tempCategoryIds = new List<int>();
                    foreach (var id in categoryIds)
                    {
                        tempCategoryIds.AddRange(await _categoryService.GetChildCategoryIdsAsync(id, currentStore.Id));
                    }
                    categoryIds.AddRange(tempCategoryIds);
                }
            }
            categoryIds = categoryIds.Distinct().ToList();

            var vendorIds = new List<int>();
            if (!string.IsNullOrEmpty(vendorIdArray))
            {
                vendorIds.AddRange(vendorIdArray.Split(',').Select(int.Parse).ToList());
            }
            else
            {
                vendorIds.AddRange(await _workContext.GetCurrentCustomerGeoRadiusVendorIdsAsync());
            }

            // filterable options
            var filterableOptions = await _specificationAttributeService
                .GetFiltrableSpecificationAttributeOptionsByCategoryIdAsync(category.Id);
            if (_catalogSettings.EnableSpecificationAttributeFiltering)
                model.SpecificationFilter = await PrepareSpecificationFilterModel(command.SpecificationOptionIds, filterableOptions);

            //filterable manufacturers
            if (_catalogSettings.EnableManufacturerFiltering)
            {
                var manufacturers = await _manufacturerService.GetManufacturersByCategoryIdAsync(category.Id);

                model.ManufacturerFilter = await PrepareManufacturerFilterModel(command.ManufacturerIds, manufacturers);
            }

            var allCategories = await _categoryService.GetAllCategoriesAsync(storeId: currentStore.Id);
            var categories = allCategories.Where(c => c.ParentCategoryId == 0).OrderBy(c => c.DisplayOrder).ToList();
            model.CategoryFilterModel = await PrepareCategoryFilterModel(categoryIds, categories);

            var filteredSpecs = command.SpecificationOptionIds is null ? null : filterableOptions.Where(fo => command.SpecificationOptionIds.Contains(fo.Id)).ToList();

            //products
            var products = await _productService.SearchProductsAsync(
                           pageIndex: command.PageNumber - 1,
                           pageSize: command.PageSize,
                           categoryIds: categoryIds,
                           storeId: currentStore.Id,
                           visibleIndividuallyOnly: true,
                           excludeFeaturedProducts: !_catalogSettings.IgnoreFeaturedProducts && !_catalogSettings.IncludeFeaturedProductsInNormalLists,
                           priceMin: selectedPriceRange?.From,
                           priceMax: selectedPriceRange?.To,
                           orderBy: (ProductSortingEnum)command.OrderBy,
                           manufacturerIds: command.ManufacturerIds,
                           isMaster: true,
                           filteredSpecOptions: filteredSpecs,
                           geoVendorIds: vendorIds,
                           keywords: searchTerm);

            model.Products = (await _productModelFactory.PrepareProductOverviewModelsAsync(products, prepareSpecificationAttributes: true)).ToList();

            model.LoadPagedList(products);

            //search term
            await model.CatalogProductsCommand.SearchTermFilter.PrepareSearchTerm(searchTerm);

            // category ids
            await model.CatalogProductsCommand.SearchTermFilter.PrepareCategoryIdsSearch(categoryIdArray);

            // sub category ids
            await model.CatalogProductsCommand.SearchTermFilter.PrepareSubCategoryIdsSearch(subCategoryIdArray);

            //vendor ids
            await model.CatalogProductsCommand.SearchTermFilter.PrepareVendorIdsSearch(vendorIdArray);

            return model;
        }

        public virtual async Task<CategoryModel> PrepareCategoryModelAsync(Category category, CatalogProductsCommand command)
        {
            if (category == null)
                throw new ArgumentNullException(nameof(category));

            if (command == null)
                throw new ArgumentNullException(nameof(command));

            var model = new CategoryModel
            {
                Id = category.Id,
                Name = await _localizationService.GetLocalizedAsync(category, x => x.Name),
                Description = await _localizationService.GetLocalizedAsync(category, x => x.Description),
                MetaKeywords = await _localizationService.GetLocalizedAsync(category, x => x.MetaKeywords),
                MetaDescription = await _localizationService.GetLocalizedAsync(category, x => x.MetaDescription),
                MetaTitle = await _localizationService.GetLocalizedAsync(category, x => x.MetaTitle),
                SeName = await _urlRecordService.GetSeNameAsync(category),
                CatalogProductsModel = await PrepareCategoryProductsModelAsync(category, command)
            };

            var customerRoleIds = string.Join(",", (await _customerService.GetCustomerRoleIdsAsync(await _workContext.GetCurrentCustomerAsync())));
            //category breadcrumb
            if (_catalogSettings.CategoryBreadcrumbEnabled)
            {
                model.DisplayCategoryBreadcrumb = true;
                model.CategoryBreadcrumb = await (await _categoryService.GetCategoryBreadCrumbAsync(category)).SelectAwait(async catBr =>
                new CategoryModel
                {
                    Id = catBr.Id,
                    Name = await _localizationService.GetLocalizedAsync(catBr, x => x.Name),
                    SeName = await _urlRecordService.GetSeNameAsync(catBr)
                }).ToListAsync();
            }

            var pictureSize = _mediaSettings.CategoryThumbPictureSize;
            var language = await _workContext.GetWorkingLanguageAsync();
            //categories
            var store = await _workContext.GetWorkingLanguageAsync();
            var cacheKey = _staticCacheManager.PrepareKeyForDefaultCache(NopModelCacheDefaults.CategoryAllModelKey,
              language, customerRoleIds, store);

            var allCategories = await _categoryService.GetAllCategoriesAsync(storeId: store.Id);
            var categories = allCategories.Where(c => c.ParentCategoryId == 0).OrderBy(c => c.DisplayOrder).ToList();
            model.SubCategories = await (await _staticCacheManager.GetAsync(cacheKey, () =>
                categories.SelectAwait(async x =>
                {
                    var subCatModel = new CategoryModel.SubCategoryModel
                    {
                        Id = x.Id,
                        Name = await _localizationService.GetLocalizedAsync(x, y => y.Name),
                        SeName = await _urlRecordService.GetSeNameAsync(x),
                        Description = await _localizationService.GetLocalizedAsync(x, y => y.Description),
                    };

                    //number of products in each category
                    if (_catalogSettings.ShowCategoryProductNumber)
                    {
                        var categoryIds = new List<int> { x.Id };
                        //include subcategories
                        if (_catalogSettings.ShowCategoryProductNumberIncludingSubcategories)
                            categoryIds.AddRange(
                                await _categoryService.GetChildCategoryIdsAsync(x.Id, store.Id));

                        subCatModel.NumberOfProducts = await _productService.GetNumberOfProductsInCategoryAsync(categoryIds, store.Id);
                    }

                    //prepare picture model
                    var categoryPictureCacheKey = _staticCacheManager.PrepareKeyForDefaultCache(NopModelCacheDefaults.CategoryPictureModelKey,
                      category.Id, pictureSize, true, (await _workContext.GetWorkingLanguageAsync()).Id, _webHelper.IsCurrentConnectionSecured(), store.Id);

                    subCatModel.PictureModel = await _staticCacheManager.GetAsync(categoryPictureCacheKey, async () =>
                    {
                        var picture = await _pictureService.GetPictureByIdAsync(x.PictureId);
                        var pictureModel = new PictureModel
                        {
                            FullSizeImageUrl = (await _pictureService.GetPictureUrlAsync(picture)).Url,
                            ImageUrl = (await _pictureService.GetPictureUrlAsync(picture, pictureSize)).Url,
                            Title = string.Format(await _localizationService.GetResourceAsync("Media.Category.ImageLinkTitleFormat"), subCatModel.Name),
                            AlternateText = string.Format(await _localizationService.GetResourceAsync("Media.Category.ImageAlternateTextFormat"), subCatModel.Name)
                        };
                        return pictureModel;
                    });

                    return subCatModel;
                }).ToListAsync()));

            //For showing Minimum product price of subcategory
            if (model.SubCategories.Count > 0)
            {
                foreach (var subCategory in model.SubCategories)
                {
                    subCategory.MinProductPrice = string.Empty;
                    var subcategoryProducts = await _productService.SearchProductsAsync(categoryIds: new List<int> { subCategory.Id });
                    if (subcategoryProducts.Count > 0)
                    {
                        var minPrice = subcategoryProducts.Min(p => p.Price);
                        var productOverviewModelList = (await _productModelFactory.PrepareProductOverviewModelsAsync(subcategoryProducts)).ToList();
                        subCategory.MinProductPrice = productOverviewModelList.Where(p => p.ProductPrice.PriceValue == minPrice).FirstOrDefault()?.ProductPrice?.Price;
                    }
                }
            }

            //featured products
            if (!_catalogSettings.IgnoreFeaturedProducts)
            {
                var featuredProducts = await _productService.GetCategoryFeaturedProductsAsync(category.Id, store.Id);
                if (featuredProducts != null)
                    model.FeaturedProducts = (await _productModelFactory.PrepareProductOverviewModelsAsync(featuredProducts)).ToList();
            }

            //prepare vendors
            var currentCustomer = await _workContext.GetCurrentCustomerAsync();
            var availableVendors = await _vendorService.GetAvailableGeoFenceVendorsAsync(currentCustomer, true);
            if (availableVendors.Any())
            {
                foreach (var item in availableVendors)
                {
                    var vendorModel = await PrepareVendorModelAsync(item, command);
                    if (vendorModel != null)
                        model.Vendors.Add(vendorModel);
                }
            }

            return model;
        }

        /// <summary>
        /// Prepare category (simple) models
        /// </summary>
        /// <param name="rootCategoryId">Root category identifier</param>
        /// <param name="loadSubCategories">A value indicating whether subcategories should be loaded</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the list of category (simple) models
        /// </returns>
        public virtual async Task<List<CategorySimpleModel>> PrepareCategorySimpleModelsAsync(int rootCategoryId, bool loadSubCategories = true)
        {
            var result = new List<CategorySimpleModel>();

            //little hack for performance optimization
            //we know that this method is used to load top and left menu for categories.
            //it'll load all categories anyway.
            //so there's no need to invoke "GetAllCategoriesByParentCategoryId" multiple times (extra SQL commands) to load childs
            //so we load all categories at once (we know they are cached)
            var store = await _storeContext.GetCurrentStoreAsync();
            var allCategories = await _categoryService.GetAllCategoriesAsync(storeId: store.Id);
            var categories = allCategories.Where(c => c.ParentCategoryId == rootCategoryId).OrderBy(c => c.DisplayOrder).ToList();
            foreach (var category in categories)
            {
                var categoryModel = new CategorySimpleModel
                {
                    Id = category.Id,
                    Name = await _localizationService.GetLocalizedAsync(category, x => x.Name),
                    SeName = await _urlRecordService.GetSeNameAsync(category),
                    IncludeInTopMenu = category.IncludeInTopMenu
                };

                //++Alchub: do not use default product count here
                ////number of products in each category
                //if (_catalogSettings.ShowCategoryProductNumber)
                //{
                //    var categoryIds = new List<int> { category.Id };
                //    //include subcategories
                //    if (_catalogSettings.ShowCategoryProductNumberIncludingSubcategories)
                //        categoryIds.AddRange(
                //            await _categoryService.GetChildCategoryIdsAsync(category.Id, store.Id));

                //    categoryModel.NumberOfProducts =
                //        await _productService.GetNumberOfProductsInCategoryAsync(categoryIds, store.Id);
                //}

                if (loadSubCategories)
                {
                    var subCategories = await PrepareCategorySimpleModelsAsync(category.Id);
                    categoryModel.SubCategories.AddRange(subCategories);
                }

                categoryModel.HaveSubCategories = categoryModel.SubCategories.Count > 0 &
                    categoryModel.SubCategories.Any(x => x.IncludeInTopMenu);

                result.Add(categoryModel);
            }

            return result;
        }

        #endregion

        #region Searching

        /// <summary>
        /// Prepare search model
        /// </summary>
        /// <param name="model">Search model</param>
        /// <param name="command">Model to get the catalog products</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the search model
        /// </returns>
        public virtual async Task<SearchModel> PrepareSearchModelAsync(SearchModel model, CatalogProductsCommand command)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            if (command == null)
                throw new ArgumentNullException(nameof(command));

            model.CatalogProductsModel = await PrepareCustomSearchProductsModelAsync(model, command);

            return model;
        }

        /// <summary>
        /// Prepares the search products model
        /// </summary>+
        /// <param name="model">Search model</param>
        /// <param name="command">Model to get the catalog products</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the search products model
        /// </returns>
        public virtual async Task<CatalogProductsModel> PrepareSearchProductsModelAsync(SearchModel searchModel, CatalogProductsCommand command)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            var model = new CatalogProductsModel
            {
                UseAjaxLoading = _catalogSettings.UseAjaxCatalogProductsLoading
            };

            //sorting
            await PrepareSortingOptionsAsync(model, command);
            //view mode
            await PrepareViewModesAsync(model, command);
            //page size
            await PreparePageSizeOptionsAsync(model, command, _catalogSettings.SearchPageAllowCustomersToSelectPageSize,
                _catalogSettings.SearchPagePageSizeOptions, _catalogSettings.SearchPageProductsPerPage);

            var searchTerms = searchModel.q == null
                ? string.Empty
                : searchModel.q.Trim();

            IPagedList<Product> products = new PagedList<Product>(new List<Product>(), 0, 1);
            //only search if query string search keyword is set (used to avoid searching or displaying search term min length error message on /search page load)
            //we don't use "!string.IsNullOrEmpty(searchTerms)" in cases of "ProductSearchTermMinimumLength" set to 0 but searching by other parameters (e.g. category or price filter)
            var isSearchTermSpecified = _httpContextAccessor.HttpContext.Request.Query.ContainsKey("q");
            if (isSearchTermSpecified)
            {
                var currentStore = await _storeContext.GetCurrentStoreAsync();

                if (searchTerms.Length < _catalogSettings.ProductSearchTermMinimumLength)
                {
                    model.WarningMessage =
                        string.Format(await _localizationService.GetResourceAsync("Search.SearchTermMinimumLengthIsNCharacters"),
                            _catalogSettings.ProductSearchTermMinimumLength);
                }
                else
                {
                    var categoryIds = new List<int>();
                    var manufacturerId = 0;
                    var searchInDescriptions = false;
                    var vendorId = 0;
                    if (searchModel.advs)
                    {
                        //advanced search
                        var categoryId = searchModel.cid;
                        if (categoryId > 0)
                        {
                            categoryIds.Add(categoryId);
                            if (searchModel.isc)
                            {
                                //include subcategories
                                categoryIds.AddRange(
                                    await _categoryService.GetChildCategoryIdsAsync(categoryId, currentStore.Id));
                            }
                        }

                        manufacturerId = searchModel.mid;

                        if (searchModel.asv)
                            vendorId = searchModel.vid;

                        searchInDescriptions = searchModel.sid;
                    }

                    //var searchInProductTags = false;
                    var searchInProductTags = searchInDescriptions;
                    var workingLanguage = await _workContext.GetWorkingLanguageAsync();

                    //price range
                    PriceRangeModel selectedPriceRange = null;
                    if (_catalogSettings.EnablePriceRangeFiltering && _catalogSettings.SearchPagePriceRangeFiltering)
                    {
                        selectedPriceRange = await GetConvertedPriceRangeAsync(command);

                        PriceRangeModel availablePriceRange;
                        if (!_catalogSettings.SearchPageManuallyPriceRange)
                        {
                            async Task<decimal?> getProductPriceAsync(ProductSortingEnum orderBy)
                            {
                                var products = await _productService.SearchProductsAsync(0, 1,
                                    categoryIds: categoryIds,
                                    manufacturerIds: new List<int> { manufacturerId },
                                    storeId: currentStore.Id,
                                    visibleIndividuallyOnly: true,
                                    keywords: searchTerms,
                                    searchDescriptions: searchInDescriptions,
                                    searchProductTags: searchInProductTags,
                                    languageId: workingLanguage.Id,
                                    vendorId: vendorId,
                                    orderBy: orderBy);

                                return products?.FirstOrDefault()?.Price ?? 0;
                            }

                            availablePriceRange = new PriceRangeModel
                            {
                                From = await getProductPriceAsync(ProductSortingEnum.PriceAsc),
                                To = await getProductPriceAsync(ProductSortingEnum.PriceDesc)
                            };
                        }
                        else
                        {
                            availablePriceRange = new PriceRangeModel
                            {
                                From = _catalogSettings.SearchPagePriceFrom,
                                To = _catalogSettings.SearchPagePriceTo
                            };
                        }

                        model.PriceRangeFilter = await PreparePriceRangeFilterAsync(selectedPriceRange, availablePriceRange);
                    }

                    //products
                    products = await _productService.SearchProductsAsync(
                        command.PageNumber - 1,
                        command.PageSize,
                        categoryIds: categoryIds,
                        manufacturerIds: new List<int> { manufacturerId },
                        storeId: currentStore.Id,
                        visibleIndividuallyOnly: true,
                        keywords: searchTerms,
                        priceMin: selectedPriceRange?.From,
                        priceMax: selectedPriceRange?.To,
                        searchDescriptions: searchInDescriptions,
                        searchProductTags: searchInProductTags,
                        languageId: workingLanguage.Id,
                        orderBy: (ProductSortingEnum)command.OrderBy,
                        vendorId: vendorId,
                        //master products only filter along with geoVendorIds
                        isMaster: true,
                        geoVendorIds: (await _workContext.GetCurrentCustomerGeoRadiusVendorIdsAsync()));

                    //search term statistics
                    if (!string.IsNullOrEmpty(searchTerms))
                    {
                        var searchTerm =
                            await _searchTermService.GetSearchTermByKeywordAsync(searchTerms, currentStore.Id);
                        if (searchTerm != null)
                        {
                            searchTerm.Count++;
                            await _searchTermService.UpdateSearchTermAsync(searchTerm);
                        }
                        else
                        {
                            searchTerm = new SearchTerm
                            {
                                Keyword = searchTerms,
                                StoreId = currentStore.Id,
                                Count = 1
                            };
                            await _searchTermService.InsertSearchTermAsync(searchTerm);
                        }
                    }

                    //event
                    await _eventPublisher.PublishAsync(new ProductSearchEvent
                    {
                        SearchTerm = searchTerms,
                        SearchInDescriptions = searchInDescriptions,
                        CategoryIds = categoryIds,
                        ManufacturerId = manufacturerId,
                        WorkingLanguageId = workingLanguage.Id,
                        VendorId = vendorId
                    });
                }
            }

            var isFiltering = !string.IsNullOrEmpty(searchTerms);
            await PrepareCatalogProductsAsync(model, products, isFiltering);

            return model;
        }

        /// <summary>
        /// Prepare sorting options
        /// </summary>
        /// <param name="pagingFilteringModel">Catalog paging filtering model</param>
        /// <param name="command">Catalog paging filtering command</param>
        public virtual async Task PrepareCustomSortingOptions(CatalogProductsModel pagingFilteringModel, CatalogProductsCommand command)
        {
            if (pagingFilteringModel == null)
                throw new ArgumentNullException(nameof(pagingFilteringModel));

            if (command == null)
                throw new ArgumentNullException(nameof(command));

            //set the order by position by default
            pagingFilteringModel.OrderBy = command.OrderBy;
            command.OrderBy = (int)ProductSortingEnum.Position;

            //ensure that product sorting is enabled
            if (!_catalogSettings.AllowProductSorting)
                return;

            //get active sorting options
            var activeSortingOptionsIds = Enum.GetValues(typeof(ProductSortingEnum)).Cast<int>()
                .Except(_catalogSettings.ProductSortingEnumDisabled).ToList();
            if (!activeSortingOptionsIds.Any())
                return;

            //order sorting options
            var orderedActiveSortingOptions = activeSortingOptionsIds
                .Select(id => new { Id = id, Order = _catalogSettings.ProductSortingEnumDisplayOrder.TryGetValue(id, out int order) ? order : id })
                .OrderBy(option => option.Order).ToList();

            pagingFilteringModel.AllowProductSorting = true;
            command.OrderBy = pagingFilteringModel.OrderBy ?? orderedActiveSortingOptions.FirstOrDefault().Id;

            //prepare available model sorting options
            var currentPageUrl = _webHelper.GetThisPageUrl(true);
            foreach (var option in orderedActiveSortingOptions)
            {
                pagingFilteringModel.AvailableSortOptions.Add(new SelectListItem
                {
                    Text = await _localizationService.GetLocalizedEnumAsync((ProductSortingEnum)option.Id),
                    Value = _webHelper.ModifyQueryString(currentPageUrl, "orderby", option.Id.ToString()),
                    Selected = option.Id == command.OrderBy
                });
            }
        }

        public virtual async Task PrepareCustomViewModesAsync(CatalogProductsModel model, CatalogProductsCommand command)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            if (command == null)
                throw new ArgumentNullException(nameof(command));

            model.AllowProductViewModeChanging = _catalogSettings.AllowProductViewModeChanging;

            var viewMode = !string.IsNullOrEmpty(command.ViewMode)
                ? command.ViewMode
                : _catalogSettings.DefaultViewMode;
            model.ViewMode = viewMode;
            if (model.AllowProductViewModeChanging)
            {
                var currentPageUrl = _webHelper.GetThisPageUrl(true);
                //grid
                model.AvailableViewModes.Add(new SelectListItem
                {
                    Text = await _localizationService.GetResourceAsync("Catalog.ViewMode.Grid"),
                    Value = _webHelper.ModifyQueryString(currentPageUrl, "viewmode", "grid"),
                    Selected = viewMode == "grid"
                });
                //list
                model.AvailableViewModes.Add(new SelectListItem
                {
                    Text = await _localizationService.GetResourceAsync("Catalog.ViewMode.List"),
                    Value = _webHelper.ModifyQueryString(currentPageUrl, "viewmode", "list"),
                    Selected = viewMode == "list"
                });
            }
        }

        public virtual Task PrepareCustomPageSizeOptionsAsync(CatalogProductsModel pagingFilteringModel, CatalogProductsCommand command,
                bool allowCustomersToSelectPageSize, string pageSizeOptions, int fixedPageSize)
        {
            if (pagingFilteringModel == null)
                throw new ArgumentNullException(nameof(pagingFilteringModel));

            if (command == null)
                throw new ArgumentNullException(nameof(command));

            if (command.PageNumber <= 0)
                command.PageNumber = 1;
            pagingFilteringModel.AllowCustomersToSelectPageSize = false;
            if (allowCustomersToSelectPageSize && pageSizeOptions != null)
            {
                var pageSizes = pageSizeOptions.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);

                if (pageSizes.Any())
                {
                    // get the first page size entry to use as the default (category page load) or if customer enters invalid value via query string
                    if (command.PageSize <= 0 || !pageSizes.Contains(command.PageSize.ToString()))
                        if (int.TryParse(pageSizes.FirstOrDefault(), out var temp))
                            if (temp > 0)
                                command.PageSize = temp;

                    var currentPageUrl = _webHelper.GetThisPageUrl(true);
                    var sortUrl = _webHelper.RemoveQueryString(currentPageUrl, "pagenumber");

                    foreach (var pageSize in pageSizes)
                    {
                        if (!int.TryParse(pageSize, out var temp))
                            continue;
                        if (temp <= 0)
                            continue;

                        pagingFilteringModel.PageSizeOptions.Add(new SelectListItem
                        {
                            Text = pageSize,
                            Value = _webHelper.ModifyQueryString(sortUrl, "pagesize", pageSize),
                            Selected = pageSize.Equals(command.PageSize.ToString(), StringComparison.InvariantCultureIgnoreCase)
                        });
                    }

                    if (pagingFilteringModel.PageSizeOptions.Any())
                    {
                        pagingFilteringModel.PageSizeOptions = pagingFilteringModel.PageSizeOptions.OrderBy(x => int.Parse(x.Text)).ToList();
                        pagingFilteringModel.AllowCustomersToSelectPageSize = true;

                        if (command.PageSize <= 0)
                            command.PageSize = int.Parse(pagingFilteringModel.PageSizeOptions.First().Text);
                    }
                }
            }
            else
                //customer is not allowed to select a page size
                command.PageSize = fixedPageSize;

            //ensure pge size is specified
            if (command.PageSize <= 0)
                command.PageSize = fixedPageSize;
            return Task.CompletedTask;
        }

        public virtual async Task<CatalogProductsModel> PrepareCustomSearchProductsModelAsync(SearchModel searchModel, CatalogProductsCommand command)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            var model = new CatalogProductsModel
            {
                UseAjaxLoading = _catalogSettings.UseAjaxCatalogProductsLoading
            };

            //sorting
            await PrepareCustomSortingOptions(model, command);
            //view mode
            await PrepareCustomViewModesAsync(model, command);
            //page size
            await PrepareCustomPageSizeOptionsAsync(model, command, _catalogSettings.SearchPageAllowCustomersToSelectPageSize,
                _catalogSettings.SearchPagePageSizeOptions, _catalogSettings.SearchPageProductsPerPage);

            //search terms of search page
            var searchKey = await model.CatalogProductsCommand.SearchTermFilter.GetSearchTerm(_webHelper);
            if (!string.IsNullOrEmpty(searchKey))
                searchModel.q = searchKey;

            var searchTerms = searchModel.q == null
                ? string.Empty
                : searchModel.q.Trim();

            IPagedList<Product> products = new PagedList<Product>(new List<Product>(), 0, 1);
            var language = await _workContext.GetWorkingLanguageAsync();
            var currentStore = await _storeContext.GetCurrentStoreAsync();
            var categoryId = await model.CatalogProductsCommand.SearchTermFilter.GetCategoryIds(_webHelper);
            var subCategoryIdArray = await model.CatalogProductsCommand.SearchTermFilter.GetSubCategoryId(_webHelper);
            var vendorIdArray = await model.CatalogProductsCommand.SearchTermFilter.GetVendorIds(_webHelper);
            var manufacturerIdArray = await model.CatalogProductsCommand.SearchTermFilter.GetManufacturerIds(_webHelper);
            var price = await model.CatalogProductsCommand.SearchTermFilter.GetPriceRange(_webHelper);
            var categoryIds = new List<int>();
            //price range
            PriceRangeModel selectedPriceRange = null;
            var selectedCategoryId = string.IsNullOrEmpty(categoryId) ? 0 : Convert.ToInt32(categoryId);

            if (selectedCategoryId > 0)
                categoryIds.Add(selectedCategoryId);

            if (selectedCategoryId > 0 && !string.IsNullOrEmpty(subCategoryIdArray))
            {
                categoryIds.AddRange(subCategoryIdArray.Split(',').Select(int.Parse).ToList());
            }
            else
            {
                //include subcategories
                if (_catalogSettings.ShowProductsFromSubcategories)
                {
                    var tempCategoryIds = new List<int>();
                    foreach (var id in categoryIds)
                    {
                        tempCategoryIds.AddRange(await _categoryService.GetChildCategoryIdsAsync(id, currentStore.Id));
                    }
                    categoryIds.AddRange(tempCategoryIds);
                }
            }
            categoryIds = categoryIds.Distinct().ToList();

            //assign categoryIds to make filter check work correctly
            var filteredCategoryIds = new List<int>();
            filteredCategoryIds.AddRange(categoryIds);

            //prepare final category to be included in search query
            var allCategories = await _categoryService.GetAllCategoriesAsync(storeId: currentStore.Id);
            if (selectedCategoryId > 0 && !string.IsNullOrEmpty(subCategoryIdArray) && _catalogSettings.ShowProductsFromSubcategories)
            {
                var removeCategoryIds = new List<int>();
                if (!string.IsNullOrEmpty(subCategoryIdArray))
                    removeCategoryIds.Add(selectedCategoryId);

                foreach (var id in categoryIds)
                {
                    var category = allCategories.FirstOrDefault(x => x.Id == id);
                    if (category != null)
                    {
                        if (category.ParentCategoryId > 0 && !removeCategoryIds.Contains(category.ParentCategoryId))
                            removeCategoryIds.Add(category.ParentCategoryId);
                    }
                }
                //flush category ids
                if (removeCategoryIds.Contains(0))
                    removeCategoryIds.Remove(0);

                removeCategoryIds = removeCategoryIds.Distinct().ToList();

                //final category ids
                categoryIds.RemoveAll(x => removeCategoryIds.Contains(x));

                //add chile categories for option
                //include subcategories
                if (_catalogSettings.ShowProductsFromSubcategories)
                {
                    var tempCategoryIds = new List<int>();
                    foreach (var id in categoryIds)
                    {
                        tempCategoryIds.AddRange(await _categoryService.GetChildCategoryIdsAsync(id, currentStore.Id));
                    }
                    categoryIds.AddRange(tempCategoryIds);
                }
            }

            //prepare filter setting wise
            var vendorIds = new List<int>();
            if (_catalogSettings.EnableVendorFiltering)
            {
                //prepare vendors filter
                var availableVendorIds = await _workContext.GetCurrentCustomerGeoRadiusVendorIdsAsync();
                if (!string.IsNullOrEmpty(vendorIdArray))
                    vendorIds.AddRange(vendorIdArray.Split(',').Select(int.Parse).ToList());
                else
                    vendorIds.AddRange(availableVendorIds);

                var availableVendors = await _vendorService.GetVendorsByIdsAsync(availableVendorIds?.ToArray());
                if (availableVendors.Any())
                {
                    foreach (var item in availableVendors)
                    {
                        model.Vendors.Add(new SelectListItem
                        {
                            Value = Convert.ToString(item.Id),
                            Text = item.Name
                        });
                    }
                }
            }

            //specification filter
            var filterOptions = new List<SpecificationAttributeOption>();
            if (_catalogSettings.EnableSpecificationAttributeFiltering)
            {
                foreach (var item in categoryIds)
                {
                    // filterable options
                    var filterableOptions = await _specificationAttributeService.
                        GetFiltrableSpecificationAttributeOptionsByCategoryIdAsync(item);

                    filterOptions.AddRange(filterableOptions);
                }

                //if (_catalogSettings.EnableSpecificationAttributeFiltering && filterOptions.Any())
                if (filterOptions.Any())
                {
                    filterOptions = filterOptions.DistinctBy(fo => fo.Id).ToList();
                    model.SpecificationFilter = await PrepareSpecificationFilterModel(command.SpecificationOptionIds, filterOptions);
                }
            }

            //manufacturers filter
            var manufacturerIds = new List<int>();
            if (_catalogSettings.EnableManufacturerFiltering)
            {
                if (!string.IsNullOrEmpty(manufacturerIdArray))
                    manufacturerIds.AddRange(manufacturerIdArray.Split(',').Select(int.Parse).ToList());

                var manufacturers = new List<Manufacturer>();
                if (!categoryIds.Any())
                    manufacturers.AddRange(await _manufacturerService.GetManufacturersByIdsAsync(manufacturerIds.ToArray()));
                else
                {
                    var filterByCatIds = new List<int>();
                    filterByCatIds.AddRange(categoryIds);
                    foreach (var catId in filterByCatIds)
                        manufacturers.AddRange(await _manufacturerService.GetManufacturersByCategoryIdAsync(catId));
                }

                if (manufacturers.Any())
                {
                    manufacturers = manufacturers.DistinctBy(m => m.Id).ToList();
                    model.ManufacturerFilter = await PrepareManufacturerFilterModel(manufacturerIds, manufacturers);
                }
            }

            //price range
            if (_catalogSettings.EnablePriceRangeFiltering && _catalogSettings.SearchPagePriceRangeFiltering)
            {
                if (!string.IsNullOrEmpty(price))
                {
                    command.Price = price;
                    selectedPriceRange = await GetConvertedPriceRangeAsync(command);
                }
                else
                    selectedPriceRange = await GetConvertedPriceRangeAsync(command);

                PriceRangeModel availablePriceRange;
                //default
                availablePriceRange = new PriceRangeModel
                {
                    From = _catalogSettings.SearchPagePriceFrom,
                    To = _catalogSettings.SearchPagePriceTo
                };
                model.PriceRangeFilter = await PreparePriceRangeFilterAsync(selectedPriceRange, availablePriceRange);
            }

            var alreadyFilteredSpecOptionIds = _catalogSettings.EnableSpecificationAttributeFiltering ? model.CatalogProductsCommand.SpecificationFilter.GetAlreadyFilteredSpecOptionIds(_webHelper) : null;
            var filteredSpecs = alreadyFilteredSpecOptionIds is null ? null : filterOptions.Where(fo => alreadyFilteredSpecOptionIds.Contains(fo.Id)).ToList();

            if (_catalogSettings.EnableCategoryFiltering)
            {
                //category filter
                model.CategoryFilterModel = await PrepareCategoryFilterModel(filteredCategoryIds, allCategories, manufacturerIds, vendorIds,
                    selectedPriceRange?.From, selectedPriceRange?.To, searchTerms, filteredSpecs);
            }

            //if no any filter is selected then show all products
            if (searchTerms.Length < _catalogSettings.ProductSearchTermMinimumLength && searchModel.cid == 0 &&
                string.IsNullOrEmpty(categoryId) && string.IsNullOrEmpty(vendorIdArray) &&
                string.IsNullOrEmpty(manufacturerIdArray) && filteredSpecs == null)
            {
                //products
                products = await _productService.SearchProductsAsync(
                    command.PageNumber - 1,
                    command.PageSize,
                    categoryIds: categoryIds,
                    manufacturerIds: manufacturerIds,
                    storeId: currentStore.Id,
                    visibleIndividuallyOnly: true,
                    keywords: searchTerms,
                    filteredSpecOptions: null,
                    priceMin: selectedPriceRange?.From,
                    priceMax: selectedPriceRange?.To,
                    //languageId: language.Id,
                    orderBy: (ProductSortingEnum)command.OrderBy,
                    //master products only filter along with geoVendorIds
                    isMaster: true,
                    geoVendorIds: vendorIds,
                    //exclude other search exept product name
                    searchDescriptions: false,
                    searchManufacturerPartNumber: false,
                    searchSku: false,
                    searchProductTags: false,
                    languageId: 0);

                model.CatalogProductsCommand.LoadPagedList(products);
            }
            else
            {
                //products
                products = await _productService.SearchProductsAsync(
                    command.PageNumber - 1,
                    command.PageSize,
                    categoryIds: categoryIds,
                    manufacturerIds: manufacturerIds,
                    storeId: currentStore.Id,
                    visibleIndividuallyOnly: true,
                    keywords: searchTerms,
                    filteredSpecOptions: filteredSpecs,
                    priceMin: selectedPriceRange?.From,
                    priceMax: selectedPriceRange?.To,
                    searchDescriptions: false,
                    searchProductTags: false,
                    //languageId: language.Id,
                    orderBy: (ProductSortingEnum)command.OrderBy,
                    //master products only filter along with geoVendorIds
                    isMaster: true,
                    geoVendorIds: vendorIds,
                    //exclude other search exept product name
                    searchManufacturerPartNumber: false,
                    searchSku: false,
                    languageId: 0);

                model.CatalogProductsCommand.LoadPagedList(products);

                //search term statistics
                if (!string.IsNullOrEmpty(searchTerms))
                {
                    var searchTerm =
                        await _searchTermService.GetSearchTermByKeywordAsync(searchTerms, currentStore.Id);
                    if (searchTerm != null)
                    {
                        searchTerm.Count++;
                        await _searchTermService.UpdateSearchTermAsync(searchTerm);
                    }
                    else
                    {
                        searchTerm = new SearchTerm
                        {
                            Keyword = searchTerms,
                            StoreId = currentStore.Id,
                            Count = 1
                        };
                        await _searchTermService.InsertSearchTermAsync(searchTerm);
                    }
                }

                ////event - comment event publishing as it was not required.
                //await _eventPublisher.PublishAsync(new ProductSearchEvent
                //{
                //    SearchTerm = searchTerms,
                //    SearchInDescriptions = false,
                //    CategoryIds = categoryIds,
                //    ManufacturerId = 0,
                //    WorkingLanguageId = language.Id,
                //    VendorId = 0
                //});
            }

            //category ids
            await model.CatalogProductsCommand.SearchTermFilter.PrepareCategoryIdsSearch(categoryId);
            //sub category ids
            await model.CatalogProductsCommand.SearchTermFilter.PrepareSubCategoryIdsSearch(subCategoryIdArray);
            //vendor ids
            await model.CatalogProductsCommand.SearchTermFilter.PrepareVendorIdsSearch(vendorIdArray);
            //manufacturer ids
            await model.CatalogProductsCommand.SearchTermFilter.PrepareManufacturerIdsSearch(vendorIdArray);

            var isFiltering = false;
            if (!string.IsNullOrEmpty(categoryId) || !string.IsNullOrEmpty(vendorIdArray) ||
                !string.IsNullOrEmpty(manufacturerIdArray) || command.SpecificationOptionIds != null || !string.IsNullOrEmpty(searchTerms))
                isFiltering = true;


            await PrepareCatalogProductsAsync(model, products, isFiltering);

            return model;
        }

        #endregion

        #region Vendor & its Products

        /// <summary>
        /// Prepares the vendor products model
        /// </summary>
        /// <param name="vendor">Vendor</param>
        /// <param name="command">Model to get the catalog products</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the vendor products model
        /// </returns>
        public virtual async Task<CatalogProductsModel> PrepareVendorProductsModelAsync(Vendor vendor, CatalogProductsCommand command)
        {
            if (vendor == null)
                throw new ArgumentNullException(nameof(vendor));

            if (command == null)
                throw new ArgumentNullException(nameof(command));

            var model = new CatalogProductsModel
            {
                UseAjaxLoading = _catalogSettings.UseAjaxCatalogProductsLoading
            };

            //sorting
            await PrepareSortingOptionsAsync(model, command);
            //view mode
            await PrepareViewModesAsync(model, command);
            //page size
            await PreparePageSizeOptionsAsync(model, command, vendor.AllowCustomersToSelectPageSize,
                vendor.PageSizeOptions, vendor.PageSize);

            //price range
            PriceRangeModel selectedPriceRange = null;
            var store = await _storeContext.GetCurrentStoreAsync();
            if (_catalogSettings.EnablePriceRangeFiltering && vendor.PriceRangeFiltering)
            {
                selectedPriceRange = await GetConvertedPriceRangeAsync(command);

                PriceRangeModel availablePriceRange;
                if (!vendor.ManuallyPriceRange)
                {
                    async Task<decimal?> getProductPriceAsync(ProductSortingEnum orderBy)
                    {
                        var products = await _productService.SearchProductsAsync(0, 1,
                            vendorId: vendor.Id,
                            storeId: store.Id,
                            visibleIndividuallyOnly: true,
                            orderBy: orderBy);

                        return products?.FirstOrDefault()?.Price ?? 0;
                    }

                    availablePriceRange = new PriceRangeModel
                    {
                        From = await getProductPriceAsync(ProductSortingEnum.PriceAsc),
                        To = await getProductPriceAsync(ProductSortingEnum.PriceDesc)
                    };
                }
                else
                {
                    availablePriceRange = new PriceRangeModel
                    {
                        From = vendor.PriceFrom,
                        To = vendor.PriceTo
                    };
                }

                model.PriceRangeFilter = await PreparePriceRangeFilterAsync(selectedPriceRange, availablePriceRange);
            }

            //products
            var products = await _productService.SearchProductsAsync(
                command.PageNumber - 1,
                command.PageSize,
                vendorId: vendor.Id,
                priceMin: selectedPriceRange?.From,
                priceMax: selectedPriceRange?.To,
                storeId: store.Id,
                visibleIndividuallyOnly: true,
                orderBy: (ProductSortingEnum)command.OrderBy,
                //master products only filter along with geoVendorIds
                isMaster: true,
                geoVendorIds: (await _workContext.GetCurrentCustomerGeoRadiusVendorIdsAsync()));

            var isFiltering = selectedPriceRange?.From is not null;
            await PrepareCatalogProductsAsync(model, products, isFiltering);

            return model;
        }

        #endregion

        #region Manufacturers

        /// <summary>
        /// Prepare manufacturers models
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the list of manufacturer models
        /// </returns>
        public virtual async Task<List<ManufacturerModel>> PrepareManufacturersModelsAsync(int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var store = await _storeContext.GetCurrentStoreAsync();
            var cacheKey = _staticCacheManager.PrepareKeyForDefaultCache(NopModelCacheDefaults.HomeManufacturersModelKey, store);
            var cachedModel = await _staticCacheManager.GetAsync(cacheKey, async () =>
            {
                var model = new List<ManufacturerModel>();

                var currentStore = await _storeContext.GetCurrentStoreAsync();
                var manufacturers = await _manufacturerService.GetAllManufacturersAsync(storeId: currentStore.Id, pageIndex: pageIndex, pageSize: pageSize);
                foreach (var manufacturer in manufacturers)
                {
                    var modelMan = new ManufacturerModel
                    {
                        Id = manufacturer.Id,
                        Name = await _localizationService.GetLocalizedAsync(manufacturer, x => x.Name),
                        Description = await _localizationService.GetLocalizedAsync(manufacturer, x => x.Description),
                        MetaKeywords = await _localizationService.GetLocalizedAsync(manufacturer, x => x.MetaKeywords),
                        MetaDescription = await _localizationService.GetLocalizedAsync(manufacturer, x => x.MetaDescription),
                        MetaTitle = await _localizationService.GetLocalizedAsync(manufacturer, x => x.MetaTitle),
                        SeName = await _urlRecordService.GetSeNameAsync(manufacturer),
                    };

                    //prepare picture model
                    var pictureSize = _mediaSettings.ManufacturerThumbPictureSize;
                    var manufacturerPictureCacheKey = _staticCacheManager.PrepareKeyForDefaultCache(NopModelCacheDefaults.ManufacturerPictureModelKey,
                        manufacturer, pictureSize, true, await _workContext.GetWorkingLanguageAsync(),
                        _webHelper.IsCurrentConnectionSecured(), currentStore);
                    modelMan.PictureModel = await _staticCacheManager.GetAsync(manufacturerPictureCacheKey, async () =>
                    {
                        var picture = await _pictureService.GetPictureByIdAsync(manufacturer.PictureId);
                        string fullSizeImageUrl, imageUrl;

                        (fullSizeImageUrl, picture) = await _pictureService.GetPictureUrlAsync(picture);
                        (imageUrl, _) = await _pictureService.GetPictureUrlAsync(picture, pictureSize);

                        var pictureModel = new PictureModel
                        {
                            FullSizeImageUrl = fullSizeImageUrl,
                            ImageUrl = imageUrl,
                            Title = string.Format(await _localizationService.GetResourceAsync("Media.Manufacturer.ImageLinkTitleFormat"), modelMan.Name),
                            AlternateText = string.Format(await _localizationService.GetResourceAsync("Media.Manufacturer.ImageAlternateTextFormat"), modelMan.Name)
                        };

                        return pictureModel;
                    });

                    model.Add(modelMan);
                }

                return model;
            });

            return cachedModel;
        }

        #endregion

        #region Elastic Search


        /// <summary>
        /// Prepare search model
        /// </summary>
        /// <param name="model">Search model</param>
        /// <param name="command">Model to get the catalog products</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the search model
        /// </returns>
        public virtual async Task<SearchModel> PrepareElasticSearchModelAsync(SearchModel model, CatalogProductsCommand command)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            if (command == null)
                throw new ArgumentNullException(nameof(command));

            model.CatalogProductsModel = await PrepareElasticCatalogModelAsync(model, command);

            return model;
        }

        /// <summary>
        /// Prepare catalog product model
        /// </summary>
        /// <param name="searchModel"></param>
        /// <param name="command"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public virtual async Task<CatalogProductsModel> PrepareElasticCatalogModelAsync(SearchModel searchModel, CatalogProductsCommand command)
        {
            var catmodel = new CatalogProductsModel
            {
                UseAjaxLoading = _catalogSettings.UseAjaxCatalogProductsLoading
            };

            //sorting
            await PrepareCustomSortingOptions(catmodel, command);
            //view mode
            await PrepareCustomViewModesAsync(catmodel, command);
            //page size
            await PrepareCustomPageSizeOptionsAsync(catmodel, command, _catalogSettings.SearchPageAllowCustomersToSelectPageSize,
                _catalogSettings.SearchPagePageSizeOptions, _catalogSettings.SearchPageProductsPerPage);

            //search terms of search page
            var searchKey = await catmodel.CatalogProductsCommand.SearchTermFilter.GetSearchTerm(_webHelper);
            if (!string.IsNullOrEmpty(searchKey))
                searchModel.q = searchKey;

            var searchTerms = searchModel.q == null
                ? string.Empty
                : searchModel.q.Trim();

            var currentStore = await _storeContext.GetCurrentStoreAsync();
            var categoryId = await catmodel.CatalogProductsCommand.SearchTermFilter.GetCategoryIds(_webHelper);
            var subCategoryIdArray = await catmodel.CatalogProductsCommand.SearchTermFilter.GetSubCategoryId(_webHelper);
            var vendorIdArray = await catmodel.CatalogProductsCommand.SearchTermFilter.GetVendorIds(_webHelper);
            var manufacturerIdArray = await catmodel.CatalogProductsCommand.SearchTermFilter.GetManufacturerIds(_webHelper);
            var price = await catmodel.CatalogProductsCommand.SearchTermFilter.GetPriceRange(_webHelper);
            var categoryIds = new List<int>();

            //prepare categoryIds related param
            var selectedCategoryId = string.IsNullOrEmpty(categoryId) ? 0 : Convert.ToInt32(categoryId);
            if (selectedCategoryId > 0)
                categoryIds.Add(selectedCategoryId);

            if (selectedCategoryId > 0 && !string.IsNullOrEmpty(subCategoryIdArray))
            {
                categoryIds.AddRange(subCategoryIdArray.Split(',').Select(int.Parse).ToList());
            }
            else
            {
                //include subcategories
                if (_catalogSettings.ShowProductsFromSubcategories)
                {
                    var tempCategoryIds = new List<int>();
                    foreach (var id in categoryIds)
                    {
                        tempCategoryIds.AddRange(await _categoryService.GetChildCategoryIdsAsync(id, currentStore.Id));
                    }
                    categoryIds.AddRange(tempCategoryIds);
                }
            }
            categoryIds = categoryIds.Distinct().ToList();

            //assign categoryIds to make filter check work correctly
            var filteredCategoryIds = new List<int>();
            filteredCategoryIds.AddRange(categoryIds);

            //prepare final category to be included in search query
            var allCategories = await _categoryService.GetAllCategoriesAsync(storeId: currentStore.Id);
            if (selectedCategoryId > 0 && !string.IsNullOrEmpty(subCategoryIdArray) && _catalogSettings.ShowProductsFromSubcategories)
            {
                var removeCategoryIds = new List<int>();
                if (!string.IsNullOrEmpty(subCategoryIdArray))
                    removeCategoryIds.Add(selectedCategoryId);

                foreach (var id in categoryIds)
                {
                    var category = allCategories.FirstOrDefault(x => x.Id == id);
                    if (category != null)
                    {
                        if (category.ParentCategoryId > 0 && !removeCategoryIds.Contains(category.ParentCategoryId))
                            removeCategoryIds.Add(category.ParentCategoryId);
                    }
                }
                //flush category ids
                if (removeCategoryIds.Contains(0))
                    removeCategoryIds.Remove(0);

                removeCategoryIds = removeCategoryIds.Distinct().ToList();

                //final category ids
                categoryIds.RemoveAll(x => removeCategoryIds.Contains(x));

                //add chile categories for option
                //include subcategories
                if (_catalogSettings.ShowProductsFromSubcategories)
                {
                    var tempCategoryIds = new List<int>();
                    foreach (var id in categoryIds)
                    {
                        tempCategoryIds.AddRange(await _categoryService.GetChildCategoryIdsAsync(id, currentStore.Id));
                    }
                    categoryIds.AddRange(tempCategoryIds);
                }
            }

            //prepare vendors param
            var vendorIds = new List<int>();
            var selectedVendorIds = new List<int>();
            if (!string.IsNullOrEmpty(vendorIdArray))
            {
                //vendorIds.AddRange(vendorIdArray.Split(',').Select(int.Parse).ToList());
                selectedVendorIds.AddRange(vendorIdArray.Split(',').Select(int.Parse).ToList());
            }
                var availableVendorIds = await _workContext.GetCurrentCustomerGeoRadiusVendorIdsAsync();
                vendorIds.AddRange(availableVendorIds);

            //prepare manufacturers param
            var manufacturerIds = new List<int>();
            if (_catalogSettings.EnableManufacturerFiltering)
            {
                if (!string.IsNullOrEmpty(manufacturerIdArray))
                    manufacturerIds.AddRange(manufacturerIdArray.Split(',').Select(int.Parse).ToList());
            }

            //price range param
            PriceRangeModel selectedPriceRange = null;
            if (_catalogSettings.EnablePriceRangeFiltering && _catalogSettings.SearchPagePriceRangeFiltering)
            {
                if (!string.IsNullOrEmpty(price))
                {
                    command.Price = price;
                    selectedPriceRange = await GetConvertedPriceRangeAsync(command);
                }
                else
                    selectedPriceRange = await GetConvertedPriceRangeAsync(command);

                PriceRangeModel availablePriceRange;
                //default
                availablePriceRange = new PriceRangeModel
                {
                    From = _catalogSettings.SearchPagePriceFrom,
                    To = _catalogSettings.SearchPagePriceTo
                };
                catmodel.PriceRangeFilter = await PreparePriceRangeFilterAsync(selectedPriceRange, availablePriceRange);
            }
            //association product ids
            var associationProductIds = await _elasticsearchManager.GetAssociateProductsListViaElasticSearchAsync(
                searchModel.q,
                storeId: currentStore.Id,
                visibleIndividuallyOnly: true,
                isMaster: true,
                categoryIds,
                manufacturerIds,
                vendorIds,
                selectedVendorIds,
                command.SpecificationOptionIds,
                priceMin: selectedPriceRange?.From,
                priceMax: selectedPriceRange?.To,
                parentCategoryId: selectedCategoryId);

            //get paged products and elastic result data
            var (products, elasticResult) = await _elasticsearchManager.SearchProducts(
                searchModel.q,
                command.PageNumber,
                command.PageSize,
                storeId: currentStore.Id,
                visibleIndividuallyOnly: true,
                isMaster: true,
                categoryIds,
                manufacturerIds,
                vendorIds,
                selectedVendorIds,
                command.SpecificationOptionIds,
                priceMin: selectedPriceRange?.From,
                priceMax: selectedPriceRange?.To,
                orderBy: (ProductSortingEnum)command.OrderBy,
                parentCategoryId: selectedCategoryId,
                associateProductIds: associationProductIds);

            //set paged param
            catmodel.CatalogProductsCommand.LoadPagedList(products);

            //search term statistics
            if (!string.IsNullOrEmpty(searchTerms))
            {
                var searchTerm =
                    await _searchTermService.GetSearchTermByKeywordAsync(searchTerms, currentStore.Id);
                if (searchTerm != null)
                {
                    searchTerm.Count++;
                    await _searchTermService.UpdateSearchTermAsync(searchTerm);
                }
                else
                {
                    searchTerm = new SearchTerm
                    {
                        Keyword = searchTerms,
                        StoreId = currentStore.Id,
                        Count = 1
                    };
                    await _searchTermService.InsertSearchTermAsync(searchTerm);
                }
            }

            //category ids
            await catmodel.CatalogProductsCommand.SearchTermFilter.PrepareCategoryIdsSearch(categoryId);
            //sub category ids
            await catmodel.CatalogProductsCommand.SearchTermFilter.PrepareSubCategoryIdsSearch(subCategoryIdArray);
            //vendor ids
            await catmodel.CatalogProductsCommand.SearchTermFilter.PrepareVendorIdsSearch(vendorIdArray);
            //manufacturer ids
            await catmodel.CatalogProductsCommand.SearchTermFilter.PrepareManufacturerIdsSearch(vendorIdArray);

            var isFiltering = false;
            if (!string.IsNullOrEmpty(categoryId) || !string.IsNullOrEmpty(vendorIdArray) ||
                !string.IsNullOrEmpty(manufacturerIdArray) || command.SpecificationOptionIds != null || !string.IsNullOrEmpty(searchTerms))
                isFiltering = true;

            //Prepare catalog products
            await PrepareCatalogProductsAsync(catmodel, products, isFiltering);

            //category filter - from elastic result
            await PrepareCategoryFilterModelFromElasticResult(catmodel, elasticResult, filteredCategoryIds);

            //vendor filter - from elastic result
            await PrepareVendorFilterModelFromElasticResult(catmodel, elasticResult, selectedVendorIds);

            //manufacturer filter - from elastic result
            await PrepareManufacturerFilterModelFromElasticResult(catmodel, elasticResult, manufacturerIds);

            //specification attributes filter - from elastic result
            await PrepareSpecificationFilterModelFromElasticResult(catmodel, elasticResult, command.SpecificationOptionIds);

            //Note: Price filter is handled on view page.

            return catmodel;
        }

        /// <summary>
        /// Search product Via Elastic search
        /// </summary>
        /// <param name="keyword"></param>
        /// <returns>List of Product search overview model</returns>
        public virtual async Task<List<ProductSearchOverviewModel>> PrepareProductSearchOverviewModelsElasticAsync(string keyword, int pageSize = int.MaxValue, bool visibleIndividuallyOnly = false,
           bool isMaster = true, int languageId = 0, IList<int> geoVendorIds = null)
        {
            //association product ids
            var associationProductIds = await _elasticsearchManager.GetAssociateProductIdsListViaElasticSearchAsync(keyword, pageSize, visibleIndividuallyOnly, isMaster, languageId, geoVendorIds);
            //get products using elastic
            var products = await _elasticsearchManager.GetSearchAutoCompleteProductsAsync(keyword, pageSize, visibleIndividuallyOnly, isMaster, languageId, geoVendorIds, associationProductIds);

            var listModel = new List<ProductSearchOverviewModel>();

            foreach (var product in products)
            {
                var productModel = new ProductSearchOverviewModel
                {
                    Id = product.Product.Id,
                    Name = product.Product.Name,
                    SeName = product.SeName,
                    DefaultPictureModel = new PictureModel
                    {
                        ImageUrl = product.Product.ImageUrl
                    }
                };

                listModel.Add(productModel);
            }

            return listModel;
        }

        #region Filter models

        /// <summary>
        /// Prepare category filter model from elastic result
        /// </summary>
        /// <param name="catModel"></param>
        /// <param name="elasticResponseModel"></param>
        /// <param name="selectedCategoriesIds"></param>
        /// <returns></returns>
        private async Task PrepareCategoryFilterModelFromElasticResult(CatalogProductsModel catModel, Master_products_result elasticResponseModel, IList<int> selectedCategoriesIds)
        {
            //fetch categories from elastic result
            var parentCategories = elasticResponseModel.ParentCategoryList;
            var subCategories = elasticResponseModel.SubCategoryList;

            var model = new CategoryFilterModel();

            if (parentCategories?.Any() == true)
            {
                //order by
                parentCategories = parentCategories.OrderBy(c => c.Category.DisplayOrder).ToList();

                model.Enabled = true;

                var workingLanguage = await _workContext.GetWorkingLanguageAsync();
                var numberOfProducts = new int?();
                var currentStore = await _storeContext.GetCurrentStoreAsync();

                foreach (var categoryDetail in parentCategories)
                {
                    var category = categoryDetail.Category;
                    if (category != null && category.ParentCategoryId == 0)
                    {
                        if (_catalogSettings.ShowCategoryProductNumber)
                            numberOfProducts = Convert.ToInt32(categoryDetail.TotalDocuments);

                        model.Categories.Add(new CategoryModel
                        {
                            Id = category.Id,
                            Name = await _localizationService
                                .GetLocalizedAsync(category, x => x.Name, workingLanguage.Id),
                            IsSelected = selectedCategoriesIds?
                                .Any(categoryId => categoryId == category.Id) == true,
                            NumberOfProducts = numberOfProducts,
                            DisplayOrder = category.DisplayOrder
                        });
                    }
                }

                if (subCategories?.Any() == true)
                {
                    //order by
                    subCategories = subCategories.OrderBy(c => c.Category.DisplayOrder).ToList();

                    foreach (var subCategoryDetail in subCategories)
                    {
                        var subCategory = subCategoryDetail.Category;
                        if (subCategory != null)
                        {
                            int? subCategoryProductCount = new int?();
                            if (_catalogSettings.ShowCategoryProductNumber)
                                subCategoryProductCount = Convert.ToInt32(subCategoryDetail.TotalDocuments);

                            model.SubCategories.Add(new CategoryModel
                            {
                                Id = subCategory.Id,
                                Name = await _localizationService
                                    .GetLocalizedAsync(subCategory, x => x.Name, workingLanguage.Id),
                                NumberOfProducts = subCategoryProductCount,
                                ParentCategryId = subCategory.ParentCategoryId,
                                DisplayOrder = subCategory.DisplayOrder
                            });
                        }
                    }
                }

                ////child category - not required
                //foreach (var item in model.SubCategories)
                //{
                //    var childSubCategories = await _categoryService.GetAllCategoriesByParentCategoryIdAsync(item.Id);
                //    foreach (var subCategory in childSubCategories)
                //    {
                //        int? childCategoryProductCount = new int?();
                //        if (_catalogSettings.ShowCategoryProductNumber)
                //        {
                //            var categoryIds = new List<int> { subCategory.Id };
                //            //include subcategories
                //            if (_catalogSettings.ShowCategoryProductNumberIncludingSubcategories)
                //                categoryIds.AddRange(
                //                    await _categoryService.GetChildCategoryIdsAsync(subCategory.Id, currentStore.Id));

                //            childCategoryProductCount = await _productService.GetNumberOfProductsInCategoryByGeoVendorIdsAsync(categoryIds, currentStore.Id,
                //                      manufacturerIds, vendorIds, priceMin, priceMax, keywords, filteredSpecOptions, true);
                //        }

                //        model.ChildCategories.Add(new CategoryModel
                //        {
                //            Id = subCategory.Id,
                //            Name = await _localizationService
                //                .GetLocalizedAsync(subCategory, x => x.Name, workingLanguage.Id),
                //            NumberOfProducts = childCategoryProductCount,
                //            ParentCategryId = item.Id,
                //            DisplayOrder = subCategory.DisplayOrder
                //        });
                //    }
                //}
            }

            catModel.CategoryFilterModel = model;

            await Task.CompletedTask;
        }

        /// <summary>
        /// Prepare vendor filter model from elastic result
        /// </summary>
        /// <param name="catmodel"></param>
        /// <param name="elasticResponseModel"></param>
        /// <param name="selectedVendorIds"></param>
        /// <returns></returns>
        private async Task PrepareVendorFilterModelFromElasticResult(CatalogProductsModel catmodel, Master_products_result elasticResponseModel, IList<int> selectedVendorIds)
        {
            //prepare vendors filter
            var availableVendors = elasticResponseModel?.Vendors?.ToList();
            foreach (var vendorDetail in availableVendors)
            {
                var vendor = vendorDetail?.Vendor;
                if (vendor != null)
                {
                    catmodel.Vendors.Add(new SelectListItem
                    {
                        Value = Convert.ToString(vendor.Id),
                        Text = vendor.Name,
                        Selected = selectedVendorIds?.Any(id => id == vendor.Id) == true,
                    });

                    //product count
                    catmodel.VendorsProductCounts.Add(new CatalogProductsModel.ProductCountCommonModel
                    {
                        Id = vendor.Id,
                        NumberOfProducts = Convert.ToInt32(vendorDetail.TotalDocuments)
                    });
                }
            }

            await Task.CompletedTask;
        }

        /// <summary>
        /// Prepare manufacturers filter model from elastic result
        /// </summary>
        /// <param name="catmodel"></param>
        /// <param name="elasticResponseModel"></param>
        /// <param name="selectedManufacturerds"></param>
        /// <returns></returns>
        private async Task PrepareManufacturerFilterModelFromElasticResult(CatalogProductsModel catmodel, Master_products_result elasticResponseModel, IList<int> selectedManufacturerds)
        {
            //prepare manufacturers filter
            if (_catalogSettings.EnableManufacturerFiltering)
            {
                var manufacturers = elasticResponseModel?.Manufacturers;
                if (manufacturers?.Any() == true)
                {
                    //distinct
                    manufacturers = manufacturers.DistinctBy(m => m.Manufacturer.Id).ToList();

                    var model = new ManufacturerFilterModel
                    {
                        Enabled = true
                    };

                    var workingLanguage = await _workContext.GetWorkingLanguageAsync();

                    foreach (var manufacturerDetail in manufacturers)
                    {
                        if (manufacturerDetail.Manufacturer != null)
                        {
                            var manufacturer = manufacturerDetail.Manufacturer;
                            model.Manufacturers.Add(new SelectListItem
                            {
                                Value = manufacturer.Id.ToString(),
                                Text = await _localizationService
                                .GetLocalizedAsync(manufacturer, x => x.Name, workingLanguage.Id),
                                Selected = selectedManufacturerds?
                                .Any(manufacturerId => manufacturerId == manufacturer.Id) == true
                            });

                            //product count
                            catmodel.ManufacturersProductCounts.Add(new CatalogProductsModel.ProductCountCommonModel
                            {
                                Id = manufacturer.Id,
                                NumberOfProducts = Convert.ToInt32(manufacturerDetail.TotalDocuments)
                            });
                        }
                    }

                    catmodel.ManufacturerFilter = model;
                }
            }

            await Task.CompletedTask;
        }

        /// <summary>
        /// Prepare specification attributes filter model from elastic result
        /// </summary>
        /// <param name="catModel"></param>
        /// <param name="elasticResponseModel"></param>
        /// <param name="selectedOptionsIds"></param>
        /// <returns></returns>
        private async Task PrepareSpecificationFilterModelFromElasticResult(CatalogProductsModel catModel, Master_products_result elasticResponseModel, IList<int> selectedOptionsIds)
        {
            //specification filter
            if (_catalogSettings.EnableSpecificationAttributeFiltering)
            {
                //fethch specAtt from elastic result
                var specificationsAttribuets = elasticResponseModel?.Specifications;
                if (specificationsAttribuets?.Any() == true)
                {
                    var model = new SpecificationFilterModel
                    {
                        Enabled = true
                    };

                    var workingLanguage = await _workContext.GetWorkingLanguageAsync();

                    //order by
                    specificationsAttribuets = specificationsAttribuets
                                                    ?.DistinctBy(x => x.SpecificationAttribute.Id)
                                                    ?.OrderBy(s => s.SpecificationAttribute?.DisplayOrder).ToList();

                    foreach (var attributeDetail in specificationsAttribuets)
                    {
                        if (attributeDetail?.SpecificationAttribute != null)
                        {
                            var attribute = attributeDetail.SpecificationAttribute;
                            var attributeFilter = new SpecificationAttributeFilterModel
                            {
                                Id = attribute.Id,
                                Name = await _localizationService
                                        .GetLocalizedAsync(attribute, x => x.Name, workingLanguage.Id)
                            };
                            model.Attributes.Add(attributeFilter);

                            //prepare options
                            foreach (var optionDetail in attributeDetail?.SpecificationAttributeOptionDetails)
                            {
                                if (optionDetail?.SpecificationAttributeOption != null)
                                {
                                    var option = optionDetail.SpecificationAttributeOption;
                                    attributeFilter.Values.Add(new SpecificationAttributeValueFilterModel
                                    {
                                        Id = option.Id,
                                        Name = await _localizationService
                                                     .GetLocalizedAsync(option, x => x.Name, workingLanguage.Id),
                                        Selected = selectedOptionsIds?.Any(optionId => optionId == option.Id) == true,
                                        ColorSquaresRgb = option.ColorSquaresRgb
                                    });

                                    //product count
                                    catModel.SpecificationOptionsProductCounts.Add(new CatalogProductsModel.ProductCountCommonModel
                                    {
                                        Id = option.Id,
                                        NumberOfProducts = Convert.ToInt32(optionDetail.TotalDocuments)
                                    });
                                }
                            }
                        }
                    }

                    catModel.SpecificationFilter = model;
                }
            }

            await Task.CompletedTask;
        }

        #endregion

        #endregion Elastic Search
    }
}
