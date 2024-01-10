using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Nop.Plugin.Api.ModelBinders;

namespace Nop.Plugin.Api.Models.V1.Catalog.Filter
{
    // JsonProperty is used only for swagger
    [ModelBinder(typeof(ParametersModelBinder<CatalogFilterRequestModel>))]
    public class CatalogFilterRequestModel
    {
        ///// <summary>
        ///// Keyword to search the product.
        ///// </summary>
        //[JsonProperty("keyword")]
        //public string Keyword { get; set; }

        ///// <summary>
        ///// price from 
        ///// </summary>
        //[JsonProperty("price_from")]
        //public int? PriceFrom { get; set; }

        ///// <summary>
        ///// price to
        ///// </summary>
        //[JsonProperty("price_to")]
        //public int? PriceTo { get; set; }

        /// <summary>
        /// category identifier (which ever category clicked)
        /// </summary>
        [JsonProperty("category_id")]
        public int CategoryId { get; set; }

        /// <summary>
        /// list of filtered manufacturer identifiers
        /// </summary>
        [JsonProperty("manufacturer_Ids")]
        public List<int> ManufacturerIds { get; set; }

        /// <summary>
        /// list of filtered vendor identifiers
        /// </summary>
        [JsonProperty("vendor_Ids")]
        public List<int> VendorIds { get; set; }

        /// <summary>
        /// list of filtered specification attributes identifiers
        /// </summary>
        [JsonProperty("specification_option_Ids")]
        public List<int> SpecificationOptionIds { get; set; }

        ///// <summary>
        ///// order by: optional param
        ///// </summary>
        //[JsonProperty("order_By")]
        //public int? OrderBy { get; set; }
    }
}
