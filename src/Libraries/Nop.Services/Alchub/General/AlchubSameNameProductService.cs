using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core.Alchub.Domain;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Discounts;
using Nop.Data;
using Nop.Services.Catalog;
using Nop.Services.ExportImport;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Seo;
using Nop.Services.Stores;

namespace Nop.Services.Alchub.General
{
    /// <summary>
    /// Alchub service
    /// </summary>
    public class AlchubSameNameProductService : IAlchubSameNameProductService
    {
        #region Fields
        private readonly IRepository<Product> _productRepository;
        private readonly IProductService _productService;
        private readonly IProductTemplateService _productTemplateService;
        private readonly AlchubSettings _alchubSettings;
        private readonly ICategoryService _categoryService;
        private readonly ILanguageService _languageService;
        private readonly ILocalizationService _localizationService;
        private readonly ILocalizedEntityService _localizedEntityService;
        private readonly IManufacturerService _manufacturerService;
        private readonly IPictureService _pictureService;
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly IProductAttributeService _productAttributeService;
        private readonly IProductTagService _productTagService;
        private readonly ISpecificationAttributeService _specificationAttributeService;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly ILogger _logger;
        private readonly IRepository<ProductManufacturer> _productManufactureRepositoy;
        private readonly IRepository<Manufacturer> _manufactureRepositoy;
        private readonly IRepository<ProductSpecificationAttribute> _productSpecificationAttributeRepository;
        private readonly IRepository<SpecificationAttribute> _specificationAttributeRepository;
        private readonly IRepository<SpecificationAttributeOption> _specificationAttributeOptionRepository;

        #endregion

        #region Ctor

        public AlchubSameNameProductService(
            IRepository<Product> productRepository,
            IProductService productService,
            IProductTemplateService productTemplateService,
            AlchubSettings alchubSettings,
             ICategoryService categoryService,
            ILanguageService languageService,
            ILocalizationService localizationService,
            ILocalizedEntityService localizedEntityService,
            IManufacturerService manufacturerService,
            IPictureService pictureService,
            IProductAttributeParser productAttributeParser,
            IProductAttributeService productAttributeService,
            IProductTagService productTagService,
            ISpecificationAttributeService specificationAttributeService,
            IStoreMappingService storeMappingService,
            IUrlRecordService urlRecordService,
            ILogger logger,
            IRepository<ProductManufacturer> productManufactureRepositoy,
            IRepository<Manufacturer> manufactureRepositoy,
            IRepository<ProductSpecificationAttribute> productSpecificationAttributeRepository,
            IRepository<SpecificationAttribute> specificationAttributeRepository,
            IRepository<SpecificationAttributeOption> specificationAttributeOptionRepository)
        {
            _productRepository = productRepository;
            _productService = productService;
            _productTemplateService = productTemplateService;
            _alchubSettings = alchubSettings;
            _categoryService = categoryService;
            _languageService = languageService;
            _localizationService = localizationService;
            _localizedEntityService = localizedEntityService;
            _manufacturerService = manufacturerService;
            _pictureService = pictureService;
            _productAttributeParser = productAttributeParser;
            _productAttributeService = productAttributeService;
            _productTagService = productTagService;
            _specificationAttributeService = specificationAttributeService;
            _storeMappingService = storeMappingService;
            _urlRecordService = urlRecordService;
            _logger = logger;
            _productManufactureRepositoy = productManufactureRepositoy;
            _manufactureRepositoy = manufactureRepositoy;
            _productSpecificationAttributeRepository = productSpecificationAttributeRepository;
            _specificationAttributeRepository = specificationAttributeRepository;
            _specificationAttributeOptionRepository = specificationAttributeOptionRepository;
        }

        #endregion

        #region Utilities

        /// <summary>
        ///  Get sort order by variant 
        /// </summary>
        /// <param name="variantName"></param>
        /// <returns></returns>
        private int GetVariantSortOrder(string variantName)
        {
            var variantsList = _alchubSettings.AlchubGroupProductsTheirVariantsOrderByVariantName?.Split(',')
         .Select(s => s.Trim())
         .ToList();

            if (variantsList == null || variantsList.Count == 0 || string.IsNullOrEmpty(variantName))
                return variantsList?.Count + 1 ?? 0; // Set a sort order greater than the list variant for variantss not found in the list

            var orderedVariants = variantsList
                .Select((value, index) => new { Value = value, Index = index + 1 })
                .ToDictionary(x => x.Value, x => x.Index);

            if (orderedVariants.ContainsKey(variantName))
                return orderedVariants[variantName];

            return variantsList.Count + 1; // Set a sort order greater than the list variant for variants not found in the list
        }

        /// <summary>
        ///  Get Size sort order
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        private int GetSizeSortOrder(string size)
        {
            var sizeList = _alchubSettings.AlchubGroupProductsTheirVariantsOrderBySizes?.Split(',')
         .Select(s => s.Trim())
         .ToList();

            if (sizeList == null || sizeList.Count == 0 || string.IsNullOrEmpty(size))
                return sizeList?.Count + 1 ?? 0; // Set a sort order greater than the list size for sizes not found in the list

            var orderedSizes = sizeList
                .Select((value, index) => new { Value = value, Index = index + 1 })
                .ToDictionary(x => x.Value, x => x.Index);

            if (orderedSizes.ContainsKey(size))
                return orderedSizes[size];

            return sizeList.Count + 1; // Set a sort order greater than the list size for sizes not found in the list
        }

        /// <summary>
        /// Get conatiner sort order
        /// </summary>
        /// <param name="container"></param>
        /// <returns></returns>
        private int GetContainerSortOrder(string container)
        {
            var containerList = _alchubSettings.AlchubGroupProductsTheirVariantsOrderByContainers?.Split(',')
        .Select(c => c.Trim())
        .ToList();

            if (containerList == null || containerList.Count == 0 || string.IsNullOrEmpty(container))
                return containerList?.Count + 1 ?? 0; // Set a sort order greater than the list size for containers not found in the list

            int index = containerList.FindIndex(c => c.Equals(container, StringComparison.OrdinalIgnoreCase));
            if (index >= 0)
                return index + 1; // Add 1 to the index to shift the sort order (assuming the list starts from 1)

            return containerList.Count + 1; // Set a sort order greater than the list size for containers not found in the list
        }


        /// <summary>
        /// Copy discount mappings
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="productCopy">New product</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        protected virtual async Task CopyDiscountsMappingAsync(Product product, Product productCopy)
        {
            foreach (var discountMapping in await _productService.GetAllDiscountsAppliedToProductAsync(product.Id))
            {
                await _productService.InsertDiscountProductMappingAsync(new DiscountProductMapping { EntityId = productCopy.Id, DiscountId = discountMapping.DiscountId });
                await _productService.UpdateProductAsync(productCopy);
            }
        }

        /// <summary>
        /// Copy tier prices
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="productCopy">New product</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        protected virtual async Task CopyTierPricesAsync(Product product, Product productCopy)
        {
            foreach (var tierPrice in await _productService.GetTierPricesByProductAsync(product.Id))
                await _productService.InsertTierPriceAsync(new TierPrice
                {
                    ProductId = productCopy.Id,
                    StoreId = tierPrice.StoreId,
                    CustomerRoleId = tierPrice.CustomerRoleId,
                    Quantity = tierPrice.Quantity,
                    Price = tierPrice.Price,
                    StartDateTimeUtc = tierPrice.StartDateTimeUtc,
                    EndDateTimeUtc = tierPrice.EndDateTimeUtc
                });
        }

