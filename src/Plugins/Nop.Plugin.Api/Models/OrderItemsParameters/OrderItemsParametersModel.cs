using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Nop.Plugin.Api.Infrastructure;
using Nop.Plugin.Api.ModelBinders;

namespace Nop.Plugin.Api.Models.OrderItemsParameters
{
    [ModelBinder(typeof(ParametersModelBinder<OrderItemsParametersModel>))]
    public class OrderItemsParametersModel
    {
        public OrderItemsParametersModel()
        {
            Limit = Constants.Configurations.DEFAULT_LIMIT;
            Page = Constants.Configurations.DEFAULT_PAGE_VALUE;
            SinceId = 0;
            Fields = string.Empty;
        }

        [JsonProperty("limit")]
        public int Limit { get; set; }

        [JsonProperty("page")]
        public int Page { get; set; }

        [JsonProperty("since_id")]
        public int SinceId { get; set; }

        [JsonProperty("fields")]
        public string Fields { get; set; }
    }
}
