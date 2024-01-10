using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Nop.Plugin.Api.Infrastructure;
using Nop.Plugin.Api.ModelBinders;
using Nop.Plugin.Api.Models.BaseModels;

namespace Nop.Plugin.Api.Models.ProductsParameters
{
    // JsonProperty is used only for swagger
    [ModelBinder(typeof(ParametersModelBinder<ProductsParametersModel>))]
    public class ProductsParametersModel : BaseSearchModel
    {
        /// <summary>
        ///     list of product ids to include in response
        /// </summary>
        [JsonProperty("ids")]
        public List<int> Ids { get; set; }
    }
}
