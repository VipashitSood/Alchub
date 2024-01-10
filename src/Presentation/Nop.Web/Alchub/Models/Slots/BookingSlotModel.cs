using System;
using System.Collections.Generic;
using Nop.Web.Framework.Models;

namespace Nop.Web.Models.Slots
{
    public record BookingSlotModel : BaseNopModel
    {
        public BookingSlotModel()
        {
        }
        public int Id { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public string Slot { get; set; }
        public string Price{ get; set; }
        public bool IsAvailable { get; set; }
        public int BlockId { get; set; }
        public int Capacity { get; set; }
        public int ProductId { get; set; }
        public bool IsBook { get; set; }
        public bool IsPickup { get; set; }
        public bool IsSelected { get; set; }
    }
}
