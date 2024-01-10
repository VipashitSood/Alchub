using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Web.Areas.Admin.Models.Catalog
{
    /// <summary>
    /// Represents a product search model
    /// </summary>
    public partial record ProductSearchModel
    {

        public ProductSearchModel()
        {
            AvailableCategories = new List<SelectListItem>();
            AvailableManufacturers = new List<SelectListItem>();
            AvailableStores = new List<SelectListItem>();
            AvailableWarehouses = new List<SelectListItem>();
            AvailableVendors = new List<SelectListItem>();
            AvailableProductTypes = new List<SelectListItem>();
            AvailablePublishedOptions = new List<SelectListItem>();
            AddProductVendors = new AddProductVendor();
            AvailableSize = new List<SelectListItem>();
        }

        #region Properties

        [NopResourceDisplayName("Admin.Catalog.Products.List.SearchUPCCode")]
        public string SearchUPCCode { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.List.SearchSize")]
        public string SearchSize { get; set; }
        public AddProductVendor AddProductVendors { get; set; }
        public IList<SelectListItem> AvailableSize { get; set; }

        public bool AllowBulkImportExport { get; set; }
        public bool AllowVendorProductActions { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.List.OverridePrice")]
        public bool OverridePrice { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.List.OverrideStock")]
        public bool OverrideStock { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.List.OverrideNegativeStock")]
        public bool OverrideNegativeStock { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.List.SelectedPublished")]
        public bool SelectedPublished { get; set; }
        #endregion

        #region Add Vendor Product
        public record AddProductVendor : BaseNopModel
        {
            [NopResourceDisplayName("Admin.Catalog.Products.List.Name")]
            public string Name { get; set; }

            [NopResourceDisplayName("Admin.Catalog.Products.List.UPCCode")]
            public string UPCCode { get; set; }

            [NopResourceDisplayName("Admin.Catalog.Products.List.Price")]
            public decimal Price { get; set; }

            [NopResourceDisplayName("Admin.Catalog.Products.List.Stock")]
            public int Stock { get; set; }

            [NopResourceDisplayName("Admin.Catalog.Products.List.Category")]
            public string Category { get; set; }

        }
        #endregion
    }
}