        /// <summary>
        /// Copy attributes mapping
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="productCopy">New product</param>
        /// <param name="originalNewPictureIdentifiers">Identifiers of pictures</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        protected virtual async Task CopyAttributesMappingAsync(Product product, Product productCopy, Dictionary<int, int> originalNewPictureIdentifiers)
        {
            var associatedAttributes = new Dictionary<int, int>();
            var associatedAttributeValues = new Dictionary<int, int>();

            //attribute mapping with condition attributes
            var oldCopyWithConditionAttributes = new List<ProductAttributeMapping>();

            //all product attribute mapping copies
            var productAttributeMappingCopies = new Dictionary<int, ProductAttributeMapping>();

            var languages = await _languageService.GetAllLanguagesAsync(true);

            foreach (var productAttributeMapping in await _productAttributeService.GetProductAttributeMappingsByProductIdAsync(product.Id))
            {
                var productAttributeMappingCopy = new ProductAttributeMapping
                {
                    ProductId = productCopy.Id,
                    ProductAttributeId = productAttributeMapping.ProductAttributeId,
                    TextPrompt = productAttributeMapping.TextPrompt,
                    IsRequired = productAttributeMapping.IsRequired,
                    AttributeControlTypeId = productAttributeMapping.AttributeControlTypeId,
                    DisplayOrder = productAttributeMapping.DisplayOrder,
                    ValidationMinLength = productAttributeMapping.ValidationMinLength,
                    ValidationMaxLength = productAttributeMapping.ValidationMaxLength,
                    ValidationFileAllowedExtensions = productAttributeMapping.ValidationFileAllowedExtensions,
                    ValidationFileMaximumSize = productAttributeMapping.ValidationFileMaximumSize,
                    DefaultValue = productAttributeMapping.DefaultValue
                };
                await _productAttributeService.InsertProductAttributeMappingAsync(productAttributeMappingCopy);
                //localization
                foreach (var lang in languages)
                {
                    var textPrompt = await _localizationService.GetLocalizedAsync(productAttributeMapping, x => x.TextPrompt, lang.Id, false, false);
                    if (!string.IsNullOrEmpty(textPrompt))
                        await _localizedEntityService.SaveLocalizedValueAsync(productAttributeMappingCopy, x => x.TextPrompt, textPrompt,
                            lang.Id);
                }

                productAttributeMappingCopies.Add(productAttributeMappingCopy.Id, productAttributeMappingCopy);

                if (!string.IsNullOrEmpty(productAttributeMapping.ConditionAttributeXml))
                {
                    oldCopyWithConditionAttributes.Add(productAttributeMapping);
                }

                //save associated value (used for combinations copying)
                associatedAttributes.Add(productAttributeMapping.Id, productAttributeMappingCopy.Id);

                // product attribute values
                var productAttributeValues = await _productAttributeService.GetProductAttributeValuesAsync(productAttributeMapping.Id);
                foreach (var productAttributeValue in productAttributeValues)
                {
                    var attributeValuePictureId = 0;
                    if (originalNewPictureIdentifiers.ContainsKey(productAttributeValue.PictureId))
                        attributeValuePictureId = originalNewPictureIdentifiers[productAttributeValue.PictureId];

                    var attributeValueCopy = new ProductAttributeValue
                    {
                        ProductAttributeMappingId = productAttributeMappingCopy.Id,
                        AttributeValueTypeId = productAttributeValue.AttributeValueTypeId,
                        AssociatedProductId = productAttributeValue.AssociatedProductId,
                        Name = productAttributeValue.Name,
                        ColorSquaresRgb = productAttributeValue.ColorSquaresRgb,
                        PriceAdjustment = productAttributeValue.PriceAdjustment,
                        PriceAdjustmentUsePercentage = productAttributeValue.PriceAdjustmentUsePercentage,
                        WeightAdjustment = productAttributeValue.WeightAdjustment,
                        Cost = productAttributeValue.Cost,
                        CustomerEntersQty = productAttributeValue.CustomerEntersQty,
                        Quantity = productAttributeValue.Quantity,
                        IsPreSelected = productAttributeValue.IsPreSelected,
                        DisplayOrder = productAttributeValue.DisplayOrder,
                        PictureId = attributeValuePictureId,
                    };
                    //picture associated to "iamge square" attribute type (if exists)
                    if (productAttributeValue.ImageSquaresPictureId > 0)
                    {
                        var origImageSquaresPicture =
                            await _pictureService.GetPictureByIdAsync(productAttributeValue.ImageSquaresPictureId);
                        if (origImageSquaresPicture != null)
                        {
                            //copy the picture
                            var imageSquaresPictureCopy = await _pictureService.InsertPictureAsync(
                                await _pictureService.LoadPictureBinaryAsync(origImageSquaresPicture),
                                origImageSquaresPicture.MimeType,
                                origImageSquaresPicture.SeoFilename,
                                origImageSquaresPicture.AltAttribute,
                                origImageSquaresPicture.TitleAttribute);
                            attributeValueCopy.ImageSquaresPictureId = imageSquaresPictureCopy.Id;
                        }
                    }

                    await _productAttributeService.InsertProductAttributeValueAsync(attributeValueCopy);

                    //save associated value (used for combinations copying)
                    associatedAttributeValues.Add(productAttributeValue.Id, attributeValueCopy.Id);

                    //localization
                    foreach (var lang in languages)
                    {
                        var name = await _localizationService.GetLocalizedAsync(productAttributeValue, x => x.Name, lang.Id, false, false);
                        if (!string.IsNullOrEmpty(name))
                            await _localizedEntityService.SaveLocalizedValueAsync(attributeValueCopy, x => x.Name, name, lang.Id);
                    }
                }
            }

            //copy attribute conditions
            foreach (var productAttributeMapping in oldCopyWithConditionAttributes)
            {
                var oldConditionAttributeMapping = (await _productAttributeParser
                    .ParseProductAttributeMappingsAsync(productAttributeMapping.ConditionAttributeXml)).FirstOrDefault();

                if (oldConditionAttributeMapping == null)
                    continue;

                var oldConditionValues = await _productAttributeParser.ParseProductAttributeValuesAsync(
                    productAttributeMapping.ConditionAttributeXml,
                    oldConditionAttributeMapping.Id);

                if (!oldConditionValues.Any())
                    continue;

                var newAttributeMappingId = associatedAttributes[oldConditionAttributeMapping.Id];
                var newConditionAttributeMapping = productAttributeMappingCopies[newAttributeMappingId];

                var newConditionAttributeXml = string.Empty;

                foreach (var oldConditionValue in oldConditionValues)
                {
                    newConditionAttributeXml = _productAttributeParser.AddProductAttribute(newConditionAttributeXml,
                        newConditionAttributeMapping, associatedAttributeValues[oldConditionValue.Id].ToString());
                }

                var attributeMappingId = associatedAttributes[productAttributeMapping.Id];
                var conditionAttribute = productAttributeMappingCopies[attributeMappingId];
                conditionAttribute.ConditionAttributeXml = newConditionAttributeXml;

                await _productAttributeService.UpdateProductAttributeMappingAsync(conditionAttribute);
            }

            //attribute combinations
            foreach (var combination in await _productAttributeService.GetAllProductAttributeCombinationsAsync(product.Id))
            {
                //generate new AttributesXml according to new value IDs
                var newAttributesXml = string.Empty;
                var parsedProductAttributes = await _productAttributeParser.ParseProductAttributeMappingsAsync(combination.AttributesXml);
                foreach (var oldAttribute in parsedProductAttributes)
                {
                    if (!associatedAttributes.ContainsKey(oldAttribute.Id))
                        continue;

                    var newAttribute = await _productAttributeService.GetProductAttributeMappingByIdAsync(associatedAttributes[oldAttribute.Id]);

                    if (newAttribute == null)
                        continue;

                    var oldAttributeValuesStr = _productAttributeParser.ParseValues(combination.AttributesXml, oldAttribute.Id);

                    foreach (var oldAttributeValueStr in oldAttributeValuesStr)
                    {
                        if (newAttribute.ShouldHaveValues())
                        {
                            //attribute values
                            var oldAttributeValue = int.Parse(oldAttributeValueStr);
                            if (!associatedAttributeValues.ContainsKey(oldAttributeValue))
                                continue;

                            var newAttributeValue = await _productAttributeService.GetProductAttributeValueByIdAsync(associatedAttributeValues[oldAttributeValue]);

                            if (newAttributeValue != null)
                            {
                                newAttributesXml = _productAttributeParser.AddProductAttribute(newAttributesXml,
                                    newAttribute, newAttributeValue.Id.ToString());
                            }
                        }
                        else
                        {
                            //just a text
                            newAttributesXml = _productAttributeParser.AddProductAttribute(newAttributesXml,
                                newAttribute, oldAttributeValueStr);
                        }
                    }
                }

                //picture
                originalNewPictureIdentifiers.TryGetValue(combination.PictureId, out var combinationPictureId);

                var combinationCopy = new ProductAttributeCombination
                {
                    ProductId = productCopy.Id,
                    AttributesXml = newAttributesXml,
                    StockQuantity = combination.StockQuantity,
                    MinStockQuantity = combination.MinStockQuantity,
                    AllowOutOfStockOrders = combination.AllowOutOfStockOrders,
                    Sku = combination.Sku,
                    ManufacturerPartNumber = combination.ManufacturerPartNumber,
                    Gtin = combination.Gtin,
                    OverriddenPrice = combination.OverriddenPrice,
                    NotifyAdminForQuantityBelow = combination.NotifyAdminForQuantityBelow,
                    PictureId = combinationPictureId
                };
                await _productAttributeService.InsertProductAttributeCombinationAsync(combinationCopy);

                //quantity change history
                await _productService.AddStockQuantityHistoryEntryAsync(productCopy, combination.StockQuantity,
                    combination.StockQuantity,
                    message: string.Format(await _localizationService.GetResourceAsync("Admin.StockQuantityHistory.Messages.CopyProduct"), product.Id), combinationId: combination.Id);
            }
        }

