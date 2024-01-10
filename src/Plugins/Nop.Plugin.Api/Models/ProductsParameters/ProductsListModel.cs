using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Nop.Plugin.Api.Infrastructure;
using Nop.Plugin.Api.ModelBinders;
using Nop.Plugin.Api.Models.BaseModels;
using System.Collections.Generic;

namespace Nop.Plugin.Api.Models.ProductsParameters
{
    // JsonProperty is used only for swagger
    [ModelBinder(typeof(ParametersModelBinder<ProductsListModel>))]
    public class ProductsListModel : BaseSearchModel
    {
        public ProductsListModel()
        {
            SubCategoryIds = new List<int>();
            PageIndex = (PageIndex ?? Constants.Configurations.DEFAULT_PAGE_VALUE) - 1;
            PageSize = Constants.Configurations.DEFAULT_LIMIT;
        }

        /// <summary>
        /// Customer id
        /// </summary>
        [JsonProperty("customer_Id")]
        public int CustomerId { get; set; }

        /// <summary>
        /// Product type id
        /// </summary>
        [JsonProperty("product_Type")]
        public int ProductType { get; set; }

        /// <summary>
        /// Search by category
        /// </summary>
        [JsonProperty("category_Id")]
        public int CategoryId { get; set; }

        /// <summary>
        /// Search by category
        /// </summary>
        [JsonProperty("sub_category_Ids")]
        public List<int> SubCategoryIds { get; set; }

        /// <summary>
        /// Keyword to search the product. If empty then products will return based on pageindex and pageszie
        /// </summary>
        [JsonProperty("keyword")]
        public string Keyword { get; set; }

        [JsonProperty("from")]
        public int? From { get; set; }

        [JsonProperty("to")]
        public int? To { get; set; }

        [JsonProperty("order_By")]
        public int? OrderBy { get; set; }

        [JsonProperty("specification_Option_Ids")]
        public List<int> SpecificationOptionIds { get; set; }

        [JsonProperty("manufacturer_Ids")]
        public List<int> ManufacturerIds { get; set; }

        [JsonProperty("vendor_Ids")]
        public List<int> VendorIds { get; set; }
    }
}
