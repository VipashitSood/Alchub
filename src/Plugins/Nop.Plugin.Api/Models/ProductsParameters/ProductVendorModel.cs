using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Nop.Plugin.Api.Attributes;
using Nop.Plugin.Api.DTO.Base;
using Nop.Plugin.Api.DTO.Images;
using static Nop.Plugin.Api.Models.ProductsParameters.ProductDetailsModel;

namespace Nop.Plugin.Api.Models.ProductsParameters
{
    public class ProductVendorModel
    {
        public ProductVendorModel()
        {
            ProductDetailVendor = new List<ProductDetailVendors>();
            Images = new List<ImageMappingDto>();
        }

        [JsonProperty("product_detail_vendors")]
        public IList<ProductDetailVendors> ProductDetailVendor { get; set; }

        /// <summary>
        /// variant image collection - this will be prepared when making variant call.
        /// </summary>
        [ImageCollectionValidation]
        [JsonProperty("variant_images")]
        public List<ImageMappingDto> Images { get; set; }

        public class ProductDetailVendors
        {
            public ProductDetailVendors()
            {
                VendorProductPrice = new ProductPriceModel();
            }

            [JsonProperty("productId")]
            public int ProductId { get; set; }

            [JsonProperty("vendorId")]
            public int VendorId { get; set; }

            [JsonProperty("vendorName")]
            public string VendorName { get; set; }

            [JsonProperty("manageDelivery")]
            public bool ManageDelivery { get; set; }

            [JsonProperty("pickAvailable")]
            public bool PickAvailable { get; set; }

            [JsonProperty("deliveryAvailable")]
            public bool DeliveryAvailable { get; set; }

            [JsonProperty("orderAmount")]
            public string OrderAmount { get; set; }

            [JsonProperty("deliveryFee")]
            public string DeliveryFee { get; set; }

            [JsonProperty("isDefaultVendor")]
            public bool IsDefaultVendor { get; set; }

            [JsonProperty("vendorProductPrice")]
            public ProductPriceModel VendorProductPrice { get; set; }

            [JsonProperty("Date")]
            public string Date { get; set; }

            [JsonProperty("Time")]
            public DateTime? Time { get; set; }

            [JsonProperty("StartTime")]
            public string StartTime { get; set; }

            [JsonProperty("StartTime2")]
            public string StartTime2 { get; set; }

            [JsonProperty("Distance")]
            public string Distance { get; set; }
            
            [JsonProperty("DistanceValue")]
            public decimal DistanceValue { get; set; }
        }
    }
}