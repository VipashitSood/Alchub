using System.Collections.Generic;
using Newtonsoft.Json;
using Nop.Plugin.Api.DTO.Base;

namespace Nop.Plugin.Api.Models.ProductsParameters
{
    /// <summary>
    /// Represents a product specification model
    /// </summary>
    public class ProductSpecificationModel : BaseDto
    {
        #region Properties

        /// <summary>
        /// Gets or sets the grouped specification attribute models
        /// </summary>
        [JsonProperty("groups")]
        public IList<ProductSpecificationAttributeGroupModel> Groups { get; set; }

        #endregion

        #region Ctor

        public ProductSpecificationModel()
        {
            Groups = new List<ProductSpecificationAttributeGroupModel>();
        }

        #endregion
    }
}