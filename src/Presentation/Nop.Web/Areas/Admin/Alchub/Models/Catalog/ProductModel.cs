using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Areas.Admin.Models.Settings;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Web.Areas.Admin.Models.Catalog
{
    /// <summary>
    /// Represents a extended product model
    /// </summary>
    public partial record ProductModel
    {
        public ProductModel()
        {
            ProductPictureModels = new List<ProductPictureModel>();
            Locales = new List<ProductLocalizedModel>();
            CopyProductModel = new CopyProductModel();
            AddPictureModel = new ProductPictureModel();
            ProductWarehouseInventoryModels = new List<ProductWarehouseInventoryModel>();
            ProductEditorSettingsModel = new ProductEditorSettingsModel();
            StockQuantityHistory = new StockQuantityHistoryModel();

            AvailableBasepriceUnits = new List<SelectListItem>();
            AvailableBasepriceBaseUnits = new List<SelectListItem>();
            AvailableProductTemplates = new List<SelectListItem>();
            AvailableTaxCategories = new List<SelectListItem>();
            AvailableDeliveryDates = new List<SelectListItem>();
            AvailableProductAvailabilityRanges = new List<SelectListItem>();
            AvailableWarehouses = new List<SelectListItem>();
            ProductsTypesSupportedByProductTemplates = new Dictionary<int, IList<SelectListItem>>();

            AvailableVendors = new List<SelectListItem>();

            SelectedStoreIds = new List<int>();
            AvailableStores = new List<SelectListItem>();

            SelectedManufacturerIds = new List<int>();
            AvailableManufacturers = new List<SelectListItem>();

            SelectedCategoryIds = new List<int>();
            AvailableCategories = new List<SelectListItem>();

            SelectedCustomerRoleIds = new List<int>();
            AvailableCustomerRoles = new List<SelectListItem>();

            SelectedDiscountIds = new List<int>();
            AvailableDiscounts = new List<SelectListItem>();

            RelatedProductSearchModel = new RelatedProductSearchModel();
            CrossSellProductSearchModel = new CrossSellProductSearchModel();
            AssociatedProductSearchModel = new AssociatedProductSearchModel();
            ProductPictureSearchModel = new ProductPictureSearchModel();
            ProductSpecificationAttributeSearchModel = new ProductSpecificationAttributeSearchModel();
            ProductOrderSearchModel = new ProductOrderSearchModel();
            TierPriceSearchModel = new TierPriceSearchModel();
            StockQuantityHistorySearchModel = new StockQuantityHistorySearchModel();
            ProductAttributeMappingSearchModel = new ProductAttributeMappingSearchModel();
            ProductAttributeCombinationSearchModel = new ProductAttributeCombinationSearchModel();
            AvailableManageInventory = new List<SelectListItem>();
            AvailableSize = new List<SelectListItem>();
        }

        #region Properties

        [NopResourceDisplayName("Admin.Catalog.Products.Fields.UPCCode")]
        public string UPCCode { get; set; }

        [NopResourceDisplayName("Alchub.Admin.Catalog.Products.Fields.Szie")]
        public string Size { get; set; }

        [NopResourceDisplayName("Alchub.Admin.Catalog.Products.Fields.Container")]
        public string Container { get; set; }

        public IList<SelectListItem> AvailableManageInventory { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.List.SearchCategory")]
        public string Category { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.Fields.OverridePrice")]
        public bool OverridePrice { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.Fields.OverrideStock")]
        public bool OverrideStock { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.Fields.OverrideNegativeStock")]
        public bool OverrideNegativeStock { get; set; }

        public IList<SelectListItem> AvailableSize { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.Fields.ImageUrl")]
        public string ImageUrl { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.Fields.IsAlcohol")]
        public bool IsAlcohol { get; set; }
        #endregion
    }
}