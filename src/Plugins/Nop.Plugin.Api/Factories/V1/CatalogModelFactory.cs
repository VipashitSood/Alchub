using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Alchub.Domain;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Plugin.Api.Models.V1.Catalog.Filter;
using Nop.Services.Alchub.API.Filter.V1;
using Nop.Services.Catalog;
using Nop.Services.Localization;

namespace Nop.Plugin.Api.Factories.V1
{
    public class CatalogModelFactory : ICatalogModelFactory
    {
        #region Fields

        private readonly ICatalogService _catalogService;
        private readonly ICategoryService _categoryService;
        private readonly IWorkContext _workContext;
        private readonly CatalogSettings _catalogSettings;
        private readonly IStoreContext _storeContext;
        private readonly ILocalizationService _localizationService;
        private readonly ISpecificationAttributeService _specificationAttributeService;
        private readonly AlchubSettings _alchubSettings;

        #endregion

        #region Ctor

        public CatalogModelFactory(ICatalogService catalogService,
            ICategoryService categoryService,
            IWorkContext workContext,
            CatalogSettings catalogSettings,
            IStoreContext storeContext,
            ILocalizationService localizationService,
            ISpecificationAttributeService specificationAttributeService,
            AlchubSettings alchubSettings)
        {
            _catalogService = catalogService;
            _categoryService = categoryService;
            _workContext = workContext;
            _catalogSettings = catalogSettings;
            _storeContext = storeContext;
            _localizationService = localizationService;
            _specificationAttributeService = specificationAttributeService;
            _alchubSettings = alchubSettings;
        }

        #endregion

        #region Constants

        private const string CATEGORY_ENTITY_NAME = "Category";
        private const string VENDOR_ENTITY_NAME = "Vendor";
        private const string BRAND_ENTITY_NAME = "Manufacturer";
        private const string SPECIFICATION_ATTRIBUTE_ENTITY_NAME = "SpecificationAttribute";

        #endregion

        #region Utilities

        /// <summary>
        /// Prepare categoryIds 
        /// </summary>
        /// <param name="selectedCategoryId"></param>
        /// <param name="subCategoryIds"></param>
        /// <returns></returns>
        private async Task<IList<int>> PrepareCategoryIdsParametor(int selectedCategoryId, List<int> subCategoryIds = null)
        {
            //prepar categoryIds
            var categoryIds = new List<int>();
            if (selectedCategoryId > 0)
                categoryIds.Add(selectedCategoryId);

            if (selectedCategoryId > 0 && subCategoryIds != null && subCategoryIds.Any())
            {
                categoryIds.AddRange(subCategoryIds);
            }
            else
            {
                //include subcategories
                if (_catalogSettings.ShowProductsFromSubcategories)
                {
                    var tempCategoryIds = new List<int>();
                    foreach (var id in categoryIds)
                    {
                        tempCategoryIds.AddRange(await _categoryService.GetChildCategoryIdsAsync(id));
                    }
                    categoryIds.AddRange(tempCategoryIds);
                }
            }
            categoryIds = categoryIds.Distinct().ToList();

            //assign categoryIds to make filter check work correctly
            var filteredCategoryIds = new List<int>();
            filteredCategoryIds.AddRange(categoryIds);

            //prepare final category to be included in search query
            if (selectedCategoryId > 0 && subCategoryIds != null && subCategoryIds.Any() && _catalogSettings.ShowProductsFromSubcategories)
            {
                var allCategories = await _categoryService.GetAllCategoriesAsync();
                var removeCategoryIds = new List<int>();
                if (subCategoryIds != null && subCategoryIds.Any())
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
                        tempCategoryIds.AddRange(await _categoryService.GetChildCategoryIdsAsync(id));
                    }
                    categoryIds.AddRange(tempCategoryIds);
                }
            }

