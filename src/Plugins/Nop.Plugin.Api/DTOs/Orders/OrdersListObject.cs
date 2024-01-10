using Newtonsoft.Json;

namespace Nop.Plugin.Api.DTO.Orders
{
    public class OrdersListObject : OrdersRootObject
    {
        [JsonProperty("pageIndex")]
        public int PageIndex { get; set; }

        [JsonProperty("pageSize")]
        public int PageSize { get; set; }

        [JsonProperty("total_records")]
        public int TotalRecords { get; set; }
    }
}
