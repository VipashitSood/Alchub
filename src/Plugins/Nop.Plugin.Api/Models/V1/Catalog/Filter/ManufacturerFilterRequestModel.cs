using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Nop.Plugin.Api.ModelBinders;

namespace Nop.Plugin.Api.Models.V1.Catalog.Filter
{
    // JsonProperty is used only for swagger
    [ModelBinder(typeof(ParametersModelBinder<ManufacturerFilterRequestModel>))]
    public class ManufacturerFilterRequestModel : BaseFilterRequestModel
    {
        /// <summary>
        /// list of filtered manufacturer identifiers
        /// </summary>
        [JsonProperty("manufacturer_Ids")]
        public List<int> ManufacturerIds { get; set; }
    }
}
