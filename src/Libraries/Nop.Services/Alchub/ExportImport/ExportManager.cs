using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using ClosedXML.Excel;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Forums;
using Nop.Core.Domain.Gdpr;
using Nop.Core.Domain.Messages;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Tax;
using Nop.Core.Domain.Vendors;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Discounts;
using Nop.Services.ExportImport.Help;
using Nop.Services.Forums;
using Nop.Services.Gdpr;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Seo;
using Nop.Services.Shipping;
using Nop.Services.Shipping.Date;
using Nop.Services.Stores;
using Nop.Services.Tax;
using Nop.Services.Vendors;

namespace Nop.Services.ExportImport
{
    /// <summary>
    /// Export manager
    /// </summary>
    public partial class ExportManager : IExportManager
    {

        #region Methods 

        protected virtual async Task<string> GetPictureAsync(Product product, short pictureIndex)
        {
            // we need only the picture at a specific index, no need to get more pictures than that
            var recordsToReturn = pictureIndex + 1;
            var pictures = await _pictureService.GetPicturesByProductIdAsync(product.Id, recordsToReturn);

            var picturePath = pictures.Count > pictureIndex ? await _pictureService.GetThumbLocalPathAsync(pictures[pictureIndex]) : null;

            if (picturePath != null)
            {
                picturePath = picturePath.Substring(picturePath.LastIndexOf("\\") + 1);
            }
            return picturePath;
        }

        /// <returns>A task that represents the asynchronous operation</returns>
        protected virtual async Task<bool> IgnoreExportProductPropertyAsync(Func<ProductEditorSettings, bool> func)
        {
            //++Alchub

            //ignore this validation, to fix export master product missing filed issue.
            return await Task.FromResult(false);

            //--Alchub
        }

        protected virtual async Task<string> GetCategoryAsync(Product product, int index)
        {
            var categories = await _categoryService.GetProductCategoriesByProductIdAsync(product.Id, true);
            var category = categories.Count > index ? await _categoryService.GetCategoryByIdAsync(categories[index].CategoryId) : null;
            if (category == null)
                return null;

            string categoryName;
            if (_catalogSettings.ExportImportRelatedEntitiesByName)
            {
                categoryName = _catalogSettings.ExportImportProductCategoryBreadcrumb
                    ? await _categoryService.GetFormattedBreadCrumbAsync(category)
                    : category.Name;
            }
            else
            {
                categoryName = category.Id.ToString();
            }
            return categoryName;
        }

        protected virtual async Task<string> GetManufacturerAsync(Product product)
        {
            //product can have only single manufacturer/brand.
            var pm = (await _manufacturerService.GetProductManufacturersByProductIdAsync(product.Id, true))?.FirstOrDefault();
            if (pm == null)
                return null;

            string manufacturerName;
            if (_catalogSettings.ExportImportRelatedEntitiesByName)
            {
                var manufacturer = await _manufacturerService.GetManufacturerByIdAsync(pm.ManufacturerId);
                manufacturerName = manufacturer.Name;
            }
            else
            {
                manufacturerName = pm.ManufacturerId.ToString();
            }
            return manufacturerName;
        }

        protected virtual async Task<string> GetAssociateProductIds(Product product)
        {
            var productIds = "";
            var products = await _productService.GetAssociatedProductsAsync(product.Id);
            productIds = string.Join(";", products.Select(x => x.Id));
            return productIds;
        }

        protected virtual async Task<string> GetAssociateMasterProductIds(Product product)
        {
            var productIds = "";
            var products = await _productService.GetAssociatedProductsAsync(product.Id);
            productIds = string.Join(",", products.Select(x => x.Id));
            return productIds;
        }

