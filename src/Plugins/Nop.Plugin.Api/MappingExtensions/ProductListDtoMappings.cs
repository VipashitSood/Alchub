using Nop.Core.Domain.Catalog;
using Nop.Plugin.Api.AutoMapper;
using Nop.Plugin.Api.DTO.Products;

namespace Nop.Plugin.Api.MappingExtensions
{
    public static class ProductListDtoMappings
    {
        public static ProductListDto ToListDto(this Product product)
        {
            return product.MapTo<Product, ProductListDto>();
        }

        public static ProductAttributeValueDto ToListDto(this ProductAttributeValue productAttributeValue)
        {
            return productAttributeValue.MapTo<ProductAttributeValue, ProductAttributeValueDto>();
        }
    }
}
