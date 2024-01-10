using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Nop.Plugin.Api.ModelBinders;

namespace Nop.Plugin.Api.Models.V1.Catalog.Filter
{
    // JsonProperty is used only for swagger
    [ModelBinder(typeof(ParametersModelBinder<VendorFilterRequestModel>))]
    public class VendorFilterRequestModel : ManufacturerFilterRequestModel
    {
        /// <summary>
        /// list of filtered vendor identifiers
        /// </summary>
        [JsonProperty("vendor_Ids")]
        public List<int> VendorIds { get; set; }
    }
}