        private async Task<PropertyManager<ImportSpecificationAttribute>> GetAttributeManagerAsync()
        {
            var attributeProperties = new[]
            {
                new PropertyByName<ImportSpecificationAttribute>("Att_CountryofOrigin", p => p.Att_CountryofOrigin),
                new PropertyByName<ImportSpecificationAttribute>("Att_StateofOrigin", p => p.Att_StateofOrigin),
                new PropertyByName<ImportSpecificationAttribute>("Att_Region", p => p.Att_Region),
                new PropertyByName<ImportSpecificationAttribute>("Att_SubType", p => p.Att_SubType),
                new PropertyByName<ImportSpecificationAttribute>("Att_ABV", p => p.Att_ABV),
                new PropertyByName<ImportSpecificationAttribute>("Att_Vintage", p => p.Att_Vintage),
                new PropertyByName<ImportSpecificationAttribute>("Att_Flavor", p => p.Att_Flavor),
                new PropertyByName<ImportSpecificationAttribute>("Att_AlcoholProof", p => p.Att_AlcoholProof),
                new PropertyByName<ImportSpecificationAttribute>("Att_Type", p => p.Att_Type),
                new PropertyByName<ImportSpecificationAttribute>("Att_Specialty", p => p.Att_Specialty),
                new PropertyByName<ImportSpecificationAttribute>("Att_Color", p => p.Att_Color),
                new PropertyByName<ImportSpecificationAttribute>("Att_Ratings", p => p.Att_Ratings),
                new PropertyByName<ImportSpecificationAttribute>("Att_Appellation", p => p.Att_Appellation),
                new PropertyByName<ImportSpecificationAttribute>("Att_FoodPairing", p => p.Att_FoodPairing),
                new PropertyByName<ImportSpecificationAttribute>("Att_Body", p => p.Att_Body),
                new PropertyByName<ImportSpecificationAttribute>("Att_TastingNotes", p => p.Att_TastingNotes),
                new PropertyByName<ImportSpecificationAttribute>("Att_Container", p => p.Att_Container),
                new PropertyByName<ImportSpecificationAttribute>("Att_BaseUnitClosure", p => p.Att_BaseUnitClosure),
                new PropertyByName<ImportSpecificationAttribute>("Att_BrandDescription", p => p.Att_BrandDescription),
                new PropertyByName<ImportSpecificationAttribute>("Att_Size", p => p.Att_Size),
                new PropertyByName<ImportSpecificationAttribute>("Att_Variant", p => p.Att_Variant)
            };

            return await Task.FromResult(new PropertyManager<ImportSpecificationAttribute>(attributeProperties, _catalogSettings));
        }

