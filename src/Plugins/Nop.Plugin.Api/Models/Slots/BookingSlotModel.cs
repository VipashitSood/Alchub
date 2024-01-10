using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Nop.Web.Framework.Models;

namespace Nop.Plugin.Api.Models.Slots
{
    public record BookingSlotModel
    {
        public BookingSlotModel()
        {
            SlotModel = new List<SlotDefault>();
        }

        [JsonProperty("start")]
        public DateTime Start { get; set; }

        [JsonProperty("end")]
        public DateTime End { get; set; }

        [JsonProperty("slots")]
        public IList<SlotDefault> SlotModel { get; set; }

        public partial record SlotDefault
        {
            [JsonProperty("id")]
            public int Id { get; set; }

            [JsonProperty("slot")]
            public string Slot { get; set; }

            [JsonProperty("slot_time2")]
            public string SlotTime2 { get; set; }

            [JsonProperty("price")]
            public string Price { get; set; }

            [JsonProperty("isAvailable")]
            public bool IsAvailable { get; set; }

            [JsonProperty("blockId")]
            public int BlockId { get; set; }

            [JsonProperty("capacity")]
            public int Capacity { get; set; }

            [JsonProperty("productId")]
            public int ProductId { get; set; }

            [JsonProperty("isBook")]
            public bool IsBook { get; set; }

            [JsonProperty("isSelected")]
            public bool IsSelected { get; set; }

            [JsonProperty("isPickup")]
            public bool IsPickup { get; set; }

            [JsonProperty("start")]
            public DateTime Start { get; set; }
        }
    }
}
