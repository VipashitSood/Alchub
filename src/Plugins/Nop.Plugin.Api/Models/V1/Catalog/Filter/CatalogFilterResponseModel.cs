using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;

namespace Nop.Plugin.Api.Models.V1.Catalog.Filter
{
    /// <summary>
    /// Represent catalog filter model - new
    /// </summary>
    public partial class CatalogFilterResponseModel
    {
        #region Ctor

        public CatalogFilterResponseModel()
        {
            Categories = new List<CategorFilterModel>();
            Manufacturers = new List<ManufacturerFilterModel>();
            Vendors = new List<VendorFilterModel>();
            SpecificationAttributes = new List<SpecificationAttributeFilterModel>();
            PriceOptions = new List<SelectListItem>();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the categories
        /// </summary>
        [JsonProperty("categories")]
        public IList<CategorFilterModel> Categories { get; set; }

        /// <summary>
        /// Gets or sets the manufactures
        /// </summary>
        [JsonProperty("manufacturers")]
        public IList<ManufacturerFilterModel> Manufacturers { get; set; }

        /// <summary>
        /// Gets or sets the vendors
        /// </summary>
        [JsonProperty("vendors")]
        public IList<VendorFilterModel> Vendors { get; set; }

        /// <summary>
        /// Gets or sets the specification attributes
        /// </summary>
        [JsonProperty("specification_attributes")]
        public IList<SpecificationAttributeFilterModel> SpecificationAttributes { get; set; }

        /// <summary>
        /// Gets or sets the available price options
        /// </summary>
        [JsonProperty("price_Options")]
        public IList<SelectListItem> PriceOptions { get; set; }

        #endregion

        #region Nested classes

        #region Category

        public record CategorFilterModel : BaseFilterEntityModel
        {
            public CategorFilterModel()
            {
                SubCategories = new List<CategorFilterModel>();
            }

            /// <summary>
            /// Gets or sets the category name
            /// </summary>
            [JsonProperty("name")]
            public string Name { get; set; }

            /// <summary>
            /// Gets or sets number of products in category after applying filters
            /// </summary>
            [JsonProperty("number_of_products")]
            public int? NumberOfProducts { get; set; }

            /// <summary>
            /// Gets or sets the sub categories
            /// </summary>
            [JsonProperty("sub_categories")]
            public List<CategorFilterModel> SubCategories { get; set; }
        }

        #endregion

        #region Manufacture

        public record ManufacturerFilterModel : BaseFilterEntityModel
        {
            /// <summary>
            /// Gets or sets the manufacturers name
            /// </summary>
            [JsonProperty("name")]
            public string Name { get; set; }

            /// <summary>
            /// Gets or sets number of products with manufacture after applying filters
            /// </summary>
            [JsonProperty("number_of_products")]
            public int? NumberOfProducts { get; set; }
        }

        #endregion

        #region Vendor

        public record VendorFilterModel : BaseFilterEntityModel
        {
            /// <summary>
            /// Gets or sets the vendors name
            /// </summary>
            [JsonProperty("name")]
            public string Name { get; set; }

            /// <summary>
            /// Gets or sets number of products with vendors after applying filters
            /// </summary>
            [JsonProperty("number_of_products")]
            public int? NumberOfProducts { get; set; }
        }

        #endregion

        #region Specification attribute

        public record SpecificationAttributeFilterModel : BaseFilterEntityModel
        {
            public SpecificationAttributeFilterModel()
            {
                Values = new List<SpecificationAttributeValueFilterModel>();
            }

            /// <summary>
            /// Gets or sets the specification attribute name
            /// </summary>
            [JsonProperty("name")]
            public string Name { get; set; }

            /// <summary>
            /// Gets or sets the values
            /// </summary>
            [JsonProperty("specification_attribute_options")]
            public IList<SpecificationAttributeValueFilterModel> Values { get; set; }

            public record SpecificationAttributeValueFilterModel : BaseFilterEntityModel
            {
                /// <summary>
                /// Gets or sets the specification attribute option name
                /// </summary>
                [JsonProperty("name")]
                public string Name { get; set; }

                /// <summary>
                /// Gets or sets the value indicating whether the value is selected
                /// </summary>
                [JsonProperty("selected")]
                public bool Selected { get; set; }

                /// <summary>
                /// Gets or sets number of products with option after applying filters
                /// </summary>
                [JsonProperty("number_of_products")]
                public int? NumberOfProducts { get; set; }
            }
        }

        #region Price

        #endregion

        #endregion

        #endregion
    }
}