        private async Task<int> ExportAttributesAsync(Product item, PropertyManager<ImportSpecificationAttribute> attributeManager, IXLWorksheet worksheet, int row, int cellPosition)
        {
            var attributes = new List<ImportSpecificationAttribute>();
            var productSpecificationAttributes = await _specificationAttributeService.GetProductSpecificationAttributesAsync(item.Id);
            if (attributeManager.CurrentObject == null)
                attributeManager.CurrentObject = new ImportSpecificationAttribute();
            foreach (var psa in productSpecificationAttributes)
            {
                var sa = new ImportSpecificationAttribute();
                var sao = await _specificationAttributeService.GetSpecificationAttributeOptionByIdAsync(psa.SpecificationAttributeOptionId);
                var specificationAttribute = await _specificationAttributeService.GetSpecificationAttributeByIdAsync(sao.SpecificationAttributeId);
                switch (specificationAttribute.Name)
                {
                    case ExportImportDefaults.ATT_COLOR:
                        sa.Att_Color = sao.Name;
                        attributeManager.CurrentObject.Att_Color = sao.Name;
                        break;
                    case ExportImportDefaults.ATT_COUNTRY_OF_ORIGIN:
                        sa.Att_CountryofOrigin = sao.Name;
                        attributeManager.CurrentObject.Att_CountryofOrigin = sao.Name;
                        break;
                    case ExportImportDefaults.ATT_ABV:
                        sa.Att_ABV = sao.Name;
                        attributeManager.CurrentObject.Att_ABV = sao.Name;
                        break;
                    case ExportImportDefaults.ATT_VINTAGE:
                        sa.Att_Vintage = sao.Name;
                        attributeManager.CurrentObject.Att_Vintage = sao.Name;
                        break;
                    case ExportImportDefaults.ATT_STATE_OF_ORIGIN:
                        sa.Att_StateofOrigin = sao.Name;
                        attributeManager.CurrentObject.Att_StateofOrigin = sao.Name;
                        break;
                    case ExportImportDefaults.ATT_REGION:
                        sa.Att_Region = sao.Name;
                        attributeManager.CurrentObject.Att_Region = sao.Name;
                        break;
                    case ExportImportDefaults.ATT_SUBTYPE:
                        sa.Att_SubType = sao.Name;
                        attributeManager.CurrentObject.Att_SubType = sao.Name;
                        break;
                    case ExportImportDefaults.ATT_FLAVOR:
                        sa.Att_Flavor = sao.Name;
                        attributeManager.CurrentObject.Att_Flavor = sao.Name;
                        break;
                    case ExportImportDefaults.ATT_ALCOHOL_PROOF:
                        sa.Att_AlcoholProof = sao.Name;
                        attributeManager.CurrentObject.Att_AlcoholProof = sao.Name;
                        break;
                    case ExportImportDefaults.ATT_TYPE:
                        sa.Att_Type = sao.Name;
                        attributeManager.CurrentObject.Att_Type = sao.Name;
                        break;
                    case ExportImportDefaults.ATT_SPECIALTY:
                        sa.Att_Specialty = sao.Name;
                        attributeManager.CurrentObject.Att_Specialty = sao.Name;
                        break;
                    case ExportImportDefaults.ATT_APPELLATION:
                        sa.Att_Appellation = sao.Name;
                        attributeManager.CurrentObject.Att_Appellation = sao.Name;
                        break;
                    case ExportImportDefaults.ATT_FOOD_PAIRING:
                        sa.Att_FoodPairing = sao.Name;
                        attributeManager.CurrentObject.Att_Color = sao.Name;
                        attributeManager.CurrentObject.Att_FoodPairing = sao.Name;
                        break;
                    case ExportImportDefaults.ATT_BODY:
                        sa.Att_Body = sao.Name;
                        attributeManager.CurrentObject.Att_Body = sao.Name;
                        break;
                    case ExportImportDefaults.ATT_TASTING_NOTES:
                        sa.Att_TastingNotes = sao.Name;
                        attributeManager.CurrentObject.Att_TastingNotes = sao.Name;
                        break;
                    case ExportImportDefaults.ATT_CONTAINER:
                        sa.Att_Container = sao.Name;
                        attributeManager.CurrentObject.Att_Container = sao.Name;
                        break;
                    case ExportImportDefaults.ATT_BASE_UNIT_CLOSURE:
                        sa.Att_BaseUnitClosure = sao.Name;
                        attributeManager.CurrentObject.Att_BaseUnitClosure = sao.Name;
                        break;
                    case ExportImportDefaults.ATT_RATINGS:
                        sa.Att_Ratings = sao.Name;
                        attributeManager.CurrentObject.Att_Ratings = sao.Name;
                        break;
                    case ExportImportDefaults.ATT_BRAND_DESCRIPTION:
                        sa.Att_BrandDescription = sao.Name;
                        attributeManager.CurrentObject.Att_BrandDescription = sao.Name;
                        break;
                    case ExportImportDefaults.ATT_SIZE:
                        sa.Att_Size = sao.Name;
                        attributeManager.CurrentObject.Att_Size = sao.Name;
                        break;
                    case ExportImportDefaults.ATT_VARIANT: //special att
                        sa.Att_Variant = sao.Name;
                        attributeManager.CurrentObject.Att_Variant = sao.Name;
                        break;
                }
                attributes.Add(sa);
            }

            if (!attributes.Any())
                return row + 1;

            await attributeManager.WriteToXlsxAsync(worksheet, row, cellPosition);
            attributeManager.CurrentObject = new ImportSpecificationAttribute();
            return row + 1;
        }

        private async Task<byte[]> ExportMasterProductsToXlsxWithAttributesAsync(PropertyByName<Product>[] properties, IEnumerable<Product> itemsToExport)
        {
            var specificationAttributeManager = await GetAttributeManagerAsync();

            await using var stream = new MemoryStream();
            // ok, we can run the real code of the sample now
            using (var workbook = new XLWorkbook())
            {
                // get handles to the worksheets
                var worksheet = workbook.Worksheets.Add(typeof(Product).Name);
                var manager = new PropertyManager<Product>(properties, _catalogSettings);
                manager.WriteCaption(worksheet);
                specificationAttributeManager.WriteCaption(worksheet, cellOffset: 21);
                var row = 2;
                foreach (var item in itemsToExport)
                {
                    manager.CurrentObject = item;
                    await manager.WriteToXlsxAsync(worksheet, row);
                    var cellPosition = manager.GetProperties.Count();

                    if (_catalogSettings.ExportImportProductSpecificationAttributes)
                        row = await ExportAttributesAsync(item, specificationAttributeManager, worksheet, row, cellPosition);
                }

                workbook.SaveAs(stream);
            }

            return stream.ToArray();
        }


