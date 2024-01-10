using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using Nop.Plugin.Api.DTOs.Categories;

namespace Nop.Plugin.Api.Models.ProductsParameters
{
    /// <summary>
    /// Represents a catalog products model
    /// </summary>
    public partial record CatalogProductsModel
    {
        #region Properties

        ///// <summary>
        ///// Gets or sets the price range filter model
        ///// </summary>
        //[JsonProperty("price_Range_Filter")]
        //public PriceRangeFilterModel PriceRangeFilter { get; set; }

        /// <summary>
        /// Gets or sets the specification filter model
        /// </summary>
        [JsonProperty("specification_Filter")]
        public SpecificationFilterModel SpecificationFilter { get; set; }

        /// <summary>
        /// Gets or sets the manufacturer filter model
        /// </summary>
        [JsonProperty("manufacturer_Filter")]
        public ManufacturerFilterModel ManufacturerFilter { get; set; }

        /// <summary>
        /// Gets or sets available sort options
        /// </summary>
        [JsonProperty("available_Sort_Options")]
        public IList<SelectListItem> AvailableSortOptions { get; set; }


        ///// <summary>
        ///// Gets or sets available view mode options
        ///// </summary>
        //[JsonProperty("available_View_Modes")]
        //public IList<SelectListItem> AvailableViewModes { get; set; }

        ///// <summary>
        ///// Gets or sets available page size options
        ///// </summary>
        //[JsonProperty("page_Size_Options")]
        //public IList<SelectListItem> PageSizeOptions { get; set; }

        [JsonProperty("categories")]
        public IList<FilterCategoryHierarchyModel> FilterCategoriesObject { get; set; }

        [JsonProperty("vendors")]
        public IList<VendorFilterModel> Vendors { get; set; }

        //[JsonProperty("vendors")]
        //public IList<SelectListItem> Vendors { get; set; }

        /// <summary>
        /// Gets or sets available price options
        /// </summary>
        [JsonProperty("price_Options")]
        public IList<SelectListItem> PriceOptions { get; set; }

        #endregion

        #region Ctor

        public CatalogProductsModel()
        {
            //PriceRangeFilter = new PriceRangeFilterModel();
            SpecificationFilter = new SpecificationFilterModel();
            ManufacturerFilter = new ManufacturerFilterModel();
            AvailableSortOptions = new List<SelectListItem>();
            //AvailableViewModes = new List<SelectListItem>();
            //PageSizeOptions = new List<SelectListItem>();
            FilterCategoriesObject = new List<FilterCategoryHierarchyModel>();
            Vendors = new List<VendorFilterModel>();
            PriceOptions = new List<SelectListItem>();
        }

        #endregion
    }
}