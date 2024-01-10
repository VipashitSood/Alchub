using System;
using Newtonsoft.Json;
using Nop.Core.Domain.Slots;
using Nop.Web.Framework.Models;

namespace Nop.Plugin.Api.Models.Slots
{
    /// <summary>
    /// Represents a zone model
    /// </summary>
    public partial record SlotModel 
    {
        public int Id { get; set; }
        [JsonProperty("start")]
        public DateTime Start { get; set; }
        [JsonProperty("end")]
        public DateTime End { get; set; }
        [JsonProperty("capacity")]
        public int Capacity { get; set; }
        [JsonProperty("price")]
        public decimal Price { get; set; }
        
    }
}