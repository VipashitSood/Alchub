using System.Collections.Generic;
using Nop.Web.Framework.Models;

namespace Nop.Plugin.Api.Models.ProductsParameters
{
    /// <summary>
    /// Represents a specification attribute filter model
    /// </summary>
    public partial record SpecificationAttributeFilterModel
    {
        #region Properties

        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the specification attribute name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the values
        /// </summary>
        public IList<SpecificationAttributeValueFilterModel> Values { get; set; }

        #endregion

        #region Ctor

        public SpecificationAttributeFilterModel()
        {
            Values = new List<SpecificationAttributeValueFilterModel>();
        }

        #endregion
    }
}