            return categoryIds;
        }

        /// <summary>
        /// Sort categories for tree representation
        /// </summary>
        /// <param name="source">Source</param>
        /// <param name="parentId">Parent category identifier</param>
        /// <param name="ignoreCategoriesWithoutExistingParent">A value indicating whether categories without parent category in provided category list (source) should be ignored</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the sorted categories
        /// </returns>
        protected virtual async Task<IList<Category>> SortCategoriesForTreeAsync(IList<Category> source, int parentId = 0,
            bool ignoreCategoriesWithoutExistingParent = false)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            var result = new List<Category>();

            foreach (var cat in source.Where(c => c.ParentCategoryId == parentId).ToList())
            {
                result.Add(cat);
                result.AddRange(await SortCategoriesForTreeAsync(source, cat.Id, true));
            }

            if (ignoreCategoriesWithoutExistingParent || result.Count == source.Count)
                return result;

            //find categories without parent in provided category source and insert them into result
            foreach (var cat in source)
                if (result.FirstOrDefault(x => x.Id == cat.Id) == null)
                    result.Add(cat);

            return result;
        }

        /// <summary>
        /// Prepare category hiarachy
        /// </summary>
        /// <param name="allCategories"></param>
        /// <param name="rootCategoryId"></param>
        /// <param name="allFilterResult"></param>
        /// <returns></returns>
        private async Task<IList<CatalogFilterResponseModel.CategorFilterModel>> PrepareCategoryHiarachy(IList<Category> allCategories,
            int rootCategoryId, IList<AllFilterResult> allFilterResult)
        {
            var result = new List<CatalogFilterResponseModel.CategorFilterModel>();

            if (allCategories == null || !allCategories.Any())
                return result;

            //root categories
            var categories = allCategories.Where(c => c.ParentCategoryId == rootCategoryId).OrderBy(c => c.DisplayOrder).ToList();
            if (categories.Any())
            {
                foreach (var category in categories)
                {
                    var categoryModel = new CatalogFilterResponseModel.CategorFilterModel
                    {
                        Id = category.Id,
                        Name = category.Name,
                        NumberOfProducts = allFilterResult.FirstOrDefault(x => x.CategoryId == category.Id)?.CategoryItemCount ?? 0
                    };

                    var subCategories = await PrepareCategoryHiarachy(allCategories, category.Id, allFilterResult);
                    categoryModel.SubCategories.AddRange(subCategories);

                    result.Add(categoryModel);
                }
            }
            else
            {
                //Note: This will runs first time only. to handle root category hiarachy.
                if (rootCategoryId == 0)
                {
                    foreach (var category in allCategories.OrderBy(c => c.DisplayOrder))
                    {
                        var categoryModel = new CatalogFilterResponseModel.CategorFilterModel
                        {
                            Id = category.Id,
                            Name = category.Name,
                            NumberOfProducts = allFilterResult.FirstOrDefault(x => x.CategoryId == category.Id)?.CategoryItemCount ?? 0
                        };

                        var subCategories = await PrepareCategoryHiarachy(allCategories, category.Id, allFilterResult);
                        categoryModel.SubCategories.AddRange(subCategories);

                        result.Add(categoryModel);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Prepare filtered categories from result
        /// </summary>
        /// <param name="allFilterResult"></param>
        /// <param name="responseModel"></param>
        /// <returns></returns>
        private async Task PrepareFilteredCategoriesFromResult(IList<AllFilterResult> allFilterResult, CatalogFilterResponseModel responseModel)
        {
            if (allFilterResult != null && allFilterResult.Any())
            {
                var resultCategoryIds = allFilterResult.Select(r => r.CategoryId).Distinct().ToList();

                //load all categories - we know its cached
                var allCategories = await _categoryService.GetAllCategoriesAsync();
                //keep result categories
                allCategories = allCategories.Where(c => resultCategoryIds.Contains(c.Id)).ToList();
                //sort
                allCategories = await SortCategoriesForTreeAsync(allCategories);

                //prepare model
                var model = await PrepareCategoryHiarachy(allCategories, 0, allFilterResult);

                responseModel.Categories = model;
            }
        }

        /// <summary>
        /// Prepare filtered manufacturers from result
        /// </summary>
        /// <param name="allFilterResult"></param>
        /// <param name="responseModel"></param>
        /// <returns></returns>
        private async Task PrepareFilteredManufacturersFromResult(IList<AllFilterResult> allFilterResult, CatalogFilterResponseModel responseModel)
        {
            if (allFilterResult != null && allFilterResult.Any())
            {
                var manufacturers = new List<CatalogFilterResponseModel.ManufacturerFilterModel>();
                var manufactureDistinctRecords = allFilterResult.DistinctBy(m => m.ManufactureId).ToList();

                foreach (var record in manufactureDistinctRecords)
                {
                    manufacturers.Add(new CatalogFilterResponseModel.ManufacturerFilterModel
                    {
                        Id = record.ManufactureId,
                        Name = record.ManfecturerName,
                        NumberOfProducts = allFilterResult.FirstOrDefault(x => x.ManufactureId == record.ManufactureId)?.ManufactureItemCount ?? 0
                    });
                }

                responseModel.Manufacturers = manufacturers;
            }

            await Task.CompletedTask;
        }

        /// <summary>
        /// Prepare filtered spec attributes and its options from result
        /// </summary>
        /// <param name="allFilterResult"></param>
        /// <param name="responseModel"></param>
        /// <returns></returns>
        private async Task PrepareFilteredSpecificationsFromResult(IList<AllFilterResult> allFilterResult, CatalogFilterResponseModel responseModel)
        {
            if (allFilterResult != null && allFilterResult.Any())
            {
                var specificationAttributes = new List<CatalogFilterResponseModel.SpecificationAttributeFilterModel>();
                var specAttributeRecords = allFilterResult.DistinctBy(m => m.SpecificationAttributeId).ToList();

                foreach (var record in specAttributeRecords)
                {
                    var specificationAttribute = new CatalogFilterResponseModel.SpecificationAttributeFilterModel
                    {
                        Id = record.SpecificationAttributeId,
                        Name = record.SpecificationAttributeName
                    };

                    //spec options
                    var specOptionsResult = allFilterResult.Where(x => x.SpecificationAttributeId == record.SpecificationAttributeId).ToList();
                    foreach (var specOption in specOptionsResult)
                    {
                        if (!specificationAttribute.Values.Select(x => x.Id).Contains(specOption.SpecificationOptionId))
                            specificationAttribute.Values.Add(new CatalogFilterResponseModel.SpecificationAttributeFilterModel.SpecificationAttributeValueFilterModel
                            {
                                Id = specOption.SpecificationOptionId,
                                Name = specOption.SpecificationOptionName,
                                NumberOfProducts = allFilterResult.FirstOrDefault(x => x.SpecificationOptionId == specOption.SpecificationOptionId)?.SpecificationItemCount ?? 0
                            });
                    }

                    //add specification
                    specificationAttributes.Add(specificationAttribute);
                }

                responseModel.SpecificationAttributes = specificationAttributes;
            }

            await Task.CompletedTask;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Prepare catalog filter response model
        /// </summary>
        /// <param name="catalogFilterRequest"></param>
        /// <param name="categoryIds"></param>
        /// <returns></returns>
        public async Task<CatalogFilterResponseModel> PreprareCatalogFilterModel(CatalogFilterRequestModel catalogFilterRequest, Customer customer)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            //prepare required param for service
            //prepare params
            var allCategoryIds = await PrepareCategoryIdsParametor(catalogFilterRequest.CategoryId);

            //var allCategoryIds = new List<int>();
            //if (catalogFilterRequest.CategoryId > 0)
            //{
            //    allCategoryIds.Add(catalogFilterRequest.CategoryId);
            //    if (_catalogSettings.ShowProductsFromSubcategories)
            //        allCategoryIds.AddRange(await _categoryService.GetChildCategoryIdsAsync(catalogFilterRequest.CategoryId));

            //    //disticnt
            //    allCategoryIds = allCategoryIds.Distinct().ToList();
            //}

            //available vendors
            var vendorIds = catalogFilterRequest.VendorIds != null ?
                            catalogFilterRequest.VendorIds.ToList() : (await _workContext.GetCurrentCustomerGeoRadiusVendorIdsAsync(customer)).ToList();

            //get result using sp call
            var allFilterResult = await _catalogService.GetAllFilterResultAsync(allCategoryIds?.ToList(), vendorIds, catalogFilterRequest.ManufacturerIds, catalogFilterRequest.SpecificationOptionIds);

            ///Pending
            var responseModel = new CatalogFilterResponseModel();
            if (allFilterResult != null && allFilterResult.Any())
            {
                //categories
                await PrepareFilteredCategoriesFromResult(allFilterResult, responseModel);

                //manufacturers
                await PrepareFilteredManufacturersFromResult(allFilterResult, responseModel);

                //spec attribute and its options
                await PrepareFilteredSpecificationsFromResult(allFilterResult, responseModel);

                //vendors - pending.
            }

            return responseModel;
        }

        #region Saparate filters

        #region Private methods

        /// <summary>
        /// Prepare sub category ids
        /// </summary>
        /// <param name="baseFilterRequestModel"></param>
        /// <returns></returns>
        private async Task<(int rootCategoryId, List<int> subCategoryIds)> PrepareSubCategoryIdsAndSetRootCategoryIdParametor(int rootCategoryId, List<int> subCategoryIds = null)
        {
            //validate not 0
            if (subCategoryIds != null && subCategoryIds.Contains(0))
                subCategoryIds.Remove(0);

            //validate subCategoryIds
            if (subCategoryIds != null && subCategoryIds.Any())
            {
                var removeCategoryIds = new List<int>();
                //we know its cached
                var allCategories = await _categoryService.GetAllCategoriesAsync();

                //set rootCategory first if same was not passed.
                if (rootCategoryId <= 0)
                {
                    //set first chil category's parent category id
                    var childCategory = allCategories.FirstOrDefault(x => x.Id == subCategoryIds.FirstOrDefault())
                        ?? throw new ArgumentException($"Child category not found with specific id {subCategoryIds.FirstOrDefault()}");

                    //assign root category
                    rootCategoryId = childCategory.ParentCategoryId;
                }

                //child cats of root cat
                allCategories = allCategories.Where(c => c.ParentCategoryId == rootCategoryId)?.ToList();
                foreach (var subCategoryId in subCategoryIds)
                {
                    var subCategory = allCategories.Where(x => x.Id == subCategoryId)?.FirstOrDefault();
                    if (subCategory == null)
                        removeCategoryIds.Add(subCategoryId);
                }

                if (removeCategoryIds.Any())
                    subCategoryIds.RemoveAll(x => removeCategoryIds.Contains(x));
            }

            return (rootCategoryId, subCategoryIds ?? new List<int>());
        }

        /// <summary>
        /// Prepare filterable sub categories from result
        /// </summary>
        /// <param name="baseFilterResult"></param>
        /// <param name="selectedSubCategoryIds"></param>
        /// <param name="baseFilterResponseModel"></param>
        /// <returns></returns>
        private async Task PrepareFilterableSubCategoryModelsFromResult(IList<BaseFilterResult> baseFilterResult, List<int> selectedSubCategoryIds, BaseFilterResponseModel baseFilterResponseModel)
        {
            //split result by entity and prepare model
            //sub categories
            var subCategoriesResult = baseFilterResult.Where(r => r.EntityName == CATEGORY_ENTITY_NAME && r.CategoryId.HasValue && r?.CategoryId > 0);
            var subCategoriesModel = new List<BaseFilterResponseModel.SubCategorFilterModel>();
            foreach (var result in subCategoriesResult?.OrderBy(x => x.DisplayOrder))
            {
                if (result.CategoryId.HasValue && result?.CategoryId > 0)
                {
                    subCategoriesModel.Add(new BaseFilterResponseModel.SubCategorFilterModel
                    {
                        Id = result.CategoryId.Value,
                        Name = result.CategoryName,
                        NumberOfProducts = result.CategoryItemCount,
                        Selected = selectedSubCategoryIds.Any(sc => sc.Equals(result.CategoryId))
                    });
                }
            }
            baseFilterResponseModel.SubCategories = subCategoriesModel;

            ////spec attribute
            //var specAttributeResult = baseFilterResult.Where(r => r.EntityName == SPECIFICATION_ATTRIBUTE_ENTITY_NAME);
            //var specAttributesModel = new List<BaseFilterResponseModel.SpecificationAttributeFilterModel>();
            //foreach (var result in specAttributeResult?.OrderBy(x => x.MainFilterDisplayOrder))
            //{
            //    specAttributesModel.Add(new BaseFilterResponseModel.SpecificationAttributeFilterModel
            //    {
            //        Id = result.Id,
            //        Name = result.MainFilter
            //    });
            //}
            //baseFilterResponseModel.SpecificationAttributes = specAttributesModel;

            await Task.CompletedTask;
        }

        /// <summary>
        /// Prepare filterable specification attributes
        /// </summary>
        /// <param name="baseFilterResult"></param>
        /// <returns></returns>
        private async Task<IList<SpecificationAttributeFilterModelV1>> PrepareFilterableSpecificationAttributeListModel(IList<BaseFilterResult> baseFilterResult)
        {
            //split result by entity and prepare model
            //spec attribute
            var specAttributeResult = baseFilterResult.Where(r => r.EntityName == SPECIFICATION_ATTRIBUTE_ENTITY_NAME);
            var specAttributesModel = new List<SpecificationAttributeFilterModelV1>();
            foreach (var result in specAttributeResult?.OrderBy(x => x.MainFilterDisplayOrder))
            {
                specAttributesModel.Add(new SpecificationAttributeFilterModelV1
                {
                    Id = result.Id,
                    Name = result.MainFilter
                });
            }

            return await Task.FromResult(specAttributesModel);
        }

        /// <summary>
        /// Prepare filterable specification attributes
        /// </summary>
        /// <param name="baseFilterResult"></param>
        /// <returns></returns>
        private async Task<IList<SpecificationAttributeFilterModelV1>> PrepareFilterableSpecificationAttributeListModel(IList<ManufacturerFilterResult> baseFilterResult)
        {
            //split result by entity and prepare model
            //spec attribute
            var specAttributeResult = baseFilterResult.Where(r => r.EntityName == SPECIFICATION_ATTRIBUTE_ENTITY_NAME);
            var specAttributesModel = new List<SpecificationAttributeFilterModelV1>();
            foreach (var result in specAttributeResult?.OrderBy(x => x.MainFilterDisplayOrder))
            {
                specAttributesModel.Add(new SpecificationAttributeFilterModelV1
                {
                    Id = result.Id,
                    Name = result.MainFilter
                });
            }

            return await Task.FromResult(specAttributesModel);
        }

        /// <summary>
        /// Prepare filterable specification attributes
        /// </summary>
        /// <param name="baseFilterResult"></param>
        /// <returns></returns>
        private async Task<IList<SpecificationAttributeFilterModelV1>> PrepareFilterableSpecificationAttributeListModel(IList<VendorFilterResult> baseFilterResult)
        {
            //split result by entity and prepare model
            //spec attribute
            var specAttributeResult = baseFilterResult.Where(r => r.EntityName == SPECIFICATION_ATTRIBUTE_ENTITY_NAME);
            var specAttributesModel = new List<SpecificationAttributeFilterModelV1>();
            foreach (var result in specAttributeResult?.OrderBy(x => x.MainFilterDisplayOrder))
            {
                specAttributesModel.Add(new SpecificationAttributeFilterModelV1
                {
                    Id = result.Id,
                    Name = result.MainFilter
                });
            }

            return await Task.FromResult(specAttributesModel);
        }

        /// <summary>
        /// Prepare filterable specification attributes
        /// </summary>
        /// <param name="baseFilterResult"></param>
        /// <returns></returns>
        private async Task<IList<SpecificationAttributeFilterModelV1>> PrepareFilterableSpecificationAttributeListModel(IList<SpecificationAttributeOptionFilterResult> baseFilterResult, int selectedSpecificationAttributeId)
        {
            //split result by entity and prepare model
            //spec attribute
            var specAttributeResult = baseFilterResult.Where(r => r.Id > 0).DistinctBy(x => x.Id); //spec attribute have positive ids in result
            var specAttributesModel = new List<SpecificationAttributeFilterModelV1>();
            foreach (var result in specAttributeResult?.OrderBy(x => x.MainFilterDisplayOrder))
            {
                specAttributesModel.Add(new SpecificationAttributeFilterModelV1
                {
                    Id = result.Id,
                    Name = result.MainFilter,
                    Selected = result.Id == selectedSpecificationAttributeId
                });
            }

            return await Task.FromResult(specAttributesModel);
        }

        /// <summary>
        /// Prepare filterable price option from result
        /// </summary>
        /// <param name="baseFilterResponseModel"></param>
        /// <returns></returns>
        private async Task PrepareFilterablePriceOptionsModel(BaseFilterResponseModel baseFilterResponseModel)
        {
            //++Price options

            //There are 3 options available for now
            var prOptionValue1 = _catalogSettings.FilterPriceRangeOption1 > 0 ? _catalogSettings.FilterPriceRangeOption1 : 10;
            var prOptionValue2 = _catalogSettings.FilterPriceRangeOption2 > 0 ? _catalogSettings.FilterPriceRangeOption2 : 20;
            var prOptionValue3 = _catalogSettings.FilterPriceRangeOption3 > 0 ? _catalogSettings.FilterPriceRangeOption3 : 30;
            var priceRangeOption1 = string.Format(await _localizationService.GetResourceAsync("Alchub.Catalog.Filter.PriceRange.Option.Text"), prOptionValue1);
            var priceRangeOption2 = string.Format(await _localizationService.GetResourceAsync("Alchub.Catalog.Filter.PriceRange.Option.Text"), prOptionValue2);
            var priceRangeOption3 = string.Format(await _localizationService.GetResourceAsync("Alchub.Catalog.Filter.PriceRange.Option.Text"), prOptionValue3);

            var priceOptions = new List<SelectListItem>
            {
                new SelectListItem { Text = priceRangeOption1, Value = prOptionValue1.ToString() },
                new SelectListItem { Text = priceRangeOption2, Value = prOptionValue2.ToString() },
                new SelectListItem { Text = priceRangeOption3, Value = prOptionValue3.ToString() }
            };

            baseFilterResponseModel.PriceOptions = priceOptions;

            //--Price options

            await Task.CompletedTask;
        }

        /// <summary>
        /// Prepare sort options
        /// </summary>
        /// <param name="baseFilterRequestModel"></param>
        /// <param name="baseFilterResponseModel"></param>
        /// <returns></returns>
        private async Task PrepareSortOptionsModel(BaseFilterRequestModel baseFilterRequestModel, BaseFilterResponseModel baseFilterResponseModel)
        {
            //get active sorting options
            var activeSortingOptionsIds = Enum.GetValues(typeof(ProductSortingEnum)).Cast<int>()
                .Except(_catalogSettings.ProductSortingEnumDisabled).ToList();

            if (activeSortingOptionsIds?.Any() == true)
            {
                //order sorting options
                var orderedActiveSortingOptions = activeSortingOptionsIds
                    .Select(id => new { Id = id, Order = _catalogSettings.ProductSortingEnumDisplayOrder.TryGetValue(id, out var order) ? order : id })
                .OrderBy(option => option.Order).ToList();

                var orderBy = baseFilterRequestModel.OrderBy ?? orderedActiveSortingOptions.FirstOrDefault().Id;
                //prepare available model sorting options
                foreach (var option in orderedActiveSortingOptions)
                {
                    baseFilterResponseModel.AvailableSortOptions.Add(new SelectListItem
                    {
                        Text = await _localizationService.GetLocalizedEnumAsync((ProductSortingEnum)option.Id),
                        Value = option.Id.ToString(),
                        Selected = option.Id == orderBy
                    });
                }
            }

            await Task.CompletedTask;
        }

        /// <summary>
        /// Prepare filterable manufacturers from result
        /// </summary>
        /// <param name="manufacturersFilterResult"></param>
        /// <param name="selectedManufacturerIds"></param>
        /// <param name="manufacturerFilterResponseModel"></param>
        /// <returns></returns>
        private async Task PrepareFilterableManufacturerModelsFromResult(IList<ManufacturerFilterResult> manufacturersFilterResult, List<int> selectedManufacturerIds, ManufacturerFilterResponseModel manufacturerFilterResponseModel)
        {
            //split result by entity and prepare model
            var manufacturersResult = manufacturersFilterResult.Where(r => r.EntityName == BRAND_ENTITY_NAME && r.ManufactureId.HasValue && r?.ManufactureId > 0);
            var manufacturersModel = new List<ManufacturerFilterResponseModel.ManufacturerFilterModelV1>();
            foreach (var result in manufacturersResult?.OrderBy(x => x.DisplayOrder))
            {
                if (result.ManufactureId.HasValue && result?.ManufactureId > 0)
                {
                    manufacturersModel.Add(new ManufacturerFilterResponseModel.ManufacturerFilterModelV1
                    {
                        Id = result.ManufactureId.Value,
                        Name = result.Manufacturer,
                        NumberOfProducts = result.ManufacturerCount,
                        Selected = selectedManufacturerIds.Any(sc => sc.Equals(result.ManufactureId))
                    });
                }
            }
            manufacturerFilterResponseModel.Manufacturers = manufacturersModel;

            await Task.CompletedTask;
        }

        /// <summary>
        /// Prepare filterable vendors from result
        /// </summary>
        /// <param name="vendorsFilterResult"></param>
        /// <param name="selectedVendorIds"></param>
        /// <param name="vendorFilterResponseModel"></param>
        /// <returns></returns>
        private async Task PrepareFilterableVendorModelsFromResult(IList<VendorFilterResult> vendorsFilterResult, List<int> selectedVendorIds, VendorFilterResponseModel vendorFilterResponseModel)
        {
            //split result by entity and prepare model
            var vendorsResult = vendorsFilterResult.Where(r => r.EntityName == VENDOR_ENTITY_NAME && r.VendorId.HasValue && r?.VendorId > 0);
            var vendorsModel = new List<VendorFilterResponseModel.VendorFilterModelV1>();
            foreach (var result in vendorsResult?.OrderBy(x => x.DisplayOrder))
            {
                if (result.VendorId.HasValue && result?.VendorId > 0)
                {
                    vendorsModel.Add(new VendorFilterResponseModel.VendorFilterModelV1
                    {
                        Id = result.VendorId.Value,
                        Name = result.Vendor,
                        NumberOfProducts = result.VendorCount,
                        Selected = selectedVendorIds.Any(sc => sc.Equals(result.VendorId))
                    });
                }
            }
            vendorFilterResponseModel.Vendors = vendorsModel;

            await Task.CompletedTask;
        }

        /// <summary>
        /// Prepare filterable specification attribute options from result
        /// </summary>
        /// <param name="specOptionsFilterResult"></param>
        /// <param name="selectedOptionIds"></param>
        /// <param name="specificationAttributeId"></param>
        /// <param name="specOptionsFilterResponseModel"></param>
        /// <returns></returns>
        private async Task PrepareFilterableSpecOptionModelsFromResult(IList<SpecificationAttributeOptionFilterResult> specOptionsFilterResult,
            List<int> selectedOptionIds,
            int specificationAttributeId,
            SpecificationAttributeOptionFilterResponseModel specOptionsFilterResponseModel)
        {
            //split result by entity and prepare model
            var specOptionsResult = specOptionsFilterResult.Where(r => r.SpecificationOptionId.HasValue && r?.SpecificationOptionId > 0 && r.Id == specificationAttributeId);
            var specOptionModel = new List<SpecificationAttributeOptionFilterResponseModel.SpecificationAttributeValueFilterModelV1>();
            foreach (var result in specOptionsResult?.OrderBy(x => x.DisplayOrder))
            {
                if (result.SpecificationOptionId.HasValue && result?.SpecificationOptionId > 0)
                {
                    specOptionModel.Add(new SpecificationAttributeOptionFilterResponseModel.SpecificationAttributeValueFilterModelV1
                    {
                        Id = result.SpecificationOptionId.Value,
                        Name = result.SpecificationOptionName,
                        NumberOfProducts = result.ItemCount,
                        Selected = selectedOptionIds.Any(sc => sc.Equals(result.SpecificationOptionId))
                    });
                }
            }
            specOptionsFilterResponseModel.SpecOptions = specOptionModel;

            await Task.CompletedTask;
        }

        /// <summary>
        /// Load specification response using method2
        /// </summary>
        /// <param name="specOptionsFilterRequestModel"></param>
        /// <param name="customer"></param>
        /// <returns></returns>
        private async Task<SpecificationAttributeOptionFilterResponseModel> PreprareSpecificationAttributeOptionResponseMethod2(SpecificationAttributeOptionFilterRequestModel specOptionsFilterRequestModel, Customer customer)
        {
            //prepare sub category ids if any and set correct root category id if required.
            var (rootCategoryId, subCategoryIds) = await PrepareSubCategoryIdsAndSetRootCategoryIdParametor(specOptionsFilterRequestModel.RootCategoryId, specOptionsFilterRequestModel.SubCategoryIds);
            specOptionsFilterRequestModel.RootCategoryId = rootCategoryId;

            //available vendors
            var vendorIds = new List<int>();
            if (specOptionsFilterRequestModel?.VendorIds != null && specOptionsFilterRequestModel.VendorIds.Any())
                vendorIds = specOptionsFilterRequestModel.VendorIds;
            else
                vendorIds = (await _workContext.GetCurrentCustomerGeoRadiusVendorIdsAsync(customer)).ToList();

            bool requestSpecMenuSaperatly = false;
            //get value by removing selected specAttribute Option Ids to get all result for selected attribute.
            var attributeSpecOptions = await _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttributeAsync(specOptionsFilterRequestModel.SpecificationAttributeId);
            var valuesSpecOptionIds = new List<int>();
            //valuesSpecOptionIds = specOptionsFilterRequestModel?.SpecificationOptionIds;
            if (specOptionsFilterRequestModel.SpecificationOptionIds != null && specOptionsFilterRequestModel.SpecificationOptionIds.Any())
            {
                valuesSpecOptionIds.AddRange(specOptionsFilterRequestModel?.SpecificationOptionIds);
                var attributeSpecOptionIds = attributeSpecOptions.Select(x => x.Id);
                var selectedAttValuesSpecOptionIds = attributeSpecOptionIds.Where(x => specOptionsFilterRequestModel.SpecificationOptionIds.Contains(x)).ToList();
                if (selectedAttValuesSpecOptionIds.Any())
                {
                    valuesSpecOptionIds.RemoveAll(x => selectedAttValuesSpecOptionIds.Contains(x));
                    requestSpecMenuSaperatly = true;
                }
            }

            //get result using sp call
            var specOptionsFilterResult = await _catalogService.GetSpecificationAttributeOptionFilterResultAsync(rootCategoryId,
                specOptionsFilterRequestModel.SpecificationAttributeId,
                vendorIds,
                subCategoryIds,
                specOptionsFilterRequestModel.ManufacturerIds,
                valuesSpecOptionIds);

            var specOptionsResponseModel = new SpecificationAttributeOptionFilterResponseModel();

            //spec options
            await PrepareFilterableSpecOptionModelsFromResult(specOptionsFilterResult,
                specOptionsFilterRequestModel.VendorIds ?? new List<int>(),
                specOptionsFilterRequestModel.SpecificationAttributeId,
                specOptionsResponseModel);

            if (requestSpecMenuSaperatly)
            {
                //get result using sp call
                specOptionsFilterResult = await _catalogService.GetSpecificationAttributeOptionFilterResultAsync(rootCategoryId,
                    specOptionsFilterRequestModel.SpecificationAttributeId,
                    vendorIds,
                    subCategoryIds,
                    specOptionsFilterRequestModel.ManufacturerIds,
                    specOptionsFilterRequestModel.SpecificationOptionIds);
            }

            //spec attribute
            specOptionsResponseModel.SpecificationAttributes = await PrepareFilterableSpecificationAttributeListModel(specOptionsFilterResult, specOptionsFilterRequestModel.SpecificationAttributeId);

            return specOptionsResponseModel;
        }

        #endregion

        /// <summary>
        /// Prepare base filter response model
        /// </summary>
        /// <param name="baseFilterRequestModel"></param>
        /// <param name="customer"></param>
        /// <returns></returns>
        public async Task<BaseFilterResponseModel> PreprareBaseFilterResponseModel(BaseFilterRequestModel baseFilterRequestModel, Customer customer)
        {
            //prepare sub category ids if any and set correct root category id if required.
            var (rootCategoryId, subCategoryIds) = await PrepareSubCategoryIdsAndSetRootCategoryIdParametor(baseFilterRequestModel.RootCategoryId, baseFilterRequestModel.SubCategoryIds);
            baseFilterRequestModel.RootCategoryId = rootCategoryId;

            //available vendors
            var vendorIds = (await _workContext.GetCurrentCustomerGeoRadiusVendorIdsAsync(customer)).ToList();

            //get result using sp call
            var baseFilterResult = await _catalogService.GetBaseFilterResultAsync(rootCategoryId, vendorIds, subCategoryIds ?? new List<int>());

            var baseResponseModel = new BaseFilterResponseModel();

            //sub categories
            await PrepareFilterableSubCategoryModelsFromResult(baseFilterResult, subCategoryIds, baseResponseModel);

            //spec attribute
            baseResponseModel.SpecificationAttributes = await PrepareFilterableSpecificationAttributeListModel(baseFilterResult);

            //price options
            await PrepareFilterablePriceOptionsModel(baseResponseModel);

            //sort options
            await PrepareSortOptionsModel(baseFilterRequestModel, baseResponseModel);

            return baseResponseModel;
        }

        /// <summary>
        /// Prepare base filter response model
        /// </summary>
        /// <param name="manufacturerFilterRequestModel"></param>
        /// <param name="customer"></param>
        /// <returns></returns>
        public async Task<ManufacturerFilterResponseModel> PreprareManufacturerFilterResponseModel(ManufacturerFilterRequestModel manufacturerFilterRequestModel, Customer customer)
        {
            //prepare sub category ids if any and set correct root category id if required.
            var (rootCategoryId, subCategoryIds) = await PrepareSubCategoryIdsAndSetRootCategoryIdParametor(manufacturerFilterRequestModel.RootCategoryId, manufacturerFilterRequestModel.SubCategoryIds);
            manufacturerFilterRequestModel.RootCategoryId = rootCategoryId;

            //available vendors
            var vendorIds = (await _workContext.GetCurrentCustomerGeoRadiusVendorIdsAsync(customer)).ToList();

            //get result using sp call
            var manufacturersFilterResult = await _catalogService.GetManufacturerFilterResultAsync(rootCategoryId,
                vendorIds,
                subCategoryIds,
                manufacturerFilterRequestModel.ManufacturerIds);

            var manufacturerResponseModel = new ManufacturerFilterResponseModel();

            //manufacturers
            await PrepareFilterableManufacturerModelsFromResult(manufacturersFilterResult,
                manufacturerFilterRequestModel.ManufacturerIds ?? new List<int>(),
                manufacturerResponseModel);

            //spec attribute
            manufacturerResponseModel.SpecificationAttributes = await PrepareFilterableSpecificationAttributeListModel(manufacturersFilterResult);

            return manufacturerResponseModel;
        }

        /// <summary>
        /// Prepare vendor filter response model
        /// </summary>
        /// <param name="vendorFilterRequestModel"></param>
        /// <param name="customer"></param>
        /// <returns></returns>
        public async Task<VendorFilterResponseModel> PreprareVendorFilterResponseModel(VendorFilterRequestModel vendorFilterRequestModel, Customer customer)
        {
            //prepare sub category ids if any and set correct root category id if required.
            var (rootCategoryId, subCategoryIds) = await PrepareSubCategoryIdsAndSetRootCategoryIdParametor(vendorFilterRequestModel.RootCategoryId, vendorFilterRequestModel.SubCategoryIds);
            vendorFilterRequestModel.RootCategoryId = rootCategoryId;

            //available vendors
            var vendorIds = new List<int>();
            var availableVendorIds = (await _workContext.GetCurrentCustomerGeoRadiusVendorIdsAsync(customer)).ToList();
            if (vendorFilterRequestModel?.VendorIds != null && vendorFilterRequestModel.VendorIds.Any())
                vendorIds = vendorFilterRequestModel.VendorIds;
            else
                vendorIds = availableVendorIds;

            //get result using sp call
            var vendorsFilterResult = await _catalogService.GetVendorFilterResultAsync(rootCategoryId,
                vendorIds,
                subCategoryIds,
                vendorFilterRequestModel.ManufacturerIds,
                availableVendorIds);

            var vendorResponseModel = new VendorFilterResponseModel();

            //vendors
            await PrepareFilterableVendorModelsFromResult(vendorsFilterResult,
                vendorFilterRequestModel.VendorIds ?? new List<int>(),
                vendorResponseModel);

            //spec attribute
            vendorResponseModel.SpecificationAttributes = await PrepareFilterableSpecificationAttributeListModel(vendorsFilterResult);

            return vendorResponseModel;
        }

        /// <summary>
        /// Prepare specification attribute options filter response model
        /// </summary>
        /// <param name="specOptionsFilterRequestModel"></param>
        /// <param name="customer"></param>
        /// <returns></returns>
        public async Task<SpecificationAttributeOptionFilterResponseModel> PreprareSpecificationAttributeOptionFilterResponseModel(SpecificationAttributeOptionFilterRequestModel specOptionsFilterRequestModel, Customer customer)
        {
            /// Load specification data using method2 or method1
            /// Method1: Default - all specification option ids check apply to load both structure and data.
            /// Method2: selected Specification option ids exept requestd attribute's will be apply to load data, and all selected specification option ids will apply to load struture data.

            //Method2. 
            //Note: Removed Method1, as we'll load data using Method2 only.
            return await PreprareSpecificationAttributeOptionResponseMethod2(specOptionsFilterRequestModel, customer);
        }

        /// <summary>
        /// Prepare filter root categories response model
        /// </summary>
        /// <returns></returns>
        public async Task<RootCategoriesResponseModel> PreprareRootCategoriesResponseModel()
        {
            //categories - load all, we know its cached
            var rootCategories = (await _categoryService.GetAllCategoriesAsync()).Where(c => c.ParentCategoryId == 0);

            var rootCategoriesList = new List<RootCategoriesResponseModel.FilterRootCategoryModel>();
            foreach (var category in rootCategories)
            {
                var categoryModel = new RootCategoriesResponseModel.FilterRootCategoryModel()
                {
                    Id = category.Id,
                    Name = category.Name,
                    DsiplayOrder = category.DisplayOrder
                };
                rootCategoriesList.Add(categoryModel);
            }

            var responseModel = new RootCategoriesResponseModel()
            {
                RootCategories = rootCategoriesList
            };

            return responseModel;
        }

        #endregion

        #endregion
    }
}