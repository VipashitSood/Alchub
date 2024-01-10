using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Nop.Plugin.Api.DTO.Categories
{
    public class CategoriesRootObject : ISerializableObject
    {
        public CategoriesRootObject()
        {
            Categories = new List<CategoryDto>();
        }

        [JsonProperty("categories")]
        public IList<CategoryDto> Categories { get; set; }

        [JsonProperty("pageIndex")]
        public int PageIndex { get; set; }

        [JsonProperty("pageSize")]
        public int PageSize { get; set; }

        [JsonProperty("total_records")]
        public int TotalRecords { get; set; }

        public string GetPrimaryPropertyName()
        {
            return "categories";
        }

        public Type GetPrimaryPropertyType()
        {
            return typeof(CategoryDto);
        }
    }
}
