using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;

namespace Nop.Plugin.Api.Models.V1.Catalog.Filter
{
    /// <summary>
    /// Represent catalog base filter response model
    /// </summary>
    public partial class BaseFilterResponseModel
    {
        #region Ctor

        public BaseFilterResponseModel()
        {
            SubCategories = new List<SubCategorFilterModel>();
            //Manufacturers = new List<ManufacturerFilterModel>();
            //Vendors = new List<VendorFilterModel>();
            SpecificationAttributes = new List<SpecificationAttributeFilterModelV1>();
            PriceOptions = new List<SelectListItem>();
            AvailableSortOptions = new List<SelectListItem>();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the categories
        /// </summary>
        [JsonProperty("sub_categories")]
        public IList<SubCategorFilterModel> SubCategories { get; set; }

        ///// <summary>
        ///// Gets or sets the manufactures
        ///// </summary>
        //[JsonProperty("manufacturers")]
        //public IList<ManufacturerFilterModel> Manufacturers { get; set; }

        ///// <summary>
        ///// Gets or sets the vendors
        ///// </summary>
        //[JsonProperty("vendors")]
        //public IList<VendorFilterModel> Vendors { get; set; }

        /// <summary>
        /// Gets or sets the specification attributes
        /// </summary>
        [JsonProperty("specification_attributes")]
        public IList<SpecificationAttributeFilterModelV1> SpecificationAttributes { get; set; }

        /// <summary>
        /// Gets or sets the available price options
        /// </summary>
        [JsonProperty("price_Options")]
        public IList<SelectListItem> PriceOptions { get; set; }

        /// <summary>
        /// Gets or sets available sort options
        /// </summary>
        [JsonProperty("available_Sort_Options")]
        public IList<SelectListItem> AvailableSortOptions { get; set; }

        #endregion

        #region Nested classes

        #region Category

        public record SubCategorFilterModel : BaseFilterEntityModel
        {
            /// <summary>
            /// Gets or sets the category name
            /// </summary>
            [JsonProperty("name")]
            public string Name { get; set; }

            /// <summary>
            /// Gets or sets the value indicating whether the value is selected
            /// </summary>
            [JsonProperty("selected")]
            public bool Selected { get; set; }

            /// <summary>
            /// Gets or sets number of products in category after applying filters
            /// </summary>
            [JsonProperty("number_of_products")]
            public int? NumberOfProducts { get; set; }
        }

        #endregion

        #endregion
    }
}