        /// <summary>
        /// Copy product specifications
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="productCopy">New product</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        protected virtual async Task CopyProductSpecificationsAsync(Product product, Product productCopy)
        {
            var allLanguages = await _languageService.GetAllLanguagesAsync();

            foreach (var productSpecificationAttribute in await _specificationAttributeService.GetProductSpecificationAttributesAsync(product.Id))
            {
                var psaCopy = new ProductSpecificationAttribute
                {
                    ProductId = productCopy.Id,
                    AttributeTypeId = productSpecificationAttribute.AttributeTypeId,
                    SpecificationAttributeOptionId = productSpecificationAttribute.SpecificationAttributeOptionId,
                    CustomValue = productSpecificationAttribute.CustomValue,
                    AllowFiltering = productSpecificationAttribute.AllowFiltering,
                    ShowOnProductPage = productSpecificationAttribute.ShowOnProductPage,
                    DisplayOrder = productSpecificationAttribute.DisplayOrder
                };

                await _specificationAttributeService.InsertProductSpecificationAttributeAsync(psaCopy);

                foreach (var language in allLanguages)
                {
                    var customValue = await _localizationService.GetLocalizedAsync(productSpecificationAttribute, x => x.CustomValue, language.Id, false, false);
                    if (!string.IsNullOrEmpty(customValue))
                        await _localizedEntityService.SaveLocalizedValueAsync(psaCopy, x => x.CustomValue, customValue, language.Id);
                }
            }
        }

        /// <summary>
        /// Copy crosssell mapping
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="productCopy">New product</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        protected virtual async Task CopyCrossSellsMappingAsync(Product product, Product productCopy)
        {
            foreach (var csProduct in await _productService.GetCrossSellProductsByProductId1Async(product.Id, true))
                await _productService.InsertCrossSellProductAsync(
                    new CrossSellProduct
                    {
                        ProductId1 = productCopy.Id,
                        ProductId2 = csProduct.ProductId2
                    });
        }

        /// <summary>
        /// Copy related products mapping
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="productCopy">New product</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        protected virtual async Task CopyRelatedProductsMappingAsync(Product product, Product productCopy)
        {
            foreach (var relatedProduct in await _productService.GetRelatedProductsByProductId1Async(product.Id, true))
                await _productService.InsertRelatedProductAsync(
                    new RelatedProduct
                    {
                        ProductId1 = productCopy.Id,
                        ProductId2 = relatedProduct.ProductId2,
                        DisplayOrder = relatedProduct.DisplayOrder
                    });
        }

        /// <summary>
        /// Copy manufacturer mapping
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="productCopy">New product</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        protected virtual async Task CopyManufacturersMappingAsync(Product product, Product productCopy)
        {
            foreach (var productManufacturers in await _manufacturerService.GetProductManufacturersByProductIdAsync(product.Id, true))
            {
                var productManufacturerCopy = new ProductManufacturer
                {
                    ProductId = productCopy.Id,
                    ManufacturerId = productManufacturers.ManufacturerId,
                    IsFeaturedProduct = productManufacturers.IsFeaturedProduct,
                    DisplayOrder = productManufacturers.DisplayOrder
                };

                await _manufacturerService.InsertProductManufacturerAsync(productManufacturerCopy);
            }
        }

        /// <summary>
        /// Copy category mapping
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="productCopy">New product</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        protected virtual async Task CopyCategoriesMappingAsync(Product product, Product productCopy)
        {
            foreach (var productCategory in await _categoryService.GetProductCategoriesByProductIdAsync(product.Id, showHidden: true))
            {
                var productCategoryCopy = new ProductCategory
                {
                    ProductId = productCopy.Id,
                    CategoryId = productCategory.CategoryId,
                    IsFeaturedProduct = productCategory.IsFeaturedProduct,
                    DisplayOrder = productCategory.DisplayOrder
                };

                await _categoryService.InsertProductCategoryAsync(productCategoryCopy);
            }
        }

