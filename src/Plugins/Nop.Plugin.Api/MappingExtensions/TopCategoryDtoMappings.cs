using Nop.Core.Domain.Catalog;
using Nop.Plugin.Api.AutoMapper;
using Nop.Plugin.Api.DTO.Categories;
using Nop.Plugin.Api.DTO.HomeScreen;

namespace Nop.Plugin.Api.MappingExtensions
{
    public static class TopCategoryDtoMappings
    {
        public static TopCategoryDto TopDto(this Category category)
        {
            return category.MapTo<Category, TopCategoryDto>();
        }

        public static Category ToEntity(this TopCategoryDto topCategoryDto)
        {
            return topCategoryDto.MapTo<TopCategoryDto, Category>();
        }
    }
}
