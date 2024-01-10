using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Nop.Plugin.Api.Attributes;
using Nop.Plugin.Api.DTO.Base;
using Nop.Plugin.Api.DTO.Images;

namespace Nop.Plugin.Api.Models
{
    public class FavoriteVendorModel
    {
        public FavoriteVendorModel()
        {
            FavoriteVendors = new List<FavoriteVendor>();
        }

        [JsonProperty("toggle_instruction")]
        public string ToggleInstruction { get; set; }

        [JsonProperty("is_favorite_toggle_on")]
        public bool IsFavoriteToggleOn { get; set; }

         [JsonProperty("favorite_vendors")]
        public IList<FavoriteVendor> FavoriteVendors { get; set; }

        public class FavoriteVendor
        {
            public FavoriteVendor()
            {
                Image = new ImageMappingDto();
            }

            [JsonProperty("vendor_id")]
            public int VendorId { get; set; }

            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("display_order")]
            public int DisplayOrder { get; set; }

            [ImageCollectionValidation]
            [JsonProperty("image")]
            public ImageMappingDto Image { get; set; }

            [JsonProperty("contact_number")]
            public string ContactNumber { get; set; }

            [JsonProperty("display_contact_number")]
            public string DisplayContactNumber { get; set; }

            [JsonProperty("distance_value")]
            public decimal DistanceValue { get; set; }

            [JsonProperty("distance")]
            public string Distance { get; set; }

            [JsonProperty("is_favorite")]
            public bool IsFavorite { get; set; }

            [JsonProperty("open_at")]
            public TimeSpan OpenAt { get; set; }

            [JsonProperty("open_until")]
            public TimeSpan OpenUntil { get; set; }

            [JsonProperty("is_deliver")]
            public bool IsDeliver { get; set; }

            [JsonProperty("is_pickup")]
            public bool IsPickup { get; set; }

            [JsonProperty("show_distance")]
            public bool ShowDistance { get; set; }

            [JsonProperty("store_address")]
            public string PickupAddress { get; set; }

            [JsonProperty("timing")]
            public string VendorTime { get; set; }

            [JsonProperty("timing_label")]
            public string VendorTimeLabel { get; set; }

            [JsonProperty("Geo_Long")]
            public string GeoLong { get; set; }

            [JsonProperty("Geo_Lant")]
            public string GeoLant { get; set; }
        }
    }
}
