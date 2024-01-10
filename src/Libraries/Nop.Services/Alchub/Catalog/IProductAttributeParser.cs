using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Nop.Core.Domain.Catalog;

namespace Nop.Services.Catalog
{
    /// <summary>
    /// Product attribute parser interface
    /// </summary>
    public partial interface IProductAttributeParser
    {
        #region Custom product attributes

        /// <summary>
        /// Get product attributes from the passed form
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="form">Form values</param>
        /// <param name="errors">Errors</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the attributes in XML format
        /// </returns>
        Task<string> ParseCustomProductAttributesAsync(Product masterProduct, int groupedProductId);

        #endregion
    }
}
