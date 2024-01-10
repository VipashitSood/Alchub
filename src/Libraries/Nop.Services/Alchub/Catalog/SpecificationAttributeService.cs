using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Data;
using Nop.Services.Security;
using Nop.Services.Stores;

namespace Nop.Services.Catalog
{
    /// <summary>
    /// Specification attribute service
    /// </summary>
    public partial class SpecificationAttributeService : ISpecificationAttributeService
    {

        #region Methods

        #region Import specification get custom value name

        public virtual async Task<SpecificationAttributeOption> GetSpecificationAttributeOptionsByCustomValueAsync(string value)
        {
            var query = from sao in _specificationAttributeOptionRepository.Table
                        orderby sao.DisplayOrder, sao.Id
                        where sao.Name.ToLower() == value.ToLower()
                        select sao;

            return query.FirstOrDefault();
        }
        #endregion
        public virtual async Task<SpecificationAttributeOption> GetSpecificationAttributeOptionsByNameAsync(string value, int specificationAttributeId)
        {
            var query = from sao in _specificationAttributeOptionRepository.Table
                        orderby sao.DisplayOrder, sao.Id
                        where sao.Name.ToLower() == value.ToLower() && sao.SpecificationAttributeId == specificationAttributeId
                        select sao;

            return query.FirstOrDefault();
        }

        public virtual async Task<SpecificationAttribute> GetSpecificationAttributeByNameAsync(string value)
        {
            var query = from sa in _specificationAttributeRepository.Table
                        orderby sa.DisplayOrder, sa.Id
                        where sa.Name.ToLower() == value.ToLower()
                        select sa;

            return query.FirstOrDefault();
        }

        /// <summary>
        /// Get product specification attrbute by product and specificationAttributeId
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="specificationAttributeId"></param>
        /// <returns></returns>
        public virtual async Task<IList<ProductSpecificationAttribute>> GetProductSpecificationAttributesByProductAndAtributeAsync(int productId, int specificationAttributeId)
        {
            var query = _productSpecificationAttributeRepository.Table;
            if (productId > 0)
                query = query.Where(psa => psa.ProductId == productId);

            query = from psa in query
                    join sao in _specificationAttributeOptionRepository.Table
                        on psa.SpecificationAttributeOptionId equals sao.Id
                    join sa in _specificationAttributeRepository.Table
                        on sao.SpecificationAttributeId equals sa.Id
                    where sa.Id == specificationAttributeId
                    select psa;

            query = query.OrderBy(psa => psa.DisplayOrder).ThenBy(psa => psa.Id);

            return await Task.FromResult(query.ToList());
        }

        /// <summary>
        /// Get Specification Attribute Id By Specification Option Id
        /// </summary>
        /// <param name="specificationOptionId"></param>
        /// <returns></returns>
        public virtual async Task<int> GetSpecificationAttributeIdBySpecificationOptionIdAsync(int specificationOptionId)
        {
            var specificationAttributeId = await _specificationAttributeOptionRepository
                .Table
                .Where(sa => sa.Id == specificationOptionId)
                .Select(sa => sa.SpecificationAttributeId)
                .FirstOrDefaultAsync();

            return specificationAttributeId;
        }

        #endregion
    }
}