        public virtual async Task<byte[]> ExportMasterProductsToXlsxAsync(IEnumerable<Product> products)
        {
            var currentVendor = await _workContext.GetCurrentVendorAsync();
            var properties = new[]
            {
                new PropertyByName<Product>("UPCCode", p => p.Sku),
                new PropertyByName<Product>("ProductId", p => p.Id),
                new PropertyByName<Product>("SKU", p => p.UPCCode),
                new PropertyByName<Product>("ChildProductIds", async p => await GetAssociateMasterProductIds(p)),
                new PropertyByName<Product>("Name", p => p.Name),
                new PropertyByName<Product>("ShortDescription", p => p.ShortDescription),
                new PropertyByName<Product>("MetaKeywords", p => p.MetaKeywords),
                new PropertyByName<Product>("MetaDescription", p => p.MetaDescription),
                new PropertyByName<Product>("MetaTitle", p => p.MetaTitle),
                new PropertyByName<Product>("SeName", async p => await _urlRecordService.GetSeNameAsync(p, 0)),
                new PropertyByName<Product>("Published", p => p.Published),
                new PropertyByName<Product>("VisibleIndividually", p => p.VisibleIndividually),
                new PropertyByName<Product>("Size", p => p.Size),
                new PropertyByName<Product>("Container", p => p.Container),
                new PropertyByName<Product>("Category1", async p => await GetCategoryAsync(p, 0)),
                new PropertyByName<Product>("Category2", async p => await GetCategoryAsync(p, 1)),
                new PropertyByName<Product>("Category3", async p => await GetCategoryAsync(p, 2)),
                new PropertyByName<Product>("Manufacturers", async p => await GetManufacturerAsync(p)),
                new PropertyByName<Product>("Picture1", async p => await GetPictureAsync(p, 0)),
                new PropertyByName<Product>("Picture2", async p => await GetPictureAsync(p, 1)),
                new PropertyByName<Product>("Picture3", async p => await GetPictureAsync(p, 2)),
            };

            var productList = products.ToList();

            if (!_catalogSettings.ExportImportProductAttributes && !_catalogSettings.ExportImportProductSpecificationAttributes)
                return await new PropertyManager<Product>(properties, _catalogSettings).ExportToXlsxAsync(productList);

            if (_productEditorSettings.ProductAttributes)
                return await ExportMasterProductsToXlsxWithAttributesAsync(properties, productList);

            return await new PropertyManager<Product>(properties, _catalogSettings).ExportToXlsxAsync(productList);
        }


