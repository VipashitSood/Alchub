using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Nop.Core.Domain.Catalog;
using Nop.Plugin.Api.Attributes;
using Nop.Plugin.Api.DTO.Base;
using Nop.Plugin.Api.DTO.Images;
using Nop.Plugin.Api.DTO.Languages;
using Nop.Plugin.Api.DTO.SpecificationAttributes;

namespace Nop.Plugin.Api.DTO.Products
{
    [JsonObject(Title = "productlist")]
    public class ProductListDto : BaseDto
    {
        private int? _productTypeId;

        /// <summary>
        ///     Gets or sets the name
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        ///     Gets or sets the price
        /// </summary>
        [JsonProperty("price")]
        public string Price { get; set; }

        //[JsonProperty("price_value")]
        //public decimal? PriceValue { get; set; }

        ///// <summary>
        /////     Gets or sets the old price
        ///// </summary>
        ///// [JsonProperty("old_price")]
        //public string OldPrice { get; set; }

        //[JsonProperty("old_price_value")]
        //public decimal? OldPriceValue { get; set; }

        [JsonIgnore]
        [ImageCollectionValidation]
        [JsonProperty("images")]
        public List<ImageMappingDto> Images { get; set; }

        /// <summary>
        ///  Gets or sets the image_url
        /// </summary>
        [JsonProperty("image_url")]
        public string PictureUrl { get; set; }

        /// <summary>
        ///  Gets or sets the fullsize_image_url
        /// </summary>
        [JsonProperty("fullsize_image_url")]
        public string FullSizePictureUrl { get; set; }

        /// <summary>
        ///     Gets or sets the rating sum (approved reviews)
        /// </summary>
        [JsonProperty("approved_rating_sum")]
        public int? ApprovedRatingSum { get; set; }

        /// <summary>
        ///     Gets or sets the total rating votes (approved reviews)
        /// </summary>
        [JsonProperty("approved_total_reviews")]
        public int? ApprovedTotalReviews { get; set; }


        /// <summary>
        ///     Gets or sets the total rating votes (approved reviews)
        /// </summary>
        [JsonProperty("rating_avg")]
        public decimal RatingAvg { get; set; }

        ///// <summary>
        /////  Gets or sets the discount (discountPer = 100 - (price * 100 / old_price))
        ///// </summary>
        //[JsonProperty("discount")]
        //public string Discount { get; set; }

        /// <summary>
        ///  Gets the value which indicates product is in cureent customers favorite list or not. 
        /// </summary>
        [JsonProperty("is_in_favorite")]
        public bool IsInFavorite { get; set; }

        /// <summary>
        ///  Gets the fastest slot timing
        /// </summary>
        [JsonProperty("fastest_slot_time")]
        public string FastestSlotTime { get; set; }

        /// <summary>
        ///  Gets or sets the product type
        /// </summary>
        [ProductTypeValidation]
        [JsonProperty("product_type")]
        public string ProductType
        {
            get
            {
                var productTypeId = _productTypeId;
                if (productTypeId != null)
                {
                    return ((ProductType)productTypeId).ToString();
                }

                return null;
            }
            set
            {
                ProductType productTypeId;
                if (Enum.TryParse(value, out productTypeId))
                {
                    _productTypeId = (int)productTypeId;
                }
                else
                {
                    _productTypeId = null;
                }
            }
        }

        [JsonProperty("size")]
        public string Size { get; set; }

        [JsonProperty("container")]
        public string Container { get; set; }

        /// <summary>
        /// Gets or sets a list of ids of products that are required to be added to the cart by this product
        /// </summary>
        [JsonIgnore]
        public IList<int> RequiredProductIds { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonIgnore]
        public List<int> AssociatedProductIds { get; set; }
    }

    #region Nested Classes

    public partial record ProductPriceModel
    {
        public string OldPrice { get; set; }
        public decimal? OldPriceValue { get; set; }
        public string Price { get; set; }
        public decimal? PriceValue { get; set; }
        public bool IsRental { get; set; }
    }

    #endregion
}
