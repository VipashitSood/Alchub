using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core.Domain.Catalog;
using Nop.Services.Catalog;
using Nop.Services.Seo;
using Nop.Web.Alchub.SyncMasterCatalog.Models;
using static Nop.Web.Alchub.SyncMasterCatalog.Models.SyncCatalogProductModel;

namespace Nop.Web.Alchub.SyncMasterCatalog.Factories
{
    public class SyncCatalogFactory : ISyncCatalogFactory
    {
        #region Const

        private const string ATT_COLOR = "Color";
        private const string ATT_TYPE = "Type";
        private const string ATT_COUNTRY_OF_ORIGIN = "Country of Origin";
        private const string ATT_STATE_OF_ORIGIN = "State of Origin";
        private const string ATT_SUBTYPE = "Sub Type";
        private const string ATT_REGION = "Region";
        private const string ATT_ABV = "ABV";
        private const string ATT_FLAVOR = "Flavor";
        private const string ATT_VINTAGE = "Vintage";
        private const string ATT_ALCOHOL_PROOF = "Alcohol Proof";
        private const string ATT_SPECIALTY = "Specialty";
        private const string ATT_RATINGS = "Ratings";
        private const string ATT_FOOD_PAIRING = "Food Pairing";
        private const string ATT_BODY = "Body";
        private const string ATT_TASTING_NOTES = "Tasting Notes";
        private const string ATT_CONTAINER = "Container";
        private const string ATT_BASE_UNIT_CLOSURE = "Base Unit Closure";
        private const string ATT_APPELLATION = "Appellation";
        private const string ATT_BRAND_DESCRIPTION = "Brand Description";
        private const string ATT_SIZE = "Size";

        #endregion

        #region Fields

        private readonly ICategoryService _categoryService;
        private readonly IManufacturerService _manufacturerService;
        private readonly ISpecificationAttributeService _specificationAttributeService;
        private readonly IProductService _productService;
        private readonly IUrlRecordService _urlRecordService;

        #endregion

        #region Ctor

