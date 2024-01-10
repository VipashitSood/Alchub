using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Media;

namespace Nop.Services.Catalog
{
    /// <summary>
    /// Copy Product service
    /// </summary>
    public partial class CopyProductService : ICopyProductService
    {

        #region Utilities

        /// <summary>
        /// Copy product
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="newName">New product name</param>
        /// <param name="isPublished">A value indicating whether a new product is published</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the 
        /// </returns>
        protected virtual async Task<Product> CopyBaseProductDataAsync(Product product, string newName, bool isPublished, bool creatingMasterProduct = false, int vendorId = 0)
        {
            //master product validation
            if (creatingMasterProduct && vendorId > 0)
                vendorId = 0;

            //product download & sample download
            var downloadId = product.DownloadId;
            var sampleDownloadId = product.SampleDownloadId;
            if (product.IsDownload)
            {
                var download = await _downloadService.GetDownloadByIdAsync(product.DownloadId);
                if (download != null)
                {
                    var downloadCopy = new Download
                    {
                        DownloadGuid = Guid.NewGuid(),
                        UseDownloadUrl = download.UseDownloadUrl,
                        DownloadUrl = download.DownloadUrl,
                        DownloadBinary = download.DownloadBinary,
                        ContentType = download.ContentType,
                        Filename = download.Filename,
                        Extension = download.Extension,
                        IsNew = download.IsNew
                    };
                    await _downloadService.InsertDownloadAsync(downloadCopy);
                    downloadId = downloadCopy.Id;
                }

                if (product.HasSampleDownload)
                {
                    var sampleDownload = await _downloadService.GetDownloadByIdAsync(product.SampleDownloadId);
                    if (sampleDownload != null)
                    {
                        var sampleDownloadCopy = new Download
                        {
                            DownloadGuid = Guid.NewGuid(),
                            UseDownloadUrl = sampleDownload.UseDownloadUrl,
                            DownloadUrl = sampleDownload.DownloadUrl,
                            DownloadBinary = sampleDownload.DownloadBinary,
                            ContentType = sampleDownload.ContentType,
                            Filename = sampleDownload.Filename,
                            Extension = sampleDownload.Extension,
                            IsNew = sampleDownload.IsNew
                        };
                        await _downloadService.InsertDownloadAsync(sampleDownloadCopy);
                        sampleDownloadId = sampleDownloadCopy.Id;
                    }
                }
            }

            var newSku = !string.IsNullOrWhiteSpace(product.Sku)
                ? string.Format(await _localizationService.GetResourceAsync("Admin.Catalog.Products.Copy.SKU.New"), product.Sku)
                : product.Sku;

            //++Alchub

            var newUpc = product.UPCCode;
            //check for grouped product
            if (creatingMasterProduct)
            {
                if (product.ProductType == ProductType.SimpleProduct)
                    //check if copying master product to create new master product, then generate upc(sku) - 01-02-23
                    newUpc = await _productService.GenerateMasterProductSKU(); //{AH000000}
                else
                    newUpc = string.Empty;
            }
            else
            {
                //make sure vendor product sku should same as base master product sku
                newSku = product.Sku;
            }

            //--Alchub

            // product
            var productCopy = new Product
            {
                ProductTypeId = product.ProductTypeId,
                ParentGroupedProductId = product.ParentGroupedProductId,
                VisibleIndividually = product.VisibleIndividually,
                Name = newName,
                ShortDescription = product.ShortDescription,
                FullDescription = product.FullDescription,
                VendorId = vendorId,
                ProductTemplateId = product.ProductTemplateId,
                AdminComment = product.AdminComment,
                ShowOnHomepage = product.ShowOnHomepage,
                MetaKeywords = product.MetaKeywords,
                MetaDescription = product.MetaDescription,
                MetaTitle = product.MetaTitle,
                AllowCustomerReviews = product.AllowCustomerReviews,
                LimitedToStores = product.LimitedToStores,
                Sku = newSku,
                ManufacturerPartNumber = product.ManufacturerPartNumber,
                Gtin = product.Gtin,
                IsGiftCard = product.IsGiftCard,
                GiftCardType = product.GiftCardType,
                OverriddenGiftCardAmount = product.OverriddenGiftCardAmount,
                RequireOtherProducts = product.RequireOtherProducts,
                RequiredProductIds = product.RequiredProductIds,
                AutomaticallyAddRequiredProducts = product.AutomaticallyAddRequiredProducts,
                IsDownload = product.IsDownload,
                DownloadId = downloadId,
                UnlimitedDownloads = product.UnlimitedDownloads,
                MaxNumberOfDownloads = product.MaxNumberOfDownloads,
                DownloadExpirationDays = product.DownloadExpirationDays,
                DownloadActivationType = product.DownloadActivationType,
                HasSampleDownload = product.HasSampleDownload,
                SampleDownloadId = sampleDownloadId,
                HasUserAgreement = product.HasUserAgreement,
                UserAgreementText = product.UserAgreementText,
                IsRecurring = product.IsRecurring,
                RecurringCycleLength = product.RecurringCycleLength,
                RecurringCyclePeriod = product.RecurringCyclePeriod,
                RecurringTotalCycles = product.RecurringTotalCycles,
                IsRental = product.IsRental,
                RentalPriceLength = product.RentalPriceLength,
                RentalPricePeriod = product.RentalPricePeriod,
                IsShipEnabled = product.IsShipEnabled,
                IsFreeShipping = product.IsFreeShipping,
                ShipSeparately = product.ShipSeparately,
                AdditionalShippingCharge = product.AdditionalShippingCharge,
                DeliveryDateId = product.DeliveryDateId,
                IsTaxExempt = product.IsTaxExempt,
                TaxCategoryId = product.TaxCategoryId,
                IsTelecommunicationsOrBroadcastingOrElectronicServices =
                product.IsTelecommunicationsOrBroadcastingOrElectronicServices,
                ManageInventoryMethod = ManageInventoryMethod.ManageStock,
                ProductAvailabilityRangeId = product.ProductAvailabilityRangeId,
                UseMultipleWarehouses = product.UseMultipleWarehouses,
                WarehouseId = product.WarehouseId,
                StockQuantity = product.StockQuantity,
                DisplayStockAvailability = product.DisplayStockAvailability,
                DisplayStockQuantity = product.DisplayStockQuantity,
                MinStockQuantity = product.MinStockQuantity,
                LowStockActivityId = product.LowStockActivityId,
                NotifyAdminForQuantityBelow = product.NotifyAdminForQuantityBelow,
                BackorderMode = product.BackorderMode,
                AllowBackInStockSubscriptions = product.AllowBackInStockSubscriptions,
                OrderMinimumQuantity = product.OrderMinimumQuantity,
                OrderMaximumQuantity = product.OrderMaximumQuantity,
                AllowedQuantities = product.AllowedQuantities,
                AllowAddingOnlyExistingAttributeCombinations = product.AllowAddingOnlyExistingAttributeCombinations,
                NotReturnable = product.NotReturnable,
                DisableBuyButton = product.DisableBuyButton,
                DisableWishlistButton = product.DisableWishlistButton,
                AvailableForPreOrder = product.AvailableForPreOrder,
                PreOrderAvailabilityStartDateTimeUtc = product.PreOrderAvailabilityStartDateTimeUtc,
                CallForPrice = product.CallForPrice,
                Price = product.Price,
                OldPrice = product.OldPrice,
                ProductCost = product.ProductCost,
                CustomerEntersPrice = product.CustomerEntersPrice,
                MinimumCustomerEnteredPrice = product.MinimumCustomerEnteredPrice,
                MaximumCustomerEnteredPrice = product.MaximumCustomerEnteredPrice,
                BasepriceEnabled = product.BasepriceEnabled,
                BasepriceAmount = product.BasepriceAmount,
                BasepriceUnitId = product.BasepriceUnitId,
                BasepriceBaseAmount = product.BasepriceBaseAmount,
                BasepriceBaseUnitId = product.BasepriceBaseUnitId,
                MarkAsNew = product.MarkAsNew,
                MarkAsNewStartDateTimeUtc = product.MarkAsNewStartDateTimeUtc,
                MarkAsNewEndDateTimeUtc = product.MarkAsNewEndDateTimeUtc,
                Weight = product.Weight,
                Length = product.Length,
                Width = product.Width,
                Height = product.Height,
                AvailableStartDateTimeUtc = product.AvailableStartDateTimeUtc,
                AvailableEndDateTimeUtc = product.AvailableEndDateTimeUtc,
                DisplayOrder = product.DisplayOrder,
                Published = isPublished,
                Deleted = product.Deleted,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow,
                UPCCode = newUpc,
                IsMaster = creatingMasterProduct ? product.IsMaster : false,
                Size = product.Size,
                Container = product.Container
            };

            //validate search engine name
            await _productService.InsertProductAsync(productCopy);

            //search engine name
            await _urlRecordService.SaveSlugAsync(productCopy, await _urlRecordService.ValidateSeNameAsync(productCopy, string.Empty, productCopy.Name, true), 0);
            return productCopy;
        }

