using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Nop.Plugin.Api.DTO.Base;
using Nop.Plugin.Api.DTO.Customers;
using Nop.Plugin.Api.DTO.Products;
using Nop.Plugin.Api.DTOs.ShoppingCarts;

namespace Nop.Plugin.Api.DTO.ShoppingCarts
{
    //[Validator(typeof(ShoppingCartItemDtoValidator))]
    [JsonObject(Title = "shopping_cart_item")]
    public class ShoppingCartItemDto : BaseDto
    {
        public ShoppingCartItemDto()
        {
            Warnings = new List<string>();
        }
        /// <summary>
        ///     Gets or sets the selected attributes
        /// </summary>
        [JsonProperty("product_attributes")]
        public List<ProductItemAttributeDto> Attributes { get; set; }

        /// <summary>
        ///     Gets or sets the price enter by a customer
        /// </summary>
        [JsonProperty("customer_entered_price")]
        public decimal? CustomerEnteredPrice { get; set; }

        /// <summary>
        ///     Gets or sets the quantity
        /// </summary>
        [JsonProperty("quantity")]
        public int? Quantity { get; set; }

        /// <summary>
        /// Gets or sets the unit price
        /// </summary>
        [JsonProperty("unit_price")]
        public string UnitPrice { get; set; }

        [JsonProperty("unit_price_value")]
        public decimal UnitPriceValue { get; set; }

        /// <summary>
        /// Gets or sets the sub total
        /// </summary>
        [JsonProperty("sub_total")]
        public string SubTotal { get; set; }

        [JsonProperty("sub_total_value")]
        public decimal SubTotalValue { get; set; }

        /// <summary>
        /// Gets or sets the discount amount
        /// </summary>
        [JsonProperty("discount")]
        public string Discount { get; set; }

        [JsonProperty("discount_value")]
        public decimal DiscountValue { get; set; }

        /// <summary>
        ///     Gets or sets the rental product start date (null if it's not a rental product)
        /// </summary>
        [JsonProperty("rental_start_date_utc")]
        public DateTime? RentalStartDateUtc { get; set; }

        /// <summary>
        ///     Gets or sets the rental product end date (null if it's not a rental product)
        /// </summary>
        [JsonProperty("rental_end_date_utc")]
        public DateTime? RentalEndDateUtc { get; set; }

        /// <summary>
        ///     Gets or sets the date and time of instance creation
        /// </summary>
        [JsonProperty("created_on_utc")]
        public DateTime? CreatedOnUtc { get; set; }

        /// <summary>
        ///     Gets or sets the date and time of instance update
        /// </summary>
        [JsonProperty("updated_on_utc")]
        public DateTime? UpdatedOnUtc { get; set; }

        /// <summary>
        ///     Gets the log type
        /// </summary>
        [JsonProperty("shopping_cart_type", Required = Required.Always)]
        public ShoppingCartType ShoppingCartType { get; set; }

        [JsonProperty("product_id")]
        public int? ProductId { get; set; }

        /// <summary>
        ///     Gets or sets the product
        /// </summary>
        [JsonProperty("product")]
        public ProductDto ProductDto { get; set; }

        [JsonProperty("customer_id")]
        public int? CustomerId { get; set; }

        [JsonProperty("slotprice")]
        public string SlotPrice { get; set; }

        [JsonProperty("slot_start_date")]
        public string SlotStartDate { get; set; }

        [JsonProperty("slot_time")]
        public string SlotTime { get; set; }

        [JsonProperty("is_pickup")]
        public bool IsPickup { get; set; }

        [JsonProperty("vendor_id")]
        public int VendorId { get; set; }

        [JsonProperty("custom_attribute_info")]
        public string CustomAttributeInfo { get; set; }

        [JsonProperty("master_product_id")]
        public string MasterProductId { get; set; }

        [JsonProperty("grouped_product_id")]
        public string GroupedProductId { get; set; }

        public IList<string> Warnings { get; set; }
    }
}
