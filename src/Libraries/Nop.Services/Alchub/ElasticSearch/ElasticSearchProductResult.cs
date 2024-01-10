using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nop.Services.Alchub.ElasticSearch
{
    public class ElasticSearchProductResult
    {
        public List<ProductElasticModel> ProductList { get; set; }
        public int TotalDocuments { get; set; }
        public List<VendorElasticModel> VendorList { get; set; }
        public List<CategoryElasticModel> CategoryList { get; set; }
        public List<CategoryElasticModel> SubCategoryList { get; set; }
        public List<ManufacturerElasticModel> ManufacturerList { get; set; }
        public List<SpecificationAttributeElasticModel> SpecificationAttributeList { get; set; }
    }
    public class ProductElasticModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string MetaKeywords { get; set; }
        public string MetaTitle { get; set; }
        public string Sku { get; set; }
        public string ManufacturerPartNumber { get; set; }
        public string Gtin { get; set; }
        public string RequiredProductIds { get; set; }
        public string AllowedQuantities { get; set; }
        public int ProductTypeId { get; set; }
        public int ParentGroupedProductId { get; set; }
        public bool VisibleIndividually { get; set; }
        public string ShortDescription { get; set; }
        public string FullDescription { get; set; }
        public string AdminComment { get; set; }
        public int ProductTemplateId { get; set; }
        public int VendorId { get; set; }
        public bool ShowOnHomepage { get; set; }
        public string MetaDescription { get; set; }
        public bool AllowCustomerReviews { get; set; }
        public int ApprovedRatingSum { get; set; }
        public int NotApprovedRatingSum { get; set; }
        public int ApprovedTotalReviews { get; set; }
        public int NotApprovedTotalReviews { get; set; }
        public bool SubjectToAcl { get; set; }
        public bool LimitedToStores { get; set; }
        public bool IsGiftCard { get; set; }
        public int GiftCardTypeId { get; set; }
        public int OverriddenGiftCardAmount { get; set; }
        public bool RequireOtherProducts { get; set; }
        public bool AutomaticallyAddRequiredProducts { get; set; }
        public bool IsDownload { get; set; }
        public int DownloadId { get; set; }
        public bool UnlimitedDownloads { get; set; }
        public int MaxNumberOfDownloads { get; set; }
        public int DownloadExpirationDays { get; set; }
        public int DownloadActivationTypeId { get; set; }
        public bool HasSampleDownload { get; set; }
        public int SampleDownloadId { get; set; }
        public bool HasUserAgreement { get; set; }
        public string UserAgreementText { get; set; }
        public bool IsRecurring { get; set; }
        public int RecurringCycleLength { get; set; }
        public int RecurringCyclePeriodId { get; set; }
        public int RecurringTotalCycles { get; set; }
        public bool IsRental { get; set; }
        public int RentalPriceLength { get; set; }
        public int RentalPricePeriodId { get; set; }
        public bool IsShipEnabled { get; set; }
        public bool IsFreeShipping { get; set; }
        public bool ShipSeparately { get; set; }
        public int AdditionalShippingCharge { get; set; }
        public int DeliveryDateId { get; set; }
        public bool IsTaxExempt { get; set; }
        public int TaxCategoryId { get; set; }
        public bool IsTelecommunicationsOrBroadcastingOrElectronicServices { get; set; }
        public int ManageInventoryMethodId { get; set; }
        public int ProductAvailabilityRangeId { get; set; }
        public bool UseMultipleWarehouses { get; set; }
        public int WarehouseId { get; set; }
        public int StockQuantity { get; set; }
        public bool DisplayStockAvailability { get; set; }
        public bool DisplayStockQuantity { get; set; }
        public int MinStockQuantity { get; set; }
        public int LowStockActivityId { get; set; }
        public int NotifyAdminForQuantityBelow { get; set; }
        public int BackorderModeId { get; set; }
        public bool AllowBackInStockSubscriptions { get; set; }
        public int OrderMinimumQuantity { get; set; }
        public int OrderMaximumQuantity { get; set; }
        public bool AllowAddingOnlyExistingAttributeCombinations { get; set; }
        public bool NotReturnable { get; set; }
        public bool DisableBuyButton { get; set; }
        public bool DisableWishlistButton { get; set; }
        public bool AvailableForPreOrder { get; set; }
        public DateTime? PreOrderAvailabilityStartDateTimeUtc { get; set; }
        public bool CallForPrice { get; set; }
        public int Price { get; set; }
        public int OldPrice { get; set; }
        public int ProductCost { get; set; }
        public bool CustomerEntersPrice { get; set; }
        public int MinimumCustomerEnteredPrice { get; set; }
        public int MaximumCustomerEnteredPrice { get; set; }
        public bool BasepriceEnabled { get; set; }
        public int BasepriceAmount { get; set; }
        public int BasepriceUnitId { get; set; }
        public int BasepriceBaseAmount { get; set; }
        public int BasepriceBaseUnitId { get; set; }
        public bool MarkAsNew { get; set; }
        public DateTime? MarkAsNewStartDateTimeUtc { get; set; }
        public DateTime? MarkAsNewEndDateTimeUtc { get; set; }
        public bool HasTierPrices { get; set; }
        public bool HasDiscountsApplied { get; set; }
        public int Weight { get; set; }
        public int Length { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public DateTime? AvailableStartDateTimeUtc { get; set; }
        public DateTime? AvailableEndDateTimeUtc { get; set; }
        public int DisplayOrder { get; set; }
        public bool Published { get; set; }
        public bool Deleted { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public DateTime UpdatedOnUtc { get; set; }
        public bool IsMaster { get; set; }
        public string UPCCode { get; set; }
        public string Size { get; set; }
        public string Container { get; set; }
        public bool OverridePrice { get; set; }
        public bool OverrideStock { get; set; }
        public bool OverrideNegativeStock { get; set; }
        public string ImageUrl { get; set; }
        public string SeName { get; set; }
        public List<VendorElasticModel> VendorList { get; set; }
        public List<CategoryElasticModel> CategoryList { get; set; }

        public List<ManufacturerElasticModel> ManufacturerList { get; set; }
        public List<SpecificationAttributeElasticModel> SpecificationAttributeList { get; set; }
    }
    public class VendorElasticModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string VenderMetaKeywords { get; set; }
        public string VenderMetaTitle { get; set; }
        public string PageSizeOptions { get; set; }
        public string Description { get; set; }
        public int? PictureId { get; set; }
        public int? AddressId { get; set; }
        public string VenderAdminComment { get; set; }
        public bool? Active { get; set; }
        public int? VenderDisplayOrder { get; set; }
        public string VenderMetaDescription { get; set; }
        public int? PageSize { get; set; }
        public bool? AllowCustomersToSelectPageSize { get; set; }
        public bool? PriceRangeFiltering { get; set; }
        public decimal PriceFrom { get; set; }
        public decimal PriceTo { get; set; }
        public bool? ManuallyPriceRange { get; set; }
        public bool? ManageDelivery { get; set; }
        public bool? PickAvailable { get; set; }
        public string GeoLocationCoordinates { get; set; }
        public string GeoFencingCoordinates { get; set; }
        public decimal? MinimumOrderAmount { get; set; }
        public decimal? OrderTax { get; set; }
        public bool? DeliveryAvailable { get; set; }
        public string PhoneNumber { get; set; }
        public string PickupAddress { get; set; }
        public int? GeoFenceShapeTypeId { get; set; }
        public decimal? RadiusDistance { get; set; }
        public long TotalDocuments { get; set; }

    }
    public class CategoryElasticModel
    {
        public int? Id { get; set; }
        public string Name { get; set; }
        public long TotalDocuments { get; set; }
        public List<SubCategoryElasticModel> SubCategoryList { get; set; }

    }
    public class SubCategoryElasticModel
    {
        public int? Id { get; set; }
        public string Name { get; set; }
        public long TotalDocuments { get; set; }
        public List<CategoryElasticModel> SubCategoryList { get; set; }

    }
    public class ManufacturerElasticModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public long TotalDocuments { get; set; }
    }
    public class SpecificationAttributeElasticModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int? DisplayOrder { get; set; }
        public List<SpecificationAttributeOptionElasticModel> SpecificationAttributeOptionList { get; set; }
    }
    public class SpecificationAttributeOptionElasticModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public long TotalDocuments { get; set; }
    }
    public class ElasticSearchProductServiceResult
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string MetaKeywords { get; set; }
        public string MetaTitle { get; set; }
        public string Sku { get; set; }
        public string ManufacturerPartNumber { get; set; }
        public string Gtin { get; set; }
        public string RequiredProductIds { get; set; }
        public string AllowedQuantities { get; set; }
        public int ProductTypeId { get; set; }
        public int ParentGroupedProductId { get; set; }
        public bool VisibleIndividually { get; set; }
        public string ShortDescription { get; set; }
        public string FullDescription { get; set; }
        public string AdminComment { get; set; }
        public int ProductTemplateId { get; set; }
        public int VendorId { get; set; }
        public bool ShowOnHomepage { get; set; }
        public string MetaDescription { get; set; }
        public bool AllowCustomerReviews { get; set; }
        public int ApprovedRatingSum { get; set; }
        public int NotApprovedRatingSum { get; set; }
        public int ApprovedTotalReviews { get; set; }
        public int NotApprovedTotalReviews { get; set; }
        public bool SubjectToAcl { get; set; }
        public bool LimitedToStores { get; set; }
        public bool IsGiftCard { get; set; }
        public int GiftCardTypeId { get; set; }
        public int OverriddenGiftCardAmount { get; set; }
        public bool RequireOtherProducts { get; set; }
        public bool AutomaticallyAddRequiredProducts { get; set; }
        public bool IsDownload { get; set; }
        public int DownloadId { get; set; }
        public bool UnlimitedDownloads { get; set; }
        public int MaxNumberOfDownloads { get; set; }
        public int DownloadExpirationDays { get; set; }
        public int DownloadActivationTypeId { get; set; }
        public bool HasSampleDownload { get; set; }
        public int SampleDownloadId { get; set; }
        public bool HasUserAgreement { get; set; }
        public string UserAgreementText { get; set; }
        public bool IsRecurring { get; set; }
        public int RecurringCycleLength { get; set; }
        public int RecurringCyclePeriodId { get; set; }
        public int RecurringTotalCycles { get; set; }
        public bool IsRental { get; set; }
        public int RentalPriceLength { get; set; }
        public int RentalPricePeriodId { get; set; }
        public bool IsShipEnabled { get; set; }
        public bool IsFreeShipping { get; set; }
        public bool ShipSeparately { get; set; }
        public int AdditionalShippingCharge { get; set; }
        public int DeliveryDateId { get; set; }
        public bool IsTaxExempt { get; set; }
        public int TaxCategoryId { get; set; }
        public bool IsTelecommunicationsOrBroadcastingOrElectronicServices { get; set; }
        public int ManageInventoryMethodId { get; set; }
        public int ProductAvailabilityRangeId { get; set; }
        public bool UseMultipleWarehouses { get; set; }
        public int WarehouseId { get; set; }
        public int StockQuantity { get; set; }
        public bool DisplayStockAvailability { get; set; }
        public bool DisplayStockQuantity { get; set; }
        public int MinStockQuantity { get; set; }
        public int LowStockActivityId { get; set; }
        public int NotifyAdminForQuantityBelow { get; set; }
        public int BackorderModeId { get; set; }
        public bool AllowBackInStockSubscriptions { get; set; }
        public int OrderMinimumQuantity { get; set; }
        public int OrderMaximumQuantity { get; set; }
        public bool AllowAddingOnlyExistingAttributeCombinations { get; set; }
        public bool NotReturnable { get; set; }
        public bool DisableBuyButton { get; set; }
        public bool DisableWishlistButton { get; set; }
        public bool AvailableForPreOrder { get; set; }
        public DateTime? PreOrderAvailabilityStartDateTimeUtc { get; set; }
        public bool CallForPrice { get; set; }
        public int Price { get; set; }
        public int OldPrice { get; set; }
        public int ProductCost { get; set; }
        public bool CustomerEntersPrice { get; set; }
        public int MinimumCustomerEnteredPrice { get; set; }
        public int MaximumCustomerEnteredPrice { get; set; }
        public bool BasepriceEnabled { get; set; }
        public int BasepriceAmount { get; set; }
        public int BasepriceUnitId { get; set; }
        public int BasepriceBaseAmount { get; set; }
        public int BasepriceBaseUnitId { get; set; }
        public bool MarkAsNew { get; set; }
        public DateTime? MarkAsNewStartDateTimeUtc { get; set; }
        public DateTime? MarkAsNewEndDateTimeUtc { get; set; }
        public bool HasTierPrices { get; set; }
        public bool HasDiscountsApplied { get; set; }
        public int Weight { get; set; }
        public int Length { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public DateTime? AvailableStartDateTimeUtc { get; set; }
        public DateTime? AvailableEndDateTimeUtc { get; set; }
        public int DisplayOrder { get; set; }
        public bool Published { get; set; }
        public bool Deleted { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public DateTime UpdatedOnUtc { get; set; }
        public bool IsMaster { get; set; }
        public string UPCCode { get; set; }
        public string Size { get; set; }
        public string Container { get; set; }
        public bool OverridePrice { get; set; }
        public bool OverrideStock { get; set; }
        public bool OverrideNegativeStock { get; set; }
        public string ImageUrl { get; set; }
        public int? VenderId { get; set; }
        public string VenderName { get; set; }
        public string Email { get; set; }
        public string VenderMetaKeywords { get; set; }
        public string VenderMetaTitle { get; set; }
        public string PageSizeOptions { get; set; }
        public string Description { get; set; }
        public int? PictureId { get; set; }
        public int? AddressId { get; set; }
        public string VenderAdminComment { get; set; }
        public bool? Active { get; set; }
        public int? VenderDisplayOrder { get; set; }
        public string VenderMetaDescription { get; set; }
        public int? PageSize { get; set; }
        public bool? AllowCustomersToSelectPageSize { get; set; }
        public bool? PriceRangeFiltering { get; set; }
        public decimal PriceFrom { get; set; }
        public decimal PriceTo { get; set; }
        public bool? ManuallyPriceRange { get; set; }
        public bool? ManageDelivery { get; set; }
        public bool? PickAvailable { get; set; }
        public string GeoLocationCoordinates { get; set; }
        public string GeoFencingCoordinates { get; set; }
        public decimal? MinimumOrderAmount { get; set; }
        public decimal? OrderTax { get; set; }
        public bool? DeliveryAvailable { get; set; }
        public string PhoneNumber { get; set; }
        public string PickupAddress { get; set; }
        public int? GeoFenceShapeTypeId { get; set; }
        public decimal? RadiusDistance { get; set; }
        public int? ManufacturerId { get; set; }
        public string ManufacturerName { get; set; }
        public int? CategoryId { get; set; }
        public string CategoryName { get; set; }
        //public int? SubCategoryId { get; set; }
        //public string SubCategoryName { get; set; }
        public int? SpecificationAttributeId { get; set; }
        public string SpecificationAttributeName { get; set; }
        public int? SpecificationAttributeDisplayOrder { get; set; }
        public int? SpecificationAttributeOptionsId { get; set; }
        public string SpecificationAttributeOptionsName { get; set; }
        public long TotalCount { get; set; }
        public string ProductSlug { get; set; }
        public string CategorySlug { get; set; }
        public int? ParentCategoryId { get; set; }
        //public int? SubCategoryDisplayOrder { get; set; }
        //public int? CategoryTemplateId { get; set; }
    }
}
