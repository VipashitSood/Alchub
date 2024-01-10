using System.Threading.Tasks;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;

namespace Nop.Services.Catalog
{
    /// <summary>
    /// Product attribute formatter interface
    /// </summary>
    public partial interface IProductAttributeFormatter
    {
        #region Custom product attributes formatter

        /// <summary>
        /// Formats attributes
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="attributesXml">Attributes in XML format</param>
        /// <param name="separator">Separator</param>
        /// <param name="htmlEncode">A value indicating whether to encode (HTML) values</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the attributes
        /// </returns>
        Task<string> FormatCustomAttributesAsync(string attributesXml, string separator = " ");

        #endregion
    }
}
