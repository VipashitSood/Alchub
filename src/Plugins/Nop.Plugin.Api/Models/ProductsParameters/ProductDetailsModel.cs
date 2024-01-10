using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Api.DTO.Base;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Api.Models.ProductsParameters
{
    public class ProductDetailsModel : BaseDto
    {
        public ProductDetailsModel()
        {
            DefaultPictureModel = new PictureModel();
            PictureModels = new List<PictureModel>();
            GiftCard = new GiftCardModel();
            ProductPrice = new ProductPriceModel();
            AddToCart = new AddToCartModel();
            ProductAttributes = new List<ProductAttributeModel>();
            AssociatedProducts = new List<ProductDetailsModel>();
            ProductReviewOverview = new ProductReviewOverviewModel();
            ProductVendorModel = new ProductVendorModel();
            TierPrices = new List<TierPriceModel>();
        }

        //picture(s)
        public bool DefaultPictureZoomEnabled { get; set; }
        public PictureModel DefaultPictureModel { get; set; }
        public ProductVendorModel ProductVendorModel { get; set; }
        public IList<PictureModel> PictureModels { get; set; }

        public string Name { get; set; }
        public string ShortDescription { get; set; }
        public string FullDescription { get; set; }
        public string MetaKeywords { get; set; }
        public string MetaDescription { get; set; }
        public string MetaTitle { get; set; }
        public string SeName { get; set; }
        public bool VisibleIndividually { get; set; }

        public ProductType ProductType { get; set; }

        public bool ShowSku { get; set; }
        public string Sku { get; set; }

        public bool ShowManufacturerPartNumber { get; set; }
        public string ManufacturerPartNumber { get; set; }

        public bool ShowGtin { get; set; }
        public string Gtin { get; set; }

        public bool ShowVendor { get; set; }

        public bool HasSampleDownload { get; set; }

        public GiftCardModel GiftCard { get; set; }

        public bool IsShipEnabled { get; set; }
        public bool IsFreeShipping { get; set; }
        public bool FreeShippingNotificationEnabled { get; set; }
        public string DeliveryDate { get; set; }

        public bool IsRental { get; set; }
        public DateTime? RentalStartDate { get; set; }
        public DateTime? RentalEndDate { get; set; }

        public DateTime? AvailableEndDate { get; set; }

        public ManageInventoryMethod ManageInventoryMethod { get; set; }

        public string StockAvailability { get; set; }

        public bool DisplayBackInStockSubscription { get; set; }

        public bool EmailAFriendEnabled { get; set; }
        public bool CompareProductsEnabled { get; set; }

        public string PageShareCode { get; set; }

        public ProductPriceModel ProductPrice { get; set; }

        public AddToCartModel AddToCart { get; set; }

        public IList<ProductAttributeModel> ProductAttributes { get; set; }

        public ProductReviewOverviewModel ProductReviewOverview { get; set; }

        public IList<TierPriceModel> TierPrices { get; set; }

        //a list of associated products. For example, "Grouped" products could have several child "simple" products
        public IList<ProductDetailsModel> AssociatedProducts { get; set; }

        public bool DisplayDiscontinuedMessage { get; set; }

        public string CurrentStoreName { get; set; }

        public bool InStock { get; set; }

        public bool AllowAddingOnlyExistingAttributeCombinations { get; set; }

        #region Nested Classes

        public class AddToCartModel : BaseDto
        {
            public AddToCartModel()
            {
                AllowedQuantities = new List<SelectListItem>();
            }
            [JsonProperty("allowed_quantities")]
            public List<SelectListItem> AllowedQuantities { get; set; }

        }

        public class ProductPriceModel : BaseDto
        {
            /// <summary>
            /// The currency (in 3-letter ISO 4217 format) of the offer price 
            /// </summary>
            public string CurrencyCode { get; set; }

            public string OldPrice { get; set; }
            public decimal? OldPriceValue { get; set; }

            public string Price { get; set; }
            public decimal PriceWithoutDiscount { get; set; }
            public decimal PriceValue { get; set; }
            public string PriceWithDiscount { get; set; }
            public decimal? PriceWithDiscountValue { get; set; }

            public bool CustomerEntersPrice { get; set; }

            public bool CallForPrice { get; set; }

            public int ProductId { get; set; }

            public bool HidePrices { get; set; }

            //rental
            public bool IsRental { get; set; }
            public string RentalPrice { get; set; }
            public decimal? RentalPriceValue { get; set; }

            /// <summary>
            /// A value indicating whether we should display tax/shipping info (used in Germany)
            /// </summary>
            public bool DisplayTaxShippingInfo { get; set; }
            /// <summary>
            /// PAngV baseprice (used in Germany)
            /// </summary>
            public string BasePricePAngV { get; set; }
            public decimal? BasePricePAngVValue { get; set; }
        }

        public class GiftCardModel : BaseDto
        {
            public bool IsGiftCard { get; set; }

            [JsonProperty("recipient_name")]
            public string RecipientName { get; set; }

            [JsonProperty("recipient_email")]
            public string RecipientEmail { get; set; }

            [JsonProperty("sender_name")]
            public string SenderName { get; set; }

            [JsonProperty("sender_email")]
            public string SenderEmail { get; set; }

            [JsonProperty("message")]
            public string Message { get; set; }

            [JsonProperty("gift_card_type")]
            public GiftCardType GiftCardType { get; set; }
        }

        public class TierPriceModel : BaseDto
        {
            [JsonProperty("price")]
            public string Price { get; set; }
            [JsonProperty("price_value")]
            public decimal PriceValue { get; set; }
            [JsonProperty("quantity")]
            public int Quantity { get; set; }
        }

        public class ProductAttributeModel : BaseDto
        {
            public ProductAttributeModel()
            {
                AllowedFileExtensions = new List<string>();
                Values = new List<ProductAttributeValueModel>();
            }

            public int ProductId { get; set; }

            public int ProductAttributeId { get; set; }

            public string Name { get; set; }

            public string Description { get; set; }

            public string TextPrompt { get; set; }

            public bool IsRequired { get; set; }

            /// <summary>
            /// Default value for textboxes
            /// </summary>
            public string DefaultValue { get; set; }
            /// <summary>
            /// Selected day value for datepicker
            /// </summary>
            public int? SelectedDay { get; set; }
            /// <summary>
            /// Selected month value for datepicker
            /// </summary>
            public int? SelectedMonth { get; set; }
            /// <summary>
            /// Selected year value for datepicker
            /// </summary>
            public int? SelectedYear { get; set; }

            /// <summary>
            /// A value indicating whether this attribute depends on some other attribute
            /// </summary>
            public bool HasCondition { get; set; }

            /// <summary>
            /// Allowed file extensions for customer uploaded files
            /// </summary>
            public IList<string> AllowedFileExtensions { get; set; }

            public AttributeControlType AttributeControlType { get; set; }

            public IList<ProductAttributeValueModel> Values { get; set; }
        }

        public class ProductAttributeValueModel : BaseDto
        {
            public ProductAttributeValueModel()
            {
                ImageSquaresPictureModel = new PictureModel();
            }
            [JsonProperty("Id")]
            public int Id { get; set; }

            [JsonProperty("name")]
            public string Name { get; set; }

            public string ColorSquaresRgb { get; set; }

            //picture model is used with "image square" attribute type
            public PictureModel ImageSquaresPictureModel { get; set; }

            public string PriceAdjustment { get; set; }

            public bool PriceAdjustmentUsePercentage { get; set; }

            public decimal PriceAdjustmentValue { get; set; }

            public bool IsPreSelected { get; set; }

            //product picture ID (associated to this value)
            public int PictureId { get; set; }

            public bool CustomerEntersQty { get; set; }

            public int Quantity { get; set; }
        }

        #endregion
    }
}