using System.Collections.Generic;
using Nop.Plugin.Api.DTO.Base;
using Nop.Web.Framework.Models;

namespace Nop.Plugin.Api.Models.ProductsParameters
{
    /// <summary>
    /// Represents a grouped product specification attribute model
    /// </summary>
    public class ProductSpecificationAttributeGroupModel : BaseDto
    {
        #region Properties

        /// <summary>
        /// Gets or sets the specification attribute group name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the specification attribute group attributes
        /// </summary>
        public IList<ProductSpecificationAttributeModel> Attributes { get; set; }

        #endregion

        #region Ctor

        public ProductSpecificationAttributeGroupModel()
        {
            Attributes = new List<ProductSpecificationAttributeModel>();
        }

        #endregion
    }
}