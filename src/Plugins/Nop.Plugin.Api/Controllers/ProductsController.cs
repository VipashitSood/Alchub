using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Discounts;
using Nop.Core.Domain.Slots;
using Nop.Plugin.Api.Attributes;
using Nop.Plugin.Api.Authorization.Attributes;
using Nop.Plugin.Api.Delta;
using Nop.Plugin.Api.DTO.Errors;
using Nop.Plugin.Api.DTO.Images;
using Nop.Plugin.Api.DTO.Products;
using Nop.Plugin.Api.DTOs.Products;
using Nop.Plugin.Api.Factories;
using Nop.Plugin.Api.Helpers;
using Nop.Plugin.Api.Infrastructure;
using Nop.Plugin.Api.JSON.ActionResults;
using Nop.Plugin.Api.JSON.Serializers;
using Nop.Plugin.Api.ModelBinders;
using Nop.Plugin.Api.Models.BaseModels;
using Nop.Plugin.Api.Models.ProductsParameters;
using Nop.Plugin.Api.Models.Slots;
using Nop.Plugin.Api.Models.V1.Catalog.Filter;
using Nop.Plugin.Api.Services;
using Nop.Services.Alchub.ElasticSearch;
using Nop.Services.Alchub.General;
using Nop.Services.Authentication;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Discounts;
using Nop.Services.Helpers;
using Nop.Services.Html;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Slots;
using Nop.Services.Stores;
using Nop.Services.Vendors;

namespace Nop.Plugin.Api.Controllers
{
    public class ProductsController : BaseApiController
    {
        private readonly IDTOHelper _dtoHelper;
        private readonly IStoreContext _storeContext;
        private readonly IWorkContext _workContext;
        private readonly IFactory<Product> _factory;
        private readonly IManufacturerService _manufacturerService;
        private readonly IProductApiService _productApiService;
        private readonly IProductAttributeService _productAttributeService;
        private readonly IProductService _productService;
        private readonly IProductTagService _productTagService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly CatalogSettings _catalogSettings;
        private readonly IHtmlFormatter _htmlFormatter;
        private readonly IProductModelFactory _productModelFactory;
        private readonly ICompareProductsService _compareProductsService;
        private readonly Nop.Services.Messages.IWorkflowMessageService _defaultWorkflowMessageService;
        private readonly ISpecificationAttributeService _specificationAttributeService;
        private readonly IAuthenticationService _authenticationService;
        private readonly IVendorService _vendorService;
        private readonly ISettingService _settingService;
        private readonly ISlotService _slotService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly ICategoryService _categoryService;
        private readonly ILogger _logger;
        private readonly IAlchubGeneralService _alchubGeneralService;
        private readonly Nop.Plugin.Api.Factories.V1.ICatalogModelFactory _catalogModelFactoryV1;
        IElasticSearchProductService _elasticSearchProductService;
        private readonly IElasticsearchManagerService _elasticsearchManager;

        public ProductsController(
            IProductApiService productApiService,
            IJsonFieldsSerializer jsonFieldsSerializer,
            IProductService productService,
            IUrlRecordService urlRecordService,
            ICustomerActivityService customerActivityService,
            ILocalizationService localizationService,
            IHtmlFormatter htmlFormatter,
            IFactory<Product> factory,
            IAclService aclService,
            IStoreMappingService storeMappingService,
            IStoreService storeService,
            IProductModelFactory productModelFactory,
            ICompareProductsService compareProductsService,
            ICustomerService customerService,
            IDiscountService discountService,
            IPictureService pictureService,
            IManufacturerService manufacturerService,
            IProductTagService productTagService,
            IProductAttributeService productAttributeService,
            IDTOHelper dtoHelper,
            IStoreContext storeContext,
            IWorkContext workContext,
            Nop.Services.Messages.IWorkflowMessageService defaultWorkflowMessageService,
            CatalogSettings catalogSettings,
            ISpecificationAttributeService specificationAttributeService,
            IAuthenticationService authenticationService,
            IVendorService vendorService, ISettingService settingService, ISlotService slotService,
            IDateTimeHelper dateTimeHelper,
            IShoppingCartService shoppingCartService,
            ICategoryService categoryService,
            ILogger logger,
            IAlchubGeneralService alchubGeneralService,
            Nop.Plugin.Api.Factories.V1.ICatalogModelFactory catalogModelFactoryV1,
            IElasticSearchProductService elasticSearchProductService,
            IElasticsearchManagerService elasticsearchManager)
            : base(jsonFieldsSerializer,
                aclService,
                customerService,
                storeMappingService,
                storeService,
                discountService,
                customerActivityService,
                localizationService,
                pictureService)
        {
            _productApiService = productApiService;
            _factory = factory;
            _manufacturerService = manufacturerService;
            _productTagService = productTagService;
            _urlRecordService = urlRecordService;
            _htmlFormatter = htmlFormatter;
            _productService = productService;
            _productAttributeService = productAttributeService;
            _dtoHelper = dtoHelper;
            _storeContext = storeContext;
            _workContext = workContext;
            _compareProductsService = compareProductsService;
            _catalogSettings = catalogSettings;
            _defaultWorkflowMessageService = defaultWorkflowMessageService;
            _specificationAttributeService = specificationAttributeService;
            _authenticationService = authenticationService;
            _productModelFactory = productModelFactory;
            _authenticationService = authenticationService;
            _vendorService = vendorService;
            _settingService = settingService;
            _slotService = slotService;
            _dateTimeHelper = dateTimeHelper;
            _shoppingCartService = shoppingCartService;
            _categoryService = categoryService;
            _logger = logger;
            _alchubGeneralService = alchubGeneralService;
            _catalogModelFactoryV1 = catalogModelFactoryV1;
            _elasticSearchProductService = elasticSearchProductService;
            _elasticsearchManager = elasticsearchManager;
        }

