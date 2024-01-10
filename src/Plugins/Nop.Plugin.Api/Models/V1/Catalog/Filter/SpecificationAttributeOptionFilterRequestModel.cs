using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Nop.Plugin.Api.ModelBinders;

namespace Nop.Plugin.Api.Models.V1.Catalog.Filter
{
    // JsonProperty is used only for swagger
    [ModelBinder(typeof(ParametersModelBinder<SpecificationAttributeOptionFilterRequestModel>))]
    public class SpecificationAttributeOptionFilterRequestModel : VendorFilterRequestModel
    {
        /// <summary>
        /// Get specification attribute identifiers
        /// </summary>
        [JsonProperty("specification_attribute_Id")]
        public int SpecificationAttributeId { get; set; }

        /// <summary>
        /// list of filtered specification attributes identifiers
        /// </summary>
        [JsonProperty("specification_option_Ids")]
        public List<int> SpecificationOptionIds { get; set; }
    }
}
