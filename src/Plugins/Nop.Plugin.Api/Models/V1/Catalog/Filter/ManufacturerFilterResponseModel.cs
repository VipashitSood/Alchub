using System.Collections.Generic;
using Newtonsoft.Json;
using static Nop.Plugin.Api.Models.V1.Catalog.Filter.BaseFilterResponseModel;

namespace Nop.Plugin.Api.Models.V1.Catalog.Filter
{
    /// <summary>
    /// Represent manufacturers filter response model
    /// </summary>
    public partial class ManufacturerFilterResponseModel
    {
        #region Ctor

        public ManufacturerFilterResponseModel()
        {
            Manufacturers = new List<ManufacturerFilterModelV1>();
            SpecificationAttributes = new List<SpecificationAttributeFilterModelV1>();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the manufactures
        /// </summary>
        [JsonProperty("manufacturers")]
        public IList<ManufacturerFilterModelV1> Manufacturers { get; set; }

        /// <summary>
        /// Gets or sets the specification attributes
        /// </summary>
        [JsonProperty("specification_attributes")]
        public IList<SpecificationAttributeFilterModelV1> SpecificationAttributes { get; set; }

        #endregion

        #region Nested classes

        #region Manufacture

        public record ManufacturerFilterModelV1 : BaseFilterEntityModel
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