        /// <summary>
        ///     Receive a list of all products
        /// </summary>
        /// <response code="200">OK</response>
        /// <response code="400">Bad Request</response>
        /// <response code="401">Unauthorized</response>
        [HttpGet]
        [Route("/api/products", Name = "GetProducts")]
        [AuthorizePermission("PublicStoreAllowNavigation")]
        [ProducesResponseType(typeof(ProductsRootObjectDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ErrorsRootObject), (int)HttpStatusCode.BadRequest)]
        [GetRequestsErrorInterceptorActionFilter]
        public async Task<BaseResponseModel> GetProducts([FromQuery] ProductsParametersModel parameters)
        {
            if (parameters.PageSize < Constants.Configurations.MIN_LIMIT || parameters.PageSize > Constants.Configurations.MAX_LIMIT)
                return ErrorResponse("invalid limit parameter", HttpStatusCode.BadRequest);

            if (parameters.PageSize < Constants.Configurations.DEFAULT_PAGE_VALUE)
                return ErrorResponse("invalid page parameter", HttpStatusCode.BadRequest);

            //current customer
            var customer = await _authenticationService.GetAuthenticatedCustomerAsync();
            if (customer is null)
                return ErrorResponse("Unauthorized! Customer not found", HttpStatusCode.Unauthorized);

            try
            {
                var store = await _storeContext.GetCurrentStoreAsync();

                //get products after filter
                var allProducts = await _productApiService.GetProductByIdsAsync(productIds: parameters.Ids,
                    pageIndex: parameters.PageIndex, pageSize: parameters.PageSize, storeId: store?.Id ?? 0);

                //prepare model & property values
                var productsListAsDtos = await allProducts.SelectAwait(async product => await _dtoHelper.PrepareProductListDTOAsync(product, customer)).ToListAsync();

                var productsListRootObject = new ProductsListRootObjectDto
                {
                    ProductsList = productsListAsDtos,
                    PageIndex = allProducts.PageIndex + 1,
                    PageSize = allProducts.PageSize,
                    TotalRecords = allProducts.TotalCount
                };

                return SuccessResponse(await _localizationService.GetResourceAsync("Nop.Api.ProductList"), productsListRootObject);
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        ///    Review of products
        /// </summary>
        /// <response code="200">OK</response>
        /// <response code="400">Bad Request</response>
        /// <response code="401">Unauthorized</response>
        [HttpGet]
        [Route("/api/products/review", Name = "AllReview")]
        [AuthorizePermission("PublicStoreAllowNavigation")]
        [ProducesResponseType(typeof(AllReviewDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ErrorsRootObject), (int)HttpStatusCode.BadRequest)]
        [GetRequestsErrorInterceptorActionFilter]
        public async Task<BaseResponseModel> GetAllReview([FromQuery] AllReviewModel allReviewModel)
        {

            //customer validate
            var customer = await _customerService.GetCustomerByIdAsync(allReviewModel.UserId);
            if (customer is null)
                return ErrorResponse("Customer not found", HttpStatusCode.BadRequest);
            try
            {
                var product = await _productService.GetProductByIdAsync(allReviewModel.ProductId);

                //get products after filter
                var allReview = await _productApiService.GetAllReview(allReviewModel.ProductId);

                return SuccessResponse(await _localizationService.GetResourceAsync("Nop.Api.AllReview"), allReview);
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        ///    Like and Dislike
        /// </summary>
        /// <response code="200">OK</response>
        /// <response code="400">Bad Request</response>
        /// <response code="401">Unauthorized</response>
        [HttpPost]
        [Route("/api/products/LikeDislike", Name = "LikeDislike")]
        [AuthorizePermission("PublicStoreAllowNavigation")]
        [ProducesResponseType(typeof(HelpfulnessDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ErrorsRootObject), 422)]
        [GetRequestsErrorInterceptorActionFilter]
        public virtual async Task<BaseResponseModel> SetProductReviewHelpfulness([FromBody] HelpfulnessModel ProductReviewDto)
        {
            if (ProductReviewDto.UserId == 0)
            {
                return ErrorResponse(await _localizationService.GetResourceAsync("Nop.Api.InvalidUserId"));
            }
            if (ProductReviewDto.ProductReviewId == 0)
            {
                return ErrorResponse(await _localizationService.GetResourceAsync("Nop.Api.InvalidReviewId"));
            }
            var customer = await _customerService.GetCustomerByIdAsync(ProductReviewDto.UserId);
            if (customer == null)
            {
                return ErrorResponse(await _localizationService.GetResourceAsync("Nop.Api.CustomerNotFound"));
            }
            try
            {
                var helpfulnessModel = await _productService.GetProductReviewByIdAsync(ProductReviewDto.ProductReviewId);
                if (helpfulnessModel == null)
                    return ErrorResponse("No product review found with the specified id", HttpStatusCode.NotFound);

                if (await _customerService.IsGuestAsync(customer) && !_catalogSettings.AllowAnonymousUsersToReviewProduct)
                {
                    return ErrorResponse(await _localizationService.GetResourceAsync("Nop.Api.OnlyRegistered"));
                }

                //customers aren't allowed to vote for their own reviews
                if (helpfulnessModel.CustomerId == customer.Id)
                {
                    return ErrorResponse(await _localizationService.GetResourceAsync("Nop.Api.YourOwnReview"));
                }

                await _productService.SetProductReviewHelpfulnessAsync(helpfulnessModel, ProductReviewDto.WasHelpfulness);

                //new totals
                await _productService.UpdateProductReviewHelpfulnessTotalsAsync(helpfulnessModel);

                var updatedReviews = await _productService.GetProductReviewByIdAsync(ProductReviewDto.ProductReviewId);
                HelpfulnessDto helpfulnessDto = new HelpfulnessDto();
                helpfulnessDto.LikeCount = updatedReviews.HelpfulYesTotal;
                helpfulnessDto.DislikeCount = updatedReviews.HelpfulNoTotal;
                helpfulnessDto.Message = await _localizationService.GetResourceAsync("Nop.Api.SuccessfullyVoted");

                return SuccessResponse(await _localizationService.GetResourceAsync("Nop.Api.LikeDislike"), helpfulnessDto);
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        ///    Add Review
        /// </summary>
        /// <response code="200">OK</response>
        /// <response code="400">Bad Request</response>
        /// <response code="401">Unauthorized</response>
        [HttpPost]
        [Route("/api/products/addreview", Name = "AddReview")]
        [AuthorizePermission("PublicStoreAllowNavigation")]
        [ProducesResponseType(typeof(AddReviewDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ErrorsRootObject), 422)]
        [GetRequestsErrorInterceptorActionFilter]
        public virtual async Task<BaseResponseModel> ProductReviewsAdd([FromBody] AddReviewModel model)
        {
            if (model.UserId == 0)
            {
                return ErrorResponse(await _localizationService.GetResourceAsync("Nop.Api.InvalidUserId"));
            }
            if (model.ProductId == 0)
            {
                return ErrorResponse(await _localizationService.GetResourceAsync("Nop.Api.InvalidProductId"));
            }

            //current customer
            var customer = await _authenticationService.GetAuthenticatedCustomerAsync();
            if (customer is null)
                return ErrorResponse("Unauthorized! Customer not found", HttpStatusCode.Unauthorized);

            try
            {
                var product = await _productService.GetProductByIdAsync(model.ProductId);
                var currentStore = await _storeContext.GetCurrentStoreAsync();

                if (product == null || product.Deleted || !product.Published || !product.AllowCustomerReviews ||
                    !await _productService.CanAddReviewAsync(product.Id, _catalogSettings.ShowProductReviewsPerStore ? currentStore.Id : 0))
                    return ErrorResponse(await _localizationService.GetResourceAsync("Nop.Api.ProductNotAvailable"));

                if (ModelState.IsValid)
                {
                    //save review
                    var rating = model.Rating;
                    if (rating < 1 || rating > 5)
                        rating = _catalogSettings.DefaultProductRatingValue;
                    var isApproved = !_catalogSettings.ProductReviewsMustBeApproved;

                    var productReview = new ProductReview
                    {
                        CustomerId = customer.Id,
                        ProductId = product.Id,
                        Title = model.Title,
                        ReviewText = model.Text,
                        Rating = rating,
                        StoreId = currentStore.Id,
                        IsApproved = true,
                        CreatedOnUtc = DateTime.UtcNow
                    };

                    await _productService.InsertProductReviewAsync(productReview);

                    AddReviewDto addReviewDto = new AddReviewDto();
                    addReviewDto.Message = await _localizationService.GetResourceAsync("Nop.Api.ReviewAddedSuccessfully");

                    return SuccessResponse(await _localizationService.GetResourceAsync("Nop.Api.ReviewAddedSuccessfully"));
                }
                return ErrorResponse("Request model is not valid", HttpStatusCode.BadRequest);

            }
            catch (Exception ex)
            {
                return ErrorResponse(ex.Message, HttpStatusCode.BadRequest);
            }
        }

        #region Email a friend

        /// <summary>
        ///    Email A Friend
        /// </summary>
        /// <response code="200">OK</response>
        /// <response code="400">Bad Request</response>
        /// <response code="401">Unauthorized</response>
        [HttpPost]
        [Route("/api/products/email_a_friend", Name = "EmailAFriend")]
        [AuthorizePermission("PublicStoreAllowNavigation")]
        [ProducesResponseType(typeof(ProductEmailAFriendDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ErrorsRootObject), 422)]
        [GetRequestsErrorInterceptorActionFilter]
        public virtual async Task<BaseResponseModel> ProductEmailAFriendSend([FromBody] ProductEmailAFriend model)
        {
            if (model.UserId == 0)
            {
                return ErrorResponse(await _localizationService.GetResourceAsync("Nop.Api.InvalidUserId"));
            }
            if (model.ProductId == 0)
            {
                return ErrorResponse(await _localizationService.GetResourceAsync("Nop.Api.InvalidProductId"));
            }

            //current customer
            var customer = await _authenticationService.GetAuthenticatedCustomerAsync();
            if (customer is null)
                return ErrorResponse("Unauthorized! Customer not found", HttpStatusCode.Unauthorized);

            try
            {
                var product = await _productService.GetProductByIdAsync(model.ProductId);
                if (product == null || product.Deleted || !product.Published || !_catalogSettings.EmailAFriendEnabled)
                    return ErrorResponse(await _localizationService.GetResourceAsync("Nop.Api.ProductNotAvailable"));

                if (ModelState.IsValid)
                {
                    //email
                    await _defaultWorkflowMessageService.SendProductEmailAFriendMessageAsync(customer,
                            (await _workContext.GetWorkingLanguageAsync()).Id, product,
                            model.YourEmailAddress, model.FriendEmailAddress,
                            _htmlFormatter.FormatText(model.PersonalMessage, false, true, false, false, false, false));


                    ProductEmailAFriendDto productEmailAFriendDto = new ProductEmailAFriendDto();
                    productEmailAFriendDto.Message = await _localizationService.GetResourceAsync("Nop.Api.MessageSentSuccessfully");

                    return SuccessResponse(await _localizationService.GetResourceAsync("Nop.Api.MessageSentSuccessfully"), productEmailAFriendDto);
                }
                return ErrorResponse("Request model is not valid", HttpStatusCode.BadRequest);
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex.Message, HttpStatusCode.BadRequest);
            }
        }

        #endregion

        ///// <summary>
        /////     Receive a list of all products
        ///// </summary>
        ///// <response code="200">OK</response>
        ///// <response code="400">Bad Request</response>
        ///// <response code="401">Unauthorized</response>
        //[HttpGet]
        //[Route("/api/products/list", Name = "GetProductsList")]
        //[AuthorizePermission("PublicStoreAllowNavigation")]
        //[ProducesResponseType(typeof(ProductsRootObjectDto), (int)HttpStatusCode.OK)]
        //[ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        //[ProducesResponseType(typeof(ErrorsRootObject), (int)HttpStatusCode.BadRequest)]
        //[GetRequestsErrorInterceptorActionFilter]
        //public async Task<BaseResponseModel> GetProductsList([FromQuery] ProductsListModel productsListModel)
        //{
        //    if (productsListModel.PageSize < Constants.Configurations.MIN_LIMIT || productsListModel.PageSize > Constants.Configurations.MAX_LIMIT)
        //        return ErrorResponse("invalid limit parameter", HttpStatusCode.BadRequest);

        //    if (productsListModel.PageSize < Constants.Configurations.DEFAULT_PAGE_VALUE)
        //        return ErrorResponse("invalid page parameter", HttpStatusCode.BadRequest);

        //    //customer validate
        //    var customer = await _customerService.GetCustomerByIdAsync(productsListModel.CustomerId);
        //    if (customer == null)
        //        return ErrorResponse("Customer not found", HttpStatusCode.BadRequest);

        //    try
        //    {
        //        decimal? from = null;
        //        decimal? to = null;
        //        if (productsListModel.From != null && productsListModel.To != null)
        //        {
        //            from = Convert.ToDecimal(productsListModel.From);
        //            to = Convert.ToDecimal(productsListModel.To);
        //        }

        //        var store = await _storeContext.GetCurrentStoreAsync();

        //        //prepar categoryIds
        //        var categoryIds = new List<int>();
        //        var selectedCategoryId = productsListModel.CategoryId;
        //        if (selectedCategoryId > 0)
        //            categoryIds.Add(selectedCategoryId);

        //        if (selectedCategoryId > 0 && productsListModel.SubCategoryIds != null && productsListModel.SubCategoryIds.Any())
        //        {
        //            categoryIds.AddRange(productsListModel.SubCategoryIds);
        //        }
        //        else
        //        {
        //            //include subcategories
        //            if (_catalogSettings.ShowProductsFromSubcategories)
        //            {
        //                var tempCategoryIds = new List<int>();
        //                foreach (var id in categoryIds)
        //                {
        //                    tempCategoryIds.AddRange(await _categoryService.GetChildCategoryIdsAsync(id, store.Id));
        //                }
        //                categoryIds.AddRange(tempCategoryIds);
        //            }
        //        }
        //        categoryIds = categoryIds.Distinct().ToList();

        //        //prepare final category to be included in search query
        //        var allCategories = await _categoryService.GetAllCategoriesAsync(storeId: store.Id);
        //        if (selectedCategoryId > 0 && productsListModel.SubCategoryIds != null && productsListModel.SubCategoryIds.Any() && _catalogSettings.ShowProductsFromSubcategories)
        //        {
        //            var removeCategoryIds = new List<int>();
        //            if (productsListModel.SubCategoryIds != null && productsListModel.SubCategoryIds.Any())
        //                removeCategoryIds.Add(selectedCategoryId);

        //            foreach (var id in categoryIds)
        //            {
        //                var category = allCategories.FirstOrDefault(x => x.Id == id);
        //                if (category != null)
        //                {
        //                    if (category.ParentCategoryId > 0 && !removeCategoryIds.Contains(category.ParentCategoryId))
        //                        removeCategoryIds.Add(category.ParentCategoryId);
        //                }
        //            }
        //            //flush category ids
        //            if (removeCategoryIds.Contains(0))
        //                removeCategoryIds.Remove(0);

        //            removeCategoryIds = removeCategoryIds.Distinct().ToList();

        //            //final category ids
        //            categoryIds.RemoveAll(x => removeCategoryIds.Contains(x));

        //            //add chile categories for option
        //            //include subcategories
        //            if (_catalogSettings.ShowProductsFromSubcategories)
        //            {
        //                var tempCategoryIds = new List<int>();
        //                foreach (var id in categoryIds)
        //                {
        //                    tempCategoryIds.AddRange(await _categoryService.GetChildCategoryIdsAsync(id, store.Id));
        //                }
        //                categoryIds.AddRange(tempCategoryIds);
        //            }
        //        }

        //        //spec filter options
        //        List<SpecificationAttributeOption> filteredSpecs = null;
        //        if (productsListModel.SpecificationOptionIds is not null)
        //        {
        //            var filterableOptions = await _specificationAttributeService.GetFiltrableSpecificationAttributeOptionsByCategoryIdAsync(productsListModel.CategoryId);
        //            filteredSpecs = filterableOptions.Where(fo => productsListModel.SpecificationOptionIds.Contains(fo.Id)).ToList();
        //        }

        //        //get products after filter
        //        var allProducts = await _productApiService.GetProductsList(userId: productsListModel.CustomerId, storeId: store?.Id ?? 0, categoryIds: categoryIds, productType: productsListModel.ProductType,
        //            pageIndex: productsListModel.PageIndex, pageSize: productsListModel.PageSize, keywords: productsListModel.Keyword, priceMin: from, priceMax: to, orderBy: productsListModel.OrderBy > 0 ? (ProductSortingEnum)productsListModel.OrderBy : ProductSortingEnum.Position, manufacturerIds: productsListModel.ManufacturerIds,
        //        filteredSpecOptions: filteredSpecs,
        //        vendorIds: productsListModel.VendorIds);

        //        //prepare model & property values
        //        var productsListAsDtos = await allProducts.SelectAwait(async product => await _dtoHelper.PrepareProductListDTOAsync(product, customer)).ToListAsync();

        //        var productsListRootObject = new ProductsListRootObjectDto
        //        {
        //            ProductsList = productsListAsDtos,
        //            PageIndex = allProducts.PageIndex + 1,
        //            PageSize = allProducts.PageSize,
        //            TotalRecords = allProducts.TotalCount
        //        };

        //        return SuccessResponse(await _localizationService.GetResourceAsync("Nop.Api.ProductList"), productsListRootObject);
        //    }
        //    catch (Exception ex)
        //    {
        //        await _logger.ErrorAsync(ex.Message, ex);
        //        return ErrorResponse(ex.Message, HttpStatusCode.BadRequest);
        //    }
        //}

        ///// <summary>
        /////     Receive a list of all products
        ///// </summary>
        ///// <response code="200">OK</response>
        ///// <response code="400">Bad Request</response>
        ///// <response code="401">Unauthorized</response>
        //[HttpGet]
        //[Route("/api/products/search", Name = "API_SearchTermAutoComplete")]
        //[AuthorizePermission("PublicStoreAllowNavigation")]
        //[ProducesResponseType(typeof(ProductsRootObjectDto), (int)HttpStatusCode.OK)]
        //[ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        //[ProducesResponseType(typeof(ErrorsRootObject), (int)HttpStatusCode.BadRequest)]
        //[GetRequestsErrorInterceptorActionFilter]
        //public async Task<BaseResponseModel> ProductSearchTermAutoComplete([FromQuery] string term, int pageSize)
        //{
        //    if (string.IsNullOrEmpty(term))
        //        return ErrorResponse("invalid term parameter", HttpStatusCode.BadRequest);

        //    term = term.Trim();

        //    if (string.IsNullOrWhiteSpace(term) || term.Length < _catalogSettings.ProductSearchTermMinimumLength)
        //        return ErrorResponse($"term parameter length should be {_catalogSettings.ProductSearchTermMinimumLength} or more", HttpStatusCode.BadRequest);

        //    //current customer
        //    var customer = await _authenticationService.GetAuthenticatedCustomerAsync();
        //    if (customer is null)
        //        return ErrorResponse("Unauthorized! Customer not found", HttpStatusCode.Unauthorized);

        //    try
        //    {
        //        //prepare required param
        //        var productNumber = _catalogSettings.ProductSearchAutoCompleteNumberOfProducts > 0 ?
        //            _catalogSettings.ProductSearchAutoCompleteNumberOfProducts : 10;
        //        if (pageSize > 0)
        //            productNumber = pageSize;

        //        var store = await _storeContext.GetCurrentStoreAsync();

        //        //products
        //        var products = await _productService.SearchProductsAsync(0,
        //            storeId: store.Id,
        //            keywords: term,
        //            visibleIndividuallyOnly: true,
        //            pageSize: productNumber,
        //            //master products only filter along with geoVendorIds
        //            isMaster: true,
        //            geoVendorIds: (await _workContext.GetCurrentCustomerGeoRadiusVendorIdsAsync(customer)),
        //            //exclude other search exept product name
        //            searchDescriptions: false,
        //            searchManufacturerPartNumber: false,
        //            searchSku: false,
        //            searchProductTags: false,
        //            languageId: 0);

        //        //prepare search result dtos
        //        var searchResultDtos = await products.SelectAwait(async product => await _dtoHelper.PrepareProductSearchResultDTOAsync(product, customer, true)).ToListAsync();
        //        var productsListRootObject = new ProductsSearchResultRootDto
        //        {
        //            ProductsList = searchResultDtos,
        //            PageIndex = products.PageIndex + 1,
        //            PageSize = products.PageSize,
        //            TotalRecords = products.TotalCount
        //        };

        //        return SuccessResponse("Product search result", productsListRootObject);
        //    }
        //    catch (Exception ex)
        //    {
        //        await _logger.ErrorAsync(ex.Message, ex);
        //        return ErrorResponse(ex.Message, HttpStatusCode.BadRequest);
        //    }
        //}

        #region Elastic Search

        /// <summary>
        ///     Receive a list of all products
        /// </summary>
        /// <response code="200">OK</response>
        /// <response code="400">Bad Request</response>
        /// <response code="401">Unauthorized</response>
        [HttpGet]
        [Route("/api/products/list", Name = "GetProductsList")]
        [AuthorizePermission("PublicStoreAllowNavigation")]
        [ProducesResponseType(typeof(ProductsRootObjectDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ErrorsRootObject), (int)HttpStatusCode.BadRequest)]
        [GetRequestsErrorInterceptorActionFilter]
        public async Task<BaseResponseModel> GetProductsList([FromQuery] ProductsListModel productsListModel)
        {
            if (productsListModel.PageSize < Constants.Configurations.MIN_LIMIT || productsListModel.PageSize > Constants.Configurations.MAX_LIMIT)
                return ErrorResponse("invalid limit parameter", HttpStatusCode.BadRequest);

            if (productsListModel.PageSize < Constants.Configurations.DEFAULT_PAGE_VALUE)
                return ErrorResponse("invalid page parameter", HttpStatusCode.BadRequest);

            //customer validate
            var customer = await _customerService.GetCustomerByIdAsync(productsListModel.CustomerId);
            if (customer == null)
                return ErrorResponse("Customer not found", HttpStatusCode.BadRequest);

            try
            {
                if (_catalogSettings.EnableElasticSearch)
                {
                    //prepare model & property values
                    var productsListRootObject = await PrepareProductListRootObjUsingElastic(productsListModel, customer);
                    return SuccessResponse(await _localizationService.GetResourceAsync("Nop.Api.ProductList"), productsListRootObject);
                }
                else
                {
                    //prepare model & property values
                    var productsListRootObject = await PrepareProductListRootObjUsingLINQ(productsListModel, customer);
                    return SuccessResponse(await _localizationService.GetResourceAsync("Nop.Api.ProductList"), productsListRootObject);
                }
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync(ex.Message, ex);
                return ErrorResponse(ex.Message, HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// Prepare product list dtos using old LINQ
        /// </summary>
        /// <param name="productsListModel"></param>
        /// <param name="customer"></param>
        /// <returns></returns>
        private async Task<ProductsListRootObjectDto> PrepareProductListRootObjUsingLINQ(ProductsListModel productsListModel, Customer customer)
        {
            decimal? from = null;
            decimal? to = null;
            if (productsListModel.From != null && productsListModel.To != null)
            {
                from = Convert.ToDecimal(productsListModel.From);
                to = Convert.ToDecimal(productsListModel.To);
            }

            var store = await _storeContext.GetCurrentStoreAsync();

            //prepar categoryIds
            var categoryIds = new List<int>();
            var selectedCategoryId = productsListModel.CategoryId;
            if (selectedCategoryId > 0)
                categoryIds.Add(selectedCategoryId);

            if (selectedCategoryId > 0 && productsListModel.SubCategoryIds != null && productsListModel.SubCategoryIds.Any())
            {
                categoryIds.AddRange(productsListModel.SubCategoryIds);
            }
            else
            {
                //include subcategories
                if (_catalogSettings.ShowProductsFromSubcategories)
                {
                    var tempCategoryIds = new List<int>();
                    foreach (var id in categoryIds)
                    {
                        tempCategoryIds.AddRange(await _categoryService.GetChildCategoryIdsAsync(id, store.Id));
                    }
                    categoryIds.AddRange(tempCategoryIds);
                }
            }
            categoryIds = categoryIds.Distinct().ToList();

            //prepare final category to be included in search query
            var allCategories = await _categoryService.GetAllCategoriesAsync(storeId: store.Id);
            if (selectedCategoryId > 0 && productsListModel.SubCategoryIds != null && productsListModel.SubCategoryIds.Any() && _catalogSettings.ShowProductsFromSubcategories)
            {
                var removeCategoryIds = new List<int>();
                if (productsListModel.SubCategoryIds != null && productsListModel.SubCategoryIds.Any())
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
                        tempCategoryIds.AddRange(await _categoryService.GetChildCategoryIdsAsync(id, store.Id));
                    }
                    categoryIds.AddRange(tempCategoryIds);
                }
            }

            //spec filter options
            List<SpecificationAttributeOption> filteredSpecs = null;
            if (productsListModel.SpecificationOptionIds is not null)
            {
                var filterableOptions = await _specificationAttributeService.GetFiltrableSpecificationAttributeOptionsByCategoryIdAsync(productsListModel.CategoryId);
                filteredSpecs = filterableOptions.Where(fo => productsListModel.SpecificationOptionIds.Contains(fo.Id)).ToList();
            }

            //get products after filter
            var allProducts = await _productApiService.GetProductsList(userId: productsListModel.CustomerId, storeId: store?.Id ?? 0, categoryIds: categoryIds, productType: productsListModel.ProductType,
            pageIndex: productsListModel.PageIndex, pageSize: productsListModel.PageSize, keywords: productsListModel.Keyword, priceMin: from, priceMax: to, orderBy: productsListModel.OrderBy > 0 ? (ProductSortingEnum)productsListModel.OrderBy : ProductSortingEnum.Position, manufacturerIds: productsListModel.ManufacturerIds,
            filteredSpecOptions: filteredSpecs,
            vendorIds: productsListModel.VendorIds);

            //prepare model & property values
            var productsListAsDtos = await allProducts.SelectAwait(async product => await _dtoHelper.PrepareProductListDTOAsync(product, customer)).ToListAsync();

            var productsListRootObject = new ProductsListRootObjectDto
            {
                ProductsList = productsListAsDtos,
                PageIndex = allProducts.PageIndex + 1,
                PageSize = allProducts.PageSize,
                TotalRecords = allProducts.TotalCount
            };

            return productsListRootObject;
        }

        /// <summary>
        /// Prepare product list dtos using elastic 
        /// </summary>
        /// <param name="productsListModel"></param>
        /// <param name="customer"></param>
        /// <returns></returns>
        private async Task<ProductsListRootObjectDto> PrepareProductListRootObjUsingElastic(ProductsListModel productsListModel, Customer customer)
        {
            var store = await _storeContext.GetCurrentStoreAsync();

            //prepare required param

            //price from & to
            decimal? from = null;
            decimal? to = null;
            if (productsListModel.From != null && productsListModel.To != null)
            {
                from = Convert.ToDecimal(productsListModel.From);
                to = Convert.ToDecimal(productsListModel.To);
            }

            //prepar categoryIds
            var categoryIds = new List<int>();
            var selectedCategoryId = productsListModel.CategoryId;
            if (selectedCategoryId > 0)
                categoryIds.Add(selectedCategoryId);

            if (selectedCategoryId > 0 && productsListModel.SubCategoryIds != null && productsListModel.SubCategoryIds.Any())
            {
                categoryIds.AddRange(productsListModel.SubCategoryIds);
            }
            else
            {
                //include subcategories
                if (_catalogSettings.ShowProductsFromSubcategories)
                {
                    var tempCategoryIds = new List<int>();
                    foreach (var id in categoryIds)
                    {
                        tempCategoryIds.AddRange(await _categoryService.GetChildCategoryIdsAsync(id, store.Id));
                    }
                    categoryIds.AddRange(tempCategoryIds);
                }
            }
            categoryIds = categoryIds.Distinct().ToList();

            //prepare final category to be included in search query
            var allCategories = await _categoryService.GetAllCategoriesAsync(storeId: store.Id);
            if (selectedCategoryId > 0 && productsListModel.SubCategoryIds != null && productsListModel.SubCategoryIds.Any() && _catalogSettings.ShowProductsFromSubcategories)
            {
                var removeCategoryIds = new List<int>();
                if (productsListModel.SubCategoryIds != null && productsListModel.SubCategoryIds.Any())
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
                        tempCategoryIds.AddRange(await _categoryService.GetChildCategoryIdsAsync(id, store.Id));
                    }
                    categoryIds.AddRange(tempCategoryIds);
                }
            }

            //prepare vendors param
            var vendorIds = new List<int>();
            var selectedVendorIds = new List<int>();
            if (productsListModel?.VendorIds?.Any() == true)
            {
                selectedVendorIds.AddRange(productsListModel.VendorIds);
            }
            
                var availableVendorIds = await _workContext.GetCurrentCustomerGeoRadiusVendorIdsAsync();
                vendorIds.AddRange(availableVendorIds);
            

            //prepare manufacturers param
            var manufacturerIds = new List<int>();
            if (productsListModel?.ManufacturerIds?.Any() == true)
                manufacturerIds.AddRange(productsListModel?.ManufacturerIds);

            //pageIndex & pageSize
            var pageIndex = ((productsListModel.PageIndex ?? Constants.Configurations.DEFAULT_PAGE_VALUE) - 1);
            if (pageIndex <= 0)
                pageIndex = 1;
            var pageSize = productsListModel.PageSize ?? Constants.Configurations.DEFAULT_LIMIT;

            //order by
            var orderBy = ProductSortingEnum.Position;
            if (productsListModel.OrderBy.HasValue)
                orderBy = (ProductSortingEnum)productsListModel.OrderBy;

            //association product ids
            var associationProductIds = await _elasticsearchManager.GetAssociateProductsListViaElasticSearchAsync(
                productsListModel.Keyword,
                storeId: store.Id,
                visibleIndividuallyOnly: true,
                isMaster: true,
                categoryIds,
                manufacturerIds,
                vendorIds,
                selectedVendorIds,
                filteredSpecOptions: productsListModel.SpecificationOptionIds,
                priceMin: from,
                priceMax: to,
                parentCategoryId: selectedCategoryId);

            //get paged products and elastic result data
            var (products, elasticResult) = await _elasticsearchManager.SearchProducts(
                productsListModel.Keyword,
                pageIndex,
                pageSize,
                storeId: store.Id,
                visibleIndividuallyOnly: true,
                isMaster: true,
                categoryIds,
                manufacturerIds,
                vendorIds,
                filteredSpecOptions: productsListModel.SpecificationOptionIds,
                priceMin: from,
                priceMax: to,
                orderBy: orderBy,
                parentCategoryId: selectedCategoryId,
                associateProductIds: associationProductIds);

            //prepare model & property values
            var productsListAsDtos = await products.SelectAwait(async product => await _dtoHelper.PrepareProductListDTOAsync(product, customer)).ToListAsync();

            var productsListRootObject = new ProductsListRootObjectDto
            {
                ProductsList = productsListAsDtos,
                PageIndex = products.PageIndex + 1,
                PageSize = products.PageSize,
                TotalRecords = products.TotalCount
            };

            return productsListRootObject;
        }

        /// <summary>
        ///     Receive a list of all products
        /// </summary>
        /// <response code="200">OK</response>
        /// <response code="400">Bad Request</response>
        /// <response code="401">Unauthorized</response>
        [HttpGet]
        [Route("/api/products/search", Name = "API_SearchTermAutoComplete")]
        [AuthorizePermission("PublicStoreAllowNavigation")]
        [ProducesResponseType(typeof(ProductsRootObjectDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ErrorsRootObject), (int)HttpStatusCode.BadRequest)]
        [GetRequestsErrorInterceptorActionFilter]
        public async Task<BaseResponseModel> ProductSearchTermAutoComplete([FromQuery] string term, int pageSize)
        {
            if (string.IsNullOrEmpty(term))
                return ErrorResponse("invalid term parameter", HttpStatusCode.BadRequest);
            term = term.Trim();

            if (string.IsNullOrWhiteSpace(term) || term.Length < _catalogSettings.ProductSearchTermMinimumLength)
                return ErrorResponse($"term parameter length should be {_catalogSettings.ProductSearchTermMinimumLength} or more", HttpStatusCode.BadRequest);

            //current customer
            var customer = await _authenticationService.GetAuthenticatedCustomerAsync();
            if (customer is null)
                return ErrorResponse("Unauthorized! Customer not found", HttpStatusCode.Unauthorized);

            try
            {
                //prepare required param
                var productNumber = _catalogSettings.ProductSearchAutoCompleteNumberOfProducts > 0 ?
                    _catalogSettings.ProductSearchAutoCompleteNumberOfProducts : 10;
                if (pageSize > 0)
                    productNumber = pageSize;

                var store = await _storeContext.GetCurrentStoreAsync();
                var productsListRootObject = new ProductsSearchResultRootDto();

                if (_catalogSettings.EnableElasticSearch)
                {
                    //association product ids
                    var associationProductIds = await _elasticsearchManager.GetAssociateProductIdsListViaElasticSearchAsync(keyword: term,
                    pageSize: productNumber,
                        visibleIndividuallyOnly: true,
                        isMaster: true,
                        languageId: 0,
                        geoVendorIds: await _workContext.GetCurrentCustomerGeoRadiusVendorIdsAsync(customer));

                    //get products using elastic
                    var products = await _elasticsearchManager.SearchAutoCompleteProductsAsync(
                        keyword: term,
                        pageSize: productNumber,
                        visibleIndividuallyOnly: true,
                        isMaster: true,
                        languageId: 0,
                        geoVendorIds: await _workContext.GetCurrentCustomerGeoRadiusVendorIdsAsync(customer),
                        associationProductIds: associationProductIds);

                    //prepare search result dtos
                    var searchResultDtos = await products.SelectAwait(async product => await _dtoHelper.PrepareProductSearchResultDtoElasticAsync(product, customer, true)).ToListAsync();
                    productsListRootObject = new ProductsSearchResultRootDto
                    {
                        ProductsList = searchResultDtos,
                        PageIndex = products.PageIndex + 1,
                        PageSize = products.PageSize,
                        TotalRecords = products.TotalCount
                    };
                }
                else
                {
                    //products
                    var products = await _productService.SearchProductsAsync(0,
                        storeId: store.Id,
                        keywords: term,
                        visibleIndividuallyOnly: true,
                        pageSize: productNumber,
                        //master products only filter along with geoVendorIds
                        isMaster: true,
                        geoVendorIds: await _workContext.GetCurrentCustomerGeoRadiusVendorIdsAsync(customer),
                        //exclude other search exept product name
                        searchDescriptions: false,
                        searchManufacturerPartNumber: false,
                        searchSku: false,
                        searchProductTags: false,
                        languageId: 0);

                    //prepare search result dtos
                    var searchResultDtos = await products.SelectAwait(async product => await _dtoHelper.PrepareProductSearchResultDTOAsync(product, customer, true)).ToListAsync();
                    productsListRootObject = new ProductsSearchResultRootDto
                    {
                        ProductsList = searchResultDtos,
                        PageIndex = products.PageIndex + 1,
                        PageSize = products.PageSize,
                        TotalRecords = products.TotalCount
                    };
                }

                return SuccessResponse("Product search result", productsListRootObject);
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync(ex.Message, ex);
                return ErrorResponse(ex.Message, HttpStatusCode.BadRequest);
            }
        }

        #endregion

        /// <summary>
        ///     Receive a count of all products
        /// </summary>
        /// <response code="200">OK</response>
        /// <response code="401">Unauthorized</response>
        [HttpGet]
        [Route("/api/products/count", Name = "GetProductsCount")]
        [AuthorizePermission("PublicStoreAllowNavigation")]
        [ProducesResponseType(typeof(ProductsCountRootObject), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ErrorsRootObject), (int)HttpStatusCode.BadRequest)]
        [GetRequestsErrorInterceptorActionFilter]
        public async Task<IActionResult> GetProductsCount([FromQuery] ProductsCountParametersModel parameters)
        {
            var allProductsCount = await _productApiService.GetProductsCountAsync(parameters.CreatedAtMin, parameters.CreatedAtMax, parameters.UpdatedAtMin,
                                                                       parameters.UpdatedAtMax, parameters.PublishedStatus, parameters.VendorName,
                                                                       parameters.CategoryId, manufacturerPartNumbers: null, parameters.IsDownload);

            var productsCountRootObject = new ProductsCountRootObject
            {
                Count = allProductsCount
            };

            return Ok(productsCountRootObject);
        }

        /// <summary>
        ///     Retrieve product by spcified id
        /// </summary>
        /// <param name="id">Id of the product</param>
        /// <param name="fields">Fields from the product you want your json to contain</param>
        /// <response code="200">OK</response>
        /// <response code="404">Not Found</response>
        /// <response code="401">Unauthorized</response>
        [HttpGet]
        [Route("/api/products/{id}", Name = "GetProductById")]
        [AuthorizePermission("PublicStoreAllowNavigation")]
        [ProducesResponseType(typeof(ProductsRootObjectDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ErrorsRootObject), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        [GetRequestsErrorInterceptorActionFilter]
        public async Task<BaseResponseModel> GetProductById([FromRoute] int id, [FromQuery] string fields = "")
        {
            if (id <= 0)
                return ErrorResponse("invalid id", HttpStatusCode.BadRequest);

            var product = _productApiService.GetProductById(id);

            if (product == null)
                return ErrorResponse("product not found", HttpStatusCode.NotFound);

            //++Alchub
            //get master product & redirect to it.
            if (!product.IsMaster)
            {
                var masterProduct = await _alchubGeneralService.GetMasterProductByUpcCodeAsync(product.UPCCode);
                if (masterProduct != null && !masterProduct.Deleted)
                    product = masterProduct;
            }

            //--Alchub

            try
            {
                //prepare product detail model
                var productDto = await _dtoHelper.PrepareProductDTOAsync(product);

                var productsRootObject = new ProductsRootObjectDto();
                productsRootObject.Products.Add(productDto);

                return SuccessResponse(await _localizationService.GetResourceAsync("Nop.Api.ProductDetail"), productsRootObject);
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex.Message, HttpStatusCode.BadRequest);
            }
        }

        [HttpGet]
        [Route("/api/products/categories", Name = "GetProductCategories")]
        [AuthorizePermission("PublicStoreAllowNavigation")]
        [ProducesResponseType(typeof(ProductCategoriesRootObjectDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ErrorsRootObject), (int)HttpStatusCode.BadRequest)]
        [GetRequestsErrorInterceptorActionFilter]
        public async Task<IActionResult> GetProductCategories([FromQuery] ProductCategoriesParametersModel parameters, [FromServices] ICategoryApiService categoryApiService)
        {
            if (parameters.ProductIds is null)
            {
                return Error(HttpStatusCode.BadRequest, "product_ids", "Product ids is null");
            }

            var productCategories = await categoryApiService.GetProductCategories(parameters.ProductIds);

            var productCategoriesRootObject = new ProductCategoriesRootObjectDto
            {
                ProductCategories = await productCategories.SelectAwait(async prodCats => new ProductCategoriesDto
                {
                    ProductId = prodCats.Key,
                    Categories = await prodCats.Value.SelectAwait(async cat => await _dtoHelper.PrepareCategoryDTOAsync(cat)).ToListAsync()
                }).ToListAsync()

                //ProductCategories = await productCategories.ToDictionaryAwaitAsync
                //(
                //	keySelector: prodCats => ValueTask.FromResult(prodCats.Key),
                //	elementSelector: async prodCats => await prodCats.Value.SelectAwait(async cat => await _dtoHelper.PrepareCategoryDTOAsync(cat)).ToListAsync()
                //)
            };

            return Ok(productCategoriesRootObject);
        }

        [HttpPost]
        [Route("/api/products", Name = "CreateProduct")]
        [AuthorizePermission("ManageProducts")]
        [ProducesResponseType(typeof(ProductsRootObjectDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ErrorsRootObject), 422)]
        public async Task<IActionResult> CreateProduct(
            [FromBody]
            [ModelBinder(typeof(JsonModelBinder<ProductDto>))]
            Delta<ProductDto> productDelta)
        {
            // Here we display the errors if the validation has failed at some point.
            if (!ModelState.IsValid)
            {
                return Error();
            }

            // Inserting the new product
            var product = await _factory.InitializeAsync();
            productDelta.Merge(product);

            await _productService.InsertProductAsync(product);

            await UpdateProductPicturesAsync(product, productDelta.Dto.Images);

            await _productTagService.UpdateProductTagsAsync(product, productDelta.Dto.Tags?.ToArray() ?? Array.Empty<string>());

            await UpdateProductManufacturersAsync(product, productDelta.Dto.ManufacturerIds);

            await UpdateAssociatedProductsAsync(product, productDelta.Dto.AssociatedProductIds);

            //search engine name
            var seName = await _urlRecordService.ValidateSeNameAsync(product, productDelta.Dto.SeName, product.Name, true);
            await _urlRecordService.SaveSlugAsync(product, seName, 0);

            await UpdateAclRolesAsync(product, productDelta.Dto.RoleIds);

            await UpdateDiscountMappingsAsync(product, productDelta.Dto.DiscountIds);

            await UpdateStoreMappingsAsync(product, productDelta.Dto.StoreIds);

            UpdateRequiredProducts(product, productDelta.Dto.RequiredProductIds);

            await _productService.UpdateProductAsync(product);

            await _customerActivityService.InsertActivityAsync("AddNewProduct", await _localizationService.GetResourceAsync("ActivityLog.AddNewProduct"), product);

            // Preparing the result dto of the new product
            var productDto = await _dtoHelper.PrepareProductDTOAsync(product);

            var productsRootObject = new ProductsRootObjectDto();

            productsRootObject.Products.Add(productDto);

            var json = _jsonFieldsSerializer.Serialize(productsRootObject, string.Empty);

            return new RawJsonActionResult(json);
        }

        [HttpPut]
        [Route("/api/products/{id}", Name = "UpdateProduct")]
        [AuthorizePermission("ManageProducts")]
        [ProducesResponseType(typeof(ProductsRootObjectDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ErrorsRootObject), 422)]
        [ProducesResponseType(typeof(ErrorsRootObject), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> UpdateProduct(
            [FromBody]
            [ModelBinder(typeof(JsonModelBinder<ProductDto>))]
            Delta<ProductDto> productDelta)
        {
            // Here we display the errors if the validation has failed at some point.
            if (!ModelState.IsValid)
            {
                return Error();
            }

            var product = _productApiService.GetProductById(productDelta.Dto.Id);

            if (product == null)
            {
                return Error(HttpStatusCode.NotFound, "product", "not found");
            }

            productDelta.Merge(product);

            product.UpdatedOnUtc = DateTime.UtcNow;
            await _productService.UpdateProductAsync(product);

            await UpdateProductAttributesAsync(product, productDelta);

            await UpdateProductPicturesAsync(product, productDelta.Dto.Images);

            await _productTagService.UpdateProductTagsAsync(product, productDelta.Dto.Tags.ToArray());

            await UpdateProductManufacturersAsync(product, productDelta.Dto.ManufacturerIds);

            await UpdateAssociatedProductsAsync(product, productDelta.Dto.AssociatedProductIds);

            // Update the SeName if specified
            if (productDelta.Dto.SeName != null)
            {
                var seName = await _urlRecordService.ValidateSeNameAsync(product, productDelta.Dto.SeName, product.Name, true);
                await _urlRecordService.SaveSlugAsync(product, seName, 0);
            }

            await UpdateDiscountMappingsAsync(product, productDelta.Dto.DiscountIds);

            await UpdateStoreMappingsAsync(product, productDelta.Dto.StoreIds);

            await UpdateAclRolesAsync(product, productDelta.Dto.RoleIds);

            await _productService.UpdateProductAsync(product);

            await _customerActivityService.InsertActivityAsync("UpdateProduct", await _localizationService.GetResourceAsync("ActivityLog.UpdateProduct"), product);

            // Preparing the result dto of the new product
            var productDto = await _dtoHelper.PrepareProductDTOAsync(product);

            var productsRootObject = new ProductsRootObjectDto();

            productsRootObject.Products.Add(productDto);

            var json = _jsonFieldsSerializer.Serialize(productsRootObject, string.Empty);

            return new RawJsonActionResult(json);
        }

        [HttpDelete]
        [Route("/api/products/{id}", Name = "DeleteProduct")]
        [AuthorizePermission("ManageProducts")]
        [ProducesResponseType(typeof(void), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ErrorsRootObject), (int)HttpStatusCode.BadRequest)]
        [GetRequestsErrorInterceptorActionFilter]
        public async Task<IActionResult> DeleteProduct([FromRoute] int id)
        {
            if (id <= 0)
            {
                return Error(HttpStatusCode.BadRequest, "id", "invalid id");
            }

            var product = _productApiService.GetProductById(id);

            if (product == null)
            {
                return Error(HttpStatusCode.NotFound, "product", "not found");
            }

            await _productService.DeleteProductAsync(product);

            //activity log
            await _customerActivityService.InsertActivityAsync("DeleteProduct", string.Format(await _localizationService.GetResourceAsync("ActivityLog.DeleteProduct"), product.Name), product);

            return new RawJsonActionResult("{}");
        }

        #region Private methods

        private async Task UpdateProductPicturesAsync(Product entityToUpdate, List<ImageMappingDto> setPictures)
        {
            // If no pictures are specified means we don't have to update anything
            if (setPictures == null)
            {
                return;
            }

            // delete unused product pictures
            var productPictures = await _productService.GetProductPicturesByProductIdAsync(entityToUpdate.Id);
            var unusedProductPictures = productPictures.Where(x => setPictures.All(y => y.Id != x.Id)).ToList();
            foreach (var unusedProductPicture in unusedProductPictures)
            {
                var picture = await _pictureService.GetPictureByIdAsync(unusedProductPicture.PictureId);
                if (picture == null)
                {
                    throw new ArgumentException("No picture found with the specified id");
                }
                await _pictureService.DeletePictureAsync(picture);
            }

            foreach (var imageDto in setPictures)
            {
                if (imageDto.Id > 0)
                {
                    // update existing product picture
                    var productPictureToUpdate = productPictures.FirstOrDefault(x => x.Id == imageDto.Id);
                    if (productPictureToUpdate != null && imageDto.Position > 0)
                    {
                        productPictureToUpdate.DisplayOrder = imageDto.Position;
                        await _productService.UpdateProductPictureAsync(productPictureToUpdate);
                    }
                }
                else
                {
                    // add new product picture
                    var newPicture = await _pictureService.InsertPictureAsync(imageDto.Binary, imageDto.MimeType, string.Empty);
                    await _productService.InsertProductPictureAsync(new ProductPicture
                    {
                        PictureId = newPicture.Id,
                        ProductId = entityToUpdate.Id,
                        DisplayOrder = imageDto.Position
                    });
                }
            }
        }

        private async Task UpdateProductAttributesAsync(Product entityToUpdate, Delta<ProductDto> productDtoDelta)
        {
            // If no product attribute mappings are specified means we don't have to update anything
            if (productDtoDelta.Dto.ProductAttributeMappings == null)
            {
                return;
            }

            // delete unused product attribute mappings
            var toBeUpdatedIds = productDtoDelta.Dto.ProductAttributeMappings.Where(y => y.Id != 0).Select(x => x.Id);
            var productAttributeMappings = await _productAttributeService.GetProductAttributeMappingsByProductIdAsync(entityToUpdate.Id);
            var unusedProductAttributeMappings = productAttributeMappings.Where(x => !toBeUpdatedIds.Contains(x.Id)).ToList();

            foreach (var unusedProductAttributeMapping in unusedProductAttributeMappings)
            {
                await _productAttributeService.DeleteProductAttributeMappingAsync(unusedProductAttributeMapping);
            }

            foreach (var productAttributeMappingDto in productDtoDelta.Dto.ProductAttributeMappings)
            {
                if (productAttributeMappingDto.Id > 0)
                {
                    // update existing product attribute mapping
                    var productAttributeMappingToUpdate = productAttributeMappings.FirstOrDefault(x => x.Id == productAttributeMappingDto.Id);
                    if (productAttributeMappingToUpdate != null)
                    {
                        productDtoDelta.Merge(productAttributeMappingDto, productAttributeMappingToUpdate, false);

                        await _productAttributeService.UpdateProductAttributeMappingAsync(productAttributeMappingToUpdate);

                        await UpdateProductAttributeValuesAsync(productAttributeMappingDto, productDtoDelta);
                    }
                }
                else
                {
                    var newProductAttributeMapping = new ProductAttributeMapping
                    {
                        ProductId = entityToUpdate.Id
                    };

                    productDtoDelta.Merge(productAttributeMappingDto, newProductAttributeMapping);

                    // add new product attribute
                    await _productAttributeService.InsertProductAttributeMappingAsync(newProductAttributeMapping);
                }
            }
        }

        private async Task UpdateProductAttributeValuesAsync(ProductAttributeMappingDto productAttributeMappingDto, Delta<ProductDto> productDtoDelta)
        {
            // If no product attribute values are specified means we don't have to update anything
            if (productAttributeMappingDto.ProductAttributeValues == null)
            {
                return;
            }

            // delete unused product attribute values
            var toBeUpdatedIds = productAttributeMappingDto.ProductAttributeValues.Where(y => y.Id != 0).Select(x => x.Id);

            var unusedProductAttributeValues = (await _productAttributeService.GetProductAttributeValuesAsync(productAttributeMappingDto.Id)).Where(x => !toBeUpdatedIds.Contains(x.Id)).ToList();

            foreach (var unusedProductAttributeValue in unusedProductAttributeValues)
            {
                await _productAttributeService.DeleteProductAttributeValueAsync(unusedProductAttributeValue);
            }

            foreach (var productAttributeValueDto in productAttributeMappingDto.ProductAttributeValues)
            {
                if (productAttributeValueDto.Id > 0)
                {
                    // update existing product attribute mapping
                    var productAttributeValueToUpdate = await _productAttributeService.GetProductAttributeValueByIdAsync(productAttributeValueDto.Id);
                    if (productAttributeValueToUpdate != null)
                    {
                        productDtoDelta.Merge(productAttributeValueDto, productAttributeValueToUpdate, false);

                        await _productAttributeService.UpdateProductAttributeValueAsync(productAttributeValueToUpdate);
                    }
                }
                else
                {
                    var newProductAttributeValue = new ProductAttributeValue();
                    productDtoDelta.Merge(productAttributeValueDto, newProductAttributeValue);

                    newProductAttributeValue.ProductAttributeMappingId = productAttributeMappingDto.Id;
                    // add new product attribute value
                    await _productAttributeService.InsertProductAttributeValueAsync(newProductAttributeValue);
                }
            }
        }

        private async Task UpdateDiscountMappingsAsync(Product product, List<int> passedDiscountIds)
        {
            if (passedDiscountIds == null)
            {
                return;
            }

            var allDiscounts = await _discountService.GetAllDiscountsAsync(DiscountType.AssignedToSkus, showHidden: true);
            var appliedProductDiscount = await _discountService.GetAppliedDiscountsAsync(product);
            foreach (var discount in allDiscounts)
            {
                if (passedDiscountIds.Contains(discount.Id))
                {
                    //new discount
                    if (appliedProductDiscount.Count(d => d.Id == discount.Id) == 0)
                    {
                        appliedProductDiscount.Add(discount);
                    }
                }
                else
                {
                    //remove discount
                    if (appliedProductDiscount.Count(d => d.Id == discount.Id) > 0)
                    {
                        appliedProductDiscount.Remove(discount);
                    }
                }
            }

            await _productService.UpdateProductAsync(product);
            await _productService.UpdateHasDiscountsAppliedAsync(product);
        }

        private async Task UpdateProductManufacturersAsync(Product product, List<int> passedManufacturerIds)
        {
            // If no manufacturers specified then there is nothing to map 
            if (passedManufacturerIds == null)
            {
                return;
            }
            var productmanufacturers = await _manufacturerService.GetProductManufacturersByProductIdAsync(product.Id);
            var unusedProductManufacturers = productmanufacturers.Where(x => !passedManufacturerIds.Contains(x.Id)).ToList();

            // remove all manufacturers that are not passed
            foreach (var unusedProductManufacturer in unusedProductManufacturers)
            {
                //_manufacturerService.DeleteProductManufacturer(unusedProductManufacturer);
            }

            foreach (var passedManufacturerId in passedManufacturerIds)
            {
                // not part of existing manufacturers so we will create a new one
                if (productmanufacturers.All(x => x.Id != passedManufacturerId))
                {
                    // if manufacturer does not exist we simply ignore it, otherwise add it to the product
                    var manufacturer = await _manufacturerService.GetManufacturerByIdAsync(passedManufacturerId);
                    if (manufacturer != null)
                    {
                        await _manufacturerService.InsertProductManufacturerAsync(new ProductManufacturer
                        {
                            ProductId = product.Id,
                            ManufacturerId = manufacturer.Id
                        });
                    }
                }
            }
        }

        private async Task UpdateAssociatedProductsAsync(Product product, List<int> passedAssociatedProductIds)
        {
            // If no associated products specified then there is nothing to map 
            if (passedAssociatedProductIds == null)
            {
                return;
            }

            var noLongerAssociatedProducts = (await _productService.GetAssociatedProductsAsync(product.Id, showHidden: true))
                               .Where(p => !passedAssociatedProductIds.Contains(p.Id));

            // update all products that are no longer associated with our product
            foreach (var noLongerAssocuatedProduct in noLongerAssociatedProducts)
            {
                noLongerAssocuatedProduct.ParentGroupedProductId = 0;
                await _productService.UpdateProductAsync(noLongerAssocuatedProduct);
            }

            var newAssociatedProducts = await _productService.GetProductsByIdsAsync(passedAssociatedProductIds.ToArray());
            foreach (var newAssociatedProduct in newAssociatedProducts)
            {
                newAssociatedProduct.ParentGroupedProductId = product.Id;
                await _productService.UpdateProductAsync(newAssociatedProduct);
            }
        }

        private void UpdateRequiredProducts(Product product, IList<int> requiredProductIds)
        {
            if (requiredProductIds is null)
                product.RequiredProductIds = null;
            else
                product.RequiredProductIds = string.Join(',', requiredProductIds.Select(id => id.ToString()));
        }

        #endregion

        #region Filter

        [HttpGet]
        [Route("/api/products/allfilters", Name = "AllFilters")]
        [AuthorizePermission("PublicStoreAllowNavigation")]
        [ProducesResponseType(typeof(CatalogProductsModel), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ErrorsRootObject), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        [GetRequestsErrorInterceptorActionFilter]
        //public async Task<BaseResponseModel> AllFilters([FromQuery] ProductsListModel productsListModel)
        public async Task<BaseResponseModel> AllFilters([FromQuery] int categoryId, int? orderBy = null)
        {
            //current customer
            var customer = await _authenticationService.GetAuthenticatedCustomerAsync();
            if (customer is null)
                return ErrorResponse("Unauthorized! Customer not found", HttpStatusCode.Unauthorized);

            //prepare filters
            var vendorIds = (await _workContext.GetCurrentCustomerGeoRadiusVendorIdsAsync(customer))?.ToList();
            var filters = await _dtoHelper.PrepareAllFilters(categoryId, orderBy, vendorIds);

            return SuccessResponse(await _localizationService.GetResourceAsync("Nop.Api.Filters"), filters);
        }

        ///// <summary>
        ///// all filter api version 1
        ///// </summary>
        ///// <param name="requestModel">CatalogFilterRequestModel</param>
        ///// <returns></returns>
        //[HttpGet]
        //[Route("/api/v1/products/allfilters", Name = "AllFilters_V1")]
        //[AuthorizePermission("PublicStoreAllowNavigation")]
        //[ProducesResponseType(typeof(CatalogProductsModel), (int)HttpStatusCode.OK)]
        //[ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        //[ProducesResponseType(typeof(ErrorsRootObject), (int)HttpStatusCode.BadRequest)]
        //[ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        //[GetRequestsErrorInterceptorActionFilter]
        //public async Task<BaseResponseModel> AllFilters_V1([FromQuery] CatalogFilterRequestModel requestModel)
        //{

        //    if (requestModel.CategoryId <= 0)
        //        return ErrorResponse("invalid categoryId parameter", HttpStatusCode.BadRequest);

        //    //current customer
        //    var customer = await _authenticationService.GetAuthenticatedCustomerAsync();
        //    if (customer is null)
        //        return ErrorResponse("Unauthorized! Customer not found", HttpStatusCode.Unauthorized);

        //    try
        //    {
        //        var model = await _catalogModelFactoryV1.PreprareCatalogFilterModel(requestModel, customer);
        //        return SuccessResponse(await _localizationService.GetResourceAsync("Nop.Api.Filters"), model);
        //    }
        //    catch (Exception ex)
        //    {
        //        return ErrorResponse(ex.Message, HttpStatusCode.InternalServerError);
        //    }
        //}

        /// <summary>
        /// filter api step0 : root categories
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("/api/v1/products/filters/root_categories", Name = "FilterRootCategories_V1")]
        [AuthorizePermission("PublicStoreAllowNavigation")]
        [ProducesResponseType(typeof(RootCategoriesResponseModel), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ErrorsRootObject), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        [GetRequestsErrorInterceptorActionFilter]
        public async Task<BaseResponseModel> FilterRootCategories_V1()
        {
            //current customer
            var customer = await _authenticationService.GetAuthenticatedCustomerAsync();
            if (customer is null)
                return ErrorResponse("Unauthorized! Customer not found", HttpStatusCode.Unauthorized);

            try
            {
                var model = await _catalogModelFactoryV1.PreprareRootCategoriesResponseModel();
                return SuccessResponse(await _localizationService.GetResourceAsync("Nop.Api.Filters.RootCategories"), model);
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex.Message, HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// all filter api step1 version 1
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("/api/v1/products/allfilters", Name = "AllFilters_V1")]
        [AuthorizePermission("PublicStoreAllowNavigation")]
        [ProducesResponseType(typeof(BaseFilterResponseModel), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ErrorsRootObject), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        [GetRequestsErrorInterceptorActionFilter]
        public async Task<BaseResponseModel> AllFilters_V1([FromQuery] BaseFilterRequestModel requestModel)
        {

            if (requestModel.RootCategoryId <= 0 && (requestModel.SubCategoryIds == null || !requestModel.SubCategoryIds.Any()))
                return ErrorResponse("invalid root categoryId parameter, rootcategoryId can be 0 only if any sub category ids passed.", HttpStatusCode.BadRequest);

            //current customer
            var customer = await _authenticationService.GetAuthenticatedCustomerAsync();
            if (customer is null)
                return ErrorResponse("Unauthorized! Customer not found", HttpStatusCode.Unauthorized);

            try
            {
                var model = await _catalogModelFactoryV1.PreprareBaseFilterResponseModel(requestModel, customer);
                return SuccessResponse(await _localizationService.GetResourceAsync("Nop.Api.Filters"), model);
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex.Message, HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// filter api step2 : manufacturers
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("/api/v1/products/filters/manufacturers", Name = "ManufacturerFilters_V1")]
        [AuthorizePermission("PublicStoreAllowNavigation")]
        [ProducesResponseType(typeof(ManufacturerFilterResponseModel), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ErrorsRootObject), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        [GetRequestsErrorInterceptorActionFilter]
        public async Task<BaseResponseModel> ManufacturerFilters_V1([FromQuery] ManufacturerFilterRequestModel requestModel)
        {

            if (requestModel.RootCategoryId <= 0 && (requestModel.SubCategoryIds == null || !requestModel.SubCategoryIds.Any()))
                return ErrorResponse("invalid root categoryId parameter, rootcategoryId can be 0 only if any sub category ids passed.", HttpStatusCode.BadRequest);

            //current customer
            var customer = await _authenticationService.GetAuthenticatedCustomerAsync();
            if (customer is null)
                return ErrorResponse("Unauthorized! Customer not found", HttpStatusCode.Unauthorized);

            try
            {
                var model = await _catalogModelFactoryV1.PreprareManufacturerFilterResponseModel(requestModel, customer);
                return SuccessResponse(await _localizationService.GetResourceAsync("Nop.Api.Filters.Manufacturers"), model);
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex.Message, HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// filter api step3 : vendors
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("/api/v1/products/filters/vendors", Name = "VendorFilters_V1")]
        [AuthorizePermission("PublicStoreAllowNavigation")]
        [ProducesResponseType(typeof(VendorFilterResponseModel), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ErrorsRootObject), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        [GetRequestsErrorInterceptorActionFilter]
        public async Task<BaseResponseModel> VendorFilters_V1([FromQuery] VendorFilterRequestModel requestModel)
        {

            if (requestModel.RootCategoryId <= 0 && (requestModel.SubCategoryIds == null || !requestModel.SubCategoryIds.Any()))
                return ErrorResponse("invalid root categoryId parameter, rootcategoryId can be 0 only if any sub category ids passed.", HttpStatusCode.BadRequest);

            //current customer
            var customer = await _authenticationService.GetAuthenticatedCustomerAsync();
            if (customer is null)
                return ErrorResponse("Unauthorized! Customer not found", HttpStatusCode.Unauthorized);

            try
            {
                var model = await _catalogModelFactoryV1.PreprareVendorFilterResponseModel(requestModel, customer);
                return SuccessResponse(await _localizationService.GetResourceAsync("Nop.Api.Filters.Vendors"), model);
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex.Message, HttpStatusCode.InternalServerError);
            }
        }

        /// <summary>
        /// filter api step4 : specification options
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("/api/v1/products/filters/specification_options", Name = "SpecOptionsFilters_V1")]
        [AuthorizePermission("PublicStoreAllowNavigation")]
        [ProducesResponseType(typeof(SpecificationAttributeOptionFilterResponseModel), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ErrorsRootObject), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        [GetRequestsErrorInterceptorActionFilter]
        public async Task<BaseResponseModel> SpecOptionsFilters_V1([FromQuery] SpecificationAttributeOptionFilterRequestModel requestModel)
        {
            if (requestModel.SpecificationAttributeId <= 0)
                return ErrorResponse("invalid SpecificationAttributeId.", HttpStatusCode.BadRequest);

            if (requestModel.RootCategoryId <= 0 && (requestModel.SubCategoryIds == null || !requestModel.SubCategoryIds.Any()))
                return ErrorResponse("invalid root categoryId parameter, rootcategoryId can be 0 only if any sub category ids passed.", HttpStatusCode.BadRequest);

            //current customer
            var customer = await _authenticationService.GetAuthenticatedCustomerAsync();
            if (customer is null)
                return ErrorResponse("Unauthorized! Customer not found", HttpStatusCode.Unauthorized);

            try
            {
                var model = await _catalogModelFactoryV1.PreprareSpecificationAttributeOptionFilterResponseModel(requestModel, customer);
                return SuccessResponse(await _localizationService.GetResourceAsync("Nop.Api.Filters.SpecificationOptions"), model);
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex.Message, HttpStatusCode.InternalServerError);
            }
        }

        #endregion

        #region Comparing products

        [HttpPost]
        [Route("/api/add_product_to_compare_list", Name = "AddProductToCompareList")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ErrorsRootObject), 422)]
        [GetRequestsErrorInterceptorActionFilter]
        public virtual async Task<BaseResponseModel> AddProductToCompareList([FromBody] AddProductToCompareListModel model)
        {
            if (model.productId == 0)
                return ErrorResponse(await _localizationService.GetResourceAsync("api.User.Invalid.productId"), HttpStatusCode.NotFound);

            var product = await _productService.GetProductByIdAsync(model.productId);
            if (product == null || product.Deleted || !product.Published)
                return ErrorResponse("No product found with the specified ID");

            //current customer
            var customer = await _authenticationService.GetAuthenticatedCustomerAsync();
            if (customer is null)
                return ErrorResponse("Unauthorized! Customer not found", HttpStatusCode.Unauthorized);

            if (!_catalogSettings.CompareProductsEnabled)
                return ErrorResponse("Product comparison is disabled");

            var compareProduct = await _productApiService.GetComparedProductsAsync(customer);
            if (compareProduct.Count < 2)//default allow 2 products to be added in compare.
            {
                await _productApiService.AddProductToCompareListAsync(model.productId, customer);
                return SuccessResponse(await _localizationService.GetResourceAsync("Products.ProductHasBeenAddedToCompareList"), "Product Added to Compare list");
            }
            else
            {
                return ErrorResponse(await _localizationService.GetResourceAsync("Api.Add.Compare.Only.TwoProducts"), HttpStatusCode.NotFound);
            }
        }

        [HttpGet]
        [Route("/api/compare_products", Name = "GetCompare_products")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ErrorsRootObject), 422)]
        [GetRequestsErrorInterceptorActionFilter]
        public virtual async Task<BaseResponseModel> CompareProducts([FromQuery] int Userid)
        {
            if (Userid == 0)
                return ErrorResponse(await _localizationService.GetResourceAsync("api.User.Invalid.Userid"), HttpStatusCode.NotFound);

            var customer = await _customerService.GetCustomerByIdAsync(Userid);
            if (customer is null)
                return ErrorResponse("Customer not found", HttpStatusCode.BadRequest);

            if (!_catalogSettings.CompareProductsEnabled)
                return ErrorResponse("Product comparison is disabled");

            var model = new CompareProductsModel
            {
                IncludeShortDescriptionInCompareProducts = _catalogSettings.IncludeShortDescriptionInCompareProducts,
                IncludeFullDescriptionInCompareProducts = _catalogSettings.IncludeFullDescriptionInCompareProducts,
            };
            var products = await (await _productApiService.GetComparedProductsAsync(customer))
            //ACL and store mapping
            .WhereAwait(async p => await _aclService.AuthorizeAsync(p) && await _storeMappingService.AuthorizeAsync(p))
            //availability dates
            .Where(p => _productService.ProductIsAvailable(p)).ToListAsync();
            //prepare model
            var productcompare = (await _productModelFactory.PrepareProductOverviewModelsAsync(products.Take(2),
                    productThumbPictureSize: 21468, forceRedirectionAfterAddingToCart: true))
                .ToList();
            foreach (var product in productcompare)
            {

                model.Products.Add(product);
            }

            return SuccessResponse("Compare Products", model);
        }

        [HttpPost]
        [Route("/api/remove_product_from_compareList", Name = "RemoveProductFromCompareList")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ErrorsRootObject), 422)]
        [GetRequestsErrorInterceptorActionFilter]
        public virtual async Task<BaseResponseModel> RemoveProductFromCompareList([FromBody] AddProductToCompareListModel model)
        {
            if (model.productId == 0)
                return ErrorResponse(await _localizationService.GetResourceAsync("api.User.Invalid.productId"), HttpStatusCode.BadRequest);

            var product = await _productService.GetProductByIdAsync(model.productId);
            if (product == null)
                return ErrorResponse("No product found with the specified ID", HttpStatusCode.NotFound);

            //current customer
            var customer = await _authenticationService.GetAuthenticatedCustomerAsync();
            if (customer is null)
                return ErrorResponse("Unauthorized! Customer not found", HttpStatusCode.Unauthorized);

            if (!_catalogSettings.CompareProductsEnabled)
                return ErrorResponse("Product comparison is disabled");

            await _productApiService.RemoveProductFromCompareListAsync(model.productId, customer);

            return SuccessResponse(await _localizationService.GetResourceAsync("Nop.Api.Products.ProductHasBeenRemovedToCompareList"));
        }

        #endregion

        #region ProductDetails

        [HttpGet]
        [Route("/api/product_details_vendors", Name = "ProductDetailsVendors")]
        [AuthorizePermission("PublicStoreAllowNavigation")]
        [ProducesResponseType(typeof(ProductsRootObjectDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ErrorsRootObject), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        [GetRequestsErrorInterceptorActionFilter]
        public async Task<BaseResponseModel> ProductDetailsVendors([FromQuery] int productid, int customerid, int sortBy)
        {
            if (productid <= 0)
                return ErrorResponse("invalid product", HttpStatusCode.BadRequest);

            if (customerid <= 0)
                return ErrorResponse("invalid customer", HttpStatusCode.BadRequest);

            var customer = await _customerService.GetCustomerByIdAsync(customerid);
            if (customer is null)
                return ErrorResponse("Unauthorized! Customer not found", HttpStatusCode.Unauthorized);

            var product = _productApiService.GetProductById(productid);
            if (product == null)
                return ErrorResponse("product not found", HttpStatusCode.NotFound);

            try
            {
                //get available vendors
                var availableVendors = (await _vendorService.GetAvailableGeoFenceVendorsAsync(customer, true, product))?.ToList();

                //prepare availbale vendors productss
                var productVendorModel = await _productModelFactory.PrepareProductVendorsAsync(product, availableVendors, customer, sortBy);

                return SuccessResponse(await _localizationService.GetResourceAsync("Nop.Api.ProductDetailsVendors"), productVendorModel);
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex.Message, HttpStatusCode.BadRequest);
            }
        }

        [HttpGet]
        [Route("/api/product_details_delivery_slots", Name = "ProductDetailsDeliverySlots")]
        [AuthorizePermission("PublicStoreAllowNavigation")]
        [ProducesResponseType(typeof(ProductsRootObjectDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ErrorsRootObject), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        [GetRequestsErrorInterceptorActionFilter]
        public async Task<BaseResponseModel> ProductDetailsDeliverySlots([FromQuery] int productId, int vendorId, string isDelivery, string isPickup, string deliveryAvailable)
        {
            if (productId <= 0)
                return ErrorResponse("invalid id", HttpStatusCode.BadRequest);

            bool delivery = false;
            bool pickup = false;
            bool isDeliveryAvailable = false;
            if (isDelivery.ToLower() == "true")
            {
                delivery = true;
            }
            if (isPickup.ToLower() == "true")
            {
                pickup = true;
            }

            if (deliveryAvailable.ToLower() == "true")
            {
                isDeliveryAvailable = true;
            }


            var product = _productApiService.GetProductById(productId);

            if (product == null)
                return ErrorResponse("product not found", HttpStatusCode.NotFound);

            try
            {
                IList<BookingSlotModel> model = new List<BookingSlotModel>();
                if (productId > 0 && vendorId > 0)
                {
                    var addDay = await _settingService.GetSettingByKeyAsync<int>("Alchub.Slot.Visible.After.Day", defaultValue: 1);
                    var addHour = await _settingService.GetSettingByKeyAsync<int>("Alchub.Slot.Add.Hour", defaultValue: 0);
                    var addMintues = await _settingService.GetSettingByKeyAsync<int>("Alchub.Slot.Add.Mintues", defaultValue: 0);
                    var dateTime = await _dateTimeHelper.ConvertToUserTimeAsync(DateTime.Now);
                    dateTime = dateTime.AddDays(addDay).AddHours(addHour).AddMinutes(addMintues);
                    var vendor = await _vendorService.GetVendorByIdAsync(vendorId);
                    if (isDeliveryAvailable)
                    {
                        if (delivery)
                        {
                            model = await _productModelFactory.PreparepareSlotListModel(0, dateTime.ToString(), dateTime.AddDays(7).ToString(), productId, vendorId);
                            if (model != null)
                            {
                                return SuccessResponse(await _localizationService.GetResourceAsync("Nop.Api.ProductDetailsDeliverySlots"), model);
                            }

                        }
                        else
                        {
                            model = await _productModelFactory.PreparepareAdminSlotListModel(0, dateTime.ToString(), dateTime.AddDays(7).ToString(), productId, vendorId, false);
                            if (model != null)
                            {
                                return SuccessResponse(await _localizationService.GetResourceAsync("Nop.Api.ProductDetailsDeliverySlots"), model);
                            }
                        }
                    }
                    if (pickup)
                    {
                        model = await _productModelFactory.PrepareparePickupSlotListModel(0, dateTime.ToString(), dateTime.AddDays(7).ToString(), productId, vendorId, true);
                        if (model != null)
                        {
                            return SuccessResponse(await _localizationService.GetResourceAsync("Nop.Api.ProductDetailsDeliverySlots"), model);
                        }
                    }
                }

                return SuccessResponse(await _localizationService.GetResourceAsync("Nop.Api.ProductDetailsDeliverySlots"), model);
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex.Message, HttpStatusCode.BadRequest);
            }
        }

        [HttpPost]
        [Route("/api/product_details_book_delivery_slot", Name = "ProductDetailsBookDeliverySlot")]
        [AuthorizePermission("PublicStoreAllowNavigation")]
        [ProducesResponseType(typeof(ProductsRootObjectDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ErrorsRootObject), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        [GetRequestsErrorInterceptorActionFilter]
        public async Task<BaseResponseModel> ProductDetailsBookDeliverySlot([FromBody] SlotRequest slotRequest)
        {
            if (slotRequest.productId <= 0)
                return ErrorResponse("invalid productId", HttpStatusCode.BadRequest);


            if (string.IsNullOrEmpty(slotRequest.startDate))
                return ErrorResponse("invalid Date", HttpStatusCode.BadRequest);


            if (slotRequest.SlotId <= 0)
                return ErrorResponse("invalid slotId", HttpStatusCode.BadRequest);


            if (string.IsNullOrEmpty(slotRequest.SlotTime))
                return ErrorResponse("invalid time SlotTime", HttpStatusCode.BadRequest);

            try
            {
                if (slotRequest.SlotId < 0)
                    throw new Exception("Invalid slot id");
                var store = _storeContext.GetCurrentStore();
                var addDay = await _settingService.GetSettingByKeyAsync<int>("Alchub.Slot.Visible.After.Day", defaultValue: 1);
                var addHour = await _settingService.GetSettingByKeyAsync<int>("Alchub.Slot.Add.Hour", defaultValue: 0);
                var addMintues = await _settingService.GetSettingByKeyAsync<int>("Alchub.Slot.Add.Mintues", defaultValue: 0);
                var dateTime = await _dateTimeHelper.ConvertToUserTimeAsync(DateTime.Now);
                dateTime = dateTime.AddDays(addDay).AddHours(addHour).AddMinutes(addMintues);
                decimal price = 0;
                var vendor = await _vendorService.GetVendorByIdAsync(slotRequest.vendorId);
                var cutomerProductSlotList = await _slotService.GetCustomerProductSlotId(slotRequest.SlotId, slotRequest.productId, slotRequest.customerId, dateTime, slotRequest.isPickup);
                if (cutomerProductSlotList.Count == 0)
                {
                    await _slotService.DeleteCustomerProductSlot(slotRequest.productId, slotRequest.customerId);
                    CustomerProductSlot customerProductSlot = new CustomerProductSlot();
                    customerProductSlot.SlotId = slotRequest.SlotId;
                    customerProductSlot.BlockId = slotRequest.BlockId;
                    customerProductSlot.StartTime = Convert.ToDateTime(slotRequest.startDate);
                    customerProductSlot.EndDateTime = Convert.ToDateTime(slotRequest.endDate);
                    customerProductSlot.EndTime = slotRequest.SlotTime;
                    customerProductSlot.Price = price;
                    customerProductSlot.CustomerId = slotRequest.customerId;
                    customerProductSlot.ProductId = slotRequest.productId;
                    customerProductSlot.IsPickup = slotRequest.isPickup;
                    customerProductSlot.LastUpdated = DateTime.UtcNow;
                    customerProductSlot.IsSelected = true;
                    await _slotService.InsertCustomerProductSlot(customerProductSlot);
                }
                else
                {
                    foreach (var cutomerProductSlot in cutomerProductSlotList)
                    {
                        cutomerProductSlot.StartTime = Convert.ToDateTime(slotRequest.startDate);
                        cutomerProductSlot.EndDateTime = Convert.ToDateTime(slotRequest.endDate);
                        cutomerProductSlot.EndTime = slotRequest.SlotTime;
                        cutomerProductSlot.SlotId = slotRequest.SlotId;
                        cutomerProductSlot.BlockId = slotRequest.BlockId;
                        cutomerProductSlot.Price = price;
                        cutomerProductSlot.IsPickup = slotRequest.isPickup;
                        cutomerProductSlot.LastUpdated = DateTime.UtcNow;
                        cutomerProductSlot.IsSelected = true;
                        await _slotService.UpdateCustomerProductSlot(cutomerProductSlot);
                    }
                }
                return SuccessResponse(await _localizationService.GetResourceAsync("Nop.Api.ProductDetailsBookDeliverySlot.Successfully"));
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex.Message, HttpStatusCode.BadRequest);
            }
        }

        #endregion

        #region Slot Management

        [HttpGet]
        [Route("/api/products/get_fastest_slot", Name = "API_GetProductFastestSlot")]
        [ProducesResponseType(typeof(FastestSlotModel), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ErrorsRootObject), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        [GetRequestsErrorInterceptorActionFilter]
        public async Task<BaseResponseModel> GetProductFastestSlot([FromQuery] int product_id)
        {
            if (product_id <= 0)
                return ErrorResponse("invalid product_id", HttpStatusCode.BadRequest);

            var product = _productApiService.GetProductById(product_id);

            if (product == null)
                return ErrorResponse("product not found", HttpStatusCode.NotFound);

            //current customer
            var customer = await _authenticationService.GetAuthenticatedCustomerAsync();
            if (customer is null)
                return ErrorResponse("Unauthorized! Customer not found", HttpStatusCode.Unauthorized);

            try
            {
                //check if enabled
                if (!_catalogSettings.ShowFastestSlotOnCatalogPage)
                {
                    return SuccessResponse("Fastest Slot feature is disabled",
                        new FastestSlotModel()
                        {
                            ProductId = product_id,
                            FastestSlotTiming = ""
                        });
                }

                //prepare fastest slot model
                var (fastestSlot, slotModel) = await _productModelFactory.PrepareProductFastestSlotAsync(product, customer);
                var model = new FastestSlotModel()
                {
                    ProductId = product_id,
                    FastestSlotTiming = fastestSlot
                };

                return SuccessResponse("Product fastest slot", model);
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex.Message, HttpStatusCode.BadRequest);
            }
        }

        #endregion
    }
}