        /// <summary>
        /// Copy warehouse mapping
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="productCopy">New product</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        protected virtual async Task CopyWarehousesMappingAsync(Product product, Product productCopy)
        {
            foreach (var pwi in await _productService.GetAllProductWarehouseInventoryRecordsAsync(product.Id))
            {
                await _productService.InsertProductWarehouseInventoryAsync(
                    new ProductWarehouseInventory
                    {
                        ProductId = productCopy.Id,
                        WarehouseId = pwi.WarehouseId,
                        StockQuantity = pwi.StockQuantity,
                        ReservedQuantity = 0
                    });

                //quantity change history
                var message = $"{await _localizationService.GetResourceAsync("Admin.StockQuantityHistory.Messages.MultipleWarehouses")} {string.Format(await _localizationService.GetResourceAsync("Admin.StockQuantityHistory.Messages.CopyProduct"), product.Id)}";
                await _productService.AddStockQuantityHistoryEntryAsync(productCopy, pwi.StockQuantity, pwi.StockQuantity, pwi.WarehouseId, message);
            }

            await _productService.UpdateProductAsync(productCopy);
        }

        /// <summary>
        /// Copy product pictures
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="newName">New product name</param>
        /// <param name="copyImages"></param>
        /// <param name="productCopy">New product</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the identifiers of old and new pictures
        /// </returns>
        protected virtual async Task<Dictionary<int, int>> CopyProductPicturesAsync(Product product, string newName, bool copyImages, Product productCopy)
        {
            //variable to store original and new picture identifiers
            var originalNewPictureIdentifiers = new Dictionary<int, int>();
            if (!copyImages)
                return originalNewPictureIdentifiers;

            foreach (var productPicture in await _productService.GetProductPicturesByProductIdAsync(product.Id))
            {
                var picture = await _pictureService.GetPictureByIdAsync(productPicture.PictureId);
                var pictureCopy = await _pictureService.InsertPictureAsync(
                    await _pictureService.LoadPictureBinaryAsync(picture),
                    picture.MimeType,
                    await _pictureService.GetPictureSeNameAsync(newName),
                    picture.AltAttribute,
                    picture.TitleAttribute);
                await _productService.InsertProductPictureAsync(new ProductPicture
                {
                    ProductId = productCopy.Id,
                    PictureId = pictureCopy.Id,
                    DisplayOrder = productPicture.DisplayOrder
                });
                originalNewPictureIdentifiers.Add(picture.Id, pictureCopy.Id);
            }

            return originalNewPictureIdentifiers;
        }

        /// <summary>
        /// Copy localization data
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="productCopy">New product</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        protected virtual async Task CopyLocalizationDataAsync(Product product, Product productCopy)
        {
            var languages = await _languageService.GetAllLanguagesAsync(true);

            //localization
            foreach (var lang in languages)
            {
                var name = await _localizationService.GetLocalizedAsync(product, x => x.Name, lang.Id, false, false);
                if (!string.IsNullOrEmpty(name))
                    await _localizedEntityService.SaveLocalizedValueAsync(productCopy, x => x.Name, name, lang.Id);

                var shortDescription = await _localizationService.GetLocalizedAsync(product, x => x.ShortDescription, lang.Id, false, false);
                if (!string.IsNullOrEmpty(shortDescription))
                    await _localizedEntityService.SaveLocalizedValueAsync(productCopy, x => x.ShortDescription, shortDescription, lang.Id);

                var fullDescription = await _localizationService.GetLocalizedAsync(product, x => x.FullDescription, lang.Id, false, false);
                if (!string.IsNullOrEmpty(fullDescription))
                    await _localizedEntityService.SaveLocalizedValueAsync(productCopy, x => x.FullDescription, fullDescription, lang.Id);

                var metaKeywords = await _localizationService.GetLocalizedAsync(product, x => x.MetaKeywords, lang.Id, false, false);
                if (!string.IsNullOrEmpty(metaKeywords))
                    await _localizedEntityService.SaveLocalizedValueAsync(productCopy, x => x.MetaKeywords, metaKeywords, lang.Id);

                var metaDescription = await _localizationService.GetLocalizedAsync(product, x => x.MetaDescription, lang.Id, false, false);
                if (!string.IsNullOrEmpty(metaDescription))
                    await _localizedEntityService.SaveLocalizedValueAsync(productCopy, x => x.MetaDescription, metaDescription, lang.Id);

                var metaTitle = await _localizationService.GetLocalizedAsync(product, x => x.MetaTitle, lang.Id, false, false);
                if (!string.IsNullOrEmpty(metaTitle))
                    await _localizedEntityService.SaveLocalizedValueAsync(productCopy, x => x.MetaTitle, metaTitle, lang.Id);

                //search engine name
                await _urlRecordService.SaveSlugAsync(productCopy, await _urlRecordService.ValidateSeNameAsync(productCopy, string.Empty, name, false), lang.Id);
            }
        }


        protected virtual async Task<IList<Product>> GetRelatedProductsByProductsAsync(int groupProductId, bool isMaster)
        {
            var query = from p in _productRepository.Table
                        where p.ParentGroupedProductId == groupProductId && !p.Deleted && isMaster
                        select p;

            var relatedProducts = await query.ToListAsync();

            return relatedProducts;
        }
        #endregion


        #region Product
        /// <summary>
        /// Gets a master product by upcCode
        /// </summary>
        /// <param name="upcCode">UPCCode</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the product
        /// </returns>
        public async Task<IList<Product>> GetAllMasterProductSameByNameAsync()
        {
            IList<Product> getAllListOfSameName = new List<Product>();
            var query = from p in _productRepository.Table
                        where !p.Deleted && p.IsMaster
                        group p by p.Name into groupedProducts
                        where groupedProducts.Count() > 1
                        select groupedProducts.ToList();

            var productsWithSameName = await query.ToListAsync();


            // Access individual products within each group
            foreach (var group in productsWithSameName)
            {

                // Check if the group contains a group product
                if (group.Any(p => p.ProductType == ProductType.GroupedProduct))
                    continue; // Skip the group if it already has a group product

                foreach (var product in group)
                {
                    getAllListOfSameName.Add(product);
                }
            }
            return getAllListOfSameName;
        }

