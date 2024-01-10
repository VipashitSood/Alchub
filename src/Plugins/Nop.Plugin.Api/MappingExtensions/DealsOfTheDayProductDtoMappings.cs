using Nop.Core.Domain.Catalog;
using Nop.Plugin.Api.AutoMapper;
using Nop.Plugin.Api.DTO.Products;

namespace Nop.Plugin.Api.MappingExtensions
{
    public static class DealsOfTheDayProductDtoMappings
    {
        public static DealsOfTheDayDto ToDealsOfTheDayDto(this Product product)
        {
            return product.MapTo<Product, DealsOfTheDayDto>();
        }

        public static ProductAttributeValueDto ToFeaturedDto(this ProductAttributeValue productAttributeValue)
        {
            return productAttributeValue.MapTo<ProductAttributeValue, ProductAttributeValueDto>();
        }
    }
}
