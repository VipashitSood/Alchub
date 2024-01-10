using System.Collections.Generic;
using Newtonsoft.Json;

namespace Nop.Plugin.Api.Models.V1.Catalog.Filter
{
    /// <summary>
    /// Represent specification attribute options filter response model
    /// </summary>
    public partial class SpecificationAttributeOptionFilterResponseModel
    {
        #region Ctor

        public SpecificationAttributeOptionFilterResponseModel()
        {
            SpecOptions = new List<SpecificationAttributeValueFilterModelV1>();
            SpecificationAttributes = new List<SpecificationAttributeFilterModelV1>();
        }

        #endregion

        /// <summary>
        /// Gets or sets the values
        /// </summary>
        [JsonProperty("specification_attribute_options")]
        public IList<SpecificationAttributeValueFilterModelV1> SpecOptions { get; set; }

        /// <summary>
        /// Gets or sets the specification attributes
        /// </summary>
        [JsonProperty("specification_attributes")]
        public IList<SpecificationAttributeFilterModelV1> SpecificationAttributes { get; set; }

        #region Nested classes

        public record SpecificationAttributeValueFilterModelV1 : BaseFilterEntityModel
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

        #endregion
    }
}