        /// <summary>
        /// Same product name in database create group products variants its products Async
        /// </summary>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task<bool> SameProductNameCreateGroupProductsAsync(IList<Product> products)
        {
            var syncedSuccessfully = false;
            var groupProductsTheirVariantsOrderBySizes = _alchubSettings.AlchubGroupProductsTheirVariantsOrderBySizes;
            var groupProductsTheirVariantsOrderByContainers = _alchubSettings.AlchubGroupProductsTheirVariantsOrderByContainers;

            // Group products by name
            var groupedProducts = products.GroupBy(p => p.Name).ToList();
            foreach (var group in groupedProducts)
            {
                // Create a group with the first product as the main product
                var mainProduct = group.First();
                try
                {
                    //group product create 
                    Product groupProduct = new Product();

                    //// Associate the main product with the group
                    groupProduct.Name = mainProduct.Name;
                    groupProduct.IsMaster = mainProduct.IsMaster;
                    groupProduct.ShortDescription = mainProduct.ShortDescription;
                    groupProduct.FullDescription = mainProduct.FullDescription;
                    groupProduct.ProductType = ProductType.GroupedProduct;
                    groupProduct.ProductTypeId = (int)ProductType.GroupedProduct;
                    groupProduct.Published = true;
                    groupProduct.VisibleIndividually = true;
                    groupProduct.ImageUrl = mainProduct.ImageUrl;
                    //default values
                    groupProduct.OrderMinimumQuantity = 1;
                    groupProduct.OrderMaximumQuantity = 10000;
                    groupProduct.IsShipEnabled = true;
                    groupProduct.AllowCustomerReviews = true;
                    groupProduct.CreatedOnUtc = DateTime.UtcNow;
                    groupProduct.UpdatedOnUtc = DateTime.UtcNow;
                    //assign default grouped product template
                    var productTemplateGrouped = (await _productTemplateService.GetAllProductTemplatesAsync())?.FirstOrDefault(pt => pt.Name.StartsWith("Grouped", StringComparison.InvariantCultureIgnoreCase));
                    if (productTemplateGrouped != null)
                        groupProduct.ProductTemplateId = productTemplateGrouped.Id;

                    //insert group product
                    await _productService.InsertProductAsync(groupProduct);
                    /// add information log create product name
                    await _logger.InformationAsync(string.Format("Alchub Same Product Name While Create Group Products Name is :- " + groupProduct.Name));

                    //Slug product
                    await _urlRecordService.SaveSlugAsync(groupProduct, await _urlRecordService.ValidateSeNameAsync(groupProduct, string.Empty, groupProduct.Name, true), 0);
                    //product <-> specifications mappings
                    await CopyProductSpecificationsAsync(mainProduct, groupProduct);
                    //product <-> warehouses mappings
                    await CopyWarehousesMappingAsync(mainProduct, groupProduct);
                    //product <-> categories mappings
                    await CopyCategoriesMappingAsync(mainProduct, groupProduct);
                    //product <-> manufacturers mappings
                    await CopyManufacturersMappingAsync(mainProduct, groupProduct);

                    // Associate the remaining products in the group with the group
                    var associatedProducts = group.ToList();
                    if (!string.IsNullOrEmpty(groupProductsTheirVariantsOrderBySizes))
                    {
                        associatedProducts = associatedProducts.OrderBy(p => GetSizeSortOrder(p.Size)).ToList();
                    }
                    else if (!string.IsNullOrEmpty(groupProductsTheirVariantsOrderByContainers))
                    {
                        associatedProducts = associatedProducts.OrderBy(p => GetContainerSortOrder(p.Container)).ToList();
                    }
                    else
                    {
                        // By default, maintain the original order of associated products
                        associatedProducts = associatedProducts.OrderBy(p => p.Id).ToList();
                    }

                    ///Simple products 
                    //// Associate the remaining products in the group with the group
                    int count = 0;
                    foreach (var product in associatedProducts)
                    {
                        var associatedProduct = await _productService.GetProductByIdAsync(product.Id);

                        if (associatedProduct != null)
                        {
                            //update group id assicated products 
                            associatedProduct.ParentGroupedProductId = groupProduct.Id;
                            associatedProduct.ProductType = ProductType.SimpleProduct;
                            associatedProduct.ProductTypeId = (int)ProductType.SimpleProduct;
                            associatedProduct.VisibleIndividually = false;
                            associatedProduct.UpdatedOnUtc = DateTime.UtcNow;
                            associatedProduct.DisplayOrder = count++;
                            associatedProduct.UpdatedOnUtc = DateTime.UtcNow;
                            await _productService.UpdateProductAsync(associatedProduct);
                        }
                    }
                }
                catch (Exception exc)
                {
                    // Handle any exceptions that occurred during the sync operation
                    await _logger.ErrorAsync(string.Format("Alchub Same Product Name While Create Group Products Error Occur Product Name :- " + mainProduct.Name), exc);
                    continue;
                }
            }
            syncedSuccessfully = true; // Set the flag to indicate successful sync
            return syncedSuccessfully;
        }



        /// <summary>
        /// Rearrange sizes and containers of related products within all group products
        /// </summary>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task<bool> ReArrangeAllGroupProductsRelatedItemsAsync()
        {
            try
            {
                var groupProducts = await GetAllGroupProductsAsync();

                foreach (var group in groupProducts)
                {
                    await ReArrangeGroupProductRelatedItemsAsync(group.Id);
                }

                return true; // Rearrangement of all groups successful
            }
            catch (Exception exc)
            {
                // Handle exceptions
                await _logger.ErrorAsync("Error occurred while rearranging group products and related items.", exc);
                return false;
            }
        }


        /// <summary>
        ///  Get All group Products and its Related Products
        /// </summary>
        /// <returns></returns>
        protected virtual async Task<IList<Product>> GetAllGroupProductsAsync()
        {
            var query = from p in _productRepository.Table
                        where !p.Deleted && p.IsMaster && p.ProductTypeId == (int)ProductType.GroupedProduct
                        select p;

            return await query.ToListAsync();
        }


        /// <summary>
        /// Rearrange sizes and containers of related products within a group
        /// </summary>
        /// <param name="groupId">ID of the group to rearrange</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        protected virtual async Task<bool> ReArrangeGroupProductRelatedItemsAsync(int groupId)
        {
            var group = await _productService.GetProductByIdAsync(groupId);

            if (group == null || group.ProductType != ProductType.GroupedProduct)
            {
                return false; // Invalid group or not a grouped product
            }

            try
            {
                // Get associated products of the group
                var associatedProducts = await GetRelatedProductsByProductsAsync(groupId, isMaster: true);

                //Get same name product of still exist in database simple product
                var productSameNameNotLinked = await GetSameByNameWhichIsNotLinkedGroupProductAsync(group.Name);

                // Combine both lists into a single list for sorting
                var allProductsToSort = associatedProducts.Concat(productSameNameNotLinked);

                // Rearrange the sizes and containers of associated products
                var sortedProducts = allProductsToSort
                    .OrderBy(p => GetSizeSortOrder(p.Size))
                    .ThenBy(p => GetContainerSortOrder(p.Container))
                    .ToList();

                int displayOrder = 0;
                foreach (var product in sortedProducts)
                {
                    var associatedProduct = await _productService.GetProductByIdAsync(product.Id);

                    if (associatedProduct != null)
                    {

                        ///Check Variant if name is changed & update to view Individual
                        if (associatedProduct.Name != group.Name && associatedProduct.IsMaster)
                        {
                            //Remove from group product Update the variant and update the master product
                            associatedProduct.ParentGroupedProductId = 0;
                            associatedProduct.VisibleIndividually = true;
                            await _productService.UpdateProductAsync(associatedProduct);
                        }
                        else
                        {
                            associatedProduct.ParentGroupedProductId = group.Id;
                            associatedProduct.VisibleIndividually = false;
                            associatedProduct.DisplayOrder = displayOrder++;
                            await _productService.UpdateProductAsync(associatedProduct);
                        }
                    }
                }

                // Get the list of same name variants
                var sameVariants = sortedProducts.Where(p => p.Name == group.Name).ToList();
                // If there is less than 1 non-master variant left, delete the group & release the variant by setting visible individually to true
                if (sameVariants.Count <= 1)
                {
                    if (sameVariants.Count == 1)
                    {
                        var lastAssociatedProduct = sameVariants[0];
                        lastAssociatedProduct.ParentGroupedProductId = 0;
                        lastAssociatedProduct.VisibleIndividually = true;
                        await _productService.UpdateProductAsync(lastAssociatedProduct);
                    }
                    // Delete the group product
                    await _productService.DeleteProductAsync(group);
                    return true; // Rearrangement and deletion successful
                }
                return true; // Rearrangement successful
            }
            catch (Exception exc)
            {
                // Handle exceptions
                await _logger.ErrorAsync("Error occurred while rearranging group products.", exc);
                return false;
            }
        }


