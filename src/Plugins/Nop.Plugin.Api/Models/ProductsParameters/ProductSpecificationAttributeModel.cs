using System.Collections.Generic;
using Newtonsoft.Json;
using Nop.Plugin.Api.DTO.Base;

namespace Nop.Plugin.Api.Models.ProductsParameters
{
    /// <summary>
    /// Represents a specification attribute model
    /// </summary>
    public class ProductSpecificationAttributeModel : BaseDto
    {
        #region Properties

        /// <summary>
        /// Gets or sets the name
        /// </summary>
        [JsonProperty("Name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the values
        /// </summary>
        [JsonProperty("values")]
        public IList<ProductSpecificationAttributeValueModel> Values { get; set; }

        #endregion

        #region Ctor

        public ProductSpecificationAttributeModel()
        {
            Values = new List<ProductSpecificationAttributeValueModel>();
        }

        #endregion
    }
}