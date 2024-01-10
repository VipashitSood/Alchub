using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core.Domain.Catalog;
using Nop.Plugin.Api.Infrastructure;

namespace Nop.Plugin.Api.Services
{
    public interface IProductManufacturerMappingsApiService
    {
        IList<ProductManufacturer> GetMappings(
            int? productId = null, int? manufacturerId = null, int limit = Constants.Configurations.DEFAULT_LIMIT,
            int page = Constants.Configurations.DEFAULT_PAGE_VALUE, int sinceId = Constants.Configurations.DEFAULT_SINCE_ID);

        int GetMappingsCount(int? productId = null, int? manufacturerId = null);

        Task<ProductManufacturer> GetByIdAsync(int id);
    }
}
