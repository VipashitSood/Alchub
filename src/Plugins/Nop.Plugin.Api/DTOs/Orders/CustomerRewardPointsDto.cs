using Newtonsoft.Json;
using Nop.Plugin.Api.DTO.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Api.DTOs.Orders
{
    [JsonObject(Title = "CustomerRewardPoints")]
    public class CustomerRewardPointsDto : BaseDto
    {
        public CustomerRewardPointsDto()
        {
            RewardPoints = new List<RewardPointsHistoryModel>();
        }

        [JsonProperty("reward_points")]
        public IList<RewardPointsHistoryModel> RewardPoints { get; set; }

        [JsonProperty("rewardpointsbalance")]
        public int RewardPointsBalance { get; set; }

        [JsonProperty("rewardpointsamount")]
        public string RewardPointsAmount { get; set; }

        [JsonProperty("minimumrewardpointsbalance")]
        public int MinimumRewardPointsBalance { get; set; }

        [JsonProperty("minimumrewardpointsamount")]
        public string MinimumRewardPointsAmount { get; set; }

        [JsonProperty("pageIndex")]
        public int PageIndex { get; set; }

        [JsonProperty("pageSize")]
        public int PageSize { get; set; }

        [JsonProperty("total_records")]
        public int TotalRecords { get; set; }

        public string GetPrimaryPropertyName()
        {
            return "customrrewardspoints";
        }

        public Type GetPrimaryPropertyType()
        {
            return typeof(CustomerRewardPointsDto);
        }

        #region Nested classes

        public partial record RewardPointsHistoryModel 
        {
            [JsonProperty("user_id")]
            public int UserId { get; set; }

            [JsonProperty("points")]
            public int Points { get; set; }

            [JsonProperty("points_balance")]
            public string PointsBalance { get; set; }

            [JsonProperty("message")]
            public string Message { get; set; }

            [JsonProperty("created_on")]
            public DateTime CreatedOn { get; set; }

            [JsonProperty("end_date")]
            public DateTime? EndDate { get; set; }
        }

        #endregion
    }
}