        public SyncCatalogFactory(ICategoryService categoryService,
            IManufacturerService manufacturerService,
            ISpecificationAttributeService specificationAttributeService,
            IProductService productService,
            IUrlRecordService urlRecordService)
        {
            _categoryService = categoryService;
            _manufacturerService = manufacturerService;
            _specificationAttributeService = specificationAttributeService;
            _productService = productService;
            _urlRecordService = urlRecordService;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Prepare categories
        /// </summary>
        /// <param name="fromDateTimeUtc"></param>
        /// <returns></returns>
        private async Task<IList<SyncCatalogCategoryModel>> PrepareCategoriesAsync(DateTime? fromDateTimeUtc, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            //get categories
            var categories = await _categoryService.SearchCategoriesAsync(storeId: 0, showHidden: true, createdOrUpdatedFromUtc: fromDateTimeUtc,
                pageIndex, pageSize);

            var list = new List<SyncCatalogCategoryModel>();
            foreach (var category in categories)
            {
                var model = new SyncCatalogCategoryModel()
                {
                    Category = category,
                    SeName = await _urlRecordService.GetSeNameAsync(category, 0, true, false)
                };
                list.Add(model);
            }

            return list;
        }

        /// <summary>
        /// Prepare manufactures
        /// </summary>
        /// <param name="fromDateTimeUtc"></param>
        /// <returns></returns>
        private async Task<IList<SyncCatalogManufacturerModel>> PrepareManufacturesAsync(DateTime? fromDateTimeUtc, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            //get manufactures
            var manufactures = await _manufacturerService.SearchManufacturersAsync(storeId: 0, showHidden: true, createdOrUpdatedFromUtc: fromDateTimeUtc,
                pageIndex, pageSize);

            var list = new List<SyncCatalogManufacturerModel>();
            foreach (var manufacture in manufactures)
            {
                var model = new SyncCatalogManufacturerModel()
                {
                    Manufacturer = manufacture,
                    SeName = await _urlRecordService.GetSeNameAsync(manufacture, 0, true, false)
                };
                list.Add(model);
            }

            return list;
        }

        /// <summary>
        /// Prepare specification attributes
        /// </summary>
        /// <returns></returns>
        private async Task<IList<SpecificationAttribute>> PrepareSpecificationAttributesAsync(int pageIndex = 0, int pageSize = int.MaxValue)
        {
            //get all specification att
            var allSpecificationAttributes = await _specificationAttributeService.GetSpecificationAttributesAsync(pageIndex, pageSize);
            return allSpecificationAttributes.ToList();
        }

        /// <summary>
        /// Prepare product specification attributes json model
        /// </summary>
        /// <param name="product"></param>
        /// <returns></returns>
        private async Task<CatalogProductSpecificationAttributeModel> PrepareProductSpecificationAttributeModelAsync(Product product)
        {
            var specificationAttModel = new CatalogProductSpecificationAttributeModel();

            //var attributes = new List<ImportSpecificationAttribute>();
            var productSpecificationAttributes = await _specificationAttributeService.GetProductSpecificationAttributesAsync(product.Id);

            foreach (var psa in productSpecificationAttributes)
            {
                //var specificationAttModel = new ImportSpecificationAttribute();
                var sao = await _specificationAttributeService.GetSpecificationAttributeOptionByIdAsync(psa.SpecificationAttributeOptionId);
                var specificationAttribute = await _specificationAttributeService.GetSpecificationAttributeByIdAsync(sao.SpecificationAttributeId);
                switch (specificationAttribute.Name)
                {
                    case ATT_COLOR:
                        specificationAttModel.Att_Color = sao.Name;
                        break;
                    case ATT_COUNTRY_OF_ORIGIN:
                        specificationAttModel.Att_CountryofOrigin = sao.Name;
                        break;
                    case ATT_ABV:
                        specificationAttModel.Att_ABV = sao.Name;
                        break;
                    case ATT_VINTAGE:
                        specificationAttModel.Att_Vintage = sao.Name;
                        break;
                    case ATT_STATE_OF_ORIGIN:
                        specificationAttModel.Att_StateofOrigin = sao.Name;
                        break;
                    case ATT_REGION:
                        specificationAttModel.Att_Region = sao.Name;
                        break;
                    case ATT_SUBTYPE:
                        specificationAttModel.Att_SubType = sao.Name;
                        break;
                    case ATT_FLAVOR:
                        specificationAttModel.Att_Flavor = sao.Name;
                        break;
                    case ATT_ALCOHOL_PROOF:
                        specificationAttModel.Att_AlcoholProof = sao.Name;
                        break;
                    case ATT_TYPE:
                        specificationAttModel.Att_Type = sao.Name;
                        break;
                    case ATT_SPECIALTY:
                        specificationAttModel.Att_Specialty = sao.Name;
                        break;
                    case ATT_APPELLATION:
                        specificationAttModel.Att_Appellation = sao.Name;
                        break;
                    case ATT_FOOD_PAIRING:
                        specificationAttModel.Att_FoodPairing = sao.Name;
                        break;
                    case ATT_BODY:
                        specificationAttModel.Att_Body = sao.Name;
                        break;
                    case ATT_TASTING_NOTES:
                        specificationAttModel.Att_TastingNotes = sao.Name;
                        break;
                    case ATT_CONTAINER:
                        specificationAttModel.Att_Container = sao.Name;
                        break;
                    case ATT_BASE_UNIT_CLOSURE:
                        specificationAttModel.Att_BaseUnitClosure = sao.Name;
                        break;
                    case ATT_RATINGS:
                        specificationAttModel.Att_Ratings = sao.Name;
                        break;
                    case ATT_BRAND_DESCRIPTION:
                        specificationAttModel.Att_BrandDescription = sao.Name;
                        break;
                    case ATT_SIZE:
                        specificationAttModel.Att_Size = sao.Name;
                        break;
                }
            }

            return specificationAttModel;
        }


        /// <summary>
        /// Prepare products
        /// </summary>
        /// <param name="fromDateTimeUtc"></param>
        /// <returns></returns>
        private async Task<IList<SyncCatalogProductModel>> PrepareMasterProductsAsync(DateTime? fromDateTimeUtc, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            //get master products
            var masterProducts = await _productService.SearchMasterProductsAsync(storeId: 0, showHidden: true, createdOrUpdatedFromUtc: fromDateTimeUtc,
                pageIndex, pageSize);
            var listModel = new List<SyncCatalogProductModel>();
            foreach (var product in masterProducts)
            {
                var productModel = new SyncCatalogProductModel()
                {
                    Id = product.Id,
                    Name = product.Name,
                    SKU = product.Sku,
                    UPCCode = product.UPCCode,
                    ShortDescription = product.ShortDescription,
                    MetaKeywords = product.MetaKeywords,
                    MetaDescription = product.MetaDescription,
                    MetaTitle = product.MetaTitle,
                    SeName = await _urlRecordService.GetSeNameAsync(product, null, false, false),
                    Published = product.Published,
                    Size = product.Size,
                    Container = product.Container,
                    FullDescription = product.FullDescription,
                    DisplayOrder = product.DisplayOrder,
                    VisibleIndividually = product.VisibleIndividually,
                    Deleted = product.Deleted,
                    ProductTypeId = product.ProductTypeId
                };

                //categories
                var productCategories = await _categoryService.GetProductCategoriesByProductIdAsync(product.Id, true);
                var categoryMappings = new List<CatalogProductCategoryModel>();
                foreach (var mapping in productCategories)
                {
                    categoryMappings.Add(new CatalogProductCategoryModel
                    {
                        Id = mapping.Id,
                        CategoryId = mapping.CategoryId,
                        ProductId = mapping.ProductId,
                        IsFeaturedProduct = mapping.IsFeaturedProduct,
                        DisplayOrder = mapping.DisplayOrder,
                        CategoryName = (await _categoryService.GetCategoryByIdAsync(mapping.CategoryId))?.Name
                    });
                }
                productModel.ProductCategories = categoryMappings;

                //manufactures
                var productManufactures = await _manufacturerService.GetProductManufacturersByProductIdAsync(product.Id, true);
                var manufactureMappings = new List<CatalogProductManufacturesModel>();
                foreach (var mapping in productManufactures)
                {
                    manufactureMappings.Add(new CatalogProductManufacturesModel
                    {
                        Id = mapping.Id,
                        ManufacturerId = mapping.ManufacturerId,
                        ProductId = mapping.ProductId,
                        IsFeaturedProduct = mapping.IsFeaturedProduct,
                        DisplayOrder = mapping.DisplayOrder,
                        ManufactureName = (await _manufacturerService.GetManufacturerByIdAsync(mapping.ManufacturerId))?.Name
                    });
                }
                productModel.ProductManufacturers = manufactureMappings;

                //specification attributes
                productModel.ProductSpecificationAttributes = await PrepareProductSpecificationAttributeModelAsync(product);

                //associated products
                if (product.ProductType == ProductType.GroupedProduct)
                {
                    var associatedProducts = await _productService.GetAssociatedProductsAsync(parentGroupedProductId: product.Id, showHidden: true);
                    var associatedProductList = new List<CatalogAssociatedProductModel.AssociatedProductModel>();
                    foreach (var associatedProduct in associatedProducts)
                    {
                        var associatedProductModel = new CatalogAssociatedProductModel.AssociatedProductModel()
                        {
                            ProductId = associatedProduct.Id,
                            SKU = associatedProduct.Sku,
                            UPCCode = associatedProduct.UPCCode,
                            DisplayOrder = associatedProduct.DisplayOrder
                        };
                        associatedProductList.Add(associatedProductModel);
                    }
                    productModel.AssociatedProducts = new CatalogAssociatedProductModel { AssociatedProducts = associatedProductList };
                }

                listModel.Add(productModel);
            }

            return listModel;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Prepare catalog data model
        /// </summary>
        /// <param name="domainUrl"></param>
        /// <param name="fromDateTime"></param>
        /// <returns></returns>
        public async Task<CatalogDataModel> PrepareCatalogDataModel(string domainUrl, DateTime? fromDateTime)
        {
            var model = new CatalogDataModel();

            //categories
            model.Categories = await PrepareCategoriesAsync(fromDateTime);

            //manufactures
            model.Manufacturers = await PrepareManufacturesAsync(fromDateTime);

            //specification attributes
            model.SpecificationAttributes = await PrepareSpecificationAttributesAsync();

            //master products
            model.MasterProducts = await PrepareMasterProductsAsync(fromDateTime);

            return model;
        }

        /// <summary>
        /// Prepare catalog data model
        /// </summary>
        /// <param name="upcCodeList"></param>
        /// <returns></returns>
        public async Task<CatalogDataModel> PrepareCatalogDataModel(string[] upcCodeList)
        {
            var model = new CatalogDataModel();

            if (!upcCodeList.Any())
                return model;

            //get master products
            var masterProducts = await _productService.GetMasterProductsByUPCCodeListAsync(upcCodeList);
            var listModel = new List<SyncCatalogProductModel>();
            foreach (var product in masterProducts)
            {
                var productModel = new SyncCatalogProductModel()
                {
                    Id = product.Id,
                    Name = product.Name,
                    SKU = product.Sku,
                    UPCCode = product.UPCCode,
                    ShortDescription = product.ShortDescription,
                    MetaKeywords = product.MetaKeywords,
                    MetaDescription = product.MetaDescription,
                    MetaTitle = product.MetaTitle,
                    SeName = await _urlRecordService.GetSeNameAsync(product, null, false, false),
                    Published = product.Published,
                    Size = product.Size,
                    Container = product.Container,
                    FullDescription = product.FullDescription,
                    DisplayOrder = product.DisplayOrder,
                    VisibleIndividually = product.VisibleIndividually,
                    Deleted = product.Deleted,
                    ProductTypeId = product.ProductTypeId
                };

                //categories
                var productCategories = await _categoryService.GetProductCategoriesByProductIdAsync(product.Id, true);
                var categoryMappings = new List<CatalogProductCategoryModel>();
                foreach (var mapping in productCategories)
                {
                    categoryMappings.Add(new CatalogProductCategoryModel
                    {
                        Id = mapping.Id,
                        CategoryId = mapping.CategoryId,
                        ProductId = mapping.ProductId,
                        IsFeaturedProduct = mapping.IsFeaturedProduct,
                        DisplayOrder = mapping.DisplayOrder,
                        CategoryName = (await _categoryService.GetCategoryByIdAsync(mapping.CategoryId))?.Name
                    });
                }
                productModel.ProductCategories = categoryMappings;

                //manufactures
                var productManufactures = await _manufacturerService.GetProductManufacturersByProductIdAsync(product.Id, true);
                var manufactureMappings = new List<CatalogProductManufacturesModel>();
                foreach (var mapping in productManufactures)
                {
                    manufactureMappings.Add(new CatalogProductManufacturesModel
                    {
                        Id = mapping.Id,
                        ManufacturerId = mapping.ManufacturerId,
                        ProductId = mapping.ProductId,
                        IsFeaturedProduct = mapping.IsFeaturedProduct,
                        DisplayOrder = mapping.DisplayOrder,
                        ManufactureName = (await _manufacturerService.GetManufacturerByIdAsync(mapping.ManufacturerId))?.Name
                    });
                }
                productModel.ProductManufacturers = manufactureMappings;

                //specification attributes
                productModel.ProductSpecificationAttributes = await PrepareProductSpecificationAttributeModelAsync(product);

                //associated products
                if (product.ProductType == ProductType.GroupedProduct)
                {
                    var associatedProducts = await _productService.GetAssociatedProductsAsync(parentGroupedProductId: product.Id, showHidden: true);
                    var associatedProductList = new List<CatalogAssociatedProductModel.AssociatedProductModel>();
                    foreach (var associatedProduct in associatedProducts)
                    {
                        var associatedProductModel = new CatalogAssociatedProductModel.AssociatedProductModel()
                        {
                            ProductId = associatedProduct.Id,
                            SKU = associatedProduct.Sku,
                            UPCCode = associatedProduct.UPCCode,
                            DisplayOrder = associatedProduct.DisplayOrder
                        };
                        associatedProductList.Add(associatedProductModel);
                    }
                    productModel.AssociatedProducts = new CatalogAssociatedProductModel { AssociatedProducts = associatedProductList };
                }

                listModel.Add(productModel);
            }

            //master products
            model.MasterProducts = listModel;

            return model;
        }

        /// <summary>
        /// Prepare catalog data model using pagination
        /// </summary>
        /// <param name="domainUrl"></param>
        /// <param name="fromDateTime"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public async Task<CatalogDataModel> PrepareCatalogDataModelUsingPagination(string domainUrl, DateTime? fromDateTime, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var model = new CatalogDataModel();

            //categories
            model.Categories = await PrepareCategoriesAsync(fromDateTime, pageIndex, pageSize);

            //manufactures
            model.Manufacturers = await PrepareManufacturesAsync(fromDateTime, pageIndex, pageSize);

            //specification attributes
            model.SpecificationAttributes = await PrepareSpecificationAttributesAsync(pageIndex, pageSize);

            //master products
            model.MasterProducts = await PrepareMasterProductsAsync(fromDateTime, pageIndex, pageSize);

            return model;
        }
        #endregion
    }
}
