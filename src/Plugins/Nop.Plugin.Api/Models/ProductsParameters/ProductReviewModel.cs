using System.Collections.Generic;
using Newtonsoft.Json;
using Nop.Plugin.Api.DTO.Base;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Api.Models.ProductsParameters
{
    public partial record ProductReviewOverviewModel : BaseNopModel
    {
        public int ProductId { get; set; }

        public int RatingSum { get; set; }

        public int TotalReviews { get; set; }
        
        public bool AllowCustomerReviews { get; set; }

        public bool CanAddNewReview { get; set; }

        public decimal  RatingAvg { get; set; }
    }

    public partial record ProductReviewsModel : BaseNopModel
    {
        public ProductReviewsModel()
        {
            Items = new List<ProductReviewModel>();
            AddProductReview = new AddProductReviewModel();
            ReviewTypeList = new List<ReviewTypeModel>();
            AddAdditionalProductReviewList = new List<AddProductReviewReviewTypeMappingModel>();
        }

        public int ProductId { get; set; }
       
        public string ProductName { get; set; }
        
        public string ProductSeName { get; set; }

        public IList<ProductReviewModel> Items { get; set; }

        public AddProductReviewModel AddProductReview { get; set; }

        public IList<ReviewTypeModel> ReviewTypeList { get; set; }

        public IList<AddProductReviewReviewTypeMappingModel> AddAdditionalProductReviewList { get; set; }        
    }

    public partial record ReviewTypeModel : BaseNopEntityModel
    {
        [JsonIgnore]
        public string Name { get; set; }
        [JsonIgnore]
        public string Description { get; set; }
        [JsonIgnore]
        public int DisplayOrder { get; set; }

        public bool IsRequired { get; set; }
        [JsonIgnore]
        public bool VisibleToAllCustomers { get; set; }

        public double AverageRating { get; set; }
    }

    public partial record ProductReviewModel : BaseNopEntityModel
    {
        public ProductReviewModel()
        {
            AdditionalProductReviewList = new List<ProductReviewReviewTypeMappingModel>();
        }
        [JsonIgnore]
        public int CustomerId { get; set; }
        [JsonIgnore]
        public string CustomerAvatarUrl { get; set; }
        [JsonIgnore]
        public string CustomerName { get; set; }
        [JsonIgnore]
        public bool AllowViewingProfiles { get; set; }
        [JsonIgnore]
        public string Title { get; set; }
        [JsonIgnore]
        public string ReviewText { get; set; }
        [JsonIgnore]
        public string ReplyText { get; set; }

        public int Rating { get; set; }
        [JsonIgnore]
        public string WrittenOnStr { get; set; }

        public ProductReviewHelpfulnessModel Helpfulness { get; set; }

        public IList<ProductReviewReviewTypeMappingModel> AdditionalProductReviewList { get; set; }
    }

    public partial record ProductReviewHelpfulnessModel : BaseNopModel
    {
        public int ProductReviewId { get; set; }
        [JsonIgnore]
        public int HelpfulYesTotal { get; set; }
        [JsonIgnore]
        public int HelpfulNoTotal { get; set; }
    }

    public partial record AddProductReviewModel : BaseNopModel
    {
        [JsonProperty("title")]
        public string Title { get; set; }
        
        [JsonProperty("review_text")]
        public string ReviewText { get; set; }

        [JsonProperty("rating")]
        public int Rating { get; set; }

        public bool DisplayCaptcha { get; set; }
        [JsonIgnore]
        public bool CanCurrentCustomerLeaveReview { get; set; }
        [JsonIgnore]
        public bool SuccessfullyAdded { get; set; }
        [JsonIgnore]
        public bool CanAddNewReview { get; set; }
        [JsonIgnore]
        public string Result { get; set; }
    }

    public class AddProductReviewReviewTypeMappingModel : BaseDto
    {
        public int ProductReviewId { get; set; }

        public int ReviewTypeId { get; set; }

        public int Rating { get; set; }
        [JsonIgnore]
        public string Name { get; set; }
        [JsonIgnore]
        public string Description { get; set; }
        [JsonIgnore]
        public int DisplayOrder { get; set; }
        [JsonIgnore]
        public bool IsRequired { get; set; }
    }

    public partial record ProductReviewReviewTypeMappingModel : BaseNopEntityModel
    {
        [JsonIgnore]
        public int ProductReviewId { get; set; }
        [JsonIgnore]
        public int ReviewTypeId { get; set; }
        
        public int Rating { get; set; }

        public string Name { get; set; }
        [JsonIgnore]
        public bool VisibleToAllCustomers { get; set; }
    }
}