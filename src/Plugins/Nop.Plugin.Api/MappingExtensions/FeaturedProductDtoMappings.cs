using Nop.Core.Domain.Catalog;
using Nop.Plugin.Api.AutoMapper;
using Nop.Plugin.Api.DTO.Products;

namespace Nop.Plugin.Api.MappingExtensions
{
    public static class FeaturedProductDtoMappings
    {
        public static FeaturedProductDto ToFeaturedDto(this Product product)
        {
            return product.MapTo<Product, FeaturedProductDto>();
        }

        public static ProductAttributeValueDto ToFeaturedDto(this ProductAttributeValue productAttributeValue)
        {
            return productAttributeValue.MapTo<ProductAttributeValue, ProductAttributeValueDto>();
        }
    }
}
