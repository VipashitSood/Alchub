using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LinqToDB.Data;
using LinqToDB.DataProvider;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Vendors;
using Nop.Data;

namespace Nop.Services.Alchub.ElasticSearch
{
    public class ElasticSearchProductService : IElasticSearchProductService
    {
        #region Fields

        private readonly INopDataProvider _dataProvider;

        #endregion

        #region Ctor

        public ElasticSearchProductService(INopDataProvider dataProvider)
        {
            _dataProvider = dataProvider;
        }

        #endregion
        #region Elastic search product sync

        /// <summary>
        /// Get all products for Elastic Sync
        /// </summary>
        /// <returns></returns>
        public async Task<Master_products_result> SyncAllProducts(int page, long noOfProducts)
        {
            try
            {
                Master_products_result elasticSearchProductResult = new Master_products_result();
                elasticSearchProductResult.Master_Products = new List<Master_products>();
                //Get All Data from Sp
                var data = await GetAllProductsToSyncElasticCacheAsync(page, noOfProducts);
                if (data.Any())
                {
                    var productIds = data.GroupBy(x => x.Id).Select(x => x.OrderByDescending(y => y.Id));
                    foreach (var p in productIds)
                    {
                        Master_products master_products = new Master_products();
                        //Get list of all product by productId
                        int singleProductId = p.Select(y => y.Id).FirstOrDefault();

                        //Get Master Products
                        var selectedProducts = data.Where(x => x.Id == singleProductId).ToList();
                        var singleSelectedProduct = selectedProducts.FirstOrDefault();
                        master_products.Id = singleProductId;
                        master_products.SeName = singleSelectedProduct.ProductSlug;
                        if (singleSelectedProduct != null)
                        {
                            
                            elasticSearchProductResult.TotalCount = singleSelectedProduct.TotalCount;
                            int venderPage = 1;
                            long vendorNoOfProducts = 100;
                            master_products.Product = await MapProductResult(singleSelectedProduct);
                            ////Get Associated Products
                            //var selectedAssociatedProducts = data.Where(x => x.ParentGroupedProductId == singleProductId).ToList();
                            //if (selectedAssociatedProducts.Count > 0)
                            //{
                            //    selectedProducts.AddRange(selectedAssociatedProducts);
                            //}
                            //Get Vendor Products
                            var vendorDataList = await GetAllVendorProductsToSyncElasticCacheAsync(singleSelectedProduct.UPCCode, venderPage, vendorNoOfProducts);
                            var singleVendor = vendorDataList.FirstOrDefault();
                            var totalVendors = vendorDataList.Count;
                            selectedProducts.AddRange(vendorDataList);
                            while (totalVendors >= vendorNoOfProducts)
                            {
                                venderPage++;
                                var nextPageVenderDataList = await GetAllVendorProductsToSyncElasticCacheAsync(singleSelectedProduct.UPCCode, venderPage, vendorNoOfProducts);
                                selectedProducts.AddRange(nextPageVenderDataList);
                                totalVendors = nextPageVenderDataList.Count;
                            }

                            if (selectedProducts.Any())
                            {

                                //Get All manufacturers of the product
                                master_products.Manufacturers = selectedProducts.Where(x => x.ManufacturerId != null).DistinctBy(x => x.ManufacturerId).Select(x => new ManufacturerDetails()
                                {
                                    Manufacturer = new Manufacturer()
                                    {
                                        Id = (int)x.ManufacturerId,
                                        Name = x.ManufacturerName,
                                    }
                                }).ToList();

                                //Get Vender List associated with products
                                master_products.Vendors = selectedProducts.Where(x => x.VenderId != null && x.IsMaster == false).DistinctBy(x => x.VenderId).Select(x => new VendorDetails()
                                {
                                    Price = x.Price,
                                    StockQuantity = x.StockQuantity,
                                    Vendor = new Vendor()
                                    {
                                        Id = Convert.ToInt32(x.VenderId),
                                        Name = x.VenderName,
                                        Email = x.Email,
                                        Description = x.Description,
                                        //VenderMetaKeywords = x.VenderMetaKeywords,
                                        PictureId = Convert.ToInt32(x.PictureId),
                                        //VenderMetaTitle = x.VenderMetaTitle,
                                        PageSizeOptions = x.PageSizeOptions,
                                        AddressId = Convert.ToInt32(x.AddressId),
                                        //VenderAdminComment = x.VenderAdminComment,
                                        //VenderDisplayOrder = x.VenderDisplayOrder,
                                        //VenderMetaDescription = x.VenderMetaDescription,
                                        PageSize = Convert.ToInt32(x.PageSize),
                                        AllowCustomersToSelectPageSize = Convert.ToBoolean(x.AllowCustomersToSelectPageSize),
                                        PriceRangeFiltering = Convert.ToBoolean(x.PriceRangeFiltering),
                                        PriceFrom = x.PriceFrom,
                                        PriceTo = x.PriceTo,
                                        ManuallyPriceRange = Convert.ToBoolean(x.ManuallyPriceRange),
                                        ManageDelivery = Convert.ToBoolean(x.ManageDelivery),
                                        PickAvailable = Convert.ToBoolean(x.PickAvailable),
                                        GeoLocationCoordinates = x.GeoLocationCoordinates,
                                        MinimumOrderAmount = Convert.ToDecimal(x.MinimumOrderAmount),
                                        OrderTax = Convert.ToDecimal(x.OrderTax),
                                        DeliveryAvailable = Convert.ToBoolean(x.DeliveryAvailable),
                                        PhoneNumber = x.PhoneNumber,
                                        PickupAddress = x.PickupAddress,
                                        GeoFenceShapeTypeId = Convert.ToInt32(x.GeoFenceShapeTypeId),
                                        RadiusDistance = Convert.ToDecimal(x.RadiusDistance)
                                    }
                                }).ToList();

                                //Get Category List associated with products
                                master_products.Categories = selectedProducts.Where(x => x.CategoryId is not null).DistinctBy(x => x.CategoryId).Select(x => new CategoryDetails()
                                {
                                    SeName = x.CategorySlug,
                                    Category = new Category()
                                    {
                                        Id = (int)x.CategoryId,
                                        Name = x.CategoryName,
                                        ParentCategoryId = Convert.ToInt32(x.ParentCategoryId),
                                    }

                                }).ToList();
                                ////Get Sub Category List associated with products
                                //var subCatergories = selectedProducts.Where(x => x.CategoryId != null && x.SubCategoryId != null && x.ParentCategoryId > 0).DistinctBy(x => x.SubCategoryId).Select(x => new CategoryDetails()
                                //{
                                //    Category = new Category()
                                //    {
                                //        Id = (int)x.SubCategoryId,
                                //        Name = x.SubCategoryName,
                                //        DisplayOrder = Convert.ToInt32(x.SubCategoryDisplayOrder),
                                //        ParentCategoryId = Convert.ToInt32(x.ParentCategoryId),
                                //        CategoryTemplateId = Convert.ToInt32(x.CategoryTemplateId),
                                //    }
                                //}).ToList();
                                ////master_products.SubCategoryList = subCatergories;
                                //master_products.Categories.AddRange(subCatergories);
                                //Get Specification Attribute List associated with products
                                master_products.Specifications = selectedProducts.Where(x => x.SpecificationAttributeId != null).DistinctBy(x => x.SpecificationAttributeId).Select(x => new SpecificationAttributeDetails()
                                {
                                    SpecificationAttribute = new SpecificationAttribute()
                                    {
                                        Id = (int)x.SpecificationAttributeId,
                                        Name = x.SpecificationAttributeName,
                                        DisplayOrder = Convert.ToInt32(x.SpecificationAttributeDisplayOrder),
                                    },
                                    //Specification Attribute options
                                    SpecificationAttributeOptionDetails = selectedProducts.Where(y => y.SpecificationAttributeId == x.SpecificationAttributeId).DistinctBy(x => x.SpecificationAttributeOptionsId)
                                    .Select(x => new SpecificationAttributeOptionDetails()
                                    {
                                        SpecificationAttributeOption = new SpecificationAttributeOption()
                                        {
                                            Id = (int)x.SpecificationAttributeOptionsId,
                                            Name = x.SpecificationAttributeOptionsName,
                                        }
                                    }).ToList()
                                }).ToList();
                                // Add signle object to  product list
                                elasticSearchProductResult.Master_Products.Add(master_products);
                            }
                        }
                    }
                }
                return elasticSearchProductResult;
            }
            catch (Exception)
            {
                throw;
            }
        }
        private async Task<Product> MapProductResult(ElasticSearchProductServiceResult elasticSearchProductServiceResult)
        {
            Product product = new Product();
            product.Id = elasticSearchProductServiceResult.Id;
            product.ProductTypeId = elasticSearchProductServiceResult.ProductTypeId;
            product.ParentGroupedProductId = elasticSearchProductServiceResult.ParentGroupedProductId;
            product.VisibleIndividually = elasticSearchProductServiceResult.VisibleIndividually;
            product.Name = elasticSearchProductServiceResult.Name;
            product.ShortDescription = elasticSearchProductServiceResult.ShortDescription;
            product.FullDescription = elasticSearchProductServiceResult.FullDescription;
            product.AdminComment = elasticSearchProductServiceResult.AdminComment;
            product.ProductTemplateId = elasticSearchProductServiceResult.ProductTemplateId;
            product.VendorId = elasticSearchProductServiceResult.VendorId;
            product.ShowOnHomepage = elasticSearchProductServiceResult.ShowOnHomepage;
            product.MetaKeywords = elasticSearchProductServiceResult.MetaKeywords;
            product.MetaDescription = elasticSearchProductServiceResult.MetaDescription;
            product.MetaTitle = elasticSearchProductServiceResult.MetaTitle;
            product.AllowCustomerReviews = elasticSearchProductServiceResult.AllowCustomerReviews;
            product.ApprovedRatingSum = elasticSearchProductServiceResult.ApprovedRatingSum;
            product.NotApprovedRatingSum = elasticSearchProductServiceResult.NotApprovedRatingSum;
            product.ApprovedTotalReviews = elasticSearchProductServiceResult.ApprovedTotalReviews;
            product.NotApprovedTotalReviews = elasticSearchProductServiceResult.NotApprovedTotalReviews;
            product.SubjectToAcl = elasticSearchProductServiceResult.SubjectToAcl;
            product.LimitedToStores = elasticSearchProductServiceResult.LimitedToStores;
            product.Sku = elasticSearchProductServiceResult.Sku;
            product.ManufacturerPartNumber = elasticSearchProductServiceResult.ManufacturerPartNumber;
            product.Gtin = elasticSearchProductServiceResult.Gtin;
            product.IsGiftCard = elasticSearchProductServiceResult.IsGiftCard;
            product.GiftCardTypeId = elasticSearchProductServiceResult.GiftCardTypeId;
            product.OverriddenGiftCardAmount = elasticSearchProductServiceResult.OverriddenGiftCardAmount;
            product.RequireOtherProducts = elasticSearchProductServiceResult.RequireOtherProducts;
            product.RequiredProductIds = elasticSearchProductServiceResult.RequiredProductIds;
            product.AutomaticallyAddRequiredProducts = elasticSearchProductServiceResult.AutomaticallyAddRequiredProducts;
            product.IsDownload = elasticSearchProductServiceResult.IsDownload;
            product.DownloadId = elasticSearchProductServiceResult.DownloadId;
            product.UnlimitedDownloads = elasticSearchProductServiceResult.UnlimitedDownloads;
            product.MaxNumberOfDownloads = elasticSearchProductServiceResult.MaxNumberOfDownloads;
            product.DownloadExpirationDays = elasticSearchProductServiceResult.DownloadExpirationDays;
            product.DownloadActivationTypeId = elasticSearchProductServiceResult.DownloadActivationTypeId;
            product.HasSampleDownload = elasticSearchProductServiceResult.HasSampleDownload;
            product.SampleDownloadId = elasticSearchProductServiceResult.SampleDownloadId;
            product.HasUserAgreement = elasticSearchProductServiceResult.HasUserAgreement;
            product.UserAgreementText = elasticSearchProductServiceResult.UserAgreementText;
            product.IsRecurring = elasticSearchProductServiceResult.IsRecurring;
            product.RecurringCycleLength = elasticSearchProductServiceResult.RecurringCycleLength;
            product.RecurringCyclePeriodId = elasticSearchProductServiceResult.RecurringCyclePeriodId;
            product.RecurringTotalCycles = elasticSearchProductServiceResult.RecurringTotalCycles;
            product.IsRental = elasticSearchProductServiceResult.IsRental;
            product.RentalPriceLength = elasticSearchProductServiceResult.RentalPriceLength;
            product.RentalPricePeriodId = elasticSearchProductServiceResult.RentalPricePeriodId;
            product.IsShipEnabled = elasticSearchProductServiceResult.IsShipEnabled;
            product.IsFreeShipping = elasticSearchProductServiceResult.IsFreeShipping;
            product.ShipSeparately = elasticSearchProductServiceResult.ShipSeparately;
            product.AdditionalShippingCharge = elasticSearchProductServiceResult.AdditionalShippingCharge;
            product.DeliveryDateId = elasticSearchProductServiceResult.DeliveryDateId;
            product.IsTaxExempt = elasticSearchProductServiceResult.IsTaxExempt;
            product.TaxCategoryId = elasticSearchProductServiceResult.TaxCategoryId;
            product.IsTelecommunicationsOrBroadcastingOrElectronicServices = elasticSearchProductServiceResult.IsTelecommunicationsOrBroadcastingOrElectronicServices;
            product.ManageInventoryMethodId = elasticSearchProductServiceResult.ManageInventoryMethodId;
            product.ProductAvailabilityRangeId = elasticSearchProductServiceResult.ProductAvailabilityRangeId;
            product.UseMultipleWarehouses = elasticSearchProductServiceResult.UseMultipleWarehouses;
            product.WarehouseId = elasticSearchProductServiceResult.WarehouseId;
            product.StockQuantity = elasticSearchProductServiceResult.StockQuantity;
            product.DisplayStockAvailability = elasticSearchProductServiceResult.DisplayStockAvailability;
            product.DisplayStockQuantity = elasticSearchProductServiceResult.DisplayStockQuantity;
            product.MinStockQuantity = elasticSearchProductServiceResult.MinStockQuantity;
            product.LowStockActivityId = elasticSearchProductServiceResult.LowStockActivityId;
            product.NotifyAdminForQuantityBelow = elasticSearchProductServiceResult.NotifyAdminForQuantityBelow;
            product.BackorderModeId = elasticSearchProductServiceResult.BackorderModeId;
            product.AllowBackInStockSubscriptions = elasticSearchProductServiceResult.AllowBackInStockSubscriptions;
            product.OrderMinimumQuantity = elasticSearchProductServiceResult.OrderMinimumQuantity;
            product.OrderMaximumQuantity = elasticSearchProductServiceResult.OrderMaximumQuantity;
            product.AllowedQuantities = elasticSearchProductServiceResult.AllowedQuantities;
            product.AllowAddingOnlyExistingAttributeCombinations = elasticSearchProductServiceResult.AllowAddingOnlyExistingAttributeCombinations;
            product.NotReturnable = elasticSearchProductServiceResult.NotReturnable;
            product.DisableBuyButton = elasticSearchProductServiceResult.DisableBuyButton;
            product.DisableWishlistButton = elasticSearchProductServiceResult.DisableWishlistButton;
            product.AvailableForPreOrder = elasticSearchProductServiceResult.AvailableForPreOrder;
            product.PreOrderAvailabilityStartDateTimeUtc = elasticSearchProductServiceResult.PreOrderAvailabilityStartDateTimeUtc;
            product.CallForPrice = elasticSearchProductServiceResult.CallForPrice;
            product.Price = elasticSearchProductServiceResult.Price;
            product.OldPrice = elasticSearchProductServiceResult.OldPrice;
            product.ProductCost = elasticSearchProductServiceResult.ProductCost;
            product.CustomerEntersPrice = elasticSearchProductServiceResult.CustomerEntersPrice;
            product.MinimumCustomerEnteredPrice = elasticSearchProductServiceResult.MinimumCustomerEnteredPrice;
            product.MaximumCustomerEnteredPrice = elasticSearchProductServiceResult.MaximumCustomerEnteredPrice;
            product.BasepriceEnabled = elasticSearchProductServiceResult.BasepriceEnabled;
            product.BasepriceAmount = elasticSearchProductServiceResult.BasepriceAmount;
            product.BasepriceUnitId = elasticSearchProductServiceResult.BasepriceUnitId;
            product.BasepriceBaseAmount = elasticSearchProductServiceResult.BasepriceBaseAmount;
            product.BasepriceBaseUnitId = elasticSearchProductServiceResult.BasepriceBaseUnitId;
            product.MarkAsNew = elasticSearchProductServiceResult.MarkAsNew;
            product.MarkAsNewStartDateTimeUtc = elasticSearchProductServiceResult.MarkAsNewStartDateTimeUtc;
            product.MarkAsNewEndDateTimeUtc = elasticSearchProductServiceResult.MarkAsNewEndDateTimeUtc;
            product.HasTierPrices = elasticSearchProductServiceResult.HasTierPrices;
            product.HasDiscountsApplied = elasticSearchProductServiceResult.HasDiscountsApplied;
            product.Weight = elasticSearchProductServiceResult.Weight;
            product.Length = elasticSearchProductServiceResult.Length;
            product.Width = elasticSearchProductServiceResult.Width;
            product.Height = elasticSearchProductServiceResult.Height;
            product.AvailableStartDateTimeUtc = elasticSearchProductServiceResult.AvailableStartDateTimeUtc;
            product.AvailableEndDateTimeUtc = elasticSearchProductServiceResult.AvailableEndDateTimeUtc;
            product.DisplayOrder = elasticSearchProductServiceResult.DisplayOrder;
            product.Published = elasticSearchProductServiceResult.Published;
            product.Deleted = elasticSearchProductServiceResult.Deleted;
            product.CreatedOnUtc = elasticSearchProductServiceResult.CreatedOnUtc;
            product.UpdatedOnUtc = elasticSearchProductServiceResult.UpdatedOnUtc;
            product.IsMaster = elasticSearchProductServiceResult.IsMaster;
            product.UPCCode = elasticSearchProductServiceResult.UPCCode;
            product.Size = elasticSearchProductServiceResult.Size;
            product.Container = elasticSearchProductServiceResult.Container;
            product.OverridePrice = elasticSearchProductServiceResult.OverridePrice;
            product.OverrideStock = elasticSearchProductServiceResult.OverrideStock;
            product.OverrideNegativeStock = elasticSearchProductServiceResult.OverrideNegativeStock;
            product.ImageUrl = elasticSearchProductServiceResult.ImageUrl;


            //product.ProductType = (ProductType)elasticSearchProductServiceResult.ProductTypeId;
            //product.BackorderMode = (BackorderMode)elasticSearchProductServiceResult.BackorderModeId;
            //product.DownloadActivationType = (DownloadActivationType)elasticSearchProductServiceResult.DownloadActivationTypeId;
            //product.GiftCardType = (GiftCardType)elasticSearchProductServiceResult.GiftCardTypeId;
            //product.LowStockActivity = (LowStockActivity)elasticSearchProductServiceResult.LowStockActivityId;
            //product.ManageInventoryMethod = (ManageInventoryMethod)elasticSearchProductServiceResult.ManageInventoryMethodId;
            //product.RecurringCyclePeriod = (RecurringProductCyclePeriod)elasticSearchProductServiceResult.RecurringCyclePeriodId;
            //product.RentalPricePeriod = (RentalPricePeriod)elasticSearchProductServiceResult.RentalPricePeriodId;


            return product;
        }
        /// <summary>
        /// Get All Master products to sync the data in Elastic cache
        /// </summary>
        /// <returns></returns>
        public async Task<IList<ElasticSearchProductServiceResult>> GetAllProductsToSyncElasticCacheAsync(int page, long noOfProducts)
        {
            try
            {
                //prepare sp parametors
                var ppage = new DataParameter()
                {
                    Name = "Page",
                    Value = page,
                    DataType = LinqToDB.DataType.Int32
                };

                var pnoOfRecords = new DataParameter()
                {
                    Name = "NoOfRecords",
                    Value = noOfProducts,
                    DataType = LinqToDB.DataType.Int64
                };
                var result = await _dataProvider.QueryProcAsync<ElasticSearchProductServiceResult>(
                "SSP_GetAllProductsForElasticSearch", ppage, pnoOfRecords);

                return result;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        /// <summary>
        /// Get All Vendor Products
        /// </summary>
        /// <param name="page"></param>
        /// <param name="noOfProducts"></param>
        /// <returns></returns>
        public async Task<IList<ElasticSearchProductServiceResult>> GetAllVendorProductsToSyncElasticCacheAsync(string upcCode, int page, long noOfProducts)
        {
            try
            {
                //prepare sp parametors
                var pupcCode = new DataParameter()
                {
                    Name = "UPCCode",
                    Value = upcCode,
                    DataType = LinqToDB.DataType.NVarChar
                };
                var ppage = new DataParameter()
                {
                    Name = "Page",
                    Value = page,
                    DataType = LinqToDB.DataType.Int32
                };

                var pnoOfRecords = new DataParameter()
                {
                    Name = "NoOfRecords",
                    Value = noOfProducts,
                    DataType = LinqToDB.DataType.Int64
                };
                var result = await _dataProvider.QueryProcAsync<ElasticSearchProductServiceResult>(
                "SSP_GetAllVenderProductsForElasticSearch", pupcCode, ppage, pnoOfRecords);

                return result;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        #endregion 
    }
}