        /// <summary>
        /// Copy associated products
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="isPublished">A value indicating whether they should be published</param>
        /// <param name="copyImages">A value indicating whether to copy images</param>
        /// <param name="copyAssociatedProducts">A value indicating whether to copy associated products</param>
        /// <param name="productCopy">New product</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        protected virtual async Task CopyAssociatedProductsAsync(Product product, bool isPublished, bool copyImages, bool copyAssociatedProducts, Product productCopy, bool creatingMasterProduct = false, int vendorId = 0)
        {
            if (!copyAssociatedProducts)
                return;

            var associatedProducts = await _productService.GetAssociatedProductsAsync(product.Id, showHidden: true);
            foreach (var associatedProduct in associatedProducts)
            {
                var associatedProductCopy = await CopyProductAsync(associatedProduct,
                    string.Format(NopCatalogDefaults.ProductCopyNameTemplate, associatedProduct.Name),
                    isPublished, copyImages, false, creatingMasterProduct: creatingMasterProduct, vendorId: vendorId);
                associatedProductCopy.ParentGroupedProductId = productCopy.Id;
                await _productService.UpdateProductAsync(associatedProductCopy);
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Create a copy of product with all depended data
        /// </summary>
        /// <param name="product">The product to copy</param>
        /// <param name="newName">The name of product duplicate</param>
        /// <param name="isPublished">A value indicating whether the product duplicate should be published</param>
        /// <param name="copyImages">A value indicating whether the product images should be copied</param>
        /// <param name="copyAssociatedProducts">A value indicating whether the copy associated products</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the product copy
        /// </returns>
        public virtual async Task<Product> CopyProductAsync(Product product, string newName,
            bool isPublished = true, bool copyImages = true, bool copyAssociatedProducts = true, bool creatingMasterProduct = false, int vendorId = 0)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            if (string.IsNullOrEmpty(newName))
                throw new ArgumentException("Product name is required");

            //do not copy mapping data for vendor product - 20-06-23
            var isVendorProductCopy = !creatingMasterProduct;

            var productCopy = await CopyBaseProductDataAsync(product, newName, isPublished, creatingMasterProduct, vendorId: vendorId);

            //localization
            await CopyLocalizationDataAsync(product, productCopy);

            if (!isVendorProductCopy)
                //copy product tags
                foreach (var productTag in await _productTagService.GetAllProductTagsByProductIdAsync(product.Id))
                    await _productTagService.InsertProductProductTagMappingAsync(new ProductProductTagMapping { ProductTagId = productTag.Id, ProductId = productCopy.Id });

            await _productService.UpdateProductAsync(productCopy);

            //quantity change history
            await _productService.AddStockQuantityHistoryEntryAsync(productCopy, product.StockQuantity, product.StockQuantity, product.WarehouseId,
                string.Format(await _localizationService.GetResourceAsync("Admin.StockQuantityHistory.Messages.CopyProduct"), product.Id));

            //copy product pictures
            var originalNewPictureIdentifiers = new Dictionary<int, int>();

            if (!isVendorProductCopy)
                originalNewPictureIdentifiers = await CopyProductPicturesAsync(product, newName, copyImages, productCopy);

            if (!isVendorProductCopy)
                //product specifications
                await CopyProductSpecificationsAsync(product, productCopy);

            //product <-> warehouses mappings
            await CopyWarehousesMappingAsync(product, productCopy);
            //product <-> categories mappings
            await CopyCategoriesMappingAsync(product, productCopy);
            //product <-> manufacturers mappings
            await CopyManufacturersMappingAsync(product, productCopy);
            //product <-> related products mappings
            await CopyRelatedProductsMappingAsync(product, productCopy);
            //product <-> cross sells mappings
            await CopyCrossSellsMappingAsync(product, productCopy);
            //product <-> attributes mappings
            await CopyAttributesMappingAsync(product, productCopy, originalNewPictureIdentifiers);
            //product <-> discounts mapping
            await CopyDiscountsMappingAsync(product, productCopy);

            //store mapping
            var selectedStoreIds = await _storeMappingService.GetStoresIdsWithAccessAsync(product);
            foreach (var id in selectedStoreIds)
                await _storeMappingService.InsertStoreMappingAsync(productCopy, id);

            if (!isVendorProductCopy)
            {
                //tier prices
                await CopyTierPricesAsync(product, productCopy);

                //update "HasTierPrices" and "HasDiscountsApplied" properties
                await _productService.UpdateHasTierPricesPropertyAsync(productCopy);
                await _productService.UpdateHasDiscountsAppliedAsync(productCopy);

                //associated products
                await CopyAssociatedProductsAsync(product, isPublished, copyImages, copyAssociatedProducts, productCopy, creatingMasterProduct, vendorId: vendorId);
            }

            return productCopy;
        }

        #endregion
    }
}