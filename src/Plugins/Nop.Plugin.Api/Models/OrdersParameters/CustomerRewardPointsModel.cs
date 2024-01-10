using Newtonsoft.Json;
using Nop.Plugin.Api.Infrastructure;
using Nop.Plugin.Api.Models.BaseModels;

namespace Nop.Plugin.Api.Models.OrdersParameters
{
    public class CustomerRewardPointsModel : BaseSearchModel
    {
        public CustomerRewardPointsModel()
        {
            PageIndex = (PageIndex ?? Constants.Configurations.DEFAULT_PAGE_VALUE) - 1;
            PageSize = Constants.Configurations.DEFAULT_LIMIT;
        }

        [JsonProperty("user_id")]
        public int UserId { get; set; }

    }
}
