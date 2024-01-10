using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Nop.Plugin.Api.ModelBinders;

namespace Nop.Plugin.Api.Models.V1.Catalog.Filter
{
    // JsonProperty is used only for swagger
    [ModelBinder(typeof(ParametersModelBinder<BaseFilterRequestModel>))]
    public class BaseFilterRequestModel
    {
        /// <summary>
        /// root category identifier
        /// </summary>
        [JsonProperty("root_category_id")]
        public int RootCategoryId { get; set; }

        /// <summary>
        /// list of filtered sub categories identifiers
        /// </summary>
        [JsonProperty("sub_category_Ids")]
        public List<int> SubCategoryIds { get; set; }

        /// <summary>
        /// order by - to set selected option
        /// </summary>
        [JsonProperty("orderBy")]
        public int? OrderBy { get; set; }
    }
}