        public virtual async Task<byte[]> ExportProductsToXlsxAsync(IEnumerable<Product> products)
        {
            var currentVendor = await _workContext.GetCurrentVendorAsync();
            var properties = new[]
            {
                new PropertyByName<Product>("ProductId", p => p.Id),
                new PropertyByName<Product>("ProductType", p => p.ProductTypeId, await IgnoreExportProductPropertyAsync(p => p.ProductType))
                {
                    DropDownElements = await ProductType.SimpleProduct.ToSelectListAsync(useLocalization: false)
                },
                new PropertyByName<Product>("ParentGroupedProductId", p => p.ParentGroupedProductId, await IgnoreExportProductPropertyAsync(p => p.ProductType)),
                new PropertyByName<Product>("VisibleIndividually", p => p.VisibleIndividually, await IgnoreExportProductPropertyAsync(p => p.VisibleIndividually)),
                new PropertyByName<Product>("Name", p => p.Name),
                new PropertyByName<Product>("ShortDescription", p => p.ShortDescription),
                new PropertyByName<Product>("FullDescription", p => p.FullDescription),
                //vendor can't change this field
                new PropertyByName<Product>("Vendor", p => p.VendorId, await IgnoreExportProductPropertyAsync(p => p.Vendor) || currentVendor != null)
                {
                    DropDownElements = (await _vendorService.GetAllVendorsAsync(showHidden: true)).Select(v => v as BaseEntity).ToSelectList(p => (p as Vendor)?.Name ?? string.Empty),
                    AllowBlank = true
                },
                new PropertyByName<Product>("ProductTemplate", p => p.ProductTemplateId, await IgnoreExportProductPropertyAsync(p => p.ProductTemplate))
                {
                    DropDownElements = (await _productTemplateService.GetAllProductTemplatesAsync()).Select(pt => pt as BaseEntity).ToSelectList(p => (p as ProductTemplate)?.Name ?? string.Empty)
                },
                //vendor can't change this field
                new PropertyByName<Product>("ShowOnHomepage", p => p.ShowOnHomepage, await IgnoreExportProductPropertyAsync(p => p.ShowOnHomepage) || currentVendor != null),
                //vendor can't change this field
                new PropertyByName<Product>("DisplayOrder", p => p.DisplayOrder, await IgnoreExportProductPropertyAsync(p => p.ShowOnHomepage) || currentVendor != null),
                new PropertyByName<Product>("MetaKeywords", p => p.MetaKeywords, await IgnoreExportProductPropertyAsync(p => p.Seo)),
                new PropertyByName<Product>("MetaDescription", p => p.MetaDescription, await IgnoreExportProductPropertyAsync(p => p.Seo)),
                new PropertyByName<Product>("MetaTitle", p => p.MetaTitle, await IgnoreExportProductPropertyAsync(p => p.Seo)),
                new PropertyByName<Product>("SeName", async p => await _urlRecordService.GetSeNameAsync(p, 0), await IgnoreExportProductPropertyAsync(p => p.Seo)),
                new PropertyByName<Product>("AllowCustomerReviews", p => p.AllowCustomerReviews, await IgnoreExportProductPropertyAsync(p => p.AllowCustomerReviews)),
                new PropertyByName<Product>("Published", p => p.Published, await IgnoreExportProductPropertyAsync(p => p.Published)),
                new PropertyByName<Product>("SKU", p => p.Sku),
                //Alchub Start
                new PropertyByName<Product>("UPCCode", p => p.UPCCode),
                new PropertyByName<Product>("Size", p => p.Size),
                new PropertyByName<Product>("Container", p => p.Container),
                //Alchub End
                new PropertyByName<Product>("ManufacturerPartNumber", p => p.ManufacturerPartNumber, await IgnoreExportProductPropertyAsync(p => p.ManufacturerPartNumber)),
                new PropertyByName<Product>("Gtin", p => p.Gtin, await IgnoreExportProductPropertyAsync(p => p.GTIN)),
                new PropertyByName<Product>("IsGiftCard", p => p.IsGiftCard, await IgnoreExportProductPropertyAsync(p => p.IsGiftCard)),
                new PropertyByName<Product>("GiftCardType", p => p.GiftCardTypeId, await IgnoreExportProductPropertyAsync(p => p.IsGiftCard))
                {
                    DropDownElements = await GiftCardType.Virtual.ToSelectListAsync(useLocalization: false)
                },
                new PropertyByName<Product>("OverriddenGiftCardAmount", p => p.OverriddenGiftCardAmount, await IgnoreExportProductPropertyAsync(p => p.IsGiftCard)),
                new PropertyByName<Product>("RequireOtherProducts", p => p.RequireOtherProducts, await IgnoreExportProductPropertyAsync(p => p.RequireOtherProductsAddedToCart)),
                new PropertyByName<Product>("RequiredProductIds", p => p.RequiredProductIds, await IgnoreExportProductPropertyAsync(p => p.RequireOtherProductsAddedToCart)),
                new PropertyByName<Product>("AutomaticallyAddRequiredProducts", p => p.AutomaticallyAddRequiredProducts, await IgnoreExportProductPropertyAsync(p => p.RequireOtherProductsAddedToCart)),
                new PropertyByName<Product>("IsDownload", p => p.IsDownload, await IgnoreExportProductPropertyAsync(p => p.DownloadableProduct)),
                new PropertyByName<Product>("DownloadId", p => p.DownloadId, await IgnoreExportProductPropertyAsync(p => p.DownloadableProduct)),
                new PropertyByName<Product>("UnlimitedDownloads", p => p.UnlimitedDownloads, await IgnoreExportProductPropertyAsync(p => p.DownloadableProduct)),
                new PropertyByName<Product>("MaxNumberOfDownloads", p => p.MaxNumberOfDownloads, await IgnoreExportProductPropertyAsync(p => p.DownloadableProduct)),
                new PropertyByName<Product>("DownloadActivationType", p => p.DownloadActivationTypeId, await IgnoreExportProductPropertyAsync(p => p.DownloadableProduct))
                {
                    DropDownElements = await DownloadActivationType.Manually.ToSelectListAsync(useLocalization: false)
                },
                new PropertyByName<Product>("HasSampleDownload", p => p.HasSampleDownload, await IgnoreExportProductPropertyAsync(p => p.DownloadableProduct)),
                new PropertyByName<Product>("SampleDownloadId", p => p.SampleDownloadId, await IgnoreExportProductPropertyAsync(p => p.DownloadableProduct)),
                new PropertyByName<Product>("HasUserAgreement", p => p.HasUserAgreement, await IgnoreExportProductPropertyAsync(p => p.DownloadableProduct)),
                new PropertyByName<Product>("UserAgreementText", p => p.UserAgreementText, await IgnoreExportProductPropertyAsync(p => p.DownloadableProduct)),
                new PropertyByName<Product>("IsRecurring", p => p.IsRecurring, await IgnoreExportProductPropertyAsync(p => p.RecurringProduct)),
                new PropertyByName<Product>("RecurringCycleLength", p => p.RecurringCycleLength, await IgnoreExportProductPropertyAsync(p => p.RecurringProduct)),
                new PropertyByName<Product>("RecurringCyclePeriod", p => p.RecurringCyclePeriodId, await IgnoreExportProductPropertyAsync(p => p.RecurringProduct))
                {
                    DropDownElements = await RecurringProductCyclePeriod.Days.ToSelectListAsync(useLocalization: false),
                    AllowBlank = true
                },
                new PropertyByName<Product>("RecurringTotalCycles", p => p.RecurringTotalCycles, await IgnoreExportProductPropertyAsync(p => p.RecurringProduct)),
                new PropertyByName<Product>("IsRental", p => p.IsRental, await IgnoreExportProductPropertyAsync(p => p.IsRental)),
                new PropertyByName<Product>("RentalPriceLength", p => p.RentalPriceLength, await IgnoreExportProductPropertyAsync(p => p.IsRental)),
                new PropertyByName<Product>("RentalPricePeriod", p => p.RentalPricePeriodId, await IgnoreExportProductPropertyAsync(p => p.IsRental))
                {
                    DropDownElements = await RentalPricePeriod.Days.ToSelectListAsync(useLocalization: false),
                    AllowBlank = true
                },
                new PropertyByName<Product>("IsShipEnabled", p => p.IsShipEnabled),
                new PropertyByName<Product>("IsFreeShipping", p => p.IsFreeShipping, await IgnoreExportProductPropertyAsync(p => p.FreeShipping)),
                new PropertyByName<Product>("ShipSeparately", p => p.ShipSeparately, await IgnoreExportProductPropertyAsync(p => p.ShipSeparately)),
                new PropertyByName<Product>("AdditionalShippingCharge", p => p.AdditionalShippingCharge, await IgnoreExportProductPropertyAsync(p => p.AdditionalShippingCharge)),
                new PropertyByName<Product>("DeliveryDate", p => p.DeliveryDateId, await IgnoreExportProductPropertyAsync(p => p.DeliveryDate))
                {
                    DropDownElements = (await _dateRangeService.GetAllDeliveryDatesAsync()).Select(dd => dd as BaseEntity).ToSelectList(p => (p as DeliveryDate)?.Name ?? string.Empty),
                    AllowBlank = true
                },
                new PropertyByName<Product>("IsTaxExempt", p => p.IsTaxExempt),
                new PropertyByName<Product>("TaxCategory", p => p.TaxCategoryId)
                {
                    DropDownElements = (await _taxCategoryService.GetAllTaxCategoriesAsync()).Select(tc => tc as BaseEntity).ToSelectList(p => (p as TaxCategory)?.Name ?? string.Empty),
                    AllowBlank = true
                },
                new PropertyByName<Product>("IsTelecommunicationsOrBroadcastingOrElectronicServices", p => p.IsTelecommunicationsOrBroadcastingOrElectronicServices, await IgnoreExportProductPropertyAsync(p => p.TelecommunicationsBroadcastingElectronicServices)),
                new PropertyByName<Product>("ManageInventoryMethod", p => p.ManageInventoryMethodId)
                {
                    DropDownElements = await ManageInventoryMethod.DontManageStock.ToSelectListAsync(useLocalization: false)
                },
                new PropertyByName<Product>("ProductAvailabilityRange", p => p.ProductAvailabilityRangeId, await IgnoreExportProductPropertyAsync(p => p.ProductAvailabilityRange))
                {
                    DropDownElements = (await _dateRangeService.GetAllProductAvailabilityRangesAsync()).Select(range => range as BaseEntity).ToSelectList(p => (p as ProductAvailabilityRange)?.Name ?? string.Empty),
                    AllowBlank = true
                },
                new PropertyByName<Product>("UseMultipleWarehouses", p => p.UseMultipleWarehouses, await IgnoreExportProductPropertyAsync(p => p.UseMultipleWarehouses)),
                new PropertyByName<Product>("WarehouseId", p => p.WarehouseId, await IgnoreExportProductPropertyAsync(p => p.Warehouse)),
                new PropertyByName<Product>("StockQuantity", p => p.StockQuantity),
                new PropertyByName<Product>("DisplayStockAvailability", p => p.DisplayStockAvailability, await IgnoreExportProductPropertyAsync(p => p.DisplayStockAvailability)),
                new PropertyByName<Product>("DisplayStockQuantity", p => p.DisplayStockQuantity, await IgnoreExportProductPropertyAsync(p => p.DisplayStockAvailability)),
                new PropertyByName<Product>("MinStockQuantity", p => p.MinStockQuantity, await IgnoreExportProductPropertyAsync(p => p.MinimumStockQuantity)),
                new PropertyByName<Product>("LowStockActivity", p => p.LowStockActivityId, await IgnoreExportProductPropertyAsync(p => p.LowStockActivity))
                {
                    DropDownElements = await LowStockActivity.Nothing.ToSelectListAsync(useLocalization: false)
                },
                new PropertyByName<Product>("NotifyAdminForQuantityBelow", p => p.NotifyAdminForQuantityBelow, await IgnoreExportProductPropertyAsync(p => p.NotifyAdminForQuantityBelow)),
                new PropertyByName<Product>("BackorderMode", p => p.BackorderModeId, await IgnoreExportProductPropertyAsync(p => p.Backorders))
                {
                    DropDownElements = await BackorderMode.NoBackorders.ToSelectListAsync(useLocalization: false)
                },
                new PropertyByName<Product>("AllowBackInStockSubscriptions", p => p.AllowBackInStockSubscriptions, await IgnoreExportProductPropertyAsync(p => p.AllowBackInStockSubscriptions)),
                new PropertyByName<Product>("OrderMinimumQuantity", p => p.OrderMinimumQuantity, await IgnoreExportProductPropertyAsync(p => p.MinimumCartQuantity)),
                new PropertyByName<Product>("OrderMaximumQuantity", p => p.OrderMaximumQuantity, await IgnoreExportProductPropertyAsync(p => p.MaximumCartQuantity)),
                new PropertyByName<Product>("AllowedQuantities", p => p.AllowedQuantities, await IgnoreExportProductPropertyAsync(p => p.AllowedQuantities)),
                new PropertyByName<Product>("AllowAddingOnlyExistingAttributeCombinations", p => p.AllowAddingOnlyExistingAttributeCombinations, await IgnoreExportProductPropertyAsync(p => p.AllowAddingOnlyExistingAttributeCombinations)),
                new PropertyByName<Product>("NotReturnable", p => p.NotReturnable, await IgnoreExportProductPropertyAsync(p => p.NotReturnable)),
                new PropertyByName<Product>("DisableBuyButton", p => p.DisableBuyButton, await IgnoreExportProductPropertyAsync(p => p.DisableBuyButton)),
                new PropertyByName<Product>("DisableWishlistButton", p => p.DisableWishlistButton, await IgnoreExportProductPropertyAsync(p => p.DisableWishlistButton)),
                new PropertyByName<Product>("AvailableForPreOrder", p => p.AvailableForPreOrder, await IgnoreExportProductPropertyAsync(p => p.AvailableForPreOrder)),
                new PropertyByName<Product>("PreOrderAvailabilityStartDateTimeUtc", p => p.PreOrderAvailabilityStartDateTimeUtc, await IgnoreExportProductPropertyAsync(p => p.AvailableForPreOrder)),
                new PropertyByName<Product>("CallForPrice", p => p.CallForPrice, await IgnoreExportProductPropertyAsync(p => p.CallForPrice)),
                new PropertyByName<Product>("Price", p => p.Price),
                new PropertyByName<Product>("OldPrice", p => p.OldPrice, await IgnoreExportProductPropertyAsync(p => p.OldPrice)),
                new PropertyByName<Product>("ProductCost", p => p.ProductCost, await IgnoreExportProductPropertyAsync(p => p.ProductCost)),
                new PropertyByName<Product>("CustomerEntersPrice", p => p.CustomerEntersPrice, await IgnoreExportProductPropertyAsync(p => p.CustomerEntersPrice)),
                new PropertyByName<Product>("MinimumCustomerEnteredPrice", p => p.MinimumCustomerEnteredPrice, await IgnoreExportProductPropertyAsync(p => p.CustomerEntersPrice)),
                new PropertyByName<Product>("MaximumCustomerEnteredPrice", p => p.MaximumCustomerEnteredPrice, await IgnoreExportProductPropertyAsync(p => p.CustomerEntersPrice)),
                new PropertyByName<Product>("BasepriceEnabled", p => p.BasepriceEnabled, await IgnoreExportProductPropertyAsync(p => p.PAngV)),
                new PropertyByName<Product>("BasepriceAmount", p => p.BasepriceAmount, await IgnoreExportProductPropertyAsync(p => p.PAngV)),
                new PropertyByName<Product>("BasepriceUnit", p => p.BasepriceUnitId, await IgnoreExportProductPropertyAsync(p => p.PAngV))
                {
                    DropDownElements = (await _measureService.GetAllMeasureWeightsAsync()).Select(mw => mw as BaseEntity).ToSelectList(p => (p as MeasureWeight)?.Name ?? string.Empty),
                    AllowBlank = true
                },
                new PropertyByName<Product>("BasepriceBaseAmount", p => p.BasepriceBaseAmount, await IgnoreExportProductPropertyAsync(p => p.PAngV)),
                new PropertyByName<Product>("BasepriceBaseUnit", p => p.BasepriceBaseUnitId, await IgnoreExportProductPropertyAsync(p => p.PAngV))
                {
                    DropDownElements = (await _measureService.GetAllMeasureWeightsAsync()).Select(mw => mw as BaseEntity).ToSelectList(p => (p as MeasureWeight)?.Name ?? string.Empty),
                    AllowBlank = true
                },
                new PropertyByName<Product>("MarkAsNew", p => p.MarkAsNew, await IgnoreExportProductPropertyAsync(p => p.MarkAsNew)),
                new PropertyByName<Product>("MarkAsNewStartDateTimeUtc", p => p.MarkAsNewStartDateTimeUtc, await IgnoreExportProductPropertyAsync(p => p.MarkAsNew)),
                new PropertyByName<Product>("MarkAsNewEndDateTimeUtc", p => p.MarkAsNewEndDateTimeUtc, await IgnoreExportProductPropertyAsync(p => p.MarkAsNew)),
                new PropertyByName<Product>("Weight", p => p.Weight, await IgnoreExportProductPropertyAsync(p => p.Weight)),
                new PropertyByName<Product>("Length", p => p.Length, await IgnoreExportProductPropertyAsync(p => p.Dimensions)),
                new PropertyByName<Product>("Width", p => p.Width, await IgnoreExportProductPropertyAsync(p => p.Dimensions)),
                new PropertyByName<Product>("Height", p => p.Height, await IgnoreExportProductPropertyAsync(p => p.Dimensions)),
                new PropertyByName<Product>("Categories", GetCategoriesAsync),
                new PropertyByName<Product>("Manufacturers", GetManufacturersAsync, await IgnoreExportProductPropertyAsync(p => p.Manufacturers)),
                new PropertyByName<Product>("ProductTags", GetProductTagsAsync, await IgnoreExportProductPropertyAsync(p => p.ProductTags)),
                new PropertyByName<Product>("IsLimitedToStores", p => p.LimitedToStores, await IgnoreExportLimitedToStoreAsync()),
                new PropertyByName<Product>("LimitedToStores", GetLimitedToStoresAsync, await IgnoreExportLimitedToStoreAsync()),
                new PropertyByName<Product>("Picture1", async p => await GetPictureAsync(p, 0)),
                new PropertyByName<Product>("Picture2", async p => await GetPictureAsync(p, 1)),
                new PropertyByName<Product>("Picture3", async p => await GetPictureAsync(p, 2)),
                new PropertyByName<Product>("VarientProductIds", async p => await GetAssociateProductIds(p)),
            };

            var productList = products.ToList();

            var productAdvancedMode = true;
            try
            {
                productAdvancedMode = await _genericAttributeService.GetAttributeAsync<bool>(await _workContext.GetCurrentCustomerAsync(), "product-advanced-mode");
            }
            catch (ArgumentNullException)
            {
            }

            if (!_catalogSettings.ExportImportProductAttributes && !_catalogSettings.ExportImportProductSpecificationAttributes)
                return await new PropertyManager<Product>(properties, _catalogSettings).ExportToXlsxAsync(productList);

            if (productAdvancedMode || _productEditorSettings.ProductAttributes)
                return await ExportProductsToXlsxWithAttributesAsync(properties, productList);

            return await new PropertyManager<Product>(properties, _catalogSettings).ExportToXlsxAsync(productList);
        }
        #endregion
    }
}