        /// <summary>
        /// Gets a master product by upcCode
        /// </summary>
        /// <param name="upcCode">UPCCode</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the product
        /// </returns>
        protected async Task<List<Product>> GetSameByNameWhichIsNotLinkedGroupProductAsync(string name)
        {
            var products = await _productRepository.Table
        .Where(p => !p.Deleted
                    && p.IsMaster
                    && p.ProductTypeId == (int)ProductType.SimpleProduct
                    && p.ParentGroupedProductId == 0).ToListAsync();

            var productsWithSameName = products
                .Where(p => string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase))
                .ToList();

            return productsWithSameName;
        }
        #endregion

        #region Assemble group product

        /// <summary>
        /// Assemble group produts, which has same Variant specification attribute & has same manufacturer(brand)
        /// </summary>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task<bool> AssembleGroupProductsAsync()
        {
            //load & validate varinat spec att is available.
            var variantSpecificationAttribute = await GetVariantSpecificationAtt();

            var syncedSuccessfully = false;
            var groupProductsTheirVariantsOrderBySizes = _alchubSettings.AlchubGroupProductsTheirVariantsOrderBySizes;
            var groupProductsTheirVariantsOrderByContainers = _alchubSettings.AlchubGroupProductsTheirVariantsOrderByContainers;

            //get products batch with varint and manufature grouping 
            var variantGroups = await GetMasterProductGroupWithSameVariantAndBrandAsync(variantSpecificationAttribute.Name); //temp
            foreach (var group in variantGroups)
            {
                if (group == null || group.Products?.Count <= 1)
                    continue;

                // Create a group with the first product as the main product
                var mainProduct = group.Products.First();
                try
                {
                    //group product create 
                    Product groupProduct = new Product();

                    //// Associate the main product with the group
                    groupProduct.Name = mainProduct.Name;
                    groupProduct.IsMaster = mainProduct.IsMaster;
                    groupProduct.ShortDescription = mainProduct.ShortDescription;
                    groupProduct.FullDescription = mainProduct.FullDescription;
                    groupProduct.ProductType = ProductType.GroupedProduct;
                    groupProduct.ProductTypeId = (int)ProductType.GroupedProduct;
                    groupProduct.Published = true;
                    groupProduct.VisibleIndividually = true;
                    groupProduct.ImageUrl = mainProduct.ImageUrl;
                    //default values
                    groupProduct.OrderMinimumQuantity = 1;
                    groupProduct.OrderMaximumQuantity = 10000;
                    groupProduct.IsShipEnabled = true;
                    groupProduct.AllowCustomerReviews = true;
                    groupProduct.CreatedOnUtc = DateTime.UtcNow;
                    groupProduct.UpdatedOnUtc = DateTime.UtcNow;
                    //assign default grouped product template
                    var productTemplateGrouped = (await _productTemplateService.GetAllProductTemplatesAsync())?.FirstOrDefault(pt => pt.Name.StartsWith("Grouped", StringComparison.InvariantCultureIgnoreCase));
                    if (productTemplateGrouped != null)
                        groupProduct.ProductTemplateId = productTemplateGrouped.Id;

                    //insert group product
                    await _productService.InsertProductAsync(groupProduct);
                    /// add information log create product name
                    await _logger.InformationAsync(string.Format($"New Assembled GroupProduct ({groupProduct.Id}) Name is :- " + groupProduct.Name));

                    //Slug product
                    await _urlRecordService.SaveSlugAsync(groupProduct, await _urlRecordService.ValidateSeNameAsync(groupProduct, string.Empty, groupProduct.Name, true), 0);
                    //product <-> specifications mappings
                    await CopyProductSpecificationsAsync(mainProduct, groupProduct);
                    //product <-> warehouses mappings
                    await CopyWarehousesMappingAsync(mainProduct, groupProduct);
                    //product <-> categories mappings
                    await CopyCategoriesMappingAsync(mainProduct, groupProduct);
                    //product <-> manufacturers mappings
                    await CopyManufacturersMappingAsync(mainProduct, groupProduct);

                    // Associate the remaining products in the group with the group
                    var associatedProducts = group.Products.ToList();
                    if (!string.IsNullOrEmpty(groupProductsTheirVariantsOrderBySizes))
                    {
                        associatedProducts = associatedProducts.OrderBy(p => GetSizeSortOrder(p.Size)).ToList();
                    }
                    else if (!string.IsNullOrEmpty(groupProductsTheirVariantsOrderByContainers))
                    {
                        associatedProducts = associatedProducts.OrderBy(p => GetContainerSortOrder(p.Container)).ToList();
                    }
                    else
                    {
                        // By default, maintain the original order of associated products
                        associatedProducts = associatedProducts.OrderBy(p => p.Id).ToList();
                    }

                    ///Simple products 
                    //// Associate the remaining products in the group with the group
                    int count = 0;
                    foreach (var product in associatedProducts)
                    {
                        var associatedProduct = await _productService.GetProductByIdAsync(product.Id);

                        if (associatedProduct != null)
                        {
                            //update group id assicated products 
                            associatedProduct.ParentGroupedProductId = groupProduct.Id;
                            associatedProduct.ProductType = ProductType.SimpleProduct;
                            associatedProduct.ProductTypeId = (int)ProductType.SimpleProduct;
                            associatedProduct.VisibleIndividually = false;
                            associatedProduct.DisplayOrder = count++;
                            associatedProduct.UpdatedOnUtc = DateTime.UtcNow;
                            await _productService.UpdateProductAsync(associatedProduct);
                        }
                    }
                }
                catch (Exception exc)
                {
                    // Handle any exceptions that occurred during the sync operation
                    await _logger.ErrorAsync(string.Format("Alchub Same Product Name While Create Group Products Error Occur Product Name :- " + mainProduct.Name), exc);
                    continue;
                }
            }
            syncedSuccessfully = true; // Set the flag to indicate successful sync
            return syncedSuccessfully;
        }

        /// <summary>
        /// Rearrange sizes and containers of related products within all group products
        /// </summary>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task<bool> ReArrangeAllGroupProductsAssociatedProductsAsync()
        {
            try
            {
                //load & validate varinat spec att is available.
                var variantSpecificationAttribute = await GetVariantSpecificationAtt();

                //get all group products
                var groupProducts = await GetAllGroupProductsAsync();

                foreach (var groupProduct in groupProducts)
                {
                    await ReArrangeAllGroupProductsAssociatedProductsAsync(groupProduct.Id, variantSpecificationAttribute);
                }

                return true; // Rearrangement of all groups successful
            }
            catch (Exception exc)
            {
                // Handle exceptions
                await _logger.ErrorAsync("Error occurred while rearranging group products and related items.", exc);
                return false;
            }
        }

        /// <summary>
        /// Get grouped products variants disctionary.
        /// </summary>
        /// <param name="groupedProducts"></param>
        /// <param name="manufacturerId"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public virtual async Task<IDictionary<Product, string>> GetGroupedProductVariants(Product groupedProducts, int manufacturerId)
        {
            if (groupedProducts == null)
                throw new ArgumentNullException(nameof(groupedProducts));

            if (manufacturerId == 0)
                throw new ArgumentOutOfRangeException(nameof(manufacturerId));

            //get batches
            var groupProductBatches = await GetMasterProductGroupWithDifferentVariantButSameBrandAsync(manufacturerId);

            //init
            var productVariantsDisc = new Dictionary<Product, string>();

            foreach (var batch in groupProductBatches)
            {
                //fetch group product 
                var product = batch.Products.FirstOrDefault(p => p.ProductType == ProductType.GroupedProduct);
                if (product == null)
                    continue;

                if (!productVariantsDisc.ContainsKey(product))
                    productVariantsDisc.Add(product, batch.SpecificationOptionName);
            }

            return productVariantsDisc;
        }

