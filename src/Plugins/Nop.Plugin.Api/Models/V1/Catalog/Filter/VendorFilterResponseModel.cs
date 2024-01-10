using System.Collections.Generic;
using Newtonsoft.Json;

namespace Nop.Plugin.Api.Models.V1.Catalog.Filter
{
    /// <summary>
    /// Represent vendor filter response model
    /// </summary>
    public partial class VendorFilterResponseModel
    {
        #region Ctor

        public VendorFilterResponseModel()
        {
            Vendors = new List<VendorFilterModelV1>();
            SpecificationAttributes = new List<SpecificationAttributeFilterModelV1>();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the vendors
        /// </summary>
        [JsonProperty("vendors")]
        public IList<VendorFilterModelV1> Vendors { get; set; }

        /// <summary>
        /// Gets or sets the specification attributes
        /// </summary>
        [JsonProperty("specification_attributes")]
        public IList<SpecificationAttributeFilterModelV1> SpecificationAttributes { get; set; }

        #endregion

        #region Nested classes

        #region Manufacture

        public record VendorFilterModelV1 : BaseFilterEntityModel
        {
            /// <summary>
            /// Gets or sets the manufacturers name
            /// </summary>
            [JsonProperty("name")]
            public string Name { get; set; }

            /// <summary>
            /// Gets or sets the value indicating whether the value is selected
            /// </summary>
            [JsonProperty("selected")]
            public bool Selected { get; set; }

            /// <summary>
            /// Gets or sets number of products with manufacture after applying filters
            /// </summary>
            [JsonProperty("number_of_products")]
            public int? NumberOfProducts { get; set; }
        }

        #endregion

        #endregion
    }
}
