using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Nop.Plugin.Api.Attributes;
using Nop.Plugin.Api.DTO;
using Nop.Plugin.Api.DTO.Base;
using Nop.Plugin.Api.DTO.Images;

namespace Nop.Plugin.Api.DTOs.Vendors
{
    [JsonObject(Title = "vendor")]
    public class VendorDto : BaseDto
    {
        /// <summary>
        /// Gets or sets vendor name
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets display order
        /// </summary>
        [JsonProperty("display_order")]
        public int DisplayOrder { get; set; }

        /// <summary>
        /// Gets or sets image
        /// </summary>
        [ImageCollectionValidation]
        [JsonProperty("image")]
        public ImageMappingDto Image { get; set; }

        /// <summary>
        /// Gets or sets contact number
        /// </summary>
        [JsonProperty("phone_number")]
        public string PhoneNumber { get; set; }

        /// <summary>
        /// Gets or sets distance value
        /// </summary>
        [JsonProperty("distance_value")]
        public decimal DistanceValue { get; set; }

        /// <summary>
        /// Gets or sets distance string
        /// </summary>
        [JsonProperty("distance")]
        public string Distance { get; set; }

        /// <summary>
        /// Determine whether this vendor is in customer favorite list or not
        /// </summary>
        [JsonProperty("is_favorite")]
        public bool IsFavorite { get; set; }

        /// <summary>
        /// Gets or sets open time of store
        /// </summary>
        [JsonProperty("open_at")]
        public TimeSpan OpenAt { get; set; }

        /// <summary>
        /// Gets or sets store close time
        /// </summary>
        [JsonProperty("open_until")]
        public TimeSpan OpenUntil { get; set; }

        /// <summary>
        /// Determine whether vendor is deliverable or not
        /// </summary>
        [JsonProperty("is_deliver")]
        public bool IsDeliver { get; set; }

        /// <summary>
        /// Determine whether vendor is pickup or not
        /// </summary>
        [JsonProperty("is_pickup")]
        public bool IsPickup { get; set; }

        /// <summary>
        /// Determine whether need to show distance or not
        /// </summary>
        [JsonProperty("show_distance")]
        public bool ShowDistance { get; set; }

        /// <summary>
        /// Gets or sets store address
        /// </summary>
        [JsonProperty("store_address")]
        public string PickupAddress { get; set; }

        /// <summary>
        /// Gets or sets vendor open/close time
        /// </summary>
        [JsonProperty("timing")]
        public string VendorTime { get; set; }

        /// <summary>
        /// Gets or sets vendor open/close time label
        /// </summary>
        [JsonProperty("timing_label")]
        public string VendorTimeLabel { get; set; }

        /// <summary>
        ///     Gets or sets the Geo_Long
        /// </summary>
        [JsonProperty("Geo_Long")]
        public string GeoLong { get; set; }

        /// <summary>
        ///     Gets or sets the GeoLant
        /// </summary>
        [JsonProperty("Geo_Lant")]
        public string GeoLant { get; set; }
    }
}
