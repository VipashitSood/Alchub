using System.Collections.Generic;
using Nop.Core.Domain.Catalog;
using Nop.Plugin.Api.Infrastructure;

namespace Nop.Plugin.Api.Services
{
    public interface ISpecificationAttributeApiService
    {
        IList<ProductSpecificationAttribute> GetProductSpecificationAttributes(
            int? productId = null, int? specificationAttributeOptionId = null, bool? allowFiltering = null, bool? showOnProductPage = null,
            int limit = Constants.Configurations.DEFAULT_LIMIT, int page = Constants.Configurations.DEFAULT_PAGE_VALUE,
            int sinceId = Constants.Configurations.DEFAULT_SINCE_ID);

        IList<SpecificationAttribute> GetSpecificationAttributes(
            int limit = Constants.Configurations.DEFAULT_LIMIT, int page = Constants.Configurations.DEFAULT_PAGE_VALUE,
            int sinceId = Constants.Configurations.DEFAULT_SINCE_ID);
    }
}
