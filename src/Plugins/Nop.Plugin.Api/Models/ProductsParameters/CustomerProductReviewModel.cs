using System.Collections.Generic;
using Nop.Plugin.Api.DTO.Base;
using Nop.Web.Framework.Models;

namespace Nop.Plugin.Api.Models.ProductsParameters
{
    public class CustomerProductReviewModel : BaseDto
    {
        public CustomerProductReviewModel()
        {
            AdditionalProductReviewList = new List<ProductReviewReviewTypeMappingModel>();
        }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductSeName { get; set; }
        public string Title { get; set; }
        public string ReviewText { get; set; }
        public string ReplyText { get; set; }
        public int Rating { get; set; }
        public string WrittenOnStr { get; set; }
        public string ApprovalStatus { get; set; }
        public IList<ProductReviewReviewTypeMappingModel> AdditionalProductReviewList { get; set; }
    }

    public record CustomerProductReviewsModel : BaseNopModel
    {
        public CustomerProductReviewsModel()
        {
            ProductReviews = new List<CustomerProductReviewModel>();
        }

        public IList<CustomerProductReviewModel> ProductReviews { get; set; }
   

        #region Nested class

        #endregion
    }
}