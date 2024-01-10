using Newtonsoft.Json;
using Nop.Plugin.Api.DTO.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Api.DTOs.Products
{
    [JsonObject(Title = "AllReview")]
    public class AllReviewDto : BaseDto
    {
        public AllReviewDto()
        {
            ReviewList = new List<ReviewModel>();
        }

        [JsonProperty("totalrating")]
        public decimal TotalRating { get; set; }

        [JsonProperty("totalreview")]
        public int TotalReview { get; set; }

        [JsonProperty("ratingcount")]
        public int RatingCount { get; set; }

        [JsonProperty("fivestar")]
        public int FiveStar { get; set; }

        [JsonProperty("fourstar")]
        public int FourStar { get; set; }

        [JsonProperty("threestar")]
        public int ThreeStar { get; set; }

        [JsonProperty("twostar")]
        public int TwoStar { get; set; }

        [JsonProperty("onestar")]
        public int OneStar { get; set; }

        public IList<ReviewModel> ReviewList { get; set; }

        [JsonProperty("rating_Avg")]
        public decimal RatingAvg { get; set; }
    }
    
    #region Nested Classes

    public partial record ReviewModel
    {
      
        public int ReviewId { get; set; }
        public string ReviewByName { get; set; }
        public string ReviewText { get; set; }
        public string ReviewTitle { get; set; }
        public decimal Rating { get; set; }
        public string ReviewDate { get; set; }
        public int LikeCount { get; set; }
        public int DislikeCount { get; set; }
    }

    #endregion
}