        #region Utitlities

        private class MasterProductBatch
        {
            public MasterProductBatch()
            {
                Products = new List<Product>();
            }
            public int ManufacturerId { get; set; }
            public int SpecificationOptionId { get; set; }
            public string SpecificationOptionName { get; set; }
            /// <summary>
            /// Display order (Display purpose.)
            /// </summary>
            public int DisplayOrder { get; set; }
            public IList<Product> Products { get; set; }
        }

        /// <summary>
        /// Get master product with same variant.
        /// </summary>
        /// <returns></returns>
        private async Task<IList<MasterProductBatch>> GetMasterProductGroupWithSameVariantAndBrandAsync(string specificationAttName = ExportImportDefaults.ATT_VARIANT)
        {
            //get master products with same variant value
            var variantQuery = from p in _productRepository.Table
                               join pm in _productManufactureRepositoy.Table on p.Id equals pm.ProductId
                               join m in _manufactureRepositoy.Table on pm.ManufacturerId equals m.Id
                               join psa in _productSpecificationAttributeRepository.Table on p.Id equals psa.ProductId
                               join sao in _specificationAttributeOptionRepository.Table on psa.SpecificationAttributeOptionId equals sao.Id
                               join sa in _specificationAttributeRepository.Table on sao.SpecificationAttributeId equals sa.Id
                               where !p.Deleted && p.IsMaster
                                     && sa.Name.Equals(specificationAttName) //Spec att: Variant
                                     && !string.IsNullOrEmpty(sao.Name) && !string.IsNullOrWhiteSpace(sao.Name)
                                     && !m.Deleted
                               group p by new
                               {
                                   SpecAttOptionId = sao.Id,
                                   ManufacturerId = m.Id,
                               } into variantProducts
                               where variantProducts.Count() > 1 //more than 1 products
                               select new
                               {
                                   SpecAttOptionId = variantProducts.Key.SpecAttOptionId,
                                   ManufacturerId = variantProducts.Key.ManufacturerId,
                                   Products = variantProducts.ToList(),
                               };

            var batchProducts = new List<MasterProductBatch>();
            foreach (var variantGroup in variantQuery.ToList())
            {
                if (variantGroup == null || variantGroup?.Products?.Count <= 1)
                    continue;

                // Check if the group contains a group product
                if (variantGroup.Products.Any(p => p.ProductType == ProductType.GroupedProduct))
                    continue; // Skip the group if it already has a group product

                batchProducts.Add(new MasterProductBatch
                {
                    SpecificationOptionId = variantGroup.SpecAttOptionId,
                    ManufacturerId = variantGroup.ManufacturerId,
                    Products = variantGroup.Products.ToList(),
                });
            }

            return await Task.FromResult(batchProducts);
        }

        /// <summary>
        /// Get master product with different variant but same brand.
        /// </summary>
        /// <returns></returns>
        private async Task<IList<MasterProductBatch>> GetMasterProductGroupWithDifferentVariantButSameBrandAsync(int manufacturerId, string specificationAttName = ExportImportDefaults.ATT_VARIANT)
        {
            //get master products with same variant value
            var sameBrandVariantQuery = from p in _productRepository.Table
                                        join pm in _productManufactureRepositoy.Table on p.Id equals pm.ProductId
                                        join m in _manufactureRepositoy.Table on pm.ManufacturerId equals m.Id
                                        join psa in _productSpecificationAttributeRepository.Table on p.Id equals psa.ProductId
                                        join sao in _specificationAttributeOptionRepository.Table on psa.SpecificationAttributeOptionId equals sao.Id
                                        join sa in _specificationAttributeRepository.Table on sao.SpecificationAttributeId equals sa.Id
                                        where !p.Deleted && p.IsMaster && p.ProductTypeId == (int)ProductType.GroupedProduct //group product only.
                                              && sa.Name.Equals(specificationAttName) //Spec att: Variant
                                              && !string.IsNullOrEmpty(sao.Name) && !string.IsNullOrWhiteSpace(sao.Name)
                                              && m.Id == manufacturerId //Maufacturer
                                        orderby p.DisplayOrder, p.Id
                                        group p by new
                                        {
                                            SpecAttOptionId = sao.Id,
                                            SpecAttOptionName = sao.Name,
                                            ManufacturerId = m.Id,
                                        } into variantProducts
                                        //where variantProducts.Count() > 1 //more than 1 products
                                        select new
                                        {
                                            SpecAttOptionId = variantProducts.Key.SpecAttOptionId,
                                            SpecAttOptionName = variantProducts.Key.SpecAttOptionName,
                                            ManufacturerId = variantProducts.Key.ManufacturerId,
                                            Products = variantProducts.ToList(),
                                        };

            var batchProducts = new List<MasterProductBatch>();
            foreach (var variantGroup in sameBrandVariantQuery.ToList())
            {
                if (variantGroup == null)
                    continue;

                batchProducts.Add(new MasterProductBatch
                {
                    SpecificationOptionId = variantGroup.SpecAttOptionId,
                    SpecificationOptionName = variantGroup.SpecAttOptionName,
                    ManufacturerId = variantGroup.ManufacturerId,
                    DisplayOrder = GetVariantSortOrder(variantGroup.SpecAttOptionName), //set display order
                    Products = variantGroup.Products.ToList(),
                });
            }

            return await Task.FromResult(batchProducts.OrderBy(b => b.DisplayOrder).ToList());
        }


        /// <summary>
        /// Get variant specification attribute
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        private async Task<SpecificationAttribute> GetVariantSpecificationAtt()
        {
            //load varint specification 
            return await _specificationAttributeService.GetSpecificationAttributeByNameAsync(ExportImportDefaults.ATT_VARIANT) ??
                 throw new ArgumentNullException("Variant Specification attribute could not found!");
        }

