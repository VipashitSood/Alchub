using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Nop.Plugin.Api.DTO;
using Nop.Plugin.Api.DTO.Base;

namespace Nop.Plugin.Api.DTOs.Products
{
    public class ProductsSearchResultRootDto : ISerializableObject
    {
        public ProductsSearchResultRootDto()
        {
            ProductsList = new List<ProductsSearchResultDto>();
        }

        [JsonProperty("products")]
        public IList<ProductsSearchResultDto> ProductsList { get; set; }

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
            return typeof(ProductsSearchResultDto);
        }
    }

    public partial class ProductsSearchResultDto : BaseDto
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("image_url")]
        public string PictureUrl { get; set; }
    }
}
