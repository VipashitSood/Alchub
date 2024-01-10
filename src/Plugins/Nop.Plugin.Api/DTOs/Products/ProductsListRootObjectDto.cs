using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Nop.Plugin.Api.DTO.Products
{
    public class ProductsListRootObjectDto : ISerializableObject
    {
        public ProductsListRootObjectDto()
        {
            ProductsList = new List<ProductListDto>();
        }

        [JsonProperty("products")]
        public IList<ProductListDto> ProductsList { get; set; }

        [JsonProperty("pageIndex")]
        public int PageIndex { get; set; }

        [JsonProperty("pageSize")]
        public int PageSize { get; set; }

        [JsonProperty("total_records")]
        public int TotalRecords { get; set; }

        public string GetPrimaryPropertyName()
        {
            return "products";
        }

        public Type GetPrimaryPropertyType()
        {
            return typeof(ProductDto);
        }
    }
}
