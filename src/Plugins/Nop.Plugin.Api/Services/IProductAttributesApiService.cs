using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core.Domain.Catalog;
using Nop.Plugin.Api.Infrastructure;

namespace Nop.Plugin.Api.Services
{
    public interface IProductAttributesApiService
    {
        IList<ProductAttribute> GetProductAttributes(
            int limit = Constants.Configurations.DEFAULT_LIMIT,
            int page = Constants.Configurations.DEFAULT_PAGE_VALUE, int sinceId = Constants.Configurations.DEFAULT_SINCE_ID);

        int GetProductAttributesCount();

        Task<ProductAttribute> GetByIdAsync(int id);
    }
}
