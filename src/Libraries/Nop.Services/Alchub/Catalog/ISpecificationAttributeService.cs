using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Catalog;

namespace Nop.Services.Catalog
{
    /// <summary>
    /// Specification attribute service interface
    /// </summary>
    public partial interface ISpecificationAttributeService
    {

        #region Import specification get custom value name
        Task<SpecificationAttributeOption> GetSpecificationAttributeOptionsByCustomValueAsync(string value);
        #endregion

        Task<SpecificationAttributeOption> GetSpecificationAttributeOptionsByNameAsync(string value, int specificationAttributeId);
        Task<SpecificationAttribute> GetSpecificationAttributeByNameAsync(string value);

        /// <summary>
        /// Get product specification attrbute by product and specificationAttributeId
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="specificationAttributeId"></param>
        /// <returns></returns>
        Task<IList<ProductSpecificationAttribute>> GetProductSpecificationAttributesByProductAndAtributeAsync(int productId, int specificationAttributeId);

        /// <summary>
        /// Get Specification Attribute Id By Specification Option Id
        /// </summary>
        /// <param name="specificationOptionId"></param>
        /// <returns></returns>
        Task<int> GetSpecificationAttributeIdBySpecificationOptionIdAsync(int specificationOptionId);
    }
}