        /// <summary>
        /// Rearrange sizes and containers of related products within a group
        /// </summary>
        /// <param name="groupProductId">ID of the group to rearrange</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        private async Task<bool> ReArrangeAllGroupProductsAssociatedProductsAsync(int groupProductId, SpecificationAttribute variantSpecificationAttribute)
        {
            var groupProduct = await _productService.GetProductByIdAsync(groupProductId);

            if (groupProduct == null || groupProduct.ProductType != ProductType.GroupedProduct)
                return false; // Invalid group or not a grouped product

            try
            {
                // Get associated products of the group
                var associatedProducts = await GetAssociatedProductsByGroupProductIdAsync(groupProductId, isMaster: true);

                //Get same variant and same brand products which are not mapped to group products.
                var (umappedAssociatedProducts, groupProductSpecAttOptionId, groupProductManufactureId) = await GetUnmappedSameVariantAndBrandProductsAsync(groupProductId, variantSpecificationAttribute);

                // Combine both lists into a single list for sorting
                var allProductsToSort = associatedProducts.Concat(umappedAssociatedProducts);

                // Rearrange the sizes and containers of associated products
                var sortedProducts = allProductsToSort
                    .OrderBy(p => GetSizeSortOrder(p.Size))
                    .ThenBy(p => GetContainerSortOrder(p.Container))
                    .ToList();

                int displayOrder = 0;
                var qualifiedAssociatedProducts = new List<Product>();

                foreach (var product in sortedProducts)
                {
                    var associatedProduct = await _productService.GetProductByIdAsync(product.Id);

                    if (associatedProduct != null)
                    {
                        //check associated qualified or not
                        bool isVariantQualified = await IsVariantQualified(associatedProduct, variantSpecificationAttribute, groupProductSpecAttOptionId, groupProductManufactureId);

                        //If qualified then update variant details or release it as individual product
                        if (!isVariantQualified)
                        {
                            //Remove from group product Update the variant and update the master product
                            associatedProduct.ParentGroupedProductId = 0;
                            associatedProduct.VisibleIndividually = true;
                            associatedProduct.UpdatedOnUtc = DateTime.UtcNow;
                            await _productService.UpdateProductAsync(associatedProduct);
                        }
                        else
                        {
                            associatedProduct.ParentGroupedProductId = groupProduct.Id;
                            associatedProduct.VisibleIndividually = false;
                            associatedProduct.DisplayOrder = displayOrder++;
                            associatedProduct.UpdatedOnUtc = DateTime.UtcNow;
                            await _productService.UpdateProductAsync(associatedProduct);

                            //add in qualify list
                            qualifiedAssociatedProducts.Add(associatedProduct);
                        }
                    }
                }

                //check there's only 1 or no qualified product then delete group product.
                if (qualifiedAssociatedProducts.Count <= 1)
                {
                    //release associated product if only 1
                    if (qualifiedAssociatedProducts.Count == 1)
                    {
                        var lastAssociatedProduct = qualifiedAssociatedProducts.First();
                        lastAssociatedProduct.ParentGroupedProductId = 0;
                        lastAssociatedProduct.VisibleIndividually = true;
                        lastAssociatedProduct.UpdatedOnUtc = DateTime.UtcNow;
                        await _productService.UpdateProductAsync(lastAssociatedProduct);
                    }

                    // delete the group product
                    await _productService.DeleteProductAsync(groupProduct);
                }

                return true; // Rearrangement successful
            }
            catch (Exception exc)
            {
                // Handle exceptions
                await _logger.ErrorAsync("Error occurred while rearranging group products.", exc);
                return false;
            }
        }

        /// <summary>
        /// Get associated product by group product identifiers
        /// </summary>
        /// <param name="groupProductId"></param>
        /// <param name="isMaster"></param>
        /// <returns></returns>
        private async Task<IList<Product>> GetAssociatedProductsByGroupProductIdAsync(int groupProductId, bool isMaster)
        {
            var query = from p in _productRepository.Table
                        where p.ParentGroupedProductId == groupProductId && !p.Deleted && isMaster
                        select p;

            var relatedProducts = await query.ToListAsync();

            return relatedProducts;
        }

        /// <summary>
        /// Gets a master product by upcCode
        /// </summary>
        /// <param name="upcCode">UPCCode</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the product
        /// </returns>
        private async Task<(List<Product> associatedProducts, int groupProductSpecAttOptionId, int groupProductManufactureId)> GetUnmappedSameVariantAndBrandProductsAsync(int groupProductId, SpecificationAttribute variantSpecificationAttribute)
        {
            //get product specification (varint) record.
            var productSpecificationVarintAttribute = (await _specificationAttributeService.GetProductSpecificationAttributesByProductAndAtributeAsync(groupProductId, variantSpecificationAttribute.Id))?.FirstOrDefault();
            if (productSpecificationVarintAttribute == null)
                return (new List<Product>(), 0, 0);

            //get manufacture.
            var productManufacturer = (await _manufacturerService.GetProductManufacturersByProductIdAsync(groupProductId)).FirstOrDefault();
            if (productManufacturer == null)
                return (new List<Product>(), 0, 0);

            //variable params
            var specificationOptionId = productSpecificationVarintAttribute.SpecificationAttributeOptionId;
            var manufacturerId = productManufacturer.ManufacturerId;
            var specificationAttName = variantSpecificationAttribute.Name;

            if (specificationOptionId == 0 || manufacturerId == 0 || string.IsNullOrEmpty(specificationAttName))
                return (new List<Product>(), 0, 0);

            //get master products with same variant value
            var unmappedProductQuery = from p in _productRepository.Table
                                       join pm in _productManufactureRepositoy.Table on p.Id equals pm.ProductId
                                       join m in _manufactureRepositoy.Table on pm.ManufacturerId equals m.Id
                                       join psa in _productSpecificationAttributeRepository.Table on p.Id equals psa.ProductId
                                       join sao in _specificationAttributeOptionRepository.Table on psa.SpecificationAttributeOptionId equals sao.Id
                                       join sa in _specificationAttributeRepository.Table on sao.SpecificationAttributeId equals sa.Id
                                       where !p.Deleted && p.IsMaster
                                             && sa.Name.Equals(specificationAttName) //Spec att: Variant
                                             && sao.Id == specificationOptionId //spec option
                                             && m.Id == manufacturerId && !m.Deleted //manufacturer
                                             && p.ProductTypeId == (int)ProductType.SimpleProduct && p.ParentGroupedProductId == 0
                                       select p;

            return (await unmappedProductQuery.ToListAsync(), specificationOptionId, manufacturerId);

            //var products = await _productRepository.Table
            //        .Where(p => !p.Deleted
            //        && p.IsMaster
            //        && p.ProductTypeId == (int)ProductType.SimpleProduct
            //        && p.ParentGroupedProductId == 0).ToListAsync();

            //var productsWithSameName = products
            //    .Where(p => string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase))
            //    .ToList();

            //return productsWithSameName;
        }

        /// <summary>
        /// Verify is variant qualified to be an associated product.
        /// </summary>
        /// <param name="associatedProduct"></param>
        /// <param name="variantSpecificationAttribute"></param>
        /// <param name="groupProductSpecAttOptionId"></param>
        /// <param name="groupProductManufactureId"></param>
        /// <returns></returns>
        private async Task<bool> IsVariantQualified(Product associatedProduct,
            SpecificationAttribute variantSpecificationAttribute,
            int groupProductSpecAttOptionId,
            int groupProductManufactureId)
        {
            //get associated product variant option & manufacture. Compare it with group product.
            //get product specification (varint) record.
            var associatedProductSpecificationVarintAttribute = (await _specificationAttributeService.GetProductSpecificationAttributesByProductAndAtributeAsync(associatedProduct.Id, variantSpecificationAttribute.Id))?.FirstOrDefault();
            if (associatedProductSpecificationVarintAttribute == null)
                return false;

            //compare variant specification option are same as group product?
            if (associatedProductSpecificationVarintAttribute.SpecificationAttributeOptionId != groupProductSpecAttOptionId)
                return false;

            //get manufacture.
            var associatedProductManufacturer = (await _manufacturerService.GetProductManufacturersByProductIdAsync(associatedProduct.Id)).FirstOrDefault();
            if (associatedProductManufacturer == null)
                return false;

            //compare manufacture
            if (associatedProductManufacturer.ManufacturerId != groupProductManufactureId)
                return false;

            return true;
        }

        #endregion

        #endregion
    }
}
